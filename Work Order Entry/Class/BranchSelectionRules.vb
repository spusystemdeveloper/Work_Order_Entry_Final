Public NotInheritable Class BranchSelectionRules

    Private Sub New()
    End Sub

    Public Shared Function DatabaseMatches(
        ByVal actualDatabaseName As String,
        ByVal expectedDatabaseName As String) As Boolean

        If String.IsNullOrWhiteSpace(actualDatabaseName) Then Return False
        If String.IsNullOrWhiteSpace(expectedDatabaseName) Then Return True

        Return String.Equals(
            actualDatabaseName.Trim(),
            expectedDatabaseName.Trim(),
            StringComparison.OrdinalIgnoreCase)
    End Function

End Class
