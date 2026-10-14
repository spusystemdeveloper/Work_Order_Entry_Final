Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class ShortageAlertListTests
    Private Function Lines() As WarehouseShortage()
        Return {New WarehouseShortage With {.OrderID = 282836, .QueueItemID = 1, .ItemID = 11,
                    .Customer = "ABARRO PHOTO", .ItemCode = "55157RMS", .Description = "BOOK PAPER",
                    .OrderedQuantity = 5, .FoundQuantity = 0, .Picker = "SHAWN"},
                New WarehouseShortage With {.OrderID = 282837, .QueueItemID = 2, .ItemID = 12,
                    .Customer = "WALK-IN", .ItemCode = "OTHER", .Description = "OTHER ITEM",
                    .OrderedQuantity = 3, .FoundQuantity = Nothing, .Picker = ""}}
    End Function

    <TestMethod>
    Public Sub SearchFindsOrderCustomerItemDescriptionAndPickerWithoutCaseSensitivity()
        Dim list As New ShortageAlertList(Lines(), New Integer() {})
        For Each query In {"282836", " abarro ", "55157rms", "book paper", "shawn"}
            list.ApplyFilter(query, False, False, Nothing)
            Assert.AreEqual(1, list.View.Count, query)
            Assert.AreEqual(282836, CInt(list.View(0)("Work Order")))
        Next
    End Sub

    <TestMethod>
    Public Sub SearchTreatsQuotesBracketsAndWildcardsAsLiteralItemText()
        Dim lines = Me.Lines()
        lines(0).ItemCode = "A[1]*100%'s"
        Dim list As New ShortageAlertList(lines, New Integer() {})
        list.ApplyFilter("A[1]*100%'s", False, False, Nothing)
        Assert.AreEqual(1, list.View.Count)
        list.ApplyFilter("*", False, False, Nothing)
        Assert.AreEqual(1, list.View.Count)
    End Sub

    <TestMethod>
    Public Sub NewAndZeroFiltersCombineWithoutTreatingMissingCountAsZero()
        Dim list As New ShortageAlertList(Lines(), {282836, 282837})
        list.ApplyFilter("", True, True, Nothing)
        Assert.AreEqual(1, list.View.Count)
        Assert.AreEqual(282836, CInt(list.View(0)("Work Order")))
        Dim current As New ShortageAlertList(Lines(), {282837})
        current.ApplyFilter("", True, True, Nothing)
        Assert.AreEqual(0, current.View.Count)
    End Sub

    <TestMethod>
    Public Sub PickerFilterSupportsExactNamesWithApostrophesAndUnassignedLines()
        Dim lines = Me.Lines()
        lines(0).Picker = "O'NEIL"
        Dim list As New ShortageAlertList(lines, New Integer() {})
        list.ApplyFilter("", False, False, "O'NEIL")
        Assert.AreEqual(282836, CInt(list.View(0)("Work Order")))
        list.ApplyFilter("", False, False, "")
        Assert.AreEqual(1, list.View.Count)
        Assert.AreEqual(282837, CInt(list.View(0)("Work Order")))
    End Sub

    <TestMethod>
    Public Sub ClearingFiltersRestoresAllLinesAndNewAlertPriority()
        Dim list As New ShortageAlertList(Lines(), {282837})
        list.ApplyFilter("no match", False, False, Nothing)
        Assert.AreEqual(0, list.View.Count)
        list.ApplyFilter("", False, False, Nothing)
        Assert.AreEqual(2, list.View.Count)
        Assert.AreEqual(282837, CInt(list.View(0)("Work Order")))
    End Sub

    <TestMethod>
    Public Sub FilteringKeepsStoredPhysicalCountAndOriginalOrderIdentity()
        Dim lines = Me.Lines()
        Dim list As New ShortageAlertList(lines, New Integer() {})
        lines(0).FoundQuantity = 99
        lines(0).OrderID = 1
        list.ApplyFilter("282836", False, True, Nothing)
        Assert.AreEqual(1, list.View.Count)
        Assert.AreEqual("0", CStr(list.View(0)("Found")))
        Assert.AreEqual(282836, CInt(list.View(0)("Work Order")))
        Assert.AreEqual(2, list.TotalLines)
    End Sub

    <TestMethod>
    Public Sub SearchPickerAndNewFiltersMustAllMatch()
        Dim list As New ShortageAlertList(Lines(), {282836})
        list.ApplyFilter("paper", True, False, "SHAWN")
        Assert.AreEqual(1, list.View.Count)
        list.ApplyFilter("paper", True, False, "")
        Assert.AreEqual(0, list.View.Count)
    End Sub
End Class
