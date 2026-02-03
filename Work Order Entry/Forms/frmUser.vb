Public Class frmUser

    Private Sub frmUser_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gridUser.DataSource = clsUser.viewUser()
    End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click

        Dim exist = clsUser.verifyUsername(txtUsr.Text)

        If exist = False Then

            If txtFn.Text = String.Empty Or txtUsr.Text = String.Empty Or txtPass.Text = String.Empty Or txtRegister.Text = String.Empty Then
                MessageBox.Show("Please Fillup all the Field!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                clsUser.addUsers(txtFn.Text, txtUsr.Text, txtPass.Text, txtRegister.Text)
                gridUser.DataSource = clsUser.viewUser()
                clearFields()
            End If
        Else
            MessageBox.Show("Username Already Taken!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If

      
        
    End Sub

    Private Sub btnEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEdit.Click
      
            If txtFn.Text = String.Empty Or txtUsr.Text = String.Empty Or txtPass.Text = String.Empty Or txtRegister.Text = String.Empty Then
                MessageBox.Show("Please Fillup all the Field!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                clsUser.updateUser(Convert.ToInt64(txtID.Text), txtFn.Text, txtUsr.Text, txtPass.Text, txtRegister.Text)
                gridUser.DataSource = clsUser.viewUser()
                clearFields()
            End If


    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        If txtID.Text = "----" Then
            MessageBox.Show("Please Select a User !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Else
            clsUser.delUser(txtID.Text)
            gridUser.DataSource = clsUser.viewUser()
            clearFields()
        End If
       
    End Sub

    Private Sub gridUser_CellMouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles gridUser.CellMouseDoubleClick

        Try
            Dim index As Integer

            index = e.RowIndex

            Dim selectedRow As DataGridViewRow
            selectedRow = gridUser.Rows(index)

            txtID.Text = selectedRow.Cells(0).Value.ToString()
            txtFn.Text = selectedRow.Cells(1).Value.ToString()
            txtUsr.Text = selectedRow.Cells(2).Value.ToString()
            txtRegister.Text = selectedRow.Cells(3).Value.ToString()
        Catch ex As Exception

        End Try


    End Sub

    Private Sub clearFields()
        txtID.Text = "----"
        txtFn.Text = ""
        txtUsr.Text = ""
        txtPass.Text = ""
        txtRegister.Text = ""
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        clearFields()
    End Sub

    Private Sub chkBox_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBox.CheckedChanged
        If chkBox.Checked = True Then
            gridUser.DataSource = clsUser.viewUserWithPass()
        Else
            gridUser.DataSource = clsUser.viewUser()
        End If
    End Sub
End Class