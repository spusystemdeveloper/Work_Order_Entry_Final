Option Strict On

Imports System.Threading.Tasks

Public Class frmOrderHistory
    Inherits Form

    Private ReadOnly orderNumber As New NumericUpDown With {.Minimum = 1, .Maximum = Integer.MaxValue, .Width = 125}
    Private ReadOnly eventsGrid As New DataGridView With {
        .Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False,
        .AllowUserToDeleteRows = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .RowHeadersVisible = False,
        .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells}
    Private ReadOnly message As New Label With {.Dock = DockStyle.Fill, .Padding = New Padding(8)}
    Private ReadOnly loadButton As New Button With {.Text = "Load history", .AutoSize = True}
    Private ReadOnly databaseConnectionString As String
    Private loading As Boolean

    Public Sub New(initialOrderID As Integer, Optional connectionString As String = Nothing)
        databaseConnectionString = connectionString
        Text = "Order history"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1000, 580)
        MinimumSize = New Size(850, 400)
        Font = New Font("Segoe UI", 9)
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 92}
        Dim toolbar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 50, .Padding = New Padding(8)}
        Dim closeButton As New Button With {.Text = "Close", .AutoSize = True}
        toolbar.Controls.Add(New Label With {.Text = "Order number", .AutoSize = True, .Padding = New Padding(0, 6, 4, 0)})
        toolbar.Controls.Add(orderNumber)
        toolbar.Controls.Add(loadButton)
        toolbar.Controls.Add(closeButton)
        header.Controls.Add(message)
        header.Controls.Add(toolbar)
        Controls.Add(eventsGrid)
        Controls.Add(header)
        AddHandler loadButton.Click, Sub(sender, e) LoadHistory()
        AddHandler closeButton.Click, Sub(sender, e) Close()
        AcceptButton = loadButton
        CancelButton = closeButton
        If initialOrderID > 0 Then orderNumber.Value = initialOrderID
        AddHandler Shown, Sub(sender, e)
                              If initialOrderID > 0 Then LoadHistory()
                          End Sub
        message.Text = "History starts when enabled for this branch. Enter any order number, including a closed order."
    End Sub

    Private Async Sub LoadHistory()
        If loading Then Return
        loading = True
        loadButton.Enabled = False
        message.Text = "Loading history..."
        Try
            Dim orderID = Decimal.ToInt32(orderNumber.Value)
            Dim connectionString = If(databaseConnectionString, DB_Conn("constr"))
            Dim result = Await Task.Run(Function() OrderHistory.Load(connectionString, orderID))
            If IsDisposed OrElse Disposing Then Return
            eventsGrid.DataSource = result
            eventsGrid.Columns("Date / time").FillWeight = 120
            eventsGrid.Columns("Event").FillWeight = 140
            eventsGrid.Columns("User").FillWeight = 110
            eventsGrid.Columns("Terminal").FillWeight = 120
            eventsGrid.Columns("Item ID").FillWeight = 50
            eventsGrid.Columns("Details").FillWeight = 380
            eventsGrid.Columns("Event").DefaultCellStyle.WrapMode = DataGridViewTriState.True
            eventsGrid.Columns("User").DefaultCellStyle.WrapMode = DataGridViewTriState.True
            eventsGrid.Columns("Terminal").DefaultCellStyle.WrapMode = DataGridViewTriState.True
            eventsGrid.Columns("Details").DefaultCellStyle.WrapMode = DataGridViewTriState.True
            eventsGrid.Columns("Date / time").DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss"
            message.Text = If(result.Rows.Count = 0, "No recorded history for this order. History starts when enabled for this branch.",
                              result.Rows.Count.ToString() & " recorded events. Times use the database server's local clock.")
        Catch ex As Exception
            If IsDisposed OrElse Disposing Then Return
            eventsGrid.DataSource = Nothing
            message.Text = "History could not be loaded."
            AppPrompt.Show(Me, ex.Message, "Order history", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            loading = False
            If Not IsDisposed AndAlso Not Disposing Then loadButton.Enabled = True
        End Try
    End Sub
End Class
