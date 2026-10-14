Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports WorkOrderEntry

<TestClass>
Public Class ShortageNotificationsTests
    Private Function Shortage(orderID As Integer, Optional found As Integer = 3,
                              Optional queueItemID As Long = 1) As WarehouseShortage
        Return New WarehouseShortage With {.OrderID = orderID, .QueueItemID = queueItemID,
            .ItemID = 1, .OrderedQuantity = 5, .FoundQuantity = found,
            .Customer = "TEST CUSTOMER", .ItemCode = "TEST-SKU", .Picker = "PICKER"}
    End Function

    <TestMethod>
    Public Sub ExistingShortagesEstablishBaselineWithoutNewAlerts()
        Dim monitor As New ShortageNotifications()
        Assert.AreEqual(0, monitor.Observe({Shortage(1)}).Count)
    End Sub

    <TestMethod>
    Public Sub NewWarehouseShortageAlertsOnce()
        Dim monitor As New ShortageNotifications()
        monitor.Observe(New WarehouseShortage() {})
        CollectionAssert.AreEqual(New List(Of Integer) From {2}, monitor.Observe({Shortage(2)}))
        Assert.AreEqual(0, monitor.Observe({Shortage(2)}).Count)
    End Sub

    <TestMethod>
    Public Sub ChangedPhysicalQuantityAlertsAgainForSameOrder()
        Dim monitor As New ShortageNotifications()
        monitor.Observe({Shortage(1, 3)})
        CollectionAssert.AreEqual(New List(Of Integer) From {1}, monitor.Observe({Shortage(1, 2)}))
    End Sub

    <TestMethod>
    Public Sub ClearedThenReflaggedShortageAlertsAgain()
        Dim monitor As New ShortageNotifications()
        monitor.Observe({Shortage(1)})
        monitor.Observe(New WarehouseShortage() {})
        CollectionAssert.AreEqual(New List(Of Integer) From {1}, monitor.Observe({Shortage(1)}))
    End Sub

    <TestMethod>
    Public Sub DuplicateRowsAndRowReorderingDoNotRepeatAlerts()
        Dim monitor As New ShortageNotifications()
        monitor.Observe({Shortage(1, 3, 1), Shortage(1, 2, 2)})
        Assert.AreEqual(0, monitor.Observe({Shortage(1, 2, 2), Shortage(1, 3, 1), Shortage(1, 3, 1)}).Count)
    End Sub

    <TestMethod>
    Public Sub AnotherShortageLineOnSameOrderAlertsOnce()
        Dim monitor As New ShortageNotifications()
        monitor.Observe({Shortage(1, 3, 1)})
        CollectionAssert.AreEqual(New List(Of Integer) From {1}, monitor.Observe({Shortage(1, 3, 1), Shortage(1, 2, 2)}))
    End Sub

    <TestMethod>
    Public Sub FailedPollPreservesSuccessfulShortageBaseline()
        Dim monitor As New ShortageNotifications()
        monitor.Observe({Shortage(1)})
        Try
            monitor.Observe(Nothing)
            Assert.Fail("Missing results must not erase the last successful poll.")
        Catch ex As ArgumentNullException
        End Try
        CollectionAssert.AreEqual(New List(Of Integer) From {2}, monitor.Observe({Shortage(1), Shortage(2)}))
    End Sub

    <TestMethod>
    Public Sub MissingAndZeroPhysicalCountsAreDifferentAndStillAlert()
        Dim monitor As New ShortageNotifications()
        Dim line = Shortage(1)
        line.FoundQuantity = Nothing
        monitor.Observe({line})
        CollectionAssert.AreEqual(New List(Of Integer) From {1}, monitor.Observe({Shortage(1, 0)}))
    End Sub

    <TestMethod>
    Public Sub EmptyAlertListCannotOpenAnOrder()
        Using alerts As New frmShortageAlerts(New WarehouseShortage() {}, New Integer() {})
            Assert.AreEqual(0, alerts.SelectedOrderID)
            Assert.IsFalse(DirectCast(alerts.Controls.Find("OpenShortageOrder", True)(0), Button).Enabled)
        End Using
    End Sub
End Class
