Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class WorkOrderCommentTests

    <TestMethod>
    Public Sub ReadsPaymentReleaseTypeAndRemarks()
        Dim comment = WorkOrderComment.Parse("CASH; Delivery; call first")
        Assert.AreEqual("CASH", comment.Payment)
        Assert.AreEqual("Delivery", comment.ReleaseType)
        Assert.AreEqual("call first", comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub KeepsSeparatorsInsideRemarks()
        Dim comment = WorkOrderComment.Parse("CASH; Delivery; call first; gate 2; rear")
        Assert.AreEqual("call first; gate 2; rear", comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub EmptyRemarksStayEmpty()
        Dim comment = WorkOrderComment.Parse("CHARGE; Pick-up; ")
        Assert.AreEqual("CHARGE", comment.Payment)
        Assert.AreEqual("Pick-up", comment.ReleaseType)
        Assert.AreEqual(String.Empty, comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub TextWithoutAllPartsIsKeptAsRemarks()
        Dim comment = WorkOrderComment.Parse("entered in RMS")
        Assert.AreEqual(String.Empty, comment.Payment)
        Assert.AreEqual(String.Empty, comment.ReleaseType)
        Assert.AreEqual("entered in RMS", comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub ImportedPaymentAndReferenceKeepBothParts()
        Dim comment = WorkOrderComment.Parse("CHARGE; 260814ATY19NEX / customer")
        Assert.AreEqual("CHARGE", comment.Payment)
        Assert.AreEqual(String.Empty, comment.ReleaseType)
        Assert.AreEqual("260814ATY19NEX / customer", comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub TwoPartsWithoutAPaymentStayTogetherAsRemarks()
        Dim comment = WorkOrderComment.Parse("call first; gate 2")
        Assert.AreEqual(String.Empty, comment.Payment)
        Assert.AreEqual("call first; gate 2", comment.Remarks)
    End Sub

    <TestMethod>
    Public Sub MissingCommentReturnsEmptyParts()
        Assert.AreEqual(String.Empty, WorkOrderComment.Parse(Nothing).Remarks)
    End Sub

End Class
