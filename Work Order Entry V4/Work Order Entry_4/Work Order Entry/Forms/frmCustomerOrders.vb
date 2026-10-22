Public Class frmCustomerOrders

    Public Shared custIDWo As Integer
    Public Shared custWoID As Integer

    Private Sub frmCustomerOrders_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try

            gridCustOrder.DataSource = clsCustomer.loadCustomerOpenWo(custIDWo, custWoID)
            lblWO.Text = custWoID

            With gridCustOrder
                .Columns(0).Width = 50
                .Columns(1).Width = 100
                .Columns(0).HeaderText = "Select"
                .Columns(1).HeaderText = "Group"
                .Columns(2).HeaderText = "Work Orders"
            End With

        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomerOrders Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try


    End Sub


    Private Sub btnGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGroup.Click

        Try

            Dim list As New List(Of String)

            Dim count = gridCustOrder.Rows.Count

            Dim checked = 0

            For Each selected As DataGridViewRow In gridCustOrder.Rows
                If selected.Cells(0).Value = True Then
                    checked = checked + 1
                End If
            Next

            '   Dim groupid = gridCustOrder.Rows(0).Cells(1).Value

            If checked <= 0 Then
                MessageBox.Show("Nothing is selected !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            Else
                If count > 1 Then

                    For Each row As DataGridViewRow In gridCustOrder.Rows
                        If row.Cells(0).Value = True Then
                            list.Add(row.Cells(1).Value)
                        End If
                    Next

                    Dim groupid = list.Min()

                    For Each row As DataGridViewRow In gridCustOrder.Rows
                        If row.Cells(0).Value = True Then
                            If Not row.Cells(2).Value.contains("/") Then
                                clsCustomer.updateQueuing(row.Cells(2).Value, groupid)
                            Else
                                Dim str() As String = Split(row.Cells(2).Value, " / ")
                                For Each x In str
                                    clsCustomer.updateQueuing(x, groupid)
                                Next
                            End If
                        End If
                    Next

                    clsCustomer.updateQueuing(custWoID, groupid)

                ElseIf count = 1 Then
                    If gridCustOrder.SelectedRows(0).Cells(0).Value = True Then
                        clsCustomer.updateQueuing(custWoID, gridCustOrder.SelectedRows(0).Cells(1).Value)
                    End If

                End If

                If clsCustomer.updateQueuingError = False Then
                    MessageBox.Show("Successfully Grouped The Orders!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Me.Close()
                Else
                    MessageBox.Show("Error !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomerOrders Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click


        Me.Dispose()
        Me.Close()

    End Sub

    Private Sub gridCustOrder_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustOrder.CellClick
        Try
            If e.ColumnIndex = 0 AndAlso e.RowIndex >= 0 Then

                If gridCustOrder.Rows(e.RowIndex).Cells(0).Value = True Then
                    gridCustOrder.Rows(e.RowIndex).Cells(0).Value = False
                Else
                    gridCustOrder.Rows(e.RowIndex).Cells(0).Value = True
                End If

            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomerOrders Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function MyInputBox(ByVal Prompt As String) As String



        Dim frmInput As New Form
        frmInput.Owner = Me
        frmInput.StartPosition = FormStartPosition.CenterScreen
        frmInput.ShowIcon = False
        frmInput.Size = New Size(310, 120)
        frmInput.MinimumSize = New Size(315, 120)
        Dim btn As New Button()
        btn.Text = "Proceed"
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

   
    Private Sub gridCustOrder_CellDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustOrder.CellDoubleClick
        Dim index As Integer
        Dim orders As String = ""
        Try


            index = e.RowIndex

            Dim selectedRow As DataGridViewRow
            selectedRow = gridCustOrder.Rows(index)

            orders = selectedRow.Cells(2).Value.ToString()

        Catch ex As Exception

        End Try

        Dim orderid = MyInputBox("Enter Work Order to Ungroup")

        If orderid = "" Then
            Exit Sub
        End If

        If orders.Contains("/") Then
            clsCustomer.updateQueuing(orderid, orderid)
            gridCustOrder.DataSource = clsCustomer.loadCustomerOpenWo(custIDWo, custWoID)
        Else
            MessageBox.Show("Nothing to Un1group!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If

    End Sub
End Class