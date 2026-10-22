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

    Private Sub gridLevel_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridLevel.CellContentClick

    End Sub

    Private Sub gridLevel_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles gridLevel.KeyPress

        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then

            Me.Dispose()

        End If

    End Sub

End Class