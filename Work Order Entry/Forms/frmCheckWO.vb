Public Class frmCheckWO
    Private Sub frmCheckWO_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        gridQtyWO.DataSource = db.SOD_fntbl_CheckOpenWO(lblItemCodeWO.Text)

        ' Column presentation is serialized in frmCheckWO.Designer.vb.
    End Sub

    Private Sub cmdClose_Click(sender As Object, e As EventArgs) Handles cmdClose.Click
        Me.Dispose()
    End Sub

    Private Sub frmCheckWO_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Me.Dispose()
    End Sub
End Class