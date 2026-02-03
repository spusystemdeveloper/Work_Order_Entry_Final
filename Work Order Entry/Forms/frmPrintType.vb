Public Class frmPrintType

    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click

        If Not txtWO.Text = "" Then

            If clsPickList.orderExist(txtWO.Text) = True Then

                If rbtnPickList.Checked = True Then

                    frmPrintPicklist.wo = txtWO.Text
                    frmPrintPicklist.ShowDialog()

                ElseIf rbtnWo.Checked = True Then

                    frmPrintWo.wo = txtWO.Text
                    frmPrintWo.Type = "Work Order"
                    frmPrintWo.ShowDialog()

                ElseIf rbtnQuotes.Checked = True Then

                    frmPrintWo.wo = txtWO.Text
                    frmPrintWo.Type = "Sales Quotation"
                    frmPrintWo.ShowDialog()

                End If

                Me.Close()

            Else
                MessageBox.Show("Order Number not Found!", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If

        Else
            MessageBox.Show("Input Order Number", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub frmPrintType_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        txtBoxValidation.AssignValidation(txtWO, ValidationType.Only_Numbers)
        txtWO.Text = ""

    End Sub
End Class