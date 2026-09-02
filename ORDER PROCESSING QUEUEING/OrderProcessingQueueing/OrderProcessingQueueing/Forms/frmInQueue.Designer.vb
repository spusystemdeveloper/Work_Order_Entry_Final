<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmInQueue
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
        Me.gridGroupID = New System.Windows.Forms.DataGridView()
        Me.GroupID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lblType = New System.Windows.Forms.Label()
        CType(Me.gridGroupID, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'gridGroupID
        '
        Me.gridGroupID.AllowUserToAddRows = False
        Me.gridGroupID.AllowUserToDeleteRows = False
        Me.gridGroupID.AllowUserToResizeColumns = False
        Me.gridGroupID.AllowUserToResizeRows = False
        Me.gridGroupID.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridGroupID.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.gridGroupID.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridGroupID.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.GroupID})
        Me.gridGroupID.Location = New System.Drawing.Point(12, 35)
        Me.gridGroupID.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.gridGroupID.MultiSelect = False
        Me.gridGroupID.Name = "gridGroupID"
        Me.gridGroupID.ReadOnly = True
        Me.gridGroupID.RowHeadersVisible = False
        Me.gridGroupID.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.gridGroupID.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.gridGroupID.RowTemplate.Height = 30
        Me.gridGroupID.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridGroupID.Size = New System.Drawing.Size(179, 202)
        Me.gridGroupID.TabIndex = 0
        '
        'GroupID
        '
        Me.GroupID.HeaderText = "Groud ID"
        Me.GroupID.Name = "GroupID"
        Me.GroupID.ReadOnly = True
        '
        'lblType
        '
        Me.lblType.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblType.Location = New System.Drawing.Point(12, 5)
        Me.lblType.Name = "lblType"
        Me.lblType.Size = New System.Drawing.Size(179, 23)
        Me.lblType.TabIndex = 1
        Me.lblType.Text = "-----"
        Me.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'frmInQueue
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(205, 250)
        Me.Controls.Add(Me.lblType)
        Me.Controls.Add(Me.gridGroupID)
        Me.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "frmInQueue"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Status"
        CType(Me.gridGroupID, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents gridGroupID As System.Windows.Forms.DataGridView
    Friend WithEvents GroupID As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lblType As System.Windows.Forms.Label
End Class
