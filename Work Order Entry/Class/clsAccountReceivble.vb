Public Class clsAccountReceivable
    Public Shared Function getCreditLimit(ByVal sCompany As String) As Decimal
        ' Assuming you have a way to get the customer ID based on company
        Dim customerid = (From s In db.Customers
                          Where s.Company.Equals(sCompany)
                          Select s.ID).SingleOrDefault()

        ' Now, call the function to get the credit limit using the customer ID
        Dim creditLimit = db.SOD_fn_GetCreditLimit(customerid)

        Return creditLimit
    End Function

    Public Shared Function getOpenWO(ByVal sCompany As String) As Decimal
        ' Assuming you have a way to get the customer ID based on company
        Dim customerid = (From s In db.Customers
                          Where s.Company.Equals(sCompany)
                          Select s.ID).SingleOrDefault()

        ' Now, call the function to get the credit limit using the customer ID
        Dim OpenWO = db.SOD_fn_GetTotalOpenWorkOrder(customerid)

        Return OpenWO
    End Function


    Public Shared Function getCustomerCreditLimit(ByVal sCompany As String) As Integer

        Dim customerid = (From s In db.Customers
                          Where s.Company.Equals(sCompany)
                          Select s.CreditLimit).SingleOrDefault

        Return customerid

    End Function

End Class

