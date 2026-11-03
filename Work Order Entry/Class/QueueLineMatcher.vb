Option Strict On

Imports System.Collections.Generic
Imports System.Linq

Public NotInheritable Class QueueLineMatcher
    Private Sub New()
    End Sub

    ' QueueingItems has no OrderEntry link. The insert procedure writes one queue
    ' row per line, so for each item the lines and rows pair up in ID order.
    ' Lines beyond the available rows are absent from the result.
    Public Shared Function Match(entries As IEnumerable(Of KeyValuePair(Of Integer, Integer)),
                                 queueRows As IEnumerable(Of KeyValuePair(Of Long, Integer))) As Dictionary(Of Integer, Long)
        Dim rowsByItem = queueRows.OrderBy(Function(row) row.Key).
            GroupBy(Function(row) row.Value).
            ToDictionary(Function(group) group.Key,
                         Function(group) New Queue(Of Long)(group.Select(Function(row) row.Key)))

        Dim result As New Dictionary(Of Integer, Long)
        For Each entry In entries.OrderBy(Function(line) line.Key)
            Dim rows As Queue(Of Long) = Nothing
            If rowsByItem.TryGetValue(entry.Value, rows) AndAlso rows.Count > 0 Then
                result(entry.Key) = rows.Dequeue()
            End If
        Next
        Return result
    End Function
End Class
