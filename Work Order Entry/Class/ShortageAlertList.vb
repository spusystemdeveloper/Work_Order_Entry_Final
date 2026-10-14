Option Strict On

Imports System.Data
Imports System.Globalization
Imports System.Linq
Imports System.Text

' A local, read-only view of the poll result. Filtering never contacts SQL Server.
Public NotInheritable Class ShortageAlertList
    Public ReadOnly Property View As DataView
    Public ReadOnly Property Pickers As List(Of String)
    Public ReadOnly Property TotalLines As Integer

    Public Sub New(lines As IEnumerable(Of WarehouseShortage), pendingOrders As IEnumerable(Of Integer))
        Dim pending As New HashSet(Of Integer)(pendingOrders)
        Dim snapshot = lines.ToList()
        Dim table As New DataTable()
        table.Columns.Add("Alert", GetType(String))
        table.Columns.Add("Work Order", GetType(Integer))
        For Each caption In {"Customer", "Item code", "Description", "Ordered", "Found", "Picker"}
            table.Columns.Add(caption, GetType(String))
        Next
        table.Columns.Add("IsNew", GetType(Boolean))
        table.Columns.Add("FoundCount", GetType(Integer))
        table.Columns.Add("QueueItemID", GetType(Long))
        table.Columns.Add("SearchText", GetType(String))
        For Each line In snapshot
            Dim isNew = pending.Contains(line.OrderID)
            Dim picker = If(line.Picker, String.Empty)
            table.Rows.Add(If(isNew, "New / changed", "Current"), line.OrderID,
                line.Customer, line.ItemCode, line.Description, line.OrderedQuantity.ToString("G"),
                If(line.FoundQuantity.HasValue, line.FoundQuantity.Value.ToString(), "Not recorded"), picker,
                isNew, If(line.FoundQuantity.HasValue, CObj(line.FoundQuantity.Value), DBNull.Value), line.QueueItemID,
                String.Join(" ", {line.OrderID.ToString(CultureInfo.InvariantCulture), line.Customer,
                    line.ItemCode, line.Description, picker}))
        Next
        View = New DataView(table) With {.Sort = "IsNew DESC, [Work Order] ASC, QueueItemID ASC"}
        Pickers = snapshot.Select(Function(line) If(line.Picker, String.Empty)).Distinct().
            OrderBy(Function(picker) picker, StringComparer.CurrentCultureIgnoreCase).ToList()
        TotalLines = snapshot.Count
    End Sub

    Public Sub ApplyFilter(search As String, newOnly As Boolean, zeroOnly As Boolean, picker As String)
        Dim conditions As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(search) Then
            conditions.Add("SearchText LIKE '%" & EscapeSearch(search.Trim()) & "%'")
        End If
        If newOnly Then conditions.Add("IsNew = true")
        If zeroOnly Then conditions.Add("FoundCount = 0")
        If picker IsNot Nothing Then conditions.Add("Picker = '" & picker.Replace("'", "''") & "'")
        View.RowFilter = String.Join(" AND ", conditions)
    End Sub

    Private Shared Function EscapeSearch(value As String) As String
        Dim escaped As New StringBuilder()
        For Each character In value
            Select Case character
                Case "["c : escaped.Append("[[]")
                Case "]"c : escaped.Append("[]]")
                Case "*"c : escaped.Append("[*]")
                Case "%"c : escaped.Append("[%]")
                Case "'"c : escaped.Append("''")
                Case Else : escaped.Append(character)
            End Select
        Next
        Return escaped.ToString()
    End Function
End Class
