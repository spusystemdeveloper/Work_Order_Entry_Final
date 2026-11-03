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

    Public Shared Function OrderHasItem(ByVal orders As DataTable, ByVal orderNumber As String,
                                        ByVal itemCode As String) As Boolean
        For Each row As DataRow In orders.Rows
            If Convert.ToString(row("OrderID")) = orderNumber AndAlso
               Convert.ToString(row("ItemLookupCode")) = itemCode Then
                Return True
            End If
        Next
        Return False
    End Function

    ' Lists requested codes that did not import, one label per code. Website
    ' imports have no Contents sheet, so their labels carry no description.
    Public Shared Function FindMissingItems(ByVal requested As IEnumerable(Of String),
                                            ByVal imported As IEnumerable(Of String),
                                            ByVal contents As DataTable) As List(Of String)
        Dim missing As New List(Of String)
        For Each code In requested.Except(imported)
            missing.Add(code & " - " & FindDescription(contents, code))
        Next
        Return missing
    End Function

    Private Shared Function FindDescription(ByVal contents As DataTable, ByVal code As String) As String
        If contents Is Nothing OrElse Not contents.Columns.Contains("ItemLookupCode") OrElse
           Not contents.Columns.Contains("Fulldesc") Then
            Return String.Empty
        End If

        For Each row As DataRow In contents.Rows
            If Convert.ToString(row("ItemLookupCode")) = code Then
                Return Convert.ToString(row("Fulldesc"))
            End If
        Next
        Return String.Empty
    End Function

    Public Shared Function IsCsv(ByVal fileName As String) As Boolean
        Return String.Equals(Path.GetExtension(fileName), ".csv",
                             StringComparison.OrdinalIgnoreCase)
    End Function

End Class
