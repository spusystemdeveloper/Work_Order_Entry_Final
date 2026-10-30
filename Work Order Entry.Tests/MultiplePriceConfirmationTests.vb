Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class MultiplePriceConfirmationTests

    <TestMethod>
    Public Sub GivenMissingOrPlaceholderPrice_WhenChecked_ThenConfirmationIsRequired()
        For Each price As Object In New Object() {Nothing, DBNull.Value, "", "-"}
            Assert.IsFalse(PriceConfirmationRules.IsConfirmedPrice(price))
        Next
    End Sub

    <TestMethod>
    Public Sub GivenNumericPriceIncludingZero_WhenChecked_ThenPriceIsConfirmed()
        For Each price As Object In New Object() {0D, 100D, 12.5D}
            Assert.IsTrue(PriceConfirmationRules.IsConfirmedPrice(price))
        Next
    End Sub

End Class
