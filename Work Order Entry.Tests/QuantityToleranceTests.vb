Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class QuantityToleranceTests

    <TestMethod>
    Public Sub FloatRemainderIsNotNegative()
        Dim remainder As Double = 0.7R + 0.1R - 0.7R - 0.1R
        Assert.IsTrue(remainder < 0R)
        Assert.IsFalse(OrderCommitmentRules.IsNegativeQuantity(remainder))
    End Sub

    <TestMethod>
    Public Sub SmallestRealFractionIsStillNegative()
        Assert.IsTrue(OrderCommitmentRules.IsNegativeQuantity(-0.001R))
    End Sub

    <TestMethod>
    Public Sub LineQuantityLimitIsInclusive()
        Assert.IsFalse(OrderCommitmentRules.ExceedsMaximumQuantity(999999R))
        Assert.IsTrue(OrderCommitmentRules.ExceedsMaximumQuantity(999999.01R))
        Assert.IsTrue(OrderCommitmentRules.ExceedsMaximumQuantity(1.23123123213123E+17R))
    End Sub

    <TestMethod>
    Public Sub OrderTotalMustFitTheMoneyColumn()
        Assert.IsFalse(OrderCommitmentRules.ExceedsMaximumTotal(922337203685476R))
        Assert.IsTrue(OrderCommitmentRules.ExceedsMaximumTotal(922337203685477R))
    End Sub

    <TestMethod>
    Public Sub ZeroAndPositiveAreNotNegative()
        Assert.IsFalse(OrderCommitmentRules.IsNegativeQuantity(0R))
        Assert.IsFalse(OrderCommitmentRules.IsNegativeQuantity(0.5R))
    End Sub

End Class
