Public Class frmPicker

    Public _queueid As Integer
    Public _itemid As Integer
    Public _itemcode As String
    Public _picker As String
    Public _Lastpicker As String
    Public _updateType As String

    Public Shared _data As New DataTable()

    Private Sub btnProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnProcess.Click
        If gridPickers.Rows.Count = 0 Then
            MessageBox.Show("No pickers found in the system. Please ensure pickers are configured in the PickerList database table.", "No Pickers Found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If gridPickers.CurrentRow Is Nothing OrElse gridPickers.CurrentRow.Index < 0 Then
            MessageBox.Show("Please select a picker from the list.", "Select Picker", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Dim pickerVal As Object = gridPickers.Item(1, gridPickers.CurrentRow.Index).Value
        If pickerVal Is Nothing OrElse String.IsNullOrWhiteSpace(pickerVal.ToString()) Then
            MessageBox.Show("Selected picker has no valid initial or code.", "Invalid Picker", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim picker As String = pickerVal.ToString().Trim()

        Cursor.Current = Cursors.WaitCursor
        Try
            If _updateType = "All" Then

                If _data IsNot Nothing AndAlso _data.Rows.Count > 0 Then
                    Dim batchUpdates As New List(Of Tuple(Of Long, Integer))()
                    Dim skippedAny As Boolean = False
                    For Each x As DataRow In _data.Rows
                        Dim currPicker = If(x("picker"), "").ToString().Trim()
                        If String.IsNullOrEmpty(currPicker) Then
                            Dim qVal = x("queueid")
                            Dim itmVal = x("itemid")
                            If qVal IsNot Nothing AndAlso itmVal IsNot Nothing AndAlso IsNumeric(qVal) AndAlso IsNumeric(itmVal) Then
                                batchUpdates.Add(Tuple.Create(Convert.ToInt64(qVal), Convert.ToInt32(itmVal)))
                            End If
                        Else
                            skippedAny = True
                        End If
                    Next

                    If batchUpdates.Count > 0 Then
                        clsQueueing.UpdateBatchPicker(batchUpdates, picker)
                        frmMain.skipUpdate = True
                    End If

                    If skippedAny Then
                        MsgBox("Some Item is already processed by another Picker. It will not be updated.", vbExclamation, "Message")
                    End If
                End If

                frmMain.prioritizeUserControl(lblGroupID.Text)      ' Prioritize Update
                Me.Close()

            Else

                If String.IsNullOrWhiteSpace(_picker) Then
                    clsQueueing.UpdatePicker(_queueid, _itemid, picker)
                    frmMain.skipUpdate = True
                    frmMain.prioritizeUserControl(lblGroupID.Text)      ' Prioritize Update
                    Me.Close()
                Else
                    MsgBox("Unable to process item! Item is being processed by another Picker.", vbCritical, "Message")
                End If

            End If
        Finally
            Cursor.Current = Cursors.Default
        End Try

    End Sub

    Private Sub gridPickers_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridPickers.CellDoubleClick
        If e.RowIndex >= 0 Then
            btnProcess_Click(sender, EventArgs.Empty)
        End If
    End Sub

    Private Sub btnUnProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUnProcess.Click

        If clsQueueing.checkStatus(_queueid, _itemid) = "Prepared" Then
            MessageBox.Show("Already in Prepared Status Cannot Change the Picker !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Else

            Dim pickerPass = clsQueueing.getPickerPass(_Lastpicker)
            Dim pass = MyInputBox("")

            If pickerPass = pass Then
                clsQueueing.UpdatePicker(_queueid, _itemid, String.Empty)
                frmMain.prioritizeUserControl(lblGroupID.Text)
                Me.Close()
            Else
                MessageBox.Show("Wrong Password!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If


        End If


    End Sub

    Private Sub frmPicker_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            If _updateType = "All" Then
                lblItemCode.Text = "---------"
                lblDesc.Text = "---------"
            End If
            LoadPickers()
            Me.Text = "Choose Picker"
        Catch ex As Exception
            MessageBox.Show("FROM : frmPicker " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadPickers()
        Try
            gridPickers.DataSource = clsQueueing.LoadPickers()
            If gridPickers.Columns.Count >= 3 Then
                gridPickers.Columns(0).HeaderText = "Reg #"
                gridPickers.Columns(1).HeaderText = "Initial"
                gridPickers.Columns(2).HeaderText = "Name"
                gridPickers.Columns(0).Width = 55
                gridPickers.Columns(1).Width = 65
            End If

            If gridPickers.Rows.Count > 0 AndAlso gridPickers.Columns.Count > 1 Then
                gridPickers.ClearSelection()
                gridPickers.Rows(0).Selected = True
                Try
                    gridPickers.CurrentCell = gridPickers.Rows(0).Cells(1)
                Catch exCell As Exception
                End Try
            End If
        Catch ex As Exception
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
        txtbox.Width = 280
        frmInput.Controls.Add(txtbox)
        frmInput.ActiveControl = txtbox
        frmInput.AcceptButton = btn
        txtbox.TextAlign = HorizontalAlignment.Center
        txtbox.Font = New Font("Century Gothic", 12)
        txtbox.Location = New Point(10, 10)
        txtbox.PasswordChar = "*"
        frmInput.Text = "Enter Last selected Picker Password ..."
        frmInput.ShowDialog()


        Return txtbox.Text
    End Function

    Sub inputclose(ByVal s As Object, ByVal e As EventArgs)

        DirectCast(DirectCast(s, Control).Parent, Form).Close()

    End Sub


    Private Sub frmPicker_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        _updateType = ""
    End Sub
End Class