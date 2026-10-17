Public Class frmOrderSaved
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(orderNumbers As IEnumerable(Of Integer), updated As Boolean, maxItems As Integer,
                   Optional salesQuotation As Boolean = False)
        Me.New()
        Dim numbers = orderNumbers.ToList()
        Dim entryName As String = If(salesQuotation, "Sales Quotation", "Work Order")
        If salesQuotation Then Text = "Sales Quotation Saved"
        txtOrderNumbers.Text = String.Join(", ", numbers)
        lblStatus.Text = entryName & If(updated, " updated successfully", " saved successfully")
        If numbers.Count > 1 Then
            lblStatus.Text = numbers.Count & " " & entryName & "s saved successfully"
            txtOrderNumbers.Font = New Drawing.Font("Tahoma", 24.0!, Drawing.FontStyle.Bold)
            lblDetails.Text = "Max " & maxItems & " items per " & entryName
        End If
        ActiveControl = btnClose
    End Sub
End Class
