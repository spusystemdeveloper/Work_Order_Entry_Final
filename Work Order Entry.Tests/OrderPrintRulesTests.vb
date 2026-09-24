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

    <TestMethod>
    Public Sub GivenEmptyOrNullTemplateName_WhenResolved_ThenWorkOrderIsDefault()
        Assert.AreEqual(OrderPrintRules.WorkOrderTemplate, OrderPrintRules.ResolveTemplateName(Nothing))
        Assert.AreEqual(OrderPrintRules.WorkOrderTemplate, OrderPrintRules.ResolveTemplateName(String.Empty))
        Assert.AreEqual(OrderPrintRules.WorkOrderTemplate, OrderPrintRules.ResolveTemplateName("   "))
    End Sub

    <TestMethod>
    Public Sub GivenExplicitTemplateName_WhenResolved_ThenProvidedNameIsPreserved()
        Assert.AreEqual("Sales Quotation", OrderPrintRules.ResolveTemplateName("Sales Quotation"))
        Assert.AreEqual("Work Order", OrderPrintRules.ResolveTemplateName("Work Order"))
    End Sub

End Class
