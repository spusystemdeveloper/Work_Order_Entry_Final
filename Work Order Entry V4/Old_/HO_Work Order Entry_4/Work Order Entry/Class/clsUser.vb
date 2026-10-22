Public Class clsUser

    Public Shared Function viewUser() As Object

        Try

            Dim usr = (From u In db.SOD_WO_Users
                   Select u.UserID, u.UserFullName, u.UserName, u.UserRegister).ToList

            Return usr

        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try
        

    End Function

    Public Shared Function viewUserWithPass() As Object

        Try

            Dim usr = (From u In db.SOD_WO_Users
                   Select u.UserID, u.UserFullName, u.UserName, u.UserRegister, u.UserPass).ToList

            Return usr

        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try


    End Function

    Public Shared Sub addUsers(ByVal name As String, ByVal username As String, ByVal pass As String, ByVal register As Integer)

        Try
            Dim query As New SOD_WO_User With {
                .UserFullName = name, _
                .UserName = username, _
                .UserPass = pass, _
                .UserRegister = register
                }

            db.SOD_WO_Users.InsertOnSubmit(query)

            db.SubmitChanges()

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try


    End Sub

    Public Shared Sub updateUser(ByVal userid As Integer, ByVal name As String, ByVal username As String, ByVal pass As String, ByVal register As Integer)

        Try
            Dim user = (From a In db.SOD_WO_Users Where a.UserID.Equals(userid)
                         Select a).SingleOrDefault

            user.UserFullName = name
            user.UserName = username
            user.UserPass = pass
            user.UserRegister = register

            db.SubmitChanges()

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Public Shared Sub delUser(ByVal userid As Integer)

        Try
            Dim del = (From a In db.SOD_WO_Users Where a.UserID.Equals(userid)
                         Select a).SingleOrDefault

            db.SOD_WO_Users.DeleteOnSubmit(del)

            db.SubmitChanges()

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub


    Public Shared Function verifyUsername(ByVal username As String) As Boolean

        Dim exist = (From u In db.SOD_WO_Users Where u.UserName.Equals(username)
                     Select u).Count

        If exist = 0 Then

            Return False

        Else

            Return True

        End If

    End Function

    Public Shared Function getUsernamePassword(ByVal username As String) As String

        Dim pass = (From u In db.SOD_WO_Users Where u.UserName.Equals(username)
                    Select u.UserPass).SingleOrDefault

        Return pass

    End Function

    Public Shared Sub getUserDetials(ByVal username As String)

        Try
            Dim pass = (From u In db.SOD_WO_Users Where u.UserName.Equals(username)
                   Select u).SingleOrDefault

            frmItemLookUp.usrFullname = pass.UserFullName
            frmItemLookUp.usrRegister = pass.UserRegister
            frmItemLookUp.usrID = pass.UserID

        Catch ex As Exception

            MessageBox.Show(ex.Message)
        End Try
       
    End Sub

    Public Shared Sub AddCustTypeUsers(ByVal _custypeid As Integer, ByVal _userid As String)

        Try

            Dim check = (From a In db.SOD_WO_CustTypeUsers
                         Where a.CustTypeID.Equals(_custypeid) And a.USERID.Equals(_userid)).Count

            If check = 0 Then
                Dim query As New SOD_WO_CustTypeUser With {
                .CustTypeID = _custypeid, _
                .USERID = _userid
                }


                db.SOD_WO_CustTypeUsers.InsertOnSubmit(query)

                db.SubmitChanges()
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try


    End Sub

    Public Shared Sub DelCustTypeUser(ByVal _custypeid As Integer, ByVal _userid As String)

        Try
            Dim del = (From a In db.SOD_WO_CustTypeUsers
                       Where a.CustTypeID.Equals(_custypeid) And a.USERID.Equals(_userid)
                       Select a).SingleOrDefault

            db.SOD_WO_CustTypeUsers.DeleteOnSubmit(del)

            db.SubmitChanges()

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub
End Class
