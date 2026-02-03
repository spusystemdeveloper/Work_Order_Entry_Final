Imports MySql.Data.MySqlClient

Module MySQL_DAL

    Public MySQL_user As String = My.Settings.MySQL_user
    Public MySQL_pass As String = My.Settings.MySQL_pass
    Public MySQL_server As String = My.Settings.MySQL_svr
    Public MySQL_database As String = My.Settings.MySQL_db
    Public MySQL_port As Integer = My.Settings.MySQL_port

    Dim mysql_database_connection As String = "server='" & MySQL_server & "';user id='" & MySQL_user & "';password='" & MySQL_pass & "';persistsecurityinfo=True;database='" &
                                        MySQL_database & "';port='" & MySQL_port & "';Convert Zero Datetime=True;Connection Timeout=60"

    Dim mysql_con As MySqlConnection
    Dim mysql_cmd As MySqlCommand
    Dim mysql_da As MySqlDataAdapter
    Dim mysql_dr As MySqlDataReader
    Dim mysql_ds As DataSet = New DataSet()
    Dim mysql_dt As DataTable = New DataTable()

    Public Function mysql_testConnection() As Boolean
        Dim _mysql_con As MySqlConnection = New MySqlConnection(mysql_database_connection)
        Try
            _mysql_con.Open()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Sub mysql_DatabaseConnection()
        Try
            mysql_con = New MySqlConnection(mysql_database_connection)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Function mysql_query(ByVal sql_query As String) As Boolean
        Try
            mysql_DatabaseConnection()
            mysql_con.Open()
            mysql_cmd = mysql_con.CreateCommand()
            mysql_cmd.CommandText = sql_query
            mysql_cmd.ExecuteNonQuery()
            mysql_con.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function mysql_load_data(ByVal sql As String) As DataTable
        Try
            mysql_dt = New DataTable()
            mysql_DatabaseConnection()
            mysql_con.Open()
            mysql_cmd = mysql_con.CreateCommand()
            mysql_da = New MySqlDataAdapter(sql, mysql_con)
            mysql_da.Fill(mysql_dt)
            mysql_con.Close()
            Return mysql_dt
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function mysql_getData(ByVal sql_query As String, ByVal colname As String) As String
        Try
            Dim result = Nothing
            mysql_DatabaseConnection()
            mysql_con.Open()
            mysql_cmd = mysql_con.CreateCommand()
            mysql_cmd.CommandText = sql_query
            mysql_dr = mysql_cmd.ExecuteReader()
            If mysql_dr.HasRows Then
                While mysql_dr.Read()
                    result = mysql_dr.Item(colname)
                End While
            End If
            mysql_dr.Close()
            mysql_con.Close()
            Return result
        Catch ex As Exception
            Return Nothing
            MessageBox.Show(ex.Message)
        End Try
    End Function

End Module

