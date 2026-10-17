<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmAppPrompt
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                If picIcon IsNot Nothing AndAlso picIcon.Image IsNot Nothing Then picIcon.Image.Dispose()
                If components IsNot Nothing Then components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.picIcon = New System.Windows.Forms.PictureBox()
        Me.lblHeading = New System.Windows.Forms.Label()
        Me.txtMessage = New System.Windows.Forms.TextBox()
        Me.btnFirst = New System.Windows.Forms.Button()
        Me.btnSecond = New System.Windows.Forms.Button()
        Me.btnThird = New System.Windows.Forms.Button()
        CType(Me.picIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picIcon
        '
        Me.picIcon.Location = New System.Drawing.Point(16, 16)
        Me.picIcon.Name = "picIcon"
        Me.picIcon.Size = New System.Drawing.Size(28, 28)
        Me.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.picIcon.TabStop = False
        '
        'lblHeading
        '
        Me.lblHeading.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Bold)
        Me.lblHeading.ForeColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.lblHeading.Location = New System.Drawing.Point(56, 16)
        Me.lblHeading.Name = "lblHeading"
        Me.lblHeading.Size = New System.Drawing.Size(428, 30)
        Me.lblHeading.Text = "Information"
        Me.lblHeading.UseMnemonic = False
        '
        'txtMessage
        '
        Me.txtMessage.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtMessage.BackColor = System.Drawing.Color.White
        Me.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtMessage.Font = New System.Drawing.Font("Tahoma", 11.0!)
        Me.txtMessage.Location = New System.Drawing.Point(16, 64)
        Me.txtMessage.MaxLength = 0
        Me.txtMessage.Multiline = True
        Me.txtMessage.Name = "txtMessage"
        Me.txtMessage.ReadOnly = True
        Me.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtMessage.Size = New System.Drawing.Size(468, 110)
        Me.txtMessage.TabStop = False
        Me.txtMessage.Text = "Message details."
        '
        'btnFirst
        '
        Me.btnFirst.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnFirst.BackColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.btnFirst.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnFirst.ForeColor = System.Drawing.Color.White
        Me.btnFirst.Location = New System.Drawing.Point(374, 190)
        Me.btnFirst.Name = "btnFirst"
        Me.btnFirst.Size = New System.Drawing.Size(110, 34)
        Me.btnFirst.TabIndex = 0
        Me.btnFirst.Text = "&OK"
        Me.btnFirst.UseVisualStyleBackColor = False
        '
        'btnSecond
        '
        Me.btnSecond.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnSecond.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnSecond.Location = New System.Drawing.Point(254, 190)
        Me.btnSecond.Name = "btnSecond"
        Me.btnSecond.Size = New System.Drawing.Size(110, 34)
        Me.btnSecond.TabIndex = 1
        Me.btnSecond.Text = "&Cancel"
        Me.btnSecond.Visible = False
        '
        'btnThird
        '
        Me.btnThird.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnThird.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnThird.Location = New System.Drawing.Point(374, 190)
        Me.btnThird.Name = "btnThird"
        Me.btnThird.Size = New System.Drawing.Size(110, 34)
        Me.btnThird.TabIndex = 2
        Me.btnThird.Text = "&Ignore"
        Me.btnThird.Visible = False
        '
        'frmAppPrompt
        '
        Me.AcceptButton = Me.btnFirst
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(500, 240)
        Me.Controls.Add(Me.picIcon)
        Me.Controls.Add(Me.lblHeading)
        Me.Controls.Add(Me.txtMessage)
        Me.Controls.Add(Me.btnFirst)
        Me.Controls.Add(Me.btnSecond)
        Me.Controls.Add(Me.btnThird)
        Me.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAppPrompt"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Message"
        CType(Me.picIcon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents picIcon As System.Windows.Forms.PictureBox
    Friend WithEvents lblHeading As System.Windows.Forms.Label
    Friend WithEvents txtMessage As System.Windows.Forms.TextBox
    Friend WithEvents btnFirst As System.Windows.Forms.Button
    Friend WithEvents btnSecond As System.Windows.Forms.Button
    Friend WithEvents btnThird As System.Windows.Forms.Button
End Class
