Option Strict On

' Preserve the existing RMS type-2 reservation arithmetic; this does not save data.
Public NotInheritable Class OrderCommitmentRules
    ' Float arithmetic can leave a tiny remainder, such as 0.7 + 0.1 - 0.7 - 0.1.
    ' Differences this small are rounding, never real stock.
    Public Const QuantityTolerance As Double = 0.000000001R

    ' Applies to every item type and quotations, which have no stock check to stop a typo.
    Public Const MaximumLineQuantity As Double = 999999R
    Public Const MaximumQuantityMessage As String = "Quantity cannot be more than 999,999."

    ' Order.Total and Order.Tax are SQL money columns, which end near 922 trillion.
    Public Const MaximumOrderTotal As Double = 922337203685477R
    Public Const MaximumTotalMessage As String = "The order total is too large to save. Please check the quantities and prices."

    Private Sub New()
    End Sub

    Public Shared Function IsNegativeQuantity(quantity As Double) As Boolean
        Return quantity < -QuantityTolerance
    End Function

    Public Shared Function ExceedsMaximumQuantity(quantity As Double) As Boolean
        Return quantity > MaximumLineQuantity
    End Function

    Public Shared Function ExceedsMaximumTotal(total As Double) As Boolean
        Return total >= MaximumOrderTotal
    End Function

    Public Shared Function CalculateChange(originalType As Integer, targetType As Integer,
                                          previousQuantity As Double, newQuantity As Double,
                                          itemType As Integer) As Double
        If itemType = 7 Then Return 0R
        Dim difference As Double = 0R
        If originalType = 2 Then difference -= previousQuantity
        If targetType = 2 Then difference += newQuantity
        Return difference
    End Function
End Class
