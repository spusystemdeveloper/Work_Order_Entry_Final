using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// Exercises the real public validators in an isolated process, never form Load/save.
class QuantityValidationProbe
{
    static readonly BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static readonly BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static Type module;
    static Form form;
    static readonly List<string> prompts = new List<string>();
    static int checks;
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    static void Set(string name, object value) { module.GetField(name, Static).SetValue(null, value); }
    static Control Control(string name) { return (Control)form.GetType().GetProperty(name, Instance).GetValue(form, null); }
    static void Sql(SqlConnection cx, string text) { using (var c = cx.CreateCommand()) { c.CommandText = text; c.ExecuteNonQuery(); } }
    static object Scalar(SqlConnection cx, string text) { using (var c = cx.CreateCommand()) { c.CommandText = text; return c.ExecuteScalar(); } }
    static object[] Row(string code, int id, object quantity, object price) { return new object[] { code, id, quantity, price }; }
    static object Run(string name, bool workOrder, string label, int expectedErrors, object[][] rows, bool exempt = false, int initialErrors = 0)
    {
        var grid = (DataGridView)Control("gridSelectItem");
        grid.Rows.Clear(); ((ToolStripItem)form.GetType().GetProperty("lblOrderNo", Instance).GetValue(form, null)).Text = label; Set("bTaxExcempt", exempt);
        foreach (var input in rows)
        {
            var r = grid.Rows[grid.Rows.Add()];
            r.Cells[0].Value = input[0]; r.Cells[1].Value = "LOCAL QUANTITY TEST";
            r.Cells[2].Value = input[2]; r.Cells[3].Value = input[3]; r.Cells[5].Value = 0d;
            r.Cells[9].Value = 1d; r.Cells[10].Value = 1; r.Cells[11].Value = input[1];
            r.Cells[12].Value = 112d; r.Cells[13].Value = "LOCAL TEST";
            r.Cells[14].Value = ""; r.Cells[17].Value = false; r.Cells[18].Value = 0;
        }
        form.GetType().GetField("iErrCnt", Instance).SetValue(form, initialErrors);
        Set("ErrorCount", 0); prompts.Clear();
        var result = form.GetType().GetMethod(workOrder ? "ValidateAllQty" : "ValidateAllQtyForQoute").Invoke(form, null);
        int errors = (int)form.GetType().GetField("iErrCnt", Instance).GetValue(form);
        Check(errors == expectedErrors, name + " expected validation count; got " + errors);
        var state = new {
            Name = name, Result = result.ToString(), Errors = errors,
            ErrorCount = module.GetField("ErrorCount", Static).GetValue(null), Prompts = prompts.ToArray(),
            Subtotal = Control("txtSub").Text, Vat = Control("txtVat").Text, Total = Control("txtTotal").Text,
            CatalogRows = ((DataGridView)Control("gridItem")).RowCount,
            Rows = grid.Rows.Cast<DataGridViewRow>().Select(r => new {
                Code = Convert.ToString(r.Cells[0].Value), Quantity = Convert.ToString(r.Cells[2].Value),
                Price = Convert.ToString(r.Cells[3].Value), Extended = Convert.ToString(r.Cells[5].Value),
                QuantityColor = r.Cells[2].Style.ForeColor.ToArgb(), QuantitySelection = r.Cells[2].Style.SelectionForeColor.ToArgb(),
                PriceColor = r.Cells[3].Style.ForeColor.ToArgb()
            }).ToArray()
        };
        Console.WriteLine("PASS: " + name); return state;
    }

