Public Class frmPickerSummary

    Private _mainForm As frmMain

    Public Sub New(ByVal mainForm As frmMain)
        InitializeComponent()
        _mainForm = mainForm
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private _isLeaderboardView As Boolean = False

    Private Sub frmPickerSummary_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        RefreshCurrentView()
        timerRefresh.Start()
    End Sub

    Private Sub frmPickerSummary_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        timerRefresh.Stop()
    End Sub

    Private Sub timerRefresh_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timerRefresh.Tick
        RefreshCurrentView()
    End Sub

    Private Sub RefreshCurrentView()
        If _isLeaderboardView Then
            LoadLeaderboard()
        Else
            LoadSummary()
        End If
    End Sub

    Public Sub LoadSummary()
        Try
            ConfigureColumnsForWorkload()

            Dim tallies As List(Of PickerTallyInfo)
            If _mainForm IsNot Nothing Then
                tallies = _mainForm.GetActivePickerTallies()
            Else
                tallies = New List(Of PickerTallyInfo)()
            End If

            Dim selectedPicker As String = ""
            If gridPickerSummary.CurrentRow IsNot Nothing AndAlso gridPickerSummary.CurrentRow.Index >= 0 Then
                selectedPicker = If(gridPickerSummary.CurrentRow.Cells(0).Value, "").ToString()
            End If

            gridPickerSummary.Rows.Clear()

            Dim totalInProcess As Integer = 0
            Dim totalPrepared As Integer = 0
            Dim totalAllItems As Integer = 0
            Dim activePickerCount As Integer = 0

            For Each info In tallies
                Dim rowIdx As Integer = gridPickerSummary.Rows.Add()
                Dim row As DataGridViewRow = gridPickerSummary.Rows(rowIdx)

                row.Cells(0).Value = info.PickerName
                row.Cells(1).Value = info.InProcessCount
                row.Cells(2).Value = info.PreparedCount
                row.Cells(3).Value = info.TotalCount
                row.Cells(4).Value = info.OrderCount

                If info.PickerName = "[Unassigned]" Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199) ' Light amber
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9)   ' Dark amber
                    row.DefaultCellStyle.Font = New Font("Century Gothic", 9.75!, FontStyle.Bold)
                Else
                    activePickerCount += 1
                End If

                totalInProcess += info.InProcessCount
                totalPrepared += info.PreparedCount
                totalAllItems += info.TotalCount

                If Not String.IsNullOrEmpty(selectedPicker) AndAlso info.PickerName = selectedPicker Then
                    row.Selected = True
                End If
            Next

            lblSummary.Text = String.Format("Active Pickers: {0} | Items: {1} In Process, {2} Prepared (Total: {3})",
                                            activePickerCount, totalInProcess, totalPrepared, totalAllItems)

        Catch ex As Exception
            ' Silent guard to keep UI stable
        End Try
    End Sub

    Public Sub LoadLeaderboard()
        Try
            ConfigureColumnsForLeaderboard()

            Dim productivity = clsQueueing.GetTodayPickerProductivity()

            Dim selectedPicker As String = ""
            If gridPickerSummary.CurrentRow IsNot Nothing AndAlso gridPickerSummary.CurrentRow.Index >= 0 Then
                selectedPicker = If(gridPickerSummary.CurrentRow.Cells(1).Value, "").ToString()
            End If

            gridPickerSummary.Rows.Clear()

            Dim totalItemsToday As Integer = productivity.Sum(Function(p) p.ItemsPreparedToday)
            Dim totalOrdersToday As Integer = productivity.Sum(Function(p) p.OrdersCompletedToday)

            For Each p In productivity
                Dim rowIdx As Integer = gridPickerSummary.Rows.Add()
                Dim row As DataGridViewRow = gridPickerSummary.Rows(rowIdx)

                Dim rankBadge As String = p.Rank.ToString()
                If p.Rank = 1 Then rankBadge = "🥇 1"
                If p.Rank = 2 Then rankBadge = "🥈 2"
                If p.Rank = 3 Then rankBadge = "🥉 3"

                row.Cells(0).Value = rankBadge
                row.Cells(1).Value = p.PickerName
                row.Cells(2).Value = p.ItemsPreparedToday
                row.Cells(3).Value = p.OrdersCompletedToday

                Dim pct As Double = If(totalItemsToday > 0, (p.ItemsPreparedToday / CDbl(totalItemsToday)) * 100, 0)
                row.Cells(4).Value = String.Format("{0:0.#}%", pct)

                If p.Rank = 1 Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 195) ' Pale gold
                    row.DefaultCellStyle.Font = New Font("Century Gothic", 9.75!, FontStyle.Bold)
                End If

                If Not String.IsNullOrEmpty(selectedPicker) AndAlso p.PickerName = selectedPicker Then
                    row.Selected = True
                End If
            Next

            Dim topName As String = If(productivity.Count > 0, productivity(0).PickerName, "None")
            lblSummary.Text = String.Format("Today: {0} items prepared across {1} orders | Top: {2}",
                                            totalItemsToday, totalOrdersToday, topName)

        Catch ex As Exception
            ' Silent guard
        End Try
    End Sub

    Private Sub ConfigureColumnsForWorkload()
        lblTitle.Text = "👥 Warehouse Picker Workload Summary"
        lblSubtitle.Text = "Real-time picker workload. Double-click any picker to filter cards on screen."
        btnViewMode.Text = "🏆 Leaderboard"
        btnFilterCards.Enabled = True

        colPicker.HeaderText = "Picker Name"
        colInProcess.HeaderText = "In Process"
        colPrepared.HeaderText = "Prepared"
        colTotalItems.HeaderText = "Total Items"
        colOrders.HeaderText = "Orders"
    End Sub

    Private Sub ConfigureColumnsForLeaderboard()
        lblTitle.Text = "🏆 Today's Picker Productivity Leaderboard"
        lblSubtitle.Text = "Rankings based on total items prepared and orders completed today."
        btnViewMode.Text = "📋 Workload"
        btnFilterCards.Enabled = False

        colPicker.HeaderText = "Rank"
        colInProcess.HeaderText = "Picker Name"
        colPrepared.HeaderText = "Items Today"
        colTotalItems.HeaderText = "Orders Today"
        colOrders.HeaderText = "Share %"
    End Sub

    Private Sub btnViewMode_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnViewMode.Click
        _isLeaderboardView = Not _isLeaderboardView
        RefreshCurrentView()
    End Sub

    Private Sub btnReassignPicker_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnReassignPicker.Click
        Try
            Dim defaultFrom As String = ""
            If gridPickerSummary.CurrentRow IsNot Nothing AndAlso gridPickerSummary.CurrentRow.Index >= 0 Then
                Dim val = If(gridPickerSummary.CurrentRow.Cells(If(_isLeaderboardView, 1, 0)).Value, "").ToString()
                If val <> "[Unassigned]" AndAlso Not val.StartsWith("🥇") AndAlso Not val.StartsWith("🥈") Then
                    defaultFrom = val
                End If
            End If

            Dim fromPicker As String = InputBox("Enter picker name to reassign FROM:" & vbCrLf & "(e.g. ALMER, JUN, MARK, AJAY)", "Reassign Picker - From", defaultFrom).Trim().ToUpper()
            If String.IsNullOrEmpty(fromPicker) Then Exit Sub

            Dim toPicker As String = InputBox("Enter target picker name to reassign TO:" & vbCrLf & "(All active In-Process/Pending items will be transferred)", "Reassign Picker - To", "").Trim().ToUpper()
            If String.IsNullOrEmpty(toPicker) Then Exit Sub

            If fromPicker.Equals(toPicker, StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("Source and target picker cannot be the same.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If

            Dim count As Integer = clsQueueing.ReassignPickerBatch(fromPicker, toPicker)
            If count > 0 Then
                MessageBox.Show(String.Format("Successfully reassigned {0} item(s) from {1} to {2}.", count, fromPicker, toPicker), "Reassignment Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                If _mainForm IsNot Nothing Then
                    _mainForm.formLoad()
                End If
                RefreshCurrentView()
            Else
                MessageBox.Show(String.Format("No active pending/in-process items found for picker '{0}'.", fromPicker), "No Items Found", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Error during reassignment: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnFilterCards_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilterCards.Click
        ApplySelectedPickerFilter()
    End Sub

    Private Sub gridPickerSummary_CellDoubleClick(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles gridPickerSummary.CellDoubleClick
        If e.RowIndex >= 0 AndAlso Not _isLeaderboardView Then
            ApplySelectedPickerFilter()
        End If
    End Sub

    Private Sub ApplySelectedPickerFilter()
        If gridPickerSummary.CurrentRow Is Nothing OrElse gridPickerSummary.CurrentRow.Index < 0 Then
            MessageBox.Show("Please select a picker to filter.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim pickerName As String = If(gridPickerSummary.CurrentRow.Cells(0).Value, "").ToString()
        If String.IsNullOrEmpty(pickerName) Then Return

        If pickerName = "[Unassigned]" Then
            pickerName = "-"
        End If

        If _mainForm IsNot Nothing Then
            _mainForm.FilterByPicker(pickerName)
        End If

        Me.Close()
    End Sub

    Private Sub btnClearFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClearFilter.Click
        If _mainForm IsNot Nothing Then
            _mainForm.ClearSearchFilter()
        End If
        Me.Close()
    End Sub

    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

End Class

Public Class PickerTallyInfo
    Public Property PickerName As String = ""
    Public Property InProcessCount As Integer = 0
    Public Property PreparedCount As Integer = 0
    Public Property TotalCount As Integer = 0
    Public Property OrderCount As Integer = 0
End Class
