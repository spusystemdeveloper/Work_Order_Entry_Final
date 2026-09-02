<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPicker
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
        Me.btnProcess = New System.Windows.Forms.Button()
        Me.btnUnProcess = New System.Windows.Forms.Button()
        Me.gridPickers = New System.Windows.Forms.DataGridView()
        Me.lblGroupID = New System.Windows.Forms.Label()
        Me.lblDesc = New System.Windows.Forms.Label()
        Me.lblItemCode = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblQueueID = New System.Windows.Forms.Label()
        CType(Me.gridPickers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnProcess
        '
        Me.btnProcess.Location = New System.Drawing.Point(8, 71)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Size = New System.Drawing.Size(75, 30)
        Me.btnProcess.TabIndex = 1
        Me.btnProcess.Text = "Process"
        Me.btnProcess.UseVisualStyleBackColor = True
        '
        'btnUnProcess
        '
        Me.btnUnProcess.Location = New System.Drawing.Point(89, 71)
        Me.btnUnProcess.Name = "btnUnProcess"
        Me.btnUnProcess.Size = New System.Drawing.Size(75, 30)
        Me.btnUnProcess.TabIndex = 2
        Me.btnUnProcess.Text = "Un-Process"
        Me.btnUnProcess.UseVisualStyleBackColor = True
        '
        'gridPickers
        '
        Me.gridPickers.AllowUserToAddRows = False
        Me.gridPickers.AllowUserToDeleteRows = False
        Me.gridPickers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridPickers.BackgroundColor = System.Drawing.SystemColors.Control
        Me.gridPickers.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridPickers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridPickers.Location = New System.Drawing.Point(10, 107)
        Me.gridPickers.Name = "gridPickers"
        Me.gridPickers.ReadOnly = True
        Me.gridPickers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gridPickers.Size = New System.Drawing.Size(276, 171)
        Me.gridPickers.TabIndex = 3
        '
        'lblGroupID
        '
        Me.lblGroupID.AutoSize = True
        Me.lblGroupID.Location = New System.Drawing.Point(57, 173)
        Me.lblGroupID.Name = "lblGroupID"
        Me.lblGroupID.Size = New System.Drawing.Size(57, 13)
        Me.lblGroupID.TabIndex = 4
        Me.lblGroupID.Text = "lblGroupID"
        '
        'lblDesc
        '
        Me.lblDesc.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDesc.Location = New System.Drawing.Point(57, 21)
        Me.lblDesc.Name = "lblDesc"
        Me.lblDesc.Size = New System.Drawing.Size(229, 41)
        Me.lblDesc.TabIndex = 16
        Me.lblDesc.Text = "----"
        '
        'lblItemCode
        '
        Me.lblItemCode.AutoSize = True
        Me.lblItemCode.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblItemCode.Location = New System.Drawing.Point(57, 6)
        Me.lblItemCode.Name = "lblItemCode"
        Me.lblItemCode.Size = New System.Drawing.Size(23, 15)
        Me.lblItemCode.TabIndex = 15
        Me.lblItemCode.Text = "----"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(9, 24)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 15)
        Me.Label2.TabIndex = 14
        Me.Label2.Text = "Desc :"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(9, 6)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(42, 15)
        Me.Label1.TabIndex = 13
        Me.Label1.Text = "Code :"
        '
        'lblQueueID
        '
        Me.lblQueueID.AutoSize = True
        Me.lblQueueID.Location = New System.Drawing.Point(12, 334)
        Me.lblQueueID.Name = "lblQueueID"
        Me.lblQueueID.Size = New System.Drawing.Size(60, 13)
        Me.lblQueueID.TabIndex = 17
        Me.lblQueueID.Text = "lblQueueID"
        '
        'frmPicker
        '
        Me.AcceptButton = Me.btnProcess
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ClientSize = New System.Drawing.Size(295, 285)
        Me.Controls.Add(Me.gridPickers)
        Me.Controls.Add(Me.lblQueueID)
        Me.Controls.Add(Me.lblDesc)
        Me.Controls.Add(Me.lblItemCode)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblGroupID)
        Me.Controls.Add(Me.btnUnProcess)
        Me.Controls.Add(Me.btnProcess)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "frmPicker"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Choose Picker"
        CType(Me.gridPickers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnProcess As System.Windows.Forms.Button
    Friend WithEvents btnUnProcess As System.Windows.Forms.Button
    Friend WithEvents gridPickers As System.Windows.Forms.DataGridView
    Friend WithEvents lblGroupID As System.Windows.Forms.Label
    Friend WithEvents lblDesc As System.Windows.Forms.Label
    Friend WithEvents lblItemCode As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblQueueID As System.Windows.Forms.Label
End Class
