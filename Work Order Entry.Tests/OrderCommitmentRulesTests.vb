Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class OrderCommitmentRulesTests
    <TestMethod>
    Public Sub WorkOrderEditsReserveOnlyTheDifferenceIncludingFractionalQuantities()
        Assert.AreEqual(-3R, OrderCommitmentRules.CalculateChange(2, 2, 10, 7, 0))
        Assert.AreEqual(0.5R, OrderCommitmentRules.CalculateChange(2, 2, 1.25, 1.75, 0))
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(2, 2, 7, 7, 0))
    End Sub

    <TestMethod>
    Public Sub NewWorkOrderReservesItsFullQuantity()
        Assert.AreEqual(5.5R, OrderCommitmentRules.CalculateChange(0, 2, 0, 5.5, 0))
    End Sub

    <TestMethod>
    Public Sub QuotationEditsDoNotReserveStock()
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(3, 3, 10, 7, 0))
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(0, 3, 0, 7, 0))
    End Sub

    <TestMethod>
    Public Sub TypeConversionKeepsTheExistingReleaseAndReserveRules()
        Assert.AreEqual(-10R, OrderCommitmentRules.CalculateChange(2, 3, 10, 7, 0))
        Assert.AreEqual(7R, OrderCommitmentRules.CalculateChange(3, 2, 10, 7, 0))
    End Sub

    <TestMethod>
    Public Sub RemovedWorkOrderLineReleasesItsPreviousQuantity()
        Assert.AreEqual(-5.5R, OrderCommitmentRules.CalculateChange(2, 0, 5.5, 0, 0))
    End Sub

    <TestMethod>
    Public Sub NonInventoryItemsRemainExemptFromReservationChanges()
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(0, 2, 0, 5, 7))
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(2, 2, 5, 3, 7))
        Assert.AreEqual(0R, OrderCommitmentRules.CalculateChange(2, 0, 5, 0, 7))
    End Sub
End Class
