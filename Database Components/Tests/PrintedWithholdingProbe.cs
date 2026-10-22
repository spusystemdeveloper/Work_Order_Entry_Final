// Actual compiled helpers and embedded reports; owned LocalDB only, no print jobs.
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Reporting.WinForms;

class PrintedWithholdingProbe
{
    static int checks;
    static Assembly app;
    static Type helper;
    static object context;
    static string output, baseline;
    static Dictionary<string, int> printablePages = new Dictionary<string, int>();
    static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static void Check(bool pass, string message) { if (!pass) throw new Exception(message); checks++; }
    static string Reference(int id, string subtotal, string template = "Work Order")
    { return (string)helper.GetMethod("Load").Invoke(null, new[] {context, (object)id, subtotal, template}); }
    static void Sql(SqlConnection cx, string sql)
    { using (var cmd = cx.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); } }

    [STAThread]
    static int Main(string[] args)
    {
        string db = "SOD_PrintedWithholding_" + Guid.NewGuid().ToString("N");
        bool created = false;
        using (var cx = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20"))
        try
        {
            output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output); baseline = args[2];
            app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            helper = app.GetType("WorkOrderEntry.WorkOrderWithholdingReference", true);
            var currentXml = XDocument.Load(app.GetManifestResourceStream(app.GetManifestResourceNames().Single(n => n.EndsWith("rprtOrder.rdlc"))));
            var ns = currentXml.Root.Name.Namespace;
            var box = currentXml.Descendants(ns + "Textbox").Single(n => (string)n.Attribute("Name") == "WithholdingReference");
            Check(box.Value.Contains("Withholding Tax (Reference)"), "Compiled template contains reference section");
            box.Remove();
            currentXml.Root.Element(ns + "ReportParameters").Elements(ns + "ReportParameter").Single(n => (string)n.Attribute("Name") == "paramWithholdingReference").Remove();
            Check(XNode.DeepEquals(currentXml, XDocument.Load(baseline)), "Every pre-existing report element/expression unchanged");
            cx.Open(); Sql(cx, "CREATE DATABASE [" + db + "]"); created = true; cx.ChangeDatabase(db);
            Sql(cx, @"CREATE TABLE dbo.[Order](ID int PRIMARY KEY,CustomerID int,[Type] int,Total money,Tax money);
CREATE TABLE dbo.Customer(ID int PRIMARY KEY,CustomNumber3 float NOT NULL,CustomNumber4 float NOT NULL,CustomNumber5 float NOT NULL,CustomText2 nvarchar(30) NOT NULL);
CREATE TABLE dbo.CustomCaption(Style int,Caption nvarchar(80));
INSERT dbo.CustomCaption VALUES(2,N'W/Tax 15%'),(8,N'W/Tax 1%'),(9,N'W/Tax 2%'),(10,N'W/Tax 5%');
INSERT dbo.Customer VALUES(10,1,2,5,N'15'),(20,0,0,0,N'15'),(30,0,0,0,N'');
INSERT dbo.[Order] VALUES(100,10,2,1100,117.86),(101,20,2,1100,117.86),(102,10,3,1100,117.86),(103,30,2,1100,117.86);");
            var builder = new SqlConnectionStringBuilder(cx.ConnectionString) {InitialCatalog = db};
            Check(builder.DataSource == "(localdb)\\MSSQLLocalDB" && builder.IntegratedSecurity, "Owned localhost endpoint required");
            context = Activator.CreateInstance(app.GetType("WorkOrderEntry.ItemLookUpDataContext", true), new object[] {builder.ConnectionString});
            string all = Reference(100, "982.14");
            Check(all == "1%: 9.82     2%: 19.64" + Environment.NewLine + "5%: 49.11     15%: 147.32", "Each enabled rate uses printed VAT-exclusive subtotal");
            Check(Reference(101, "982.14") == "15%: 147.32", "Saved order's different customer supplies its sole enabled rate");
            Check(Reference(100, "1,100.00").Contains("1%: 11.00"), "Actual tax-exempt subtotal is not divided by VAT again");
            Check(Reference(102, "982.14") == "", "Stored quotation is excluded even with Work Order template name");
            Check(Reference(100, "982.14", "Sales Quotation") == "", "Quotation template excluded");
            Check(Reference(103, "982.14") == "", "No enabled rates excluded");
            Check(Reference(999, "982.14") == "" && Reference(0, "982.14") == "", "Missing order excluded");
            Check(Reference(100, "invalid") == "", "Invalid basis excluded");
            Check(Reference(101, "-0.10") == "15%: -0.02", "Signed rounding retained");
            Check(Reference(101, "0.00") == "15%: 0.00", "Zero reference remains a valid amount");
            Sql(cx, "UPDATE dbo.CustomCaption SET Caption=N'Credit Terms' WHERE Style=2");
            Check(Reference(101, "982.14") == "", "Unrelated branch caption excluded");
            Sql(cx, "UPDATE dbo.CustomCaption SET Caption=N'W/Tax 15%' WHERE Style=2; INSERT dbo.CustomCaption VALUES(2,N'W/Tax 15%');");
            Check(Reference(101, "982.14") == "", "Ambiguous duplicate caption excluded");
            Sql(cx, "DELETE dbo.CustomCaption WHERE Style=2; INSERT dbo.CustomCaption VALUES(2,N'W/Tax 15%');");
            using (var cmd = cx.CreateCommand())
            { cmd.CommandText = "SELECT COUNT(*) FROM dbo.[Order] WHERE Total=1100 AND Tax=117.86"; Check((int)cmd.ExecuteScalar() == 4, "Stored totals unchanged"); }

            Render("AllRates", all, "Work Order", 1, false);
            Render("AllRatesPreview", all, "Work Order", 1, false);
            Render("Only15", Reference(101, "982.14"), "Work Order", 1, false);
            Render("NoRates", "", "Work Order", 1, false);
            Render("Quotation", all, "Sales Quotation", 1, false); // RDLC must also hide it.
            string large = Reference(100, "123,456,789.00");
            Render("LargeValues", large, "Work Order", 1, false);
            Render("MultiPage", all, "Work Order", 60, false);
            Render("Empty", all, "Work Order", 0, false);
            Render("BaselineNoRates", "", "Work Order", 1, true);
            Render("BaselineQuotation", "", "Sales Quotation", 1, true);
            Render("BaselineMultiPage", "", "Work Order", 60, true);
            Render("BaselineLargeValues", "", "Work Order", 1, true);
            Check(printablePages["AllRates"] == printablePages["BaselineNoRates"] && printablePages["Only15"] == printablePages["BaselineNoRates"], "Reference does not add printable direct-print pages");
            Check(printablePages["NoRates"] == printablePages["BaselineNoRates"] && printablePages["Quotation"] == printablePages["BaselineQuotation"], "No-rate and quotation printable page counts unchanged");
            Check(printablePages["MultiPage"] == printablePages["BaselineMultiPage"], "Multi-page printable page count unchanged");
            Sql(cx, "DROP TABLE dbo.CustomCaption");
            Check(Reference(100, "982.14") == "", "Optional missing-caption failure cannot block printing");
            Console.WriteLine("PASS: " + checks + " printed-withholding local/helper/render checks; " + output);
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
        finally
        {
            if (context is IDisposable) ((IDisposable)context).Dispose();
            if (created) { cx.ChangeDatabase("master"); Sql(cx, "ALTER DATABASE [" + db + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + db + "]"); }
        }
    }

    static void Render(string name, string reference, string template, int rows, bool original)
    {
        string resource = app.GetManifestResourceNames().Single(n => n.EndsWith("rprtOrder.rdlc"));
        XDocument xml;
        using (var stream = original ? File.OpenRead(baseline) : app.GetManifestResourceStream(resource)) xml = XDocument.Load(stream);
        var ns = xml.Root.Name.Namespace;
        using (var form = (Form)Activator.CreateInstance(app.GetType("WorkOrderEntry.frmPrintWo", true)))
        {
            // Never Show/Load this form: V4 Load physically prints.
            var viewer = (ReportViewer)form.GetType().GetProperty("rptviewer", Flags).GetValue(form, null);
            form.GetType().GetMethod("PrintSetting", Flags).Invoke(form, null);
            var report = viewer.LocalReport;
            if (original) using (var stream = File.OpenRead(baseline)) report.LoadReportDefinition(stream);
            foreach (var set in xml.Root.Element(ns + "DataSets").Elements(ns + "DataSet"))
            {
                var table = new DataTable();
                foreach (var field in set.Element(ns + "Fields").Elements(ns + "Field"))
                    table.Columns.Add((string)field.Attribute("Name"), (string)field.Attribute("Name") == "QtyOrder" ? typeof(int) : typeof(string));
                for (int i = 0; i < rows; i++) table.Rows.Add(table.Columns.Cast<DataColumn>().Select(c =>
                    c.DataType == typeof(int) ? (object)2 : c.ColumnName == "Price" ? "550.00" : c.ColumnName == "Extended" ? "1,100.00" :
                    c.ColumnName == "Itemcode" ? "TEST-" + (i + 1) : "LOCAL FIXTURE ITEM").ToArray());
                report.DataSources.Add(new ReportDataSource((string)set.Attribute("Name"), table));
            }
            foreach (var parameter in xml.Root.Element(ns + "ReportParameters").Elements(ns + "ReportParameter"))
            {
                string key = (string)parameter.Attribute("Name");
                string value = key == "paramType" ? template : key == "paramWithholdingReference" ? reference :
                    key == "paramTotal" ? (name.Contains("LargeValues") ? "123,456,789.00" : "1,100.00") :
                    key == "paramSubtotal" ? (name.Contains("LargeValues") ? "123,456,789.00" : "982.14") :
                    key == "paramSalesTax" ? (name.Contains("LargeValues") ? "0.00" : "117.86") :
                    key == "paramWOno" ? "100" : key == "paramDate" ? "10/22/2026" : key == "paramTime" ? "10:00" : "LOCAL FIXTURE";
                report.SetParameters(new ReportParameter(key, value));
            }
            // Cover the existing direct-print render dimensions, plus viewer preview.
            string device = name == "AllRatesPreview" ?
                "<PageWidth>8.5in</PageWidth><PageHeight>6.5in</PageHeight><MarginTop>0in</MarginTop><MarginLeft>0in</MarginLeft><MarginRight>0in</MarginRight><MarginBottom>0in</MarginBottom>" :
                "<PageWidth>8.5in</PageWidth><PageHeight>6in</PageHeight><MarginTop>0.1in</MarginTop><MarginLeft>0.1in</MarginLeft><MarginRight>0.1in</MarginRight><MarginBottom>0.1in</MarginBottom>";
            byte[] pdf = report.Render("PDF", "<DeviceInfo>" + device + "</DeviceInfo>");
            Check(pdf.Length > 1000, name + " PDF renders");
            File.WriteAllBytes(Path.Combine(output, name + ".pdf"), pdf);
            File.WriteAllBytes(Path.Combine(output, name + ".tiff"), report.Render("IMAGE", "<DeviceInfo><OutputFormat>TIFF</OutputFormat>" + device + "</DeviceInfo>"));
            var streams = new List<MemoryStream>();
            Warning[] warnings;
            report.Render("IMAGE", "<DeviceInfo><OutputFormat>EMF</OutputFormat>" + device + "</DeviceInfo>",
                (string n, string extension, Encoding encoding, string mime, bool seek) => {
                    var stream = new MemoryStream(); streams.Add(stream); return stream;
                }, out warnings);
            // Match the existing print extension's blank-EMF filter, without printing.
            printablePages[name] = streams.Count(s => s.Length > 408);
            File.WriteAllLines(Path.Combine(output, name + "-emf-pages.txt"), streams.Select(s => s.Length.ToString()));
            foreach (var stream in streams) stream.Dispose();
            var page = viewer.GetPageSettings();
            Check(page.PaperSize.Width == 850 && page.PaperSize.Height == 650 && page.Margins.Top == 0 && page.Margins.Bottom == 0 &&
                page.Margins.Left == 0 && page.Margins.Right == 0, "Existing paper settings retained");
        }
    }
}
