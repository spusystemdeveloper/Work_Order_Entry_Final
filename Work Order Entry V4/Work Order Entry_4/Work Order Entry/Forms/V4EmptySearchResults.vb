Partial Public Class frmCustomer
    Private Sub gridCustomer_EmptyResultsPaint(sender As Object, e As PaintEventArgs) Handles gridCustomer.Paint
        GridEmptyState.Draw(gridCustomer, e, "No customers found. Try another search.")
    End Sub
End Class

Partial Public Class frmSalesRep
    Private Sub gridSalesRep_EmptyResultsPaint(sender As Object, e As PaintEventArgs) Handles gridSalesRep.Paint
        GridEmptyState.Draw(gridSalesRep, e, "No sales representatives found. Try another search.")
    End Sub
End Class

Partial Public Class frmItemLookUp
    Private Sub gridItem_EmptyResultsPaint(sender As Object, e As PaintEventArgs) Handles gridItem.Paint
        GridEmptyState.Draw(gridItem, e, "No items found. Try another code or description.")
    End Sub
End Class
