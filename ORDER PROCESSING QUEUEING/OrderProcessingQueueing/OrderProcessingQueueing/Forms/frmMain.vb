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

            For Each ctrl In flowPnl_Orders.Controls

                If ctrl.name = groupid Then

                    CType(flowPnl_Orders.Controls.Find(groupid, False)(0), UCGridOrder).updateData()

                    Dim countEntry = CType(flowPnl_Orders.Controls.Find(groupid, False)(0), UCGridOrder).gridOrder.Rows.Count

                    If countEntry <= 0 Then
                        Me.flowPnl_Orders.Controls.RemoveByKey(groupid)
                    End If

                End If

            Next
        Next

    End Sub

    Public Sub prioritizeUserControl(ByVal controlName As String)

        For Each ctrl In flowPnl_Orders.Controls

            If ctrl.name = controlName Then

                CType(flowPnl_Orders.Controls.Find(controlName, False)(0), UCGridOrder).updateData() ' prioritize update
                skipUpdate = True

            End If

        Next

        countQueue()

    End Sub

    Public Sub formLoad()

        Cursor.Current = Cursors.WaitCursor
        chk_Panels.Stop()
        db_lastupdated = clsQueueing.checkTableUpdate()
        LoadOrders()
        chk_Panels.Start()
        countQueue()
        Cursor.Current = Cursors.Default

    End Sub


    Public Sub LoadOrders()

        Dim countEntry

        Dim data = clsQueueing.LoadOpenWO(rType, pType)                    ' added where clause. add only the new GroupID

        For Each d In data

            Dim _grid = New UCGridOrder

            _grid.Name = d                                          ' name the added usercontrol based on group id
            _grid.lblWO.Text = clsQueueing.getOrderIDs(d)           ' concatenate multitple Order in the Group
            _grid.lblGroupID.Text = d
            _grid.pnlWo.Name = clsQueueing.getOrderIDs(d)

            countEntry = clsQueueing.CountOrderEntry(d, pType, rType)

            If Not countEntry = 0 Then
                addToPnl(_grid)
            End If

        Next

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

                                      For Each x In groupIDList

                                          inqueue = inqueue + 1

                                          For Each ctrl In flowPnl_Orders.Controls

                                              If ctrl.name = x Then

                                                  If CType(flowPnl_Orders.Controls.Find(x, False)(0), UCGridOrder).BackColor = Color.OldLace Then
                                                      forProcess = forProcess + 1
                                                      queueListForProcess.Add(x)
                                                  End If

                                                  If CType(flowPnl_Orders.Controls.Find(x, False)(0), UCGridOrder).BackColor = Color.DarkSalmon Then
                                                      InProcess = InProcess + 1
                                                      queueListProcessing.Add(x)
                                                  End If

                                                  If CType(flowPnl_Orders.Controls.Find(x, False)(0), UCGridOrder).BackColor = Color.PaleGreen Then
                                                      Prepared = Prepared + 1
                                                      queueListPrepared.Add(x)
                                                  End If
                                                  If CType(flowPnl_Orders.Controls.Find(x, False)(0), UCGridOrder).BackColor = Color.CornflowerBlue Then
                                                      forInvoice = forInvoice + 1
                                                      queueListForInvoice.Add(x)
                                                  End If

                                              End If

                                          Next
                                      Next


                                      changeTxt(lblTotalInQueue, inqueue)
                                      changeTxt(lblForProcess, forProcess)
                                      changeTxt(lblProcessing, InProcess)
                                      changeTxt(lblPrepared, Prepared)
                                      changeTxt(lblForInvoice, forInvoice)

                                      'lblTotalInQueue.Text = inqueue
                                      'lblForProcess.Text = forProcess
                                      'lblProcessing.Text = InProcess
                                      'lblPrepared.Text = Prepared
                                      'lblForInvoice.Text = forInvoice

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

        Dim deviceInfo As String = "<DeviceInfo><OutputFormat>EMF</OutputFormat><PageWidth>11in</PageWidth><PageHeight>8.5in</PageHeight><MarginTop>0.10in</MarginTop><MarginLeft>0.10in</MarginLeft><MarginRight>0.10in</MarginRight><MarginBottom>0.10in</MarginBottom></DeviceInfo>"
        Dim warnings As Warning()
        m_streams = New List(Of Stream)()
        report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)
        For Each stream As Stream In m_streams
            stream.Position = 0
        Next

        Dim printDoc As New PrintDocument()
        '   printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF"
        Dim ps As New PrinterSettings()
        ' ps.PrinterName = printDoc.PrinterSettings.PrinterName
        ps.DefaultPageSettings.Landscape = True
        printDoc.PrinterSettings = ps

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


    Private Sub picBoxForProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picBoxForProcess.Click
        frmInQueue.lblType.Text = "For Process"
        For Each x In queueListForProcess
            frmInQueue.gridGroupID.Rows.Add(x)
        Next
        frmInQueue.ShowDialog()
    End Sub

    Private Sub picBoxProcessing_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picBoxProcessing.Click
        frmInQueue.lblType.Text = "Processing"
        For Each x In queueListProcessing
            frmInQueue.gridGroupID.Rows.Add(x)
        Next
        frmInQueue.ShowDialog()
    End Sub

    Private Sub picBoxPrepared_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picBoxPrepared.Click
        frmInQueue.lblType.Text = "Prepared"
        For Each x In queueListPrepared
            frmInQueue.gridGroupID.Rows.Add(x)
        Next
        frmInQueue.ShowDialog()
    End Sub

    Private Sub picBoxForInvoice_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picBoxForInvoice.Click
        frmInQueue.lblType.Text = "For Invoice"
        For Each x In queueListForInvoice
            frmInQueue.gridGroupID.Rows.Add(x)
        Next

        frmInQueue.ShowDialog()
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Cursor.Current = Cursors.WaitCursor
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

End Class