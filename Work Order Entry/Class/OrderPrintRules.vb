Public NotInheritable Class OrderPrintRules

    Private Sub New()
    End Sub

    Public Shared Function GetTemplateName(ByVal orderType As Integer) As String
        Select Case orderType
            Case 2
                Return "Work Order"
            Case 3
                Return "Sales Quotation"
            Case Else
                Return String.Empty
        End Select
    End Function

End Class
