Option Strict On

Imports System.Data
Imports System.Data.SqlClient

Public NotInheritable Class OrderHistory
    Private Sub New()
    End Sub

    Public Shared Sub SetActor(context As ItemLookUpDataContext, username As String, register As String)
        ' SQL Server 2016 session context is read by the history triggers.
        ' Values are overwritten for each write connection; no credentials are logged.
        context.ExecuteCommand(
            "EXEC sys.sp_set_session_context @key=N'SOD_WO_Actor', @value={0}; " &
            "EXEC sys.sp_set_session_context @key=N'SOD_WO_Register', @value={1};",
            If(username, String.Empty), If(register, String.Empty))
    End Sub

    Public Shared Function Load(connectionString As String, orderID As Integer) As DataTable
        Using connection As New SqlConnection(connectionString), command = connection.CreateCommand()
            connection.Open()
            command.CommandTimeout = 10
            command.CommandText = "SELECT CASE WHEN OBJECT_ID(N'dbo.SOD_WO_History', N'U') IS NULL THEN 0 ELSE 1 END;"
            If Convert.ToInt32(command.ExecuteScalar()) = 0 Then
                Throw New InvalidOperationException("Order history is not enabled for this branch. Contact your administrator.")
            End If
            command.CommandText =
                "SELECT EventTime AS [Date / time], EventName AS [Event], Actor AS [User], " &
                "Terminal AS [Terminal], ItemID AS [Item ID], Details AS [Details] " &
                "FROM dbo.SOD_WO_History WHERE OrderID = @OrderID ORDER BY EventID DESC;"
            command.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderID
            Dim result As New DataTable()
            Using adapter As New SqlDataAdapter(command)
                adapter.Fill(result)
            End Using
            Return result
        End Using
    End Function
End Class
