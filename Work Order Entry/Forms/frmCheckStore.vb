Public Class frmCheckStore

    Private Sub frmCheckStore_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            gridQty.DataSource = db.SOD_fntbl_CheckQty(lblItemCode.Text)

            ' Presentation and column mappings are serialized in the Designer.
        Catch ex As Exception
            AppPrompt.Show("FROM : frmCheckStore Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
