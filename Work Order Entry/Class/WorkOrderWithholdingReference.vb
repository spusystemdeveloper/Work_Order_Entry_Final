Imports System.Globalization
Imports System.Text.RegularExpressions

' Print-only reference. Never changes an order or fills an RMS tender.
Public NotInheritable Class WorkOrderWithholdingReference
    Private Sub New()
    End Sub

    Private Const WalkInMarker As String = "\[WO-WTAX:v1;RATES=(?:(?:1|2|5|15)(?:,(?:1|2|5|15))*)?\]"

    Public Shared Function IsWalkIn(dbx As ItemLookUpDataContext, customerID As Integer) As Boolean
        Dim account = dbx.ExecuteQuery(Of String)(
            "SELECT AccountNumber FROM dbo.Customer WHERE ID={0}", customerID).SingleOrDefault()
        Return String.Equals(If(account, "").Trim().Replace("-", "").Replace(" ", ""),
                             "WALKIN", StringComparison.OrdinalIgnoreCase)
    End Function

    ' Nothing means no order override; an empty list explicitly disables all rates.
    Public Shared Function ReadWalkInRates(notes As String) As List(Of Integer)
        Dim matches = Regex.Matches(If(notes, ""), WalkInMarker)
        If matches.Count = 0 Then Return Nothing
        Dim token = matches(matches.Count - 1).Value
        Dim value = token.Substring("[WO-WTAX:v1;RATES=".Length).TrimEnd("]"c)
        If value = "" Then Return New List(Of Integer)()
        Return value.Split(","c).Select(Function(rate) Integer.Parse(rate, CultureInfo.InvariantCulture)).Distinct().OrderBy(Function(rate) rate).ToList()
    End Function

    Public Shared Function WriteWalkInRates(notes As String, rates As IEnumerable(Of Integer)) As String
        Dim original = If(notes, "")
        If rates Is Nothing Then Return Regex.Replace(original, WalkInMarker, "")
        Dim selected = rates.Distinct().OrderBy(Function(rate) rate).ToList()
        If selected.Any(Function(rate) Not {1, 2, 5, 15}.Contains(rate)) Then
            Throw New ArgumentException("Unsupported withholding rate.")
        End If
        Dim token = "[WO-WTAX:v1;RATES=" & String.Join(",", selected) & "]"
        If Regex.IsMatch(original, WalkInMarker) Then
            Return Regex.Replace(original, WalkInMarker, Function(match) token)
        End If
        Return original & If(original.Length = 0, "", Environment.NewLine) & token
    End Function

    Public Shared Function RatesForOrder(dbx As ItemLookUpDataContext, customerID As Integer,
                                         notes As String) As List(Of Integer)
        Dim saved = ReadWalkInRates(notes)
        If saved IsNot Nothing AndAlso IsWalkIn(dbx, customerID) Then Return saved
        Return CustomerWithholdingSuggestions.LoadRates(dbx, customerID)
    End Function

    Public Shared Function FormatItemAmounts(summary As ItemWithholdingSummary, rates As IEnumerable(Of Integer),
                                            paymentType As String, orderTotal As Decimal) As String
        ' Filter rounded reference amounts only; calculation and saved totals stay unchanged.
        Dim nonzeroRates = rates.Where(Function(rate) summary.Amount(rate) <> 0D).ToList()
        Dim payment = If(paymentType, "").Trim().ToUpperInvariant()
        Select Case payment
            Case "DEBIT CARD" : payment = "DEBIT"
            Case "CREDIT CARD" : payment = "CREDIT"
        End Select
        Dim amounts As New List(Of String)()
        For Each rate As Integer In nonzeroRates
            Dim label = "WTax " & rate.ToString(CultureInfo.InvariantCulture) & "%"
            If payment = "CHARGE" Then label &= "_Charge"
            amounts.Add(label & ": " & summary.Amount(rate).ToString("N2"))
        Next
        If nonzeroRates.Count > 0 Then
            Dim withholding = nonzeroRates.Sum(Function(rate) summary.Amount(rate))
            amounts.Add("Total WTax: " & withholding.ToString("N2"))
            If {"CASH", "CHARGE", "DEBIT", "CREDIT"}.Contains(payment) Then
                Dim net = orderTotal - withholding
                Dim paymentLabel = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(payment.ToLowerInvariant())
                amounts.Add("Est. " & paymentLabel & " Due: " & net.ToString("N2"))
            End If
        End If
        If summary.ReviewLines > 0 Then amounts.Add(summary.ReviewMessage())
        Return String.Join(Environment.NewLine, amounts)
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
                "SELECT CustomerID, Total, CONVERT(bit, CASE WHEN Tax<>0 THEN 1 ELSE 0 END) AS Taxable, " &
                "Closed, CONVERT(nvarchar(max), Comment) AS OrderComment, CONVERT(nvarchar(max), ShippingNotes) AS ShippingNotes " &
                "FROM dbo.[Order] WHERE ID={0} AND [Type]=2", orderID).SingleOrDefault()
            If order Is Nothing OrElse order.CustomerID <= 0 Then Return ""
            ' Save/recall store payment in the first semicolon-delimited segment.
            ' A payment word in free-text remarks must not change the tender label.
            Dim payment = If(order.OrderComment, "").Split(";"c)(0).Trim()
            Dim rates = RatesForOrder(dbx, order.CustomerID, order.ShippingNotes)
            If rates.Count = 0 Then Return FormatItemAmounts(New ItemWithholdingSummary(), rates, payment, order.Total)
            Dim lineQuery = If(order.Closed,
                "SELECT e.ItemID, CONVERT(decimal(28,8), e.Price * e.Quantity) AS GrossAmount, CONVERT(nvarchar(max), i.Notes) AS Notes " &
                "FROM dbo.TransactionEntry e INNER JOIN dbo.[Transaction] t ON t.TransactionNumber=e.TransactionNumber " &
                "LEFT JOIN dbo.Item i ON i.ID=e.ItemID WHERE t.RecallID={0}",
                "SELECT e.ItemID, CONVERT(decimal(28,8), e.Price * e.QuantityOnOrder) AS GrossAmount, " &
                "CONVERT(nvarchar(max), i.Notes) AS Notes FROM dbo.OrderEntry e " &
                "LEFT JOIN dbo.Item i ON i.ID=e.ItemID WHERE e.OrderID={0}")
            Dim lines = dbx.ExecuteQuery(Of WithholdingItemLine)(lineQuery, orderID).ToList()
            Return FormatItemAmounts(ItemWithholdingSuggestions.Calculate(lines, basis, order.Taxable), rates, payment, order.Total)
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
    Public Property Total As Decimal
    Public Property Taxable As Boolean
    Public Property Closed As Boolean
    Public Property OrderComment As String
    Public Property ShippingNotes As String
End Class
