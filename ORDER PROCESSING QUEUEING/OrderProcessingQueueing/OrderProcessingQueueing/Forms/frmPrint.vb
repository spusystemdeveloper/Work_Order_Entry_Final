Imports Microsoft.Reporting.WinForms

Public Class frmPrint

    Dim ReportDataSourcedr As New ReportDataSource

    Private Sub frmPrint_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        PrintSetting()

        ReportDataSourcedr.Name = "ItemOrderEntry"
        ReportDataSourcedr.Value = frmMain.ItemOrderedList
        Me.rpt_viewer.LocalReport.DataSources.Add(ReportDataSourcedr)

        Me.rpt_viewer.RefreshReport()

    End Sub

    Private Sub PrintSetting()

        Dim Size As New System.Drawing.Printing.PaperSize
        Dim pg As New System.Drawing.Printing.PageSettings()

        With Size

            Size = New Printing.PaperSize()
            Size.Height = 550
            Size.Width = 850

        End With

        With pg
            .Margins.Top = 0.25
            .Margins.Bottom = 0.25
            .Margins.Left = 0.25
            .Margins.Right = 0.25
            .PaperSize = Size
            .Landscape = True
            .PrinterSettings.PrintRange = Printing.PrintRange.Selection
        End With

        rpt_viewer.SetPageSettings(pg)

        Me.rpt_viewer.SetDisplayMode(DisplayMode.PrintLayout)

    End Sub

    Private Sub frmPrint_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        rpt_viewer.LocalReport.ReleaseSandboxAppDomain()
    End Sub
End Class