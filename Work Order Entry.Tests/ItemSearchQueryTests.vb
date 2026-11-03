Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class ItemSearchQueryTests

    Private Const Prefix As String = "SELECT TOP 500 * FROM SOD_VIEWITEMSWO  WHERE Inactive = 0 AND FULLDESC Like '%"

    <TestMethod>
    Public Sub SingleKeywordKeepsTheExistingQueryText()
        Assert.AreEqual(Prefix & "PAPER%'", ItemSearchQuery.Build("PAPER"))
    End Sub

    <TestMethod>
    Public Sub EveryKeywordMustMatch()
        Assert.AreEqual(Prefix & "A4%' AND [FULLDESC] Like '%PAPER%'", ItemSearchQuery.Build("A4,PAPER"))
    End Sub

    <TestMethod>
    Public Sub UserWildcardsStillApply()
        Assert.AreEqual(Prefix & "50%_OFF%'", ItemSearchQuery.Build("50%_OFF"))
    End Sub

    <TestMethod>
    Public Sub ApostropheStaysInsideTheLiteral()
        Assert.AreEqual(Prefix & "MEN''S%'", ItemSearchQuery.Build("MEN'S"))
    End Sub

    <TestMethod>
    Public Sub ApostropheInEveryKeywordIsDoubled()
        Assert.AreEqual(Prefix & "O''NEIL%' AND [FULLDESC] Like '%MEN''S%'", ItemSearchQuery.Build("O'NEIL,MEN'S"))
    End Sub

    <TestMethod>
    Public Sub QuoteCannotEndTheLiteral()
        Assert.AreEqual(Prefix & "x''; DROP TABLE t; --%'", ItemSearchQuery.Build("x'; DROP TABLE t; --"))
    End Sub

End Class
