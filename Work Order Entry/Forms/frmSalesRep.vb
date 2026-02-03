Public Class frmSalesRep

    Private Sub frmSalesRep_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        gridSalesRep.DataSource = clsSalesRep.SearchSalesRep(txtSales.Text)
        gridCustomerWidth()

    End Sub

    Private Sub gridCustomerWidth()

        gridSalesRep.Columns(0).Width = 100
        gridSalesRep.Columns(1).Width = 369

    End Sub

    Private Sub txtSales_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSales.KeyDown

        If e.KeyCode = Keys.Down Then
            gridSalesRep.Focus()
        End If

    End Sub

    Private Sub txtSales_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSales.KeyPress

        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
            gridSalesRep.Focus()
            Me.gridSalesRep.DataSource = clsSalesRep.SearchSalesRep(txtSales.Text)
            gridCustomerWidth()
        ElseIf e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then
            Me.Dispose()
        End If

    End Sub
    Private Sub GetSalesRepInfo(ByVal iRow As Integer)

        iSalesID = gridSalesRep.Item(0, iRow).Value
        frmItemLookUp.txtSales.Text = gridSalesRep.Item(1, iRow).Value
        frmItemLookUp.txtCustomer.Focus()
        Me.Dispose()

    End Sub

    Private Sub cmdOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOk.Click

        GetSalesRepInfo(iRowSales)

    End Sub

    Private Sub CallIndex()

        If gridSalesRep.RowCount = 0 Then
            Exit Sub
        End If

        iRowSales = clsItemLookUp.RowIndex(gridSalesRep)

    End Sub

    Private Sub gridSalesRep_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSalesRep.Click

        CallIndex()

    End Sub

    Private Sub gridSalesRep_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSalesRep.DoubleClick

        GetSalesRepInfo(iRowSales)

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click

        Me.Dispose()

    End Sub

    Private Sub gridSalesRep_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles gridSalesRep.KeyPress

        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then

            iRowSales = clsItemLookUp.RowIndexEnter(gridSalesRep, iRowSales)
            GetSalesRepInfo(iRowSales)

        ElseIf e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then

            Me.Dispose()

        End If

    End Sub
    Private Sub gridSalesRep_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridSalesRep.KeyUp

        CallIndex()

    End Sub

   
    Private Sub txtSales_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSales.TextChanged

    End Sub

    Private Sub gridSalesRep_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridSalesRep.CellContentClick

    End Sub
End Class