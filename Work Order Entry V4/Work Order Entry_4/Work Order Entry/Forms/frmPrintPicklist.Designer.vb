<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrintPicklist
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
        Dim ReportDataSource2 As Microsoft.Reporting.WinForms.ReportDataSource = New Microsoft.Reporting.WinForms.ReportDataSource()
        Me.rptviewer = New Microsoft.Reporting.WinForms.ReportViewer()
        Me.SuspendLayout()
        '
        'rptviewer
        '
        Me.rptviewer.Dock = System.Windows.Forms.DockStyle.Fill
        ReportDataSource2.Name = "ItemOrderEntry"
        Me.rptviewer.LocalReport.DataSources.Add(ReportDataSource2)
        Me.rptviewer.LocalReport.ReportEmbeddedResource = "WorkOrderEntry.rptPickList.rdlc"
        Me.rptviewer.Location = New System.Drawing.Point(0, 0)
        Me.rptviewer.Name = "rptviewer"
        Me.rptviewer.Size = New System.Drawing.Size(886, 404)
        Me.rptviewer.TabIndex = 1
        '
        'frmPrintPicklist
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(886, 404)
        Me.Controls.Add(Me.rptviewer)
        Me.Name = "frmPrintPicklist"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "printPicklist"
        Me.TopMost = True
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents rptviewer As Microsoft.Reporting.WinForms.ReportViewer
End Class
