Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Linq

Public Class WarehouseShortage
    Public Property OrderID As Integer
    Public Property QueueItemID As Long
    Public Property ItemID As Integer
    Public Property Customer As String
    Public Property ItemCode As String
    Public Property Description As String
    Public Property OrderedQuantity As Double
    Public Property FoundQuantity As Integer?
    Public Property Picker As String
End Class

Public NotInheritable Class ShortageNotifications
    Private known As New Dictionary(Of Integer, String)()
    Private initialized As Boolean

    Public Function Observe(lines As IEnumerable(Of WarehouseShortage)) As List(Of Integer)
        If lines Is Nothing Then Throw New ArgumentNullException(NameOf(lines))
        Dim current = lines.GroupBy(Function(line) line.OrderID).ToDictionary(
            Function(group) group.Key,
            Function(group) String.Join(";", group.Select(Function(line)
                                                              Return line.QueueItemID.ToString(CultureInfo.InvariantCulture) & "/" &
                    line.ItemID.ToString(CultureInfo.InvariantCulture) & "/" &
                    line.OrderedQuantity.ToString("R", CultureInfo.InvariantCulture) & "/" &
                    If(line.FoundQuantity.HasValue, line.FoundQuantity.Value.ToString(CultureInfo.InvariantCulture), "missing")
                                                          End Function).Distinct().OrderBy(Function(signature) signature)))
        Dim changed As New List(Of Integer)()
        If initialized Then
            For Each entry In current
                Dim previous As String = Nothing
                If Not known.TryGetValue(entry.Key, previous) OrElse previous <> entry.Value Then changed.Add(entry.Key)
            Next
        End If
        known = current
        initialized = True
        Return changed.OrderBy(Function(id) id).ToList()
    End Function

    Public Shared Function Load(connectionString As String) As List(Of WarehouseShortage)
        Dim result As New List(Of WarehouseShortage)()
        Using connection As New SqlConnection(connectionString), command = connection.CreateCommand()
            ' Background poll: a healthy run is sub-second, so give up early rather than
            ' tie up a busy server; the caller backs off and retries later.
            command.CommandTimeout = 10
            command.CommandText =
                "SELECT o.ID, qi.id, qi.ItemID, COALESCE(c.Company,N''), i.ItemLookupCode, i.Description, " &
                "oe.Quantity, qi.QtyPre, qi.Picker FROM dbo.QueueingItems qi " &
                "JOIN dbo.Queueing q ON q.id=qi.QueueingID JOIN dbo.[Order] o ON o.ID=q.OrderID " &
                "JOIN dbo.Item i ON i.ID=qi.ItemID LEFT JOIN dbo.Customer c ON c.ID=o.CustomerID " &
                "CROSS APPLY (SELECT SUM(e.QuantityOnOrder) AS Quantity FROM dbo.OrderEntry e " &
                "WHERE e.OrderID=o.ID AND e.ItemID=qi.ItemID) oe " &
                "WHERE qi.Status='Shortage' AND q.Status=0 AND o.Type=2 AND o.Closed=0 " &
                "AND oe.Quantity>0 AND o.Time>=@Cutoff ORDER BY o.ID,qi.id;"
            command.Parameters.Add("@Cutoff", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-60)
            connection.Open()
            Using reader = command.ExecuteReader()
                While reader.Read()
                    result.Add(New WarehouseShortage With {
                        .OrderID = Convert.ToInt32(reader.GetValue(0)),
                        .QueueItemID = Convert.ToInt64(reader.GetValue(1)),
                        .ItemID = Convert.ToInt32(reader.GetValue(2)),
                        .Customer = Convert.ToString(reader.GetValue(3)),
                        .ItemCode = Convert.ToString(reader.GetValue(4)),
                        .Description = Convert.ToString(reader.GetValue(5)),
                        .OrderedQuantity = Convert.ToDouble(reader.GetValue(6)),
                        .FoundQuantity = If(reader.IsDBNull(7), CType(Nothing, Integer?), Convert.ToInt32(reader.GetValue(7))),
                        .Picker = If(reader.IsDBNull(8), String.Empty, reader.GetString(8))})
                End While
            End Using
        End Using
        Return result
    End Function
End Class
