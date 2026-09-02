Public Class frmSettings

    Private Sub frmSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'cmbDefaultWhse.DataSource = (From a In db.Stores Select a.ID, a.Name).ToList
        'cmbDefaultWhse.DisplayMember = "Name"
        'cmbDefaultWhse.ValueMember = "ID"

        For Each n In clsItemLookUp.LoadSettings
            txtLimit.Text = n.LimitEntry
            txtPassword.Text = n.Password
            txtPassword2.Text = n.Password2
            txtPassword3.Text = n.Password3
            cmbLevel.Text = n.PriceLevel
            'chkPriceChange.Checked = n.AllowPriceChange
            'If n.StoreID = 0 Then
            '    chkDisableWhse.Checked = False
            '    cmbDefaultWhse.Enabled = False
            'Else
            '    chkDisableWhse.Checked = True
            '    cmbDefaultWhse.Enabled = True
            '    cmbDefaultWhse.SelectedValue = n.StoreID
            'End If
        Next

        txtConfirmP.Text = String.Empty
        txtConfirmP2.Text = String.Empty

        txtMySQLdb.Text = My.Settings.MySQL_db
        txtMySQLpass.Text = My.Settings.MySQL_pass
        txtMySQLsvr.Text = My.Settings.MySQL_svr
        txtMySQLuser.Text = My.Settings.MySQL_user

        txtWebCust.Text = My.Settings.ImportDefaultCust
        txtWebRep.Text = My.Settings.ImportDefaultRep
        txtWebShip.Text = My.Settings.ImportDefaultShipping

        chkEnableBranchQueue.Checked = StoreProcessingSettings.QueueingEnabled
        chkAllowOrderGrouping.Checked = StoreProcessingSettings.AllowOrderGrouping
        chkShowForInvoice.Checked = StoreProcessingSettings.ShowForInvoiceButton
        chkShowImportButton.Checked = StoreProcessingSettings.ShowImportButton
        chkAllowBranchSelection.Checked =
            StoreProcessingSettings.BranchSelectionEnabled
        txtExpectedDatabase.Text =
            StoreProcessingSettings.ExpectedDatabaseName
        UpdateBranchFunctionControlState()
        UpdateBranchSelectionControlState()

        LoadCustType()

    End Sub




    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        UpdateSettings()
    End Sub

    Private Sub UpdateSettings()
        Dim iErr As Integer
        Dim iStoreID As Integer = 0

        iErr = 0

        If Not IsNumeric(txtLimit.Text) Then
            iErr = iErr + 1
            MsgBox("Please input a valid number!", vbCritical, "Error")
        End If

        'If chkDisableWhse.Checked = True Then
        '    iStoreID = cmbDefaultWhse.SelectedValue
        'Else
        '    iStoreID = 0
        'End If

        If iErr = 0 Then
            'clsItemLookUp.uptSettings(txtLimit.Text, txtPassword.Text, cmbLevel.Text, iStoreID, chkPriceChange.Checked)
            clsItemLookUp.uptSettings(txtLimit.Text, cmbLevel.Text, iStoreID, 0)
            MsgBox("Settings successfully updated!", vbInformation, "Message")
            Me.Close()
        End If

        'If chkPriceChange.Checked = True Then
        '    bAllow = True
        'End If

    End Sub

    Private Sub UpdatePass()
        Dim iErr As Integer
        Dim iStoreID As Integer = 0

        iErr = 0

        If txtPassword.Text <> txtConfirmP.Text Then
            iErr = iErr + 1
            MsgBox("Password did not match!", vbCritical, "Error")
        End If

        If txtPassword.Text = String.Empty Then
            iErr = iErr + 1
            MsgBox("Please input a password!", vbCritical, "Error")
        End If

        If iErr = 0 Then
            'clsItemLookUp.uptSettings(txtLimit.Text, txtPassword.Text, cmbLevel.Text, iStoreID, chkPriceChange.Checked)
            clsItemLookUp.uptPass(txtPassword.Text)
            MsgBox("Settings successfully updated!", vbInformation, "Message")
            Me.Close()
        End If

        'If chkPriceChange.Checked = True Then
        '    bAllow = True
        'End If

    End Sub
    Private Sub UpdatePass2()
        Dim iErr As Integer
        Dim iStoreID As Integer = 0

        iErr = 0

        If txtPassword2.Text <> txtConfirmP2.Text Then
            iErr = iErr + 1
            MsgBox("Password did not match!", vbCritical, "Error")
        End If

        If txtPassword2.Text = String.Empty Then
            iErr = iErr + 1
            MsgBox("Please input a password!", vbCritical, "Error")
        End If

        If iErr = 0 Then
            'clsItemLookUp.uptSettings(txtLimit.Text, txtPassword.Text, cmbLevel.Text, iStoreID, chkPriceChange.Checked)
            clsItemLookUp.uptPass2(txtPassword2.Text)
            MsgBox("Settings successfully updated!", vbInformation, "Message")
            Me.Close()
        End If

        'If chkPriceChange.Checked = True Then
        '    bAllow = True
        'End If

    End Sub

    Private Sub UpdatePass3()
        Dim iErr As Integer
        Dim iStoreID As Integer = 0

        iErr = 0

        If txtPassword3.Text <> txtConfirmP3.Text Then
            iErr = iErr + 1
            MsgBox("Password did not match!", vbCritical, "Error")
        End If

        If txtPassword3.Text = String.Empty Then
            iErr = iErr + 1
            MsgBox("Please input a password!", vbCritical, "Error")
        End If

        If iErr = 0 Then
            'clsItemLookUp.uptSettings(txtLimit.Text, txtPassword.Text, cmbLevel.Text, iStoreID, chkPriceChange.Checked)
            clsItemLookUp.uptPass3(txtPassword3.Text)
            MsgBox("Settings successfully updated!", vbInformation, "Message")
            Me.Close()
        End If

        'If chkPriceChange.Checked = True Then
        '    bAllow = True
        'End If

    End Sub

    Private Sub chkDisableWhse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'If chkDisableWhse.Checked = True Then
        '    cmbDefaultWhse.Enabled = True
        'Else
        '    cmbDefaultWhse.Enabled = False
        'End If
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        frmUser.ShowDialog()

    End Sub

    Private Sub cmdSaveBranchSettings_Click(ByVal sender As System.Object,
                                            ByVal e As System.EventArgs) Handles cmdSaveBranchSettings.Click
        Dim expectedDatabaseName As String = txtExpectedDatabase.Text.Trim()

        If Not chkAllowBranchSelection.Checked AndAlso
           String.IsNullOrWhiteSpace(expectedDatabaseName) Then
            MessageBox.Show(
                "Enter the assigned database when branch selection is disabled.",
                "Assigned Database Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation)
            txtExpectedDatabase.Focus()
            Exit Sub
        End If

        Try
            StoreProcessingSettings.SaveBranchSettings(
                chkEnableBranchQueue.Checked,
                chkAllowOrderGrouping.Checked,
                chkShowForInvoice.Checked,
                chkShowImportButton.Checked,
                chkAllowBranchSelection.Checked,
                expectedDatabaseName)
            MessageBox.Show(
                "Branch settings were saved." & vbCrLf &
                "Restart the application to apply branch selection changes.",
                "Branch Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
        Catch ex As UnauthorizedAccessException
            MessageBox.Show(
                "Windows did not allow this application to update its configuration file." &
                vbCrLf & "Run the application with permission to modify its installation folder.",
                "Unable to Save Branch Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        Catch ex As Exception
            MessageBox.Show(
                "Unable to save branch function visibility." & vbCrLf & ex.Message,
                "Unable to Save Branch Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub chkEnableBranchQueue_CheckedChanged(
        ByVal sender As System.Object,
        ByVal e As System.EventArgs) Handles chkEnableBranchQueue.CheckedChanged
        UpdateBranchFunctionControlState()
    End Sub

    Private Sub UpdateBranchFunctionControlState()
        chkAllowOrderGrouping.Enabled = chkEnableBranchQueue.Checked
        chkShowForInvoice.Enabled = chkEnableBranchQueue.Checked
    End Sub

    Private Sub chkAllowBranchSelection_CheckedChanged(
        ByVal sender As System.Object,
        ByVal e As System.EventArgs) Handles chkAllowBranchSelection.CheckedChanged
        UpdateBranchSelectionControlState()
    End Sub

    Private Sub UpdateBranchSelectionControlState()
        lblAssignedDatabase.Enabled = Not chkAllowBranchSelection.Checked
        txtExpectedDatabase.Enabled = Not chkAllowBranchSelection.Checked
    End Sub

    Private Sub cmdAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAdd.Click
        If txtCustType.Text = "" Then
            MsgBox("Please input Customer type", vbExclamation, "Message")
            Exit Sub
        End If

        If cmdAdd.Text = "Add" Then
            AddCustType()
        Else
            UpdateCustType()
        End If

    End Sub

    Private Sub AddCustType()
        'If gridCustType.Rows.Count = 0 Then
        Dim check = (From a In db.SOD_WO_CustTypes
                     Where a.CustType.Equals(txtCustType.Text)
                     Select a.CustType).Count

        If check = 0 Then
            clsCustomer.AddCustType(txtCustType.Text, cmbPriceBound.Text, cmbPricePass.Text)
        Else
            MsgBox("Customer Type already exists!", MsgBoxStyle.Exclamation, "Message")
        End If

        LoadCustType()
        'Else

        'End If
    End Sub

    Private Sub UpdateCustType()
        If gridCustType.Rows.Count > 0 Then

            Dim check = (From a In db.SOD_WO_CustTypes
                         Where a.CustType.Equals(txtCustType.Text)
                         Select a.CustType).Count

            Dim custype = gridCustType.Item(1, gridCustType.CurrentRow.Index).Value

            If txtCustType.Text <> custype Then
                If check = 0 Then
                    GoTo addcustype
                Else
                    MsgBox("Customer Type already exists!", MsgBoxStyle.Exclamation, "Message")
                    Exit Sub
                End If
            Else
                GoTo addcustype
            End If

addcustype:
            Dim typeid = gridCustType.Item(0, gridCustType.CurrentRow.Index).Value
            clsCustomer.UptCustType(typeid, txtCustType.Text, cmbPriceBound.Text, cmbPricePass.Text)
            LoadCustType()

        End If

    End Sub

    Private Sub LoadCustType()
        'Dim query = (From a In db.SOD_WO_Confs
        '         Select a.CustType).SingleOrDefault

        'gridCustType.Rows.Clear()
        'txtCustType.Text = String.Empty
        'cmdAdd.Text = "Add"

        'For Each s In Split(query, "#")
        '    If s <> String.Empty Then
        '        gridCustType.Rows.Add(s)
        '    End If
        'Next

        txtCustType.Text = String.Empty
        cmdAdd.Text = "Add"
        gridCustType.DataSource = clsCustomer.GetCustType

    End Sub

    Private Sub cmdDel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDel.Click
        If gridCustType.Rows.Count > 0 Then

            Dim typeid = gridCustType.Item(0, gridCustType.CurrentRow.Index).Value

            clsCustomer.DelCustType(typeid)
            LoadCustType()

        End If
    End Sub

    Private Sub gridCustType_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustType.CellClick
        If gridCustType.Rows.Count > 0 Then
            txtCustType.Text = gridCustType.Item(1, gridCustType.CurrentRow.Index).Value
            cmbPriceBound.Text = gridCustType.Item(2, gridCustType.CurrentRow.Index).Value
            cmbPricePass.Text = gridCustType.Item(3, gridCustType.CurrentRow.Index).Value
            cmdAdd.Text = "Upd"
        End If
    End Sub

    Private Sub cmdNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdNew.Click
        txtCustType.Text = String.Empty
        txtCustType.Focus()
        cmdAdd.Text = "Add"
    End Sub

    Private Sub cmdPass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPass.Click
        UpdatePass()
    End Sub

    Private Sub cmdPass2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPass2.Click
        UpdatePass2()
    End Sub

    Private Sub gridCustType_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles gridCustType.DoubleClick
        If gridCustType.Rows.Count > 0 Then
            frmUserCustType.Text = "Set Order Takers: " & UCase(gridCustType.Item(1, gridCustType.CurrentRow.Index).Value)
            frmUserCustType._custypeid = gridCustType.Item(0, gridCustType.CurrentRow.Index).Value
            frmUserCustType.ShowDialog(Me)
        End If
    End Sub

    Private Sub cmdPass3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPass3.Click
        UpdatePass3()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        My.Settings.MySQL_db = txtMySQLdb.Text
        My.Settings.MySQL_pass = txtMySQLpass.Text
        My.Settings.MySQL_svr = txtMySQLsvr.Text
        My.Settings.MySQL_user = txtMySQLuser.Text

        My.Settings.Save()
        Application.Restart()

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        My.Settings.ImportDefaultCust = txtWebCust.Text
        My.Settings.ImportDefaultRep = txtWebRep.Text
        My.Settings.ImportDefaultShipping = txtWebShip.Text

        My.Settings.Save()
        Application.Restart()

    End Sub

    Private Sub InsertWoLogs()



    End Sub

End Class
