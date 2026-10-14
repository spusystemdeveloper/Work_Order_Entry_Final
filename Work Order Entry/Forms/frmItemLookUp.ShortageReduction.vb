Imports System.Linq

Partial Public Class frmItemLookUp
    Private shortageLines As New Dictionary(Of Integer, ShortageLineState)()
    Private pendingShortages As New Dictionary(Of Integer, ShortageReductionRequest)()
    Private shortageRowAppearance As New Dictionary(Of Integer, Tuple(Of DataGridViewCellStyle, Boolean, Boolean))()
    Private shortageStatus As ToolStripStatusLabel

    Private Sub ResetShortageReductions()
        shortageLines.Clear()
        pendingShortages.Clear()
        shortageRowAppearance.Clear()
        UpdateShortageStatus()
    End Sub

    Private Sub UpdateShortageStatus()
        If shortageStatus Is Nothing Then Return
        shortageStatus.Text = pendingShortages.Count & " shortage reduction(s) - save to apply"
        shortageStatus.Visible = pendingShortages.Count > 0
    End Sub

    Private Sub ReduceSelectedShortage(sender As Object, e As EventArgs)
        Try
            Dim row = gridSelectItem.CurrentRow
            If row Is Nothing OrElse row.IsNewRow OrElse recalledSnapshot Is Nothing OrElse
               Not UseQueueingForCurrentOrder() Then
                Throw New InvalidOperationException("Recall a queued Work Order and select its shortage line first.")
            End If
            Dim entryID = Convert.ToInt32(row.Cells(16).Value)
            If pendingShortages.ContainsKey(entryID) Then
                Throw New InvalidOperationException("This line already has a pending reduction. Save it or use Undo selected shortage reduction.")
            End If
            Dim line As ShortageLineState = Nothing
            If Not shortageLines.TryGetValue(entryID, line) Then
                Throw New InvalidOperationException(
                    "Selected item: " & Convert.ToString(row.Cells(0).Value) & vbCrLf & vbCrLf &
                    "This item was not flagged Shortage by the warehouse when this order was recalled." & vbCrLf & vbCrLf &
                    "In Selected Items, click the shortage-highlighted row, then try the shortage reduction again." & vbCrLf & vbCrLf &
                    "If the warehouse flagged this item after recall, use Order actions > Reload order first.")
            End If
            ShortageReductionRules.Validate(line)
            If Convert.ToDouble(row.Cells(2).Value) <> line.OrderedQuantity Then
                Throw New InvalidOperationException("The line quantity has already been edited. Reload the order before accepting its recorded shortage.")
            End If
            Using context = GetDB()
                ShortageReductionService.EnsureHistoryEnabled(context)
            End Using
            Using confirm As New frmShortageReductionConfirm(Convert.ToString(row.Cells(0).Value), line,
                                                             Convert.ToDecimal(row.Cells(3).Value))
                If confirm.ShowDialog(Me) <> DialogResult.OK Then Return
                Dim request As New ShortageReductionRequest(line, confirm.AgreementNote)
                shortageRowAppearance(entryID) = Tuple.Create(row.DefaultCellStyle.Clone(),
                                                               row.Cells(2).ReadOnly, row.Cells(18).ReadOnly)
                pendingShortages.Add(entryID, request)
                row.Cells(2).Value = request.FoundQuantity
                row.Cells(18).Value = request.FoundQuantity
                row.Cells(2).ReadOnly = True
                row.Cells(18).ReadOnly = True
                row.DefaultCellStyle.BackColor = Color.LightBlue
                row.DefaultCellStyle.ForeColor = Color.DarkBlue
                row.DefaultCellStyle.SelectionBackColor = Color.SteelBlue
                row.DefaultCellStyle.SelectionForeColor = Color.White
                row.Cells(2).ToolTipText = "Customer accepted the warehouse count. Save to apply; use Order actions to undo."
                UpdateAmt()
                UpdateShortageStatus()
            End Using
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Accept shortage", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub UndoSelectedShortage(sender As Object, e As EventArgs)
        Dim row = gridSelectItem.CurrentRow
        If row Is Nothing OrElse row.IsNewRow Then Return
        UndoShortageRow(row)
        UpdateAmt()
        UpdateShortageStatus()
    End Sub

    Private Sub UndoShortageRow(row As DataGridViewRow)
        Dim entryID = Convert.ToInt32(row.Cells(16).Value)
        Dim request As ShortageReductionRequest = Nothing
        If Not pendingShortages.TryGetValue(entryID, request) Then Return
        row.Cells(2).Value = request.OriginalQuantity
        row.Cells(18).Value = request.FoundQuantity
        RestoreShortageAppearance(row, entryID)
        pendingShortages.Remove(entryID)
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If pendingShortages.Count > 0 Then
            If MessageBox.Show(Me,
                "Shortage reductions have not been saved. Close and discard these reductions?" & vbCrLf &
                "Choose No to return and save the order. Customer agreement notes are not stored in drafts.",
                "Unsaved shortage reductions", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                e.Cancel = True
                Return
            End If
            ' Restore original quantities before the existing close handler saves a draft.
            For Each row As DataGridViewRow In gridSelectItem.Rows
                If Not row.IsNewRow Then UndoShortageRow(row)
            Next
            ResetShortageReductions()
            UpdateAmt()
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub RestoreShortageAppearance(row As DataGridViewRow, entryID As Integer)
        Dim appearance As Tuple(Of DataGridViewCellStyle, Boolean, Boolean) = Nothing
        If shortageRowAppearance.TryGetValue(entryID, appearance) Then
            row.DefaultCellStyle = appearance.Item1
            row.Cells(2).ReadOnly = appearance.Item2
            row.Cells(18).ReadOnly = appearance.Item3
        End If
        row.Cells(2).ToolTipText = String.Empty
        shortageRowAppearance.Remove(entryID)
    End Sub

    Private Function GetShortageRequestsForSave() As List(Of ShortageReductionRequest)
        Dim requests = pendingShortages.Values.Where(Function(request) Not delitems.Contains(request.EntryID)).ToList()
        If requests.Count = 0 Then Return requests
        If Not UseQueueingForCurrentOrder() Then
            Throw New InvalidOperationException("Keep this as a queued Work Order to save the shortage decision, or undo the reduction first.")
        End If
        Dim rows = gridSelectItem.Rows.Cast(Of DataGridViewRow)().Where(Function(row) Not row.IsNewRow).ToList()
        For Each request In requests
            Dim matches = rows.Where(Function(candidate) Convert.ToInt32(candidate.Cells(16).Value) = request.EntryID).ToList()
            If matches.Count <> 1 OrElse rows.Where(Function(candidate) Convert.ToInt32(candidate.Cells(11).Value) = request.ItemID).Count() <> 1 Then
                Throw New InvalidOperationException("A pending shortage line was removed or duplicated. Undo the reduction or reload the order.")
            End If
            Dim row = matches(0)
            If Convert.ToInt32(row.Cells(11).Value) <> request.ItemID OrElse
               Convert.ToDouble(row.Cells(2).Value) <> request.FoundQuantity OrElse
               Convert.ToDouble(row.Cells(18).Value) <> request.FoundQuantity Then
                Throw New InvalidOperationException("A pending shortage quantity has changed. Undo the reduction or reload the order.")
            End If
        Next
        Return requests
    End Function

    Private Sub CompleteShortageReductions()
        For Each row As DataGridViewRow In gridSelectItem.Rows
            If row.IsNewRow Then Continue For
            Dim entryID = Convert.ToInt32(row.Cells(16).Value)
            If Not pendingShortages.ContainsKey(entryID) Then Continue For
            RestoreShortageAppearance(row, entryID)
            row.DefaultCellStyle = New DataGridViewCellStyle()
        Next
        ResetShortageReductions()
    End Sub
End Class
