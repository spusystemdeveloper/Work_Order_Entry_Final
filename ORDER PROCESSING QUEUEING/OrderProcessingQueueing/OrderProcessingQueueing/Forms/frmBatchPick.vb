Imports System.Drawing.Printing

Public Class frmBatchPick

    Private _mainForm As frmMain
    Private _allItems As New List(Of BatchPickItem)()

    Public Class BatchPickItem
        Public Property ItemCode As String = ""
        Public Property Description As String = ""
        Public Property TotalQtyToPick As Integer = 0
        Public Property OrderCount As Integer = 0
        Public Property WorkOrders As String = ""
        Public Property PickLocation As String = ""
    End Class

    Public Sub New(ByVal mainForm As frmMain)
        InitializeComponent()
        _mainForm = mainForm
    End Sub

    Public Sub New()
        InitializeComponent()
        _mainForm = GetMainForm()
    End Sub

    Private Function GetMainForm() As frmMain
        If _mainForm IsNot Nothing AndAlso Not _mainForm.IsDisposed Then
            Return _mainForm
        End If
        For Each f As Form In Application.OpenForms
            If TypeOf f Is frmMain Then
                _mainForm = DirectCast(f, frmMain)
                Return _mainForm
            End If
        Next
        Return Nothing
    End Function

    Private Sub frmBatchPick_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        GetMainForm()
        cmbLocation.SelectedIndex = 0
        LoadBatchItems()
    End Sub

    Public Sub LoadBatchItems()
        Try
            _allItems.Clear()
            Dim main = GetMainForm()
            If main Is Nothing Then Exit Sub

            ' Key: ItemCode + "|" + PickLoc
            Dim itemDict As New Dictionary(Of String, BatchPickItem)(StringComparer.OrdinalIgnoreCase)
            Dim orderSetDict As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)

            For Each ctrl As Control In main.flowPnl_Orders.Controls
                Dim grid = TryCast(ctrl, UCGridOrder)
                If grid IsNot Nothing Then
                    ' Only aggregate active orders (not For Invoicing / Prepared)
                    Dim bg = grid.BackColor
                    If bg = Color.PaleGreen OrElse bg = Color.CornflowerBlue Then Continue For

                    Dim orderID As String = If(grid.lblWO IsNot Nothing, grid.lblWO.Text, grid.Name)
                    Dim defaultLoc As String = If(Not String.IsNullOrEmpty(main.pType), main.pType, "STORE")

                    For i As Integer = 0 To grid.gridOrder.Rows.Count - 1
                        Dim row = grid.gridOrder.Rows(i)
                        Dim itemCode As String = If(row.Cells(1).Value, "").ToString().Trim()
                        Dim desc As String = If(row.Cells(2).Value, "").ToString().Trim()
                        Dim status As String = If(row.Cells(6).Value, "").ToString().Trim()

                        ' Skip footer and prepared items
                        If itemCode.Equals("Set all as Prepared", StringComparison.OrdinalIgnoreCase) OrElse
                           desc.Equals("Set all as Prepared", StringComparison.OrdinalIgnoreCase) OrElse
                           status.Equals("Prepared", StringComparison.OrdinalIgnoreCase) OrElse
                           status.Equals("For Invoicing", StringComparison.OrdinalIgnoreCase) Then
                            Continue For
                        End If

                        Dim qtyOrd As Integer = 0
                        Dim qtyPrep As Integer = 0
                        Integer.TryParse(If(row.Cells(3).Value, "0").ToString(), qtyOrd)
                        Integer.TryParse(If(row.Cells(4).Value, "0").ToString(), qtyPrep)

                        Dim qtyRemaining As Integer = Math.Max(0, qtyOrd - qtyPrep)
                        If qtyRemaining <= 0 Then Continue For

                        Dim loc As String = defaultLoc
                        Dim key As String = itemCode & "|" & loc

                        If Not itemDict.ContainsKey(key) Then
                            itemDict(key) = New BatchPickItem With {
                                .ItemCode = itemCode,
                                .Description = desc,
                                .PickLocation = loc,
                                .TotalQtyToPick = 0
                            }
                            orderSetDict(key) = New HashSet(Of String)()
                        End If

                        itemDict(key).TotalQtyToPick += qtyRemaining
                        orderSetDict(key).Add(orderID)
                    Next
                End If
            Next

            For Each kvp In itemDict
                Dim orders = orderSetDict(kvp.Key).ToList()
                kvp.Value.OrderCount = orders.Count
                kvp.Value.WorkOrders = String.Join(", ", orders)
                _allItems.Add(kvp.Value)
            Next

            _allItems = _allItems.OrderBy(Function(x) x.PickLocation) _
                                 .ThenByDescending(Function(x) x.TotalQtyToPick) _
                                 .ToList()

            ApplyGridFilter()

        Catch ex As Exception
            MessageBox.Show("Error loading batch pick items: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ApplyGridFilter()
        Try
            Dim filterLoc As String = If(cmbLocation.SelectedItem, "All Locations").ToString()
            Dim searchText As String = txtSearch.Text.Trim()

            Dim filtered = _allItems.AsEnumerable()

            If Not filterLoc.Equals("All Locations", StringComparison.OrdinalIgnoreCase) Then
                filtered = filtered.Where(Function(x) x.PickLocation.Equals(filterLoc, StringComparison.OrdinalIgnoreCase))
            End If

            If Not String.IsNullOrEmpty(searchText) Then
                filtered = filtered.Where(Function(x) x.ItemCode.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                                      x.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                                      x.WorkOrders.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            End If

            gridBatchPick.Rows.Clear()
            Dim totalUnits As Integer = 0

            For Each it In filtered
                Dim rowIdx As Integer = gridBatchPick.Rows.Add()
                Dim row As DataGridViewRow = gridBatchPick.Rows(rowIdx)
                row.Cells(0).Value = it.ItemCode
                row.Cells(1).Value = it.Description
                row.Cells(2).Value = it.TotalQtyToPick
                row.Cells(3).Value = it.OrderCount
                row.Cells(4).Value = it.WorkOrders
                row.Cells(5).Value = it.PickLocation
                totalUnits += it.TotalQtyToPick
            Next

            lblSummary.Text = String.Format("Items to Pick: {0} distinct SKUs | Total Units: {1} | Active Orders: {2}",
                                            gridBatchPick.Rows.Count, totalUnits, _allItems.SelectMany(Function(x) x.WorkOrders.Split(","c)).Distinct().Count())

        Catch ex As Exception
            ' Silent guard
        End Try
    End Sub

    Private Sub cmbLocation_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbLocation.SelectedIndexChanged
        ApplyGridFilter()
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtSearch.TextChanged
        ApplyGridFilter()
    End Sub

    Private Sub btnRefresh_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRefresh.Click
        LoadBatchItems()
    End Sub

    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    ' =========================================================================
    ' BATCH PICK SHEET PRINTING
    ' =========================================================================

    Private _printFontRegular As New Font("Century Gothic", 9.0!, FontStyle.Regular)
    Private _printFontBold As New Font("Century Gothic", 9.0!, FontStyle.Bold)
    Private _printFontTitle As New Font("Century Gothic", 12.0!, FontStyle.Bold)
    Private _printRowIndex As Integer = 0

    Private Sub btnPrint_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrint.Click
        Try
            If gridBatchPick.Rows.Count = 0 Then
                MessageBox.Show("No items to print in current view.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            _printRowIndex = 0
            Dim printDoc As New PrintDocument()
            AddHandler printDoc.PrintPage, AddressOf PrintBatchPage

            Dim previewDlg As New PrintPreviewDialog()
            previewDlg.Document = printDoc
            previewDlg.WindowState = FormWindowState.Maximized
            previewDlg.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show("Print error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PrintBatchPage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        Dim g = e.Graphics
        Dim left = e.MarginBounds.Left
        Dim top = e.MarginBounds.Top
        Dim width = e.MarginBounds.Width
        Dim y = top

        ' Header
        g.DrawString("CONSOLIDATED BATCH / WAVE PICK SHEET", _printFontTitle, Brushes.Black, left, y)
        y += 24
        g.DrawString("Printed: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm") & " | Location: " & cmbLocation.SelectedItem.ToString(), _printFontRegular, Brushes.DimGray, left, y)
        y += 28

        ' Column headers
        g.FillRectangle(Brushes.LightGray, left, y, width, 22)
        g.DrawRectangle(Pens.Gray, left, y, width, 22)
        g.DrawString("ITEM CODE", _printFontBold, Brushes.Black, left + 5, y + 3)
        g.DrawString("DESCRIPTION", _printFontBold, Brushes.Black, left + 120, y + 3)
        g.DrawString("QTY", _printFontBold, Brushes.Black, left + width - 200, y + 3)
        g.DrawString("LOC", _printFontBold, Brushes.Black, left + width - 140, y + 3)
        g.DrawString("CHECK", _printFontBold, Brushes.Black, left + width - 70, y + 3)
        y += 24

        While _printRowIndex < gridBatchPick.Rows.Count
            Dim row = gridBatchPick.Rows(_printRowIndex)
            Dim code = If(row.Cells(0).Value, "").ToString()
            Dim desc = If(row.Cells(1).Value, "").ToString()
            Dim qty = If(row.Cells(2).Value, "").ToString()
            Dim loc = If(row.Cells(5).Value, "").ToString()

            If desc.Length > 40 Then desc = desc.Substring(0, 37) & "..."

            g.DrawString(code, _printFontRegular, Brushes.Black, left + 5, y + 3)
            g.DrawString(desc, _printFontRegular, Brushes.Black, left + 120, y + 3)
            g.DrawString(qty, _printFontBold, Brushes.Black, left + width - 200, y + 3)
            g.DrawString(loc, _printFontRegular, Brushes.Black, left + width - 140, y + 3)
            g.DrawRectangle(Pens.Black, left + width - 60, y + 2, 14, 14) ' Checkbox square

            y += 22
            _printRowIndex += 1

            If y > e.MarginBounds.Bottom - 40 AndAlso _printRowIndex < gridBatchPick.Rows.Count Then
                e.HasMorePages = True
                Return
            End If
        End While

        e.HasMorePages = False
    End Sub

End Class
