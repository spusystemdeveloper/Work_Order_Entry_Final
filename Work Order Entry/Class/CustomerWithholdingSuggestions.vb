Imports System.Globalization

' Read-only tender estimates. These amounts never feed order totals or saves.
Public NotInheritable Class CustomerWithholdingSuggestions
    Private Sub New()
    End Sub

    Public Shared Function EnabledRates(number3 As Double, number4 As Double,
                                        number5 As Double, text2 As String) As List(Of Integer)
        Dim rates As New List(Of Integer)()
        If number3 = 1R Then rates.Add(1)
        If number4 = 2R Then rates.Add(2)
        If number5 = 5R Then rates.Add(5)
        If String.Equals(If(text2, "").Trim(), "15", StringComparison.Ordinal) Then rates.Add(15)
        Return rates
    End Function

    Public Shared Function Amount(vatExclusiveAmount As Decimal, rate As Integer) As Decimal
        If rate <> 1 AndAlso rate <> 2 AndAlso rate <> 5 AndAlso rate <> 15 Then
            Throw New ArgumentOutOfRangeException(NameOf(rate))
        End If
        Return Decimal.Round(vatExclusiveAmount * (CDec(rate) / 100D),
                             2, MidpointRounding.AwayFromZero)
    End Function

    Public Shared Function LoadRates(dbx As ItemLookUpDataContext, customerID As Integer) As List(Of Integer)
        If dbx Is Nothing Then Throw New ArgumentNullException(NameOf(dbx))
        If customerID <= 0 Then Return New List(Of Integer)()

        ' One query per customer, not per amount change. Caption checks prevent
        ' another branch's unrelated custom fields being treated as W/Tax flags.
        Dim fields = dbx.ExecuteQuery(Of CustomerWithholdingFields)(
            "SELECT c.CustomNumber3, c.CustomNumber4, c.CustomNumber5, c.CustomText2, " &
            "(SELECT MAX(Caption) FROM dbo.CustomCaption WHERE Style=8 HAVING COUNT(*)=1) AS Caption1, " &
            "(SELECT MAX(Caption) FROM dbo.CustomCaption WHERE Style=9 HAVING COUNT(*)=1) AS Caption2, " &
            "(SELECT MAX(Caption) FROM dbo.CustomCaption WHERE Style=10 HAVING COUNT(*)=1) AS Caption5, " &
            "(SELECT MAX(Caption) FROM dbo.CustomCaption WHERE Style=2 HAVING COUNT(*)=1) AS Caption15 " &
            "FROM dbo.Customer c WHERE c.ID={0}", customerID).SingleOrDefault()
        If fields Is Nothing Then Return New List(Of Integer)()

        Return EnabledRates(fields.CustomNumber3, fields.CustomNumber4,
                            fields.CustomNumber5, fields.CustomText2).
            Where(Function(rate) CaptionMatches(fields.CaptionFor(rate), rate)).ToList()
    End Function

    Public Shared Function CaptionMatches(caption As String, rate As Integer) As Boolean
        Dim normalized = String.Concat(If(caption, "").Where(Function(c) Not Char.IsWhiteSpace(c))).
            TrimEnd(":"c)
        Return String.Equals(normalized, "W/Tax" & rate.ToString(CultureInfo.InvariantCulture) & "%",
                             StringComparison.OrdinalIgnoreCase)
    End Function
End Class

