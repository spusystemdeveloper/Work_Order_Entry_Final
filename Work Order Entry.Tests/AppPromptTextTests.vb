Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class AppPromptTextTests

    <TestMethod>
    Public Sub UnexpectedErrorsKeepDetailBelowPlainSentence()
        Dim detail As String = "FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : timeout"
        Dim text = AppPrompt.FriendlyText(detail)
        Assert.IsTrue(text.StartsWith("Something went wrong.", StringComparison.Ordinal))
        Assert.IsTrue(text.EndsWith(detail, StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub OrdinaryMessagesAreUnchanged()
        Assert.AreEqual("Please select a customer.", AppPrompt.FriendlyText("Please select a customer."))
        Assert.IsNull(AppPrompt.FriendlyText(Nothing))
    End Sub

    <TestMethod>
    Public Sub GeneralTitlesUseTheStandardCaption()
        For Each caption In {"Message!", "Message !", "Message", "Warning", "Validation Warning"}
            Assert.AreEqual(AppPrompt.StandardCaption, AppPrompt.FriendlyCaption(caption))
        Next
    End Sub

    <TestMethod>
    Public Sub NumberedAndSpecificTitlesAreKept()
        Assert.AreEqual("MESSAGE : ERROR 0042", AppPrompt.FriendlyCaption("MESSAGE : ERROR 0042"))
        Assert.AreEqual("First account", AppPrompt.FriendlyCaption("First account"))
    End Sub

End Class
