Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class BranchSelectionRulesTests

    <TestMethod>
    Public Sub GivenMatchingDatabaseNames_WhenValidated_ThenBranchIsAccepted()
        Assert.IsTrue(
            BranchSelectionRules.DatabaseMatches(
                "DVO_STORE_DB", "DVO_STORE_DB"))
    End Sub

    <TestMethod>
    Public Sub GivenDifferentDatabaseNames_WhenValidated_ThenBranchIsRejected()
        Assert.IsFalse(
            BranchSelectionRules.DatabaseMatches(
                "GSC_STORE_DB", "DVO_STORE_DB"))
    End Sub

    <TestMethod>
    Public Sub GivenNoExpectedName_WhenActualDatabaseExists_ThenLocalRmsDatabaseIsAccepted()
        Assert.IsTrue(
            BranchSelectionRules.DatabaseMatches(
                "CEBU_STORE_DB", String.Empty))
    End Sub

End Class
