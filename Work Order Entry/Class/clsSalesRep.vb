Public Class clsSalesRep

    Public Shared Function SearchSalesRep(ByVal sName As String) As Object

        Using dbx = GetDB()
            Dim salesrep = (From s In dbx.SalesReps _
                          Where (s.Name.Contains(sName)) _
                          Select s.ID, s.Name Order By _
                          Name).ToList
            Return salesrep
        End Using

    End Function

    Public Shared Function getPrimarySalesRepName(ByVal sCompany As Integer) As String

        Using dbx = GetDB()
            Dim salesrep = (From s In dbx.SalesReps _
                            Where s.ID.Equals(sCompany) _
                            Select s.Name).SingleOrDefault

            Return salesrep
        End Using

    End Function

End Class
