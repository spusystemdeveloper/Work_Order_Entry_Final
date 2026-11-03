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

    <TestMethod>
    Public Sub GivenWebsiteImportWithoutContents_WhenItemIsMissing_ThenItIsStillReported()
        Dim missing = ImportFileRules.FindMissingItems({"A", "B"}, {"A"}, Nothing)
        CollectionAssert.AreEqual(New String() {"B - "}, missing.ToArray())
    End Sub

    <TestMethod>
    Public Sub GivenRepeatedMissingCode_WhenChecked_ThenItIsReportedOnceWithDescription()
        Dim contents As New System.Data.DataTable()
        contents.Columns.Add("ItemLookupCode", GetType(String))
        contents.Columns.Add("Fulldesc", GetType(String))
        contents.Rows.Add("B", "Second item")
        contents.Rows.Add("B", "Second item")
        Dim missing = ImportFileRules.FindMissingItems({"B", "B"}, New String() {}, contents)
        CollectionAssert.AreEqual(New String() {"B - Second item"}, missing.ToArray())
    End Sub

    <TestMethod>
    Public Sub GivenShippingAlreadyOnOrder_WhenChecked_ThenItIsFoundForThatOrderOnly()
        Dim orders As New System.Data.DataTable()
        orders.Columns.Add("OrderID", GetType(Integer))
        orders.Columns.Add("ItemLookupCode", GetType(String))
        orders.Rows.Add(100, "SHIP")
        orders.Rows.Add(200, "ITEM")
        Assert.IsTrue(ImportFileRules.OrderHasItem(orders, "100", "SHIP"))
        Assert.IsFalse(ImportFileRules.OrderHasItem(orders, "200", "SHIP"))
    End Sub

    <TestMethod>
    Public Sub GivenEveryItemImported_WhenChecked_ThenNothingIsMissing()
        Assert.AreEqual(0, ImportFileRules.FindMissingItems({"A"}, {"A"}, Nothing).Count)
    End Sub

    <TestMethod>
    Public Sub GivenContentsWithoutDescriptionColumn_WhenItemIsMissing_ThenItIsStillReported()
        Dim contents As New System.Data.DataTable()
        contents.Columns.Add("ItemLookupCode", GetType(String))
        Dim missing = ImportFileRules.FindMissingItems({"B"}, New String() {}, contents)
        CollectionAssert.AreEqual(New String() {"B - "}, missing.ToArray())
    End Sub

End Class
