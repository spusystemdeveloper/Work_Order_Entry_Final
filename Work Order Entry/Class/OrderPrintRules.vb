Public NotInheritable Class OrderPrintRules

    Private Sub New()
    End Sub

    Public Const WorkOrderTemplate As String = "Work Order"
    Public Const SalesQuotationTemplate As String = "Sales Quotation"

    Public Shared Function GetTemplateName(ByVal orderType As Integer) As String
        Select Case orderType
            Case 2
                Return WorkOrderTemplate
            Case 3
                Return SalesQuotationTemplate
            Case Else
                Return String.Empty
        End Select
    End Function

    Public Shared Function ResolveTemplateName(ByVal templateName As String) As String
        If String.IsNullOrWhiteSpace(templateName) Then
            Return WorkOrderTemplate
        End If
        Return templateName
    End Function

End Class
