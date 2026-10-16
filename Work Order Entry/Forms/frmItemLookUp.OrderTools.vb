Imports System.Threading.Tasks

Partial Public Class frmItemLookUp
    Private recalledSnapshot As OrderEditSnapshot
    Private activeRecallOrderID As Integer
    Private readyTimer As Windows.Forms.Timer
    Private readyMonitor As New ReadyOrderNotifications()
    Private pendingReadyGroups As New HashSet(Of Integer)()
    Private currentReadyGroups As New List(Of Integer)()
    Private readyStatus As ToolStripStatusLabel
    Private readyPollRunning As Boolean
    Private orderToolsClosing As Boolean
    Private readyConnectionString As String
    Private readyPollFailureLogged As Boolean
    Private orderActionsMenu As ToolStripDropDownButton

    Private Async Sub InitializeOrderTools(sender As Object, e As EventArgs) Handles MyBase.Shown
        If String.IsNullOrWhiteSpace(usrUsername) Then Return
        Dim actions As New ToolStripDropDownButton("Order actions") With {
            .Visible = StoreProcessingSettings.ShowOrderActions
        }
        orderActionsMenu = actions
        actions.DropDownItems.Add("History...", Nothing,
            Sub(s, args)
                Using history As New frmOrderHistory(activeRecallOrderID)
                    history.ShowDialog(Me)
                End Using
            End Sub)
        actions.DropDownItems.Add("Reload order", Nothing, AddressOf ReloadCurrentOrder)
        actions.DropDownItems.Add("Warehouse shortages...", Nothing, AddressOf OpenShortageAlerts)
        actions.DropDownItems.Add("Reduce selected shortage to found quantity...", Nothing, AddressOf ReduceSelectedShortage)
        actions.DropDownItems.Add("Undo selected shortage reduction", Nothing, AddressOf UndoSelectedShortage)
        BottomStrip.Items.Add(actions)
        InitializeShortageAlerts()
        shortageStatus = New ToolStripStatusLabel With {.ForeColor = Color.DarkBlue, .Visible = False}
        BottomStrip.Items.Add(shortageStatus)
        readyStatus = New ToolStripStatusLabel With {.IsLink = True, .Visible = False}
        AddHandler readyStatus.Click, Sub(s, args) ShowReadyOrders()
        BottomStrip.Items.Add(readyStatus)
        readyTimer = New Windows.Forms.Timer With {.Interval = 15000}
        AddHandler readyTimer.Tick, AddressOf ReadyTimerTick
        readyTimer.Start()
        Await PollReadyOrdersAsync()
        Await PollShortageAlertsAsync()
    End Sub

    Private Sub ApplyOrderToolsVisibility()
        If orderActionsMenu IsNot Nothing Then
            orderActionsMenu.Visible = StoreProcessingSettings.ShowOrderActions
        End If
        If readyStatus IsNot Nothing Then
            readyStatus.Visible = StoreProcessingSettings.ShowOrderNotifications AndAlso
                StoreProcessingSettings.ShowForInvoiceButton AndAlso
                StoreProcessingSettings.EnableReadyOrderNotifications AndAlso
                Not String.IsNullOrEmpty(readyStatus.Text)
        End If
        If shortageAlertStatus IsNot Nothing Then
            shortageAlertStatus.Visible = StoreProcessingSettings.ShowOrderNotifications AndAlso
                StoreProcessingSettings.EnableShortageNotifications AndAlso
                (shortageAlertFailureLogged OrElse currentShortageAlerts.Count > 0)
        End If
    End Sub

    Private Sub ReloadCurrentOrder(sender As Object, e As EventArgs)
        If activeRecallOrderID <= 0 Then
            MessageBox.Show(Me, "Recall an order before reloading it.", "Reload order",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If MessageBox.Show(Me, "Reload the latest saved order? Unsaved changes on this screen will be discarded.",
                           "Reload order", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            RecallQuote(activeRecallOrderID)
        End If
    End Sub

    Private Sub ShowOrderConflict(ex As OrderEditConflictException)
        MessageBox.Show(Me, ex.Message, "Order changed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Async Sub ReadyTimerTick(sender As Object, e As EventArgs)
        Await PollReadyOrdersAsync()
        Await PollShortageAlertsAsync()
    End Sub

    Private Async Function PollReadyOrdersAsync() As Task
        If readyPollRunning OrElse orderToolsClosing OrElse readyStatus Is Nothing Then Return
        If Not StoreProcessingSettings.ShowForInvoiceButton OrElse
           Not StoreProcessingSettings.EnableReadyOrderNotifications Then
            readyStatus.Visible = False
            cmdForInvoice.Text = "For Invoice"
            readyMonitor = New ReadyOrderNotifications()
            pendingReadyGroups.Clear()
            currentReadyGroups.Clear()
            Return
        End If
        readyPollRunning = True
        Try
            Dim connectionString = DB_Conn("constr")
            If Not String.Equals(connectionString, readyConnectionString, StringComparison.Ordinal) Then
                readyMonitor = New ReadyOrderNotifications()
                pendingReadyGroups.Clear()
                readyConnectionString = connectionString
            End If
            Dim groups = Await Task.Run(Function() ReadyOrderNotifications.LoadPreparedGroups(connectionString))
            If orderToolsClosing OrElse IsDisposed Then Return
            If Not StoreProcessingSettings.ShowForInvoiceButton OrElse
               Not StoreProcessingSettings.EnableReadyOrderNotifications Then Return
            pendingReadyGroups.UnionWith(readyMonitor.Observe(groups))
            pendingReadyGroups.IntersectWith(groups)
            currentReadyGroups = groups
            readyPollFailureLogged = False
            UpdateReadyStatus()
        Catch ex As Exception
            If orderToolsClosing OrElse IsDisposed Then Return
            readyStatus.Text = "Ready status unavailable - retrying"
            readyStatus.IsLink = False
            readyStatus.ForeColor = Color.DarkOrange
            readyStatus.Visible = StoreProcessingSettings.ShowOrderNotifications
            cmdForInvoice.Text = "For Invoice"
            ' Keep the successful baseline when a poll fails. Log once until recovery.
            If Not readyPollFailureLogged Then
                ErrorLogger.LogError(ex, "Ready for invoicing notification")
                readyPollFailureLogged = True
            End If
        Finally
            readyPollRunning = False
        End Try
    End Function

    Private Sub UpdateReadyStatus()
        cmdForInvoice.Text = "For Invoice (" & currentReadyGroups.Count & ")"
        readyStatus.Text = If(pendingReadyGroups.Count > 0,
            pendingReadyGroups.Count & " newly ready - open",
            "Ready for invoicing: " & currentReadyGroups.Count & " - open")
        readyStatus.IsLink = True
        readyStatus.LinkColor = If(pendingReadyGroups.Count > 0, Color.DarkGreen, Color.DarkBlue)
        readyStatus.Visible = StoreProcessingSettings.ShowOrderNotifications AndAlso
            StoreProcessingSettings.ShowForInvoiceButton AndAlso
            StoreProcessingSettings.EnableReadyOrderNotifications
    End Sub

    Private Async Sub ShowReadyOrders()
        If Not StoreProcessingSettings.ShowForInvoiceButton Then Return
        Try
            Using invoice As New frmForInvoice()
                If pendingReadyGroups.Count > 0 Then
                    invoice.PreferredGroup = pendingReadyGroups.Min()
                ElseIf currentReadyGroups.Count = 1 Then
                    invoice.PreferredGroup = currentReadyGroups(0)
                End If
                pendingReadyGroups.Clear()
                If readyStatus IsNot Nothing Then UpdateReadyStatus()
                invoice.ShowDialog(Me)
            End Using
            Await PollReadyOrdersAsync()
        Catch ex As Exception
            If Not orderToolsClosing AndAlso Not IsDisposed Then
                MessageBox.Show(Me, ex.Message, "For invoicing", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End Try
    End Sub

    Private Sub StopOrderTools(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        orderToolsClosing = True
        If readyTimer IsNot Nothing Then
            readyTimer.Stop()
            readyTimer.Dispose()
            readyTimer = Nothing
        End If
    End Sub
End Class
