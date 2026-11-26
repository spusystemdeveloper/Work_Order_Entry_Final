<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmImportItems
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

        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.griditems = New System.Windows.Forms.DataGridView()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Label1 = New System.Windows.Forms.Label()
        CType(Me.griditems, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'griditems
        '
        Me.griditems.AllowUserToAddRows = False
        Me.griditems.AllowUserToDeleteRows = False
        Me.griditems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.griditems.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.griditems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.griditems.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column2})
        Me.griditems.Location = New System.Drawing.Point(12, 46)
        Me.griditems.Margin = New System.Windows.Forms.Padding(4)
        Me.griditems.MultiSelect = False
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
        Me.griditems.DefaultCellStyle = ImportBodyStyle
        Me.griditems.ColumnHeadersDefaultCellStyle = ImportHeaderStyle
        Me.griditems.AlternatingRowsDefaultCellStyle = ImportAlternateStyle
        Me.griditems.ColumnHeadersHeight = 34
        Me.griditems.EnableHeadersVisualStyles = False
        Me.griditems.BackgroundColor = System.Drawing.Color.White
        Me.griditems.GridColor = System.Drawing.Color.FromArgb(215, 223, 233)
        Me.griditems.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.griditems.RowTemplate.Height = 40
        Me.griditems.RowTemplate.MinimumHeight = 40
        Me.griditems.Name = "griditems"
        Me.griditems.ReadOnly = True
        Me.griditems.RowHeadersVisible = False
        Me.griditems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.griditems.Size = New System.Drawing.Size(756, 302)
        Me.griditems.TabIndex = 0
        '
        'Column2
        '
        Me.Column2.FillWeight = 98.47716!
        Me.Column2.HeaderText = "Description"
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 14)
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(740, 22)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Search using the Description"
        '
        'frmImportItems
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(780, 360)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.griditems)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmImportItems"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Please Manually Search and Add the Replacement for this Item(s)."
        Me.TopMost = True
        CType(Me.griditems, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents griditems As System.Windows.Forms.DataGridView
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
