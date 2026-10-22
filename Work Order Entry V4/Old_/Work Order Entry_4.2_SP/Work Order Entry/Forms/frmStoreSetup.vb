Public Class frmStoreSetup

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub frmStoreSetup_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            cmbDatabases.DataSource = (From a In dbInitial.Stores Select a).ToList
            cmbDatabases.ValueMember = "StoreCode"
            cmbDatabases.DisplayMember = "StoreCode"

        Catch ex As Exception

        End Try

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        ModConnectDB.dbn = cmbDatabases.Text

        db = New ItemLookUpDataContext(DB_Conn("constr"))
        dbnew = New ItemLookUpDataContext(DB_Conn("constr"))

        Me.Hide()
        frmItemLookUp.Show()

    End Sub

    Private Sub frmStoreSetup_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        If e.KeyCode = Keys.Escape Then
            Application.Exit()
        End If
    End Sub
End Class