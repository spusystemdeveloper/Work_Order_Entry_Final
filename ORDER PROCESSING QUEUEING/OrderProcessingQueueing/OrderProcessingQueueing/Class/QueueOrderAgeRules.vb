Imports System
Imports System.Drawing

Public Enum QueuePriorityLevel
    Normal
    Warning
    Alert
End Enum

Public NotInheritable Class QueueOrderAgeRules

    Private Sub New()
    End Sub

    Public Const NormalMinutesThreshold As Double = 10.0
    Public Const WarningMinutesThreshold As Double = 20.0

    Private Shared _enabled As Nullable(Of Boolean) = Nothing

    Public Shared Property EnableOrderAgeAlerts As Boolean
        Get
            If Not _enabled.HasValue Then
                Dim configVal As String = System.Configuration.ConfigurationManager.AppSettings("EnableOrderAgeAlerts")
                Dim parsed As Boolean = True
                If Not String.IsNullOrEmpty(configVal) AndAlso Boolean.TryParse(configVal, parsed) Then
                    _enabled = parsed
                Else
                    _enabled = True
                End If
            End If
            Return _enabled.Value
        End Get
        Set(ByVal value As Boolean)
            _enabled = value
        End Set
    End Property

    Public Shared Function GetPriorityLevel(ByVal elapsedMinutes As Double) As QueuePriorityLevel
        If elapsedMinutes < NormalMinutesThreshold Then
            Return QueuePriorityLevel.Normal
        ElseIf elapsedMinutes < WarningMinutesThreshold Then
            Return QueuePriorityLevel.Warning
        Else
            Return QueuePriorityLevel.Alert
        End If
    End Function

    Public Shared Function FormatElapsedText(ByVal elapsedMinutes As Double) As String
        If elapsedMinutes < 0 Then
            Return "Just now"
        ElseIf elapsedMinutes < 1 Then
            Return "< 1m"
        ElseIf elapsedMinutes < 60 Then
            Return String.Format("{0:0}m", Math.Floor(elapsedMinutes))
        Else
            Dim hours As Integer = CInt(Math.Floor(elapsedMinutes / 60))
            Dim mins As Integer = CInt(Math.Floor(elapsedMinutes Mod 60))
            Return String.Format("{0}h {1}m", hours, mins)
        End If
    End Function

    Public Shared Function GetHeaderColor(ByVal priority As QueuePriorityLevel, ByVal isEnabled As Boolean) As Color
        If Not isEnabled Then
            Return SystemColors.ControlDarkDark
        End If

        Select Case priority
            Case QueuePriorityLevel.Normal
                Return Color.FromArgb(46, 125, 50)     ' Dark Forest Green
            Case QueuePriorityLevel.Warning
                Return Color.FromArgb(217, 119, 6)    ' Warm Amber
            Case QueuePriorityLevel.Alert
                Return Color.FromArgb(198, 40, 40)    ' Crimson Red
            Case Else
                Return SystemColors.ControlDarkDark
        End Select
    End Function

    Public Shared Function GetStatusBadgeText(ByVal priority As QueuePriorityLevel, ByVal elapsedMinutes As Double) As String
        Dim timeStr As String = FormatElapsedText(elapsedMinutes)
        Select Case priority
            Case QueuePriorityLevel.Normal
                Return "⏱️ " & timeStr
            Case QueuePriorityLevel.Warning
                Return "⚠️ " & timeStr
            Case QueuePriorityLevel.Alert
                Return "🔥 " & timeStr
            Case Else
                Return timeStr
        End Select
    End Function

End Class
