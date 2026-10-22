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
