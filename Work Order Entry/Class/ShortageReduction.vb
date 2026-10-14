Option Strict On

Imports System.Globalization
Imports System.Linq

Public Class ShortageLineState
    Public Property OrderID As Integer
    Public Property EntryID As Integer
    Public Property ItemID As Integer
    Public Property QueueItemID As Long
    Public Property QueueHeaderID As Long
    Public Property OrderType As Integer
    Public Property OrderClosed As Boolean
    Public Property QueueStatus As Integer?
    Public Property ItemType As Integer
    Public Property OrderedQuantity As Double
    Public Property FoundQuantity As Integer?
    Public Property Status As String
    Public Property Picker As String
    Public Property MatchingOrderLines As Integer
    Public Property MatchingQueueLines As Integer
End Class

Public NotInheritable Class ShortageReductionRules
    Private Sub New()
    End Sub

    Public Shared Sub Validate(line As ShortageLineState)
        If line Is Nothing OrElse line.OrderID <= 0 OrElse line.EntryID <= 0 OrElse
           line.ItemID <= 0 OrElse line.QueueItemID <= 0 OrElse line.QueueHeaderID <= 0 Then
            Throw New InvalidOperationException("Select a recalled Work Order line flagged as a warehouse shortage.")
        End If
        If line.OrderType <> 2 OrElse line.OrderClosed OrElse line.QueueStatus <> 0 OrElse
           Not line.QueueStatus.HasValue OrElse line.ItemType = 7 OrElse
           Not String.Equals(line.Status, "Shortage", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Only an open Work Order's inventory line flagged Shortage can be reduced to the quantity physically found.")
        End If
        ' The legacy queue maps by ItemID, not OrderEntryID. Do not guess which
        ' line a prepared quantity belongs to when a SKU/header is duplicated.
        If line.MatchingOrderLines <> 1 OrElse line.MatchingQueueLines <> 1 Then
            Throw New InvalidOperationException("This item has multiple order or queue lines. Resolve the duplicate lines before accepting its shortage.")
        End If
        If Double.IsNaN(line.OrderedQuantity) OrElse Double.IsInfinity(line.OrderedQuantity) OrElse
           line.OrderedQuantity <= 0 OrElse Not line.FoundQuantity.HasValue Then
            Throw New InvalidOperationException("The ordered or physically found quantity is missing or invalid. Reload the order and check with the warehouse.")
        End If
        If line.FoundQuantity.Value <= 0 Then
            Throw New InvalidOperationException("No stock was found. Keep this item pending or use Remove after agreeing with the customer. This action cannot create a zero-quantity line.")
        End If
        If line.FoundQuantity.Value >= line.OrderedQuantity Then
            Throw New InvalidOperationException("The physically found quantity must be less than the ordered quantity.")
        End If
    End Sub
End Class

Public NotInheritable Class ShortageReductionRequest
    Public ReadOnly Property OrderID As Integer
    Public ReadOnly Property EntryID As Integer
    Public ReadOnly Property ItemID As Integer
    Public ReadOnly Property QueueItemID As Long
    Public ReadOnly Property QueueHeaderID As Long
    Public ReadOnly Property OriginalQuantity As Double
    Public ReadOnly Property FoundQuantity As Integer
    Public ReadOnly Property Picker As String
    Public ReadOnly Property Reason As String

    Public ReadOnly Property ReservationRelease As Double
        Get
            Return OriginalQuantity - FoundQuantity
        End Get
    End Property

    Public Sub New(line As ShortageLineState, reason As String)
        ShortageReductionRules.Validate(line)
        If String.IsNullOrWhiteSpace(reason) OrElse reason.Trim().Length > 500 Then
            Throw New InvalidOperationException("Enter a customer-agreement note of up to 500 characters.")
        End If
        OrderID = line.OrderID
        EntryID = line.EntryID
        ItemID = line.ItemID
        QueueItemID = line.QueueItemID
        QueueHeaderID = line.QueueHeaderID
        OriginalQuantity = line.OrderedQuantity
        FoundQuantity = line.FoundQuantity.Value
        Picker = If(line.Picker, String.Empty)
        Me.Reason = reason.Trim()
    End Sub
End Class

Public NotInheritable Class ShortageReductionService
    Private Sub New()
    End Sub

    Public Shared Function Load(context As ItemLookUpDataContext, orderID As Integer) As List(Of ShortageLineState)
        Return context.ExecuteQuery(Of ShortageLineState)(
            "SELECT o.ID AS OrderID, oe.ID AS EntryID, oe.ItemID, qi.id AS QueueItemID, " &
            "q.id AS QueueHeaderID, o.Type AS OrderType, o.Closed AS OrderClosed, q.Status AS QueueStatus, " &
            "CONVERT(int, i.ItemType) AS ItemType, oe.QuantityOnOrder AS OrderedQuantity, qi.QtyPre AS FoundQuantity, qi.Status, qi.Picker, " &
            "(SELECT COUNT(*) FROM dbo.OrderEntry x WHERE x.OrderID=o.ID AND x.ItemID=oe.ItemID) AS MatchingOrderLines, " &
            "(SELECT COUNT(*) FROM dbo.Queueing h JOIN dbo.QueueingItems x ON x.QueueingID=h.id " &
            " WHERE h.OrderID=o.ID AND x.ItemID=oe.ItemID) AS MatchingQueueLines " &
            "FROM dbo.[Order] o JOIN dbo.OrderEntry oe ON oe.OrderID=o.ID " &
            "JOIN dbo.Item i ON i.ID=oe.ItemID JOIN dbo.Queueing q ON q.OrderID=o.ID " &
            "JOIN dbo.QueueingItems qi ON qi.QueueingID=q.id AND qi.ItemID=oe.ItemID " &
            "WHERE o.ID={0} AND qi.Status='Shortage';", orderID).ToList()
    End Function

    Public Shared Sub EnsureHistoryEnabled(context As ItemLookUpDataContext)
        If context.ExecuteQuery(Of Integer)(
            "SELECT CASE WHEN OBJECT_ID(N'dbo.SOD_WO_History', N'U') IS NULL THEN 0 ELSE 1 END;").Single() = 0 Then
            Throw New InvalidOperationException("Order history must be enabled for this branch before accepting shortage reductions. Contact your administrator.")
        End If
    End Sub

    Public Shared Sub ValidateBeforeSave(context As ItemLookUpDataContext, orderID As Integer,
                                         requests As IEnumerable(Of ShortageReductionRequest))
        If context.Transaction Is Nothing Then Throw New InvalidOperationException("Shortage decisions require a save transaction.")
        Dim pending = requests.ToList()
        If pending.Count = 0 Then Return
        EnsureHistoryEnabled(context)
        If pending.Select(Function(request) request.EntryID).Distinct().Count() <> pending.Count Then
            Throw New InvalidOperationException("The same shortage line cannot be accepted twice in one save.")
        End If
        Dim current = Load(context, orderID)
        For Each request In pending
            Dim matches = current.Where(Function(candidate) candidate.EntryID = request.EntryID AndAlso
                                            candidate.QueueItemID = request.QueueItemID).ToList()
            If request.OrderID <> orderID OrElse matches.Count <> 1 Then Throw New OrderEditConflictException()
            Dim line = matches(0)
            ShortageReductionRules.Validate(line)
            If line.ItemID <> request.ItemID OrElse line.QueueHeaderID <> request.QueueHeaderID OrElse
               line.OrderedQuantity <> request.OriginalQuantity OrElse line.FoundQuantity <> request.FoundQuantity OrElse
               Not String.Equals(If(line.Picker, String.Empty), request.Picker, StringComparison.Ordinal) Then
                Throw New OrderEditConflictException()
            End If
        Next
    End Sub

    Public Shared Sub ApplyToQueueItem(item As QueueingItem, request As ShortageReductionRequest)
        If item Is Nothing OrElse item.id <> request.QueueItemID OrElse
           item.QueueingID <> request.QueueHeaderID OrElse item.ItemID <> request.ItemID OrElse
           item.QtyPre <> request.FoundQuantity OrElse Not item.QtyPre.HasValue OrElse
           Not String.Equals(item.Status, "Shortage", StringComparison.OrdinalIgnoreCase) OrElse
           Not String.Equals(If(item.Picker, String.Empty), request.Picker, StringComparison.Ordinal) Then
            Throw New OrderEditConflictException()
        End If
        item.QtyPre = request.FoundQuantity
        item.Status = "Prepared"
        ' Preserve the warehouse picker; this is not customer-prepared stock.
    End Sub

    Public Shared Sub RecordDecision(context As ItemLookUpDataContext, request As ShortageReductionRequest)
        If context.Transaction Is Nothing Then Throw New InvalidOperationException("Shortage decisions require a save transaction.")
        Dim details = "Entry: " & request.EntryID & "; customer accepted quantity: " &
            request.OriginalQuantity.ToString("G", CultureInfo.InvariantCulture) & " -> " & request.FoundQuantity &
            "; physically found: " & request.FoundQuantity & "; picker: " & request.Picker &
            "; reserved quantity released: " & request.ReservationRelease.ToString("G", CultureInfo.InvariantCulture) &
            "; reason: " & request.Reason
        Dim affected = context.ExecuteCommand(
            "INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, ItemID, Details) " &
            "SELECT {0}, N'Shortage accepted', " &
            "COALESCE(NULLIF(CONVERT(nvarchar(128), SESSION_CONTEXT(N'SOD_WO_Actor')), N''), ORIGINAL_LOGIN()), " &
            "LEFT(CONCAT(HOST_NAME(), N' / ', CONVERT(nvarchar(128), SESSION_CONTEXT(N'SOD_WO_Register'))), 256), {1}, {2} " &
            "WHERE EXISTS (SELECT 1 FROM dbo.OrderEntry oe JOIN dbo.[Order] o ON o.ID=oe.OrderID " &
            "JOIN dbo.Queueing q ON q.OrderID=o.ID JOIN dbo.QueueingItems qi ON qi.QueueingID=q.id " &
            "WHERE oe.ID={3} AND o.ID={0} AND o.Type=2 AND o.Closed=0 AND q.Status=0 " &
            "AND oe.ItemID={1} AND oe.QuantityOnOrder={4} AND qi.id={5} " &
            "AND qi.ItemID={1} AND qi.QtyPre={4} AND qi.Status='Prepared');",
            request.OrderID, request.ItemID, details, request.EntryID, request.FoundQuantity, request.QueueItemID)
        If affected <> 1 Then Throw New InvalidOperationException("The shortage reduction was not saved consistently. No changes have been committed.")
    End Sub
End Class
