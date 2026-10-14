Option Strict On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms

' Sliding progress bar shown over a form while it runs long synchronous work (saving, importing).
' The overlay runs on its own UI thread so it keeps animating while the form's thread is busy.
' It hides itself while a prompt or dialog of the busy form is visible, so message boxes
' raised mid-operation stay usable.
'
' Usage:  Using LoadingOverlay.Show(Me, "Saving order...")
'             ...work...
'         End Using
Public NotInheritable Class LoadingOverlay
    Implements IDisposable

    Private Const ShowDelayMs As Integer = 250
    Private Const FrameMs As Integer = 50          ' 20 fps is smooth enough for the sweep
    Private Const ChecksEveryFrames As Integer = 4 ' window/dialog checks ~5 times a second

    Private Shared activeCount As Integer

    Private ReadOnly target As Form
    Private ReadOnly targetHandle As IntPtr
    Private ReadOnly message As String
    Private ReadOnly started As DateTime = DateTime.UtcNow
    Private ReadOnly thread As Thread
    Private overlayHandle As IntPtr
    Private stopRequested As Integer
    Private disposed As Boolean

    Private Sub New(owner As Form, text As String)
        target = owner
        targetHandle = owner.Handle
        message = text
        thread = New Thread(AddressOf RunOverlay) With {.IsBackground = True, .Name = "LoadingOverlay"}
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
    End Sub

    Public Shared Function Show(owner As Form, text As String) As IDisposable
        ' Nested operations (e.g. save inside an import) keep the outer overlay.
        If owner Is Nothing OrElse owner.IsDisposed OrElse Interlocked.Increment(activeCount) > 1 Then
            Return New NestedScope()
        End If
        Try
            ' A busy form would otherwise be swapped for a "Not Responding" ghost window,
            ' which takes the foreground away from this process and hides the overlay.
            DisableProcessWindowsGhosting()
            Return New LoadingOverlay(owner, text)
        Catch
            Interlocked.Decrement(activeCount)
            Return New NestedScope()
        End Try
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Return
        disposed = True
        Dim overlayWasForeground = overlayHandle <> IntPtr.Zero AndAlso GetForegroundWindow() = overlayHandle
        Interlocked.Exchange(stopRequested, 1)
        thread.Join(2000)
        Interlocked.Decrement(activeCount)
        If overlayWasForeground AndAlso Not target.IsDisposed AndAlso target.Visible Then target.Activate()
    End Sub

    Private Sub RunOverlay()
        Try
            Using view As New OverlayForm(message)
                overlayHandle = view.Handle
                Dim timer As New System.Windows.Forms.Timer With {.Interval = FrameMs}
                Dim frame = 0
                AddHandler timer.Tick,
                    Sub()
                        If Thread.VolatileRead(stopRequested) = 1 Then
                            timer.Stop()
                            view.AllowClose = True
                            Application.ExitThread()
                            Return
                        End If
                        If frame Mod ChecksEveryFrames = 0 Then UpdatePlacement(view)
                        frame += 1
                        If view.Visible Then view.Advance()
                    End Sub
                timer.Start()
                Application.Run()
                timer.Dispose()
            End Using
        Catch
            ' A loading indicator must never break the operation it decorates.
        End Try
    End Sub

    Private Sub UpdatePlacement(view As OverlayForm)
        Dim shouldShow = (DateTime.UtcNow - started).TotalMilliseconds >= ShowDelayMs AndAlso
                         IsWindowVisible(targetHandle) AndAlso Not IsIconic(targetHandle) AndAlso
                         AppIsForeground() AndAlso Not DialogIsOpen()
        If Not shouldShow Then
            If view.Visible Then view.Hide()
            Return
        End If

        Dim client As RECT
        If Not GetClientRect(targetHandle, client) Then Return
        Dim origin As New POINT()
        ClientToScreen(targetHandle, origin)
        Dim bounds As New Rectangle(origin.X, origin.Y, client.Right - client.Left, client.Bottom - client.Top)
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return
        If view.Bounds <> bounds Then view.Bounds = bounds
        If Not view.Visible Then
            view.Show()
            SetWindowPos(view.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE Or SWP_NOSIZE Or SWP_NOACTIVATE)
        End If
    End Sub

    ' While the busy form shows a prompt or dialog, Windows disables it and the dialog is
    ' a visible window on the same thread. A hidden modal form (e.g. one that hid itself
    ' before starting the import) does not count.
    Private Function DialogIsOpen() As Boolean
        If IsWindowEnabled(targetHandle) Then Return False
        Dim found = False
        Dim threadId = GetWindowThreadProcessId(targetHandle, 0)
        EnumThreadWindows(threadId,
            Function(hWnd, lParam)
                If hWnd <> targetHandle AndAlso hWnd <> overlayHandle AndAlso
                   IsWindowVisible(hWnd) AndAlso IsWindowEnabled(hWnd) Then
                    Dim r As RECT
                    If GetWindowRect(hWnd, r) AndAlso r.Right - r.Left > 1 AndAlso r.Bottom - r.Top > 1 Then
                        found = True
                        Return False
                    End If
                End If
                Return True
            End Function, IntPtr.Zero)
        Return found
    End Function

    Private Shared Function AppIsForeground() As Boolean
        Dim foreground = GetForegroundWindow()
        If foreground = IntPtr.Zero Then Return False
        Dim processId As Integer
        GetWindowThreadProcessId(foreground, processId)
        Return processId = CurrentProcessId
    End Function

    Private Shared ReadOnly CurrentProcessId As Integer = Process.GetCurrentProcess().Id

    Private NotInheritable Class NestedScope
        Implements IDisposable
        Private done As Boolean
        Public Sub Dispose() Implements IDisposable.Dispose
            If done Then Return
            done = True
            Interlocked.Decrement(activeCount)
        End Sub
    End Class

    Private NotInheritable Class OverlayForm
        Inherits Form

        Private ReadOnly caption As String
        Private Const BarWidth As Integer = 260
        Private Const BarHeight As Integer = 6
        Private Const SegmentWidth As Integer = 80
        Private Const SweepStep As Integer = 9
        Private Const Gap As Integer = 12

        Private Shared ReadOnly TrackColor As Color = Color.FromArgb(220, 224, 230)
        Private Shared ReadOnly BarColor As Color = Color.FromArgb(0, 120, 215)
        Private Shared ReadOnly TextColor As Color = Color.FromArgb(40, 40, 40)
        Private ReadOnly trackBrush As New SolidBrush(TrackColor)
        Private ReadOnly barBrush As New SolidBrush(BarColor)
        Private ReadOnly captionSize As Size
        Private bar As Rectangle
        Private captionAt As Drawing.Point
        Private offset As Integer = -SegmentWidth
        Public AllowClose As Boolean

        Public Sub New(text As String)
            caption = text
            FormBorderStyle = FormBorderStyle.None
            ShowInTaskbar = False
            StartPosition = FormStartPosition.Manual
            BackColor = Color.White
            Opacity = 0.85
            DoubleBuffered = True
            Cursor = Cursors.WaitCursor
            Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
            captionSize = TextRenderer.MeasureText(caption, Font)
        End Sub

        Protected Overrides Sub OnResize(e As EventArgs)
            MyBase.OnResize(e)
            Dim top = (ClientSize.Height - (captionSize.Height + Gap + BarHeight)) \ 2
            captionAt = New Drawing.Point((ClientSize.Width - captionSize.Width) \ 2, top)
            bar = New Rectangle((ClientSize.Width - BarWidth) \ 2, top + captionSize.Height + Gap, BarWidth, BarHeight)
            Invalidate()
        End Sub

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Dim cp = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW
                Return cp
            End Get
        End Property

        Public Sub Advance()
            ' Slide the segment across the track, then restart from just off the left edge.
            offset += SweepStep
            If offset > BarWidth Then offset = -SegmentWidth
            Invalidate(bar)
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            ' Alt+F4 on the overlay must not leave the busy form uncovered mid-save.
            If Not AllowClose AndAlso e.CloseReason = CloseReason.UserClosing Then e.Cancel = True
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            g.FillRectangle(trackBrush, bar)
            Dim left = Math.Max(bar.Left, bar.Left + offset)
            Dim right = Math.Min(bar.Right, bar.Left + offset + SegmentWidth)
            If right > left Then g.FillRectangle(barBrush, left, bar.Top, right - left, bar.Height)
            If e.ClipRectangle.IntersectsWith(New Rectangle(captionAt, captionSize)) Then
                TextRenderer.DrawText(g, caption, Font, captionAt, TextColor)
            End If
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                trackBrush.Dispose()
                barBrush.Dispose()
                Font.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

