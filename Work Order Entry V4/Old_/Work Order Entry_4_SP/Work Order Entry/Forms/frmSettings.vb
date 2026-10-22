Public Class frmSettings

    Private Sub frmSettings_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        cmbDefaultWhse.DataSource = (From a In db.Stores Select a.ID, a.Name).ToList
        cmbDefaultWhse.DisplayMember = "Name"
        cmbDefaultWhse.ValueMember = "ID"

        For Each n In clsItemLookUp.LoadSettings
            txtLimit.Text = n.LimitEntry
            txtPassword.Text = n.Password
            cmbLevel.Text = n.PriceLevel
            chkPriceChange.Checked = n.AllowPriceChange
            If n.StoreID = 0 Then
                chkDisableWhse.Checked = False
                cmbDefaultWhse.Enabled = False
            Else
                chkDisableWhse.Checked = True
                cmbDefaultWhse.Enabled = True
                cmbDefaultWhse.SelectedValue = n.StoreID
            End If
        Next

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
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

        If txtPassword.Text <> txtConfirmP.Text Then
            iErr = iErr + 1
            MsgBox("Password did not match!", vbCritical, "Error")
        End If

        If txtPassword.Text = String.Empty Then
            iErr = iErr + 1
            MsgBox("Please input a password!", vbCritical, "Error")
        End If

        If chkDisableWhse.Checked = True Then
            iStoreID = cmbDefaultWhse.SelectedValue
        Else
            iStoreID = 0
        End If

        If iErr = 0 Then
            clsItemLookUp.uptSettings(txtLimit.Text, txtPassword.Text, cmbLevel.Text, iStoreID, chkPriceChange.Checked)
            MsgBox("Settings successfully updated!", vbInformation, "Message")
            Me.Close()
        End If

        If chkPriceChange.Checked = True Then
            bAllow = True
        End If

    End Sub

    Private Sub chkDisableWhse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDisableWhse.CheckedChanged
        If chkDisableWhse.Checked = True Then
            cmbDefaultWhse.Enabled = True
        Else
            cmbDefaultWhse.Enabled = False
        End If
    End Sub

 
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        frmUser.ShowDialog()

    End Sub
End Class