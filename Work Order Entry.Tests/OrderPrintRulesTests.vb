<TestClass>
Public Class OrderPrintRulesTests

    <TestMethod>
    Public Sub GivenWorkOrderType_WhenResolved_ThenWorkOrderTemplateIsUsed()
        Assert.AreEqual("Work Order", OrderPrintRules.GetTemplateName(2))
    End Sub

    <TestMethod>
    Public Sub GivenQuotationType_WhenResolved_ThenQuotationTemplateIsUsed()
        Assert.AreEqual("Sales Quotation", OrderPrintRules.GetTemplateName(3))
    End Sub

    <TestMethod>
    Public Sub GivenUnknownOrderType_WhenResolved_ThenNoTemplateIsSelected()
        Assert.AreEqual(String.Empty, OrderPrintRules.GetTemplateName(99))
    End Sub

End Class
