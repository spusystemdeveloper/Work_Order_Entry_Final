Imports System.Data
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class RecallSearchFilterTests

    Private Shared Function Matches(comment As String, search As String) As Integer
        Dim table As New DataTable()
        table.Columns.Add("Comment", GetType(String))
        table.Rows.Add(comment)
        Dim view As New DataView(table)
        view.RowFilter = "[Comment] Like '%" & ShortageAlertList.EscapeSearch(search) & "%'"
        Return view.Count
    End Function

    <TestMethod>
    Public Sub OpeningBracketSearchesLiterally()
        Assert.AreEqual(1, Matches("[A] item", "[A"))
    End Sub

    <TestMethod>
    Public Sub PercentInsideTextSearchesLiterally()
        Assert.AreEqual(1, Matches("a%b", "a%b"))
        Assert.AreEqual(0, Matches("axb", "a%b"))
    End Sub

    <TestMethod>
    Public Sub AsteriskAndApostropheSearchLiterally()
        Assert.AreEqual(1, Matches("2*4 MEN'S", "2*4 MEN'S"))
    End Sub

End Class
