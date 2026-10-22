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
        Me.optQuote = New System.Windows.Forms.RadioButton()
        Me.optWork = New System.Windows.Forms.RadioButton()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.gridOrder = New System.Windows.Forms.DataGridView()
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
        Me.GroupBox1.Size = New System.Drawing.Size(736, 31)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(546, 9)
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
        Me.gridOrder.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        Me.gridOrder.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridOrder.Location = New System.Drawing.Point(14, 77)
        Me.gridOrder.Name = "gridOrder"
        Me.gridOrder.ReadOnly = True
        Me.gridOrder.RowHeadersVisible = False
        Me.gridOrder.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridOrder.Size = New System.Drawing.Size(736, 297)
        Me.gridOrder.TabIndex = 3
        '
        'cmdOK
        '
        Me.cmdOK.Location = New System.Drawing.Point(594, 381)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 4
        Me.cmdOK.Text = "&OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(675, 381)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 5
        Me.cmdCancel.Text = "&Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'txtSearch
        '
        Me.txtSearch.Location = New System.Drawing.Point(14, 51)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(736, 20)
        Me.txtSearch.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 35)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(82, 13)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "&Lookup Order#:"
        '
        'lblRec
        '
        Me.lblRec.Location = New System.Drawing.Point(594, 35)
        Me.lblRec.Name = "lblRec"
        Me.lblRec.Size = New System.Drawing.Size(153, 13)
        Me.lblRec.TabIndex = 7
        Me.lblRec.Text = "Label2"
        Me.lblRec.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'frmRecall
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(760, 411)
        Me.Controls.Add(Me.lblRec)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.gridOrder)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
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
End Class
