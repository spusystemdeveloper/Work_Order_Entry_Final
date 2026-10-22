Public Class frmForInvoice

    Dim groupid As String
    Dim orders As String

    Private Sub frmForInvoice_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click

        Me.Dispose()
        Me.Close()

    End Sub

    Private Sub btnGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGroup.Click

        If Not groupid = 0 Then


            Dim SearchStrArr() As String = Split(orders, ", ")

            For Each x In SearchStrArr

                clsItemLookUp.fakeUpdate(x)
                frmPrintWo.wo = x
                frmPrintWo.ShowDialog()

            Next

            clsItemLookUp.UpdateQueueStatus(groupid, "For Invoicing")

            gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text, frmItemLookUp.usrUsername)
            groupid = 0


        Else
            MessageBox.Show("Please select an Order", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
      

    End Sub


    Private Sub gridCustOrder_DataBindingComplete(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewBindingCompleteEventArgs) Handles gridCustOrder.DataBindingComplete
        Try
            Me.gridCustOrder.Rows(0).Selected = False
            groupid = 0
        Catch ex As Exception

        End Try

    End Sub

    Private Sub gridCustOrder_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustOrder.CellClick
        Try
            Dim index As Integer

            index = e.RowIndex

            Dim selectedRow As DataGridViewRow
            selectedRow = gridCustOrder.Rows(index)

            groupid = selectedRow.Cells(0).Value.ToString()
            orders = selectedRow.Cells(2).Value.ToString()

        Catch ex As Exception
            groupid = 0
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        'Dim woID As String = MyInputBox("Enter Work Order Number to Print")

        'If woID = String.Empty Then
        '    Exit Sub
        'End If

        'frmPrintWo.wo = Convert.ToInt32(woID)
        'frmPrintWo.ShowDialog()

        ''Dim res = clsItemLookUp.printManualWO(frmItemLookUp.usrUsername, woID)

        ''If res = True Then

        ''    frmPrintWo.wo = Convert.ToInt32(woID)
        ''    frmPrintWo.ShowDialog()

        ''Else
        ''    MessageBox.Show("Cannot Print the Work Order !" & vbCrLf & vbCrLf & "REASONS :" & vbCrLf & vbCrLf & "1. This Work Order was not made by you." & vbCrLf & "2. The Order is not yet For Invoicing.", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        ''End If

    End Sub

    Private Function MyInputBox(ByVal Prompt As String) As String

        Dim frmInput As New Form
        frmInput.Owner = Me
        frmInput.StartPosition = FormStartPosition.CenterScreen
        frmInput.ShowIcon = False
        frmInput.Size = New Size(310, 120)
        frmInput.MinimumSize = New Size(315, 120)
        Dim btn As New Button()
        btn.Text = "Print"
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


End Class