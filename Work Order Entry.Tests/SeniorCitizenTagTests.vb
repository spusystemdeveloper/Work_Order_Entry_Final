Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class SeniorCitizenTagTests

    <TestMethod>
    Public Sub GivenSeniorCitizenAndNumericId_WhenTagIsBuilt_ThenSuffixIsAdded()
        ' Given / When
        Dim tag = CStr(clsItemLookUp.TagSC("SC", "12345"))

        ' Then
        Assert.AreEqual(" - S", tag)
    End Sub

    <TestMethod>
    Public Sub GivenSeniorCitizenAndNonNumericId_WhenTagIsBuilt_ThenNoSuffixIsAdded()
        Assert.AreEqual("", CStr(clsItemLookUp.TagSC("SC", "NOT-NUMERIC")))
    End Sub

    <TestMethod>
    Public Sub GivenRegularCustomer_WhenTagIsBuilt_ThenNoSuffixIsAdded()
        Assert.AreEqual("", CStr(clsItemLookUp.TagSC("REGULAR", "12345")))
    End Sub

End Class
