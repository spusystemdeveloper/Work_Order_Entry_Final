<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmRecall
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
        Dim headerStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim cellStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim alternateStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim orderStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Dim wrappedStyle As New System.Windows.Forms.DataGridViewCellStyle()
        Me.optQuote = New System.Windows.Forms.RadioButton()
        Me.optWork = New System.Windows.Forms.RadioButton()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.gridOrder = New System.Windows.Forms.DataGridView()
        Me.recallOrderNumber = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.recallDate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.recallReference = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.recallCustomer = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.recallComment = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblRec = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        CType(Me.gridOrder, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'optQuote
        '
        Me.optQuote.AutoSize = True
        Me.optQuote.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.optQuote.Location = New System.Drawing.Point(30, 9)
        Me.optQuote.Name = "optQuote"
        Me.optQuote.Size = New System.Drawing.Size(100, 17)
        Me.optQuote.TabIndex = 0
        Me.optQuote.Text = "Sales Quotation"
        Me.optQuote.UseVisualStyleBackColor = True
        '
        'optWork
        '
        Me.optWork.AutoSize = True
        Me.optWork.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.optWork.Location = New System.Drawing.Point(173, 9)
        Me.optWork.Name = "optWork"
        Me.optWork.Size = New System.Drawing.Size(80, 17)
        Me.optWork.TabIndex = 1
        Me.optWork.Text = "Work Order"
        Me.optWork.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.CheckBox1)
        Me.GroupBox1.Controls.Add(Me.optQuote)
        Me.GroupBox1.Controls.Add(Me.optWork)
        Me.GroupBox1.Location = New System.Drawing.Point(14, 1)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Size = New System.Drawing.Size(1076, 31)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.CheckBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CheckBox1.Location = New System.Drawing.Point(830, 9)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(190, 17)
        Me.CheckBox1.TabIndex = 2
        Me.CheckBox1.Text = "Show All Entries for all Order Taker"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'gridOrder
        '
        Me.gridOrder.AllowUserToAddRows = False
        Me.gridOrder.AllowUserToDeleteRows = False
        Me.gridOrder.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gridOrder.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.gridOrder.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.recallOrderNumber, Me.recallDate, Me.recallReference, Me.recallCustomer, Me.recallComment})
        headerStyle.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        headerStyle.BackColor = System.Drawing.Color.FromArgb(232, 237, 244)
        headerStyle.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55)
        headerStyle.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        headerStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridOrder.ColumnHeadersDefaultCellStyle = headerStyle
        cellStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        cellStyle.BackColor = System.Drawing.Color.White
        cellStyle.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55)
        cellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        cellStyle.SelectionForeColor = System.Drawing.Color.White
        cellStyle.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        cellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        cellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridOrder.DefaultCellStyle = cellStyle
        alternateStyle.BackColor = System.Drawing.Color.FromArgb(246, 248, 251)
        Me.gridOrder.AlternatingRowsDefaultCellStyle = alternateStyle
        Me.gridOrder.EnableHeadersVisualStyles = False
        Me.gridOrder.GridColor = System.Drawing.Color.FromArgb(218, 224, 232)
        Me.gridOrder.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.gridOrder.RowTemplate.MinimumHeight = 44
        Me.gridOrder.RowTemplate.Height = 44
        Me.gridOrder.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        Me.gridOrder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridOrder.ColumnHeadersHeight = 34
        Me.gridOrder.Location = New System.Drawing.Point(14, 77)
        Me.gridOrder.Name = "gridOrder"
        Me.gridOrder.ReadOnly = True
        Me.gridOrder.RowHeadersVisible = False
        Me.gridOrder.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridOrder.Size = New System.Drawing.Size(1076, 426)
        Me.gridOrder.TabIndex = 3
        '
        'recallOrderNumber
        '
        Me.recallOrderNumber.Name = "Order#"
        Me.recallOrderNumber.DataPropertyName = "Order#"
        Me.recallOrderNumber.HeaderText = "Order#"
        Me.recallOrderNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.recallOrderNumber.Width = 100
        orderStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.recallOrderNumber.DefaultCellStyle = orderStyle
        '
        'recallDate
        '
        Me.recallDate.Name = "Date"
        Me.recallDate.DataPropertyName = "Date"
        Me.recallDate.HeaderText = "Date"
        Me.recallDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.recallDate.Width = 190
        '
        'recallReference
        '
        Me.recallReference.Name = "Reference"
        Me.recallReference.DataPropertyName = "Reference"
        Me.recallReference.HeaderText = "Reference"
        Me.recallReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.recallReference.Width = 110
        '
        'recallCustomer
        '
        Me.recallCustomer.Name = "Customer"
        Me.recallCustomer.DataPropertyName = "Customer"
        Me.recallCustomer.HeaderText = "Customer"
        Me.recallCustomer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.recallCustomer.MinimumWidth = 240
        Me.recallCustomer.FillWeight = 40.0!
        wrappedStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.recallCustomer.DefaultCellStyle = wrappedStyle
        '
        'recallComment
        '
        Me.recallComment.Name = "Comment"
        Me.recallComment.DataPropertyName = "Comment"
        Me.recallComment.HeaderText = "Comment"
        Me.recallComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.recallComment.MinimumWidth = 300
        Me.recallComment.FillWeight = 60.0!
        Me.recallComment.DefaultCellStyle = wrappedStyle
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdOK.Location = New System.Drawing.Point(934, 510)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 4
        Me.cmdOK.Text = "&OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdCancel.Location = New System.Drawing.Point(1015, 510)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 5
        Me.cmdCancel.Text = "&Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'txtSearch
        '
        Me.txtSearch.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSearch.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.txtSearch.Location = New System.Drawing.Point(14, 51)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(1076, 22)
        Me.txtSearch.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(12, 35)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(82, 13)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "&Lookup Order#:"
        '
        'lblRec
        '
        Me.lblRec.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblRec.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.lblRec.Location = New System.Drawing.Point(934, 35)
        Me.lblRec.Name = "lblRec"
        Me.lblRec.Size = New System.Drawing.Size(153, 14)
        Me.lblRec.TabIndex = 7
        Me.lblRec.Text = "Label2"
        Me.lblRec.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'frmRecall
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1100, 540)
        Me.Controls.Add(Me.lblRec)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.gridOrder)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.MinimumSize = New System.Drawing.Size(800, 450)
        Me.KeyPreview = True
        Me.Name = "frmRecall"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Recall"
        Me.TopMost = True
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.gridOrder, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents optQuote As System.Windows.Forms.RadioButton
    Friend WithEvents optWork As System.Windows.Forms.RadioButton
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents gridOrder As System.Windows.Forms.DataGridView
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblRec As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents recallOrderNumber As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents recallDate As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents recallReference As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents recallCustomer As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents recallComment As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
