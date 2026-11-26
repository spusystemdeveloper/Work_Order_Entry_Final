<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCheckStore
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
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim headerStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim rowStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim quantityStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.gridQty = New System.Windows.Forms.DataGridView()
        Me.storeID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.storeName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.storeCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.storeQuantity = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.storeAddress = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lblItemCode = New System.Windows.Forms.Label()
        Me.lblItemName = New System.Windows.Forms.Label()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        CType(Me.gridQty, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(72, 16)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Item Code:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 48)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(76, 16)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "Item Name:"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.gridQty)
        Me.GroupBox1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 104)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(736, 306)
        Me.GroupBox1.TabIndex = 7
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Interstore Availability"
        '
        'gridQty
        '
        Me.gridQty.AllowUserToAddRows = False
        Me.gridQty.AllowUserToDeleteRows = False
        Me.gridQty.AllowUserToOrderColumns = True
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(246, 248, 251)
        Me.gridQty.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.gridQty.AutoGenerateColumns = False
        Me.gridQty.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.storeID, Me.storeName, Me.storeCode, Me.storeQuantity, Me.storeAddress})
        Me.gridQty.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridQty.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.None
        Me.gridQty.BackgroundColor = System.Drawing.Color.White
        Me.gridQty.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular)
        Me.gridQty.EnableHeadersVisualStyles = False
        headerStyle.BackColor = System.Drawing.Color.FromArgb(232, 237, 244)
        headerStyle.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55)
        headerStyle.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        headerStyle.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        headerStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False
        headerStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 237, 244)
        headerStyle.SelectionForeColor = System.Drawing.Color.FromArgb(31, 41, 55)
        Me.gridQty.ColumnHeadersDefaultCellStyle = headerStyle
        Me.gridQty.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridQty.ColumnHeadersHeight = 32
        rowStyle.BackColor = System.Drawing.Color.White
        rowStyle.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55)
        rowStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        rowStyle.SelectionForeColor = System.Drawing.Color.White
        rowStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        rowStyle.Padding = New System.Windows.Forms.Padding(4, 2, 4, 2)
        rowStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False
        Me.gridQty.DefaultCellStyle = rowStyle
        Me.gridQty.GridColor = System.Drawing.Color.FromArgb(218, 224, 232)
        Me.gridQty.RowTemplate.Height = 30
        Me.gridQty.RowHeadersWidth = 36
        Me.gridQty.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.gridQty.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        Me.gridQty.Location = New System.Drawing.Point(8, 26)
        Me.gridQty.Name = "gridQty"
        Me.gridQty.ReadOnly = True
        Me.gridQty.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridQty.Size = New System.Drawing.Size(720, 272)
        Me.gridQty.TabIndex = 0
        Me.storeID.Name = "storeID"
        Me.storeID.DataPropertyName = "ID"
        Me.storeID.HeaderText = "ID"
        Me.storeID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.storeID.Width = 60
        Me.storeID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.storeName.Name = "storeName"
        Me.storeName.DataPropertyName = "Name"
        Me.storeName.HeaderText = "STORE NAME"
        Me.storeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.storeName.MinimumWidth = 180
        Me.storeName.FillWeight = 50.0!
        Me.storeName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.storeCode.Name = "storeCode"
        Me.storeCode.DataPropertyName = "StoreCode"
        Me.storeCode.HeaderText = "STORE CODE"
        Me.storeCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.storeCode.MinimumWidth = 180
        Me.storeCode.FillWeight = 50.0!
        Me.storeCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.storeQuantity.Name = "storeQuantity"
        Me.storeQuantity.DataPropertyName = "Qty"
        Me.storeQuantity.HeaderText = "QTY"
        Me.storeQuantity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.storeQuantity.MinimumWidth = 90
        Me.storeQuantity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        quantityStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.storeQuantity.DefaultCellStyle = quantityStyle
        Me.storeAddress.Name = "storeAddress"
        Me.storeAddress.DataPropertyName = "Address"
        Me.storeAddress.HeaderText = "ADDRESS"
        Me.storeAddress.Visible = False
        Me.storeAddress.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'lblItemCode
        '
        Me.lblItemCode.AutoSize = True
        Me.lblItemCode.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblItemCode.Location = New System.Drawing.Point(104, 16)
        Me.lblItemCode.Name = "lblItemCode"
        Me.lblItemCode.Size = New System.Drawing.Size(67, 16)
        Me.lblItemCode.TabIndex = 8
        Me.lblItemCode.Text = "I00001-PC"
        '
        'lblItemName
        '
        Me.lblItemName.AutoSize = False
        Me.lblItemName.AutoEllipsis = True
        Me.lblItemName.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblItemName.Location = New System.Drawing.Point(104, 44)
        Me.lblItemName.Name = "lblItemName"
        Me.lblItemName.Size = New System.Drawing.Size(640, 48)
        Me.lblItemName.TabIndex = 9
        Me.lblItemName.Text = "PAER ONE BOOK PAPER 8.5 X 11"
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right
        Me.cmdClose.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.cmdClose.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.cmdClose.ForeColor = System.Drawing.Color.White
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdClose.Location = New System.Drawing.Point(657, 426)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(91, 31)
        Me.cmdClose.TabIndex = 10
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = False
        '
        'frmCheckStore
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(760, 470)
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.lblItemName)
        Me.Controls.Add(Me.lblItemCode)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "frmCheckStore"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Check Interstore"
        Me.TopMost = True
        Me.GroupBox1.ResumeLayout(False)
        CType(Me.gridQty, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents lblItemCode As System.Windows.Forms.Label
    Friend WithEvents lblItemName As System.Windows.Forms.Label
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents gridQty As System.Windows.Forms.DataGridView
    Friend WithEvents storeID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents storeName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents storeCode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents storeQuantity As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents storeAddress As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
