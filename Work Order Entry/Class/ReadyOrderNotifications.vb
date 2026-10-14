Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Linq

Public NotInheritable Class ReadyOrderNotifications
    Private known As New HashSet(Of Integer)()
    Private initialized As Boolean

    ' The first successful poll establishes a baseline. Subsequent transitions
    ' notify once; an order that leaves and re-enters Prepared can notify again.
    Public Function Observe(groups As IEnumerable(Of Integer)) As List(Of Integer)
        If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))
        Dim current As New HashSet(Of Integer)(groups)
        Dim added = If(initialized, current.Except(known).OrderBy(Function(id) id).ToList(),
                       New List(Of Integer)())
        known = current
        initialized = True
        Return added
    End Function

    Public Shared Function LoadPreparedGroups(connectionString As String) As List(Of Integer)
        Dim groups As New List(Of Integer)()
        Using connection As New SqlConnection(connectionString), command = connection.CreateCommand()
            command.CommandTimeout = 30
            ' Keep eligibility aligned with clsItemLookUp.getOrderForInvoicing.
            command.CommandText =
                "SELECT DISTINCT v.GroupTo FROM dbo.SOD_ViewForInvoice v " &
                "JOIN dbo.[Order] o ON v.GroupTo = o.ID " &
                "WHERE v.Status = 'Prepared' AND o.Closed = 0 " &
                "AND o.Time >= @Cutoff ORDER BY v.GroupTo;"
            command.Parameters.Add("@Cutoff", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-60)
            connection.Open()
            Using reader = command.ExecuteReader()
                While reader.Read()
                    groups.Add(Convert.ToInt32(reader.GetValue(0)))
                End While
            End Using
        End Using
        Return groups
    End Function
End Class
