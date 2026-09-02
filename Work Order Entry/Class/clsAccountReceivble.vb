Public Class clsAccountReceivable
    Public Shared Function getCreditLimit(ByVal sCompany As String) As Decimal
        Using dbx = GetDB()
            ' Assuming you have a way to get the customer ID based on company
            Dim customerid = (From s In dbx.Customers
                              Where s.Company.Equals(sCompany)
                              Select s.ID).SingleOrDefault()

            ' Now, call the function to get the credit limit using the customer ID
            Dim creditLimit = dbx.SOD_fn_GetCreditLimit(customerid)

            Return creditLimit
        End Using
    End Function

    Public Shared Function getCreditLimit(ByVal customerID As Integer) As Decimal
        Using dbx = GetDB()
            Return Convert.ToDecimal(dbx.SOD_fn_GetCreditLimit(customerID))
        End Using
    End Function

    Public Shared Function getOpenWO(ByVal iCusID As Integer) As Decimal
        ' Assuming you have a way to get the customer ID based on company
        'Dim customerid = (From s In db.Customers
        '                  Where s.Company.Equals(sCompany)
        '                  Select s.ID).SingleOrDefault()

        Using dbx = GetDB()
            ' Now, call the function to get the credit limit using the customer ID
            Dim OpenWO = dbx.SOD_fn_GetTotalOpenWorkOrder(iCusID)

            Return OpenWO
        End Using
    End Function


    Public Shared Function getCustomerCreditLimit(ByVal customerID As Integer) As Decimal

        Using dbx = GetDB()
            Dim customerCreditValue = (From customer In dbx.Customers
                                       Where customer.ID = customerID
                                       Select Value = customer.CreditLimit).SingleOrDefault

            Return Convert.ToDecimal(customerCreditValue)
        End Using

    End Function

End Class

