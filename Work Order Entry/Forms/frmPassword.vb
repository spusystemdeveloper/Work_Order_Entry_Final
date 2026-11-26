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
            ' Stay open so the user can retype; Cancel still ends the dialog.
            If sType <> "Settings" AndAlso sType <> "CancelOrder" Then ispriceApproved = 2
            lblError.Visible = True
            txtPass.SelectAll()
            txtPass.Focus()
        End If
    End Sub

    ' Say what the password is for, since one dialog serves several actions.
    Private Sub ApplyPurpose()
        Dim title As String
        Dim purpose As String
        Select Case sType
            Case "Price"
                title = "Price Level Password"
                purpose = "Enter the manager password to change the price level."
            Case "EditPrice"
                title = "Price Approval Password"
                purpose = "Enter the manager password to approve the prices on this order."
            Case "Settings"
                title = "Settings Password"
                purpose = "Enter the password to open Settings."
            Case "CancelOrder"
                title = "Cancel Order Password"
                purpose = "Enter the password to cancel this order."
            Case Else
                title = "Password Required"
                purpose = "Enter the password to continue."
        End Select
        Me.Text = title
        lblHeading.Text = title
        lblPurpose.Text = purpose
        lblError.Visible = False
    End Sub

    ' Settings and cancel passwords are not price approvals, so they keep this state.
    Private Sub RecordPriceApproval()
        ispriceApproved = 1
        approvalPassword = txtPass.Text
    End Sub

    Private Sub frmPassword_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ApplyPurpose()
        txtPass.Text = String.Empty
        txtPass.Focus()
        Me.AcceptButton = cmdOK
    End Sub

    Private Sub txtPass_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPass.TextChanged
        lblError.Visible = False
    End Sub

    Private Sub txtPass_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtPass.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            ValidateInput()
        End If
    End Sub

    Private Sub frmPassword_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        txtPass.Text = String.Empty
        txtPass.Focus()
    End Sub
End Class