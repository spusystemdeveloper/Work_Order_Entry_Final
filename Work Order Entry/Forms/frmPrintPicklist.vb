Imports Microsoft.Reporting.WinForms

Public Class frmPrintPicklist

    Public Shared wo As Integer

    Dim ReportDataSourcedr As New ReportDataSource

    Public Shared ItemOrderedList As List(Of clsPickList) = New List(Of clsPickList)()

    Private Sub frmPrintPicklist_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PrintSetting()
        printPicklist()
    End Sub


    Private Sub printPicklist()

        Try
            rptviewer.Clear()
            ItemOrderedList.Clear()

            For Each x In clsPickList.getPickList(wo)

                Dim item As clsPickList = New clsPickList
                Dim rType = ""

                If x.Remarks.ToString().Contains("Pick-up") Then
                    rType = "Pick-up"
                ElseIf x.Remarks.ToString().Contains("Delivery") Then
                    rType = "Delivery"
                End If

                item.itemDesc = x.Description
                item.itemCode = x.Itemcode
                item.picker = ""
                item.wokrOrder = x.OrderID
                item.qtyOrder = x.QtyOrder
                item.qtyPrep = clsPickList.getQtyPrep(x.ItemID, x.OrderID)
                item.printDate = DateTime.Now.ToString("dd/MM/yyyy")
                item.printTime = DateTime.Now.ToString("HH:mm:ss tt")
                'item.branch = (From a In db.Configurations Select a.StoreCity).SingleOrDefault
                item.branch = getLoc()
                item.OPIS = clsPickList.getOPIS(wo)
                item.OPIS_Name = clsPickList.getOPISname(clsPickList.getOPIS(wo))
                item.type = "For " & rType
                item.Customer = clsPickList.getCustomerName(x.OrderID)

                ItemOrderedList.Add(item)

            Next

            ReportDataSourcedr.Name = "ItemOrderEntry"
            ReportDataSourcedr.Value = ItemOrderedList

            rptviewer.LocalReport.DataSources.Add(ReportDataSourcedr)
            rptviewer.RefreshReport()
            'rptviewer.LocalReport.Print()


        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try



    End Sub
    Private Function getLoc() As String
        Dim f = (From a In db.Configurations Select a.StoreID).FirstOrDefault

        Select Case f
            Case 4009
                Return "MAA WAREHOUSE"
            Case 4015
                Return "VINZON WAREHOUSE"
            Case 4001
                Return "DAVAO STORE"
            Case Else
                Return ""
        End Select
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

    Private Sub frmPrintPicklist_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed

        rptviewer.Clear()
        Me.Dispose()
    End Sub

End Class