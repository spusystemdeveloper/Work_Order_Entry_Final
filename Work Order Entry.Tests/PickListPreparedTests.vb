Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class PickListPreparedTests

    Private Shared Function Entry(entryID As Integer, itemID As Integer) As KeyValuePair(Of Integer, Integer)
        Return New KeyValuePair(Of Integer, Integer)(entryID, itemID)
    End Function

    Private Shared Function Row(rowID As Long, itemID As Integer) As KeyValuePair(Of Long, Integer)
        Return New KeyValuePair(Of Long, Integer)(rowID, itemID)
    End Function

    Private Shared Function Prepared(ParamArray pairs() As Object) As Dictionary(Of Long, Integer?)
        Dim result As New Dictionary(Of Long, Integer?)
        For index As Integer = 0 To pairs.Length - 1 Step 2
            result(CLng(pairs(index))) = CType(pairs(index + 1), Integer?)
        Next
        Return result
    End Function

    <TestMethod>
    Public Sub EachLineOfARepeatedItemShowsItsOwnPreparedQuantity()
        Dim result = clsPickList.PreparedByEntry({Entry(1, 10), Entry(2, 10)},
                                                 {Row(100, 10), Row(101, 10)},
                                                 Prepared(100L, 1, 101L, 3))
        Assert.AreEqual(1, result(1))
        Assert.AreEqual(3, result(2))
    End Sub

    <TestMethod>
    Public Sub RepeatedLinesPairInLineOrderRegardlessOfRowOrder()
        Dim result = clsPickList.PreparedByEntry({Entry(2, 10), Entry(1, 10)},
                                                 {Row(101, 10), Row(100, 10)},
                                                 Prepared(100L, 5, 101L, 7))
        Assert.AreEqual(5, result(1))
        Assert.AreEqual(7, result(2))
    End Sub

    <TestMethod>
    Public Sub BlankPreparedQuantityIsZero()
        Dim result = clsPickList.PreparedByEntry({Entry(1, 10)}, {Row(100, 10)}, Prepared(100L, Nothing))
        Assert.AreEqual(0, result(1))
    End Sub

    <TestMethod>
    Public Sub LineWithoutQueueRowIsNotListedSoItPrintsZero()
        Dim result = clsPickList.PreparedByEntry({Entry(1, 10), Entry(2, 10)}, {Row(100, 10)}, Prepared(100L, 4))
        Assert.AreEqual(4, result(1))
        Assert.IsFalse(result.ContainsKey(2))
    End Sub

    <TestMethod>
    Public Sub DifferentItemsKeepTheirOwnQuantities()
        Dim result = clsPickList.PreparedByEntry({Entry(1, 10), Entry(2, 20)},
                                                 {Row(100, 20), Row(101, 10)},
                                                 Prepared(100L, 9, 101L, 2))
        Assert.AreEqual(2, result(1))
        Assert.AreEqual(9, result(2))
    End Sub

End Class
