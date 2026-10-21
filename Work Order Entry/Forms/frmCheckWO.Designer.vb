<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmCheckWO
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
        Dim WOBodyStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim WOHeaderStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim WOOrderStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim WOQuantityStyle As New System.Windows.Forms.DataGridViewCellStyle()

        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.lblItemNameWO = New System.Windows.Forms.Label()
        Me.lblItemCodeWO = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.gridQtyWO = New System.Windows.Forms.DataGridView()
        Me.WOOrderId = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOCompany = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOAccount = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOItemCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WODescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOQuantity = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WOTaker = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WODate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.WODays = New System.Windows.Forms.DataGridViewTextBoxColumn()

        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        CType(Me.gridQtyWO, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdClose.Location = New System.Drawing.Point(875, 494)
        Me.cmdClose.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(91, 32)
        Me.cmdClose.TabIndex = 16
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'lblItemNameWO
        '
        Me.lblItemNameWO.AutoSize = False
        Me.lblItemNameWO.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.lblItemNameWO.Location = New System.Drawing.Point(110, 48)
        Me.lblItemNameWO.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblItemNameWO.Name = "lblItemNameWO"
        Me.lblItemNameWO.Size = New System.Drawing.Size(858, 48)
        Me.lblItemNameWO.TabIndex = 15
        Me.lblItemNameWO.Text = "PAER ONE BOOK PAPER 8.5 X 11"
        '
        'lblItemCodeWO
        '
        Me.lblItemCodeWO.AutoSize = False
        Me.lblItemCodeWO.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblItemCodeWO.Location = New System.Drawing.Point(110, 16)
        Me.lblItemCodeWO.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblItemCodeWO.Name = "lblItemCodeWO"
        Me.lblItemCodeWO.Size = New System.Drawing.Size(858, 24)
        Me.lblItemCodeWO.TabIndex = 14
        Me.lblItemCodeWO.Text = "I00001-PC"
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.gridQtyWO)
        Me.GroupBox1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(12, 108)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(960, 374)
        Me.GroupBox1.TabIndex = 13
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Open Work Order Item Availability"
        '
        'gridQtyWO
        '
        Me.gridQtyWO.AllowUserToAddRows = False
        Me.gridQtyWO.AllowUserToDeleteRows = False
        Me.gridQtyWO.AllowUserToOrderColumns = True
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.gridQtyWO.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.gridQtyWO.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gridQtyWO.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridQtyWO.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridQtyWO.Location = New System.Drawing.Point(10, 26)
        WOBodyStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        WOBodyStyle.BackColor = System.Drawing.Color.White
        WOBodyStyle.ForeColor = System.Drawing.SystemColors.ControlText
        WOBodyStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        WOBodyStyle.SelectionForeColor = System.Drawing.Color.White
        WOBodyStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        WOHeaderStyle.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        WOHeaderStyle.BackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        WOHeaderStyle.ForeColor = System.Drawing.SystemColors.ControlText
        WOHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        WOHeaderStyle.SelectionForeColor = System.Drawing.SystemColors.ControlText
        WOHeaderStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        WOOrderStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        WOQuantityStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.gridQtyWO.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.WOOrderId, Me.WOStatus, Me.WOCompany, Me.WOAccount, Me.WOItemCode, Me.WODescription, Me.WOQuantity, Me.WOTaker, Me.WODate, Me.WODays})
        Me.gridQtyWO.DefaultCellStyle = WOBodyStyle
        Me.gridQtyWO.ColumnHeadersDefaultCellStyle = WOHeaderStyle
        Me.gridQtyWO.EnableHeadersVisualStyles = False
        Me.gridQtyWO.BackgroundColor = System.Drawing.Color.White
        Me.gridQtyWO.GridColor = System.Drawing.Color.FromArgb(215, 223, 233)
        Me.gridQtyWO.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.gridQtyWO.ColumnHeadersHeight = 36
        Me.gridQtyWO.RowTemplate.MinimumHeight = 40
        Me.gridQtyWO.RowTemplate.Height = 40
        Me.gridQtyWO.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.gridQtyWO.Name = "gridQtyWO"
        Me.gridQtyWO.ReadOnly = True
        Me.gridQtyWO.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridQtyWO.Size = New System.Drawing.Size(940, 338)
        Me.gridQtyWO.TabIndex = 0
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label2.Location = New System.Drawing.Point(16, 50)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(90, 20)
        Me.Label2.TabIndex = 12
        Me.Label2.Text = "Item Name:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(16, 18)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(90, 20)
        Me.Label1.TabIndex = 11
        Me.Label1.Text = "Item Code:"
        '
        Me.WOOrderId.Name = "WOOrderId"
        Me.WOOrderId.DataPropertyName = "ID"
        Me.WOOrderId.HeaderText = "ORDER ID"
        Me.WOOrderId.ReadOnly = True
        Me.WOOrderId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOOrderId.MinimumWidth = 110
        Me.WOOrderId.Width = 110
        Me.WOOrderId.DefaultCellStyle = WOOrderStyle
        Me.WOStatus.Name = "WOStatus"
        Me.WOStatus.DataPropertyName = "Status"
        Me.WOStatus.HeaderText = "STATUS"
        Me.WOStatus.ReadOnly = True
        Me.WOStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOStatus.MinimumWidth = 90
        Me.WOStatus.Width = 90
        Me.WOCompany.Name = "WOCompany"
        Me.WOCompany.DataPropertyName = "Company"
        Me.WOCompany.HeaderText = "COMPANY"
        Me.WOCompany.ReadOnly = True
        Me.WOCompany.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.WOCompany.MinimumWidth = 240
        Me.WOCompany.Width = 300
        Me.WOAccount.Name = "WOAccount"
        Me.WOAccount.DataPropertyName = "AccountNumber"
        Me.WOAccount.HeaderText = "ACCOUNT NUMBER"
        Me.WOAccount.ReadOnly = True
        Me.WOAccount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOAccount.MinimumWidth = 160
        Me.WOAccount.Width = 160
        Me.WOItemCode.Name = "WOItemCode"
        Me.WOItemCode.DataPropertyName = "ItemLookupCode"
        Me.WOItemCode.HeaderText = "ITEMLOOKUP"
        Me.WOItemCode.ReadOnly = True
        Me.WOItemCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOItemCode.MinimumWidth = 100
        Me.WOItemCode.Width = 100
        Me.WOItemCode.Visible = False
        Me.WODescription.Name = "WODescription"
        Me.WODescription.DataPropertyName = "Description"
        Me.WODescription.HeaderText = "DESCRIPTION"
        Me.WODescription.ReadOnly = True
        Me.WODescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WODescription.MinimumWidth = 100
        Me.WODescription.Width = 100
        Me.WODescription.Visible = False
        Me.WOQuantity.Name = "WOQuantity"
        Me.WOQuantity.DataPropertyName = "QuantityOnOrder"
        Me.WOQuantity.HeaderText = "QTY ORDERED"
        Me.WOQuantity.ReadOnly = True
        Me.WOQuantity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOQuantity.MinimumWidth = 135
        Me.WOQuantity.Width = 135
        Me.WOQuantity.DefaultCellStyle = WOQuantityStyle
        Me.WOTaker.Name = "WOTaker"
        Me.WOTaker.DataPropertyName = "Name"
        Me.WOTaker.HeaderText = "ORDER TAKER"
        Me.WOTaker.ReadOnly = True
        Me.WOTaker.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WOTaker.MinimumWidth = 180
        Me.WOTaker.Width = 180
        Me.WODate.Name = "WODate"
        Me.WODate.DataPropertyName = "OrderDate"
        Me.WODate.HeaderText = "DateCreated"
        Me.WODate.ReadOnly = True
        Me.WODate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WODate.MinimumWidth = 125
        Me.WODate.Width = 125
        Me.WODays.Name = "WODays"
        Me.WODays.DataPropertyName = "DaysOpenLabel"
        Me.WODays.HeaderText = "No.Days"
        Me.WODays.ReadOnly = True
        Me.WODays.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.WODays.MinimumWidth = 90
        Me.WODays.Width = 90
        'frmCheckWO
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ClientSize = New System.Drawing.Size(984, 540)
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.lblItemNameWO)
        Me.Controls.Add(Me.lblItemCodeWO)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.MinimumSize = New System.Drawing.Size(1000, 400)
        Me.Name = "frmCheckWO"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Check Open Work Orders"
        Me.GroupBox1.ResumeLayout(False)
        CType(Me.gridQtyWO, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents WOOrderId As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOStatus As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOCompany As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOAccount As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOItemCode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WODescription As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOQuantity As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WOTaker As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WODate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents WODays As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents cmdClose As Button
    Friend WithEvents lblItemNameWO As Label
    Friend WithEvents lblItemCodeWO As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents gridQtyWO As DataGridView
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
End Class
