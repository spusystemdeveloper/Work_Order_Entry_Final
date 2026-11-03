Public Class frmPassword

    Public sType As String = String.Empty

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
    End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        ValidateInput()
    End Sub

    Private Sub ValidateInput()

        If clsItemLookUp.ConfPass(txtPass.Text) > 0 Then
            If sType = "Price" Then
                bPass = True
                'bAllow = chkAll.Checked
                frmPriceLevel.LoadLevels()
                RecordPriceApproval()
            ElseIf sType = "Settings" Then
                frmSettings.ShowDialog()
            ElseIf sType <> "CancelOrder" Then
                RecordPriceApproval()
            End If

            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()

        Else
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            AppPrompt.MsgBox("Invalid Password!", vbCritical, "Error")
            If sType <> "Settings" AndAlso sType <> "CancelOrder" Then ispriceApproved = 2
        End If
    End Sub

    ' Settings and cancel passwords are not price approvals, so they keep this state.
    Private Sub RecordPriceApproval()
        ispriceApproved = 1
        approvalPassword = txtPass.Text
    End Sub

    Private Sub frmPassword_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtPass.Text = String.Empty
        txtPass.Focus()
        Me.AcceptButton = cmdOK
    End Sub

    Private Sub txtPass_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtPass.KeyDown
        If e.KeyCode = Keys.Enter Then
            ValidateInput()
        End If
    End Sub

    Private Sub frmPassword_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        txtPass.Text = String.Empty
        txtPass.Focus()
    End Sub
End Class