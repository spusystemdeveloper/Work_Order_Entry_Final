Public NotInheritable Class PriceConfirmationRules

    Private Sub New()
    End Sub

    Public Shared Function IsConfirmedPrice(ByVal value As Object) As Boolean
        If value Is Nothing Then Return False

        Dim confirmedPrice As Decimal
        Return Decimal.TryParse(value.ToString(), confirmedPrice)
    End Function

End Class
