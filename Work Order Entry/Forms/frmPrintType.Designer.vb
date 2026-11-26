<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrintType
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
        Me.txtWO = New System.Windows.Forms.TextBox()
        Me.rbtnOrder = New System.Windows.Forms.RadioButton()
        Me.rbtnPickList = New System.Windows.Forms.RadioButton()
        Me.btnPrint = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblOrderNumber = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtWO
        '
        Me.txtWO.Font = New System.Drawing.Font("Tahoma", 24.0!, System.Drawing.FontStyle.Bold)
        Me.txtWO.Location = New System.Drawing.Point(24, 216)
        Me.txtWO.BackColor = System.Drawing.Color.FromArgb(245, 249, 255)
        Me.txtWO.Name = "txtWO"
        Me.txtWO.Size = New System.Drawing.Size(414, 50)
        Me.txtWO.TabIndex = 0
        Me.txtWO.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'rbtnOrder
        '
        Me.rbtnOrder.AutoSize = False
        Me.rbtnOrder.Checked = True
        Me.rbtnOrder.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Bold)
        Me.rbtnOrder.Location = New System.Drawing.Point(24, 64)
        Me.rbtnOrder.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.rbtnOrder.Padding = New System.Windows.Forms.Padding(12, 0, 12, 0)
        Me.rbtnOrder.Name = "rbtnOrder"
        Me.rbtnOrder.Size = New System.Drawing.Size(592, 44)
        Me.rbtnOrder.TabIndex = 1
        Me.rbtnOrder.TabStop = True
        Me.rbtnOrder.Text = "Work Order / Quotation"
        Me.rbtnOrder.UseVisualStyleBackColor = False
        '
        'rbtnPickList
        '
        Me.rbtnPickList.AutoSize = False
        Me.rbtnPickList.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Bold)
        Me.rbtnPickList.Location = New System.Drawing.Point(24, 116)
        Me.rbtnPickList.BackColor = System.Drawing.Color.FromArgb(245, 247, 250)
        Me.rbtnPickList.Padding = New System.Windows.Forms.Padding(12, 0, 12, 0)
        Me.rbtnPickList.Name = "rbtnPickList"
        Me.rbtnPickList.Size = New System.Drawing.Size(592, 44)
        Me.rbtnPickList.TabIndex = 2
        Me.rbtnPickList.Text = "Pick List"
        Me.rbtnPickList.UseVisualStyleBackColor = False
        '
        'btnPrint
        '
        Me.btnPrint.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Bold)
        Me.btnPrint.Location = New System.Drawing.Point(454, 212)
        Me.btnPrint.BackColor = System.Drawing.Color.FromArgb(37, 99, 160)
        Me.btnPrint.ForeColor = System.Drawing.Color.White
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.Size = New System.Drawing.Size(162, 56)
        Me.btnPrint.TabIndex = 3
        Me.btnPrint.Text = "Print"
        Me.btnPrint.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Tahoma", 16.0!, System.Drawing.FontStyle.Bold)
        Me.Label1.Location = New System.Drawing.Point(24, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(580, 30)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Choose what to print"
        '
        'frmPrintType
        '
        Me.lblOrderNumber.AutoSize = True
        Me.lblOrderNumber.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblOrderNumber.Location = New System.Drawing.Point(24, 180)
        Me.lblOrderNumber.Name = "lblOrderNumber"
        Me.lblOrderNumber.Size = New System.Drawing.Size(420, 26)
        Me.lblOrderNumber.TabIndex = 5
        Me.lblOrderNumber.Text = "Enter order number:"
        Me.AcceptButton = Me.btnPrint
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(640, 292)
        Me.Controls.Add(Me.lblOrderNumber)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnPrint)
        Me.Controls.Add(Me.rbtnPickList)
        Me.Controls.Add(Me.rbtnOrder)
        Me.Controls.Add(Me.txtWO)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.KeyPreview = True
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.MinimizeBox = False
        Me.BackColor = System.Drawing.Color.White
        Me.Name = "frmPrintType"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Print - Input Work Order Number"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtWO As System.Windows.Forms.TextBox
    Friend WithEvents rbtnOrder As System.Windows.Forms.RadioButton
    Friend WithEvents rbtnPickList As System.Windows.Forms.RadioButton
    Friend WithEvents btnPrint As System.Windows.Forms.Button
    Friend WithEvents lblOrderNumber As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
