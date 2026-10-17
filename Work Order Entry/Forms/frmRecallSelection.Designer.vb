<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmRecallSelection
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
        Me.listBoxSelection = New System.Windows.Forms.ListBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.lblChoose = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'listBoxSelection
        '
        Me.listBoxSelection.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.listBoxSelection.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Bold)
        Me.listBoxSelection.FormattingEnabled = True
        Me.listBoxSelection.IntegralHeight = False
        Me.listBoxSelection.ItemHeight = 24
        Me.listBoxSelection.Location = New System.Drawing.Point(16, 50)
        Me.listBoxSelection.Name = "listBoxSelection"
        Me.listBoxSelection.Size = New System.Drawing.Size(428, 100)
        Me.listBoxSelection.TabIndex = 0
        '
        'btnOK
        '
        Me.btnOK.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnOK.BackColor = System.Drawing.Color.FromArgb(38, 99, 158)
        Me.btnOK.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnOK.ForeColor = System.Drawing.Color.White
        Me.btnOK.Location = New System.Drawing.Point(214, 170)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(110, 34)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "&Ok"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'Button1
        '
        Me.Button1.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.Button1.Font = New System.Drawing.Font("Tahoma", 10.0!, System.Drawing.FontStyle.Bold)
        Me.Button1.Location = New System.Drawing.Point(334, 170)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(110, 34)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "&Cancel"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'lblChoose
        '
        Me.lblChoose.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblChoose.Font = New System.Drawing.Font("Tahoma", 11.0!, System.Drawing.FontStyle.Bold)
        Me.lblChoose.Location = New System.Drawing.Point(16, 16)
        Me.lblChoose.Name = "lblChoose"
        Me.lblChoose.Size = New System.Drawing.Size(428, 24)
        Me.lblChoose.Text = "Choose an option"
        '
        'frmRecallSelection
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(460, 220)
        Me.Controls.Add(Me.lblChoose)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.listBoxSelection)
        Me.Font = New System.Drawing.Font("Tahoma", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.MinimumSize = New System.Drawing.Size(400, 240)
        Me.Name = "frmRecallSelection"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Recall Selection"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents listBoxSelection As ListBox
    Friend WithEvents btnOK As Button
    Friend WithEvents Button1 As Button
    Friend WithEvents lblChoose As Label
End Class
