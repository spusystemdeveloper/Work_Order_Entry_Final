Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class QueueLineMatcherTests

    Private Shared Function Entry(entryID As Integer, itemID As Integer) As KeyValuePair(Of Integer, Integer)
        Return New KeyValuePair(Of Integer, Integer)(entryID, itemID)
    End Function

    Private Shared Function Row(rowID As Long, itemID As Integer) As KeyValuePair(Of Long, Integer)
        Return New KeyValuePair(Of Long, Integer)(rowID, itemID)
    End Function

    <TestMethod>
    Public Sub DifferentItemsPairByItem()
        Dim result = QueueLineMatcher.Match({Entry(1, 10), Entry(2, 20)}, {Row(100, 20), Row(101, 10)})
        Assert.AreEqual(101L, result(1))
        Assert.AreEqual(100L, result(2))
    End Sub

    <TestMethod>
    Public Sub RepeatedItemPairsInIDOrderRegardlessOfInputOrder()
        Dim result = QueueLineMatcher.Match({Entry(3, 10), Entry(1, 10), Entry(2, 10)},
                                            {Row(102, 10), Row(100, 10), Row(101, 10)})
        Assert.AreEqual(100L, result(1))
        Assert.AreEqual(101L, result(2))
        Assert.AreEqual(102L, result(3))
    End Sub

    <TestMethod>
    Public Sub LineWithoutRowIsLeftOut()
        Dim result = QueueLineMatcher.Match({Entry(1, 10), Entry(2, 10)}, {Row(100, 10)})
        Assert.AreEqual(100L, result(1))
        Assert.IsFalse(result.ContainsKey(2))
    End Sub

    <TestMethod>
    Public Sub ExtraRowsAreNotAssigned()
        Dim result = QueueLineMatcher.Match({Entry(1, 10)}, {Row(100, 10), Row(101, 10)})
        Assert.AreEqual(1, result.Count)
        Assert.AreEqual(100L, result(1))
    End Sub

End Class
