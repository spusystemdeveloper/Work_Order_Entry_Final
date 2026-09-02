Imports ExcelDataReader
Imports System.IO
Imports System.Text

Public Class clsImport

    Public Shared ImportData As DataTableCollection
    Public Shared ImportDataCSV As DataTable

    Public Shared iAccountNumber_ As String
    Public Shared iPriceLevel_ As Integer
    Public Shared iEmployee_ As Integer
    Public Shared iTitle_ As String
    Public Shared iTaxExempt_ As Integer
    Public Shared iCustID_ As Integer
    Public Shared iCompany_ As String
    Public Shared iCustomText5_ As String
    Public Shared iSalesRepID_ As Integer
    Public Shared iSaleRepName_ As String
    Public Shared iComment_ As String
    Public Shared iType_ As Integer

    Public Shared importType_ As Integer
    Public Shared importProceed_ As Boolean = False
    Public Shared importFileName_ As String = String.Empty

    Public Shared importedItems As List(Of String) = New List(Of String)
    Public Shared importedItems_ As List(Of String) = New List(Of String)
    Public Shared importedItemsNotFound As List(Of String) = New List(Of String)
    Public Shared importedItemCode As List(Of String) = New List(Of String)

    Public Shared Sub importSelectFile()
        Try
            importProceed_ = False
            importType_ = ImportFileRules.UnknownImport
            importFileName_ = String.Empty
            ImportData = Nothing
            ImportDataCSV = Nothing

            Dim ofd As New OpenFileDialog() With {
                .Filter = "Supported Import Files|*.xlsx;*.xls;*.csv|" &
                          "Excel Workbook|*.xlsx;*.xls|" &
                          "CSV File|*.csv"
            }

            Using ofd
                If ofd.ShowDialog() <> DialogResult.OK Then Exit Sub

                importFileName_ = ofd.FileName
                importType_ = ImportFileRules.GetImportType(importFileName_)

                If importType_ = ImportFileRules.UnknownImport Then
                    MessageBox.Show(
                        "Filename not valid. Include Quotation, Sale, Transfer, " &
                        "Purchase, or WEBSITE in the filename.",
                        "Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation)
                    Exit Sub
                End If

                If importType_ = ImportFileRules.TransferOrPurchaseImport AndAlso
                   ImportFileRules.IsCsv(importFileName_) Then
                    MessageBox.Show(
                        "Transfer and Purchase imports require an Excel workbook " &
                        "with a Contents sheet.",
                        "Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation)
                    Exit Sub
                End If

                ' Legacy quotation CSV files are validated by frmUploader.
                If importType_ = ImportFileRules.QuotationOrSaleImport AndAlso
                   ImportFileRules.IsCsv(importFileName_) Then
                    importProceed_ = True
                    Exit Sub
                End If

                Using stream As FileStream =
                    File.Open(importFileName_, FileMode.Open, FileAccess.Read)
                    Dim reader As IExcelDataReader

                    If ImportFileRules.IsCsv(importFileName_) Then
                        reader = ExcelReaderFactory.CreateCsvReader(stream)
                    Else
                        reader = ExcelReaderFactory.CreateReader(stream)
                    End If

                    Using reader
                        Dim conf As New ExcelDataSetConfiguration() With {
                            .ConfigureDataTable =
                                Function(__) New ExcelDataTableConfiguration() With {
                                    .UseHeaderRow = True
                                }
                        }
                        Dim result As DataSet = reader.AsDataSet(conf)

                        If importType_ = ImportFileRules.WebsiteImport Then
                            If result.Tables.Count = 0 Then
                                Throw New InvalidDataException(
                                    "The WEBSITE CSV file contains no data.")
                            End If
                            ImportDataCSV = result.Tables(0)
                        Else
                            ImportData = result.Tables
                        End If
                    End Using
                End Using

                importProceed_ = True
            End Using

        Catch ex As Exception
            importProceed_ = False
            MessageBox.Show(
                "Unable to open the import file. Close it in Excel and try again." &
                vbCrLf & vbCrLf & ex.Message,
                "Import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation)
        End Try

    End Sub


    Public Shared Function checkIfNull(ByVal str)

        Dim res = Nothing
        If Not IsDBNull(str) Then
            res = str
            Return res
        Else
            Return res
        End If

    End Function

    Public Shared Sub importOrder()

        Dim Order = (From x In clsImport.ImportData("Entry").AsEnumerable()
                     Join c In db.Customers On Convert.ToInt32(x("CustomerID")) Equals c.ID
                     Join s In db.SalesReps On Convert.ToInt32(x("SalesRepID")) Equals s.ID
                           Select New With {
                         .Company = c.Company,
                         .AccountNumber = c.AccountNumber,
                         .PriceLevel = c.PriceLevel,
                         .Employee = c.Employee,
                         .Title = c.Title,
                         .CustID = c.ID,
                         .CustomText5 = c.CustomText5,
                         .TaxExempt = c.TaxExempt,
                         .SalesRepID = s.ID,
                         .SaleRepName = s.Name,
                         .Comment = x("Comment"),
                         .Type = x("Type")}).SingleOrDefault

        iAccountNumber_ = Order.AccountNumber
        iPriceLevel_ = Order.PriceLevel
        iEmployee_ = Order.Employee
        iTitle_ = checkIfNull(Order.Title)
        iTaxExempt_ = Order.TaxExempt
        iCustID_ = Order.CustID
        iCompany_ = Order.Company
        iCustomText5_ = checkIfNull(Order.CustomText5)
        iSalesRepID_ = Order.SalesRepID
        iSaleRepName_ = Order.SaleRepName
        iComment_ = checkIfNull(Order.Comment)
        iType_ = Order.Type


    End Sub

    Public Shared Function importOrderEntry()

        Dim Order = (From x In ImportData("Contents").AsEnumerable()
                     Join i In db.Items On x.Field(Of String)("ItemLookupCode") Equals i.ItemLookupCode
                     Select New With {
                      .ItemLookupCode = x("ItemLookupCode"),
                      .ItemName = i.Description.ToString() & i.ExtendedDescription.ToString(),
                      .QuantityOnOrder = x("QuantityOnOrder"),
                      .Price = x("Price"),
                      .DISC = 0,
                      .Total = Convert.ToDouble(x("Price") * x("QuantityOnOrder")),
                      .LessV = Convert.ToDouble(x("FullPrice") / 1.12),
                      .VSales = Convert.ToDouble(x("FullPrice") - (x("FullPrice") / 1.12)),
                      .DiscP = 0,
                      .Cost = x("Cost"),
                      .Taxable = x("Taxable"),
                      .ItemID = x("ID"),
                      .FullPrice = x("FullPrice"),
                      .Description = i.Description,
                      .ExtendedDescription = i.ExtendedDescription,
                      .ID = 0,
                      .DiscAmount = 0,
                      .Comment = checkIfNull(x("Comment"))}).ToList()

        importedItemCode.Clear()
        For Each x In Order
            importedItemCode.Add(x.ItemLookupCode)
        Next

        Return Order


    End Function


    Public Shared Function importPOEntry()

        Dim _PO = (From x In ImportData("Contents").AsEnumerable()
                  Select New With {
                   .ItemLookupCode = x("ItemLookupCode")}).ToList()

        Dim PO = (From x In ImportData("Contents").AsEnumerable()
                      Join i In db.Items On x.Field(Of String)("ItemLookupCode") Equals i.ItemLookupCode
                     Select New With {
                      .ItemLookupCode = x("ItemLookupCode"),
                      .ItemName = i.Description.ToString() & i.ExtendedDescription.ToString(),
                      .QuantityOnOrder = x("QuantityOrdered"),
                      .Price = x("Price"),
                      .DISC = 0,
                      .Total = Convert.ToDouble(x("Price") * x("QuantityOrdered")),
                      .LessV = 0,
                      .VSales = 0,
                      .DiscP = 0,
                      .Cost = x("Price"),
                      .Taxable = 0,
                      .ItemID = i.ID,
                      .FullPrice = i.Price,
                      .Description = i.Description,
                      .ExtendedDescription = i.ExtendedDescription,
                      .ID = 0,
                      .DiscAmount = 0,
                      .Comment = ""}).ToList()
        importedItems.Clear()
        importedItems_.Clear()
        importedItemsNotFound.Clear()

        importedItemCode.Clear()
        For Each x In PO
            importedItemCode.Add(x.ItemLookupCode)
        Next


        For Each x In PO
            importedItems.Add(x.ItemLookupCode)
        Next


        For Each y In _PO
            importedItems_.Add(y.ItemLookupCode)
        Next

        Return PO

    End Function


    Public Shared Function importFromWebsite(ByVal orderNumber As String) As Object


        Dim s = (From x In ImportDataCSV.AsEnumerable
                   Where x("OrderID").ToString().Equals(orderNumber)
                   Select New With {
                   .CustomerName = x("CustomerName").ToString,
                   .Shipping = x("Shipping").ToString,
                   .Date = x("OrderPlaced").ToString}).FirstOrDefault


        Dim r As DataRow = ImportDataCSV.NewRow
        r("OrderID") = Convert.ToInt32(orderNumber)
        r("ItemLookupCode") = My.Settings.ImportDefaultShipping
        r("QuantityOrdered") = Convert.ToInt32(1)
        r("Price") = Convert.ToDouble(s.Shipping)
        r("OrderPlaced") = s.Date
        r("CustomerName") = s.CustomerName
        r("Shipping") = Convert.ToDouble(s.Shipping)
        ImportDataCSV.Rows.Add(r)


        Dim res = (From x In ImportDataCSV.AsEnumerable
                   Where x("OrderID").ToString().Equals(orderNumber)
                   Select New With {
                   .ItemLookupCode = x("ItemLookupCode")}).ToList()

        Dim _res = (From x In ImportDataCSV.AsEnumerable
                      Join i In db.Items On x("ItemLookupCode") Equals i.ItemLookupCode
                       Where x("OrderID").ToString().Equals(orderNumber)
                     Select New With {
                      .ItemLookupCode = i.ItemLookupCode,
                      .ItemName = i.Description.ToString() & i.ExtendedDescription.ToString(),
                      .QuantityOnOrder = x("QuantityOrdered"),
                      .Price = x("Price"),
                      .DISC = 0,
                      .Total = x("Price") * x("QuantityOrdered"),
                      .LessV = 0,
                      .VSales = 0,
                      .DiscP = 0,
                      .Cost = i.Cost,
                      .Taxable = 0,
                      .ItemID = i.ID,
                      .FullPrice = i.Price,
                      .Description = i.Description,
                      .ExtendedDescription = i.ExtendedDescription,
                      .ID = 0,
                      .DiscAmount = 0,
                      .Comment = "SOD WEBSITE / Order Number: " & x("OrderID") & " / Customer Name: " & x("CustomerName")}).ToList()

        importedItems.Clear()
        importedItems_.Clear()
        importedItemsNotFound.Clear()

        importedItemCode.Clear()

        For Each x In _res
            importedItemCode.Add(x.ItemLookupCode)
        Next

        For Each x In _res
            importedItems.Add(x.ItemLookupCode)
        Next


        For Each y In res
            importedItems_.Add(y.ItemLookupCode)
        Next

        Return _res

    End Function

    Public Shared Function checkIfItemCodeExist() As Integer

        Try

            importedItemsNotFound.Clear()
            Dim Difference = importedItems_.Except(importedItems).ToArray()


            For Each x As String In Difference
                Dim desc = (From z In ImportData("Contents").AsEnumerable() Where z("ItemLookupCode").Equals(x) Select z("Fulldesc")).SingleOrDefault
                importedItemsNotFound.Add(x & " - " & desc)
            Next


            If importedItemsNotFound.Count > 0 Then

                Dim res = MessageBox.Show("There are Items that cannot be Imported due to the Item Code not existed or changed in the Current Database. " & vbCrLf & vbCrLf &
                                        "Some Item Code from other Branches are not the same at the momment. Item Code will be the same once all the branches uses the Consolidated Master Database." & vbCrLf & vbCrLf &
                                            "Do you want to show it ?",
                                            "Message!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation)
                If res = MsgBoxResult.Yes Then

                    'Dim res1 = MessageBox.Show("Items Found : " & importedItemsNotFound.Count & vbCrLf & vbCrLf &
                    '                           String.Join(vbCrLf & vbCrLf, importedItemsNotFound.ToArray()) & vbCrLf & vbCrLf &
                    '                           "Do you want to Search and Replace them? ", "Message!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation)
                    'If res1 = MsgBoxResult.Yes Then
                    For Each y In importedItemsNotFound

                        frmImportItems.griditems.Rows.Add(y)

                    Next

                    frmImportItems.Show()
                End If

                'End If

                Return 1
            Else
                Return 0

            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return 0
        End Try

    End Function

End Class
