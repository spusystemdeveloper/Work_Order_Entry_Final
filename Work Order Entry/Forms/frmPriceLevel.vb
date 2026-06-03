Public Class frmPriceLevel

    Dim record As Object

    Private Sub frmPriceLevel_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        bPass = False
    End Sub

    Private Sub frmPriceLevel_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.Escape Then
            bPass = False
            Me.Close()
        ElseIf e.KeyCode = Keys.F9 Then
            frmPassword.sType = "Price"
            frmPassword.ShowDialog()
        End If
    End Sub


    Private Sub frmPriceLevel_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        LoadLevels()
        'addCost()

    End Sub

    Public Sub addCost()

        Dim dCostPrice As Double

        'Dim globalCust = (From c In db.Customers Where c.ID.Equals(iCusID) Select c.GlobalCustomer).SingleOrDefault
        Dim ccType = (From c In db.Customers Where c.ID.Equals(iCusID) Select c.CustomText4).SingleOrDefault.ToString
        Dim queryType = (From a In db.SOD_WO_Confs
                    Select a.CustType).SingleOrDefault

        dCostPrice = (From i In db.Items Where i.ItemLookupCode.Equals(txtItemCode.Text) Select i.Cost).SingleOrDefault

        If queryType.Contains(ccType) And ccType <> String.Empty Then
            gridLevel.Rows.Add("COST", dCostPrice)
        End If

        'MessageBox.Show("Global : " & globalCust & vbCrLf & "Cost : " & dCostPrice)
    End Sub


    Public Sub LoadLevels()

        gridLevel.Rows.Clear()
        txtItemCode.Text = sItemCode
        txtItemName.Text = sItemName

        'clsItemLookUp.LoadLevel(iPrice, lblItemCode.Text, 0)

        'If iPrice = 0 Then
        '    clsItemLookUp.LoadLevel(1, lblItemCode.Text, 0)
        'Else
        '    clsItemLookUp.LoadLevel(iPrice - 1, lblItemCode.Text, 0)
        'End If

        bLevelFlag = False

        For x = 0 To iPrice
            clsItemLookUp.LoadLevel(x, txtItemCode.Text, 0)
        Next x

        gridLevel.CurrentCell = gridLevel.Rows(0).Cells(0)
        gridLevel.Columns(1).DefaultCellStyle.Format = "C"

    End Sub

    Public Sub DisplayPrice(ByVal sPriceType As String, ByVal dPrice As Double)

        Dim previousAllowUserToAddRows = gridLevel.AllowUserToAddRows
        gridLevel.AllowUserToAddRows = True

        Dim newTimeRecord As DataGridViewRow = gridLevel.Rows(gridLevel.NewRowIndex).Clone

        With record
            newTimeRecord.Cells(ItemCode.Index).Value = sPriceType
            newTimeRecord.Cells(Price.Index).Value = dPrice
        End With

        'If bPass = True Then
        'gridLevel.Rows.Add(newTimeRecord)
        'gridLevel.AllowUserToAddRows = previousAllowUserToAddRows
        'gridLevel.CurrentCell = gridLevel(1, gridLevel.RowCount - 1)
        'End If

        gridLevel.Rows.Add(newTimeRecord)
        gridLevel.AllowUserToAddRows = previousAllowUserToAddRows
        gridLevel.CurrentCell = gridLevel(1, gridLevel.RowCount - 1)

    End Sub

    Private Sub CallIndex()

        If gridLevel.RowCount = 0 Then
            Exit Sub
        End If

        iRowLevel = clsItemLookUp.RowIndex(gridLevel)

    End Sub

    Private Sub gridLevel_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridLevel.CellDoubleClick
        If e.RowIndex <> -1 Then
            GetItem()
        End If
    End Sub

    Private Sub gridLevel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridLevel.Click

        CallIndex()

    End Sub


    Private Sub insertItem()
        Dim selectedPriceLevel As String = String.Empty
        Dim selectedPrice As Double

        If gridLevel.CurrentRow IsNot Nothing AndAlso gridLevel.CurrentRow.Cells(0).Value IsNot Nothing Then
            selectedPriceLevel = gridLevel.CurrentRow.Cells(0).Value.ToString().Trim()
        End If

        If gridLevel.CurrentRow Is Nothing OrElse Not Double.TryParse(gridLevel.Item(1, gridLevel.CurrentRow.Index).Value.ToString(), selectedPrice) Then
            Exit Sub
        End If

        If bInsert = True Then
            frmItemLookUp.txtPriceLevel.Text = selectedPriceLevel
            Me.DialogResult = Windows.Forms.DialogResult.OK

            frmItemLookUp.InsertSelectedItem(frmItemLookUp.gridItem.CurrentRow.Index)
            frmItemLookUp.lblStatItemSelected.Text = frmItemLookUp.gridSelectItem.RowCount & " item" & IIf(frmItemLookUp.gridSelectItem.RowCount > 1, "s", "") & " selected"
        Else
            frmItemLookUp.ApplySelectedPriceLevel(selectedPriceLevel, selectedPrice)
        End If

        If bInsert = True Then
            frmItemLookUp.UpdateAmt()
        End If

    End Sub

    'Private Sub insertItem()

    '    If bInsert = True Then
    '        Me.DialogResult = Windows.Forms.DialogResult.OK
    '        frmItemLookUp.InsertSelectedItem(frmItemLookUp.gridItem.CurrentRow.Index)
    '        frmItemLookUp.lblStatItemSelected.Text = frmItemLookUp.gridSelectItem.RowCount & " item" & IIf(frmItemLookUp.gridSelectItem.RowCount > 1, "s", "") & " selected"
    '    Else
    '        frmItemLookUp.gridSelectItem.Item(3, frmItemLookUp.gridSelectItem.CurrentRow.Index).Value = gridLevel.Item(1, gridLevel.CurrentRow.Index).Value
    '        frmItemLookUp.gridSelectItem.Item(5, frmItemLookUp.gridSelectItem.CurrentRow.Index).Value = frmItemLookUp.gridSelectItem.Item(2, frmItemLookUp.gridSelectItem.CurrentRow.Index).Value * gridLevel.Item(1, gridLevel.CurrentRow.Index).Value
    '    End If

    '    frmItemLookUp.UpdateAmt()

    'End Sub


    Private Sub GetItem()

        Dim iRowIndex = gridLevel.SelectedCells.Item(0).RowIndex.ToString()

        Dim level = 0

        Dim mask = (From c In db.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault
        Dim globalCust = (From c In db.Customers Where c.ID.Equals(iCusID) Select c.GlobalCustomer).SingleOrDefault

        If mask = "PRICE (RETAIL)" Then
            level = 0
        ElseIf mask = "PRICE A (WHOLESALE)" Then
            level = 1
        ElseIf mask = "PRICE B (D1)" Then
            level = 2
        ElseIf mask = "PRICE C (D2)" Then
            level = 3
        ElseIf mask = "NONE" Then
            level = 4
        End If

        If iRowIndex < level Then
            insertItem()
        Else

            If globalCust = True Then
                insertItem()
            Else

                'frmPassword.sType = "EditPrice"
                'Dim pDialog As DialogResult = frmPassword.ShowDialog
                'If pDialog = Windows.Forms.DialogResult.Cancel Then
                '    Exit Sub
                'Else
                '    insertItem()
                'End If
                insertItem()

            End If


        End If

        Me.Dispose()

    End Sub

    'comment
    'Private Sub GetItem()
    '    Dim irowindex = gridLevel.SelectedCells.Item(0).RowIndex.ToString
    '    Dim level = 0

    '    'MsgBox(irowindex & " " & iPricePass & " " & dLowest)
    '    If irowindex > iPricePass Then
    '        frmPassword.sType = "EditPrice"
    '        Dim pDialog As DialogResult = frmPassword.ShowDialog
    '        If pDialog = Windows.Forms.DialogResult.Cancel Then
    '            Exit Sub
    '        Else
    '            insertItem()
    '        End If
    '    Else
    '        insertItem()
    '    End If

    '    Me.Dispose()

    'End Sub


    'comment
    'Private Sub GetItem()

    '    Dim iRowIndex = gridLevel.SelectedCells.Item(0).RowIndex.ToString()

    '    Dim level = 0

    '    Dim mask = (From c In db.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault
    '    Dim globalCust = (From c In db.Customers Where c.ID.Equals(iCusID) Select c.GlobalCustomer).SingleOrDefault

    '    If mask = "PRICE (RETAIL)" Then
    '        level = 0
    '    ElseIf mask = "PRICE A (WHOLESALE)" Then
    '        level = 1
    '    ElseIf mask = "PRICE B (D1)" Then
    '        level = 2
    '    ElseIf mask = "PRICE C (D2)" Then
    '        level = 3
    '    ElseIf mask = "NONE" Then
    '        level = 4
    '    End If

    '    If iRowIndex < level Then
    '        insertItem()
    '    Else

    '        If custType = "BRANCH" Then
    '            insertItem()
    '        Else

    '            frmPassword.sType = "EditPrice"
    '            Dim pDialog As DialogResult = frmPassword.ShowDialog
    '            If pDialog = Windows.Forms.DialogResult.Cancel Then
    '                Exit Sub
    '            Else
    '                insertItem()
    '            End If

    '        End If


    '    End If

    '    Me.Dispose()

    'End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click

        GetItem()

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        frmItemLookUp.txtBarcode.Text = ""
        bPass = False
        Me.Dispose()

    End Sub
    Private Sub gridLevel_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridLevel.KeyDown

        If e.KeyCode = Keys.Enter Then
            GetItem()
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            Me.Dispose()
        End If
    End Sub
    Private Sub gridLevel_CellClick(sender As System.Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridLevel.CellClick
        ' CallIndex()
        insertItem()
        If e.RowIndex <> -1 Then
            ' Assuming the price is in the second column (index 1)
            Dim selectedPrice As Double = Convert.ToDouble(gridLevel.Rows(e.RowIndex).Cells(1).Value)

            ' Call a method in frmItemLookUp to update the price
            ' frmItemLookUp.UpdatePriceFromPriceLevel(selectedPrice)
        End If
    End Sub
End Class
