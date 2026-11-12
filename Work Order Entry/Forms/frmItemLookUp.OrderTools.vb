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
        ApplyStatusBarStyle()
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
        ApplyStatusBarStyle()
        Await PollReadyOrdersAsync()
        Await PollShortageAlertsAsync()
    End Sub

    Private statusBarFont As Font

    ' The default status bar renderer ignores a label's BackColor, which the
    ' "ready for invoicing" badge needs.
    Private NotInheritable Class StatusBarRenderer
        Inherits ToolStripSystemRenderer

        Protected Overrides Sub OnRenderLabelBackground(e As ToolStripItemRenderEventArgs)
            MyBase.OnRenderLabelBackground(e)
            Dim color As Color = e.Item.BackColor
            If color = Drawing.Color.Empty OrElse color = Drawing.Color.Transparent Then Return
            Using brush As New SolidBrush(color)
                e.Graphics.FillRectangle(brush, New Rectangle(Point.Empty, e.Item.Size))
            End Using
        End Sub
    End Class

    ' One plain look for every status item: same font, thin dividers, even padding.
    ' Only appearance changes; texts and click handlers are untouched.
    Private Sub ApplyStatusBarStyle()
        If statusBarFont Is Nothing Then statusBarFont = New Font("Tahoma", 9.5F, FontStyle.Bold)
        Dim textColor As Color = Color.FromArgb(20, 20, 20)
        BottomStrip.BackColor = Color.FromArgb(244, 246, 249)
        If Not TypeOf BottomStrip.Renderer Is StatusBarRenderer Then BottomStrip.Renderer = New StatusBarRenderer()
        BottomStrip.Font = statusBarFont
        ' The old constant "STATUS" caption carried no information; keep the item as the spacer.
        ToolStripStatusLabel1.Text = String.Empty

        Dim divided As ToolStripItem() = {statDb, ToolStripStatusLabel3, ToolStripStatusLabel2,
                                          lblOrderNo, lblStatItemSelected, lblStatItemCount}
        For Each item As ToolStripItem In BottomStrip.Items
            item.Font = statusBarFont
            If item IsNot shortageStatus Then item.ForeColor = textColor
            Dim label = TryCast(item, ToolStripStatusLabel)
            If label Is Nothing OrElse label Is readyStatus Then Continue For
            label.BorderSides = ToolStripStatusLabelBorderSides.None
            label.Padding = New Padding(8, 0, 8, 0)
        Next
        For Each item As ToolStripItem In divided
            Dim label = TryCast(item, ToolStripStatusLabel)
            If label Is Nothing Then Continue For
            label.BorderSides = ToolStripStatusLabelBorderSides.Left
            label.BorderStyle = Border3DStyle.Etched
        Next
        If readyStatus IsNot Nothing Then StyleReadyBadge(pendingReadyGroups.Count > 0)
    End Sub

    ' "Ready for invoicing" as a small badge: blue normally, green when new orders are ready.
    Private Sub StyleReadyBadge(hasNew As Boolean)
        If readyStatus Is Nothing Then Return
        readyStatus.Font = New Font("Tahoma", 9.5F, FontStyle.Bold)
        readyStatus.LinkBehavior = LinkBehavior.NeverUnderline
        readyStatus.LinkColor = Color.White
        readyStatus.ActiveLinkColor = Color.White
        readyStatus.VisitedLinkColor = Color.White
        readyStatus.BackColor = If(hasNew, Color.FromArgb(22, 128, 70), Color.FromArgb(38, 99, 158))
        readyStatus.Padding = New Padding(8, 0, 8, 0)
        readyStatus.Margin = New Padding(6, 3, 6, 2)
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
            AppPrompt.Show(Me, "Recall an order before reloading it.", "Reload order",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If AppPrompt.Show(Me, "Reload the latest saved order? Unsaved changes on this screen will be discarded.",
                           "Reload order", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            RecallQuote(activeRecallOrderID)
        End If
    End Sub

    Private Sub ShowOrderConflict(ex As OrderEditConflictException)
        AppPrompt.Show(Me, ex.Message, "Order changed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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
            readyStatus.BackColor = Color.Empty
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
        StyleReadyBadge(pendingReadyGroups.Count > 0)
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
                AppPrompt.Show(Me, ex.Message, "For invoicing", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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
