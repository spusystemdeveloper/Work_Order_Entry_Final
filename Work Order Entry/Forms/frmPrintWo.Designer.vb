<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrintWo
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
        Me.components = New System.ComponentModel.Container()
        Dim ReportDataSource1 As Microsoft.Reporting.WinForms.ReportDataSource = New Microsoft.Reporting.WinForms.ReportDataSource()
        Me.rptviewer = New Microsoft.Reporting.WinForms.ReportViewer()
        Me.Print_DialogTimer = New System.Windows.Forms.Timer(Me.components)
        Me.previewHeader = New System.Windows.Forms.Panel()
        Me.lblPreviewTitle = New System.Windows.Forms.Label()
        Me.lblPreviewHelp = New System.Windows.Forms.Label()
        Me.previewHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'rptviewer
        '
        Me.rptviewer.Dock = System.Windows.Forms.DockStyle.Fill
        ReportDataSource1.Name = "ItemWoEntry"
        Me.rptviewer.LocalReport.DataSources.Add(ReportDataSource1)
        Me.rptviewer.LocalReport.ReportEmbeddedResource = "WorkOrderEntry.rprtOrder.rdlc"
        Me.rptviewer.Location = New System.Drawing.Point(12, 76)
        Me.rptviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.rptviewer.Name = "rptviewer"
        Me.rptviewer.Size = New System.Drawing.Size(960, 512)
        Me.rptviewer.TabIndex = 0
        '
        'Print_DialogTimer
        '
        Me.Print_DialogTimer.Interval = 3500
        '
        'frmPrintWo
        '
        Me.previewHeader.Controls.Add(Me.lblPreviewTitle)
        Me.previewHeader.Controls.Add(Me.lblPreviewHelp)
        Me.previewHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.previewHeader.Location = New System.Drawing.Point(12, 12)
        Me.previewHeader.Name = "previewHeader"
        Me.previewHeader.Size = New System.Drawing.Size(960, 64)
        Me.previewHeader.TabIndex = 10
        Me.previewHeader.TabStop = False
        Me.lblPreviewTitle.AutoSize = True
        Me.lblPreviewTitle.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblPreviewTitle.Location = New System.Drawing.Point(0, 4)
        Me.lblPreviewTitle.Name = "lblPreviewTitle"
        Me.lblPreviewTitle.Size = New System.Drawing.Size(400, 24)
        Me.lblPreviewTitle.TabIndex = 0
        Me.lblPreviewTitle.Text = "Work Order / Sales Quotation preview"
        Me.lblPreviewHelp.AutoSize = True
        Me.lblPreviewHelp.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Regular)
        Me.lblPreviewHelp.Location = New System.Drawing.Point(0, 34)
        Me.lblPreviewHelp.Name = "lblPreviewHelp"
        Me.lblPreviewHelp.Size = New System.Drawing.Size(600, 18)
        Me.lblPreviewHelp.TabIndex = 1
        Me.lblPreviewHelp.Text = "Use the toolbar below for printing, page setup and export."
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(984, 600)
        Me.Controls.Add(Me.rptviewer)
        Me.Controls.Add(Me.previewHeader)
        Me.MinimumSize = New System.Drawing.Size(800, 450)
        Me.Padding = New System.Windows.Forms.Padding(12)
        Me.Name = "frmPrintWo"
        Me.Text = "Work Order / Sales Quotation - Print Preview"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.previewHeader.ResumeLayout(False)
        Me.previewHeader.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents previewHeader As System.Windows.Forms.Panel
    Friend WithEvents lblPreviewTitle As System.Windows.Forms.Label
    Friend WithEvents lblPreviewHelp As System.Windows.Forms.Label
    Public WithEvents rptviewer As Microsoft.Reporting.WinForms.ReportViewer
    Friend WithEvents Print_DialogTimer As System.Windows.Forms.Timer
End Class
