Imports System.Globalization
Imports System.ComponentModel

Partial Public Class frmItemLookUp
    Private withholdingCustomerID As Integer
    Private withholdingRates As New List(Of Integer)()
    Private updatingWithholding As Boolean

    Private Sub WithholdingInputsChanged(sender As Object, e As EventArgs) _
        Handles txtCustomer.TextChanged, txtSub.TextChanged,
                chkWorkOrder.CheckedChanged, chkQuote.CheckedChanged, MyBase.Shown
        ' Avoid database work during InitializeComponent or Designer construction.
        If Not IsHandleCreated Then Return
        UpdateWithholdingSuggestions()
    End Sub

    Private Sub UpdateWithholdingSuggestions()
        If DesignMode OrElse LicenseManager.UsageMode = LicenseUsageMode.Designtime OrElse
           Not IsHandleCreated OrElse updatingWithholding OrElse grpWithholding Is Nothing OrElse
           lblWithholdingBasis Is Nothing Then Return
        updatingWithholding = True
        Try
            If iCusID <= 0 OrElse String.IsNullOrWhiteSpace(txtCustomer.Text) Then
                withholdingCustomerID = 0
                withholdingRates.Clear()
                grpWithholding.Visible = False
                Return
            End If
            If Not chkWorkOrder.Checked OrElse chkQuote.Checked Then
                grpWithholding.Visible = False
                Return
            End If
            If withholdingCustomerID <> iCusID Then
                withholdingRates.Clear()
                withholdingCustomerID = iCusID
                Using dbx = GetDB()
                    withholdingRates = CustomerWithholdingSuggestions.LoadRates(dbx, iCusID)
                End Using
            End If

            Dim basis As Decimal
            If withholdingRates.Count = 0 OrElse Not Decimal.TryParse(
                txtSub.Text, NumberStyles.Number, CultureInfo.CurrentCulture, basis) Then
                grpWithholding.Visible = False
                Return
            End If

            lblWithholdingBasis.Text = "VAT-exclusive: " & basis.ToString("N2")
            Dim labels = {lblWithholding1, lblWithholding2, lblWithholding5, lblWithholding15}
            Dim rates = {1, 2, 5, 15}
            For index As Integer = 0 To rates.Length - 1
                Dim enabled = withholdingRates.Contains(rates(index))
                labels(index).Visible = enabled
                labels(index).Text = If(enabled,
                    rates(index).ToString() & "%: " &
                    CustomerWithholdingSuggestions.Amount(basis, rates(index)).ToString("N2"), "")
                withholdingToolTip.SetToolTip(labels(index),
                    labels(index).Text & Environment.NewLine & lblWithholdingBasis.Text &
                    Environment.NewLine & "Suggestion only. Enter the applicable tender in RMS.")
            Next
            ' Pack applicable rates first; hidden labels must not leave empty slots.
            withholdingLayout.SuspendLayout()
            Try
                Dim column As Integer = 0
                For Each enabledFirst As Boolean In {True, False}
                    For index As Integer = 0 To rates.Length - 1
                        If withholdingRates.Contains(rates(index)) = enabledFirst Then
                            If withholdingLayout.GetColumn(labels(index)) <> column Then
                                withholdingLayout.SetColumn(labels(index), column)
                            End If
                            column += 1
                        End If
                    Next
                Next
            Finally
                withholdingLayout.ResumeLayout()
            End Try
            withholdingToolTip.SetToolTip(lblWithholdingBasis,
                lblWithholdingBasis.Text & Environment.NewLine &
                "Uses the existing order subtotal. Order totals are unchanged.")
            grpWithholding.Visible = True
        Catch ex As Exception
            ' A failed optional estimate must not interrupt customer selection,
            ' quantity validation, saving, printing or the existing focus flow.
            grpWithholding.Visible = False
            ErrorLogger.LogError(ex, "Customer withholding suggestions")
        Finally
            updatingWithholding = False
            ArrangeSmallScreen(Nothing, EventArgs.Empty)
        End Try
    End Sub
End Class
