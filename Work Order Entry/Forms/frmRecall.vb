Public Class frmRecall

    Dim parm As String

    Dim recallData As New BindingSource
    Private searchDebounceTimer As Windows.Forms.Timer

    Private Sub frmRecall_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub

    Public Sub LoadOrder(ByVal iType As Integer)

        recallData.DataSource = clsRecall.LoadOrders(iType)
        gridOrder.DataSource = recallData
        With gridOrder
            .Columns(0).HeaderText = "Order#"
            .Columns(1).HeaderText = "Date"
            .Columns(2).HeaderText = "Reference"
            .Columns(3).HeaderText = "Customer"
            .Columns(4).HeaderText = "Comment"
        End With

        gridOrder.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        gridOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        parm = "Order#"
        lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")

        frmItemLookUp.chkBoxQtoWo.Checked = False

        'optWork.Checked = True

        Me.ActiveControl = txtSearch
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
    End Sub

    Private Sub optWork_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optWork.CheckedChanged

        If optWork.Checked = True Then

            recallData.DataSource = clsRecall.LoadOrders(2)
            gridOrder.DataSource = recallData
            lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")
            txtSearch.Focus()
        End If

    End Sub

    Private Sub optQuote_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optQuote.CheckedChanged

        If optQuote.Checked = True Then
            'comment 1 change from true to false
            CheckBox1.Checked = False
            recallData.DataSource = clsRecall.LoadOrders(3)
            gridOrder.DataSource = recallData
            lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")
            txtSearch.Focus()
        End If

    End Sub

    Private Sub txtSearch_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Down Then
            gridOrder.Focus()
        ElseIf e.KeyCode = Keys.Left Then
            optQuote.Checked = True
            txtSearch.Focus()
        ElseIf e.KeyCode = Keys.Right Then
            optWork.Checked = True
            txtSearch.Focus()
        End If
    End Sub

    Private Sub gridOrder_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridOrder.DoubleClick
        If gridOrder.RowCount > 0 Then
            frmItemLookUp.RecallQuote(gridOrder.Item(0, gridOrder.CurrentRow.Index).Value)

            Me.Dispose()
        End If
    End Sub

    Private Sub gridOrder_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridOrder.KeyDown
        If e.KeyCode = Keys.Left Then
            optQuote.Checked = True
            ' txtSearch.Focus()
        ElseIf e.KeyCode = Keys.Right Then
            optWork.Checked = True
            ' txtSearch.Focus()
        ElseIf e.KeyCode = Keys.Enter Then
            frmItemLookUp.RecallQuote(gridOrder.Item(0, gridOrder.CurrentRow.Index).Value)

            Me.Dispose()
        End If
    End Sub

    Private Sub gridOrder_ColumnHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles gridOrder.ColumnHeaderMouseClick
        parm = gridOrder.Columns(e.ColumnIndex).HeaderText
    End Sub

    Private Sub txtSearch_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyUp
        If e.KeyCode = Keys.Enter Then
            If searchDebounceTimer IsNot Nothing Then searchDebounceTimer.Stop()
            ApplyRecallFilter()
        End If
    End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        If gridOrder.RowCount > 0 Then
            frmItemLookUp.RecallQuote(gridOrder.Item(0, gridOrder.CurrentRow.Index).Value)
            WoRecallType = 1
            Me.Dispose()
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If optWork.Checked = True Then
            LoadOrder(2)
        ElseIf optQuote.Checked = True Then
            LoadOrder(3)
        End If
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        If searchDebounceTimer Is Nothing Then
            searchDebounceTimer = New Windows.Forms.Timer()
            searchDebounceTimer.Interval = 300
            AddHandler searchDebounceTimer.Tick, AddressOf SearchDebounceTimer_Tick
        End If
        searchDebounceTimer.Stop()
        searchDebounceTimer.Start()
    End Sub

    Private Sub SearchDebounceTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        searchDebounceTimer.Stop()
        ApplyRecallFilter()
    End Sub

    Private Sub ApplyRecallFilter()
        Dim filterText As String = txtSearch.Text.Replace("'", "''").Trim()

        If String.IsNullOrEmpty(filterText) Then
            recallData.RemoveFilter()
        ElseIf parm = "Order#" Then
            recallData.Filter = "[Order#] Like '%" & filterText & "%'"
        ElseIf parm = "Date" Then
            recallData.Filter = "[Date] Like '%" & filterText & "%'"
        ElseIf parm = "Reference" Then
            recallData.Filter = "[Reference] Like '%" & filterText & "%'"
        ElseIf parm = "Customer" Then
            recallData.Filter = "[Customer] Like '%" & filterText & "%'"
        ElseIf parm = "Comment" Then
            recallData.Filter = "[Comment] Like '%" & filterText & "%'"
        Else
            recallData.Filter = "[Order#] Like '%" & filterText & "%'"
        End If

        lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")
    End Sub

    Private Sub frmRecall_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If searchDebounceTimer IsNot Nothing Then
            searchDebounceTimer.Stop()
            searchDebounceTimer.Dispose()
            searchDebounceTimer = Nothing
        End If
    End Sub

End Class