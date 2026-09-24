Public Class clsQueueing

    Public Shared Function LoadOpenWO(ByVal rType As String, ByVal pLoc As String) As Object

        Try
            Dim cutoffDate As Date = DateTime.Today.AddDays(-60)
            Dim res = (From a In db.Queueings
                      Join b In db.Orders On a.OrderID Equals b.ID
                      Join c In db.QueueingItems On a.id Equals c.QueueingID
                 Where a.Status.Equals(0) AndAlso b.Comment.Contains(rType) AndAlso Not frmMain.groupIDList.Contains(a.GroupTo) AndAlso c.PickLoc.Equals(pLoc) AndAlso Not c.Status.Equals("Closed") AndAlso b.Time >= cutoffDate
                 Order By a.id Ascending
                Select a.GroupTo Distinct).ToList

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()

            Return Nothing
        End Try

    End Function

    Public Shared Function LoadOpenWOUpdate(ByVal rType As String, ByVal pLoc As String) As Object
        Try
            Dim cutoffDate As Date = DateTime.Today.AddDays(-60)
            Dim res = (From a In db.Queueings
                      Join b In db.Orders On a.OrderID Equals b.ID
                      Join c In db.QueueingItems On a.id Equals c.QueueingID
                         Where a.Status.Equals(0) AndAlso b.Comment.Contains(rType) AndAlso c.PickLoc.Equals(pLoc) AndAlso Not c.Status.Equals("Closed") AndAlso b.Time >= cutoffDate
                  Order By a.id Ascending
                 Select a.GroupTo Distinct).ToList

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function

    Public Shared Function getOrderTaker(ByVal opis As String) As String

        Dim res = (From a In db.SOD_WO_Users Where a.UserName.Equals(opis) Select a.UserFullName).SingleOrDefault

        Return res

    End Function

    Public Shared Function LoadOrders(ByVal groupID As Integer, ByVal picLoc As String, ByVal rType As String) As Object
        Try
            Dim res = (From a In db.Queueings
                 Join b In db.QueueingItems On a.id Equals b.QueueingID
                 Join c In db.Items On b.ItemID Equals c.ID
                 Join d In db.Orders On a.OrderID Equals d.ID
                 Where a.GroupTo = groupID And b.PickLoc.Equals(picLoc) And d.Comment.Contains(rType)
                 Order By b.id Ascending
                Select New With {a.GroupTo, a.id, a.OrderID, b.ItemID, .ItemCode = c.ItemLookupCode, .Description = c.Description + c.ExtendedDescription.ToString, b.QtyPre, b.Picker, b.Status, b.QueueingID, a.OPIS, .OrderTime = d.Time}).Distinct.ToList

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return New List(Of Object)()
        End Try

    End Function

    Public Shared Function CountOrderEntry(ByVal groupID As Integer, ByVal picLoc As String, ByVal rType As String) As Integer
        Try
            Dim res = (From a In db.Queueings
                 Join b In db.QueueingItems On a.id Equals b.QueueingID
                 Join d In db.Orders On a.OrderID Equals d.ID
                 Where a.GroupTo = groupID AndAlso b.PickLoc.Equals(picLoc) AndAlso d.Comment.Contains(rType)
                 Select b.id).Count()

            Return res
        Catch ex As Exception
            Return 0
        End Try

    End Function


    Public Shared Function countOrders(ByVal groupID As Integer) As Object

        Try

            Dim res = (From a In db.Queueings
                 Join b In db.QueueingItems On a.id Equals b.QueueingID
                 Join c In db.Items On b.ItemID Equals c.ID
                 Where a.GroupTo = groupID
                Select b.ItemID).Count

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try


    End Function

    Public Shared Function GetQtyOrd(ByVal iwo As Integer, ByVal itemid As Integer) As Integer

        Try

            Dim qty = (From a In db.Orders
                   Join b In db.OrderEntries On a.ID Equals b.OrderID
                   Where a.ID.Equals(iwo) And b.ItemID.Equals(itemid)
                   Select b.QuantityOnOrder).Sum

            If qty = Nothing Then
                Return 0
            Else
                Return qty
            End If


        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return 0
        End Try

    End Function

    Public Shared Function GetBatchQtyOrd(ByVal orderIDs As List(Of Integer)) As Dictionary(Of String, Integer)
        Dim dict As New Dictionary(Of String, Integer)()
        Try
            If orderIDs Is Nothing OrElse orderIDs.Count = 0 Then Return dict

            Dim entries = (From oe In db.OrderEntries
                           Where orderIDs.Contains(oe.OrderID)
                           Group By oe.OrderID, oe.ItemID Into Group
                           Select OrderID, ItemID, Qty = CInt(Group.Sum(Function(x) x.QuantityOnOrder))).ToList()

            For Each e In entries
                Dim key As String = e.OrderID.ToString() & "_" & e.ItemID.ToString()
                dict(key) = e.Qty
            Next
        Catch ex As Exception
        End Try
        Return dict
    End Function

    Public Shared Function LoadPickers()
        Try
            Dim regInt As Integer = 0
            Dim hasReg As Boolean = Integer.TryParse(If(DB_RegNo, ""), regInt)

            Dim pp = (From a In db.PickerLists).ToList()

            If hasReg Then
                Dim filtered = pp.Where(Function(a) a.RegisterNo.HasValue AndAlso a.RegisterNo.Value = regInt).ToList()
                If filtered.Count > 0 Then
                    Return (From a In filtered Select a.RegisterNo, a.Initial, a.Name).ToList()
                End If
            End If

            ' Fallback: If no pickers matched the workstation register number, load all pickers
            Return (From a In pp Select a.RegisterNo, a.Initial, a.Name).ToList()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function

    Private Shared Sub TouchOrderLastUpdated(ByVal queueIDs As IEnumerable(Of Long))
        Try
            If queueIDs Is Nothing Then Exit Sub
            Dim qList = queueIDs.Distinct().ToList()
            If qList.Count = 0 Then Exit Sub

            Dim orderIDs = (From q In db.Queueings Where qList.Contains(q.id) Select q.OrderID).Distinct().ToList()
            Dim ordersToTouch = (From o In db.Orders Where orderIDs.Contains(o.ID)).ToList()
            For Each ord In ordersToTouch
                ord.LastUpdated = DateTime.Now
            Next
        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub UpdatePicker(ByVal queueid As Integer, ByVal itemid As Integer, ByVal picker As String)

        Try

            Dim upd = (From a In db.QueueingItems
                  Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(itemid)).ToList

            For Each x In upd
                x.Picker = picker
                If String.IsNullOrEmpty(picker) Then
                    x.Status = "-"
                Else
                    x.Status = "Processing"
                End If
            Next

            TouchOrderLastUpdated({CLng(queueid)})
            db.SubmitChanges()

        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Public Shared Sub UpdateBatchPicker(ByVal updates As List(Of Tuple(Of Long, Integer)), ByVal picker As String)
        Try
            If updates Is Nothing OrElse updates.Count = 0 Then Exit Sub

            Dim queueIDs = (From u In updates Select u.Item1).Distinct().ToList()
            Dim dbItems = (From a In db.QueueingItems
                           Where queueIDs.Contains(a.QueueingID.Value)).ToList()

            For Each u In updates
                Dim qid = u.Item1
                Dim iid = u.Item2
                For Each item In dbItems
                    If item.QueueingID.HasValue AndAlso item.QueueingID.Value = qid AndAlso item.ItemID.HasValue AndAlso item.ItemID.Value = iid Then
                        item.Picker = picker
                        If String.IsNullOrEmpty(picker) Then
                            item.Status = "-"
                        Else
                            item.Status = "Processing"
                        End If
                    End If
                Next
            Next

            TouchOrderLastUpdated(queueIDs)
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try
    End Sub

    Public Shared Sub UpdateQtyPrep(ByVal queueid As Integer, ByVal itemid As Integer, ByVal qty As String)
        Try

            Dim upd = (From a In db.QueueingItems
                  Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(itemid)).ToList()(0)

            upd.QtyPre = qty
            If upd.Status = "Shortage" OrElse upd.Status = "Check Stock" Then
                upd.Status = "Processing"
            End If
            TouchOrderLastUpdated({CLng(queueid)})
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Public Shared Sub UpdateBatchQtyPrep(ByVal updates As List(Of Tuple(Of Long, Integer, Integer)))
        Try
            If updates Is Nothing OrElse updates.Count = 0 Then Exit Sub

            Dim queueIDs = (From u In updates Select u.Item1).Distinct().ToList()
            Dim dbItems = (From a In db.QueueingItems
                           Where queueIDs.Contains(a.QueueingID.Value)).ToList()

            For Each u In updates
                Dim qid = u.Item1
                Dim iid = u.Item2
                Dim qty = u.Item3
                For Each item In dbItems
                    If item.QueueingID.HasValue AndAlso item.QueueingID.Value = qid AndAlso item.ItemID.HasValue AndAlso item.ItemID.Value = iid Then
                        item.QtyPre = qty
                    End If
                Next
            Next

            TouchOrderLastUpdated(queueIDs)
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
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
            '   MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
            checkForErrors()
        End Try

    End Function

    Public Shared Function getCustomerName(ByVal OrderID As Integer) As String
        Try

            Dim res = (From a In db.Orders Where a.ID.Equals(OrderID)
                       Join b In db.Customers On a.CustomerID Equals b.ID
                       Select b.Company).SingleOrDefault
            Return res

        Catch ex As Exception
            '   MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Sub UpdateStatus(ByVal queueid As Integer, ByVal itemid As Integer, ByVal stat As String)
        Try

            Dim upt = (From a In db.QueueingItems Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(itemid)
                 Select a).ToList

            For Each x In upt
                x.Status = stat
            Next
            TouchOrderLastUpdated({CLng(queueid)})
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try

    End Sub

    Public Shared Sub UpdateBatchStatus(ByVal updates As List(Of Tuple(Of Long, Integer)), ByVal stat As String)
        Try
            If updates Is Nothing OrElse updates.Count = 0 Then Exit Sub

            Dim queueIDs = (From u In updates Select u.Item1).Distinct().ToList()
            Dim dbItems = (From a In db.QueueingItems
                           Where queueIDs.Contains(a.QueueingID.Value)).ToList()

            For Each u In updates
                Dim qid = u.Item1
                Dim iid = u.Item2
                For Each item In dbItems
                    If item.QueueingID.HasValue AndAlso item.QueueingID.Value = qid AndAlso item.ItemID.HasValue AndAlso item.ItemID.Value = iid Then
                        item.Status = stat
                    End If
                Next
            Next

            TouchOrderLastUpdated(queueIDs)
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try
    End Sub

    Public Shared Sub UpdateQueueStatus(ByVal Groupid As Integer)
        Try

            Dim upt = (From a In db.Queueings Where a.GroupTo.Equals(Groupid)
                 Select a).ToList

            For Each x In upt
                x.Status = 1
            Next
            db.SubmitChanges()
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
        End Try


    End Sub


    Public Shared Function checkStatus(ByVal queueid As Integer, ByVal itemid As Integer) As String

        Try

            Dim upt = (From a In db.QueueingItems Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(itemid)
                           Select a.Status).SingleOrDefault

            Return upt
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try


    End Function

    Public Shared Function checkTableUpdate() As String

        Dim str = ""
        Try

            Dim upt = (From a In db.SOD_viewTableLastUpdates Where Not a.TableName.Equals("Item")
                      Select a).ToList

            'Dim upt = (From a In db.SOD_viewTableLastUpdates Where Not a.TableName.Equals("Item")
            '           Select a).ToList

            If upt Is Nothing Then
                Return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            Else
                For Each x In upt

                    str = str & String.Join("", x.TableName & "/" & x.LastUpdate & vbCrLf)
                Next

                Return str

            End If


        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
        End Try

    End Function


    Public Shared Function getPickerPass(ByVal initials As String) As String

        Try

            Dim pass = (From a In db.PickerLists Where a.Initial.Equals(initials)
                        Select a.Password).SingleOrDefault

            Return pass

        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0013", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try


    End Function


    Public Shared Sub checkForErrors()
        If ErrorCount > 5 Then
            ' Application.Restart()
        End If
    End Sub

    Public Shared Function getGroupID(ByVal search As String) As Object
        Try
            If IsNumeric(search) Then
                Dim ordId As Long = Convert.ToInt64(search)
                Dim g = (From a In db.Queueings Where a.OrderID = ordId Select a.GroupTo).FirstOrDefault()
                If g > 0 Then Return g
            End If

            Dim ordr = (From a In db.SOD_ViewForInvoices Where a.Orders.Contains(search)
                        Select a.groupto).ToList()(0)
            Return ordr

        Catch ex As Exception
            ' MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function

    ''Public Shared Function getLastUpdatedWo() As Object
    ''    Try
    ''        Dim x = (From a In db.SOD_Item_Logs Where a.Type.Equals("Order Update Logs")
    ''                    Order By a.Date Descending
    ''                    Select a.ReferenceID).ToList()(0)
    ''        Return x

    ''    Catch ex As Exception
    ''        MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
    ''        ErrorCount = ErrorCount + 1
    ''        checkForErrors()
    ''        Return Nothing
    ''    End Try

    'End Function


    Public Shared Function getLastUpdatedWo(ByVal rType As String) As Object
        Try

            Dim x = (From a In db.SOD_fntbl_LastupdateOrder(rType)
                        Select a.ID).ToList()
            Return x

        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function


    Public Shared Function getOPISname(ByVal uname As String) As String
        Try

            Dim getname = (From a In db.SOD_WO_Users Where a.UserName.Equals(uname)
                           Select a.UserFullName).SingleOrDefault
            Return getname

        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0016", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function getQtyPrep(ByVal itemID As Integer, ByVal orderID As Integer) As Integer

        Try
            Dim vqty = (From a In db.QueueingItems _
                             Join b In db.Queueings On a.QueueingID Equals b.id Where a.ItemID.Equals(itemID) And b.OrderID.Equals(orderID) _
                             Select a.QtyPre).ToList()(0)

            Return vqty
        Catch ex As Exception
            Return 0
        End Try

    End Function

    ' =========================================================================
    ' EXPERIMENTAL FEATURES HELPERS
    ' =========================================================================

    Public Shared Function GetCompletedTodayCount() As Integer
        Try
            Dim today = DateTime.Today
            Dim count = (From a In db.Queueings
                         Join b In db.Orders On a.OrderID Equals b.ID
                         Where b.Time >= today AndAlso (b.Closed = True OrElse a.Status = 1)
                         Select a.GroupTo).Distinct().Count()
            Return count
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Shared Function ReassignPickerBatch(ByVal fromPicker As String, ByVal toPicker As String) As Integer
        Try
            If String.IsNullOrEmpty(fromPicker) OrElse String.IsNullOrEmpty(toPicker) Then Return 0

            Dim items = (From qi In db.QueueingItems
                         Join q In db.Queueings On qi.QueueingID Equals q.id
                         Where qi.Picker.Equals(fromPicker) AndAlso
                               (qi.Status.Equals("In Process") OrElse qi.Status.Equals("Processing") OrElse qi.Status.Equals("Pending") OrElse qi.Status.Equals("Shortage") OrElse qi.Status.Equals("-"))
                         Select qi, q.OrderID, q.id).ToList()

            If items.Count = 0 Then Return 0

            Dim queueIDs As New HashSet(Of Long)()
            For Each it In items
                it.qi.Picker = toPicker
                queueIDs.Add(it.id)
            Next

            TouchOrderLastUpdated(queueIDs)
            db.SubmitChanges()

            Return items.Count
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Class PickerProductivityInfo
        Public Property Rank As Integer = 0
        Public Property PickerName As String = ""
        Public Property ItemsPreparedToday As Integer = 0
        Public Property OrdersCompletedToday As Integer = 0
    End Class

    Public Shared Function GetTodayPickerProductivity() As List(Of PickerProductivityInfo)
        Dim list As New List(Of PickerProductivityInfo)()
        Try
            Dim today = DateTime.Today
            Dim query = (From qi In db.QueueingItems
                         Join q In db.Queueings On qi.QueueingID Equals q.id
                         Join o In db.Orders On q.OrderID Equals o.ID
                         Where o.Time >= today AndAlso
                               (qi.Status.Equals("Prepared") OrElse qi.Status.Equals("For Invoicing") OrElse o.Closed = True) AndAlso
                               qi.Picker IsNot Nothing AndAlso qi.Picker <> "" AndAlso qi.Picker <> "-" AndAlso qi.Picker <> "Pick All"
                         Select qi.Picker, q.OrderID).ToList()

            Dim grouped = query.GroupBy(Function(x) x.Picker.Trim(), StringComparer.OrdinalIgnoreCase) _
                               .Select(Function(g) New PickerProductivityInfo With {
                                   .PickerName = g.Key,
                                   .ItemsPreparedToday = g.Count(),
                                   .OrdersCompletedToday = g.Select(Function(x) x.OrderID).Distinct().Count()
                               }) _
                               .OrderByDescending(Function(p) p.ItemsPreparedToday) _
                               .ToList()

            Dim r As Integer = 1
            For Each item In grouped
                item.Rank = r
                r += 1
                list.Add(item)
            Next
        Catch ex As Exception
            ' Silent guard
        End Try
        Return list
    End Function

    Public Shared Function FlagItemShortage(ByVal queueid As Long, ByVal itemid As Integer, ByVal qtyFound As Integer) As Boolean
        Try
            Dim items = (From qi In db.QueueingItems
                         Where qi.QueueingID.HasValue AndAlso qi.QueueingID.Value = queueid AndAlso
                               qi.ItemID.HasValue AndAlso qi.ItemID.Value = itemid
                         Select qi).ToList()

            If items.Count = 0 Then Return False

            For Each qi In items
                qi.QtyPre = qtyFound
                qi.Status = "Shortage"
            Next

            TouchOrderLastUpdated({queueid})
            db.SubmitChanges()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class


