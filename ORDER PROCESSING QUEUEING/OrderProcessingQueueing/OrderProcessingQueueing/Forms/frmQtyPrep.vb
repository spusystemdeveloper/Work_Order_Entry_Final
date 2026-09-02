Public Class frmQtyPrep

    Public _queueid As Integer
    Public _itemid As Integer
    Public _itemcode As String
    Public _picker As String
    Public _qtyOrd As Integer

    Private Sub frmQtyPrep_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = "Set Quantity"
        Me.txtQty.Text = _qtyOrd
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click

        If IsNumeric(txtQty.Text) Then

            If txtQty.Text > _qtyOrd Then

                MessageBox.Show("Quantity Prepared cannot be more than the Quantity Order !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

            ElseIf txtQty.Text < _qtyOrd Then

                MessageBox.Show("Quantity Prepare cannot be less than the Quantity Order !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

                Dim res As Integer
                res = MsgBox("Do you still want to Continue ?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Message!")

                If res = MsgBoxResult.Yes Then
                    GoTo Proceed
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
End Class
