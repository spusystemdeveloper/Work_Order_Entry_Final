Public Class frmPickupDisplay

    Private _mainForm As frmMain
    Private _isFullScreen As Boolean = False
    Private _previousWindowState As FormWindowState = FormWindowState.Normal
    Private _previousBorderStyle As FormBorderStyle = FormBorderStyle.Sizable

    Public Sub New(ByVal mainForm As frmMain)
        InitializeComponent()
        _mainForm = mainForm
    End Sub

    Public Sub New()
        InitializeComponent()
        _mainForm = GetMainForm()
    End Sub

    Private Function GetMainForm() As frmMain
        If _mainForm IsNot Nothing AndAlso Not _mainForm.IsDisposed Then
            Return _mainForm
        End If
        For Each f As Form In Application.OpenForms
            If TypeOf f Is frmMain Then
                _mainForm = DirectCast(f, frmMain)
                Return _mainForm
            End If
        Next
        Return Nothing
    End Function

    Private Sub frmPickupDisplay_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        GetMainForm()
        UpdateClock()
        RefreshDisplay()
        timerRefresh.Start()
        timerClock.Start()
    End Sub

    Private Sub frmPickupDisplay_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        timerRefresh.Stop()
        timerClock.Stop()
    End Sub

    Private Sub timerClock_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timerClock.Tick
        UpdateClock()
    End Sub

    Private Sub UpdateClock()
        lblClock.Text = DateTime.Now.ToString("hh:mm:ss tt")
    End Sub

    Private Sub timerRefresh_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timerRefresh.Tick
        RefreshDisplay()
    End Sub

    Public Sub RefreshDisplay()
        Try
            Dim preparingList As New List(Of Tuple(Of String, String))()
            Dim readyList As New List(Of Tuple(Of String, String))()

            Dim main = GetMainForm()
            If main IsNot Nothing Then
                For Each ctrl As Control In main.flowPnl_Orders.Controls
                    Dim grid = TryCast(ctrl, UCGridOrder)
                    If grid IsNot Nothing Then
                        Dim woNum As String = If(grid.lblWO IsNot Nothing, grid.lblWO.Text.Trim(), "")
                        Dim custName As String = If(grid.lblCust IsNot Nothing, grid.lblCust.Text.Trim(), "")

                        If String.IsNullOrEmpty(woNum) Then Continue For

                        Dim bg = grid.BackColor
                        If bg = Color.PaleGreen OrElse bg = Color.CornflowerBlue Then
                            readyList.Add(Tuple.Create(woNum, custName))
                        ElseIf bg = Color.DarkSalmon OrElse bg = Color.OldLace Then
                            preparingList.Add(Tuple.Create(woNum, custName))
                        End If
                    End If
                Next
            End If

            ' Update Preparing grid
            gridPreparing.Rows.Clear()
            For Each item In preparingList.Take(30)
                Dim rowIdx = gridPreparing.Rows.Add()
                Dim r = gridPreparing.Rows(rowIdx)
                r.Cells(0).Value = item.Item1
                r.Cells(1).Value = item.Item2
            Next

            ' Update Ready grid
            gridReady.Rows.Clear()
            For Each item In readyList.Take(30)
                Dim rowIdx = gridReady.Rows.Add()
                Dim r = gridReady.Rows(rowIdx)
                r.Cells(0).Value = item.Item1
                r.Cells(1).Value = item.Item2
            Next

            lblPreparingTitle.Text = String.Format("🔨 BEING PREPARED ({0})", preparingList.Count)
            lblReadyTitle.Text = String.Format("✅ READY FOR PICKUP / CASHIER ({0})", readyList.Count)

        Catch ex As Exception
            ' Silent guard
        End Try
    End Sub

    ' F11 toggles Fullscreen mode for dedicated TV monitors
    Private Sub frmPickupDisplay_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.F11 Then
            ToggleFullScreen()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Escape AndAlso _isFullScreen Then
            ToggleFullScreen()
            e.Handled = True
        End If
    End Sub

    Private Sub pnlHeader_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles pnlHeader.DoubleClick
        ToggleFullScreen()
    End Sub

    Private Sub ToggleFullScreen()
        If Not _isFullScreen Then
            _previousWindowState = Me.WindowState
            _previousBorderStyle = Me.FormBorderStyle
            Me.FormBorderStyle = FormBorderStyle.None
            Me.WindowState = FormWindowState.Maximized
            _isFullScreen = True
        Else
            Me.FormBorderStyle = _previousBorderStyle
            Me.WindowState = _previousWindowState
            _isFullScreen = False
        End If
    End Sub

End Class
