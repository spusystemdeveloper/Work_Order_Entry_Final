Public NotInheritable Class OrderPriceApprovalRules
    Private Sub New()
    End Sub

    Public Shared Function RequiresOverride(currentPrice As Decimal, minimumPrice As Decimal,
                                            recallType As Integer, savedPrice As Decimal?) As Boolean
        Select Case recallType
            Case 0
                Return currentPrice < minimumPrice
            Case 1
                Return currentPrice < minimumPrice AndAlso
                       (Not savedPrice.HasValue OrElse currentPrice <> savedPrice.Value)
            Case Else
                Return False
        End Select
    End Function
End Class

Public Enum OrderSaveOutcome
    Saved
    Cancelled
    ValidationFailed
    Conflict
    Failed
End Enum
