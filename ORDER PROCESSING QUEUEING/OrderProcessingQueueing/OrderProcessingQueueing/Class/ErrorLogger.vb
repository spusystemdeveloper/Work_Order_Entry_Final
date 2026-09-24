Imports System.IO
Imports System.Text

Public Class ErrorLogger

    Private Shared ReadOnly LockObj As New Object()

    ''' <summary>
    ''' Writes an exception and contextual details to a daily error log file.
    ''' Guaranteed not to throw an exception if logging fails.
    ''' </summary>
    ''' <param name="ex">The exception that occurred.</param>
    ''' <param name="context">Optional context or operation name.</param>
    ''' <returns>The full path of the log file written.</returns>
    Public Shared Function LogError(ByVal ex As Exception, Optional ByVal context As String = "") As String
        Dim logFilePath As String = String.Empty

        Try
            Dim logDirectory As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            Dim fileName As String = "Error_" & DateTime.Now.ToString("yyyy-MM-dd") & ".log"
            logFilePath = Path.Combine(logDirectory, fileName)

            Dim sb As New StringBuilder()
            sb.AppendLine(New String("="c, 80))
            sb.AppendLine("[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] UNHANDLED EXCEPTION INTERCEPTED")
            If Not String.IsNullOrWhiteSpace(context) Then
                sb.AppendLine("Context      : " & context)
            End If

            ' Safe retrieval of terminal settings and environment
            Dim pickLoc As String = SafeGetSetting(Function() My.Settings.PickLoc, "Unknown")
            Dim releaseType As String = SafeGetSetting(Function() My.Settings.ReleaseType, "Unknown")
            sb.AppendLine("PickLoc      : " & pickLoc)
            sb.AppendLine("ReleaseType  : " & releaseType)
            sb.AppendLine("Machine      : " & Environment.MachineName)
            sb.AppendLine("OS Version   : " & Environment.OSVersion.ToString())

            If ex IsNot Nothing Then
                sb.AppendLine("Exception    : " & ex.GetType().FullName)
                sb.AppendLine("Message      : " & ex.Message)
                If Not String.IsNullOrEmpty(ex.Source) Then
                    sb.AppendLine("Source       : " & ex.Source)
                End If
                If Not String.IsNullOrEmpty(ex.StackTrace) Then
                    sb.AppendLine("Stack Trace  :")
                    sb.AppendLine(ex.StackTrace)
                End If

                Dim innerEx As Exception = ex.InnerException
                Dim depth As Integer = 1
                While innerEx IsNot Nothing
                    sb.AppendLine("--- Inner Exception (" & depth.ToString() & ") ---")
                    sb.AppendLine("Type    : " & innerEx.GetType().FullName)
                    sb.AppendLine("Message : " & innerEx.Message)
                    If Not String.IsNullOrEmpty(innerEx.StackTrace) Then
                        sb.AppendLine(innerEx.StackTrace)
                    End If
                    innerEx = innerEx.InnerException
                    depth += 1
                End While
            Else
                sb.AppendLine("Exception    : None (Exception object was null)")
            End If

            sb.AppendLine(New String("="c, 80))
            sb.AppendLine()

            SyncLock LockObj
                File.AppendAllText(logFilePath, sb.ToString(), Encoding.UTF8)
            End SyncLock

        Catch
            ' Fail-safe: do not allow error logging itself to cause a secondary crash
        End Try

        Return logFilePath
    End Function

    Private Shared Function SafeGetSetting(ByVal getter As Func(Of String), ByVal fallback As String) As String
        Try
            Dim val As String = getter()
            If Not String.IsNullOrWhiteSpace(val) Then Return val.Trim()
        Catch
        End Try
        Return fallback
    End Function

End Class
