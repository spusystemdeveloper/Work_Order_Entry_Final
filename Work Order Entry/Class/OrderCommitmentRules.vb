Option Strict On

' Preserve the existing RMS type-2 reservation arithmetic; this does not save data.
Public NotInheritable Class OrderCommitmentRules
    Private Sub New()
    End Sub

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
