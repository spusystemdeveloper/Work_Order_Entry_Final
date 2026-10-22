Public Class frmRecall
    Dim parm As String

    Private Sub frmRecall_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub

    Private Sub frmRecall_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LoadOrder()
    End Sub

    Public Sub LoadOrder()

        gridOrder.DataSource = clsRecall.LoadOrders(String.Empty, "Order#", 4)

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

        optWork.Checked = True
        Me.ActiveControl = txtSearch

    End Sub

    'Private Sub LoadWO()
    '    gridOrder.DataSource = clsRecall.LoadWO

    '    With gridOrder
    '        .Columns(0).HeaderText = "Work Order#"
    '        .Columns(1).HeaderText = "Date"
    '        .Columns(2).HeaderText = "Reference"
    '        .Columns(3).HeaderText = "Customer"
    '        .Columns(4).HeaderText = "Comment"
    '    End With

    '    gridOrder.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    '    lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")

    'End Sub
    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
    End Sub

    Private Sub optWork_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optWork.CheckedChanged
        If optWork.Checked = True Then
            gridOrder.DataSource = clsRecall.LoadOrders(String.Empty, "Order#", 2)
            lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")
            txtSearch.Focus()
        End If
    End Sub

    Private Sub optQuote_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optQuote.CheckedChanged
        If optQuote.Checked = True Then
            gridOrder.DataSource = clsRecall.LoadOrders(String.Empty, "Order#", 3)
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
            txtSearch.Focus()
        ElseIf e.KeyCode = Keys.Right Then
            optWork.Checked = True
            txtSearch.Focus()
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
            gridOrder.DataSource = clsRecall.LoadOrders(txtSearch.Text, parm, IIf(optQuote.Checked = True, 3, 4))
            lblRec.Text = gridOrder.RowCount & IIf(gridOrder.RowCount > 1, " entries", " entry")
        End If
    End Sub


    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        If gridOrder.RowCount > 0 Then
            frmItemLookUp.RecallQuote(gridOrder.Item(0, gridOrder.CurrentRow.Index).Value)
            Me.Dispose()
        End If
    End Sub
End Class