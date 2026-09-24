<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmQtyPrep
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
        Me.txtQty = New System.Windows.Forms.TextBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnShortage = New System.Windows.Forms.Button()
        Me.lblGroupID = New System.Windows.Forms.Label()
        Me.lblItemId = New System.Windows.Forms.Label()
        Me.lblQid = New System.Windows.Forms.Label()
        Me.lblDesc = New System.Windows.Forms.Label()
        Me.lblItemCode = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtQty
        '
        Me.txtQty.Font = New System.Drawing.Font("Microsoft Sans Serif", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtQty.Location = New System.Drawing.Point(12, 74)
        Me.txtQty.Multiline = True
        Me.txtQty.Name = "txtQty"
        Me.txtQty.Size = New System.Drawing.Size(274, 83)
        Me.txtQty.TabIndex = 0
        Me.txtQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnOK
        '
        Me.btnOK.Location = New System.Drawing.Point(18, 166)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(120, 38)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnShortage
        '
        Me.btnShortage.BackColor = System.Drawing.Color.FromArgb(254, 243, 199)
        Me.btnShortage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnShortage.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnShortage.ForeColor = System.Drawing.Color.FromArgb(180, 83, 9)
        Me.btnShortage.Location = New System.Drawing.Point(150, 166)
        Me.btnShortage.Name = "btnShortage"
        Me.btnShortage.Size = New System.Drawing.Size(130, 38)
        Me.btnShortage.TabIndex = 2
        Me.btnShortage.Text = "⚠️ Shortage"
        Me.btnShortage.UseVisualStyleBackColor = False
        '
        'lblGroupID
        '
        Me.lblGroupID.AutoSize = True
        Me.lblGroupID.Location = New System.Drawing.Point(75, 273)
        Me.lblGroupID.Name = "lblGroupID"
        Me.lblGroupID.Size = New System.Drawing.Size(57, 13)
        Me.lblGroupID.TabIndex = 2
        Me.lblGroupID.Text = "lblGroupID"
        '
        'lblItemId
        '
        Me.lblItemId.AutoSize = True
        Me.lblItemId.Location = New System.Drawing.Point(23, 248)
        Me.lblItemId.Name = "lblItemId"
        Me.lblItemId.Size = New System.Drawing.Size(46, 13)
        Me.lblItemId.TabIndex = 3
        Me.lblItemId.Text = "lblItemId"
        '
        'lblQid
        '
        Me.lblQid.AutoSize = True
        Me.lblQid.Location = New System.Drawing.Point(23, 273)
        Me.lblQid.Name = "lblQid"
        Me.lblQid.Size = New System.Drawing.Size(33, 13)
        Me.lblQid.TabIndex = 4
        Me.lblQid.Text = "lblQid"
        '
        'lblDesc
        '
        Me.lblDesc.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDesc.Location = New System.Drawing.Point(57, 24)
        Me.lblDesc.Name = "lblDesc"
        Me.lblDesc.Size = New System.Drawing.Size(229, 41)
        Me.lblDesc.TabIndex = 12
        Me.lblDesc.Text = "----"
        '
        'lblItemCode
        '
        Me.lblItemCode.AutoSize = True
        Me.lblItemCode.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblItemCode.Location = New System.Drawing.Point(57, 9)
        Me.lblItemCode.Name = "lblItemCode"
        Me.lblItemCode.Size = New System.Drawing.Size(23, 15)
        Me.lblItemCode.TabIndex = 11
        Me.lblItemCode.Text = "----"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(9, 27)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 15)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Desc :"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(9, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(42, 15)
        Me.Label1.TabIndex = 9
        Me.Label1.Text = "Code :"
        '
        'frmQtyPrep
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ClientSize = New System.Drawing.Size(297, 217)
        Me.Controls.Add(Me.lblDesc)
        Me.Controls.Add(Me.lblItemCode)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblQid)
        Me.Controls.Add(Me.lblItemId)
        Me.Controls.Add(Me.lblGroupID)
        Me.Controls.Add(Me.btnShortage)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.txtQty)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "frmQtyPrep"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Set Quantity"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtQty As System.Windows.Forms.TextBox
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnShortage As System.Windows.Forms.Button
    Friend WithEvents lblGroupID As System.Windows.Forms.Label
    Friend WithEvents lblItemId As System.Windows.Forms.Label
    Friend WithEvents lblQid As System.Windows.Forms.Label
    Friend WithEvents lblDesc As System.Windows.Forms.Label
    Friend WithEvents lblItemCode As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
