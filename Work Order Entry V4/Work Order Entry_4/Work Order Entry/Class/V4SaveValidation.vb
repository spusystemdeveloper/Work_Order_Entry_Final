Imports System.Windows.Forms

' Input checks only. Keep the legacy GSC SQL functions and save implementation.
Public NotInheritable Class V4SaveValidation
    Private Sub New()
    End Sub

    Public Shared Function CustomerSelected(id As Integer, name As String) As Boolean
        Return id > 0 AndAlso Not String.IsNullOrWhiteSpace(name)
    End Function

    Public Shared Function HeaderIssue(customerID As Integer, customerName As String,
                                       salesID As Integer, salesName As String,
                                       payment As String, entryType As Integer,
                                       existingOrder As Boolean, hasItems As Boolean) As String
        If entryType <> 2 AndAlso entryType <> 3 Then Return "EntryType"
        ' Preserve V4's optional header fields when editing an existing quotation.
        Dim requireSelection = Not existingOrder OrElse entryType = 2
        If requireSelection AndAlso Not CustomerSelected(customerID, customerName) Then Return "Customer"
        If requireSelection AndAlso Not CustomerSelected(salesID, salesName) Then Return "SalesRep"
        If entryType = 2 AndAlso String.IsNullOrWhiteSpace(payment) Then Return "Payment"
        If Not hasItems Then Return "Items"
        Return ""
    End Function

    Public Shared Function HasItems(grid As DataGridView) As Boolean
        For Each row As DataGridViewRow In grid.Rows
            If Not row.IsNewRow Then Return True
        Next
        Return False
    End Function

    Public Shared Function RowIssue(grid As DataGridView, ByRef rowIndex As Integer,
                                    ByRef columnIndex As Integer) As String
        rowIndex = -1
        columnIndex = -1
        For Each row As DataGridViewRow In grid.Rows
            If row.IsNewRow Then Continue For
            rowIndex = row.Index
            columnIndex = 0
            If String.IsNullOrWhiteSpace(Convert.ToString(row.Cells(0).Value)) Then Return "Please select a valid item."
            Try
                columnIndex = 11
                If CInt(row.Cells(11).Value) <= 0 Then Return "Please select a valid item."
                columnIndex = 2
                Dim quantity = CDbl(row.Cells(2).Value)
                If Double.IsNaN(quantity) OrElse Double.IsInfinity(quantity) OrElse quantity <= 0 Then Return "Please enter a valid quantity greater than zero."
                For Each index In New Integer() {3, 9, 12}
                    columnIndex = index
                    If index = 3 AndAlso (row.Cells(index).Value Is Nothing OrElse IsDBNull(row.Cells(index).Value)) Then Return "Please confirm the item price."
                    CheckMoney(row.Cells(index).Value)
                Next
                columnIndex = 18
                ' V4 stores prepared counts as integers; keep its existing conversion.
                Dim prepared = CInt(row.Cells(18).Value)
                If prepared > Math.Round(quantity, 0, MidpointRounding.ToEven) Then Return "Qty Prepared cannot be greater than the Ordered Qty."
            Catch ex As Exception When TypeOf ex Is InvalidCastException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException
                Return "Please correct the item's numeric values before saving."
            End Try
        Next
        rowIndex = -1
        columnIndex = -1
        Return ""
    End Function

    Public Shared Function TotalsValid(vat As String, total As String) As Boolean
        Try
            CheckMoney(vat)
            CheckMoney(total)
            Return True
        Catch ex As Exception When TypeOf ex Is InvalidCastException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException
            Return False
        End Try
    End Function

    Private Shared Sub CheckMoney(value As Object)
        ' Match V4's Double conversion followed by its LINQ-to-SQL Money parameter.
        Dim amount = CDec(CDbl(value))
        If amount < -922337203685477.5808D OrElse amount > 922337203685477.5807D Then
            Throw New OverflowException("Amount cannot be stored in a SQL Money field.")
        End If
    End Sub
End Class