' References only: allocate the existing VAT-exclusive subtotal to its item
' classes. Never recalculate or write order VAT, prices, stock or tender values.
Public NotInheritable Class ItemWithholdingSuggestions
    Private Sub New()
    End Sub

    Public Shared Function Calculate(lines As IEnumerable(Of WithholdingItemLine),
                                     subtotal As Decimal, taxable As Boolean) As ItemWithholdingSummary
        Dim items = lines.ToList()
        Dim gross = items.Sum(Function(line) line.GrossAmount)
        If gross = 0D AndAlso subtotal <> 0D Then Throw New ArgumentException("Item amounts do not match the subtotal.")
        Dim factor As Decimal = If(taxable, 1D / 1.12D, 1D)
        Dim result As New ItemWithholdingSummary()
        For Each line In items
            Dim rates As New List(Of Integer)()
            If Not TryGetRates(line.Notes, rates) Then
                result.ReviewLines += 1
                Continue For
            End If
            For Each rate As Integer In rates
                result.Bases(rate) += line.GrossAmount
            Next
        Next
        For Each rate As Integer In {1, 2, 15}
            ' Multiply before dividing: an intermediate repeating VAT factor can
            ' move a half-cent across the rounding boundary.
            If gross = 0D Then
                result.Bases(rate) *= factor
            Else
                result.Bases(rate) = result.Bases(rate) * subtotal / gross
            End If
        Next
        ' Customer-enabled 5% uses the existing whole VAT-exclusive subtotal.
        ' Item markers continue to control income withholding (1%, 2%, 15%) only.
        result.Bases(5) = If(taxable, subtotal, 0D)
        Return result
    End Function

    Private Shared Function TryGetRates(notes As String, rates As List(Of Integer)) As Boolean
        Dim markers As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each match As System.Text.RegularExpressions.Match In
            System.Text.RegularExpressions.Regex.Matches(If(notes, ""), "\[WTAX:[^\]]*\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            ' REVIEW is retired; leave existing Notes text untouched.
            If Not String.Equals(match.Value, "[WTAX:REVIEW]", StringComparison.OrdinalIgnoreCase) Then
                markers.Add(match.Value.ToUpperInvariant())
            End If
        Next
        If markers.Count = 0 Then
            rates.Add(1)  ' Unmarked items default to GOODS; rate eligibility is checked separately.
            Return True
        End If
        Dim primary = markers.Where(Function(marker) {"[WTAX:GOODS]", "[WTAX:SERVICES]", "[WTAX:EWT15]", "[WTAX:GIFT_CERT]"}.Contains(marker)).ToList()
        If primary.Count <> 1 OrElse markers.Any(Function(marker) Not primary.Contains(marker) AndAlso marker <> "[WTAX:VAT5]") Then Return False
        If primary(0) = "[WTAX:GIFT_CERT]" Then Return Not markers.Contains("[WTAX:VAT5]")
        Select Case primary(0)
            Case "[WTAX:GOODS]" : rates.Add(1)
            Case "[WTAX:SERVICES]" : rates.Add(2)
            Case "[WTAX:EWT15]" : rates.Add(15)
        End Select
        Return True
    End Function

    Public Shared Sub LoadNotes(dbx As ItemLookUpDataContext, lines As IList(Of WithholdingItemLine))
        Dim ids = lines.Where(Function(line) line.ItemID > 0).Select(Function(line) line.ItemID).Distinct().ToList()
        Dim notes As New Dictionary(Of Integer, String)()
        For offset As Integer = 0 To ids.Count - 1 Step 1000
            ' IDs are validated integers; one batch read, not a query per grid row.
            Dim keys = String.Join(",", ids.Skip(offset).Take(1000).Select(Function(id) id.ToString(CultureInfo.InvariantCulture)))
            For Each item In dbx.ExecuteQuery(Of WithholdingItemNotes)("SELECT ID AS ItemID, CONVERT(nvarchar(max), Notes) AS Notes FROM dbo.Item WHERE ID IN (" & keys & ")")
                notes(item.ItemID) = item.Notes
            Next
        Next
        For Each line In lines
            line.Notes = If(notes.ContainsKey(line.ItemID), notes(line.ItemID), Nothing)
        Next
    End Sub
End Class

Public Class WithholdingItemNotes
    Public Property ItemID As Integer
    Public Property Notes As String
End Class

Public Class WithholdingItemLine
    Public Property ItemID As Integer
    Public Property Notes As String
    Public Property GrossAmount As Decimal
End Class

Public Class ItemWithholdingSummary
    Public ReadOnly Bases As New Dictionary(Of Integer, Decimal) From {{1, 0D}, {2, 0D}, {5, 0D}, {15, 0D}}
    Public Property ReviewLines As Integer

    Public Function Amount(rate As Integer) As Decimal
        Return CustomerWithholdingSuggestions.Amount(Bases(rate), rate)
    End Function

    Public Function ReviewMessage() As String
        Return If(ReviewLines = 0, "", "Needs review: " & ReviewLines.ToString(CultureInfo.InvariantCulture) & " item line(s) excluded from 1%/2%/15% (unknown/conflicting markers).")
    End Function
End Class

Public Class CustomerWithholdingFields
    Public Property CustomNumber3 As Double
    Public Property CustomNumber4 As Double
    Public Property CustomNumber5 As Double
    Public Property CustomText2 As String
    Public Property Caption1 As String
    Public Property Caption2 As String
    Public Property Caption5 As String
    Public Property Caption15 As String

    Public Function CaptionFor(rate As Integer) As String
        Select Case rate
            Case 1 : Return Caption1
            Case 2 : Return Caption2
            Case 5 : Return Caption5
            Case 15 : Return Caption15
            Case Else : Return Nothing
        End Select
    End Function
End Class
