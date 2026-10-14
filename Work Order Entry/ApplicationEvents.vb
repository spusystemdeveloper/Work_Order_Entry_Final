Namespace My

    Partial Friend Class MyApplication

        Private Sub MyApplication_UnhandledException(ByVal sender As Object,
                                                      ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Try
                Dim ex As Exception = e.Exception
                Dim logFile As String = ErrorLogger.LogError(ex, "Global Application Unhandled Exception")

                Dim errorMessage As String = "An unexpected error occurred and was safely intercepted to prevent a crash." & vbCrLf & vbCrLf &
                                             "Details : " & If(ex IsNot Nothing, ex.Message, "Unknown error") & vbCrLf &
                                             "Log File : " & logFile & vbCrLf & vbCrLf &
                                             "Would you like to keep the application open and continue working?" & vbCrLf &
                                             "(Click 'Yes' to continue, or 'No' to close safely)"

                Dim result As Windows.Forms.DialogResult =
                    Windows.Forms.MessageBox.Show(errorMessage,
                                                 "Work Order Entry - Unexpected Error Intercepted",
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

