Imports System.IO
Public Class frmUploader

    Dim sErrMsg As String = String.Empty
    Dim sSucMsg As String = String.Empty
    Dim sComp As String = String.Empty
    Private sArrayQuote() As clsImportQuotation
    ' Quotation numbers of the rows added to the order by this upload.
    Private ReadOnly importedQuotes As New List(Of ImportedQuoteReference)

    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click
        Me.Close()
    End Sub

    Private Sub cmdImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdImport.Click

        If cmdImport.Text = "&Finish" Then
            bSuccessImport = True
            Me.Close()
        Else
            panelInfo.Visible = False
            Using LoadingOverlay.Show(Me, "Importing items...")
                VerifyImportFile()
            End Using

            ' The rows are already added, so switch to Finish before writing the log.
            ' A log failure must not let Import run again and add the rows twice.
            cmdImport.Text = "&Finish"
            cmdReport.Enabled = True

            If CInt(lblError.Text) > 0 Then
                cmdImport.Enabled = False
                bSuccessImport = False
                frmItemLookUp.gridSelectItem.Rows.Clear()
                frmItemLookUp.ClearPendingImportedQuotes()
                frmItemLookUp.UpdateAmt()                
            Else
                cmdImport.Enabled = True
                bSuccessImport = True
                frmItemLookUp.AddPendingImportedQuotes(importedQuotes)
            End If

            SaveLogToTxt()

        End If

    End Sub

    Private Sub VerifyImportFile()
        Try
            frmItemLookUp.isBulkLoading = True
            frmItemLookUp.gridSelectItem.SuspendLayout()
            Dim parser As New FileIO.TextFieldParser(lblFile.Text)
            parser.Delimiters = New String() {","} ' fields are separated by comma
            parser.HasFieldsEnclosedInQuotes = True ' each of the values is enclosed with double quotes
            parser.TrimWhiteSpace = True
            parser.CommentTokens = New String() {";"}

            Dim iRow, iRowNo, iSuccess, iError As Integer
            Dim bQuote, bCust, bDate, bItem, bQty

            iRow = 0
            iRowNo = 6
            parser.ReadLine()
            GridDesign()

            Do While Not parser.EndOfData

                Dim fields As String() = parser.ReadFields

                If iRow >= 2 Then

                    '#For Quotation Number 
                    If Integer.TryParse(fields(0), Nothing) Then 'Check if column(A) is integer
                        If clsImportQuotation.CheckQuotationLog(fields(0), iSalesID) = True Then 'Check if quotation already exist
                            sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(0), "Quotation No already exist/uploaded.")
                            bQuote = False
                        ElseIf frmItemLookUp.IsQuotationPendingImport(CInt(fields(0)), iSalesID) Then
                            ' Uploaded earlier into this unsaved order.
                            sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(0), "Quotation No is already on this order.")
                            bQuote = False
                        Else
                            bQuote = True
                        End If
                    Else
                        sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(0), "Not a valid Quotation No.")
                        bQuote = False
                    End If
                    '#End For Quotation Number

                    '#For Customer
                    If clsImportQuotation.CheckExistCustomer(fields(2)) = False Then
                        sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(2), "Customer Account does not exist.")
                        bCust = False
                    Else
                        bCust = True
                    End If
                    '#End For Customer

                    '#For Date
                    If IsDate(fields(3)) = False Then
                        sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(3), "Invalid date format.")
                        bDate = False
                    Else
                        bDate = True
                    End If
                    '#End For Date

                    '#For Itemlookupcode
                    If clsImportQuotation.CheckExistItem(fields(5)) = False Then
                        sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(5), "Item Lookup Code does not exist.")
                        bItem = False
                    Else
                        bItem = True
                    End If
                    '#End for Itemlookupcode

                    '#For Qty
                    If IsNumeric(fields(7)) = False Then
                        sErrMsg = sErrMsg & clsImportQuotation.PopulateError(Me.gridInfo, iRowNo, 1, fields(7), "Invalid quantity value.")
                        bQty = False
                    Else
                        bQty = True
                    End If
                    '#End For Qty

                    ' A missing item is an error too, so the line is not added to the order.
                    If bQuote And bCust And bDate And bItem And bQty Then

                        bTaxExcempt = clsImportQuotation.GetCustomerProperties(fields(2), "TaxExempt")
                        sItemCode = fields(5)
                        iRefNum = fields(0)

                        clsItemLookUp.GetPrice(1, clsImportQuotation.GetItemProperties(fields(5), "Taxable"), fields(8))

                        Dim previousAllowUserToAddRows = frmItemLookUp.gridSelectItem.AllowUserToAddRows
                        frmItemLookUp.gridSelectItem.AllowUserToAddRows = True


                        With frmItemLookUp

                            Dim newTimeRecord As DataGridViewRow = frmItemLookUp.gridSelectItem.Rows(frmItemLookUp.gridSelectItem.NewRowIndex).Clone

                            newTimeRecord.Cells(.ItemCode.Index).Value = fields(5)
                            newTimeRecord.Cells(.ItemName.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "Description")
                            newTimeRecord.Cells(.QTY.Index).Value = CDbl(fields(7))
                            newTimeRecord.Cells(.Price.Index).Value = CDbl(fields(8))
                            newTimeRecord.Cells(.DISC.Index).Value = 0
                            newTimeRecord.Cells(.TOTAL.Index).Value = CDbl(fields(7)) * CDbl(fields(8))
                            newTimeRecord.Cells(.LessV.Index).Value = dTSales
                            newTimeRecord.Cells(.VSales.Index).Value = dVSales
                            newTimeRecord.Cells(.DiscP.Index).Value = dDisc
                            newTimeRecord.Cells(.Cost.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "Cost")
                            newTimeRecord.Cells(.Taxable.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "Taxable")
                            newTimeRecord.Cells(.ItemID.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "Id")
                            newTimeRecord.Cells(.FullPrice.Index).Value = CDbl(fields(8))
                            newTimeRecord.Cells(.Description.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "Description")
                            newTimeRecord.Cells(.Extended.Index).Value = clsImportQuotation.GetItemProperties(fields(5), "ExtDesc")
                            newTimeRecord.Cells(.DiscAmount.Index).Value = 0


                            .gridSelectItem.Rows.Add(newTimeRecord)

                            .gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
                            '.gridSelectItem.CurrentCell = .gridSelectItem(2, .gridSelectItem.RowCount - 1)
                            '.gridSelectItem.BeginEdit(True)

                        End With

                        Dim quoteReference As New ImportedQuoteReference(CInt(fields(0)), iSalesID)
                        If Not importedQuotes.Contains(quoteReference) Then importedQuotes.Add(quoteReference)

                        sSucMsg = sSucMsg & "--> Row " & iRowNo & " (Verified)" & vbNewLine
                        With gridInfo.Rows.Item(gridInfo.Rows.Add())
                            .Cells(0).Value = iRowNo
                            .Cells(1).Value = String.Empty
                            .Cells(2).Value = String.Empty
                            .Cells(3).Value = "Success"
                        End With

                        iSuccess = iSuccess + 1

                    Else
                        iError = iError + 1
                    End If

                    iRowNo = iRowNo + 1
                End If

                iRow = iRow + 1

            Loop

            lblTotal.Text = iRow - 2
            lblSuccess.Text = iSuccess
            lblError.Text = iError
            frmItemLookUp.gridSelectItem.ResumeLayout()
            frmItemLookUp.UpdateAmt()

        Catch ex As Exception
            AppPrompt.MsgBox(ex.ToString, vbCritical, "Program Error")
        Finally
            frmItemLookUp.gridSelectItem.ResumeLayout()
            frmItemLookUp.isBulkLoading = False
        End Try

    End Sub

    Private Sub GridDesign()
        Dim col1, col2, col3, col4 As New DataGridViewTextBoxColumn
        col1.HeaderText = "Row #"
        col2.HeaderText = "Col #"
        col3.HeaderText = "Data"
        col4.HeaderText = "Message"

        With gridInfo
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            .Columns.Add(col1)
            .Columns.Add(col2)
            .Columns.Add(col3)
            .Columns.Add(col4)

            .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            .Columns(0).Width = 74
            .Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            .Columns(1).Width = 74
            .Columns(2).MinimumWidth = 180
            .Columns(2).FillWeight = 35
            .Columns(3).MinimumWidth = 320
            .Columns(3).FillWeight = 65

            .RowTemplate.Height = 40
            .DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 160)
            .DefaultCellStyle.SelectionForeColor = Color.White

            .Font = New Font("Tahoma", 10)


        End With


    End Sub
    ' Writes the upload log to the ImportLogFolder setting (default: the original
    ' shared folder). If that folder cannot be reached, the log is saved on this PC.
    ' Logging never affects the rows already added to the order.
    Private Sub SaveLogToTxt()
        Dim sFilename As String = Path.GetFileNameWithoutExtension(lblFile.Text) & ".txt"
        Dim sharedFolder As String = StoreProcessingSettings.ImportLogFolder
        Dim localFolder As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WorkOrderEntry", "ImportLogs")

        Try
            WriteLog(Path.Combine(sharedFolder, sFilename))
            Return
        Catch ex As Exception
            ErrorLogger.LogError(ex, "Writing the import log to " & sharedFolder)
        End Try

        Try
            Directory.CreateDirectory(localFolder)
            Dim localPath As String = Path.Combine(localFolder, sFilename)
            WriteLog(localPath)
            AppPrompt.Show("The import log folder could not be reached:" & vbNewLine & sharedFolder & vbNewLine & vbNewLine &
                           "The log was saved on this PC instead:" & vbNewLine & localPath & vbNewLine & vbNewLine &
                           "The imported items are not affected.",
                           "Import Log", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            ErrorLogger.LogError(ex, "Writing the import log to " & localFolder)
            AppPrompt.Show("The import log could not be saved. The imported items are not affected.",
                           "Import Log", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub WriteLog(ByVal sFilePath As String)
        Using sw As StreamWriter = File.AppendText(sFilePath)
            sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"))
            sw.WriteLine("#Error")
            sw.WriteLine(IIf(Len(sErrMsg) > 0, sErrMsg, "--> None"))
            sw.WriteLine("#Successful")
            sw.WriteLine(IIf(Len(sSucMsg) > 0, sSucMsg, "--> None"))
            sw.WriteLine(vbNewLine & "------------------------------------------------------------------------------")
        End Using
    End Sub

    Private Sub ImportToWorkOrder()

        frmItemLookUp.gridSelectItem.DataSource = sArrayQuote
        frmItemLookUp.gridSelectItem.Refresh()

    End Sub

End Class