#Region "Win32"
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Shared ReadOnly HWND_TOP As IntPtr = IntPtr.Zero
    Private Const SWP_NOSIZE As Integer = &H1
    Private Const SWP_NOMOVE As Integer = &H2
    Private Const SWP_NOACTIVATE As Integer = &H10

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left, Top, Right, Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure POINT
        Public X, Y As Integer
    End Structure

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")> Private Shared Sub DisableProcessWindowsGhosting()
    End Sub
    <DllImport("user32.dll")> Private Shared Function GetForegroundWindow() As IntPtr
    End Function
    <DllImport("user32.dll")> Private Shared Function GetWindowThreadProcessId(hWnd As IntPtr, ByRef processId As Integer) As Integer
    End Function
    <DllImport("user32.dll")> Private Shared Function IsWindowVisible(hWnd As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function IsWindowEnabled(hWnd As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function IsIconic(hWnd As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function GetClientRect(hWnd As IntPtr, ByRef rect As RECT) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function GetWindowRect(hWnd As IntPtr, ByRef rect As RECT) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function ClientToScreen(hWnd As IntPtr, ByRef point As POINT) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function SetWindowPos(hWnd As IntPtr, insertAfter As IntPtr, x As Integer, y As Integer, cx As Integer, cy As Integer, flags As Integer) As Boolean
    End Function
    <DllImport("user32.dll")> Private Shared Function EnumThreadWindows(threadId As Integer, callback As EnumWindowsProc, lParam As IntPtr) As Boolean
    End Function
#End Region
End Class
