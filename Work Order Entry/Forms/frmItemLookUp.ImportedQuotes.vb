' A quotation number loaded by the CSV uploader, with the sales rep it was
' checked against. SOD_ImportedQuote is keyed by both.
Public Structure ImportedQuoteReference
    Public ReadOnly ReferenceID As Integer
    Public ReadOnly SalesRepID As Integer

    Public Sub New(ByVal referenceID As Integer, ByVal salesRepID As Integer)
        Me.ReferenceID = referenceID
        Me.SalesRepID = salesRepID
    End Sub
End Structure

Partial Public Class frmItemLookUp

    ' Quotations uploaded into the order on screen. The save that commits those
    ' lines records them in SOD_ImportedQuote, so the uploader can reject the
    ' same quotation later. Nothing is recorded if the order is never saved.
    Private ReadOnly pendingImportedQuotes As New List(Of ImportedQuoteReference)

    Friend Function IsQuotationPendingImport(ByVal referenceID As Integer, ByVal salesRepID As Integer) As Boolean
        Return pendingImportedQuotes.Contains(New ImportedQuoteReference(referenceID, salesRepID))
    End Function

    Friend Sub AddPendingImportedQuotes(ByVal references As IEnumerable(Of ImportedQuoteReference))
        For Each reference In references
            If Not pendingImportedQuotes.Contains(reference) Then pendingImportedQuotes.Add(reference)
        Next
    End Sub

    Friend Sub ClearPendingImportedQuotes()
        pendingImportedQuotes.Clear()
    End Sub

    ' Runs inside the save transaction, before SubmitChanges, so the log rows
    ' commit or roll back with the order.
    Private Sub RecordImportedQuotations(ByVal dbSave As ItemLookUpDataContext, ByVal orderID As Integer)
        If pendingImportedQuotes.Count = 0 Then Return

        ' The quotation import table is optional at some branches.
        If dbSave.ExecuteQuery(Of Integer)(
            "SELECT CASE WHEN OBJECT_ID(N'dbo.SOD_ImportedQuote', N'U') IS NULL THEN 0 ELSE 1 END").Single() = 0 Then Return

        For Each reference In pendingImportedQuotes
            Dim referenceID As Integer = reference.ReferenceID
            Dim salesRepID As Integer = reference.SalesRepID
            Dim alreadyLogged = (From q In dbSave.SOD_ImportedQuotes
                                 Where q.ReferenceID.Equals(referenceID) And q.SalesRepID.Equals(salesRepID)
                                 Select q.ID).Any()
            If alreadyLogged Then Continue For

            dbSave.SOD_ImportedQuotes.InsertOnSubmit(New SOD_ImportedQuote With {
                .ImportDate = DateTime.Today,
                .QuoteID = orderID,
                .ReferenceID = referenceID,
                .SalesRepID = salesRepID
            })
        Next
    End Sub

End Class
