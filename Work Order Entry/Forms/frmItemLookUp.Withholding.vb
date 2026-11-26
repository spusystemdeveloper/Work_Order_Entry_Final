Imports System.Globalization
Imports System.ComponentModel

Partial Public Class frmItemLookUp
    Private withholdingCustomerID As Integer
    Private withholdingRates As New List(Of Integer)()
    Private updatingWithholding As Boolean
    Private walkInCustomer As Boolean
    Private walkInRates As List(Of Integer)
    Private walkInRatesCustomerID As Integer
    Private btnWalkInWithholding As Button

    Private Sub EnsureWalkInWithholdingButton()
        If btnWalkInWithholding IsNot Nothing Then Return
        btnWalkInWithholding = New Button With {
            .Name = "btnWalkInWithholding", .Text = "Set W/Tax", .Dock = DockStyle.Top,
            .Height = btnTax.Height, .Font = btnTax.Font, .TextAlign = btnTax.TextAlign,
            .Image = btnTax.Image, .ImageAlign = btnTax.ImageAlign,
            .TextImageRelation = btnTax.TextImageRelation,
            .BackColor = btnTax.BackColor, .ForeColor = btnTax.ForeColor,
            .UseVisualStyleBackColor = btnTax.UseVisualStyleBackColor,
            .FlatStyle = btnTax.FlatStyle, .Visible = False}
        btnWalkInWithholding.FlatAppearance.BorderColor = btnTax.FlatAppearance.BorderColor
        btnWalkInWithholding.FlatAppearance.BorderSize = btnTax.FlatAppearance.BorderSize
        panelButtons.Controls.Add(btnWalkInWithholding)
        btnWalkInWithholding.BringToFront()
        AddHandler btnWalkInWithholding.Click, AddressOf SetWalkInWithholding
    End Sub

    Private Sub SetWalkInWithholding(sender As Object, e As EventArgs)
        If Not walkInCustomer OrElse Not chkWorkOrder.Checked OrElse chkQuote.Checked Then Return
        Using picker As New Form With {.Text = "WALK-IN withholding", .ClientSize = New Size(340, 235),
                                      .StartPosition = FormStartPosition.CenterParent, .Font = New Font("Segoe UI", 11),
                                      .FormBorderStyle = FormBorderStyle.FixedDialog, .MaximizeBox = False,
                                      .MinimizeBox = False, .ShowInTaskbar = False}
            Dim heading As New Label With {.Text = "Select rates for this Work Order only.", .AutoSize = True, .Location = New Point(14, 12)}
            picker.Controls.Add(heading)
            Dim options As New List(Of CheckBox)()
            For Each rate As Integer In {1, 2, 5, 15}
                Dim optionBox As New CheckBox With {.Text = rate.ToString() & "%", .Tag = rate,
                    .Checked = withholdingRates.Contains(rate), .Location = New Point(18, 45 + options.Count * 32), .AutoSize = True}
                options.Add(optionBox)
                picker.Controls.Add(optionBox)
            Next
            Dim accept As New Button With {.Text = "OK", .DialogResult = DialogResult.OK, .Location = New Point(145, 193), .Size = New Size(85, 30)}
            Dim cancel As New Button With {.Text = "Cancel", .DialogResult = DialogResult.Cancel, .Location = New Point(237, 193), .Size = New Size(85, 30)}
            picker.Controls.Add(accept) : picker.Controls.Add(cancel)
            picker.AcceptButton = accept : picker.CancelButton = cancel
            If picker.ShowDialog(Me) <> DialogResult.OK Then Return
            walkInRates = options.Where(Function(optionBox) optionBox.Checked).Select(Function(optionBox) CInt(optionBox.Tag)).ToList()
            walkInRatesCustomerID = iCusID
            withholdingRates = New List(Of Integer)(walkInRates)
            UpdateWithholdingSuggestions()
        End Using
    End Sub

    Private Sub LoadWalkInWithholding(orderID As Integer)
        Using dbx = GetDB()
            Dim notes = dbx.ExecuteQuery(Of String)(
                "SELECT CONVERT(nvarchar(max), ShippingNotes) FROM dbo.[Order] WHERE ID={0}", orderID).SingleOrDefault()
            walkInRates = WorkOrderWithholdingReference.ReadWalkInRates(notes)
            walkInRatesCustomerID = iCusID
            withholdingCustomerID = 0
        End Using
    End Sub

    Private Function ShippingNotesWithWithholding(notes As String) As String
        Dim rates As List(Of Integer) = Nothing
        If walkInCustomer AndAlso walkInRatesCustomerID = iCusID AndAlso
           chkWorkOrder.Checked AndAlso Not chkQuote.Checked Then rates = walkInRates
        Return WorkOrderWithholdingReference.WriteWalkInRates(notes, rates)
    End Function

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
            EnsureWalkInWithholdingButton()
            btnWalkInWithholding.Visible = False
            If iCusID <= 0 OrElse String.IsNullOrWhiteSpace(txtCustomer.Text) Then
                withholdingCustomerID = 0
                withholdingRates.Clear()
                walkInRates = Nothing
                walkInRatesCustomerID = 0
                walkInCustomer = False
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
                    walkInCustomer = WorkOrderWithholdingReference.IsWalkIn(dbx, iCusID)
                    withholdingRates = CustomerWithholdingSuggestions.LoadRates(dbx, iCusID)
                End Using
                If walkInRatesCustomerID <> iCusID OrElse Not walkInCustomer Then
                    walkInRates = Nothing
                    walkInRatesCustomerID = 0
                End If
                If walkInCustomer AndAlso walkInRates IsNot Nothing Then withholdingRates = New List(Of Integer)(walkInRates)
            End If
            btnWalkInWithholding.Visible = walkInCustomer

            Dim basis As Decimal
            If withholdingRates.Count = 0 OrElse Not Decimal.TryParse(
                txtSub.Text, NumberStyles.Number, CultureInfo.CurrentCulture, basis) Then
                grpWithholding.Visible = False
                Return
            End If

            Dim lines As New List(Of WithholdingItemLine)()
            For Each row As DataGridViewRow In gridSelectItem.Rows
                If row.IsNewRow Then Continue For
                Dim itemID As Integer
                Integer.TryParse(Convert.ToString(row.Cells("ItemID").Value), itemID)
                Dim price = Decimal.Parse(Convert.ToString(row.Cells("Price").Value), NumberStyles.Number, CultureInfo.CurrentCulture)
                Dim quantity = Decimal.Parse(Convert.ToString(row.Cells("QTY").Value), NumberStyles.Number, CultureInfo.CurrentCulture)
                lines.Add(New WithholdingItemLine With {.ItemID = itemID, .GrossAmount = price * quantity})
            Next
            Using dbx As New ItemLookUpDataContext(DB_Conn("constr"))
                ItemWithholdingSuggestions.LoadNotes(dbx, lines)
            End Using
            Dim suggestion = ItemWithholdingSuggestions.Calculate(lines, basis, Not bTaxExcempt)
            lblWithholdingBasis.Text = "VAT-exclusive: " & basis.ToString("N2") & " | 1/2/15%: item markers; 5%: " &
                If(walkInCustomer AndAlso walkInRates IsNot Nothing, "order selection", "customer")
            If suggestion.ReviewLines > 0 Then lblWithholdingBasis.Text &= " | NEEDS REVIEW: " & suggestion.ReviewLines.ToString()
            Dim labels = {lblWithholding1, lblWithholding2, lblWithholding5, lblWithholding15}
            Dim rates = {1, 2, 5, 15}
            Dim displayed = rates.Select(Function(rate) withholdingRates.Contains(rate) AndAlso suggestion.Amount(rate) <> 0D).ToArray()
            For index As Integer = 0 To rates.Length - 1
                Dim enabled = withholdingRates.Contains(rates(index))
                labels(index).Visible = displayed(index)
                labels(index).Text = If(enabled,
                    rates(index).ToString() & "%: " &
                    suggestion.Amount(rates(index)).ToString("N2"), "")
                withholdingToolTip.SetToolTip(labels(index),
                    labels(index).Text & Environment.NewLine & "Matching VAT-exclusive base: " & suggestion.Bases(rates(index)).ToString("N2") &
                    Environment.NewLine & "Suggestion only. Enter the applicable tender in RMS.")
            Next
            ' Pack applicable rates first; hidden labels must not leave empty slots.
            withholdingLayout.SuspendLayout()
            Try
                Dim column As Integer = 0
                For Each enabledFirst As Boolean In {True, False}
                    For index As Integer = 0 To rates.Length - 1
                        If displayed(index) = enabledFirst Then
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
                "1%, 2%, 15% use item markers. Enabled 5% uses the VAT-exclusive order subtotal when VAT applies. Totals unchanged." & Environment.NewLine & suggestion.ReviewMessage())
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
