<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUploader
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

        Me.cmdImport = New System.Windows.Forms.Button()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.cmdReport = New System.Windows.Forms.Button()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.panelInfo = New System.Windows.Forms.Panel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblFile = New System.Windows.Forms.Label()
        Me.gridInfo = New System.Windows.Forms.DataGridView()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Panel7 = New System.Windows.Forms.Panel()
        Me.Panel6 = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lblError = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblSuccess = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblTotal = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.panelInfo.SuspendLayout()
        CType(Me.gridInfo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel5.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.Panel6.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdImport
        '
        Me.cmdImport.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        Me.cmdImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdImport.Location = New System.Drawing.Point(588, 12)
        Me.cmdImport.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.cmdImport.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.cmdImport.ForeColor = System.Drawing.Color.White
        Me.cmdImport.Name = "cmdImport"
        Me.cmdImport.Size = New System.Drawing.Size(108, 32)
        Me.cmdImport.TabIndex = 5
        Me.cmdImport.Text = "&Import"
        Me.cmdImport.UseVisualStyleBackColor = False
        '
        'cmdClose
        '
        Me.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdClose.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdClose.Location = New System.Drawing.Point(708, 12)
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.cmdClose.BackColor = System.Drawing.Color.White
        Me.cmdClose.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(116, 32)
        Me.cmdClose.TabIndex = 6
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(840, 76)
        Me.Panel1.TabIndex = 9
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.Label2.Location = New System.Drawing.Point(16, 40)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(790, 20)
        Me.Label2.TabIndex = 11
        Me.Label2.Text = "This will import the contents of the file in Work/Sales Order Addin. "
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(16, 12)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(300, 20)
        Me.Label1.TabIndex = 10
        Me.Label1.Text = "Importing the File"
        '
        'Panel2
        '
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.cmdReport)
        Me.Panel2.Controls.Add(Me.cmdImport)
        Me.Panel2.Controls.Add(Me.cmdClose)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel2.Location = New System.Drawing.Point(0, 484)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(840, 56)
        Me.Panel2.TabIndex = 10
        '
        'cmdReport
        '
        Me.cmdReport.Enabled = False
        Me.cmdReport.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.cmdReport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.cmdReport.Location = New System.Drawing.Point(456, 12)
        Me.cmdReport.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.cmdReport.BackColor = System.Drawing.Color.White
        Me.cmdReport.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdReport.Name = "cmdReport"
        Me.cmdReport.Size = New System.Drawing.Size(120, 32)
        Me.cmdReport.TabIndex = 7
        Me.cmdReport.Text = "&Report"
        Me.cmdReport.UseVisualStyleBackColor = False
        Me.cmdReport.Visible = False
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.SystemColors.Control
        Me.Panel3.Controls.Add(Me.panelInfo)
        Me.Panel3.Controls.Add(Me.gridInfo)
        Me.Panel3.Controls.Add(Me.Panel5)
        Me.Panel3.Controls.Add(Me.Panel4)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel3.Location = New System.Drawing.Point(0, 76)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Padding = New System.Windows.Forms.Padding(16)
        Me.Panel3.Size = New System.Drawing.Size(840, 408)
        Me.Panel3.TabIndex = 11
        '
        'panelInfo
        '
        Me.panelInfo.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.panelInfo.Controls.Add(Me.Label3)
        Me.panelInfo.Controls.Add(Me.lblFile)
        Me.panelInfo.Location = New System.Drawing.Point(16, 110)
        Me.panelInfo.Name = "panelInfo"
        Me.panelInfo.Size = New System.Drawing.Size(808, 282)
        Me.panelInfo.TabIndex = 16
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Label3.Location = New System.Drawing.Point(16, 18)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(760, 22)
        Me.Label3.TabIndex = 12
        Me.Label3.Text = "Click Import to begin the importing of the file:"
        '
        'lblFile
        '
        Me.lblFile.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.lblFile.Location = New System.Drawing.Point(16, 54)
        Me.lblFile.Name = "lblFile"
        Me.lblFile.Size = New System.Drawing.Size(776, 210)
        Me.lblFile.TabIndex = 11
        Me.lblFile.Text = "Filename to be imported."
        '
        'gridInfo
        '
        Me.gridInfo.BackgroundColor = System.Drawing.Color.White
        Me.gridInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.gridInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gridInfo.Location = New System.Drawing.Point(16, 110)
        ImportBodyStyle.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        ImportBodyStyle.BackColor = System.Drawing.Color.White
        ImportBodyStyle.ForeColor = System.Drawing.SystemColors.ControlText
        ImportBodyStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        ImportBodyStyle.SelectionForeColor = System.Drawing.Color.White
        ImportBodyStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        ImportHeaderStyle.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold)
        ImportHeaderStyle.BackColor = System.Drawing.Color.FromArgb(232, 238, 246)
        ImportHeaderStyle.ForeColor = System.Drawing.SystemColors.ControlText
        ImportHeaderStyle.SelectionBackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        ImportHeaderStyle.SelectionForeColor = System.Drawing.Color.White
        ImportHeaderStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        ImportAlternateStyle.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.gridInfo.DefaultCellStyle = ImportBodyStyle
        Me.gridInfo.ColumnHeadersDefaultCellStyle = ImportHeaderStyle
        Me.gridInfo.AlternatingRowsDefaultCellStyle = ImportAlternateStyle
        Me.gridInfo.ColumnHeadersHeight = 34
        Me.gridInfo.EnableHeadersVisualStyles = False
        Me.gridInfo.GridColor = System.Drawing.Color.FromArgb(215, 223, 233)
        Me.gridInfo.RowTemplate.Height = 40
        Me.gridInfo.RowTemplate.MinimumHeight = 40
        Me.gridInfo.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
        Me.gridInfo.Name = "gridInfo"
        Me.gridInfo.RowHeadersVisible = False
        Me.gridInfo.Size = New System.Drawing.Size(808, 282)
        Me.gridInfo.TabIndex = 15
        '
        'Panel5
        '
        Me.Panel5.Controls.Add(Me.Label10)
        Me.Panel5.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel5.Location = New System.Drawing.Point(16, 80)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(808, 30)
        Me.Panel5.TabIndex = 13
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(0, 7)
        Me.Label10.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(120, 18)
        Me.Label10.TabIndex = 2
        Me.Label10.Text = "&Details:"
        '
        'Panel4
        '
        Me.Panel4.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.Panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Panel4.Controls.Add(Me.Panel7)
        Me.Panel4.Controls.Add(Me.Panel6)
        Me.Panel4.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel4.Location = New System.Drawing.Point(16, 16)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(808, 64)
        Me.Panel4.TabIndex = 12
        '
        'Panel7
        '
        Me.Panel7.Dock = System.Windows.Forms.DockStyle.Left
        Me.Panel7.Location = New System.Drawing.Point(0, 0)
        Me.Panel7.Name = "Panel7"
        Me.Panel7.Size = New System.Drawing.Size(0, 60)
        Me.Panel7.TabIndex = 7
        '
        'Panel6
        '
        Me.Panel6.Controls.Add(Me.Label4)
        Me.Panel6.Controls.Add(Me.lblError)
        Me.Panel6.Controls.Add(Me.Label5)
        Me.Panel6.Controls.Add(Me.lblSuccess)
        Me.Panel6.Controls.Add(Me.Label6)
        Me.Panel6.Controls.Add(Me.lblTotal)
        Me.Panel6.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel6.Location = New System.Drawing.Point(0, 0)
        Me.Panel6.Name = "Panel6"
        Me.Panel6.Size = New System.Drawing.Size(804, 60)
        Me.Panel6.TabIndex = 6
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 22)
        Me.Label4.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(110, 24)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = "Total"
        '
        'lblError
        '
        Me.lblError.AutoSize = False
        Me.lblError.Location = New System.Drawing.Point(600, 20)
        Me.lblError.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblError.Name = "lblError"
        Me.lblError.Size = New System.Drawing.Size(110, 24)
        Me.lblError.TabIndex = 5
        Me.lblError.Text = "0"
        Me.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(270, 22)
        Me.Label5.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(110, 24)
        Me.Label5.TabIndex = 1
        Me.Label5.Text = "Success"
        '
        'lblSuccess
        '
        Me.lblSuccess.AutoSize = False
        Me.lblSuccess.Location = New System.Drawing.Point(346, 20)
        Me.lblSuccess.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblSuccess.Name = "lblSuccess"
        Me.lblSuccess.Size = New System.Drawing.Size(110, 24)
        Me.lblSuccess.TabIndex = 4
        Me.lblSuccess.Text = "0"
        Me.lblSuccess.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(542, 22)
        Me.Label6.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Regular)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(110, 24)
        Me.Label6.TabIndex = 2
        Me.Label6.Text = "Error"
        '
        'lblTotal
        '
        Me.lblTotal.AutoSize = False
        Me.lblTotal.Location = New System.Drawing.Point(82, 20)
        Me.lblTotal.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.lblTotal.Name = "lblTotal"
        Me.lblTotal.Size = New System.Drawing.Size(110, 24)
        Me.lblTotal.TabIndex = 3
        Me.lblTotal.Text = "0"
        Me.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'frmUploader
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(840, 540)
        Me.Controls.Add(Me.Panel3)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmUploader"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Import Quotation"
        Me.TopMost = True
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.panelInfo.ResumeLayout(False)
        Me.panelInfo.PerformLayout()
        CType(Me.gridInfo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        Me.Panel4.ResumeLayout(False)
        Me.Panel6.ResumeLayout(False)
        Me.Panel6.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents cmdImport As System.Windows.Forms.Button
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Panel4 As System.Windows.Forms.Panel
    Friend WithEvents lblError As System.Windows.Forms.Label
    Friend WithEvents lblSuccess As System.Windows.Forms.Label
    Friend WithEvents lblTotal As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents panelInfo As System.Windows.Forms.Panel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents gridInfo As System.Windows.Forms.DataGridView
    Friend WithEvents Panel5 As System.Windows.Forms.Panel
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Panel6 As System.Windows.Forms.Panel
    Friend WithEvents Panel7 As System.Windows.Forms.Panel
    Friend WithEvents cmdReport As System.Windows.Forms.Button
End Class
