<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmItemLookUp
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmItemLookUp))
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle12 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle11 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.ofdImport = New System.Windows.Forms.OpenFileDialog()
        Me.txtRemarks = New System.Windows.Forms.TextBox()
        Me.lblRemarks = New System.Windows.Forms.Label()
        Me.txtType = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtCustomer = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtSales = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.BottomStrip = New System.Windows.Forms.StatusStrip()
        Me.statServer = New System.Windows.Forms.ToolStripStatusLabel()
        Me.statDb = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabel3 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripStatusLabel2 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblOrderNo = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblStatItemSelected = New System.Windows.Forms.ToolStripStatusLabel()
        Me.lblStatItemCount = New System.Windows.Forms.ToolStripStatusLabel()
        Me.panelButtons = New System.Windows.Forms.Panel()
        Me.cmdForInvoice = New System.Windows.Forms.Button()
        Me.cmdLogout = New System.Windows.Forms.Button()
        Me.cmdRecall = New System.Windows.Forms.Button()
        Me.cmdSettings = New System.Windows.Forms.Button()
        Me.cmdOk = New System.Windows.Forms.Button()
        Me.cmdImport = New System.Windows.Forms.Button()
        Me.cmdPriceLevel = New System.Windows.Forms.Button()
        Me.cmdInterStore = New System.Windows.Forms.Button()
        Me.cmdRefresh = New System.Windows.Forms.Button()
        Me.cmdRemove = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.cmdSelect = New System.Windows.Forms.Button()
        Me.panelTotal = New System.Windows.Forms.Panel()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.txtVat = New System.Windows.Forms.TextBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.txtSub = New System.Windows.Forms.TextBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.txtTotal = New System.Windows.Forms.TextBox()
        Me.panelGrid = New System.Windows.Forms.Panel()
        Me.panelItem = New System.Windows.Forms.Panel()
        Me.gridItem = New System.Windows.Forms.DataGridView()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.chkQuote = New System.Windows.Forms.CheckBox()
        Me.chkWorkOrder = New System.Windows.Forms.CheckBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.rbtnDelivery = New System.Windows.Forms.RadioButton()
        Me.rbtnPickup = New System.Windows.Forms.RadioButton()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cboPayment = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.cmdSSales = New System.Windows.Forms.Button()
        Me.cmdSCustomer = New System.Windows.Forms.Button()
        Me.cmdSearch = New System.Windows.Forms.Button()
        Me.panelSelectItem = New System.Windows.Forms.Panel()
        Me.gridSelectItem = New System.Windows.Forms.DataGridView()
        Me.ItemCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.QTY = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Price = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DISC = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TOTAL = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.LessV = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.VSales = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DiscP = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Cost = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Taxable = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FullPrice = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Description = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Extended = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DiscAmount = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.OrderEntryID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.chkPickLoc = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.CustPrep = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Update_Timer = New System.Windows.Forms.Timer(Me.components)
        Me.Check_errors = New System.Windows.Forms.Timer(Me.components)
        Me.BottomStrip.SuspendLayout()
        Me.panelButtons.SuspendLayout()
        Me.panelTotal.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.panelGrid.SuspendLayout()
        Me.panelItem.SuspendLayout()
        CType(Me.gridItem, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel4.SuspendLayout()
        Me.Panel5.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panelSelectItem.SuspendLayout()
        CType(Me.gridSelectItem, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ofdImport
        '
        Me.ofdImport.FileName = "OpenFileDialog1"
        '
        'txtRemarks
        '
        Me.txtRemarks.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtRemarks.Location = New System.Drawing.Point(8, 184)
        Me.txtRemarks.Name = "txtRemarks"
        Me.txtRemarks.Size = New System.Drawing.Size(326, 23)
        Me.txtRemarks.TabIndex = 6
        '
        'lblRemarks
        '
        Me.lblRemarks.AutoSize = True
        Me.lblRemarks.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRemarks.Location = New System.Drawing.Point(5, 167)
        Me.lblRemarks.Name = "lblRemarks"
        Me.lblRemarks.Size = New System.Drawing.Size(125, 13)
        Me.lblRemarks.TabIndex = 61
        Me.lblRemarks.Text = "Comment (e.g. PO#):"
        '
        'txtType
        '
        Me.txtType.BackColor = System.Drawing.SystemColors.Control
        Me.txtType.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtType.Location = New System.Drawing.Point(8, 92)
        Me.txtType.Name = "txtType"
        Me.txtType.ReadOnly = True
        Me.txtType.Size = New System.Drawing.Size(208, 23)
        Me.txtType.TabIndex = 3
        Me.txtType.TabStop = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(5, 77)
        Me.Label5.Name = "Label5"
        Me.Label5.Padding = New System.Windows.Forms.Padding(0, 0, 27, 0)
        Me.Label5.Size = New System.Drawing.Size(117, 13)
        Me.Label5.TabIndex = 59
        Me.Label5.Text = "Business Type:"
        '
        'txtCustomer
        '
        Me.txtCustomer.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCustomer.Location = New System.Drawing.Point(6, 49)
        Me.txtCustomer.Name = "txtCustomer"
        Me.txtCustomer.ReadOnly = True
        Me.txtCustomer.Size = New System.Drawing.Size(287, 23)
        Me.txtCustomer.TabIndex = 1
        Me.txtCustomer.TabStop = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(5, 34)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 13)
        Me.Label3.TabIndex = 61
        Me.Label3.Text = "Customer:"
        '
        'txtSales
        '
        Me.txtSales.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSales.Location = New System.Drawing.Point(8, 137)
        Me.txtSales.Name = "txtSales"
        Me.txtSales.ReadOnly = True
        Me.txtSales.Size = New System.Drawing.Size(285, 23)
        Me.txtSales.TabIndex = 4
        Me.txtSales.TabStop = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(5, 121)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(65, 13)
        Me.Label2.TabIndex = 54
        Me.Label2.Text = "Sales Rep:"
        '
        'txtSearch
        '
        Me.txtSearch.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSearch.Location = New System.Drawing.Point(6, 261)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(287, 23)
        Me.txtSearch.TabIndex = 8
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(5, 243)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(190, 13)
        Me.Label1.TabIndex = 63
        Me.Label1.Text = "Look Up Item Code/Description: "
        '
        'BottomStrip
        '
        Me.BottomStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statServer, Me.statDb, Me.ToolStripStatusLabel1, Me.ToolStripStatusLabel3, Me.ToolStripStatusLabel2, Me.lblOrderNo, Me.lblStatItemSelected, Me.lblStatItemCount})
        Me.BottomStrip.Location = New System.Drawing.Point(0, 721)
        Me.BottomStrip.Name = "BottomStrip"
        Me.BottomStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode
        Me.BottomStrip.Size = New System.Drawing.Size(1350, 26)
        Me.BottomStrip.SizingGrip = False
        Me.BottomStrip.TabIndex = 21
        Me.BottomStrip.Text = "StatusStrip1"
        '
        'statServer
        '
        Me.statServer.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.statServer.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken
        Me.statServer.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.statServer.Name = "statServer"
        Me.statServer.Padding = New System.Windows.Forms.Padding(0, 1, 50, 1)
        Me.statServer.Size = New System.Drawing.Size(114, 21)
        Me.statServer.Text = "Location: "
        Me.statServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'statDb
        '
        Me.statDb.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.statDb.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken
        Me.statDb.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.statDb.Name = "statDb"
        Me.statDb.Padding = New System.Windows.Forms.Padding(0, 1, 50, 1)
        Me.statDb.Size = New System.Drawing.Size(115, 21)
        Me.statDb.Text = "Database:"
        Me.statDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(716, 21)
        Me.ToolStripStatusLabel1.Spring = True
        Me.ToolStripStatusLabel1.Text = "STATUS : "
        Me.ToolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripStatusLabel3
        '
        Me.ToolStripStatusLabel3.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabel3.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripStatusLabel3.Name = "ToolStripStatusLabel3"
        Me.ToolStripStatusLabel3.Size = New System.Drawing.Size(67, 21)
        Me.ToolStripStatusLabel3.Text = "Register : "
        '
        'ToolStripStatusLabel2
        '
        Me.ToolStripStatusLabel2.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.ToolStripStatusLabel2.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        Me.ToolStripStatusLabel2.Size = New System.Drawing.Size(46, 21)
        Me.ToolStripStatusLabel2.Text = "User : "
        '
        'lblOrderNo
        '
        Me.lblOrderNo.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.lblOrderNo.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOrderNo.Name = "lblOrderNo"
        Me.lblOrderNo.Size = New System.Drawing.Size(57, 21)
        Me.lblOrderNo.Text = "Order #:"
        '
        'lblStatItemSelected
        '
        Me.lblStatItemSelected.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.lblStatItemSelected.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatItemSelected.Name = "lblStatItemSelected"
        Me.lblStatItemSelected.Size = New System.Drawing.Size(97, 21)
        Me.lblStatItemSelected.Text = "0 item selected"
        Me.lblStatItemSelected.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblStatItemCount
        '
        Me.lblStatItemCount.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
                    Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me.lblStatItemCount.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatItemCount.Name = "lblStatItemCount"
        Me.lblStatItemCount.Padding = New System.Windows.Forms.Padding(50, 0, 0, 0)
        Me.lblStatItemCount.Size = New System.Drawing.Size(123, 21)
        Me.lblStatItemCount.Text = "6009 items"
        Me.lblStatItemCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'panelButtons
        '
        Me.panelButtons.Controls.Add(Me.cmdForInvoice)
        Me.panelButtons.Controls.Add(Me.cmdLogout)
        Me.panelButtons.Controls.Add(Me.cmdRecall)
        Me.panelButtons.Controls.Add(Me.cmdSettings)
        Me.panelButtons.Controls.Add(Me.cmdOk)
        Me.panelButtons.Controls.Add(Me.cmdImport)
        Me.panelButtons.Controls.Add(Me.cmdPriceLevel)
        Me.panelButtons.Controls.Add(Me.cmdInterStore)
        Me.panelButtons.Controls.Add(Me.cmdRefresh)
        Me.panelButtons.Controls.Add(Me.cmdRemove)
        Me.panelButtons.Controls.Add(Me.cmdCancel)
        Me.panelButtons.Controls.Add(Me.cmdClose)
        Me.panelButtons.Controls.Add(Me.cmdSelect)
        Me.panelButtons.Dock = System.Windows.Forms.DockStyle.Right
        Me.panelButtons.Location = New System.Drawing.Point(1231, 0)
        Me.panelButtons.Name = "panelButtons"
        Me.panelButtons.Padding = New System.Windows.Forms.Padding(3)
        Me.panelButtons.Size = New System.Drawing.Size(119, 721)
        Me.panelButtons.TabIndex = 23
        '
        'cmdForInvoice
        '
        Me.cmdForInvoice.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdForInvoice.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdForInvoice.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdForInvoice.Location = New System.Drawing.Point(3, 487)
        Me.cmdForInvoice.Name = "cmdForInvoice"
        Me.cmdForInvoice.Size = New System.Drawing.Size(113, 33)
        Me.cmdForInvoice.TabIndex = 23
        Me.cmdForInvoice.Text = "For Invoice"
        Me.cmdForInvoice.UseVisualStyleBackColor = True
        '
        'cmdLogout
        '
        Me.cmdLogout.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdLogout.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdLogout.Image = CType(resources.GetObject("cmdLogout.Image"), System.Drawing.Image)
        Me.cmdLogout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdLogout.Location = New System.Drawing.Point(3, 168)
        Me.cmdLogout.Name = "cmdLogout"
        Me.cmdLogout.Size = New System.Drawing.Size(113, 33)
        Me.cmdLogout.TabIndex = 22
        Me.cmdLogout.Text = "Logout"
        Me.cmdLogout.UseVisualStyleBackColor = True
        '
        'cmdRecall
        '
        Me.cmdRecall.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdRecall.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdRecall.Image = CType(resources.GetObject("cmdRecall.Image"), System.Drawing.Image)
        Me.cmdRecall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRecall.Location = New System.Drawing.Point(3, 520)
        Me.cmdRecall.Name = "cmdRecall"
        Me.cmdRecall.Size = New System.Drawing.Size(113, 33)
        Me.cmdRecall.TabIndex = 20
        Me.cmdRecall.Text = "Recall [F11]"
        Me.cmdRecall.UseVisualStyleBackColor = True
        '
        'cmdSettings
        '
        Me.cmdSettings.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdSettings.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSettings.Image = CType(resources.GetObject("cmdSettings.Image"), System.Drawing.Image)
        Me.cmdSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSettings.Location = New System.Drawing.Point(3, 135)
        Me.cmdSettings.Name = "cmdSettings"
        Me.cmdSettings.Size = New System.Drawing.Size(113, 33)
        Me.cmdSettings.TabIndex = 19
        Me.cmdSettings.Text = "Settings"
        Me.cmdSettings.UseVisualStyleBackColor = True
        '
        'cmdOk
        '
        Me.cmdOk.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdOk.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdOk.Image = CType(resources.GetObject("cmdOk.Image"), System.Drawing.Image)
        Me.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdOk.Location = New System.Drawing.Point(3, 553)
        Me.cmdOk.Name = "cmdOk"
        Me.cmdOk.Size = New System.Drawing.Size(113, 33)
        Me.cmdOk.TabIndex = 14
        Me.cmdOk.Text = "Save [F12]"
        Me.cmdOk.UseVisualStyleBackColor = True
        '
        'cmdImport
        '
        Me.cmdImport.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdImport.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdImport.Image = CType(resources.GetObject("cmdImport.Image"), System.Drawing.Image)
        Me.cmdImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdImport.Location = New System.Drawing.Point(3, 586)
        Me.cmdImport.Name = "cmdImport"
        Me.cmdImport.Size = New System.Drawing.Size(113, 33)
        Me.cmdImport.TabIndex = 15
        Me.cmdImport.Text = "    Import [F6]"
        Me.cmdImport.UseVisualStyleBackColor = True
        '
        'cmdPriceLevel
        '
        Me.cmdPriceLevel.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdPriceLevel.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPriceLevel.Image = CType(resources.GetObject("cmdPriceLevel.Image"), System.Drawing.Image)
        Me.cmdPriceLevel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdPriceLevel.Location = New System.Drawing.Point(3, 102)
        Me.cmdPriceLevel.Name = "cmdPriceLevel"
        Me.cmdPriceLevel.Size = New System.Drawing.Size(113, 33)
        Me.cmdPriceLevel.TabIndex = 13
        Me.cmdPriceLevel.Text = "Price Level"
        Me.cmdPriceLevel.UseVisualStyleBackColor = True
        '
        'cmdInterStore
        '
        Me.cmdInterStore.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdInterStore.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdInterStore.Image = CType(resources.GetObject("cmdInterStore.Image"), System.Drawing.Image)
        Me.cmdInterStore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdInterStore.Location = New System.Drawing.Point(3, 69)
        Me.cmdInterStore.Name = "cmdInterStore"
        Me.cmdInterStore.Size = New System.Drawing.Size(113, 33)
        Me.cmdInterStore.TabIndex = 12
        Me.cmdInterStore.Text = "InterStore"
        Me.cmdInterStore.UseVisualStyleBackColor = True
        '
        'cmdRefresh
        '
        Me.cmdRefresh.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdRefresh.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdRefresh.Image = CType(resources.GetObject("cmdRefresh.Image"), System.Drawing.Image)
        Me.cmdRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRefresh.Location = New System.Drawing.Point(3, 36)
        Me.cmdRefresh.Name = "cmdRefresh"
        Me.cmdRefresh.Size = New System.Drawing.Size(113, 33)
        Me.cmdRefresh.TabIndex = 11
        Me.cmdRefresh.Text = "Refresh"
        Me.cmdRefresh.UseVisualStyleBackColor = True
        '
        'cmdRemove
        '
        Me.cmdRemove.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdRemove.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdRemove.Image = CType(resources.GetObject("cmdRemove.Image"), System.Drawing.Image)
        Me.cmdRemove.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRemove.Location = New System.Drawing.Point(3, 619)
        Me.cmdRemove.Name = "cmdRemove"
        Me.cmdRemove.Size = New System.Drawing.Size(113, 33)
        Me.cmdRemove.TabIndex = 16
        Me.cmdRemove.Text = "      Remove [Del]"
        Me.cmdRemove.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdCancel.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdCancel.Image = CType(resources.GetObject("cmdCancel.Image"), System.Drawing.Image)
        Me.cmdCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdCancel.Location = New System.Drawing.Point(3, 652)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(113, 33)
        Me.cmdCancel.TabIndex = 17
        Me.cmdCancel.Text = " Cancel [F9]"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdClose
        '
        Me.cmdClose.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdClose.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdClose.Image = CType(resources.GetObject("cmdClose.Image"), System.Drawing.Image)
        Me.cmdClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdClose.Location = New System.Drawing.Point(3, 685)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(113, 33)
        Me.cmdClose.TabIndex = 18
        Me.cmdClose.Text = "   Close [Esc]"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'cmdSelect
        '
        Me.cmdSelect.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdSelect.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSelect.Image = CType(resources.GetObject("cmdSelect.Image"), System.Drawing.Image)
        Me.cmdSelect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSelect.Location = New System.Drawing.Point(3, 3)
        Me.cmdSelect.Name = "cmdSelect"
        Me.cmdSelect.Size = New System.Drawing.Size(113, 33)
        Me.cmdSelect.TabIndex = 10
        Me.cmdSelect.Text = "Select"
        Me.cmdSelect.UseVisualStyleBackColor = True
        '
        'panelTotal
        '
        Me.panelTotal.BackColor = System.Drawing.SystemColors.Control
        Me.panelTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panelTotal.Controls.Add(Me.GroupBox3)
        Me.panelTotal.Controls.Add(Me.GroupBox2)
        Me.panelTotal.Controls.Add(Me.GroupBox4)
        Me.panelTotal.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelTotal.Location = New System.Drawing.Point(0, 658)
        Me.panelTotal.Name = "panelTotal"
        Me.panelTotal.Padding = New System.Windows.Forms.Padding(5)
        Me.panelTotal.Size = New System.Drawing.Size(1231, 63)
        Me.panelTotal.TabIndex = 24
        '
        'GroupBox3
        '
        Me.GroupBox3.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.GroupBox3.Controls.Add(Me.txtVat)
        Me.GroupBox3.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold)
        Me.GroupBox3.ForeColor = System.Drawing.Color.Black
        Me.GroupBox3.Location = New System.Drawing.Point(630, 1)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(279, 58)
        Me.GroupBox3.TabIndex = 16
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "VAT"
        '
        'txtVat
        '
        Me.txtVat.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtVat.Font = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtVat.Location = New System.Drawing.Point(3, 19)
        Me.txtVat.Name = "txtVat"
        Me.txtVat.ReadOnly = True
        Me.txtVat.Size = New System.Drawing.Size(273, 30)
        Me.txtVat.TabIndex = 15
        Me.txtVat.TabStop = False
        Me.txtVat.Text = "0.00"
        Me.txtVat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'GroupBox2
        '
        Me.GroupBox2.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.GroupBox2.Controls.Add(Me.txtSub)
        Me.GroupBox2.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2.ForeColor = System.Drawing.Color.Black
        Me.GroupBox2.Location = New System.Drawing.Point(345, 1)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(279, 58)
        Me.GroupBox2.TabIndex = 11
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Sub ToTal"
        '
        'txtSub
        '
        Me.txtSub.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtSub.Font = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSub.Location = New System.Drawing.Point(3, 19)
        Me.txtSub.Name = "txtSub"
        Me.txtSub.ReadOnly = True
        Me.txtSub.Size = New System.Drawing.Size(273, 30)
        Me.txtSub.TabIndex = 14
        Me.txtSub.TabStop = False
        Me.txtSub.Text = "0.00"
        Me.txtSub.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'GroupBox4
        '
        Me.GroupBox4.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.GroupBox4.Controls.Add(Me.txtTotal)
        Me.GroupBox4.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold)
        Me.GroupBox4.ForeColor = System.Drawing.Color.Black
        Me.GroupBox4.Location = New System.Drawing.Point(912, 1)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(312, 58)
        Me.GroupBox4.TabIndex = 17
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Total"
        '
        'txtTotal
        '
        Me.txtTotal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtTotal.Font = New System.Drawing.Font("Tahoma", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTotal.Location = New System.Drawing.Point(3, 19)
        Me.txtTotal.Name = "txtTotal"
        Me.txtTotal.ReadOnly = True
        Me.txtTotal.Size = New System.Drawing.Size(306, 30)
        Me.txtTotal.TabIndex = 16
        Me.txtTotal.TabStop = False
        Me.txtTotal.Text = "0.00"
        Me.txtTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'panelGrid
        '
        Me.panelGrid.Controls.Add(Me.panelItem)
        Me.panelGrid.Controls.Add(Me.panelSelectItem)
        Me.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelGrid.Location = New System.Drawing.Point(0, 0)
        Me.panelGrid.Name = "panelGrid"
        Me.panelGrid.Size = New System.Drawing.Size(1231, 658)
        Me.panelGrid.TabIndex = 25
        '
        'panelItem
        '
        Me.panelItem.Controls.Add(Me.gridItem)
        Me.panelItem.Controls.Add(Me.Panel4)
        Me.panelItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelItem.Location = New System.Drawing.Point(0, 0)
        Me.panelItem.Name = "panelItem"
        Me.panelItem.Padding = New System.Windows.Forms.Padding(10, 0, 10, 0)
        Me.panelItem.Size = New System.Drawing.Size(1231, 438)
        Me.panelItem.TabIndex = 18
        '
        'gridItem
        '
        Me.gridItem.AllowUserToAddRows = False
        Me.gridItem.AllowUserToDeleteRows = False
        Me.gridItem.AllowUserToOrderColumns = True
        DataGridViewCellStyle7.BackColor = System.Drawing.Color.Lavender
        Me.gridItem.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle7
        Me.gridItem.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridItem.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        Me.gridItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridItem.Location = New System.Drawing.Point(352, 0)
        Me.gridItem.Name = "gridItem"
        Me.gridItem.ReadOnly = True
        Me.gridItem.RowTemplate.DefaultCellStyle.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gridItem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridItem.Size = New System.Drawing.Size(869, 438)
        Me.gridItem.TabIndex = 18
        Me.gridItem.TabStop = False
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.TreeView1)
        Me.Panel4.Controls.Add(Me.Panel5)
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel4.Location = New System.Drawing.Point(10, 0)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(342, 438)
        Me.Panel4.TabIndex = 17
        '
        'TreeView1
        '
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Location = New System.Drawing.Point(0, 310)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(342, 128)
        Me.TreeView1.TabIndex = 1
        '
        'Panel5
        '
        Me.Panel5.AutoScroll = True
        Me.Panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel5.Controls.Add(Me.chkQuote)
        Me.Panel5.Controls.Add(Me.chkWorkOrder)
        Me.Panel5.Controls.Add(Me.PictureBox1)
        Me.Panel5.Controls.Add(Me.PictureBox2)
        Me.Panel5.Controls.Add(Me.Label9)
        Me.Panel5.Controls.Add(Me.rbtnDelivery)
        Me.Panel5.Controls.Add(Me.rbtnPickup)
        Me.Panel5.Controls.Add(Me.Label8)
        Me.Panel5.Controls.Add(Me.Label6)
        Me.Panel5.Controls.Add(Me.cboPayment)
        Me.Panel5.Controls.Add(Me.Label7)
        Me.Panel5.Controls.Add(Me.txtRemarks)
        Me.Panel5.Controls.Add(Me.lblRemarks)
        Me.Panel5.Controls.Add(Me.cmdSSales)
        Me.Panel5.Controls.Add(Me.txtCustomer)
        Me.Panel5.Controls.Add(Me.txtType)
        Me.Panel5.Controls.Add(Me.Label5)
        Me.Panel5.Controls.Add(Me.txtSales)
        Me.Panel5.Controls.Add(Me.Label2)
        Me.Panel5.Controls.Add(Me.Label3)
        Me.Panel5.Controls.Add(Me.cmdSCustomer)
        Me.Panel5.Controls.Add(Me.txtSearch)
        Me.Panel5.Controls.Add(Me.Label1)
        Me.Panel5.Controls.Add(Me.cmdSearch)
        Me.Panel5.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel5.Location = New System.Drawing.Point(0, 0)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(342, 310)
        Me.Panel5.TabIndex = 0
        '
        'chkQuote
        '
        Me.chkQuote.AutoSize = True
        Me.chkQuote.Location = New System.Drawing.Point(176, 6)
        Me.chkQuote.Name = "chkQuote"
        Me.chkQuote.Size = New System.Drawing.Size(75, 18)
        Me.chkQuote.TabIndex = 78
        Me.chkQuote.Text = "Quotation"
        Me.chkQuote.UseVisualStyleBackColor = True
        '
        'chkWorkOrder
        '
        Me.chkWorkOrder.AutoSize = True
        Me.chkWorkOrder.Checked = True
        Me.chkWorkOrder.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWorkOrder.Location = New System.Drawing.Point(86, 6)
        Me.chkWorkOrder.Name = "chkWorkOrder"
        Me.chkWorkOrder.Size = New System.Drawing.Size(84, 18)
        Me.chkWorkOrder.TabIndex = 77
        Me.chkWorkOrder.Text = "Work Order"
        Me.chkWorkOrder.UseVisualStyleBackColor = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.SystemColors.ButtonShadow
        Me.PictureBox1.Location = New System.Drawing.Point(7, 28)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(327, 1)
        Me.PictureBox1.TabIndex = 76
        Me.PictureBox1.TabStop = False
        '
        'PictureBox2
        '
        Me.PictureBox2.BackColor = System.Drawing.SystemColors.ButtonShadow
        Me.PictureBox2.Location = New System.Drawing.Point(7, 235)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(327, 1)
        Me.PictureBox2.TabIndex = 75
        Me.PictureBox2.TabStop = False
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(5, 7)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(74, 13)
        Me.Label9.TabIndex = 71
        Me.Label9.Text = "Entry Type :"
        '
        'rbtnDelivery
        '
        Me.rbtnDelivery.AutoSize = True
        Me.rbtnDelivery.Checked = True
        Me.rbtnDelivery.Font = New System.Drawing.Font("Calibri", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnDelivery.Location = New System.Drawing.Point(167, 212)
        Me.rbtnDelivery.Name = "rbtnDelivery"
        Me.rbtnDelivery.Size = New System.Drawing.Size(70, 19)
        Me.rbtnDelivery.TabIndex = 70
        Me.rbtnDelivery.TabStop = True
        Me.rbtnDelivery.Text = "Delivery"
        Me.rbtnDelivery.UseVisualStyleBackColor = True
        '
        'rbtnPickup
        '
        Me.rbtnPickup.AutoSize = True
        Me.rbtnPickup.Font = New System.Drawing.Font("Calibri", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnPickup.Location = New System.Drawing.Point(99, 212)
        Me.rbtnPickup.Name = "rbtnPickup"
        Me.rbtnPickup.Size = New System.Drawing.Size(64, 19)
        Me.rbtnPickup.TabIndex = 69
        Me.rbtnPickup.Text = "Pick-up"
        Me.rbtnPickup.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(5, 214)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(89, 13)
        Me.Label8.TabIndex = 68
        Me.Label8.Text = "Release Type :"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(5, 288)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(279, 13)
        Me.Label6.TabIndex = 67
        Me.Label6.Text = "Press F4 to Remove Recent Search Keyword. F5 to Clear All"
        '
        'cboPayment
        '
        Me.cboPayment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboPayment.Font = New System.Drawing.Font("Tahoma", 9.75!)
        Me.cboPayment.FormattingEnabled = True
        Me.cboPayment.Items.AddRange(New Object() {"CASH", "CHARGE", "CREDIT CARD", "DEBIT CARD"})
        Me.cboPayment.Location = New System.Drawing.Point(222, 92)
        Me.cboPayment.Name = "cboPayment"
        Me.cboPayment.Size = New System.Drawing.Size(112, 24)
        Me.cboPayment.TabIndex = 66
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(219, 77)
        Me.Label7.Name = "Label7"
        Me.Label7.Padding = New System.Windows.Forms.Padding(0, 0, 27, 0)
        Me.Label7.Size = New System.Drawing.Size(119, 13)
        Me.Label7.TabIndex = 65
        Me.Label7.Text = "Payment Type:"
        '
        'cmdSSales
        '
        Me.cmdSSales.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSSales.Image = CType(resources.GetObject("cmdSSales.Image"), System.Drawing.Image)
        Me.cmdSSales.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSSales.Location = New System.Drawing.Point(293, 136)
        Me.cmdSSales.Name = "cmdSSales"
        Me.cmdSSales.Size = New System.Drawing.Size(41, 25)
        Me.cmdSSales.TabIndex = 5
        Me.cmdSSales.Text = "F7"
        Me.cmdSSales.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdSSales.UseVisualStyleBackColor = True
        '
        'cmdSCustomer
        '
        Me.cmdSCustomer.FlatAppearance.BorderColor = System.Drawing.Color.Lime
        Me.cmdSCustomer.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSCustomer.Image = CType(resources.GetObject("cmdSCustomer.Image"), System.Drawing.Image)
        Me.cmdSCustomer.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSCustomer.Location = New System.Drawing.Point(293, 48)
        Me.cmdSCustomer.Name = "cmdSCustomer"
        Me.cmdSCustomer.Size = New System.Drawing.Size(41, 25)
        Me.cmdSCustomer.TabIndex = 2
        Me.cmdSCustomer.Text = "F8"
        Me.cmdSCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdSCustomer.UseVisualStyleBackColor = True
        '
        'cmdSearch
        '
        Me.cmdSearch.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSearch.Image = CType(resources.GetObject("cmdSearch.Image"), System.Drawing.Image)
        Me.cmdSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSearch.Location = New System.Drawing.Point(293, 260)
        Me.cmdSearch.Name = "cmdSearch"
        Me.cmdSearch.Size = New System.Drawing.Size(41, 25)
        Me.cmdSearch.TabIndex = 9
        Me.cmdSearch.Text = "F1"
        Me.cmdSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdSearch.UseVisualStyleBackColor = True
        '
        'panelSelectItem
        '
        Me.panelSelectItem.Controls.Add(Me.gridSelectItem)
        Me.panelSelectItem.Controls.Add(Me.Label4)
        Me.panelSelectItem.Controls.Add(Me.GroupBox1)
        Me.panelSelectItem.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelSelectItem.Location = New System.Drawing.Point(0, 438)
        Me.panelSelectItem.Name = "panelSelectItem"
        Me.panelSelectItem.Padding = New System.Windows.Forms.Padding(10, 0, 10, 11)
        Me.panelSelectItem.Size = New System.Drawing.Size(1231, 220)
        Me.panelSelectItem.TabIndex = 19
        '
        'gridSelectItem
        '
        Me.gridSelectItem.AllowUserToAddRows = False
        Me.gridSelectItem.AllowUserToDeleteRows = False
        Me.gridSelectItem.AllowUserToOrderColumns = True
        DataGridViewCellStyle8.BackColor = System.Drawing.Color.Lavender
        Me.gridSelectItem.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle8
        Me.gridSelectItem.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        Me.gridSelectItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridSelectItem.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ItemCode, Me.ItemName, Me.QTY, Me.Price, Me.DISC, Me.TOTAL, Me.LessV, Me.VSales, Me.DiscP, Me.Cost, Me.Taxable, Me.ItemID, Me.FullPrice, Me.Description, Me.Extended, Me.DiscAmount, Me.OrderEntryID, Me.chkPickLoc, Me.CustPrep})
        DataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle12.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.LightBlue
        DataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridSelectItem.DefaultCellStyle = DataGridViewCellStyle12
        Me.gridSelectItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridSelectItem.Location = New System.Drawing.Point(10, 29)
        Me.gridSelectItem.Name = "gridSelectItem"
        Me.gridSelectItem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.gridSelectItem.Size = New System.Drawing.Size(1211, 180)
        Me.gridSelectItem.TabIndex = 15
        Me.gridSelectItem.TabStop = False
        '
        'ItemCode
        '
        Me.ItemCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ItemCode.HeaderText = "ITEM CODE"
        Me.ItemCode.Name = "ItemCode"
        Me.ItemCode.ReadOnly = True
        '
        'ItemName
        '
        Me.ItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ItemName.HeaderText = "ITEM NAME"
        Me.ItemName.Name = "ItemName"
        Me.ItemName.ReadOnly = True
        '
        'QTY
        '
        Me.QTY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight
        Me.QTY.DefaultCellStyle = DataGridViewCellStyle9
        Me.QTY.HeaderText = "QUANTITY"
        Me.QTY.Name = "QTY"
        Me.QTY.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'Price
        '
        Me.Price.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight
        DataGridViewCellStyle10.Format = "N2"
        DataGridViewCellStyle10.NullValue = Nothing
        Me.Price.DefaultCellStyle = DataGridViewCellStyle10
        Me.Price.HeaderText = "PRICE"
        Me.Price.Name = "Price"
        '
        'DISC
        '
        Me.DISC.HeaderText = "DISC"
        Me.DISC.Name = "DISC"
        Me.DISC.ReadOnly = True
        Me.DISC.Visible = False
        Me.DISC.Width = 70
        '
        'TOTAL
        '
        Me.TOTAL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle11.Format = "##0.00"
        DataGridViewCellStyle11.NullValue = Nothing
        Me.TOTAL.DefaultCellStyle = DataGridViewCellStyle11
        Me.TOTAL.HeaderText = "TOTAL PRICE"
        Me.TOTAL.Name = "TOTAL"
        Me.TOTAL.ReadOnly = True
        '
        'LessV
        '
        Me.LessV.HeaderText = "Less VAT"
        Me.LessV.Name = "LessV"
        Me.LessV.ReadOnly = True
        Me.LessV.Visible = False
        '
        'VSales
        '
        Me.VSales.HeaderText = "VAT Sales"
        Me.VSales.Name = "VSales"
        Me.VSales.ReadOnly = True
        Me.VSales.Visible = False
        '
        'DiscP
        '
        Me.DiscP.HeaderText = "Disc Price"
        Me.DiscP.Name = "DiscP"
        Me.DiscP.ReadOnly = True
        Me.DiscP.Visible = False
        '
        'Cost
        '
        Me.Cost.HeaderText = "Cost"
        Me.Cost.Name = "Cost"
        Me.Cost.Visible = False
        '
        'Taxable
        '
        Me.Taxable.HeaderText = "Taxable"
        Me.Taxable.Name = "Taxable"
        Me.Taxable.Visible = False
        '
        'ItemID
        '
        Me.ItemID.HeaderText = "ItemID"
        Me.ItemID.Name = "ItemID"
        Me.ItemID.Visible = False
        '
        'FullPrice
        '
        Me.FullPrice.HeaderText = "FullPrice"
        Me.FullPrice.Name = "FullPrice"
        Me.FullPrice.Visible = False
        '
        'Description
        '
        Me.Description.HeaderText = "Description"
        Me.Description.Name = "Description"
        Me.Description.Visible = False
        '
        'Extended
        '
        Me.Extended.HeaderText = "Extended"
        Me.Extended.Name = "Extended"
        Me.Extended.Visible = False
        '
        'DiscAmount
        '
        Me.DiscAmount.HeaderText = "DiscAmount"
        Me.DiscAmount.Name = "DiscAmount"
        Me.DiscAmount.Visible = False
        '
        'OrderEntryID
        '
        Me.OrderEntryID.HeaderText = "OrderEntryID"
        Me.OrderEntryID.Name = "OrderEntryID"
        Me.OrderEntryID.Visible = False
        '
        'chkPickLoc
        '
        Me.chkPickLoc.HeaderText = "UP-STORE"
        Me.chkPickLoc.MinimumWidth = 75
        Me.chkPickLoc.Name = "chkPickLoc"
        Me.chkPickLoc.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.chkPickLoc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.chkPickLoc.Width = 75
        '
        'CustPrep
        '
        Me.CustPrep.HeaderText = "Prepared"
        Me.CustPrep.Name = "CustPrep"
        Me.CustPrep.Width = 60
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(10, 11)
        Me.Label4.Name = "Label4"
        Me.Label4.Padding = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.Label4.Size = New System.Drawing.Size(96, 18)
        Me.Label4.TabIndex = 22
        Me.Label4.Text = "Selected Items:"
        '
        'GroupBox1
        '
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox1.Location = New System.Drawing.Point(10, 0)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(3, 0, 3, 3)
        Me.GroupBox1.Size = New System.Drawing.Size(1211, 11)
        Me.GroupBox1.TabIndex = 20
        Me.GroupBox1.TabStop = False
        '
        'Update_Timer
        '
        Me.Update_Timer.Interval = 15000
        '
        'Check_errors
        '
        Me.Check_errors.Enabled = True
        Me.Check_errors.Interval = 1000
        '
        'frmItemLookUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 14.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1350, 747)
        Me.Controls.Add(Me.panelGrid)
        Me.Controls.Add(Me.panelTotal)
        Me.Controls.Add(Me.panelButtons)
        Me.Controls.Add(Me.BottomStrip)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "frmItemLookUp"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Work Order Entry - V4"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.BottomStrip.ResumeLayout(False)
        Me.BottomStrip.PerformLayout()
        Me.panelButtons.ResumeLayout(False)
        Me.panelTotal.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.panelGrid.ResumeLayout(False)
        Me.panelItem.ResumeLayout(False)
        CType(Me.gridItem, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel4.ResumeLayout(False)
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panelSelectItem.ResumeLayout(False)
        Me.panelSelectItem.PerformLayout()
        CType(Me.gridSelectItem, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ofdImport As System.Windows.Forms.OpenFileDialog
    Friend WithEvents BottomStrip As System.Windows.Forms.StatusStrip
    Friend WithEvents statServer As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents statDb As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblStatItemSelected As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblStatItemCount As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents panelButtons As System.Windows.Forms.Panel
    Friend WithEvents cmdSelect As System.Windows.Forms.Button
    Friend WithEvents panelTotal As System.Windows.Forms.Panel
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents txtSub As System.Windows.Forms.TextBox
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents cmdRemove As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdPriceLevel As System.Windows.Forms.Button
    Friend WithEvents cmdInterStore As System.Windows.Forms.Button
    Friend WithEvents cmdRefresh As System.Windows.Forms.Button
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents txtTotal As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents txtVat As System.Windows.Forms.TextBox
    Friend WithEvents panelGrid As System.Windows.Forms.Panel
    Friend WithEvents panelSelectItem As System.Windows.Forms.Panel
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents panelItem As System.Windows.Forms.Panel
    Friend WithEvents gridSelectItem As System.Windows.Forms.DataGridView
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmdOk As System.Windows.Forms.Button
    Friend WithEvents cmdImport As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents cmdSSales As System.Windows.Forms.Button
    Friend WithEvents txtSales As System.Windows.Forms.TextBox
    Friend WithEvents cmdSCustomer As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtCustomer As System.Windows.Forms.TextBox
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtRemarks As System.Windows.Forms.TextBox
    Friend WithEvents lblRemarks As System.Windows.Forms.Label
    Friend WithEvents txtType As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmdSearch As System.Windows.Forms.Button
    Friend WithEvents Panel4 As System.Windows.Forms.Panel
    Friend WithEvents gridItem As System.Windows.Forms.DataGridView
    Friend WithEvents TreeView1 As System.Windows.Forms.TreeView
    Friend WithEvents Panel5 As System.Windows.Forms.Panel
    Friend WithEvents cmdSettings As System.Windows.Forms.Button
    Friend WithEvents cmdRecall As System.Windows.Forms.Button
    Friend WithEvents lblOrderNo As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents cboPayment As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents Update_Timer As System.Windows.Forms.Timer
    Friend WithEvents ToolStripStatusLabel3 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel2 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents cmdLogout As System.Windows.Forms.Button
    Friend WithEvents rbtnDelivery As System.Windows.Forms.RadioButton
    Friend WithEvents rbtnPickup As System.Windows.Forms.RadioButton
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cmdForInvoice As System.Windows.Forms.Button
    Friend WithEvents Check_errors As System.Windows.Forms.Timer
    Friend WithEvents ItemCode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents ItemName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents QTY As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Price As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DISC As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents TOTAL As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents LessV As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents VSales As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DiscP As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Cost As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Taxable As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents ItemID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents FullPrice As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Description As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Extended As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DiscAmount As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents OrderEntryID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents chkPickLoc As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents CustPrep As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents chkQuote As System.Windows.Forms.CheckBox
    Friend WithEvents chkWorkOrder As System.Windows.Forms.CheckBox
End Class
