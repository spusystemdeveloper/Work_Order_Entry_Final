Public Class frmOrderSaved
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(orderNumbers As IEnumerable(Of Integer), updated As Boolean, maxItems As Integer)
        Me.New()
        Dim numbers = orderNumbers.ToList()
        txtOrderNumbers.Text = String.Join(", ", numbers)
        lblStatus.Text = If(updated, "Work Order updated successfully", "Work Order saved successfully")
        If numbers.Count > 1 Then
            lblStatus.Text = numbers.Count & " Work Orders saved successfully"
            txtOrderNumbers.Font = New Drawing.Font("Tahoma", 24.0!, Drawing.FontStyle.Bold)
            lblDetails.Text = "Max " & maxItems & " items per Work Order"
        End If
        ActiveControl = btnClose
    End Sub
End Class
