Imports System.Linq
Imports System.Threading.Tasks

Partial Public Class frmItemLookUp
    Private shortageAlertStatus As ToolStripStatusLabel
    Private shortageAlertMonitor As New ShortageNotifications()
    Private currentShortageAlerts As New List(Of WarehouseShortage)()
    Private pendingShortageAlerts As New HashSet(Of Integer)()
    Private shortageAlertPollRunning As Boolean
    Private shortageAlertWindowOpen As Boolean
    Private shortageAlertConnectionString As String
    Private shortageAlertFailureLogged As Boolean

    Private Sub InitializeShortageAlerts()
        shortageAlertStatus = New ToolStripStatusLabel With {.IsLink = True, .Visible = False}
        AddHandler shortageAlertStatus.Click, AddressOf OpenShortageAlerts
        BottomStrip.Items.Add(shortageAlertStatus)
    End Sub

    Private Async Function PollShortageAlertsAsync() As Task
        If shortageAlertPollRunning OrElse orderToolsClosing OrElse shortageAlertStatus Is Nothing Then Return
        If Not StoreProcessingSettings.EnableShortageNotifications Then
            shortageAlertStatus.Visible = False
            shortageAlertMonitor = New ShortageNotifications()
            currentShortageAlerts.Clear()
            pendingShortageAlerts.Clear()
            Return
        End If
        shortageAlertPollRunning = True
        Try
            Dim connectionString = DB_Conn("constr")
            If Not String.Equals(connectionString, shortageAlertConnectionString, StringComparison.Ordinal) Then
                shortageAlertMonitor = New ShortageNotifications()
                currentShortageAlerts.Clear()
                pendingShortageAlerts.Clear()
                shortageAlertConnectionString = connectionString
            End If
            Dim lines = Await Task.Run(Function() ShortageNotifications.Load(connectionString))
            If orderToolsClosing OrElse IsDisposed Then Return
            If Not StoreProcessingSettings.EnableShortageNotifications OrElse
                Not String.Equals(connectionString, DB_Conn("constr"), StringComparison.Ordinal) Then Return
            pendingShortageAlerts.UnionWith(shortageAlertMonitor.Observe(lines))
            pendingShortageAlerts.IntersectWith(lines.Select(Function(line) line.OrderID))
            currentShortageAlerts = lines
            shortageAlertFailureLogged = False
            UpdateShortageAlertStatus()
        Catch ex As Exception
            If orderToolsClosing OrElse IsDisposed Then Return
            shortageAlertStatus.Text = "Shortage check unavailable - retrying"
            shortageAlertStatus.IsLink = False
            shortageAlertStatus.ForeColor = Color.DarkOrange
            shortageAlertStatus.Visible = True
            If Not shortageAlertFailureLogged Then
                ErrorLogger.LogError(ex, "Warehouse shortage notification")
                shortageAlertFailureLogged = True
            End If
        Finally
            shortageAlertPollRunning = False
        End Try
    End Function

    Private Sub UpdateShortageAlertStatus()
        If shortageAlertStatus Is Nothing Then Return
        Dim orders = currentShortageAlerts.Select(Function(line) line.OrderID).Distinct().OrderBy(Function(id) id).ToList()
        shortageAlertStatus.IsLink = True
        shortageAlertStatus.LinkColor = If(pendingShortageAlerts.Count > 0, Color.Firebrick, Color.DarkOrange)
        shortageAlertStatus.Text = If(orders.Count = 1,
            If(pendingShortageAlerts.Count > 0, "New shortage", "Shortage") & " WO #" & orders.FirstOrDefault() & " - open",
            "Shortages: " & orders.Count & If(pendingShortageAlerts.Count > 0, " (" & pendingShortageAlerts.Count & " new/changed)", "") & " - open")
        shortageAlertStatus.ToolTipText = String.Join(vbCrLf, orders.Take(10).Select(Function(id)
            Return "WO #" & id & " - " & currentShortageAlerts.First(Function(line) line.OrderID = id).Customer
        End Function))
        shortageAlertStatus.Visible = orders.Count > 0 AndAlso StoreProcessingSettings.EnableShortageNotifications
    End Sub

    Private Sub AcknowledgeShortageAlert(orderID As Integer)
        Dim notified = currentShortageAlerts.Where(Function(line) line.OrderID = orderID).ToList()
        Dim recalled = shortageLines.Values.GroupBy(Function(line) line.QueueItemID).Select(Function(group) group.First()).ToList()
        ' A timer can observe a newer warehouse change while the recall notice is open.
        ' Acknowledge only the shortage quantities actually loaded on this screen.
        If notified.Count <> recalled.Count OrElse notified.Any(Function(line)
            Return Not recalled.Any(Function(saved) saved.QueueItemID = line.QueueItemID AndAlso
                saved.OrderedQuantity = line.OrderedQuantity AndAlso saved.FoundQuantity.Equals(line.FoundQuantity))
        End Function) Then Return
        pendingShortageAlerts.Remove(orderID)
        UpdateShortageAlertStatus()
    End Sub

    Private Async Sub OpenShortageAlerts(sender As Object, e As EventArgs)
        If shortageAlertWindowOpen OrElse Not StoreProcessingSettings.EnableShortageNotifications Then Return
        shortageAlertWindowOpen = True
        Try
            Await PollShortageAlertsAsync()
            If orderToolsClosing OrElse IsDisposed OrElse shortageAlertFailureLogged OrElse
                Not StoreProcessingSettings.EnableShortageNotifications Then Return
            If currentShortageAlerts.Count = 0 Then
                MessageBox.Show(Me, "There are no current warehouse shortages.", "Shortage alerts",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim orderID As Integer
            Using alerts As New frmShortageAlerts(currentShortageAlerts, pendingShortageAlerts)
                If alerts.ShowDialog(Me) <> DialogResult.OK Then Return
                orderID = alerts.SelectedOrderID
            End Using
            If gridSelectItem.Rows.Cast(Of DataGridViewRow)().Any(Function(row) Not row.IsNewRow) OrElse
                activeRecallOrderID > 0 OrElse Not String.IsNullOrWhiteSpace(txtRemarks.Text) Then
                If MessageBox.Show(Me, "Open Work Order #" & orderID & "? Unsaved changes on this screen, including pending shortage reductions, will be discarded.",
                    "Open shortage order", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            End If
            ' Check eligibility before replacing the screen; RecallQuote validates it again.
            Using context = GetDB()
                OrderEditGuard.Capture(context, orderID)
            End Using
            WoRecallType = 1
            RecallQuote(orderID)
            Await PollShortageAlertsAsync()
        Catch ex As OrderEditConflictException
            ShowOrderConflict(ex)
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Shortage alerts", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            shortageAlertWindowOpen = False
        End Try
    End Sub
End Class
