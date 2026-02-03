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


            Dim item = (From i In db.SOD_fntbl_NestedSearchItem(bEmployee, iPrice, sSearch) _
                        Select i.ItemLookUpCode, i.ItemName, i.MyPrice, i.Cost, i.AvailableQty, i.ParentQuantity, i.TaxID, _
                        i.SubDescription2, i.Price, i.ID, i.Description, i.ExtendedDescription, i.ItemType, i.Inactive, i.ItemDes, i.SKULevel _
                        Where Inactive = 0 Order By ItemLookUpCode.Substring(0, 5), SubDescription2).ToList
            Return item
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


            Dim item = (From i In db.SOD_fntbl_NestedSearchItem(bEmployee, iPrice, sSearch) _
                         Select i _
                         Where i.Inactive = False _
                         Order By _
                         i.Description).Count

            Return item
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

            Dim mask = (From c In db.SOD_WO_Confs Select c.PriceLevel).FirstOrDefault

            Dim query = (From L In db.SOD_fntbl_PriceLevel(sCode)
                         Select L)


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

            Dim item = (From i In db.SOD_fntbl_PriceLevel(sItemCode)).ToList

            For Each rs In item

                dPriceC = rs.PriceC

            Next

            Return dPriceC
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Sub uptSettings(ByVal iLimit As Integer, ByVal iLevel As String, ByVal iStoreID As Integer, ByVal allowprice As Boolean)

        Try



            Dim conf = (From c In db.SOD_WO_Confs).ToList()(0)

            conf.LimitEntry = iLimit
            conf.PriceLevel = iLevel
            conf.StoreID = iStoreID
            conf.AllowPriceChange = allowprice

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Public Shared Sub uptPass(ByVal iPass As String)

        Try


            Dim conf = (From c In db.SOD_WO_Confs).ToList()(0)

            conf.Password = iPass

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Public Shared Sub uptPass2(ByVal iPass2 As String)

        Try



            Dim conf = (From c In db.SOD_WO_Confs).ToList()(0)

            conf.Password2 = iPass2

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub


    Public Shared Sub uptPass3(ByVal iPass3 As String)

        Try



            Dim conf = (From c In db.SOD_WO_Confs).ToList()(0)

            conf.Password3 = iPass3

            Try
                db.SubmitChanges()
            Catch ex As Exception
                MsgBox("Error on Saving settings!" & vbNewLine & ex.ToString, vbExclamation, "Message")
            End Try

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Function ConfPass(ByVal spass As String) As Integer

        Try

            Dim conf = (From c In db.SOD_WO_Confs Where c.Password.Equals(spass) Or c.Password2.Equals(spass) Or c.Password3.Equals(spass)).Count

            Return conf
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function LoadSettings()

        Try


            Dim conf = (From i In db.SOD_WO_Confs).ToList

            Return conf
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try



    End Function

    Public Shared Function GetItemType(ByVal sItemCode As String) As Integer

        Try


            Dim sType = (From a In db.Items
                     Where a.ItemLookupCode.Equals(sItemCode)
                     Select a.ItemType).SingleOrDefault
            Return sType
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0012", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function PickUpToOtherWhse() As Integer

        Try


            Dim isAllow = (From a In db.SOD_WO_Confs
                         Select a.StoreID).SingleOrDefault
            Return isAllow
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0013", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function


    Public Shared Function getItemQty(ByVal itemid As String) As Integer

        Try

            Dim db3 = New ItemLookUpDataContext(DB_Conn("constr"))

            Dim available = 0
            Dim qty = (From a In db3.Items
                       Where a.ItemLookupCode.Equals(itemid)
                       Select New With {.Available = a.Quantity - a.QuantityCommitted})
            For Each x In qty
                available = x.Available
            Next

            Return available
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try


    End Function

    Public Shared Function getItemQtyCommitted(ByVal itemid As String) As Integer

        Try

            Dim db3 = New ItemLookUpDataContext(DB_Conn("constr"))

            Dim QuantityCommitted = 0
            Dim qty = (From a In db3.Items
                       Where a.ItemLookupCode.Equals(itemid)
                       Select New With {.Available = a.QuantityCommitted})
            For Each x In qty
                QuantityCommitted = x.Available
            Next

            Return QuantityCommitted
        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0014", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Sub setQuantityCommitted(ByVal itemid As Integer, ByVal qtyCommitted As Integer)

        Try

            Dim dbItem = New ItemLookUpDataContext(DB_Conn("constr"))

            Dim committed = (From i In dbItem.Items Where i.ID.Equals(itemid)
                             Select i).SingleOrDefault

            If Not committed.ItemType = 7 Then
                committed.QuantityCommitted = committed.QuantityCommitted + qtyCommitted
            End If

            Try
                dbItem.SubmitChanges()

            Catch ex As Exception
                '  MessageBox.Show(ex.Message)
            End Try


        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try
    End Sub

    Public Shared Sub SetQuantityCommittedToQoute(ByVal itemId As Integer, ByVal qtyCommitted As Integer)
        Try
            Using dbItem As New ItemLookUpDataContext(DB_Conn("constr"))

                Dim item = dbItem.Items.SingleOrDefault(Function(i) i.ID = itemId)
                'MessageBox.Show("qtyCommitted " & qtyCommitted)
                If item IsNot Nothing Then
                    ' Only commit if not ItemType 7
                    If item.ItemType <> 7 Then
                        item.QuantityCommitted += qtyCommitted
                        'MessageBox.Show("SetQuantityCommitted " & item.QuantityCommitted & " + " & qtyCommitted)
                    End If
                    Try
                        dbItem.SubmitChanges()
                    Catch ex As Exception
                    End Try
                Else
                    MessageBox.Show("Item with ID " & itemId & " not found.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If

            End Using

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class" & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0015", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount += 1
        End Try
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


            Dim item = (From a In db.Items Where a.Inactive.Equals(0) Where Not a.Description = String.Empty
                        Order By a.ItemLookupCode
                      Select New With { _
                          .Itemcode = a.ItemLookupCode, _
                          .Description = a.Description.ToString() & a.ExtendedDescription.ToString, _
                          .Price = a.Price, _
                          .Cost = a.Cost, _
                          .Available = a.Quantity - a.QuantityCommitted, _
                          .ParentQty = "", _
                          .TaxID = a.TaxID, _
                          .Subdesc = a.SubDescription2, _
                          .Price1 = a.Price, _
                          .ID = a.ID, _
                          .Description1 = a.Description, _
                          .ExtendedDescription = a.ExtendedDescription, _
                          .ItemType = a.ItemType, _
                          .Inactive = a.Inactive, _
                          .ItemDes = a.Description.ToString() & a.ExtendedDescription.ToString, _
                          .SKuLevel = a.SubDescription2, _
                          .FullDesc = a.ItemLookupCode.ToString & " | " & a.Description.ToString() & a.ExtendedDescription.ToString
                      }).ToList


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


            Dim item = (From a In dbnew.Items Where a.Inactive.Equals(0) Where Not a.Description = String.Empty
                        Order By a.ItemLookupCode
                      Select New With { _
                          .Itemcode = a.ItemLookupCode, _
                          .Description = a.Description.ToString() & a.ExtendedDescription.ToString, _
                          .Price = a.Price, _
                          .Cost = a.Cost, _
                          .Available = a.Quantity - a.QuantityCommitted, _
                          .ParentQty = "", _
                          .TaxID = a.TaxID, _
                          .Subdesc = a.SubDescription2, _
                          .Price1 = a.Price, _
                          .ID = a.ID, _
                          .Description1 = a.Description, _
                          .ExtendedDescription = a.ExtendedDescription, _
                          .ItemType = a.ItemType, _
                          .Inactive = a.Inactive, _
                          .ItemDes = a.Description.ToString() & a.ExtendedDescription.ToString, _
                          .SKuLevel = a.SubDescription2, _
                          .FullDesc = a.ItemLookupCode.ToString & " | " & a.Description.ToString() & a.ExtendedDescription.ToString
                      }).ToList


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


            Dim WoEntry = (From i In db.SOD_fntbl_WoEntry(orderid)
                           Select i).ToList

            Return WoEntry

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

            Dim WoDetails = (From i In db.SOD_fntbl_WoDetails(woid, woRegister, woCashier)).SingleOrDefault

            Branch = WoDetails.Branch
            Orderid = WoDetails.Orderid
            AccountNo = WoDetails.AccountNo
            Company = WoDetails.Company
            register = WoDetails.Register
            Cashier = WoDetails.Cashier
            OrderDate = WoDetails.OrderDate
            Ordertime = WoDetails.Ordertime
            Reference = WoDetails.Reference
            Comment = WoDetails.Comment
            Subtotal = WoDetails.Subtotal
            SalesTax = WoDetails.SalesTax
            Total = WoDetails.Total
            Address = WoDetails.Address

        Catch ex As Exception
            MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0019", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1

        End Try

    End Sub


    Public Shared Function getOrderForInvoicing(ByVal search As String, ByVal filter As String) As Object



        Dim ordr = (From a In db.SOD_ViewForInvoices Where a.Status.Equals("Prepared") And a.Orders.Contains(search) And a.OPIS.Equals(filter)
                  Select New With {
                      .Group = a.groupto,
                      .Status = a.Status,
                      .Orders = a.Orders}).ToList
        Return ordr

    End Function

    Public Shared Function printManualWO(ByVal OPIS As String, ByVal wo As Integer) As Boolean



        Dim ordrs = (From c In db.Queueings
                     Join d In db.QueueingItems On c.id Equals d.QueueingID
                     Where c.OrderID.Equals(wo) And c.OPIS.Equals(OPIS) And d.Status.Equals("For Invoicing")).Count

        If ordrs > 0 Then
            Return True
        Else
            Return False
        End If

    End Function


    Public Shared Sub UpdateQueueStatus(ByVal groupid As Integer, ByVal stat As String)

        Try


            Dim upt = (From a In db.QueueingItems
                Join b In db.Queueings On a.QueueingID Equals b.id
                Where b.GroupTo.Equals(groupid)
              Select a).ToList

            For Each x In upt
                x.Status = stat
            Next

            db.SubmitChanges()


        Catch ex As Exception
            MessageBox.Show(ex.Message, "Message!", MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try


    End Sub


    Public Shared Function checkTableUpdate() As String

        Try


            Dim upt = (From a In db.SOD_viewTableLastUpdates Where a.TableName.Equals("Item")
                       Select a.LastUpdate).SingleOrDefault

            If upt Is Nothing Then
                Return frmItemLookUp.db_lastupdate
            Else
                Return upt
            End If


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


            Dim item = (From a In db.SOD_viewLastUpdatedItems
                        Select a.ItemLookupCode, a.Available).ToList

            For Each i In item
                Dim dr As DataRow = dt3.NewRow()
                dr("Itemcode") = i.ItemLookupCode
                dr("Available") = i.Available
                dt3.Rows.Add(dr)
            Next

        Catch ex As Exception
            '   MessageBox.Show("FROM : clsItemLookUp Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0022", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Public Shared Function getStoreCode() As Object

        Dim code = (From a In db.Stores _
                    Select a.StoreCode).ToList()
        Return code

    End Function


    Public Shared Sub updateQueueStat(ByVal orderid, ByVal itemid)

        Dim x = (From a In db.Queueings
                    Join b In db.QueueingItems On a.id Equals b.QueueingID
                    Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                    Select a, b).Count

        If x = 1 Then

            Dim stat = (From a In db.Queueings
                    Join b In db.QueueingItems On a.id Equals b.QueueingID
                    Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                    Select a, b).SingleOrDefault

            stat.b.Picker = "CUST"
            stat.b.Status = "Prepared"
        Else

            Dim stat = (From a In db.Queueings
                    Join b In db.QueueingItems On a.id Equals b.QueueingID
                    Where (a.OrderID.Equals(orderid) And b.ItemID.Equals(itemid))
                    Select a, b).ToList

            For Each y In stat
                y.b.Picker = "CUST"
                y.b.Status = "Prepared"
            Next

        End If

        db.SubmitChanges()

    End Sub

    Public Shared Sub updateOrder(ByVal _OrderID As Integer)

        Dim up = (From a In db.Orders Where a.Equals(_OrderID) Select a).SingleOrDefault

        up.LastUpdated = Date.Now

        db.SubmitChanges()

    End Sub

End Class
