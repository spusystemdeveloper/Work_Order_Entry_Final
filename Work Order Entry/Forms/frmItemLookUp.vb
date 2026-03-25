Imports System.Data.Linq
Imports System.Data.SqlTypes
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports System.IO
Imports System.Runtime.Remoting.Contexts
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web.Services
Imports ExcelDataReader
Imports Microsoft.Reporting.WinForms
Imports Mysqlx.Crud
Imports ZstdSharp.Unsafe

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
    Dim itemImage As Image


    Private previousPrice As Decimal

    'comment:
    Dim searchStr As String = ""

    Public Shared Thread_1 As Thread
    Public Shared Thread_2 As Thread

    Public Shared usrID As String
    Public Shared usrRegister As String
    Public Shared usrFullname As String
    Public Shared usrUsername As String
    Public Shared DatagridColor As Integer = 0

    Dim fromImport As Boolean

    Dim updateSuccess As Boolean = True

    Public Shared ItemOrderedList As List(Of clsPickList) = New List(Of clsPickList)()


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
                ElseIf txtSearch.Text.Contains("'") Then

                    MessageBox.Show("Searching ' or Apostrophe is not Allowed !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                Else


                    FilterText()
                    txtSearch.Text = String.Empty
                    SearchItem()
                End If

                txtSearch.Text = String.Empty

            End If

            If Me.txtSearch.Focused = False Then
                'searching
                'txtSearch.Focus()
                'txtSearch.Text = e.KeyChar.ToString
                'txtSearch.SelectionStart = txtSearch.Text.Length
                'e.Handled = True
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
            FilterString = String.Join("%' AND [FULLDESC] Like '%", SearchStrArr)
            Dim FilterStr = "SELECT TOP 500 * FROM SOD_VIEWITEMSWO  WHERE Inactive = 0 AND FULLDESC Like '%" & FilterString & "%'"
            'Dim FilterStr = "SELECT ITEMLOOKUPCODE AS ITEMCODE,DESCRIPTION AS 'ITEM NAME',PRICE AS 'ITEM PRICE',COST,AVAILABLE,ParentQuantity,SKULEVEL FROM SOD_VIEWITEMS  WHERE Inactive = 0 AND FULLDESC Like '%" & FilterString & "%'"

            'itemData.Filter = FilterStr
            gridItem.DataSource = load_data(FilterStr)
            txtSearch.Focus()
            GridColumnWidth()
            checkSearch()
            iRow = 0
            'searching
            'txtSearch.Focus()
            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1

        End Try

    End Sub

    Private Sub checkSearch()

        Try

            If gridItem.RowCount = 0 Then

                MessageBox.Show("No Items Found!", "Message Error !", MessageBoxButtons.OK, MessageBoxIcon.Error)


                Try
                    Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                    TreeView1.Nodes.Remove(node(0))
                    FilterText()

                    If TreeView1.Nodes.Count = 0 Then
                        gridItem.DataSource = getItem()
                    Else
                        SearchItem()
                    End If

                Catch ex As Exception

                End Try
                'searching
                'Me.ActiveControl = txtSearch

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    'Private Sub GridColumnWidth()

    '    Try

    '        With gridItem
    '            .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            '.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnsMode.Fill
    '            .Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            .Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            .Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells

    '            .Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            .Columns(7).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            '.Columns(17).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    '            '.Columns(18).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells

    '            .Columns(0).Frozen = True
    '            '.Columns(1).MinimumWidth = 450
    '            .Columns(1).MinimumWidth = 1000
    '            'for column width
    '            '.Columns(0).Width = 100
    '            '.Columns(1).Width = 400
    '            '.Columns(2).Width = 90
    '            '.Columns(3).Width = 0
    '            '.Columns(4).Width = 90
    '            '.Columns(5).Width = 100
    '            'for column name
    '            .Columns(0).DefaultCellStyle.Font = New Font("Arial", 10, FontStyle.Bold)
    '            .Columns(0).HeaderText = "ITEM CODE"
    '            .Columns(1).HeaderText = "ITEM NAME"
    '            .Columns(2).HeaderText = "ITEM PRICE"
    '            .Columns(3).HeaderText = "COST"
    '            .Columns(4).HeaderText = "QUANTITY"
    '            .Columns(5).HeaderText = "CHILD QTY"
    '            .Columns(7).HeaderText = "SKU Level"
    '            .Columns(17).HeaderText = "SOH"
    '            .Columns(18).HeaderText = "QTY COM"

    '            'for format & alignment
    '            .Columns(2).DefaultCellStyle.Format = "C"
    '            .Columns(3).DefaultCellStyle.Format = "N2"
    '            .Columns(4).DefaultCellStyle.Format = "N0"
    '            .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
    '            .Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
    '            .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
    '            .Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
    '            .Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
    '            .Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter
    '            .Columns(17).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
    '            .Columns(17).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
    '            .Columns(18).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
    '            .Columns(18).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter


    '            .Columns(3).Visible = False
    '            .Columns(5).Visible = False
    '            .Columns(6).Visible = False
    '            .Columns(7).Visible = False
    '            .Columns(8).Visible = False
    '            .Columns(9).Visible = False
    '            .Columns(10).Visible = False
    '            .Columns(11).Visible = False
    '            .Columns(12).Visible = False
    '            .Columns(13).Visible = False
    '            .Columns(14).Visible = False
    '            .Columns(15).Visible = False
    '            .Columns(16).Visible = False
    '        End With

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try


    'End Sub

    Private Sub GridColumnWidth()

        Try

            With gridItem

                ' Auto size columns
                .Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill ' ✅ for wrapping
                .Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(5).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
                .Columns(7).AutoSizeMode = DataGridViewAutoSizeColumnsMode.DisplayedCells

                ' Enable row auto height (IMPORTANT for wrap)
                .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells

                ' Freeze first column
                .Columns(0).Frozen = True

                ' Remove this (prevents wrapping)
                '.Columns(1).MinimumWidth = 1000

                ' Column headers
                .Columns(0).DefaultCellStyle.Font = New Font("Arial", 12, FontStyle.Bold)
                .Columns(0).HeaderText = "ITEM CODE"
                .Columns(1).HeaderText = "ITEM NAME"
                .Columns(2).HeaderText = "ITEM PRICE"
                .Columns(3).HeaderText = "COST"
                .Columns(4).HeaderText = "QUANTITY"
                .Columns(5).HeaderText = "CHILD QTY"
                .Columns(7).HeaderText = "SKU Level"
                .Columns(17).HeaderText = "SOH"
                .Columns(18).HeaderText = "QTY COM"

                ' ✅ Wrap + styling for ITEM NAME
                .Columns(1).DefaultCellStyle.WrapMode = DataGridViewTriState.True
                .Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft
                .Columns(1).DefaultCellStyle.Padding = New Padding(5)

                ' Format & alignment
                .Columns(2).DefaultCellStyle.Format = "C"
                .Columns(3).DefaultCellStyle.Format = "N2"
                .Columns(4).DefaultCellStyle.Format = "N0"

                .Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight
                .Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight

                .Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight

                .Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

                .Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter

                .Columns(17).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(17).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

                .Columns(18).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                .Columns(18).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

                ' Hide columns
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

    ''Make loaded items appear in bold and larger font in the grid
    'Private Sub gridItem_CellFormatting(ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs) Handles gridItem.CellFormatting
    '    Try
    '        'Ignore header row and new row
    '        If e.RowIndex < 0 OrElse e.RowIndex >= gridItem.Rows.Count Then Exit Sub

    '        Dim row As DataGridViewRow = gridItem.Rows(e.RowIndex)

    '        ' TODO: Adjust this logic to match your own definition of "loaded"
    '        ' Example below: treat the item as loaded if "QTY COM" (column 18) is greater than 0
    '        Dim isLoaded As Boolean = False
    '        Try
    '            'Use the column index or name that represents loaded quantity/status
    '            Dim qtyComIndex As Integer = 18 ' change if your "loaded" column is different
    '            If qtyComIndex >= 0 AndAlso qtyComIndex < row.Cells.Count Then
    '                Dim val = row.Cells(qtyComIndex).Value
    '                If val IsNot Nothing AndAlso IsNumeric(val) AndAlso CDec(val) > 0D Then
    '                    isLoaded = True
    '                End If
    '            End If
    '        Catch
    '            'Ignore per-row errors in determining loaded state
    '        End Try

    '        'Set a larger base font size for the ITEM ROWS only (do not touch the header font)
    '        'You can adjust the size (e.g., 11, 12, 13) to whatever you prefer.
    '        Dim baseSize As Single = 12.0F

    '        'Choose a safe base font for cells (fallback if DefaultCellStyle.Font is not set)
    '        Dim baseFamily As FontFamily
    '        Try
    '            'Force Arial for item rows
    '            baseFamily = New FontFamily("Calibri")
    '        Catch
    '            'If Arial is not available for some reason, fall back to the grid's current font family
    '            Dim fallbackFont As Font = gridItem.DefaultCellStyle.Font
    '            If fallbackFont Is Nothing Then
    '                fallbackFont = gridItem.Font
    '            End If
    '            If fallbackFont Is Nothing Then
    '                fallbackFont = Me.Font
    '            End If
    '            baseFamily = fallbackFont.FontFamily
    '        End Try

    '        'All item rows bold in Arial; loaded items a bit bigger and bold
    '        Dim normalFont As New Font(baseFamily, baseSize, FontStyle.Bold)
    '        Dim loadedFont As New Font(baseFamily, baseSize + 2.0F, FontStyle.Bold)

    '        'Increase row height a bit more to give extra vertical spacing between rows
    '        If gridItem.RowTemplate.Height < 26 Then
    '            gridItem.RowTemplate.Height = 26
    '        End If

    '        If isLoaded Then
    '            'Bold and clearly larger font for LOADED items (item rows only)
    '            e.CellStyle.Font = loadedFont
    '        Else
    '            'Normal (but larger) font for NOT loaded items
    '            e.CellStyle.Font = normalFont
    '        End If

    '    Catch
    '        'Ignore formatting-time exceptions to avoid breaking the grid
    '    End Try
    'End Sub
    Private Sub gridItem_CellFormatting(ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs) Handles gridItem.CellFormatting
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= gridItem.Rows.Count Then Exit Sub

            Dim row As DataGridViewRow = gridItem.Rows(e.RowIndex)

            ' Detect loaded
            Dim isLoaded As Boolean = False
            Dim qtyComIndex As Integer = 18

            If qtyComIndex < row.Cells.Count Then
                Dim val = row.Cells(qtyComIndex).Value
                If val IsNot Nothing AndAlso IsNumeric(val) AndAlso CDec(val) > 0D Then
                    isLoaded = True
                End If
            End If

            ' Base font
            Dim baseFont As New Font("Arial", 12.0F, FontStyle.Bold)
            Dim loadedFont As New Font("Arial", 12.0F, FontStyle.Bold)

            ' ✅ APPLY TO WHOLE ROW (not per cell)
            If isLoaded Then
                row.DefaultCellStyle.Font = loadedFont
            Else
                row.DefaultCellStyle.Font = baseFont
            End If

        Catch
        End Try
    End Sub
    Private Sub txtSearch_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyUp

        Try
            If e.KeyCode = Keys.F5 Then

                TreeView1.Nodes.Clear()
                gridItem.DataSource = getItem()
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
                            gridItem.DataSource = getItem()
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


    'comment
    Public Sub InsertSelectedItem(ByVal i As Integer)
        'Try
        '    Dim itemCodeExists As Boolean = False

        '    ' Check if the item code already exists in the gridSelectItem
        '    For Each row As DataGridViewRow In gridSelectItem.Rows
        '        If row.Cells(ItemCode.Index).Value IsNot Nothing AndAlso row.Cells(ItemCode.Index).Value.ToString() = gridItem.Item(0, i).Value.ToString() Then
        '            ' Item code exists, update the quantity
        '            row.Cells(QTY.Index).Value = CInt(row.Cells(QTY.Index).Value) + 1
        '            ' Update the total price based on the new quantity
        '            row.Cells(TOTAL.Index).Value = CInt(row.Cells(QTY.Index).Value) * CDbl(row.Cells(Price.Index).Value)
        '            itemCodeExists = True
        '            Exit For
        '        End If
        '    Next

        '    ' If the item code does not exist, insert the new item
        '    If Not itemCodeExists Then
        '        Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
        '        gridSelectItem.AllowUserToAddRows = True

        '        Dim newTimeRecord As DataGridViewRow = gridSelectItem.Rows(gridSelectItem.NewRowIndex).Clone

        '        With record
        '            newTimeRecord.Cells(ItemCode.Index).Value = gridItem.Item(0, i).Value
        '            newTimeRecord.Cells(ItemName.Index).Value = gridItem.Item(1, i).Value
        '            newTimeRecord.Cells(QTY.Index).Value = "1"
        '            newTimeRecord.Cells(CustPrep.Index).Value = "0"

        '            If bTaxExcempt = True Then
        '                newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
        '            Else
        '                newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
        '            End If

        '            newTimeRecord.Cells(DISC.Index).Value = 0
        '            newTimeRecord.Cells(TOTAL.Index).Value = newTimeRecord.Cells(QTY.Index).Value * newTimeRecord.Cells(Price.Index).Value
        '            newTimeRecord.Cells(LessV.Index).Value = dTSales
        '            newTimeRecord.Cells(VSales.Index).Value = dVSales
        '            newTimeRecord.Cells(DiscP.Index).Value = dDisc
        '            newTimeRecord.Cells(Cost.Index).Value = gridItem.Item(3, i).Value
        '            newTimeRecord.Cells(Taxable.Index).Value = gridItem.Item(6, i).Value
        '            newTimeRecord.Cells(ItemID.Index).Value = gridItem.Item(9, i).Value
        '            newTimeRecord.Cells(FullPrice.Index).Value = gridItem.Item(8, i).Value
        '            newTimeRecord.Cells(Description.Index).Value = gridItem.Item(10, i).Value
        '            newTimeRecord.Cells(Extended.Index).Value = gridItem.Item(11, i).Value & clsItemLookUp.TagSC(sTitle, gridItem.Item(7, i).Value)
        '            newTimeRecord.Cells(DiscAmount.Index).Value = 0
        '        End With

        '        gridSelectItem.Rows.Add(newTimeRecord)

        '        gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
        '        gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)
        '    End If

        'Catch ex As Exception
        '    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '    ErrorCount += 1
        'End Try

        Try
            Dim itemCodeExists As Boolean = False

            ' Check if the item code already exists in the gridSelectItem

            For Each row As DataGridViewRow In gridSelectItem.Rows
                If row.Cells(ItemCode.Index).Value IsNot Nothing AndAlso row.Cells(ItemCode.Index).Value.ToString() = gridItem.Item(0, i).Value.ToString() Then
                    ' Item code exists, update the quantity
                    row.Cells(QTY.Index).Value = CInt(row.Cells(QTY.Index).Value) + 1
                    'row.Cells(QTY.Index).Value = row.Cells(QTY.Index).Value + 1

                    ' Update the total price based on the new quantity
                    row.Cells(TOTAL.Index).Value = row.Cells(QTY.Index).Value * row.Cells(Price.Index).Value
                    itemCodeExists = True
                    Exit For
                End If
            Next

            ' If the item code does not exist, insert the new item
            If Not itemCodeExists Then
                Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
                gridSelectItem.AllowUserToAddRows = True

                Dim newTimeRecord As DataGridViewRow = gridSelectItem.Rows(gridSelectItem.NewRowIndex).Clone

                With record
                    newTimeRecord.Cells(ItemCode.Index).Value = gridItem.Item(0, i).Value
                    newTimeRecord.Cells(ItemName.Index).Value = gridItem.Item(1, i).Value
                    newTimeRecord.Cells(QTY.Index).Value = 1
                    newTimeRecord.Cells(CustPrep.Index).Value = "0"

                    'If bTaxExcempt = True Then
                    '    newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
                    'Else
                    '    newTimeRecord.Cells(Price.Index).Value = Format(frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value, "#,##0.00")
                    'End If

                    Dim rawValue As Object = frmPriceLevel.gridLevel.Item(1, frmPriceLevel.gridLevel.CurrentRow.Index).Value
                    Dim decimalValue As Decimal = 0D

                    If rawValue IsNot Nothing AndAlso Decimal.TryParse(rawValue.ToString(), decimalValue) Then
                        If bTaxExcempt Then
                            newTimeRecord.Cells(Price.Index).Value = decimalValue
                        Else
                            newTimeRecord.Cells(Price.Index).Value = decimalValue
                        End If
                    Else
                        newTimeRecord.Cells(Price.Index).Value = 0D
                    End If

                    newTimeRecord.Cells(DISC.Index).Value = 0
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
                    newTimeRecord.Cells(LASTPURCHASEDPRICE.Index).Value = clsRecall.GetItemLastPrice(txtCustomer.Text, (gridItem.Item(0, i).Value))

                End With

                gridSelectItem.Rows.Add(newTimeRecord)


                gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
                gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
        End Try
    End Sub

    Private Sub gridItem_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridItem.DoubleClick

        Try
            If gridItem.Rows.Count = 0 Then
                Exit Sub
            End If

            If iCusID = 0 Then
                MsgBox("Please select customer first!", vbExclamation, "Message")
                frmCustomer.Show()
            Else
                SelectPrice()
            End If

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
                'comment
                availVal = clsItemLookUp.getItemQty(sItemCode)

                If availVal < 1 And chkWorkOrder.Checked = True Then
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
                        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ErrorCount = ErrorCount + 1
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

                    'comment
                    If chkWorkOrder.Checked = True Then
                        ValidateAllQty()
                    End If
                End If
            End If

            If gridSelectItem.Rows.Count = 0 Then
                dTotalTSales = 0
                dTotalVSales = 0
                dTotalSales = 0
                txtSub.Text = Format(dTotalTSales, "#,##0.00")
                txtVat.Text = Format(dTotalVSales, "#,##0.00")
                txtTotal.Text = Format(dTotalSales, "#,##0.00")
                'searching
                'txtSearch.Focus()
            Else
                UpdateAmt()
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOk.Click

        SaveOrderHandler()

    End Sub

    Public Sub SaveOrderHandler()
        Try
            If chkBoxQtoWo.Checked = True And chkQuote.Checked = True Then

                Dim selectionForm As New frmRecallSelection("SaveToQuote")
                Dim result As DialogResult = selectionForm.ShowDialog()

                If result = DialogResult.OK Then
                    Select Case selectionForm.SelectedOption
                        Case "Save Changes"
                            If Not ValidateAndSave() Then
                                Exit Sub
                            End If
                        Case "Convert to a work order"
                            convertToWorkOrder()
                            MessageBox.Show("Converted To Work Order! ", "Recall", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Case Else
                            MessageBox.Show("Option not implemented: " & selectionForm.SelectedOption, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End Select
                End If
            Else
                If Not ValidateAndSave() Then
                    Exit Sub
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0013", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    Public Sub convertToWorkOrder()

        gridSelectItem.Columns(17).Visible = True
        gridSelectItem.Columns(18).Visible = True
        cboPayment.Enabled = True
        chkQuote.Checked = False
        chkWorkOrder.Checked = True
        WoRecallType = 1

    End Sub

    Private Sub SaveOrder()
        Try
            If gridSelectItem.RowCount <= 0 Then Exit Sub

            iErrCnt = 0
            Dim isWorkOrder As Boolean = chkWorkOrder.Checked
            Dim isQuote As Boolean = chkQuote.Checked
            Dim isQtoWo As Boolean = chkBoxQtoWo.Checked

            ' === VALIDATION PHASE ===
            If (isWorkOrder AndAlso Not isQtoWo) OrElse (isQuote AndAlso isQtoWo) Then
                ValidateAllQty()
            ElseIf (isWorkOrder AndAlso isQtoWo) Then
                'MessageBox.Show("If naka check ang wo, ug is converted to quote mag validate sya ug qty")
                ValidateAllQtyForQoute()
            ElseIf isQuote AndAlso Not isQtoWo Then
                ' Validate qty not zero for quote
                For Each row As DataGridViewRow In gridSelectItem.Rows
                    If Not row.IsNewRow Then
                        Dim qtyValue As Object = row.Cells(2).Value
                        If qtyValue IsNot Nothing AndAlso IsNumeric(qtyValue) Then
                            If Convert.ToDecimal(qtyValue) = 0 Then
                                MessageBox.Show("Quantity cannot be zero. Please correct the item before proceeding.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                gridSelectItem.CurrentCell = row.Cells(2)
                                gridSelectItem.BeginEdit(True)
                                Exit Sub
                            End If
                        End If
                    End If
                Next
            End If

            ' === HANDLE VALIDATION ERRORS ===
            If iErrCnt > 0 Then
                RefreshItemList()
                Exit Sub
            End If

            ' === PROCEED WITH ORDER SAVE ===
            forDelivery()

            Dim orderNoParts() As String = Split(lblOrderNo.Text, ": ")
            If orderNoParts.Length > 1 AndAlso IsNumeric(orderNoParts(1)) Then
                UpdateOrder(orderNoParts(1))
            Else
                InsertEntry()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
        End Try
    End Sub
    Private Sub RefreshItemList()
        Cursor.Current = Cursors.WaitCursor

        TreeView1.Nodes.Clear()
        gridItem.DataSource = getItem()
        lblStatItemCount.Text = gridItem.RowCount & " items"
        txtSearch.Text = String.Empty
        'searching
        'Me.ActiveControl = txtSearch

        Cursor.Current = Cursors.Default
    End Sub

    Public Function getReleaseType() As String

        If rbtnPickup.Checked Then
            Return "Pick-up"
        ElseIf rbtnDelivery.Checked Then
            Return "Delivery"
        Else
            Return Nothing
        End If

    End Function

    Public Function getEntryType() As Integer

        If chkWorkOrder.Checked Then
            Return 2
        ElseIf chkQuote.Checked Then
            Return 3
        ElseIf chkBoxQtoWo.Checked Then
            Return 4
        Else
            Return Nothing
        End If

    End Function

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

            If Me.txtSales.Text = "" Then
                MsgBox("Please select a Sales Representative", vbExclamation, "Message!")
                frmSalesRep.Show()
                Exit Sub
            ElseIf Me.cboPayment.Text = "" And getEntryType() = 2 Then
                MsgBox("Please select a Payment Method", vbExclamation, "Message!")
                cboPayment.Focus()
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select a Customer", vbExclamation, "Message!")
                frmCustomer.Show()
                Exit Sub
            ElseIf gridSelectItem.Rows.Count = 0 Then
                MsgBox("Please select Item to order", vbExclamation, "Message!")
                Exit Sub
            End If

            For x As Integer = 0 To gridSelectItem.Rows.Count - 1
                For y As Integer = 0 To gridSelectItem.Rows.Count - 1
                    If y <> x AndAlso gridSelectItem.Rows(x).Cells(0).Value.ToString = gridSelectItem.Rows(y).Cells(0).Value.ToString Then

                        Dim res As Integer
                        res = MsgBox("There are Same Items in one Entry! Please Sum up the Qty Order!" & vbCrLf & vbCrLf & "This might cause an issue in the future." & vbCrLf & vbCrLf & "Do you wish to continue ?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Message!")
                        If res = MsgBoxResult.Yes Then
                            GoTo Proceed
                        ElseIf res = MsgBoxResult.No Then
                            Exit Sub
                        End If
                    End If
                Next
            Next

Proceed:

            If getEntryType() = 2 Then
                iReturnValue = MsgBox("Do you want to save this entry as Work Order ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            ElseIf getEntryType() = 3 Then
                iReturnValue = MsgBox("Do you want to save this entry as Sales Quotation ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            End If

            If iReturnValue = MsgBoxResult.Yes Then

                Cursor.Current = Cursors.WaitCursor

                Dim orderComment As String = ""

                If getEntryType() = 2 Then

                    orderComment = cboPayment.Text & "; " & getReleaseType() & "; " & txtRemarks.Text

                Else

                    orderComment = txtRemarks.Text

                End If

                iOrderID = db.SOD_sp_InsertQoute(sReg, iCusID, iSalesID, Double.Parse(txtVat.Text), Double.Parse(txtTotal.Text), orderComment, usrUsername.ToUpper, getEntryType())

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

                    db.SOD_sp_InsertQouteEntry(dCost, iOrderID, iItemID, dFullPrice, dPrice, dQuantityOnOrder, iSalesRepID, iTaxable, sDescription, ipickLoc, qtyPrep, getEntryType())

                    If ispriceApproved = 1 AndAlso IsPriceBelowAllowed(iItemID, dPrice) Then

                        InsertPriceLogs(iOrderID, iItemID, dFullPrice, dPrice, frmPassword.txtPass.Text, "Inserted")

                    End If

                    If getEntryType() = 2 Then

                        clsItemLookUp.setQuantityCommitted(iItemID, dQuantityOnOrder)

                    End If

                    If dQuantityOnOrder = qtyPrep Then

                        clsItemLookUp.updateQueueStat(iOrderID, iItemID)

                    End If

                Next

                If getEntryType() = 2 Then

                    MessageBox.Show("Successfully Saved into Work Order !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    clsItemLookUp.fakeUpdate(iOrderID)

                    'checkCustomerGroupWo(iCusID, iOrderID, "Insert")
                    prompWO(iOrderID)

                    'removed pick list if mag save ug work order
                    'If rbtnPickup.Checked = True Then

                    '    If checkIfpickupALL() = False Then
                    '        frmPrintPicklist.wo = iOrderID
                    '        frmPrintPicklist.ShowDialog()
                    '    End If

                    'End If

                ElseIf getEntryType() = 3 Then
                    MessageBox.Show("Successfully Saved into Sales Quotation !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    frmPrintWo.Type = "Sales Quotation"
                    frmPrintWo.wo = iOrderID
                    frmPrintWo.ShowDialog()
                End If

                RefreshDetails(True)

                Cursor.Current = Cursors.Default
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    'comment
    Public Sub setQtyCommittedQuotetoWo()

        Dim iRow As Integer
        Dim iItemID As Integer
        Dim dQuantityOnOrder As Double

        For iRow = 0 To gridSelectItem.Rows.Count - 1

            Dim sItem = (From i In db.Items Where i.ID.Equals(iItemID) Select i.QuantityCommitted).FirstOrDefault

            iItemID = gridSelectItem.Item(11, iRow).Value
            dQuantityOnOrder = gridSelectItem.Item(2, iRow).Value
            clsItemLookUp.SetQuantityCommittedToQoute(iItemID, dQuantityOnOrder)
        Next
        Exit Sub
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
            Dim iOrderEntryID As Integer

            If Me.txtSales.Text = "" And getEntryType() = 2 Then
                MsgBox("Please select sales representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" And getEntryType() = 2 Then
                MsgBox("Please select customer", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.cboPayment.Text = "" And getEntryType() = 2 Then
                MsgBox("Please select Payment Type", vbExclamation, "Message!")
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

                Dim existingOrder = (From q In db.Queueings
                                     Where q.OrderID = orderid
                                     Select q).FirstOrDefault()

                If existingOrder Is Nothing Then

                    Dim UpQueueing As New ItemLookUpDataContext(DB_Conn("constr"))

                    Dim newQueue As New Queueing With {
                        .OrderID = orderid,
                            .Status = 0,
                               .GroupTo = orderid,
                                    .OPIS = usrUsername.ToUpper
                                }
                    UpQueueing.Queueings.InsertOnSubmit(newQueue)
                    UpQueueing.SubmitChanges()

                End If

                Dim orderComment As String = ""

                If getEntryType() = 2 Then
                    orderComment = cboPayment.Text & "; " & getReleaseType() & "; " & txtRemarks.Text
                Else
                    orderComment = txtRemarks.Text
                End If

                clsRecall.UpdateOrder(getEntryType(), orderid, iCusID, iSalesID, Double.Parse(txtVat.Text), Double.Parse(txtTotal.Text), orderComment)

                For iRow = 0 To gridSelectItem.Rows.Count - 1

                    dCost = gridSelectItem.Item(9, iRow).Value
                    iOrderID = orderid
                    iItemID = gridSelectItem.Item(11, iRow).Value
                    dFullPrice = gridSelectItem.Item(12, iRow).Value
                    dPrice = gridSelectItem.Item(3, iRow).Value
                    dQuantityOnOrder = gridSelectItem.Item(2, iRow).Value
                    iSalesRepID = iSalesID
                    iOrderEntryID = gridSelectItem.Item(16, iRow).Value

                    If bTaxExcempt = 0 Then
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

                    Dim existingOrder2 As Long = (From q In db.Queueings
                                                  Where q.OrderID = orderid
                                                  Select q.id).FirstOrDefault()

                    If existingOrder2 > 0 Then

                        Dim getQ = (From q In db.QueueingItems
                                    Where q.QueueingID = existingOrder2 And q.ItemID = iItemID
                                    Select q).FirstOrDefault()

                        If getQ Is Nothing Then
                            Dim newQueues As New QueueingItem With {
                                .QueueingID = existingOrder2,
                                .ItemID = iItemID,
                                .PickLoc = pickLoc
                            }
                            db.QueueingItems.InsertOnSubmit(newQueues)
                            db.SubmitChanges()
                        End If
                    End If

                    If ispriceApproved = 1 AndAlso IsPriceBelowAllowed(iItemID, dPrice) Then

                        If ifItemExistInOrder(iItemID, iOrderID) Then

                            InsertPriceLogs(iOrderID, iItemID, dFullPrice, dPrice, frmPassword.txtPass.Text, "Updated")

                        Else

                            InsertPriceLogs(iOrderID, iItemID, dFullPrice, dPrice, frmPassword.txtPass.Text, "Inserted")

                        End If

                    End If

                    clsRecall.UpdateOrderEntry(iOrderID, iItemID, iSalesID, dCost, dFullPrice, dPrice, dQuantityOnOrder, iTaxable, sDescription, pickLoc, qtyPrep, iOrderEntryID)

                    If dQuantityOnOrder = qtyPrep Then

                        clsItemLookUp.updateQueueStat(iOrderID, iItemID)

                    End If
                Next

                clsRecall.DeleteOrderItem(iOrderID, delitems)

                If getEntryType() = 2 Then
                    MessageBox.Show("Work Order Successfully Updated !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    'checkCustomerGroupWo(iCusID, iOrderID, "Update")
                    prompWO(iOrderID)

                ElseIf getEntryType() = 3 Then
                    MessageBox.Show("Sales Quotation Successfully Updated !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    'frmPrintWo.wo = iOrderID
                    'frmPrintWo.ShowDialog()
                End If

                RefreshDetails(True)

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

                dCurPrice = gridSelectItem.Item("Price", iRow).Value
                iVat = gridSelectItem.Item(10, iRow).Value

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
            txtBarcode.Text = ""
            txtBarcode.Focus()

        Catch ex As Exception

        End Try


    End Sub

    Private Sub cmdRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRemove.Click

        Try
            removeItems()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0018", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click

        Try

            If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

                'CancelOrder()

                frmPassword.sType = "CancelOrder"
                Dim pDialog As DialogResult = frmPassword.ShowDialog
                If pDialog = Windows.Forms.DialogResult.Cancel Then
                    MsgBox("Order was not cancelled!", vbExclamation, "Message")
                Else
                    CancelOrder()
                End If

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0019", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub CancelOrder()

        Try
            Dim iMsg As Integer
            iMsg = MsgBox("Do you want to cancel this order?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            If iMsg = MsgBoxResult.Yes Then

                clsRecall.CancelOrder(Split(lblOrderNo.Text, ": ")(1))
                RefreshDetails(True)

                MsgBox("Order was cancelled successfully!", vbInformation, "Message")

            Else
                MsgBox("Order was not cancelled!", vbExclamation, "Message")
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0020", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
                    'txtSearch.Focus()
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0021", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelect.Click

        Try

            If gridItem.RowCount > 0 Then
                SelectPrice()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0022", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub frmItemLookUp_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown

        Try

            If e.KeyCode = Keys.F3 Then
                ' Toggle the checkbox state for chkBarcode
                If chkBarcode.Checked Then
                    ' If barcode is checked, uncheck it and enable txtSearch
                    chkBarcode.Checked = False
                    txtSearch.Enabled = True
                    txtSearch.Text = ""
                    txtBarcode.Enabled = False
                    txtBarcode.Text = "Press F3 To Scan barcode"
                    TreeView1.Nodes.Clear()
                    txtSearch.Focus()
                Else
                    ' If barcode is not checked, check it and enable txtBarcode
                    chkBarcode.Checked = True
                    txtSearch.Enabled = False
                    txtSearch.Text = "Press F3 To Search items"
                    txtBarcode.Enabled = True
                    txtBarcode.Text = ""
                    TreeView1.Nodes.Clear()
                    txtBarcode.Focus()
                End If
            End If
            If e.KeyCode = Keys.F12 Then
                SaveOrderHandler()
            ElseIf e.KeyCode = Keys.F11 Then
                'frmRecallSelection.ShowDialog()
                recallOrders()
                'frmRecall.ShowDialog()
            ElseIf e.KeyCode = Keys.F6 Then
                cmdImport_Click(sender, e)
            ElseIf e.KeyCode = Keys.F9 Then
                cmdCancel_Click(sender, e)
            ElseIf e.KeyCode = Keys.F7 Then
                frmSalesRep.ShowDialog()
            ElseIf e.KeyCode = Keys.F8 Then
                ShowCustomer()
                'Additional code for Barcode gnome
                'ElseIf e.KeyCode = Keys.F3 Then
                '    chkBarcode.Checked = True
                'ElseIf e.KeyCode = Keys.F4 Then
                '    chkBarcode.Checked = False
                '    txtBarcode.Text = "Search Barcode"
            ElseIf e.KeyCode = Keys.F2 Then
                FilterText()
                txtSearch.Text = String.Empty
                SearchItem()
            ElseIf e.KeyCode = Keys.Escape Then
                If MsgBox("Do you really want To close Work/Sales Order Entry?", vbQuestion + vbYesNo, "Quit") = vbYes Then
                    Me.Close()
                End If
            ElseIf (e.KeyCode And Not Keys.Modifiers) = Keys.D1 AndAlso e.Modifiers = Keys.Control Then
                gridItem.Focus()
            ElseIf (e.KeyCode And Not Keys.Modifiers) = Keys.D2 AndAlso e.Modifiers = Keys.Control Then
                gridSelectItem.Focus()
            End If
        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 23", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub frmItemLookUp_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'comment:
        Timer1.Interval = 1000
        Timer1.Start()
        txtBarcode.Enabled = False
        txtSearch.Focus()

        Try
            frmLogin.ShowDialog()

            'Dim h As System.Net.IPHostEntry = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName)
            Dim h As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)
            sReg = Format(db.SOD_fn_GetReg(h.AddressList.GetValue(0).ToString), "#")

            Me.gridSelectItem.ColumnHeadersDefaultCellStyle.Font = New Font("Tahoma", 9.75, FontStyle.Bold)
            Me.gridSelectItem.DefaultCellStyle.Font = New Font("Tahoma", 9.75, FontStyle.Regular)

            Me.statServer.Text = "SERVER  " & DB_Conn("svr")
            Me.statDb.Text = "DATABASE  " & DB_Conn("dbn")

            lblStatItemCount.Text = gridSelectItem.RowCount & " items"
            lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"

            iStoreID = (From a In db.Configurations Select a.StoreID).SingleOrDefault

            picPanel.Visible = False

            'frmLogin.ShowDialog()

            FormatGrid()

            RefreshDetails(False)

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : ERROR 0024", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Sub loadData()

        Try

            ' db_lastupdate = clsItemLookUp.checkTableUpdate()

            Cursor.Current = Cursors.WaitCursor

            ' System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False

            clsItemLookUp.getItemtoGrid()
            'itemData.DataSource = load_data("Select * FROM SOD_ViewItems"
            gridItem.DataSource = getItem()

            GridColumnWidth()
            ' SearchItem()
            iRow = 0
            'searching
            'txtSearch.Focus()
            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

            ToolStripStatusLabel1.Text = "STATUS "

            ' Update_Timer.Start()
            Cursor.Current = Cursors.Default

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 25", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    Public Function getItem() As DataTable
        Return load_data("SELECT TOP 500 * FROM SOD_VIEWITEMSWO WHERE Inactive = 0")
    End Function
    Private Sub FormatGrid()

        Try

            With gridSelectItem

                For Each col As DataGridViewColumn In .Columns
                    col.SortMode = DataGridViewColumnSortMode.NotSortable
                Next

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
                .Columns(19).DefaultCellStyle.Format = "C"
                '.Columns(19).Width = 160
                .Columns(19).FillWeight = 50


            End With

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 26", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click

        If MsgBox("Do you really want To close Work/Sales Order Entry?", vbQuestion + vbYesNo, "Quit") = vbYes Then
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
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 27", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 28", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdSSales_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSSales.Click

        Try
            frmSalesRep.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 29", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    'Private Sub cmdSCustomer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSCustomer.Click

    '    Try

    '        ShowCustomer()
    '    Catch ex As Exception
    '        MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 30", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try

    'End Sub
    Private Sub cmdSCustomer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSCustomer.Click
        'comment
        'Try
        ClearItem()
        ShowCustomer()
        GetCustomerPriceLevel()
        'Catch ex As Exception
        '    MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 33", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '    ErrorCount = ErrorCount + 1
        'End Try

        'Try

        '    txtPriceLevel.Text = clsCustomer.getPriceLevel(txtCustomer.Text)
        '    txtCustomerId.Text = clsCustomer.getCustomerID(txtCustomer.Text)
        '    txtAR.Text = clsAccountReceivable.getCreditLimit(txtCustomer.Text)
        '    txtOpenWO.Text = clsAccountReceivable.getOpenWO(txtCustomer.Text)
        '    txtCreditLimit.Text = clsAccountReceivable.getCustomerCreditLimit(txtCustomer.Text)
        '    txtAvailable.Text = Val(txtCreditLimit.Text) - (Val(txtAR.Text) + Val(txtOpenWO.Text))

        'Catch ex As Exception

        'End Try

        'Dim priceLevel As Integer

        '' Check if the text can be converted to an integer
        'If Integer.TryParse(txtPriceLevel.Text, priceLevel) Then
        '    Select Case priceLevel
        '        Case 0
        '            txtPriceLevel.Text = "Price"
        '        Case 1
        '            txtPriceLevel.Text = "PriceA"
        '        Case 2
        '            txtPriceLevel.Text = "PriceB"
        '        Case 3
        '            txtPriceLevel.Text = "PriceC"
        '        Case Else
        '            txtPriceLevel.Text = "None"
        '    End Select

        '    ' Assuming txtCustomer.Text contains the item code
        '    Dim priceAmount As Decimal = clsCustomer.getPriceAmount(txtCustomer.Text, priceLevel)
        '    txtPrice.Text = priceAmount.ToString()
        'Else
        '    ' Handle case where text is not a valid integer
        '    txtPriceLevel.Text = "Invalid input"
        'End If

    End Sub

    'Old function
    'Public Sub GetCustomerPriceLevel()

    '    Try

    '        txtPriceLevel.Text = clsCustomer.getPriceLevel(txtCustomer.Text)
    '        txtCustomerId.Text = clsCustomer.getCustomerID(txtCustomer.Text)
    '        txtAR.Text = clsAccountReceivable.getCreditLimit(txtCustomer.Text)
    '        txtOpenWO.Text = clsAccountReceivable.getOpenWO(txtCustomer.Text)
    '        txtCreditLimit.Text = clsAccountReceivable.getCustomerCreditLimit(txtCustomer.Text)
    '        txtAvailable.Text = Val(txtCreditLimit.Text) - (Val(txtAR.Text) + Val(txtOpenWO.Text))

    '    Catch ex As Exception

    '    End Try

    '    txtPriceLevel.Text = clsCustomer.getPriceLevel(txtCustomer.Text)
    '    Dim priceLevel As Integer

    '    ' Check if the text can be converted to an integer
    '    If Integer.TryParse(txtPriceLevel.Text, priceLevel) Then
    '        Select Case priceLevel
    '            Case 0
    '                txtPriceLevel.Text = "Price"
    '            Case 1
    '                txtPriceLevel.Text = "PriceA"
    '            Case 2
    '                txtPriceLevel.Text = "PriceB"
    '            Case 3
    '                txtPriceLevel.Text = "PriceC"
    '            Case Else
    '                txtPriceLevel.Text = "None"
    '        End Select

    '        ' Assuming txtCustomer.Text contains the item code
    '        Dim priceAmount As Decimal = clsCustomer.getPriceAmount(txtCustomer.Text, priceLevel)
    '        txtPrice.Text = priceAmount.ToString()
    '    Else
    '        ' Handle case where text is not a valid integer
    '        txtPriceLevel.Text = "Invalid input"
    '    End If

    'End Sub

    'new function

    Public Sub GetCustomerPriceLevel()
        Try

            ' Retrieve customer info
            'Dim customerId As String = clsCustomer.getCustomerID(txtCustomer.Text)
            Dim priceLevelRaw As String = clsCustomer.getPriceLevel(iCusID)
            'Dim ar As Decimal = Val(clsAccountReceivable.getCreditLimit(txtCustomer.Text))
            Dim ar As Decimal = iCusID
            'Dim openWO As Decimal = Val(clsAccountReceivable.getOpenWO(txtCustomer.Text))
            Dim openWO As Decimal = clsAccountReceivable.getOpenWO(iCusID)
            'Dim creditLimit As Decimal = Val(clsAccountReceivable.getCustomerCreditLimit(txtCustomer.Text))
            Dim creditLimit As Decimal = Val(clsAccountReceivable.getCustomerCreditLimit(iCusID))
            Dim available As Decimal = creditLimit - (ar + openWO)
            'MessageBox.Show("Customer ID: " & iCusID & vbCrLf &
            '            "Price Level (raw): " & priceLevelRaw & vbCrLf &
            '            "Accounts Receivable: " & ar.ToString() & vbCrLf &
            '            "Open Work Orders: " & openWO.ToString() & vbCrLf &
            '            "Credit Limit: " & creditLimit.ToString() & vbCrLf &
            '            "Available Credit: " & available.ToString(),
            '            "Debug Info",
            '            MessageBoxButtons.OK,
            '            MessageBoxIcon.Information)
            ' Assign to UI
            txtCustomerId.Text = iCusID
            txtAR.Text = ar.ToString()
            txtOpenWO.Text = openWO.ToString()
            txtCreditLimit.Text = creditLimit.ToString()
            txtAvailable.Text = available.ToString()

            ' Handle price level mapping
            Dim priceLevel As Integer
            If Integer.TryParse(priceLevelRaw, priceLevel) Then
                Dim levelName As String
                Select Case priceLevel
                    Case 0 : levelName = "Price"
                    Case 1 : levelName = "PriceA"
                    Case 2 : levelName = "PriceB"
                    Case 3 : levelName = "PriceC"
                    Case Else : levelName = "None"
                End Select

                txtPriceLevel.Text = levelName

                ' Assuming txtCustomer.Text is item code
                Dim priceAmount As Decimal = clsCustomer.getPriceAmount(txtCustomer.Text, priceLevel)
                txtPrice.Text = priceAmount.ToString()
            Else
                txtPriceLevel.Text = "Invalid input"
            End If

        Catch ex As Exception
            MessageBox.Show("Error retrieving customer price level: " & ex.Message,
                        "Customer Lookup Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowCustomer()
        'comment
        Try

            If gridSelectItem.RowCount > 0 Then
                If IsNumeric(Split(lblOrderNo.Text, " ")(1)) Or Not txtCustomer.Text = Nothing Then

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
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 31", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub gridItem_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridItem.KeyDown

        Try

            If e.KeyCode = Keys.Back Then
                'Searching
                'Me.txtSearch.Focus()
            ElseIf e.KeyCode = Keys.Enter Then
                If gridItem.RowCount > 0 Then
                    SelectPrice()
                    e.SuppressKeyPress = True
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 32", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub txtSales_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSales.KeyPress
        'comment
        Try

            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                frmSalesRep.ShowDialog()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 33", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub txtCustomer_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCustomer.KeyPress

        Try

            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                frmCustomer.ShowDialog()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 34", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSearch.Click

        Try

            If txtSearch.Text = String.Empty Then
                MessageBox.Show("Search cannot be empty !", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            ElseIf txtSearch.Text.Contains("'") Then

                MessageBox.Show("Searching ' or Apostrophe is not Allowed !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                FilterText()
                txtSearch.Text = String.Empty
                SearchItem()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0035", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRefresh.Click

        Try

            RefreshDetails(True)

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0036", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Sub RefreshDetails(ByVal reloaditem As Boolean)

        Try

            Cursor.Current = Cursors.WaitCursor

            db.Refresh(Data.Linq.RefreshMode.OverwriteCurrentValues)
            db = New ItemLookUpDataContext(DB_Conn("constr"))
            txtPriceLevel.Text = ""
            txtSales.Text = ""
            txtCustomer.Text = ""
            txtSearch.Text = ""
            txtRemarks.Text = String.Empty
            txtType.Text = String.Empty
            txtRemarks.Enabled = True

            WoRecallType = 0

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
            'Me.txtCustomer.Focus()

            TreeView1.Nodes.Clear()

            lblStatItemCount.Text = gridItem.RowCount & " items"
            txtSearch.Text = String.Empty
            'txtSearch.Focus()

            chkWorkOrder.Checked = True
            gridSelectItem.Columns(17).Visible = True
            gridSelectItem.Columns(18).Visible = True

            chkWorkOrder.Enabled = True
            chkQuote.Enabled = True

            rbtnDelivery.Enabled = True
            rbtnPickup.Enabled = True

            rbtnPickup.Checked = True
            cboPayment.Enabled = True

            chkWebsite.Checked = False
            txtRemarks.ReadOnly = False
            ispriceApproved = 0

            If reloaditem = True Then
                loadData()
                GridColumnWidth()
            End If
            chkBoxQtoWo.Checked = False

            Cursor.Current = Cursors.Default
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0037", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    'Public Sub RefreshDetailsOpen(ByVal reloaditem As Boolean)

    '    Try

    '        Cursor.Current = Cursors.WaitCursor

    '        db.Refresh(Data.Linq.RefreshMode.OverwriteCurrentValues)
    '        db = New ItemLookUpDataContext(DB_Conn("constr"))

    '        txtSales.Text = ""
    '        txtCustomer.Text = ""
    '        txtSearch.Text = ""
    '        txtRemarks.Text = String.Empty
    '        txtType.Text = String.Empty
    '        txtRemarks.Enabled = True

    '        gridSelectItem.Rows.Clear()
    '        cboPayment.SelectedIndex = -1
    '        bEmployee = False
    '        iPrice = 0
    '        iSalesID = 0
    '        iCusID = 0
    '        sAcctNum = ""
    '        sTitle = ""
    '        bTaxExcempt = False
    '        dTotalTSales = 0
    '        dTotalVSales = 0
    '        dTotalSales = 0
    '        Me.txtSub.Text = "0.00"
    '        Me.txtVat.Text = "0.00"
    '        Me.txtTotal.Text = "0.00"

    '        lblStatItemSelected.Text = gridSelectItem.RowCount & " item" & IIf(gridSelectItem.RowCount > 1, "s", "") & " selected"

    '        lblOrderNo.Text = "Order #: "
    '        Me.txtCustomer.Focus()

    '        TreeView1.Nodes.Clear()

    '        lblStatItemCount.Text = gridItem.RowCount & " items"
    '        txtSearch.Text = String.Empty
    '        'searching
    '        'txtSearch.Focus()
    '        ispriceApproved = 1

    '        chkWorkOrder.Checked = True
    '        gridSelectItem.Columns(17).Visible = True
    '        gridSelectItem.Columns(18).Visible = True

    '        chkWorkOrder.Enabled = True
    '        chkQuote.Enabled = True

    '        rbtnDelivery.Enabled = True
    '        rbtnPickup.Enabled = True

    '        rbtnPickup.Checked = True
    '        cboPayment.Enabled = True

    '        chkWebsite.Checked = False
    '        txtRemarks.ReadOnly = False

    '        If reloaditem = True Then
    '            loadData()
    '            GridColumnWidth()
    '        End If

    '        Cursor.Current = Cursors.Default
    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0037", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try

    'End Sub
    'comment
    'Private Sub gridSelectItem_CellBeginEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellCancelEventArgs) Handles gridSelectItem.CellBeginEdit

    '    'Try

    '    '    If e.ColumnIndex = 2 Then

    '    '        curQty = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)

    '    '    ElseIf e.ColumnIndex = 3 Then
    '    '        Try
    '    '            curPrice = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)
    '    '        Catch ex As Exception

    '    '        End Try

    '    '    End If

    '    'Catch ex As Exception
    '    '    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0038", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    '    ErrorCount = ErrorCount + 1
    '    'End Try

    '    Try
    '        Dim NaN As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex("[^\d]")
    '        Dim str As String = gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value

    '        ' Validate and correct empty or non-numeric values in column 2
    '        If e.ColumnIndex = 2 Then
    '            If str = String.Empty Or NaN.IsMatch(str) Then
    '                gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = 0
    '            End If
    '        End If

    '        gridSelectItem.Rows(e.RowIndex).Selected = True
    '        gridSelectItem.CurrentCell = gridSelectItem.Rows(e.RowIndex).Cells(e.ColumnIndex)
    '        Dim editVal As Double = Convert.ToDouble(gridSelectItem.CurrentCell.Value)

    '        If e.ColumnIndex = 2 Then
    '            If gridSelectItem.RowCount > 0 Then
    '                iErrCnt = 0
    '                ValidateAllQty()
    '                If iErrCnt > 0 Then
    '                    MsgBox("Insufficient quantity to fulfill the request!", MsgBoxStyle.Exclamation, "Message")
    '                    gridSelectItem.CurrentCell.Value = curQty
    '                    gridSelectItem.CurrentCell.Style.ForeColor = Color.Black
    '                    gridSelectItem.CurrentCell.Style.SelectionForeColor = Color.Black
    '                End If
    '            End If
    '            Me.ActiveControl = txtSearch
    '        End If

    '        If e.ColumnIndex = 3 Then
    '            'curPrice = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)
    '            'For x = 0 To iPrice
    '            '    clsItemLookUp.LoadLevel(x, gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value, 0)
    '            'Next x
    '            'If bAllow = False Then
    '            '    ' Call ValidatePriceInput to get the current price
    '            '    ValidatePriceInput()
    '            '    Dim currentPrice As Decimal
    '            '    If Decimal.TryParse(txtPrice.Text, currentPrice) Then
    '            '        If editVal <= dLowest AndAlso editVal < currentPrice Then
    '            '            If bFlag = True Then
    '            '                frmPassword.sType = "EditPrice"
    '            '                Dim pDialog As DialogResult = frmPassword.ShowDialog()
    '            '                If pDialog = Windows.Forms.DialogResult.Cancel Then
    '            '                    gridSelectItem.CurrentCell.Value = curPrice
    '            '                End If
    '            '            End If
    '            '        End If
    '            '    End If
    '            'End If
    '        End If
    '        If e.ColumnIndex = 18 Then
    '            If Convert.ToInt32(gridSelectItem.Item(2, e.RowIndex).Value) < Convert.ToInt32(gridSelectItem.Item(18, e.RowIndex).Value) Then
    '                MessageBox.Show("Qty Prepared cannot be greater than the Ordered Qty", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '                gridSelectItem.Item(18, e.RowIndex).Value = 0
    '            End If
    '        End If

    '        UpdateAmt()

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0051", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount += 1
    '    End Try
    'End Sub

    Private Sub gridSelectItem_CellBeginEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellCancelEventArgs) Handles gridSelectItem.CellBeginEdit

        Try

            If e.ColumnIndex = 2 Then

                curQty = CType(gridSelectItem(e.ColumnIndex, e.RowIndex).Value, Double)
                'curQty = gridSelectItem(e.ColumnIndex, e.RowIndex).Value
            ElseIf e.ColumnIndex = 3 Then

                Try
                    curPrice = gridSelectItem(e.ColumnIndex, e.RowIndex).Value
                Catch ex As Exception

                End Try

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0041", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub gridSelectItem_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridSelectItem.CellValueChanged

        If gridSelectItem.RowCount > 0 Then

            If e.ColumnIndex = 3 Then

                If WoRecallType = 0 Then
                    If Not bAllow Then
                        'MessageBox.Show("Recall Type 0")
                        ValidatePriceRow(e.RowIndex, e.ColumnIndex, 0)

                    End If
                ElseIf WoRecallType = 1 Then
                    'ValidatePriceRow(e.RowIndex, e.ColumnIndex, 1)
                    'MessageBox.Show("Recall Type 1")

                    If Not bAllow Then
                        ValidatePriceRow(e.RowIndex, e.ColumnIndex, 1)
                    End If
                End If

            End If
        End If


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

                Else
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(3).Style.ForeColor = Color.Black
                    gridSelectItem.Rows(gridSelectItem.CurrentRow.Index).Cells(3).Style.SelectionForeColor = Color.White
                End If

                If iItemType = 0 Then
                    sPrevItemCode = gridSelectItem(0, gridSelectItem.CurrentRow.Index).Value

                    For n As Integer = 0 To gridSelectItem.Rows.Count - 1
                        sLineItemCode = gridSelectItem(0, n).Value


                        dNetAvailable = clsItemLookUp.getItemQty(sLineItemCode)

                        MsgBox(dNetAvailable)
                        If Val(gridSelectItem(2, n).Value) > dNetAvailable Then
                            MsgBox("Insufficient Quantity!", vbExclamation, "Message")
                            gridSelectItem(2, n).Value = 0

                        End If

                    Next

err_flag:

                    If iCurRow <> -1 Then
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
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0039", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    'Public Function ValidateAllQty() As DialogResult

    '    Try

    '        Dim sLineItemCode, sPrevItemCode
    '        Dim sMsg As String = ""
    '        Dim dReqQty, dCommittedQty As Double
    '        Dim dAvailable, dNetAvailable As Double
    '        Dim sCurIndex As Integer

    '        dReqQty = 0
    '        dAvailable = 0
    '        dNetAvailable = 0
    '        dCommittedQty = 0
    '        sPrevItemCode = String.Empty
    '        sCurIndex = 0


    '        For i As Integer = 0 To gridSelectItem.Rows.Count - 1

    '            If IsNumeric(gridSelectItem(2, i).Value) = False Then
    '                gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                sMsg = "The string '" & gridSelectItem(2, i).Value & "' is not a valid quantity."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            If Val(gridSelectItem(2, i).Value) <= 0 Then
    '                gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                sMsg = "An invalid quantity was detected."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            If IsNumeric(gridSelectItem(3, i).Value) = False Then
    '                gridSelectItem.Rows(i).Cells(3).Style.ForeColor = Color.Red
    '                If gridSelectItem(3, i).Value = "-" Then
    '                    Return MessageBox.Show("Please Check the Price for the Item : " & vbCrLf & vbCrLf & gridSelectItem(1, i).Value, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '                End If
    '                sMsg = "The number '" & gridSelectItem(3, i).Value & "' is not a valid amount."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            sPrevItemCode = gridSelectItem(0, i).Value
    '            sCurIndex = gridSelectItem(11, i).Value


    '            If clsItemLookUp.GetItemType(sPrevItemCode) = 0 Or clsItemLookUp.GetItemType(sPrevItemCode) = 3 Then

    '                If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

    '                    dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode) + clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex)

    '                Else

    '                    dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode)

    '                End If

    '                dCommittedQty = 0

    '                For n As Integer = 0 To gridSelectItem.Rows.Count - 1
    '                    sLineItemCode = gridSelectItem(0, n).Value
    '                    If sPrevItemCode = sLineItemCode Then
    '                        dCommittedQty = dCommittedQty + Val(gridSelectItem(2, n).Value)
    '                    End If
    '                Next

    '                dAvailable = dNetAvailable - dCommittedQty

    '                MessageBox.Show("dNetAvailable " & dNetAvailable)
    '                MessageBox.Show("dCommittedQty " & dCommittedQty)
    '                MessageBox.Show("dAvailable " & dAvailable)

    '                'If chkWorkOrder.Checked = True And chkBoxQtoWo.Checked = False Then

    '                If dAvailable < 0 Then

    '                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
    '                        iErrCnt = iErrCnt + 1
    '                        sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."

    '                    Else

    '                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
    '                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White

    '                    End If
    '                ' End If
    '                'ElseIf chkWorkOrder.Checked = True And chkBoxQtoWo.Checked = True Then

    '                '    MessageBox.Show("mao ni ang checking sa qty if mag save ang wo from quote")
    '                '    MessageBox.Show(" comqty < available  " & dCommittedQty & " < " & dAvailable)


    '                '    If dCommittedQty <= dAvailable Or dCommittedQty <= 0 Then

    '                '        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                '        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
    '                '        iErrCnt = iErrCnt + 1
    '                '        sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."

    '                '    Else

    '                '        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
    '                '        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White

    '                '    End If

    '                'End If
    '                'ElseIf chkWorkOrder.Checked = True And chkBoxQtoWo.Checked = True Then

    '                '    'MessageBox.Show("mao ni ang checking sa qty if mag save ang wo from quote")
    '                '    'MessageBox.Show("comqty vs available: " & dCommittedQty & " vs " & dAvailable)

    '                '    If dCommittedQty < dAvailable Then
    '                '        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                '        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
    '                '        iErrCnt = iErrCnt + 1
    '                '        sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."
    '                '    Else
    '                '        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
    '                '        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White
    '                '    End If

    '                'End If

    '            End If

    '        Next

    '        UpdateAmt()

    '        If sMsg = String.Empty Or sMsg = "" Then
    '            Return Nothing
    '        Else
    '            Return MessageBox.Show(sMsg, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0040", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '        Return Nothing
    '    End Try


    'End Function
    Public Function ValidateAllQty() As DialogResult

        Try

            Dim sLineItemCode, sPrevItemCode
            Dim sMsg As String = ""
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
                        Return MessageBox.Show("Please Check the Price for the Item : " & vbCrLf & vbCrLf & gridSelectItem(1, i).Value, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    sMsg = "The number '" & gridSelectItem(3, i).Value & "' is not a valid amount."
                    iErrCnt = iErrCnt + 1
                End If

                sPrevItemCode = gridSelectItem(0, i).Value
                sCurIndex = gridSelectItem(11, i).Value

                If clsItemLookUp.GetItemType(sPrevItemCode) = 0 Or clsItemLookUp.GetItemType(sPrevItemCode) = 3 Then

                    If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

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

                    dAvailable = dNetAvailable - dCommittedQty

                    If dAvailable < 0 Then

                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
                        iErrCnt = iErrCnt + 1

                        sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."


                        loadData()
                        GridColumnWidth()


                    Else
                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White
                    End If

                End If

            Next

            UpdateAmt()

            If sMsg = String.Empty Or sMsg = "" Then
                Return Nothing
            Else
                Return MessageBox.Show(sMsg, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0042", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function
    'comment
    'Public Function ValidateAllQtyForQoute() As DialogResult
    '    MessageBox.Show("ValidateAllQtyForQoute()")
    '    Try

    '        Dim sLineItemCode, sPrevItemCode
    '        Dim sMsg As String = ""
    '        Dim dReqQty, dCommittedQty As Double
    '        Dim dAvailable, dNetAvailable As Double
    '        Dim sCurIndex As Integer

    '        dReqQty = 0
    '        dAvailable = 0
    '        dNetAvailable = 0
    '        dCommittedQty = 0
    '        sPrevItemCode = String.Empty
    '        sCurIndex = 0


    '        For i As Integer = 0 To gridSelectItem.Rows.Count - 1

    '            If IsNumeric(gridSelectItem(2, i).Value) = False Then
    '                gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                sMsg = "The string '" & gridSelectItem(2, i).Value & "' is not a valid quantity."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            If Val(gridSelectItem(2, i).Value) <= 0 Then
    '                gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                sMsg = "An invalid quantity was detected."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            If IsNumeric(gridSelectItem(3, i).Value) = False Then
    '                gridSelectItem.Rows(i).Cells(3).Style.ForeColor = Color.Red
    '                If gridSelectItem(3, i).Value = "-" Then
    '                    Return MessageBox.Show("Please Check the Price for the Item : " & vbCrLf & vbCrLf & gridSelectItem(1, i).Value, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '                End If
    '                sMsg = "The number '" & gridSelectItem(3, i).Value & "' is not a valid amount."
    '                iErrCnt = iErrCnt + 1
    '            End If

    '            sPrevItemCode = gridSelectItem(0, i).Value
    '            sCurIndex = gridSelectItem(11, i).Value

    '            If clsItemLookUp.GetItemType(sPrevItemCode) = 0 Or clsItemLookUp.GetItemType(sPrevItemCode) = 3 Then

    '                If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

    '                    dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode) + clsItemLookUp.getItemQtyCommitted(sPrevItemCode) + clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex)

    '                Else

    '                    dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode)

    '                End If

    '                dCommittedQty = 0

    '                For n As Integer = 0 To gridSelectItem.Rows.Count - 1
    '                    sLineItemCode = gridSelectItem(0, n).Value
    '                    If sPrevItemCode = sLineItemCode Then
    '                        dCommittedQty = dCommittedQty + Val(gridSelectItem(2, n).Value)
    '                    End If
    '                Next

    '                dAvailable = dNetAvailable - dCommittedQty

    '                MessageBox.Show("dNetAvailable: " & dNetAvailable)
    '                MessageBox.Show("dCommittedQty: " & dCommittedQty)
    '                MessageBox.Show("dAvailable =: " & dNetAvailable & " - " & dCommittedQty & " = " & dAvailable)
    '                MessageBox.Show("dAvailable: " & dAvailable)
    '                MessageBox.Show("if dAvailable < 0")


    '                If dAvailable < dCommittedQty Then

    '                    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
    '                    gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
    '                    iErrCnt = iErrCnt + 1
    '                    sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."

    '                Else
    '                    gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
    '                    gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White

    '                End If

    '            End If

    '        Next

    '        UpdateAmt()

    '        If sMsg = String.Empty Or sMsg = "" Then
    '            Return Nothing
    '        Else
    '            Return MessageBox.Show(sMsg, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0040", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '        Return Nothing
    '    End Try
    'End Function

    Public Function ValidateAllQtyForQoute() As DialogResult
        'MessageBox.Show("ValidateAllQtyForQoute()")
        Try

            Dim sLineItemCode, sPrevItemCode
            Dim sMsg As String = ""
            Dim dReqQty, dCommittedQty As Double
            Dim dAvailable, dNetAvailable As Double
            Dim sCurIndex As Integer
            'Comment

            'Dim onHandQty As Double
            'Dim totalCommitted As Double
            'Dim currentQuoteQty As Double
            'Dim netAvailable As Double


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
                        Return MessageBox.Show("Please Check the Price for the Item : " & vbCrLf & vbCrLf & gridSelectItem(1, i).Value, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    sMsg = "The number '" & gridSelectItem(3, i).Value & "' is not a valid amount."
                    iErrCnt = iErrCnt + 1
                End If

                sPrevItemCode = gridSelectItem(0, i).Value
                sCurIndex = gridSelectItem(11, i).Value

                If clsItemLookUp.GetItemType(sPrevItemCode) = 0 Or clsItemLookUp.GetItemType(sPrevItemCode) = 3 Then

                    If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then

                        'dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode) + clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex)

                        'onHandQty = clsItemLookUp.getItemQty(sPrevItemCode)
                        'totalCommitted = clsItemLookUp.getItemQtyCommitted(sPrevItemCode)
                        'currentQuoteQty = clsRecall.CommittedWO(Split(lblOrderNo.Text, ": ")(1), sCurIndex) ' use your actual logic here
                        'netAvailable = onHandQty - (totalCommitted - currentQuoteQty)
                        dNetAvailable = clsItemLookUp.getItemQty(sPrevItemCode)
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

                    'dAvailable = dNetAvailable - dCommittedQty

                    'MessageBox.Show("dNetAvailable: " & dNetAvailable)
                    'MessageBox.Show("dCommittedQty: " & dCommittedQty)
                    'MessageBox.Show("dAvailable =: " & dNetAvailable & " - " & dCommittedQty & " = " & dAvailable)
                    'MessageBox.Show("dAvailable: " & dAvailable)
                    'MessageBox.Show("if dAvailable < 0")

                    'MessageBox.Show("onHandQty " & onHandQty)
                    'MessageBox.Show("totalCommitted " & totalCommitted)
                    'MessageBox.Show("currentQuoteQty" & currentQuoteQty)
                    'MessageBox.Show("dNetAvailable " & dNetAvailable)
                    'MessageBox.Show("dCommittedQty: " & dCommittedQty)

                    dAvailable = dNetAvailable - dCommittedQty

                    'check = dAvailable + dCommittedQty
                    'essageBox.Show("dAvailable: " & dAvailable = netAvailable - dCommittedQty)
                    'MessageBox.Show("dAvailable: " & dAvailable)
                    'MessageBox.Show("check: " & check)

                    If dAvailable < 0 Then

                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Red
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.Red
                        iErrCnt = iErrCnt + 1

                        sMsg = "Please Check The Latest Available Quantity." & vbCrLf & vbCrLf & "The Available Quantity Of The Item(s) Has Been Changed/Updated Prior To Saving."

                        loadData()
                        GridColumnWidth()

                    Else

                        gridSelectItem.Rows(i).Cells(2).Style.ForeColor = Color.Black
                        gridSelectItem.Rows(i).Cells(2).Style.SelectionForeColor = Color.White

                    End If

                End If

            Next

            UpdateAmt()

            If String.IsNullOrEmpty(sMsg) Then
                Return Nothing
            Else
                Return MessageBox.Show(sMsg, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0040", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try
    End Function

    Private Sub gridSelectItem_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridSelectItem.KeyDown

        Try

            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                e.Handled = True
                txtSearch.Text = String.Empty
                'searching
                'txtSearch.Focus()
            ElseIf e.KeyCode = Keys.Delete Then
                removeItems()
            ElseIf e.KeyCode = Keys.F2 Then
                sItemCode = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value
                EditPrice()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0041", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub EditPrice()

        Try

            bInsert = False
            frmPriceLevel.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0042", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0043", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0044", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0045", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub
    Private Sub cmdImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdImport.Click

        clsImport.importSelectFile()

        If clsImport.importProceed_ = True Then
            If clsImport.importType_ = 1 Then
                ImportQuote()
                SearchItemImport()
                Exit Sub
            ElseIf clsImport.importType_ = 2 Then
                frmCustomer.ShowDialog()
                frmSalesRep.ShowDialog()
                ImportPO()
                SearchItemImport()
                Exit Sub
            ElseIf clsImport.importType_ = 3 Then

                Exit Sub
            End If
            clsImport.importProceed_ = False
        End If

    End Sub
    Private Sub SearchItemImport()

        Try

            Dim importeditems = "(" & String.Join(",", clsImport.importedItemCode.ToArray) & ")"
            Dim FilterStr = "SELECT TOP 500 * FROM SOD_VIEWITEMSWO WHERE Inactive = 0 AND ItemLookupcode in " & importeditems
            gridItem.DataSource = load_data(FilterStr)

            checkSearch()
            txtSearch.Focus()
            GridColumnWidth()

            iRow = 0
            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0046", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    'Private Sub ImportQuote()

    '    Try

    '        If Me.txtSales.Text = "" Then
    '            MsgBox("Please select sales representative", vbExclamation, "Message!")
    '            Exit Sub
    '        ElseIf Me.txtCustomer.Text = "" Then
    '            MsgBox("Please select customer", vbExclamation, "Message!")
    '            Exit Sub
    '        End If

    '        ofdImport.Title = "Please Select a File"
    '        ofdImport.InitialDirectory = "C:\Users\SBD_USER1\Desktop\RMS_IAN_FILES\SALES_AGENT_SQL_JOB"
    '        ofdImport.FileName = String.Empty

    '        Dim fileDialog As DialogResult = ofdImport.ShowDialog()

    '        If fileDialog = Windows.Forms.DialogResult.OK Then
    '            Dim frm As New frmUploader
    '            frm.lblFile.Text = ofdImport.FileName
    '            frm.ShowDialog(Me)
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0049", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try

    'End Sub

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
                gridItem.DataSource = getItem()
                lblStatItemCount.Text = gridItem.RowCount & " items"
                txtSearch.Text = String.Empty
                'searching
                'Me.ActiveControl = txtSearch
            End If

            If e.KeyCode = Keys.F4 Then
                If txtSearch.Text = String.Empty Then
                    Try
                        Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                        TreeView1.Nodes.Remove(node(0))
                        FilterText()

                        If TreeView1.Nodes.Count = 0 Then
                            gridItem.DataSource = getItem()
                        Else
                            SearchItem()
                        End If
                        'searching
                        'Me.ActiveControl = txtSearch
                    Catch ex As Exception

                    End Try
                Else
                    MessageBox.Show("Please Clear the search box before removing the last searched keyword", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If


            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0047", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Function isNumber(ByVal input As String) As Boolean
        Return Regex.IsMatch(input.Trim, "\A-{0,1}[0-9.]*\Z")
    End Function

    Private Sub gridSelectItem_CellEndEdit(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles gridSelectItem.CellEndEdit


        Try
            Dim cellValue As String = gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value?.ToString().Trim()
            'Dim currentPrice As Decimal

            ' === Quantity Column (Index 2) ===
            If e.ColumnIndex = 2 Then
                'Dim regexNonNumeric As New Regex("[^\d]")
                'If String.IsNullOrEmpty(cellValue) OrElse regexNonNumeric.IsMatch(cellValue) Then
                '    gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = 0
                'End If
                Dim rawValue As Object = gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value
                Dim qty As Decimal = 0D

                ' Only parse if there is a value
                If rawValue IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(rawValue.ToString()) Then
                    Dim parsed As Decimal
                    If Decimal.TryParse(rawValue.ToString(), parsed) Then
                        qty = parsed
                    End If
                End If

                ' If invalid or blank, reset to 0 (as Decimal)
                gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = qty
                ' Validate all quantity values
                For Each row As DataGridViewRow In gridSelectItem.Rows
                    If Not row.IsNewRow Then
                        Dim qtyValue As Object = row.Cells(2).Value

                        If qtyValue IsNot Nothing AndAlso IsNumeric(qtyValue) Then
                            If Convert.ToDecimal(qtyValue) = 0 Then
                                MessageBox.Show("Quantity cannot be zero. Please correct the item before proceeding.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                gridSelectItem.CurrentCell = row.Cells(2)
                                gridSelectItem.BeginEdit(True)
                                row.Cells(2).Value = 1
                            End If
                        End If
                    End If
                Next

                gridSelectItem.Rows(e.RowIndex).Selected = True
                gridSelectItem.CurrentCell = gridSelectItem.Rows(e.RowIndex).Cells(e.ColumnIndex)
                'Dim editQty As Double = Convert.ToDouble(gridSelectItem.CurrentCell.Value)
                Dim editQty As Double = gridSelectItem.CurrentCell.Value
                If chkWorkOrder.Checked And chkBoxQtoWo.Checked = False Then

                    iErrCnt = 0

                    ValidateAllQty()

                    If iErrCnt > 0 Then

                        MsgBox("Insufficient quantity to fulfill the request!", MsgBoxStyle.Exclamation, "Message")
                        gridSelectItem.CurrentCell.Value = curQty
                        gridSelectItem.CurrentCell.Style.ForeColor = Color.Black
                        gridSelectItem.CurrentCell.Style.SelectionForeColor = Color.Black

                    End If

                End If

                If chkWorkOrder.Checked = True And chkBoxQtoWo.Checked = True Then
                    iErrCnt = 0
                    ValidateAllQtyForQoute()
                    If iErrCnt > 0 Then
                        MsgBox("Insufficient quantity to fulfill the request!", MsgBoxStyle.Exclamation, "Message")
                        'gridSelectItem.CurrentCell.Value = curQty
                        gridSelectItem.CurrentCell.Style.ForeColor = Color.Black
                        gridSelectItem.CurrentCell.Style.SelectionForeColor = Color.Black
                    End If
                End If

                'searching
                'Me.ActiveControl = txtSearch
            End If

            ' === Price Column (Index 3) ===
            If e.ColumnIndex = 3 Then

                'Dim inputPrice As String = cellValue
                'Dim priceRegex As New Regex("^\d+(\.\d{1,2})?$")

                'If String.IsNullOrEmpty(inputPrice) OrElse Not priceRegex.IsMatch(inputPrice) Then
                '    MessageBox.Show("Please enter a valid price (numeric, up to 2 decimal places).", "Invalid Price", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                '    gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = curPrice
                '    Return
                'End If
                Try

                    Dim inputPrice As String = cellValue.Trim()

                    ' --- STEP 1: Clean and normalize input ---
                    inputPrice = inputPrice.Replace("₱", "").Replace("$", "").Trim()
                    inputPrice = inputPrice.Replace(",", ".") ' convert comma decimal to dot

                    If inputPrice.StartsWith("(") AndAlso inputPrice.EndsWith(")") Then
                        inputPrice = "-" & inputPrice.Trim("(", ")")
                    End If

                    ' --- STEP 2: Validate using Decimal.TryParse instead of regex ---
                    Dim parsedPrice As Decimal
                    If Not Decimal.TryParse(inputPrice,
                            Globalization.NumberStyles.AllowDecimalPoint Or
                            Globalization.NumberStyles.AllowThousands Or
                            Globalization.NumberStyles.AllowLeadingSign,
                            Globalization.CultureInfo.InvariantCulture,
                            parsedPrice) Then
                        MessageBox.Show("Please enter a valid price (numeric, up to 2 decimal places).",
                        "Invalid Price", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = curPrice
                        Return
                    End If

                    ' --- STEP 3: Optional rounding to 2 decimals ---
                    parsedPrice = Math.Round(parsedPrice, 2)

                    ' --- STEP 4: Assign cleaned value back to cell ---
                    gridSelectItem.Item(e.ColumnIndex, e.RowIndex).Value = parsedPrice


                    ' Load price levels
                    For x = 0 To iPrice
                        clsItemLookUp.LoadLevel(x, gridSelectItem.Item(0, e.RowIndex).Value, 0)
                    Next

                    '================================ Validation ===================================='

                    If WoRecallType = 0 Then
                        If Not bAllow Then
                            'MessageBox.Show("Recall Type 0")
                            ValidatePriceRow(e.RowIndex, e.ColumnIndex, 0)

                        End If
                    ElseIf WoRecallType = 1 Then
                        'ValidatePriceRow(e.RowIndex, e.ColumnIndex, 1)
                        'MessageBox.Show("Recall Type 1")

                        If Not bAllow Then
                            ValidatePriceRow(e.RowIndex, e.ColumnIndex, 1)
                        End If
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error processing price input: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try

            End If

            ' === Qty Prepared Column (Index 18) ===
            If e.ColumnIndex = 18 Then

                'Dim orderedQty As Integer = Convert.ToInt32(gridSelectItem.Item(2, e.RowIndex).Value)
                Dim orderedQty As Integer = gridSelectItem.Item(2, e.RowIndex).Value
                Dim preparedQty As Integer = Convert.ToInt32(gridSelectItem.Item(18, e.RowIndex).Value)

                If preparedQty > orderedQty Then
                    MessageBox.Show("Qty Prepared cannot be greater than the Ordered Qty", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    gridSelectItem.Item(18, e.RowIndex).Value = 0
                End If
            End If

            ' Update total amounts
            UpdateAmt()


        Catch ex As Exception
            MessageBox.Show("You've entered an invalid input. Only numeric values are allowed.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            gridSelectItem.CurrentCell.Value = curPrice
        End Try
    End Sub
    Private Sub cmdSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSettings.Click

        Try

            frmPassword.sType = "Settings"
            frmPassword.ShowDialog()

        Catch ex As Exception
            'MessageBox.Show(ex.Message)
        End Try


    End Sub

    Private Sub TreeView1_NodeMouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeNodeMouseClickEventArgs) Handles TreeView1.NodeMouseDoubleClick

        Try

            TreeView1.Nodes.Remove(TreeView1.SelectedNode)
            FilterText()
            SearchItem()

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0049", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub cmdRecall_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRecall.Click
        recallOrders()
    End Sub

    Public Sub recallOrders()
        Try

            Dim selectionForm As New frmRecallSelection("Recall")

            Dim result As DialogResult = selectionForm.ShowDialog()
            '' Process selection

            If result = DialogResult.OK Then
                Select Case selectionForm.SelectedOption
                    Case "Recall a quote"
                        WoRecallType = 1
                        frmRecall.LoadOrder(3)
                        frmRecall.ShowDialog()
                    Case "Recall a work order"
                        WoRecallType = 1
                        frmRecall.LoadOrder(2)
                        frmRecall.ShowDialog()
                    Case Else
                        MessageBox.Show("Option not implemented: " & selectionForm.SelectedOption, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Select
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0050", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub
    Public Sub RecallQuote(ByVal orderid As Integer)

        Try
            gridSelectItem.Rows.Clear()
            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            Dim sType = "", sComment As String = "", ReleaseType As String
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

                If x.Type = 2 Then

                    sType = Split(x.Comment, "; ")(0)
                    ReleaseType = Split(x.Comment, "; ")(1)
                    sComment = Split(x.Comment, "; ")(2)

                    If ReleaseType = "Pick-up" Then
                        rbtnPickup.Checked = True
                    ElseIf ReleaseType = "Delivery" Then
                        rbtnDelivery.Checked = True
                    End If

                    chkWorkOrder.Checked = True
                    chkQuote.Checked = False

                    chkWorkOrder.Enabled = False
                    chkQuote.Enabled = False

                ElseIf x.Type = 3 Then

                    sType = ""
                    ReleaseType = ""
                    sComment = x.Comment

                    chkWorkOrder.Checked = False
                    chkQuote.Checked = True

                    chkWorkOrder.Enabled = False
                    chkQuote.Enabled = False
                    chkBoxQtoWo.Checked = True
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
                    newTimeRecord.Cells(LASTPURCHASEDPRICE.Index).Value = clsRecall.GetItemLastPrice(txtCustomer.Text, n.ItemLookUpcode)
                    If n.PickLoc = "UP-STORE" Then
                        newTimeRecord.Cells(chkPickLoc.Index).Value = True
                    Else
                        newTimeRecord.Cells(chkPickLoc.Index).Value = False
                    End If
                    newTimeRecord.Cells(OrderEntryID.Index).Value = n.ID

                    Dim vqty As Integer

                    If iOrderType = 3 Then
                        vqty = 0
                    Else

                        Try
                            vqty = (From a In db.QueueingItems
                                    Join b In db.Queueings On a.QueueingID Equals b.id Where a.ItemID.Equals(iItemID) And b.OrderID.Equals(orderid)
                                    Select a.QtyPre).ToList()(0)
                        Catch ex As Exception
                            vqty = 0
                        End Try

                    End If

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
            GetCustomerPriceLevel()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0051", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub
    Private Sub ImportQuote()

        Try

            Cursor.Current = Cursors.WaitCursor

            gridSelectItem.Rows.Clear()
            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            Dim sType = "", sComment As String = "", ReleaseType As String
            Dim iOrderType As Integer = 0

            clsImport.importOrder()

            sAcctNum = clsImport.iAccountNumber_
            iPrice = clsImport.iPriceLevel_
            bEmployee = clsImport.iEmployee_
            sTitle = clsImport.iTitle_
            bTaxExcempt = clsImport.iTaxExempt_
            iCusID = clsImport.iCustID_
            txtCustomer.Text = clsImport.iCompany_
            txtType.Text = clsImport.iCustomText5_

            iSalesID = clsImport.iSalesRepID_
            txtSales.Text = clsImport.iSaleRepName_

            sType = ""
            ReleaseType = ""
            sComment = clsImport.iComment_

            txtRemarks.Text = Trim(sComment)
            cboPayment.Text = Trim(sType)

            iOrderType = clsImport.iType_

            For Each n In clsImport.importOrderEntry()

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

                    newTimeRecord.Cells(CustPrep.Index).Value = 0

                End With

                dPrice = n.Price
                sOldDes = txtSearch.Text
                gridSelectItem.Rows.Add(newTimeRecord)

            Next

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            'gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)

            lblStatItemSelected.Text = gridSelectItem.Rows.Count & " items selected"
            UpdateAmt()

            Cursor.Current = Cursors.Default

            MessageBox.Show("Successfully Imported!" & vbCrLf & vbCrLf & "Please Check The Latest Quantity Available of Each Items Before Saving as Work Order.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception

            MessageBox.Show("Import Error!" & vbCrLf & vbCrLf & ex.Message, "MESSAGE : ERROR 0052", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try


    End Sub

    Private Sub ImportPO()

        Try

            Cursor.Current = Cursors.WaitCursor

            gridSelectItem.Rows.Clear()
            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            If Me.txtSales.Text = "" Then
                MsgBox("Please select sales representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select customer", vbExclamation, "Message!")
                Exit Sub
            End If

            For Each n In clsImport.importPOEntry()

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

                    newTimeRecord.Cells(CustPrep.Index).Value = 0

                End With

                dPrice = n.Price
                sOldDes = txtSearch.Text
                gridSelectItem.Rows.Add(newTimeRecord)

            Next

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            'gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)

            lblStatItemSelected.Text = gridSelectItem.Rows.Count & " items selected"
            UpdateAmt()

            Cursor.Current = Cursors.WaitCursor

            Dim res As Integer = clsImport.checkIfItemCodeExist()
            If res = 0 Then

                MessageBox.Show("Successfully Imported!" & vbCrLf & vbCrLf & "Please Check The Latest Quantity Available of Each Items Before Saving as Work Order.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

            End If

        Catch ex As Exception

            MessageBox.Show("Import Error!" & vbCrLf & vbCrLf & ex.Message, "MESSAGE : ERROR 0053", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub

    Public Sub ImportFromWebsite(ByVal orderNumber As Integer)

        Try

            Cursor.Current = Cursors.WaitCursor

            gridSelectItem.Rows.Clear()
            Dim previousAllowUserToAddRows = gridSelectItem.AllowUserToAddRows
            gridSelectItem.AllowUserToAddRows = True

            Dim customer = (From c In db.Customers
                            Where (c.AccountNumber.Equals(My.Settings.ImportDefaultCust))
                            Select c.AccountNumber, c.Company, c.PriceLevel, c.Employee, c.Title, c.TaxExempt, c.ID, c.CustomText2, c.CustomText4).FirstOrDefault

            sAcctNum = customer.AccountNumber
            bEmployee = customer.Employee
            sTitle = customer.Title
            bTaxExcempt = customer.TaxExempt
            iCusID = customer.ID
            txtCustomer.Text = customer.Company
            txtType.Text = customer.CustomText2
            custType = customer.CustomText4

            Dim salesRep = (From a In db.SalesReps Where a.Number.Equals(My.Settings.ImportDefaultRep) Select a.ID, a.Name).FirstOrDefault
            iSalesID = salesRep.ID
            txtSales.Text = salesRep.Name


            If Me.txtSales.Text = "" Then
                MsgBox("Please select sales representative", vbExclamation, "Message!")
                Exit Sub
            ElseIf Me.txtCustomer.Text = "" Then
                MsgBox("Please select customer", vbExclamation, "Message!")
                Exit Sub
            End If

            For Each n In clsImport.importFromWebsite(orderNumber)

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

                    txtRemarks.Text = n.Comment

                    newTimeRecord.Cells(chkPickLoc.Index).Value = True

                    newTimeRecord.Cells(OrderEntryID.Index).Value = n.ID

                    newTimeRecord.Cells(CustPrep.Index).Value = 0


                End With

                dPrice = n.Price
                sOldDes = txtSearch.Text
                gridSelectItem.Rows.Add(newTimeRecord)

            Next

            SearchItemImport()

            cboPayment.SelectedIndex = 1
            txtRemarks.ReadOnly = True

            RemoveHandler rbtnDelivery.CheckedChanged, AddressOf rbtnDelivery_CheckedChanged
            RemoveHandler rbtnPickup.CheckedChanged, AddressOf rbtnPickup_CheckedChanged

            rbtnDelivery.Checked = True

            AddHandler rbtnDelivery.CheckedChanged, AddressOf rbtnDelivery_CheckedChanged
            AddHandler rbtnPickup.CheckedChanged, AddressOf rbtnPickup_CheckedChanged

            rbtnDelivery.Enabled = True
            rbtnPickup.Enabled = True

            gridSelectItem.AllowUserToAddRows = previousAllowUserToAddRows
            'gridSelectItem.CurrentCell = gridSelectItem(0, gridSelectItem.RowCount - 1)

            lblStatItemSelected.Text = gridSelectItem.Rows.Count & " items selected"
            UpdateAmt()

            Cursor.Current = Cursors.WaitCursor

            Dim res As Integer = clsImport.checkIfItemCodeExist()
            If res = 0 Then

                MessageBox.Show("Successfully Imported!" & vbCrLf & vbCrLf & "Please Check The Latest Quantity Available of Each Items Before Saving as Work Order.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

            End If

        Catch ex As Exception

            MessageBox.Show("Import Error!" & vbCrLf & vbCrLf & ex.Message, "MESSAGE : ERROR 0054", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub

    Private Sub gridSelectItem_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridSelectItem.SelectionChanged

        'Try

        '    If gridSelectItem.CurrentCell.ColumnIndex = 2 Then
        '        'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.Font = New Font(Control.DefaultFont, FontStyle.Bold)
        '        'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.SelectionBackColor = Color.LightBlue
        '        'gridSelectItem.Item(gridSelectItem.CurrentCell.ColumnIndex, gridSelectItem.CurrentCell.RowIndex).Style.SelectionForeColor = Color.Black
        '    End If

        'Catch ex As Exception
        '    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0055", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '    ErrorCount = ErrorCount + 1
        'End Try

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

            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0055", MessageBoxButtons.OK, MessageBoxIcon.Error)
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

    'Private Sub updateItemData()

    '    'Try
    '    '    ToolStripStatusLabel1.Text = "STATUS : Updating Items ...... "

    '    '    'clsItemLookUp.getLastUpdatedItems()

    '    '    'Dim count = clsItemLookUp.dt3.Rows.Count

    '    '    'If Not count = 0 Then

    '    '    '    updateSuccess = False

    '    '    '    For Each r As DataRow In clsItemLookUp.dt3.Rows

    '    '    '        Try
    '    '    '            SetItemAvailable(r("ItemCode").ToString(), Convert.ToInt64(r("Available")))
    '    '    '        Catch ex As Exception
    '    '    '        End Try

    '    '    '    Next


    '    '    '    updateSuccess = True

    '    '    'End If
    '    '    '    itemData.DataSource = load_data("SELECT * FROM SOD_ViewItems")

    '    '    ToolStripStatusLabel1.Text = "STATUS :"


    '    'Catch ex As Exception

    '    '    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0058", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    '    ErrorCount = ErrorCount + 1

    '    'End Try

    'End Sub

    'Private Sub Update_Timer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Update_Timer.Tick

    '    'If updateSuccess = True Then

    '    '    'If Not db_lastupdate = clsItemLookUp.checkTableUpdate() Then

    '    '    '    db_lastupdate = clsItemLookUp.checkTableUpdate()

    '    '    '    Try

    '    '    '        Thread_1 = New Thread(AddressOf updateItemData)
    '    '    '        Thread_1.Start()

    '    '    '    Catch ex As Exception
    '    '    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0059", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    '    '        ErrorCount = ErrorCount + 1
    '    '    '    End Try

    '    '    'End If

    '    'End If

    'End Sub

    'Private Sub cmdLogout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    'End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)
        frmPrintWo.ShowDialog()
    End Sub

    'Private Sub Check_errors_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Check_errors.Tick

    '    If ErrorCount >= 5 Then
    '        Update_Timer.Stop()
    '        Check_errors.Stop()
    '        MessageBox.Show("The Program Exceeded the Allowed Errors!" & vbCrLf & vbCrLf & "Program will be Restarted !", "IMPORTANT MESSAGE!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    '        Application.Restart()
    '    End If
    'End Sub

    Private Sub cmdForInvoice_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdForInvoice.Click
        frmForInvoice.ShowDialog()
    End Sub

    Private Sub chkWorkOrder_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWorkOrder.CheckedChanged
        If chkWorkOrder.Checked = True Then
            'MsgBox("Set to WORK ORDER", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            rbtnPickup.Enabled = True
            rbtnDelivery.Enabled = True
            chkQuote.Checked = False
            cboPayment.Enabled = True
        End If
    End Sub

    Private Sub chkQuote_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkQuote.CheckedChanged

        If chkQuote.Checked = True Then

            chkWorkOrder.Checked = False
            Dim x = MsgBox("Do you really want to make a Sales Quotation?" & vbCrLf & vbCrLf & "Please be reminded that creating a Sales Quotaion will not be Committed to the System", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
            If x = MsgBoxResult.Yes Then
                chkWorkOrder.Checked = False
                gridSelectItem.Columns(17).Visible = False
                gridSelectItem.Columns(18).Visible = False
                rbtnDelivery.Enabled = False
                rbtnPickup.Enabled = False
                cboPayment.Enabled = False
                cboPayment.SelectedIndex = -1
                chkBoxQtoWo.Checked = False
            Else
                chkWorkOrder.Checked = True
                cboPayment.Enabled = True
                'chkBoxQtoWo.Checked = True
            End If

        End If


    End Sub

    Private Sub frmItemLookUp_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Application.Exit()
    End Sub

    Private Sub MyInputBox(ByVal Prompt As String, ByVal text As String, ByVal btntext As String)

        Dim frmInput As New Form
        frmInput.Owner = Me
        frmInput.StartPosition = FormStartPosition.CenterScreen
        frmInput.ShowIcon = False
        frmInput.Size = New Size(310, 120)
        frmInput.MinimumSize = New Size(315, 120)
        frmInput.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedSingle
        frmInput.KeyPreview = True
        Dim btn As New Button()
        btn.Text = btntext

        btn.Height = 30
        frmInput.Controls.Add(btn)
        frmInput.MaximizeBox = False
        frmInput.MinimizeBox = False
        btn.Location = New Point(210, 165)
        btn.Width = 80
        AddHandler btn.Click, AddressOf inputclose
        Dim lbl As New Label
        lbl.Width = 280
        lbl.Height = 50
        frmInput.Controls.Add(lbl)
        frmInput.ActiveControl = lbl
        frmInput.AcceptButton = btn
        'lbl.TextAlign = HorizontalAlignment.Centered
        lbl.TextAlign = ContentAlignment.MiddleCenter
        lbl.Font = New Font("Century Gothic", 35, FontStyle.Bold)
        lbl.Location = New Point(10, 10)
        lbl.Text = text
        lbl.AutoSize = False
        frmInput.Text = Prompt
        frmInput.ShowDialog()

    End Sub

    Sub inputclose(ByVal s As Object, ByVal e As EventArgs)

        DirectCast(DirectCast(s, Control).Parent, Form).Close()

    End Sub


    Private Sub prompWO(ByVal orderid As String)
        MyInputBox("Work Order Number", orderid, "Close")
    End Sub


    Private Sub chkShowImage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowImage.CheckedChanged
        If chkShowImage.Checked = True Then
            picPanel.Visible = True

            Me.picPanel.Location = New Point(358, gridItem.Size.Height - picPanel.Size.Height - 5)
            picItem.ZoomMode = ZoomPictureBox.ZoomType.MousePosition

        Else
            picPanel.Visible = False
        End If
    End Sub

    Private Sub setPic(ByVal itemcode As String)

        Cursor.Current = Cursors.WaitCursor

        If chkShowImage.Checked = True Then
            Try
                itemImage = Image.FromFile(rmsPath("Pictures") & "HD\" & itemcode & ".JPG")
                picItem.Image = itemImage
            Catch ex As Exception
                picItem.Image = My.Resources.no_image_icon_15
            End Try
        End If

        Cursor.Current = Cursors.Default

    End Sub

    Private Sub gridItem_SelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles gridItem.SelectionChanged
        Try
            itemImage = Nothing
            setPic(gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString())
        Catch ex As Exception

        End Try

        'Try
        '    ' Check if the value is DBNull or Nothing before calling ToString()
        '    If gridItem.Item(0, gridItem.CurrentRow.Index).Value Is DBNull.Value OrElse gridItem.Item(0, gridItem.CurrentRow.Index).Value Is Nothing Then
        '        itemImage = Nothing
        '    Else
        '        ' Proceed with setting the picture if the value is not null
        '        setPic(gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString())
        '    End If
        'Catch ex As Exception
        '    ' Log the exception or handle it in a meaningful way
        '    'MessageBox.Show("An error occurred: " & ex.Message)
        'End Try
    End Sub

    Private Sub picItem_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picItem.MouseLeave
        picItem.Image = itemImage
    End Sub

    Private Sub picPanel_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picPanel.MouseLeave
        picItem.Image = itemImage
    End Sub

    Private Sub frmItemLookUp_SizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged
        Me.picPanel.Location = New Point(358, gridItem.Size.Height - picPanel.Size.Height - 5)
    End Sub

    Private Sub picPanel_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles picPanel.Leave
        picItem.Image = itemImage
    End Sub


    Private Sub forDelivery()

        If rbtnDelivery.Checked = True Then

            For Each xrow As DataGridViewRow In gridSelectItem.Rows

                xrow.Cells(17).ReadOnly = True
                ' xrow.Cells(18).ReadOnly = True

                xrow.Cells(17).Value = True
                'xrow.Cells(18).Value = 0

            Next

        Else
            gridSelectItem.Columns(17).ReadOnly = False
            gridSelectItem.Columns(18).ReadOnly = False
        End If

    End Sub

    Private Sub releaseType()

        If gridSelectItem.Rows.Count > 0 Then

            If rbtnDelivery.Checked = True Then

                Dim x = MsgBox("Changing Release Type to Delivery will Automatically be Picked in UP-STORE" &
                               vbCrLf & vbCrLf & "Do You want to Change ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
                If x = MsgBoxResult.Yes Then


                    forDelivery()


                Else
                    Exit Sub
                End If

            Else

                Dim x = MsgBox("Do You want to Change Release Type to Pick-up?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")
                If x = MsgBoxResult.Yes Then
                    MessageBox.Show("Please Re-Check the Pick Location and Prepared Qty.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    forDelivery()
                Else
                    Exit Sub
                End If

            End If

        End If

    End Sub

    Private Sub rbtnDelivery_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnDelivery.CheckedChanged

        releaseType()

    End Sub

    Private Sub rbtnPickup_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnPickup.CheckedChanged

        releaseType()

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        frmPrintType.ShowDialog()
    End Sub


    Private Function checkIfpickupALL() As Boolean

        Dim itemcount = gridSelectItem.Rows.Count

        Dim prepCout = 0

        For Each xrow As DataGridViewRow In gridSelectItem.Rows

            If xrow.Cells(2).Value = xrow.Cells(18).Value Then
                prepCout = prepCout + 1
            End If

        Next

        If itemcount = prepCout Then
            Return True
        Else
            Return False

        End If

    End Function


    Private Sub cmdImportWeb_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdImportWeb.Click

        Cursor.Current = Cursors.WaitCursor

        clsImport.ImportDataCSV = mysql_load_data("SELECT a.*,b.OrderPlaced,b.CustomerName,b.Shipping FROM SOD_WEBSITE_VIEW_ORDER_ITEMS a inner join SOD_WEBSITE_VIEW_ORDERS b on a.OrderID = b.OrderID Where b.Orderstatus = 'Processing' order by b.OrderID ASC;")
        frmWebsiteImport.ShowDialog()

        Cursor.Current = Cursors.Default


    End Sub

    Private Sub chkWebsite_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkWebsite.CheckedChanged

        If chkWebsite.Checked = True Then

            Dim w_order = WebsiteInputBox("Enter Order Number")
inputOrdernum:
            If w_order = "" Then
                MessageBox.Show("Please Input Order Number!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                w_order = WebsiteInputBox("Enter Order Number")
                GoTo inputOrdernum
            End If

            Dim w_cust = WebsiteInputBox("Enter Customer Name")
inputCust:

            If w_cust = "" Then
                MessageBox.Show("Please Input Customer Name!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                w_cust = WebsiteInputBox("Enter Customer Name")
                GoTo inputCust
            End If

            cboPayment.SelectedIndex = 1

            RemoveHandler rbtnDelivery.CheckedChanged, AddressOf rbtnDelivery_CheckedChanged
            RemoveHandler rbtnPickup.CheckedChanged, AddressOf rbtnPickup_CheckedChanged

            rbtnDelivery.Checked = True

            AddHandler rbtnDelivery.CheckedChanged, AddressOf rbtnDelivery_CheckedChanged
            AddHandler rbtnPickup.CheckedChanged, AddressOf rbtnPickup_CheckedChanged


            txtRemarks.Text = "SOD WEBSITE / Order Number: " & w_order & " / Customer Name: " & w_cust
            txtRemarks.ReadOnly = True

        Else

            RefreshDetails(False)

        End If

    End Sub


    Private Function WebsiteInputBox(ByVal Prompt As String) As String

        Dim frmInput As New Form
        frmInput.Owner = Me
        frmInput.StartPosition = FormStartPosition.CenterScreen
        frmInput.ShowIcon = False
        frmInput.Size = New Size(310, 120)
        frmInput.MinimumSize = New Size(315, 120)
        Dim btn As New Button()
        btn.Text = "Ok"
        btn.Height = 30
        frmInput.Controls.Add(btn)
        frmInput.MaximizeBox = False
        frmInput.MinimizeBox = False
        btn.Location = New Point(210, 45)
        btn.Width = 80
        AddHandler btn.Click, AddressOf inputclose
        Dim txtbox As New TextBox
        txtbox.Width = 280
        frmInput.Controls.Add(txtbox)
        frmInput.ActiveControl = txtbox
        frmInput.AcceptButton = btn
        txtbox.TextAlign = ContentAlignment.MiddleCenter
        txtbox.Font = New Font("Century Gothic", 12)
        txtbox.Location = New Point(10, 10)
        frmInput.Text = Prompt
        frmInput.ShowDialog()

        Return txtbox.Text
    End Function

    'comment
    Private Sub chkBarcode_CheckedChanged(sender As Object, e As EventArgs) Handles chkBarcode.CheckedChanged

        txtBarcode.Focus()

        If chkBarcode.Checked = True Then
            txtBarcode.Enabled = True
            txtBarcode.Text = ""

            '' Check if txtBarcode.Text is Nothing or empty, then set placeholder text
            'If String.IsNullOrEmpty(txtBarcode.Text) Then
            '    txtBarcode.Text = "Barcode Number..."
            'End If

            txtSearch.Enabled = False
            cmdSearch.Enabled = False
            'RefreshDetails(True)
            Try
                ' Check if txtBarcode.Text is empty
                If txtBarcode.Text = String.Empty Or txtBarcode.Text = "Barcode Number..." Then
                    txtBarcode.Focus()
                    'MessageBox.Show("Search cannot be empty !", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Else
                    FilterText2()
                    txtBarcode.Text = String.Empty

                    'SearchItem()
                    SearchBarcode()
                    'SelectPrice()
                End If

            Catch ex As Exception
                MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0057", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ErrorCount = ErrorCount + 1
            End Try

        Else
            txtBarcode.Enabled = False
            txtSearch.Enabled = True
            cmdSearch.Enabled = True
            'searching
            'txtSearch.Focus()
            'RefreshDetails(True)
        End If

    End Sub

    'comment:
    Private Sub FilterText2()
        Try
            ' Clear the TreeView nodes before processing
            TreeView1.Nodes.Clear()

            Dim nodes As TreeNodeCollection = TreeView1.Nodes
            Dim root = New TreeNode(txtBarcode.Text)
            root.Name = txtBarcode.Text

            Dim node As TreeNodeCollection = TreeView1.Nodes
            Dim txt As String = String.Empty

            If txtBarcode.Text <> String.Empty Then
                TreeView1.Nodes.Add(root)
            End If

            For Each n As TreeNode In TreeView1.Nodes
                txt = txt + n.Text + ","
                lastFilterTxt = n.Name
            Next

            Try
                sfilterTxt = txt.Substring(0, txt.Length - 1)
            Catch ex As Exception
                ' Handle substring exception, if any
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0058", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    'comment:
    'Private Sub SearchBarcode()
    '    Try
    '        Dim SearchStrArr() As String = Split(sfilterTxt, ",")
    '        Dim SubDescription2Filter As String = String.Join("' OR [SUBDESCRIPTION2] = '", SearchStrArr)
    '        Dim FilterString As String = "[SUBDESCRIPTION2] = '" & SubDescription2Filter & "'"
    '        Dim FilterStr = "SELECT TOP 500 * FROM SOD_VIEWITEMSWO WHERE Inactive = 0 AND (" & FilterString & ")"

    '        gridItem.DataSource = load_data(FilterStr)

    '        GridColumnWidth()
    '        If isItemCode(searchStr) = True Then

    '            If iCusID = 0 Then
    '                MsgBox("Please select customer first!", vbExclamation, "Message")
    '            Else

    '                Dim availVal = clsItemLookUp.getItemQty(gridItem.Item(0, gridItem.CurrentCell.RowIndex).Value)
    '                If availVal < 1 Then
    '                    MsgBox("This item is out of stock.", vbExclamation, "Message!")
    '                    Exit Sub
    '                Else
    '                    InsertSelectedItem(gridItem.CurrentCell.RowIndex)
    '                    gridSelectItem.Focus()
    '                    gridSelectItem.BeginEdit(True)
    '                End If
    '                TreeView1.Nodes.Clear()
    '            End If

    '        End If
    '        checkSearch1()
    '        TreeView1.Nodes.Clear()
    '        iRow = 0
    '        txtBarcode.Focus()
    '        lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0059", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try
    'End Sub
    'Private Sub SearchBarcode()
    '    Try
    '        Dim SearchStrArr() As String = Split(sfilterTxt, ",")

    '        Dim BarcodeFilter As String = String.Join("%' OR [SUBDESCRIPTION2] LIKE '%", SearchStrArr)
    '        Dim ItemLookupFilter As String = String.Join("%' OR [ItemLookupCode] LIKE '%", SearchStrArr)

    '        Dim FilterString As String =
    '        "[SUBDESCRIPTION2] LIKE '%" & BarcodeFilter & "%' OR " &
    '        "[ItemLookupCode] LIKE '%" & ItemLookupFilter & "%'"

    '        Dim FilterStr As String =
    '        "SELECT TOP 500 * FROM SOD_VIEWITEMSWO WHERE Inactive = 0 AND (" & FilterString & ")"

    '        gridItem.DataSource = load_data(FilterStr)

    '        GridColumnWidth()

    '        If isItemCode(searchStr) = True Then

    '            If iCusID = 0 Then
    '                MsgBox("Please select customer first!", vbExclamation, "Message")
    '            Else

    '                Dim availVal = clsItemLookUp.getItemQty(gridItem.Item(0, gridItem.CurrentCell.RowIndex).Value)

    '                If availVal < 1 Then
    '                    MsgBox("This item is out of stock.", vbExclamation, "Message!")
    '                    Exit Sub
    '                Else
    '                    InsertSelectedItem(gridItem.CurrentCell.RowIndex)
    '                    gridSelectItem.Focus()
    '                    gridSelectItem.BeginEdit(True)
    '                End If

    '                TreeView1.Nodes.Clear()

    '            End If

    '        End If

    '        checkSearch1()
    '        TreeView1.Nodes.Clear()
    '        iRow = 0
    '        txtBarcode.Focus()

    '        lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0059", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try
    'End Sub
    Private Sub SearchBarcode()
        Try
            Dim SearchStrArr() As String = Split(sfilterTxt, ",")
            Dim lf As String = Chr(10)

            ' Build filter conditions for each search term
            Dim conditions As New List(Of String)

            For Each term As String In SearchStrArr
                Dim t As String = term.Trim().Replace("'", "''")

                ' ItemLookupCode: exact match OR ends-with match
                Dim itemCodeCondition As String =
                "([ItemLookupCode] = '" & t & "'" &
                " OR [ItemLookupCode] LIKE '%" & t & "')"

                ' SubDescription2: exact token match
                ' Handles newline-separated AND +-separated barcodes
                Dim subDesc2Condition As String =
                "([SUBDESCRIPTION2] = '" & t & "'" &
                " OR [SUBDESCRIPTION2] LIKE '" & t & lf & "%'" &
                " OR [SUBDESCRIPTION2] LIKE '%" & lf & t & "'" &
                " OR [SUBDESCRIPTION2] LIKE '%" & lf & t & lf & "%'" &
                " OR [SUBDESCRIPTION2] LIKE '" & t & "+%'" &
                " OR [SUBDESCRIPTION2] LIKE '%+" & t & "'" &
                " OR [SUBDESCRIPTION2] LIKE '%+" & t & "+%')"

                conditions.Add("(" & itemCodeCondition & " OR " & subDesc2Condition & ")")
            Next

            ' Join multiple search terms with AND (all terms must match somewhere)
            Dim FilterString As String = String.Join(" AND ", conditions)

            Dim FilterStr As String =
            "SELECT TOP 500 * FROM SOD_VIEWITEMSWO " &
            "WHERE Inactive = 0 AND (" & FilterString & ")"

            gridItem.DataSource = load_data(FilterStr)
            GridColumnWidth()

            If isItemCode(searchStr) = True Then

                If iCusID = 0 Then
                    MsgBox("Please select customer first!", vbExclamation, "Message")
                Else
                    Dim availVal = clsItemLookUp.getItemQty(gridItem.Item(0, gridItem.CurrentCell.RowIndex).Value)

                    If availVal < 1 Then
                        MsgBox("This item is out of stock.", vbExclamation, "Message!")
                        Exit Sub
                    Else
                        InsertSelectedItem(gridItem.CurrentCell.RowIndex)
                        gridSelectItem.Focus()
                        gridSelectItem.BeginEdit(True)
                    End If

                    TreeView1.Nodes.Clear()
                End If

            End If

            checkSearch1()
            TreeView1.Nodes.Clear()
            iRow = 0
            txtBarcode.Focus()

            lblStatItemCount.Text = gridItem.RowCount & " item" & IIf(gridItem.RowCount > 1, "s", "")

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message,
                        "MESSAGE : ERROR 0059", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub
    'comment:
    Private Sub checkSearch1()

        Try

            If gridItem.RowCount = 0 Then

                MessageBox.Show("No Items Found!", "Message !", MessageBoxButtons.OK, MessageBoxIcon.Error)
                txtBarcode.Text = ""
                txtBarcode.Focus()

                'Try
                '    Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                '    TreeView1.Nodes.Remove(node(0))
                '    FilterText()

                '    If TreeView1.Nodes.Count = 0 Then
                '        itemData.Filter = Nothing
                '    Else
                '        SearchBarcode()
                '    End If

                'Catch ex As Exception
                '    MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0060", MessageBoxButtons.OK, MessageBoxIcon.Error)
                '    ErrorCount = ErrorCount + 1
                'End Try

                Me.ActiveControl = txtBarcode

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0060", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    'comment:
    Private Function isItemCode(ByVal str As String) As Boolean

        Dim count_num As Integer = 0
        Dim count_str As Integer = 0
        Dim i As Integer
        Dim arr() As Char = str.ToCharArray()

        For i = 0 To arr.Length - 1
            If Char.IsDigit(arr(i)) Then
                count_num = count_num + 1
            Else
                count_str = count_str + 1
            End If
        Next

        If count_num = 5 And count_str >= 3 Then
            Return True
        Else
            Return False
        End If

    End Function

    'comment:
    Private Sub txtBarcode_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtBarcode.KeyPress

        Try
            ' Check if Enter key is pressed
            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                ' If the barcode field is empty
                If String.IsNullOrEmpty(txtBarcode.Text) Then
                    MessageBox.Show("Search cannot be empty!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    txtBarcode.Text = String.Empty ' Clear the barcode
                    Exit Sub
                    ' Check if the barcode contains letters
                    'ElseIf txtBarcode.Text.Any(Function(c) Char.IsLetter(c)) Then
                    '    MessageBox.Show("Barcode cannot contain letters!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    '    txtBarcode.Text = String.Empty ' Clear the barcode
                    '    Exit Sub
                Else
                    ' Perform the search and other logic
                    FilterText2() ' Assuming this performs filtering

                    ' Clear the barcode text and perform the barcode search
                    SearchBarcode()
                    TreeView1.Nodes.Clear() ' Clear any previous tree nodes

                    ' Get values from grid (ensure valid grid data)
                    If gridItem.CurrentRow IsNot Nothing Then
                        'SelectPrice()
                    Else
                        'MessageBox.Show("No valid row selected in the grid.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        txtBarcode.Text = String.Empty
                        Exit Sub
                    End If
                End If
            End If

            ' Handle the focus of txtBarcode (make sure the cursor stays at the end of the text)
            If Not Me.txtBarcode.Focused Then
                txtBarcode.Focus()
                txtBarcode.SelectionStart = txtBarcode.Text.Length ' Move the cursor to the end
                e.Handled = True ' Prevent further processing of the key press
            End If

        Catch ex As Exception
            ' Handle any exceptions and log them
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0061", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
        End Try
    End Sub

    'comment:
    Private Sub txtBarcode_KeyUp(sender As Object, e As KeyEventArgs) Handles txtBarcode.KeyUp
        Try
            If e.KeyCode = Keys.F5 Then

                TreeView1.Nodes.Clear()
                itemData.Filter = Nothing
                lblStatItemCount.Text = gridItem.RowCount & " items"
                txtBarcode.Text = String.Empty
                Me.ActiveControl = txtSearch

            End If

            If e.KeyCode = Keys.F4 Then
                If txtBarcode.Text = String.Empty Then
                    Try
                        Dim node() As TreeNode = TreeView1.Nodes.Find(lastFilterTxt, True)
                        TreeView1.Nodes.Remove(node(0))
                        FilterText2()

                        If TreeView1.Nodes.Count = 0 Then
                            itemData.Filter = Nothing
                        Else
                            SearchBarcode()
                        End If

                        Me.ActiveControl = txtBarcode
                    Catch ex As Exception

                    End Try
                Else
                    MessageBox.Show("Please Clear the search box before removing the last searched keyword", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If

            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0062", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    'comment:
    Private Sub txtBarcode_KeyDown(sender As Object, e As KeyEventArgs) Handles txtBarcode.KeyDown
        Try

            If e.KeyCode = Keys.Down Then
                gridItem.Focus()
                ' SelectPrice()
            ElseIf e.KeyCode = Keys.F9 Then
                txtBarcode.Text = sOldDes
                txtBarcode.SelectionStart = txtBarcode.TextLength
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0063", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    'comment:
    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Timer1.Start()
    End Sub


    '-------------mao ni tung code para magupdate and qty ug price
    Public Sub UpdatePriceFromPriceLevel(ByVal newPrice As Double)
        ' Assuming the price needs to be updated in the currently selected row
        If gridSelectItem.CurrentRow IsNot Nothing Then
            gridSelectItem.CurrentRow.Cells(3).Value = newPrice ' Update unit price
            'Dim newQty As Integer = Convert.ToInt32(gridSelectItem.CurrentRow.Cells(2).Value)
            Dim newQty As Integer = gridSelectItem.CurrentRow.Cells(2).Value
            gridSelectItem.CurrentRow.Cells(5).Value = newQty * newPrice ' Update total price

            '------Para mu update pud ang SubTotal ug VAT ug Total mao ni ang function para muupdate sya ---------------------'

            UpdateAmt()

        End If
    End Sub


    Public Sub ValidatePriceInput()

        Try
            ' Ensure a valid row is selected
            If gridSelectItem.CurrentRow IsNot Nothing Then
                ' Get the ItemCode and currentPrice from the selected row
                Dim itemCode As String = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value.ToString()
                Dim currentPrice As Decimal = Convert.ToDecimal(gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value)

                ' Retrieve the item from the database
                'Dim item = db.Items.FirstOrDefault(Function(i) i.ItemLookupCode = itemCode)
                Dim item = (From a In db.Items Where a.ItemLookupCode.Equals(itemCode) Select a).FirstOrDefault

                'MessageBox.Show("Itemcode " & itemCode)
                'MessageBox.Show("currentPrice " & currentPrice)

                ' Initialize price variable
                Dim price As Decimal? = Nothing

                ' Check if item is found and retrieve the appropriate price
                If item IsNot Nothing Then
                    Select Case Me.txtPriceLevel.Text
                        Case "Price"
                            price = item.Price
                        Case "PriceA"
                            price = item.PriceA
                        Case "PriceB"
                            price = item.PriceB
                        Case "PriceC"
                            price = item.PriceC
                        Case Else
                            price = Nothing
                    End Select
                End If

                ' Display the price or a message if no matching price is found
                If price.HasValue Then

                    Me.txtPrice.Text = price.Value.ToString("N2")

                    ' If currentPrice is less than the txtPrice, show the password form
                    If currentPrice < Convert.ToDecimal(txtPrice.Text) Then
                        ' Save the original currentPrice
                        Dim originalPrice As Decimal = curPrice

                        ' Show the password dialog
                        'Dim pDialog As DialogResult = frmPassword.ShowDialog()

                        '' If the dialog is canceled or closed, revert to the original price
                        'If pDialog = Windows.Forms.DialogResult.Cancel Then
                        '    ' Explicitly reset the current price in the grid
                        '    'gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value = originalPrice
                        '    gridSelectItem.CurrentCell.Value = curPrice
                        'End If
                    End If
                Else
                    Me.txtPrice.Text = "No matching price found"
                End If
            Else
                MessageBox.Show("Please select a valid row.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    'Public Sub ValidatePriceInput()
    '    Try
    '        ' Ensure a valid row is selected
    '        If gridSelectItem.CurrentRow Is Nothing Then
    '            MessageBox.Show("Please select a valid row.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '            Exit Sub
    '        End If

    '        ' Get item code and current price from the selected row
    '        Dim itemCode As String = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value.ToString()
    '        Dim currentPrice As Decimal = Convert.ToDecimal(gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value)

    '        ' Retrieve item details from database
    '        Dim item = db.Items.FirstOrDefault(Function(i) i.ItemLookupCode = itemCode)
    '        If item Is Nothing Then
    '            txtPrice.Text = "No matching price found"
    '            Exit Sub
    '        End If

    '        ' Determine the appropriate price level
    '        Dim selectedPrice As Decimal? = Nothing
    '        Select Case txtPriceLevel.Text
    '            Case "Price" : selectedPrice = item.Price
    '            Case "PriceA" : selectedPrice = item.PriceA
    '            Case "PriceB" : selectedPrice = item.PriceB
    '            Case "PriceC" : selectedPrice = item.PriceC
    '        End Select

    '        ' Display and compare prices
    '        If selectedPrice.HasValue Then
    '            txtPrice.Text = selectedPrice.Value.ToString("N2")
    '            ' If user-entered price is lower than allowed, prompt for override
    '            If currentPrice < selectedPrice.Value Then
    '                ' Uncomment below if you want to enforce password check
    '                'frmPassword.sType = "EditPrice"
    '                'Dim pDialog As DialogResult = frmPassword.ShowDialog()
    '                'If pDialog = DialogResult.Cancel Then
    '                '    gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value = curPrice
    '                'End If
    '            End If
    '        Else
    '            txtPrice.Text = "No matching price found"
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("An error occurred: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub

    ' --------------------- CODE ni sya para e fetch ang tama na item sa gridlevel sa frmLinqLevel --------------
    Public Sub filterLinqLevel()
        Try
            ' Ensure a valid row is selected
            If gridSelectItem.CurrentRow IsNot Nothing Then
                ' Get the ItemCode and currentPrice from the selected row
                Dim itemCode As String = gridSelectItem.Item(0, gridSelectItem.CurrentRow.Index).Value.ToString()
                Dim currentPrice As Double = Convert.ToDouble(gridSelectItem.Item(3, gridSelectItem.CurrentRow.Index).Value)

                ' Show the ItemCode and Price in a message box (for debugging)
                'MessageBox.Show("Item Code: " & itemCode & vbCrLf & "Current Price: " & currentPrice.ToString(), "Selected Item Info", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Open frmLinqLevel and pass ItemCode and Price
                Dim frm As New frmLinqLevel()
                frmLinqLevel.lblItemCode.Text = itemCode
                'frm.sItemName = "Your Item Name" ' Set the item name if available
                'frm.dPrice = currentPrice ' Pass the current price to the frmLinqLevel

                frmLinqLevel.ShowDialog() ' Show the frmLinqLevel form
            End If

        Catch ex As Exception
            ' Handle the error if needed
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    'Private Sub txtTotal_TextChanged(sender As Object, e As EventArgs) Handles txtTotal.TextChanged
    '    Dim total As Decimal
    '    Dim available As Decimal

    '    ' Try parsing both textboxes safely
    '    If Decimal.TryParse(txtTotal.Text, total) AndAlso Decimal.TryParse(txtAvailable.Text, available) Then
    '        If total > available Then
    '            MessageBox.Show("Exceeded available credit limit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '        End If
    '    End If
    'End Sub

    Private Sub txtTotal_TextChanged(sender As Object, e As EventArgs) Handles txtTotal.TextChanged
        Dim total As Decimal
        Dim available As Decimal

        ' Check if ComboBox selection is "Charge"
        If cboPayment.SelectedItem IsNot Nothing AndAlso cboPayment.SelectedItem.ToString().ToLower() = "charge" Then
            ' Try parsing both textboxes safely
            If Decimal.TryParse(txtTotal.Text, total) AndAlso Decimal.TryParse(txtAvailable.Text, available) Then
                If total > available Then
                    MessageBox.Show("Exceeded available credit limit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            RefreshDetails(True)

            Me.ToolStripStatusLabel3.Text = "Register : "
            Me.ToolStripStatusLabel2.Text = "User : "

            itemData.DataSource = Nothing
            Update_Timer.Enabled = False
            frmLogin.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("FROM : frmItemlookUp Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0056", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub cmdOpenWO_Click(sender As Object, e As EventArgs) Handles cmdOpenWO.Click
        Try

            If Me.gridItem.Rows.Count = 0 Then
                Exit Sub
            End If

            frmCheckWO.lblItemCodeWO.Text = gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString()
            frmCheckWO.lblItemNameWO.Text = gridItem.Item(1, gridItem.CurrentRow.Index).Value.ToString()
            frmCheckWO.ShowDialog()

        Catch ex As Exception
            MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 28", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub


    'Private Sub cmdOpenWO_Click_1(sender As Object, e As EventArgs) Handles cmdOpenWO.Click
    '    Try

    '        If Me.gridItem.Rows.Count = 0 Then
    '            Exit Sub
    '        End If

    '        frmCheckWO.lblItemCodeWO.Text = gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString()
    '        frmCheckWO.lblItemNameWO.Text = gridItem.Item(1, gridItem.CurrentRow.Index).Value.ToString()
    '        frmCheckWO.ShowDialog()

    '    Catch ex As Exception
    '        MessageBox.Show("FROM  frmItemLookUp Form " & vbCrLf & vbCrLf & "REASON :  " & ex.Message, "MESSAGE : Error 28", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try
    'End Sub

    'Private Function HasInvalidPrice() As Boolean

    '    Try

    '        If IsNumeric(Split(lblOrderNo.Text, ": ")(1)) Then
    '            MessageBox.Show("has work order")
    '        End If

    '        For Each row As DataGridViewRow In gridSelectItem.Rows
    '            If row.IsNewRow Then Continue For

    '            Dim itemCode As String = row.Cells(0).Value?.ToString()
    '            Dim currentPrice As Decimal

    '            If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '            If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '            ' Fetch item from DB
    '            Dim item = (From a In db.Items
    '                        Where a.ItemLookupCode.Equals(itemCode)
    '                        Select a).FirstOrDefault()

    '            If item IsNot Nothing Then
    '                Dim allowedPrice As Decimal?

    '                Select Case Me.txtPriceLevel.Text
    '                    Case "Price" : allowedPrice = item.Price
    '                    Case "PriceA" : allowedPrice = item.PriceA
    '                    Case "PriceB" : allowedPrice = item.PriceB
    '                    Case "PriceC" : allowedPrice = item.PriceC
    '                    Case Else : allowedPrice = Nothing
    '                End Select

    '                ' Check if user price is below allowed price
    '                If allowedPrice.HasValue AndAlso currentPrice < allowedPrice.Value Then
    '                    Return True ' password required
    '                End If

    '                ' Color only rows that are below allowed price
    '                If Decimal.TryParse(row.Cells(3).Value?.ToString(), currentPrice) AndAlso currentPrice < allowedPrice Then
    '                    MarkRowAsPriceOverridden(row)
    '                End If

    '            End If
    '        Next
    '    Catch ex As Exception
    '        MessageBox.Show("Error during price validation: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try

    '    Return False ' all prices >= allowed → no password
    'End Function

    'Private Sub RevertInvalidPrices()
    '    For Each row As DataGridViewRow In gridSelectItem.Rows
    '        If row.IsNewRow Then Continue For

    '        Dim itemCode As String = row.Cells(0).Value?.ToString()
    '        Dim currentPrice As Decimal

    '        If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '        If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '        Dim item = (From a In db.Items
    '                    Where a.ItemLookupCode.Equals(itemCode)
    '                    Select a).FirstOrDefault()

    '        If item IsNot Nothing Then
    '            Dim allowedPrice As Decimal?

    '            Select Case Me.txtPriceLevel.Text
    '                Case "Price" : allowedPrice = item.Price
    '                Case "PriceA" : allowedPrice = item.PriceA
    '                Case "PriceB" : allowedPrice = item.PriceB
    '                Case "PriceC" : allowedPrice = item.PriceC
    '                Case Else : allowedPrice = Nothing
    '            End Select

    '            If allowedPrice.HasValue AndAlso currentPrice < allowedPrice.Value Then
    '                row.Cells(3).Value = allowedPrice.Value ' reset to allowed price
    '                row.DefaultCellStyle.BackColor = Color.White
    '                row.DefaultCellStyle.ForeColor = Color.Black
    '            End If
    '        End If
    '    Next
    'End Sub

    Private Sub MarkRowAsPriceOverridden(row As DataGridViewRow)
        ' Change the background color of the entire row
        DatagridColor = 1
        row.DefaultCellStyle.BackColor = Color.Yellow
        row.DefaultCellStyle.ForeColor = Color.Black

    End Sub

    'Validate Save function ni sya
    'Private Function ValidateAndSave() As Boolean
    '    'Try
    '    Dim needsPassword As Boolean = False
    '        Dim rowsNeedingOverride As New List(Of DataGridViewRow)

    '        ' Step 1: Scan rows to check if any price is invalid
    '        For Each row As DataGridViewRow In gridSelectItem.Rows
    '            If row.IsNewRow Then Continue For

    '            Dim itemCode As String = row.Cells(0).Value?.ToString()
    '            Dim currentPrice As Decimal

    '            If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '            If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '            ' --- Get item details ---
    '            Dim item = (From a In db.Items
    '                        Where a.ItemLookupCode.Equals(itemCode)
    '                        Select a).FirstOrDefault()
    '            If item Is Nothing Then Continue For

    '            ' --- Determine price bounds (selected level = lower bound) ---
    '            Dim lowerPrice As Decimal = item.Price
    '            Dim upperPrice As Decimal = item.PriceC
    '            Select Case Me.txtPriceLevel.Text
    '                Case "Price" : lowerPrice = item.Price : upperPrice = item.Price
    '                Case "PriceA" : lowerPrice = item.PriceA : upperPrice = item.PriceA
    '                Case "PriceB" : lowerPrice = item.PriceB : upperPrice = item.PriceB
    '                Case "PriceC" : lowerPrice = item.PriceC : upperPrice = item.PriceC
    '            End Select

    '            Dim allowedPrice As Decimal? = Nothing

    '            If WoRecallType = 0 Then
    '                ' --- New order: must not go below selected price ---
    '                allowedPrice = lowerPrice

    '            ElseIf WoRecallType = 1 Then
    '                ' --- Recall order ---
    '                Dim orderID As Integer = Convert.ToInt32(Split(lblOrderNo.Text, ": ")(1))

    '                ' Get Work Order price if item already existed in WO
    '                Dim orderEntry = (From o In db.OrderEntries
    '                                  Join i In db.Items On i.ID Equals o.ItemID
    '                                  Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                                  Select o).FirstOrDefault()

    '                If orderEntry IsNot Nothing Then
    '                    Dim woOrderPrice As Decimal = orderEntry.Price

    '                    ' --- Validation logic (same as ValidatePriceRow) ---
    '                    If currentPrice = woOrderPrice Then
    '                        ' exactly the same as WO price → valid
    '                        allowedPrice = currentPrice

    '                    ElseIf currentPrice >= lowerPrice AndAlso currentPrice <= upperPrice Then
    '                        ' within allowed price range → valid
    '                        allowedPrice = currentPrice

    '                    Else
    '                        ' outside WO price and range → requires password
    '                        allowedPrice = Math.Max(woOrderPrice, lowerPrice)
    '                    End If
    '                Else
    '                    ' New item in recall: treat as new order
    '                    allowedPrice = lowerPrice
    '                End If
    '            End If

    '            ' --- Compare final price ---
    '            If allowedPrice.HasValue AndAlso currentPrice < allowedPrice.Value Then
    '                needsPassword = True
    '                rowsNeedingOverride.Add(row)
    '            End If
    '        Next

    '        ' Step 2: Ask for password if needed
    '        If needsPassword AndAlso Not bAllow Then
    '            frmPassword.sType = "EditPrice"
    '            MessageBox.Show("This work order requires a password to proceed.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)

    '            If frmPassword.ShowDialog() <> DialogResult.OK Then
    '                Return False ' stop saving
    '            End If

    '            ' Step 3: Mark overridden rows
    '            For Each row In rowsNeedingOverride
    '                MarkRowAsPriceOverridden(row)
    '            Next
    '        End If

    '        ' Step 4: Save the order
    '        SaveOrder()
    '        Return True

    '    'Catch ex As Exception
    '    '    MessageBox.Show("Error during validation: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    '    Return False
    '    'End Try
    'End Function

    'Private Function ValidateAndSave() As Boolean
    '    Try
    '        Dim needsPassword As Boolean = False
    '        Dim rowsNeedingOverride As New List(Of DataGridViewRow)

    '        ' Step 1: Scan rows to check if any price is invalid
    '        For Each row As DataGridViewRow In gridSelectItem.Rows
    '            If row.IsNewRow Then Continue For

    '            Dim itemCode As String = row.Cells(0).Value?.ToString()
    '            Dim currentPrice As Decimal

    '            If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '            If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '            ' --- Get item details ---
    '            Dim item = (From a In db.Items
    '                        Where a.ItemLookupCode.Equals(itemCode)
    '                        Select a).FirstOrDefault()
    '            If item Is Nothing Then Continue For

    '            ' --- Determine price bounds (selected level = lower bound) ---
    '            Dim lowerPrice As Decimal = item.Price
    '            Dim upperPrice As Decimal = item.PriceC
    '            Select Case Me.txtPriceLevel.Text
    '                Case "Price" : lowerPrice = item.Price : upperPrice = item.Price
    '                Case "PriceA" : lowerPrice = item.PriceA : upperPrice = item.PriceA
    '                Case "PriceB" : lowerPrice = item.PriceB : upperPrice = item.PriceB
    '                Case "PriceC" : lowerPrice = item.PriceC : upperPrice = item.PriceC
    '            End Select

    '            Dim allowedPrice As Decimal? = Nothing

    '            If WoRecallType = 0 Then
    '                ' --- New order: must not go below selected price ---
    '                allowedPrice = lowerPrice

    '            ElseIf WoRecallType = 1 Then
    '                ' --- Recall order ---
    '                'Dim orderID As Integer = Convert.ToInt32(Split(lblOrderNo.Text, ": ")(1))
    '                Dim orderID As Integer
    '                Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()

    '                If Not Integer.TryParse(lastPart, orderID) Then
    '                    'Throw New Exception("Invalid Work Order number: " & lblOrderNo.Text)
    '                    WoRecallType = 0
    '                End If


    '                ' Get Work Order price if item already existed in WO
    '                Dim orderEntry = (From o In db.OrderEntries
    '                                  Join i In db.Items On i.ID Equals o.ItemID
    '                                  Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                                  Select o).FirstOrDefault()

    '                If orderEntry IsNot Nothing Then
    '                    Dim woOrderPrice As Decimal = orderEntry.Price

    '                    ' --- Validation logic (same as ValidatePriceRow) ---
    '                    If currentPrice = woOrderPrice Then
    '                        ' exactly the same as WO price → valid
    '                        allowedPrice = currentPrice

    '                    ElseIf currentPrice >= lowerPrice AndAlso currentPrice <= upperPrice Then
    '                        ' within allowed price range → valid
    '                        allowedPrice = currentPrice

    '                    Else
    '                        ' outside WO price and range → requires password
    '                        allowedPrice = Math.Max(woOrderPrice, lowerPrice)
    '                    End If
    '                Else
    '                    ' New item in recall: treat as new order
    '                    allowedPrice = lowerPrice
    '                End If
    '            End If

    '            ' --- Compare final price ---
    '            If allowedPrice.HasValue AndAlso currentPrice < allowedPrice.Value Then
    '                needsPassword = True
    '                rowsNeedingOverride.Add(row)
    '            End If
    '        Next

    '        ' Step 2: Ask for password if needed
    '        If needsPassword AndAlso Not bAllow Then
    '            frmPassword.sType = "EditPrice"
    '            MessageBox.Show("This work order requires a password to proceed.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)

    '            If frmPassword.ShowDialog() <> DialogResult.OK Then
    '                Return False ' stop saving
    '            End If

    '            ' Step 3: Mark overridden rows
    '            For Each row In rowsNeedingOverride
    '                MarkRowAsPriceOverridden(row)

    '            Next
    '        End If

    '        ' Step 4: Save the order
    '        SaveOrder()
    '        Return True

    '    Catch ex As Exception
    '        MessageBox.Show("Error during validation: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    Return False
    '    End Try
    'End Function

    'Private Function ValidateAndSave() As Boolean
    '    Dim needsPassword As Boolean = False
    '    Dim rowsNeedingOverride As New List(Of DataGridViewRow)

    '    For Each row As DataGridViewRow In gridSelectItem.Rows
    '        If row.IsNewRow Then Continue For

    '        Dim itemCode As String = row.Cells(0).Value?.ToString()
    '        Dim currentPrice As Decimal

    '        If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '        If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '        Dim item = (From a In db.Items
    '                    Where a.ItemLookupCode.Equals(itemCode)
    '                    Select a).FirstOrDefault()

    '        If item Is Nothing Then Continue For

    '        ' --- Determine price range based on customer's price level ---
    '        Dim minPrice As Decimal ' Customer's assigned tier (lower bound)
    '        Dim maxPrice As Decimal = item.Price ' Always base price (upper bound)

    '        Select Case Me.txtPriceLevel.Text
    '            Case "Price"
    '                minPrice = item.Price
    '                maxPrice = item.Price ' Range: $73.50 - $73.50
    '            Case "PriceA"
    '                minPrice = item.PriceA ' Range: $68.60 - $73.50
    '            Case "PriceB"
    '                minPrice = item.PriceB ' Range: $63.70 - $73.50
    '            Case "PriceC"
    '                minPrice = item.PriceC ' Range: $61.25 - $73.50
    '            Case Else
    '                minPrice = item.Price
    '                maxPrice = item.Price
    '        End Select

    '        Dim requiresOverride As Boolean = False

    '        If WoRecallType = 0 Then
    '            ' --- New order: price must be within customer's range ---
    '            If currentPrice < minPrice OrElse currentPrice > maxPrice Then
    '                requiresOverride = True
    '            End If

    '        ElseIf WoRecallType = 1 Then
    '            ' --- Recall order ---
    '            Dim orderID As Integer
    '            Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
    '            If Not Integer.TryParse(lastPart, orderID) Then
    '                'Throw New Exception("Invalid Work Order number: " & lblOrderNo.Text)
    '                WoRecallType = 0
    '            End If

    '            Dim orderEntry = (From o In db.OrderEntries
    '                              Join i In db.Items On i.ID Equals o.ItemID
    '                              Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                              Select o).FirstOrDefault()

    '            If orderEntry IsNot Nothing Then
    '                Dim woOrderPrice As Decimal = orderEntry.Price

    '                ' Valid if: matches WO price OR within customer's range
    '                Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
    '                                          (currentPrice >= minPrice AndAlso currentPrice <= maxPrice)

    '                If Not isValidPrice Then
    '                    requiresOverride = True
    '                End If
    '            Else
    '                ' New item in recall: treat as new order
    '                If currentPrice < minPrice OrElse currentPrice > maxPrice Then
    '                    requiresOverride = True
    '                End If
    '            End If
    '        End If

    '        If requiresOverride Then
    '            needsPassword = True
    '            rowsNeedingOverride.Add(row)
    '        End If
    '    Next

    '    ' Step 2: Ask for password if needed
    '    If needsPassword AndAlso Not bAllow Then
    '        frmPassword.sType = "EditPrice"
    '        MessageBox.Show("This work order requires a password to proceed.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '        If frmPassword.ShowDialog() <> DialogResult.OK Then
    '            Return False
    '        End If

    '        For Each row In rowsNeedingOverride
    '            MarkRowAsPriceOverridden(row)
    '        Next
    '    End If

    '    SaveOrder()
    '    Return True
    'End Function
    'Private Function ValidateAndSave() As Boolean
    '    Try


    '        Dim needsPassword As Boolean = False
    '        Dim rowsNeedingOverride As New List(Of DataGridViewRow)

    '        For Each row As DataGridViewRow In gridSelectItem.Rows
    '            If row.IsNewRow Then Continue For

    '            Dim itemCode As String = row.Cells(0).Value?.ToString()
    '            Dim currentPrice As Decimal

    '            If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
    '            If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

    '            Dim item = (From a In db.Items
    '                        Where a.ItemLookupCode.Equals(itemCode)
    '                        Select a).FirstOrDefault()

    '            If item Is Nothing Then Continue For

    '            ' --- Determine price range with $0.00 fallback logic ---
    '            Dim minPrice As Decimal
    '            Dim maxPrice As Decimal = item.Price

    '            Select Case Me.txtPriceLevel.Text
    '                Case "Price"
    '                    minPrice = item.Price
    '                    maxPrice = item.Price

    '                Case "PriceA"
    '                    If item.PriceA > 0 Then
    '                        minPrice = item.PriceA
    '                    Else
    '                        minPrice = item.Price
    '                        maxPrice = item.Price
    '                    End If

    '                Case "PriceB"
    '                    If item.PriceB > 0 Then
    '                        minPrice = item.PriceB
    '                    ElseIf item.PriceA > 0 Then
    '                        minPrice = item.PriceA
    '                    Else
    '                        minPrice = item.Price
    '                        maxPrice = item.Price
    '                    End If

    '                Case "PriceC"
    '                    If item.PriceC > 0 Then
    '                        minPrice = item.PriceC
    '                    ElseIf item.PriceB > 0 Then
    '                        minPrice = item.PriceB
    '                    ElseIf item.PriceA > 0 Then
    '                        minPrice = item.PriceA
    '                    Else
    '                        minPrice = item.Price
    '                        maxPrice = item.Price
    '                    End If

    '                Case Else
    '                    minPrice = item.Price
    '                    maxPrice = item.Price
    '            End Select

    '            Dim requiresOverride As Boolean = False

    '            If WoRecallType = 0 Then
    '                ' --- New order: price must be within customer's range ---
    '                If currentPrice < minPrice OrElse currentPrice > maxPrice Then
    '                    requiresOverride = True
    '                End If

    '            ElseIf WoRecallType = 1 Then
    '                ' --- Recall order ---
    '                Dim orderID As Integer
    '                Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
    '                If Not Integer.TryParse(lastPart, orderID) Then
    '                    'Throw New Exception("Invalid Work Order number: " & lblOrderNo.Text)
    '                    WoRecallType = 0
    '                End If

    '                Dim orderEntry = (From o In db.OrderEntries
    '                                  Join i In db.Items On i.ID Equals o.ItemID
    '                                  Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                                  Select o).FirstOrDefault()

    '                If orderEntry IsNot Nothing Then
    '                    Dim woOrderPrice As Decimal = orderEntry.Price

    '                    ' Valid if: matches WO price OR within customer's range
    '                    Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
    '                                              (currentPrice >= minPrice AndAlso currentPrice <= maxPrice)

    '                    If Not isValidPrice Then
    '                        requiresOverride = True
    '                    End If
    '                Else
    '                    ' New item in recall: treat as new order
    '                    If currentPrice < minPrice OrElse currentPrice > maxPrice Then
    '                        requiresOverride = True
    '                    End If
    '                End If
    '            End If

    '            If requiresOverride Then
    '                needsPassword = True
    '                rowsNeedingOverride.Add(row)
    '            End If
    '        Next

    '        ' Step 2: Ask for password if needed
    '        If needsPassword AndAlso Not bAllow Then
    '            frmPassword.sType = "EditPrice"
    '            MessageBox.Show("This work order requires a password to proceed.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '            If frmPassword.ShowDialog() <> DialogResult.OK Then
    '                Return False
    '            End If

    '            For Each row In rowsNeedingOverride
    '                MarkRowAsPriceOverridden(row)
    '            Next
    '        End If

    '        SaveOrder()
    '        Return True
    '    Catch ex As Exception
    '        MessageBox.Show("Error during validation: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        Return False
    '    End Try

    'End Function

    Private Function ValidateAndSave() As Boolean
        Dim needsPassword As Boolean = False
        Dim rowsNeedingOverride As New List(Of DataGridViewRow)

        For Each row As DataGridViewRow In gridSelectItem.Rows
            If row.IsNewRow Then Continue For

            Dim itemCode As String = row.Cells(0).Value?.ToString()
            Dim currentPrice As Decimal

            If String.IsNullOrEmpty(itemCode) OrElse row.Cells(3).Value Is Nothing Then Continue For
            If Not Decimal.TryParse(row.Cells(3).Value.ToString(), currentPrice) Then Continue For

            Dim item = (From a In db.Items
                        Where a.ItemLookupCode.Equals(itemCode)
                        Select a).FirstOrDefault()

            If item Is Nothing Then Continue For

            ' --- Determine minimum price (customer's price level) with $0.00 fallback ---
            Dim minPrice As Decimal ' Only need minimum - no upper limit

            Select Case Me.txtPriceLevel.Text
                Case "Price"
                    minPrice = item.Price

                Case "PriceA"
                    If item.PriceA > 0 Then
                        minPrice = item.PriceA
                    Else
                        minPrice = item.Price
                    End If

                Case "PriceB"
                    If item.PriceB > 0 Then
                        minPrice = item.PriceB
                    ElseIf item.PriceA > 0 Then
                        minPrice = item.PriceA
                    Else
                        minPrice = item.Price
                    End If

                Case "PriceC"
                    If item.PriceC > 0 Then
                        minPrice = item.PriceC
                    ElseIf item.PriceB > 0 Then
                        minPrice = item.PriceB
                    ElseIf item.PriceA > 0 Then
                        minPrice = item.PriceA
                    Else
                        minPrice = item.Price
                    End If

                Case Else
                    minPrice = item.Price
            End Select

            Dim requiresOverride As Boolean = False

            If WoRecallType = 0 Then
                ' --- New order: price must not be below minimum ---
                If currentPrice < minPrice Then
                    requiresOverride = True
                End If

            ElseIf WoRecallType = 1 Then
                ' --- Recall order ---
                Dim orderID As Integer
                Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
                If Not Integer.TryParse(lastPart, orderID) Then
                    'Throw New Exception("Invalid Work Order number: " & lblOrderNo.Text)
                    WoRecallType = 0
                End If

                Dim orderEntry = (From o In db.OrderEntries
                                  Join i In db.Items On i.ID Equals o.ItemID
                                  Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
                                  Select o).FirstOrDefault()

                If orderEntry IsNot Nothing Then
                    Dim woOrderPrice As Decimal = orderEntry.Price

                    ' Valid if: matches WO price OR at/above minimum price
                    Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
                                              (currentPrice >= minPrice)

                    If Not isValidPrice Then
                        requiresOverride = True
                    End If
                Else
                    ' New item in recall: treat as new order
                    If currentPrice < minPrice Then
                        requiresOverride = True
                    End If
                End If
            End If

            If requiresOverride Then
                needsPassword = True
                rowsNeedingOverride.Add(row)
            End If
        Next

        ' Step 2: Ask for password if needed
        If needsPassword AndAlso Not bAllow Then
            frmPassword.sType = "EditPrice"
            MessageBox.Show("This work order requires a password to proceed.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            If frmPassword.ShowDialog() <> DialogResult.OK Then
                Return False
            End If

            For Each row In rowsNeedingOverride
                MarkRowAsPriceOverridden(row)
            Next
        End If

        SaveOrder()
        Return True
    End Function

    'Public Sub ValidatePriceRow(rowIndex As Integer, columnIndex As Integer, type As Integer)
    '    Try
    '        ' --- Step 1: Extract row data ---
    '        Dim row = gridSelectItem.Rows(rowIndex)
    '        Dim currentPrice As Decimal

    '        If Not Decimal.TryParse(row.Cells(columnIndex).Value?.ToString(), currentPrice) Then Exit Sub

    '        Dim itemCode As String = row.Cells(0).Value?.ToString()
    '        If String.IsNullOrWhiteSpace(itemCode) Then Exit Sub

    '        ' --- Step 2: Fetch item details ---
    '        Dim item = (From a In db.Items
    '                    Where a.ItemLookupCode = itemCode
    '                    Select a).FirstOrDefault()
    '        If item Is Nothing Then Exit Sub

    '        ' --- Step 3: Determine selected price level (acts as MIN allowed) ---
    '        Dim selectedPrice As Decimal = item.Price
    '        Select Case txtPriceLevel.Text
    '            Case "Price" : selectedPrice = item.Price
    '            Case "PriceA" : selectedPrice = item.PriceA
    '            Case "PriceB" : selectedPrice = item.PriceB
    '            Case "PriceC" : selectedPrice = item.PriceC
    '        End Select

    '        Dim highlight As Boolean = False

    '        ' --- Step 4: Validation logic (no clamping) ---
    '        If type = 0 Then
    '            ' New order: allow anything >= selected level
    '            highlight = (currentPrice < selectedPrice)

    '        ElseIf type = 1 Then
    '            ' Recall order
    '            Dim orderID As Integer
    '            If Not Integer.TryParse(lblOrderNo.Text.Split(":"c).LastOrDefault()?.Trim(), orderID) Then Exit Sub

    '            Dim orderEntry = (From o In db.OrderEntries
    '                              Join i In db.Items On i.ID Equals o.ItemID
    '                              Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                              Select o).FirstOrDefault()

    '            If orderEntry IsNot Nothing Then
    '                ' Existing item: must match wo price or be valid within selected level
    '                Dim woOrderPrice As Decimal = orderEntry.Price

    '                If currentPrice = woOrderPrice Then
    '                    highlight = False ' exact match → white
    '                ElseIf currentPrice >= selectedPrice Then
    '                    highlight = False ' valid within range → white
    '                Else
    '                    highlight = True  ' otherwise invalid → yellow
    '                End If
    '            Else
    '                ' Newly added item in recall: follow selected level as minimum
    '                highlight = (currentPrice < selectedPrice)
    '            End If
    '        End If

    '        ' --- Step 5: Apply coloring ---
    '        If highlight Then
    '            row.DefaultCellStyle.BackColor = Color.Yellow
    '            DatagridColor = 1
    '        Else
    '            row.DefaultCellStyle.BackColor = Color.White
    '            DatagridColor = 0
    '        End If
    '        row.DefaultCellStyle.ForeColor = Color.Black

    '    Catch ex As Exception
    '        MessageBox.Show("Error validating price: " & ex.Message,
    '                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub
    'Public Sub ValidatePriceRow(rowIndex As Integer, columnIndex As Integer, type As Integer)
    '    Try
    '        ' --- Step 1: Extract row data ---
    '        Dim row = gridSelectItem.Rows(rowIndex)
    '        Dim currentPrice As Decimal
    '        If Not Decimal.TryParse(row.Cells(columnIndex).Value?.ToString(), currentPrice) Then Exit Sub

    '        Dim itemCode As String = row.Cells(0).Value?.ToString()
    '        If String.IsNullOrWhiteSpace(itemCode) Then Exit Sub

    '        ' --- Step 2: Fetch item details ---
    '        Dim item = (From a In db.Items
    '                    Where a.ItemLookupCode = itemCode
    '                    Select a).FirstOrDefault()

    '        If item Is Nothing Then Exit Sub

    '        ' --- Step 3: Determine price range based on customer's price level ---
    '        Dim minPrice As Decimal ' Customer's assigned tier (lower bound)
    '        Dim maxPrice As Decimal = item.Price ' Always base price (upper bound)

    '        Select Case txtPriceLevel.Text
    '            Case "Price"
    '                minPrice = item.Price
    '                maxPrice = item.Price ' Range: exact price only
    '            Case "PriceA"
    '                minPrice = item.PriceA ' Range: PriceA to Price
    '            Case "PriceB"
    '                minPrice = item.PriceB ' Range: PriceB to Price
    '            Case "PriceC"
    '                minPrice = item.PriceC ' Range: PriceC to Price
    '            Case Else
    '                minPrice = item.Price
    '                maxPrice = item.Price
    '        End Select

    '        Dim highlight As Boolean = False

    '        ' --- Step 4: Validation logic ---
    '        If type = 0 Then
    '            ' --- New order: price must be within customer's range ---
    '            highlight = (currentPrice < minPrice OrElse currentPrice > maxPrice)

    '        ElseIf type = 1 Then
    '            ' --- Recall order ---
    '            Dim orderID As Integer
    '            Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
    '            If Not Integer.TryParse(lastPart, orderID) Then Exit Sub

    '            Dim orderEntry = (From o In db.OrderEntries
    '                              Join i In db.Items On i.ID Equals o.ItemID
    '                              Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                              Select o).FirstOrDefault()

    '            If orderEntry IsNot Nothing Then
    '                ' --- Existing item in WO ---
    '                Dim woOrderPrice As Decimal = orderEntry.Price

    '                ' Valid if: matches WO price OR within customer's range
    '                Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
    '                                          (currentPrice >= minPrice AndAlso currentPrice <= maxPrice)

    '                highlight = Not isValidPrice
    '            Else
    '                ' --- New item added to recall: treat as new order ---
    '                highlight = (currentPrice < minPrice OrElse currentPrice > maxPrice)
    '            End If
    '        End If

    '        ' --- Step 5: Apply coloring ---
    '        If highlight Then
    '            row.DefaultCellStyle.BackColor = Color.Yellow
    '            DatagridColor = 1
    '        Else
    '            row.DefaultCellStyle.BackColor = Color.White
    '            DatagridColor = 0
    '        End If

    '        row.DefaultCellStyle.ForeColor = Color.Black

    '    Catch ex As Exception
    '        MessageBox.Show("Error validating price: " & ex.Message,
    '                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub
    'Public Sub ValidatePriceRow(rowIndex As Integer, columnIndex As Integer, type As Integer)
    '    Try
    '        ' --- Step 1: Extract row data ---
    '        Dim row = gridSelectItem.Rows(rowIndex)
    '        Dim currentPrice As Decimal
    '        If Not Decimal.TryParse(row.Cells(columnIndex).Value?.ToString(), currentPrice) Then Exit Sub

    '        Dim itemCode As String = row.Cells(0).Value?.ToString()
    '        If String.IsNullOrWhiteSpace(itemCode) Then Exit Sub

    '        ' --- Step 2: Fetch item details ---
    '        Dim item = (From a In db.Items
    '                    Where a.ItemLookupCode = itemCode
    '                    Select a).FirstOrDefault()

    '        If item Is Nothing Then Exit Sub

    '        ' --- Step 3: Determine price range with $0.00 fallback logic ---
    '        Dim minPrice As Decimal
    '        Dim maxPrice As Decimal = item.Price ' Always base price (upper bound)

    '        Select Case txtPriceLevel.Text
    '            Case "Price"
    '                minPrice = item.Price
    '                maxPrice = item.Price

    '            Case "PriceA"
    '                If item.PriceA > 0 Then
    '                    minPrice = item.PriceA
    '                Else
    '                    ' PriceA not set, fall back to base Price
    '                    minPrice = item.Price
    '                    maxPrice = item.Price
    '                End If

    '            Case "PriceB"
    '                If item.PriceB > 0 Then
    '                    minPrice = item.PriceB
    '                ElseIf item.PriceA > 0 Then
    '                    ' PriceB not set, fall back to PriceA
    '                    minPrice = item.PriceA
    '                Else
    '                    ' Neither PriceB nor PriceA set, fall back to base Price
    '                    minPrice = item.Price
    '                    maxPrice = item.Price
    '                End If

    '            Case "PriceC"
    '                If item.PriceC > 0 Then
    '                    minPrice = item.PriceC
    '                ElseIf item.PriceB > 0 Then
    '                    ' PriceC not set, fall back to PriceB
    '                    minPrice = item.PriceB
    '                ElseIf item.PriceA > 0 Then
    '                    ' PriceC and PriceB not set, fall back to PriceA
    '                    minPrice = item.PriceA
    '                Else
    '                    ' No discount tiers set, fall back to base Price
    '                    minPrice = item.Price
    '                    maxPrice = item.Price
    '                End If

    '            Case Else
    '                minPrice = item.Price
    '                maxPrice = item.Price
    '        End Select

    '        Dim highlight As Boolean = False

    '        ' --- Step 4: Validation logic ---
    '        If type = 0 Then
    '            ' --- New order: price must be within customer's range ---
    '            highlight = (currentPrice < minPrice OrElse currentPrice > maxPrice)

    '        ElseIf type = 1 Then
    '            ' --- Recall order ---
    '            Dim orderID As Integer
    '            Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
    '            If Not Integer.TryParse(lastPart, orderID) Then Exit Sub

    '            Dim orderEntry = (From o In db.OrderEntries
    '                              Join i In db.Items On i.ID Equals o.ItemID
    '                              Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
    '                              Select o).FirstOrDefault()

    '            If orderEntry IsNot Nothing Then
    '                ' --- Existing item in WO ---
    '                Dim woOrderPrice As Decimal = orderEntry.Price

    '                ' Valid if: matches WO price OR within customer's range
    '                Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
    '                                          (currentPrice >= minPrice AndAlso currentPrice <= maxPrice)

    '                highlight = Not isValidPrice
    '            Else
    '                ' --- New item added to recall: treat as new order ---
    '                highlight = (currentPrice < minPrice OrElse currentPrice > maxPrice)
    '            End If
    '        End If

    '        ' --- Step 5: Apply coloring ---
    '        If highlight Then
    '            row.DefaultCellStyle.BackColor = Color.Yellow
    '            DatagridColor = 1
    '        Else
    '            row.DefaultCellStyle.BackColor = Color.White
    '            DatagridColor = 0
    '        End If

    '        row.DefaultCellStyle.ForeColor = Color.Black

    '    Catch ex As Exception
    '        MessageBox.Show("Error validating price: " & ex.Message,
    '                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub
    Public Sub ValidatePriceRow(rowIndex As Integer, columnIndex As Integer, type As Integer)
        Try
            ' --- Step 1: Extract row data ---
            Dim row = gridSelectItem.Rows(rowIndex)
            Dim currentPrice As Decimal
            If Not Decimal.TryParse(row.Cells(columnIndex).Value?.ToString(), currentPrice) Then Exit Sub

            Dim itemCode As String = row.Cells(0).Value?.ToString()
            If String.IsNullOrWhiteSpace(itemCode) Then Exit Sub

            ' --- Step 2: Fetch item details ---
            Dim item = (From a In db.Items
                        Where a.ItemLookupCode = itemCode
                        Select a).FirstOrDefault()

            If item Is Nothing Then Exit Sub

            ' --- Step 3: Determine minimum price (customer's price level) with $0.00 fallback ---
            Dim minPrice As Decimal ' Only need minimum - no upper limit

            Select Case txtPriceLevel.Text
                Case "Price"
                    minPrice = item.Price

                Case "PriceA"
                    If item.PriceA > 0 Then
                        minPrice = item.PriceA
                    Else
                        ' PriceA not set, fall back to base Price
                        minPrice = item.Price
                    End If

                Case "PriceB"
                    If item.PriceB > 0 Then
                        minPrice = item.PriceB
                    ElseIf item.PriceA > 0 Then
                        ' PriceB not set, fall back to PriceA
                        minPrice = item.PriceA
                    Else
                        ' Neither PriceB nor PriceA set, fall back to base Price
                        minPrice = item.Price
                    End If

                Case "PriceC"
                    If item.PriceC > 0 Then
                        minPrice = item.PriceC
                    ElseIf item.PriceB > 0 Then
                        ' PriceC not set, fall back to PriceB
                        minPrice = item.PriceB
                    ElseIf item.PriceA > 0 Then
                        ' PriceC and PriceB not set, fall back to PriceA
                        minPrice = item.PriceA
                    Else
                        ' No discount tiers set, fall back to base Price
                        minPrice = item.Price
                    End If

                Case Else
                    minPrice = item.Price
            End Select

            Dim highlight As Boolean = False

            ' --- Step 4: Validation logic (only check lower bound) ---
            If type = 0 Then
                ' --- New order: price must not be below minimum ---
                highlight = (currentPrice < minPrice)

            ElseIf type = 1 Then
                ' --- Recall order ---
                Dim orderID As Integer
                Dim lastPart = lblOrderNo.Text.Split(" "c).Last().Trim()
                If Not Integer.TryParse(lastPart, orderID) Then Exit Sub

                Dim orderEntry = (From o In db.OrderEntries
                                  Join i In db.Items On i.ID Equals o.ItemID
                                  Where i.ItemLookupCode = itemCode AndAlso o.OrderID = orderID
                                  Select o).FirstOrDefault()

                If orderEntry IsNot Nothing Then
                    ' --- Existing item in WO ---
                    Dim woOrderPrice As Decimal = orderEntry.Price

                    ' Valid if: matches WO price OR at/above minimum price
                    Dim isValidPrice As Boolean = (currentPrice = woOrderPrice) OrElse
                                              (currentPrice >= minPrice)

                    highlight = Not isValidPrice
                Else
                    ' --- New item added to recall: treat as new order ---
                    highlight = (currentPrice < minPrice)
                End If
            End If

            ' --- Step 5: Apply coloring ---
            If highlight Then
                row.DefaultCellStyle.BackColor = Color.Yellow
                DatagridColor = 1
            Else
                row.DefaultCellStyle.BackColor = Color.White
                DatagridColor = 0
            End If

            row.DefaultCellStyle.ForeColor = Color.Black

        Catch ex As Exception
            MessageBox.Show("Error validating price: " & ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Shared Function ConfPass(ByVal spass As String) As Integer

        Try

            Dim conf = (From c In db.SOD_WO_Confs Where c.Password.Equals(spass) Or c.Password2.Equals(spass) Or c.Password3.Equals(spass)).Count

            Return conf
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    'Public Sub InsertPriceLogs(orderId As Integer, itemId As Integer, mFullPrice As String, mUpdatedPrice As String, inputPass As String, sStatus As String)
    '    Try
    '        Dim approver As String = ""
    '        'Dim approver As String = "Unknown"
    '        Dim dPrice As Double
    '        Dim dPricea As Double
    '        Dim dPriceb As Double
    '        Dim dPricec As Double
    '        Dim sLastUpdate As DateTime

    '        ' Get config row (assuming only 1 row for settings)
    '        Dim cfg = (From c In db.SOD_WO_Confs
    '                   Select c).FirstOrDefault()

    '        If cfg IsNot Nothing Then
    '            If cfg.Password = inputPass Then
    '                approver = "Administrator" ' Hardcoded unless you add Pass1Username column
    '            ElseIf cfg.Password2 = inputPass Then
    '                approver = cfg.Pass2Username
    '            ElseIf cfg.Password3 = inputPass Then
    '                approver = cfg.Pass3Username
    '            End If
    '        End If

    '        Dim iItemIDs = (From a In db.Items Where a.ID.Equals(itemId) Select a).ToList

    '        For Each n In iItemIDs

    '            dPrice = Double.Parse(n.Price)
    '            dPricea = Double.Parse(n.PriceA)
    '            dPriceb = Double.Parse(n.PriceB)
    '            dPricec = Double.Parse(n.PriceC)
    '            sLastUpdate = (n.LastUpdated)
    '        Next

    '        ' Create new log object
    '        Dim newLog As New SOD_WO_LOG With {
    '        .OrderId = orderId,
    '        .ItemID = itemId,
    '        .FullPrice = mFullPrice,
    '        .UpdatedPrice = mUpdatedPrice,
    '        .ApprovedUser = approver,  ' <- resolved username
    '        .Status = sStatus,
    '        .Date = DateTime.Now,
    '        .Price = dPrice,
    '        .Pricea = dPricea,
    '        .Priceb = dPriceb,
    '        .Pricec = dPricec,
    '        .LastUpdatedPrice = sLastUpdate,
    '        .OrderTaker = usrFullname.ToUpper
    '    }

    '        ' Add to context
    '        db.SOD_WO_LOGs.InsertOnSubmit(newLog)

    '        ' Save changes
    '        db.SubmitChanges()

    '    Catch ex As Exception
    '        MessageBox.Show("Error inserting log: " & ex.Message,
    '                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '    End Try
    'End Sub

    Public Sub InsertPriceLogs(orderId As Integer, itemId As Integer, mFullPrice As String, mUpdatedPrice As String, inputPass As String, sStatus As String)
        Try

            ' Always use fresh context for logs to avoid conflicts
            Dim dbLog As New ItemLookUpDataContext(DB_Conn("constr"))

            'Dim approver As String = "Unknown"
            Dim approver As String = ""
            Dim cfg = (From c In dbLog.SOD_WO_Confs Select c).FirstOrDefault()
            Dim iCusPrice As Integer = clsCustomer.getPriceLevel(txtCustomer.Text)

            If cfg IsNot Nothing Then
                If cfg.Password = inputPass Then
                    approver = "Administrator"
                ElseIf cfg.Password2 = inputPass Then
                    approver = cfg.Pass2Username
                ElseIf cfg.Password3 = inputPass Then
                    approver = cfg.Pass3Username
                End If
            End If

            ' Fetch item details safely
            Dim item = (From a In dbLog.Items Where a.ID = itemId Select a).FirstOrDefault()

            Dim dPrice As Double = 0
            Dim dPricea As Double = 0
            Dim dPriceb As Double = 0
            Dim dPricec As Double = 0
            Dim sLastUpdate As DateTime = DateTime.Now

            If item IsNot Nothing Then
                Double.TryParse(item.Price.ToString(), dPrice)
                Double.TryParse(item.PriceA.ToString(), dPricea)
                Double.TryParse(item.PriceB.ToString(), dPriceb)
                Double.TryParse(item.PriceC.ToString(), dPricec)
                sLastUpdate = If(item.LastUpdated <> Nothing, item.LastUpdated, DateTime.Now)
            End If

            ' Create and insert log
            Dim newLog As New SOD_WO_LOG With {
            .OrderId = orderId,
            .ItemID = itemId,
            .FullPrice = mFullPrice,
            .UpdatedPrice = mUpdatedPrice,
            .ApprovedUser = approver,
            .Status = sStatus,
            .Date = DateTime.Now,
            .Price = dPrice,
            .Pricea = dPricea,
            .Priceb = dPriceb,
            .Pricec = dPricec,
            .LastUpdatedPrice = sLastUpdate,
            .OrderTaker = usrFullname.ToUpper,
            .CustomerPriceLevel = If(iCusPrice, iCusPrice, 0)
        }

            dbLog.SOD_WO_LOGs.InsertOnSubmit(newLog)
            dbLog.SubmitChanges()

        Catch ex As Exception
            MessageBox.Show("Error inserting log: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Public Function IsPriceBelowAllowed(itemCode As Integer, currentPrice As Decimal) As Boolean
        Dim allowedPrice As Decimal? = Nothing

        Dim item = (From a In db.Items
                    Where a.ID.Equals(itemCode)
                    Select a).FirstOrDefault()
        Dim lowerPrice As Decimal = item.Price
        Dim upperPrice As Decimal = item.PriceC

        If item IsNot Nothing Then
            Select Case Me.txtPriceLevel.Text
                Case "Price" : allowedPrice = item.Price
                Case "PriceA" : allowedPrice = item.PriceA
                Case "PriceB" : allowedPrice = item.PriceB
                Case "PriceC" : allowedPrice = item.PriceC
            End Select
        End If

        If allowedPrice.HasValue AndAlso currentPrice < allowedPrice.Value Then
            Return True
        End If

        Return False

    End Function

    Public Function ifItemExistInOrder(itemID As Integer, orderID As Integer) As Boolean
        Try
            ' Check if the item exists in the order
            Dim exists As Boolean = (From o In db.OrderEntries
                                     Where o.OrderID = orderID AndAlso o.ItemID = itemID
                                     Select o).Any()
            Return exists

        Catch ex As Exception
            MessageBox.Show("Error in ifItemExistInOrder: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub txtSearch_Leave(sender As Object, e As EventArgs) Handles txtSearch.Leave
        pnl2TextSearch.BackColor = Panel5.BackColor
    End Sub

    Private Sub txtSearch_Enter(sender As Object, e As EventArgs) Handles txtSearch.Enter
        pnl2TextSearch.BackColor = Color.Salmon
    End Sub

    Private Sub txtBarcode_Leave(sender As Object, e As EventArgs) Handles txtBarcode.Leave
        pnl1Barcode.BackColor = Panel5.BackColor
    End Sub

    Private Sub txtBarcode_Enter(sender As Object, e As EventArgs) Handles txtBarcode.Enter
        pnl1Barcode.BackColor = Color.Salmon
    End Sub

    Private Sub txtSearch_EnabledChanged(sender As Object, e As EventArgs) Handles txtSearch.EnabledChanged

        If txtSearch.Enabled Then
            pnl2TextSearch.BackColor = Color.Salmon
        Else
            pnl2TextSearch.BackColor = Panel5.BackColor
        End If

    End Sub

End Class