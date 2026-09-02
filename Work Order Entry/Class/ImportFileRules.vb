Imports System.IO

Public NotInheritable Class ImportFileRules

    Public Const UnknownImport As Integer = 0
    Public Const QuotationOrSaleImport As Integer = 1
    Public Const TransferOrPurchaseImport As Integer = 2
    Public Const WebsiteImport As Integer = 3

    Private Sub New()
    End Sub

    Public Shared Function GetImportType(ByVal fileName As String) As Integer
        If String.IsNullOrWhiteSpace(fileName) Then Return UnknownImport

        Dim extension As String = Path.GetExtension(fileName).ToUpperInvariant()
        If extension <> ".XLSX" AndAlso
           extension <> ".XLS" AndAlso
           extension <> ".CSV" Then
            Return UnknownImport
        End If

        Dim name As String =
            Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant()

        If extension = ".CSV" AndAlso name.Contains("WEBSITE") Then
            Return WebsiteImport
        End If

        If name.Contains("QUOTATION") OrElse name.Contains("SALE") Then
            Return QuotationOrSaleImport
        End If

        If name.Contains("TRANSFER") OrElse name.Contains("PURCHASE") Then
            Return TransferOrPurchaseImport
        End If

        Return UnknownImport
    End Function

    Public Shared Function IsCsv(ByVal fileName As String) As Boolean
        Return String.Equals(Path.GetExtension(fileName), ".csv",
                             StringComparison.OrdinalIgnoreCase)
    End Function

End Class
