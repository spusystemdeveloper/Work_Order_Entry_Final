Imports System.IO
Imports System.Threading
Imports Microsoft.Reporting.WinForms


Public Class frmItemLookUp

    Dim record As Object
    Dim sOldDes As String
    Dim dQty As Double
    Dim iItemType As Integer
    Dim iErrCnt As Integer
    Dim curQty As Integer
    Dim sfilterTxt As String
    Dim curPrice As Double
    Dim bFlag As Boolean = True
    Dim delitems As New List(Of Integer)
    Dim itemData As New BindingSource
    Dim lastFilterTxt As String
    Dim FilterStr

    Public Shared Thread_1 As Thread
    Public Shared Thread_2 As Thread

    Public Shared usrRegister As String
    Public Shared usrFullname As String
    Public Shared usrUsername As String

    Dim updateSuccess As Boolean = True

    Public db_lastupdate

    Private Sub txtSearch_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown

        Try

            If e.KeyCode = Keys.Down Then
                gridItem.Focus()
            ElseIf e.KeyCode = Keys.F9 Then
                txtSearch.Text = sOldDes
                txtSearch.SelectionStart = txtSearch.TextLength
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub txtSearch_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSearch.KeyPress

        Try

            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then

                If txtSearch.Text = String.Empty Then
                    MessageBox.Show("Search cannot be empty !", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Else

                    FilterText()
                    txtSearch.Text = String.Empty
                    SearchItem()
                End If

                txtSearch.Text = String.Empty

            End If

            If Me.txtSearch.Focused = False Then
                txtSearch.Focus()
                txtSearch.Text = e.KeyChar.ToString
                txtSearch.SelectionStart = txtSearch.Text.Length
                e.Handled = True
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub FilterText()

        Try

            Dim nodes As TreeNodeCollection = TreeView1.Nodes
            Dim root = New TreeNode(txtSearch.Text)
            root.Name = txtSearch.Text

            Dim node As TreeNodeCollection = TreeView1.Nodes
            Dim txt As String = String.Empty

            If txtSearch.Text <> String.Empty Then
                TreeView1.Nodes.Add(root)
            End If

            For Each n As TreeNode In TreeView1.Nodes
                txt = txt + n.Text + ","
                lastFilterTxt = n.Name
            Next

            Try
                sfilterTxt = txt.Substring(0, txt.Length - 1)
            Catch ex As Exception

            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub
    Private Sub SearchItem()

        Try

            Dim SearchStrArr() As String = Split(sfilterTxt, ",")
            Dim FilterString As String = ""
            FilterString = String.Join("%' AND [FullDesc] Like '%", SearchStrArr)
            Dim FilterStr = "[FullDesc] Like '%" & FilterString & "%'"
            itemData.Filter = FilterStr
            checkSearch()

            iRow = 0
            txtSearch.Focus()
            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")


        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub checkSearch()

        Try

            If gridItem.RowCount = 0 Then

                MessageBox.Show("No Items Found !", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Error)


                Try
                    Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                    TreeView1.Nodes.Remove(node(0))
                    FilterText()

                    If TreeView1.Nodes.Count = 0 Then
                        itemData.Filter = Nothing
                    Else
                        SearchItem()
                    End If

                Catch ex As Exception

                End Try

                Me.ActiveControl = txtSearch

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub GridColumnWidth()

        Try

            With gridItem
                .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                '.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnsMode.Fill
                .Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(7).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells


                .Columns(0).Frozen = True
                .Columns(1).MinimumWidth = 450
                'for column width
                '.Columns(0).Width = 100
                '.Columns(1).Width = 400
                '.Columns(2).Width = 90
                '.Columns(3).Width = 0
                '.Columns(4).Width = 90
                '.Columns(5).Width = 100
                'for column name
                .Columns(0).DefaultCellStyle.Font = New Font("Arial", 10, FontStyle.Bold)
                .Columns(0).HeaderText = "ITEM CODE"
                .Columns(1).HeaderText = "ITEM NAME"
                .Columns(2).HeaderText = "ITEM PRICE"
                .Columns(3).HeaderText = "COST"
                .Columns(4).HeaderText = "AVAILABLE QUANTITY"
                .Columns(5).HeaderText = "CHILD QTY"
                .Columns(7).HeaderText = "SKU Level"
                'for format & alignment
                '.Columns(2).DefaultCellStyle.Format = "##0.00"
                .Columns(2).DefaultCellStyle.Format = "C"
                .Columns(3).DefaultCellStyle.Format = "N2"
                .Columns(4).DefaultCellStyle.Format = "N0"
                .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter


                .Columns(3).Visible = False
                .Columns(5).Visible = False
                .Columns(6).Visible = False
                .Columns(7).Visible = False
                .Columns(8).Visible = False
                .Columns(9).Visible = False
                .Columns(10).Visible = False
                .Columns(11).Visible = False
                .Columns(12).Visible = False
                .Columns(13).Visible = False
                .Columns(14).Visible = False
                .Columns(15).Visible = False
                .Columns(16).Visible = False
            End With

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub txtSearch_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyUp

        Try
            If e.KeyCode = Keys.F5 Then

                TreeView1.Nodes.Clear()
                itemData.Filter = Nothing
                lblStatItemCount.Text = gridItem.RowCount & " items"
                txtSearch.Text = String.Empty
                Me.ActiveControl = txtSearch

            End If


            If e.KeyCode = Keys.F4 Then
                If txtSearch.Text = String.Empty Then
                    Try
                        Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                        TreeView1.Nodes.Remove(node(0))
                        FilterText()

                        If TreeView1.Nodes.Count = 0 Then
                            itemData.Filter = Nothing
                        Else
                            SearchItem()
                        End If

                        Me.ActiveControl = txtSearch
                    Catch ex As Exception

                    End Try
                Else
                    MessageBox.Show("Please Clear the search box before removing the last searched keyword", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
               

            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub InsertSelectedItem(ByVal i As Integer)

        Try

            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            Dim newTimeRecord As DataGridViewRow = gridSelectItem.Rows(gridSelectItem.NewRowIndex).Clone
            ' Dim iRow As Integer
            With record
                newTimeRecord.Cells(ItemCode.Index).Value = gridItem.Item(0, i).Value
                newTimeRecord.Cells(ItemName.Index).Value = gridItem.Item(1, i).Value
                newTimeRecord.Cells(QTY.Index).Value = "1"
                newTimeRecord.Cells(CustPrep.Index).Value = "0"

                If bTaxExcempt = True Then
                    newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
                Else
                    newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
                End If

                newTimeRecord.Cells(DISC.Index).Value = 0
                'newTimeRecord.Cells(TOTAL.Index).Value = dNetPrice
                newTimeRecord.Cells(TOTAL.Index).Value = newTimeRecord.Cells(QTY.Index).Value * newTimeRecord.Cells(Price.Index).Value
                newTimeRecord.Cells(LessV.Index).Value = dTSales
                newTimeRecord.Cells(VSales.Index).Value = dVSales
                newTimeRecord.Cells(DiscP.Index).Value = dDisc
                newTimeRecord.Cells(Cost.Index).Value = gridItem.Item(3, i).Value
                newTimeRecord.Cells(Taxable.Index).Value = gridItem.Item(6, i).Value
                newTimeRecord.Cells(ItemID.Index).Value = gridItem.Item(9, i).Value
                newTimeRecord.Cells(FullPrice.Index).Value = gridItem.Item(8, i).Value
                newTimeRecord.Cells(Description.Index).Value = gridItem.Item(10, i).Value
                newTimeRecord.Cells(Extended.Index).Value = gridItem.Item(11, i).Value & clsItemLookUp.TagSC(sTitle, gridItem.Item(7, i).Value)
                newTimeRecord.Cells(DiscAmount.Index).Value = 0
            End With

            dPrice = frmPriceLevel.gridLevel.Item(1, iRowLevel).Value
            sOldDes = txtSearch.Text
            gridSelectItem.Rows.Add(newTimeRecord)

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)
            'ValidateInputQty()
            'clsItemLookUp.GetPrice(1, gridItem.Item(6, i).Value, frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value)

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Public Sub InsertRecalledOrder()

        Try

            'clsItemLookUp.GetPrice(1, gridItem.Item(6, i).Value, frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value)

            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            'Dim newTimeRecord As DataGridViewRow = gridSelectItem.Rows(gridSelectItem.NewRowIndex).Clone

            'With record
            '    newTimeRecord.Cells(ItemCode.Index).Value = gridItem.Item(0, i).Value
            '    newTimeRecord.Cells(ItemName.Index).Value = gridItem.Item(1, i).Value
            '    newTimeRecord.Cells(QTY.Index).Value = "1"
            '    newTimeRecord.Cells(Price.Index).Value = frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value
            '    newTimeRecord.Cells(DISC.Index).Value = 0
            '    newTimeRecord.Cells(TOTAL.Index).Value = dNetPrice
            '    newTimeRecord.Cells(LessV.Index).Value = dTSales
            '    newTimeRecord.Cells(VSales.Index).Value = dVSales
            '    newTimeRecord.Cells(DiscP.Index).Value = dDisc
            '    newTimeRecord.Cells(Cost.Index).Value = gridItem.Item(3, i).Value
            '    newTimeRecord.Cells(Taxable.Index).Value = gridItem.Item(6, i).Value
            '    newTimeRecord.Cells(ItemID.Index).Value = gridItem.Item(9, i).Value
            '    newTimeRecord.Cells(FullPrice.Index).Value = gridItem.Item(8, i).Value
            '    newTimeRecord.Cells(Description.Index).Value = gridItem.Item(10, i).Value
            '    newTimeRecord.Cells(Extended.Index).Value = gridItem.Item(11, i).Value & clsItemLookUp.TagSC(sTitle, gridItem.Item(7, i).Value)
            '    newTimeRecord.Cells(DiscAmount.Index).Value = 0
            'End With

            'dPrice = frmPriceLevel.gridLevel.Item(1, iRowLevel).Value
            'sOldDes = txtSearch.Text
            'gridSelectItem.Rows.Add(newTimeRecord)

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)
            ValidateInputQty()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try


    End Sub
    Private Sub gridItem_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridItem.DoubleClick

        Try
            If gridItem.Rows.Count = 0 Then
                Exit Sub
            End If

            SelectPrice()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub SelectPrice()

        Try

            Dim iReturnValue As Integer
            Dim availVal As Integer
            'Dim iStoreID As Integer

            sItemCode = gridItem.Item(0, gridItem.CurrentRow.Index).Value
            sItemName = gridItem.Item(1, gridItem.CurrentRow.Index).Value
            iItemType = Val(gridItem.Item(12, gridItem.CurrentRow.Index).Value)

            If iItemType = 0 Then

                'If cmbLocation.SelectedValue = iStoreID Then
                '    availVal = db.SOD_fn_GetConvertQty(sItemCode, 1)
                'Else
                '    availVal = db.SOD_fn_GetQty(cmbLocation.SelectedValue, sItemCode, 0)
                'End If
                availVal = clsItemLookUp.getItemQty(sItemCode)
                'MsgBox(availVal)
                If availVal < 1 Then
                    iReturnValue = MsgBox("This item is out of stock.", vbExclamation, "Message!")
                Else

                    Try
                        bInsert = True
                        Dim result = frmPriceLevel.ShowDialog(Me)

                        If result = Windows.Forms.DialogResult.OK Then
                            If gridSelectItem.RowCount > 6 Then
                                If MsgBox("The number of items selected exceeds the maximum allowable entries for each transaction, Continue?" & vbNewLine & vbNewLine & "Note: Some Items will not be printed in the invoice.", vbYesNo + vbExclamation, "Warning") = vbNo Then
                                    gridSelectItem.Rows.Remove(gridSelectItem.CurrentRow)
                                    lblStatItemSelected.Text = gridSelectItem.RowCount & " items selected" & IIf(gridSelectItem.RowCount > 1, "s", "")
                                End If
                            End If
                            gridSelectItem.CurrentCell = gridSelectItem(2, gridSelectItem.RowCount - 1)
                            gridSelectItem.BeginEdit(True)
                        End If
                    Catch ex As Exception

                    End Try


                End If


            Else

                bInsert = True
                'iRow = gridItem.CurrentRow.Index
                frmPriceLevel.ShowDialog()

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub CallIndex()

        Try

            If gridItem.RowCount = 0 Then
                Exit Sub
            End If

            iRow = clsItemLookUp.RowIndex(gridItem)

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Private Sub removeItems()

        Try

            Dim iReturnValue As Integer

            iReturnValue = MsgBox("Do you want to remove this item?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            If iReturnValue = MsgBoxResult.Yes Then
                If Not gridSelectItem.CurrentRow.IsNewRow Then
                    delitems.Add(gridSelectItem.Item(16, gridSelectItem.CurrentCell.RowIndex).Value)
                    gridSelectItem.Rows.Remove(gridSelectItem.CurrentRow)
                    lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"
                    gridSelectItem.Focus()
                    ValidateAllQty()
                End If
            End If

            If gridSelectItem.Rows.Count = 0 Then
                dTotalTSales = 0
                dTotalVSales = 0
                dTotalSales = 0
                txtSub.Text = Format(dTotalTSales, "#,##0.00")
                txtVat.Text = Format(dTotalVSales, "#,##0.00")
                txtTotal.Text = Format(dTotalSales, "#,##0.00")
                txtSearch.Focus()
            Else
                UpdateAmt()
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub


    Private Sub cmdOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOk.Click

        Try
            SaveWorkOrder()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0013", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub SaveWorkOrder()

        Try

            If gridSelectItem.RowCount > 0 Then

                iErrCnt = 0

                ValidateAllQty()

                If iErrCnt > 0 Then
                    MsgBox("Invalid data found on the entry.", vbExclamation, "Work Order Entry")
                Else
                    If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then
                        UpdateOrder(Split(lblOrderNo.Text, ": ")(1))
                    Else
                        InsertEntry()
                    End If
                End If

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub InsertEntry()

        Try

            Dim iRow As Integer
            Dim dCost As Double
            Dim iOrderID As Integer
            Dim iItemID As Integer
            Dim dFullPrice As Double
            Dim dPrice As Double
            Dim dQuantityOnOrder As Double
            Dim iSalesRepID As Integer
            Dim iTaxable As Integer
            Dim sDescription As String
            Dim sComment As String
            Dim iReturnValue As Integer
            Dim qtyPrep As Integer
            ' Dim Intnum As String

            If Me.txtSales.Text = "" Then
                MsgBox("Please select a Sales Representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.cboPayment.Text = "" Then
                MsgBox("Please select a Payment Method", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select a Customer", vbExclamation, "Message!")
                Exit Sub
            ElseIf gridSelectItem.Rows.Count = 0 Then
                MsgBox("Please select Item to order", vbExclamation, "Message!")
                Exit Sub
            End If

            For x As Integer = 0 To gridSelectItem.Rows.Count - 1
                For y As Integer = 0 To gridSelectItem.Rows.Count - 1
                    If y <> x AndAlso gridSelectItem.Rows(x).Cells(0).Value.ToString = gridSelectItem.Rows(y).Cells(0).Value.ToString Then

                        Dim res As Integer
                        res = MsgBox("There is a same Item in one Entry! Please Sum up the Qty Order!" & vbCrLf & vbCrLf & "This might cause an issue in the feature." & vbCrLf & vbCrLf & "Do you wish to continue ?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Message!")
                        If res = MsgBoxResult.Yes Then
                            GoTo Proceed
                        ElseIf res = MsgBoxResult.No Then
                            Exit Sub
                        End If
                    End If
                Next
            Next
Proceed:

            iReturnValue = MsgBox("Do you want to save this entry as Work Order ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

            If iReturnValue = MsgBoxResult.Yes Then

                Cursor.Current = Cursors.WaitCursor

                Dim ReleaseType = ""
                If rbtnPickup.Checked Then
                    ReleaseType = "Pick-up"
                ElseIf rbtnDelivery.Checked Then
                    ReleaseType = "Delivery"
                End If

                'iOrderID = db.SOD_sp_InsertQoute(sReg, iCusID, iSalesID, Double.Parse(txtVat.Text), Double.Parse(txtTotal.Text), IIf(txtType.Text = String.Empty, txtRemarks.Text, "[" & txtType.Text & " - " & txtRemarks.Text & "]"))
                iOrderID = db.SOD_sp_InsertQoute(sReg, iCusID, iSalesID, Double.Parse(txtVat.Text), Double.Parse(txtTotal.Text), cboPayment.Text & "; " & ReleaseType & "; " & txtRemarks.Text, usrUsername)

                For iRow = 0 To gridSelectItem.Rows.Count - 1

                    dCost = gridSelectItem.Item(9, iRow).Value
                    iOrderID = iOrderID
                    iItemID = gridSelectItem.Item(11, iRow).Value
                    dFullPrice = gridSelectItem.Item(12, iRow).Value
                    dPrice = gridSelectItem.Item(3, iRow).Value
                    dQuantityOnOrder = gridSelectItem.Item(2, iRow).Value
                    iSalesRepID = iSalesID

                    If bTaxExcempt = True Then
                        iTaxable = 0
                    Else
                        iTaxable = 1
                    End If

                    sDescription = gridSelectItem.Item(13, iRow).Value
                    sComment = gridSelectItem.Item(14, iRow).Value
                    qtyPrep = gridSelectItem.Item(18, iRow).Value
                    Dim ipickLoc As String = ""

                    If gridSelectItem.Item(17, iRow).Value = True Then
                        ipickLoc = "UP-STORE"
                    Else
                        ipickLoc = "STORE"
                    End If

                    db.SOD_sp_InsertQouteEntry(dCost, iOrderID, iItemID, dFullPrice, dPrice, dQuantityOnOrder, iSalesRepID, iTaxable, sDescription, ipickLoc, qtyPrep)
                    clsItemLookUp.setQuantityCommitted(iItemID, dQuantityOnOrder)
                Next

                If bSuccessImport = True Then
                    clsImportQuotation.InsertQuotationLog(iOrderID, iRefNum, iSalesRepID)
                End If
                ' Intnum = InputBox("Successfully saved into Work Order," & Chr(10) & "Pls. copy entry# for viewing into RMS :", "Message!", iOrderID)
                MessageBox.Show("Successfully Saved into Work Order !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                clsItemLookUp.fakeUpdate(iOrderID)
                checkCustomerGroupWo(iCusID, iOrderID, "Insert")
                printWO(iOrderID, 2)

                RefreshDetails()

                Cursor.Current = Cursors.Default
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub checkCustomerGroupWo(ByVal custid As Integer, ByVal custOrderId As Integer, ByVal type As String)
        Try

            If Not clsCustomer.getGroupID(custOrderId) = custOrderId Then

                If type = "Update" Then
                    Exit Sub
                End If

            End If

            If clsCustomer.checkCustomerOpenWo(custid, custOrderId) = True Then

                Dim iReturnValue As Integer

                iReturnValue = MsgBox("This Customer has a Current Open Work Order. Group the Orders?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                If iReturnValue = MsgBoxResult.Yes Then
                    frmCustomerOrders.custIDWo = custid
                    frmCustomerOrders.custWoID = custOrderId
                    frmCustomerOrders.ShowDialog()

                Else
                    Exit Sub
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0016", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub UpdateOrder(ByVal orderid As Integer)

        Try

            Dim iRow As Integer
            Dim dCost As Double
            Dim iOrderID As Integer
            Dim iItemID As Integer
            Dim dFullPrice As Double
            Dim dPrice As Double
            Dim dQuantityOnOrder As Double
            Dim iSalesRepID As Integer
            Dim iTaxable As Integer
            Dim sDescription As String
            Dim sComment As String
            Dim iReturnValue As Integer
            Dim qtyPrep As Integer
            ' Dim Intnum As String
            Dim iOrderEntryID As Integer

            If Me.txtSales.Text = "" Then
                MsgBox("Please select sales representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select customer", vbExclamation, "Message!")
                Exit Sub
            ElseIf gridSelectItem.Rows.Count = 0 Then
                MsgBox("Please select item to order", vbExclamation, "Message!")
                Exit Sub
            End If

            For x As Integer = 0 To gridSelectItem.Rows.Count - 1
                For y As Integer = 0 To gridSelectItem.Rows.Count - 1
                    If y <> x AndAlso gridSelectItem.Rows(x).Cells(0).Value.ToString = gridSelectItem.Rows(y).Cells(0).Value.ToString Then

                        Dim res As Integer
                        res = MsgBox("There is a same Item in one Entry! Please Sum up the Qty Order!" & vbCrLf & vbCrLf & "This might cause an issue in the feature." & vbCrLf & vbCrLf & "Do you wish to continue ?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Message!")
                        If res = MsgBoxResult.Yes Then
                            GoTo Proceed
                        ElseIf res = MsgBoxResult.No Then
                            Exit Sub
                        End If
                    End If
                Next
            Next
Proceed:

            iReturnValue = MsgBox("Save changes?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

            If iReturnValue = MsgBoxResult.Yes Then

                Dim ReleaseType = ""
                If rbtnPickup.Checked Then
                    ReleaseType = "Pick-up"
                ElseIf rbtnDelivery.Checked Then
                    ReleaseType = "Delivery"
                End If

                clsRecall.UpdateOrder(orderid, iCusID, iSalesID, Double.Parse(txtVat.Text), Double.Parse(txtTotal.Text), cboPayment.Text & "; " & ReleaseType & "; " & txtRemarks.Text)

                For iRow = 0 To gridSelectItem.Rows.Count - 1

                    dCost = gridSelectItem.Item(9, iRow).Value
                    iOrderID = orderid
                    iItemID = gridSelectItem.Item(11, iRow).Value
                    dFullPrice = gridSelectItem.Item(12, iRow).Value
                    dPrice = gridSelectItem.Item(3, iRow).Value
                    dQuantityOnOrder = gridSelectItem.Item(2, iRow).Value
                    iSalesRepID = iSalesID
                    iOrderEntryID = gridSelectItem.Item(16, iRow).Value

                    If gridSelectItem.Item(10, iRow).Value = 1 Then
                        iTaxable = 1
                    Else
                        iTaxable = 0
                    End If

                    sDescription = gridSelectItem.Item(13, iRow).Value
                    sComment = gridSelectItem.Item(14, iRow).Value
                    qtyPrep = gridSelectItem.Item(18, iRow).Value

                    Dim pickLoc = ""
                    If gridSelectItem.Item(17, iRow).Value = True Then
                        pickLoc = "UP-STORE"
                    Else
                        pickLoc = "STORE"
                    End If

                    clsRecall.UpdateOrderEntry(iOrderID, iItemID, iSalesID, dCost, dFullPrice, dPrice, dQuantityOnOrder, iTaxable, sDescription, pickLoc, qtyPrep, iOrderEntryID)

                Next

                clsRecall.DeleteOrderItem(iOrderID, delitems)
                'Intnum = InputBox("Work Order Successfully Updated !," & Chr(10) & "Pls. copy entry# for viewing into RMS:", "Message!", orderid)
                MessageBox.Show("Work Order Successfully Updated !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                checkCustomerGroupWo(iCusID, iOrderID, "Update")
                printWO(iOrderID, 2)

                RefreshDetails()

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0017", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Sub UpdateAmt()

        Try

            Dim iRow, iVat As Integer
            Dim dCurPrice, dFinPrice As Double
            Dim dSub, dVat, dTot As Double

            dCurPrice = 0
            dFinPrice = 0
            dSub = 0
            dVat = 0
            dTot = 0


            For iRow = 0 To gridSelectItem.Rows.Count - 1
                'clsItemLookUp.GetPrice(gridSelectItem.Item(2, iRow).Value, gridSelectItem.Item(10, iRow).Value, gridSelectItem.Item(3, iRow).Value)
                dCurPrice = gridSelectItem.Item("Price", iRow).Value
                iVat = gridSelectItem.Item(10, iRow).Value

                'If bTaxExcempt = True Then
                '    If iVat = 1 Then
                '        'dFinPrice = dCurPrice / 1.12
                '        'gridSelectItem.Item("Price", iRow).Value = gridSelectItem.Item("Price", iRow).Value / 1.12
                '        gridSelectItem.Item(5, iRow).Value = gridSelectItem.Item("QTY", iRow).Value * gridSelectItem.Item("Price", iRow).Value
                '    Else
                '        'dFinPrice = dCurPrice
                '        'gridSelectItem.Item("Price", iRow).Value = gridSelectItem.Item("Price", iRow).Value
                '        gridSelectItem.Item(5, iRow).Value = gridSelectItem.Item("QTY", iRow).Value * gridSelectItem.Item("Price", iRow).Value
                '    End If
                '    'MsgBox(dFinPrice)
                '    ''gridSelectItem.Item("Price", iRow).Value = dFinPrice
                '    'gridSelectItem.Item(5, iRow).Value = gridSelectItem.Item("QTY", iRow).Value * dFinPrice
                '    gridSelectItem.Item(5, iRow).Value = gridSelectItem.Item("QTY", iRow).Value * gridSelectItem.Item("Price", iRow).Value
                'Else
                '    If iVat = 1 Then
                '        dCurPrice = dCurPrice / 1.12
                '        dFinPrice = dCurPrice * gridSelectItem.Item("QTY", iRow).Value
                '    Else
                '        dCurPrice = dCurPrice
                '        dFinPrice = dCurPrice * gridSelectItem.Item("QTY", iRow).Value
                '    End If
                'End If

                gridSelectItem.Item(5, iRow).Value = dCurPrice * gridSelectItem.Item("QTY", iRow).Value
                dTot = dTot + gridSelectItem.Item(5, iRow).Value

            Next


            If bTaxExcempt Then
                dVat = 0
            Else
                dVat = dTot - (dTot / 1.12)
            End If

            dSub = dTot - dVat

            Me.txtSub.Text = Format(dSub, "#,##0.00")
            Me.txtVat.Text = Format(dVat, "#,##0.00")
            Me.txtTotal.Text = Format(dTot, "#,##0.00")

            'Me.txtSub.Text = Format(dTotalTSales, "#,##0.00")
            'Me.txtVat.Text = Format(dTotalVSales, "#,##0.00")
            'Me.txtTotal.Text = Format(dTotalSales, "#,##0.00")

        Catch ex As Exception
            ' MessageBox.Show(ex.Message & " Here ")
            '  MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0018", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ' ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub cmdRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRemove.Click

        Try
            removeItems()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0019", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    Try
    '        clsRecall.DeleteOrderItem(1, delitems)
    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0020", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try

    'End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click

        Try

            If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

                CancelOrder()

            'frmPassword.sType = "CancelOrder"
            'Dim pDialog As DialogResult = frmPassword.ShowDialog
            'If pDialog = Windows.Forms.DialogResult.Cancel Then
            '    MsgBox("Order was not cancelled!", vbExclamation, "Message")
            'Else
            '    CancelOrder()

            'End If

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0021", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub CancelOrder()

        Try
            Dim iMsg As Integer
            iMsg = MsgBox("Do you want to cancel this order?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            If iMsg = MsgBoxResult.Yes Then

                clsRecall.CancelOrder(Split(lblOrderNo.Text, ": ")(1))
                RefreshDetails()

                MsgBox("Order was cancelled successfully!", vbInformation, "Message")

            Else
                MsgBox("Order was not cancelled!", vbExclamation, "Message")
            End If


            'Dim iMsg As Integer
            'iMsg = MsgBox("Do you want to cancel this order?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            'If iMsg = MsgBoxResult.Yes Then

            '    clsRecall.CancelOrder(Split(lblOrderNo.Text, ": ")(1))

            'Else
            '    Dim iOrderType As String
            '    Dim iOrderNo As Integer

            '    iOrderType = Split(lblOrderNo.Text, ": ")(0)
            '    iOrderNo = Split(lblOrderNo.Text, ": ")(1)

            '    If iOrderType = "Sales Quotation" Then
            '        clsRecall.CancelQuotation(iOrderNo)
            '    Else
            '        clsRecall.CancelOrder(iOrderNo)
            '    End If
            '    MsgBox("Order was cancelled successfully!", vbInformation, "Message")
            '    Application.Restart()
            'End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0022", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub ClearItem()

        Try

            Dim iReturnValue As Integer

            If gridSelectItem.RowCount > 0 Then
                iReturnValue = MsgBox("Do you want to clear all selected items?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
                If iReturnValue = MsgBoxResult.Yes Then
                    gridSelectItem.Rows.Clear()
                    lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"
                End If

                If gridSelectItem.Rows.Count = 0 Then
                    dTotalTSales = 0
                    dTotalVSales = 0
                    dTotalSales = 0
                    txtSearch.Focus()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0023", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub cmdSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelect.Click

        Try

            If gridItem.RowCount > 0 Then
                SelectPrice()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0024", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub frmItemLookUp_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown

        Try

            If e.KeyCode = Keys.F12 Then
                SaveWorkOrder()
            ElseIf e.KeyCode = Keys.F11 Then
                frmRecall.ShowDialog()
            ElseIf e.KeyCode = Keys.F6 Then
                ImportQuote()
            ElseIf e.KeyCode = Keys.F9 Then
                cmdCancel_Click(sender, e)
            ElseIf e.KeyCode = Keys.F7 Then
                frmSalesRep.ShowDialog()
            ElseIf e.KeyCode = Keys.F8 Then
                ShowCustomer()
            ElseIf e.KeyCode = Keys.F1 Then
                FilterText()
                txtSearch.Text = String.Empty
                SearchItem()
            ElseIf e.KeyCode = Keys.Escape Then
                If MsgBox("Do you really want to close Work/Sales Order Entry?", vbQuestion + vbYesNo, "Quit") = vbYes Then
                    Me.Close()
                End If
            ElseIf (e.KeyCode And Not Keys.Modifiers) = Keys.D1 AndAlso e.Modifiers = Keys.Control Then
                gridItem.Focus()
            ElseIf (e.KeyCode And Not Keys.Modifiers) = Keys.D2 AndAlso e.Modifiers = Keys.Control Then
                gridSelectItem.Focus()
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0025", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub frmItemLookUp_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            Dim h As System.Net.IPHostEntry = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName)

            sReg = Format(db.SOD_fn_GetReg(h.AddressList.GetValue(0).ToString), "#")

            Me.gridSelectItem.ColumnHeadersDefaultCellStyle.Font = New Font("Tahoma", 9.75, FontStyle.Bold)
            Me.gridSelectItem.DefaultCellStyle.Font = New Font("Tahoma", 9.75, FontStyle.Regular)

            Me.statServer.Text = "SERVER : " & DB_Server
            Me.statDb.Text = "DATABASE : " & DB_Name
            lblStatItemCount.Text = gridSelectItem.RowCount & " items"
            lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"

            iStoreID = (From a In db.Configurations Select a.StoreID).SingleOrDefault
            frmLogin.ShowDialog()
            FormatGrid()
            RefreshDetails()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0026", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub loadData()

        Try

            db_lastupdate = clsItemLookUp.checkTableUpdate()

            Cursor.Current = Cursors.WaitCursor

            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False

            clsItemLookUp.getItemtoGrid()
            itemData.DataSource = clsItemLookUp.dt1
            gridItem.DataSource = itemData

            GridColumnWidth()
            SearchItem()
            iRow = 0
            txtSearch.Focus()
            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

            ToolStripStatusLabel1.Text = "STATUS :"

            Update_Timer.Start()
            Cursor.Current = Cursors.Default

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0027", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub FormatGrid()

        Try

            With gridSelectItem


                'for column width
                .Columns(0).Width = 100
                .Columns(1).Width = 300
                .Columns(2).Width = 100
                .Columns(3).Width = 100
                .Columns(5).Width = 150
                .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(3).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(5).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(2).DefaultCellStyle.Format = "N0"
                .Columns(3).DefaultCellStyle.Format = "C"
                .Columns(5).DefaultCellStyle.Format = "C"
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

                .Columns(17).Width = 100
                .Columns(18).Width = 100
            End With

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0028", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click

        If MsgBox("Do you really want to close Work/Sales Order Entry?", vbQuestion + vbYesNo, "Quit") = vbYes Then
            Me.Close()
        End If

    End Sub

    Private Sub gridItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridItem.Click

        Try

            If Me.gridItem.Rows.Count = 0 Then
                Exit Sub
            End If

            sItemCode = gridItem.Item(0, gridItem.CurrentRow.Index).Value
            sItemName = gridItem.Item(1, gridItem.CurrentRow.Index).Value

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0029", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdInterStore_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdInterStore.Click

        Try

            If Me.gridItem.Rows.Count = 0 Then
                Exit Sub
            End If

            frmCheckStore.lblItemCode.Text = gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString()
            frmCheckStore.lblItemName.Text = gridItem.Item(1, gridItem.CurrentRow.Index).Value.ToString()
            frmCheckStore.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0030", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Private Sub cmdSSales_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSSales.Click

        Try
            frmSalesRep.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0031", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdSCustomer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSCustomer.Click

        Try
            ShowCustomer()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0032", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub ShowCustomer()

        Try

            If gridSelectItem.RowCount > 0 Then
                If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Or Not txtCustomer.Text = Nothing Then

                    If MsgBox("Do you want to change the Customer of this Order ? " & vbCrLf & vbCrLf & "The Price of Selected Item will be Reset . " & vbCrLf & vbCrLf & "Please Recheck the price after Changing the Customer", vbInformation + vbYesNo, "Message!") = vbYes Then
                        frmCustomer.ShowDialog()

                        For Each row As DataGridViewRow In gridSelectItem.Rows
                            row.Cells(3).Value = "-"
                        Next
                    End If
                Else
                    frmCustomer.ShowDialog()
                End If

                bAllow = False
            Else
                frmCustomer.ShowDialog()
            End If


        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0033", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub gridItem_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridItem.KeyDown

        Try

            If e.KeyCode = Keys.Back Then
                Me.txtSearch.Focus()
            ElseIf e.KeyCode = Keys.Enter Then
                If gridItem.RowCount > 0 Then
                    SelectPrice()
                    e.SuppressKeyPress = True
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0034", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub txtSales_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSales.KeyPress

        Try

            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                frmSalesRep.ShowDialog()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0035", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub txtCustomer_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCustomer.KeyPress

        Try

            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                frmCustomer.ShowDialog()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0036", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSearch.Click

        Try

            If txtSearch.Text = String.Empty Then
                MessageBox.Show("Search cannot be empty !", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else

                FilterText()
                txtSearch.Text = String.Empty
                SearchItem()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0037", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRefresh.Click

        Try

            RefreshDetails()
            loadData()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0038", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub RefreshDetails()

        Try

            txtSales.Text = ""
            txtCustomer.Text = ""
            txtSearch.Text = ""
            txtRemarks.Text = String.Empty
            txtType.Text = String.Empty

            gridSelectItem.Rows.Clear()
            cboPayment.SelectedIndex = -1
            bEmployee = False
            iPrice = 0
            iSalesID = 0
            iCusID = 0
            sAcctNum = ""
            sTitle = ""
            bTaxExcempt = False
            dTotalTSales = 0
            dTotalVSales = 0
            dTotalSales = 0
            Me.txtSub.Text = "0.00"
            Me.txtVat.Text = "0.00"
            Me.txtTotal.Text = "0.00"
            lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"
            lblOrderNo.Text = "Order #: "
            Me.txtCustomer.Focus()

            ' TreeView1.Nodes.Clear()
            itemData.Filter = Nothing
            lblStatItemCount.Text = gridItem.RowCount & " items"
            txtSearch.Text = String.Empty
            txtSearch.Focus()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0039", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub gridSelectItem_CellBeginEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellCancelEventArgs) Handles gridSelectItem.CellBeginEdit

        Try

            If e.ColumnIndex = 2 Then

                curQty = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)

            ElseIf e.ColumnIndex = 3 Then
                Try
                    curPrice = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)
                Catch ex As Exception

                End Try

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0040", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub gridSelectItem_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridSelectItem.CellValueChanged

        'If gridSelectItem.RowCount > 0 Then
        '    If e.ColumnIndex = 2 Or e.ColumnIndex = 3 Then
        '        ValidateInputQty()
        '    End If
        'End If

    End Sub
    Public Sub ValidateInputQty()

        Try

            Dim iReturnValue, iCurRow As Integer
            Dim sLineItemCode, sPrevItemCode As String
            Dim dReqQty, dCommittedQty As Double
            Dim dAvailable, dNetAvailable As Double

            iCurRow = -1
            dReqQty = 0
            dAvailable = 0
            dNetAvailable = 0
            dCommittedQty = 0
            sPrevItemCode = String.Empty
            bFlag = True

            If gridSelectItem.RowCount <> 0 Then

                If IsNumeric(gridSelectItem(2, gridSelectItem.CurrentRow.Index).Value) = False Then
                    MsgBox("The string '" & gridSelectItem(2, gridSelectItem.CurrentRow.Index).Value & "' is not a valid quantity.", vbExclamation, "Work/Sales Oder Entry")
                    gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value = 1
                    iErrCnt = iErrCnt + 1
                Else
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Style.ForeColor = Color.Black
                End If

                If Val(gridSelectItem(2, gridSelectItem.CurrentRow.Index).Value) < 0 Then
                    MsgBox("An invalid quantity was detected.", vbCritical, "Work/Sales Order Entry")
                    gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value = 1
                    iErrCnt = iErrCnt + 1
                Else
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Style.ForeColor = Color.Black
                End If

                If IsNumeric(gridSelectItem(3, gridSelectItem.CurrentRow.Index).Value) = False Then
                    MsgBox("The string '" & gridSelectItem(2, gridSelectItem.CurrentRow.Index).Value & "' is not a valid currency.", vbExclamation, "Work/Sales Oder Entry")
                    gridSelectItem(3, gridSelectItem.CurrentRow.Index).Value = dPrice
                    iErrCnt = iErrCnt + 1
                Else
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(3).Style.ForeColor = Color.Black
                End If

                If Val(gridSelectItem(3, gridSelectItem.CurrentRow.Index).Value) < clsItemLookUp.SearchItemPriceC(gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value) Then
                    'temp commented for CUE
                    'gridSelectItem(3, gridSelectItem.CurrentRow.Index).Value = curPrice
                    'MsgBox("The price should not be less than the Price C (D2).", vbExclamation, "Work/Sales Order Entry")
                    'bFlag = False
                    'iErrCnt = iErrCnt + 1
                Else
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(3).Style.ForeColor = Color.Black
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(3).Style.SelectionForeColor = Color.White
                End If

                If iItemType = 0 Then
                    sPrevItemCode = gridSelectItem(0, gridSelectItem.CurrentRow.Index).Value
                    'dNetAvailable = db.SOD_fn_GetConvertQty(sPrevItemCode, 1)

                    'If dNetAvailable < 0 Then
                    '    iCurRow = gridSelectItem.CurrentRow.Index
                    '    GoTo err_flag
                    'End If

                    For n As Integer = 0 To gridSelectItem.Rows.Count - 1
                        sLineItemCode = gridSelectItem(0, n).Value


                        dNetAvailable = clsItemLookUp.getItemQty(sLineItemCode)

                        MsgBox(dNetAvailable)
                        If Val(gridSelectItem(2, n).Value) > dNetAvailable Then
                            'MsgBox(curQty)
                            MsgBox("Insufficient Quantity!", vbExclamation, "Message")
                            ' gridSelectItem.Rows.Remove(gridSelectItem.CurrentRow)
                            gridSelectItem(2, n).Value = 0

                        End If

                    Next


                    '                sPrevItemCode = gridSelectItem(0, gridSelectItem.CurrentRow.Index).Value
                    '                dNetAvailable = db.SOD_fn_GetConvertQty(sPrevItemCode, 0)
                    '                dCommittedQty = 0

                    '                For n As Integer = 0 To gridSelectItem.Rows.Count - 1

                    '                    sLineItemCode = gridSelectItem(0, n).Value

                    '                    If sLineItemCode.Substring(0, 5) = sPrevItemCode.Substring(0, 5) Then
                    '                        dCommittedQty = dCommittedQty + db.SOD_fn_GetAvailableQty(sLineItemCode, Val(gridSelectItem(2, n).Value))
                    '                    End If

                    '                Next

                    '                dAvailable = dNetAvailable - dCommittedQty

                    '                If dAvailable < 0 Then
                    '                    iCurRow = gridSelectItem.CurrentRow.Index
                    '                    GoTo err_flag

                    '                    iErrCnt = iErrCnt + 1
                    '                    If gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Value <= db.SOD_fn_GetConvertQty(sPrevItemCode, 1) Then
                    '                        gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Style.ForeColor = Color.Black
                    '                    Else
                    '                        gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Style.ForeColor = Color.Red
                    '                    End If
                    '                End If

                    '                gridSelectItem.Item(5, gridSelectItem.CurrentRow.Index).Value = gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value * gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value
                    '                gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value = CInt(gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value)
                    '                gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value = CDec(gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value)
                    '                gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(2).Style.SelectionForeColor = Color.White


err_flag:
                    If iCurRow <> -1 Then
                        '                    'iReturnValue = MsgBox("Insufficient quantity to fulfill the request." & vbNewLine & vbNewLine & _
                        '                    '                          "Item Information:" & vbNewLine & _
                        '                    '                          "Item lookup code: " & gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value & vbNewLine & _
                        '                    '                          "Description: " & gridSelectItem.Item(1, gridSelectItem.CurrentRow.Index).Value & vbNewLine & _
                        '                    '                          "Requested: " & gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value & vbNewLine & _
                        '                    '                          "On hand: " & db.SOD_fn_GetConvertQty(gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value, 1) & vbNewLine & _
                        '                    '                          "Commited/Reserved: " & dCommittedQty & vbNewLine & _
                        '                    '                          "Available: " & dAvailable, vbExclamation, "Work/Sales Order Entry")
                        iReturnValue = MsgBox("Insufficient quantity to fulfill the request.", vbExclamation, "Work/Sales Order Entry")
                        gridSelectItem.Rows.Remove(gridSelectItem.CurrentRow)
                    End If

                    iCurRow = -1

                    gridSelectItem.Item(5, gridSelectItem.CurrentRow.Index).Value = gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value * gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value
                Else
                    gridSelectItem.Item(5, gridSelectItem.CurrentRow.Index).Value = gridSelectItem.Item(2, gridSelectItem.CurrentRow.Index).Value * gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value
                End If

                UpdateAmt()

            Else
                iErrCnt = iErrCnt + 1
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0041", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub ValidateAllQty()

        Try

            Dim sLineItemCode, sPrevItemCode, sMsg As String
            Dim dReqQty, dCommittedQty As Double
            Dim dAvailable, dNetAvailable As Double
            Dim sCurIndex As Integer

            dReqQty = 0
            dAvailable = 0
            dNetAvailable = 0
            dCommittedQty = 0
            sPrevItemCode = String.Empty
            sCurIndex = 0


            For i As Integer = 0 To gridSelectItem.Rows.Count - 1

                If IsNumeric(gridSelectItem(2, i).Value) = False Then
                    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                    sMsg = "The string '" & gridSelectItem(2, i).Value & "' is not a valid quantity."
                    iErrCnt = iErrCnt + 1
                End If

                If Val(gridSelectItem(2, i).Value) <= 0 Then
                    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                    sMsg = "An invalid quantity was detected."
                    iErrCnt = iErrCnt + 1
                End If

                If IsNumeric(gridSelectItem(3, i).Value) = False Then
                    gridSelectItem.Rows(i).Cells(3).Style.ForeColor = Color.Red
                    If gridSelectItem(3, i).Value = "-" Then
                        MessageBox.Show("Please Check the Price", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    sMsg = "The number '" & gridSelectItem(3, i).Value & "' is not a valid amount."
                    iErrCnt = iErrCnt + 1
                End If

                'If Val(gridSelectItem(3, i).Value) < clsItemLookUp.SearchItemPriceC(gridSelectItem.Item(0, i).Value) Then
                '    'temp commented for CUE
                '    'gridSelectItem.Rows(i).Cells(3).Style.ForeColor = Color.Red
                '    'gridSelectItem.Rows(i).Cells(3).Style.SelectionForeColor = Color.Red
                '    'sMsg = "The price should not be less than the Price C (D2)."
                '    'iErrCnt = iErrCnt + 1
                'End If

                sPrevItemCode = gridSelectItem(0, i).Value
                sCurIndex = gridSelectItem(11, i).Value

                ''If cmbLocation.SelectedValue = iStoreID Then
                ''    dNetAvailable = db.SOD_fn_GetConvertQty(sPrevItemCode, 1)
                ''    dCommittedQty = 0
                ''    For n As Integer = 0 To gridSelectItem.Rows.Count - 1

                ''        sLineItemCode = gridSelectItem(0, n).Value

                ''        If sLineItemCode.Substring(0, 5) = sPrevItemCode.Substring(0, 5) Then
                ''            dCommittedQty = dCommittedQty + db.SOD_fn_GetAvailableQty(sLineItemCode, Val(gridSelectItem(2, n).Value))
                ''        End If

                ''    Next

                ''Else
                ''    dNetAvailable = db.SOD_fn_GetQty(cmbLocation.SelectedValue, sPrevItemCode, 0)
                ''    dCommittedQty = 0
                ''End If

                'If cmbLocation.SelectedValue = iStoreID Then

                If clsItemLookUp.GetItemType(sPrevItemCode) = 0 Or clsItemLookUp.GetItemType(sPrevItemCode) = 3 Then

                    If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then
                        'MsgBox(db.SOD_fn_GetQty(cmbLocation.SelectedValue, sPrevItemCode, 0))
                        'MsgBox(clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex))
                        dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode) + clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex)

                    Else
                        dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode)

                    End If

                    dCommittedQty = 0
                    For n As Integer = 0 To gridSelectItem.Rows.Count - 1
                        sLineItemCode = gridSelectItem(0, n).Value
                        If sPrevItemCode = sLineItemCode Then
                            dCommittedQty = dCommittedQty + Val(gridSelectItem(2, n).Value)
                        End If
                    Next
                    'End If
                    'MsgBox(dAvailable & " = " & dNetAvailable & " - " & dCommittedQty)
                    dAvailable = dNetAvailable - dCommittedQty
                    'MsgBox("avail " & dAvailable)
                    'MsgBox(dAvailable & " = " & dNetAvailable & " - " & dCommittedQty)

                    If dAvailable < 0 Then
                        'If gridSelectItem.Rows(i).Cells(2).Value <= db.SOD_fn_GetConvertQty(sPrevItemCode, 1) Then
                        '    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
                        '    gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White
                        'Else
                        '    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                        '    gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
                        'End If
                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
                        iErrCnt = iErrCnt + 1
                    Else
                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White
                    End If

                End If

            Next

            'gridSelectItem.CurrentCell = Nothing
            UpdateAmt()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0042", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub


    Private Sub gridSelectItem_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridSelectItem.KeyDown

        Try

            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                e.Handled = True
                txtSearch.Text = String.Empty
                txtSearch.Focus()
            ElseIf e.KeyCode = Keys.Delete Then
                removeItems()
            ElseIf e.KeyCode = Keys.F2 Then
                sItemCode = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value
                EditPrice()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0043", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub EditPrice()

        Try

            bInsert = False
            frmPriceLevel.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0044", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub gridSelectItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSelectItem.Click

        Try

            If gridSelectItem.RowCount > 0 Then
                If gridSelectItem.SelectedRows.Count > 0 Then
                    sItemCode = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value
                    sItemName = gridSelectItem.Item(1, gridSelectItem.CurrentRow.Index).Value
                    dPrice = clsItemLookUp.SearchItemPriceC(gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0045", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub gridSelectItem_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSelectItem.DoubleClick

        Try

            If gridSelectItem.Rows.Count = 0 Then
                Exit Sub
            End If

            sItemCode = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value
            sItemName = gridSelectItem.Item(1, gridSelectItem.CurrentRow.Index).Value
            dPrice = clsItemLookUp.SearchItemPriceC(gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value)

            EditPrice()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0046", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdPriceLevel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPriceLevel.Click

        Try

            If Me.gridItem.RowCount = 0 Then
                Exit Sub
            End If

            frmLinqLevel.Show()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0047", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub cmdImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdImport.Click

        Try

            ImportQuote()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0048", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub
    Private Sub ImportQuote()

        Try

            If Me.txtSales.Text = "" Then
                MsgBox("Please select sales representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select customer", vbExclamation, "Message!")
                Exit Sub
            End If

            ofdImport.Title = "Please Select a File"
            ofdImport.InitialDirectory = "C:\Users\SBD_USER1\Desktop\RMS_IAN_FILES\SALES_AGENT_SQL_JOB"
            ofdImport.FileName = String.Empty

            Dim fileDialog As DialogResult = ofdImport.ShowDialog()

            If fileDialog = Windows.Forms.DialogResult.OK Then
                Dim frm As New frmUploader
                frm.lblFile.Text = ofdImport.FileName
                frm.ShowDialog(Me)
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0049", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub gridItem_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridItem.KeyUp

        Try

            If e.KeyCode = Keys.Up Or e.KeyCode = Keys.Down Then
                If gridItem.RowCount > 0 Then
                    sItemCode = gridItem.Item(0, gridItem.CurrentRow.Index).Value
                    sItemName = gridItem.Item(1, gridItem.CurrentRow.Index).Value
                End If
            End If

            If e.KeyCode = Keys.F5 Then
                TreeView1.Nodes.Clear()
                itemData.Filter = Nothing
                lblStatItemCount.Text = gridItem.RowCount & " items"
                txtSearch.Text = String.Empty
                Me.ActiveControl = txtSearch
            End If

            If e.KeyCode = Keys.F4 Then
                If txtSearch.Text = String.Empty Then
                    Try
                        Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                        TreeView1.Nodes.Remove(node(0))
                        FilterText()

                        If TreeView1.Nodes.Count = 0 Then
                            itemData.Filter = Nothing
                        Else
                            SearchItem()
                        End If

                        Me.ActiveControl = txtSearch
                    Catch ex As Exception

                    End Try
                Else
                    MessageBox.Show("Please Clear the search box before removing the last searched keyword", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
               

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0050", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub gridSelectItem_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridSelectItem.CellEndEdit

        Try

            'Try
            Dim NaN As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex("[^\d]")
            Dim str As String = gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value

            If e.ColumnIndex = 2 Then
                If str = String.Empty Or NaN.IsMatch(str) = True Then
                    gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = 0
                End If
            End If

            gridSelectItem.Rows(e.RowIndex).Selected = True
            gridSelectItem.CurrentCell = gridSelectItem.Rows(e.RowIndex).Cells(e.ColumnIndex)
            Dim editVal As Double = gridSelectItem.CurrentCell.Value

            If e.ColumnIndex = 2 Then

                If gridSelectItem.RowCount > 0 Then


                    iErrCnt = 0
                    ValidateAllQty()

                    If iErrCnt > 0 Then
                        MsgBox("Insufficient quantity to fullfill the request!", MsgBoxStyle.Exclamation, "Message")
                        gridSelectItem.CurrentCell.Value = curQty
                        gridSelectItem.CurrentCell.Style.ForeColor = Color.Black
                        gridSelectItem.CurrentCell.Style.SelectionForeColor = Color.Black
                    End If

                End If

                 Me.ActiveControl = txtSearch
            End If

            If e.ColumnIndex = 3 Then

                For x = 0 To iPrice

                    clsItemLookUp.LoadLevel(x, gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value, 0)

                Next x

                If bAllow = False Then

                    If editVal <= dLowest Then
                        If bFlag = True Then
                            frmPassword.sType = "EditPrice"
                            Dim pDialog As DialogResult = frmPassword.ShowDialog
                            If pDialog = Windows.Forms.DialogResult.Cancel Then
                                gridSelectItem.CurrentCell.Value = curPrice
                            End If
                        End If
                    End If
                End If



            End If

            UpdateAmt()
            'Catch ex As Exception
            '    MsgBox(ex.ToString)
            'End Try

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0051", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub cmdSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSettings.Click

        Try

            frmPassword.sType = "Settings"
            frmPassword.ShowDialog()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try


    End Sub

    Private Sub TreeView1_NodeMouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeNodeMouseClickEventArgs) Handles TreeView1.NodeMouseDoubleClick

        Try

            TreeView1.Nodes.Remove(TreeView1.SelectedNode)
            FilterText()
            SearchItem()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0052", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub cmdRecall_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRecall.Click

        Try

            frmRecall.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0053", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub RecallQuote(ByVal orderid As Integer)

        Try
            gridSelectItem.Rows.Clear()
            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            Dim sType, sComment As String, ReleaseType As String
            Dim iOrderType As Integer = 0

            For Each x In clsRecall.RecallOrder(orderid)
                sAcctNum = x.AccountNumber
                iPrice = x.PriceLevel
                bEmployee = x.Employee
                sTitle = x.Title
                bTaxExcempt = x.TaxExempt
                iCusID = x.CustID
                txtCustomer.Text = x.Company
                txtType.Text = x.CustomText5

                iSalesID = x.SalesRepID
                txtSales.Text = x.SaleRepName

                sType = Split(x.Comment, "; ")(0)
                ReleaseType = Split(x.Comment, "; ")(1)
                sComment = Split(x.Comment, "; ")(2)

                If ReleaseType = "Pick-up" Then
                    rbtnDelivery.Checked = False
                    rbtnPickup.Checked = True
                ElseIf ReleaseType = "Delivery" Then
                    rbtnPickup.Checked = False
                    rbtnDelivery.Checked = True
                End If

                txtRemarks.Text = Trim(sComment)
                cboPayment.Text = Trim(sType)


                iOrderType = x.Type
            Next

            For Each n In clsRecall.RecallOrderEntry(orderid)

                clsItemLookUp.GetPrice(1, n.Taxable, n.Price)
                Dim newTimeRecord As DataGridViewRow = gridSelectItem.Rows(gridSelectItem.NewRowIndex).Clone
                Dim iItemID = n.ItemId
                With record
                    newTimeRecord.Cells(ItemCode.Index).Value = n.ItemLookUpcode
                    newTimeRecord.Cells(ItemName.Index).Value = n.ItemName
                    newTimeRecord.Cells(QTY.Index).Value = n.QuantityOnOrder
                    newTimeRecord.Cells(Price.Index).Value = n.Price
                    newTimeRecord.Cells(DISC.Index).Value = n.DISC
                    newTimeRecord.Cells(TOTAL.Index).Value = n.Total
                    newTimeRecord.Cells(LessV.Index).Value = n.LessV
                    newTimeRecord.Cells(VSales.Index).Value = n.VSales
                    newTimeRecord.Cells(DiscP.Index).Value = n.DiscP
                    newTimeRecord.Cells(Cost.Index).Value = n.Cost
                    newTimeRecord.Cells(Taxable.Index).Value = n.Taxable
                    newTimeRecord.Cells(ItemID.Index).Value = n.ItemID
                    newTimeRecord.Cells(FullPrice.Index).Value = n.FullPrice
                    newTimeRecord.Cells(Description.Index).Value = n.Description
                    newTimeRecord.Cells(Extended.Index).Value = n.ExtendedDescription
                    newTimeRecord.Cells(DiscAmount.Index).Value = n.DiscAmount
                    newTimeRecord.Cells(OrderEntryID.Index).Value = n.ID

                    If n.Comment = "UP-STORE" Then
                        newTimeRecord.Cells(chkPickLoc.Index).Value = True
                    Else
                        newTimeRecord.Cells(chkPickLoc.Index).Value = False
                    End If
                    newTimeRecord.Cells(OrderEntryID.Index).Value = n.ID

                    Dim vqty As Integer = (From a In db.QueueingItems _
                               Join b In db.Queueings On a.QueueingID Equals b.id Where a.ItemID.Equals(iItemID) And b.OrderID.Equals(orderid) _
                               Select a.QtyPre).ToList()(0)

                    newTimeRecord.Cells(CustPrep.Index).Value = vqty

                End With

                dPrice = n.Price
                sOldDes = txtSearch.Text
                gridSelectItem.Rows.Add(newTimeRecord)

            Next

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            'gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)

            lblOrderNo.Text = IIf(iOrderType = 3, "Sales Quotation #: ", "Work Order #: ") & orderid
            lblStatItemSelected.Text = gridSelectItem.Rows.Count & " items selected"
            UpdateAmt()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0054", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Private Sub gridSelectItem_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSelectItem.SelectionChanged

        Try

            If gridSelectItem.CurrentCell.ColumnIndex = 2 Then
                'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.Font = New Font(Control.DefaultFont, FontStyle.Bold)
                'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.SelectionBackColor = Color.LightBlue
                'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.SelectionForeColor = Color.Black
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0055", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Public Function getUpdates(ByVal FirstDataTable As DataTable, ByVal SecondDataTable As DataTable) As DataTable
        Try
            Dim ResultDataTable As DataTable = New DataTable("ResultDataTable")

            Using ds As DataSet = New DataSet()

                ds.Tables.AddRange(New DataTable() {FirstDataTable.Copy(), SecondDataTable.Copy()})
                Dim firstColumns As DataColumn() = New DataColumn(ds.Tables(0).Columns.Count - 1) {}

                For i As Integer = 0 To firstColumns.Length - 1
                    firstColumns(i) = ds.Tables(0).Columns(i)
                Next

                Dim secondColumns As DataColumn() = New DataColumn(ds.Tables(1).Columns.Count - 1) {}

                For i As Integer = 0 To secondColumns.Length - 1
                    secondColumns(i) = ds.Tables(1).Columns(i)
                Next

                Dim r2 As DataRelation = New DataRelation(String.Empty, secondColumns, firstColumns, False)
                ds.Relations.Add(r2)

                For i As Integer = 0 To FirstDataTable.Columns.Count - 1
                    ResultDataTable.Columns.Add(FirstDataTable.Columns(i).ColumnName, FirstDataTable.Columns(i).DataType)
                Next

                ResultDataTable.BeginLoadData()

                For Each parentrow As DataRow In ds.Tables(1).Rows
                    Dim childrows As DataRow() = parentrow.GetChildRows(r2)
                    If childrows Is Nothing OrElse childrows.Length = 0 Then ResultDataTable.LoadDataRow(parentrow.ItemArray, True)
                Next

                ResultDataTable.EndLoadData()

            End Using

            Return ResultDataTable
        Catch ex As Exception

            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0056", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function


    Public Sub SetItemAvailable(ByVal Itemcode As String, ByVal Available As Integer)

        Try
            Dim result As DataRow() = clsItemLookUp.dt1.Select("ItemCode = '" & Itemcode & "' ")

            If result.Count > 0 Then
                result.First.SetField(Of Integer)("Available", Available)
                clsItemLookUp.dt3.AcceptChanges()
            End If

        Catch ex As Exception
            '   MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0057", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub updateItemData()

        Try
            ToolStripStatusLabel1.Text = "STATUS : Updating Items ...... "

            clsItemLookUp.getLastUpdatedItems()

            Dim count = clsItemLookUp.dt3.Rows.Count

            If Not count = 0 Then

                updateSuccess = False

                For Each r As DataRow In clsItemLookUp.dt3.Rows

                    Try
                        SetItemAvailable(r("ItemCode").ToString(), Convert.ToInt64(r("Available")))
                    Catch ex As Exception
                    End Try

                Next


                updateSuccess = True

            End If

            ToolStripStatusLabel1.Text = "STATUS :"


        Catch ex As Exception

            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0058", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1

        End Try

    End Sub

    Private Sub Update_Timer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Update_Timer.Tick

        If updateSuccess = True Then

            If Not db_lastupdate = clsItemLookUp.checkTableUpdate() Then

                db_lastupdate = clsItemLookUp.checkTableUpdate()

                Try

                    Thread_1 = New Thread(AddressOf updateItemData)
                    Thread_1.Start()

                Catch ex As Exception
                    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0059", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ErrorCount = ErrorCount + 1
                End Try

            End If

        End If


    End Sub

    Private Sub cmdLogout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdLogout.Click
        Try
            RefreshDetails()

            Me.ToolStripStatusLabel3.Text = "Register : "
            Me.ToolStripStatusLabel2.Text = "User : "

            itemData.DataSource = Nothing
            Update_Timer.Enabled = False
            frmLogin.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0060", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
      
    End Sub

    Public Sub printWO(ByVal iOrderID As String, ByVal type As Integer)
        Try
            If type = 1 Then

                '  Dim res As Integer

                '   res = MsgBox("Do you want to print the Work Order ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
                '  If res = MsgBoxResult.Yes Then
                frmPrintWo.wo = iOrderID
                frmPrintWo.ShowDialog()
                'End If


            ElseIf type = 2 Then

                If rbtnDelivery.Checked Then
                    frmPrintWo.wo = iOrderID
                    frmPrintWo.ShowDialog()
                End If

            End If


        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0061", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)
        frmPrintWo.ShowDialog()
    End Sub

    Private Sub Check_errors_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Check_errors.Tick

        If ErrorCount >= 5 Then
            Update_Timer.Stop()
            Check_errors.Stop()
            MessageBox.Show("The Program Exceeded the Allowed Errors!" & vbCrLf & vbCrLf & "Program will be Restarted !", "IMPORTANT MESSAGE!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Application.Restart()
        End If
    End Sub

    Private Sub cmdForInvoice_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdForInvoice.Click
        frmForInvoice.ShowDialog()
    End Sub
End Class