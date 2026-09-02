Imports System.Threading
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports Microsoft.Reporting.WinForms
Imports System.IO

Public Class UCGridOrder


    Event gridOrderPrep(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs)
    Event gridOrderPicker(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs)

    Public Shared ItemOrderedList As List(Of clsOrderedItems) = New List(Of clsOrderedItems)()

    Public Sub updateData()

        Try
            LoadOrders(lblGroupID.Text)
            checkStatus()
        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Public Sub LoadOrders(ByVal groupID As Integer)

        Try
            Dim picLoc = My.Settings.PickLoc
            Dim rType = My.Settings.ReleaseType

            Dim data = clsQueueing.LoadOrders(groupID, picLoc, rType)
            gridOrder.Rows.Clear()

            For Each d In data

                Dim rowId As Integer = gridOrder.Rows.Add()
                Dim row As DataGridViewRow = gridOrder.Rows(rowId)

                If d.Status = "Prepared" Or d.Status = "For Invoicing" Then
                    row.Cells(0).Value = True
                End If

                row.Cells(1).Value = d.ItemCode
                row.Cells(2).Value = d.Description
                row.Cells(3).Value = clsQueueing.GetQtyOrd(d.OrderID, d.ItemID)
                row.Cells(4).Value = d.QtyPre
                row.Cells(5).Value = d.Picker
                row.Cells(6).Value = d.Status
                row.Cells(7).Value = d.ItemID
                row.Cells(8).Value = d.OrderId
                row.Cells(9).Value = d.GroupTo
                row.Cells(10).Value = d.QueueingID

                lblCust.Text = clsQueueing.getCustomerName(d.GroupTo)
                lblOPIS.Text = d.OPIS
                lblQueueID.Text = d.GroupTo
                lblWO.Text = clsQueueing.getOrderIDs(d.GroupTo)
                lblGroupID.Text = d.GroupTo

            Next

            '   addFooter()
            'gridOrder.Refresh()

        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub addFooter()

        Dim x As Integer = gridOrder.Rows.Add()
        Dim y As DataGridViewRow = gridOrder.Rows(x)
        y.Cells(0).Value = False
        y.Cells(1).Value = "Set all as Prepared"
        y.Cells(4).Value = "Prep All"
        y.Cells(5).Value = "Pick All"

        gridOrder.Rows(gridOrder.Rows.Count - 1).DefaultCellStyle.BackColor = Panel1.BackColor
        gridOrder.Rows(gridOrder.Rows.Count - 1).DefaultCellStyle.Font = New Font("Century Gothic", 8, FontStyle.Bold)
        gridOrder.Rows(gridOrder.Rows.Count - 1).DefaultCellStyle.ForeColor = Color.White
        gridOrder.Rows(gridOrder.Rows.Count - 1).DefaultCellStyle.SelectionBackColor = Panel1.BackColor
        gridOrder.Rows(gridOrder.Rows.Count - 1).DefaultCellStyle.SelectionForeColor = Color.White
        gridOrder.Rows(gridOrder.Rows.Count - 1).Height = 40

    End Sub

    Private Sub gridOrder_PrepButtonClick(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs) Handles Me.gridOrderPrep

        Try
            If gridOrder.Item(5, gridOrder.CurrentRow.Index).Value = "" Then
                MessageBox.Show("Please Select a Picker First!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                frmQtyPrep.lblItemCode.Text = gridOrder.Item(1, gridOrder.CurrentRow.Index).Value
                frmQtyPrep.lblDesc.Text = gridOrder.Item(2, gridOrder.CurrentRow.Index).Value

                frmQtyPrep._queueid = gridOrder.Item(10, gridOrder.CurrentRow.Index).Value
                frmQtyPrep._itemid = gridOrder.Item(7, gridOrder.CurrentRow.Index).Value
                frmQtyPrep._itemcode = gridOrder.Item(1, gridOrder.CurrentRow.Index).Value
                frmQtyPrep._qtyOrd = gridOrder.Item(3, gridOrder.CurrentRow.Index).Value

                frmQtyPrep.lblGroupID.Text = lblGroupID.Text
                frmQtyPrep.lblQid.Text = gridOrder.Item(10, gridOrder.CurrentRow.Index).Value
                frmQtyPrep.lblItemId.Text = gridOrder.Item(7, gridOrder.CurrentRow.Index).Value
                frmQtyPrep.ShowDialog()

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub gridOrder_PickerButtonClick(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs) Handles Me.gridOrderPicker

        Try
            frmPicker.lblGroupID.Text = lblGroupID.Text
            frmPicker.lblQueueID.Text = gridOrder.Item(10, gridOrder.CurrentRow.Index).Value
            frmPicker.lblItemCode.Text = gridOrder.Item(1, gridOrder.CurrentRow.Index).Value
            frmPicker.lblDesc.Text = gridOrder.Item(2, gridOrder.CurrentRow.Index).Value
            frmPicker._Lastpicker = gridOrder.Item(5, gridOrder.CurrentRow.Index).Value
            frmPicker._queueid = gridOrder.Item(10, gridOrder.CurrentRow.Index).Value
            frmPicker._itemid = gridOrder.Item(7, gridOrder.CurrentRow.Index).Value
            frmPicker._itemcode = gridOrder.Item(1, gridOrder.CurrentRow.Index).Value
            frmPicker._picker = gridOrder.CurrentCell.Value
            frmPicker.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub checkForErrors()

        If ErrorCount > 5 Then
            Application.Restart()
        End If

    End Sub

    Public Sub checkStatus()

        Try
            Dim rowCount = gridOrder.Rows.Count()
            Dim statPrepared = 0
            Dim statForInvoice = 0
            Dim statProcess = 0

            For Each x As DataGridViewRow In gridOrder.Rows

                Select Case x.Cells(6).Value

                    Case "Prepared"
                        statPrepared = statPrepared + 1
                    Case "For Invoicing"
                        statForInvoice = statForInvoice + 1
                    Case "Processing"
                        Me.BackColor = Color.DarkSalmon
                End Select

            Next

            If rowCount = statPrepared Then
                Me.BackColor = Color.PaleGreen
                Me.gridOrder.Enabled = False
                '     gridOrder.Rows.Remove(gridOrder.Rows(gridOrder.Rows.Count - 1))
            ElseIf rowCount = statForInvoice Then
                Me.BackColor = Color.CornflowerBlue
                Me.gridOrder.Enabled = False
                '   gridOrder.Rows.Remove(gridOrder.Rows(gridOrder.Rows.Count - 1))
            End If

            If Me.BackColor = Color.DarkSalmon Or Me.BackColor = Color.OldLace Then
                addFooter()
            End If


        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub UCGridOrder_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            Me.DoubleBuffered = True
            LoadOrders(lblGroupID.Text)
            checkStatus()

        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub gridOrder_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridOrder.CellContentClick

        Try
            Dim senderGrid = DirectCast(sender, DataGridView)
            Dim res As Integer

            If TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso e.ColumnIndex = 4 AndAlso e.RowIndex >= 0 Then

                If e.RowIndex = gridOrder.Rows.Count - 1 Then

                    res = MsgBox("Are you sure that all the Items Ordered and you picked is equal?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        For Each x As DataGridViewRow In gridOrder.Rows

                            If Not x.Index = gridOrder.Rows.Count - 1 Then
                                clsQueueing.UpdateQtyPrep(Convert.ToInt64(x.Cells(10).Value), Convert.ToInt64(x.Cells(7).Value), Convert.ToInt64(x.Cells(3).Value))
                            End If

                        Next

                        frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                        frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl
                        Exit Sub
                    Else
                        Exit Sub
                    End If

                End If

                RaiseEvent gridOrderPrep(senderGrid, e)

            ElseIf TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso e.ColumnIndex = 5 AndAlso e.RowIndex >= 0 Then

                If e.RowIndex = gridOrder.Rows.Count - 1 Then

                    res = MsgBox("Are you sure to set a Picker on All the items?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        frmPicker._updateType = "All"
                        frmPicker._data.Reset()
                        frmPicker._data.Columns.Add("queueid", GetType(Integer))
                        frmPicker._data.Columns.Add("itemid", GetType(Integer))
                        frmPicker._data.Columns.Add("picker", GetType(String))

                        For Each x As DataGridViewRow In gridOrder.Rows

                            Dim dr As DataRow = frmPicker._data.NewRow()
                            dr("queueid") = Convert.ToInt64(x.Cells(10).Value)
                            dr("itemid") = Convert.ToInt64(x.Cells(7).Value)
                            dr("picker") = x.Cells(5).Value.ToString()
                            frmPicker._data.Rows.Add(dr)

                        Next
                    Else
                        Exit Sub
                    End If

                End If

                RaiseEvent gridOrderPicker(senderGrid, e)

            ElseIf e.ColumnIndex = 0 AndAlso e.RowIndex >= 0 Then

                If e.RowIndex = gridOrder.Rows.Count - 1 Then

                    res = MsgBox("Are you sure to set as Prepared All the items?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        For Each x As DataGridViewRow In gridOrder.Rows

                            If x.Cells(6).Value = "-" Then

                                MessageBox.Show("Some Item are not yet Prepared !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                Exit Sub

                            Else
                                If x.Cells(3).Value = x.Cells(4).Value Then
                                    If Not x.Index = gridOrder.Rows.Count - 1 Then
                                        clsQueueing.UpdateStatus(Convert.ToInt64(x.Cells(10).Value), Convert.ToInt64(x.Cells(7).Value), "Prepared")
                                    End If
                                Else
                                    If Not x.Index = gridOrder.Rows.Count - 1 Then
                                        MessageBox.Show("Some item Quantity Ordered and Quantity Prepared are not Equal !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Exit Sub
                                    End If
                                End If

                            End If
                        Next

                        frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                        frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl

                    End If

                Else
                    If gridOrder.Item(6, gridOrder.CurrentRow.Index).Value = "-" Then

                        MessageBox.Show("Item is not yet Prepared !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                    ElseIf gridOrder.Item(6, gridOrder.CurrentRow.Index).Value = "For Invoicing" Or gridOrder.Item(6, gridOrder.CurrentRow.Index).Value = "Prepared" Then

                        MessageBox.Show("Cannot be Uncheked if Already Prepared or Already For Invoicing!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit Sub

                    Else
                        res = MsgBox("Set as Prepared ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                        If res = MsgBoxResult.Yes Then
                            If Convert.ToInt64(gridOrder.Item(3, gridOrder.CurrentRow.Index).Value) = Convert.ToInt64(gridOrder.Item(4, gridOrder.CurrentRow.Index).Value) Then
                                gridOrder.Rows(e.RowIndex).Cells(0).Value = True
                                clsQueueing.UpdateStatus(Convert.ToInt64(gridOrder.Item(10, gridOrder.CurrentRow.Index).Value), Convert.ToInt64(gridOrder.Item(7, gridOrder.CurrentRow.Index).Value), "Prepared")

                                frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                                frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl

                            Else
                                MessageBox.Show("Quantity Ordered and Quantity Prepared not Equal !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            End If

                        End If

                    End If
                End If


            End If
        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Dim oldColor

    Public Sub prompSearch()

        Try
            oldColor = Me.BackColor
            Me.BackColor = Color.Plum
            Panel1.Visible = True
            gridOrder.Enabled = False
        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click

        Try
            Panel1.Visible = False
            gridOrder.Enabled = True
            Me.BackColor = oldColor
        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Private Sub btnReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReload.Click
        updateData()
    End Sub

    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click
        printItems(lblGroupID.Text)
    End Sub

    Public Sub printItems(ByVal _GroupID As Integer)


        Dim rType = My.Settings.ReleaseType
        Dim pType = My.Settings.PickLoc

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

End Class
