<TestClass>
Public Class ImportFileRulesTests

    <TestMethod>
    Public Sub GivenQuotationWorkbook_WhenDetected_ThenQuotationImportIsUsed()
        Assert.AreEqual(
            ImportFileRules.QuotationOrSaleImport,
            ImportFileRules.GetImportType("Branch Quotation.xlsx"))
    End Sub

    <TestMethod>
    Public Sub GivenSaleCsv_WhenDetected_ThenLegacyQuotationImportIsUsed()
        Assert.AreEqual(
            ImportFileRules.QuotationOrSaleImport,
            ImportFileRules.GetImportType("Daily Sale.csv"))
    End Sub

    <TestMethod>
    Public Sub GivenPurchaseWorkbook_WhenDetected_ThenPurchaseImportIsUsed()
        Assert.AreEqual(
            ImportFileRules.TransferOrPurchaseImport,
            ImportFileRules.GetImportType("Purchase Export.XLS"))
    End Sub

    <TestMethod>
    Public Sub GivenWebsiteCsv_WhenDetected_ThenWebsiteImportIsUsed()
        Assert.AreEqual(
            ImportFileRules.WebsiteImport,
            ImportFileRules.GetImportType("WEBSITE Orders.csv"))
    End Sub

    <TestMethod>
    Public Sub GivenUnrecognizedFilename_WhenDetected_ThenImportIsRejected()
        Assert.AreEqual(
            ImportFileRules.UnknownImport,
            ImportFileRules.GetImportType("orders.xlsx"))
    End Sub

    <TestMethod>
    Public Sub GivenUppercaseCsvExtension_WhenChecked_ThenCsvIsRecognized()
        Assert.IsTrue(ImportFileRules.IsCsv("Quotation.CSV"))
    End Sub

End Class
