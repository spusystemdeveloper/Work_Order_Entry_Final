Public Class frmCheckStore

    Private Sub frmCheckStore_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            gridQty.DataSource = db.SOD_fntbl_CheckQty(lblItemCode.Text)

            'for column width
            gridQty.Columns(0).Width = 40
            gridQty.Columns(1).Width = 200
            gridQty.Columns(2).Width = 200
            gridQty.Columns(3).Width = 70
            gridQty.Columns(4).Width = 300
            'for column name
            gridQty.Columns(0).HeaderText = "ID"
            gridQty.Columns(1).HeaderText = "STORE NAME"
            gridQty.Columns(2).HeaderText = "STORE CODE"
            gridQty.Columns(3).HeaderText = "QTY"
            gridQty.Columns(4).HeaderText = "ADDRESS"
            gridQty.Columns(4).Visible = False

            'for format & alignment
            gridQty.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
        Catch ex As Exception
            MessageBox.Show("FROM : frmCheckStore Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click

        Me.Dispose()

    End Sub

    Private Sub frmCheckStore_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Me.Dispose()
    End Sub

End Class