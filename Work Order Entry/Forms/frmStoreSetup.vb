Imports Org.BouncyCastle.X509

Public Class frmStoreSetup

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub frmStoreSetup_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try
            ' Load available store databases into dropdown
            Dim storeList As New List(Of String)
            Try
                If dbInitial IsNot Nothing Then
                    storeList = (From a In dbInitial.Stores Select a.StoreCode).Distinct().ToList()
                End If
            Catch exStore As Exception
            End Try

            ' Fallback: add current database name if store list is empty
            If storeList.Count = 0 AndAlso Not String.IsNullOrEmpty(ModConnectDB.dbn) Then
                storeList.Add(ModConnectDB.dbn)
            End If

            cmbDatabases.DataSource = storeList

            ' Pre-select current database
            If Not String.IsNullOrEmpty(DB_Conn("dbn")) AndAlso storeList.Contains(DB_Conn("dbn")) Then
                cmbDatabases.SelectedItem = DB_Conn("dbn")
            ElseIf storeList.Count > 0 Then
                cmbDatabases.SelectedIndex = 0
            End If

        Catch ex As Exception
            MessageBox.Show("Cannot load database locations!" & vbCrLf & ex.Message,
                            "Database Selection", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If cmbDatabases.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a database location.", "Database Selection",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If

        ' Set selected database and connect
        ModConnectDB.dbn = cmbDatabases.SelectedItem.ToString()
        db = New ItemLookUpDataContext(DB_Conn("constr"))
        dbnew = New ItemLookUpDataContext(DB_Conn("constr"))
        database_location = DB_Conn("constr")

        Me.DialogResult = DialogResult.OK
        Me.Close()

    End Sub

    Private Sub frmStoreSetup_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        If e.KeyCode = Keys.Escape Then
            Application.Exit()
        End If
    End Sub

End Class