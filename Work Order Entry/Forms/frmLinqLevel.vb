Public Class frmLinqLevel
    Dim record As Object
    Private Sub frmLinqLevel_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        gridLevel.Rows.Clear()
        lblItemCode.Text = sItemCode
        txtItemName.Text = sItemName

        bLevelFlag = False
        Dim maxLevel As Integer = Math.Max(iPrice, GetCustomerPriceLevelIndex())
        For x = 0 To maxLevel
            clsItemLookUp.LoadLevel(x, lblItemCode.Text, 1)
        Next x

        SelectCustomerPriceLevel()

    End Sub

    Public Sub DisplayPrice(ByVal sPriceType As String, ByVal dPrice As Double)

        Dim previousAllowUserToAddRows = gridLevel.AllowUserToAddRows
        gridLevel.AllowUserToAddRows = True

        Dim newTimeRecord As DataGridViewRow = gridLevel.Rows(gridLevel.NewRowIndex).Clone

        With record
            newTimeRecord.Cells(ItemCode.Index).Value = sPriceType
            newTimeRecord.Cells(Price.Index).Value = dPrice
        End With

        gridLevel.Rows.Add(newTimeRecord)
        gridLevel.AllowUserToAddRows = previousAllowUserToAddRows
        gridLevel.CurrentCell = gridLevel(1, gridLevel.RowCount - 1)

    End Sub

   
    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click

        Me.Dispose()

    End Sub
    'comment
    'Private Sub gridLevel_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridLevel.CellContentClick

    'End Sub

    'Private Sub gridLevel_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles gridLevel.KeyPress

    '    If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then

    '        Me.Dispose()

    '    End If

    'End Sub
    'Private Sub gridLevel_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridLevel.CellContentClick
    '    If e.RowIndex <> -1 Then
    '        ' Get the selected price from gridLevel
    '        Dim selectedPrice As Double = Convert.ToDouble(gridLevel.Rows(e.RowIndex).Cells(1).Value)

    '        ' Call a method in frmItemLookUp to update the price
    '        frmItemLookUp.UpdatePriceFromPriceLevel(selectedPrice)

    '        ' Update only the price in gridSelectItem without modifying the quantity
    '        UpdateSelectedItemPrice(selectedPrice)
    '    End If
    'End Sub

    Private Sub UpdateSelectedItemPrice(ByVal selectedPrice As Double)
        Dim selectedPriceLevel As String = String.Empty

        If gridLevel.CurrentRow IsNot Nothing AndAlso gridLevel.CurrentRow.Cells(0).Value IsNot Nothing Then
            selectedPriceLevel = gridLevel.CurrentRow.Cells(0).Value.ToString().Trim()
        End If

        frmItemLookUp.ApplySelectedPriceLevel(selectedPriceLevel, selectedPrice)
        Me.Dispose()
    End Sub


    ' --------------------Additional code para e load niya ang Data ---------------------------------
    Public Sub LoadLevels()

        gridLevel.Rows.Clear()
        lblItemCode.Text = sItemCode
        txtItemName.Text = sItemName

        bLevelFlag = False
        Dim maxLevel As Integer = Math.Max(iPrice, GetCustomerPriceLevelIndex())
        For x = 0 To maxLevel
            clsItemLookUp.LoadLevel(x, lblItemCode.Text, 1)
        Next x

        SelectCustomerPriceLevel()
        gridLevel.Columns(1).DefaultCellStyle.Format = "C"

    End Sub

    Private Function GetCustomerPriceLevelIndex() As Integer
        Select Case frmItemLookUp.txtPriceLevel.Text.Trim().ToUpperInvariant()
            Case "PRICEA", "PRICE A", "PRICE A (WHOLESALE)"
                Return 1
            Case "PRICEB", "PRICE B", "PRICE B (D1)"
                Return 2
            Case "PRICEC", "PRICE C", "PRICE C (D2)"
                Return 3
            Case Else
                Return 0
        End Select
    End Function

    Private Sub SelectCustomerPriceLevel()
        If gridLevel.RowCount = 0 Then Exit Sub

        Dim levelText As String = frmItemLookUp.txtPriceLevel.Text.Trim().ToUpperInvariant()
        Dim expectedLabel As String = "PRICE (RETAIL)"

        Select Case levelText
            Case "PRICE", "PRICE (RETAIL)"
                expectedLabel = "PRICE (RETAIL)"
            Case "PRICEA", "PRICE A", "PRICE A (WHOLESALE)"
                expectedLabel = "PRICE A (WHOLESALE)"
            Case "PRICEB", "PRICE B", "PRICE B (D1)"
                expectedLabel = "PRICE B (D1)"
            Case "PRICEC", "PRICE C", "PRICE C (D2)"
                expectedLabel = "PRICE C (D2)"
        End Select

        For Each row As DataGridViewRow In gridLevel.Rows
            If row.IsNewRow OrElse row.Cells(0).Value Is Nothing Then Continue For
            If row.Cells(0).Value.ToString().Trim().ToUpperInvariant() = expectedLabel Then
                gridLevel.CurrentCell = row.Cells(0)
                row.Selected = True
                Exit Sub
            End If
        Next

        gridLevel.CurrentCell = gridLevel.Rows(0).Cells(0)
    End Sub

    Private Sub gridLevel_KeyPress(sender As Object, e As KeyPressEventArgs) Handles gridLevel.KeyPress
        ' Check if any key was pressed (optional: check for specific keys like Enter or Arrow keys)
        If gridLevel.CurrentRow IsNot Nothing Then
            ' Get the selected price from gridLevel's current row
            Dim selectedPrice As Double = Convert.ToDouble(gridLevel.CurrentRow.Cells(1).Value)

            UpdateSelectedItemPrice(selectedPrice)
        End If
    End Sub


    'Private Sub gridLevel_Click(sender As System.Object, e As System.EventArgs) Handles gridLevel.Click
    '    ' Check if any key was pressed (optional: check for specific keys like Enter or Arrow keys)
    '    If gridLevel.CurrentRow IsNot Nothing Then
    '        ' Get the selected price from gridLevel's current row
    '        Dim selectedPrice As Double = Convert.ToDouble(gridLevel.CurrentRow.Cells(1).Value)

    '        ' Call a method in frmItemLookUp to update the price
    '        frmItemLookUp.UpdatePriceFromPriceLevel(selectedPrice)

    '        ' Update only the price in gridSelectItem without modifying the quantity
    '        UpdateSelectedItemPrice(selectedPrice)
    '    End If
    'End Sub

    'Private Sub gridLevel_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles gridLevel.KeyPress
    '    ' Check if any key was pressed (optional: check for specific keys like Enter or Arrow keys)
    '    If gridLevel.CurrentRow IsNot Nothing Then
    '        ' Get the selected price from gridLevel's current row
    '        Dim selectedPrice As Double = Convert.ToDouble(gridLevel.CurrentRow.Cells(1).Value)

    '        ' Call a method in frmItemLookUp to update the price
    '        frmItemLookUp.UpdatePriceFromPriceLevel(selectedPrice)

    '        ' Update only the price in gridSelectItem without modifying the quantity
    '        UpdateSelectedItemPrice(selectedPrice)
    '    End If
    'End Sub
End Class
