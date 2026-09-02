Public Class clsCustomer

    'Public Shared Function SearchCustomer(ByVal sCompany As String) As Object
    '    Try

    '        Dim customer = (From c In db.Customers _
    '                      Where (c.Company.Contains(sCompany)) _
    '                      Select c.AccountNumber, c.Company, c.PriceLevel, c.Employee, c.Title, c.TaxExempt, c.ID, c.CustomText2, c.CustomText4 Order By _
    '                      Company).ToList
    '        Return customer
    '    Catch ex As Exception
    '        MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '        Return Nothing
    '    End Try

    'End Function


    Public Shared Function SearchCustomer(ByVal sSearch As String) As Object
        Try
            Using dbx = GetDB()
                Dim customer = (From c In dbx.Customers
                                Where c.Company.Contains(sSearch) _
                               OrElse c.AccountNumber.Contains(sSearch)
                                Order By c.Company
                                Select c.AccountNumber,
                                   c.Company,
                                   c.PriceLevel,
                                   c.Employee,
                                   c.Title,
                                   c.TaxExempt,
                                   c.ID,
                                   c.CustomText2,
                                   c.CustomText4).ToList()

                Return customer
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf &
                        "REASON : " & ex.Message,
                        "MESSAGE : ERROR 0001",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
            ErrorCount += 1
            Return Nothing
        End Try
    End Function


    Public Shared Function getPrimarySalesRepID(ByVal sCompany As Integer) As Integer
        Try
            Using dbx = GetDB()
                Dim salesrep = (From s In dbx.Customers
                                Where s.ID.Equals(sCompany)
                                Select s.SalesRepID).SingleOrDefault

                Return salesrep
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function loadCustomerOpenWo(ByVal custID As Integer, ByVal custOrderId As Integer) As Object

        Try
            Using dbx = GetDB()
                Dim orders = (From o In dbx.Queueings
                              Join x In dbx.Orders On o.OrderID Equals x.ID
                              Where o.Status.Equals(0) And x.CustomerID.Equals(custID) And Not o.OrderID.Equals(custOrderId)
                              Order By o.OrderID Ascending
                              Select New With {
                              .OrderID = o.GroupTo,
                              .WorkOrders = getOrderIDs(o.GroupTo)}).Distinct.ToList

                Return orders
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function



    Public Shared Function loadCustomerOpenWoFirst(ByVal custID As Integer, ByVal custOrderId As Integer) As String

        Try
            Using dbx = GetDB()
                Dim orders = (From o In dbx.Orders
                              Where o.Closed.Equals(0) And o.Type.Equals(2) And o.CustomerID.Equals(custID) And o.Comment.Contains("Pick-up") And Not o.ID.Equals(custOrderId)
                              Order By o.ID Ascending
                              Select o.ID).Distinct.ToList()(0)

                Return orders
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function getGroupID(ByVal OrderID As Integer) As String

        Using dbx = GetDB()
            Dim chk = (From a In dbx.Queueings Where a.OrderID.Equals(OrderID)
                       Select a.GroupTo).SingleOrDefault
            Return chk
        End Using

    End Function

    Public Shared Function checkCustomerOpenWo(ByVal custID As Integer, ByVal custOrderId As Integer) As Boolean

        Try
            Using dbx = GetDB()
                Dim orders = (From o In dbx.Queueings
                              Join x In dbx.Orders On o.OrderID Equals x.ID
                              Where o.Status.Equals(0) And x.CustomerID.Equals(custID) And Not o.OrderID.Equals(custOrderId)
                              Select New With {
                             .OrderID = o.id,
                             .Total = x.Total,
                             .Comment = x.Comment
                         }).Count

                If orders > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared updateQueuingError As Boolean


    Public Shared Sub updateQueuing(ByVal orderid As String, ByVal grouptoId As String)

        Try
            Using dbx = GetDB()

                Dim Order = (From a In dbx.Queueings Where a.OrderID.Equals(orderid)
                             Select a).SingleOrDefault

                Order.GroupTo = grouptoId

                dbx.SubmitChanges()
            End Using

            updateQueuingError = False

        Catch ex As Exception
            MessageBox.Show("WORK ORDER NOT FOUND! " & vbCrLf & vbCrLf & "FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            updateQueuingError = True
        End Try

    End Sub

    Public Shared Function getOrderIDs(ByVal groupid As Integer) As String


        Try
            Using dbx = GetDB()
                Dim res = ""
                Dim str = (From a In dbx.Queueings Where a.GroupTo.Equals(groupid)
                           Select a.OrderID).ToList

                For Each x In str
                    res = res & x & " / "
                Next

                Return res.Substring(0, res.Length - 3)
            End Using


        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            updateQueuingError = True
            Return Nothing
        End Try

    End Function

    Public Shared Function GetCustType() As Object

        Using dbx = GetDB()
            Dim query = (From a In dbx.SOD_WO_CustTypes
                         Select a.ID, a.CustType, a.PriceBound, a.PricePassAt).ToList

            Return query
        End Using

    End Function

    Public Shared Sub AddCustType(ByVal scust As String, ByVal sprice As String, ByVal spricepass As String)
        Try
            Using dbx = GetDB()
                Dim query As New SOD_WO_CustType With {
                    .CustType = scust,
                    .PriceBound = sprice,
                    .PricePassAt = spricepass
                    }


                dbx.SOD_WO_CustTypes.InsertOnSubmit(query)

                dbx.SubmitChanges()
            End Using

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Public Shared Sub UptCustType(ByVal typeid As Integer, ByVal scust As String, ByVal sprice As String, ByVal spricepass As String)
        Try
            Using dbx = GetDB()

                Dim query = (From a In dbx.SOD_WO_CustTypes Where a.ID.Equals(typeid)
                             Select a).SingleOrDefault

                query.CustType = scust
                query.PriceBound = sprice
                query.PricePassAt = spricepass

                dbx.SubmitChanges()
            End Using

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Shared Sub DelCustType(ByVal typeid As Integer)
        Try
            Using dbx = GetDB()
                Dim del = (From a In dbx.SOD_WO_CustTypes Where a.ID.Equals(typeid)
                           Select a).SingleOrDefault

                dbx.SOD_WO_CustTypes.DeleteOnSubmit(del)

                dbx.SubmitChanges()
            End Using

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Shared Function GetLevel(ByVal sprice As String) As Integer

        If sprice = "PRICE (RETAIL)" Then
            GetLevel = 0
        ElseIf sprice = "PRICE A (WHOLESALE)" Then
            GetLevel = 1
        ElseIf sprice = "PRICE B (D1)" Then
            GetLevel = 2
        ElseIf sprice = "PRICE C (D2)" Then
            GetLevel = 3
        ElseIf sprice = "COST" Then
            GetLevel = 4
        Else
            GetLevel = 0
        End If

        Return GetLevel

    End Function

    '----------Additional function to get the Pricelevel per Customer
    Public Shared Function getPriceLevel(ByVal sCustomer As String) As Integer
        Try
            Using dbx = GetDB()
                Dim price = (From c In dbx.Customers
                             Where c.ID.Equals(sCustomer)
                             Select c.PriceLevel).FirstOrDefault()
                Return price
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
            Return Nothing
        End Try
    End Function


    '--------Additional function to get the Price Amount per Customer
    Public Shared Function getPriceAmount(ByVal sItemcode As String, ByVal priceLevel As Integer) As Decimal
        Try
            Using dbx = GetDB()
                Dim item = (From i In dbx.Items
                            Where i.ItemLookupCode = sItemcode
                            Select i).FirstOrDefault()

                If item IsNot Nothing Then
                    Select Case priceLevel
                        Case 0
                            Return item.Price
                        Case 1
                            Return item.PriceA
                        Case 2
                            Return item.PriceB
                        Case 3
                            Return item.PriceC
                        Case Else
                            Return 0 ' Or handle as needed
                    End Select
                Else
                    Return 0 ' Or handle as needed
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsCustomer Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
            Return 0
        End Try
    End Function


    Public Shared Function getCustomerID(ByVal sCompanyId As String) As Integer

        Using dbx = GetDB()
            Dim customerid = (From s In dbx.Customers
                              Where s.Company.Equals(sCompanyId)
                              Select s.ID).SingleOrDefault

            Return customerid
        End Using

    End Function



End Class
