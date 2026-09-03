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
        Me.chkShowForInvoice = New System.Windows.Forms.CheckBox()
        Me.chkShowImportButton = New System.Windows.Forms.CheckBox()
        Me.chkAllowOrderGrouping = New System.Windows.Forms.CheckBox()
        Me.chkEnableBranchQueue = New System.Windows.Forms.CheckBox()
        Me.chkAllowBranchSelection = New System.Windows.Forms.CheckBox()
        Me.chkLockCustomerSalesRepOnRecall = New System.Windows.Forms.CheckBox()
        Me.chkLockSalesRepToCustomer = New System.Windows.Forms.CheckBox()
        Me.lblAssignedDatabase = New System.Windows.Forms.Label()
        Me.txtExpectedDatabase = New System.Windows.Forms.TextBox()
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
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.gridCustType, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.cmdOK)
        Me.GroupBox1.Controls.Add(Me.cmbLevel)
        Me.GroupBox1.Controls.Add(Me.txtLimit)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(3, 3)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(422, 109)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Default Settings"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(19, 49)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(97, 13)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Price Level Bound:"
        '
        'cmdOK
        '
        Me.cmdOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdOK.Location = New System.Drawing.Point(123, 68)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(38, 23)
        Me.cmdOK.TabIndex = 1
        Me.cmdOK.Text = "&OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmbLevel
        '
        Me.cmbLevel.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbLevel.FormattingEnabled = True
        Me.cmbLevel.Items.AddRange(New Object() {"NONE", "PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)"})
        Me.cmbLevel.Location = New System.Drawing.Point(123, 41)
        Me.cmbLevel.Name = "cmbLevel"
        Me.cmbLevel.Size = New System.Drawing.Size(121, 21)
        Me.cmbLevel.TabIndex = 6
        '
        'txtLimit
        '
        Me.txtLimit.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLimit.Location = New System.Drawing.Point(123, 15)
        Me.txtLimit.Name = "txtLimit"
        Me.txtLimit.Size = New System.Drawing.Size(100, 20)
        Me.txtLimit.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(20, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(66, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Limit Entries:"
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(21, 19)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(113, 23)
        Me.Button1.TabIndex = 12
        Me.Button1.Text = "Manage Users"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(18, 52)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(103, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Confirm Password 1:"
        '
        'txtConfirmP
        '
        Me.txtConfirmP.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtConfirmP.Location = New System.Drawing.Point(136, 45)
        Me.txtConfirmP.Name = "txtConfirmP"
        Me.txtConfirmP.Size = New System.Drawing.Size(83, 20)
        Me.txtConfirmP.TabIndex = 4
        Me.txtConfirmP.UseSystemPasswordChar = True
        '
        'txtPassword
        '
        Me.txtPassword.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPassword.Location = New System.Drawing.Point(136, 19)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(83, 20)
        Me.txtPassword.TabIndex = 3
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(18, 26)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(65, 13)
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
        Me.GroupBox2.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2.Location = New System.Drawing.Point(431, 3)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(465, 186)
        Me.GroupBox2.TabIndex = 13
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Password to Prompt"
        '
        'cmdPass3
        '
        Me.cmdPass3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPass3.Location = New System.Drawing.Point(136, 155)
        Me.cmdPass3.Name = "cmdPass3"
        Me.cmdPass3.Size = New System.Drawing.Size(38, 23)
        Me.cmdPass3.TabIndex = 16
        Me.cmdPass3.Text = "&OK"
        Me.cmdPass3.UseVisualStyleBackColor = True
        '
        'txtPassword3
        '
        Me.txtPassword3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPassword3.Location = New System.Drawing.Point(136, 103)
        Me.txtPassword3.Name = "txtPassword3"
        Me.txtPassword3.Size = New System.Drawing.Size(83, 20)
        Me.txtPassword3.TabIndex = 13
        Me.txtPassword3.UseSystemPasswordChar = True
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(18, 110)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(65, 13)
        Me.Label10.TabIndex = 12
        Me.Label10.Text = "Password 3:"
        '
        'txtConfirmP3
        '
        Me.txtConfirmP3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtConfirmP3.Location = New System.Drawing.Point(136, 129)
        Me.txtConfirmP3.Name = "txtConfirmP3"
        Me.txtConfirmP3.Size = New System.Drawing.Size(83, 20)
        Me.txtConfirmP3.TabIndex = 14
        Me.txtConfirmP3.UseSystemPasswordChar = True
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label11.Location = New System.Drawing.Point(18, 136)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(103, 13)
        Me.Label11.TabIndex = 15
        Me.Label11.Text = "Confirm Password 3:"
        '
        'cmdPass2
        '
        Me.cmdPass2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPass2.Location = New System.Drawing.Point(363, 67)
        Me.cmdPass2.Name = "cmdPass2"
        Me.cmdPass2.Size = New System.Drawing.Size(38, 23)
        Me.cmdPass2.TabIndex = 11
        Me.cmdPass2.Text = "&OK"
        Me.cmdPass2.UseVisualStyleBackColor = True
        '
        'cmdPass
        '
        Me.cmdPass.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPass.Location = New System.Drawing.Point(136, 71)
        Me.cmdPass.Name = "cmdPass"
        Me.cmdPass.Size = New System.Drawing.Size(38, 23)
        Me.cmdPass.TabIndex = 11
        Me.cmdPass.Text = "&OK"
        Me.cmdPass.UseVisualStyleBackColor = True
        '
        'txtPassword2
        '
        Me.txtPassword2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPassword2.Location = New System.Drawing.Point(363, 15)
        Me.txtPassword2.Name = "txtPassword2"
        Me.txtPassword2.Size = New System.Drawing.Size(83, 20)
        Me.txtPassword2.TabIndex = 7
        Me.txtPassword2.UseSystemPasswordChar = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(245, 22)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(65, 13)
        Me.Label6.TabIndex = 6
        Me.Label6.Text = "Password 2:"
        '
        'txtConfirmP2
        '
        Me.txtConfirmP2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtConfirmP2.Location = New System.Drawing.Point(363, 41)
        Me.txtConfirmP2.Name = "txtConfirmP2"
        Me.txtConfirmP2.Size = New System.Drawing.Size(83, 20)
        Me.txtConfirmP2.TabIndex = 8
        Me.txtConfirmP2.UseSystemPasswordChar = True
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(245, 48)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(103, 13)
        Me.Label7.TabIndex = 9
        Me.Label7.Text = "Confirm Password 2:"
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
        Me.GroupBox3.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox3.Location = New System.Drawing.Point(3, 118)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(422, 252)
        Me.GroupBox3.TabIndex = 14
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Customer Type Price Level "
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(260, 24)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(130, 13)
        Me.Label9.TabIndex = 12
        Me.Label9.Text = "Password on Price Below:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(133, 24)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(68, 13)
        Me.Label8.TabIndex = 11
        Me.Label8.Text = "Price Bound:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(3, 24)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(34, 13)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "Type:"
        '
        'cmbPricePass
        '
        Me.cmbPricePass.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbPricePass.FormattingEnabled = True
        Me.cmbPricePass.Items.AddRange(New Object() {"PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)", "COST"})
        Me.cmbPricePass.Location = New System.Drawing.Point(263, 40)
        Me.cmbPricePass.Name = "cmbPricePass"
        Me.cmbPricePass.Size = New System.Drawing.Size(110, 21)
        Me.cmbPricePass.TabIndex = 9
        '
        'cmbPriceBound
        '
        Me.cmbPriceBound.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbPriceBound.FormattingEnabled = True
        Me.cmbPriceBound.Items.AddRange(New Object() {"PRICE (RETAIL)", "PRICE A (WHOLESALE)", "PRICE B (D1)", "PRICE C (D2)", "COST"})
        Me.cmbPriceBound.Location = New System.Drawing.Point(136, 40)
        Me.cmbPriceBound.Name = "cmbPriceBound"
        Me.cmbPriceBound.Size = New System.Drawing.Size(121, 21)
        Me.cmbPriceBound.TabIndex = 8
        '
        'cmdNew
        '
        Me.cmdNew.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdNew.Location = New System.Drawing.Point(378, 38)
        Me.cmdNew.Name = "cmdNew"
        Me.cmdNew.Size = New System.Drawing.Size(38, 23)
        Me.cmdNew.TabIndex = 4
        Me.cmdNew.Text = "New"
        Me.cmdNew.UseVisualStyleBackColor = True
        '
        'cmdDel
        '
        Me.cmdDel.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdDel.Location = New System.Drawing.Point(378, 96)
        Me.cmdDel.Name = "cmdDel"
        Me.cmdDel.Size = New System.Drawing.Size(38, 23)
        Me.cmdDel.TabIndex = 3
        Me.cmdDel.Text = "Del"
        Me.cmdDel.UseVisualStyleBackColor = True
        '
        'cmdAdd
        '
        Me.cmdAdd.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdAdd.Location = New System.Drawing.Point(378, 67)
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(38, 23)
        Me.cmdAdd.TabIndex = 2
        Me.cmdAdd.Text = "Add"
        Me.cmdAdd.UseVisualStyleBackColor = True
        '
        'txtCustType
        '
        Me.txtCustType.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCustType.Location = New System.Drawing.Point(6, 40)
        Me.txtCustType.Name = "txtCustType"
        Me.txtCustType.Size = New System.Drawing.Size(124, 20)
        Me.txtCustType.TabIndex = 1
        '
        'gridCustType
        '
        Me.gridCustType.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridCustType.Location = New System.Drawing.Point(6, 67)
        Me.gridCustType.Name = "gridCustType"
        Me.gridCustType.Size = New System.Drawing.Size(367, 179)
        Me.gridCustType.TabIndex = 0
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.cmdSaveBranchSettings)
        Me.GroupBox4.Controls.Add(Me.chkShowForInvoice)
        Me.GroupBox4.Controls.Add(Me.chkShowImportButton)
        Me.GroupBox4.Controls.Add(Me.chkAllowOrderGrouping)
        Me.GroupBox4.Controls.Add(Me.chkEnableBranchQueue)
        Me.GroupBox4.Controls.Add(Me.chkAllowBranchSelection)
        Me.GroupBox4.Controls.Add(Me.chkLockCustomerSalesRepOnRecall)
        Me.GroupBox4.Controls.Add(Me.chkLockSalesRepToCustomer)
        Me.GroupBox4.Controls.Add(Me.lblAssignedDatabase)
        Me.GroupBox4.Controls.Add(Me.txtExpectedDatabase)
        Me.GroupBox4.Controls.Add(Me.Button1)
        Me.GroupBox4.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox4.Location = New System.Drawing.Point(9, 376)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(416, 165)
        Me.GroupBox4.TabIndex = 15
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Branch Functions"
        '
        'cmdSaveBranchSettings
        '
        Me.cmdSaveBranchSettings.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSaveBranchSettings.Location = New System.Drawing.Point(332, 50)
        Me.cmdSaveBranchSettings.Name = "cmdSaveBranchSettings"
        Me.cmdSaveBranchSettings.Size = New System.Drawing.Size(70, 23)
        Me.cmdSaveBranchSettings.TabIndex = 14
        Me.cmdSaveBranchSettings.Text = "Save"
        Me.cmdSaveBranchSettings.UseVisualStyleBackColor = True
        '
        'chkShowForInvoice
        '
        Me.chkShowForInvoice.AutoSize = True
        Me.chkShowForInvoice.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowForInvoice.Location = New System.Drawing.Point(175, 86)
        Me.chkShowForInvoice.Name = "chkShowForInvoice"
        Me.chkShowForInvoice.Size = New System.Drawing.Size(154, 17)
        Me.chkShowForInvoice.TabIndex = 13
        Me.chkShowForInvoice.Text = "Show For Invoicing button"
        Me.chkShowForInvoice.UseVisualStyleBackColor = True
        '
        'chkShowImportButton
        '
        Me.chkShowImportButton.AutoSize = True
        Me.chkShowImportButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkShowImportButton.Location = New System.Drawing.Point(175, 65)
        Me.chkShowImportButton.Name = "chkShowImportButton"
        Me.chkShowImportButton.Size = New System.Drawing.Size(121, 17)
        Me.chkShowImportButton.TabIndex = 17
        Me.chkShowImportButton.Text = "Show Import button"
        Me.chkShowImportButton.UseVisualStyleBackColor = True
        '
        'chkAllowOrderGrouping
        '
        Me.chkAllowOrderGrouping.AutoSize = True
        Me.chkAllowOrderGrouping.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAllowOrderGrouping.Location = New System.Drawing.Point(175, 44)
        Me.chkAllowOrderGrouping.Name = "chkAllowOrderGrouping"
        Me.chkAllowOrderGrouping.Size = New System.Drawing.Size(126, 17)
        Me.chkAllowOrderGrouping.TabIndex = 15
        Me.chkAllowOrderGrouping.Text = "Allow order grouping"
        Me.chkAllowOrderGrouping.UseVisualStyleBackColor = True
        '
        'chkEnableBranchQueue
        '
        Me.chkEnableBranchQueue.AutoSize = True
        Me.chkEnableBranchQueue.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkEnableBranchQueue.Location = New System.Drawing.Point(175, 23)
        Me.chkEnableBranchQueue.Name = "chkEnableBranchQueue"
        Me.chkEnableBranchQueue.Size = New System.Drawing.Size(132, 17)
        Me.chkEnableBranchQueue.TabIndex = 16
        Me.chkEnableBranchQueue.Text = "Enable Branch Queue"
        Me.chkEnableBranchQueue.UseVisualStyleBackColor = True
        '
        'chkAllowBranchSelection
        '
        Me.chkAllowBranchSelection.AutoSize = True
        Me.chkAllowBranchSelection.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAllowBranchSelection.Location = New System.Drawing.Point(21, 51)
        Me.chkAllowBranchSelection.Name = "chkAllowBranchSelection"
        Me.chkAllowBranchSelection.Size = New System.Drawing.Size(136, 17)
        Me.chkAllowBranchSelection.TabIndex = 18
        Me.chkAllowBranchSelection.Text = "Allow Branch Selection"
        Me.chkAllowBranchSelection.UseVisualStyleBackColor = True
        '
        'chkLockCustomerSalesRepOnRecall
        '
        Me.chkLockCustomerSalesRepOnRecall.AutoSize = True
        Me.chkLockCustomerSalesRepOnRecall.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkLockCustomerSalesRepOnRecall.Location = New System.Drawing.Point(21, 121)
        Me.chkLockCustomerSalesRepOnRecall.Name = "chkLockCustomerSalesRepOnRecall"
        Me.chkLockCustomerSalesRepOnRecall.Size = New System.Drawing.Size(240, 17)
        Me.chkLockCustomerSalesRepOnRecall.TabIndex = 21
        Me.chkLockCustomerSalesRepOnRecall.Text = "Lock Customer/Sales Rep on Recall"
        Me.chkLockCustomerSalesRepOnRecall.UseVisualStyleBackColor = True
        '
        'chkLockSalesRepToCustomer
        '
        Me.chkLockSalesRepToCustomer.AutoSize = True
        Me.chkLockSalesRepToCustomer.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkLockSalesRepToCustomer.Location = New System.Drawing.Point(21, 141)
        Me.chkLockSalesRepToCustomer.Name = "chkLockSalesRepToCustomer"
        Me.chkLockSalesRepToCustomer.Size = New System.Drawing.Size(291, 17)
        Me.chkLockSalesRepToCustomer.TabIndex = 22
        Me.chkLockSalesRepToCustomer.Text = "Lock Sales Rep to Customer's Rep (New Orders)"
        Me.chkLockSalesRepToCustomer.UseVisualStyleBackColor = True
        '
        'lblAssignedDatabase
        '
        Me.lblAssignedDatabase.AutoSize = True
        Me.lblAssignedDatabase.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAssignedDatabase.Location = New System.Drawing.Point(18, 78)
        Me.lblAssignedDatabase.Name = "lblAssignedDatabase"
        Me.lblAssignedDatabase.Size = New System.Drawing.Size(104, 13)
        Me.lblAssignedDatabase.TabIndex = 19
        Me.lblAssignedDatabase.Text = "Assigned Database:"
        '
        'txtExpectedDatabase
        '
        Me.txtExpectedDatabase.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtExpectedDatabase.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtExpectedDatabase.Location = New System.Drawing.Point(21, 98)
        Me.txtExpectedDatabase.Name = "txtExpectedDatabase"
        Me.txtExpectedDatabase.Size = New System.Drawing.Size(282, 20)
        Me.txtExpectedDatabase.TabIndex = 20
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.GroupBox7)
        Me.GroupBox5.Controls.Add(Me.GroupBox6)
        Me.GroupBox5.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox5.Location = New System.Drawing.Point(431, 195)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(463, 254)
        Me.GroupBox5.TabIndex = 16
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "Website"
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
        Me.GroupBox7.Location = New System.Drawing.Point(10, 25)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Size = New System.Drawing.Size(209, 175)
        Me.GroupBox7.TabIndex = 22
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "Import Defauls"
        '
        'Button3
        '
        Me.Button3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button3.Location = New System.Drawing.Point(162, 109)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(38, 23)
        Me.Button3.TabIndex = 21
        Me.Button3.Text = "&OK"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label16.Location = New System.Drawing.Point(10, 34)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(57, 13)
        Me.Label16.TabIndex = 21
        Me.Label16.Text = "Customer :"
        '
        'txtWebCust
        '
        Me.txtWebCust.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtWebCust.Location = New System.Drawing.Point(82, 31)
        Me.txtWebCust.Name = "txtWebCust"
        Me.txtWebCust.Size = New System.Drawing.Size(118, 20)
        Me.txtWebCust.TabIndex = 22
        '
        'txtWebShip
        '
        Me.txtWebShip.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtWebShip.Location = New System.Drawing.Point(82, 83)
        Me.txtWebShip.Name = "txtWebShip"
        Me.txtWebShip.Size = New System.Drawing.Size(118, 20)
        Me.txtWebShip.TabIndex = 26
        '
        'txtWebRep
        '
        Me.txtWebRep.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtWebRep.Location = New System.Drawing.Point(82, 57)
        Me.txtWebRep.Name = "txtWebRep"
        Me.txtWebRep.Size = New System.Drawing.Size(118, 20)
        Me.txtWebRep.TabIndex = 23
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label17.Location = New System.Drawing.Point(10, 60)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(62, 13)
        Me.Label17.TabIndex = 24
        Me.Label17.Text = "Sales Rep :"
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label18.Location = New System.Drawing.Point(10, 86)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(54, 13)
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
        Me.GroupBox6.Location = New System.Drawing.Point(225, 25)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(221, 175)
        Me.GroupBox6.TabIndex = 21
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "MySQL Connection"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(12, 31)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(44, 13)
        Me.Label12.TabIndex = 12
        Me.Label12.Text = "Server :"
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button2.Location = New System.Drawing.Point(164, 132)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(38, 23)
        Me.Button2.TabIndex = 16
        Me.Button2.Text = "&OK"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'txtMySQLuser
        '
        Me.txtMySQLuser.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMySQLuser.Location = New System.Drawing.Point(84, 80)
        Me.txtMySQLuser.Name = "txtMySQLuser"
        Me.txtMySQLuser.Size = New System.Drawing.Size(118, 20)
        Me.txtMySQLuser.TabIndex = 18
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.Location = New System.Drawing.Point(12, 57)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(53, 13)
        Me.Label13.TabIndex = 15
        Me.Label13.Text = "Database"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.Location = New System.Drawing.Point(12, 83)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(61, 13)
        Me.Label14.TabIndex = 17
        Me.Label14.Text = "Username :"
        '
        'txtMySQLdb
        '
        Me.txtMySQLdb.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMySQLdb.Location = New System.Drawing.Point(84, 54)
        Me.txtMySQLdb.Name = "txtMySQLdb"
        Me.txtMySQLdb.Size = New System.Drawing.Size(118, 20)
        Me.txtMySQLdb.TabIndex = 14
        '
        'txtMySQLpass
        '
        Me.txtMySQLpass.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMySQLpass.Location = New System.Drawing.Point(84, 106)
        Me.txtMySQLpass.Name = "txtMySQLpass"
        Me.txtMySQLpass.Size = New System.Drawing.Size(118, 20)
        Me.txtMySQLpass.TabIndex = 19
        '
        'txtMySQLsvr
        '
        Me.txtMySQLsvr.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtMySQLsvr.Location = New System.Drawing.Point(84, 28)
        Me.txtMySQLsvr.Name = "txtMySQLsvr"
        Me.txtMySQLsvr.Size = New System.Drawing.Size(118, 20)
        Me.txtMySQLsvr.TabIndex = 13
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.Location = New System.Drawing.Point(12, 109)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(59, 13)
        Me.Label15.TabIndex = 20
        Me.Label15.Text = "Password :"
        '
        'frmSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(906, 550)
        Me.Controls.Add(Me.GroupBox5)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
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
End Class
