Public Class frmCheckWO
    Private Sub frmCheckWO_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        gridQtyWO.DataSource = db.SOD_fntbl_CheckOpenWO(lblItemCodeWO.Text)

        gridQtyWO.Columns(0).Width = 100
        gridQtyWO.Columns(1).Width = 100
        gridQtyWO.Columns(2).Width = 200
        gridQtyWO.Columns(3).Width = 160
        gridQtyWO.Columns(6).Width = 160
        gridQtyWO.Columns(7).Width = 140
        gridQtyWO.Columns(8).Width = 80

        gridQtyWO.Columns(0).HeaderText = "ORDER ID"
        gridQtyWO.Columns(1).HeaderText = "STATUS"
        gridQtyWO.Columns(2).HeaderText = "COMPANY"
        gridQtyWO.Columns(3).HeaderText = "ACCOUNT NUMBER"
        gridQtyWO.Columns(4).HeaderText = "ITEMLOOKUP"
        gridQtyWO.Columns(5).HeaderText = "DESCRIPTION"
        gridQtyWO.Columns(6).HeaderText = "QTY ORDERED"
        gridQtyWO.Columns(7).HeaderText = "ORDER TAKER"
        gridQtyWO.Columns(8).HeaderText = "DateCreated"
        gridQtyWO.Columns(9).HeaderText = "No.Days"



        gridQtyWO.Columns(4).Visible = False
        gridQtyWO.Columns(5).Visible = False

        'Dim centerCols As Integer() = {0, 1, 3, 4, 6}

        'For Each colIndex As Integer In centerCols
        '    gridQtyWO.Columns(colIndex).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        '    gridQtyWO.Columns(colIndex).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        'Next

        ' Keep text-heavy columns left-aligned (optional for clarity)
        'Dim leftCols As Integer() = {2, 5, 7}
        'For Each colIndex As Integer In leftCols
        '    gridQtyWO.Columns(colIndex).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        '    gridQtyWO.Columns(colIndex).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft
        'Next
    End Sub

    Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
        Me.Dispose()
    End Sub

    Private Sub frmCheckWO_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Me.Dispose()
    End Sub
End Class