    [STAThread] static int Main(string[] args)
    {
        string database = "SOD_QtyValidation_" + Guid.NewGuid().ToString("N"); bool created = false;
        using (var cx = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False"))
        using (var timer = new Timer { Interval = 20 })
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var app = Assembly.LoadFrom(Path.GetFullPath(args[0])); module = app.GetType("WorkOrderEntry.ModConnectDB", true);
            cx.Open(); Sql(cx, "CREATE DATABASE [" + database + "]"); created = true; cx.ChangeDatabase(database);
            Sql(cx, @"CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode nvarchar(30),ItemType smallint,Quantity float,QuantityCommitted float);
CREATE TABLE dbo.[Order](ID int PRIMARY KEY,Closed bit);
CREATE TABLE dbo.OrderEntry(ID int PRIMARY KEY,OrderID int,ItemID int,QuantityOnOrder float);
CREATE TABLE dbo.SOD_WO(ID bigint,OrderID bigint,ItemID bigint,Invoice int,Status int,QtyOrder int);
INSERT dbo.Item VALUES(1,'GOODS',0,100,20),(2,'ASSEMBLY',3,10,3),(3,'SERVICE',7,0,0),(4,'OTHER',1,0,0);
INSERT dbo.[Order] VALUES(100,0),(200,1);
INSERT dbo.OrderEntry VALUES(1,100,1,20.5),(2,200,1,20);
INSERT dbo.SOD_WO VALUES(300,300,1,0,1,30);");
            Sql(cx, @"CREATE VIEW dbo.SOD_ViewItemsWO AS SELECT ItemLookupCode,'LOCAL CATALOG' AS DESCRIPTION,
CAST(112 AS money) AS PRICE,CAST(1 AS money) AS COST,Quantity-QuantityCommitted AS AVAILABLE,
0 AS ParentQuantity,1 AS TaxID,'' AS SubDescription2,CAST(112 AS money) AS [PRICE 1],ID,
'LOCAL CATALOG' AS DESCRIPTION1,'' AS ExtendedDescription,ItemType,CAST(0 AS bit) AS Inactive,
'' AS ITEMDES,'' AS SKULEVEL,ItemLookupCode AS FULLDESC,Quantity AS SOH,QuantityCommitted AS [QTY COM] FROM dbo.Item;");
            Set("dbs", "(localdb)\\MSSQLLocalDB;Integrated Security=True;Pooling=False");
            Set("dbn", database); Set("dbu", ""); Set("dbp", ""); Set("iCusID", 0);
            string endpoint = (string)module.GetMethod("DB_Conn", Static).Invoke(null, new object[] { "constr" });
            var target = new SqlConnectionStringBuilder(endpoint);
            Check(target.DataSource == "(localdb)\\MSSQLLocalDB" && target.InitialCatalog == database && target.IntegratedSecurity, "Owned LocalDB endpoint required");
            foreach (string name in new[] { "db", "dbnew", "dbInitial" })
            {
                var old = module.GetField(name, Static).GetValue(null) as IDisposable;
                if (old != null) old.Dispose();
                var context = Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemLookUpDataContext", true), new object[] { endpoint });
                Set(name, context);
            }
            form = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmItemLookUp", true));
            form.GetType().GetField("isLoadingRecalledOrder", Instance).SetValue(form, true);
            ((DataGridView)Control("gridSelectItem")).AllowUserToAddRows = false;
            timer.Tick += delegate {
                foreach (Form dialog in Application.OpenForms.Cast<Form>().ToArray())
                {
                    if (dialog.GetType().Name != "frmAppPrompt") continue;
                    var message = (Control)dialog.GetType().GetProperty("txtMessage", Instance).GetValue(dialog, null);
                    prompts.Add(dialog.Text + "|" + message.Text); dialog.DialogResult = DialogResult.OK; dialog.Close();
                }
            };
            timer.Start();
            bool formattedOnly = args.Length > 2 && args[2] == "--formatted";
            var outcomes = new List<object>();
            // October 30: an unfinished check now counts as an error so the save stops.
            // Earlier builds returned zero, which let SaveOrder continue.
            int unfinishedCheckErrors = app.GetType("WorkOrderEntry.OrderCommitmentRules", true).GetMethod("IsNegativeQuantity", Static) == null ? 0 : 1;
            if (!formattedOnly) {
            outcomes.Add(Run("NewWorkOrder", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, 5d, 112d) }));
            outcomes.Add(Run("WorkOrderShortage", true, "Work Order #: ", 1, new[] { Row("GOODS", 1, 81d, 112d) }));
            outcomes.Add(Run("CaseInsensitiveDuplicates", true, "Work Order #: ", 2, new[] { Row("GOODS", 1, 40d, 112d), Row("goods", 1, 41d, 112d) }));
            outcomes.Add(Run("RecallReservationIncludingFraction", true, "Work Order #: 100", 0, new[] { Row("GOODS", 1, 100.5d, 112d) }));
            outcomes.Add(Run("QuotationCannotReuseReservation", false, "Work Order #: 100", 1, new[] { Row("GOODS", 1, 100.5d, 112d) }));
            outcomes.Add(Run("ClosedOrderNoReservation", true, "Work Order #: 200", 1, new[] { Row("GOODS", 1, 90d, 112d) }));
            outcomes.Add(Run("LegacySodReservation", true, "Work Order #: 300", 0, new[] { Row("GOODS", 1, 110d, 112d) }));
            outcomes.Add(Run("QuoteNew", false, "Sales Quotation #: ", 0, new[] { Row("GOODS", 1, 80d, 112d) }));
            outcomes.Add(Run("QuoteDuplicates", false, "Sales Quotation #: 100", 2, new[] { Row("GOODS", 1, 40d, 112d), Row("GOODS", 1, 41d, 112d) }));
            outcomes.Add(Run("TypeThreeShortage", true, "Work Order #: ", 1, new[] { Row("ASSEMBLY", 2, 8d, 112d) }));
            outcomes.Add(Run("NonInventory", true, "Work Order #: ", 0, new[] { Row("SERVICE", 3, 999d, 112d) }));
            outcomes.Add(Run("OtherType", false, "Sales Quotation #: ", 0, new[] { Row("OTHER", 4, 999d, 112d) }));
            outcomes.Add(Run("FractionalQuantity", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, 2.5d, 112d) }));
            outcomes.Add(Run("TaxExemptTotals", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, 2.5d, 112d) }, true));
            outcomes.Add(Run("ZeroQuantity", true, "Work Order #: ", 1, new[] { Row("SERVICE", 3, 0d, 112d) }));
            outcomes.Add(Run("NegativeQuantity", false, "Sales Quotation #: ", 1, new[] { Row("SERVICE", 3, -1d, 112d) }));
            outcomes.Add(Run("InvalidQuantity", true, "Work Order #: ", 2, new[] { Row("SERVICE", 3, "bad", 112d) }));
            outcomes.Add(Run("InvalidPrice", false, "Sales Quotation #: ", 1, new[] { Row("SERVICE", 3, 1d, "bad") }));
            outcomes.Add(Run("UnconfirmedPriceEarlyReturn", true, "Work Order #: ", unfinishedCheckErrors, new[] { Row("GOODS", 1, 1d, "-") }));
            outcomes.Add(Run("ExistingErrorCounter", true, "Work Order #: ", 5, new[] { Row("GOODS", 1, 81d, 112d) }, false, 4));
            outcomes.Add(Run("InvalidRecallLabel", true, "Work Order #: unknown", 1, new[] { Row("GOODS", 1, 81d, 112d) }));
            outcomes.Add(Run("EmptyGrid", false, "Sales Quotation #: ", 0, new object[0][]));
            outcomes.Add(Run("WorkOrderCaughtException", true, "Work Order #: ", unfinishedCheckErrors, new[] { Row("GOODS", 1, DBNull.Value, 112d) }));
            outcomes.Add(Run("QuotationCaughtException", false, "Sales Quotation #: ", unfinishedCheckErrors, new[] { Row("GOODS", 1, DBNull.Value, 112d) }));
            } else {
                // Same amounts as the reported local incident, isolated from store data.
                Sql(cx, "UPDATE dbo.Item SET Quantity=71202,QuantityCommitted=2692 WHERE ID=1;");
                outcomes.Add(Run("FormattedConversionShortage", false, "Sales Quotation #: 100", 1, new[] { Row("GOODS", 1, "100,000", 178d) }));
                outcomes.Add(Run("NumericConversionShortage", false, "Sales Quotation #: 100", 1, new[] { Row("GOODS", 1, 100000d, 178d) }));
                outcomes.Add(Run("FormattedWithinStock", false, "Sales Quotation #: 100", 0, new[] { Row("GOODS", 1, "60,000", 178d) }));
                outcomes.Add(Run("FormattedDuplicates", false, "Sales Quotation #: 100", 2, new[] { Row("GOODS", 1, "40,000", 178d), Row("GOODS", 1, "40,000", 178d) }));
                outcomes.Add(Run("FormattedFraction", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, "1,000.5", 178d) }));
                outcomes.Add(Run("PrepareActualConversion", false, "Sales Quotation #: 100", 1, new[] { Row("GOODS", 1, "100,000", 178d) }));
                ((CheckBox)Control("chkQuote")).Checked = true;
                ((CheckBox)Control("chkBoxQtoWo")).Checked = true;
                form.GetType().GetMethod("convertToWorkOrder").Invoke(form, null);
                Control("cboPayment").Text = "CASH";
                prompts.Clear();
                object result = form.GetType().GetMethod("SaveOrder", Instance).Invoke(form, null);
                Check(result.ToString() == "ValidationFailed", "Actual conversion save rejects formatted shortage before database writes");
                Check(prompts.Count == 1 && prompts[0].Contains("Not enough stock for:") && prompts[0].Contains("available 68510, ordered 100000"), "Actual conversion names the stock shortage");
                Check(Convert.ToDouble(Scalar(cx, "SELECT QuantityCommitted FROM dbo.Item WHERE ID=1")) == 2692d, "Rejected conversion leaves prior commitments unchanged");
                // Available stock keeps its decimals, within float rounding only.
                Sql(cx, "UPDATE dbo.Item SET Quantity=0.6,QuantityCommitted=0 WHERE ID=1;");
                outcomes.Add(Run("FractionalStockShortage", true, "Work Order #: ", 1, new[] { Row("GOODS", 1, 0.8d, 178d) }));
                Check(prompts.Count == 1 && prompts[0].Contains("available 0.6, ordered 0.8"), "Shortage message shows exact fractional amounts");
                outcomes.Add(Run("FractionalStockExactMatch", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, 0.6d, 178d) }));
                Sql(cx, "UPDATE dbo.Item SET Quantity=1,QuantityCommitted=0.9 WHERE ID=1;");
                outcomes.Add(Run("FractionalFloatRemainder", true, "Work Order #: ", 0, new[] { Row("GOODS", 1, 0.1d, 178d) }));
                outcomes.Add(Run("FractionalRealShortage", true, "Work Order #: ", 1, new[] { Row("GOODS", 1, 0.11d, 178d) }));
                // Legacy rows match by OrderID and item; another row's own ID must not.
                Sql(cx, "UPDATE dbo.Item SET Quantity=100,QuantityCommitted=20 WHERE ID=1;");
                Sql(cx, "INSERT dbo.SOD_WO VALUES(900,400,1,0,1,30),(100,999,1,0,1,5);");
                outcomes.Add(Run("LegacyRowsMatchByOrderID", true, "Work Order #: 400", 0, new[] { Row("GOODS", 1, 110d, 178d) }));
                outcomes.Add(Run("LegacyRowIDIsNotAnOrderID", true, "Work Order #: 100", 0, new[] { Row("GOODS", 1, 100.5d, 178d) }));
                Sql(cx, "DELETE dbo.SOD_WO WHERE ID IN(900,100);");
                // A non-inventory item has no stock check, so these limits are its only ceiling.
                outcomes.Add(Run("ServiceItemAtQuantityLimit", true, "Work Order #: ", 0, new[] { Row("SERVICE", 3, 999999d, 1d) }));
                var limitGrid = (DataGridView)Control("gridSelectItem");
                var save = form.GetType().GetMethod("SaveOrder", Instance);
                limitGrid.Rows[0].Cells[2].Value = 123123123213123213d; prompts.Clear();
                Check(save.Invoke(form, null).ToString() == "ValidationFailed", "Save rejects a service quantity over the limit");
                Check(prompts.Count == 1 && prompts[0].Contains("Quantity cannot be more than 999,999."), "Save names the quantity limit");
                limitGrid.Rows[0].Cells[2].Value = 999999d; limitGrid.Rows[0].Cells[3].Value = 1000000000d; prompts.Clear();
                Check(save.Invoke(form, null).ToString() == "ValidationFailed", "Save rejects an order total the money column cannot store");
                Check(prompts.Count == 1 && prompts[0].Contains("order total is too large"), "Save names the total limit");
                // The actual quantity-cell handlers: starting an edit must not overflow.
                limitGrid.Rows[0].Cells[3].Value = 1d; limitGrid.Rows[0].Cells[2].Value = 123123123213123213d; prompts.Clear();
                var begin = form.GetType().GetMethod("gridSelectItem_CellBeginEdit", Instance);
                begin.Invoke(form, new object[] { limitGrid, new DataGridViewCellCancelEventArgs(2, 0) });
                Check(prompts.Count == 0, "Starting to edit a very large quantity shows no overflow error");
                limitGrid.Rows[0].Cells[2].Value = 2.5d;
                begin.Invoke(form, new object[] { limitGrid, new DataGridViewCellCancelEventArgs(2, 0) });
                limitGrid.Rows[0].Cells[2].Value = "123123123213123213"; prompts.Clear();
                form.GetType().GetMethod("gridSelectItem_CellEndEdit", Instance).Invoke(form, new object[] { limitGrid, new DataGridViewCellEventArgs(2, 0) });
                Check(prompts.Count == 1 && prompts[0].Contains("Quantity cannot be more than 999,999."), "Typing over the limit shows the limit message");
                Check(Convert.ToDouble(limitGrid.Rows[0].Cells[2].Value) == 2.5d, "Typing over the limit restores the exact fractional quantity");
            }
            Check(Convert.ToDouble(Scalar(cx, "SELECT SUM(Quantity) FROM dbo.Item")) == 110d && Convert.ToDouble(Scalar(cx, "SELECT SUM(QuantityCommitted) FROM dbo.Item")) == 23d, "Physical stock and commitments unchanged");
            Check(Convert.ToInt32(Scalar(cx, "SELECT COUNT(*) FROM dbo.OrderEntry")) == 2, "No order writes");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1])));
            File.WriteAllText(args[1], new JavaScriptSerializer().Serialize(outcomes));
            Console.WriteLine("PASS: " + checks + " checks, " + outcomes.Count + " real-validator cases; only owned LocalDB fixture writes; no production form Load or store save.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally
        {
            timer.Stop(); if (form != null) form.Dispose();
            foreach (string name in new[] { "db", "dbnew", "dbInitial" }) if (module != null) { var context = module.GetField(name, Static).GetValue(null) as IDisposable; if (context != null) context.Dispose(); }
            SqlConnection.ClearAllPools(); if (created) { cx.ChangeDatabase("master"); Sql(cx, "ALTER DATABASE [" + database + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + database + "]"); }
        }
    }
}
