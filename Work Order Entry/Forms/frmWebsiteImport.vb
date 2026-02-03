Public Class frmWebsiteImport

    Private Sub frmWebsiteImport_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        loadImports()

    End Sub

    Private Function checkifAlreadyImported(ByVal OrderID As Integer) As Boolean

        Dim y = (From a In db.Orders Where a.Comment.Contains("SOD WEBSITE") And a.Comment.Contains(OrderID)).Count

        If y > 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Sub loadImports()

        If chkShowAll.Checked = True Then

            Dim x = (From row In clsImport.ImportDataCSV
                     Where (row("OrderID").ToString.Contains(txtSearch.Text) Or row("CustomerName").ToString.Contains(txtSearch.Text))
                   Group row By OrderID = row("OrderID") Into OrderIDGroup = Group
                   Select New With {
                       Key OrderID,
                       .CustomerName = OrderIDGroup.Max(Function(r) r("CustomerName")),
                       .DateOrder = OrderIDGroup.Max(Function(r) r("OrderPlaced"))
                  }).ToList

            gridOrders.DataSource = x

        Else

            Dim x = (From row In clsImport.ImportDataCSV
                    Where (checkifAlreadyImported(row("OrderID")) = False And
                           (row("OrderID").ToString.Contains(txtSearch.Text) Or row("CustomerName").ToString.Contains(txtSearch.Text)))
                   Group row By OrderID = row("OrderID") Into OrderIDGroup = Group
                   Select New With {
                       Key OrderID,
                       .CustomerName = OrderIDGroup.Max(Function(r) r("CustomerName")),
                       .DateOrder = OrderIDGroup.Max(Function(r) r("OrderPlaced"))
                  }).ToList

            gridOrders.DataSource = x

        End If

        gridOrders.Columns(0).HeaderText = "Order Number"
        gridOrders.Columns(1).HeaderText = "Customer Name"
        gridOrders.Columns(2).HeaderText = "Order Date Placed"

    End Sub


    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImport.Click

        Dim iRowIndex = gridOrders.CurrentCell.RowIndex

        Dim xID = gridOrders.Rows(iRowIndex).Cells(0).Value
        Me.Hide()
        frmItemLookUp.ImportFromWebsite(xID)
        Me.Close()
    End Sub


    Private Sub chkShowAll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowAll.CheckedChanged
        loadImports()
    End Sub


    Private Sub txtSearch_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            loadImports()
        End If
    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click

        Cursor.Current = Cursors.WaitCursor
        clsImport.ImportDataCSV = mysql_load_data("SELECT a.*,b.OrderPlaced,b.CustomerName,b.Shipping FROM SOD_WEBSITE_VIEW_ORDER_ITEMS a inner join SOD_WEBSITE_VIEW_ORDERS b on a.OrderID = b.OrderID Where b.Orderstatus = 'Processing' order by b.OrderID ASC;")
        loadImports()
        Cursor.Current = Cursors.Default

    End Sub

End Class