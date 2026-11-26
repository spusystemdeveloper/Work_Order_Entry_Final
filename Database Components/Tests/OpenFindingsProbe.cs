// Findings 31-41 (November 18): exercises the compiled V8 code against an owned,
// disposable LocalDB database built from the application's LINQ mapping.
// No store database, store config or production form Load is used.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

class OpenFindingsProbe
{
    const BindingFlags Statics = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static Assembly app;
    static Type module;
    static Form form;
    static SqlConnection cx;
    static int checks;
    static readonly List<string> prompts = new List<string>();

    static void Set(string name, object value) { module.GetField(name, Statics).SetValue(null, value); }
    static object Sql(string text)
    {
        using (var cmd = cx.CreateCommand()) { cmd.CommandText = text; return cmd.ExecuteScalar(); }
    }
    static void Check(bool ok, string name) { if (!ok) throw new Exception("FAILED: " + name); checks++; Console.WriteLine("PASS: " + name); }
    static Type T(string name) { return app.GetType("WorkOrderEntry." + name, true); }
    static object Call(Type type, string method, params object[] args) { return type.GetMethod(method, Statics).Invoke(null, args); }
    static object CallForm(string method, params object[] args)
    {
        var candidates = form.GetType().GetMethods(Members).Where(m => m.Name == method && m.GetParameters().Length == args.Length).ToArray();
        return candidates[0].Invoke(form, args);
    }
    static Control Ctl(string name) { return (Control)form.GetType().GetProperty(name, Members).GetValue(form, null); }
    static object NewContext() { return module.GetMethod("GetDB", Statics).Invoke(null, null); }

