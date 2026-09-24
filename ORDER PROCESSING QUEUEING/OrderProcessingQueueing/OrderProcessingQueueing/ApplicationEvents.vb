Namespace My

    Partial Friend Class MyApplication

        Private Sub MyApplication_UnhandledException(ByVal sender As Object,
                                                      ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Try
                Dim ex As Exception = e.Exception
                Dim logFile As String = ErrorLogger.LogError(ex, "Queueing Application Unhandled Exception")

                ' For customer TV board or kiosk background timers, prevent sudden crashes
                ' on transient database / network connection hiccups
                Dim isTransientDbIssue As Boolean = False
                If ex IsNot Nothing Then
                    Dim exType As String = ex.GetType().FullName
                    If exType.Contains("SqlException") OrElse
                       exType.Contains("TimeoutException") OrElse
                       ex.Message.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       ex.Message.IndexOf("transport-level", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                       ex.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        isTransientDbIssue = True
                    End If
                End If

                If isTransientDbIssue Then
                    ' Suppress termination so TV Display or Queue Board retries on next timer tick
                    e.ExitApplication = False
                    Exit Sub
                End If

                Dim errorMessage As String = "An unexpected error occurred in Order Processing Queueing." & vbCrLf & vbCrLf &
                                             "Details : " & If(ex IsNot Nothing, ex.Message, "Unknown error") & vbCrLf &
                                             "Log File : " & logFile & vbCrLf & vbCrLf &
                                             "Would you like to keep the application open and continue?" & vbCrLf &
                                             "(Click 'Yes' to continue, or 'No' to close safely)"

                Dim result As Windows.Forms.DialogResult =
                    Windows.Forms.MessageBox.Show(errorMessage,
                                                 "Order Processing Queueing - Error Intercepted",
                                                 Windows.Forms.MessageBoxButtons.YesNo,
                                                 Windows.Forms.MessageBoxIcon.Warning)

                If result = Windows.Forms.DialogResult.Yes Then
                    e.ExitApplication = False
                Else
                    e.ExitApplication = True
                End If
            Catch
                ' Ensure unhandled exception handler never throws
            End Try
        End Sub

    End Class

End Namespace
