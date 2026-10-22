Imports ExcelDataReader
Imports System.IO

Public Class clsImport

    Public Shared ImportData As DataTableCollection

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

    Public Shared Sub importSelectFile()
        Try
            Dim ofd As OpenFileDialog = New OpenFileDialog() With {.Filter = "Excel Workbook|*.xlsx;*.xls"}

            Using ofd
                If ofd.ShowDialog() = DialogResult.OK Then

                    If ofd.FileName.Contains("Quotation") Or ofd.FileName.Contains("Sale") Or ofd.FileName.Contains("Transfer") Or ofd.FileName.Contains("Purchase") Then

                        Using stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read)
                            Using reader As IExcelDataReader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream)
                                Dim result As DataSet = reader.AsDataSet(New ExcelDataReader.ExcelDataSetConfiguration() With {
                                                                             .ConfigureDataTable = Function(__) New ExcelDataReader.ExcelDataTableConfiguration() With {
                                                                            .UseHeaderRow = True}})
                                ImportData = result.Tables
                            End Using
                        End Using
                    Else
                        MessageBox.Show("Excel Filename not Valid!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        Exit Sub
                    End If

                    importProceed_ = True

                Else
                    importProceed_ = False
                    Exit Sub
                End If

            End Using

            If ofd.FileName.Contains("Quotation") Or ofd.FileName.Contains("Sale") Then
                importType_ = 1
            ElseIf ofd.FileName.Contains("Transfer") Or ofd.FileName.Contains("Purchase") Then
                importType_ = 2
            End If

        Catch ex As Exception
            MessageBox.Show("Please Close the Excel File Before Opening !" & ex.Message, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
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

        Return Order

    End Function


    Public Shared Function importPOEntry()

        Dim PO = (From x In ImportData("Contents").AsEnumerable()
                     Join i In db.Items On Convert.ToInt32(x("ItemID")) Equals i.ID
                     Select New With {
                      .ItemLookupCode = i.ItemLookupCode,
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

        Return PO

    End Function

End Class
