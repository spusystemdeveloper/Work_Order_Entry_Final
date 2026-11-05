<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmPassword
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
        Me.lblHeading = New System.Windows.Forms.Label()
        Me.lblPurpose = New System.Windows.Forms.Label()
        Me.txtPass = New System.Windows.Forms.TextBox()
        Me.lblError = New System.Windows.Forms.Label()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.chkAll = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'lblHeading
        '
        Me.lblHeading.AutoSize = True
        Me.lblHeading.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Bold)
        Me.lblHeading.ForeColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.lblHeading.Location = New System.Drawing.Point(20, 16)
        Me.lblHeading.Name = "lblHeading"
        Me.lblHeading.Size = New System.Drawing.Size(150, 23)
        Me.lblHeading.TabIndex = 4
        Me.lblHeading.Text = "Password Required"
        '
        'lblPurpose
        '
        Me.lblPurpose.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.lblPurpose.Location = New System.Drawing.Point(22, 48)
        Me.lblPurpose.Name = "lblPurpose"
        Me.lblPurpose.Size = New System.Drawing.Size(376, 36)
        Me.lblPurpose.TabIndex = 5
        Me.lblPurpose.Text = "Enter the password to continue."
        '
        'txtPass
        '
        Me.txtPass.Font = New System.Drawing.Font("Tahoma", 11.0!)
        Me.txtPass.Location = New System.Drawing.Point(24, 90)
        Me.txtPass.Name = "txtPass"
        Me.txtPass.Size = New System.Drawing.Size(372, 25)
        Me.txtPass.TabIndex = 0
        Me.txtPass.UseSystemPasswordChar = True
        '
        'lblError
        '
        Me.lblError.Font = New System.Drawing.Font("Tahoma", 9.0!)
        Me.lblError.ForeColor = System.Drawing.Color.Firebrick
        Me.lblError.Location = New System.Drawing.Point(22, 120)
        Me.lblError.Name = "lblError"
        Me.lblError.Size = New System.Drawing.Size(376, 18)
        Me.lblError.TabIndex = 6
        Me.lblError.Text = "Invalid password. Please try again."
        Me.lblError.Visible = False
        '
        'cmdCancel
        '
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.cmdCancel.Location = New System.Drawing.Point(286, 148)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(110, 34)
        Me.cmdCancel.TabIndex = 2
        Me.cmdCancel.Text = "&Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdOK
        '
        Me.cmdOK.BackColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.cmdOK.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.cmdOK.ForeColor = System.Drawing.Color.White
        Me.cmdOK.Location = New System.Drawing.Point(170, 148)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(110, 34)
        Me.cmdOK.TabIndex = 1
        Me.cmdOK.Text = "&OK"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'chkAll
        '
        Me.chkAll.AutoSize = True
        Me.chkAll.Location = New System.Drawing.Point(24, 156)
        Me.chkAll.Name = "chkAll"
        Me.chkAll.Size = New System.Drawing.Size(136, 17)
        Me.chkAll.TabIndex = 3
        Me.chkAll.Text = "Allow all Price Changes"
        Me.chkAll.UseVisualStyleBackColor = True
        Me.chkAll.Visible = False
        '
        'frmPassword
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(420, 202)
        Me.Controls.Add(Me.lblHeading)
        Me.Controls.Add(Me.lblPurpose)
        Me.Controls.Add(Me.lblError)
        Me.Controls.Add(Me.chkAll)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.txtPass)
        Me.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmPassword"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Input Password"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblHeading As System.Windows.Forms.Label
    Friend WithEvents lblPurpose As System.Windows.Forms.Label
    Friend WithEvents lblError As System.Windows.Forms.Label
    Friend WithEvents txtPass As System.Windows.Forms.TextBox
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents chkAll As System.Windows.Forms.CheckBox
End Class
