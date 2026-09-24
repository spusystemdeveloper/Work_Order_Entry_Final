Public Class frmQtyPrep

    Public _queueid As Integer
    Public _itemid As Integer
    Public _itemcode As String
    Public _picker As String
    Public _qtyOrd As Integer
    Public _status As String = ""

    Private Sub frmQtyPrep_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = "Set Quantity"
        Me.txtQty.Text = _qtyOrd

        If _status = "Shortage" Then
            btnShortage.Text = "✅ Clear Short"
            btnShortage.BackColor = Color.FromArgb(209, 250, 229)
            btnShortage.ForeColor = Color.FromArgb(4, 120, 87)
        Else
            btnShortage.Text = "⚠️ Shortage"
            btnShortage.BackColor = Color.FromArgb(254, 243, 199)
            btnShortage.ForeColor = Color.FromArgb(180, 83, 9)
        End If
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click

        If IsNumeric(txtQty.Text) Then

            If txtQty.Text > _qtyOrd Then

                MessageBox.Show("Quantity Prepared cannot be more than the Quantity Order !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

            ElseIf CInt(txtQty.Text) < _qtyOrd Then

                Dim res As DialogResult = MessageBox.Show("Prepared quantity (" & txtQty.Text & ") is less than ordered (" & _qtyOrd & ")." & vbCrLf & vbCrLf &
                                                          "Do you want to flag this item as SHORTAGE (⚠️ Out of Stock) to alert Front Counter / Cashier?" & vbCrLf & vbCrLf &
                                                          "[Yes] = Flag as Shortage Alert" & vbCrLf &
                                                          "[No] = Save as normal partial prep" & vbCrLf &
                                                          "[Cancel] = Do not save", "Quantity Warning / Shortage", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

                If res = DialogResult.Yes Then
                    clsQueueing.FlagItemShortage(_queueid, _itemid, CInt(txtQty.Text))
                    frmMain.prioritizeUserControl(lblGroupID.Text)
                    Me.Close()
                    Exit Sub
                ElseIf res = DialogResult.No Then
                    GoTo Proceed
                Else
                    Exit Sub
                End If

            Else
Proceed:
                clsQueueing.UpdateQtyPrep(_queueid, _itemid, txtQty.Text)
                frmMain.prioritizeUserControl(lblGroupID.Text)                   ' prioritize update
                Me.Close()

            End If

        Else
            MsgBox("Please input valid quantity!", vbCritical, "Message")
        End If

    End Sub

    Private Sub btnShortage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShortage.Click
        If _status = "Shortage" Then
            Dim confirmClear = MessageBox.Show("Clear the SHORTAGE flag and restore this item to normal Processing status?" & vbCrLf & vbCrLf &
                                               "Item: " & lblItemCode.Text & vbCrLf &
                                               "Ordered: " & _qtyOrd,
                                               "Clear Shortage Flag", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirmClear = DialogResult.Yes Then
                Dim qtyVal As Integer = _qtyOrd
                If IsNumeric(txtQty.Text) AndAlso CInt(txtQty.Text) > 0 Then
                    qtyVal = CInt(txtQty.Text)
                End If
                clsQueueing.UpdateQtyPrep(_queueid, _itemid, qtyVal.ToString())
                clsQueueing.UpdateStatus(_queueid, _itemid, "Processing")
                frmMain.prioritizeUserControl(lblGroupID.Text)
                Me.Close()
            End If
            Exit Sub
        End If

        Dim qtyFound As Integer = 0
        If IsNumeric(txtQty.Text) Then
            qtyFound = CInt(txtQty.Text)
        End If

        If qtyFound > _qtyOrd Then
            MessageBox.Show("Quantity found cannot exceed ordered quantity (" & _qtyOrd & ")!", "Shortage", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If

        Dim confirm = MessageBox.Show("Flag this item as SHORTAGE / OUT OF STOCK?" & vbCrLf & vbCrLf &
                                      "Item: " & lblItemCode.Text & vbCrLf &
                                      "Ordered: " & _qtyOrd & vbCrLf &
                                      "Found in Stock: " & qtyFound & vbCrLf & vbCrLf &
                                      "This will immediately notify Order Takers / Cashiers.",
                                      "Confirm Stock Shortage", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

        If confirm = DialogResult.Yes Then
            clsQueueing.FlagItemShortage(_queueid, _itemid, qtyFound)
            frmMain.prioritizeUserControl(lblGroupID.Text)
            Me.Close()
        End If
    End Sub
End Class
