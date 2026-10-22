Public Class clsCustomer

    Public Shared Function SearchCustomer(ByVal sCompany As String) As Object
        Try

            Dim customer = (From c In db.Customers _
                          Where (c.Company.Contains(sCompany)) _
                          Select c.AccountNumber, c.Company, c.PriceLevel, c.Employee, c.Title, c.TaxExempt, c.ID, c.CustomText2 Order By _
                          Company).ToList
            Return customer
        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function getPrimarySalesRepID(ByVal sCompany As Integer) As Integer
        Try
            Dim salesrep = (From s In db.Customers _
                      Where s.ID.Equals(sCompany) _
                      Select s.SalesRepID).SingleOrDefault

            Return salesrep

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function loadCustomerOpenWo(ByVal custID As Integer, ByVal custOrderId As Integer) As Object

        Try
            Dim orders = (From o In db.Queueings
                           Join x In db.Orders On o.OrderID Equals x.ID
                           Where o.Status.Equals(0) And x.CustomerID.Equals(custID) And Not o.OrderID.Equals(custOrderId)
                           Order By o.OrderID Ascending
                      Select New With {
                          .OrderID = o.GroupTo,
                          .WorkOrders = getOrderIDs(o.GroupTo)}).Distinct.ToList

            Return orders

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function



    Public Shared Function loadCustomerOpenWoFirst(ByVal custID As Integer, ByVal custOrderId As Integer) As String

        Try
            Dim orders = (From o In db.Orders
                           Where o.Closed.Equals(0) And o.Type.Equals(4) And o.CustomerID.Equals(custID) And o.Comment.Contains("Pick-up") And Not o.ID.Equals(custOrderId)
                           Order By o.ID Ascending
                      Select o.ID).Distinct.ToList()(0)

            Return orders

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003.1", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function getGroupID(ByVal OrderID As Integer) As String

        Dim chk = (From a In db.Queueings Where a.OrderID.Equals(OrderID)
                   Select a.GroupTo).SingleOrDefault
        Return chk

    End Function

    Public Shared Function checkCustomerOpenWo(ByVal custID As Integer, ByVal custOrderId As Integer) As Boolean

        Try
            Dim orders = (From o In db.Queueings
                          Join x In db.Orders On o.OrderID Equals x.ID
                          Where o.Status.Equals(0) And x.CustomerID.Equals(custID) And Not o.OrderID.Equals(custOrderId)
                     Select New With {
                         .OrderID = o.id, _
                         .Total = x.Total, _
                         .Comment = x.Comment
                     }).Count

            If orders > 0 Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared updateQueuingError As Boolean


    Public Shared Sub updateQueuing(ByVal orderid As String, ByVal grouptoId As String)

        Try

            Dim Order = (From a In db.Queueings Where a.OrderID.Equals(orderid)
                       Select a).SingleOrDefault

            Order.GroupTo = grouptoId

            db.SubmitChanges()

            updateQueuingError = False

        Catch ex As Exception
            MessageBox.Show("WORK ORDER NOT FOUND! " & vbCrLf & vbCrLf & "FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            updateQueuingError = True
        End Try

    End Sub

    Public Shared Function getOrderIDs(ByVal groupid As Integer) As String


        Try
            Dim res = ""
            Dim str = (From a In db.Queueings Where a.GroupTo.Equals(groupid)
                       Select a.OrderID).ToList

            For Each x In str
                res = res & x & " / "
            Next

            Return res.Substring(0, res.Length - 3)

          
        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            updateQueuingError = True
            Return Nothing
        End Try
      
    End Function

End Class
