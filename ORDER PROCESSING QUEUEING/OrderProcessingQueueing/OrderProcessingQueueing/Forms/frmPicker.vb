Public Class frmPicker

    Public _queueid As Integer
    Public _itemid As Integer
    Public _itemcode As String
    Public _picker As String
    Public _Lastpicker As String
    Public _updateType As String

    Public Shared _data As New DataTable()

    Private Sub btnProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnProcess.Click
        Dim picker = gridPickers.Item(1, gridPickers.CurrentRow.Index).Value

        If _updateType = "All" Then

            _data.Rows.Remove(_data.Rows(_data.Rows.Count - 1))

            For Each x As DataRow In _data.Rows

                If x("picker") = "" Or x("picker") = String.Empty Then

                    clsQueueing.UpdatePicker(x("queueid"), x("itemid"), picker)
                    frmMain.skipUpdate = True

                Else
                    MsgBox("Some Item is already proccessed by another Picker. It will not be updated.", vbExclamation, "Message")

                End If
            Next

            frmMain.prioritizeUserControl(lblGroupID.Text)      ' Prioritize Update
            Me.Close()

        Else

            If _picker = "" Then
                'curcell.Value = picker
                clsQueueing.UpdatePicker(_queueid, _itemid, picker)
                frmMain.prioritizeUserControl(lblGroupID.Text)      ' Prioritize Update
                Me.Close()

            Else
                MsgBox("Unable to process item! Item is being proccessed by another Picker.", vbCritical, "Message")
            End If

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
        If _updateType = "All" Then
            lblItemCode.Text = "---------"
            lblDesc.Text = "---------"
        End If
        LoadPickers()
        Me.Text = "Choose Picker"
    End Sub

    Private Sub LoadPickers()
        gridPickers.DataSource = clsQueueing.LoadPickers
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