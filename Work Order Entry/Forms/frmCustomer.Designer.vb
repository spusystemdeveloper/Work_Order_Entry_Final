<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCustomer
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
        Dim lookupBooleanStyle5 As New System.Windows.Forms.DataGridViewCellStyle()
        Dim lookupBooleanStyle3 As New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.gridCustomer = New System.Windows.Forms.DataGridView()
        Me.lookupAccountNumber = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupCompany = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupPriceLevel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupEmployee = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.lookupTitle = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupTaxExempt = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.lookupID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupCustomText2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lookupCustomText4 = New System.Windows.Forms.DataGridViewTextBoxColumn()

        Me.txtCompany = New System.Windows.Forms.TextBox()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOk = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        CType(Me.gridCustomer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gridCustomer
        '
        Me.gridCustomer.AllowUserToAddRows = False
        Me.gridCustomer.AllowUserToDeleteRows = False
        Me.gridCustomer.AllowUserToOrderColumns = True
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.gridCustomer.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridCustomer.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.gridCustomer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle3.Font = New System.Drawing.Font("Tahoma", 11.0!, System.Drawing.FontStyle.Regular)
        DataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White
        DataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridCustomer.DefaultCellStyle = DataGridViewCellStyle3
        Me.gridCustomer.Location = New System.Drawing.Point(12, 72)
        Me.gridCustomer.ColumnHeadersHeight = 32
        Me.gridCustomer.EnableHeadersVisualStyles = False
        Me.gridCustomer.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.gridCustomer.RowTemplate.Height = 34
        Me.gridCustomer.RowTemplate.MinimumHeight = 34
        Me.gridCustomer.BackgroundColor = System.Drawing.Color.White
        Me.gridCustomer.GridColor = System.Drawing.Color.FromArgb(215, 223, 233)
        Me.gridCustomer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.gridCustomer.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.lookupAccountNumber, Me.lookupCompany, Me.lookupPriceLevel, Me.lookupEmployee, Me.lookupTitle, Me.lookupTaxExempt, Me.lookupID, Me.lookupCustomText2, Me.lookupCustomText4})
        Me.gridCustomer.Name = "gridCustomer"
        Me.gridCustomer.ReadOnly = True
        Me.gridCustomer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridCustomer.Size = New System.Drawing.Size(636, 260)
        Me.gridCustomer.TabIndex = 0
        '
        'txtCompany
        '
        Me.txtCompany.Font = New System.Drawing.Font("Tahoma", 11.0!, System.Drawing.FontStyle.Regular)
        Me.txtCompany.Location = New System.Drawing.Point(12, 36)
        Me.txtCompany.BackColor = System.Drawing.Color.FromArgb(245, 249, 255)
        Me.txtCompany.Name = "txtCompany"
        Me.txtCompany.Size = New System.Drawing.Size(636, 26)
        Me.txtCompany.TabIndex = 0
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(556, 346)
        Me.cmdCancel.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.cmdCancel.BackColor = System.Drawing.Color.White
        Me.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(92, 32)
        Me.cmdCancel.TabIndex = 2
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdOk
        '
        Me.cmdOk.Location = New System.Drawing.Point(452, 346)
        Me.cmdOk.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.cmdOk.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.cmdOk.ForeColor = System.Drawing.Color.White
        Me.cmdOk.Name = "cmdOk"
        Me.cmdOk.Size = New System.Drawing.Size(92, 32)
        Me.cmdOk.TabIndex = 3
        Me.cmdOk.Text = "OK"
        Me.cmdOk.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(12, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(400, 20)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Lookup Company:"
        '
        'frmCustomer
        '
        Me.lookupAccountNumber.Name = "lookupAccountNumber"
        Me.lookupAccountNumber.DataPropertyName = "AccountNumber"
        Me.lookupAccountNumber.HeaderText = "Account Number"
        Me.lookupAccountNumber.ReadOnly = True
        Me.lookupAccountNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupAccountNumber.MinimumWidth = 140
        Me.lookupAccountNumber.Width = 140
        Me.lookupCompany.Name = "lookupCompany"
        Me.lookupCompany.DataPropertyName = "Company"
        Me.lookupCompany.HeaderText = "Company"
        Me.lookupCompany.ReadOnly = True
        Me.lookupCompany.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.lookupCompany.MinimumWidth = 360
        Me.lookupCompany.Width = 360
        Me.lookupPriceLevel.Name = "lookupPriceLevel"
        Me.lookupPriceLevel.DataPropertyName = "PriceLevel"
        Me.lookupPriceLevel.HeaderText = "PriceLevel"
        Me.lookupPriceLevel.ReadOnly = True
        Me.lookupPriceLevel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupPriceLevel.MinimumWidth = 100
        Me.lookupPriceLevel.Width = 100
        Me.lookupPriceLevel.Visible = False
        Me.lookupEmployee.Name = "lookupEmployee"
        Me.lookupEmployee.DataPropertyName = "Employee"
        Me.lookupEmployee.HeaderText = "Employee"
        Me.lookupEmployee.ReadOnly = True
        Me.lookupEmployee.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupEmployee.MinimumWidth = 100
        Me.lookupEmployee.Width = 100
        Me.lookupEmployee.Visible = False
        lookupBooleanStyle3.NullValue = False
        Me.lookupEmployee.DefaultCellStyle = lookupBooleanStyle3
        Me.lookupTitle.Name = "lookupTitle"
        Me.lookupTitle.DataPropertyName = "Title"
        Me.lookupTitle.HeaderText = "Title"
        Me.lookupTitle.ReadOnly = True
        Me.lookupTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupTitle.MinimumWidth = 100
        Me.lookupTitle.Width = 100
        Me.lookupTitle.Visible = False
        Me.lookupTaxExempt.Name = "lookupTaxExempt"
        Me.lookupTaxExempt.DataPropertyName = "TaxExempt"
        Me.lookupTaxExempt.HeaderText = "TaxExempt"
        Me.lookupTaxExempt.ReadOnly = True
        Me.lookupTaxExempt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupTaxExempt.MinimumWidth = 100
        Me.lookupTaxExempt.Width = 100
        Me.lookupTaxExempt.Visible = False
        lookupBooleanStyle5.NullValue = False
        Me.lookupTaxExempt.DefaultCellStyle = lookupBooleanStyle5
        Me.lookupID.Name = "lookupID"
        Me.lookupID.DataPropertyName = "ID"
        Me.lookupID.HeaderText = "ID"
        Me.lookupID.ReadOnly = True
        Me.lookupID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupID.MinimumWidth = 70
        Me.lookupID.Width = 70
        Me.lookupID.Visible = False
        Me.lookupCustomText2.Name = "lookupCustomText2"
        Me.lookupCustomText2.DataPropertyName = "CustomText2"
        Me.lookupCustomText2.HeaderText = "CustomText2"
        Me.lookupCustomText2.ReadOnly = True
        Me.lookupCustomText2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupCustomText2.MinimumWidth = 100
        Me.lookupCustomText2.Width = 100
        Me.lookupCustomText2.Visible = False
        Me.lookupCustomText4.Name = "lookupCustomText4"
        Me.lookupCustomText4.DataPropertyName = "CustomText4"
        Me.lookupCustomText4.HeaderText = "Customer Type"
        Me.lookupCustomText4.ReadOnly = True
        Me.lookupCustomText4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.lookupCustomText4.MinimumWidth = 140
        Me.lookupCustomText4.Width = 140
        Me.lookupCustomText4.Visible = False
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(660, 390)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmdOk)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.txtCompany)
        Me.Controls.Add(Me.gridCustomer)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "frmCustomer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Customers"
        Me.TopMost = True
        CType(Me.gridCustomer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lookupAccountNumber As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupCompany As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupPriceLevel As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupEmployee As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents lookupTitle As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupTaxExempt As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents lookupID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupCustomText2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lookupCustomText4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents txtCompany As System.Windows.Forms.TextBox
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdOk As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents gridCustomer As System.Windows.Forms.DataGridView

End Class
