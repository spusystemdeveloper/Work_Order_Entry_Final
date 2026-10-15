Option Strict On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms

' Running, jumping cat animation shown over a form while it runs long synchronous work (saving, importing).
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
    Private Const FrameMs As Integer = 50          ' 20 fps is smooth enough for the cat's stride
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
        If owner Is Nothing OrElse owner.IsDisposed Then Return New NestedScope(False)
        ' Settings > "Show loading animation" off: the work runs without the overlay.
        If Not StoreProcessingSettings.ShowLoadingAnimation Then Return New NestedScope(False)
        If Interlocked.Increment(activeCount) > 1 Then
            Return New NestedScope()
        End If
        Try
            ' A busy form would otherwise be swapped for a "Not Responding" ghost window,
            ' which takes the foreground away from this process and hides the overlay.
            DisableProcessWindowsGhosting()
            Return New LoadingOverlay(owner, text)
        Catch
            Interlocked.Decrement(activeCount)
            Return New NestedScope(False)
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
        Private ReadOnly ownsCount As Boolean
        Private done As Boolean
        Public Sub New(Optional counted As Boolean = True)
            ownsCount = counted
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            If done Then Return
            done = True
            If ownsCount Then Interlocked.Decrement(activeCount)
        End Sub
    End Class

    Private NotInheritable Class OverlayForm
        Inherits Form

        Private ReadOnly caption As String
        Private Const SceneWidth As Integer = 300
        Private Const SceneHeight As Integer = 132
        Private Const Gap As Integer = 8
        Private Const CatScale As Single = 1.2F
        Private Const CatCenterLocalX As Single = 39.0F ' local x midway between the feet
        Private Const CatScreenX As Integer = 90         ' cat centre, from the scene's left edge
        Private Const CatGroundY As Single = 44.0F       ' local y where the feet touch the ground
        Private Const PhaseStep As Single = 0.55F
        Private Const DashLength As Integer = 12
        Private Const DashSpacing As Integer = 22
        Private Const GroundSpeed As Integer = 7
        Private Const BlinkEveryFrames As Integer = 60
        ' Obstacles scroll in from the right; the cat jumps on a parabola whose height
        ' follows the nearest obstacle's distance, so it always clears it in time.
        Private Const ObstacleSpacing As Integer = 230
        Private Const ObstacleKinds As Integer = 2
        Private Const ObstacleSize As Integer = 18
        Private Const JumpHalfWidth As Single = 70.0F
        Private Const JumpHeight As Single = 48.0F

        Private Shared ReadOnly FurColor As Color = Color.FromArgb(244, 162, 89)
        Private Shared ReadOnly FarFurColor As Color = Color.FromArgb(222, 138, 66)
        Private Shared ReadOnly StripeColor As Color = Color.FromArgb(205, 110, 42)
        Private Shared ReadOnly OutlineColor As Color = Color.FromArgb(92, 56, 34)
        Private Shared ReadOnly InnerEarColor As Color = Color.FromArgb(248, 188, 186)
        Private Shared ReadOnly GroundColor As Color = Color.FromArgb(196, 202, 210)
        Private Shared ReadOnly ShadowColor As Color = Color.FromArgb(60, 0, 0, 0)
        Private Shared ReadOnly BoxColor As Color = Color.FromArgb(196, 150, 98)
        Private Shared ReadOnly BoxTapeColor As Color = Color.FromArgb(150, 108, 62)
        Private Shared ReadOnly YarnColor As Color = Color.FromArgb(120, 150, 230)
        Private Shared ReadOnly YarnLineColor As Color = Color.FromArgb(70, 95, 180)
        Private Shared ReadOnly TextColor As Color = Color.FromArgb(40, 40, 40)

        Private ReadOnly furBrush As New SolidBrush(FurColor)
        Private ReadOnly innerEarBrush As New SolidBrush(InnerEarColor)
        Private ReadOnly outlineBrush As New SolidBrush(OutlineColor)
        Private ReadOnly groundBrush As New SolidBrush(GroundColor)
        Private ReadOnly shadowBrush As New SolidBrush(ShadowColor)
        Private ReadOnly boxBrush As New SolidBrush(BoxColor)
        Private ReadOnly yarnBrush As New SolidBrush(YarnColor)
        Private ReadOnly outlinePen As New Pen(OutlineColor, 2.0F)
        Private ReadOnly whiskerPen As New Pen(OutlineColor, 1.0F)
        Private ReadOnly boxTapePen As New Pen(BoxTapeColor, 2.0F)
        Private ReadOnly yarnLinePen As New Pen(YarnLineColor, 1.5F)
        Private ReadOnly stripePen As New Pen(StripeColor, 3.0F) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round}
        Private ReadOnly limbOutlinePen As New Pen(OutlineColor, 8.0F) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round}
        Private ReadOnly limbPen As New Pen(FurColor, 5.0F) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round}
        Private ReadOnly farLimbPen As New Pen(FarFurColor, 5.0F) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round}
        Private ReadOnly captionSize As Size
        Private scene As Rectangle
        Private captionAt As Drawing.Point
        Private phase As Single
        ' How far the ground has scrolled, in pixels. Starting at 100 puts the first
        ' obstacle off the left edge, so the cat starts on the ground with a clear run.
        Private distance As Integer = 100
        Private frame As Integer
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
            Dim top = (ClientSize.Height - (SceneHeight + Gap + captionSize.Height)) \ 2
            scene = New Rectangle((ClientSize.Width - SceneWidth) \ 2, top, SceneWidth, SceneHeight)
            captionAt = New Drawing.Point((ClientSize.Width - captionSize.Width) \ 2, scene.Bottom + Gap)
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
            ' One running stride step; the ground and obstacles scroll left past the cat.
            phase += PhaseStep
            If phase > CSng(Math.PI * 2) Then phase -= CSng(Math.PI * 2)
            ' Wrap on a whole obstacle cycle so the scene repeats seamlessly.
            distance = (distance + GroundSpeed) Mod (ObstacleSpacing * ObstacleKinds * DashSpacing)
            frame += 1
            Invalidate(scene)
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            ' Alt+F4 on the overlay must not leave the busy form uncovered mid-save.
            If Not AllowClose AndAlso e.CloseReason = CloseReason.UserClosing Then e.Cancel = True
            MyBase.OnFormClosing(e)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            If e.ClipRectangle.IntersectsWith(scene) Then
                g.SetClip(scene)
                DrawGround(g)
                g.SmoothingMode = SmoothingMode.AntiAlias
                DrawObstacles(g)
                DrawCat(g, CurrentLift())
                g.SmoothingMode = SmoothingMode.Default
                g.ResetClip()
            End If
            If e.ClipRectangle.IntersectsWith(New Rectangle(captionAt, captionSize)) Then
                TextRenderer.DrawText(g, caption, Font, captionAt, TextColor)
            End If
        End Sub

        Private ReadOnly Property GroundY As Integer
            Get
                Return scene.Bottom - 10
            End Get
        End Property

        Private ReadOnly Property CatX As Integer
            Get
                Return scene.Left + CatScreenX
            End Get
        End Property

        Private Sub DrawGround(g As Graphics)
            Dim x = scene.Left - distance Mod DashSpacing
            Do While x < scene.Right
                g.FillRectangle(groundBrush, x, GroundY, DashLength, 3)
                x += DashSpacing
            Loop
        End Sub

        ' Screen x of each obstacle's centre that could be on screen, with its kind.
        Private Iterator Function VisibleObstacles() As IEnumerable(Of KeyValuePair(Of Integer, Integer))
            Dim first = distance \ ObstacleSpacing
            ' One step back covers the obstacle the cat has just jumped over.
            For index = first - 1 To first + SceneWidth \ ObstacleSpacing + 1
                Dim centre = scene.Left + index * ObstacleSpacing - distance + SceneWidth
                Yield New KeyValuePair(Of Integer, Integer)(centre, ((index Mod ObstacleKinds) + ObstacleKinds) Mod ObstacleKinds)
            Next
        End Function

        ' Height of the jump in pixels: a parabola over the obstacle nearest the cat.
        Private Function CurrentLift() As Single
            Dim lift = 0.0F
            For Each obstacle In VisibleObstacles()
                Dim d = Math.Abs(obstacle.Key - CatX) / JumpHalfWidth
                If d < 1.0F Then lift = Math.Max(lift, JumpHeight * (1.0F - d * d))
            Next
            Return lift
        End Function

        Private Sub DrawObstacles(g As Graphics)
            Dim half = ObstacleSize \ 2
            For Each obstacle In VisibleObstacles()
                Dim left = obstacle.Key - half
                If left > scene.Right OrElse left + ObstacleSize < scene.Left Then Continue For
                Dim top = GroundY - ObstacleSize + 1
                If obstacle.Value = 0 Then
                    ' Cardboard box with a tape strip.
                    g.FillRectangle(boxBrush, left, top, ObstacleSize, ObstacleSize)
                    g.DrawLine(boxTapePen, left + half, top, left + half, top + ObstacleSize)
                    g.DrawRectangle(outlinePen, left, top, ObstacleSize, ObstacleSize)
                Else
                    ' Ball of yarn.
                    Dim ball As New Rectangle(left, top, ObstacleSize, ObstacleSize)
                    g.FillEllipse(yarnBrush, ball)
                    g.DrawArc(yarnLinePen, left + 3, top + 2, ObstacleSize - 6, ObstacleSize - 4, 200, 140)
                    g.DrawArc(yarnLinePen, left + 2, top + 5, ObstacleSize - 4, ObstacleSize - 8, 20, 140)
                    g.DrawEllipse(outlinePen, ball)
                End If
            Next
        End Sub

        ' Drawn in local units (body spans x 8..68, feet at y = CatGroundY), then scaled.
        Private Sub DrawCat(g As Graphics, lift As Single)
            Dim airborne = lift > 1.0F
            Dim swing = CSng(Math.Sin(phase))
            Dim bob = If(airborne, 0.0F, CSng(Math.Abs(Math.Cos(phase))) * 3.0F)

            ' Shadow stays on the ground and shrinks as the cat rises.
            Dim shadowWidth = 56.0F * CatScale * (1.0F - lift / (JumpHeight * 2.0F))
            g.FillEllipse(shadowBrush, CatX - shadowWidth / 2.0F, GroundY - 2.0F, shadowWidth, 5.0F)

            Dim state = g.Save()
            g.TranslateTransform(CatX - CatCenterLocalX * CatScale,
                                 GroundY - (CatGroundY + bob) * CatScale - lift)
            g.ScaleTransform(CatScale, CatScale)

            ' In the air the legs stretch out: front ones forward, back ones behind.
            Dim backSwing = If(airborne, -1.5F, swing)
            Dim frontSwing = If(airborne, 1.5F, -swing)

            ' Far legs sit behind the body in a darker shade.
            DrawLeg(g, farLimbPen, 22.0F, backSwing)
            DrawLeg(g, farLimbPen, 56.0F, frontSwing)

            ' Tail swishes with the stride and streams out behind during a jump.
            Dim tail As PointF() = If(airborne,
                {New PointF(12.0F, 20.0F), New PointF(0.0F, 18.0F), New PointF(-10.0F, 14.0F), New PointF(-18.0F, 6.0F)},
                {New PointF(12.0F, 20.0F),
                 New PointF(-6.0F, 16.0F),
                 New PointF(-14.0F, -2.0F + swing * 5.0F),
                 New PointF(-6.0F + swing * 4.0F, -12.0F + swing * 3.0F)})
            g.DrawBeziers(limbOutlinePen, tail)
            g.DrawBeziers(limbPen, tail)

            ' Body with tabby stripes.
            Dim body As New RectangleF(8.0F, 10.0F, 60.0F, 26.0F)
            g.FillEllipse(furBrush, body)
            g.DrawEllipse(outlinePen, body)
            g.DrawLine(stripePen, 30.0F, 12.0F, 32.0F, 19.0F)
            g.DrawLine(stripePen, 38.0F, 11.0F, 40.0F, 18.0F)
            g.DrawLine(stripePen, 46.0F, 12.0F, 48.0F, 19.0F)

            ' Near legs swing opposite to the far ones while running.
            DrawLeg(g, limbPen, 16.0F, If(airborne, backSwing, -swing))
            DrawLeg(g, limbPen, 62.0F, If(airborne, frontSwing, swing))

            ' Ears go under the head so the head hides their base.
            DrawEar(g, New PointF(59.0F, 7.0F), New PointF(61.0F, -9.0F), New PointF(69.0F, 2.0F))
            DrawEar(g, New PointF(73.0F, 1.0F), New PointF(81.0F, -9.0F), New PointF(83.0F, 7.0F))

            Dim head As New RectangleF(56.0F, 0.0F, 28.0F, 26.0F)
            g.FillEllipse(furBrush, head)
            g.DrawEllipse(outlinePen, head)

            ' Face: wide eyes mid-jump, otherwise blink briefly every few seconds.
            If airborne Then
                g.FillEllipse(outlineBrush, 72.0F, 7.0F, 5.0F, 7.0F)
            ElseIf frame Mod BlinkEveryFrames < 3 Then
                g.DrawLine(outlinePen, 72.0F, 11.0F, 77.0F, 11.0F)
            Else
                g.FillEllipse(outlineBrush, 73.0F, 8.0F, 4.0F, 6.0F)
            End If
            g.FillPolygon(innerEarBrush, {New PointF(82.0F, 14.0F), New PointF(86.0F, 14.0F), New PointF(84.0F, 17.0F)})
            g.DrawLine(whiskerPen, 80.0F, 18.0F, 92.0F, 15.0F)
            g.DrawLine(whiskerPen, 80.0F, 20.0F, 92.0F, 21.0F)

            g.Restore(state)
        End Sub

        Private Sub DrawLeg(g As Graphics, pen As Pen, hipX As Single, swing As Single)
            Const HipY As Single = 30.0F
            Const Length As Single = CatGroundY - HipY
            Dim angle = swing * 0.6F
            Dim foot As New PointF(hipX + CSng(Math.Sin(angle)) * Length, HipY + CSng(Math.Cos(angle)) * Length)
            g.DrawLine(limbOutlinePen, hipX, HipY, foot.X, foot.Y)
            g.DrawLine(pen, hipX, HipY, foot.X, foot.Y)
        End Sub

        Private Sub DrawEar(g As Graphics, baseLeft As PointF, tip As PointF, baseRight As PointF)
            Dim ear = {baseLeft, tip, baseRight}
            g.FillPolygon(furBrush, ear)
            g.DrawPolygon(outlinePen, ear)
            ' Pink inner ear: a smaller triangle pulled toward the tip.
            Dim inner = {
                New PointF((baseLeft.X * 2 + tip.X) / 3.0F, (baseLeft.Y * 2 + tip.Y) / 3.0F + 1.0F),
                New PointF((tip.X * 3 + baseLeft.X + baseRight.X) / 5.0F, (tip.Y * 3 + baseLeft.Y + baseRight.Y) / 5.0F + 1.0F),
                New PointF((baseRight.X * 2 + tip.X) / 3.0F, (baseRight.Y * 2 + tip.Y) / 3.0F + 1.0F)}
            g.FillPolygon(innerEarBrush, inner)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                furBrush.Dispose()
                innerEarBrush.Dispose()
                outlineBrush.Dispose()
                groundBrush.Dispose()
                shadowBrush.Dispose()
                boxBrush.Dispose()
                yarnBrush.Dispose()
                outlinePen.Dispose()
                whiskerPen.Dispose()
                boxTapePen.Dispose()
                yarnLinePen.Dispose()
                stripePen.Dispose()
                limbOutlinePen.Dispose()
                limbPen.Dispose()
                farLimbPen.Dispose()
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
