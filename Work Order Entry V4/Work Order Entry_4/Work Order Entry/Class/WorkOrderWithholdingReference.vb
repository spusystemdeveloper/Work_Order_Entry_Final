Imports System.Globalization

' Print-only reference. Never changes an order or fills an RMS tender.
Public NotInheritable Class WorkOrderWithholdingReference
    Private Sub New()
    End Sub

    Public Shared Function FormatAmounts(basis As Decimal, rates As IEnumerable(Of Integer)) As String
        Dim amounts = rates.Select(Function(rate) rate.ToString(CultureInfo.InvariantCulture) &
            "%: " & CustomerWithholdingSuggestions.Amount(basis, rate).ToString("N2")).ToList()
        Dim lines As New List(Of String)()
        For index As Integer = 0 To amounts.Count - 1 Step 2
            lines.Add(String.Join("     ", amounts.Skip(index).Take(2)))
        Next
        Return String.Join(Environment.NewLine, lines)
    End Function

    Public Shared Function Load(dbx As ItemLookUpDataContext, orderID As Integer,
                                printedSubtotal As String, templateName As String) As String
        If orderID <= 0 OrElse templateName <> "Work Order" Then Return ""
        Dim basis As Decimal
        If Not Decimal.TryParse(printedSubtotal, NumberStyles.Number,
                                CultureInfo.CurrentCulture, basis) Then Return ""
        Try
            ' Read the saved order's customer, not the main form's selection.
            Dim order = dbx.ExecuteQuery(Of WithholdingOrderCustomer)(
                "SELECT CustomerID FROM dbo.[Order] WHERE ID={0} AND [Type]=2", orderID).SingleOrDefault()
            If order Is Nothing OrElse order.CustomerID <= 0 Then Return ""
            Return FormatAmounts(basis, CustomerWithholdingSuggestions.LoadRates(dbx, order.CustomerID))
        Catch ex As Exception
            ' An unavailable optional reference must not prevent existing printing.
            ' Do not include connection strings or SQL credentials in diagnostics.
            System.Diagnostics.Trace.TraceWarning("Work Order withholding reference is unavailable.")
            Return ""
        End Try
    End Function
End Class

Public Class WithholdingOrderCustomer
    Public Property CustomerID As Integer
End Class
