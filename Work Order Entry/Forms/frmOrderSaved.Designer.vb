<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmOrderSaved
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.txtOrderNumbers = New System.Windows.Forms.TextBox()
        Me.lblDetails = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblStatus
        '
        Me.lblStatus.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblStatus.Location = New System.Drawing.Point(16, 12)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(408, 36)
        Me.lblStatus.Text = "Work Order saved successfully"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtOrderNumbers
        '
        Me.txtOrderNumbers.BackColor = System.Drawing.Color.White
        Me.txtOrderNumbers.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtOrderNumbers.Font = New System.Drawing.Font("Tahoma", 42.0!, System.Drawing.FontStyle.Bold)
        Me.txtOrderNumbers.Location = New System.Drawing.Point(16, 58)
        Me.txtOrderNumbers.Multiline = True
        Me.txtOrderNumbers.Name = "txtOrderNumbers"
        Me.txtOrderNumbers.ReadOnly = True
        Me.txtOrderNumbers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtOrderNumbers.Size = New System.Drawing.Size(408, 96)
        Me.txtOrderNumbers.TabStop = False
        Me.txtOrderNumbers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblDetails
        '
        Me.lblDetails.Location = New System.Drawing.Point(16, 162)
        Me.lblDetails.Name = "lblDetails"
        Me.lblDetails.Size = New System.Drawing.Size(408, 22)
        Me.lblDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(314, 188)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(110, 34)
        Me.btnClose.TabIndex = 0
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'frmOrderSaved
        '
        Me.AcceptButton = Me.btnClose
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.CancelButton = Me.btnClose
        Me.ClientSize = New System.Drawing.Size(440, 238)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.txtOrderNumbers)
        Me.Controls.Add(Me.lblDetails)
        Me.Controls.Add(Me.btnClose)
        Me.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmOrderSaved"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Work Order Saved"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents txtOrderNumbers As System.Windows.Forms.TextBox
    Friend WithEvents lblDetails As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
End Class
