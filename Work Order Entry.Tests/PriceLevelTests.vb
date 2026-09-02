Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class PriceLevelTests

    <TestMethod>
    Public Sub GivenRetailPrice_WhenLevelIsRequested_ThenReturnsZero()
        ' Given
        Const displayedPrice As String = "PRICE (RETAIL)"

        ' When
        Dim level = clsCustomer.GetLevel(displayedPrice)

        ' Then
        Assert.AreEqual(0, level)
    End Sub

    <TestMethod>
    Public Sub GivenWholesalePriceA_WhenLevelIsRequested_ThenReturnsOne()
        Assert.AreEqual(1, clsCustomer.GetLevel("PRICE A (WHOLESALE)"))
    End Sub

    <TestMethod>
    Public Sub GivenPriceB_WhenLevelIsRequested_ThenReturnsTwo()
        Assert.AreEqual(2, clsCustomer.GetLevel("PRICE B (D1)"))
    End Sub

    <TestMethod>
    Public Sub GivenPriceC_WhenLevelIsRequested_ThenReturnsThree()
        Assert.AreEqual(3, clsCustomer.GetLevel("PRICE C (D2)"))
    End Sub

    <TestMethod>
    Public Sub GivenCost_WhenLevelIsRequested_ThenReturnsFour()
        Assert.AreEqual(4, clsCustomer.GetLevel("COST"))
    End Sub

    <TestMethod>
    Public Sub GivenUnknownPriceText_WhenLevelIsRequested_ThenDefaultsToRetail()
        Assert.AreEqual(0, clsCustomer.GetLevel("UNKNOWN"))
    End Sub

End Class
