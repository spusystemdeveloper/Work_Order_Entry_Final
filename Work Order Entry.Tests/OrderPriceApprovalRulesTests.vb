Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class OrderPriceApprovalRulesTests
    <TestMethod>
    Public Sub NewOrdersRequireApprovalOnlyBelowTheMinimum()
        Assert.IsTrue(OrderPriceApprovalRules.RequiresOverride(99D, 100D, 0, Nothing))
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(100D, 100D, 0, Nothing))
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(101D, 100D, 0, Nothing))
    End Sub

    <TestMethod>
    Public Sub RecallKeepsThePreviouslyApprovedPriceEvenBelowTheMinimum()
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(75D, 100D, 1, 75D))
        Assert.IsTrue(OrderPriceApprovalRules.RequiresOverride(76D, 100D, 1, 75D))
        Assert.IsTrue(OrderPriceApprovalRules.RequiresOverride(74D, 100D, 1, 75D))
    End Sub

    <TestMethod>
    Public Sub NewLinesOnRecalledOrdersStillRequireApproval()
        Assert.IsTrue(OrderPriceApprovalRules.RequiresOverride(75D, 100D, 1, Nothing))
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(100D, 100D, 1, Nothing))
    End Sub

    <TestMethod>
    Public Sub RecallCanRaiseThePriceToTheMinimumWithoutApproval()
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(100D, 100D, 1, 75D))
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(125D, 100D, 1, 75D))
    End Sub

    <TestMethod>
    Public Sub ZeroSavedPriceIsDistinctFromAMissingOrderLine()
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(0D, 100D, 1, 0D))
        Assert.IsTrue(OrderPriceApprovalRules.RequiresOverride(0D, 100D, 1, Nothing))
    End Sub

    <TestMethod>
    Public Sub OtherRecallModesRetainTheirExistingApprovalBehavior()
        Assert.IsFalse(OrderPriceApprovalRules.RequiresOverride(1D, 100D, 2, Nothing))
    End Sub
End Class
