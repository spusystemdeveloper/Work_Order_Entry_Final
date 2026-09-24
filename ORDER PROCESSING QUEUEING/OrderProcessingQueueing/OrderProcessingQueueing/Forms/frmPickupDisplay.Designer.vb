<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPickupDisplay
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.lblClock = New System.Windows.Forms.Label()
        Me.lblNotice = New System.Windows.Forms.Label()
        Me.lblStoreTitle = New System.Windows.Forms.Label()
        Me.splitColumns = New System.Windows.Forms.SplitContainer()
        Me.pnlPreparingHeader = New System.Windows.Forms.Panel()
        Me.lblPreparingTitle = New System.Windows.Forms.Label()
        Me.gridPreparing = New System.Windows.Forms.DataGridView()
        Me.colPrepWO = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPrepCust = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlReadyHeader = New System.Windows.Forms.Panel()
        Me.lblReadyTitle = New System.Windows.Forms.Label()
        Me.gridReady = New System.Windows.Forms.DataGridView()
        Me.colReadyWO = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colReadyCust = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.timerRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.timerClock = New System.Windows.Forms.Timer(Me.components)
        Me.pnlHeader.SuspendLayout()
        CType(Me.splitColumns, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitColumns.Panel1.SuspendLayout()
        Me.splitColumns.Panel2.SuspendLayout()
        Me.splitColumns.SuspendLayout()
        Me.pnlPreparingHeader.SuspendLayout()
        CType(Me.gridPreparing, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlReadyHeader.SuspendLayout()
        CType(Me.gridReady, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(15, Byte), Integer), CType(CType(23, Byte), Integer), CType(CType(42, Byte), Integer))
        Me.pnlHeader.Controls.Add(Me.lblClock)
        Me.pnlHeader.Controls.Add(Me.lblNotice)
        Me.pnlHeader.Controls.Add(Me.lblStoreTitle)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(1024, 75)
        Me.pnlHeader.TabIndex = 0
        '
        'lblClock
        '
        Me.lblClock.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblClock.Font = New System.Drawing.Font("Century Gothic", 20.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblClock.ForeColor = System.Drawing.Color.FromArgb(CType(CType(56, Byte), Integer), CType(CType(189, Byte), Integer), CType(CType(248, Byte), Integer))
        Me.lblClock.Location = New System.Drawing.Point(824, 15)
        Me.lblClock.Name = "lblClock"
        Me.lblClock.Size = New System.Drawing.Size(188, 45)
        Me.lblClock.TabIndex = 2
        Me.lblClock.Text = "12:00 PM"
        Me.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblNotice
        '
        Me.lblNotice.AutoSize = True
        Me.lblNotice.Font = New System.Drawing.Font("Century Gothic", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNotice.ForeColor = System.Drawing.Color.FromArgb(CType(CType(148, Byte), Integer), CType(CType(163, Byte), Integer), CType(CType(184, Byte), Integer))
        Me.lblNotice.Location = New System.Drawing.Point(16, 43)
        Me.lblNotice.Name = "lblNotice"
        Me.lblNotice.Size = New System.Drawing.Size(462, 20)
        Me.lblNotice.TabIndex = 1
        Me.lblNotice.Text = "Please proceed to the cashier counter when your order is READY."
        '
        'lblStoreTitle
        '
        Me.lblStoreTitle.AutoSize = True
        Me.lblStoreTitle.Font = New System.Drawing.Font("Century Gothic", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStoreTitle.ForeColor = System.Drawing.Color.White
        Me.lblStoreTitle.Location = New System.Drawing.Point(14, 12)
        Me.lblStoreTitle.Name = "lblStoreTitle"
        Me.lblStoreTitle.Size = New System.Drawing.Size(420, 28)
        Me.lblStoreTitle.TabIndex = 0
        Me.lblStoreTitle.Text = "📦 ORDER PICKUP STATUS BOARD"
        '
        'splitColumns
        '
        Me.splitColumns.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitColumns.IsSplitterFixed = True
        Me.splitColumns.Location = New System.Drawing.Point(0, 75)
        Me.splitColumns.Name = "splitColumns"
        '
        'splitColumns.Panel1
        '
        Me.splitColumns.Panel1.Controls.Add(Me.gridPreparing)
        Me.splitColumns.Panel1.Controls.Add(Me.pnlPreparingHeader)
        '
        'splitColumns.Panel2
        '
        Me.splitColumns.Panel2.Controls.Add(Me.gridReady)
        Me.splitColumns.Panel2.Controls.Add(Me.pnlReadyHeader)
        Me.splitColumns.Size = New System.Drawing.Size(1024, 625)
        Me.splitColumns.SplitterDistance = 460
        Me.splitColumns.TabIndex = 1
        '
        'pnlPreparingHeader
        '
        Me.pnlPreparingHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(217, Byte), Integer), CType(CType(119, Byte), Integer), CType(CType(6, Byte), Integer))
        Me.pnlPreparingHeader.Controls.Add(Me.lblPreparingTitle)
        Me.pnlPreparingHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlPreparingHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlPreparingHeader.Name = "pnlPreparingHeader"
        Me.pnlPreparingHeader.Size = New System.Drawing.Size(460, 48)
        Me.pnlPreparingHeader.TabIndex = 0
        '
        'lblPreparingTitle
        '
        Me.lblPreparingTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblPreparingTitle.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPreparingTitle.ForeColor = System.Drawing.Color.White
        Me.lblPreparingTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblPreparingTitle.Name = "lblPreparingTitle"
        Me.lblPreparingTitle.Size = New System.Drawing.Size(460, 48)
        Me.lblPreparingTitle.TabIndex = 0
        Me.lblPreparingTitle.Text = "🔨 BEING PREPARED"
        Me.lblPreparingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'gridPreparing
        '
        Me.gridPreparing.AllowUserToAddRows = False
        Me.gridPreparing.AllowUserToDeleteRows = False
        Me.gridPreparing.AllowUserToResizeRows = False
        Me.gridPreparing.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridPreparing.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(235, Byte), Integer))
        Me.gridPreparing.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gridPreparing.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.gridPreparing.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridPreparing.ColumnHeadersVisible = False
        Me.gridPreparing.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colPrepWO, Me.colPrepCust})
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(251, Byte), Integer), CType(CType(235, Byte), Integer))
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Century Gothic", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(120, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(15, Byte), Integer))
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(254, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(199, Byte), Integer))
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(120, Byte), Integer), CType(CType(53, Byte), Integer), CType(CType(15, Byte), Integer))
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridPreparing.DefaultCellStyle = DataGridViewCellStyle1
        Me.gridPreparing.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridPreparing.GridColor = System.Drawing.Color.FromArgb(CType(CType(253, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.gridPreparing.Location = New System.Drawing.Point(0, 48)
        Me.gridPreparing.MultiSelect = False
        Me.gridPreparing.Name = "gridPreparing"
        Me.gridPreparing.ReadOnly = True
        Me.gridPreparing.RowHeadersVisible = False
        Me.gridPreparing.RowTemplate.Height = 44
        Me.gridPreparing.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridPreparing.Size = New System.Drawing.Size(460, 577)
        Me.gridPreparing.TabIndex = 1
        '
        'colPrepWO
        '
        Me.colPrepWO.FillWeight = 40.0!
        Me.colPrepWO.HeaderText = "WO #"
        Me.colPrepWO.Name = "colPrepWO"
        Me.colPrepWO.ReadOnly = True
        '
        'colPrepCust
        '
        Me.colPrepCust.FillWeight = 60.0!
        Me.colPrepCust.HeaderText = "Customer"
        Me.colPrepCust.Name = "colPrepCust"
        Me.colPrepCust.ReadOnly = True
        '
        'pnlReadyHeader
        '
        Me.pnlReadyHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(5, Byte), Integer), CType(CType(150, Byte), Integer), CType(CType(105, Byte), Integer))
        Me.pnlReadyHeader.Controls.Add(Me.lblReadyTitle)
        Me.pnlReadyHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlReadyHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlReadyHeader.Name = "pnlReadyHeader"
        Me.pnlReadyHeader.Size = New System.Drawing.Size(560, 48)
        Me.pnlReadyHeader.TabIndex = 0
        '
        'lblReadyTitle
        '
        Me.lblReadyTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblReadyTitle.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblReadyTitle.ForeColor = System.Drawing.Color.White
        Me.lblReadyTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblReadyTitle.Name = "lblReadyTitle"
        Me.lblReadyTitle.Size = New System.Drawing.Size(560, 48)
        Me.lblReadyTitle.TabIndex = 0
        Me.lblReadyTitle.Text = "✅ READY FOR PICKUP / CASHIER"
        Me.lblReadyTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'gridReady
        '
        Me.gridReady.AllowUserToAddRows = False
        Me.gridReady.AllowUserToDeleteRows = False
        Me.gridReady.AllowUserToResizeRows = False
        Me.gridReady.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridReady.BackgroundColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(253, Byte), Integer), CType(CType(245, Byte), Integer))
        Me.gridReady.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gridReady.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.gridReady.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridReady.ColumnHeadersVisible = False
        Me.gridReady.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colReadyWO, Me.colReadyCust})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(236, Byte), Integer), CType(CType(253, Byte), Integer), CType(CType(245, Byte), Integer))
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(6, Byte), Integer), CType(CType(95, Byte), Integer), CType(CType(70, Byte), Integer))
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(209, Byte), Integer), CType(CType(250, Byte), Integer), CType(CType(229, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(CType(CType(6, Byte), Integer), CType(CType(95, Byte), Integer), CType(CType(70, Byte), Integer))
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridReady.DefaultCellStyle = DataGridViewCellStyle2
        Me.gridReady.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridReady.GridColor = System.Drawing.Color.FromArgb(CType(CType(167, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(208, Byte), Integer))
        Me.gridReady.Location = New System.Drawing.Point(0, 48)
        Me.gridReady.MultiSelect = False
        Me.gridReady.Name = "gridReady"
        Me.gridReady.ReadOnly = True
        Me.gridReady.RowHeadersVisible = False
        Me.gridReady.RowTemplate.Height = 46
        Me.gridReady.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridReady.Size = New System.Drawing.Size(560, 577)
        Me.gridReady.TabIndex = 1
        '
        'colReadyWO
        '
        Me.colReadyWO.FillWeight = 38.0!
        Me.colReadyWO.HeaderText = "WO #"
        Me.colReadyWO.Name = "colReadyWO"
        Me.colReadyWO.ReadOnly = True
        '
        'colReadyCust
        '
        Me.colReadyCust.FillWeight = 62.0!
        Me.colReadyCust.HeaderText = "Customer"
        Me.colReadyCust.Name = "colReadyCust"
        Me.colReadyCust.ReadOnly = True
        '
        'timerRefresh
        '
        Me.timerRefresh.Interval = 5000
        '
        'timerClock
        '
        Me.timerClock.Interval = 1000
        '
        'frmPickupDisplay
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1024, 700)
        Me.Controls.Add(Me.splitColumns)
        Me.Controls.Add(Me.pnlHeader)
        Me.KeyPreview = True
        Me.Name = "frmPickupDisplay"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Order Pickup Display Board"
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlHeader.PerformLayout()
        Me.splitColumns.Panel1.ResumeLayout(False)
        Me.splitColumns.Panel2.ResumeLayout(False)
        CType(Me.splitColumns, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitColumns.ResumeLayout(False)
        Me.pnlPreparingHeader.ResumeLayout(False)
        CType(Me.gridPreparing, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlReadyHeader.ResumeLayout(False)
        CType(Me.gridReady, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlHeader As System.Windows.Forms.Panel
    Friend WithEvents lblStoreTitle As System.Windows.Forms.Label
    Friend WithEvents lblNotice As System.Windows.Forms.Label
    Friend WithEvents lblClock As System.Windows.Forms.Label
    Friend WithEvents splitColumns As System.Windows.Forms.SplitContainer
    Friend WithEvents pnlPreparingHeader As System.Windows.Forms.Panel
    Friend WithEvents lblPreparingTitle As System.Windows.Forms.Label
    Friend WithEvents gridPreparing As System.Windows.Forms.DataGridView
    Friend WithEvents colPrepWO As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPrepCust As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents pnlReadyHeader As System.Windows.Forms.Panel
    Friend WithEvents lblReadyTitle As System.Windows.Forms.Label
    Friend WithEvents gridReady As System.Windows.Forms.DataGridView
    Friend WithEvents colReadyWO As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colReadyCust As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents timerRefresh As System.Windows.Forms.Timer
    Friend WithEvents timerClock As System.Windows.Forms.Timer
End Class
