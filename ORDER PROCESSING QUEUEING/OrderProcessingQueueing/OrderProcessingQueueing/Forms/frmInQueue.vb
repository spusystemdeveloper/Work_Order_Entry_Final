Public Class frmInQueue

    Private Sub gridGroupID_CellDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridGroupID.CellDoubleClick

        Try
            Dim index As Integer

            index = e.RowIndex

            Dim selectedRow As DataGridViewRow
            selectedRow = gridGroupID.Rows(index)

            Dim orderid = selectedRow.Cells(0).Value.ToString()

            frmMain.focustoGroup(orderid)
            gridGroupID.Rows.Clear()
            Me.Dispose()
            Me.Close()

        Catch ex As Exception

        End Try

    End Sub
End Class