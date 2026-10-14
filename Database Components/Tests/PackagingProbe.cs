// Synthetic clean-folder check: no real app startup, SQL, dialogs or print jobs.
using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using ExcelDataReader;
using Microsoft.Reporting.WinForms;

class PackagingProbe
{
    [STAThread]
    static int Main()
    {
        try
        {
            // Initialize the application's embedded dependency loader without login.
            using (WorkOrderEntry.LoadingOverlay.Show(null, "Packaging test")) { }
            // MissingMethodException can occur when this method is JIT-compiled,
            // before its Try block or file picker runs. Compile the actual entry
            // point without opening its dialog or touching application state.
            RuntimeHelpers.PrepareMethod(typeof(WorkOrderEntry.clsImport)
                .GetMethod("importSelectFile").MethodHandle);
            Console.WriteLine("PASS: Actual Import entry point compiles without dependency errors.");
            CheckCsv();
            CheckWorkbook();
            CheckExcelAssemblyIdentity();
            CheckReport("rprtOrder.rdlc", "Work Order");
            CheckReport("rptPickList.rdlc", "Pick list");
            string location = typeof(LocalReport).Assembly.Location;
            if (String.IsNullOrEmpty(location) || !File.Exists(location))
                throw new Exception("ReportViewer must load from a physical file.");
            Console.WriteLine("PASS: ReportViewer has a physical packaged DLL location.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    static void CheckCsv()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("ItemCode,Qty\r\nTEST,3\r\n")))
        using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
            CheckRows(reader);
        Console.WriteLine("PASS: Embedded CSV import.");
    }

    static void CheckWorkbook()
    {
        using (var stream = new MemoryStream())
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                Entry(zip, "[Content_Types].xml", "<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'><Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/><Default Extension='xml' ContentType='application/xml'/><Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/><Override PartName='/xl/worksheets/sheet1.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'/></Types>");
                Entry(zip, "_rels/.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>");
                Entry(zip, "xl/workbook.xml", "<workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><sheets><sheet name='Items' sheetId='1' r:id='rId1'/></sheets></workbook>");
                Entry(zip, "xl/_rels/workbook.xml.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet1.xml'/></Relationships>");
                Entry(zip, "xl/worksheets/sheet1.xml", "<worksheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'><sheetData><row r='1'><c r='A1' t='inlineStr'><is><t>ItemCode</t></is></c><c r='B1' t='inlineStr'><is><t>Qty</t></is></c></row><row r='2'><c r='A2' t='inlineStr'><is><t>TEST</t></is></c><c r='B2'><v>3</v></c></row></sheetData></worksheet>");
            }
            stream.Position = 0;
            using (var reader = ExcelReaderFactory.CreateReader(stream)) CheckRows(reader);
        }
        Console.WriteLine("PASS: Embedded XLSX import.");
    }

    static void Entry(ZipArchive zip, string path, string content)
    {
        using (var writer = new StreamWriter(zip.CreateEntry(path).Open(), new UTF8Encoding(false)))
            writer.Write(content);
    }

    static void CheckExcelAssemblyIdentity()
    {
        foreach (string name in new[] { "ExcelDataReader", "ExcelDataReader.DataSet" })
            if (AppDomain.CurrentDomain.GetAssemblies().Count(assembly =>
                assembly.GetName().Name == name) != 1)
                throw new Exception("Expected exactly one loaded " + name + " assembly.");
        var readerType = typeof(ExcelDataSetConfiguration).GetProperty("ConfigureDataTable")
            .PropertyType.GetGenericArguments()[0];
        if (readerType != typeof(IExcelDataReader))
            throw new Exception("Import and DataSet use different Excel reader type identities.");
        Console.WriteLine("PASS: Excel reader and DataSet share one dependency identity.");
    }

    static void CheckRows(IExcelDataReader reader)
    {
        var result = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });
        if (result.Tables.Count != 1 || result.Tables[0].Rows.Count != 1 ||
            (string)result.Tables[0].Rows[0][0] != "TEST" || Convert.ToInt32(result.Tables[0].Rows[0][1]) != 3)
            throw new Exception("Imported fixture values differ.");
    }

    static void CheckReport(string suffix, string caption)
    {
        var assembly = typeof(WorkOrderEntry.LoadingOverlay).Assembly;
        string resource = assembly.GetManifestResourceNames().Single(name => name.EndsWith(suffix));
        XDocument xml;
        using (var definition = assembly.GetManifestResourceStream(resource)) xml = XDocument.Load(definition);
        var ns = xml.Root.Name.Namespace;
        var rd = XNamespace.Get("http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");
        using (var report = new LocalReport())
        {
            using (var definition = assembly.GetManifestResourceStream(resource)) report.LoadReportDefinition(definition);
            foreach (var set in xml.Root.Element(ns + "DataSets").Elements(ns + "DataSet"))
            {
                var table = new DataTable();
                foreach (var field in set.Element(ns + "Fields").Elements(ns + "Field"))
                {
                    string type = (string)field.Element(rd + "TypeName") ?? "System.String";
                    table.Columns.Add((string)field.Element(ns + "DataField"),
                        type.Contains("Int32") ? typeof(int) : typeof(string));
                }
                table.Rows.Add(table.Columns.Cast<DataColumn>().Select(column =>
                    column.DataType == typeof(int) ? (object)3 :
                    column.ColumnName == "Extended" ? "300.00" :
                    column.ColumnName == "Price" ? "100.00" : "Fixture").ToArray());
                report.DataSources.Add(new ReportDataSource((string)set.Attribute("Name"), table));
            }
            var parameters = xml.Root.Element(ns + "ReportParameters");
            if (parameters != null)
                foreach (var parameter in parameters.Elements(ns + "ReportParameter"))
                {
                    string name = (string)parameter.Attribute("Name");
                    report.SetParameters(new ReportParameter(name,
                        name == "paramDate" ? "10/14/2026" : name == "paramTime" ? "18:00" :
                        name == "paramTotal" || name == "paramSubtotal" ? "300.00" : name == "paramSalesTax" ? "0.00" : "Fixture"));
                }
            byte[] pdf = report.Render("PDF");
            if (pdf.Length < 1000) throw new Exception("Empty report output.");
            File.WriteAllBytes(suffix + ".pdf", pdf);
        }
        Console.WriteLine("PASS: " + caption + " PDF rendering.");
    }
}
