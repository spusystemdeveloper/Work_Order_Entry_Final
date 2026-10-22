Public Class frmLogin

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim exist = clsUser.verifyUsername(txtUsername.Text)

        If exist = False Then
            MessageBox.Show("Username not Registered!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Else

            If txtPassword.Text = clsUser.getUsernamePassword(txtUsername.Text) Then

                Label3.Text = "Loading Items .. Please Wait"

                clsUser.getUserDetials(txtUsername.Text)
                frmItemLookUp.ToolStripStatusLabel3.Text = "Register : " & frmItemLookUp.usrRegister & ""
                frmItemLookUp.ToolStripStatusLabel2.Text = "User : " & frmItemLookUp.usrFullname & ""
                frmItemLookUp.usrUsername = txtUsername.Text
                txtUsername.Text = ""
                txtPassword.Text = ""
                Timer1.Start()


            Else
                MessageBox.Show("Please Check your Credentials!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        End If

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
      
        Application.Exit()

    End Sub

    Private Sub frmLogin_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ActiveControl = txtUsername
    End Sub

    Private Sub txtUsername_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtUsername.KeyPress
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
            Button1_Click(sender, e)
        End If
    End Sub

    Private Sub txtPassword_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPassword.KeyPress
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
            Button1_Click(sender, e)
        End If
    End Sub

    
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        '  frmItemLookUp.loadData()
        ' frmItemLookUp.Update_Timer.Enabled = True
        Label3.Text = "Login"
        Me.Close()
    End Sub
End Class