// Compiled V4/V8 UI, saved-order helper and embedded reports. Owned LocalDB only.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Reporting.WinForms;

class ItemWithholdingProbe
{
    static Assembly app;
    static Type module, helper;
    static Form form, host;
    static object context;
    static int checks;
    static BindingFlags stat = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static BindingFlags inst = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++; Console.WriteLine("PASS: " + name); }
    static void Sql(SqlConnection cx, string sql) { using (var cmd = cx.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); } }
    static object Value(string name) { return module.GetField(name, stat).GetValue(null); }
    static void Set(string name, object value) { module.GetField(name, stat).SetValue(null, value); }
    static Control Control(string name) { return (Control)form.GetType().GetProperty(name, inst).GetValue(form, null); }
    static string Text(string name) { return Control(name).Text; }
    static void Refresh() { form.GetType().GetMethod("UpdateWithholdingSuggestions", inst).Invoke(form, null); }
    static string Reference(int id, string subtotal, string template = "Work Order")
    { return (string)helper.GetMethod("Load").Invoke(null, new object[] { context, id, subtotal, template }); }
    static void AddLine(DataGridView grid, int id, decimal price, decimal qty)
    { int row = grid.Rows.Add(); grid.Rows[row].Cells["ItemID"].Value = id; grid.Rows[row].Cells["Price"].Value = price; grid.Rows[row].Cells["QTY"].Value = qty; grid.Rows[row].Cells[10].Value = 1; }

    [STAThread]
    static int Main(string[] args)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-PH");
        string database = "SOD_ItemWTax_" + Guid.NewGuid().ToString("N"), output = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(output);
        bool created = false;
        using (var cx = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20"))
        try
        {
            app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            module = app.GetType("WorkOrderEntry.ModConnectDB", true);
            helper = app.GetType("WorkOrderEntry.WorkOrderWithholdingReference", true);
            cx.Open(); Sql(cx, "CREATE DATABASE [" + database + "]"); created = true; cx.ChangeDatabase(database);
            Sql(cx, @"CREATE TABLE dbo.Item(ID int PRIMARY KEY,Notes ntext NULL);
CREATE TABLE dbo.Customer(ID int PRIMARY KEY,CustomNumber3 float,CustomNumber4 float,CustomNumber5 float,CustomText2 nvarchar(30));
CREATE TABLE dbo.CustomCaption(Style int,Caption nvarchar(80));
CREATE TABLE dbo.[Order](ID int PRIMARY KEY,CustomerID int,[Type] int,Total money,Tax money,Taxable bit,Closed bit,Comment nvarchar(max));
CREATE TABLE dbo.OrderEntry(OrderID int,ItemID int,Price money,QuantityOnOrder float);
CREATE TABLE dbo.[Transaction](TransactionNumber int,RecallID int);
CREATE TABLE dbo.TransactionEntry(TransactionNumber int,ItemID int,Price money,Quantity float);
INSERT dbo.Item VALUES(1,N'[WTAX:GOODS]'),(2,N'[WTAX:SERVICES]'),(3,N'[WTAX:EWT15]'),(4,N'[WTAX:GIFT_CERT]'),(5,N'ordinary notes'),(6,N'[WTAX:GOODS] [WTAX:SERVICES]');
INSERT dbo.Customer VALUES(1,1,2,5,N'15'),(2,0,2,0,N''),(3,0,0,0,N'');
INSERT dbo.CustomCaption VALUES(8,N'W/Tax 1%'),(9,N'W/Tax 2%'),(10,N'W/Tax 5%'),(2,N'W/Tax 15%');
INSERT dbo.[Order] VALUES(100,1,2,1680,180,1,0,N'CASH; Pick-up;'),(101,2,2,1680,180,1,0,N'CHARGE; Delivery;'),(102,1,3,1680,180,1,0,N'CASH;'),(103,3,2,1680,180,1,0,N'CASH;');
INSERT dbo.OrderEntry VALUES(100,1,1120,1),(100,2,560,1),(101,1,1120,1),(101,2,560,1);");
            Sql(cx, "ALTER TABLE dbo.Customer ADD AccountNumber nvarchar(20) NOT NULL DEFAULT ''; ALTER TABLE dbo.[Order] ADD ShippingNotes ntext NOT NULL DEFAULT '';");
            string endpoint = "Server=(localdb)\\MSSQLLocalDB;Database=" + database + ";Integrated Security=True;Pooling=False";
            context = Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemLookUpDataContext"), new object[] { endpoint });
            var query = context.GetType().GetMethods().Single(m => m.Name == "ExecuteQuery" && m.IsGenericMethodDefinition);
            foreach (string dto in new[] { "WithholdingOrderCustomer", "WithholdingItemLine" })
            {
                string sql = dto == "WithholdingOrderCustomer" ? "SELECT CustomerID,Taxable,Closed,Comment AS OrderComment FROM dbo.[Order] WHERE ID=100" : "SELECT e.ItemID,CONVERT(decimal(28,8),e.Price*e.QuantityOnOrder) AS GrossAmount,CONVERT(nvarchar(max),i.Notes) AS Notes FROM dbo.OrderEntry e JOIN dbo.Item i ON i.ID=e.ItemID WHERE e.OrderID=100";
                var found = (System.Collections.IEnumerable)query.MakeGenericMethod(app.GetType("WorkOrderEntry." + dto)).Invoke(context, new object[] {sql, new object[0]});
                Check(found.Cast<object>().Count() > 0, "Synthetic DTO query " + dto);
            }
            string mixed = Reference(100, "1500.00");
            Sql(cx, "UPDATE dbo.OrderEntry SET Price=1100 WHERE OrderID=100 AND ItemID=1; UPDATE dbo.[Order] SET Total=1660,Tax=177.86 WHERE ID=100");
            string userMixed = Reference(100, "1482.14");
            Check(userMixed == "WTax 1%: 9.82" + Environment.NewLine + "WTax 2%: 10.00" + Environment.NewLine + "WTax 5%: 74.11" + Environment.NewLine + "Total WTax: 93.93" + Environment.NewLine + "Est. Cash Due: 1,566.07", "User's goods/services example retains exact reference arithmetic");
            Sql(cx, "UPDATE dbo.OrderEntry SET Price=1120 WHERE OrderID=100 AND ItemID=1; UPDATE dbo.[Order] SET Total=1680,Tax=180 WHERE ID=100");
            Sql(cx, "INSERT dbo.[Order](ID,CustomerID,[Type],Total,Tax,Taxable,Closed,Comment) VALUES(104,1,2,1680,0,0,0,N'CASH;'); INSERT dbo.OrderEntry VALUES(104,1,1120,1),(104,2,560,1);");
            string exemptReference = Reference(104, "1680.00");
            Check(exemptReference.Contains("Est. Cash Due: 1,657.60") && !exemptReference.Contains("WTax 5%"), "Exempt sample preserves income withholding without VAT withholding");
            Console.WriteLine("Synthetic saved reference: [" + mixed + "]");
            Check(mixed.Contains("WTax 1%: 10.00") && mixed.Contains("WTax 2%: 10.00"), "Saved mixed order uses separate 1000/500 bases");
            Check(mixed.Contains("WTax 5%: 75.00") && !mixed.Contains("WTax 15%:"), "Customer-enabled 5% needs no item marker; unmatched income rates stay hidden");
            foreach (string notes in new[]{null,"","ordinary shipping note","[WTAX:REVIEW]"}) {
                using(var cmd=cx.CreateCommand()){cmd.CommandText="UPDATE dbo.Item SET Notes=@notes WHERE ID=1";cmd.Parameters.AddWithValue("@notes",(object)notes??DBNull.Value);cmd.ExecuteNonQuery();}
                Check(Reference(100,"1500.00")==mixed,"Saved reprint defaults blank/ordinary/retired REVIEW notes to goods");
            }
            Sql(cx,"UPDATE dbo.Item SET Notes=N'[WTAX:GOODS]' WHERE ID=1");
            string charge = Reference(101, "1500.00");
            Check(charge == "WTax 2%_Charge: 10.00" + Environment.NewLine + "Total WTax: 10.00" + Environment.NewLine + "Est. Charge Due: 1,670.00", "Saved customer eligibility and net CHARGE amount preserved");
            Check(mixed.EndsWith("Est. Cash Due: 1,585.00") && mixed.Split(new[]{Environment.NewLine}, StringSplitOptions.None).Length == 5, "CASH subtracts enabled item withholding and prints one amount per line");
            Check(Reference(101, "0.01") == "", "Rounded zero withholding hides payment and WTax lines");
            var reviewOnly = Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemWithholdingSummary"));
            reviewOnly.GetType().GetProperty("ReviewLines").SetValue(reviewOnly, 1, null);
            string reviewOnlyText = (string)app.GetType("WorkOrderEntry.WorkOrderWithholdingReference").GetMethod("FormatItemAmounts").Invoke(null, new object[] { reviewOnly, new int[] {1,2,5,15}, "CASH", 1680m });
            Check(reviewOnlyText.StartsWith("Needs review:") && !reviewOnlyText.Contains("CASH:") && !reviewOnlyText.Contains("WTax 1%:"), "Review warning remains without payment or zero amounts");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:GOODS]' WHERE ID=2");
            Check(Reference(101, "1500.00") == "", "Enabled customer rate with no matching base hides payment");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:SERVICES]' WHERE ID=2");
            foreach (string card in new[] { "DEBIT", "CREDIT" }) {
                Sql(cx, "UPDATE dbo.[Order] SET Comment=N'" + card + " CARD; Pick-up;' WHERE ID=100");
                Check(Reference(100,"1500.00") == mixed.Replace("Est. Cash Due:","Est. " + (card=="DEBIT"?"Debit":"Credit") + " Due:"), card + " uses saved payment and same net amount");
            }
            Sql(cx, "UPDATE dbo.[Order] SET Comment=N'CASH; Pick-up;' WHERE ID=100");
            Check(Reference(102, "1500.00") == "" && Reference(100, "1500.00", "Sales Quotation") == "", "Quotations excluded by stored type and template");
            Check(Reference(103, "1500.00") == "" && Reference(999, "1500.00") == "", "No-rate customer and missing order hide payment");
            Sql(cx, "UPDATE dbo.Customer SET CustomNumber5=0 WHERE ID=1");
            Check(!Reference(100,"1500.00").Contains("WTax 5%:"), "Customer disabling 5% hides it without changing item notes");
            Sql(cx, "UPDATE dbo.Customer SET CustomNumber5=5 WHERE ID=1");
            Check(Reference(100,"1500.00").Contains("WTax 5%: 75.00"), "Customer-only re-enable takes effect on reprint without item VAT5");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:GOODS] [WTAX:VAT5]' WHERE ID=1");
            Check(Reference(100, "1500.00").Contains("WTax 5%: 75.00"), "Legacy VAT5 marker does not add a second 5% amount");
            Sql(cx, "UPDATE dbo.[Order] SET Closed=1,Total=784,Tax=84 WHERE ID=100; INSERT dbo.[Transaction] VALUES(500,100); INSERT dbo.TransactionEntry VALUES(500,2,560,1),(500,3,224,1);");
            string settled = Reference(100, "700.00");
            Check(!settled.Contains("WTax 1%:") && settled.Contains("WTax 2%: 10.00") && settled.Contains("WTax 15%: 30.00"), "Closed reprint follows the report's settled transaction lines, not stale order lines");
            Sql(cx, "UPDATE dbo.[Order] SET Closed=0,Total=4144,Tax=444 WHERE ID=100; INSERT dbo.OrderEntry VALUES(100,3,2240,1),(100,4,112,1),(100,5,112,1);");
            string review = Reference(100, "3700.00");
            Check(review.Contains("WTax 1%: 11.00") && review.Contains("WTax 2%: 10.00") && review.Contains("WTax 15%: 300.00"), "Ordinary notes default to goods while gifts remain excluded");
            Check(!review.Contains("Needs review:"), "Unmarked items no longer produce a review warning");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:GOODS] [WTAX:SERVICES]' WHERE ID=5");
            Check(Reference(100, "3700.00").Contains("Needs review: 1"), "Conflicting marker excludes line on reprint");
            Sql(cx, "UPDATE dbo.[Order] SET Taxable=0 WHERE ID=100");
            Check(Reference(100, "3700.00").Contains("WTax 5%: 185.00"), "Printed VAT eligibility follows actual tax amount despite a stale header flag");
            Sql(cx, "UPDATE dbo.[Order] SET Tax=0 WHERE ID=100");
            Check(!Reference(100, "4144.00").Contains("WTax 5%:"), "Exempt saved order hides zero VAT5");
            Sql(cx, "UPDATE dbo.[Order] SET Taxable=1,Tax=444 WHERE ID=100");
            Check(Reference(100, "invalid") == "", "Invalid subtotal cannot produce a reference");

            // Configure all app contexts only inside this test process, before creating its hidden form.
            foreach (string name in new[] { "db", "dbnew", "dbInitial" })
            { var old = Value(name); if (old != null) { var connection = (IDbConnection)old.GetType().GetProperty("Connection").GetValue(old, null); if (connection.State != ConnectionState.Closed) throw new Exception("Unexpected app connection"); ((IDisposable)old).Dispose(); } Set(name, null); }
            Set("dbs", "(localdb)\\MSSQLLocalDB;Integrated Security=True;Pooling=False"); Set("dbn", database); Set("dbu", ""); Set("dbp", ""); Set("iCusID", 0); Set("bTaxExcempt", false);
            foreach (string name in new[] { "db", "dbnew", "dbInitial" }) Set(name, Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemLookUpDataContext"), new object[] { endpoint }));
            form = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmItemLookUp"));
            var quote = (CheckBox)Control("chkQuote");
            var legacy = form.GetType().GetMethod("chkQuote_CheckedChanged", inst);
            if (legacy != null) quote.CheckedChanged -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), form, legacy);
            typeof(Control).GetMethod("CreateHandle", inst).Invoke(form, null); // Never Show/Load the production form.
            int footerHeight = form.GetType().GetProperty("panelTotalsContainer", inst) != null ? 171 : 155;
            host = new Form { StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000), ShowInTaskbar = false, ClientSize = new Size(1212, footerHeight) };
            var footer = Control("panelTotal"); host.Controls.Add(footer); host.Show();
            var grid = (DataGridView)Control("gridSelectItem"); grid.AllowUserToAddRows = false;
            AddLine(grid, 1, 1120, 1); AddLine(grid, 2, 560, 1);
            ((CheckBox)Control("chkWorkOrder")).Checked = true; quote.Checked = false;
            Set("iCusID", 1); Control("txtCustomer").Text = "Fixture customer";
            form.GetType().GetMethod("UpdateAmt").Invoke(form, null); Refresh();
            Check(Text("txtSub") == "1,500.00" && Text("txtVat") == "180.00" && Text("txtTotal") == "1,680.00", "Actual UpdateAmt preserves taxable subtotal/VAT/total");
            Check(Text("lblWithholding1") == "1%: 10.00" && Text("lblWithholding2") == "2%: 10.00" && Text("lblWithholding5") == "5%: 75.00", "Actual UI uses item notes and matches print amounts");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:EWT15]' WHERE ID=2"); Refresh();
            Check(Text("lblWithholding2") == "2%: 0.00" && Text("lblWithholding15") == "15%: 75.00", "Marker edits with unchanged subtotal refresh without stale per-item cache");
            Check(!Control("lblWithholding2").Visible && Control("lblWithholding15").Visible && ((TableLayoutPanel)Control("withholdingLayout")).GetColumn(Control("lblWithholding15")) == 2, "Zero service amount hides and nonzero 15 percent packs left");
            Sql(cx, "UPDATE dbo.Item SET Notes=N'[WTAX:SERVICES]' WHERE ID=2");
            Set("bTaxExcempt", true); form.GetType().GetMethod("UpdateAmt").Invoke(form, null); Refresh();
            Check(Text("txtSub") == "1,680.00" && Text("txtVat") == "0.00" && Text("txtTotal") == "1,680.00", "Exempt customer keeps existing zero-VAT totals");
            Check(Text("lblWithholding1") == "1%: 11.20" && Text("lblWithholding2") == "2%: 11.20" && Text("lblWithholding5") == "5%: 0.00", "Exempt UI retains income withholding but excludes VAT5");
            Check(!Control("lblWithholding5").Visible, "Zero exempt VAT withholding is hidden on screen");
            Set("bTaxExcempt", false); AddLine(grid, 5, 112, 1); form.GetType().GetMethod("UpdateAmt").Invoke(form, null); Refresh();
            Check(Text("lblWithholdingBasis").Contains("NEEDS REVIEW: 1"), "UI still identifies conflicting explicit markers");
            Check(Text("lblWithholding1") == "1%: 10.00" && Text("lblWithholding2") == "2%: 10.00", "Conflicting markers still do not inflate classified bases");
            Set("iCusID", 2); Refresh();
            Check(!Control("lblWithholding1").Visible && Control("lblWithholding2").Visible && !Control("lblWithholding5").Visible, "Customer's enabled rates still control displayed suggestions");
            Set("iCusID", 3); Refresh(); Check(!Control("grpWithholding").Visible, "No enabled customer flags hides reference");
            Set("iCusID", 1); Refresh(); ((CheckBox)Control("chkWorkOrder")).Checked = false; quote.Checked = true; Refresh();
            Check(!Control("grpWithholding").Visible, "Quotation display stays excluded");
            quote.Checked = false; ((CheckBox)Control("chkWorkOrder")).Checked = true; Refresh();
            for (int mask = 0; mask < 16; mask++)
            {
                Sql(cx, "UPDATE dbo.Customer SET CustomNumber3=" + ((mask & 1) != 0 ? 1 : 0) + ",CustomNumber4=" + ((mask & 2) != 0 ? 2 : 0) + ",CustomNumber5=" + ((mask & 4) != 0 ? 5 : 0) + ",CustomText2=N'" + ((mask & 8) != 0 ? "15" : "") + "' WHERE ID=1");
                Set("iCusID", 0); Refresh(); Set("iCusID", 1); Refresh();
                var layout = (TableLayoutPanel)Control("withholdingLayout"); int col = 0;
                for (int n = 0; n < 4; n++) { var label = Control(new[] { "lblWithholding1", "lblWithholding2", "lblWithholding5", "lblWithholding15" }[n]); bool displayed = (mask & (1 << n)) != 0 && n != 3; Check(displayed ? label.Visible && layout.GetColumn(label) == col++ : !label.Visible, "Only nonzero enabled rate packs left mask=" + mask + " index=" + n); }
            }
            Sql(cx, "UPDATE dbo.Customer SET CustomNumber3=0,CustomNumber4=2,CustomNumber5=0,CustomText2=N'' WHERE ID=1");
            Set("iCusID", 0); Refresh(); Set("iCusID", 1); Control("txtSub").Text="0.01"; Refresh();
            Check(!Control("lblWithholding2").Visible, "Amount rounded to zero hides on screen");
            Control("txtSub").Text="1600.00"; Refresh();
            Check(Control("lblWithholding2").Visible && ((TableLayoutPanel)Control("withholdingLayout")).GetColumn(Control("lblWithholding2")) == 0, "Nonzero amount returns in first column after rounded zero");
            form.GetType().GetMethod("UpdateAmt").Invoke(form, null); Refresh();
            Sql(cx, "UPDATE dbo.Customer SET CustomNumber3=1,CustomNumber4=2,CustomNumber5=5,CustomText2=N'15' WHERE ID=1");
            Set("iCusID", 0); Refresh(); Set("iCusID", 1); Refresh();
            foreach (int width in new[] { 1212, 1446, 1766 })
            { host.ClientSize = new Size(width, footerHeight); Refresh(); host.PerformLayout(); footer.PerformLayout(); using (var bitmap = new Bitmap(footer.Width, footer.Height)) { footer.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(output, "UI-" + width + ".png")); } Check(Control("grpWithholding").Bottom < Control("GroupBox2").Top, "Reference remains above totals at width " + width); }
            Render(output, "MixedCash", mixed, "1500.00", "180.00", "1680.00", "Work Order", 2);
            Render(output, "UserMixed", userMixed, "1482.14", "177.86", "1660.00", "Work Order", 2);
            Render(output, "Exempt", exemptReference, "1680.00", "0.00", "1680.00", "Work Order", 2);



            Render(output, "Charge", charge, "1500.00", "180.00", "1680.00", "Work Order", 2);
            Render(output, "Review", review, "3700.00", "444.00", "4144.00", "Work Order", 5);
            Render(output, "MultiPage", review, "3700.00", "444.00", "4144.00", "Work Order", 60);
            Render(output, "Quote", mixed, "1500.00", "180.00", "1680.00", "Sales Quotation", 2);
            Render(output, "NoReference", "", "1500.00", "180.00", "1680.00", "Work Order", 2);
            using (var cmd = cx.CreateCommand()) { cmd.CommandText = "SELECT COUNT(*) FROM dbo.[Order] WHERE ID=100 AND Total=4144 AND Tax=444"; Check((int)cmd.ExecuteScalar() == 1, "Reference/UI work did not write saved totals"); }
            if (helper.GetMethod("WriteWalkInRates") != null) TestWalkIn(cx, output);
            Console.WriteLine("PASS: " + checks + " actual compiled item-marker/UI/print checks; owned fixture only, no POS or physical printing."); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally
        {
            if (host != null) host.Dispose(); if (form != null) form.Dispose(); if (context is IDisposable) ((IDisposable)context).Dispose();
            if (module != null) foreach (string name in new[] { "db", "dbnew", "dbInitial" }) { var value = Value(name); if (value is IDisposable) ((IDisposable)value).Dispose(); Set(name, null); }
            if (created) { cx.ChangeDatabase("master"); Sql(cx, "ALTER DATABASE [" + database + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + database + "]"); }
        }
    }

    static void TestWalkIn(SqlConnection cx, string output)
    {
        Sql(cx, @"INSERT dbo.Customer(ID,CustomNumber3,CustomNumber4,CustomNumber5,CustomText2,AccountNumber) VALUES(9,0,0,0,N'',N'WALK-IN');
INSERT dbo.[Order](ID,CustomerID,[Type],Total,Tax,Taxable,Closed,Comment,ShippingNotes) VALUES(109,9,2,1680,180,1,0,N'CASH; Pick-up;',N'Deliver to reception');
INSERT dbo.OrderEntry VALUES(109,1,1120,1),(109,2,560,1);");
        Set("iCusID", 9); Refresh();
        var button = (Button)form.GetType().GetField("btnWalkInWithholding", inst).GetValue(form);
        Check(button != null && (bool)typeof(Control).GetMethod("GetState", inst).Invoke(button, new object[] {2}), "WALK-IN button requested visible with no customer rates");
        bool cancelPicker = false;
        using (var timer = new Timer { Interval = 25 })
        {
            timer.Tick += delegate {
                foreach (Form dialog in Application.OpenForms.Cast<Form>().ToArray()) {
                    if (dialog.Text != "WALK-IN withholding") continue;
                    foreach (var box in dialog.Controls.OfType<CheckBox>()) box.Checked = !cancelPicker && ((int)box.Tag == 1 || (int)box.Tag == 2);
                    dialog.DialogResult = cancelPicker ? DialogResult.Cancel : DialogResult.OK; dialog.Close();
                }
            };
            timer.Start();
            // Invoke the real click handler; no production main-form Load.
            form.GetType().GetMethod("SetWalkInWithholding", inst).Invoke(form, new object[]{button, EventArgs.Empty});
            cancelPicker = true;
            form.GetType().GetMethod("SetWalkInWithholding", inst).Invoke(form, new object[]{button, EventArgs.Empty});
            Check(((System.Collections.Generic.List<int>)form.GetType().GetField("walkInRates", inst).GetValue(form)).SequenceEqual(new[]{1,2}), "Cancel leaves previously selected rates unchanged");
            timer.Stop();
        }
        var merge = form.GetType().GetMethod("ShippingNotesWithWithholding", inst);
        string notes = (string)merge.Invoke(form, new object[]{"Deliver to reception"});
        Check(notes.StartsWith("Deliver to reception") && notes.Contains("RATES=1,2]"), "picker selection preserves notes and creates order marker");
        using (var cmd = cx.CreateCommand()) { cmd.CommandText = "UPDATE dbo.[Order] SET ShippingNotes=@notes WHERE ID=109"; cmd.Parameters.AddWithValue("@notes", notes); cmd.ExecuteNonQuery(); }
        string saved = Reference(109, "1500.00");
        Check(saved.Contains("WTax 1%: 10.00") && saved.Contains("WTax 2%: 10.00") && saved.Contains("1,660.00"), "saved WALK-IN reprint uses order selections without customer flags");
        form.GetType().GetField("walkInRates", inst).SetValue(form, null);
        form.GetType().GetMethod("LoadWalkInWithholding", inst).Invoke(form, new object[]{109}); Refresh();
        var recalled = (System.Collections.Generic.List<int>)form.GetType().GetField("walkInRates", inst).GetValue(form);
        Check(recalled.SequenceEqual(new[]{1,2}), "recall loader restores saved rates");
        Check((string)merge.Invoke(form, new object[]{notes}) == notes, "re-saving recalled selections preserves notes without duplicate markers");
        Sql(cx, @"UPDATE dbo.[Order] SET Closed=1 WHERE ID=109; INSERT dbo.[Transaction] VALUES(900,109); INSERT dbo.TransactionEntry VALUES(900,1,1120,1),(900,2,560,1);");
        Check(Reference(109, "1500.00") == saved, "simulated settlement retains header selections for closed-order reprint");
        Render(output, "WalkIn", saved, "1500.00", "180.00", "1680.00", "Work Order", 2);
        Set("iCusID", 1); Refresh();
        Check((string)merge.Invoke(form, new object[]{notes}) == "Deliver to reception" + Environment.NewLine, "switching to named customer removes only order override");
        Sql(cx, "UPDATE dbo.[Order] SET CustomerID=1 WHERE ID=109");
        Check(Reference(109, "1500.00").Contains("WTax 5%"), "named customer ignores leftover WALK-IN marker and uses customer settings");
        Set("iCusID", 9); Refresh();
        form.GetType().GetField("walkInRates", inst).SetValue(form, new System.Collections.Generic.List<int>());
        form.GetType().GetField("walkInRatesCustomerID", inst).SetValue(form, 9);
        string disabled = (string)merge.Invoke(form, new object[]{notes});
        Check(disabled.Contains("RATES=]"), "all unchecked selection is stored explicitly");
        using(var cmd=cx.CreateCommand()){cmd.CommandText="UPDATE dbo.[Order] SET CustomerID=9,ShippingNotes=@notes WHERE ID=109";cmd.Parameters.AddWithValue("@notes",disabled);cmd.ExecuteNonQuery();}
        Check(Reference(109,"1500.00")=="", "explicit all-off selection hides withholding on reprint");
        Sql(cx,"UPDATE dbo.Customer SET CustomNumber3=1 WHERE ID=9");
        Check(Reference(109,"1500.00")=="", "explicit all-off order overrides subsequently enabled shared customer flag");
        Sql(cx,"UPDATE dbo.Customer SET CustomNumber3=0 WHERE ID=9");
        using(var cmd=cx.CreateCommand()){cmd.CommandText="SELECT SUM(CustomNumber3+CustomNumber4+CustomNumber5) FROM dbo.Customer WHERE ID=9";Check(Convert.ToDecimal(cmd.ExecuteScalar())==0M,"shared WALK-IN customer settings unchanged");}
    }

    static void Render(string output, string name, string reference, string subtotal, string vat, string total, string template, int rows)
    {
        var xml = XDocument.Load(app.GetManifestResourceStream(app.GetManifestResourceNames().Single(n => n.EndsWith("rprtOrder.rdlc")))) ; var ns = xml.Root.Name.Namespace;
        using (var preview = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmPrintWo")))
        {
            var viewer = (ReportViewer)preview.GetType().GetProperty("rptviewer", inst).GetValue(preview, null);
            preview.GetType().GetMethod("PrintSetting", inst).Invoke(preview, null);
            var report = viewer.LocalReport;
            foreach (var set in xml.Root.Element(ns + "DataSets").Elements(ns + "DataSet"))
            { var table = new DataTable(); foreach (var field in set.Element(ns + "Fields").Elements(ns + "Field")) table.Columns.Add((string)field.Attribute("Name"), (string)field.Attribute("Name") == "QtyOrder" ? typeof(int) : typeof(string)); for (int i = 0; i < rows; i++) table.Rows.Add(table.Columns.Cast<DataColumn>().Select(c => c.DataType == typeof(int) ? (object)(name == "UserMixed" && i == 0 ? 5 : 1) : c.ColumnName == "Price" ? (name == "Rounding" ? "0.05" : name == "Zero" ? "0.00" : name == "MixedCash" || name == "Exempt" || name == "ActualVat" || name == "Charge" ? (i == 0 ? "1120.00" : "560.00") : name == "UserMixed" ? (i == 0 ? "220.00" : "560.00") : "100.00") : c.ColumnName == "Extended" ? (name == "Zero" ? "0.00" : name == "Rounding" ? "0.05" : name == "UserMixed" ? (i == 0 ? "1100.00" : "560.00") : name == "MixedCash" || name == "Exempt" || name == "ActualVat" || name == "Charge" ? (i == 0 ? "1120.00" : "560.00") : "100.00") : c.ColumnName == "Itemcode" ? "TEST" + i : "LOCAL FIXTURE " + i).ToArray()); report.DataSources.Add(new ReportDataSource((string)set.Attribute("Name"), table)); }
            foreach (var p in xml.Root.Element(ns + "ReportParameters").Elements(ns + "ReportParameter"))
            { string key = (string)p.Attribute("Name"); string value = key == "paramType" ? template : key == "paramWithholdingReference" ? reference : key == "paramSubtotal" ? decimal.Parse(subtotal).ToString("N2") : key == "paramSalesTax" ? decimal.Parse(vat).ToString("N2") : key == "paramTotal" ? decimal.Parse(total).ToString("N2") : key == "paramWOno" ? "100" : "LOCAL FIXTURE"; report.SetParameters(new ReportParameter(key, value)); }
            string device = "<DeviceInfo><PageWidth>8.5in</PageWidth><PageHeight>6in</PageHeight><MarginTop>0.1in</MarginTop><MarginLeft>0.1in</MarginLeft><MarginRight>0.1in</MarginRight><MarginBottom>0.1in</MarginBottom></DeviceInfo>";
            byte[] pdf = report.Render("PDF", device); Check(pdf.Length > 1000, name + " actual embedded PDF renders"); File.WriteAllBytes(Path.Combine(output, name + ".pdf"), pdf);
            var page = viewer.GetPageSettings(); Check(page.PaperSize.Width == 850 && page.PaperSize.Height == 650, name + " existing paper settings retained");
        }
    }
}
