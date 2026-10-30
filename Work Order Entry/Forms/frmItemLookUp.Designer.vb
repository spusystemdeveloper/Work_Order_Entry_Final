<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmItemLookUp
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim SelectedPriceStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim SelectedCostStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim SelectedDescriptionStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmItemLookUp))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle12 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle13 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle14 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle10 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle11 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle15 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle16 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle17 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle18 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle19 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle20 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle21 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle22 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle23 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
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
        Me.Button2 = New System.Windows.Forms.Button()
        Me.btnTax = New System.Windows.Forms.Button()
        Me.cmdOpenWO = New System.Windows.Forms.Button()
        Me.txtAvailable = New System.Windows.Forms.TextBox()
        Me.cmdImportWeb = New System.Windows.Forms.Button()
        Me.txtCreditLimit = New System.Windows.Forms.TextBox()
        Me.cmdForInvoice = New System.Windows.Forms.Button()
        Me.cmdImport = New System.Windows.Forms.Button()
        Me.txtOpenWO = New System.Windows.Forms.TextBox()
        Me.txtAR = New System.Windows.Forms.TextBox()
        Me.txtCustomerId = New System.Windows.Forms.TextBox()
        Me.txtPrice = New System.Windows.Forms.TextBox()
        Me.txtPriceLevel = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.cmdOk = New System.Windows.Forms.Button()
        Me.cmdSettings = New System.Windows.Forms.Button()
        Me.cmdRecall = New System.Windows.Forms.Button()
        Me.cmdPriceLevel = New System.Windows.Forms.Button()
        Me.cmdInterStore = New System.Windows.Forms.Button()
        Me.cmdRefresh = New System.Windows.Forms.Button()
        Me.cmdRemove = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.cmdSelect = New System.Windows.Forms.Button()
        Me.txtBarcode = New System.Windows.Forms.TextBox()
        Me.chkBarcode = New System.Windows.Forms.CheckBox()
        Me.panelTotal = New System.Windows.Forms.Panel()
        Me.grpWithholding = New System.Windows.Forms.GroupBox()
        Me.withholdingLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.lblWithholding1 = New System.Windows.Forms.Label()
        Me.lblWithholding2 = New System.Windows.Forms.Label()
        Me.lblWithholding5 = New System.Windows.Forms.Label()
        Me.lblWithholding15 = New System.Windows.Forms.Label()
        Me.lblWithholdingBasis = New System.Windows.Forms.Label()
        Me.chkBoxQtoWo = New System.Windows.Forms.CheckBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.txtVat = New System.Windows.Forms.TextBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.txtSub = New System.Windows.Forms.TextBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.txtTotal = New System.Windows.Forms.TextBox()
        Me.withholdingToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.panelTotalsContainer = New System.Windows.Forms.Panel()
        Me.panelGrid = New System.Windows.Forms.Panel()
        Me.panelItem = New System.Windows.Forms.Panel()
        Me.picPanel = New WorkOrderEntry.CustomPanel()
        Me.picItem = New WorkOrderEntry.ZoomPictureBox()
        Me.gridItem = New System.Windows.Forms.DataGridView()
        Me.ItemLookupCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupDESCRIPTION = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupPRICE = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupCOST = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.AVAILABLE = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ParentQuantity = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TaxID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.SubDescription2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupPRICE1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DESCRIPTION1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ExtendedDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Inactive = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.ITEMDES = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.SKULEVEL = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FULLDESC = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Quantity = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.QuantityCommitted = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.chkWebsite = New System.Windows.Forms.CheckBox()
        Me.chkQuote = New System.Windows.Forms.CheckBox()
        Me.chkShowImage = New System.Windows.Forms.CheckBox()
        Me.chkWorkOrder = New System.Windows.Forms.CheckBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.rbtnDelivery = New System.Windows.Forms.RadioButton()
        Me.rbtnPickup = New System.Windows.Forms.RadioButton()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.cboPayment = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.cmdSSales = New System.Windows.Forms.Button()
        Me.cmdSCustomer = New System.Windows.Forms.Button()
        Me.cmdSearch = New System.Windows.Forms.Button()
        Me.pnl1Barcode = New System.Windows.Forms.Panel()
        Me.pnl2TextSearch = New System.Windows.Forms.Panel()
        Me.panelSelectItem = New System.Windows.Forms.Panel()
        Me.gridSelectItem = New System.Windows.Forms.DataGridView()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Price = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Cost = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Description = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.QTY = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DISC = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TOTAL = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.LessV = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.VSales = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DiscP = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Taxable = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ItemID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.FullPrice = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Extended = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DiscAmount = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.OrderEntryID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.chkPickLoc = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.CustPrep = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.LASTPURCHASEDPRICE = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Update_Timer = New System.Windows.Forms.Timer(Me.components)
        Me.Check_errors = New System.Windows.Forms.Timer(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.BottomStrip.SuspendLayout()
        Me.panelButtons.SuspendLayout()
        Me.panelTotal.SuspendLayout()
        Me.grpWithholding.SuspendLayout()
        Me.withholdingLayout.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.panelTotalsContainer.SuspendLayout()
        Me.panelGrid.SuspendLayout()
        Me.panelItem.SuspendLayout()
        Me.picPanel.SuspendLayout()
        CType(Me.gridItem, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel4.SuspendLayout()
        Me.Panel5.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.txtRemarks.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtRemarks.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtRemarks.Location = New System.Drawing.Point(8, 167)
        Me.txtRemarks.Name = "txtRemarks"
        Me.txtRemarks.Size = New System.Drawing.Size(326, 22)
        Me.txtRemarks.TabIndex = 6
        '
        'lblRemarks
        '
        Me.lblRemarks.AutoSize = True
        Me.lblRemarks.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.lblRemarks.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.lblRemarks.Location = New System.Drawing.Point(8, 146)
        Me.lblRemarks.Name = "lblRemarks"
        Me.lblRemarks.Size = New System.Drawing.Size(125, 13)
        Me.lblRemarks.TabIndex = 61
        Me.lblRemarks.Text = "Comment (e.g. PO#):"
        '
        'txtType
        '
        Me.txtType.BackColor = System.Drawing.SystemColors.Control
        Me.txtType.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtType.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtType.Location = New System.Drawing.Point(8, 86)
        Me.txtType.Name = "txtType"
        Me.txtType.ReadOnly = True
        Me.txtType.Size = New System.Drawing.Size(208, 22)
        Me.txtType.TabIndex = 3
        Me.txtType.TabStop = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label5.Location = New System.Drawing.Point(8, 65)
        Me.Label5.Name = "Label5"
        Me.Label5.Padding = New System.Windows.Forms.Padding(0, 0, 27, 0)
        Me.Label5.Size = New System.Drawing.Size(178, 13)
        Me.Label5.TabIndex = 59
        Me.Label5.Text = "Business/Customer Type:"
        '
        'txtCustomer
        '
        Me.txtCustomer.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtCustomer.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtCustomer.Location = New System.Drawing.Point(8, 46)
        Me.txtCustomer.Name = "txtCustomer"
        Me.txtCustomer.ReadOnly = True
        Me.txtCustomer.Size = New System.Drawing.Size(279, 22)
        Me.txtCustomer.TabIndex = 1
        Me.txtCustomer.TabStop = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label3.Location = New System.Drawing.Point(8, 26)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 13)
        Me.Label3.TabIndex = 61
        Me.Label3.Text = "Customer:"
        '
        'txtSales
        '
        Me.txtSales.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtSales.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSales.Location = New System.Drawing.Point(8, 127)
        Me.txtSales.Name = "txtSales"
        Me.txtSales.ReadOnly = True
        Me.txtSales.Size = New System.Drawing.Size(279, 22)
        Me.txtSales.TabIndex = 4
        Me.txtSales.TabStop = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label2.Location = New System.Drawing.Point(8, 106)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(65, 13)
        Me.Label2.TabIndex = 54
        Me.Label2.Text = "Sales Rep:"
        '
        'txtSearch
        '
        Me.txtSearch.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtSearch.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSearch.Location = New System.Drawing.Point(8, 238)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(279, 22)
        Me.txtSearch.TabIndex = 8
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(8, 216)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(190, 13)
        Me.Label1.TabIndex = 63
        Me.Label1.Text = "Look Up Item Code/Description: "
        '
        'BottomStrip
        '
        Me.BottomStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.BottomStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statServer, Me.statDb, Me.ToolStripStatusLabel1, Me.ToolStripStatusLabel3, Me.ToolStripStatusLabel2, Me.lblOrderNo, Me.lblStatItemSelected, Me.lblStatItemCount})
        Me.BottomStrip.Location = New System.Drawing.Point(0, 763)
        Me.BottomStrip.Name = "BottomStrip"
        Me.BottomStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode
        Me.BottomStrip.Size = New System.Drawing.Size(1323, 26)
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
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(689, 21)
        Me.ToolStripStatusLabel1.Spring = True
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
        Me.panelButtons.Controls.Add(Me.Button2)
        Me.panelButtons.Controls.Add(Me.btnTax)
        Me.panelButtons.Controls.Add(Me.cmdOpenWO)
        Me.panelButtons.Controls.Add(Me.txtAvailable)
        Me.panelButtons.Controls.Add(Me.cmdImportWeb)
        Me.panelButtons.Controls.Add(Me.txtCreditLimit)
        Me.panelButtons.Controls.Add(Me.cmdForInvoice)
        Me.panelButtons.Controls.Add(Me.cmdImport)
        Me.panelButtons.Controls.Add(Me.txtOpenWO)
        Me.panelButtons.Controls.Add(Me.txtAR)
        Me.panelButtons.Controls.Add(Me.txtCustomerId)
        Me.panelButtons.Controls.Add(Me.txtPrice)
        Me.panelButtons.Controls.Add(Me.txtPriceLevel)
        Me.panelButtons.Controls.Add(Me.Button1)
        Me.panelButtons.Controls.Add(Me.cmdOk)
        Me.panelButtons.Controls.Add(Me.cmdSettings)
        Me.panelButtons.Controls.Add(Me.cmdRecall)
        Me.panelButtons.Controls.Add(Me.cmdPriceLevel)
        Me.panelButtons.Controls.Add(Me.cmdInterStore)
        Me.panelButtons.Controls.Add(Me.cmdRefresh)
        Me.panelButtons.Controls.Add(Me.cmdRemove)
        Me.panelButtons.Controls.Add(Me.cmdCancel)
        Me.panelButtons.Controls.Add(Me.cmdClose)
        Me.panelButtons.Controls.Add(Me.cmdSelect)
        Me.panelButtons.Dock = System.Windows.Forms.DockStyle.Right
        Me.panelButtons.Location = New System.Drawing.Point(1179, 0)
        Me.panelButtons.Name = "panelButtons"
        Me.panelButtons.Padding = New System.Windows.Forms.Padding(0, 3, 3, 3)
        Me.panelButtons.Size = New System.Drawing.Size(144, 763)
        Me.panelButtons.TabIndex = 23
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.White
        Me.Button2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button2.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.Button2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Button2.Image = CType(resources.GetObject("Button2.Image"), System.Drawing.Image)
        Me.Button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button2.Location = New System.Drawing.Point(0, 234)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(141, 33)
        Me.Button2.TabIndex = 16
        Me.Button2.Text = "Logout"
        Me.Button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Button2.UseVisualStyleBackColor = False
        '
        'btnTax
        '
        Me.btnTax.BackColor = System.Drawing.Color.White
        Me.btnTax.Dock = System.Windows.Forms.DockStyle.Top
        Me.btnTax.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.btnTax.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnTax.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.btnTax.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.btnTax.Image = CType(resources.GetObject("btnTax.Image"), System.Drawing.Image)
        Me.btnTax.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnTax.Location = New System.Drawing.Point(0, 201)
        Me.btnTax.Name = "btnTax"
        Me.btnTax.Size = New System.Drawing.Size(141, 33)
        Me.btnTax.TabIndex = 35
        Me.btnTax.Text = "Set Tax"
        Me.btnTax.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.btnTax.UseVisualStyleBackColor = False
        '
        'cmdOpenWO
        '
        Me.cmdOpenWO.BackColor = System.Drawing.Color.White
        Me.cmdOpenWO.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdOpenWO.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdOpenWO.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdOpenWO.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdOpenWO.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdOpenWO.Image = CType(resources.GetObject("cmdOpenWO.Image"), System.Drawing.Image)
        Me.cmdOpenWO.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdOpenWO.Location = New System.Drawing.Point(0, 168)
        Me.cmdOpenWO.Name = "cmdOpenWO"
        Me.cmdOpenWO.Size = New System.Drawing.Size(141, 33)
        Me.cmdOpenWO.TabIndex = 15
        Me.cmdOpenWO.Text = "Check WO"
        Me.cmdOpenWO.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdOpenWO.UseVisualStyleBackColor = False
        '
        'txtAvailable
        '
        Me.txtAvailable.Location = New System.Drawing.Point(6, 428)
        Me.txtAvailable.Name = "txtAvailable"
        Me.txtAvailable.Size = New System.Drawing.Size(108, 21)
        Me.txtAvailable.TabIndex = 34
        Me.txtAvailable.Visible = False
        '
        'cmdImportWeb
        '
        Me.cmdImportWeb.BackColor = System.Drawing.Color.White
        Me.cmdImportWeb.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdImportWeb.Enabled = False
        Me.cmdImportWeb.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdImportWeb.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdImportWeb.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdImportWeb.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdImportWeb.Image = CType(resources.GetObject("cmdImportWeb.Image"), System.Drawing.Image)
        Me.cmdImportWeb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdImportWeb.Location = New System.Drawing.Point(0, 462)
        Me.cmdImportWeb.Name = "cmdImportWeb"
        Me.cmdImportWeb.Size = New System.Drawing.Size(141, 33)
        Me.cmdImportWeb.TabIndex = 25
        Me.cmdImportWeb.Text = "Import Web"
        Me.cmdImportWeb.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdImportWeb.UseVisualStyleBackColor = False
        Me.cmdImportWeb.Visible = False
        '
        'txtCreditLimit
        '
        Me.txtCreditLimit.Location = New System.Drawing.Point(3, 401)
        Me.txtCreditLimit.Name = "txtCreditLimit"
        Me.txtCreditLimit.Size = New System.Drawing.Size(108, 21)
        Me.txtCreditLimit.TabIndex = 33
        Me.txtCreditLimit.Visible = False
        '
        'cmdForInvoice
        '
        Me.cmdForInvoice.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.cmdForInvoice.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdForInvoice.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdForInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdForInvoice.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdForInvoice.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdForInvoice.Image = Global.WorkOrderEntry.My.Resources.Resources.icons8_invoice_15
        Me.cmdForInvoice.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdForInvoice.Location = New System.Drawing.Point(0, 495)
        Me.cmdForInvoice.Name = "cmdForInvoice"
        Me.cmdForInvoice.Size = New System.Drawing.Size(141, 33)
        Me.cmdForInvoice.TabIndex = 18
        Me.cmdForInvoice.Text = "For Invoice"
        Me.cmdForInvoice.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdForInvoice.UseVisualStyleBackColor = False
        Me.cmdForInvoice.Visible = False
        '
        'cmdImport
        '
        Me.cmdImport.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.cmdImport.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdImport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdImport.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdImport.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdImport.Image = CType(resources.GetObject("cmdImport.Image"), System.Drawing.Image)
        Me.cmdImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdImport.Location = New System.Drawing.Point(0, 528)
        Me.cmdImport.Name = "cmdImport"
        Me.cmdImport.Size = New System.Drawing.Size(141, 34)
        Me.cmdImport.TabIndex = 15
        Me.cmdImport.Text = "Import [F6]"
        Me.cmdImport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdImport.UseVisualStyleBackColor = False
        Me.cmdImport.Visible = False
        '
        'txtOpenWO
        '
        Me.txtOpenWO.Location = New System.Drawing.Point(4, 377)
        Me.txtOpenWO.Name = "txtOpenWO"
        Me.txtOpenWO.Size = New System.Drawing.Size(108, 21)
        Me.txtOpenWO.TabIndex = 32
        Me.txtOpenWO.Visible = False
        '
        'txtAR
        '
        Me.txtAR.Location = New System.Drawing.Point(6, 350)
        Me.txtAR.Name = "txtAR"
        Me.txtAR.Size = New System.Drawing.Size(108, 21)
        Me.txtAR.TabIndex = 31
        Me.txtAR.Visible = False
        '
        'txtCustomerId
        '
        Me.txtCustomerId.Location = New System.Drawing.Point(6, 323)
        Me.txtCustomerId.Name = "txtCustomerId"
        Me.txtCustomerId.Size = New System.Drawing.Size(108, 21)
        Me.txtCustomerId.TabIndex = 30
        Me.txtCustomerId.Visible = False
        '
        'txtPrice
        '
        Me.txtPrice.Location = New System.Drawing.Point(4, 296)
        Me.txtPrice.Name = "txtPrice"
        Me.txtPrice.Size = New System.Drawing.Size(107, 21)
        Me.txtPrice.TabIndex = 29
        Me.txtPrice.Visible = False
        '
        'txtPriceLevel
        '
        Me.txtPriceLevel.Enabled = False
        Me.txtPriceLevel.Location = New System.Drawing.Point(3, 269)
        Me.txtPriceLevel.Name = "txtPriceLevel"
        Me.txtPriceLevel.Size = New System.Drawing.Size(107, 21)
        Me.txtPriceLevel.TabIndex = 28
        Me.txtPriceLevel.Visible = False
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.Button1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.Button1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Button1.Image = Global.WorkOrderEntry.My.Resources.Resources.icons8_print_15
        Me.Button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button1.Location = New System.Drawing.Point(0, 562)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(141, 33)
        Me.Button1.TabIndex = 17
        Me.Button1.Text = "Print"
        Me.Button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.Button1.UseVisualStyleBackColor = False
        '
        'cmdOk
        '
        Me.cmdOk.BackColor = System.Drawing.Color.FromArgb(CType(CType(37, Byte), Integer), CType(CType(99, Byte), Integer), CType(CType(160, Byte), Integer))
        Me.cmdOk.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdOk.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdOk.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.cmdOk.ForeColor = System.Drawing.Color.White
        Me.cmdOk.Image = CType(resources.GetObject("cmdOk.Image"), System.Drawing.Image)
        Me.cmdOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdOk.Location = New System.Drawing.Point(0, 595)
        Me.cmdOk.Name = "cmdOk"
        Me.cmdOk.Size = New System.Drawing.Size(141, 33)
        Me.cmdOk.TabIndex = 20
        Me.cmdOk.Text = "Save [F12]"
        Me.cmdOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdOk.UseVisualStyleBackColor = False
        '
        'cmdSettings
        '
        Me.cmdSettings.BackColor = System.Drawing.Color.White
        Me.cmdSettings.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdSettings.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdSettings.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdSettings.Image = CType(resources.GetObject("cmdSettings.Image"), System.Drawing.Image)
        Me.cmdSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSettings.Location = New System.Drawing.Point(0, 135)
        Me.cmdSettings.Name = "cmdSettings"
        Me.cmdSettings.Size = New System.Drawing.Size(141, 33)
        Me.cmdSettings.TabIndex = 14
        Me.cmdSettings.Text = "Settings"
        Me.cmdSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdSettings.UseVisualStyleBackColor = False
        '
        'cmdRecall
        '
        Me.cmdRecall.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(253, Byte), Integer))
        Me.cmdRecall.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdRecall.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdRecall.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdRecall.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.cmdRecall.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdRecall.Image = CType(resources.GetObject("cmdRecall.Image"), System.Drawing.Image)
        Me.cmdRecall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRecall.Location = New System.Drawing.Point(0, 628)
        Me.cmdRecall.Name = "cmdRecall"
        Me.cmdRecall.Size = New System.Drawing.Size(141, 33)
        Me.cmdRecall.TabIndex = 19
        Me.cmdRecall.Text = "Recall [F11]"
        Me.cmdRecall.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdRecall.UseVisualStyleBackColor = False
        '
        'cmdPriceLevel
        '
        Me.cmdPriceLevel.BackColor = System.Drawing.Color.White
        Me.cmdPriceLevel.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdPriceLevel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdPriceLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPriceLevel.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdPriceLevel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdPriceLevel.Image = CType(resources.GetObject("cmdPriceLevel.Image"), System.Drawing.Image)
        Me.cmdPriceLevel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdPriceLevel.Location = New System.Drawing.Point(0, 102)
        Me.cmdPriceLevel.Name = "cmdPriceLevel"
        Me.cmdPriceLevel.Size = New System.Drawing.Size(141, 33)
        Me.cmdPriceLevel.TabIndex = 13
        Me.cmdPriceLevel.Text = "Price Level"
        Me.cmdPriceLevel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdPriceLevel.UseVisualStyleBackColor = False
        '
        'cmdInterStore
        '
        Me.cmdInterStore.BackColor = System.Drawing.Color.White
        Me.cmdInterStore.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdInterStore.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdInterStore.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdInterStore.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdInterStore.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdInterStore.Image = CType(resources.GetObject("cmdInterStore.Image"), System.Drawing.Image)
        Me.cmdInterStore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdInterStore.Location = New System.Drawing.Point(0, 69)
        Me.cmdInterStore.Name = "cmdInterStore"
        Me.cmdInterStore.Size = New System.Drawing.Size(141, 33)
        Me.cmdInterStore.TabIndex = 12
        Me.cmdInterStore.Text = "InterStore"
        Me.cmdInterStore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdInterStore.UseVisualStyleBackColor = False
        '
        'cmdRefresh
        '
        Me.cmdRefresh.BackColor = System.Drawing.Color.White
        Me.cmdRefresh.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdRefresh.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdRefresh.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdRefresh.Image = CType(resources.GetObject("cmdRefresh.Image"), System.Drawing.Image)
        Me.cmdRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRefresh.Location = New System.Drawing.Point(0, 36)
        Me.cmdRefresh.Name = "cmdRefresh"
        Me.cmdRefresh.Size = New System.Drawing.Size(141, 33)
        Me.cmdRefresh.TabIndex = 11
        Me.cmdRefresh.Text = "Refresh"
        Me.cmdRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdRefresh.UseVisualStyleBackColor = False
        '
        'cmdRemove
        '
        Me.cmdRemove.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(242, Byte), Integer))
        Me.cmdRemove.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdRemove.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdRemove.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdRemove.ForeColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.cmdRemove.Image = CType(resources.GetObject("cmdRemove.Image"), System.Drawing.Image)
        Me.cmdRemove.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdRemove.Location = New System.Drawing.Point(0, 661)
        Me.cmdRemove.Name = "cmdRemove"
        Me.cmdRemove.Size = New System.Drawing.Size(141, 33)
        Me.cmdRemove.TabIndex = 21
        Me.cmdRemove.Text = "Remove [Del]"
        Me.cmdRemove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdRemove.UseVisualStyleBackColor = False
        '
        'cmdCancel
        '
        Me.cmdCancel.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(242, Byte), Integer))
        Me.cmdCancel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdCancel.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdCancel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(150, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.cmdCancel.Image = CType(resources.GetObject("cmdCancel.Image"), System.Drawing.Image)
        Me.cmdCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdCancel.Location = New System.Drawing.Point(0, 694)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(141, 33)
        Me.cmdCancel.TabIndex = 22
        Me.cmdCancel.Text = "Cancel [F9]"
        Me.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdClose
        '
        Me.cmdClose.BackColor = System.Drawing.Color.White
        Me.cmdClose.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.cmdClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdClose.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdClose.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdClose.Image = CType(resources.GetObject("cmdClose.Image"), System.Drawing.Image)
        Me.cmdClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdClose.Location = New System.Drawing.Point(0, 727)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(141, 33)
        Me.cmdClose.TabIndex = 23
        Me.cmdClose.Text = "Close [Esc]"
        Me.cmdClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdClose.UseVisualStyleBackColor = False
        '
        'cmdSelect
        '
        Me.cmdSelect.BackColor = System.Drawing.Color.White
        Me.cmdSelect.Dock = System.Windows.Forms.DockStyle.Top
        Me.cmdSelect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(210, Byte), Integer), CType(CType(218, Byte), Integer), CType(CType(228, Byte), Integer))
        Me.cmdSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdSelect.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cmdSelect.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.cmdSelect.Image = CType(resources.GetObject("cmdSelect.Image"), System.Drawing.Image)
        Me.cmdSelect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdSelect.Location = New System.Drawing.Point(0, 3)
        Me.cmdSelect.Name = "cmdSelect"
        Me.cmdSelect.Size = New System.Drawing.Size(141, 33)
        Me.cmdSelect.TabIndex = 10
        Me.cmdSelect.Text = "Select"
        Me.cmdSelect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.cmdSelect.UseVisualStyleBackColor = False
        '
        'txtBarcode
        '
        Me.txtBarcode.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtBarcode.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.txtBarcode.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtBarcode.Location = New System.Drawing.Point(8, 268)
        Me.txtBarcode.Name = "txtBarcode"
        Me.txtBarcode.Size = New System.Drawing.Size(219, 22)
        Me.txtBarcode.TabIndex = 27
        '
        'chkBarcode
        '
        Me.chkBarcode.AutoSize = True
        Me.chkBarcode.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkBarcode.Location = New System.Drawing.Point(236, 272)
        Me.chkBarcode.Name = "chkBarcode"
        Me.chkBarcode.Size = New System.Drawing.Size(98, 17)
        Me.chkBarcode.TabIndex = 26
        Me.chkBarcode.Text = "Barcode [F3]"
        Me.chkBarcode.UseVisualStyleBackColor = True
        '
        'panelTotal
        '
        Me.panelTotal.BackColor = System.Drawing.SystemColors.Control
        Me.panelTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panelTotal.Controls.Add(Me.grpWithholding)
        Me.panelTotal.Controls.Add(Me.chkBoxQtoWo)
        Me.panelTotal.Controls.Add(Me.GroupBox3)
        Me.panelTotal.Controls.Add(Me.GroupBox2)
        Me.panelTotal.Controls.Add(Me.GroupBox4)
        Me.panelTotal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelTotal.Location = New System.Drawing.Point(10, 0)
        Me.panelTotal.Name = "panelTotal"
        Me.panelTotal.Padding = New System.Windows.Forms.Padding(5)
        Me.panelTotal.Size = New System.Drawing.Size(1168, 171)
        Me.panelTotal.TabIndex = 24
        '
        'grpWithholding
        '
        Me.grpWithholding.BackColor = System.Drawing.Color.AliceBlue
        Me.grpWithholding.Controls.Add(Me.withholdingLayout)
        Me.grpWithholding.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold)
        Me.grpWithholding.ForeColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(88, Byte), Integer))
        Me.grpWithholding.Location = New System.Drawing.Point(5, 3)
        Me.grpWithholding.Name = "grpWithholding"
        Me.grpWithholding.Size = New System.Drawing.Size(1174, 86)
        Me.grpWithholding.TabIndex = 25
        Me.grpWithholding.TabStop = False
        Me.grpWithholding.Text = "Suggested W/Tax"
        '
        'withholdingLayout
        '
        Me.withholdingLayout.ColumnCount = 4
        Me.withholdingLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.withholdingLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.withholdingLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.withholdingLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.withholdingLayout.Controls.Add(Me.lblWithholding1, 0, 0)
        Me.withholdingLayout.Controls.Add(Me.lblWithholding2, 1, 0)
        Me.withholdingLayout.Controls.Add(Me.lblWithholding5, 2, 0)
        Me.withholdingLayout.Controls.Add(Me.lblWithholding15, 3, 0)
        Me.withholdingLayout.Controls.Add(Me.lblWithholdingBasis, 0, 1)
        Me.withholdingLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.withholdingLayout.Location = New System.Drawing.Point(3, 19)
        Me.withholdingLayout.Name = "withholdingLayout"
        Me.withholdingLayout.RowCount = 2
        Me.withholdingLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.0!))
        Me.withholdingLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.0!))
        Me.withholdingLayout.Size = New System.Drawing.Size(1168, 64)
        Me.withholdingLayout.TabIndex = 0
        '
        'lblWithholding1
        '
        Me.lblWithholding1.AutoEllipsis = True
        Me.lblWithholding1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblWithholding1.Font = New System.Drawing.Font("Tahoma", 13.5!, System.Drawing.FontStyle.Bold)
        Me.lblWithholding1.Location = New System.Drawing.Point(3, 0)
        Me.lblWithholding1.Name = "lblWithholding1"
        Me.lblWithholding1.Padding = New System.Windows.Forms.Padding(6, 0, 0, 0)
        Me.lblWithholding1.Size = New System.Drawing.Size(286, 39)
        Me.lblWithholding1.TabIndex = 0
        Me.lblWithholding1.Text = "1%: 0.00"
        Me.lblWithholding1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblWithholding2
        '
        Me.lblWithholding2.AutoEllipsis = True
        Me.lblWithholding2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblWithholding2.Font = New System.Drawing.Font("Tahoma", 13.5!, System.Drawing.FontStyle.Bold)
        Me.lblWithholding2.Location = New System.Drawing.Point(295, 0)
        Me.lblWithholding2.Name = "lblWithholding2"
        Me.lblWithholding2.Padding = New System.Windows.Forms.Padding(6, 0, 0, 0)
        Me.lblWithholding2.Size = New System.Drawing.Size(286, 39)
        Me.lblWithholding2.TabIndex = 1
        Me.lblWithholding2.Text = "2%: 0.00"
        Me.lblWithholding2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblWithholding5
        '
        Me.lblWithholding5.AutoEllipsis = True
        Me.lblWithholding5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblWithholding5.Font = New System.Drawing.Font("Tahoma", 13.5!, System.Drawing.FontStyle.Bold)
        Me.lblWithholding5.Location = New System.Drawing.Point(587, 0)
        Me.lblWithholding5.Name = "lblWithholding5"
        Me.lblWithholding5.Padding = New System.Windows.Forms.Padding(6, 0, 0, 0)
        Me.lblWithholding5.Size = New System.Drawing.Size(286, 39)
        Me.lblWithholding5.TabIndex = 2
        Me.lblWithholding5.Text = "5%: 0.00"
        Me.lblWithholding5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblWithholding15
        '
        Me.lblWithholding15.AutoEllipsis = True
        Me.lblWithholding15.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblWithholding15.Font = New System.Drawing.Font("Tahoma", 13.5!, System.Drawing.FontStyle.Bold)
        Me.lblWithholding15.Location = New System.Drawing.Point(879, 0)
        Me.lblWithholding15.Name = "lblWithholding15"
        Me.lblWithholding15.Padding = New System.Windows.Forms.Padding(6, 0, 0, 0)
        Me.lblWithholding15.Size = New System.Drawing.Size(286, 39)
        Me.lblWithholding15.TabIndex = 3
        Me.lblWithholding15.Text = "15%: 0.00"
        Me.lblWithholding15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblWithholdingBasis
        '
        Me.lblWithholdingBasis.AutoEllipsis = True
        Me.withholdingLayout.SetColumnSpan(Me.lblWithholdingBasis, 4)
        Me.lblWithholdingBasis.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblWithholdingBasis.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.lblWithholdingBasis.ForeColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.lblWithholdingBasis.Location = New System.Drawing.Point(3, 39)
        Me.lblWithholdingBasis.Name = "lblWithholdingBasis"
        Me.lblWithholdingBasis.Padding = New System.Windows.Forms.Padding(6, 0, 0, 0)
        Me.lblWithholdingBasis.Size = New System.Drawing.Size(1162, 25)
        Me.lblWithholdingBasis.TabIndex = 4
        Me.lblWithholdingBasis.Text = "VAT-exclusive: 0.00"
        Me.lblWithholdingBasis.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'chkBoxQtoWo
        '
        Me.chkBoxQtoWo.AutoSize = True
        Me.chkBoxQtoWo.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkBoxQtoWo.Location = New System.Drawing.Point(8, 8)
        Me.chkBoxQtoWo.Name = "chkBoxQtoWo"
        Me.chkBoxQtoWo.Size = New System.Drawing.Size(168, 17)
        Me.chkBoxQtoWo.TabIndex = 18
        Me.chkBoxQtoWo.Text = "IsQuoteConvertedToWo?"
        Me.chkBoxQtoWo.UseVisualStyleBackColor = True
        Me.chkBoxQtoWo.Visible = False
        '
        'GroupBox3
        '
        Me.GroupBox3.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.GroupBox3.Controls.Add(Me.txtVat)
        Me.GroupBox3.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold)
        Me.GroupBox3.ForeColor = System.Drawing.Color.Black
        Me.GroupBox3.Location = New System.Drawing.Point(567, 101)
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
        Me.GroupBox2.Location = New System.Drawing.Point(282, 101)
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
        Me.GroupBox4.Location = New System.Drawing.Point(849, 101)
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
        'panelTotalsContainer
        '
        Me.panelTotalsContainer.Controls.Add(Me.panelTotal)
        Me.panelTotalsContainer.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelTotalsContainer.Location = New System.Drawing.Point(0, 592)
        Me.panelTotalsContainer.Name = "panelTotalsContainer"
        Me.panelTotalsContainer.Padding = New System.Windows.Forms.Padding(10, 0, 1, 0)
        Me.panelTotalsContainer.Size = New System.Drawing.Size(1179, 171)
        Me.panelTotalsContainer.TabIndex = 24
        '
        'panelGrid
        '
        Me.panelGrid.Controls.Add(Me.panelItem)
        Me.panelGrid.Controls.Add(Me.panelSelectItem)
        Me.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelGrid.Location = New System.Drawing.Point(0, 0)
        Me.panelGrid.Name = "panelGrid"
        Me.panelGrid.Size = New System.Drawing.Size(1179, 592)
        Me.panelGrid.TabIndex = 25
        '
        'panelItem
        '
        Me.panelItem.Controls.Add(Me.picPanel)
        Me.panelItem.Controls.Add(Me.gridItem)
        Me.panelItem.Controls.Add(Me.Panel4)
        Me.panelItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelItem.Location = New System.Drawing.Point(0, 0)
        Me.panelItem.Name = "panelItem"
        Me.panelItem.Padding = New System.Windows.Forms.Padding(10, 0, 1, 0)
        Me.panelItem.Size = New System.Drawing.Size(1179, 372)
        Me.panelItem.TabIndex = 18
        '
        'picPanel
        '
        Me.picPanel.BackColor = System.Drawing.Color.LightSteelBlue
        Me.picPanel.Controls.Add(Me.picItem)
        Me.picPanel.Cursor = System.Windows.Forms.Cursors.SizeAll
        Me.picPanel.Location = New System.Drawing.Point(358, 182)
        Me.picPanel.Name = "picPanel"
        Me.picPanel.Padding = New System.Windows.Forms.Padding(10)
        Me.picPanel.Size = New System.Drawing.Size(250, 250)
        Me.picPanel.TabIndex = 19
        Me.picPanel.Visible = False
        '
        'picItem
        '
        Me.picItem.BackColor = System.Drawing.Color.White
        Me.picItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.picItem.EnableMouseDragging = True
        Me.picItem.EnableMouseWheelZooming = True
        Me.picItem.Image = Nothing
        Me.picItem.ImagePosition = New System.Drawing.Point(0, 0)
        Me.picItem.Location = New System.Drawing.Point(10, 10)
        Me.picItem.MaximumZoomFactor = 64.0R
        Me.picItem.MinimumImageHeight = 10
        Me.picItem.MinimumImageWidth = 10
        Me.picItem.MouseWheelDivisor = 500
        Me.picItem.Name = "picItem"
        Me.picItem.Size = New System.Drawing.Size(230, 230)
        Me.picItem.TabIndex = 0
        Me.picItem.ZoomFactor = 1.15R
        '
        'gridItem
        '
        Me.gridItem.AllowUserToAddRows = False
        Me.gridItem.AllowUserToDeleteRows = False
        Me.gridItem.AllowUserToOrderColumns = True
        Me.gridItem.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(246, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.gridItem.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.gridItem.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(232, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(244, Byte), Integer))
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle2.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(232, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(244, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridItem.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.gridItem.ColumnHeadersHeight = 34
        Me.gridItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridItem.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ItemLookupCode, Me.lookupDESCRIPTION, Me.lookupPRICE, Me.lookupCOST, Me.AVAILABLE, Me.ParentQuantity, Me.TaxID, Me.SubDescription2, Me.lookupPRICE1, Me.ID, Me.DESCRIPTION1, Me.ExtendedDescription, Me.ItemType, Me.Inactive, Me.ITEMDES, Me.SKULEVEL, Me.FULLDESC, Me.Quantity, Me.QuantityCommitted})
        DataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle12.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle12.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle12.Padding = New System.Windows.Forms.Padding(6, 3, 6, 3)
        DataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(37, Byte), Integer), CType(CType(99, Byte), Integer), CType(CType(160, Byte), Integer))
        DataGridViewCellStyle12.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridItem.DefaultCellStyle = DataGridViewCellStyle12
        Me.gridItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridItem.EnableHeadersVisualStyles = False
        Me.gridItem.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.gridItem.GridColor = System.Drawing.Color.FromArgb(CType(CType(218, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.gridItem.Location = New System.Drawing.Point(352, 0)
        Me.gridItem.MultiSelect = False
        Me.gridItem.Name = "gridItem"
        Me.gridItem.ReadOnly = True
        DataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle13.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridItem.RowHeadersDefaultCellStyle = DataGridViewCellStyle13
        Me.gridItem.RowHeadersWidth = 51
        DataGridViewCellStyle14.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.gridItem.RowsDefaultCellStyle = DataGridViewCellStyle14
        Me.gridItem.RowTemplate.DefaultCellStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.gridItem.RowTemplate.Height = 44
        Me.gridItem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridItem.Size = New System.Drawing.Size(826, 372)
        Me.gridItem.TabIndex = 0
        Me.gridItem.TabStop = False
        '
        'ItemLookupCode
        '
        Me.ItemLookupCode.DataPropertyName = "ItemLookupCode"
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.ItemLookupCode.DefaultCellStyle = DataGridViewCellStyle3
        Me.ItemLookupCode.Frozen = True
        Me.ItemLookupCode.HeaderText = "ITEM CODE"
        Me.ItemLookupCode.Name = "ItemLookupCode"
        Me.ItemLookupCode.ReadOnly = True
        Me.ItemLookupCode.Width = 120
        '
        'DESCRIPTION
        '
        Me.lookupDESCRIPTION.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.lookupDESCRIPTION.DataPropertyName = "DESCRIPTION"
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle4.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        DataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.lookupDESCRIPTION.DefaultCellStyle = DataGridViewCellStyle4
        Me.lookupDESCRIPTION.FillWeight = 1.0!
        Me.lookupDESCRIPTION.HeaderText = "ITEM NAME"
        Me.lookupDESCRIPTION.MinimumWidth = 320
        Me.lookupDESCRIPTION.Name = "DESCRIPTION"
        Me.lookupDESCRIPTION.ReadOnly = True
        '
        'PRICE
        '
        Me.lookupPRICE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.lookupPRICE.DataPropertyName = "PRICE"
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle5.Format = "C"
        Me.lookupPRICE.DefaultCellStyle = DataGridViewCellStyle5
        Me.lookupPRICE.HeaderText = "ITEM PRICE"
        Me.lookupPRICE.MinimumWidth = 108
        Me.lookupPRICE.Name = "PRICE"
        Me.lookupPRICE.ReadOnly = True
        Me.lookupPRICE.Width = 131
        '
        'COST
        '
        Me.lookupCOST.DataPropertyName = "COST"
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle6.Format = "N2"
        Me.lookupCOST.DefaultCellStyle = DataGridViewCellStyle6
        Me.lookupCOST.HeaderText = "COST"
        Me.lookupCOST.Name = "COST"
        Me.lookupCOST.ReadOnly = True
        Me.lookupCOST.Visible = False
        '
        'AVAILABLE
        '
        Me.AVAILABLE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.AVAILABLE.DataPropertyName = "AVAILABLE"
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle7.Format = "#,0.###############"
        Me.AVAILABLE.DefaultCellStyle = DataGridViewCellStyle7
        Me.AVAILABLE.HeaderText = "AVAIL QTY"
        Me.AVAILABLE.MinimumWidth = 96
        Me.AVAILABLE.Name = "AVAILABLE"
        Me.AVAILABLE.ReadOnly = True
        Me.AVAILABLE.Width = 123
        '
        'ParentQuantity
        '
        Me.ParentQuantity.DataPropertyName = "ParentQuantity"
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.ParentQuantity.DefaultCellStyle = DataGridViewCellStyle8
        Me.ParentQuantity.HeaderText = "CHILD QTY"
        Me.ParentQuantity.Name = "ParentQuantity"
        Me.ParentQuantity.ReadOnly = True
        Me.ParentQuantity.Visible = False
        Me.ParentQuantity.Width = 90
        '
        'TaxID
        '
        Me.TaxID.DataPropertyName = "TaxID"
        Me.TaxID.HeaderText = "TaxID"
        Me.TaxID.Name = "TaxID"
        Me.TaxID.ReadOnly = True
        Me.TaxID.Visible = False
        '
        'SubDescription2
        '
        Me.SubDescription2.DataPropertyName = "SubDescription2"
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.SubDescription2.DefaultCellStyle = DataGridViewCellStyle9
        Me.SubDescription2.HeaderText = "SKU LEVEL"
        Me.SubDescription2.Name = "SubDescription2"
        Me.SubDescription2.ReadOnly = True
        Me.SubDescription2.Visible = False
        Me.SubDescription2.Width = 90
        '
        'lookupPRICE1
        '
        Me.lookupPRICE1.DataPropertyName = "PRICE 1"
        Me.lookupPRICE1.HeaderText = "PRICE 1"
        Me.lookupPRICE1.Name = "lookupPRICE1"
        Me.lookupPRICE1.ReadOnly = True
        Me.lookupPRICE1.Visible = False
        '
        'ID
        '
        Me.ID.DataPropertyName = "ID"
        Me.ID.HeaderText = "ID"
        Me.ID.Name = "ID"
        Me.ID.ReadOnly = True
        Me.ID.Visible = False
        '
        'DESCRIPTION1
        '
        Me.DESCRIPTION1.DataPropertyName = "DESCRIPTION1"
        Me.DESCRIPTION1.HeaderText = "DESCRIPTION1"
        Me.DESCRIPTION1.Name = "DESCRIPTION1"
        Me.DESCRIPTION1.ReadOnly = True
        Me.DESCRIPTION1.Visible = False
        '
        'ExtendedDescription
        '
        Me.ExtendedDescription.DataPropertyName = "ExtendedDescription"
        Me.ExtendedDescription.HeaderText = "ExtendedDescription"
        Me.ExtendedDescription.Name = "ExtendedDescription"
        Me.ExtendedDescription.ReadOnly = True
        Me.ExtendedDescription.Visible = False
        '
        'ItemType
        '
        Me.ItemType.DataPropertyName = "ItemType"
        Me.ItemType.HeaderText = "ItemType"
        Me.ItemType.Name = "ItemType"
        Me.ItemType.ReadOnly = True
        Me.ItemType.Visible = False
        '
        'Inactive
        '
        Me.Inactive.DataPropertyName = "Inactive"
        Me.Inactive.HeaderText = "Inactive"
        Me.Inactive.Name = "Inactive"
        Me.Inactive.ReadOnly = True
        Me.Inactive.Visible = False
        '
        'ITEMDES
        '
        Me.ITEMDES.DataPropertyName = "ITEMDES"
        Me.ITEMDES.HeaderText = "ITEMDES"
        Me.ITEMDES.Name = "ITEMDES"
        Me.ITEMDES.ReadOnly = True
        Me.ITEMDES.Visible = False
        '
        'SKULEVEL
        '
        Me.SKULEVEL.DataPropertyName = "SKULEVEL"
        Me.SKULEVEL.HeaderText = "SKULEVEL"
        Me.SKULEVEL.Name = "SKULEVEL"
        Me.SKULEVEL.ReadOnly = True
        Me.SKULEVEL.Visible = False
        '
        'FULLDESC
        '
        Me.FULLDESC.DataPropertyName = "FULLDESC"
        Me.FULLDESC.HeaderText = "FULLDESC"
        Me.FULLDESC.Name = "FULLDESC"
        Me.FULLDESC.ReadOnly = True
        Me.FULLDESC.Visible = False
        '
        'Quantity
        '
        Me.Quantity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.Quantity.DataPropertyName = "Quantity"
        DataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.Quantity.DefaultCellStyle = DataGridViewCellStyle10
        Me.Quantity.HeaderText = "SOH"
        Me.Quantity.MinimumWidth = 110
        Me.Quantity.Name = "Quantity"
        Me.Quantity.ReadOnly = True
        Me.Quantity.Width = 110
        '
        'QuantityCommitted
        '
        Me.QuantityCommitted.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.QuantityCommitted.DataPropertyName = "QuantityCommitted"
        DataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.QuantityCommitted.DefaultCellStyle = DataGridViewCellStyle11
        Me.QuantityCommitted.HeaderText = "QTY COM"
        Me.QuantityCommitted.MinimumWidth = 94
        Me.QuantityCommitted.Name = "QuantityCommitted"
        Me.QuantityCommitted.ReadOnly = True
        Me.QuantityCommitted.Width = 111
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.Label6)
        Me.Panel4.Controls.Add(Me.TreeView1)
        Me.Panel4.Controls.Add(Me.Panel5)
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel4.Location = New System.Drawing.Point(10, 0)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(342, 372)
        Me.Panel4.TabIndex = 17
        '
        'Label6
        '
        Me.Label6.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label6.AutoSize = True
        Me.Label6.BackColor = System.Drawing.Color.White
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(5, 353)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(305, 11)
        Me.Label6.TabIndex = 67
        Me.Label6.Text = "Press F4 to Remove Recent Search Keyword. F5 to Clear All"
        '
        'TreeView1
        '
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.TreeView1.Location = New System.Drawing.Point(0, 324)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.Size = New System.Drawing.Size(342, 48)
        Me.TreeView1.TabIndex = 1
        '
        'Panel5
        '
        Me.Panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel5.Controls.Add(Me.PictureBox3)
        Me.Panel5.Controls.Add(Me.chkWebsite)
        Me.Panel5.Controls.Add(Me.chkBarcode)
        Me.Panel5.Controls.Add(Me.txtBarcode)
        Me.Panel5.Controls.Add(Me.chkQuote)
        Me.Panel5.Controls.Add(Me.chkShowImage)
        Me.Panel5.Controls.Add(Me.chkWorkOrder)
        Me.Panel5.Controls.Add(Me.PictureBox1)
        Me.Panel5.Controls.Add(Me.PictureBox2)
        Me.Panel5.Controls.Add(Me.Label9)
        Me.Panel5.Controls.Add(Me.rbtnDelivery)
        Me.Panel5.Controls.Add(Me.rbtnPickup)
        Me.Panel5.Controls.Add(Me.Label8)
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
        Me.Panel5.Controls.Add(Me.pnl1Barcode)
        Me.Panel5.Controls.Add(Me.pnl2TextSearch)
        Me.Panel5.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel5.Location = New System.Drawing.Point(0, 0)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(342, 324)
        Me.Panel5.TabIndex = 0
        '
        'PictureBox3
        '
        Me.PictureBox3.BackColor = System.Drawing.Color.Black
        Me.PictureBox3.Location = New System.Drawing.Point(252, 6)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(1, 18)
        Me.PictureBox3.TabIndex = 80
        Me.PictureBox3.TabStop = False
        '
        'chkWebsite
        '
        Me.chkWebsite.AutoSize = True
        Me.chkWebsite.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkWebsite.Location = New System.Drawing.Point(266, 6)
        Me.chkWebsite.Name = "chkWebsite"
        Me.chkWebsite.Size = New System.Drawing.Size(72, 17)
        Me.chkWebsite.TabIndex = 79
        Me.chkWebsite.Text = "Website"
        Me.chkWebsite.UseVisualStyleBackColor = True
        Me.chkWebsite.Visible = False
        '
        'chkQuote
        '
        Me.chkQuote.AutoSize = True
        Me.chkQuote.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkQuote.Location = New System.Drawing.Point(172, 6)
        Me.chkQuote.Name = "chkQuote"
        Me.chkQuote.Size = New System.Drawing.Size(82, 17)
        Me.chkQuote.TabIndex = 78
        Me.chkQuote.Text = "Quotation"
        Me.chkQuote.UseVisualStyleBackColor = True
        '
        'chkShowImage
        '
        Me.chkShowImage.AutoSize = True
        Me.chkShowImage.BackColor = System.Drawing.SystemColors.Control
        Me.chkShowImage.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkShowImage.Location = New System.Drawing.Point(6, 302)
        Me.chkShowImage.Name = "chkShowImage"
        Me.chkShowImage.Size = New System.Drawing.Size(96, 17)
        Me.chkShowImage.TabIndex = 24
        Me.chkShowImage.Text = "Show Image"
        Me.chkShowImage.UseVisualStyleBackColor = False
        '
        'chkWorkOrder
        '
        Me.chkWorkOrder.AutoSize = True
        Me.chkWorkOrder.Checked = True
        Me.chkWorkOrder.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWorkOrder.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.chkWorkOrder.Location = New System.Drawing.Point(80, 7)
        Me.chkWorkOrder.Name = "chkWorkOrder"
        Me.chkWorkOrder.Size = New System.Drawing.Size(91, 17)
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
        Me.PictureBox2.Location = New System.Drawing.Point(7, 218)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(327, 1)
        Me.PictureBox2.TabIndex = 75
        Me.PictureBox2.TabStop = False
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label9.Location = New System.Drawing.Point(8, 7)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(74, 13)
        Me.Label9.TabIndex = 71
        Me.Label9.Text = "Entry Type :"
        '
        'rbtnDelivery
        '
        Me.rbtnDelivery.AutoSize = True
        Me.rbtnDelivery.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.rbtnDelivery.Location = New System.Drawing.Point(173, 195)
        Me.rbtnDelivery.Name = "rbtnDelivery"
        Me.rbtnDelivery.Size = New System.Drawing.Size(72, 17)
        Me.rbtnDelivery.TabIndex = 70
        Me.rbtnDelivery.Text = "Delivery"
        Me.rbtnDelivery.UseVisualStyleBackColor = True
        '
        'rbtnPickup
        '
        Me.rbtnPickup.AutoSize = True
        Me.rbtnPickup.Checked = True
        Me.rbtnPickup.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.rbtnPickup.Location = New System.Drawing.Point(99, 195)
        Me.rbtnPickup.Name = "rbtnPickup"
        Me.rbtnPickup.Size = New System.Drawing.Size(67, 17)
        Me.rbtnPickup.TabIndex = 69
        Me.rbtnPickup.TabStop = True
        Me.rbtnPickup.Text = "Pick-up"
        Me.rbtnPickup.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label8.Location = New System.Drawing.Point(8, 197)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(89, 13)
        Me.Label8.TabIndex = 68
        Me.Label8.Text = "Release Type :"
        '
        'cboPayment
        '
        Me.cboPayment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboPayment.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.cboPayment.FormattingEnabled = True
        Me.cboPayment.Items.AddRange(New Object() {"CASH", "CHARGE", "CREDIT CARD", "DEBIT CARD"})
        Me.cboPayment.Location = New System.Drawing.Point(222, 86)
        Me.cboPayment.Name = "cboPayment"
        Me.cboPayment.Size = New System.Drawing.Size(112, 22)
        Me.cboPayment.TabIndex = 66
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold)
        Me.Label7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.Label7.ImageAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.Label7.Location = New System.Drawing.Point(222, 65)
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
        Me.cmdSSales.Location = New System.Drawing.Point(293, 126)
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
        Me.cmdSCustomer.Location = New System.Drawing.Point(293, 45)
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
        Me.cmdSearch.Location = New System.Drawing.Point(297, 237)
        Me.cmdSearch.Name = "cmdSearch"
        Me.cmdSearch.Size = New System.Drawing.Size(41, 25)
        Me.cmdSearch.TabIndex = 9
        Me.cmdSearch.Text = "F2"
        Me.cmdSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cmdSearch.UseVisualStyleBackColor = True
        '
        'pnl1Barcode
        '
        Me.pnl1Barcode.BackColor = System.Drawing.SystemColors.Control
        Me.pnl1Barcode.Location = New System.Drawing.Point(6, 266)
        Me.pnl1Barcode.Name = "pnl1Barcode"
        Me.pnl1Barcode.Size = New System.Drawing.Size(223, 26)
        Me.pnl1Barcode.TabIndex = 81
        '
        'pnl2TextSearch
        '
        Me.pnl2TextSearch.BackColor = System.Drawing.SystemColors.Control
        Me.pnl2TextSearch.Location = New System.Drawing.Point(6, 236)
        Me.pnl2TextSearch.Name = "pnl2TextSearch"
        Me.pnl2TextSearch.Size = New System.Drawing.Size(283, 26)
        Me.pnl2TextSearch.TabIndex = 82
        '
        'panelSelectItem
        '
        Me.panelSelectItem.Controls.Add(Me.gridSelectItem)
        Me.panelSelectItem.Controls.Add(Me.Label4)
        Me.panelSelectItem.Controls.Add(Me.GroupBox1)
        Me.panelSelectItem.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelSelectItem.Location = New System.Drawing.Point(0, 372)
        Me.panelSelectItem.Name = "panelSelectItem"
        Me.panelSelectItem.Padding = New System.Windows.Forms.Padding(10, 0, 1, 11)
        Me.panelSelectItem.Size = New System.Drawing.Size(1179, 220)
        Me.panelSelectItem.TabIndex = 19
        '
        'gridSelectItem
        '
        Me.gridSelectItem.AllowUserToAddRows = False
        Me.gridSelectItem.AllowUserToDeleteRows = False
        Me.gridSelectItem.AllowUserToOrderColumns = True
        DataGridViewCellStyle15.BackColor = System.Drawing.Color.FromArgb(CType(CType(246, Byte), Integer), CType(CType(248, Byte), Integer), CType(CType(251, Byte), Integer))
        Me.gridSelectItem.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle15
        Me.gridSelectItem.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridSelectItem.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        DataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle16.BackColor = System.Drawing.Color.FromArgb(CType(CType(232, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(244, Byte), Integer))
        DataGridViewCellStyle16.Font = New System.Drawing.Font("Tahoma", 10.0!)
        DataGridViewCellStyle16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle16.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        DataGridViewCellStyle16.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(232, Byte), Integer), CType(CType(237, Byte), Integer), CType(CType(244, Byte), Integer))
        DataGridViewCellStyle16.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridSelectItem.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle16
        Me.gridSelectItem.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ItemCode, Me.ItemName, Me.QTY, Me.Price, Me.DISC, Me.TOTAL, Me.LessV, Me.VSales, Me.DiscP, Me.Cost, Me.Taxable, Me.ItemID, Me.FullPrice, Me.Description, Me.Extended, Me.DiscAmount, Me.OrderEntryID, Me.chkPickLoc, Me.CustPrep, Me.LASTPURCHASEDPRICE})
        Me.gridSelectItem.ColumnHeadersHeight = 34
        Me.gridSelectItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        DataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle17.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle17.Font = New System.Drawing.Font("Tahoma", 10.0!)
        DataGridViewCellStyle17.ForeColor = System.Drawing.Color.FromArgb(CType(CType(31, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(55, Byte), Integer))
        DataGridViewCellStyle17.Padding = New System.Windows.Forms.Padding(6, 3, 6, 3)
        DataGridViewCellStyle17.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(37, Byte), Integer), CType(CType(99, Byte), Integer), CType(CType(160, Byte), Integer))
        DataGridViewCellStyle17.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridSelectItem.DefaultCellStyle = DataGridViewCellStyle17
        Me.gridSelectItem.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridSelectItem.EnableHeadersVisualStyles = False
        Me.gridSelectItem.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.gridSelectItem.GridColor = System.Drawing.Color.FromArgb(CType(CType(218, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.gridSelectItem.Location = New System.Drawing.Point(10, 29)
        Me.gridSelectItem.Name = "gridSelectItem"
        DataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle18.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridSelectItem.RowHeadersDefaultCellStyle = DataGridViewCellStyle18
        Me.gridSelectItem.RowHeadersWidth = 51
        Me.gridSelectItem.RowTemplate.Height = 30
        Me.gridSelectItem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.gridSelectItem.Size = New System.Drawing.Size(1168, 180)
        Me.gridSelectItem.TabIndex = 15
        Me.gridSelectItem.TabStop = False
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
        Me.GroupBox1.Size = New System.Drawing.Size(1168, 11)
        Me.GroupBox1.TabIndex = 20
        Me.GroupBox1.TabStop = False
        '
        'Price
        '
        Me.Price.Name = "Price"
        Me.Price.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.Price.MinimumWidth = 100
        Me.Price.Width = 100
        Me.Price.HeaderText = "PRICE"
        Me.Price.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        SelectedPriceStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        SelectedPriceStyle.Format = "C"
        SelectedPriceStyle.NullValue = Nothing
        Me.Price.DefaultCellStyle = SelectedPriceStyle
        '
        'Cost
        '
        Me.Cost.Name = "Cost"
        Me.Cost.Visible = False
        Me.Cost.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.NotSet
        Me.Cost.MinimumWidth = 6
        Me.Cost.Width = 125
        Me.Cost.HeaderText = "Cost"
        Me.Cost.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Cost.DefaultCellStyle = SelectedCostStyle
        '
        'Description
        '
        Me.Description.Name = "Description"
        Me.Description.Visible = False
        Me.Description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.NotSet
        Me.Description.MinimumWidth = 6
        Me.Description.Width = 125
        Me.Description.HeaderText = "Description"
        Me.Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Description.DefaultCellStyle = SelectedDescriptionStyle
        '
        'ItemCode
        '
        Me.ItemCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle19.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.ItemCode.DefaultCellStyle = DataGridViewCellStyle19
        Me.ItemCode.HeaderText = "ITEM CODE"
        Me.ItemCode.MinimumWidth = 6
        Me.ItemCode.Name = "ItemCode"
        Me.ItemCode.ReadOnly = True
        Me.ItemCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.ItemCode.Width = 110
        '
        'ItemName
        '
        Me.ItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ItemName.HeaderText = "ITEM NAME"
        Me.ItemName.MinimumWidth = 220
        Me.ItemName.Name = "ItemName"
        Me.ItemName.ReadOnly = True
        Me.ItemName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'QTY
        '
        Me.QTY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        DataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle20.Format = "#,0.###############"
        Me.QTY.DefaultCellStyle = DataGridViewCellStyle20
        Me.QTY.HeaderText = "QUANTITY"
        Me.QTY.MinimumWidth = 90
        Me.QTY.Name = "QTY"
        Me.QTY.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.QTY.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DISC
        '
        Me.DISC.HeaderText = "DISC"
        Me.DISC.MinimumWidth = 6
        Me.DISC.Name = "DISC"
        Me.DISC.ReadOnly = True
        Me.DISC.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DISC.Visible = False
        Me.DISC.Width = 70
        '
        'TOTAL
        '
        Me.TOTAL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        DataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle21.Format = "C"
        DataGridViewCellStyle21.NullValue = Nothing
        Me.TOTAL.DefaultCellStyle = DataGridViewCellStyle21
        Me.TOTAL.HeaderText = "TOTAL PRICE"
        Me.TOTAL.MinimumWidth = 120
        Me.TOTAL.Name = "TOTAL"
        Me.TOTAL.ReadOnly = True
        Me.TOTAL.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'LessV
        '
        Me.LessV.HeaderText = "Less VAT"
        Me.LessV.MinimumWidth = 6
        Me.LessV.Name = "LessV"
        Me.LessV.ReadOnly = True
        Me.LessV.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.LessV.Visible = False
        Me.LessV.Width = 125
        '
        'VSales
        '
        Me.VSales.HeaderText = "VAT Sales"
        Me.VSales.MinimumWidth = 6
        Me.VSales.Name = "VSales"
        Me.VSales.ReadOnly = True
        Me.VSales.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.VSales.Visible = False
        Me.VSales.Width = 125
        '
        'DiscP
        '
        Me.DiscP.HeaderText = "Disc Price"
        Me.DiscP.MinimumWidth = 6
        Me.DiscP.Name = "DiscP"
        Me.DiscP.ReadOnly = True
        Me.DiscP.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DiscP.Visible = False
        Me.DiscP.Width = 125
        '
        'Taxable
        '
        Me.Taxable.HeaderText = "Taxable"
        Me.Taxable.MinimumWidth = 6
        Me.Taxable.Name = "Taxable"
        Me.Taxable.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Taxable.Visible = False
        Me.Taxable.Width = 125
        '
        'ItemID
        '
        Me.ItemID.HeaderText = "ItemID"
        Me.ItemID.MinimumWidth = 6
        Me.ItemID.Name = "ItemID"
        Me.ItemID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.ItemID.Visible = False
        Me.ItemID.Width = 125
        '
        'FullPrice
        '
        Me.FullPrice.HeaderText = "FullPrice"
        Me.FullPrice.MinimumWidth = 6
        Me.FullPrice.Name = "FullPrice"
        Me.FullPrice.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.FullPrice.Visible = False
        Me.FullPrice.Width = 125
        '
        'Extended
        '
        Me.Extended.HeaderText = "Extended"
        Me.Extended.MinimumWidth = 6
        Me.Extended.Name = "Extended"
        Me.Extended.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.Extended.Visible = False
        Me.Extended.Width = 125
        '
        'DiscAmount
        '
        Me.DiscAmount.HeaderText = "DiscAmount"
        Me.DiscAmount.MinimumWidth = 6
        Me.DiscAmount.Name = "DiscAmount"
        Me.DiscAmount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DiscAmount.Visible = False
        Me.DiscAmount.Width = 125
        '
        'OrderEntryID
        '
        Me.OrderEntryID.HeaderText = "OrderEntryID"
        Me.OrderEntryID.MinimumWidth = 6
        Me.OrderEntryID.Name = "OrderEntryID"
        Me.OrderEntryID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.OrderEntryID.Visible = False
        Me.OrderEntryID.Width = 90
        '
        'chkPickLoc
        '
        Me.chkPickLoc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.chkPickLoc.HeaderText = "UP-STORE"
        Me.chkPickLoc.MinimumWidth = 75
        Me.chkPickLoc.Name = "chkPickLoc"
        Me.chkPickLoc.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'CustPrep
        '
        Me.CustPrep.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.CustPrep.DefaultCellStyle = DataGridViewCellStyle22
        Me.CustPrep.HeaderText = "PREPARED"
        Me.CustPrep.MinimumWidth = 6
        Me.CustPrep.Name = "CustPrep"
        Me.CustPrep.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.CustPrep.Width = 105
        '
        'LASTPURCHASEDPRICE
        '
        Me.LASTPURCHASEDPRICE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle23.Format = "C"
        Me.LASTPURCHASEDPRICE.DefaultCellStyle = DataGridViewCellStyle23
        Me.LASTPURCHASEDPRICE.HeaderText = "LAST PURCHASE PRICE"
        Me.LASTPURCHASEDPRICE.MinimumWidth = 6
        Me.LASTPURCHASEDPRICE.Name = "LASTPURCHASEDPRICE"
        Me.LASTPURCHASEDPRICE.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.LASTPURCHASEDPRICE.Width = 180
        '
        'Update_Timer
        '
        Me.Update_Timer.Interval = 5000
        '
        'Check_errors
        '
        Me.Check_errors.Enabled = True
        Me.Check_errors.Interval = 1000
        '
        'frmItemLookUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1323, 789)
        Me.Controls.Add(Me.panelGrid)
        Me.Controls.Add(Me.panelTotalsContainer)
        Me.Controls.Add(Me.panelButtons)
        Me.Controls.Add(Me.BottomStrip)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "frmItemLookUp"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Work Order Entry_V8"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.BottomStrip.ResumeLayout(False)
        Me.BottomStrip.PerformLayout()
        Me.panelButtons.ResumeLayout(False)
        Me.panelButtons.PerformLayout()
        Me.panelTotal.ResumeLayout(False)
        Me.panelTotal.PerformLayout()
        Me.grpWithholding.ResumeLayout(False)
        Me.withholdingLayout.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.panelTotalsContainer.ResumeLayout(False)
        Me.panelGrid.ResumeLayout(False)
        Me.panelItem.ResumeLayout(False)
        Me.picPanel.ResumeLayout(False)
        CType(Me.gridItem, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents grpWithholding As System.Windows.Forms.GroupBox
    Friend WithEvents withholdingLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblWithholdingBasis As System.Windows.Forms.Label
    Friend WithEvents lblWithholding1 As System.Windows.Forms.Label
    Friend WithEvents lblWithholding2 As System.Windows.Forms.Label
    Friend WithEvents lblWithholding5 As System.Windows.Forms.Label
    Friend WithEvents lblWithholding15 As System.Windows.Forms.Label
    Friend WithEvents withholdingToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents panelTotalsContainer As System.Windows.Forms.Panel
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
    Friend WithEvents lookupItemLookupCode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupDESCRIPTION As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupPRICE As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupCOST As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupAVAILABLE As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupParentQuantity As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupTaxID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupSubDescription2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupPRICE1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupDESCRIPTION1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupExtendedDescription As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupItemType As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupInactive As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents lookupITEMDES As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupSKULEVEL As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupFULLDESC As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupQuantity As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupQuantityCommitted As System.Windows.Forms.DataGridViewTextBoxColumn
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
    Friend WithEvents rbtnDelivery As System.Windows.Forms.RadioButton
    Friend WithEvents rbtnPickup As System.Windows.Forms.RadioButton
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cmdForInvoice As System.Windows.Forms.Button
    Friend WithEvents Check_errors As System.Windows.Forms.Timer
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents chkQuote As System.Windows.Forms.CheckBox
    Friend WithEvents chkWorkOrder As System.Windows.Forms.CheckBox
    Friend WithEvents picPanel As WorkOrderEntry.CustomPanel
    Friend WithEvents picItem As WorkOrderEntry.ZoomPictureBox
    Friend WithEvents chkShowImage As System.Windows.Forms.CheckBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents cmdImportWeb As System.Windows.Forms.Button
    Friend WithEvents chkWebsite As System.Windows.Forms.CheckBox
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents txtBarcode As TextBox
    Friend WithEvents chkBarcode As CheckBox
    Friend WithEvents txtPrice As TextBox
    Friend WithEvents txtPriceLevel As TextBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents chkBoxQtoWo As CheckBox
    Friend WithEvents txtCustomerId As TextBox
    Friend WithEvents txtOpenWO As TextBox
    Friend WithEvents txtAR As TextBox
    Friend WithEvents txtCreditLimit As TextBox
    Friend WithEvents txtAvailable As TextBox
    Friend WithEvents cmdOpenWO As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents pnl1Barcode As Panel
    Friend WithEvents pnl2TextSearch As Panel
    Friend WithEvents btnTax As Button
    Friend WithEvents ItemCode As DataGridViewTextBoxColumn
    Friend WithEvents ItemName As DataGridViewTextBoxColumn
    Friend WithEvents QTY As DataGridViewTextBoxColumn
    Friend WithEvents DISC As DataGridViewTextBoxColumn
    Friend WithEvents TOTAL As DataGridViewTextBoxColumn
    Friend WithEvents LessV As DataGridViewTextBoxColumn
    Friend WithEvents VSales As DataGridViewTextBoxColumn
    Friend WithEvents DiscP As DataGridViewTextBoxColumn
    Friend WithEvents Taxable As DataGridViewTextBoxColumn
    Friend WithEvents ItemID As DataGridViewTextBoxColumn
    Friend WithEvents FullPrice As DataGridViewTextBoxColumn
    Friend WithEvents Extended As DataGridViewTextBoxColumn
    Friend WithEvents DiscAmount As DataGridViewTextBoxColumn
    Friend WithEvents OrderEntryID As DataGridViewTextBoxColumn
    Friend WithEvents chkPickLoc As DataGridViewCheckBoxColumn
    Friend WithEvents CustPrep As DataGridViewTextBoxColumn
    Friend WithEvents LASTPURCHASEDPRICE As DataGridViewTextBoxColumn
    Friend WithEvents ItemLookupCode As DataGridViewTextBoxColumn
    Friend WithEvents AVAILABLE As DataGridViewTextBoxColumn
    Friend WithEvents ParentQuantity As DataGridViewTextBoxColumn
    Friend WithEvents TaxID As DataGridViewTextBoxColumn
    Friend WithEvents SubDescription2 As DataGridViewTextBoxColumn
    Friend WithEvents ID As DataGridViewTextBoxColumn
    Friend WithEvents DESCRIPTION1 As DataGridViewTextBoxColumn
    Friend WithEvents ExtendedDescription As DataGridViewTextBoxColumn
    Friend WithEvents ItemType As DataGridViewTextBoxColumn
    Friend WithEvents Inactive As DataGridViewCheckBoxColumn
    Friend WithEvents ITEMDES As DataGridViewTextBoxColumn
    Friend WithEvents SKULEVEL As DataGridViewTextBoxColumn
    Friend WithEvents FULLDESC As DataGridViewTextBoxColumn
    Friend WithEvents Quantity As DataGridViewTextBoxColumn
    Friend WithEvents QuantityCommitted As DataGridViewTextBoxColumn
    Friend WithEvents Price As DataGridViewTextBoxColumn
    Friend WithEvents Cost As DataGridViewTextBoxColumn
    Friend WithEvents Description As DataGridViewTextBoxColumn
End Class
