Public Class clsQueueing

    Public Shared Function LoadOpenWO(ByVal rType As String, ByVal pLoc As String) As Object

        Try
            Dim res = (From a In db.Queueings
                      Join b In db.Orders On a.OrderID Equals b.ID
                      Join c In db.QueueingItems On a.id Equals c.QueueingID
                 Where a.Status.Equals(0) And b.Comment.Contains(rType) And Not frmMain.groupIDList.Contains(a.GroupTo) And c.PickLoc.Equals(pLoc)
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

            Dim res = (From a In db.Queueings
                      Join b In db.Orders On a.OrderID Equals b.ID
                      Join c In db.QueueingItems On a.id Equals c.QueueingID
                        Where a.Status.Equals(0) And b.Comment.Contains(rType) And c.PickLoc.Equals(pLoc)
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
                Select New With {a.GroupTo, a.id, a.OrderID, b.ItemID, .ItemCode = c.ItemLookupCode, .Description = c.Description + c.ExtendedDescription.ToString, b.QtyPre, b.Picker, b.Status, b.QueueingID, a.OPIS}).Distinct.ToList
            db.Refresh(Data.Linq.RefreshMode.KeepChanges)

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function

    Public Shared Function CountOrderEntry(ByVal groupID As Integer, ByVal picLoc As String, ByVal rType As String) As Integer
        Try

            Dim res = (From a In db.Queueings
                 Join b In db.QueueingItems On a.id Equals b.QueueingID
                 Join c In db.Items On b.ItemID Equals c.ID
                  Join d In db.Orders On a.OrderID Equals d.ID
                 Where a.GroupTo = groupID And b.PickLoc.Equals(picLoc) And d.Comment.Contains(rType)
                Select New With {a.GroupTo, a.id, a.OrderID, b.ItemID, .ItemCode = c.ItemLookupCode, .Description = c.Description + c.ExtendedDescription.ToString, b.QtyPre, b.Picker, b.Status, b.QueueingID}).Count
            db.Refresh(Data.Linq.RefreshMode.KeepChanges)

            Return res
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
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

    Public Shared Function LoadPickers()
        Try

            Dim pp = (From a In db.PickerLists
                 Where a.RegisterNo.Equals(DB_RegNo)
                 Select a.RegisterNo, a.Initial).ToList
            Return pp
        Catch ex As Exception
            MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            checkForErrors()
            Return Nothing
        End Try

    End Function

    Public Shared Sub UpdatePicker(ByVal queueid As Integer, ByVal itemid As Integer, ByVal picker As String)

        Try

            Dim upd = (From a In db.QueueingItems
                  Where a.QueueingID.Equals(queueid) And a.ItemID.Equals(itemid)).ToList

            For Each x In upd
                x.Picker = picker
            Next

            db.SubmitChanges()

            If picker = String.Empty Then
                UpdateStatus(queueid, itemid, "-")
            Else
                UpdateStatus(queueid, itemid, "Processing")
            End If

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

End Class


