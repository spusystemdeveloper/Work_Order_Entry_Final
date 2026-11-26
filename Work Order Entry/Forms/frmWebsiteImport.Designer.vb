<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmWebsiteImport
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
        Dim ImportBodyStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim ImportHeaderStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim ImportAlternateStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim ImportOrderStyle As New System.Windows.Forms.DataGridViewCellStyle()

        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.gridOrders = New System.Windows.Forms.DataGridView()
        Me.ImportOrderNumber = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ImportCustomerName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ImportOrderDate = New System.Windows.Forms.DataGridViewTextBoxColumn()

        Me.btnImport = New System.Windows.Forms.Button()
        Me.chkShowAll = New System.Windows.Forms.CheckBox()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnRefresh = New System.Windows.Forms.Button()
        CType(Me.gridOrders, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gridOrders
        '
        Me.gridOrders.AllowUserToAddRows = False
        Me.gridOrders.AllowUserToDeleteRows = False
        Me.gridOrders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridOrders.Location = New System.Drawing.Point(12, 60)
        Me.gridOrders.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.gridOrders.MultiSelect = False
        ImportBodyStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        ImportBodyStyle.BackColor = System.Drawing.Color.White
        ImportBodyStyle.ForeColor = System.Drawing.SystemColors.ControlText
        ImportBodyStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        ImportBodyStyle.SelectionForeColor = System.Drawing.Color.White
        ImportBodyStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        ImportHeaderStyle.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        ImportHeaderStyle.BackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        ImportHeaderStyle.ForeColor = System.Drawing.SystemColors.ControlText
        ImportHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        ImportHeaderStyle.SelectionForeColor = System.Drawing.SystemColors.ControlText
        ImportHeaderStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        ImportAlternateStyle.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.gridOrders.DefaultCellStyle = ImportBodyStyle
        Me.gridOrders.ColumnHeadersDefaultCellStyle = ImportHeaderStyle
        Me.gridOrders.AlternatingRowsDefaultCellStyle = ImportAlternateStyle
        Me.gridOrders.ColumnHeadersHeight = 34
        Me.gridOrders.EnableHeadersVisualStyles = False
        Me.gridOrders.BackgroundColor = System.Drawing.Color.White
        Me.gridOrders.GridColor = System.Drawing.Color.FromArgb(215, 223, 233)
        Me.gridOrders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.gridOrders.RowTemplate.MinimumHeight = 40
        Me.gridOrders.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.gridOrders.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.ImportOrderNumber, Me.ImportCustomerName, Me.ImportOrderDate})
        Me.gridOrders.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gridOrders.Name = "gridOrders"
        Me.gridOrders.ReadOnly = True
        Me.gridOrders.RowHeadersVisible = False
        Me.gridOrders.RowTemplate.Height = 40
        Me.gridOrders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridOrders.Size = New System.Drawing.Size(836, 384)
        Me.gridOrders.TabIndex = 1
        '
        'btnImport
        '
        Me.btnImport.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.btnImport.Location = New System.Drawing.Point(696, 458)
        Me.btnImport.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.btnImport.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.btnImport.ForeColor = System.Drawing.Color.White
        Me.btnImport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(152, 36)
        Me.btnImport.TabIndex = 2
        Me.btnImport.Text = "Import Order"
        Me.btnImport.UseVisualStyleBackColor = False
        '
        'chkShowAll
        '
        Me.chkShowAll.AutoSize = True
        Me.chkShowAll.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.chkShowAll.Location = New System.Drawing.Point(12, 466)
        Me.chkShowAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkShowAll.Name = "chkShowAll"
        Me.chkShowAll.Size = New System.Drawing.Size(130, 22)
        Me.chkShowAll.TabIndex = 3
        Me.chkShowAll.Text = "Show All"
        Me.chkShowAll.UseVisualStyleBackColor = True
        '
        'txtSearch
        '
        Me.txtSearch.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtSearch.Location = New System.Drawing.Point(92, 16)
        Me.txtSearch.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(628, 25)
        Me.txtSearch.TabIndex = 4
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(12, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(65, 18)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Search :"
        '
        'btnRefresh
        '
        Me.btnRefresh.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.btnRefresh.Location = New System.Drawing.Point(732, 12)
        Me.btnRefresh.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.btnRefresh.BackColor = System.Drawing.Color.White
        Me.btnRefresh.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnRefresh.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(116, 34)
        Me.btnRefresh.TabIndex = 6
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = False
        '
        'frmWebsiteImport
        '
        Me.ImportOrderNumber.Name = "ImportOrderNumber"
        ImportOrderStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.ImportOrderNumber.DefaultCellStyle = ImportOrderStyle
        Me.ImportOrderNumber.DataPropertyName = "OrderID"
        Me.ImportOrderNumber.HeaderText = "Order Number"
        Me.ImportOrderNumber.ReadOnly = True
        Me.ImportOrderNumber.MinimumWidth = 140
        Me.ImportOrderNumber.Width = 140
        Me.ImportOrderNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.ImportCustomerName.Name = "ImportCustomerName"
        Me.ImportCustomerName.DataPropertyName = "CustomerName"
        Me.ImportCustomerName.HeaderText = "Customer Name"
        Me.ImportCustomerName.ReadOnly = True
        Me.ImportCustomerName.MinimumWidth = 300
        Me.ImportCustomerName.Width = 450
        Me.ImportCustomerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.ImportOrderDate.Name = "ImportOrderDate"
        Me.ImportOrderDate.DataPropertyName = "DateOrder"
        Me.ImportOrderDate.HeaderText = "Order Date Placed"
        Me.ImportOrderDate.ReadOnly = True
        Me.ImportOrderDate.MinimumWidth = 220
        Me.ImportOrderDate.Width = 220
        Me.ImportOrderDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(860, 510)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.chkShowAll)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.gridOrders)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Name = "frmWebsiteImport"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "WEBSITE : Select Order To Import"
        CType(Me.gridOrders, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ImportOrderNumber As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents ImportCustomerName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents ImportOrderDate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents gridOrders As System.Windows.Forms.DataGridView
    Friend WithEvents btnImport As System.Windows.Forms.Button
    Friend WithEvents chkShowAll As System.Windows.Forms.CheckBox
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
End Class
