Option Strict On

Imports System.Linq

Public Class frmShortageAlerts
    Inherits Form

    Private ReadOnly shortagesGrid As New DataGridView With {
        .Name = "ShortagesGrid", .Dock = DockStyle.Fill, .ReadOnly = True,
        .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
        .AutoGenerateColumns = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False,
        .RowHeadersVisible = False}
    Private ReadOnly search As New TextBox With {.Name = "ShortageSearch", .Width = 230}
    Private ReadOnly pickerFilter As New ComboBox With {
        .Name = "ShortagePicker", .Width = 180, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly newOnly As New CheckBox With {.Name = "NewShortagesOnly", .Text = "New / changed only", .AutoSize = True}
    Private ReadOnly zeroOnly As New CheckBox With {.Name = "ZeroFoundOnly", .Text = "Zero found only", .AutoSize = True}
    Private ReadOnly resultCount As New Label With {.Name = "ShortageResultCount", .AutoSize = True, .Padding = New Padding(8, 5, 0, 0)}
    Private ReadOnly openButton As New Button With {.Name = "OpenShortageOrder", .Text = "Open selected order", .AutoSize = True}
    Private ReadOnly list As ShortageAlertList
    Private acceptedOrder As Integer
    Private changingFilters As Boolean

    Public ReadOnly Property SelectedOrderID As Integer
        Get
            If acceptedOrder > 0 Then Return acceptedOrder
            If shortagesGrid.CurrentRow Is Nothing Then Return 0
            Return Convert.ToInt32(shortagesGrid.CurrentRow.Cells("Work Order").Value)
        End Get
    End Property

    Public Sub New(lines As IEnumerable(Of WarehouseShortage), pendingOrders As IEnumerable(Of Integer))
        list = New ShortageAlertList(lines, pendingOrders)
        Text = "Warehouse shortage alerts"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1100, 550)
        MinimumSize = New Size(850, 430)
        Font = New Font("Segoe UI", 9)
        ConfigureGrid()
        pickerFilter.Items.Add("All pickers")
        For Each picker In list.Pickers
            pickerFilter.Items.Add(If(String.IsNullOrEmpty(picker), "(Unassigned)", picker))
        Next
        pickerFilter.SelectedIndex = 0
        shortagesGrid.DataSource = list.View

        Dim header As New TableLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 140, .ColumnCount = 1, .RowCount = 3, .Padding = New Padding(8)}
        header.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        header.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        header.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        header.Controls.Add(New Label With {.Dock = DockStyle.Fill, .Text =
            "Warehouse physical counts for open Work Orders across this store. Select a line and open its order." & vbCrLf &
            "Opening checks the latest order. Changes on your current screen require confirmation before discarding."}, 0, 0)
        Dim searchRow As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False}
        searchRow.Controls.Add(New Label With {.Text = "Search", .AutoSize = True, .Padding = New Padding(0, 5, 4, 0)})
        searchRow.Controls.Add(search)
        searchRow.Controls.Add(New Label With {.Text = "Picker", .AutoSize = True, .Padding = New Padding(12, 5, 4, 0)})
        searchRow.Controls.Add(pickerFilter)
        Dim clearButton As New Button With {.Name = "ClearShortageFilters", .Text = "Clear filters", .AutoSize = True}
        searchRow.Controls.Add(clearButton)
        header.Controls.Add(searchRow, 0, 1)
        Dim options As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False}
        options.Controls.Add(newOnly)
        options.Controls.Add(zeroOnly)
        options.Controls.Add(resultCount)
        header.Controls.Add(options, 0, 2)
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 48,
            .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)}
        Dim closeButton As New Button With {.Text = "Close", .AutoSize = True, .DialogResult = DialogResult.Cancel}
        footer.Controls.Add(closeButton)
        footer.Controls.Add(openButton)
        Controls.Add(shortagesGrid)
        Controls.Add(header)
        Controls.Add(footer)
        AcceptButton = openButton
        CancelButton = closeButton
        AddHandler search.TextChanged, Sub(sender, e) ApplyFilters()
        AddHandler pickerFilter.SelectedIndexChanged, Sub(sender, e) ApplyFilters()
        AddHandler newOnly.CheckedChanged, Sub(sender, e) ApplyFilters()
        AddHandler zeroOnly.CheckedChanged, Sub(sender, e) ApplyFilters()
        AddHandler clearButton.Click, Sub(sender, e) ClearFilters()
        AddHandler openButton.Click, Sub(sender, e) AcceptSelection()
        AddHandler shortagesGrid.CellDoubleClick, Sub(sender, e)
                                                     If e.RowIndex >= 0 Then AcceptSelection()
                                                 End Sub
        AddHandler Shown, Sub(sender, e) ApplyFilters()
        openButton.Enabled = list.TotalLines > 0
        UpdateResultCount()
    End Sub

    Private Sub ConfigureGrid()
        Dim captions = {"Alert", "Work Order", "Customer", "Item code", "Description", "Ordered", "Found", "Picker"}
        Dim weights = {80, 75, 150, 100, 230, 65, 80, 100}
        For index = 0 To captions.Length - 1
            shortagesGrid.Columns.Add(New DataGridViewTextBoxColumn With {
                .Name = captions(index), .HeaderText = captions(index), .DataPropertyName = captions(index),
                .FillWeight = weights(index), .SortMode = DataGridViewColumnSortMode.Automatic})
        Next
        shortagesGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True
    End Sub

    Private Sub ApplyFilters()
        If changingFilters Then Return
        Dim previousOrder = SelectedOrderID
        Dim selectedPicker As String = Nothing
        If pickerFilter.SelectedIndex > 0 Then selectedPicker = list.Pickers(pickerFilter.SelectedIndex - 1)
        list.ApplyFilter(search.Text, newOnly.Checked, zeroOnly.Checked, selectedPicker)
        openButton.Enabled = list.View.Count > 0
        If shortagesGrid.Rows.Count > 0 Then
            Dim selectedRow = shortagesGrid.Rows.Cast(Of DataGridViewRow)().
                FirstOrDefault(Function(row) Convert.ToInt32(row.Cells("Work Order").Value) = previousOrder)
            shortagesGrid.CurrentCell = If(selectedRow, shortagesGrid.Rows(0)).Cells("Work Order")
        End If
        UpdateResultCount()
    End Sub

    Private Sub UpdateResultCount()
        resultCount.Text = list.View.Count & " of " & list.TotalLines & " shortage lines"
    End Sub

    Private Sub ClearFilters()
        changingFilters = True
        Try
            search.Clear()
            newOnly.Checked = False
            zeroOnly.Checked = False
            pickerFilter.SelectedIndex = 0
        Finally
            changingFilters = False
        End Try
        ApplyFilters()
    End Sub

    Private Sub AcceptSelection()
        If SelectedOrderID <= 0 OrElse list.View.Count = 0 Then Return
        acceptedOrder = SelectedOrderID
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
