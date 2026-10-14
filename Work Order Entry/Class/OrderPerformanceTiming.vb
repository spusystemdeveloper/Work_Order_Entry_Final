Imports System.Configuration
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO

' Optional diagnostics: no customer, item, password, or connection details are recorded.
Public NotInheritable Class OrderPerformanceTiming
    Implements IDisposable

    Private Shared ReadOnly LogLock As New Object()
    Private ReadOnly watch As Stopwatch
    Private ReadOnly phase As String
    Private ReadOnly orderID As Integer
    Public Property LineCount As Integer
    Private ReadOnly directory As String
    Private disposed As Boolean
    Private excludedMilliseconds As Double
    Public Property Outcome As String = "Completed"

    Public Sub New(phase As String, orderID As Integer, lineCount As Integer,
                   enabled As Boolean, logDirectory As String)
        Me.phase = phase
        Me.orderID = orderID
        Me.lineCount = lineCount
        Me.directory = logDirectory
        If enabled Then watch = Stopwatch.StartNew()
    End Sub

    Public Shared Function Start(phase As String, orderID As Integer, lineCount As Integer) As OrderPerformanceTiming
        Dim enabled As Boolean = False
        Try
            Boolean.TryParse(ConfigurationManager.AppSettings("EnableOrderPerformanceLogging"), enabled)
        Catch
            ' Diagnostics must never prevent normal order processing.
        End Try
        Return New OrderPerformanceTiming(phase, orderID, lineCount, enabled,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "WorkOrderEntry", "Logs"))
    End Function

    Public Shared Function Measure(Of T)(phase As String, orderID As Integer, lineCount As Integer,
                                         read As Func(Of T)) As T
        Using timing = Start(phase, orderID, lineCount)
            Try
                Return read()
            Catch
                timing.Outcome = "Failed"
                Throw
            End Try
        End Using
    End Function

    Public Sub ExcludeElapsed(milliseconds As Double)
        If disposed OrElse milliseconds <= 0 OrElse Double.IsNaN(milliseconds) OrElse Double.IsInfinity(milliseconds) Then Return
        excludedMilliseconds += milliseconds
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Return
        disposed = True
        If watch Is Nothing Then Return
        watch.Stop()
        Try
            Dim now = DateTime.Now
            Dim line = String.Join(",", now.ToString("o", CultureInfo.InvariantCulture),
                                   SafeField(phase), orderID.ToString(CultureInfo.InvariantCulture),
                                   lineCount.ToString(CultureInfo.InvariantCulture),
                                   Math.Max(0, watch.Elapsed.TotalMilliseconds - excludedMilliseconds).ToString("F3", CultureInfo.InvariantCulture),
                                   SafeField(Outcome)) & Environment.NewLine
            SyncLock LogLock
                IO.Directory.CreateDirectory(directory)
                Dim path = IO.Path.Combine(directory, "OrderTiming_" & now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) & ".csv")
                If Not File.Exists(path) Then
                    File.AppendAllText(path, "Time,Phase,OrderID,Lines,Milliseconds,Outcome" & Environment.NewLine)
                End If
                File.AppendAllText(path, line)
            End SyncLock
        Catch
            ' A full disk or denied log permission must not change a save result.
        End Try
    End Sub

    Private Shared Function SafeField(value As String) As String
        Return If(value, String.Empty).Replace(",", " ").Replace(vbCr, " ").Replace(vbLf, " ")
    End Function
End Class
