Public Class clsItemLookUp

    Public Shared dt1 As New DataTable()
    Public Shared dt2 As New DataTable()
    Public Shared dt3 As New DataTable()


    Public Shared Function SearchItem(ByVal sSearch As String) As Object

        Try
            'Dim item = (From i In db.SOD_fntbl_SearchItem(bEmployee, iPrice, sSearch) _
            '              Select i _
            '              Where i.Inactive = 0 Order By i.ItemLookUpCode.Substring(0, 5), i.SubDescription2).ToList
            'Return item


            Using dbx = GetDB()
                Dim item = (From i In dbx.SOD_fntbl_NestedSearchItem(bEmployee, iPrice, sSearch)
                            Select i.ItemLookUpCode, i.ItemName, i.MyPrice, i.Cost, i.AvailableQty, i.ParentQuantity, i.TaxID,
                            i.SubDescription2, i.Price, i.ID, i.Description, i.ExtendedDescription, i.ItemType, i.Inactive, i.ItemDes, i.SKULevel
                            Where Inactive = 0 Order By ItemLookUpCode.Substring(0, 5), SubDescription2).ToList
                Return item
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try



    End Function

    Public Shared Function SearchItemExist(ByVal sSearch As String) As Integer

        Try
            'Dim item = (From i In db.SOD_fntbl_SearchItem(bEmployee, iPrice, sSearch) _
            '              Select i _
            '              Where i.Inactive = 0 _
            '              Order By _
            '              i.Description).Count

            'Return item


            Using dbx = GetDB()
                Dim item = (From i In dbx.SOD_fntbl_NestedSearchItem(bEmployee, iPrice, sSearch)
                            Select i
                            Where i.Inactive = False
                            Order By
                             i.Description).Count

                Return item
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Sub GetPrice(ByVal dQty As Double, ByVal iVtble As Integer, ByVal dPrice As Double)

        Try
            Dim dTotal As Double

            dTotal = 0
            dTSales = 0
            dVSales = 0
            dDisc = 0

            dNetPrice = 0
            dTotalTSales = 0
            dTotalVSales = 0
            dTotalSales = 0

            dPrice = dPrice * dQty

            If bTaxExcempt = True Then

                If iVtble = 1 Then

                    dTSales = dPrice / 1.12
                    dVSales = 0
                    dDisc = 0
                    dTotal = dTSales

                Else

                    dTSales = dPrice
                    dVSales = 0
                    dDisc = 0
                    dTotal = dTSales

                End If

            Else

                If iVtble = 1 Then

                    dTSales = dPrice / 1.12
                    dVSales = dPrice - dTSales
                    dDisc = 0
                    dTotal = dPrice

                Else

                    dTSales = dPrice
                    dVSales = 0
                    dDisc = 0
                    dTotal = dTSales

                End If

            End If

            dNetPrice = dTotal
            dTotalTSales = dTotalTSales + dTSales
            dTotalVSales = dTotalVSales + dVSales
            dTotalSales = dTotalTSales + dTotalVSales
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Public Shared Function TagSC(ByVal sTitle As String, ByVal sSC As String)

        Try

            If sTitle = "SC" Then

                If IsNumeric(sSC) = True Then
                    Return " - S"
                Else
                    Return ""
                End If

            Else

                Return ""

            End If
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function RowIndex(ByRef gridVal As DataGridView)

        Try
            Dim iRowDiff As Integer

            iRowDiff = (gridVal.RowCount - 1) - gridVal.CurrentRow.Index

            If iRowDiff = 1 Then
                Return IIf((gridVal.CurrentRow.Index - 1) < 0, 0, gridVal.CurrentRow.Index)
            Else
                Return gridVal.CurrentRow.Index
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try



    End Function

    Public Shared Function RowIndexEnter(ByRef gridVal As DataGridView, ByVal iRowVal As Integer)

        Try

            If gridVal.CurrentRow.Index = gridVal.RowCount - 1 Then

                If (gridVal.RowCount - 1) - iRowVal = 2 Then
                    Return gridVal.CurrentRow.Index - 1
                Else
                    Return iRowVal
                End If

            Else
                Return gridVal.CurrentRow.Index - 1
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    'Public Shared Sub LoadLevel(ByVal iLevel As Integer, ByVal sCode As String, ByVal iType As Integer)

    '    Try


    '        Dim sPriceType As String
    '        Dim dPrice As Double
    '        'Dim dPriceMask As Double


    '        dPrice = 0
    '        sPriceType = ""

    '        'd1
    '        Dim mask = (From c In db.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault

    '        Dim query = (From L In db.SOD_fntbl_PriceLevel(sCode) _
    '          Select L)


    '        For Each Level In query

    '            If iLevel = 0 Then
    '                sPriceType = "PRICE (RETAIL)"
    '                dPrice = Level.Price
    '                dLowest = Level.Price
    '            ElseIf iLevel = 1 Then
    '                sPriceType = "PRICE A (WHOLESALE)"
    '                dPrice = Level.PriceA
    '                dLowest = Level.PriceA
    '            ElseIf iLevel = 2 Then
    '                sPriceType = "PRICE B (D1)"
    '                dPrice = Level.PriceB
    '                dLowest = Level.PriceB
    '            ElseIf iLevel = 3 Then
    '                sPriceType = "PRICE C (D2)"
    '                dPrice = Level.PriceC
    '                dLowest = Level.PriceC
    '            ElseIf iLevel = 4 Then
    '                sPriceType = "COST"
    '                dPrice = Level.Cost
    '                dLowest = Level.Cost
    '            End If

    '            'If mask = "PRICE (RETAIL)" Then
    '            '    dPriceMask = Level.Price
    '            'ElseIf mask = "PRICE A (WHOLESALE)" Then
    '            '    dPriceMask = Level.PriceA
    '            'ElseIf mask = "PRICE B (D1)" Then
    '            '    dPriceMask = Level.PriceB
    '            'ElseIf mask = "PRICE C (D2)" Then
    '            '    dPriceMask = Level.PriceC
    '            'ElseIf mask = "NONE" Then
    '            '    dPriceMask = Level.PriceC
    '            'End If

    '            'dLowest = dPriceMask

    '            If iPricePass = 0 Then
    '                dLowest = Level.Price
    '            ElseIf iPricePass = 1 Then
    '                dLowest = Level.PriceA
    '            ElseIf iPricePass = 2 Then
    '                dLowest = Level.PriceB
    '            ElseIf iPricePass = 3 Then
    '                dLowest = Level.PriceC
    '            ElseIf iPricePass = 4 Then
    '                dLowest = Level.Cost
    '            End If

    '        Next

    '        Dim sLevel As String

    '        For Each n In clsItemLookUp.LoadSettings
    '            sLevel = n.PriceLevel
    '        Next

    '        'If sPriceType = sLevel Then
    '        '    bLevelFlag = True
    '        'End If

    '        If bLevelFlag = False Then
    '            If iType = 0 Then
    '                frmPriceLevel.DisplayPrice(sPriceType, dPrice)
    '            Else
    '                frmLinqLevel.DisplayPrice(sPriceType, dPrice)
    '            End If
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try



    'End Sub

    'Public Shared Sub LoadLevel(ByVal iLevel As Integer, ByVal sCode As String, ByVal iType As Integer)

    '    Try
    '        Dim sPriceType As String
    '        Dim dPrice As Double
    '        Dim dPriceMask As Double


    '        dPrice = 0
    '        sPriceType = ""

    '        Dim mask = (From c In db.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault

    '        Dim query = (From L In db.SOD_fntbl_PriceLevel(sCode)
    '                     Select L)


    '        For Each Level In query

    '            If iLevel = 0 Then
    '                sPriceType = "PRICE (RETAIL)"
    '                dPrice = Level.Price
    '            ElseIf iLevel = 1 Then
    '                sPriceType = "PRICE A (WHOLESALE)"
    '                dPrice = Level.PriceA
    '            ElseIf iLevel = 2 Then
    '                sPriceType = "PRICE B (D1)"
    '                dPrice = Level.PriceB
    '            ElseIf iLevel = 3 Then
    '                sPriceType = "PRICE C (D2)"
    '                dPrice = Level.PriceC
    '            End If


    '            If mask = "PRICE (RETAIL)" Then
    '                dPriceMask = Level.Price
    '            ElseIf mask = "PRICE A (WHOLESALE)" Then
    '                dPriceMask = Level.PriceA
    '            ElseIf mask = "PRICE B (D1)" Then
    '                dPriceMask = Level.PriceB
    '            ElseIf mask = "PRICE C (D2)" Then
    '                dPriceMask = Level.PriceC
    '            ElseIf mask = "NONE" Then
    '                'dPriceMask = Level.PriceC
    '            End If

    '            dLowest = dPriceMask

    '        Next

    '        Dim sLevel As String

    '        For Each n In clsItemLookUp.LoadSettings
    '            sLevel = n.PriceLevel
    '        Next

    '        'If sPriceType = sLevel Then
    '        '    bLevelFlag = True
    '        'End If

    '        If bLevelFlag = False Then
    '            If iType = 0 Then
    '                frmPriceLevel.DisplayPrice(sPriceType, dPrice)
    '            Else
    '                frmLinqLevel.DisplayPrice(sPriceType, dPrice)
    '            End If
    '        End If

    '    Catch ex As Exception
    '        MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '        ErrorCount = ErrorCount + 1
    '    End Try



    'End Sub
    Public Shared Sub LoadLevel(ByVal iLevel As Integer, ByVal sCode As String, ByVal iType As Integer)

        Try
            Dim sPriceType As String
            Dim dPrice As Double
            Dim dPriceMask As Double


            dPrice = 0
            sPriceType = ""

            Using dbx = GetDB()
                Dim mask = (From c In dbx.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault

                Dim query = (From L In dbx.SOD_fntbl_PriceLevel(sCode)
                             Select L).ToList


                For Each Level In query

                    If iLevel = 0 Then
                        sPriceType = "PRICE (RETAIL)"
                        dPrice = Level.Price
                    ElseIf iLevel = 1 Then
                        sPriceType = "PRICE A (WHOLESALE)"
                        dPrice = Level.PriceA
                    ElseIf iLevel = 2 Then
                        sPriceType = "PRICE B (D1)"
                        dPrice = Level.PriceB
                    ElseIf iLevel = 3 Then
                        sPriceType = "PRICE C (D2)"
                        dPrice = Level.PriceC
                    End If


                    If mask = "PRICE (RETAIL)" Then
                        dPriceMask = Level.Price
                    ElseIf mask = "PRICE A (WHOLESALE)" Then
                        dPriceMask = Level.PriceA
                    ElseIf mask = "PRICE B (D1)" Then
                        dPriceMask = Level.PriceB
                    ElseIf mask = "PRICE C (D2)" Then
                        dPriceMask = Level.PriceC
                    ElseIf mask = "NONE" Then
                        'dPriceMask = Level.PriceC
                    End If

                    dLowest = dPriceMask

                Next
            End Using

            Dim sLevel As String

            For Each n In clsItemLookUp.LoadSettings
                sLevel = n.PriceLevel
            Next

            'If sPriceType = sLevel Then
            '    bLevelFlag = True
            'End If

            If bLevelFlag = False Then
                If iType = 0 Then
                    frmPriceLevel.DisplayPrice(sPriceType, dPrice)
                Else
                    frmLinqLevel.DisplayPrice(sPriceType, dPrice)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub


    Public Shared Function SearchItemPriceC(ByVal sItemCode As String) As Double

        Try


            Dim dPriceC As Double

            Using dbx = GetDB()
                Dim item = (From i In dbx.SOD_fntbl_PriceLevel(sItemCode)).ToList

                For Each rs In item

                    dPriceC = rs.PriceC

                Next
            End Using

            Return dPriceC
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Sub uptSettings(ByVal iLimit As Integer, ByVal iLevel As String, ByVal iStoreID As Integer, ByVal allowprice As Boolean)

        Try



            Using dbx = GetDB()
                Dim conf = (From c In dbx.SOD_WO_Confs).ToList()(0)

                conf.LimitEntry = iLimit
                conf.PriceLevel = iLevel
                conf.StoreID = iStoreID
                conf.AllowPriceChange = allowprice

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Public Shared Sub uptPass(ByVal iPass As String)

        Try


            Using dbx = GetDB()
                Dim conf = (From c In dbx.SOD_WO_Confs).ToList()(0)

                conf.Password = iPass

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Public Shared Sub uptPass2(ByVal iPass2 As String)

        Try



            Using dbx = GetDB()
                Dim conf = (From c In dbx.SOD_WO_Confs).ToList()(0)

                conf.Password2 = iPass2

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub


    Public Shared Sub uptPass3(ByVal iPass3 As String)

        Try



            Using dbx = GetDB()
                Dim conf = (From c In dbx.SOD_WO_Confs).ToList()(0)

                conf.Password3 = iPass3

                Try
                    dbx.SubmitChanges()
                Catch ex As Exception
                    MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
                End Try
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Function ConfPass(ByVal spass As String) As Integer

        Try

            Using dbx = GetDB()
                Dim conf = (From c In dbx.SOD_WO_Confs Where c.Password.Equals(spass) Or c.Password2.Equals(spass) Or c.Password3.Equals(spass)).Count

                Return conf
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function LoadSettings()

        Try


            Using dbx = GetDB()
                Dim conf = (From i In dbx.SOD_WO_Confs).ToList

                Return conf
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try



    End Function

    Public Shared Function GetItemType(ByVal sItemCode As String) As Integer

        Try


            Using dbx = GetDB()
                Dim sType = (From a In dbx.Items
                             Where a.ItemLookupCode.Equals(sItemCode)
                             Select a.ItemType).SingleOrDefault
                Return sType
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function PickUpToOtherWhse() As Integer

        Try


            Using dbx = GetDB()
                Dim isAllow = (From a In dbx.SOD_WO_Confs
                               Select a.StoreID).SingleOrDefault
                Return isAllow
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0013", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function


    Public Shared Function getItemQty(ByVal itemid As String) As Integer

        Try

            Using dbx = GetDB()
                Dim available = 0
                Dim qty = (From a In dbx.Items
                           Where a.ItemLookupCode.Equals(itemid)
                           Select New With {.Available = a.Quantity - a.QuantityCommitted})
                For Each x In qty
                    available = x.Available
                Next

                Return available
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function getItemQtyCommitted(ByVal itemid As String) As Integer

        Try

            Using dbx = GetDB()
                Dim QuantityCommitted = 0
                Dim qty = (From a In dbx.Items
                           Where a.ItemLookupCode.Equals(itemid)
                           Select New With {.Available = a.QuantityCommitted})
                For Each x In qty
                    QuantityCommitted = x.Available
                Next

                Return QuantityCommitted
            End Using
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Sub setQuantityCommitted(ByVal itemid As Integer, ByVal qtyCommitted As Double)

        Try

            Using dbItem = GetDB()
                ApplyQuantityCommittedDifference(dbItem, itemid, qtyCommitted)
            End Using


        Catch ex As Exception
            ErrorCount = ErrorCount + 1
            Throw New InvalidOperationException("Unable to update committed quantity for item " & itemid & ".", ex)
        End Try
    End Sub

    Public Shared Sub SetQuantityCommittedToQoute(ByVal itemId As Integer, ByVal qtyCommitted As Double)
        Try
            Using dbItem = GetDB()
                ApplyQuantityCommittedDifference(dbItem, itemId, qtyCommitted)
            End Using

        Catch ex As Exception
            ErrorCount += 1
            Throw New InvalidOperationException("Unable to commit quotation quantity for item " & itemId & ".", ex)
        End Try
    End Sub

    Public Shared Sub ApplyQuantityCommittedDifference(ByVal dbContext As ItemLookUpDataContext,
                                                       ByVal itemId As Integer,
                                                       ByVal quantityDifference As Double)
        If dbContext Is Nothing Then Throw New ArgumentNullException(NameOf(dbContext))
        If quantityDifference = 0R Then Exit Sub

        ' SQL performs the arithmetic while holding the row update lock. Concurrent
        ' users add or subtract their own difference instead of overwriting a value
        ' that another Work Order transaction has just committed.
        Dim affectedRows As Integer = dbContext.ExecuteCommand(
            "UPDATE dbo.Item WITH (ROWLOCK) " &
            "SET QuantityCommitted = QuantityCommitted + {0} " &
            "WHERE ID = {1} AND ItemType <> 7 " &
            "AND QuantityCommitted + {0} >= 0",
            quantityDifference,
            itemId)

        If affectedRows = 1 Then Exit Sub

        Dim itemState = (From candidate In dbContext.Items
                         Where candidate.ID = itemId
                         Select New With {
                             candidate.ItemType,
                             candidate.QuantityCommitted
                         }).SingleOrDefault()

        If itemState Is Nothing Then
            Throw New InvalidOperationException(
                "Item " & itemId & " was not found while updating committed quantity.")
        End If

        If itemState.ItemType = 7 Then Exit Sub

        Throw New InvalidOperationException(
            "The committed-quantity adjustment for item " & itemId &
            " would make QuantityCommitted negative. Current value: " &
            itemState.QuantityCommitted.ToString() & ", adjustment: " &
            quantityDifference.ToString() & ".")
    End Sub

    Public Shared Sub AcquireOrderTransactionLock(ByVal dbContext As ItemLookUpDataContext,
                                                  ByVal orderId As Integer)
        If dbContext Is Nothing Then Throw New ArgumentNullException(NameOf(dbContext))
        If dbContext.Transaction Is Nothing Then
            Throw New InvalidOperationException(
                "An active database transaction is required before locking an order.")
        End If

        Dim lockResource As String = "SOD_WO_ORDER_" & orderId.ToString()
        Dim lockResult As Integer = dbContext.ExecuteQuery(Of Integer)(
            "DECLARE @LockResult int; " &
            "EXEC @LockResult = sys.sp_getapplock " &
            "@Resource = {0}, @LockMode = 'Exclusive', " &
            "@LockOwner = 'Transaction', @LockTimeout = 10000; " &
            "SELECT @LockResult;",
            lockResource).Single()

        If lockResult < 0 Then
            Throw New InvalidOperationException(
                "Order " & orderId &
                " is currently being changed by another user. Please recall the order and try again.")
        End If
    End Sub

    Public Shared Function getItemtoGrid() As DataTable

        Try
            dt1.Reset()

            dt1.Columns.Add("Itemcode", GetType(String))
            dt1.Columns.Add("Description", GetType(String))
            dt1.Columns.Add("Price", GetType(Double))
            dt1.Columns.Add("Cost", GetType(Double))
            dt1.Columns.Add("Available", GetType(Integer))
            dt1.Columns.Add("ParentQty", GetType(String))
            dt1.Columns.Add("TaxID", GetType(Integer))
            dt1.Columns.Add("Subdesc", GetType(String))
            dt1.Columns.Add("Price1", GetType(Double))
            dt1.Columns.Add("ID", GetType(Integer))
            dt1.Columns.Add("Description1", GetType(String))
            dt1.Columns.Add("ExtendedDescription", GetType(String))
            dt1.Columns.Add("ItemType", GetType(Integer))
            dt1.Columns.Add("Inactive", GetType(Integer))
            dt1.Columns.Add("ItemDes", GetType(String))
            dt1.Columns.Add("SKuLevel", GetType(String))
            dt1.Columns.Add("FullDesc", GetType(String))


            Dim item
            Using dbx = GetDB()
                item = (From a In dbx.Items Where a.Inactive.Equals(0) Where Not a.Description = String.Empty
                        Order By a.ItemLookupCode
                        Select New With {
                              .Itemcode = a.ItemLookupCode,
                              .Description = a.Description.ToString() & a.ExtendedDescription.ToString,
                              .Price = a.Price,
                              .Cost = a.Cost,
                              .Available = a.Quantity - a.QuantityCommitted,
                              .ParentQty = "",
                              .TaxID = a.TaxID,
                              .Subdesc = a.SubDescription2,
                              .Price1 = a.Price,
                              .ID = a.ID,
                              .Description1 = a.Description,
                              .ExtendedDescription = a.ExtendedDescription,
                              .ItemType = a.ItemType,
                              .Inactive = a.Inactive,
                              .ItemDes = a.Description.ToString() & a.ExtendedDescription.ToString,
                              .SKuLevel = a.SubDescription2,
                              .FullDesc = a.ItemLookupCode.ToString & " | " & a.Description.ToString() & a.ExtendedDescription.ToString
                          }).ToList
            End Using


            For Each i In item

                Dim dr As DataRow = dt1.NewRow()
                dr("Itemcode") = i.Itemcode
                dr("Description") = i.Description
                dr("Price") = i.Price
                dr("Cost") = i.Cost
                dr("Available") = i.Available
                dr("ParentQty") = i.ParentQty
                dr("TaxID") = i.TaxID
                dr("Subdesc") = i.Subdesc
                dr("Price1") = i.Price1
                dr("ID") = i.ID
                dr("Description1") = i.Description1
                dr("ExtendedDescription") = i.ExtendedDescription
                dr("ItemType") = i.ItemType
                dr("Inactive") = i.Inactive
                dr("ItemDes") = i.ItemDes
                dr("SKuLevel") = i.SKuLevel
                dr("FullDesc") = i.FullDesc

                dt1.Rows.Add(dr)

            Next

            Return dt1


        Catch ex As Exception
            ' MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0016", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function


    Public Shared Sub updateItemtoGrid()

        Try
            dt2.Reset()

            dt2.Columns.Add("Itemcode", GetType(String))
            dt2.Columns.Add("Description", GetType(String))
            dt2.Columns.Add("Price", GetType(Double))
            dt2.Columns.Add("Cost", GetType(Double))
            dt2.Columns.Add("Available", GetType(Integer))
            dt2.Columns.Add("ParentQty", GetType(String))
            dt2.Columns.Add("TaxID", GetType(Integer))
            dt2.Columns.Add("Subdesc", GetType(String))
            dt2.Columns.Add("Price1", GetType(Double))
            dt2.Columns.Add("ID", GetType(Integer))
            dt2.Columns.Add("Description1", GetType(String))
            dt2.Columns.Add("ExtendedDescription", GetType(String))
            dt2.Columns.Add("ItemType", GetType(Integer))
            dt2.Columns.Add("Inactive", GetType(Integer))
            dt2.Columns.Add("ItemDes", GetType(String))
            dt2.Columns.Add("SKuLevel", GetType(String))
            dt2.Columns.Add("FullDesc", GetType(String))


            Dim item
            Using dbx = GetDBNew()
                item = (From a In dbx.Items Where a.Inactive.Equals(0) Where Not a.Description = String.Empty
                        Order By a.ItemLookupCode
                        Select New With {
                              .Itemcode = a.ItemLookupCode,
                              .Description = a.Description.ToString() & a.ExtendedDescription.ToString,
                              .Price = a.Price,
                              .Cost = a.Cost,
                              .Available = a.Quantity - a.QuantityCommitted,
                              .ParentQty = "",
                              .TaxID = a.TaxID,
                              .Subdesc = a.SubDescription2,
                              .Price1 = a.Price,
                              .ID = a.ID,
                              .Description1 = a.Description,
                              .ExtendedDescription = a.ExtendedDescription,
                              .ItemType = a.ItemType,
                              .Inactive = a.Inactive,
                              .ItemDes = a.Description.ToString() & a.ExtendedDescription.ToString,
                              .SKuLevel = a.SubDescription2,
                              .FullDesc = a.ItemLookupCode.ToString & " | " & a.Description.ToString() & a.ExtendedDescription.ToString
                          }).ToList
            End Using


            For Each i In item
                Dim dr As DataRow = dt2.NewRow()
                dr("Itemcode") = i.Itemcode
                dr("Description") = i.Description
                dr("Price") = i.Price
                dr("Cost") = i.Cost
                dr("Available") = i.Available
                dr("ParentQty") = i.ParentQty
                dr("TaxID") = i.TaxID
                dr("Subdesc") = i.Subdesc
                dr("Price1") = i.Price1
                dr("ID") = i.ID
                dr("Description1") = i.Description1
                dr("ExtendedDescription") = i.ExtendedDescription
                dr("ItemType") = i.ItemType
                dr("Inactive") = i.Inactive
                dr("ItemDes") = i.ItemDes
                dr("SKuLevel") = i.SKuLevel
                dr("FullDesc") = i.FullDesc

                dt2.Rows.Add(dr)

            Next

        Catch ex As Exception
            '  MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0017", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Function viewWoEntry(ByVal orderid As Integer) As Object
        Try


            Using dbx = GetDB()
                Dim WoEntry = (From i In dbx.SOD_fntbl_WoEntry(orderid)
                               Select i).ToList

                Return WoEntry
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0018", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Address As String = ""
    Public Shared Branch As String = ""
    Public Shared Orderid As String = ""
    Public Shared AccountNo As String = ""
    Public Shared Company As String = ""
    Public Shared Register As String = ""
    Public Shared Cashier As String = ""
    Public Shared OrderDate As String = ""
    Public Shared Ordertime As String = ""
    Public Shared Reference As String = ""
    Public Shared Comment As String = ""
    Public Shared Subtotal As Double = 0
    Public Shared SalesTax As Double = 0
    Public Shared Total As Double = 0

    Public Shared Sub viewWoDetials(ByVal woid As Integer)

        Dim woRegister = frmItemLookUp.usrRegister
        Dim woCashier = frmItemLookUp.usrFullname

        Try

            Using dbx = GetDB()
                Dim WoDetails = (From i In dbx.SOD_fntbl_WoDetails(woid, woRegister, woCashier)).SingleOrDefault

                If WoDetails IsNot Nothing Then
                    Branch = WoDetails.Branch
                    Orderid = WoDetails.Orderid
                    AccountNo = WoDetails.AccountNo
                    Company = WoDetails.Company
                    Register = WoDetails.Register
                    Cashier = WoDetails.Cashier
                    OrderDate = WoDetails.OrderDate
                    Ordertime = WoDetails.Ordertime
                    Reference = WoDetails.Reference
                    Comment = WoDetails.Comment
                    Subtotal = WoDetails.Subtotal
                    SalesTax = WoDetails.SalesTax
                    Total = WoDetails.Total
                    Address = WoDetails.Address
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0019", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1

        End Try

    End Sub


    Public Shared Function getOrderForInvoicing(ByVal search As String, ByVal filter As String) As Object

        Using dbx = GetDB()
            Dim cutoffDate As DateTime = DateTime.Now.AddDays(-60)
            Dim ordr = (From a In dbx.SOD_ViewForInvoices
                        Join o In dbx.Orders On a.groupto Equals o.ID
                        Where a.Status.Equals("Prepared") AndAlso o.Closed = False AndAlso o.Time >= cutoffDate AndAlso a.Orders.Contains(search) AndAlso (String.IsNullOrEmpty(filter) OrElse a.OPIS.Equals(filter))
                        Order By o.Time Descending
                        Select New With {
                          .Group = a.groupto,
                          .Status = a.Status,
                          .Orders = a.Orders,
                          .Encoder = a.OPIS}).ToList()
            Return ordr
        End Using

    End Function

    Public Shared Function printManualWO(ByVal OPIS As String, ByVal wo As Integer) As Boolean



        Using dbx = GetDB()
            Dim ordrs = (From c In dbx.Queueings
                         Join d In dbx.QueueingItems On c.id Equals d.QueueingID
                         Where c.OrderID.Equals(wo) And c.OPIS.Equals(OPIS) And d.Status.Equals("For Invoicing")).Count

            If ordrs > 0 Then
                Return True
            Else
                Return False
            End If
        End Using

    End Function


    Public Shared Sub UpdateQueueStatus(ByVal groupid As Integer, ByVal stat As String)

        Try


            Using dbx = GetDB()
                Dim upt = (From a In dbx.QueueingItems
                           Join b In dbx.Queueings On a.QueueingID Equals b.id
                           Where b.GroupTo.Equals(groupid)
                           Select a).ToList

                For Each x In upt
                    x.Status = stat
                Next

                Dim orderIds = (From b In dbx.Queueings Where b.GroupTo.Equals(groupid) AndAlso b.OrderID.HasValue Select b.OrderID.Value).Distinct().ToList()
                Dim ordersToTouch = (From o In dbx.Orders Where orderIds.Contains(o.ID)).ToList()
                For Each ord In ordersToTouch
                    ord.LastUpdated = DateTime.Now
                Next

                dbx.SubmitChanges()
            End Using


        Catch ex As Exception
            MessageBox.Show(ex.Message, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try


    End Sub


    Public Shared Function checkTableUpdate() As String

        Try


            Using dbx = GetDB()
                Dim upt = (From a In dbx.SOD_viewTableLastUpdates Where a.TableName.Equals("Item")
                           Select a.LastUpdate).SingleOrDefault

                If upt Is Nothing Then
                    Return frmItemLookUp.db_lastupdate
                Else
                    Return upt
                End If
            End Using


        Catch ex As Exception
            '  MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0020", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1

            Return frmItemLookUp.db_lastupdate
        End Try

    End Function



    Public Shared Sub fakeUpdate(ByVal orderid As String)

        Try
            'fake update pra ma logs

            query("update OrderEntry set VoucherID = 0 " +
                  "where id = (select top 1 id from OrderEntry where orderid = " & orderid & " order by id desc)")

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0021", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Sub getLastUpdatedItems()
        Try
            dt3.Reset()

            dt3.Columns.Add("Itemcode", GetType(String))
            dt3.Columns.Add("Available", GetType(Integer))


            Using dbx = GetDB()
                Dim item = (From a In dbx.SOD_viewLastUpdatedItems
                            Select a.ItemLookupCode, a.Available).ToList

                For Each i In item
                    Dim dr As DataRow = dt3.NewRow()
                    dr("Itemcode") = i.ItemLookupCode
                    dr("Available") = i.Available
                    dt3.Rows.Add(dr)
                Next
            End Using

        Catch ex As Exception
            '   MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0022", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Function getStoreCode() As Object

        Using dbx = GetDB()
            Dim code = (From a In dbx.Stores
                        Select a.StoreCode).ToList()
            Return code
        End Using

    End Function


    Public Shared Sub updateQueueStat(ByVal orderid, ByVal itemid)

        Using dbx = GetDB()
            Dim x = (From a In dbx.Queueings
                     Join b In dbx.QueueingItems On a.id Equals b.QueueingID
                     Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                     Select a, b).Count

            If x = 1 Then

                Dim stat = (From a In dbx.Queueings
                            Join b In dbx.QueueingItems On a.id Equals b.QueueingID
                            Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                            Select a, b).SingleOrDefault

                stat.b.Picker = "CUST"
                stat.b.Status = "Prepared"
            Else

                Dim stat = (From a In dbx.Queueings
                            Join b In dbx.QueueingItems On a.id Equals b.QueueingID
                            Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                            Select a, b).ToList

                For Each y In stat
                    y.b.Picker = "CUST"
                    y.b.Status = "Prepared"
                Next

            End If

            dbx.SubmitChanges()
        End Using

    End Sub

    Public Shared Sub updateOrder(ByVal _OrderID As Integer)

        Using dbx = GetDB()
            Dim up = (From a In dbx.Orders Where a.Equals(_OrderID) Select a).SingleOrDefault

            up.LastUpdated = Date.Now

            dbx.SubmitChanges()
        End Using

    End Sub

End Class
