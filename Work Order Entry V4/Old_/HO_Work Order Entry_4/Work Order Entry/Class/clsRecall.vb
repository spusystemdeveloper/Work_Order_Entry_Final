Public Class clsRecall



    Public Shared Function LoadOrders(ByVal iType As Integer) As DataTable

        Dim dt1 As New DataTable
        Try
            dt1.Reset()

            dt1.Columns.Add("Order#", GetType(String))
            dt1.Columns.Add("Date", GetType(String))
            dt1.Columns.Add("Reference", GetType(String))
            dt1.Columns.Add("Customer", GetType(String))
            dt1.Columns.Add("Comment", GetType(String))


            If frmRecall.CheckBox1.Checked Then

                frmRecall.Text = "LIST OF OPEN ENTRIES"

                Dim entry = From a In db.Orders
                        Where (a.Closed = 0 And a.Type = iType)
                     Order By a.ID Descending
                     Group Join b In db.Customers On a.CustomerID Equals b.ID Into Group _
                     From b In Group.DefaultIfEmpty()
                     Select New With {a.ID, a.Time, a.ReferenceNumber, b.AccountNumber, a.Comment}


                For Each i In entry

                    Dim dr As DataRow = dt1.NewRow()
                    dr("Order#") = i.ID
                    dr("Date") = i.Time
                    dr("Reference") = i.ReferenceNumber
                    dr("Customer") = i.AccountNumber
                    dr("Comment") = i.Comment


                    dt1.Rows.Add(dr)

                Next

            Else

                frmRecall.Text = "LIST OF OPEN ENTRIES FOR ORDER TAKER : " & frmItemLookUp.usrUsername.ToUpper & " - " & frmItemLookUp.usrFullname.ToUpper

                Dim entry = From a In db.Orders
                            Join b In db.Queueings On a.ID Equals b.OrderID
                            Join c In db.SOD_WO_Users On b.OPIS Equals c.UserName
                    Where (a.Closed = 0 And a.Type = iType) And c.UserName.Equals(frmItemLookUp.usrUsername)
                     Order By a.ID Descending
                     Group Join d In db.Customers On a.CustomerID Equals d.ID Into Group _
                     From d In Group.DefaultIfEmpty()
                     Select New With {a.ID, a.Time, a.ReferenceNumber, d.AccountNumber, a.Comment}


                For Each i In entry

                    Dim dr As DataRow = dt1.NewRow()
                    dr("Order#") = i.ID
                    dr("Date") = i.Time
                    dr("Reference") = i.ReferenceNumber
                    dr("Customer") = i.AccountNumber
                    dr("Comment") = i.Comment


                    dt1.Rows.Add(dr)

                Next

            End If


            Return dt1

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing

        End Try

    End Function

    Public Shared Function LoadWO(ByVal str As String, ByVal parm As String) As Object


        Try
            Dim wo = From a In db.Orders
                 Where a.Closed = 0 And a.Type = 2
                 Order By a.ID Descending
                 Group Join b In db.Customers On a.CustomerID Equals b.ID Into Group _
                 From b In Group.DefaultIfEmpty()
                 Select New With {a.ID, a.Time, a.ReferenceNumber, b.AccountNumber, a.Comment}

            Return wo
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


        'Dim quote = (From q In db.Orders
        '             Join c In db.Customers On q.CustomerID Equals c.ID
        '             Where q.Closed = 0 And q.Type = 2
        '             Select q.ID, q.Time, q.ReferenceNumber, c.AccountNumber, q.Comment).ToList


    End Function


    Public Shared Function RecallOrder(ByVal orderid As Integer)

        Try
            Dim quote = (From a In db.Orders
                    Join c In db.Customers On a.CustomerID Equals c.ID
                    Join e In db.SalesReps On a.SalesRepID Equals e.ID
                    Where a.Closed = 0 And a.ID = orderid
                    Select New With {c.Company, c.AccountNumber, c.PriceLevel, c.Employee, c.Title, .CustID = c.ID, c.CustomText5, c.TaxExempt, _
                                     a.SalesRepID, .SaleRepName = e.Name, a.Comment, a.Type}).ToList

            Return quote

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function RecallOrderEntry(ByVal orderid As Integer)
        'Dim quote = (From a In db.Orders
        '             Join b In db.OrderEntries On a.ID Equals b.OrderID
        '             Join c In db.Customers On a.CustomerID Equals c.ID
        '             Join d In db.Items On b.ItemID Equals d.ID
        '             Join e In db.SalesReps On a.SalesRepID Equals e.ID
        '             Where a.Closed = 0 And a.ID = orderid
        '             Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder, _
        '                              b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12, .VSales = b.FullPrice - (b.FullPrice / 1.12), _
        '                              .DiscP = 0, b.Cost, b.Taxable, b.ItemID, b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, _
        '                              c.Company, c.AccountNumber, c.PriceLevel, c.Employee, c.Title, .CustID = c.ID, c.CustomText5, c.TaxExempt, _
        '                              a.SalesRepID, .SaleRepName = e.Name}).ToList

        'Return quote


        Try


            Dim quoteEntry = (From b In db.OrderEntries
                         Join d In db.Items On b.ItemID Equals d.ID
                         Where b.OrderID = orderid
                         Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder, _
                                          b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12, .VSales = b.FullPrice - (b.FullPrice / 1.12), _
                                          .DiscP = 0, b.Cost, b.Taxable, b.ItemID, b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, b.ID, .Comment = b.Comment}).ToList

            Return quoteEntry

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Sub UpdateOrder(ByVal orderid As Integer, ByVal iCustID As Integer, ByVal iSalesRepID As Integer, ByVal dTax As Double, ByVal dTotal As Double, ByVal sComment As String)

        Try

            Dim uptOrder = (From a In db.Orders
                           Where a.ID.Equals(orderid)).ToList()(0)

            uptOrder.CustomerID = iCustID
            uptOrder.SalesRepID = iSalesRepID
            uptOrder.Tax = dTax
            uptOrder.Total = dTotal
            uptOrder.Comment = sComment
            uptOrder.LastUpdated = DateValue(Now)


            Try
                db.SubmitChanges()
            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Error updating sales order.", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
            End Try
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Sub UpdateOrderEntry(ByVal orderid As Integer, ByVal iItemID As Integer, ByVal iSalesRepID As Integer, ByVal dCost As Double, ByVal dFullPrice As Double, ByVal dPrice As Double, ByVal dQuantityOnOrder As Integer, ByVal iTaxable As Integer, ByVal sDescription As String, ByVal sComment As String, ByVal qtyPrep As Integer, ByVal orderentryid As Integer)

        Try

            Dim bExist = (From a In db.OrderEntries
                         Where a.OrderID.Equals(orderid) And a.ID.Equals(orderentryid)
                         Select a).Count

            Dim curType As Integer = (From b In db.Orders Where b.ID = orderid Select b.Type).SingleOrDefault()

            If bExist > 0 Then

                Dim uptOrderEntry = (From a In db.OrderEntries
                                    Where a.OrderID.Equals(orderid) And a.ID.Equals(orderentryid)).ToList()(0)

                Dim curQty As Integer = uptOrderEntry.QuantityOnOrder

                uptOrderEntry.Cost = dCost
                uptOrderEntry.FullPrice = dFullPrice
                uptOrderEntry.Price = dPrice
                uptOrderEntry.QuantityOnOrder = dQuantityOnOrder
                uptOrderEntry.Description = sDescription
                uptOrderEntry.Comment = sComment
                uptOrderEntry.SalesRepID = iSalesID
                uptOrderEntry.LastUpdated = DateTime.Now

                If curType = 2 Then
                    Dim uptItem = (From a In db.Items
                                   Where a.ID.Equals(iItemID)).SingleOrDefault

                    Dim oldqty = clsRecall.getWOCommittedQty(orderid, iItemID)

                    uptItem.QuantityCommitted = (uptItem.QuantityCommitted - oldqty) + dQuantityOnOrder

                    Dim queueid = (From a In db.Queueings Where a.OrderID.Equals(orderid)
                              Select a.id).SingleOrDefault

                    Dim picLoc = (From a In db.QueueingItems Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(iItemID)
                                  Select a).ToList()(0)

                    picLoc.PickLoc = sComment
                    picLoc.QtyPre = qtyPrep

                End If


                db.SubmitChanges()
            Else
                Dim iStoreID = (From a In db.Orders
                              Where a.ID.Equals(orderid)
                              Select a.StoreID).ToList()(0)

                db.SOD_sp_InsertQouteEntry(dCost, orderid, iItemID, dFullPrice, dPrice, dQuantityOnOrder, iSalesRepID, iTaxable, sDescription, sComment, qtyPrep, frmItemLookUp.getEntryType())

                If curType = 2 Then

                    Dim uptItems = (From a In db.Items
                                   Where a.ID.Equals(iItemID)).ToList()(0)
                    Dim curCom As Integer = uptItems.QuantityCommitted

                    uptItems.QuantityCommitted = uptItems.QuantityCommitted + (dQuantityOnOrder)
                    db.Refresh(Data.Linq.RefreshMode.KeepChanges)
                    db.SubmitChanges()

                End If

            End If

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Public Shared Sub DeleteOrderItem(ByVal orderid As Integer, ByVal aItems As List(Of Integer))
        Try
            For Each ii As Integer In aItems

                Dim i = ii

                Dim items = From d In db.OrderEntries
                            Where (d.OrderID = orderid And d.ID = i)
                            Select d

                Dim qItemID = (From a In db.OrderEntries Where (a.OrderID = orderid And a.ID = i)
                              Select a.ItemID).SingleOrDefault

                Dim Qitems = (From a In db.QueueingItems
                             Join b In db.Queueings On a.QueueingID Equals b.id
                             Where (b.OrderID.Equals(orderid) And a.ItemID = qItemID)
                             Select a)

                Dim curType As Integer = (From b In db.Orders Where b.ID = orderid Select b.Type).SingleOrDefault()

                Dim dQuantityOnOrder As Integer

                For Each x As OrderEntry In items

                    If curType = 2 Then
                        dQuantityOnOrder = x.QuantityOnOrder
                        Dim uptItems = (From a In db.Items
                                       Where a.ID.Equals(x.ItemID)).ToList()(0)
                        Dim curCom As Integer = uptItems.QuantityCommitted

                        uptItems.QuantityCommitted = uptItems.QuantityCommitted - (dQuantityOnOrder)
                    End If
                    db.OrderEntries.DeleteOnSubmit(x)
                Next

                For Each y As QueueingItem In Qitems
                    db.QueueingItems.DeleteOnSubmit(y)
                Next

            Next

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Error deleting Order entry", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Sub CancelOrder(ByVal orderid As Integer)

        Try

            Dim orderentry = (From b In db.OrderEntries
                              Where b.OrderID = orderid).ToList

            For Each i In orderentry
                i.QuantityOnOrder = 0
            Next

            Dim order = (From a In db.Orders
                       Where a.ID = orderid).SingleOrDefault

            Dim upQty = (From a In db.OrderEntries Where a.OrderID.Equals(orderid)
                          Join b In db.Items On a.ItemID Equals b.ID
                          Select b.ItemLookupCode, a.QuantityOnOrder
                          ).ToList

            For Each x In upQty

                UpdateQTYCommitted(x.ItemLookupCode, x.QuantityOnOrder)

            Next

            Dim stat = (From a In db.Queueings Where a.OrderID.Equals(orderid)).ToList()(0)

            order.Closed = True

            stat.Status = 2

            Try
                db.SubmitChanges()


            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Error in cancelling Order", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Public Shared Sub UpdateQTYCommitted(ByVal itemlookupcode As String, ByVal qty As Integer)

        Try
            Dim update = (From c In db.Items
                Where c.ItemLookupCode.Equals(itemlookupcode)).ToList()(0)

            update.QuantityCommitted = update.QuantityCommitted - qty

            Try
                db.SubmitChanges()

            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Error Updating Quantity Committed", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
            End Try
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Public Shared Function CommittedWO(ByVal orderid As Integer, ByVal itemid As Integer)

        Try

            Dim exists = (From a In db.SOD_WOs Where a.ID = orderid).Count
            Dim qty
            If exists > 0 Then


                qty = (From a In db.SOD_WOs
                           Where a.OrderID = orderid And a.Invoice = False And Not a.Status = 0 And a.ItemID = itemid
                           Select a.QtyOrder).Sum
            Else
                Dim existsItem = (From a In db.OrderEntries Where a.OrderID = orderid And a.ItemID = itemid).Count

                If existsItem > 0 Then
                    qty = (From a In db.Orders
                                       Join b In db.OrderEntries On a.ID Equals b.OrderID
                                       Where a.ID = orderid And a.Closed = 0 And b.ItemID = itemid
                                           Select b.QuantityOnOrder).Sum
                Else
                    qty = 0
                End If

            End If

            Return qty

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function


    Public Shared Function getWOCommittedQty(ByVal orderid As Integer, ByVal itemid As Integer) As Integer

        Try

            Dim qty = (From o In db.OrderEntries Where o.OrderID.Equals(orderid) And o.ItemID.Equals(itemid) Select o.QuantityOnOrder).Sum
            Return qty

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Sub CancelQuotation(ByVal orderid As Integer)

        Try

            Dim upd = (From a In db.Orders Where a.ID.Equals(orderid)).SingleOrDefault
            upd.Closed = True

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MessageBox.Show(ex.ToString, "Error Updating Quantity Committed", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub
End Class
