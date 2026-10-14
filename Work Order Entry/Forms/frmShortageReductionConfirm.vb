Option Strict On

Public Class frmShortageReductionConfirm
    Inherits Form

    Private ReadOnly agreement As New CheckBox With {
        .Name = "CustomerAgreement", .Text = "The customer agrees to receive the quantity physically found.",
        .Dock = DockStyle.Fill, .AutoSize = False}
    Private ReadOnly note As New TextBox With {
        .Name = "AgreementNote", .Multiline = True, .MaxLength = 500, .Dock = DockStyle.Fill,
        .Text = "Customer agreed to accept the quantity physically found by the warehouse."}
    Private ReadOnly applyButton As New Button With {
        .Name = "ApplyReduction", .Text = "Apply to order", .AutoSize = True, .Enabled = False}

    Public ReadOnly Property AgreementNote As String
        Get
            Return note.Text.Trim()
        End Get
    End Property

    Public Sub New(itemCode As String, line As ShortageLineState, unitPrice As Decimal)
        ShortageReductionRules.Validate(line)
        If unitPrice < 0 Then Throw New InvalidOperationException("Confirm the item's price before accepting a shortage.")
        Text = "Accept quantity physically found"
        Font = New Font("Segoe UI", 9)
        ClientSize = New Size(600, 425)
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(16), .ColumnCount = 1, .RowCount = 6}
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 148))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        Dim summary As New Label With {.Dock = DockStyle.Fill, .AutoSize = False, .Text =
            "Item: " & itemCode & vbCrLf &
            "Ordered: " & line.OrderedQuantity.ToString("G") & "    Physically found: " & line.FoundQuantity.Value & vbCrLf &
            "Revised line total: " & (unitPrice * CDec(line.OrderedQuantity)).ToString("N2") &
            " → " & (unitPrice * line.FoundQuantity.Value).ToString("N2") & vbCrLf &
            "Reserved quantity released when saved: " & (line.OrderedQuantity - line.FoundQuantity.Value).ToString("G") & vbCrLf &
            "Warehouse picker: " & If(String.IsNullOrWhiteSpace(line.Picker), "Unassigned", line.Picker) & vbCrLf & vbCrLf &
            "This uses the warehouse's physical count. RMS stock on hand is unchanged."}
        layout.Controls.Add(summary, 0, 0)
        layout.Controls.Add(agreement, 0, 1)
        layout.Controls.Add(New Label With {.Text = "Customer-agreement note (required)", .Dock = DockStyle.Fill}, 0, 2)
        layout.Controls.Add(note, 0, 3)
        layout.Controls.Add(New Label With {.Text = "The change remains pending until you save the order.", .Dock = DockStyle.Fill, .Padding = New Padding(0, 8, 0, 0)}, 0, 4)
        Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft}
        Dim cancelButton As New Button With {.Text = "Cancel", .AutoSize = True, .DialogResult = DialogResult.Cancel}
        footer.Controls.Add(cancelButton)
        footer.Controls.Add(applyButton)
        layout.Controls.Add(footer, 0, 5)
        Controls.Add(layout)
        AcceptButton = applyButton
        CancelButton = cancelButton
        AddHandler agreement.CheckedChanged, Sub(sender, e) RefreshApplyButton()
        AddHandler note.TextChanged, Sub(sender, e) RefreshApplyButton()
        AddHandler applyButton.Click, Sub(sender, e)
                                          If agreement.Checked AndAlso Not String.IsNullOrWhiteSpace(note.Text) Then
                                              DialogResult = DialogResult.OK
                                              Close()
                                          End If
                                      End Sub
    End Sub

    Private Sub RefreshApplyButton()
        applyButton.Enabled = agreement.Checked AndAlso Not String.IsNullOrWhiteSpace(note.Text)
    End Sub
End Class
