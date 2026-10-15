Public Class clsImportQuotation

    ' While an import runs, each item and customer is fetched once. Without this a CSV
    ' row queries the same item up to seven times (exists check plus each property).
    Private Shared itemCache As Dictionary(Of String, Item())
    Private Shared customerCache As Dictionary(Of String, Customer())

    Public Shared Sub BeginLookupCache()
        itemCache = New Dictionary(Of String, Item())(StringComparer.Ordinal)
        customerCache = New Dictionary(Of String, Customer())(StringComparer.Ordinal)
    End Sub

    Public Shared Sub EndLookupCache()
        itemCache = Nothing
        customerCache = Nothing
    End Sub

    Private Shared Function FindItems(ByVal sItemCode As String) As Item()
        Dim found As Item() = Nothing
        If itemCache IsNot Nothing AndAlso sItemCode IsNot Nothing AndAlso itemCache.TryGetValue(sItemCode, found) Then Return found

        Using dbx = GetDB()
            dbx.ObjectTrackingEnabled = False
            found = (From i In dbx.Items
                     Where i.ItemLookupCode.Equals(sItemCode)
                     Select i).ToArray
        End Using

        If itemCache IsNot Nothing AndAlso sItemCode IsNot Nothing Then itemCache(sItemCode) = found
        Return found
    End Function

    Private Shared Function FindCustomers(ByVal sAcctNo As String) As Customer()
        Dim found As Customer() = Nothing
        If customerCache IsNot Nothing AndAlso sAcctNo IsNot Nothing AndAlso customerCache.TryGetValue(sAcctNo, found) Then Return found

        Using dbx = GetDB()
            dbx.ObjectTrackingEnabled = False
            found = (From c In dbx.Customers
                     Where c.AccountNumber.Equals(sAcctNo)
                     Select c).ToArray
        End Using

        If customerCache IsNot Nothing AndAlso sAcctNo IsNot Nothing Then customerCache(sAcctNo) = found
        Return found
    End Function

    Public Shared Function CheckExistItem(ByVal sItemcode As String)
        Dim bExist = FindItems(sItemcode).SingleOrDefault

        If bExist Is Nothing Then
            Return False
        Else
            Return True
        End If

    End Function

    Public Shared Function CheckExistCustomer(ByVal sAcctNo As String)
        Dim bExist = FindCustomers(sAcctNo).SingleOrDefault

        If bExist Is Nothing Then
            Return False
        Else
            Return True
        End If

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
        Dim sVal As String = String.Empty

        For Each x In FindItems(sItemCode)

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

    End Function
    Public Shared Function GetCustomerProperties(ByVal sAcctNo As String, ByVal sColName As String)
        Dim sVal As String = String.Empty

        For Each x In FindCustomers(sAcctNo)

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
