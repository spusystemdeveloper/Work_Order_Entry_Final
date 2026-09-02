Imports Microsoft.Win32
Module ModConnectDB

    'Public db As New ItemLookUpDataContext("Data Source=localhost;Initial Catalog=MAIN_WHSE_2015_DB;Persist Security Info=True;User ID=sa;Password=starbright24680")
    Public db As New DatabaseDataContext(DB_Conn("constr"))
    Public db1 As New DatabaseDataContext(DB_Conn("constr"))

    Public ErrorCount As Integer = 0

    Public sConnectDB As String
    Public sAcctNum As String
    Public sTitle As String
    Public sReg As String
    Public bEmployee As Boolean
    Public bTaxExcempt As Boolean
    Public iPrice As Integer
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
    Public dLowest As Double
    Public bLevelFlag As Boolean = False


    Public DB_Server As New String(DB_Conn("svr"))
    Public DB_Name As New String(DB_Conn("dbn"))
    Public DB_RegNo As New String(DB_Conn("regno"))

    Public Function DB_Conn(ByVal i As String) As String

        Dim regKey As RegistryKey
        Dim regNoKey As RegistryKey
        Dim val As String
        Dim dbs As String
        Dim dbn As String
        Dim dbu As String
        Dim dbp As String
        Dim regno As String

        regNoKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\WOW6432Node\Microsoft\Retail Management System\Store Operations\POSUser\Register")
        regno = regNoKey.GetValue("Number").ToString

        regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\WOW6432Node\SHOTINC\RMS\STORE")
        val = regKey.GetValue("Online").ToString

        dbs = String.Empty
        dbn = String.Empty
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
        ElseIf i = "regno" Then
            Return regno
        Else
            Return String.Empty
        End If

        regKey.Close()

    End Function

End Module
