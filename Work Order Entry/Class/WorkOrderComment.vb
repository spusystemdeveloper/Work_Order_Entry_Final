Option Strict On

Imports System.Linq

' Work Orders store "payment; release type; remarks" in Order.Comment.
Public NotInheritable Class WorkOrderComment
    Private Const Separator As String = "; "
    Private Shared ReadOnly PaymentTypes As String() = {"CASH", "CHARGE", "CREDIT CARD", "DEBIT CARD"}

    Public ReadOnly Property Payment As String
    Public ReadOnly Property ReleaseType As String
    Public ReadOnly Property Remarks As String

    Private Sub New(payment As String, releaseType As String, remarks As String)
        Me.Payment = payment
        Me.ReleaseType = releaseType
        Me.Remarks = remarks
    End Sub

    ' Remarks may contain the separator, so only the first two are split. Imported
    ' orders store "CHARGE; reference" with no release type. Any other text without
    ' all three parts is kept as remarks rather than discarded.
    Public Shared Function Parse(comment As String) As WorkOrderComment
        Dim text As String = If(comment, String.Empty)
        Dim parts() As String = text.Split(New String() {Separator}, 3, StringSplitOptions.None)
        If parts.Length = 3 Then Return New WorkOrderComment(parts(0), parts(1), parts(2))
        If parts.Length = 2 AndAlso IsPaymentType(parts(0)) Then
            Return New WorkOrderComment(parts(0), String.Empty, parts(1))
        End If
        Return New WorkOrderComment(String.Empty, String.Empty, text)
    End Function

    Private Shared Function IsPaymentType(value As String) As Boolean
        Return PaymentTypes.Contains(value, StringComparer.OrdinalIgnoreCase)
    End Function
End Class
