Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class MultiplePriceConfirmationTests

    <TestMethod>
    Public Sub GivenThreeChangedItems_WhenPricesAreReset_ThenAllThreeRequireConfirmation()
        ' Given
        Dim prices As Object() = {"-", "-", "-"}

        ' When
        Dim remaining =
            PriceConfirmationRules.CountUnconfirmedPrices(prices)

        ' Then
        Assert.AreEqual(3, remaining)
    End Sub

    <TestMethod>
    Public Sub GivenFirstItemConfirmed_WhenAdvancing_ThenNextUnconfirmedItemIsSelected()
        ' Given: item 1 is confirmed; items 2 and 3 are not.
        Dim prices As Object() = {100D, "-", "-"}

        ' When
        Dim nextIndex =
            PriceConfirmationRules.FindFirstUnconfirmedPriceIndex(prices)

        ' Then: the next item is the second row (zero-based index 1).
        Assert.AreEqual(1, nextIndex)
    End Sub

End Class
