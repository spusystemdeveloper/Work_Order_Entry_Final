Imports Microsoft.Win32
Imports System.Data.SqlClient

Module ModConnectDB

    Public ErrorCount As Integer = 0

    'Public db As New ItemLookUpDataContext("Data Source=localhost;Initial Catalog=MAIN_WHSE_2015_DB;Persist Security Info=True;User ID=sa;Password=starbright24680")

    Public db As New ItemLookUpDataContext(DB_Conn("constr"))
    Public dbnew As New ItemLookUpDataContext(DB_Conn("constr"))
    Public dbInitial As New ItemLookUpDataContext(DB_ConnInitial("constr"))

    Public Function GetDB() As ItemLookUpDataContext
        Return New ItemLookUpDataContext(DB_Conn("constr"))
    End Function

    Public Function GetDBNew() As ItemLookUpDataContext
        Return New ItemLookUpDataContext(DB_Conn("constr"))
    End Function

    Public Function GetDBInitial() As ItemLookUpDataContext
        Return New ItemLookUpDataContext(DB_ConnInitial("constr"))
    End Function

    Public sConnectDB As String
    Public sAcctNum As String
    Public sTitle As String
    Public sReg As String
    Public bEmployee As Boolean
    Public bTaxExcempt As Boolean
    Public iPrice As Integer
    Public iPricePass As Integer
    Public iSalesID As Integer
    Public dTSales As Double
    Public dVSales As Double
    Public dTotalTSales As Double
    Public dTotalVSales As Double
    Public dTotalSales As Double
    Public dDisc As Double
    Public dNetPrice As Double
    Public dPrice As Double
    Public iCusID As Integer
    Public iRow As Integer
    Public iRowLevel As Integer
    Public iRowCust As Integer
    Public iRowSales As Integer
    Public sItemCode As String
    Public sItemName As String
    Public bInsert As Boolean
    Public bSuccessImport As Boolean
    Public iRefNum As Long
    Public bPass As Boolean = False
    Public bAllow As Boolean = False
    Public iStoreID As Integer
    Public dLowest As Double = 0
    Public bLevelFlag As Boolean = False
    Public custType As String = String.Empty

    Public WoRecallType As String = 0
    Public ispriceApproved As Integer = 0

    Public dbs As String
    Public dbn As String
    Public dbu As String
    Public dbp As String

    Public Function DB_Conn(ByVal i As String) As String

        If i = "constr" Then
            Return "Data Source=" + dbs + ";Initial Catalog=" + dbn + ";Persist Security Info=True;User ID=" + dbu + ";Password=" + dbp
        ElseIf i = "dbn" Then
            Return dbn
        ElseIf i = "svr" Then
            Return dbs
        Else
            Return String.Empty
        End If

    End Function

    Public Function DB_ConnInitial(ByVal i As String) As String

        Dim regKey As RegistryKey
        Dim val As String


        regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\SHOTINC\RMS\STORE")
        val = regKey.GetValue("Online").ToString

        dbs = String.Empty
        dbu = String.Empty
        dbp = String.Empty

        For Each x In Split(val, ";")
            If Split(x, "=")(0) = "Data Source" Then
                dbs = Split(x, "=")(1)
            End If
            If Split(x, "=")(0) = "Initial Catalog" Then
                dbn = Split(x, "=")(1)
            End If
            If Split(x, "=")(0) = "User ID" Then
                dbu = Split(x, "=")(1)
            End If
            If Split(x, "=")(0) = "Password" Then
                dbp = Split(x, "=")(1)
            End If
        Next

        If i = "constr" Then
            Return "Data Source=" + dbs + ";Initial Catalog=" + dbn + ";Persist Security Info=True;User ID=" + dbu + ";Password=" + dbp
        ElseIf i = "dbn" Then
            Return dbn
        ElseIf i = "svr" Then
            Return dbs
        Else
            Return String.Empty
        End If

        regKey.Close()

    End Function

    Public Function rmsPath(ByVal i As String) As String

        Dim regKey As RegistryKey
        Dim Path As String = ""

        regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Retail Management System\Store Operations\Manager\Path")

        If i = "Pictures" Then
            Path = regKey.GetValue("Pictures").ToString
        End If

        Return Path

        regKey.Close()

    End Function

   Public sql_con As SqlConnection
    Public sql_con2 As SqlConnection
    Public sql_cmd As SqlCommand
    Public sql_da As SqlDataAdapter
    Public sql_ds As DataSet = New DataSet()
    Public sql_dt As DataTable = New DataTable()

    Public database_location As String = DB_Conn("constr")

    Public Sub DatabaseConnection()
        Try
            sql_con = New SQLConnection(database_location)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    'QUERY SQL
    Public Sub query(ByVal sql_query As String)
        Try
            DatabaseConnection()
            sql_con.Open()
            sql_cmd = sql_con.CreateCommand()
            sql_cmd.CommandText = sql_query
            sql_cmd.ExecuteNonQuery()
            '  MessageBox.Show("SUCCESS !", "", MessageBoxButtons.OK, MessageBoxIcon.Information)
            sql_con.Close()
        Catch ex As Exception

            MessageBox.Show(ex.Message)
        End Try
    End Sub

    'THIS METHOD IS FOR RETRIEVING DATA IN THE DATABASE
    Public Function load_data(ByVal sql As String) As DataTable

        Try
            sql_dt = New DataTable()
            DatabaseConnection()
            sql_con.Open()
            sql_cmd = sql_con.CreateCommand()
            sql_da = New SqlDataAdapter(sql, sql_con)
            sql_da.Fill(sql_dt)

            Return sql_dt

        Catch ex As Exception

            MessageBox.Show(ex.Message + " Please Check Database !")
            Application.Exit()
            Return Nothing

        End Try

        sql_con.Close()

    End Function



End Module
