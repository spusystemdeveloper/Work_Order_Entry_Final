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
        Me.SuspendLayout()
        '
        'rptviewer
        '
        Me.rptviewer.Dock = System.Windows.Forms.DockStyle.Fill
        ReportDataSource1.Name = "ItemWoEntry"
        Me.rptviewer.LocalReport.DataSources.Add(ReportDataSource1)
        Me.rptviewer.LocalReport.ReportEmbeddedResource = "WorkOrderEntry.rprtOrder.rdlc"
        Me.rptviewer.Location = New System.Drawing.Point(0, 0)
        Me.rptviewer.Name = "rptviewer"
        Me.rptviewer.Size = New System.Drawing.Size(959, 474)
        Me.rptviewer.TabIndex = 0
        '
        'Print_DialogTimer
        '
        Me.Print_DialogTimer.Interval = 3500
        '
        'frmPrintWo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(959, 474)
        Me.Controls.Add(Me.rptviewer)
        Me.Name = "frmPrintWo"
        Me.Text = "Print Form"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents rptviewer As Microsoft.Reporting.WinForms.ReportViewer
    Friend WithEvents Print_DialogTimer As System.Windows.Forms.Timer
End Class
