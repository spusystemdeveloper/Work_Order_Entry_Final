Imports System.Threading
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports Microsoft.Reporting.WinForms
Imports System.IO

Public Class UCGridOrder


    Event gridOrderPrep(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs)
    Event gridOrderPicker(ByVal sender As DataGridView, ByVal e As DataGridViewCellEventArgs)

    Public Shared ItemOrderedList As List(Of clsOrderedItems) = New List(Of clsOrderedItems)()

    Public OrderTime As Nullable(Of Date) = Nothing
    Public lblOrderAge As Label

    Public Sub New()
        InitializeComponent()
        InitOrderAgeLabel()
    End Sub

    Private Sub InitOrderAgeLabel()
        If lblOrderAge Is Nothing Then
            lblOrderAge = New Label With {
                .AutoSize = True,
                .Font = New Font("Century Gothic", 9.75!, FontStyle.Bold),
                .ForeColor = Color.White,
                .Name = "lblOrderAge",
                .Text = "",
                .Visible = QueueOrderAgeRules.EnableOrderAgeAlerts
            }
            flowLayout_Header.Controls.Add(lblOrderAge)
        End If
    End Sub

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
            If data Is Nothing Then Exit Sub

            lblWO.Text = clsQueueing.getOrderIDs(groupID)
            lblQueueID.Text = groupID.ToString()
            lblGroupID.Text = groupID.ToString()

            Dim orderIDs As New List(Of Integer)()
            For Each d In data
                If Not orderIDs.Contains(d.OrderID) Then
                    orderIDs.Add(d.OrderID)
                End If
            Next

            Dim custName As String = clsQueueing.getCustomerName(groupID)
            If String.IsNullOrEmpty(custName) AndAlso orderIDs.Count > 0 Then
                custName = clsQueueing.getCustomerName(orderIDs(0))
            End If
            lblCust.Text = custName

            Dim qtyDict = clsQueueing.GetBatchQtyOrd(orderIDs)

            For Each d In data

                Dim rowId As Integer = gridOrder.Rows.Add()
                Dim row As DataGridViewRow = gridOrder.Rows(rowId)

                If d.Status = "Prepared" Or d.Status = "For Invoicing" Then
                    row.Cells(0).Value = True
                End If

                row.Cells(1).Value = d.ItemCode
                row.Cells(2).Value = d.Description

                Dim key As String = d.OrderID.ToString() & "_" & d.ItemID.ToString()
                Dim qtyVal As Integer = 0
                If Not qtyDict.TryGetValue(key, qtyVal) Then
                    qtyVal = 0
                End If
                row.Cells(3).Value = qtyVal
                row.Cells(4).Value = d.QtyPre
                row.Cells(5).Value = d.Picker
                row.Cells(6).Value = d.Status
                row.Cells(7).Value = d.ItemID
                row.Cells(8).Value = d.OrderId
                row.Cells(9).Value = d.GroupTo
                row.Cells(10).Value = d.QueueingID

                If d.Status = "Shortage" Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199)
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9)
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(253, 230, 138)
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(146, 64, 14)
                ElseIf d.Status = "Check Stock" Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(224, 242, 254)
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(3, 105, 161)
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(186, 230, 253)
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(3, 105, 161)
                End If

                lblOPIS.Text = d.OPIS

                Try
                    If d.OrderTime IsNot Nothing Then
                        Me.OrderTime = Convert.ToDateTime(d.OrderTime)
                    End If
                Catch ex As Exception
                End Try

            Next

            '   addFooter()
            'gridOrder.Refresh()

        Catch ex As Exception
            MessageBox.Show("FROM : UCGridOrder UserControl " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Public Function MatchesSearch(ByVal searchText As String) As Boolean
        If String.IsNullOrEmpty(searchText) Then Return True
        Dim query As String = searchText.Trim().ToLower()

        If lblWO IsNot Nothing AndAlso lblWO.Text.ToLower().Contains(query) Then Return True
        If lblCust IsNot Nothing AndAlso lblCust.Text.ToLower().Contains(query) Then Return True
        If lblGroupID IsNot Nothing AndAlso lblGroupID.Text.ToLower().Contains(query) Then Return True

        If gridOrder IsNot Nothing Then
            For i As Integer = 0 To gridOrder.Rows.Count - 1
                Dim row = gridOrder.Rows(i)
                Dim code = If(row.Cells(1).Value, "").ToString().ToLower()
                Dim desc = If(row.Cells(2).Value, "").ToString().ToLower()
                Dim picker = If(row.Cells(5).Value, "").ToString().ToLower()
                If code.Contains(query) OrElse desc.Contains(query) OrElse picker.Contains(query) Then Return True
            Next
        End If

        Return False
    End Function

    Public Structure PickerItemDetail
        Public ItemCode As String
        Public Description As String
        Public Status As String
        Public Picker As String
    End Structure

    Public Function GetItemsForPickerTally() As List(Of PickerItemDetail)
        Dim list As New List(Of PickerItemDetail)()
        If gridOrder Is Nothing Then Return list

        For i As Integer = 0 To gridOrder.Rows.Count - 1
            Dim row = gridOrder.Rows(i)
            Dim itemCode = If(row.Cells(1).Value, "").ToString().Trim()
            Dim desc = If(row.Cells(2).Value, "").ToString().Trim()
            Dim picker = If(row.Cells(5).Value, "").ToString().Trim()

            ' Skip the action footer row ("Set all as Prepared" / "Prep All" / "Pick All")
            If itemCode.Equals("Set all as Prepared", StringComparison.OrdinalIgnoreCase) OrElse
               desc.Equals("Set all as Prepared", StringComparison.OrdinalIgnoreCase) OrElse
               picker.Equals("Pick All", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Dim detail As New PickerItemDetail()
            detail.ItemCode = itemCode
            detail.Description = desc
            detail.Picker = picker
            detail.Status = If(row.Cells(6).Value, "").ToString().Trim()
            list.Add(detail)
        Next
        Return list
    End Function

    Private Sub removeFooter()
        For i As Integer = gridOrder.Rows.Count - 1 To 0 Step -1
            Dim desc = If(gridOrder.Rows(i).Cells(1).Value, "").ToString()
            If desc = "Set all as Prepared" Then
                gridOrder.Rows.RemoveAt(i)
            End If
        Next
    End Sub

    Private Sub addFooter()

        removeFooter()

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
            If e.RowIndex < 0 OrElse e.RowIndex >= gridOrder.Rows.Count Then Exit Sub

            Dim row As DataGridViewRow = gridOrder.Rows(e.RowIndex)
            Dim pickVal = row.Cells(5).Value
            Dim pickerStr As String = If(pickVal IsNot Nothing, pickVal.ToString().Trim(), "")

            If String.IsNullOrEmpty(pickerStr) Then
                MessageBox.Show("Please Select a Picker First!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                Dim qVal = row.Cells(10).Value
                Dim itemVal = row.Cells(7).Value
                Dim codeVal = row.Cells(1).Value
                Dim descVal = row.Cells(2).Value
                Dim qtyVal = row.Cells(3).Value

                frmQtyPrep.lblItemCode.Text = If(codeVal IsNot Nothing, codeVal.ToString(), "")
                frmQtyPrep.lblDesc.Text = If(descVal IsNot Nothing, descVal.ToString(), "")

                frmQtyPrep._queueid = If(qVal IsNot Nothing AndAlso IsNumeric(qVal), Convert.ToInt32(qVal), 0)
                frmQtyPrep._itemid = If(itemVal IsNot Nothing AndAlso IsNumeric(itemVal), Convert.ToInt32(itemVal), 0)
                frmQtyPrep._itemcode = If(codeVal IsNot Nothing, codeVal.ToString(), "")
                frmQtyPrep._qtyOrd = If(qtyVal IsNot Nothing AndAlso IsNumeric(qtyVal), Convert.ToInt32(qtyVal), 0)
                frmQtyPrep._status = If(row.Cells(6).Value IsNot Nothing, row.Cells(6).Value.ToString(), "")

                frmQtyPrep.lblGroupID.Text = If(lblGroupID IsNot Nothing, lblGroupID.Text, "")
                frmQtyPrep.lblQid.Text = If(qVal IsNot Nothing, qVal.ToString(), "")
                frmQtyPrep.lblItemId.Text = If(itemVal IsNot Nothing, itemVal.ToString(), "")
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
            If e.RowIndex < 0 OrElse e.RowIndex >= gridOrder.Rows.Count Then Exit Sub

            Dim row As DataGridViewRow = gridOrder.Rows(e.RowIndex)
            If row Is Nothing Then Exit Sub

            frmPicker.lblGroupID.Text = If(lblGroupID IsNot Nothing, lblGroupID.Text, "")

            If frmPicker._updateType = "All" Then
                frmPicker.lblQueueID.Text = If(lblQueueID IsNot Nothing, lblQueueID.Text, "")
                frmPicker.lblItemCode.Text = "---------"
                frmPicker.lblDesc.Text = "---------"
                frmPicker._Lastpicker = ""
                frmPicker._queueid = 0
                frmPicker._itemid = 0
                frmPicker._itemcode = ""
                frmPicker._picker = ""
            Else
                frmPicker._updateType = "Single"

                Dim qVal = If(row.Cells.Count > 10, row.Cells(10).Value, Nothing)
                Dim itemVal = If(row.Cells.Count > 7, row.Cells(7).Value, Nothing)
                Dim codeVal = If(row.Cells.Count > 1, row.Cells(1).Value, Nothing)
                Dim descVal = If(row.Cells.Count > 2, row.Cells(2).Value, Nothing)
                Dim pickVal = If(row.Cells.Count > 5, row.Cells(5).Value, Nothing)

                frmPicker.lblQueueID.Text = If(qVal IsNot Nothing, qVal.ToString(), "")
                frmPicker.lblItemCode.Text = If(codeVal IsNot Nothing, codeVal.ToString(), "")
                frmPicker.lblDesc.Text = If(descVal IsNot Nothing, descVal.ToString(), "")

                frmPicker._Lastpicker = If(pickVal IsNot Nothing, pickVal.ToString(), "")
                frmPicker._queueid = If(qVal IsNot Nothing AndAlso IsNumeric(qVal), Convert.ToInt32(qVal), 0)
                frmPicker._itemid = If(itemVal IsNot Nothing AndAlso IsNumeric(itemVal), Convert.ToInt32(itemVal), 0)
                frmPicker._itemcode = If(codeVal IsNot Nothing, codeVal.ToString(), "")
                frmPicker._picker = If(pickVal IsNot Nothing, pickVal.ToString(), "")
            End If

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
            Dim statPrepared = 0
            Dim statForInvoice = 0
            Dim statProcess = 0
            Dim statShortage = 0
            Dim itemRowCount = 0

            For Each x As DataGridViewRow In gridOrder.Rows

                Dim desc = If(x.Cells(1).Value, "").ToString()
                If desc = "Set all as Prepared" Then
                    Continue For
                End If

                itemRowCount += 1

                Dim itemStat As String = If(x.Cells(6).Value, "").ToString()
                Select Case itemStat
                    Case "Prepared"
                        statPrepared = statPrepared + 1
                    Case "For Invoicing"
                        statForInvoice = statForInvoice + 1
                    Case "Processing"
                        Me.BackColor = Color.DarkSalmon
                    Case "Shortage"
                        statShortage = statShortage + 1
                        Me.BackColor = Color.DarkSalmon
                End Select

            Next

            If itemRowCount > 0 AndAlso itemRowCount = statPrepared Then
                Me.BackColor = Color.PaleGreen
                Me.gridOrder.Enabled = False
                If lblOrderAge IsNot Nothing Then
                    lblOrderAge.Text = " | Prepared"
                End If
                flowLayout_Header.BackColor = Color.FromArgb(46, 125, 50)
                removeFooter()
            ElseIf itemRowCount > 0 AndAlso (itemRowCount = statForInvoice OrElse (statForInvoice > 0 AndAlso (statPrepared + statForInvoice) = itemRowCount)) Then
                Me.BackColor = Color.CornflowerBlue
                Me.gridOrder.Enabled = False
                If lblOrderAge IsNot Nothing Then
                    lblOrderAge.Text = " | For Invoicing"
                End If
                flowLayout_Header.BackColor = Color.FromArgb(30, 58, 138)
                removeFooter()
            Else
                If statShortage > 0 Then
                    If lblOrderAge IsNot Nothing Then
                        lblOrderAge.Visible = True
                        lblOrderAge.Text = " | ⚠️ SHORTAGE (" & statShortage & ")"
                    End If
                    flowLayout_Header.BackColor = Color.FromArgb(217, 119, 6)
                Else
                    ApplyOrderAgeHighlight()
                End If
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

    Public Sub ApplyOrderAgeHighlight()
        Try
            If lblOrderAge Is Nothing Then InitOrderAgeLabel()

            If Not QueueOrderAgeRules.EnableOrderAgeAlerts Then
                flowLayout_Header.BackColor = SystemColors.ControlDarkDark
                If lblOrderAge IsNot Nothing Then lblOrderAge.Visible = False
                Exit Sub
            End If

            If lblOrderAge IsNot Nothing Then
                lblOrderAge.Visible = True
                If OrderTime.HasValue Then
                    Dim elapsedMinutes As Double = (DateTime.Now - OrderTime.Value).TotalMinutes
                    If elapsedMinutes < 0 Then elapsedMinutes = 0
                    Dim priority As QueuePriorityLevel = QueueOrderAgeRules.GetPriorityLevel(elapsedMinutes)
                    flowLayout_Header.BackColor = QueueOrderAgeRules.GetHeaderColor(priority, True)
                    lblOrderAge.Text = " | " & QueueOrderAgeRules.GetStatusBadgeText(priority, elapsedMinutes)
                Else
                    flowLayout_Header.BackColor = SystemColors.ControlDarkDark
                    lblOrderAge.Text = ""
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Sub RefreshOrderAge()
        If Me.BackColor = Color.PaleGreen Then
            If lblOrderAge IsNot Nothing Then lblOrderAge.Text = " | Prepared"
        ElseIf Me.BackColor = Color.CornflowerBlue Then
            If lblOrderAge IsNot Nothing Then lblOrderAge.Text = " | For Invoicing"
        Else
            Dim shortageCount As Integer = 0
            For Each row As DataGridViewRow In gridOrder.Rows
                If If(row.Cells(6).Value, "").ToString() = "Shortage" Then
                    shortageCount += 1
                End If
            Next
            If shortageCount > 0 Then
                If lblOrderAge IsNot Nothing Then
                    lblOrderAge.Visible = True
                    lblOrderAge.Text = " | ⚠️ SHORTAGE (" & shortageCount & ")"
                End If
                flowLayout_Header.BackColor = Color.FromArgb(217, 119, 6)
            Else
                ApplyOrderAgeHighlight()
            End If
        End If
    End Sub

    Private Sub UCGridOrder_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            Me.DoubleBuffered = True
            lblGroupID.Visible = False
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
            If e.RowIndex < 0 OrElse e.RowIndex >= gridOrder.Rows.Count Then Exit Sub

            Dim senderGrid = DirectCast(sender, DataGridView)
            Dim res As Integer
            Dim clickedRow As DataGridViewRow = gridOrder.Rows(e.RowIndex)

            If TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso e.ColumnIndex = 4 Then

                Dim isFooterPrep As Boolean = (e.RowIndex = gridOrder.Rows.Count - 1) AndAlso (clickedRow.Cells(4).Value IsNot Nothing AndAlso clickedRow.Cells(4).Value.ToString() = "Prep All")

                If isFooterPrep Then

                    res = MsgBox("Are you sure that all the Items Ordered and you picked is equal?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        Cursor.Current = Cursors.WaitCursor
                        Try
                            Dim batchUpdates As New List(Of Tuple(Of Long, Integer, Integer))()
                            For Each x As DataGridViewRow In gridOrder.Rows
                                If x.Index <> gridOrder.Rows.Count - 1 Then
                                    Dim qVal = x.Cells(10).Value
                                    Dim itmVal = x.Cells(7).Value
                                    Dim qtyVal = x.Cells(3).Value
                                    If qVal IsNot Nothing AndAlso itmVal IsNot Nothing AndAlso qtyVal IsNot Nothing AndAlso IsNumeric(qVal) AndAlso IsNumeric(itmVal) AndAlso IsNumeric(qtyVal) Then
                                        batchUpdates.Add(Tuple.Create(Convert.ToInt64(qVal), Convert.ToInt32(itmVal), Convert.ToInt32(qtyVal)))
                                        x.Cells(4).Value = qtyVal
                                    End If
                                End If
                            Next

                            If batchUpdates.Count > 0 Then
                                clsQueueing.UpdateBatchQtyPrep(batchUpdates)
                            End If

                            checkStatus()
                            frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                            frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl
                        Finally
                            Cursor.Current = Cursors.Default
                        End Try
                        Exit Sub
                    Else
                        Exit Sub
                    End If

                End If

                RaiseEvent gridOrderPrep(senderGrid, e)

            ElseIf TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso e.ColumnIndex = 5 Then

                Dim isFooterPicker As Boolean = (clickedRow.Cells(5).Value IsNot Nothing AndAlso clickedRow.Cells(5).Value.ToString() = "Pick All")

                If isFooterPicker Then

                    res = MsgBox("Are you sure to set a Picker on All the items?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        frmPicker._updateType = "All"
                        frmPicker._data.Reset()
                        frmPicker._data.Columns.Add("queueid", GetType(Integer))
                        frmPicker._data.Columns.Add("itemid", GetType(Integer))
                        frmPicker._data.Columns.Add("picker", GetType(String))

                        For Each x As DataGridViewRow In gridOrder.Rows
                            Dim desc = If(x.Cells(1).Value, "").ToString()
                            If desc <> "Set all as Prepared" Then
                                Dim qVal = x.Cells(10).Value
                                Dim itmVal = x.Cells(7).Value
                                Dim pickVal = x.Cells(5).Value
                                Dim dr As DataRow = frmPicker._data.NewRow()
                                dr("queueid") = If(qVal IsNot Nothing AndAlso IsNumeric(qVal), Convert.ToInt32(qVal), 0)
                                dr("itemid") = If(itmVal IsNot Nothing AndAlso IsNumeric(itmVal), Convert.ToInt32(itmVal), 0)
                                dr("picker") = If(pickVal IsNot Nothing, pickVal.ToString(), "")
                                frmPicker._data.Rows.Add(dr)
                            End If
                        Next
                    Else
                        Exit Sub
                    End If
                Else
                    frmPicker._updateType = "Single"
                End If

                RaiseEvent gridOrderPicker(senderGrid, e)

            ElseIf e.ColumnIndex = 0 Then

                Dim isFooterCheck As Boolean = (e.RowIndex = gridOrder.Rows.Count - 1) AndAlso (clickedRow.Cells(1).Value IsNot Nothing AndAlso clickedRow.Cells(1).Value.ToString() = "Set all as Prepared")

                If isFooterCheck Then

                    res = MsgBox("Are you sure to set as Prepared All the items?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                    If res = MsgBoxResult.Yes Then

                        Cursor.Current = Cursors.WaitCursor
                        Try
                            Dim batchUpdates As New List(Of Tuple(Of Long, Integer))()
                            For Each x As DataGridViewRow In gridOrder.Rows
                                If x.Index <> gridOrder.Rows.Count - 1 Then
                                    Dim statusVal As String = If(x.Cells(6).Value IsNot Nothing, x.Cells(6).Value.ToString(), "")
                                    If statusVal = "-" Then
                                        MessageBox.Show("Some Item are not yet Prepared !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                                        Exit Sub
                                    Else
                                        Dim qtyOrd = If(x.Cells(3).Value IsNot Nothing AndAlso IsNumeric(x.Cells(3).Value), Convert.ToInt64(x.Cells(3).Value), 0L)
                                        Dim qtyPre = If(x.Cells(4).Value IsNot Nothing AndAlso IsNumeric(x.Cells(4).Value), Convert.ToInt64(x.Cells(4).Value), -1L)
                                        If qtyOrd = qtyPre Then
                                            Dim qVal = x.Cells(10).Value
                                            Dim itmVal = x.Cells(7).Value
                                            If qVal IsNot Nothing AndAlso itmVal IsNot Nothing AndAlso IsNumeric(qVal) AndAlso IsNumeric(itmVal) Then
                                                batchUpdates.Add(Tuple.Create(Convert.ToInt64(qVal), Convert.ToInt32(itmVal)))
                                                x.Cells(0).Value = True
                                                x.Cells(6).Value = "Prepared"
                                            End If
                                        Else
                                            MessageBox.Show("Some item Quantity Ordered and Quantity Prepared are not Equal !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Exit Sub
                                        End If
                                    End If
                                End If
                            Next

                            If batchUpdates.Count > 0 Then
                                clsQueueing.UpdateBatchStatus(batchUpdates, "Prepared")
                            End If

                            checkStatus()
                            frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                            frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl
                        Finally
                            Cursor.Current = Cursors.Default
                        End Try

                    End If

                Else
                    Dim statusVal As String = If(clickedRow.Cells(6).Value IsNot Nothing, clickedRow.Cells(6).Value.ToString(), "")
                    If statusVal = "-" Then

                        MessageBox.Show("Item is not yet Prepared !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                    ElseIf statusVal = "For Invoicing" OrElse statusVal = "Prepared" Then

                        MessageBox.Show("Cannot be Uncheked if Already Prepared or Already For Invoicing!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit Sub

                    Else
                        res = MsgBox("Set as Prepared ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                        If res = MsgBoxResult.Yes Then
                            Dim qtyOrd = If(clickedRow.Cells(3).Value IsNot Nothing AndAlso IsNumeric(clickedRow.Cells(3).Value), Convert.ToInt64(clickedRow.Cells(3).Value), 0L)
                            Dim qtyPre = If(clickedRow.Cells(4).Value IsNot Nothing AndAlso IsNumeric(clickedRow.Cells(4).Value), Convert.ToInt64(clickedRow.Cells(4).Value), -1L)
                            If qtyOrd = qtyPre Then
                                Cursor.Current = Cursors.WaitCursor
                                Try
                                    clickedRow.Cells(0).Value = True
                                    clickedRow.Cells(6).Value = "Prepared"
                                    Dim qVal = clickedRow.Cells(10).Value
                                    Dim itmVal = clickedRow.Cells(7).Value
                                    If qVal IsNot Nothing AndAlso itmVal IsNot Nothing AndAlso IsNumeric(qVal) AndAlso IsNumeric(itmVal) Then
                                        clsQueueing.UpdateStatus(Convert.ToInt64(qVal), Convert.ToInt64(itmVal), "Prepared")
                                    End If

                                    checkStatus()
                                    frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                                    frmMain.skipUpdate = True                                        ' skip update because it was already updated in the prioritizeUserControl
                                Finally
                                    Cursor.Current = Cursors.Default
                                End Try

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

End Class
