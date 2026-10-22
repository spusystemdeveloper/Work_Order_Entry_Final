Imports System.Drawing
Imports System.Windows.Forms

Friend Module GridEmptyState
    Public Sub Draw(grid As DataGridView, e As PaintEventArgs, message As String)
        ' Paint only: no placeholder row, focus change, query or filter change.
        ' Nothing bound yet is an initial/loading state, not an empty result.
        If grid.DataSource Is Nothing OrElse grid.RowCount <> 0 Then Return

        Dim area = grid.ClientRectangle
        area.Inflate(-12, -12)
        If grid.ColumnHeadersVisible Then
            area.Y += grid.ColumnHeadersHeight
            area.Height -= grid.ColumnHeadersHeight
        End If
        If area.Width <= 0 OrElse area.Height <= 0 Then Return

        Dim messageFont = If(grid.DefaultCellStyle.Font, grid.Font)
        TextRenderer.DrawText(e.Graphics, message, messageFont, area,
                              SystemColors.GrayText,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix)
    End Sub
End Module
