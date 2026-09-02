<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrint
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
        Dim ReportDataSource1 As Microsoft.Reporting.WinForms.ReportDataSource = New Microsoft.Reporting.WinForms.ReportDataSource()
        Me.rpt_viewer = New Microsoft.Reporting.WinForms.ReportViewer()
        Me.SuspendLayout()
        '
        'rpt_viewer
        '
        Me.rpt_viewer.Dock = System.Windows.Forms.DockStyle.Fill
        ReportDataSource1.Name = "ItemOrderedEntry"
        ReportDataSource1.Value = Nothing
        Me.rpt_viewer.LocalReport.DataSources.Add(ReportDataSource1)
        Me.rpt_viewer.LocalReport.ReportEmbeddedResource = "OrderProcessingQueueing.rpt_ItemOrdered.rdlc"
        Me.rpt_viewer.Location = New System.Drawing.Point(0, 0)
        Me.rpt_viewer.Name = "rpt_viewer"
        Me.rpt_viewer.Size = New System.Drawing.Size(1003, 525)
        Me.rpt_viewer.TabIndex = 0
        '
        'frmPrint
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1003, 525)
        Me.Controls.Add(Me.rpt_viewer)
        Me.Name = "frmPrint"
        Me.Text = "frmPrint"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents rpt_viewer As Microsoft.Reporting.WinForms.ReportViewer
End Class
