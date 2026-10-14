Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Collections.Generic
Imports WorkOrderEntry

<TestClass>
Public Class ReadyOrderNotificationsTests
    <TestMethod>
    Public Sub GivenAlreadyReadyOrders_WhenFirstPollSucceeds_ThenNoNewAlertIsRaised()
        Dim monitor As New ReadyOrderNotifications()
        Assert.AreEqual(0, monitor.Observe({1, 2}).Count)
    End Sub

    <TestMethod>
    Public Sub GivenNewPreparedGroup_WhenPolled_ThenOnlyNewGroupIsReported()
        Dim monitor As New ReadyOrderNotifications()
        monitor.Observe({1})
        CollectionAssert.AreEqual(New List(Of Integer) From {2}, monitor.Observe({1, 2}))
    End Sub

    <TestMethod>
    Public Sub GivenUnchangedGroups_WhenPolledAgain_ThenNoDuplicateAlertIsRaised()
        Dim monitor As New ReadyOrderNotifications()
        monitor.Observe({1})
        monitor.Observe({1, 2})
        Assert.AreEqual(0, monitor.Observe({2, 1}).Count)
    End Sub

    <TestMethod>
    Public Sub GivenGroupLeftPrepared_WhenItBecomesPreparedAgain_ThenItIsReportedAgain()
        Dim monitor As New ReadyOrderNotifications()
        monitor.Observe({1})
        monitor.Observe(New Integer() {})
        CollectionAssert.AreEqual(New List(Of Integer) From {1}, monitor.Observe({1}))
    End Sub

    <TestMethod>
    Public Sub GivenDuplicateRows_WhenPolled_ThenOneAlertPerGroupIsRaised()
        Dim monitor As New ReadyOrderNotifications()
        monitor.Observe(New Integer() {})
        CollectionAssert.AreEqual(New List(Of Integer) From {1, 2}, monitor.Observe({2, 1, 2}))
    End Sub

    <TestMethod>
    Public Sub GivenFailedPoll_WhenPollingRecovers_ThenPreviousBaselineIsPreserved()
        Dim monitor As New ReadyOrderNotifications()
        monitor.Observe({1})
        Try
            monitor.Observe(Nothing)
            Assert.Fail("A missing query result must not replace the successful baseline.")
        Catch ex As ArgumentNullException
        End Try
        CollectionAssert.AreEqual(New List(Of Integer) From {2}, monitor.Observe({1, 2}))
    End Sub
End Class
