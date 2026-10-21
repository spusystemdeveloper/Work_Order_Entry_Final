Imports System.Drawing
Imports System.Windows.Forms

'Only window geometry. No data loading, settings persistence or order actions.
Friend Module SmallScreenLayout
    Friend Sub Prepare(window As Form)
        Fit(window, Screen.FromControl(window).WorkingArea)
        AddHandler window.Resize, AddressOf WindowResized
    End Sub

    Private Sub WindowResized(sender As Object, e As EventArgs)
        Dim window = DirectCast(sender, Form)
        Fit(window, Screen.FromControl(window).WorkingArea)
    End Sub

    Friend Sub Fit(window As Form, area As Rectangle)
        If window.WindowState <> FormWindowState.Normal Then Return
        Dim available = New Size(Math.Max(1, area.Width - 16), Math.Max(1, area.Height - 16))
        window.MinimumSize = New Size(Math.Min(window.MinimumSize.Width, available.Width),
                                      Math.Min(window.MinimumSize.Height, available.Height))
        Dim size = New Size(Math.Min(window.Width, available.Width), Math.Min(window.Height, available.Height))
        Dim location = New Point(Math.Max(area.Left, Math.Min(window.Left, area.Right - size.Width)),
                                 Math.Max(area.Top, Math.Min(window.Top, area.Bottom - size.Height)))
        Dim bounds = New Rectangle(location, size)
        If window.Bounds <> bounds Then window.Bounds = bounds
    End Sub
End Module

Partial Class frmSettings
    Private Sub FitSettingsWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmRecall
    Private Sub FitRecallWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmCheckWO
    Private Sub FitCheckWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmPrintWo
    Private Sub FitOrderPreviewWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmPrintPicklist
    Private Sub FitPickPreviewWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmWebsiteImport
    Private Sub FitWebsiteWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmUploader
    Private Sub FitUploaderWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmOrderHistory
    Private Sub FitHistoryWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmShortageAlerts
    Private Sub FitShortageWindow(sender As Object, e As EventArgs) Handles MyBase.Shown
        SmallScreenLayout.Prepare(Me)
    End Sub
End Class

Partial Class frmItemLookUp
    Private arrangingSmallScreen As Boolean

    Private Sub ArrangeSmallScreen(sender As Object, e As EventArgs) Handles MyBase.SizeChanged, MyBase.FontChanged, panelGrid.SizeChanged, Panel5.SizeChanged, panelTotal.SizeChanged
        If arrangingSmallScreen OrElse panelGrid Is Nothing OrElse Panel5 Is Nothing OrElse panelSelectItem Is Nothing OrElse panelTotal Is Nothing OrElse GroupBox2 Is Nothing OrElse GroupBox3 Is Nothing OrElse GroupBox4 Is Nothing Then Return
        arrangingSmallScreen = True
        Try
            Dim scale = Font.Size / 9.0F
            Dim preferred = CInt(220 * scale)
            Dim minimum = CInt(120 * scale)
            panelSelectItem.Height = Math.Min(preferred, Math.Max(minimum, panelGrid.ClientSize.Height - Panel5.Height - 12))

            'Keep the original right-aligned totals widths when space allows.
            Dim gap = CInt(6 * scale)
            Dim suggestionWidth = If(grpWithholding IsNot Nothing AndAlso grpWithholding.Visible,
                                     Math.Min(CInt(300 * scale), panelTotal.ClientSize.Width \ 3), 0)
            Dim suggestionSpace = If(suggestionWidth > 0, suggestionWidth + gap, 0)
            Dim available = Math.Max(3, panelTotal.ClientSize.Width - 10 - 2 * gap - suggestionSpace)
            Dim ratio = Math.Min(1.0F, available / (870.0F * scale))
            Dim subWidth = CInt(279 * scale * ratio)
            Dim vatWidth = CInt(279 * scale * ratio)
            Dim totalWidth = Math.Min(CInt(312 * scale * ratio), available - subWidth - vatWidth)
            Dim left = Math.Max(5, panelTotal.ClientSize.Width - 5 - subWidth - vatWidth - totalWidth - 2 * gap)
            GroupBox2.SetBounds(left, GroupBox2.Top, subWidth, GroupBox2.Height)
            GroupBox3.SetBounds(left + subWidth + gap, GroupBox3.Top, vatWidth, GroupBox3.Height)
            GroupBox4.SetBounds(left + subWidth + vatWidth + 2 * gap, GroupBox4.Top, totalWidth, GroupBox4.Height)
            If grpWithholding IsNot Nothing AndAlso suggestionWidth > 0 Then
                grpWithholding.SetBounds(5, 3, suggestionWidth, panelTotal.ClientSize.Height - 6)
            End If
        Finally
            arrangingSmallScreen = False
        End Try
    End Sub
End Class
