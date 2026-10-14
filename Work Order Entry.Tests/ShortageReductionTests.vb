Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry
Imports System.Windows.Forms

<TestClass>
Public Class ShortageReductionTests
    Private Function ValidLine() As ShortageLineState
        Return New ShortageLineState With {.OrderID = 5, .EntryID = 10, .ItemID = 2,
            .QueueHeaderID = 3, .QueueItemID = 3, .OrderType = 2, .QueueStatus = 0,
            .OrderedQuantity = 10, .FoundQuantity = 7, .Status = "Shortage", .Picker = "PICKER",
            .MatchingOrderLines = 1, .MatchingQueueLines = 1}
    End Function

    <TestMethod>
    Public Sub PhysicalCountReleasesOnlyUnfulfilledReservationAndCopiesRecallState()
        Dim line = ValidLine()
        Dim request As New ShortageReductionRequest(line, " Customer accepted seven. ")
        line.OrderedQuantity = 99
        line.FoundQuantity = 2
        line.Picker = "CHANGED"
        Assert.AreEqual(7, request.FoundQuantity)
        Assert.AreEqual(3R, request.ReservationRelease)
        Assert.AreEqual("PICKER", request.Picker)
        Assert.AreEqual("Customer accepted seven.", request.Reason)
    End Sub

    <TestMethod>
    Public Sub FractionalOriginalReservationIsNotRounded()
        Dim line = ValidLine()
        line.OrderedQuantity = 7.5
        Assert.AreEqual(0.5R, New ShortageReductionRequest(line, "Accepted seven").ReservationRelease)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub ZeroFoundCannotCreateAnEmptyOrderLine()
        Dim line = ValidLine()
        line.FoundQuantity = 0
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub MissingPhysicalCountIsRejected()
        Dim line = ValidLine()
        line.FoundQuantity = Nothing
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub DuplicateSkuCannotBeMappedToOnePickingCount()
        Dim line = ValidLine()
        line.MatchingOrderLines = 2
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub DuplicateQueueCountIsRejected()
        Dim line = ValidLine()
        line.MatchingQueueLines = 2
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub SettledOrderIsRejected()
        Dim line = ValidLine()
        line.OrderClosed = True
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub CancelledQueueIsRejected()
        Dim line = ValidLine()
        line.QueueStatus = 2
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub QuoteCannotAcceptWarehouseShortage()
        Dim line = ValidLine()
        line.OrderType = 3
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub NonInventoryItemIsRejected()
        Dim line = ValidLine()
        line.ItemType = 7
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub AlreadyPreparedLineIsRejected()
        Dim line = ValidLine()
        line.Status = "Prepared"
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub CountAtOrAboveOrderCannotIncreaseOrder()
        Dim line = ValidLine()
        line.FoundQuantity = 10
        ShortageReductionRules.Validate(line)
    End Sub

    <TestMethod, ExpectedException(GetType(InvalidOperationException))>
    Public Sub AgreementNoteIsRequired()
        Dim request As New ShortageReductionRequest(ValidLine(), "   ")
    End Sub

    <TestMethod>
    Public Sub PreparedStatePreservesWarehousePicker()
        Dim line = ValidLine()
        Dim queue As New QueueingItem With {.id = line.QueueItemID, .QueueingID = line.QueueHeaderID,
            .ItemID = line.ItemID, .QtyPre = line.FoundQuantity, .Status = "Shortage", .Picker = line.Picker}
        ShortageReductionService.ApplyToQueueItem(queue, New ShortageReductionRequest(line, "Accepted"))
        Assert.AreEqual("Prepared", queue.Status)
        Assert.AreEqual("PICKER", queue.Picker)
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub ChangedPickingCountCannotBeOverwritten()
        Dim line = ValidLine()
        Dim queue As New QueueingItem With {.id = line.QueueItemID, .QueueingID = line.QueueHeaderID,
            .ItemID = line.ItemID, .QtyPre = 6, .Status = "Shortage", .Picker = line.Picker}
        ShortageReductionService.ApplyToQueueItem(queue, New ShortageReductionRequest(line, "Accepted"))
    End Sub

    <TestMethod>
    Public Sub ConsentMustBeCheckedAndNoteMustBePresent()
        Using confirm As New frmShortageReductionConfirm("TEST-SKU", ValidLine(), 112D)
            Dim check = DirectCast(confirm.Controls.Find("CustomerAgreement", True)(0), CheckBox)
            Dim note = DirectCast(confirm.Controls.Find("AgreementNote", True)(0), TextBox)
            Dim apply = DirectCast(confirm.Controls.Find("ApplyReduction", True)(0), Button)
            Assert.IsFalse(check.Checked)
            Assert.IsFalse(apply.Enabled)
            check.Checked = True
            Assert.IsTrue(apply.Enabled)
            note.Text = " "
            Assert.IsFalse(apply.Enabled)
            note.Text = "Customer confirmed seven."
            Assert.IsTrue(apply.Enabled)
        End Using
    End Sub
End Class
