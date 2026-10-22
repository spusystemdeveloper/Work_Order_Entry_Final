Public Class frmForInvoice

    Dim groupid As String
    Dim orders As String

    Private Sub frmForInvoice_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        Me.Dispose()
        Me.Close()

    End Sub

    Private Sub btnGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGroup.Click

        If Not groupid = 0 Then
            clsItemLookUp.UpdateQueueStatus(groupid, "For Invoicing")

            Dim SearchStrArr() As String = Split(orders, ", ")

            For Each x In SearchStrArr
                clsItemLookUp.fakeUpdate(x)
                frmPrintWo.wo = x
                frmPrintWo.ShowDialog()

            Next

            gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
            groupid = 0
        Else
            MessageBox.Show("Please select an Order", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
      

    End Sub


    Private Sub gridCustOrder_DataBindingComplete(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewBindingCompleteEventArgs) Handles gridCustOrder.DataBindingComplete
        Try
            Me.gridCustOrder.Rows(0).Selected = False
            groupid = 0
        Catch ex As Exception

        End Try

    End Sub

    Private Sub gridCustOrder_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustOrder.CellClick
        Try
            Dim index As Integer

            index = e.RowIndex

            Dim selectedRow As DataGridViewRow
            selectedRow = gridCustOrder.Rows(index)

            groupid = selectedRow.Cells(0).Value.ToString()
            orders = selectedRow.Cells(2).Value.ToString()

        Catch ex As Exception
            groupid = 0
        End Try
    End Sub
End Class