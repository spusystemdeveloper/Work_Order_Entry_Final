<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSettings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.cmbLevel = New System.Windows.Forms.ComboBox()
        Me.txtLimit = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtConfirmP = New System.Windows.Forms.TextBox()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.cmdPass3 = New System.Windows.Forms.Button()
        Me.txtPassword3 = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.txtConfirmP3 = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.cmdPass2 = New System.Windows.Forms.Button()
        Me.cmdPass = New System.Windows.Forms.Button()
        Me.txtPassword2 = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txtConfirmP2 = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmbPricePass = New System.Windows.Forms.ComboBox()
        Me.cmbPriceBound = New System.Windows.Forms.ComboBox()
        Me.cmdNew = New System.Windows.Forms.Button()
        Me.cmdDel = New System.Windows.Forms.Button()
        Me.cmdAdd = New System.Windows.Forms.Button()
        Me.txtCustType = New System.Windows.Forms.TextBox()
        Me.gridCustType = New System.Windows.Forms.DataGridView()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.cmdSaveBranchSettings = New System.Windows.Forms.Button()
        Me.chkAllowBranchSelection = New System.Windows.Forms.CheckBox()
        Me.chkLockCustomerSalesRepOnRecall = New System.Windows.Forms.CheckBox()
        Me.chkLockSalesRepToCustomer = New System.Windows.Forms.CheckBox()
        Me.lblAssignedDatabase = New System.Windows.Forms.Label()
        Me.txtExpectedDatabase = New System.Windows.Forms.TextBox()
        Me.chkShowForInvoice = New System.Windows.Forms.CheckBox()
        Me.chkShowImportButton = New System.Windows.Forms.CheckBox()
        Me.chkEnableSetTaxButton = New System.Windows.Forms.CheckBox()
        Me.chkShowOrderActions = New System.Windows.Forms.CheckBox()
        Me.chkShowOrderNotifications = New System.Windows.Forms.CheckBox()
        Me.chkAllowDuplicateItemEntry = New System.Windows.Forms.CheckBox()
        Me.chkAllowOrderGrouping = New System.Windows.Forms.CheckBox()
        Me.chkEnableBranchQueue = New System.Windows.Forms.CheckBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.txtWebCust = New System.Windows.Forms.TextBox()
        Me.txtWebShip = New System.Windows.Forms.TextBox()
        Me.txtWebRep = New System.Windows.Forms.TextBox()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.txtMySQLuser = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.txtMySQLdb = New System.Windows.Forms.TextBox()
        Me.txtMySQLpass = New System.Windows.Forms.TextBox()
        Me.txtMySQLsvr = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.gridCustType, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.cmdOK)
        Me.GroupBox1.Controls.Add(Me.cmbLevel)
        Me.GroupBox1.Controls.Add(Me.txtLimit)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(440, 124)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Default Settings"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label4.Location = New System.Drawing.Point(16, 63)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(115, 16)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Price Level Bound:"
        '
        'cmdOK
        '
        Me.cmdOK.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdOK.Location = New System.Drawing.Point(150, 86)
        Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdOK.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdOK.FlatAppearance.BorderSize = 1
        Me.cmdOK.BackColor = System.Drawing.Color.White
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(132, 28)
        Me.cmdOK.TabIndex = 1
        Me.cmdOK.Text = "Apply defaults (&O)"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'cmbLevel
        '
        Me.cmbLevel.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmbLevel.FormattingEnabled = True
        Me.cmbLevel.Items.AddRange(New Object() {"NONE", "PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)"})
        Me.cmbLevel.Location = New System.Drawing.Point(150, 60)
        Me.cmbLevel.Name = "cmbLevel"
        Me.cmbLevel.Size = New System.Drawing.Size(240, 23)
        Me.cmbLevel.TabIndex = 6
        '
        'txtLimit
        '
        Me.txtLimit.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtLimit.Location = New System.Drawing.Point(150, 26)
        Me.txtLimit.Name = "txtLimit"
        Me.txtLimit.Size = New System.Drawing.Size(132, 22)
        Me.txtLimit.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label1.Location = New System.Drawing.Point(16, 29)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(80, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Limit Entries:"
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.Button1.Location = New System.Drawing.Point(16, 26)
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button1.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.Button1.FlatAppearance.BorderSize = 1
        Me.Button1.BackColor = System.Drawing.Color.White
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(136, 30)
        Me.Button1.TabIndex = 12
        Me.Button1.Text = "Manage Users"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label3.Location = New System.Drawing.Point(206, 30)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(48, 16)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Confirm:"
        '
        'txtConfirmP
        '
        Me.txtConfirmP.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtConfirmP.Location = New System.Drawing.Point(264, 26)
        Me.txtConfirmP.Name = "txtConfirmP"
        Me.txtConfirmP.Size = New System.Drawing.Size(112, 22)
        Me.txtConfirmP.TabIndex = 4
        Me.txtConfirmP.UseSystemPasswordChar = True
        '
        'txtPassword
        '
        Me.txtPassword.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPassword.Location = New System.Drawing.Point(96, 26)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(100, 22)
        Me.txtPassword.TabIndex = 3
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label2.Location = New System.Drawing.Point(12, 30)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(80, 16)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Password 1:"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.cmdPass3)
        Me.GroupBox2.Controls.Add(Me.txtPassword3)
        Me.GroupBox2.Controls.Add(Me.Label10)
        Me.GroupBox2.Controls.Add(Me.txtConfirmP3)
        Me.GroupBox2.Controls.Add(Me.Label11)
        Me.GroupBox2.Controls.Add(Me.cmdPass2)
        Me.GroupBox2.Controls.Add(Me.cmdPass)
        Me.GroupBox2.Controls.Add(Me.txtPassword2)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.txtConfirmP2)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.txtPassword)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.txtConfirmP)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox2.Location = New System.Drawing.Point(464, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(508, 142)
        Me.GroupBox2.TabIndex = 13
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Password to Prompt"
        '
        'cmdPass3
        '
        Me.cmdPass3.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdPass3.Location = New System.Drawing.Point(388, 96)
        Me.cmdPass3.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPass3.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdPass3.FlatAppearance.BorderSize = 1
        Me.cmdPass3.BackColor = System.Drawing.Color.White
        Me.cmdPass3.Name = "cmdPass3"
        Me.cmdPass3.Size = New System.Drawing.Size(108, 28)
        Me.cmdPass3.TabIndex = 16
        Me.cmdPass3.Text = "Update 3 (&O)"
        Me.cmdPass3.UseVisualStyleBackColor = False
        '
        'txtPassword3
        '
        Me.txtPassword3.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPassword3.Location = New System.Drawing.Point(96, 98)
        Me.txtPassword3.Name = "txtPassword3"
        Me.txtPassword3.Size = New System.Drawing.Size(100, 22)
        Me.txtPassword3.TabIndex = 13
        Me.txtPassword3.UseSystemPasswordChar = True
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label10.Location = New System.Drawing.Point(12, 102)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(80, 16)
        Me.Label10.TabIndex = 12
        Me.Label10.Text = "Password 3:"
        '
        'txtConfirmP3
        '
        Me.txtConfirmP3.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtConfirmP3.Location = New System.Drawing.Point(264, 98)
        Me.txtConfirmP3.Name = "txtConfirmP3"
        Me.txtConfirmP3.Size = New System.Drawing.Size(112, 22)
        Me.txtConfirmP3.TabIndex = 14
        Me.txtConfirmP3.UseSystemPasswordChar = True
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label11.Location = New System.Drawing.Point(206, 102)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(48, 16)
        Me.Label11.TabIndex = 15
        Me.Label11.Text = "Confirm:"
        '
        'cmdPass2
        '
        Me.cmdPass2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdPass2.Location = New System.Drawing.Point(388, 60)
        Me.cmdPass2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPass2.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdPass2.FlatAppearance.BorderSize = 1
        Me.cmdPass2.BackColor = System.Drawing.Color.White
        Me.cmdPass2.Name = "cmdPass2"
        Me.cmdPass2.Size = New System.Drawing.Size(108, 28)
        Me.cmdPass2.TabIndex = 11
        Me.cmdPass2.Text = "Update 2 (&O)"
        Me.cmdPass2.UseVisualStyleBackColor = False
        '
        'cmdPass
        '
        Me.cmdPass.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdPass.Location = New System.Drawing.Point(388, 24)
        Me.cmdPass.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPass.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdPass.FlatAppearance.BorderSize = 1
        Me.cmdPass.BackColor = System.Drawing.Color.White
        Me.cmdPass.Name = "cmdPass"
        Me.cmdPass.Size = New System.Drawing.Size(108, 28)
        Me.cmdPass.TabIndex = 11
        Me.cmdPass.Text = "Update 1 (&O)"
        Me.cmdPass.UseVisualStyleBackColor = False
        '
        'txtPassword2
        '
        Me.txtPassword2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtPassword2.Location = New System.Drawing.Point(96, 62)
        Me.txtPassword2.Name = "txtPassword2"
        Me.txtPassword2.Size = New System.Drawing.Size(100, 22)
        Me.txtPassword2.TabIndex = 7
        Me.txtPassword2.UseSystemPasswordChar = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label6.Location = New System.Drawing.Point(12, 66)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(80, 16)
        Me.Label6.TabIndex = 6
        Me.Label6.Text = "Password 2:"
        '
        'txtConfirmP2
        '
        Me.txtConfirmP2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtConfirmP2.Location = New System.Drawing.Point(264, 62)
        Me.txtConfirmP2.Name = "txtConfirmP2"
        Me.txtConfirmP2.Size = New System.Drawing.Size(112, 22)
        Me.txtConfirmP2.TabIndex = 8
        Me.txtConfirmP2.UseSystemPasswordChar = True
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label7.Location = New System.Drawing.Point(206, 66)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(48, 16)
        Me.Label7.TabIndex = 9
        Me.Label7.Text = "Confirm:"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.Label9)
        Me.GroupBox3.Controls.Add(Me.Label8)
        Me.GroupBox3.Controls.Add(Me.Label5)
        Me.GroupBox3.Controls.Add(Me.cmbPricePass)
        Me.GroupBox3.Controls.Add(Me.cmbPriceBound)
        Me.GroupBox3.Controls.Add(Me.cmdNew)
        Me.GroupBox3.Controls.Add(Me.cmdDel)
        Me.GroupBox3.Controls.Add(Me.cmdAdd)
        Me.GroupBox3.Controls.Add(Me.txtCustType)
        Me.GroupBox3.Controls.Add(Me.gridCustType)
        Me.GroupBox3.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox3.Location = New System.Drawing.Point(12, 148)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(440, 252)
        Me.GroupBox3.TabIndex = 14
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Customer Type Price Level "
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label9.Location = New System.Drawing.Point(268, 28)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(156, 16)
        Me.Label9.TabIndex = 12
        Me.Label9.Text = "Password on Price Below:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label8.Location = New System.Drawing.Point(140, 28)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(100, 16)
        Me.Label8.TabIndex = 11
        Me.Label8.Text = "Price Bound:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label5.Location = New System.Drawing.Point(16, 28)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(110, 16)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "Type:"
        '
        'cmbPricePass
        '
        Me.cmbPricePass.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmbPricePass.FormattingEnabled = True
        Me.cmbPricePass.Items.AddRange(New Object() {"PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)", "COST"})
        Me.cmbPricePass.Location = New System.Drawing.Point(268, 50)
        Me.cmbPricePass.Name = "cmbPricePass"
        Me.cmbPricePass.Size = New System.Drawing.Size(156, 23)
        Me.cmbPricePass.TabIndex = 9
        '
        'cmbPriceBound
        '
        Me.cmbPriceBound.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmbPriceBound.FormattingEnabled = True
        Me.cmbPriceBound.Items.AddRange(New Object() {"PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)", "COST"})
        Me.cmbPriceBound.Location = New System.Drawing.Point(140, 50)
        Me.cmbPriceBound.Name = "cmbPriceBound"
        Me.cmbPriceBound.Size = New System.Drawing.Size(120, 23)
        Me.cmbPriceBound.TabIndex = 8
        '
        'cmdNew
        '
        Me.cmdNew.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdNew.Location = New System.Drawing.Point(16, 210)
        Me.cmdNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdNew.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdNew.FlatAppearance.BorderSize = 1
        Me.cmdNew.BackColor = System.Drawing.Color.White
        Me.cmdNew.Name = "cmdNew"
        Me.cmdNew.Size = New System.Drawing.Size(84, 28)
        Me.cmdNew.TabIndex = 4
        Me.cmdNew.Text = "New"
        Me.cmdNew.UseVisualStyleBackColor = False
        '
        'cmdDel
        '
        Me.cmdDel.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdDel.Location = New System.Drawing.Point(200, 210)
        Me.cmdDel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdDel.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdDel.FlatAppearance.BorderSize = 1
        Me.cmdDel.BackColor = System.Drawing.Color.White
        Me.cmdDel.Name = "cmdDel"
        Me.cmdDel.Size = New System.Drawing.Size(84, 28)
        Me.cmdDel.TabIndex = 3
        Me.cmdDel.Text = "Del"
        Me.cmdDel.UseVisualStyleBackColor = False
        '
        'cmdAdd
        '
        Me.cmdAdd.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdAdd.Location = New System.Drawing.Point(108, 210)
        Me.cmdAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdAdd.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdAdd.FlatAppearance.BorderSize = 1
        Me.cmdAdd.BackColor = System.Drawing.Color.White
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(84, 28)
        Me.cmdAdd.TabIndex = 2
        Me.cmdAdd.Text = "Add"
        Me.cmdAdd.UseVisualStyleBackColor = False
        '
        'txtCustType
        '
        Me.txtCustType.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtCustType.Location = New System.Drawing.Point(16, 50)
        Me.txtCustType.Name = "txtCustType"
        Me.txtCustType.Size = New System.Drawing.Size(116, 23)
        Me.txtCustType.TabIndex = 1
        '
        'gridCustType
        '
        Me.gridCustType.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridCustType.Location = New System.Drawing.Point(16, 86)
        Me.gridCustType.Name = "gridCustType"
        Me.gridCustType.EnableHeadersVisualStyles = False
        Me.gridCustType.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Control
        Me.gridCustType.ColumnHeadersDefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.WindowText
        Me.gridCustType.Size = New System.Drawing.Size(408, 112)
        Me.gridCustType.TabIndex = 0
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.chkAllowBranchSelection)
        Me.GroupBox4.Controls.Add(Me.chkLockCustomerSalesRepOnRecall)
        Me.GroupBox4.Controls.Add(Me.chkLockSalesRepToCustomer)
        Me.GroupBox4.Controls.Add(Me.lblAssignedDatabase)
        Me.GroupBox4.Controls.Add(Me.txtExpectedDatabase)
        Me.GroupBox4.Controls.Add(Me.Button1)
        Me.GroupBox4.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox4.Location = New System.Drawing.Point(12, 412)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(440, 236)
        Me.GroupBox4.TabIndex = 15
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Branch Configuration"
        '
        'cmdSaveBranchSettings
        '
        Me.cmdSaveBranchSettings.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.cmdSaveBranchSettings.Location = New System.Drawing.Point(248, 210)
        Me.cmdSaveBranchSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdSaveBranchSettings.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.cmdSaveBranchSettings.FlatAppearance.BorderSize = 1
        Me.cmdSaveBranchSettings.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.cmdSaveBranchSettings.ForeColor = System.Drawing.Color.White
        Me.cmdSaveBranchSettings.Name = "cmdSaveBranchSettings"
        Me.cmdSaveBranchSettings.Size = New System.Drawing.Size(244, 32)
        Me.cmdSaveBranchSettings.TabIndex = 14
        Me.cmdSaveBranchSettings.Text = "Save branch / order options"
        Me.cmdSaveBranchSettings.UseVisualStyleBackColor = False
        '
        'chkAllowBranchSelection
        '
        Me.chkAllowBranchSelection.AutoSize = True
        Me.chkAllowBranchSelection.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkAllowBranchSelection.Location = New System.Drawing.Point(16, 70)
        Me.chkAllowBranchSelection.Name = "chkAllowBranchSelection"
        Me.chkAllowBranchSelection.Size = New System.Drawing.Size(220, 22)
        Me.chkAllowBranchSelection.TabIndex = 18
        Me.chkAllowBranchSelection.Text = "Allow Branch Selection"
        Me.chkAllowBranchSelection.UseVisualStyleBackColor = True
        '
        'chkLockCustomerSalesRepOnRecall
        '
        Me.chkLockCustomerSalesRepOnRecall.AutoSize = True
        Me.chkLockCustomerSalesRepOnRecall.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkLockCustomerSalesRepOnRecall.Location = New System.Drawing.Point(16, 162)
        Me.chkLockCustomerSalesRepOnRecall.Name = "chkLockCustomerSalesRepOnRecall"
        Me.chkLockCustomerSalesRepOnRecall.Size = New System.Drawing.Size(430, 22)
        Me.chkLockCustomerSalesRepOnRecall.TabIndex = 21
        Me.chkLockCustomerSalesRepOnRecall.Text = "Lock Customer/Sales Rep on Recall"
        Me.chkLockCustomerSalesRepOnRecall.UseVisualStyleBackColor = True
        '
        'chkLockSalesRepToCustomer
        '
        Me.chkLockSalesRepToCustomer.AutoSize = True
        Me.chkLockSalesRepToCustomer.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkLockSalesRepToCustomer.Location = New System.Drawing.Point(16, 194)
        Me.chkLockSalesRepToCustomer.Name = "chkLockSalesRepToCustomer"
        Me.chkLockSalesRepToCustomer.Size = New System.Drawing.Size(436, 22)
        Me.chkLockSalesRepToCustomer.TabIndex = 22
        Me.chkLockSalesRepToCustomer.Text = "Lock Sales Rep to Customer's Rep (New Orders)"
        Me.chkLockSalesRepToCustomer.UseVisualStyleBackColor = True
        '
        'lblAssignedDatabase
        '
        Me.lblAssignedDatabase.AutoSize = True
        Me.lblAssignedDatabase.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAssignedDatabase.Location = New System.Drawing.Point(16, 102)
        Me.lblAssignedDatabase.Name = "lblAssignedDatabase"
        Me.lblAssignedDatabase.Size = New System.Drawing.Size(150, 16)
        Me.lblAssignedDatabase.TabIndex = 19
        Me.lblAssignedDatabase.Text = "Assigned Database:"
        '
        'txtExpectedDatabase
        '
        Me.txtExpectedDatabase.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtExpectedDatabase.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtExpectedDatabase.Location = New System.Drawing.Point(16, 124)
        Me.txtExpectedDatabase.Name = "txtExpectedDatabase"
        Me.txtExpectedDatabase.Size = New System.Drawing.Size(408, 22)
        Me.txtExpectedDatabase.TabIndex = 20
        '
        'chkShowForInvoice
        '
        Me.chkShowForInvoice.AutoSize = True
        Me.chkShowForInvoice.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkShowForInvoice.Location = New System.Drawing.Point(34, 114)
        Me.chkShowForInvoice.Name = "chkShowForInvoice"
        Me.chkShowForInvoice.Size = New System.Drawing.Size(250, 22)
        Me.chkShowForInvoice.TabIndex = 13
        Me.chkShowForInvoice.Text = "Show For Invoicing"
        Me.chkShowForInvoice.UseVisualStyleBackColor = True
        '
        'chkShowImportButton
        '
        Me.chkShowImportButton.AutoSize = True
        Me.chkShowImportButton.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkShowImportButton.Location = New System.Drawing.Point(248, 86)
        Me.chkShowImportButton.Name = "chkShowImportButton"
        Me.chkShowImportButton.Size = New System.Drawing.Size(250, 22)
        Me.chkShowImportButton.TabIndex = 17
        Me.chkShowImportButton.Text = "Show Import [F6]"
        Me.chkShowImportButton.UseVisualStyleBackColor = True
        '
        'chkEnableSetTaxButton
        '
        Me.chkEnableSetTaxButton.AutoSize = True
        Me.chkEnableSetTaxButton.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkEnableSetTaxButton.Location = New System.Drawing.Point(248, 114)
        Me.chkEnableSetTaxButton.Name = "chkEnableSetTaxButton"
        Me.chkEnableSetTaxButton.Size = New System.Drawing.Size(250, 22)
        Me.chkEnableSetTaxButton.TabIndex = 18
        Me.chkEnableSetTaxButton.Text = "Enable Set Tax button"
        Me.chkEnableSetTaxButton.UseVisualStyleBackColor = True
        '
        'chkShowOrderActions
        '
        Me.chkShowOrderActions.AutoSize = True
        Me.chkShowOrderActions.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkShowOrderActions.Location = New System.Drawing.Point(248, 142)
        Me.chkShowOrderActions.Name = "chkShowOrderActions"
        Me.chkShowOrderActions.Size = New System.Drawing.Size(250, 22)
        Me.chkShowOrderActions.TabIndex = 19
        Me.chkShowOrderActions.Text = "Show Order actions"
        Me.chkShowOrderActions.UseVisualStyleBackColor = True
        '
        'chkShowOrderNotifications
        '
        Me.chkShowOrderNotifications.AutoSize = True
        Me.chkShowOrderNotifications.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkShowOrderNotifications.Location = New System.Drawing.Point(248, 170)
        Me.chkShowOrderNotifications.Name = "chkShowOrderNotifications"
        Me.chkShowOrderNotifications.Size = New System.Drawing.Size(250, 22)
        Me.chkShowOrderNotifications.TabIndex = 20
        Me.chkShowOrderNotifications.Text = "Show order notifications"
        Me.chkShowOrderNotifications.UseVisualStyleBackColor = True
        '
        'chkAllowDuplicateItemEntry
        '
        Me.chkAllowDuplicateItemEntry.AutoSize = True
        Me.chkAllowDuplicateItemEntry.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkAllowDuplicateItemEntry.Location = New System.Drawing.Point(248, 58)
        Me.chkAllowDuplicateItemEntry.Name = "chkAllowDuplicateItemEntry"
        Me.chkAllowDuplicateItemEntry.Size = New System.Drawing.Size(250, 22)
        Me.chkAllowDuplicateItemEntry.TabIndex = 23
        Me.chkAllowDuplicateItemEntry.Text = "Allow duplicate items"
        Me.chkAllowDuplicateItemEntry.UseVisualStyleBackColor = True
        '
        'chkAllowOrderGrouping
        '
        Me.chkAllowOrderGrouping.AutoSize = True
        Me.chkAllowOrderGrouping.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkAllowOrderGrouping.Location = New System.Drawing.Point(34, 86)
        Me.chkAllowOrderGrouping.Name = "chkAllowOrderGrouping"
        Me.chkAllowOrderGrouping.Size = New System.Drawing.Size(250, 22)
        Me.chkAllowOrderGrouping.TabIndex = 15
        Me.chkAllowOrderGrouping.Text = "Order grouping"
        Me.chkAllowOrderGrouping.UseVisualStyleBackColor = True
        '
        'chkEnableBranchQueue
        '
        Me.chkEnableBranchQueue.AutoSize = True
        Me.chkEnableBranchQueue.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkEnableBranchQueue.Location = New System.Drawing.Point(16, 58)
        Me.chkEnableBranchQueue.Name = "chkEnableBranchQueue"
        Me.chkEnableBranchQueue.Size = New System.Drawing.Size(250, 22)
        Me.chkEnableBranchQueue.TabIndex = 16
        Me.chkEnableBranchQueue.Text = "Enable Branch Queue"
        Me.chkEnableBranchQueue.UseVisualStyleBackColor = True
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.GroupBox7)
        Me.GroupBox5.Controls.Add(Me.GroupBox6)
        Me.GroupBox5.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox5.Location = New System.Drawing.Point(464, 166)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(508, 214)
        Me.GroupBox5.TabIndex = 16
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "Website - Deprecate"
        '
        'GroupBox7
        '
        Me.GroupBox7.Controls.Add(Me.Button3)
        Me.GroupBox7.Controls.Add(Me.Label16)
        Me.GroupBox7.Controls.Add(Me.txtWebCust)
        Me.GroupBox7.Controls.Add(Me.txtWebShip)
        Me.GroupBox7.Controls.Add(Me.txtWebRep)
        Me.GroupBox7.Controls.Add(Me.Label17)
        Me.GroupBox7.Controls.Add(Me.Label18)
        Me.GroupBox7.Location = New System.Drawing.Point(12, 24)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Size = New System.Drawing.Size(226, 178)
        Me.GroupBox7.TabIndex = 22
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "Import defaults"
        '
        'Button3
        '
        Me.Button3.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.Button3.Location = New System.Drawing.Point(88, 140)
        Me.Button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button3.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.Button3.FlatAppearance.BorderSize = 1
        Me.Button3.BackColor = System.Drawing.Color.White
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(126, 28)
        Me.Button3.TabIndex = 21
        Me.Button3.Text = "Apply import (&O)"
        Me.Button3.UseVisualStyleBackColor = False
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label16.Location = New System.Drawing.Point(12, 34)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(70, 16)
        Me.Label16.TabIndex = 21
        Me.Label16.Text = "Customer :"
        '
        'txtWebCust
        '
        Me.txtWebCust.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtWebCust.Location = New System.Drawing.Point(88, 30)
        Me.txtWebCust.Name = "txtWebCust"
        Me.txtWebCust.Size = New System.Drawing.Size(126, 22)
        Me.txtWebCust.TabIndex = 22
        '
        'txtWebShip
        '
        Me.txtWebShip.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtWebShip.Location = New System.Drawing.Point(88, 98)
        Me.txtWebShip.Name = "txtWebShip"
        Me.txtWebShip.Size = New System.Drawing.Size(126, 22)
        Me.txtWebShip.TabIndex = 26
        '
        'txtWebRep
        '
        Me.txtWebRep.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtWebRep.Location = New System.Drawing.Point(88, 64)
        Me.txtWebRep.Name = "txtWebRep"
        Me.txtWebRep.Size = New System.Drawing.Size(126, 22)
        Me.txtWebRep.TabIndex = 23
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label17.Location = New System.Drawing.Point(12, 68)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(70, 16)
        Me.Label17.TabIndex = 24
        Me.Label17.Text = "Sales Rep :"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label18.Location = New System.Drawing.Point(12, 102)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(70, 16)
        Me.Label18.TabIndex = 25
        Me.Label18.Text = "Shipping :"
        '
        'GroupBox6
        '
        Me.GroupBox6.Controls.Add(Me.Label12)
        Me.GroupBox6.Controls.Add(Me.Button2)
        Me.GroupBox6.Controls.Add(Me.txtMySQLuser)
        Me.GroupBox6.Controls.Add(Me.Label13)
        Me.GroupBox6.Controls.Add(Me.Label14)
        Me.GroupBox6.Controls.Add(Me.txtMySQLdb)
        Me.GroupBox6.Controls.Add(Me.txtMySQLpass)
        Me.GroupBox6.Controls.Add(Me.txtMySQLsvr)
        Me.GroupBox6.Controls.Add(Me.Label15)
        Me.GroupBox6.Location = New System.Drawing.Point(250, 24)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(246, 178)
        Me.GroupBox6.TabIndex = 21
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "MySQL Connection"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label12.Location = New System.Drawing.Point(12, 28)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(70, 16)
        Me.Label12.TabIndex = 12
        Me.Label12.Text = "Server :"
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.Button2.Location = New System.Drawing.Point(88, 140)
        Me.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button2.FlatAppearance.BorderColor = System.Drawing.Color.SlateGray
        Me.Button2.FlatAppearance.BorderSize = 1
        Me.Button2.BackColor = System.Drawing.Color.White
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(146, 28)
        Me.Button2.TabIndex = 16
        Me.Button2.Text = "Apply connection (&O)"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'txtMySQLuser
        '
        Me.txtMySQLuser.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtMySQLuser.Location = New System.Drawing.Point(88, 84)
        Me.txtMySQLuser.Name = "txtMySQLuser"
        Me.txtMySQLuser.Size = New System.Drawing.Size(146, 22)
        Me.txtMySQLuser.TabIndex = 18
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label13.Location = New System.Drawing.Point(12, 58)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(70, 16)
        Me.Label13.TabIndex = 15
        Me.Label13.Text = "Database"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label14.Location = New System.Drawing.Point(12, 88)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(70, 16)
        Me.Label14.TabIndex = 17
        Me.Label14.Text = "Username :"
        '
        'txtMySQLdb
        '
        Me.txtMySQLdb.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtMySQLdb.Location = New System.Drawing.Point(88, 54)
        Me.txtMySQLdb.Name = "txtMySQLdb"
        Me.txtMySQLdb.Size = New System.Drawing.Size(146, 22)
        Me.txtMySQLdb.TabIndex = 14
        '
        'txtMySQLpass
        '
        Me.txtMySQLpass.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtMySQLpass.Location = New System.Drawing.Point(88, 114)
        Me.txtMySQLpass.Name = "txtMySQLpass"
        Me.txtMySQLpass.Size = New System.Drawing.Size(146, 22)
        Me.txtMySQLpass.TabIndex = 19
        '
        'txtMySQLsvr
        '
        Me.txtMySQLsvr.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtMySQLsvr.Location = New System.Drawing.Point(88, 24)
        Me.txtMySQLsvr.Name = "txtMySQLsvr"
        Me.txtMySQLsvr.Size = New System.Drawing.Size(146, 22)
        Me.txtMySQLsvr.TabIndex = 13
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.Label15.Location = New System.Drawing.Point(12, 118)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(70, 16)
        Me.Label15.TabIndex = 20
        Me.Label15.Text = "Password :"
        '
        'GroupBox8
        '
        Me.GroupBox8.Controls.Add(Me.cmdSaveBranchSettings)
        Me.GroupBox8.Controls.Add(Me.Label20)
        Me.GroupBox8.Controls.Add(Me.Label19)
        Me.GroupBox8.Controls.Add(Me.chkEnableBranchQueue)
        Me.GroupBox8.Controls.Add(Me.chkShowForInvoice)
        Me.GroupBox8.Controls.Add(Me.chkAllowOrderGrouping)
        Me.GroupBox8.Controls.Add(Me.chkAllowDuplicateItemEntry)
        Me.GroupBox8.Controls.Add(Me.chkShowImportButton)
        Me.GroupBox8.Controls.Add(Me.chkEnableSetTaxButton)
        Me.GroupBox8.Controls.Add(Me.chkShowOrderActions)
        Me.GroupBox8.Controls.Add(Me.chkShowOrderNotifications)
        Me.GroupBox8.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox8.Location = New System.Drawing.Point(464, 392)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.Size = New System.Drawing.Size(508, 256)
        Me.GroupBox8.TabIndex = 17
        Me.GroupBox8.TabStop = False
        Me.GroupBox8.Text = "Work Order and Queue Options"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(16, 26)
        Me.Label19.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(160, 18)
        Me.Label19.TabIndex = 24
        Me.Label19.Text = "Queueing Mode:"
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(248, 26)
        Me.Label20.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(180, 18)
        Me.Label20.TabIndex = 25
        Me.Label20.Text = "Order Entry Features:"
        '
        'frmSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(984, 660)
        Me.Controls.Add(Me.GroupBox8)
        Me.Controls.Add(Me.GroupBox5)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.AutoScroll = True
        Me.Name = "frmSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.TopMost = True
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        CType(Me.gridCustType, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtConfirmP As System.Windows.Forms.TextBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtLimit As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmbLevel As System.Windows.Forms.ComboBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtPassword2 As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents txtConfirmP2 As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents cmdDel As System.Windows.Forms.Button
    Friend WithEvents cmdAdd As System.Windows.Forms.Button
    Friend WithEvents txtCustType As System.Windows.Forms.TextBox
    Friend WithEvents gridCustType As System.Windows.Forms.DataGridView
    Friend WithEvents cmdNew As System.Windows.Forms.Button
    Friend WithEvents cmdPass As System.Windows.Forms.Button
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents chkShowForInvoice As System.Windows.Forms.CheckBox
    Friend WithEvents cmdSaveBranchSettings As System.Windows.Forms.Button
    Friend WithEvents chkAllowOrderGrouping As System.Windows.Forms.CheckBox
    Friend WithEvents chkEnableBranchQueue As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowImportButton As System.Windows.Forms.CheckBox
    Friend WithEvents chkEnableSetTaxButton As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowOrderActions As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowOrderNotifications As System.Windows.Forms.CheckBox
    Friend WithEvents chkAllowDuplicateItemEntry As System.Windows.Forms.CheckBox
    Friend WithEvents chkAllowBranchSelection As System.Windows.Forms.CheckBox
    Friend WithEvents chkLockCustomerSalesRepOnRecall As System.Windows.Forms.CheckBox
    Friend WithEvents chkLockSalesRepToCustomer As System.Windows.Forms.CheckBox
    Friend WithEvents lblAssignedDatabase As System.Windows.Forms.Label
    Friend WithEvents txtExpectedDatabase As System.Windows.Forms.TextBox
    Friend WithEvents cmdPass2 As System.Windows.Forms.Button
    Friend WithEvents cmbPriceBound As System.Windows.Forms.ComboBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmbPricePass As System.Windows.Forms.ComboBox
    Friend WithEvents cmdPass3 As System.Windows.Forms.Button
    Friend WithEvents txtPassword3 As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtConfirmP3 As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents txtMySQLuser As System.Windows.Forms.TextBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents txtMySQLpass As System.Windows.Forms.TextBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents txtMySQLsvr As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents txtMySQLdb As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents GroupBox7 As System.Windows.Forms.GroupBox
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents txtWebCust As System.Windows.Forms.TextBox
    Friend WithEvents txtWebShip As System.Windows.Forms.TextBox
    Friend WithEvents txtWebRep As System.Windows.Forms.TextBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents GroupBox6 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox8 As GroupBox
    Friend WithEvents Label20 As Label
    Friend WithEvents Label19 As Label
End Class
