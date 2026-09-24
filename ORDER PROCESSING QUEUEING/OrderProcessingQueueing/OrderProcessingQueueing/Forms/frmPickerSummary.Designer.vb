<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPickerSummary
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
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.pnlTop = New System.Windows.Forms.Panel()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.pnlBottom = New System.Windows.Forms.Panel()
        Me.lblSummary = New System.Windows.Forms.Label()
        Me.btnFilterCards = New System.Windows.Forms.Button()
        Me.btnClearFilter = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.gridPickerSummary = New System.Windows.Forms.DataGridView()
        Me.colPicker = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colInProcess = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPrepared = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotalItems = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colOrders = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.timerRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.btnViewMode = New System.Windows.Forms.Button()
        Me.btnReassignPicker = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlBottom.SuspendLayout()
        CType(Me.gridPickerSummary, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.BackColor = System.Drawing.Color.FromArgb(CType(CType(248, Byte), Integer), CType(CType(250, Byte), Integer), CType(CType(252, Byte), Integer))
        Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTop.Controls.Add(Me.btnReassignPicker)
        Me.pnlTop.Controls.Add(Me.btnViewMode)
        Me.pnlTop.Controls.Add(Me.lblSubtitle)
        Me.pnlTop.Controls.Add(Me.lblTitle)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(740, 60)
        Me.pnlTop.TabIndex = 0
        '
        'lblSubtitle
        '
        Me.lblSubtitle.AutoSize = True
        Me.lblSubtitle.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSubtitle.ForeColor = System.Drawing.Color.DimGray
        Me.lblSubtitle.Location = New System.Drawing.Point(13, 33)
        Me.lblSubtitle.Name = "lblSubtitle"
        Me.lblSubtitle.Size = New System.Drawing.Size(434, 17)
        Me.lblSubtitle.TabIndex = 1
        Me.lblSubtitle.Text = "Real-time picker workload. Double-click any picker to filter cards on screen."
        Me.lblSubtitle.UseCompatibleTextRendering = True
        '
        'btnViewMode
        '
        Me.btnViewMode.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnViewMode.BackColor = System.Drawing.Color.White
        Me.btnViewMode.FlatAppearance.BorderColor = System.Drawing.Color.Gray
        Me.btnViewMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnViewMode.Font = New System.Drawing.Font("Century Gothic", 8.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnViewMode.Location = New System.Drawing.Point(490, 13)
        Me.btnViewMode.Name = "btnViewMode"
        Me.btnViewMode.Size = New System.Drawing.Size(115, 32)
        Me.btnViewMode.TabIndex = 2
        Me.btnViewMode.Text = "🏆 Leaderboard"
        Me.btnViewMode.UseVisualStyleBackColor = False
        '
        'btnReassignPicker
        '
        Me.btnReassignPicker.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnReassignPicker.BackColor = System.Drawing.Color.White
        Me.btnReassignPicker.FlatAppearance.BorderColor = System.Drawing.Color.Gray
        Me.btnReassignPicker.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnReassignPicker.Font = New System.Drawing.Font("Century Gothic", 8.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnReassignPicker.Location = New System.Drawing.Point(612, 13)
        Me.btnReassignPicker.Name = "btnReassignPicker"
        Me.btnReassignPicker.Size = New System.Drawing.Size(115, 32)
        Me.btnReassignPicker.TabIndex = 3
        Me.btnReassignPicker.Text = "🔄 Reassign..."
        Me.btnReassignPicker.UseVisualStyleBackColor = False
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(59, Byte), Integer))
        Me.lblTitle.Location = New System.Drawing.Point(12, 10)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(340, 21)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "👥 Warehouse Picker Workload Summary"
        Me.lblTitle.UseCompatibleTextRendering = True
        '
        'pnlBottom
        '
        Me.pnlBottom.BackColor = System.Drawing.Color.Ivory
        Me.pnlBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlBottom.Controls.Add(Me.lblSummary)
        Me.pnlBottom.Controls.Add(Me.btnFilterCards)
        Me.pnlBottom.Controls.Add(Me.btnClearFilter)
        Me.pnlBottom.Controls.Add(Me.btnClose)
        Me.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBottom.Location = New System.Drawing.Point(0, 406)
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.Size = New System.Drawing.Size(740, 54)
        Me.pnlBottom.TabIndex = 1
        '
        'lblSummary
        '
        Me.lblSummary.Font = New System.Drawing.Font("Century Gothic", 8.5!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSummary.ForeColor = System.Drawing.Color.FromArgb(CType(CType(51, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(85, Byte), Integer))
        Me.lblSummary.Location = New System.Drawing.Point(12, 6)
        Me.lblSummary.Name = "lblSummary"
        Me.lblSummary.Size = New System.Drawing.Size(405, 40)
        Me.lblSummary.TabIndex = 3
        Me.lblSummary.Text = "Active Pickers: 0"
        Me.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblSummary.UseCompatibleTextRendering = True
        '
        'btnFilterCards
        '
        Me.btnFilterCards.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnFilterCards.BackColor = System.Drawing.Color.FromArgb(CType(CType(219, Byte), Integer), CType(CType(234, Byte), Integer), CType(CType(254, Byte), Integer))
        Me.btnFilterCards.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue
        Me.btnFilterCards.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnFilterCards.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnFilterCards.ForeColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(175, Byte), Integer))
        Me.btnFilterCards.Location = New System.Drawing.Point(426, 11)
        Me.btnFilterCards.Name = "btnFilterCards"
        Me.btnFilterCards.Size = New System.Drawing.Size(106, 32)
        Me.btnFilterCards.TabIndex = 2
        Me.btnFilterCards.Text = "Filter Cards"
        Me.btnFilterCards.UseVisualStyleBackColor = False
        '
        'btnClearFilter
        '
        Me.btnClearFilter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClearFilter.BackColor = System.Drawing.Color.White
        Me.btnClearFilter.FlatAppearance.BorderColor = System.Drawing.Color.Gray
        Me.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClearFilter.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClearFilter.Location = New System.Drawing.Point(538, 11)
        Me.btnClearFilter.Name = "btnClearFilter"
        Me.btnClearFilter.Size = New System.Drawing.Size(96, 32)
        Me.btnClearFilter.TabIndex = 1
        Me.btnClearFilter.Text = "Show All"
        Me.btnClearFilter.UseVisualStyleBackColor = False
        '
        'btnClose
        '
        Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClose.BackColor = System.Drawing.Color.White
        Me.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.Gray
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClose.Location = New System.Drawing.Point(640, 11)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(86, 32)
        Me.btnClose.TabIndex = 0
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'gridPickerSummary
        '
        Me.gridPickerSummary.AllowUserToAddRows = False
        Me.gridPickerSummary.AllowUserToDeleteRows = False
        Me.gridPickerSummary.AllowUserToResizeRows = False
        Me.gridPickerSummary.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridPickerSummary.BackgroundColor = System.Drawing.Color.White
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(241, Byte), Integer), CType(CType(245, Byte), Integer), CType(CType(249, Byte), Integer))
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(41, Byte), Integer), CType(CType(59, Byte), Integer))
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridPickerSummary.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.gridPickerSummary.ColumnHeadersHeight = 32
        Me.gridPickerSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridPickerSummary.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colPicker, Me.colInProcess, Me.colPrepared, Me.colTotalItems, Me.colOrders})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(219, Byte), Integer), CType(CType(234, Byte), Integer), CType(CType(254, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridPickerSummary.DefaultCellStyle = DataGridViewCellStyle2
        Me.gridPickerSummary.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridPickerSummary.EnableHeadersVisualStyles = False
        Me.gridPickerSummary.Location = New System.Drawing.Point(0, 60)
        Me.gridPickerSummary.MultiSelect = False
        Me.gridPickerSummary.Name = "gridPickerSummary"
        Me.gridPickerSummary.ReadOnly = True
        Me.gridPickerSummary.RowHeadersVisible = False
        Me.gridPickerSummary.RowTemplate.Height = 28
        Me.gridPickerSummary.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridPickerSummary.Size = New System.Drawing.Size(740, 346)
        Me.gridPickerSummary.TabIndex = 2
        '
        'colPicker
        '
        Me.colPicker.FillWeight = 120.0!
        Me.colPicker.HeaderText = "Picker Name"
        Me.colPicker.Name = "colPicker"
        Me.colPicker.ReadOnly = True
        '
        'colInProcess
        '
        Me.colInProcess.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colInProcess.FillWeight = 75.0!
        Me.colInProcess.HeaderText = "In Process"
        Me.colInProcess.Name = "colInProcess"
        Me.colInProcess.ReadOnly = True
        '
        'colPrepared
        '
        Me.colPrepared.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colPrepared.FillWeight = 75.0!
        Me.colPrepared.HeaderText = "Prepared"
        Me.colPrepared.Name = "colPrepared"
        Me.colPrepared.ReadOnly = True
        '
        'colTotalItems
        '
        Me.colTotalItems.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colTotalItems.FillWeight = 75.0!
        Me.colTotalItems.HeaderText = "Total Items"
        Me.colTotalItems.Name = "colTotalItems"
        Me.colTotalItems.ReadOnly = True
        '
        'colOrders
        '
        Me.colOrders.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colOrders.FillWeight = 75.0!
        Me.colOrders.HeaderText = "Orders"
        Me.colOrders.Name = "colOrders"
        Me.colOrders.ReadOnly = True
        '
        'timerRefresh
        '
        Me.timerRefresh.Interval = 5000
        '
        'frmPickerSummary
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(740, 460)
        Me.Controls.Add(Me.gridPickerSummary)
        Me.Controls.Add(Me.pnlBottom)
        Me.Controls.Add(Me.pnlTop)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmPickerSummary"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Warehouse Picker Workload Summary"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlBottom.ResumeLayout(False)
        Me.pnlBottom.PerformLayout()
        CType(Me.gridPickerSummary, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblSubtitle As System.Windows.Forms.Label
    Friend WithEvents pnlBottom As System.Windows.Forms.Panel
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents btnFilterCards As System.Windows.Forms.Button
    Friend WithEvents btnClearFilter As System.Windows.Forms.Button
    Friend WithEvents lblSummary As System.Windows.Forms.Label
    Friend WithEvents gridPickerSummary As System.Windows.Forms.DataGridView
    Friend WithEvents colPicker As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colInProcess As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPrepared As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotalItems As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colOrders As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents timerRefresh As System.Windows.Forms.Timer
    Friend WithEvents btnViewMode As System.Windows.Forms.Button
    Friend WithEvents btnReassignPicker As System.Windows.Forms.Button
End Class
