Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms

' This standalone visual demo does not reference the production application.
Module PreviewApp
    <STAThread>
    Sub Main(args As String())
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        If args.Length = 2 AndAlso args(0) = "--check" Then
            PreviewWindow.CheckAndRender(args(1))
        Else
            Application.Run(New PreviewWindow())
        End If
    End Sub
End Module

Public Class PreviewWindow
    Inherits Form

    Private ReadOnly ink As Color = Color.FromArgb(28, 35, 44)
    Private ReadOnly accent As Color = Color.FromArgb(0, 120, 212)
    Private ReadOnly pale As Color = Color.FromArgb(247, 249, 252)
    Private ReadOnly lookup As New DataGridView()
    Private ReadOnly selected As New DataGridView()
    Private ReadOnly search As New TextBox()
    Private ReadOnly emptyMessage As New Label()
    Private ReadOnly taxTabs As New TabControl()
    Private ReadOnly totals As New TableLayoutPanel()
    Private ReadOnly footer As New Label()
    Private ReadOnly tip As New ToolTip()
    Private ReadOnly sampleItems As Object()() = {
        New Object() {"55157CSE", "BOOK PAPER IK ONE A4 70gsm 500s 5 RMS (CSE)", 890D, 1000D, 1020D, 20D},
        New Object() {"55142CSE", "BOOK PAPER IK ONE 8-1/2in x 11in 80gsm 500s 5 RMS (CSE)", 980D, 1000D, 1012D, 12D},
        New Object() {"55144RMS", "BOOK PAPER IK ONE A4 80gsm 500s (RMS)", 203D, 320D, 400D, 80D},
        New Object() {"55143RMS", "BOOK PAPER IK ONE 8-1/2in x 13in 80gsm 500s (RMS)", 231D, 520D, 600D, 80D},
        New Object() {"55158RMS", "BOOK PAPER IK ONE 8-1/2in x 11in 70gsm 500s (RMS)", 171D, 500D, 520D, 20D},
        New Object() {"55159RMS", "BOOK PAPER IK ONE 8-1/2in x 13in 70gsm 500s (RMS)", 203D, 600D, 640D, 40D},
        New Object() {"PEN001", "BALLPEN FINE POINT BLACK (PCS)", 12.5D, 1250D, 1500D, 250D},
        New Object() {"NOTE001", "SPIRAL NOTEBOOK 80 LEAVES (PCS)", 45D, 300D, 350D, 50D}}

    Public Sub New()
        Text = "Work Order Entry - Modern Blue - PREVIEW ONLY"
        Font = New Font("Segoe UI", 10.5F)
        ForeColor = ink
        BackColor = Color.FromArgb(243, 245, 248)
        AutoScaleMode = AutoScaleMode.Dpi
        MinimumSize = New Size(1180, 690)
        Size = New Size(1366, 768)
        StartPosition = FormStartPosition.CenterScreen

        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 6, .Padding = New Padding(10)}
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 145))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 108))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 45))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 60))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 40))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 26))
        Controls.Add(root)

        Dim details = CreateDetails()
        root.Controls.Add(details, 0, 0)
        root.SetColumnSpan(details, 2)
        root.Controls.Add(CreateSearchBar(), 0, 1)
        root.SetColumnSpan(root.GetControlFromPosition(0, 1), 2)
        ConfigureGrid(lookup, {"ITEM CODE", "ITEM NAME", "PRICE", "AVAIL QTY", "SOH", "QTY COM"}, {110, 350, 95, 95, 85, 90})
        ConfigureGrid(selected, {"ITEM CODE", "ITEM NAME", "QTY", "PRICE", "TOTAL", "LAST PURCHASE"}, {110, 350, 80, 95, 115, 120})
        Dim lookupHost As New PreviewCard With {.Dock = DockStyle.Fill, .Padding = New Padding(10), .Margin = New Padding(0, 0, 8, 8)}
        lookupHost.Controls.Add(lookup)
        emptyMessage.Text = "No items found. Try another code or description."
        emptyMessage.Dock = DockStyle.Fill
        emptyMessage.TextAlign = ContentAlignment.MiddleCenter
        emptyMessage.BackColor = Color.White
        emptyMessage.Visible = False
        lookupHost.Controls.Add(emptyMessage)
        root.Controls.Add(lookupHost, 0, 2)
        Dim selectedCard As New PreviewCard With {.Dock = DockStyle.Fill, .Padding = New Padding(10, 3, 10, 8), .Margin = New Padding(0, 0, 8, 8)}
        Dim selectedHost As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2, .Margin = New Padding(0)}
        selectedHost.RowStyles.Add(New RowStyle(SizeType.Absolute, 26))
        selectedHost.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        selectedHost.Controls.Add(Caption("Selected Items"), 0, 0)
        selectedHost.Controls.Add(selected, 0, 1)
        selectedCard.Controls.Add(selectedHost)
        root.Controls.Add(selectedCard, 0, 3)
        root.Controls.Add(CreateActions(), 1, 2)
        root.SetRowSpan(root.GetControlFromPosition(1, 2), 3)

        Dim bottom As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 1, .Margin = New Padding(0)}
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 53))
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 47))
        bottom.Controls.Add(CreateTaxTabs(), 0, 0)
        bottom.Controls.Add(CreateTotals(), 1, 0)
        root.Controls.Add(bottom, 0, 4)
        footer.Text = "UI PREVIEW ONLY  |  Sample data  |  No database connection"
        footer.Dock = DockStyle.Fill
        footer.TextAlign = ContentAlignment.MiddleLeft
        footer.Font = New Font("Segoe UI", 9.0F)
        root.Controls.Add(footer, 0, 5)
        root.SetColumnSpan(footer, 2)
        selected.Rows.Add("55143RMS", "BOOK PAPER IK ONE 8-1/2in x 13in 80gsm 500s (RMS)", 10D, 231D, 2310D, 220D)
        selected.Rows.Add("55144RMS", "BOOK PAPER IK ONE A4 80gsm 500s (RMS)", 5D, 203D, 1015D, 195D)
        AddHandler search.TextChanged, Sub() FilterItems()
        FilterItems()
    End Sub

    Private Function Caption(text As String) As Label
        Return New Label With {.Text = text, .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .Font = New Font(Font, FontStyle.Bold), .AutoEllipsis = True, .Margin = New Padding(0)}
    End Function

    Private Function CreateDetails() As Control
        Dim card As New PreviewCard With {.Dock = DockStyle.Fill, .Padding = New Padding(12, 4, 12, 4), .Margin = New Padding(0, 0, 0, 6)}
        Dim fields As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 4, .RowCount = 2, .Margin = New Padding(0)}
        For Each width As Single In {36.0F, 18.0F, 18.0F, 28.0F}
            fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, width))
        Next
        fields.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        fields.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        card.Controls.Add(fields)
        AddHeaderField(fields, 0, 0, "Customer [F8]", "BARANGAY GOVERNMENT OF MAA")
        AddHeaderField(fields, 1, 0, "Sales Rep [F7]", "WALK-IN")
        AddHeaderField(fields, 2, 0, "Payment", "CHARGE")
        Dim mode As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False, .Padding = New Padding(5, 16, 0, 0), .Margin = New Padding(0)}
        mode.Controls.Add(New RadioButton With {.Text = "Work Order", .AutoSize = True, .Checked = True})
        mode.Controls.Add(New RadioButton With {.Text = "Quotation", .AutoSize = True})
        fields.Controls.Add(mode, 3, 0)
        AddHeaderField(fields, 0, 1, "Comment", "Office supplies")
        AddHeaderField(fields, 1, 1, "Customer Type", "GOVERNMENT")
        Dim release As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False, .Padding = New Padding(5, 16, 0, 0), .Margin = New Padding(0)}
        release.Controls.Add(New RadioButton With {.Text = "Pick-up", .AutoSize = True, .Checked = True})
        release.Controls.Add(New RadioButton With {.Text = "Delivery", .AutoSize = True})
        fields.Controls.Add(release, 2, 1)
        fields.SetColumnSpan(release, 2)
        Return card
    End Function

    Private Sub AddHeaderField(host As TableLayoutPanel, column As Integer, row As Integer, title As String, value As String)
        Dim field As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2, .Margin = New Padding(0, 0, 12, 0)}
        field.RowStyles.Add(New RowStyle(SizeType.Absolute, 21))
        field.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        field.Controls.Add(Caption(title), 0, 0)
        Dim input As New TextBox With {.Text = value, .ReadOnly = True, .BorderStyle = BorderStyle.None, .BackColor = Color.White, .Dock = DockStyle.Fill, .Margin = New Padding(0, 1, 0, 0)}
        tip.SetToolTip(input, value)
        field.Controls.Add(input, 0, 1)
        host.Controls.Add(field, column, row)
    End Sub

    Private Function CreateSearchBar() As Control
        Dim bar As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 6, .RowCount = 1, .Margin = New Padding(0, 1, 0, 7)}
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 162))
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 128))
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
        bar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
        bar.Controls.Add(Caption("Item search [F2]"), 0, 0)
        search.Dock = DockStyle.Fill
        search.Margin = New Padding(0, 5, 6, 0)
        tip.SetToolTip(search, "Filter sample items by code or description.")
        bar.Controls.Add(search, 1, 0)
        Dim find As New Button With {.Text = "Search", .FlatStyle = FlatStyle.Flat, .Dock = DockStyle.Fill, .BackColor = Color.White, .Margin = New Padding(0, 0, 9, 0)}
        find.FlatAppearance.BorderColor = Color.FromArgb(200, 209, 219)
        AddHandler find.Click, Sub() FilterItems()
        bar.Controls.Add(find, 2, 0)
        bar.Controls.Add(New CheckBox With {.Text = "Barcode [F3]", .Dock = DockStyle.Fill}, 3, 0)
        bar.Controls.Add(New CheckBox With {.Text = "Show image", .Dock = DockStyle.Fill}, 4, 0)
        Dim recent As New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDownList, .Margin = New Padding(0, 5, 0, 0)}
        recent.Items.AddRange({"Recent searches", "PAPER", "BALLPEN", "NOTEBOOK"})
        recent.SelectedIndex = 0
        AddHandler recent.SelectedIndexChanged, Sub()
                                                   If recent.SelectedIndex > 0 Then search.Text = CStr(recent.SelectedItem)
                                               End Sub
        bar.Controls.Add(recent, 5, 0)
        Return bar
    End Function
    Private Sub AddDetailRow(host As TableLayoutPanel, control As Control, height As Integer)
        Dim row = host.RowCount
        host.RowCount += 1
        host.RowStyles.Add(New RowStyle(SizeType.Absolute, height))
        host.Controls.Add(control, 0, row)
    End Sub

    Private Sub ConfigureGrid(grid As DataGridView, names As String(), widths As Integer())
        grid.Dock = DockStyle.Fill
        grid.Margin = New Padding(0)
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = False
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.ColumnHeadersHeight = 34
        grid.RowTemplate.Height = 34
        grid.GridColor = Color.FromArgb(230, 234, 239)
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        grid.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {.BackColor = pale, .ForeColor = ink, .SelectionBackColor = pale, .SelectionForeColor = ink, .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .Padding = New Padding(5, 0, 2, 0)}
        grid.DefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.White, .ForeColor = ink, .SelectionBackColor = accent, .SelectionForeColor = Color.White, .Padding = New Padding(5, 0, 2, 0)}
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253)
        For i = 0 To names.Length - 1
            Dim col As New DataGridViewTextBoxColumn With {.Name = "Col" & i, .HeaderText = names(i), .Width = widths(i), .MinimumWidth = If(i = 1, 190, 72)}
            If names(i) = "LAST PURCHASE" Then
                col.HeaderText = "LAST PRICE"
                col.ToolTipText = "Last purchase price"
            End If
            If i = 1 Then
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            ElseIf i > 1 Then
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight
                col.DefaultCellStyle.Format = If(names(i) = "PRICE" OrElse names(i) = "TOTAL" OrElse names(i) = "LAST PURCHASE", "N2", "#,##0.###")
                col.ValueType = GetType(Decimal)
            End If
            grid.Columns.Add(col)
        Next
    End Sub

    Private Function CreateActions() As Control
        Dim actions As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .Margin = New Padding(4, 0, 0, 0)}
        actions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For Each name As String In {"Select", "Refresh", "InterStore", "Price Level", "Settings", "Check WO", "Set Tax", "Logout"}
            AddAction(actions, name)
        Next
        AddDetailRow(actions, New Panel(), 12)
        For Each name As String In {"Print", "Save [F12]", "Recall [F11]", "Remove [Del]", "Cancel [F9]", "Close [Esc]"}
            AddAction(actions, name)
        Next
        actions.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        actions.Controls.Add(New Panel With {.Dock = DockStyle.Fill}, 0, actions.RowCount)
        actions.RowCount += 1
        Return actions
    End Function

    Private Sub AddAction(host As TableLayoutPanel, name As String)
        Dim button As New Button With {.Text = name, .Dock = DockStyle.Fill, .FlatStyle = FlatStyle.Flat, .BackColor = Color.White, .ForeColor = ink, .Margin = New Padding(0, 0, 0, 4), .TextAlign = ContentAlignment.MiddleLeft, .Padding = New Padding(8, 0, 0, 0)}
        button.FlatAppearance.BorderColor = Color.FromArgb(187, 204, 211)
        If name.StartsWith("Save", StringComparison.Ordinal) Then
            button.BackColor = accent
            button.ForeColor = Color.White
            button.Font = New Font(Font, FontStyle.Bold)
        ElseIf name.StartsWith("Remove", StringComparison.Ordinal) OrElse name.StartsWith("Cancel", StringComparison.Ordinal) Then
            button.ForeColor = Color.FromArgb(163, 33, 33)
        End If
        AddHandler button.Click, Sub()
                                     If name.StartsWith("Close", StringComparison.Ordinal) Then
                                         Close()
                                     ElseIf name = "Refresh" Then
                                         search.Clear()
                                         FilterItems()
                                     Else
                                         footer.Text = "PREVIEW: " & name & " - appearance only; no action performed."
                                     End If
                                 End Sub
        button.FlatAppearance.BorderColor = Color.FromArgb(218, 224, 232)
        button.FlatAppearance.MouseOverBackColor = If(name.StartsWith("Save", StringComparison.Ordinal), Color.FromArgb(0, 98, 180), Color.FromArgb(232, 241, 251))
        AddDetailRow(host, button, 34)
    End Sub

    Private Function CreateTaxTabs() As Control
        Dim card As New PreviewCard With {.Dock = DockStyle.Fill, .Padding = New Padding(8, 2, 8, 4), .Margin = New Padding(0, 0, 8, 0)}
        Dim panel As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2, .Margin = New Padding(0)}
        card.Controls.Add(panel)
        panel.RowStyles.Add(New RowStyle(SizeType.Absolute, 23))
        panel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        panel.Controls.Add(Caption("Suggested W/Tax"), 0, 0)
        taxTabs.Dock = DockStyle.Fill
        taxTabs.SizeMode = TabSizeMode.Fixed
        taxTabs.ItemSize = New Size(109, 29)
        taxTabs.DrawMode = TabDrawMode.OwnerDrawFixed
        taxTabs.Font = New Font("Segoe UI", 9.5F)
        AddHandler taxTabs.DrawItem, AddressOf DrawTaxTab
        For Each pair In New String()() {New String() {"1%  29.69", "1% = 29.69"}, New String() {"2%  59.38", "2% = 59.38"}, New String() {"5%  148.44", "5% = 148.44"}, New String() {"15%  445.31", "15% = 445.31"}}
            Dim page As New TabPage(pair(0)) With {.BackColor = Color.White}
            Dim line As New Label With {.Text = pair(1) & "    |    VAT-exclusive: 2,968.75", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .Padding = New Padding(6, 0, 0, 0), .ForeColor = accent}
            page.Controls.Add(line)
            taxTabs.TabPages.Add(page)
        Next
        taxTabs.SelectedIndex = 1
        panel.Controls.Add(taxTabs, 0, 1)
        Return card
    End Function

    Private Sub DrawTaxTab(sender As Object, e As DrawItemEventArgs)
        Dim active = e.Index = taxTabs.SelectedIndex
        Using background As New SolidBrush(Color.White)
            e.Graphics.FillRectangle(background, e.Bounds)
        End Using
        TextRenderer.DrawText(e.Graphics, taxTabs.TabPages(e.Index).Text, taxTabs.Font, e.Bounds, If(active, accent, ink), TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        If active Then
            Using underline As New SolidBrush(accent)
                e.Graphics.FillRectangle(underline, e.Bounds.X + 7, e.Bounds.Bottom - 3, e.Bounds.Width - 14, 3)
            End Using
            If taxTabs.Focused Then ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(e.Bounds, -5, -5))
        End If
    End Sub

    Private Function CreateTotals() As Control
        totals.Dock = DockStyle.Fill
        totals.ColumnCount = 3
        totals.RowCount = 2
        totals.Padding = New Padding(8, 5, 8, 5)
        totals.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
        totals.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Dim titles = {"Sub Total", "VAT", "Total"}
        Dim values = {"2,968.75", "356.25", "3,325.00"}
        For i = 0 To 2
            totals.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, If(i = 2, 38, 31)))
            totals.Controls.Add(Caption(titles(i)), i, 0)
            totals.Controls.Add(New Label With {.Text = values(i), .Font = New Font("Segoe UI", If(i = 2, 19.0F, 16.0F), FontStyle.Bold), .ForeColor = If(i = 2, accent, ink), .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleRight, .BackColor = Color.White, .Margin = New Padding(2)}, i, 1)
        Next
        Dim card As New PreviewCard With {.Dock = DockStyle.Fill, .Margin = New Padding(0, 0, 8, 0)}
        card.Controls.Add(totals)
        Return card
    End Function

    Private Sub FilterItems()
        lookup.Rows.Clear()
        Dim term = search.Text.Trim()
        For Each item In sampleItems
            If term.Length = 0 OrElse (CStr(item(0)) & " " & CStr(item(1))).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 Then lookup.Rows.Add(item)
        Next
        emptyMessage.Visible = lookup.Rows.Count = 0
        If emptyMessage.Visible Then emptyMessage.BringToFront()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then tip.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    ' Focused preview checks: no production assembly, data, printer or configuration.
    Public Shared Sub CheckAndRender(outputDirectory As String)
        Directory.CreateDirectory(outputDirectory)
        Dim report As New System.Collections.Generic.List(Of String)()
        For Each size As Size In New Size() {New Size(1366, 728), New Size(1920, 1000)}
            Using window As New PreviewWindow()
                window.Size = size
                window.StartPosition = FormStartPosition.Manual
                window.Location = New Point(-10000, -10000)
                window.Show()
                Application.DoEvents()
                If window.lookup.Rows.Count <> 8 OrElse window.selected.Rows.Count <> 2 Then Throw New Exception("Sample rows missing")
                window.search.Text = "not-present"
                If Not window.emptyMessage.Visible OrElse window.lookup.Rows.Count <> 0 Then Throw New Exception("Empty search message missing")
                window.search.Text = "55143RMS"
                If window.lookup.Rows.Count <> 1 Then Throw New Exception("Sample search failed")
                window.search.Clear()
                For index = 0 To 3
                    window.taxTabs.SelectedIndex = index
                    If DirectCast(window.totals.GetControlFromPosition(2, 1), Label).Text <> "3,325.00" Then Throw New Exception("Tab changed total")
                Next
                window.taxTabs.SelectedIndex = 1
                If window.taxTabs.Width < 445 OrElse window.taxTabs.Height < 60 Then Throw New Exception("Tax tabs clipped")
                If window.selected.DisplayedRowCount(False) < 2 Then Throw New Exception("Selected sample rows clipped")
                Dim lastAction = window.Controls(0).Controls.OfType(Of TableLayoutPanel)().First(Function(p) p.Controls.OfType(Of Button)().Any())
                If lastAction.Controls.OfType(Of Button)().Last().Bottom > lastAction.ClientSize.Height Then Throw New Exception("Actions clipped")
                Using bitmap As New Bitmap(window.Width, window.Height)
                    window.DrawToBitmap(bitmap, New Rectangle(Point.Empty, window.Size))
                    bitmap.Save(Path.Combine(outputDirectory, "Preview-" & size.Width & ".png"))
                End Using
                report.Add(size.Width & ": sample rows, search, empty state, four tax tabs, unchanged sample total and visible actions passed")
                window.Close()
            End Using
        Next
        File.WriteAllLines(Path.Combine(outputDirectory, "Checks.txt"), report)
    End Sub
End Class

' Standard WinForms drawing only; no Guna dependency or Windows 11 backdrop.
Public Class PreviewCard
    Inherits Panel

    Public Sub New()
        DoubleBuffered = True
        BackColor = Color.White
        SetStyle(ControlStyles.ResizeRedraw, True)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If Width < 14 OrElse Height < 14 Then Return
        e.Graphics.Clear(If(Parent Is Nothing, SystemColors.Control, Parent.BackColor))
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim bounds As New Rectangle(0, 0, Width - 1, Height - 1)
        Using path As New GraphicsPath()
            path.AddArc(bounds.Left, bounds.Top, 12, 12, 180, 90)
            path.AddArc(bounds.Right - 12, bounds.Top, 12, 12, 270, 90)
            path.AddArc(bounds.Right - 12, bounds.Bottom - 12, 12, 12, 0, 90)
            path.AddArc(bounds.Left, bounds.Bottom - 12, 12, 12, 90, 90)
            path.CloseFigure()
            Using fill As New SolidBrush(BackColor), border As New Pen(Color.FromArgb(224, 229, 235))
                e.Graphics.FillPath(fill, path)
                e.Graphics.DrawPath(border, path)
            End Using
        End Using
    End Sub
End Class
