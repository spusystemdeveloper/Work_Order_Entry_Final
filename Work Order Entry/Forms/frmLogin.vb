'Public Class frmLogin
'    Public StoreDb As String = "DVO_STORE_DB"
'    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

'        Dim exist = clsUser.verifyUsername(txtUsername.Text)
'        'ModConnectDB.dbn = cmbDatabases.Text
'        ModConnectDB.dbn = StoreDb
'        db = New ItemLookUpDataContext(DB_Conn("constr"))
'        dbnew = New ItemLookUpDataContext(DB_Conn("constr"))
'        database_location = DB_Conn("constr")

'        Me.Hide()
'        frmItemLookUp.Show()

'        If exist = False Then
'            MessageBox.Show("Usernameee not Registered!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
'        Else

'            If txtPassword.Text = clsUser.getUsernamePassword(txtUsername.Text) Then

'                Label3.Text = "Loading Items .. Please Wait"

'                clsUser.getUserDetials(txtUsername.Text)
'                frmItemLookUp.ToolStripStatusLabel3.Text = "Register : " & frmItemLookUp.usrRegister & ""
'                frmItemLookUp.ToolStripStatusLabel2.Text = "User : " & frmItemLookUp.usrFullname & ""
'                frmItemLookUp.usrUsername = txtUsername.Text
'                txtUsername.Text = ""
'                txtPassword.Text = ""
'                Timer1.Start()


'            Else
'                MessageBox.Show("Please Check your Credentials!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
'            End If

'        End If

'    End Sub

'    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

'        Application.Exit()

'    End Sub

'    Private Sub frmLogin_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
'        Me.ActiveControl = txtUsername
'        ModConnectDB.dbn = StoreDb
'    End Sub

'    Private Sub txtUsername_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtUsername.KeyPress
'        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
'            Button1_Click(sender, e)
'        End If
'    End Sub

'    Private Sub txtPassword_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPassword.KeyPress
'        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
'            Button1_Click(sender, e)
'        End If
'    End Sub


'    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
'        Timer1.Stop()
'        '  frmItemLookUp.loadData()
'        ' frmItemLookUp.Update_Timer.Enabled = True
'        Label3.Text = "Loginssss"
'        Me.Close()
'    End Sub

'    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

'    End Sub
'End Class
Imports Microsoft.Win32
Imports System.Diagnostics

