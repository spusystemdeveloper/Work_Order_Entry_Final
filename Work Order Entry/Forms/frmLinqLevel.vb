Public Class frmLinqLevel
    Dim record As Object
    Private Sub frmLinqLevel_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        gridLevel.Rows.Clear()
        lblItemCode.Text = sItemCode
        txtItemName.Text = sItemName

        bLevelFlag = False
        For x = 0 To iPrice
            clsItemLookUp.LoadLevel(x, lblItemCode.Text, 1)
        Next x

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
        For x = 0 To iPrice
            clsItemLookUp.LoadLevel(x, lblItemCode.Text, 1)
        Next x

        gridLevel.CurrentCell = gridLevel.Rows(0).Cells(0)
        gridLevel.Columns(1).DefaultCellStyle.Format = "C"

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
