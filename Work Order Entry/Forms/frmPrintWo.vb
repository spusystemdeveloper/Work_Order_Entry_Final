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

            If Convert.ToString(clsItemLookUp.Comment).Contains("SOD WEBSITE") Then

                Dim paramComp As New ReportParameter("paramComp", "Starbright Office Depot, Inc." & " - WEBSITE")
                rptviewer.LocalReport.SetParameters(paramComp)

            Else

                Dim paramComp As New ReportParameter("paramComp", "Starbright Office Depot, Inc.")
                rptviewer.LocalReport.SetParameters(paramComp)
            End If

            Dim paramAddress As New ReportParameter("paramAddress", Convert.ToString(clsItemLookUp.Address))
            rptviewer.LocalReport.SetParameters(paramAddress)
            Dim paramBranch As New ReportParameter("paramBranch", Convert.ToString(clsItemLookUp.Branch))
            rptviewer.LocalReport.SetParameters(paramBranch)
            Dim paramWOno As New ReportParameter("paramWOno", Convert.ToString(clsItemLookUp.Orderid))
            rptviewer.LocalReport.SetParameters(paramWOno)
            Dim paramAccountNo As New ReportParameter("paramAccountNo", Convert.ToString(clsItemLookUp.AccountNo))
            rptviewer.LocalReport.SetParameters(paramAccountNo)
            Dim paramCompany As New ReportParameter("paramCompany", Convert.ToString(clsItemLookUp.Company))
            rptviewer.LocalReport.SetParameters(paramCompany)
            Dim paramRegister As New ReportParameter("paramRegister", Convert.ToString(clsItemLookUp.Register))
            rptviewer.LocalReport.SetParameters(paramRegister)
            Dim paramCashier As New ReportParameter("paramCashier", Convert.ToString(clsItemLookUp.Cashier))
            rptviewer.LocalReport.SetParameters(paramCashier)
            Dim paramDate As New ReportParameter("paramDate", Convert.ToString(clsItemLookUp.OrderDate))
            rptviewer.LocalReport.SetParameters(paramDate)
            Dim paramTime As New ReportParameter("paramTime", Convert.ToString(clsItemLookUp.Ordertime))
            rptviewer.LocalReport.SetParameters(paramTime)
            Dim paramReference As New ReportParameter("paramReference", Convert.ToString(clsItemLookUp.Reference))
            rptviewer.LocalReport.SetParameters(paramReference)
            Dim paramComment As New ReportParameter("paramComment", Convert.ToString(clsItemLookUp.Comment))
            rptviewer.LocalReport.SetParameters(paramComment)



            Dim paramSubtotal As New ReportParameter("paramSubtotal", clsItemLookUp.Subtotal.ToString("N2"))
            rptviewer.LocalReport.SetParameters(paramSubtotal)
            Dim paramSalesTax As New ReportParameter("paramSalesTax", clsItemLookUp.SalesTax.ToString("N2"))
            rptviewer.LocalReport.SetParameters(paramSalesTax)

            Dim paramTotal As New ReportParameter("paramTotal", clsItemLookUp.Total.ToString("N2"))
            rptviewer.LocalReport.SetParameters(paramTotal)

            'Dim type As String = ""
            'If frmItemLookUp.getEntryType() = 3 Then

            '    Type = "Sales Quotation"
            'Else
            '    Type = "Work Order"

            'End If


            Dim paramType As New ReportParameter("paramType", type)
            rptviewer.LocalReport.SetParameters(paramType)

            Me.rptviewer.LocalReport.DataSources.Add(ReportDataSourcedr)
            rptviewer.RefreshReport()
            'rptviewer.LocalReport.Print()


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub


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
        Me.Dispose()
    End Sub

    Private Sub rptviewer_RenderingComplete(ByVal sender As System.Object, ByVal e As Microsoft.Reporting.WinForms.RenderingCompleteEventArgs) Handles rptviewer.RenderingComplete
        Print_DialogTimer.Start()
    End Sub

End Class