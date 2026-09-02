Imports Microsoft.VisualBasic.Devices

Public Class clsRecall



    Public Shared Function LoadOrders(ByVal iType As Integer) As DataTable

        If iType = 2 Then
            frmRecall.optWork.Checked = True
            frmRecall.optQuote.Checked = False
            frmRecall.optQuote.Enabled = False
        ElseIf iType = 3 Then
            frmRecall.optWork.Checked = False
            frmRecall.optQuote.Checked = True
            frmRecall.optWork.Enabled = False
        End If

        Dim dt1 As New DataTable
        Try
            dt1.Reset()

            dt1.Columns.Add("Order#", GetType(String))
            dt1.Columns.Add("Date", GetType(String))
            dt1.Columns.Add("Reference", GetType(String))
            dt1.Columns.Add("Customer", GetType(String))
            dt1.Columns.Add("Comment", GetType(String))


            Using dbx = GetDB()
                If frmRecall.CheckBox1.Checked Then

                    frmRecall.Text = "LIST OF OPEN ENTRIES"
                    Dim entry = From a In dbx.Orders
                                Where (a.Closed = 0 And a.Type = iType)
                                Order By a.ID Descending
                                Group Join b In dbx.Customers On a.CustomerID Equals b.ID Into Group
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

                    Dim entry = (From a In dbx.Orders
                                 Where a.Closed = 0 AndAlso a.Type = iType
                                 Order By a.ID Descending
                                 Group Join d In dbx.Customers On a.CustomerID Equals d.ID Into Group
                                 From d In Group.DefaultIfEmpty()
                                 Select New With {
                                     a.ID,
                                     a.Time,
                                     a.ReferenceNumber,
                                     d.AccountNumber,
                                     a.Comment
                                 }).ToList()

                    Dim queuedOrderIDs As New System.Collections.Generic.HashSet(Of Long)()
                    Dim userQueuedOrderIDs As New System.Collections.Generic.HashSet(Of Long)()

                    queuedOrderIDs = New System.Collections.Generic.HashSet(Of Long)(
                        (From queue In dbx.Queueings
                         Join currentOrder In dbx.Orders
                             On queue.OrderID Equals CType(currentOrder.ID, Long?)
                         Where queue.OrderID.HasValue AndAlso
                               currentOrder.Closed = 0 AndAlso
                               currentOrder.Type = iType
                         Select queue.OrderID.Value).ToList())
                    userQueuedOrderIDs = New System.Collections.Generic.HashSet(Of Long)(
                        (From queue In dbx.Queueings
                         Join currentOrder In dbx.Orders
                             On queue.OrderID Equals CType(currentOrder.ID, Long?)
                         Where queue.OrderID.HasValue AndAlso
                               currentOrder.Closed = 0 AndAlso
                               currentOrder.Type = iType AndAlso
                               queue.OPIS = frmItemLookUp.usrUsername
                         Select queue.OrderID.Value).ToList())

                    For Each i In entry
                        ' Direct work orders have no queue owner and remain visible.
                        ' Queued work orders retain the existing order-taker filter.
                        If queuedOrderIDs.Contains(CLng(i.ID)) AndAlso
                           Not userQueuedOrderIDs.Contains(CLng(i.ID)) Then Continue For

                        Dim dr As DataRow = dt1.NewRow()
                        dr("Order#") = i.ID
                        dr("Date") = i.Time
                        dr("Reference") = i.ReferenceNumber
                        dr("Customer") = i.AccountNumber
                        dr("Comment") = i.Comment

                        dt1.Rows.Add(dr)

                    Next

                End If
            End Using


            Return dt1

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing

        End Try

    End Function

    Public Shared Function LoadWO(ByVal str As String, ByVal parm As String) As Object


        Try

            Using dbx = GetDB()
                Dim wo = (From a In dbx.Orders
                          Where a.Closed = 0 And a.Type = 2
                          Order By a.ID Descending
                          Group Join b In dbx.Customers On a.CustomerID Equals b.ID Into Group
                          From b In Group.DefaultIfEmpty()
                          Select New With {a.ID, a.Time, a.ReferenceNumber, b.AccountNumber, a.Comment}).ToList()
                Return wo
            End Using
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
            Using dbx = GetDB()
                Dim quote = (From a In dbx.Orders
                             Join c In dbx.Customers On a.CustomerID Equals c.ID
                             Join e In dbx.SalesReps On a.SalesRepID Equals e.ID
                             Where a.Closed = 0 And a.ID = orderid
                             Select New With {c.Company, c.AccountNumber, c.PriceLevel, c.Employee, c.Title, .CustID = c.ID, c.CustomText5, c.TaxExempt,
    a.SalesRepID, .SaleRepName = e.Name, a.Comment, a.Type, a.DefaultTaxChangeReasonCodeID}).ToList
                Return quote
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    'Public Shared Function RecallOrderEntry(ByVal orderid As Integer)
    '    'Dim quote = (From a In db.Orders
    '    '             Join b In db.OrderEntries On a.ID Equals b.OrderID
    '    '             Join c In db.Customers On a.CustomerID Equals c.ID
    '    '             Join d In db.Items On b.ItemID Equals d.ID
    '    '             Join e In db.SalesReps On a.SalesRepID Equals e.ID
    '    '             Where a.Closed = 0 And a.ID = orderid
    '    '             Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder, _
    '    '                              b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12, .VSales = b.FullPrice - (b.FullPrice / 1.12), _
    '    '                              .DiscP = 0, b.Cost, b.Taxable, b.ItemID, b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, _
    '    '                              c.Company, c.AccountNumber, c.PriceLevel, c.Employee, c.Title, .CustID = c.ID, c.CustomText5, c.TaxExempt, _
    '    '                              a.SalesRepID, .SaleRepName = e.Name}).ToList

    '    'Return quote


    '    Try


    '        'Dim quoteEntry = (From b In db.OrderEntries
    '        '                  Join d In db.Items On b.ItemID Equals d.ID
    '        '                  Where b.OrderID = orderid
    '        '                  Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder,
    '        '                              b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12, .VSales = b.FullPrice - (b.FullPrice / 1.12),
    '        '                              .DiscP = 0, b.Cost, b.Taxable, b.ItemID, b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, b.ID, .Comment = b.Comment}).ToList

    '        'Return quoteEntry

    '        'Dim quoteEntry = (From b In db.OrderEntries
    '        '                  Join d In db.Items On b.ItemID Equals d.ID
    '        '                  Join c In db.QueueingItems On c.QueueingID Equals b.OrderID
    '        '                  Where b.OrderID = orderid
    '        '                  Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder,
    '        '                              b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12, .VSales = b.FullPrice - (b.FullPrice / 1.12),
    '        '                              .DiscP = 0, b.Cost, b.Taxable, b.ItemID, b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, b.ID, .Comment = b.Comment, c.PickLoc}).ToList

    '        'Return quoteEntry
    '        'Dim quoteEntry = (From b In db.OrderEntries
    '        '                  Join d In db.Items On b.ItemID Equals d.ID
    '        '                  Join c In db.QueueingItems On c.QueueingID Equals b.OrderID
    '        '                  Where b.OrderID = orderid
    '        '                  Select New With {d.ItemLookupCode, .ItemName = d.Description & d.ExtendedDescription.ToString, b.QuantityOnOrder,
    '        '                      b.Price, .DISC = 0, .Total = b.Price * b.QuantityOnOrder, .LessV = b.FullPrice / 1.12,
    '        '                      .VSales = b.FullPrice - (b.FullPrice / 1.12), .DiscP = 0, b.Cost, b.Taxable, b.ItemID,
    '        '                      b.FullPrice, d.Description, d.ExtendedDescription, .DiscAmount = 0, b.ID,
    '        '                      .Comment = b.Comment, c.PickLoc}).Distinct().ToList()
    '        'Return quoteEntry

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '        Return Nothing
    '    End Try


    'End Function

    'Public Shared Function RecallOrderEntry(ByVal orderid As Integer) As Object

    '    Try

    '        Dim quoteEntry = (From b In db.OrderEntries
    '                          Join d In db.Items On oe.ItemID Equals i.ID
    '                          Join c In (From q In db.QueueingItems
    '                                     Select New With {q.QueueingID, .PickLoc = q.PickLoc})
    '                          On b.OrderID Equals qi.QueueingID
    '                          Where b.OrderID = orderid
    '                          Select New With {
    '                              b.OrderID,
    '                              b.ItemID,
    '                              d.ID,
    '                              c.PickLoc
    '                          }).Distinct().ToList()

    '        Return quoteEntry


    '    Catch ex As Exception
    '        MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message,
    '                    "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount += 1
    '        Return Nothing
    '    End Try
    'End Function
    Public Shared Function RecallOrderEntry(ByVal orderid As Integer)
        Try
            Using dbx = GetDB()
                If StoreProcessingSettings.QueueingEnabled Then
                    Return (From oe In dbx.OrderEntries
                            Join i In dbx.Items On oe.ItemID Equals i.ID
                            Where oe.OrderID = orderid
                            Select New With {
                                i.ItemLookupCode,
                                .ItemName = i.Description & i.ExtendedDescription.ToString(),
                                oe.QuantityOnOrder,
                                oe.Price,
                                .DISC = 0,
                                .Total = oe.Price * oe.QuantityOnOrder,
                                .LessV = oe.FullPrice / 1.12,
                                .VSales = oe.FullPrice - (oe.FullPrice / 1.12),
                                .DiscP = 0,
                                oe.Cost,
                                oe.Taxable,
                                oe.ItemID,
                                oe.FullPrice,
                                i.Description,
                                i.ExtendedDescription,
                                .DiscAmount = 0,
                                oe.ID,
                                .Comment = oe.Comment,
                                .PickLoc = (From queueItem In dbx.QueueingItems
                                            Join queue In dbx.Queueings
                                                On queueItem.QueueingID Equals queue.id
                                            Where queue.OrderID = orderid AndAlso
                                                  queueItem.ItemID = oe.ItemID
                                            Select queueItem.PickLoc).FirstOrDefault()
                            }).Distinct().ToList()
                End If

                Return (From oe In dbx.OrderEntries
                        Join i In dbx.Items On oe.ItemID Equals i.ID
                        Where oe.OrderID = orderid
                        Select New With {
                            i.ItemLookupCode,
                            .ItemName = i.Description & i.ExtendedDescription.ToString(),
                            oe.QuantityOnOrder,
                            oe.Price,
                            .DISC = 0,
                            .Total = oe.Price * oe.QuantityOnOrder,
                            .LessV = oe.FullPrice / 1.12,
                            .VSales = oe.FullPrice - (oe.FullPrice / 1.12),
                            .DiscP = 0,
                            oe.Cost,
                            oe.Taxable,
                            oe.ItemID,
                            oe.FullPrice,
                            i.Description,
                            i.ExtendedDescription,
                            .DiscAmount = 0,
                            oe.ID,
                            .Comment = oe.Comment,
                            .PickLoc = CType(Nothing, String)
                        }).Distinct().ToList()
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message,
                    "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
            Return Nothing
        End Try
    End Function

    'comment
    Public Shared Sub UpdateOrder(ByVal iType As Integer, ByVal orderid As Integer, ByVal iCustID As Integer, ByVal iSalesRepID As Integer, ByVal dTax As Double, ByVal dTotal As Double, ByVal sComment As String)

        Try

            Using dbx = GetDB()
                Dim uptOrder = (From a In dbx.Orders
                                Where a.ID.Equals(orderid)).ToList()(0)
                uptOrder.Type = iType
                uptOrder.CustomerID = iCustID
                uptOrder.SalesRepID = iSalesRepID
                uptOrder.Tax = dTax
                uptOrder.Total = dTotal
                uptOrder.Comment = sComment
                uptOrder.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt")

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MessageBox.Show(ex.ToString, "Error updating sales order.", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
                End Try
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Sub UpdateOrderEntry(ByVal orderid As Integer, ByVal iItemID As Integer, ByVal iSalesRepID As Integer, ByVal dCost As Double, ByVal dFullPrice As Double, ByVal dPrice As Double, ByVal dQuantityOnOrder As Double, ByVal iTaxable As Integer, ByVal sDescription As String, ByVal sComment As String, ByVal qtyPrep As Integer, ByVal orderentryid As Integer)

        Try
            Using dbx = GetDB()

                Dim bExist = (From a In dbx.OrderEntries
                              Where a.OrderID.Equals(orderid) And a.ID.Equals(orderentryid)
                              Select a).Count

                Dim curType As Integer = (From b In dbx.Orders Where b.ID = orderid Select b.Type).SingleOrDefault()

                If bExist > 0 Then
                    Dim uptOrderEntry = (From a In dbx.OrderEntries
                                         Where a.OrderID.Equals(orderid) And a.ID.Equals(orderentryid)).ToList()(0)

                    Dim curQty As Double = uptOrderEntry.QuantityOnOrder

                    uptOrderEntry.Cost = dCost
                    uptOrderEntry.FullPrice = dFullPrice
                    uptOrderEntry.Price = dPrice
                    uptOrderEntry.QuantityOnOrder = dQuantityOnOrder
                    uptOrderEntry.Description = sDescription
                    'uptOrderEntry.Comment = sComment
                    uptOrderEntry.Taxable = iTaxable
                    uptOrderEntry.SalesRepID = iSalesID
                    uptOrderEntry.LastUpdated = DateTime.Now

                    If curType = 2 Then

                        Using dbItem = GetDB()
                            Dim uptItem = (From a In dbItem.Items
                                           Where a.ID.Equals(iItemID)).SingleOrDefault

                            Dim oldqty = clsRecall.getWOCommittedQty(orderid, iItemID)

                            If Not uptItem.ItemType = 7 And frmItemLookUp.chkWorkOrder.Checked = True And frmItemLookUp.chkBoxQtoWo.Checked = False Then

                                clsItemLookUp.ApplyQuantityCommittedDifference(
                                    dbItem, iItemID, dQuantityOnOrder - oldqty)
                            End If

                            If Not uptItem.ItemType = 7 And frmItemLookUp.chkWorkOrder.Checked = True And frmItemLookUp.chkBoxQtoWo.Checked = True Then

                                clsItemLookUp.setQuantityCommitted(iItemID, dQuantityOnOrder)
                            End If

                            dbItem.SubmitChanges()
                        End Using

                        Dim queueid = (From a In dbx.Queueings Where a.OrderID.Equals(orderid)
                                       Select a.id).SingleOrDefault

                        Dim picLoc = (From a In dbx.QueueingItems Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(iItemID)
                                      Select a).ToList()(0)

                        picLoc.PickLoc = sComment
                        picLoc.QtyPre = qtyPrep


                    End If
                    dbx.SubmitChanges()
                Else
                    Dim iStoreID = (From a In dbx.Orders
                                    Where a.ID.Equals(orderid)
                                    Select a.StoreID).ToList()(0)

                    dbx.SOD_sp_InsertQouteEntry(dCost, orderid, iItemID, dFullPrice, dPrice, dQuantityOnOrder, iSalesRepID, iTaxable, sDescription, sComment, qtyPrep, frmItemLookUp.getEntryType())

                    If curType = 2 Then

                        Using dbItem = GetDB()
                            Dim uptItems = (From a In dbItem.Items
                                            Where a.ID.Equals(iItemID)).ToList()(0)
                            If Not uptItems.ItemType = 7 Then
                                clsItemLookUp.ApplyQuantityCommittedDifference(
                                    dbItem, iItemID, dQuantityOnOrder)
                            End If
                            dbItem.SubmitChanges()
                        End Using

                        dbx.Refresh(Data.Linq.RefreshMode.KeepChanges)
                        dbx.SubmitChanges()

                    End If

                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub


    Public Shared Sub DeleteOrderItem(ByVal orderid As Integer, ByVal aItems As List(Of Integer))

        Try
            Using dbx = GetDB()

                For Each ii As Integer In aItems
                    Dim i = ii
                    Dim items = From d In dbx.OrderEntries
                                Where (d.OrderID = orderid And d.ID = i)
                                Select d

                    Dim qItemID = (From a In dbx.OrderEntries Where (a.OrderID = orderid And a.ID = i)
                                   Select a.ItemID).SingleOrDefault

                    Dim Qitems = (From a In dbx.QueueingItems
                                  Join b In dbx.Queueings On a.QueueingID Equals b.id
                                  Where (b.OrderID.Equals(orderid) And a.ItemID = qItemID)
                                  Select a)

                    Dim curType As Integer = (From b In dbx.Orders Where b.ID = orderid Select b.Type).SingleOrDefault()

                    Dim dQuantityOnOrder As Double

                    For Each x As OrderEntry In items

                        If curType = 2 Then

                            dQuantityOnOrder = x.QuantityOnOrder
                            Using dbItem = GetDB()
                                Dim uptItems = (From a In dbItem.Items
                                                Where a.ID.Equals(x.ItemID)).ToList()(0)
                                If Not uptItems.ItemType = 7 Then
                                    clsItemLookUp.ApplyQuantityCommittedDifference(
                                        dbItem, x.ItemID, -dQuantityOnOrder)
                                End If
                                dbItem.SubmitChanges()
                            End Using

                            frmItemLookUp.InsertPriceLogs(x.OrderID, x.ItemID, x.FullPrice, x.Price, "", "Deleted")

                        End If

                        dbx.OrderEntries.DeleteOnSubmit(x)

                    Next

                    For Each y As QueueingItem In Qitems
                        dbx.QueueingItems.DeleteOnSubmit(y)
                    Next

                Next

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MessageBox.Show(ex.ToString, "Error deleting Order entry", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Sub CancelOrder(ByVal orderid As Integer)

        Try
            Using dbx = GetDB()
                If dbx.Connection.State = ConnectionState.Closed Then dbx.Connection.Open()

                Using cancelTransaction = dbx.Connection.BeginTransaction()
                    dbx.Transaction = cancelTransaction

                    Try
                        clsItemLookUp.AcquireOrderTransactionLock(dbx, orderid)

                        Dim order = (From candidate In dbx.Orders
                                     Where candidate.ID = orderid
                                     Select candidate).SingleOrDefault()

                        If order Is Nothing Then
                            Throw New InvalidOperationException("Order " & orderid & " was not found.")
                        End If

                        Dim orderEntries = (From candidate In dbx.OrderEntries
                                            Where candidate.OrderID = orderid
                                            Select candidate).ToList()

                        For Each entry In orderEntries
                            If order.Type = 2 AndAlso entry.QuantityOnOrder <> 0R Then
                                clsItemLookUp.ApplyQuantityCommittedDifference(
                                    dbx, entry.ItemID, -entry.QuantityOnOrder)
                            End If

                            entry.QuantityOnOrder = 0R
                        Next

                        For Each queueHeader In (From candidate In dbx.Queueings
                                                 Where candidate.OrderID = orderid
                                                 Select candidate).ToList()
                            queueHeader.Status = 2
                        Next

                        order.Closed = True
                        dbx.SubmitChanges()
                        cancelTransaction.Commit()
                    Catch
                        cancelTransaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using
        Catch ex As Exception
            ErrorCount = ErrorCount + 1
            Throw New InvalidOperationException(
                "Unable to cancel order " & orderid & " and release its committed quantities.", ex)
        End Try

    End Sub

    Public Shared Sub UpdateQTYCommitted(ByVal itemlookupcode As String, ByVal qty As Double)

        Try
            Using dbItem = GetDB()
                Dim item = (From candidate In dbItem.Items
                            Where candidate.ItemLookupCode = itemlookupcode
                            Select candidate).SingleOrDefault()

                If item Is Nothing Then
                    Throw New InvalidOperationException(
                        "Item " & itemlookupcode & " was not found while releasing committed quantity.")
                End If

                If item.ItemType <> 7 Then
                    clsItemLookUp.ApplyQuantityCommittedDifference(dbItem, item.ID, -qty)
                End If
            End Using
        Catch ex As Exception
            ErrorCount = ErrorCount + 1
            Throw New InvalidOperationException(
                "Unable to release committed quantity for item " & itemlookupcode & ".", ex)
        End Try

    End Sub


    Public Shared Function CommittedWO(ByVal orderid As Integer, ByVal itemid As Integer)

        Try
            Using dbx = GetDB()

                Dim exists = (From a In dbx.SOD_WOs Where a.ID = orderid).Count
                Dim qty
                If exists > 0 Then

                    qty = (From a In dbx.SOD_WOs
                           Where a.OrderID = orderid And a.Invoice = False And Not a.Status = 0 And a.ItemID = itemid
                           Select a.QtyOrder).Sum
                Else
                    Dim existsItem = (From a In dbx.OrderEntries Where a.OrderID = orderid And a.ItemID = itemid).Count

                    If existsItem > 0 Then
                        qty = (From a In dbx.Orders
                               Join b In dbx.OrderEntries On a.ID Equals b.OrderID
                               Where a.ID = orderid And a.Closed = 0 And b.ItemID = itemid
                               Select b.QuantityOnOrder).Sum
                    Else
                        qty = 0
                    End If
                End If
                Return qty
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function


    Public Shared Function getWOCommittedQty(ByVal orderid As Integer, ByVal itemid As Integer) As Double
        Try
            Using dbx = GetDB()
                Dim qty = (From o In dbx.OrderEntries Where o.OrderID.Equals(orderid) And o.ItemID.Equals(itemid) Select o.QuantityOnOrder).Sum
                Return qty
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Sub CancelQuotation(ByVal orderid As Integer)

        Try
            Using dbx = GetDB()

                Dim upd = (From a In dbx.Orders Where a.ID.Equals(orderid)).SingleOrDefault
                upd.Closed = True

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MessageBox.Show(ex.ToString, "Error Updating Quantity Committed", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error)
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsRecall Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub


    Public Shared Function getItemLastPriceold(ByVal sItemCode As String)
        Using dbx = GetDB()
            Dim query = (From a In dbx.TransactionEntries
                         Join b In dbx.Items On a.ItemID Equals b.ID
                         Where b.ItemLookupCode.Equals(sItemCode)
                         Order By a.TransactionTime Descending
                         Select a.Price).FirstOrDefault()

            Return query
        End Using
    End Function


    'Public Class ItemPriceInfo
    '    Public Property ItemID As Integer
    '    Public Property ItemName As String
    '    Public Property LastPrice As Decimal
    '    Public Property LastPurchaseDate As DateTime
    'End Class

    'Public Shared Function getItemLastPrice(ByVal customerId As Integer) As List(Of ItemPriceInfo)
    '    Dim result = (From t In db.Transactions
    '                  Join te In db.TransactionEntries On t.TransactionNumber Equals te.TransactionNumber
    '                  Join i In db.Items On te.ItemID Equals i.ID
    '                  Where t.CustomerID = customerId
    '                  Order By t.Time Descending
    '                  Select New With {
    '                      .ItemID = i.ID,
    '                      .ItemName = i.Description, i.ExtendedDescription,
    '                      .Price = te.Price,
    '                      .TransactionDate = t.Time
    '                  }).ToList()

    '    ' Group by ItemID to get the most recent price per item
    '    Dim lastPrices = result.
    '                     GroupBy(Function(x) x.ItemID).
    '                     Select(Function(g) New ItemPriceInfo With {
    '                         .ItemID = g.First().ItemID,
    '                         .ItemName = g.First().ItemName,
    '                         .LastPrice = g.First().Price,
    '                         .LastPurchaseDate = g.First().TransactionDate
    '                     }).ToList()

    '    Return lastPrices
    'End Function

    'Public Shared Function GetItemLastPrice(ByVal customerId As Integer, ByVal sItemCode As String) As Decimal
    '    ' This will fetch the latest price or return 0 if not found
    '    Dim lastPrice As Decimal? = (From t In db.Transactions
    '                                 Join te In db.TransactionEntries On t.TransactionNumber Equals te.TransactionNumber
    '                                 Join i In db.Items On te.ItemID Equals i.ID
    '                                 Where t.CustomerID = customerId And i.ItemLookupCode.Equals(sItemCode) And t.ReferenceNumber Like {"%CI%"} Or t.ReferenceNumber Like '%CI%'
    '                                 Order By t.Time Descending
    '                                 Select CType(te.Price, Decimal?)).FirstOrDefault()

    '    Return If(lastPrice.HasValue, lastPrice.Value, 0D)


    'End Function


    'Public Shared Function GetItemLastPrice(ByVal customerId As Integer, ByVal sItemCode As String) As Decimal
    '    ' This will fetch the latest price for CH or CI transactions or return 0 if not found
    '    Dim lastPrice As Decimal? = (From t In db.Transactions
    '                                 Join te In db.TransactionEntries On t.TransactionNumber Equals te.TransactionNumber
    '                                 Join i In db.Items On te.ItemID Equals i.ID
    '                                 Where t.CustomerID = customerId And i.ItemLookupCode.Equals(sItemCode) _
    '                               And (t.ReferenceNumber.StartsWith("CH") Or t.ReferenceNumber.StartsWith("CI"))
    '                                 Order By t.Time Descending
    '                                 Select CType(te.Price, Decimal?)).FirstOrDefault()

    '    Return If(lastPrice.HasValue, lastPrice.Value, 0D)
    'End Function

    Public Shared Function GetItemLastPrice(ByVal sCustomerText As String, ByVal sItemCode As String) As Decimal
        ' This will fetch the latest price for CH or CI transactions or return 0 if not found
        Using dbx = GetDB()
            Dim iCustomer As Integer = (From c In dbx.Customers Where c.Company.Equals(sCustomerText) Select c.ID).FirstOrDefault

            Dim lastPrice As Decimal? = (From t In dbx.Transactions
                                         Join te In dbx.TransactionEntries On t.TransactionNumber Equals te.TransactionNumber
                                         Join i In dbx.Items On te.ItemID Equals i.ID
                                         Where t.CustomerID = iCustomer And i.ItemLookupCode.Equals(sItemCode) _
                                       And (t.ReferenceNumber.StartsWith("CH") Or t.ReferenceNumber.StartsWith("CI"))
                                         Order By t.Time Descending
                                         Select CType(te.Price, Decimal?)).FirstOrDefault()

            Return If(lastPrice.HasValue, lastPrice.Value, 0D)
        End Using
    End Function

End Class
