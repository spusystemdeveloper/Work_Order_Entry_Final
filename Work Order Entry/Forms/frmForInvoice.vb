Public Class frmForInvoice

    Dim groupid As String
    Dim orders As String
    Private searchDebounceTimer As Windows.Forms.Timer

    Private Sub frmForInvoice_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ExecuteSearch()
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearch.TextChanged
        If searchDebounceTimer Is Nothing Then
            searchDebounceTimer = New Windows.Forms.Timer()
            searchDebounceTimer.Interval = 300
            AddHandler searchDebounceTimer.Tick, AddressOf SearchDebounceTimer_Tick
        End If
        searchDebounceTimer.Stop()
        searchDebounceTimer.Start()
    End Sub

    Private Sub SearchDebounceTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        If searchDebounceTimer IsNot Nothing Then searchDebounceTimer.Stop()
        ExecuteSearch()
    End Sub

    Private Sub txtSearch_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            If searchDebounceTimer IsNot Nothing Then searchDebounceTimer.Stop()
            ExecuteSearch()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub ExecuteSearch()
        Dim userFilter As String = If(chkStoreWide.Checked, Nothing, frmItemLookUp.usrUsername)
        gridCustOrder.DataSource = Nothing
        gridCustOrder.DataSource = clsItemLookUp.getOrderForInvoicing(txtSearch.Text.Trim(), userFilter)
    End Sub

    Private Sub chkStoreWide_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkStoreWide.CheckedChanged
        ExecuteSearch()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        CleanupTimer()
        Me.Dispose()
        Me.Close()
    End Sub

    Private Sub frmForInvoice_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        CleanupTimer()
    End Sub

    Private Sub CleanupTimer()
        If searchDebounceTimer IsNot Nothing Then
            searchDebounceTimer.Stop()
            searchDebounceTimer.Dispose()
            searchDebounceTimer = Nothing
        End If
    End Sub

    Private Sub btnGroup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGroup.Click
        UpdateSelectionFromGrid()

        If Not String.IsNullOrEmpty(groupid) AndAlso groupid <> "0" Then

            Dim SearchStrArr() As String = Split(orders, ", ")

            For Each x In SearchStrArr

                clsItemLookUp.fakeUpdate(x)
                frmPrintWo.Type = OrderPrintRules.WorkOrderTemplate
                frmPrintWo.wo = x
                frmPrintWo.ShowDialog()

            Next

            clsItemLookUp.UpdateQueueStatus(CInt(groupid), "For Invoicing")

            ExecuteSearch()
            groupid = "0"
            orders = ""

        Else
            MessageBox.Show("Please select an Order", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If

    End Sub

    Private Sub gridCustOrder_DataBindingComplete(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewBindingCompleteEventArgs) Handles gridCustOrder.DataBindingComplete
        Try
            If gridCustOrder.Rows.Count > 0 Then
                gridCustOrder.Rows(0).Selected = False
            End If
            groupid = "0"
            orders = ""
        Catch ex As Exception

        End Try
    End Sub

    Private Sub gridCustOrder_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles gridCustOrder.SelectionChanged
        UpdateSelectionFromGrid()
    End Sub

    Private Sub gridCustOrder_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustOrder.CellClick
        UpdateSelectionFromGrid()
    End Sub

    Private Sub UpdateSelectionFromGrid()
        Try
            If gridCustOrder.CurrentRow IsNot Nothing AndAlso gridCustOrder.CurrentRow.Index >= 0 Then
                groupid = If(gridCustOrder.CurrentRow.Cells(0).Value, "0").ToString()
                orders = If(gridCustOrder.CurrentRow.Cells(2).Value, "").ToString()
            Else
                groupid = "0"
                orders = ""
            End If
        Catch ex As Exception
            groupid = "0"
            orders = ""
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