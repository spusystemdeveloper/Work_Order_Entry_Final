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
        Me.SuspendLayout()
        '
        'txtWO
        '
        Me.txtWO.Font = New System.Drawing.Font("Century Gothic", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtWO.Location = New System.Drawing.Point(12, 62)
        Me.txtWO.Name = "txtWO"
        Me.txtWO.Size = New System.Drawing.Size(275, 33)
        Me.txtWO.TabIndex = 0
        Me.txtWO.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'rbtnOrder
        '
        Me.rbtnOrder.AutoSize = True
        Me.rbtnOrder.Checked = True
        Me.rbtnOrder.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnOrder.Location = New System.Drawing.Point(12, 35)
        Me.rbtnOrder.Name = "rbtnOrder"
        Me.rbtnOrder.Size = New System.Drawing.Size(116, 20)
        Me.rbtnOrder.TabIndex = 1
        Me.rbtnOrder.TabStop = True
        Me.rbtnOrder.Text = "Order Number"
        Me.rbtnOrder.UseVisualStyleBackColor = True
        '
        'rbtnPickList
        '
        Me.rbtnPickList.AutoSize = True
        Me.rbtnPickList.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbtnPickList.Location = New System.Drawing.Point(150, 35)
        Me.rbtnPickList.Name = "rbtnPickList"
        Me.rbtnPickList.Size = New System.Drawing.Size(76, 20)
        Me.rbtnPickList.TabIndex = 2
        Me.rbtnPickList.Text = "Pick List"
        Me.rbtnPickList.UseVisualStyleBackColor = True
        '
        'btnPrint
        '
        Me.btnPrint.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPrint.Location = New System.Drawing.Point(293, 5)
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.Size = New System.Drawing.Size(130, 90)
        Me.btnPrint.TabIndex = 3
        Me.btnPrint.Text = "Print"
        Me.btnPrint.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(110, 16)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Select Template"
        '
        'frmPrintType
        '
        Me.AcceptButton = Me.btnPrint
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(427, 102)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnPrint)
        Me.Controls.Add(Me.rbtnPickList)
        Me.Controls.Add(Me.rbtnOrder)
        Me.Controls.Add(Me.txtWO)
        Me.Font = New System.Drawing.Font("Century Gothic", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.KeyPreview = True
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.MinimizeBox = False
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
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
