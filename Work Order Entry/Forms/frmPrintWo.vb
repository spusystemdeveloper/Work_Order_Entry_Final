Imports System
Imports System.IO
Imports System.Data
Imports System.Text
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports Microsoft.Reporting.WinForms

Public Class frmPrintWo

    Public Shared wo As Integer
    Public Shared Type As String = ""

    Dim ReportDataSourcedr As New ReportDataSource

    Public Shared list As New List(Of String)

    Private Sub frmReport_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        PrintSetting()
        printWo()

    End Sub

    Private Sub printWo()

        Try
            Dim ReportDataSourcedr As New ReportDataSource
            rptviewer.Clear()

            ReportDataSourcedr.Name = "ItemWoEntry"
            ReportDataSourcedr.Value = clsItemLookUp.viewWoEntry(wo)
            Me.rptviewer.LocalReport.DataSources.Add(ReportDataSourcedr)

            clsItemLookUp.viewWoDetials(wo)

            Type = OrderPrintRules.ResolveTemplateName(Type)
            Dim withholdingReference As String
            Using dbx = GetDB()
                withholdingReference = WorkOrderWithholdingReference.Load(
                    dbx, wo, clsItemLookUp.Subtotal.ToString("N2"), Type)
            End Using
            rptviewer.LocalReport.SetParameters(CreateReportParameters(withholdingReference))

            rptviewer.RefreshReport()


        Catch ex As Exception
            ErrorLogger.LogError(ex, "Work Order / Quotation preview; OrderID=" & wo.ToString())
            Dim message As String = ex.Message
            If ex.InnerException IsNot Nothing Then
                message &= vbCrLf & vbCrLf & "Details: " & ex.GetBaseException().Message
            End If
            AppPrompt.Show(message)
        End Try

    End Sub


    Private Function CreateReportParameters(ByVal withholdingReference As String) As ReportParameter()
        Dim companyName As String = "Starbright Office Depot, Inc."
        If Convert.ToString(clsItemLookUp.Comment).Contains("SOD WEBSITE") Then
            companyName &= " - WEBSITE"
        End If

        Return {
            New ReportParameter("paramComp", companyName),
            New ReportParameter("paramAddress", Convert.ToString(clsItemLookUp.Address)),
            New ReportParameter("paramBranch", Convert.ToString(clsItemLookUp.Branch)),
            New ReportParameter("paramWOno", Convert.ToString(clsItemLookUp.Orderid)),
            New ReportParameter("paramAccountNo", Convert.ToString(clsItemLookUp.AccountNo)),
            New ReportParameter("paramCompany", Convert.ToString(clsItemLookUp.Company)),
            New ReportParameter("paramRegister", Convert.ToString(clsItemLookUp.Register)),
            New ReportParameter("paramCashier", Convert.ToString(clsItemLookUp.Cashier)),
            New ReportParameter("paramDate", Convert.ToString(clsItemLookUp.OrderDate)),
            New ReportParameter("paramTime", Convert.ToString(clsItemLookUp.Ordertime)),
            New ReportParameter("paramReference", Convert.ToString(clsItemLookUp.Reference)),
            New ReportParameter("paramComment", Convert.ToString(clsItemLookUp.Comment)),
            New ReportParameter("paramSubtotal", clsItemLookUp.Subtotal.ToString("N2")),
            New ReportParameter("paramSalesTax", clsItemLookUp.SalesTax.ToString("N2")),
            New ReportParameter("paramTotal", clsItemLookUp.Total.ToString("N2")),
            New ReportParameter("paramType", Type),
            New ReportParameter("paramWithholdingReference", withholdingReference)
        }
    End Function

    Private Sub PrintSetting()

        Dim Size As New System.Drawing.Printing.PaperSize
        Dim pg As New System.Drawing.Printing.PageSettings()

        With Size

            Size = New Printing.PaperSize()
            Size.Height = 650
            Size.Width = 850

        End With

        With pg
            .Margins.Top = 0.25
            .Margins.Bottom = 0.25
            .Margins.Left = 0.25
            .Margins.Right = 0.25
            .PaperSize = Size
            .PrinterSettings.FromPage = 0
            .PrinterSettings.ToPage = 1
            .PrinterSettings.PrintRange = Printing.PrintRange.Selection
        End With

        rptviewer.SetPageSettings(pg)

        Me.rptviewer.SetDisplayMode(DisplayMode.PrintLayout)

    End Sub

    Private Sub Print_DialogTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Print_DialogTimer.Tick

        Print_DialogTimer.Stop()
        Print_DialogTimer.Enabled = False
        '   rptviewer.PrintDialog()

    End Sub

    Private Sub frmPrintWo_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed

        rptviewer.Clear()
        Type = ""
        wo = 0
        Me.Dispose()
    End Sub

    Private Sub rptviewer_RenderingComplete(ByVal sender As System.Object, ByVal e As Microsoft.Reporting.WinForms.RenderingCompleteEventArgs) Handles rptviewer.RenderingComplete
        Print_DialogTimer.Start()
    End Sub

    Private Sub rptviewer_ReportError(sender As Object, e As ReportErrorEventArgs) Handles rptviewer.ReportError
        ' Capture asynchronous processing errors without changing ReportViewer handling.
        ErrorLogger.LogError(e.Exception, "Work Order / Quotation rendering; OrderID=" & wo.ToString())
    End Sub

End Class
