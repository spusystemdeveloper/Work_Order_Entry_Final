Partial Public Class frmItemLookUp
    Friend WithEvents grpWithholding As GroupBox
    Friend WithEvents withholdingLayout As TableLayoutPanel
    Friend WithEvents lblWithholdingBasis As Label
    Friend WithEvents lblWithholding1 As Label
    Friend WithEvents lblWithholding2 As Label
    Friend WithEvents lblWithholding5 As Label
    Friend WithEvents lblWithholding15 As Label
    Friend WithEvents withholdingToolTip As ToolTip
    Private showWithholding As Boolean
    Private normalSelectedHeight As Integer

    Public Sub New()
        InitializeComponent()
        normalSelectedHeight = panelSelectItem.Height
        ' Separate display only; existing totals and grid columns stay intact.
        grpWithholding = New GroupBox With {
            .Text = "Suggested W/Tax", .BackColor = Color.AliceBlue,
            .ForeColor = Color.FromArgb(20, 58, 94),
            .Font = New Font("Tahoma", 9.75F, FontStyle.Bold),
            .TabStop = False, .Visible = False}
        withholdingLayout = New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .ColumnCount = 4, .RowCount = 2,
            .Padding = New Padding(6, 0, 6, 0), .TabStop = False}
        For index As Integer = 0 To 3
            withholdingLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        Next
        withholdingLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 62.0F))
        withholdingLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 38.0F))
        lblWithholding1 = NewWithholdingLabel()
        lblWithholding2 = NewWithholdingLabel()
        lblWithholding5 = NewWithholdingLabel()
        lblWithholding15 = NewWithholdingLabel()
        Dim labels = {lblWithholding1, lblWithholding2, lblWithholding5, lblWithholding15}
        For index As Integer = 0 To labels.Length - 1
            withholdingLayout.Controls.Add(labels(index), index, 0)
        Next
        lblWithholdingBasis = New Label With {
            .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Tahoma", 9.0F), .TabStop = False}
        withholdingLayout.Controls.Add(lblWithholdingBasis, 0, 1)
        withholdingLayout.SetColumnSpan(lblWithholdingBasis, 4)
        grpWithholding.Controls.Add(withholdingLayout)
        withholdingToolTip = New ToolTip(components)
        panelTotal.Controls.Add(grpWithholding)
        LayoutWithholdingFooter()
    End Sub

    Private Shared Function NewWithholdingLabel() As Label
        Return New Label With {
            .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Tahoma", 13.5F, FontStyle.Bold),
            .AutoEllipsis = True, .TabStop = False}
    End Function

    Private Sub WithholdingFooterResized(sender As Object, e As EventArgs) Handles panelTotal.SizeChanged, MyBase.ClientSizeChanged
        LayoutWithholdingFooter()
    End Sub

    Private Sub LayoutWithholdingFooter()
        If grpWithholding Is Nothing Then Return
        Dim extraHeight As Integer = If(showWithholding, 92, 0)
        If panelTotal.Height <> 63 + extraHeight Then panelTotal.Height = 63 + extraHeight
        grpWithholding.SetBounds(5, 3, Math.Max(0, panelTotal.ClientSize.Width - 10), 86)
        For Each totalBox As GroupBox In {GroupBox2, GroupBox3, GroupBox4}
            totalBox.Top = 1 + extraHeight
        Next
        ' Keep the customer area usable on 1366x768; restore the original
        ' selected-item height whenever there is enough space.
        panelGrid.PerformLayout()
        Dim room As Integer = panelItem.Height - Panel5.Height
        panelSelectItem.Height = Math.Max(120, Math.Min(normalSelectedHeight,
                                                      panelSelectItem.Height + room))
    End Sub
End Class
