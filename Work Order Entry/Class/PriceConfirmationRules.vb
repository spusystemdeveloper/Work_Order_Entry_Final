Public NotInheritable Class PriceConfirmationRules

    Private Sub New()
    End Sub

    Public Shared Function IsConfirmedPrice(ByVal value As Object) As Boolean
        If value Is Nothing Then Return False

        Dim confirmedPrice As Decimal
        Return Decimal.TryParse(value.ToString(), confirmedPrice)
    End Function

    Public Shared Function CountUnconfirmedPrices(
        ByVal prices As IEnumerable(Of Object)) As Integer

        Return prices.Count(Function(value) Not IsConfirmedPrice(value))
    End Function

    Public Shared Function FindFirstUnconfirmedPriceIndex(
        ByVal prices As IEnumerable(Of Object)) As Integer

        Dim index As Integer = 0

        For Each value In prices
            If Not IsConfirmedPrice(value) Then Return index
            index += 1
        Next

        Return -1
    End Function

End Class
