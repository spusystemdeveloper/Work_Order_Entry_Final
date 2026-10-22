Public Class CustomPanel
    Inherits Panel

  
    Private Const WM_MOUSEMOVE As Integer = &H200
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const MK_LBUTTON As Integer = &H1
    Private Const WM_NCHITTEST As Integer = &H84
    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTLEFT As Integer = &HA
    Private Const HTRIGHT As Integer = &HB
    Private Const HTTOP As Integer = &HC
    Private Const HTTOPLEFT As Integer = &HD
    Private Const HTTOPRIGHT As Integer = &HE
    Private Const HTBOTTOM As Integer = &HF
    Private Const HTBOTTOMLEFT As Integer = &H10
    Private Const HTBOTTOMRIGHT As Integer = &H11
    Private OffSet As Point = Point.Empty
    Private Selected As Boolean = False

    Public Sub New()
        Me.Cursor = Cursors.SizeAll
        Me.BackColor = Color.Transparent
    End Sub

    Protected Overrides Sub OnPaint(ByVal pe As PaintEventArgs)
        MyBase.OnPaint(pe)

        If Me.Capture Then 'draws a 'dashed' border if mouse is captured
            Using bp As New Pen(Color.White, 1)
                pe.Graphics.DrawRectangle(bp, 0, 0, Me.Width - 1, Me.Height - 1)
                bp.DashStyle = Drawing2D.DashStyle.DashDot
                bp.Color = Color.Black
                pe.Graphics.DrawRectangle(bp, 0, 0, Me.Width - 1, Me.Height - 1)
            End Using

        Else 'draws a 'single line' border if mouse is not captured
            pe.Graphics.DrawRectangle(Pens.Black, 0, 0, Me.Width - 1, Me.Height - 1)
        End If
    End Sub

    Protected Overrides Sub OnMouseCaptureChanged(ByVal e As EventArgs)
        MyBase.OnMouseCaptureChanged(e)
        Me.Invalidate(False)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_NCLBUTTONDOWN Then
            OnMouseCaptureChanged(EventArgs.Empty)
        ElseIf m.Msg = WM_NCHITTEST Then
            Dim loc As Point = Me.PointToClient(MousePosition)
            Dim bTop As Boolean = (loc.Y < 6)
            Dim bLeft As Boolean = (loc.X < 6)
            Dim bRight As Boolean = (loc.X > Me.Width - 7)
            Dim bBottom As Boolean = (loc.Y > Me.Height - 7)

            If bTop AndAlso bLeft Then
                m.Result = CType(HTTOPLEFT, IntPtr)
                Return
            ElseIf bTop AndAlso bRight Then
                m.Result = CType(HTTOPRIGHT, IntPtr)
                Return
            ElseIf bBottom AndAlso bLeft Then
                m.Result = CType(HTBOTTOMLEFT, IntPtr)
                Return
            ElseIf bBottom AndAlso bRight Then
                m.Result = CType(HTBOTTOMRIGHT, IntPtr)
                Return
            ElseIf bLeft Then
                m.Result = CType(HTLEFT, IntPtr)
                Return
            ElseIf bTop Then
                m.Result = CType(HTTOP, IntPtr)
                Return
            ElseIf bRight Then
                m.Result = CType(HTRIGHT, IntPtr)
                Return
            ElseIf bBottom Then
                m.Result = CType(HTBOTTOM, IntPtr)
                Return
            End If
        ElseIf m.Msg = WM_LBUTTONDOWN Then
            OffSet = New Point(MousePosition.X - Me.Location.X, MousePosition.Y - Me.Location.Y)
            OnMouseCaptureChanged(EventArgs.Empty)
        ElseIf m.Msg = WM_MOUSEMOVE AndAlso m.WParam.ToInt32 = MK_LBUTTON Then
            Me.Location = New Point(MousePosition.X - OffSet.X, MousePosition.Y - OffSet.Y)
        End If
        MyBase.WndProc(m)
    End Sub
End Class