Public Class frmLogin

    ' Auto-selected store database (same as frmStoreSetup)
    Public StoreDb As String = "DVO_STORE_DB"

    Private Sub frmLogin_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ActiveControl = txtUsername

        Try
            db = New ItemLookUpDataContext(DB_Conn("constr"))
            dbnew = New ItemLookUpDataContext(DB_Conn("constr"))
            database_location = DB_Conn("constr")
        Catch ex As Exception
            MessageBox.Show("Cannot connect to database!" & vbCrLf & ex.Message,
                            "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Try
            Dim connectionString = DB_Conn("constr")

            ' Creating a LINQ-to-SQL context does not open the connection.  Test it
            ' before running any authentication query so connection failures stay
            ' inside this error boundary.
            Using connectionTest As New ItemLookUpDataContext(connectionString)
                connectionTest.Connection.Open()
                connectionTest.Connection.Close()
            End Using

            db = New ItemLookUpDataContext(connectionString)
            dbnew = New ItemLookUpDataContext(connectionString)
            database_location = connectionString

            If Not clsUser.verifyUsername(txtUsername.Text) Then
                MessageBox.Show("Username not Registered!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            If txtPassword.Text <> clsUser.getUsernamePassword(txtUsername.Text) Then
                MessageBox.Show("Please Check your Credentials!!!!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            Label3.Text = "Loading Items .. Please Wait"
            clsUser.getUserDetials(txtUsername.Text)

            Dim userRegister As Integer = CInt(frmItemLookUp.usrRegister)
            If Not ValidateRegisterRules(userRegister) Then
                Label3.Text = "Login"
                Exit Sub
            End If

            frmItemLookUp.ToolStripStatusLabel3.Text = "Register : " & frmItemLookUp.usrRegister
            frmItemLookUp.ToolStripStatusLabel2.Text = "User : " & frmItemLookUp.usrFullname
            frmItemLookUp.usrUsername = txtUsername.Text

            txtUsername.Text = ""
            txtPassword.Text = ""

            frmItemLookUp.Show()
            Timer1.Start()
        Catch ex As Exception
            MessageBox.Show("Database connection failed!" & vbCrLf & ex.Message,
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Label3.Text = "Login"
        End Try

    End Sub

    ''' <summary>
    ''' Validates that the user's assigned register matches the RMS registry register number.
    ''' - If SOMANAGER, SOPOSUSER, or Inventory Transfer Manager are running and the register
    '''   does NOT match, login is blocked (no auto-switch).
    ''' - If none of those apps are running and the register does NOT match, user can choose
    '''   to update the registry to the user's register.
    ''' </summary>
    Private Function ValidateRegisterRules(ByVal userRegister As Integer) As Boolean

        ' Detect RMS-related apps running
        Dim isManagerRunning As Boolean = Process.GetProcessesByName("SOMANAGER").Length > 0
        Dim isPosRunning As Boolean = Process.GetProcessesByName("SOPOSUSER").Length > 0
        Dim isInventoryTransferManager As Boolean = Process.GetProcessesByName("Inventory Transfer Manager").Length > 0

        ' Try both with and without spaces, depending on how the process is named
        'Dim isInvTransferRunning As Boolean =
        '    Process.GetProcessesByName("InventoryTransferManager").Length > 0 OrElse
        '    Process.GetProcessesByName("Inventory Transfer Manager").Length > 0

        Dim isRmsRunning As Boolean = (isManagerRunning OrElse isPosRunning OrElse isInventoryTransferManager)

        ' Current PC register from RMS registry
        Dim rmsRegisterNumberNullable As Integer?
        Try
            rmsRegisterNumberNullable = ReadRmsRegisterNumber()
        Catch ex As Exception
            MessageBox.Show(
                "Cannot read RMS Register Number from registry." & vbCrLf & ex.Message,
                "Register Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
            Return False
        End Try

        If Not rmsRegisterNumberNullable.HasValue Then
            MessageBox.Show(
                "Cannot read RMS Register Number from registry." & vbCrLf &
                "Missing key/value: HKLM\SOFTWARE\WOW6432Node\Microsoft\Retail Management System\Store Operations\POSUser\Register\Number",
                "Register Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
            Return False
        End If

        Dim rmsRegisterNumber As Integer = rmsRegisterNumberNullable.Value

        ' RULE: If RMS apps are running
        If isRmsRunning Then
            If rmsRegisterNumber = userRegister Then
                ' RMS apps running and register matches -> allow login
                Return True
            End If

            ' RMS apps running and register does NOT match -> block (no auto-switch)
            Dim messageText As String
            If isInventoryTransferManager Then
                ' Special message when Inventory Transfer Manager is running
                messageText = "Login blocked because Inventory Transfer Manager is currently running." & vbCrLf & vbCrLf &
                    "This computer is Register " & rmsRegisterNumber & "." & vbCrLf &
                    "Your account is assigned to Register " & userRegister & "." & vbCrLf & vbCrLf &
                    "Please close Inventory Transfer Manager or use the correct register PC for this account."
            Else
                ' Default message for other RMS apps
                messageText = "Login blocked because RMS applications are currently running." & vbCrLf & vbCrLf &
                    "This computer is Register " & rmsRegisterNumber & "." & vbCrLf &
                    "Your account is assigned to Register " & userRegister & "." & vbCrLf & vbCrLf &
                    "Please close or use the correct register PC for this account."
            End If

            MessageBox.Show(
                messageText,
                "Register Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return False
        End If

        ' RULE: RMS apps are NOT running -> allow switching if mismatch, with confirmation
        If rmsRegisterNumber <> userRegister Then

            Dim result As DialogResult = MessageBox.Show(
                "This computer is Register " & rmsRegisterNumber &
                ", but your user is assigned to Register " & userRegister & "." & vbCrLf & vbCrLf &
                "Click OK to update the RMS registry Register Number to " & userRegister & " and continue login.",
                "Register Validation",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation
            )

            If result <> DialogResult.OK Then Return False

            Try
                WriteRmsRegisterNumber(userRegister)
            Catch ex As Exception
                MessageBox.Show(
                    "Failed to update RMS Register Number in registry." & vbCrLf &
                    "Please run this application as Administrator (UAC prompt) and try again." & vbCrLf & vbCrLf &
                    ex.Message,
                    "Register Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
                Return False
            End Try
        End If

        Return True
    End Function
    Private Shared Function ReadRmsRegisterNumber() As Integer?
        ' RMS stores the Register number in the 32-bit view (WOW6432Node on 64-bit Windows).
        Const subKeyPath As String = "SOFTWARE\Microsoft\Retail Management System\Store Operations\POSUser\Register"
        Const valueName As String = "Number"

        Using baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            Using key = baseKey.OpenSubKey(subKeyPath, writable:=False)
                If key Is Nothing Then Return Nothing

                Dim raw = key.GetValue(valueName, Nothing)
                If raw Is Nothing Then Return Nothing

                Dim parsed As Integer
                If Integer.TryParse(raw.ToString(), parsed) Then
                    Return parsed
                End If

                Return Nothing
            End Using
        End Using
    End Function

    Private Shared Sub WriteRmsRegisterNumber(ByVal registerNumber As Integer)
        ' Writes to the 32-bit view so it maps to WOW6432Node on 64-bit Windows.
        Const subKeyPath As String = "SOFTWARE\Microsoft\Retail Management System\Store Operations\POSUser\Register"
        Const valueName As String = "Number"

        Using baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            Using key = baseKey.OpenSubKey(subKeyPath, writable:=True)
                If key Is Nothing Then
                    Throw New InvalidOperationException("Registry key not found: HKLM\SOFTWARE\WOW6432Node\Microsoft\Retail Management System\Store Operations\POSUser\Register")
                End If

                ' Store as REG_SZ (string), as requested
                key.SetValue(valueName, registerNumber.ToString(), RegistryValueKind.String)
            End Using
        End Using
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Application.Exit()
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
        Label3.Text = "Login"
        Me.Close()
    End Sub

End Class
