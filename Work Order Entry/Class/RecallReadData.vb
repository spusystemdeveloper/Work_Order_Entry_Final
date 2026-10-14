Option Strict On

Imports System.Collections.Generic
Imports System.Linq

' These results belong to one recall only. No data is shared between users or recalls.
Public NotInheritable Class RecallReadData
    Private Sub New()
    End Sub

    Public Shared Function ResolveCustomer(context As ItemLookUpDataContext, customerID As Integer, customerText As String) As Integer
        If customerID > 0 Then Return customerID
        Dim text = If(customerText, "").Trim()
        If text.Length = 0 Then Return 0
        Dim resolved = (From customer In context.Customers
                        Where customer.Company = text OrElse customer.AccountNumber = text
                        Select customer.ID).FirstOrDefault()
        If resolved <= 0 Then
            resolved = (From customer In context.Customers
                        Where customer.Company.Trim() = text
                        Select customer.ID).FirstOrDefault()
        End If
        Return resolved
    End Function

    Public Shared Function LoadQueue(context As ItemLookUpDataContext, orderID As Integer) As Dictionary(Of Integer, RecallQueueDetail)
        Dim rows = (From detail In context.QueueingItems
                    Join header In context.Queueings On detail.QueueingID Equals header.id
                    Where header.OrderID = orderID AndAlso detail.ItemID.HasValue
                    Select New RecallQueueDetail With {
                        .ItemID = detail.ItemID.Value,
                        .Prepared = detail.QtyPre,
                        .Status = detail.Status,
                        .Picker = detail.Picker}).ToList()
        ' Match the previous FirstOrDefault behavior for an item appearing more than once.
        Return rows.GroupBy(Function(row) row.ItemID).
            ToDictionary(Function(group) group.Key, Function(group) group.First())
    End Function

    Public Shared Function LoadLastPrices(context As ItemLookUpDataContext, customerID As Integer,
                                          itemCodes As IEnumerable(Of String)) As Dictionary(Of String, Decimal)
        Dim result As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
        If customerID <= 0 Then Return result
        Dim codes = itemCodes.Select(Function(code) If(code, "").Trim()).
            Where(Function(code) code.Length > 0).Distinct(StringComparer.Ordinal).ToList()
        ' Bound parameter count for large orders. Values are always SQL parameters.
        For offset As Integer = 0 To codes.Count - 1 Step 200
            Dim batch = codes.Skip(offset).Take(200).ToList()
            Dim arguments As New List(Of Object) From {customerID}
            arguments.AddRange(batch.Cast(Of Object)())
            Dim placeholders = String.Join(",", Enumerable.Range(1, batch.Count).
                Select(Function(index) "(" & index & ",{" & index & "})"))
            ' Rank the relevant sales together instead of scanning history for every item.
            ' RequestID also preserves database collation behavior for codes differing by case.
            Dim sql = "WITH Requested AS (SELECT codes.RequestID,codes.ItemCode FROM (VALUES " &
                placeholders & ") AS codes(RequestID,ItemCode)), " &
                "RankedSales AS (SELECT requested.RequestID,te.Price, " &
                "CASE WHEN t.ReferenceNumber LIKE N'CH%' OR t.ReferenceNumber LIKE N'CI%' THEN 1 ELSE 0 END AS IsInvoice, " &
                "ROW_NUMBER() OVER (PARTITION BY requested.RequestID ORDER BY t.Time DESC,t.TransactionNumber DESC) AS AnyRank, " &
                "ROW_NUMBER() OVER (PARTITION BY requested.RequestID ORDER BY " &
                "CASE WHEN t.ReferenceNumber LIKE N'CH%' OR t.ReferenceNumber LIKE N'CI%' THEN 0 ELSE 1 END, " &
                "t.Time DESC,t.TransactionNumber DESC) AS InvoiceRank " &
                "FROM Requested requested JOIN dbo.Item i ON i.ItemLookupCode=requested.ItemCode " &
                "JOIN dbo.TransactionEntry te ON te.ItemID=i.ID " &
                "JOIN dbo.[Transaction] t ON t.TransactionNumber=te.TransactionNumber " &
                "WHERE t.CustomerID={0} AND te.Quantity>0), " &
                "Prices AS (SELECT RequestID, " &
                "MAX(CASE WHEN AnyRank=1 THEN Price END) AS LatestPrice, " &
                "MAX(CASE WHEN InvoiceRank=1 AND IsInvoice=1 THEN Price END) AS InvoicePrice " &
                "FROM RankedSales GROUP BY RequestID) " &
                "SELECT requested.ItemCode, CONVERT(decimal(19,4), " &
                "COALESCE(NULLIF(prices.InvoicePrice,0),prices.LatestPrice,0)) AS Price " &
                "FROM Requested requested LEFT JOIN Prices prices ON requested.RequestID=prices.RequestID"
            For Each row In context.ExecuteQuery(Of RecallLastPrice)(sql, arguments.ToArray())
                result(row.ItemCode.Trim()) = row.Price
            Next
        Next
        Return result
    End Function

    Public Shared Function LoadTaxability(context As ItemLookUpDataContext, itemCodes As IEnumerable(Of String)) As Dictionary(Of String, Boolean)
        Dim result As New Dictionary(Of String, Boolean)(StringComparer.Ordinal)
        ' Keep the original lookup text; the existing tax lookup does not trim codes.
        Dim codes = itemCodes.Where(Function(code) Not String.IsNullOrEmpty(code)).
            Distinct(StringComparer.Ordinal).ToList()
        For offset As Integer = 0 To codes.Count - 1 Step 200
            Dim batch = codes.Skip(offset).Take(200).ToList()
            Dim placeholders = String.Join(",", Enumerable.Range(0, batch.Count).
                Select(Function(index) "({" & index & "})"))
            Dim sql = "SELECT requested.ItemCode, CONVERT(bit,COALESCE(" &
                "(SELECT TOP (1) i.Taxable FROM dbo.Item i WHERE i.ItemLookupCode=requested.ItemCode),0)) AS Taxable " &
                "FROM (VALUES " & placeholders & ") AS requested(ItemCode)"
            For Each row In context.ExecuteQuery(Of RecallItemTaxability)(sql, batch.Cast(Of Object)().ToArray())
                result(row.ItemCode) = row.Taxable
            Next
        Next
        Return result
    End Function

    Public Shared Function LastPriceOrZero(prices As Dictionary(Of String, Decimal), itemCode As String) As Decimal
        Dim value As Decimal
        If prices.TryGetValue(If(itemCode, "").Trim(), value) Then Return value
        Return 0D
    End Function
End Class

Public Class RecallQueueDetail
    Public Property ItemID As Integer
    Public Property Prepared As Integer?
    Public Property Status As String
    Public Property Picker As String
End Class

Public Class RecallLastPrice
    Public Property ItemCode As String
    Public Property Price As Decimal
End Class

Public Class RecallItemTaxability
    Public Property ItemCode As String
    Public Property Taxable As Boolean
End Class
