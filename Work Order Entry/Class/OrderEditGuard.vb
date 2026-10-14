Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class OrderEditConflictException
    Inherits InvalidOperationException

    Public Sub New()
        MyBase.New("This order has changed since it was recalled, or is already closed. " &
                   "Your changes have not been saved. Use Order actions > Reload order " &
                   "to load the latest version, then review your changes again.")
    End Sub
End Class

Public NotInheritable Class OrderEditSnapshot
    Public ReadOnly Property OrderID As Integer
    Public ReadOnly Property DatabaseIdentity As String
    Public ReadOnly Property Fingerprint As String

    Public Sub New(orderID As Integer, databaseIdentity As String, fingerprint As String)
        Me.OrderID = orderID
        Me.DatabaseIdentity = databaseIdentity
        Me.Fingerprint = fingerprint
    End Sub
End Class

Public NotInheritable Class OrderEditGuard
    Private Sub New()
    End Sub

    ' Include entries and warehouse state: an entry or picker change does not
    ' necessarily advance the RMS order header's rowversion or LastUpdated.
    Public Shared Function Capture(context As ItemLookUpDataContext, orderID As Integer,
                                   Optional lockForWrite As Boolean = False) As OrderEditSnapshot
        If lockForWrite AndAlso context.Transaction Is Nothing Then
            Throw New InvalidOperationException("Order validation requires a save transaction.")
        End If
        Dim connection = DirectCast(context.Connection, SqlConnection)
        Dim openedHere = connection.State = ConnectionState.Closed
        If openedHere Then connection.Open()
        Try
            Dim hint = If(lockForWrite, " WITH (UPDLOCK, HOLDLOCK)", String.Empty)
            Using command = connection.CreateCommand()
                command.Transaction = DirectCast(context.Transaction, SqlTransaction)
                command.CommandTimeout = 15
                command.Parameters.Add("@OrderID", SqlDbType.Int).Value = orderID
                command.CommandText =
                    "SELECT Closed FROM dbo.[Order]" & hint & " WHERE ID = @OrderID; " &
                    "SELECT (SELECT * FROM dbo.[Order]" & hint &
                    " WHERE ID = @OrderID FOR XML RAW('Row'), BINARY BASE64); " &
                    "SELECT (SELECT * FROM dbo.OrderEntry" & hint &
                    " WHERE OrderID = @OrderID ORDER BY ID FOR XML RAW('Row'), BINARY BASE64); " &
                    "SELECT (SELECT * FROM dbo.Queueing" & hint &
                    " WHERE OrderID = @OrderID ORDER BY id FOR XML RAW('Row'), BINARY BASE64); " &
                    "SELECT (SELECT qi.* FROM dbo.QueueingItems qi" & hint &
                    " JOIN dbo.Queueing q" & hint & " ON qi.QueueingID = q.id " &
                    "WHERE q.OrderID = @OrderID ORDER BY qi.id FOR XML RAW('Row'), BINARY BASE64);"
                Using reader = command.ExecuteReader(), buffer As New MemoryStream()
                    If Not reader.Read() OrElse reader.IsDBNull(0) OrElse reader.GetBoolean(0) Then
                        Throw New OrderEditConflictException()
                    End If
                    Using writer As New BinaryWriter(buffer, Encoding.UTF8, True)
                        For index As Integer = 1 To 4
                            If Not reader.NextResult() OrElse Not reader.Read() Then
                                Throw New InvalidOperationException("Unable to read the complete order state.")
                            End If
                            writer.Write(If(reader.IsDBNull(0), String.Empty, reader.GetString(0)))
                        Next
                    End Using
                    Using hash = SHA256.Create()
                        Return New OrderEditSnapshot(orderID,
                            connection.DataSource & "|" & connection.Database,
                            Convert.ToBase64String(hash.ComputeHash(buffer.ToArray())))
                    End Using
                End Using
            End Using
        Finally
            If openedHere Then connection.Close()
        End Try
    End Function

    Public Shared Sub EnsureUnchanged(expected As OrderEditSnapshot, current As OrderEditSnapshot)
        If expected Is Nothing OrElse current Is Nothing OrElse
           expected.OrderID <> current.OrderID OrElse
           Not String.Equals(expected.DatabaseIdentity, current.DatabaseIdentity, StringComparison.OrdinalIgnoreCase) OrElse
           String.IsNullOrEmpty(expected.Fingerprint) OrElse
           Not String.Equals(expected.Fingerprint, current.Fingerprint, StringComparison.Ordinal) Then
            Throw New OrderEditConflictException()
        End If
    End Sub
End Class
