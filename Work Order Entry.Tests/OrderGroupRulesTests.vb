Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class OrderGroupRulesTests

    <TestMethod>
    Public Sub AnOrderInTheSelectedGroupIsAccepted()
        Dim orderID As Integer
        Assert.IsTrue(OrderGroupRules.TryFindOrderInGroup("101 / 102 / 103", " 102 ", orderID))
        Assert.AreEqual(102, orderID)
    End Sub

    <TestMethod>
    Public Sub AnOrderOutsideTheSelectedGroupIsRejected()
        Dim orderID As Integer
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup("101 / 102 / 103", "104", orderID))
        Assert.AreEqual(0, orderID)
    End Sub

    <TestMethod>
    Public Sub PartialNumbersAndTextAreRejected()
        Dim orderID As Integer
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup("1011 / 102", "101", orderID))
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup("101 / 102", "10x", orderID))
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup("101 / 102", "-101", orderID))
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup("101 / 102", "", orderID))
        Assert.IsFalse(OrderGroupRules.TryFindOrderInGroup(Nothing, "101", orderID))
    End Sub

    <TestMethod>
    Public Sub OnlyRowsWithMoreThanOneOrderAreGroups()
        Assert.IsTrue(OrderGroupRules.IsGroup("101 / 102"))
        Assert.IsFalse(OrderGroupRules.IsGroup("101"))
        Assert.IsFalse(OrderGroupRules.IsGroup(Nothing))
    End Sub

End Class
