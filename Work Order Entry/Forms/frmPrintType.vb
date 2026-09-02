Public Class frmPrintType

    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click

        Dim orderNumber As Integer

        If Not Integer.TryParse(txtWO.Text.Trim(), orderNumber) Then
            MessageBox.Show("Input a valid Order Number.", "Message!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            txtWO.Focus()
            Exit Sub
        End If

        Dim orderType As Integer? = clsPickList.GetOrderType(orderNumber)

        If Not orderType.HasValue Then
            MessageBox.Show("Order Number not found!", "Message!",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            txtWO.SelectAll()
            txtWO.Focus()
            Exit Sub
        End If

        If rbtnPickList.Checked Then
            frmPrintPicklist.wo = orderNumber
            frmPrintPicklist.ShowDialog()
        Else
            Dim templateName As String =
                OrderPrintRules.GetTemplateName(orderType.Value)

            If String.IsNullOrEmpty(templateName) Then
                MessageBox.Show(
                    "Order Number " & orderNumber &
                    " has an unsupported order type (" & orderType.Value & ").",
                    "Unable to Print Order",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation)
                Exit Sub
            End If

            frmPrintWo.wo = orderNumber
            frmPrintWo.Type = templateName
            frmPrintWo.ShowDialog()
        End If

        Me.Close()
    End Sub

    Private Sub frmPrintType_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        txtBoxValidation.AssignValidation(txtWO, ValidationType.Only_Numbers)
        txtWO.Text = ""

    End Sub
End Class
