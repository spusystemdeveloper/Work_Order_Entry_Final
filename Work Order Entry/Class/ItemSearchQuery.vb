Public NotInheritable Class ItemSearchQuery

    Private Sub New()
    End Sub

    ' Comma-separated keywords must all match. Apostrophes are doubled so a keyword
    ' stays a string literal; LIKE wildcards typed by the user keep working.
    Public Shared Function Build(ByVal filterText As String) As String
        Dim searchTerms() As String = Split(filterText.Replace("'", "''"), ",")
        Dim filterString As String = String.Join("%' AND [FULLDESC] Like '%", searchTerms)
        Return "SELECT TOP 500 * FROM SOD_VIEWITEMSWO  WHERE Inactive = 0 AND FULLDESC Like '%" & filterString & "%'"
    End Function

End Class
