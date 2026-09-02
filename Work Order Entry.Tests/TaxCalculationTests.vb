Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class TaxCalculationTests

    Private Shared ReadOnly StateModule As Type =
        GetType(clsItemLookUp).Assembly.GetType(
            "WorkOrderEntry.ModConnectDB", throwOnError:=True)

    <TestMethod>
    Public Sub GivenTaxableItems_WhenPriceIsCalculated_ThenVatIsSeparated()
        ' Given: 2 items at VAT-inclusive price 112.00 each.
        SetState("bTaxExcempt", False)

        ' When
        clsItemLookUp.GetPrice(2, 1, 112)

        ' Then: 224 total = 200 VATable sales + 24 VAT.
        AssertAmount(200, GetState("dTSales"), "VATable sales")
        AssertAmount(24, GetState("dVSales"), "VAT")
        AssertAmount(224, GetState("dNetPrice"), "net price")
    End Sub

    <TestMethod>
    Public Sub GivenTaxExemptOrder_WhenPriceIsCalculated_ThenVatIsRemoved()
        ' Given
        SetState("bTaxExcempt", True)

        ' When
        clsItemLookUp.GetPrice(2, 1, 112)

        ' Then
        AssertAmount(200, GetState("dTSales"), "tax-exempt sales")
        AssertAmount(0, GetState("dVSales"), "VAT")
        AssertAmount(200, GetState("dNetPrice"), "net price")
    End Sub

    <TestMethod>
    Public Sub GivenNonTaxableItem_WhenPriceIsCalculated_ThenNoVatIsAdded()
        ' Given
        SetState("bTaxExcempt", False)

        ' When
        clsItemLookUp.GetPrice(3, 0, 50)

        ' Then
        AssertAmount(150, GetState("dTSales"), "sales")
        AssertAmount(0, GetState("dVSales"), "VAT")
        AssertAmount(150, GetState("dNetPrice"), "net price")
    End Sub

    Private Shared Sub SetState(ByVal fieldName As String,
                                ByVal value As Object)
        StateModule.GetField(
            fieldName,
            BindingFlags.Public Or BindingFlags.Static).SetValue(Nothing, value)
    End Sub

    Private Shared Function GetState(ByVal fieldName As String) As Double
        Dim value = StateModule.GetField(
            fieldName,
            BindingFlags.Public Or BindingFlags.Static).GetValue(Nothing)
        Return CDbl(value)
    End Function

    Private Shared Sub AssertAmount(ByVal expected As Double,
                                    ByVal actual As Double,
                                    ByVal description As String)
        Assert.AreEqual(expected, actual, 0.0001, description)
    End Sub

End Class
