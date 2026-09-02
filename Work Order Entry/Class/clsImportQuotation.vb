Public Class clsImportQuotation

    Public Shared Function CheckExistItem(ByVal sItemcode As String)
        Using dbx = GetDB()
            Dim bExist = (From i In dbx.GetTable(Of Item)() _
                          Where i.ItemLookupCode.Equals(sItemcode) _
                          Select i).SingleOrDefault

            If bExist Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Using

    End Function

    Public Shared Function CheckExistCustomer(ByVal sAcctNo As String)
        Using dbx = GetDB()
            Dim bExist = (From c In dbx.GetTable(Of Customer)() _
                      Where c.AccountNumber.Equals(sAcctNo) _
                      Select c).SingleOrDefault

            If bExist Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Using

    End Function

    Public Shared Function CheckQuotationLog(ByVal iRefID As Integer, ByVal iSalesRepID As Integer) As Boolean
        Using dbx = GetDB()
            Dim bExist = (From q In dbx.GetTable(Of SOD_ImportedQuote)() _
                Where q.ReferenceID.Equals(iRefID) And q.SalesRepID.Equals(iSalesRepID) _
                  Select q).SingleOrDefault

            If bExist Is Nothing Then
                Return False
            Else
                Return True
            End If
        End Using

    End Function

    Public Shared Function PopulateError(ByVal oGrid As Object, ByVal iRowNo As Integer, ByVal iColNo As Integer, ByVal iData As String, ByVal iMsg As String) As String
        Dim sErrMsg As String = String.Empty

        With oGrid.Rows.Item(oGrid.Rows.Add())
            .Cells(0).Value = iRowNo
            .Cells(1).Value = iColNo
            .Cells(2).Value = iData
            .Cells(3).Value = iMsg
        End With

        sErrMsg = sErrMsg & "--> " & "Row " & iRowNo & ", Col " & iColNo & ": " & iData & " (" & iMsg & ")" & vbNewLine
        Return sErrMsg

    End Function

    Public Shared Function GetItemProperties(ByVal sItemCode As String, ByVal sColName As String)
        Using dbx = GetDB()
            Dim item = (From i In dbx.Items
                        Where i.ItemLookupCode.Equals(sItemCode) _
                        Select i).ToArray

            Dim sVal As String = String.Empty

            If item Is Nothing Then
                Return String.Empty
            Else
                For Each x In item

                    Select Case sColName
                        Case "Description"
                            sVal = x.Description.ToString
                        Case "Cost"
                            sVal = x.Cost.ToString
                        Case "Taxable"
                            sVal = 1
                        Case "Id"
                            sVal = x.ID.ToString
                        Case "ExtDesc"
                            sVal = x.ExtendedDescription.ToString
                        Case Else
                            sVal = String.Empty
                    End Select

                Next
                Return sVal
            End If
        End Using

    End Function
    Public Shared Function GetCustomerProperties(ByVal sAcctNo As String, ByVal sColName As String)
        Using dbx = GetDB()
            Dim cust = (From c In dbx.Customers
                        Where c.AccountNumber.Equals(sAcctNo) _
                        Select c).ToArray

            Dim sVal As String = String.Empty

            If cust Is Nothing Then
                Return String.Empty
            Else
                For Each x In cust

                    Select Case sColName
                        Case "Company"
                            sVal = x.Company.ToString
                        Case "TaxExempt"
                            sVal = x.TaxExempt.ToString
                        Case "Taxable"
                            sVal = 1
                        Case Else
                            sVal = String.Empty
                    End Select

                Next
                Return sVal
            End If
        End Using

    End Function

    Public Shared Sub InsertQuotationLog(ByVal quoteid As Integer, ByVal referenceid As Integer, ByVal salesrepid As Integer)


        Using dbx = GetDB()
            Dim data = (From q In dbx.GetTable(Of SOD_ImportedQuote)() _
                                Where q.ReferenceID = referenceid And q.SalesRepID = salesrepid _
                                Select q).SingleOrDefault

            If data Is Nothing Then

                Dim r As New SOD_ImportedQuote With { _
                .ImportDate = DateTime.Today, _
                .QuoteID = quoteid, _
                .ReferenceID = referenceid, _
                .SalesRepID = salesrepid _
                }
                dbx.SOD_ImportedQuotes.InsertOnSubmit(r)

             End If

            Try

                dbx.SubmitChanges()

            Catch ex As Exception
                MsgBox("Error on saving quotation!" & vbNewLine & ex.ToString, vbExclamation, "Message")
            End Try
        End Using

    End Sub

End Class
