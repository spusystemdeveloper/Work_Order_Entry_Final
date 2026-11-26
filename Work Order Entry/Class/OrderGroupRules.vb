' Rules for the customer order grouping screen.
Public NotInheritable Class OrderGroupRules

    Private Sub New()
    End Sub

    ' The grid shows a group as "101 / 102 / 103". Ungrouping is only allowed for
    ' an order typed exactly as one of those numbers, so a mistyped number cannot
    ' change a different order's queue group.
    Public Shared Function TryFindOrderInGroup(ByVal groupText As String,
                                               ByVal typedOrder As String,
                                               ByRef orderID As Integer) As Boolean
        orderID = 0
        If String.IsNullOrWhiteSpace(groupText) OrElse String.IsNullOrWhiteSpace(typedOrder) Then Return False

        Dim typed As Integer
        If Not Integer.TryParse(typedOrder.Trim(), Globalization.NumberStyles.None,
                                Globalization.CultureInfo.InvariantCulture, typed) Then Return False

        For Each part In groupText.Split("/"c)
            Dim member As Integer
            If Integer.TryParse(part.Trim(), Globalization.NumberStyles.None,
                                Globalization.CultureInfo.InvariantCulture, member) AndAlso member = typed Then
                orderID = typed
                Return True
            End If
        Next

        Return False
    End Function

    ' A row lists a group only when it holds more than one order.
    Public Shared Function IsGroup(ByVal groupText As String) As Boolean
        Return groupText IsNot Nothing AndAlso groupText.Contains("/")
    End Function

End Class
