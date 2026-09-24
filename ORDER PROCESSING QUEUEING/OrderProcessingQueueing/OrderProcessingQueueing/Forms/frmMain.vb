Imports System.Threading
Imports System.IO
Imports Microsoft.Reporting.WinForms
Imports System.Drawing.Printing
Imports System.Drawing.Imaging

Public Class frmMain

    Public Shared groupIDList As New List(Of String)            ' List sa Group ID pag Load sa Program
    Public Shared groupIDListNew As New List(Of String)         ' List sa bago nga nadagdag nga Group ID
    Public Shared groupIDifference As New List(Of String)       ' List sa Difference sa groupIDList ug groupIDListNew pra ma remove 

    Public queueListPrepared As New List(Of String)
    Public queueListProcessing As New List(Of String)
    Public queueListForInvoice As New List(Of String)
    Public queueListForProcess As New List(Of String)


    Public Shared ItemOrderedList As List(Of clsOrderedItems) = New List(Of clsOrderedItems)()      ' List sa Items for picking para sa Printing

    Public skipUpdate As Boolean = False

    Dim db_lastupdated

    Public pType = ""
    Public rType = ""
    Private m_overdueCount As Integer = 0
    Private queueSearchDebounceTimer As Windows.Forms.Timer

    Private Const WM_HSCROLL As Integer = &H114
    Private Const WM_VSCROLL As Integer = &H115

    Protected Overrides Sub WndProc(ByRef m As Message)
        Try
            If (m.Msg = WM_HSCROLL OrElse m.Msg = WM_VSCROLL) AndAlso ((CInt(m.WParam) And &HFFFF) = 5) Then
                m.WParam = CType(((CInt(m.WParam) And Not &HFFFF) Or 4), IntPtr)
            End If
            MyBase.WndProc(m)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub reloadUserControl()

        Dim recentEditedOrder = clsQueueing.getLastUpdatedWo(rType)

        For Each x In recentEditedOrder

            Dim groupid = clsQueueing.getGroupID(x)
            If groupid IsNot Nothing Then
                Dim groupName As String = groupid.ToString()
                Dim found = flowPnl_Orders.Controls.Find(groupName, False)
                If found.Length > 0 AndAlso TypeOf found(0) Is UCGridOrder Then
                    Dim uc = DirectCast(found(0), UCGridOrder)
                    uc.updateData()

                    Dim countEntry = uc.gridOrder.Rows.Count
                    If countEntry <= 0 Then
                        flowPnl_Orders.Controls.Remove(uc)
                    End If
                End If
            End If

        Next

    End Sub

    Public Sub prioritizeUserControl(ByVal controlName As String)

        Dim found = flowPnl_Orders.Controls.Find(controlName, False)
        If found.Length > 0 AndAlso TypeOf found(0) Is UCGridOrder Then
            CType(found(0), UCGridOrder).updateData() ' prioritize update
            skipUpdate = True
        End If

        countQueue()
        ApplyFilter(currentFilterStage)

    End Sub

    Public Sub formLoad()

        Cursor.Current = Cursors.WaitCursor
        chk_Panels.Stop()
        db_lastupdated = clsQueueing.checkTableUpdate()
        LoadOrders()
        chk_Panels.Start()
        countQueue()
        ApplyFilter(currentFilterStage)
        Cursor.Current = Cursors.Default

    End Sub


    Public Sub LoadOrders()

        Dim countEntry

        Dim data = clsQueueing.LoadOpenWO(rType, pType)                    ' added where clause. add only the new GroupID

        flowPnl_Orders.SuspendLayout()
        Try
            For Each d In data

                Dim _grid = New UCGridOrder
                Dim orderIds As String = clsQueueing.getOrderIDs(d)

                _grid.Name = d                                          ' name the added usercontrol based on group id
                _grid.lblWO.Text = orderIds                             ' concatenate multitple Order in the Group
                _grid.lblGroupID.Text = d
                _grid.pnlWo.Name = orderIds

                countEntry = clsQueueing.CountOrderEntry(d, pType, rType)

                If Not countEntry = 0 Then
                    addToPnl(_grid)
                End If

            Next
        Finally
            flowPnl_Orders.ResumeLayout(True)
        End Try

        loadControlList()                                            ' Add Added UserControl to List 

    End Sub

    Private Delegate Sub addToPnlDeletegate(ByVal cntrl As Control)

    Private Sub addToPnl(ByVal cntrl As Control)
        If flowPnl_Orders.InvokeRequired Then
            Dim d As New addToPnlDeletegate(AddressOf addToPnl)
            Me.Invoke(d, New Object() {cntrl})
        Else
            flowPnl_Orders.Controls.Add(cntrl)
        End If
    End Sub


    Private Delegate Sub changeTxtDeletegate(ByVal cntrl As Control, ByVal txt As String)

    Private Sub changeTxt(ByVal cntrl As Control, ByVal txt As String)
        If cntrl.InvokeRequired Then
            Dim d As New changeTxtDeletegate(AddressOf changeTxt)
            Me.Invoke(d, New Object() {cntrl, txt})
        Else
            cntrl.Text = txt
        End If
    End Sub

    Private Sub manangeUserControl()

        LoadOrdersUpdate()          ' Get New Result from Open Wo then add to list
        LoadOrdersDifference()      ' Find Difference To remove in the Panel

    End Sub

    Private Sub loadControlList()

        groupIDList.Clear()

        For Each ctrl In flowPnl_Orders.Controls
            groupIDList.Add(ctrl.name)
        Next

    End Sub


    Private Sub LoadOrdersUpdate()


        Dim data = clsQueueing.LoadOpenWOUpdate(rType, pType)

        groupIDListNew.Clear()

        For Each d In data
            groupIDListNew.Add(d)
        Next

    End Sub

    Private Sub LoadOrdersDifference()

        groupIDifference.Clear()
        Dim Difference = groupIDList.Except(groupIDListNew).ToArray()

        For Each x As String In Difference
            groupIDifference.Add(x)
        Next

    End Sub

    Private Sub removeOldControls()

        For Each x As String In groupIDifference

            For Each ctrl In flowPnl_Orders.Controls

                If ctrl.name = x Then
                    Me.flowPnl_Orders.Controls.Remove(ctrl)
                End If

            Next
        Next

    End Sub

    Private Sub checkForErrors()
        'If ErrorCount >= 3 Then
        '    chk_Panels.Stop()
        '    MessageBox.Show("The Program Encountered Too many Errors ! The Program will be Restarted", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        '    Application.Restart()
        'End If
    End Sub

    Private Sub countQueue()

        Dim xthread As New Thread(Sub()

                                      queueListPrepared.Clear()
                                      queueListProcessing.Clear()
                                      queueListForInvoice.Clear()
                                      queueListForProcess.Clear()

                                       Dim inqueue = 0
                                       Dim forProcess = 0
                                       Dim InProcess = 0
                                       Dim Prepared = 0
                                       Dim forInvoice = 0
                                       Dim overdueCount = 0

                                       Dim controlsList As New List(Of Control)
                                       If flowPnl_Orders.InvokeRequired Then
                                           Me.Invoke(Sub()
                                                         For Each ctrl As Control In flowPnl_Orders.Controls
                                                             controlsList.Add(ctrl)
                                                         Next
                                                     End Sub)
                                       Else
                                           For Each ctrl As Control In flowPnl_Orders.Controls
                                               controlsList.Add(ctrl)
                                           Next
                                       End If

                                       For Each ctrl In controlsList
                                           If TypeOf ctrl Is UCGridOrder Then
                                               Dim gridOrder = DirectCast(ctrl, UCGridOrder)
                                               Dim groupName As String = gridOrder.Name
                                               inqueue += 1

                                               Dim cColor As Color = gridOrder.BackColor
                                               If cColor = Color.OldLace Then
                                                   forProcess += 1
                                                   queueListForProcess.Add(groupName)
                                               ElseIf cColor = Color.DarkSalmon Then
                                                   InProcess += 1
                                                   queueListProcessing.Add(groupName)
                                               ElseIf cColor = Color.PaleGreen Then
                                                   Prepared += 1
                                                   queueListPrepared.Add(groupName)
                                               ElseIf cColor = Color.CornflowerBlue Then
                                                   forInvoice += 1
                                                   queueListForInvoice.Add(groupName)
                                               End If

                                               If gridOrder.OrderTime.HasValue AndAlso cColor <> Color.CornflowerBlue Then
                                                   Dim elapsed As Double = (DateTime.Now - gridOrder.OrderTime.Value).TotalMinutes
                                                   If QueueOrderAgeRules.GetPriorityLevel(elapsed) = QueuePriorityLevel.Alert Then
                                                       overdueCount += 1
                                                   End If
                                               End If
                                           End If
                                       Next

                                       m_overdueCount = overdueCount
                                       changeTxt(lblTotalInQueue, inqueue)
                                       changeTxt(lblForProcess, forProcess)
                                       changeTxt(lblProcessing, InProcess)
                                       changeTxt(lblPrepared, Prepared)
                                       changeTxt(lblForInvoice, forInvoice)
                                       changeTxt(lblOverdue, overdueCount)

                                        Dim completedToday As Integer = 0
                                        Try
                                            completedToday = clsQueueing.GetCompletedTodayCount()
                                        Catch ex As Exception
                                        End Try
                                        changeTxt(lblCompletedToday, completedToday.ToString())

                                       If Me.InvokeRequired Then
                                           Me.Invoke(Sub() UpdateTabVisuals())
                                       Else
                                           UpdateTabVisuals()
                                       End If

                                   End Sub)
        xthread.Start()

    End Sub

    Private Sub updateData()

        reloadUserControl()
        LoadOrders()
        manangeUserControl()
        removeOldControls()
        ' checkForErrors()
        countQueue()
        ApplyFilter(currentFilterStage)

    End Sub

    Private Sub chk_Panels_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chk_Panels.Tick

        If Not db_lastupdated = clsQueueing.checkTableUpdate() Then

            db_lastupdated = clsQueueing.checkTableUpdate()

            If skipUpdate = True Then

                skipUpdate = False
                Exit Sub

            Else
                updateData()
            End If

        End If

        RefreshOrderAges()

    End Sub

    Public Sub RefreshOrderAges()
        Try
            For Each ctrl In flowPnl_Orders.Controls
                If TypeOf ctrl Is UCGridOrder Then
                    CType(ctrl, UCGridOrder).RefreshOrderAge()
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub

    Public Sub clearList()

        groupIDList.Clear()
        groupIDListNew.Clear()
        groupIDifference.Clear()

        queueListPrepared.Clear()
        queueListProcessing.Clear()
        queueListForInvoice.Clear()
        queueListForProcess.Clear()

    End Sub

    Private Sub changePicLoc()

        clearList()
        flowPnl_Orders.Controls.Clear()
        formLoad()


    End Sub

    Private Sub rbtnStore_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        changePicLoc()
    End Sub

    Private Sub rbtnUpStore_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        changePicLoc()
    End Sub

    Public Sub printItems(ByVal _GroupID As Integer)

        ItemOrderedList.Clear()

        If _GroupID = Nothing Then
            Exit Sub
        End If

        Dim ItemOrdered = clsQueueing.LoadOrders(_GroupID, pType, rType)

        Dim wo = clsQueueing.getOrderIDs(_GroupID)
        Dim branch = (From a In db.Configurations Select a.StoreCity).ToList()(0)

        If wo = Nothing Then
            Exit Sub
        End If

        For Each x In ItemOrdered

            Dim item As clsOrderedItems = New clsOrderedItems

            item.qtyOrder = clsQueueing.GetQtyOrd(x.OrderID, x.ItemID)
            item.qtyPrep = clsQueueing.getQtyPrep(x.ItemID, x.OrderID)
            item.itemDesc = x.Description
            item.itemCode = x.Itemcode
            item.picker = x.Picker
            item.wokrOrder = x.Orderid
            item.wokrOrderS = wo
            item.branch = branch
            item.printDate = DateTime.Now.ToString("dd/MM/yyyy")
            item.printTime = DateTime.Now.ToString("HH:mm:ss tt")
            item.OPIS = x.OPIS
            item.OPIS_Name = clsQueueing.getOrderTaker(x.OPIS)
            item.type = "For " & rType
            item.Customer = clsQueueing.getCustomerName(x.OrderID)
            ItemOrderedList.Add(item)

        Next

        printWithoutPreview()

    End Sub

    Private m_streams As IList(Of Stream)
    Dim m_currentPageIndex As Integer

    Private Function CreateStream(ByVal name As String, ByVal fileNameExtension As String, ByVal encoding As System.Text.Encoding, ByVal mimeType As String, ByVal willSeek As Boolean) As Stream

        Dim stream As Stream = New MemoryStream()
        m_streams.Add(stream)
        Return stream

    End Function

    Private Sub printWithoutPreview()

        Dim report As New LocalReport()
        report.DataSources.Add(New ReportDataSource("ItemOrderEntry", ItemOrderedList))
        report.ReportEmbeddedResource = "OrderProcessingQueueing.rpt_PickList.rdlc"

        Dim deviceInfo As String = "<DeviceInfo><OutputFormat>EMF</OutputFormat><PageWidth>8.5in</PageWidth><PageHeight>5.5in</PageHeight><MarginTop>0.10in</MarginTop><MarginLeft>0.10in</MarginLeft><MarginRight>0.10in</MarginRight><MarginBottom>0.10in</MarginBottom></DeviceInfo>"
        Dim warnings As Warning() = New Warning() {}
        m_streams = New List(Of Stream)()
        report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)

        ' Filter out any empty trailing stream (< 408 bytes) to avoid printing blank extra page
        Dim emptyStreams = m_streams.Where(Function(x) x.Length <= 408).ToList()
        For Each strm As Stream In emptyStreams
            m_streams.Remove(strm)
        Next

        For Each stream As Stream In m_streams
            stream.Position = 0
        Next

        If m_streams.Count = 0 Then Exit Sub

        Dim printDoc As New PrintDocument()
        Dim halfSheetSize As New PaperSize("HalfLetter", 850, 550)
        printDoc.DefaultPageSettings.PaperSize = halfSheetSize
        printDoc.DefaultPageSettings.Landscape = False
        printDoc.DefaultPageSettings.Margins = New Margins(10, 10, 10, 10)

        AddHandler printDoc.PrintPage, New PrintPageEventHandler(AddressOf PrintPage)

        m_currentPageIndex = 0

        Try
            printDoc.Print()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Private Sub PrintPage(ByVal sender As Object, ByVal ev As PrintPageEventArgs)

        Dim pageImage As New Metafile(m_streams(m_currentPageIndex))

        ' Adjust rectangular area with printer margins.
        Dim adjustedRect As New Rectangle(ev.PageBounds.Left - CInt(ev.PageSettings.HardMarginX), ev.PageBounds.Top - CInt(ev.PageSettings.HardMarginY), ev.PageBounds.Width, ev.PageBounds.Height)

        ' Draw a white background for the report
        ev.Graphics.FillRectangle(Brushes.White, adjustedRect)

        ' Draw the report content
        ev.Graphics.DrawImage(pageImage, adjustedRect)

        ' Prepare for the next page. Make sure we haven't hit the end.
        m_currentPageIndex += 1
        ev.HasMorePages = (m_currentPageIndex < m_streams.Count)

    End Sub

    Private Function MyInputBox(ByVal Prompt As String, ByVal ButtonText As String) As String

        Dim frmInput As New Form
        frmInput.Owner = Me
        frmInput.StartPosition = FormStartPosition.CenterScreen
        frmInput.ShowIcon = False
        frmInput.Size = New Size(310, 120)
        frmInput.MinimumSize = New Size(315, 120)
        Dim btn As New Button()
        btn.Text = ButtonText
        btn.Height = 30
        frmInput.Controls.Add(btn)
        frmInput.MaximizeBox = False
        frmInput.MinimizeBox = False
        btn.Location = New Point(210, 45)
        btn.Width = 80
        AddHandler btn.Click, AddressOf inputclose
        Dim txtbox As New TextBox
        txtBoxValidation.AssignValidation(txtbox, ValidationType.Only_Numbers)

        txtbox.Width = 280
        frmInput.Controls.Add(txtbox)
        frmInput.ActiveControl = txtbox
        frmInput.AcceptButton = btn
        txtbox.TextAlign = HorizontalAlignment.Center
        txtbox.Font = New Font("Century Gothic", 12)
        txtbox.Location = New Point(10, 10)
        frmInput.Text = Prompt
        frmInput.ShowDialog()

        Return txtbox.Text
    End Function

    Sub inputclose(ByVal s As Object, ByVal e As EventArgs)

        DirectCast(DirectCast(s, Control).Parent, Form).Close()

    End Sub

    Public Sub focustoGroup(ByVal controlName As String)

        If controlName = String.Empty Then
            Exit Sub
        End If

        For Each ctrl In flowPnl_Orders.Controls

            Dim groupid = clsQueueing.getGroupID(controlName)

            If groupid = Nothing Then
                Exit Sub
            End If

            If ctrl.Name = groupid Then

                flowPnl_Orders.ScrollControlIntoView(CType(flowPnl_Orders.Controls.Find(groupid, False)(0), UCGridOrder).pnlWo)
                CType(flowPnl_Orders.Controls.Find(groupid, False)(0), UCGridOrder).prompSearch()

            End If

        Next

    End Sub

    Private Sub frmMain_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown

        If e.KeyCode = Keys.P Then

            Dim GroupID = MyInputBox("Enter GroupID to Print ...", "Print")

            If GroupID = String.Empty Then
                Exit Sub
            End If

            printItems(GroupID)

        ElseIf e.KeyCode = Keys.F Then
            Dim OrderNumber = MyInputBox("Enter Work Order Number to Find ... ", "Find")
            focustoGroup(OrderNumber)
        End If

    End Sub


    Public Enum QueueFilterStage
        All
        ForProcess
        Processing
        Prepared
        ForInvoice
        Overdue
    End Enum

    Public currentFilterStage As QueueFilterStage = QueueFilterStage.All

    Public Sub ApplyFilter(ByVal stage As QueueFilterStage)
        currentFilterStage = stage

        If flowPnl_Orders.InvokeRequired Then
            Me.Invoke(Sub() ApplyFilter(stage))
            Exit Sub
        End If

        Dim searchText As String = ""
        If txtQueueSearch IsNot Nothing Then
            searchText = txtQueueSearch.Text.Trim()
        End If

        flowPnl_Orders.SuspendLayout()
        Try
            For Each ctrl As Control In flowPnl_Orders.Controls
                If TypeOf ctrl Is UCGridOrder Then
                    Dim grid = DirectCast(ctrl, UCGridOrder)
                    Dim cColor As Color = grid.BackColor
                    Dim matchesStage As Boolean = False

                    Select Case stage
                        Case QueueFilterStage.All
                            matchesStage = True
                        Case QueueFilterStage.ForProcess
                            matchesStage = (cColor = Color.OldLace)
                        Case QueueFilterStage.Processing
                            matchesStage = (cColor = Color.DarkSalmon)
                        Case QueueFilterStage.Prepared
                            matchesStage = (cColor = Color.PaleGreen)
                        Case QueueFilterStage.ForInvoice
                            matchesStage = (cColor = Color.CornflowerBlue)
                        Case QueueFilterStage.Overdue
                            If grid.OrderTime.HasValue AndAlso cColor <> Color.CornflowerBlue Then
                                Dim elapsed As Double = (DateTime.Now - grid.OrderTime.Value).TotalMinutes
                                matchesStage = (QueueOrderAgeRules.GetPriorityLevel(elapsed) = QueuePriorityLevel.Alert)
                            Else
                                matchesStage = False
                            End If
                    End Select

                    Dim matchesSearch As Boolean = grid.MatchesSearch(searchText)
                    grid.Visible = matchesStage AndAlso matchesSearch
                End If
            Next
        Finally
            flowPnl_Orders.ResumeLayout(True)
        End Try

        UpdateTabVisuals()
    End Sub

    Public Sub ToggleFilter(ByVal targetStage As QueueFilterStage)
        If currentFilterStage = targetStage Then
            ApplyFilter(QueueFilterStage.All)
        Else
            ApplyFilter(targetStage)
        End If
        flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub UpdateTabVisuals()
        Dim panels As Panel() = {Panel2, Panel1, Panel3, Panel4, Panel5, PanelOverdue}
        Dim stages As QueueFilterStage() = {QueueFilterStage.All, QueueFilterStage.ForProcess, QueueFilterStage.Processing, QueueFilterStage.Prepared, QueueFilterStage.ForInvoice, QueueFilterStage.Overdue}

        For i As Integer = 0 To panels.Length - 1
            Dim pnl = panels(i)
            If pnl IsNot Nothing Then
                If stages(i) = currentFilterStage Then
                    pnl.BackColor = Color.FromArgb(215, 230, 250)
                    pnl.BorderStyle = BorderStyle.FixedSingle
                ElseIf stages(i) = QueueFilterStage.Overdue AndAlso m_overdueCount > 0 Then
                    pnl.BackColor = Color.FromArgb(254, 226, 226)
                    pnl.BorderStyle = BorderStyle.FixedSingle
                Else
                    pnl.BackColor = Color.Ivory
                    pnl.BorderStyle = BorderStyle.None
                End If
            End If
        Next
    End Sub

    Private Sub InitFilterTabs()
        Dim filterControls As Control() = {
            Panel2, Label1, lblTotalInQueue,
            Panel1, Label3, lblForProcess, picBoxForProcess,
            Panel3, Label6, lblProcessing, picBoxProcessing,
            Panel4, Label9, lblPrepared, picBoxPrepared,
            Panel5, Label12, lblForInvoice, picBoxForInvoice,
            PanelOverdue, LabelOverdueTitle, lblOverdue, picBoxOverdue
        }

        For Each c In filterControls
            If c IsNot Nothing Then
                c.Cursor = Cursors.Hand
            End If
        Next
        UpdateTabVisuals()
    End Sub

    Private Sub TabAll_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Panel2.Click, Label1.Click, lblTotalInQueue.Click
        ApplyFilter(QueueFilterStage.All)
        flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub TabForProcess_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Panel1.Click, Label3.Click, lblForProcess.Click, picBoxForProcess.Click
        ToggleFilter(QueueFilterStage.ForProcess)
    End Sub

    Private Sub TabProcessing_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Panel3.Click, Label6.Click, lblProcessing.Click, picBoxProcessing.Click
        ToggleFilter(QueueFilterStage.Processing)
    End Sub

    Private Sub TabPrepared_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Panel4.Click, Label9.Click, lblPrepared.Click, picBoxPrepared.Click
        ToggleFilter(QueueFilterStage.Prepared)
    End Sub

    Private Sub TabForInvoice_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Panel5.Click, Label12.Click, lblForInvoice.Click, picBoxForInvoice.Click
        ToggleFilter(QueueFilterStage.ForInvoice)
    End Sub

    Private Sub TabOverdue_Click(ByVal sender As Object, ByVal e As EventArgs) Handles PanelOverdue.Click, LabelOverdueTitle.Click, lblOverdue.Click, picBoxOverdue.Click
        ToggleFilter(QueueFilterStage.Overdue)
    End Sub

    Private Sub txtQueueSearch_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtQueueSearch.TextChanged
        If queueSearchDebounceTimer Is Nothing Then
            queueSearchDebounceTimer = New Windows.Forms.Timer()
            queueSearchDebounceTimer.Interval = 300
            AddHandler queueSearchDebounceTimer.Tick, AddressOf QueueSearchDebounceTimer_Tick
        End If
        queueSearchDebounceTimer.Stop()
        queueSearchDebounceTimer.Start()
    End Sub

    Private Sub QueueSearchDebounceTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        If queueSearchDebounceTimer IsNot Nothing Then queueSearchDebounceTimer.Stop()
        ApplyFilter(currentFilterStage)
        flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub txtQueueSearch_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtQueueSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            If queueSearchDebounceTimer IsNot Nothing Then queueSearchDebounceTimer.Stop()
            ApplyFilter(currentFilterStage)
            flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
            e.Handled = True
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            txtQueueSearch.Clear()
            ApplyFilter(currentFilterStage)
            flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub btnClearSearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClearSearch.Click
        ClearSearchFilter()
        txtQueueSearch.Focus()
    End Sub

    Private Sub btnPickerSummary_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPickerSummary.Click
        Dim dlg As New frmPickerSummary(Me)
        dlg.ShowDialog(Me)
    End Sub

    Public Function GetActivePickerTallies() As List(Of PickerTallyInfo)
        Dim pickerDict As New Dictionary(Of String, PickerTallyInfo)(StringComparer.OrdinalIgnoreCase)
        Dim pickerOrders As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase)

        For Each ctrl As Control In flowPnl_Orders.Controls
            Dim grid = TryCast(ctrl, UCGridOrder)
            If grid IsNot Nothing Then
                Dim orderID As String = If(grid.lblWO IsNot Nothing, grid.lblWO.Text, grid.Name)
                Dim items = grid.GetItemsForPickerTally()

                For Each it In items
                    Dim pName As String = it.Picker.Trim()
                    If String.IsNullOrEmpty(pName) OrElse pName = "-" Then
                        pName = "[Unassigned]"
                    End If

                    If Not pickerDict.ContainsKey(pName) Then
                        pickerDict(pName) = New PickerTallyInfo() With {.PickerName = pName}
                        pickerOrders(pName) = New HashSet(Of String)()
                    End If

                    Dim tally = pickerDict(pName)
                    tally.TotalCount += 1

                    If it.Status.Equals("In Process", StringComparison.OrdinalIgnoreCase) Then
                        tally.InProcessCount += 1
                    ElseIf it.Status.Equals("Prepared", StringComparison.OrdinalIgnoreCase) OrElse it.Status.Equals("For Invoicing", StringComparison.OrdinalIgnoreCase) Then
                        tally.PreparedCount += 1
                    End If

                    pickerOrders(pName).Add(orderID)
                Next
            End If
        Next

        For Each kvp In pickerOrders
            If pickerDict.ContainsKey(kvp.Key) Then
                pickerDict(kvp.Key).OrderCount = kvp.Value.Count
            End If
        Next

        Dim result = pickerDict.Values.OrderByDescending(Function(p) p.TotalCount).ToList()
        Return result
    End Function

    Public Sub FilterByPicker(ByVal pickerName As String)
        txtQueueSearch.Text = pickerName
        ApplyFilter(currentFilterStage)
        flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
    End Sub

    Public Sub ClearSearchFilter()
        txtQueueSearch.Clear()
        ApplyFilter(currentFilterStage)
        flowPnl_Orders.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub TabForProcess_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles Panel1.DoubleClick, Label3.DoubleClick, lblForProcess.DoubleClick, picBoxForProcess.DoubleClick
        ShowQueueListDialog("For Process", queueListForProcess)
    End Sub

    Private Sub TabProcessing_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles Panel3.DoubleClick, Label6.DoubleClick, lblProcessing.DoubleClick, picBoxProcessing.DoubleClick
        ShowQueueListDialog("Processing", queueListProcessing)
    End Sub

    Private Sub TabPrepared_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles Panel4.DoubleClick, Label9.DoubleClick, lblPrepared.DoubleClick, picBoxPrepared.DoubleClick
        ShowQueueListDialog("Prepared", queueListPrepared)
    End Sub

    Private Sub TabForInvoice_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles Panel5.DoubleClick, Label12.DoubleClick, lblForInvoice.DoubleClick, picBoxForInvoice.DoubleClick
        ShowQueueListDialog("For Invoice", queueListForInvoice)
    End Sub

    Private Sub ShowQueueListDialog(ByVal typeName As String, ByVal list As List(Of String))
        Try
            frmInQueue.lblType.Text = typeName
            frmInQueue.gridGroupID.Rows.Clear()
            For Each x In list
                frmInQueue.gridGroupID.Rows.Add(x)
            Next
            frmInQueue.ShowDialog()
        Catch ex As Exception
        End Try
    End Sub

    Private m_wheelFilter As MouseWheelMessageFilter
    Public Const ROW_STEP As Integer = 413

    Public Function IsMouseOverFlowPanel(ByVal screenPt As Point) As Boolean
        Try
            If flowPnl_Orders Is Nothing OrElse flowPnl_Orders.IsDisposed Then Return False
            Dim clientPt = flowPnl_Orders.PointToClient(screenPt)
            Return flowPnl_Orders.ClientRectangle.Contains(clientPt)
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub ScrollFlowPanelByRow(ByVal delta As Integer)
        Try
            If flowPnl_Orders Is Nothing OrElse flowPnl_Orders.IsDisposed Then Exit Sub

            Dim currentY As Integer = Math.Abs(flowPnl_Orders.AutoScrollPosition.Y)
            Dim newY As Integer

            If delta < 0 Then
                ' Scroll down by 1 row
                newY = ((currentY \ ROW_STEP) + 1) * ROW_STEP
            Else
                ' Scroll up by 1 row
                newY = Math.Max(0, ((currentY - 1) \ ROW_STEP) * ROW_STEP)
            End If

            flowPnl_Orders.AutoScrollPosition = New Point(0, newY)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub flowPnl_Orders_Scroll(ByVal sender As Object, ByVal e As ScrollEventArgs) Handles flowPnl_Orders.Scroll
        If e.Type = ScrollEventType.EndScroll Then
            Try
                Dim currentY As Integer = Math.Abs(flowPnl_Orders.AutoScrollPosition.Y)
                Dim snappedY As Integer = CInt(Math.Round(currentY / ROW_STEP)) * ROW_STEP
                flowPnl_Orders.AutoScrollPosition = New Point(0, snappedY)
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_wheelFilter IsNot Nothing Then
            Application.RemoveMessageFilter(m_wheelFilter)
            m_wheelFilter = Nothing
        End If
        If queueSearchDebounceTimer IsNot Nothing Then
            queueSearchDebounceTimer.Stop()
            queueSearchDebounceTimer.Dispose()
            queueSearchDebounceTimer = Nothing
        End If
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Cursor.Current = Cursors.WaitCursor
        Try
            GetType(FlowLayoutPanel).InvokeMember("DoubleBuffered", Reflection.BindingFlags.SetProperty Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic, Nothing, flowPnl_Orders, New Object() {True})
        Catch ex As Exception
        End Try

        flowPnl_Orders.VerticalScroll.SmallChange = ROW_STEP
        flowPnl_Orders.VerticalScroll.LargeChange = ROW_STEP

        If m_wheelFilter Is Nothing Then
            m_wheelFilter = New MouseWheelMessageFilter(Me)
            Application.AddMessageFilter(m_wheelFilter)
        End If

        InitFilterTabs()

        loadSet()

        Cursor.Current = Cursors.Default
    End Sub

    Public Sub loadSet()

        lblServer.Text = DB_Server
        lblDatabase.Text = DB_Name

        rType = My.Settings.ReleaseType
        pType = My.Settings.PickLoc

        txtReleaseType.Text = "Open Work Order : For " & rType & " | " & pType
        formLoad()
        delay.Start()

    End Sub

    Private Sub btnSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSet.Click
        frmSetup.Show()
    End Sub

    Private Sub delay_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles delay.Tick

        delay.Stop()

    End Sub

    Private Sub btnReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReload.Click

        Cursor.Current = Cursors.WaitCursor
        clearList()
        flowPnl_Orders.Controls.Clear()
        loadSet()
        Cursor.Current = Cursors.Default

    End Sub

    Private m_batchPickForm As frmBatchPick
    Private m_tvDisplayForm As frmPickupDisplay

    Private Sub btnBatchPick_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBatchPick.Click
        If m_batchPickForm Is Nothing OrElse m_batchPickForm.IsDisposed Then
            m_batchPickForm = New frmBatchPick(Me)
        End If
        m_batchPickForm.Show()
        m_batchPickForm.BringToFront()
        m_batchPickForm.LoadBatchItems()
    End Sub

    Private Sub btnTvDisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTvDisplay.Click
        If m_tvDisplayForm Is Nothing OrElse m_tvDisplayForm.IsDisposed Then
            m_tvDisplayForm = New frmPickupDisplay(Me)
        End If
        m_tvDisplayForm.Show()
        m_tvDisplayForm.BringToFront()
        m_tvDisplayForm.RefreshDisplay()
    End Sub

End Class

Friend Class MouseWheelMessageFilter
    Implements IMessageFilter

    Private Const WM_MOUSEWHEEL As Integer = &H20A
    Private m_form As frmMain

    Public Sub New(ByVal form As frmMain)
        m_form = form
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        If m.Msg = WM_MOUSEWHEEL Then
            If m_form IsNot Nothing AndAlso m_form.Visible AndAlso Not m_form.IsDisposed Then
                Dim screenPt As Point = Cursor.Position
                If m_form.IsMouseOverFlowPanel(screenPt) Then
                    Dim delta As Integer = (CType(m.WParam.ToInt64(), Int64) >> 16) And &HFFFF
                    If delta > 32767 Then delta -= 65536
                    m_form.ScrollFlowPanelByRow(delta)
                    Return True
                End If
            End If
        End If
        Return False
    End Function
End Class