    // Inserts one row, filling every NOT NULL column without a value with a neutral default.
    static void Insert(string table, Dictionary<string, string> values)
    {
        var cols = new List<string>(); var vals = new List<string>(); bool identity = false;
        using (var cmd = cx.CreateCommand())
        {
            cmd.CommandText = "SELECT c.name, t.name, c.is_nullable, c.is_identity FROM sys.columns c JOIN sys.types t ON t.user_type_id=c.user_type_id WHERE c.object_id=OBJECT_ID(@t) ORDER BY c.column_id";
            cmd.Parameters.AddWithValue("@t", "dbo." + table);
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                {
                    string col = r.GetString(0), type = r.GetString(1); bool nullable = r.GetBoolean(2), isIdentity = r.GetBoolean(3);
                    string given;
                    if (values.TryGetValue(col, out given)) { cols.Add("[" + col + "]"); vals.Add(given); if (isIdentity) identity = true; continue; }
                    if (nullable || isIdentity || type == "timestamp") continue;
                    cols.Add("[" + col + "]");
                    vals.Add(type.Contains("char") || type.Contains("text") ? "N''" :
                             type.Contains("date") ? "GETDATE()" :
                             type == "uniqueidentifier" ? "NEWID()" :
                             type.Contains("binary") ? "0x00" : "0");
                }
        }
        string sql = "INSERT dbo.[" + table + "](" + string.Join(",", cols) + ") VALUES(" + string.Join(",", vals) + ");";
        if (identity) sql = "SET IDENTITY_INSERT dbo.[" + table + "] ON; " + sql + " SET IDENTITY_INSERT dbo.[" + table + "] OFF;";
        Sql(sql);
    }
    static Dictionary<string, string> V(params string[] pairs)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < pairs.Length; i += 2) d[pairs[i]] = pairs[i + 1];
        return d;
    }

    [STAThread]
    static int Main(string[] args)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-PH");
        string repo = Path.GetFullPath(args[1]);
        string db = "SOD_OpenFindings_" + Guid.NewGuid().ToString("N");
        bool created = false;
        var timer = new Timer { Interval = 30 };
        cx = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False");
        try
        {
            app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            module = T("ModConnectDB");
            Set("dbs", "(localdb)\\MSSQLLocalDB;Integrated Security=True;Pooling=False");
            Set("dbn", db); Set("dbu", ""); Set("dbp", "");
            var builder = new SqlConnectionStringBuilder((string)module.GetMethod("DB_Conn", Statics).Invoke(null, new object[] { "constr" }));
            Check(builder.DataSource == "(localdb)\\MSSQLLocalDB" && builder.InitialCatalog == db && builder.IntegratedSecurity, "owned LocalDB endpoint");

            // Build every mapped table from the application's own LINQ mapping.
            using (var ctx = (IDisposable)NewContext()) { ctx.GetType().GetMethod("CreateDatabase").Invoke(ctx, null); }
            created = true;
            cx.Open(); cx.ChangeDatabase(db);
            // The module's shared contexts were created before dbs/dbn were set.
            foreach (var name in new[] { "db", "dbnew" }) { var f = module.GetField(name, Statics); if (f != null) f.SetValue(null, NewContext()); }

            timer.Tick += delegate
            {
                foreach (Form prompt in Application.OpenForms.Cast<Form>().ToArray())
                {
                    if (prompt.GetType().Name != "frmAppPrompt") continue;
                    var message = (Control)prompt.GetType().GetProperty("txtMessage", Members).GetValue(prompt, null);
                    prompts.Add(prompt.Text + ": " + message.Text);
                    prompt.DialogResult = DialogResult.OK; prompt.Close();
                }
            };
            timer.Start();

            form = (Form)Activator.CreateInstance(T("frmItemLookUp"));
            typeof(Control).GetMethod("CreateHandle", Members).Invoke(form, null); // never Load/Show
            Finding31Drafts(repo);
            Finding34ImportedQuotes();
            Finding39Recall();
            Finding40Usernames();
            Finding41FreshPrices();

            Console.WriteLine("PASS: " + checks + " checks. Prompts seen: " + prompts.Count);
            foreach (var p in prompts) Console.WriteLine("  prompt -> " + p.Replace("\r", " ").Replace("\n", " "));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString());
            foreach (var p in prompts) Console.Error.WriteLine("  prompt -> " + p.Replace("\r", " ").Replace("\n", " "));
            return 1;
        }
        finally
        {
            timer.Stop();
            if (form != null) form.Dispose();
            SqlConnection.ClearAllPools();
            if (created)
            {
                if (cx.State != System.Data.ConnectionState.Open) cx.Open();
                cx.ChangeDatabase("master");
                Sql("ALTER DATABASE [" + db + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + db + "]");
            }
            cx.Dispose();
        }
    }

    static string QtyType() { return (string)Sql("SELECT t.name FROM sys.columns c JOIN sys.types t ON t.user_type_id=c.user_type_id WHERE c.object_id=OBJECT_ID(N'dbo.SOD_WO_DraftDetail') AND c.name=N'Qty'"); }
    static void RunScript(string path)
    {
        using (var cmd = cx.CreateCommand()) { cmd.CommandText = File.ReadAllText(path); cmd.ExecuteNonQuery(); }
    }

    static void Finding31Drafts(string repo)
    {
        // Use the real draft tables (with ON DELETE CASCADE), then simulate a store
        // installed before this fix: Qty int, holding a whole number.
        Sql("DROP TABLE dbo.SOD_WO_DraftDetail; DROP TABLE dbo.SOD_WO_DraftHeader;");
        RunScript(Path.Combine(repo, "Database Components", "Migrations", "Ensure_Work_Order_Draft_Tables.sql"));
        Sql("ALTER TABLE dbo.SOD_WO_DraftDetail ALTER COLUMN Qty int NULL;");
        Insert("SOD_WO_DraftHeader", V("RegisterID", "N'9'", "UserName", "N'OLDSTORE'"));
        Insert("SOD_WO_DraftDetail", V("DraftID", "(SELECT MAX(DraftID) FROM dbo.SOD_WO_DraftHeader)", "[LineNo]".Trim('[', ']'), "0", "ItemCode", "N'OLD'", "Qty", "3"));
        Check(QtyType() == "int", "fixture starts with the old int Qty column");

        // The new app against an unmigrated store: drafts must keep working.
        var draft = T("clsWorkOrderDraft");
        Check((bool)Call(draft, "EnsureTables"), "draft recovery still available before the migration runs");
        Check((bool)Call(draft, "HasDraft", "9", "OLDSTORE"), "existing draft on the int column is found");
        DraftRoundTrip(false);

        foreach (var root in new[] { "Database Components", "Database Updater\\Database Components" })
        {
            string script = Path.Combine(repo, root, "Migrations", "Ensure_Work_Order_Draft_Tables.sql");
            RunScript(script);
            Check(QtyType() == "float", "migration converts Qty to float (" + root + ")");
            RunScript(script);
            Check(QtyType() == "float", "migration repeat run is harmless (" + root + ")");
        }
        Check(Convert.ToDouble(Sql("SELECT Qty FROM dbo.SOD_WO_DraftDetail WHERE ItemCode=N'OLD'")) == 3.0, "existing draft quantity kept by the conversion");
        Sql("DELETE dbo.SOD_WO_DraftHeader;");
        DraftRoundTrip(true);
    }

    // Saves two lines through SaveDraft, then recovers them through LoadDraft.
    static void DraftRoundTrip(bool migrated)
    {
        string label = migrated ? " (float column)" : " (old int column)";
        var formType = form.GetType();
        formType.GetField("usrRegister", Statics).SetValue(null, "7");
        formType.GetField("usrUsername", Statics).SetValue(null, "PROBE");
        Set("iCusID", 0);
        var grid = (DataGridView)Ctl("gridSelectItem");
        int itemCode = ((DataGridViewColumn)formType.GetProperty("ItemCode", Members).GetValue(form, null)).Index;
        int qty = ((DataGridViewColumn)formType.GetProperty("QTY", Members).GetValue(form, null)).Index;
        grid.Rows.Clear();
        int row = grid.Rows.Add();
        grid.Rows[row].Cells[itemCode].Value = "FRAC1";
        grid.Rows[row].Cells[qty].Value = 2.5;
        row = grid.Rows.Add();
        grid.Rows[row].Cells[itemCode].Value = "WHOLE";
        grid.Rows[row].Cells[qty].Value = 4.0;

        // Saving twice also exercises replacing the existing lines.
        Call(T("clsWorkOrderDraft"), "SaveDraft", form);
        Call(T("clsWorkOrderDraft"), "SaveDraft", form);
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SOD_WO_DraftDetail d JOIN dbo.SOD_WO_DraftHeader h ON h.DraftID=d.DraftID WHERE h.UserName=N'PROBE'")) == 2, "saving again replaces the draft lines" + label);
        double expectedFraction = migrated ? 2.5 : 2.0; // SQL Server truncates 2.5 into an int column
        Check(Convert.ToDouble(Sql("SELECT Qty FROM dbo.SOD_WO_DraftDetail WHERE ItemCode=N'FRAC1'")) == expectedFraction, "draft stores " + expectedFraction + " for 2.5" + label);

        grid.Rows.Clear();
        Call(T("clsWorkOrderDraft"), "LoadDraft", form);
        var loaded = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToDictionary(r => Convert.ToString(r.Cells[itemCode].Value), r => Convert.ToDouble(r.Cells[qty].Value));
        Check(loaded.Count == 2 && loaded["FRAC1"] == expectedFraction && loaded["WHOLE"] == 4.0, "draft recovery restores the saved quantities" + label);
        grid.Rows.Clear();
        Call(T("clsWorkOrderDraft"), "DeleteDraft", "7", "PROBE");
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SOD_WO_DraftDetail WHERE ItemCode IN (N'FRAC1',N'WHOLE')")) == 0, "deleting the draft removes its lines" + label);
    }

    static void Finding34ImportedQuotes()
    {
        var refType = T("ImportedQuoteReference");
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(refType));
        list.Add(Activator.CreateInstance(refType, 1201, 5));
        list.Add(Activator.CreateInstance(refType, 1202, 5));
        CallForm("ClearPendingImportedQuotes");
        CallForm("AddPendingImportedQuotes", list);
        CallForm("AddPendingImportedQuotes", list);
        Check((bool)CallForm("IsQuotationPendingImport", 1201, 5), "uploaded quotation is remembered for the unsaved order");
        Check(!(bool)CallForm("IsQuotationPendingImport", 1201, 6), "same number for another sales rep is a different quotation");

        // Same transaction shape as the save: record, SubmitChanges, commit.
        for (int pass = 0; pass < 2; pass++)
        {
            using (var ctx = (IDisposable)NewContext())
            {
                var conn = (System.Data.Common.DbConnection)ctx.GetType().GetProperty("Connection").GetValue(ctx, null);
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    ctx.GetType().GetProperty("Transaction").SetValue(ctx, tx, null);
                    CallForm("RecordImportedQuotations", ctx, 777);
                    ctx.GetType().GetMethod("SubmitChanges", Type.EmptyTypes).Invoke(ctx, null);
                    tx.Commit();
                }
            }
        }
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SOD_ImportedQuote")) == 2, "save records each uploaded quotation once");
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SOD_ImportedQuote WHERE QuoteID=777 AND SalesRepID=5 AND ReferenceID IN (1201,1202)")) == 2, "record holds the saved order number and sales rep");
        Check((bool)Call(T("clsImportQuotation"), "CheckQuotationLog", 1201, 5), "uploader's duplicate check now finds the saved quotation");

        // A rolled-back save leaves no record.
        CallForm("ClearPendingImportedQuotes");
        var one = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(refType));
        one.Add(Activator.CreateInstance(refType, 1300, 5));
        CallForm("AddPendingImportedQuotes", one);
        using (var ctx = (IDisposable)NewContext())
        {
            var conn = (System.Data.Common.DbConnection)ctx.GetType().GetProperty("Connection").GetValue(ctx, null);
            conn.Open();
            using (var tx = conn.BeginTransaction())
            {
                ctx.GetType().GetProperty("Transaction").SetValue(ctx, tx, null);
                CallForm("RecordImportedQuotations", ctx, 778);
                ctx.GetType().GetMethod("SubmitChanges", Type.EmptyTypes).Invoke(ctx, null);
                tx.Rollback();
            }
        }
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SOD_ImportedQuote WHERE ReferenceID=1300")) == 0, "rolled-back save records nothing");

        // Branches without the optional table still save.
        Sql("EXEC sp_rename N'dbo.SOD_ImportedQuote', N'SOD_ImportedQuote_Hidden';");
        using (var ctx = (IDisposable)NewContext())
        {
            CallForm("RecordImportedQuotations", ctx, 779);
            ctx.GetType().GetMethod("SubmitChanges", Type.EmptyTypes).Invoke(ctx, null);
        }
        Sql("EXEC sp_rename N'dbo.SOD_ImportedQuote_Hidden', N'SOD_ImportedQuote';");
        Check(true, "missing optional SOD_ImportedQuote table is skipped");

        CallForm("RefreshDetails", false);
        Check(!(bool)CallForm("IsQuotationPendingImport", 1300, 5), "clearing the screen forgets pending quotations");
    }

    static void Finding39Recall()
    {
        Insert("Customer", V("ID", "1", "Company", "N'REAL CUSTOMER'", "AccountNumber", "N'C1'", "PriceLevel", "2", "TaxExempt", "1"));
        Insert("SalesRep", V("ID", "3", "Name", "N'REP THREE'"));
        Insert("Order", V("ID", "501", "CustomerID", "1", "SalesRepID", "3", "Type", "2", "Closed", "0", "Comment", "N'CASH; Pick-up; r'"));
        Insert("Order", V("ID", "502", "CustomerID", "0", "SalesRepID", "0", "Type", "2", "Closed", "0", "Comment", "N'CASH; Pick-up; none'"));
        Insert("Order", V("ID", "503", "CustomerID", "1", "SalesRepID", "3", "Type", "2", "Closed", "1", "Comment", "N'closed'"));

        var recall = T("clsRecall");
        var withCustomer = ((IEnumerable)Call(recall, "RecallOrder", 501)).Cast<object>().ToList();
        Func<object, string, object> P = (o, n) => o.GetType().GetProperty(n).GetValue(o, null);
        Check(withCustomer.Count == 1 && (string)P(withCustomer[0], "Company") == "REAL CUSTOMER" && (int?)P(withCustomer[0], "CustID") == 1
              && (short?)P(withCustomer[0], "PriceLevel") == 2 && (bool?)P(withCustomer[0], "TaxExempt") == true && (string)P(withCustomer[0], "SaleRepName") == "REP THREE",
              "normal order recalls the same header values");
        var noCustomer = ((IEnumerable)Call(recall, "RecallOrder", 502)).Cast<object>().ToList();
        Check(noCustomer.Count == 1 && P(noCustomer[0], "CustID") == null && P(noCustomer[0], "Company") == null && (int)P(noCustomer[0], "Type") == 2,
              "order with CustomerID 0 and SalesRepID 0 still returns its header");
        Check(((IEnumerable)Call(recall, "RecallOrder", 503)).Cast<object>().Count() == 0, "closed order returns no header");

        // RMS table read at the start of every recall (not in the LINQ mapping).
        Sql("IF OBJECT_ID(N'dbo.ReasonCode', N'U') IS NULL CREATE TABLE dbo.ReasonCode(ID int PRIMARY KEY, Code nvarchar(17) NOT NULL, Type int NOT NULL); INSERT dbo.ReasonCode VALUES(39, N'701', 6);");

        // Screen check: a previous customer must not survive the recall.
        Ctl("txtCustomer").Text = "PREVIOUS CUSTOMER";
        Ctl("txtSales").Text = "PREVIOUS REP";
        Set("iCusID", 1); Set("iSalesID", 3); Set("bTaxExcempt", true);
        int before = prompts.Count;
        CallForm("RecallQuote", 502);
        Application.DoEvents();
        Console.WriteLine("  after recall: customer='" + Ctl("txtCustomer").Text + "' rep='" + Ctl("txtSales").Text + "' iCusID=" +
            module.GetField("iCusID", Statics).GetValue(null) + " tax=" + module.GetField("bTaxExcempt", Statics).GetValue(null) +
            " order label='" + ((ToolStripItem)form.GetType().GetProperty("lblOrderNo", Members).GetValue(form, null)).Text + "'");
        Check(Ctl("txtCustomer").Text == "" && Ctl("txtSales").Text == "" && (int)module.GetField("iCusID", Statics).GetValue(null) == 0
              && (bool)module.GetField("bTaxExcempt", Statics).GetValue(null) == false,
              "recalling an order with no customer clears the previous header");
        Console.WriteLine("  prompts during no-customer recall: " + (prompts.Count - before));
    }

    static void Finding40Usernames()
    {
        Insert("SOD_WO_User", V("UserID", "1", "UserName", "'ALICE'", "UserFullName", "'Alice'", "UserPass", "'x'", "UserRegister", "1"));
        Insert("SOD_WO_User", V("UserID", "2", "UserName", "'BOB'", "UserFullName", "'Bob'", "UserPass", "'x'", "UserRegister", "1"));
        var user = T("clsUser");
        Check((bool)Call(user, "IsUsernameUsedByAnotherUser", "alice", 2), "renaming BOB to alice is rejected (case-insensitive like login)");
        Check(!(bool)Call(user, "IsUsernameUsedByAnotherUser", "ALICE", 1), "saving ALICE with her own name is allowed");
        Check(!(bool)Call(user, "IsUsernameUsedByAnotherUser", "CAROL", 1), "a new unused name is allowed");
        Check(user.GetMethod("viewUserWithPass", Statics) == null, "the password listing query is gone");
    }

    static void Finding41FreshPrices()
    {
        Insert("Item", V("ID", "900", "ItemLookupCode", "N'P900'", "Price", "100"));
        Set("iPrice", 0);
        Ctl("txtPriceLevel").Text = "Price";
        // Warm the shared context the way the old code did.
        var shared = module.GetField("db", Statics).GetValue(null);
        var items = (IEnumerable)shared.GetType().GetProperty("Items").GetValue(shared, null);
        Check(items.Cast<object>().Count() == 1, "shared context has loaded the item");
        Check((bool)CallForm("IsPriceBelowAllowed", 900, 90m), "90 is below the 100 price");
        Sql("UPDATE dbo.Item SET Price = 80 WHERE ID = 900;");
        Check(!(bool)CallForm("IsPriceBelowAllowed", 900, 90m), "after the price drops to 80, 90 is allowed (fresh read)");
    }
}
