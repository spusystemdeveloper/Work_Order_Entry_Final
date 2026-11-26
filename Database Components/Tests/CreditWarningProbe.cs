// Exercises real V8 events and modal warnings against an owned LocalDB fixture.
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

class CreditWarningProbe
{
    static BindingFlags fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static BindingFlags instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static Type module;
    static Form form;
    static int prompts, checks;
    static string unexpected;
    static Control Control(string name) { return (Control)form.GetType().GetProperty(name, instance).GetValue(form, null); }
    static void Set(string name, object value) { module.GetField(name, fields).SetValue(null, value); }
    static void Sql(SqlConnection cx, string text) { using (var cmd = cx.CreateCommand()) { cmd.CommandText = text; cmd.ExecuteNonQuery(); } }
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; Console.WriteLine("PASS: " + name); }
    static void Expect(Action action, int count, string name)
    {
        int before = prompts;
        action();
        Application.DoEvents();
        Check(unexpected == null && prompts - before == count, name + (unexpected == null ? "" : ": " + unexpected));
    }
    static void LoadCustomer(int id)
    {
        Set("iCusID", id);
        form.GetType().GetMethod("GetCustomerPriceLevel").Invoke(form, null);
    }

    [STAThread]
    static int Main(string[] args)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-PH");
        string db = "SOD_CreditWarning_" + Guid.NewGuid().ToString("N");
        bool created = false;
        using (var cx = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False"))
        using (var timer = new Timer { Interval = 30 })
        try
        {
            var app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            module = app.GetType("WorkOrderEntry.ModConnectDB", true);
            cx.Open(); Sql(cx, "CREATE DATABASE [" + db + "]"); created = true; cx.ChangeDatabase(db);
            Sql(cx, "CREATE TABLE dbo.Customer(ID int PRIMARY KEY, PriceLevel smallint NOT NULL, CreditLimit money NOT NULL); INSERT dbo.Customer VALUES(1,0,150),(2,0,1000),(3,0,150);");
            Sql(cx, "CREATE FUNCTION dbo.SOD_fn_GetCreditLimit(@id int) RETURNS decimal(18,2) AS BEGIN RETURN 20; END;");
            Sql(cx, "CREATE FUNCTION dbo.SOD_fn_GetTotalOpenWorkOrder(@id int) RETURNS decimal(18,2) AS BEGIN RETURN 30; END;");
            // Override only this process's connection fields before any app query.
            Set("dbs", "(localdb)\\MSSQLLocalDB;Integrated Security=True;Pooling=False");
            Set("dbn", db); Set("dbu", ""); Set("dbp", ""); Set("iCusID", 0);
            var endpoint = (string)module.GetMethod("DB_Conn", fields).Invoke(null, new object[] { "constr" });
            var builder = new SqlConnectionStringBuilder(endpoint);
            Check(builder.DataSource == "(localdb)\\MSSQLLocalDB" && builder.InitialCatalog == db && builder.IntegratedSecurity, "owned localhost endpoint");
            form = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmItemLookUp", true));
            form.GetType().GetField("isLoadingRecalledOrder", instance).SetValue(form, true);
            typeof(Control).GetMethod("CreateHandle", instance).Invoke(form, null); // Never Load/Show the production form.
            timer.Tick += delegate
            {
                foreach (Form prompt in Application.OpenForms.Cast<Form>().ToArray())
                {
                    if (prompt.GetType().Name != "frmAppPrompt") continue;
                    var message = (Control)prompt.GetType().GetProperty("txtMessage", instance).GetValue(prompt, null);
                    if (prompt.Text != "Warning" || message.Text != "Exceeded available credit limit.") { unexpected = prompt.Text + ": " + message.Text; Console.WriteLine("Unexpected local prompt: " + unexpected); }
                    prompts++;
                    prompt.DialogResult = DialogResult.OK;
                    prompt.Close();
                }
            };
            timer.Start();
            var payment = (ComboBox)Control("cboPayment");
            Control("txtTotal").Text = "200.00";
            Expect(() => LoadCustomer(1), 0, "CASH/unselected customer load does not warn");
            Check(decimal.Parse(Control("txtAvailable").Text) == 100M, "existing limit minus AR and open orders preserved");
            Expect(() => payment.Text = "CHARGE", 1, "selecting CHARGE after items warns");
            Expect(() => Control("txtTotal").Text = "250.00", 1, "changing total while CHARGE warns");
            Expect(() => Control("txtTotal").Text = "100.00", 0, "total equal to available does not warn");
            Expect(() => Control("txtTotal").Text = "99.99", 0, "total below available does not warn");
            Expect(() => Control("txtTotal").Text = "101.00", 1, "total above available warns");
            Expect(() => LoadCustomer(2), 0, "changing to sufficient-credit customer does not warn");
            Expect(() => LoadCustomer(1), 1, "changing to insufficient-credit customer warns");
            Expect(() => LoadCustomer(3), 1, "same available amount on another customer still warns");
            Expect(() => payment.Text = "CASH", 0, "switching to CASH does not warn");
            Expect(() => Control("txtTotal").Text = "300.00", 0, "CASH total change does not warn");
            Expect(() => payment.Text = "DEBIT", 0, "DEBIT does not warn");
            Expect(() => payment.Text = "CREDIT", 0, "CREDIT does not warn");
            Expect(() => payment.Text = "CHARGE", 1, "switching back to CHARGE warns");
            Expect(() => { Set("iCusID", 2); Control("txtTotal").Text = "301.00"; }, 0, "previous customer's cached balance is not used");
            Expect(() => { Set("iCusID", 0); Control("txtTotal").Text = "302.00"; }, 0, "no selected customer does not warn");
            Set("iCusID", 3);
            Expect(() => Control("txtTotal").Text = "", 0, "blank total does not warn");
            Expect(() => Control("txtTotal").Text = "invalid", 0, "invalid total does not warn");
            Control("txtAvailable").Text = "invalid";
            Expect(() => Control("txtTotal").Text = "400.00", 0, "invalid available credit does not warn");
            Expect(() => LoadCustomer(3), 1, "fresh customer lookup restores warning after invalid balance");
            Check(Control("txtTotal").Text == "400.00" && payment.Text.Equals("CHARGE", StringComparison.OrdinalIgnoreCase), "warning leaves total and payment unchanged");
            Check(Control("cmdOk").Enabled, "warning does not disable Save");
            using (var cmd = cx.CreateCommand()) { cmd.CommandText = "SELECT SUM(CreditLimit) FROM dbo.Customer"; Check(Convert.ToDecimal(cmd.ExecuteScalar()) == 1300M, "customer credit limits unchanged"); }
            Console.WriteLine("PASS: " + checks + " credit warning checks; no production form Load, save or live connection.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally
        {
            timer.Stop();
            if (form != null) form.Dispose();
            SqlConnection.ClearAllPools();
            if (created) { cx.ChangeDatabase("master"); Sql(cx, "ALTER DATABASE [" + db + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + db + "]"); }
        }
    }
}
