Public Class clsWorkOrderDraft

    Private Shared Function ResolveRegisterID(Optional ByVal registerID As String = Nothing) As String
        Dim key As String = SafeText(registerID).Trim()
        If key <> String.Empty Then Return key

        key = SafeText(frmItemLookUp.usrRegister).Trim()
        If key <> String.Empty Then Return key

        key = SafeText(sReg).Trim()
        Return key
    End Function

    Private Shared Function ResolveRegisterIDs(Optional ByVal registerID As String = Nothing) As List(Of String)
        Dim keys As New List(Of String)

        AddRegisterKey(keys, registerID)
        AddRegisterKey(keys, frmItemLookUp.usrRegister)
        AddRegisterKey(keys, sReg)

        Return keys
    End Function

    Private Shared Sub AddRegisterKey(ByVal keys As List(Of String), ByVal value As Object)
        Dim key As String = SafeText(value).Trim()
        If key = String.Empty Then Exit Sub
        If Not keys.Contains(key) Then keys.Add(key)
    End Sub

    Private Shared Function ResolveUserName(Optional ByVal userName As String = Nothing) As String
        Dim key As String = SafeText(userName).Trim()
        If key <> String.Empty Then Return key

        key = SafeText(frmItemLookUp.usrUsername).Trim()
        Return key
    End Function

    Public Shared Function EnsureTables() As Boolean
        Try
            Using db = GetDB()
                db.SOD_WO_DraftHeaders.Take(1).ToList()
                ' Do not load whole detail rows here: Qty is mapped as float, and a
                ' store that has not run the draft migration still has an int column.
                db.SOD_WO_DraftDetails.Select(Function(d) d.DraftDetailID).Take(1).ToList()
            End Using
            Return True
        Catch
            ' Draft recovery is optional. A store that has not deployed the custom
            ' draft tables must still be able to create, update, and cancel orders.
            Return False
        End Try
    End Function

    Public Shared Function HasDraft(ByVal registerID As String, ByVal userName As String) As Boolean
        If Not EnsureTables() Then Return False

        Dim registerKey As String = ResolveRegisterID(registerID)
        If registerKey = String.Empty Then Return False
        Dim userKey As String = ResolveUserName(userName)
        If userKey = String.Empty Then Return False

        Using db = GetDB()
            Return (From h In db.SOD_WO_DraftHeaders
                    Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                    Select h.DraftID).Any()
        End Using
    End Function

    ' A draft has no order number, so saving one built from an existing order
    ' would insert a duplicate. Older versions saved these drafts on close.
    Public Shared Function IsDraftOfExistingOrder(ByVal registerID As String, ByVal userName As String) As Boolean
        If Not EnsureTables() Then Return False

        Dim registerKey As String = ResolveRegisterID(registerID)
        If registerKey = String.Empty Then Return False
        Dim userKey As String = ResolveUserName(userName)
        If userKey = String.Empty Then Return False

        Using db = GetDB()
            Return (From h In db.SOD_WO_DraftHeaders
                    Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                    From d In h.SOD_WO_DraftDetails
                    Where d.OrderEntryID > 0
                    Select d.DraftDetailID).Any()
        End Using
    End Function

    Public Shared Function GetDraftSummary(ByVal registerID As String, ByVal userName As String) As String
        If Not EnsureTables() Then Return "Draft recovery is not installed"

        Dim registerKey As String = ResolveRegisterID(registerID)
        If registerKey = String.Empty Then Return "Unsaved work order"
        Dim userKey As String = ResolveUserName(userName)
        If userKey = String.Empty Then Return "Unsaved work order"

        Using db = GetDB()
            Dim summary = (From h In db.SOD_WO_DraftHeaders
                           Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                           Order By h.UpdatedAt Descending
                           Select New With {
                               .UpdatedAt = h.UpdatedAt,
                               .CustomerName = h.CustomerName,
                               .ItemCount = h.SOD_WO_DraftDetails.Count
                           }).FirstOrDefault()

            If summary IsNot Nothing Then
                Return summary.ItemCount.ToString() & " item(s), customer: " &
                       SafeText(summary.CustomerName) & ", last saved: " &
                       summary.UpdatedAt.ToString("g")
            End If
        End Using

        Return "Unsaved work order"
    End Function

    Public Shared Sub DeleteDraft(ByVal registerID As String, ByVal userName As String)
        If Not EnsureTables() Then Exit Sub

        Dim registerKey As String = ResolveRegisterID(registerID)
        If registerKey = String.Empty Then Exit Sub
        Dim userKey As String = ResolveUserName(userName)
        If userKey = String.Empty Then Exit Sub

        Using db = GetDB()
            Dim drafts = (From h In db.SOD_WO_DraftHeaders
                          Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                          Select h).ToList()

            If drafts.Count = 0 Then Exit Sub

            db.SOD_WO_DraftHeaders.DeleteAllOnSubmit(drafts)
            db.SubmitChanges()
        End Using
    End Sub

    Public Shared Sub SaveDraft(ByVal form As frmItemLookUp)
        If Not EnsureTables() Then Exit Sub

        Dim registerKey As String = ResolveRegisterID()
        If String.IsNullOrWhiteSpace(registerKey) Then
            Throw New ApplicationException("Register ID is missing. Please log in again before saving a draft.")
        End If
        Dim userKey As String = ResolveUserName()
        If userKey = String.Empty Then
            Throw New ApplicationException("User name is missing. Please log in again before saving a draft.")
        End If

        If String.IsNullOrWhiteSpace(form.txtCustomer.Text) AndAlso
           String.IsNullOrWhiteSpace(form.txtSales.Text) AndAlso
           String.IsNullOrWhiteSpace(form.txtRemarks.Text) AndAlso
           String.IsNullOrWhiteSpace(form.cboPayment.Text) AndAlso
           Not form.gridSelectItem.Rows.Cast(Of DataGridViewRow)().Any(Function(r) Not r.IsNewRow AndAlso CellText(r, form.ItemCode.Index) <> String.Empty) Then
            DeleteDraft(registerKey, userKey)
            Exit Sub
        End If

        Using db = GetDB()
            Dim header = (From h In db.SOD_WO_DraftHeaders
                          Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                          Select h).FirstOrDefault()

            If header Is Nothing Then
                header = New SOD_WO_DraftHeader With {
                    .RegisterID = registerKey,
                    .UserName = userKey,
                    .CreatedAt = DateTime.Now
                }
                db.SOD_WO_DraftHeaders.InsertOnSubmit(header)
            Else
                header.RegisterID = registerKey
            End If

            If iCusID > 0 AndAlso SafeText(form.txtPriceLevel.Text) = String.Empty Then
                form.RestoreDraftHiddenFields()
            End If

            Dim draftPriceLevel As String = SafeText(form.txtPriceLevel.Text)
            If draftPriceLevel = String.Empty Then
                draftPriceLevel = ResolveCustomerPriceLevelText(iCusID)
            End If

            header.EntryType = form.getEntryType()
            header.CustomerID = iCusID
            header.CustomerName = SafeText(form.txtCustomer.Text)
            header.BusinessCustomerType = SafeText(form.txtType.Text)
            header.PaymentType = SafeText(form.cboPayment.Text)
            header.SalesRepID = iSalesID
            header.SalesRepName = SafeText(form.txtSales.Text)
            header.Remarks = SafeText(form.txtRemarks.Text)
            header.ReleaseType = SafeText(form.getReleaseType())
            header.PriceLevel = draftPriceLevel
            header.TaxExempt = bTaxExcempt
            header.ZeroRated = False
            header.UpdatedAt = DateTime.Now

            db.SubmitChanges()

            ' Deleted in SQL rather than loaded first, so an unmigrated int Qty column
            ' is never read through the float mapping.
            db.ExecuteCommand("DELETE FROM dbo.SOD_WO_DraftDetail WHERE DraftID = {0}", header.DraftID)

            Dim lineNo As Integer = 0
            For Each row As DataGridViewRow In form.gridSelectItem.Rows
                If row.IsNewRow Then Continue For
                If CellText(row, form.ItemCode.Index) = String.Empty Then Continue For

                Dim detail As New SOD_WO_DraftDetail With {
                    .DraftID = header.DraftID,
                    .LineNo = lineNo,
                    .ItemCode = CellText(row, form.ItemCode.Index),
                    .ItemName = CellText(row, form.ItemName.Index),
                    .Qty = CellDouble(row, form.QTY.Index),
                    .Price = CellDecimal(row, form.Price.Index),
                    .Disc = CellDecimal(row, form.DISC.Index),
                    .TotalPrice = CellDecimal(row, form.TOTAL.Index),
                    .LessVat = CellDecimal(row, form.LessV.Index),
                    .VatSales = CellDecimal(row, form.VSales.Index),
                    .DiscPrice = CellDecimal(row, form.DiscP.Index),
                    .Cost = CellDecimal(row, form.Cost.Index),
                    .Taxable = CellInt(row, form.Taxable.Index),
                    .ItemID = CellLong(row, form.ItemID.Index),
                    .FullPrice = CellDecimal(row, form.FullPrice.Index),
                    .Description = CellText(row, form.Description.Index),
                    .ExtendedDescription = CellText(row, form.Extended.Index),
                    .DiscAmount = CellDecimal(row, form.DiscAmount.Index),
                    .OrderEntryID = CellInt(row, form.OrderEntryID.Index),
                    .StoreWhse = CellBool(row, form.chkPickLoc.Index),
                    .PreparedQty = CellInt(row, form.CustPrep.Index),
                    .LastPurchasedPrice = CellDecimal(row, form.LASTPURCHASEDPRICE.Index),
                    .CreatedAt = DateTime.Now,
                    .SOD_WO_DraftHeader = header
                }

                db.SOD_WO_DraftDetails.InsertOnSubmit(detail)
                lineNo += 1
            Next

            db.SubmitChanges()
        End Using
    End Sub

    Public Shared Sub LoadDraft(ByVal form As frmItemLookUp)
        If Not EnsureTables() Then Exit Sub

        Dim registerKey As String = ResolveRegisterID()
        If String.IsNullOrWhiteSpace(registerKey) Then Exit Sub
        Dim userKey As String = ResolveUserName()
        If userKey = String.Empty Then Exit Sub

        'form.BeginDraftRestore()
        Try
            Using db = GetDB()
                Dim header = (From h In db.SOD_WO_DraftHeaders
                              Where h.RegisterID = registerKey AndAlso h.UserName = userKey
                              Order By h.UpdatedAt Descending
                              Select h).FirstOrDefault()

                If header Is Nothing Then Exit Sub

                iCusID = SafeInt(header.CustomerID)
                iSalesID = SafeInt(header.SalesRepID)

                form.txtCustomer.Text = SafeText(header.CustomerName)
                form.txtType.Text = SafeText(header.BusinessCustomerType)
                form.cboPayment.Text = SafeText(header.PaymentType)
                form.txtSales.Text = SafeText(header.SalesRepName)
                form.txtRemarks.Text = SafeText(header.Remarks)
                form.txtPriceLevel.Text = SafeText(header.PriceLevel)
                If SafeText(form.txtPriceLevel.Text) = String.Empty Then
                    form.txtPriceLevel.Text = ResolveCustomerPriceLevelText(iCusID)
                End If
                bTaxExcempt = header.TaxExempt
                If SafeText(form.txtPriceLevel.Text) = String.Empty Then
                    form.RestoreDraftHiddenFields()
                End If

                If SafeText(header.ReleaseType) = "Delivery" Then
                    form.rbtnDelivery.Checked = True
                Else
                    form.rbtnPickup.Checked = True
                End If

                Select Case SafeInt(header.EntryType)
                    Case 3
                        form.chkQuote.Checked = True
                        form.chkWorkOrder.Checked = False
                    Case 4
                        form.chkBoxQtoWo.Checked = True
                    Case Else
                        form.chkWorkOrder.Checked = True
                        form.chkQuote.Checked = False
                End Select

                form.gridSelectItem.Rows.Clear()

                ' CAST keeps this working whether Qty is still int (store not yet
                ' migrated) or float (migrated, keeps 2.5).
                Dim details = db.ExecuteQuery(Of DraftLine)(
                    "SELECT [LineNo], ItemCode, ItemName, CAST(Qty AS float) AS Qty, Price, Disc, TotalPrice, " &
                    "LessVat, VatSales, DiscPrice, Cost, Taxable, ItemID, FullPrice, Description, " &
                    "ExtendedDescription, DiscAmount, OrderEntryID, StoreWhse, PreparedQty, LastPurchasedPrice " &
                    "FROM dbo.SOD_WO_DraftDetail WHERE DraftID = {0} ORDER BY [LineNo]", header.DraftID).ToList()

                For Each detail In details
                    Dim rowIndex As Integer = form.gridSelectItem.Rows.Add()
                    Dim row As DataGridViewRow = form.gridSelectItem.Rows(rowIndex)

                    row.Cells(form.ItemCode.Index).Value = SafeText(detail.ItemCode)
                    row.Cells(form.ItemName.Index).Value = SafeText(detail.ItemName)
                    row.Cells(form.QTY.Index).Value = If(detail.Qty, 0R)
                    row.Cells(form.Price.Index).Value = SafeDecimal(detail.Price)
                    row.Cells(form.DISC.Index).Value = SafeDecimal(detail.Disc)
                    row.Cells(form.TOTAL.Index).Value = SafeDecimal(detail.TotalPrice)
                    row.Cells(form.LessV.Index).Value = SafeDecimal(detail.LessVat)
                    row.Cells(form.VSales.Index).Value = SafeDecimal(detail.VatSales)
                    row.Cells(form.DiscP.Index).Value = SafeDecimal(detail.DiscPrice)
                    row.Cells(form.Cost.Index).Value = SafeDecimal(detail.Cost)
                    row.Cells(form.Taxable.Index).Value = SafeInt(detail.Taxable)
                    row.Cells(form.ItemID.Index).Value = SafeLong(detail.ItemID)
                    row.Cells(form.FullPrice.Index).Value = SafeDecimal(detail.FullPrice)
                    row.Cells(form.Description.Index).Value = SafeText(detail.Description)
                    row.Cells(form.Extended.Index).Value = SafeText(detail.ExtendedDescription)
                    row.Cells(form.DiscAmount.Index).Value = SafeDecimal(detail.DiscAmount)
                    row.Cells(form.OrderEntryID.Index).Value = SafeInt(detail.OrderEntryID)
                    row.Cells(form.chkPickLoc.Index).Value = detail.StoreWhse
                    row.Cells(form.CustPrep.Index).Value = SafeInt(detail.PreparedQty)
                    row.Cells(form.LASTPURCHASEDPRICE.Index).Value = SafeDecimal(detail.LastPurchasedPrice)
                Next

                'form.ApplyDraftReleaseType(SafeText(header.ReleaseType))
                'form.RestoreDraftHiddenFields()

                form.ApplyCustomerTaxStatusToRows()
                form.UpdateAmt()
            End Using
        Finally
            'form.EndDraftRestore()
        End Try
    End Sub

    ' One saved draft line, read with explicit SQL (see LoadDraft).
    Public NotInheritable Class DraftLine
        Public Property LineNo As Integer
        Public Property ItemCode As String
        Public Property ItemName As String
        Public Property Qty As Double?
        Public Property Price As Decimal?
        Public Property Disc As Decimal?
        Public Property TotalPrice As Decimal?
        Public Property LessVat As Decimal?
        Public Property VatSales As Decimal?
        Public Property DiscPrice As Decimal?
        Public Property Cost As Decimal?
        Public Property Taxable As Integer?
        Public Property ItemID As Long?
        Public Property FullPrice As Decimal?
        Public Property Description As String
        Public Property ExtendedDescription As String
        Public Property DiscAmount As Decimal?
        Public Property OrderEntryID As Integer?
        Public Property StoreWhse As Boolean
        Public Property PreparedQty As Integer?
        Public Property LastPurchasedPrice As Decimal?
    End Class

    Private Shared Function CellText(ByVal row As DataGridViewRow, ByVal index As Integer) As String
        If row.Cells(index).Value Is Nothing Then Return String.Empty
        Return row.Cells(index).Value.ToString()
    End Function

    Private Shared Function CellInt(ByVal row As DataGridViewRow, ByVal index As Integer) As Integer
        Dim result As Integer = 0
        Integer.TryParse(CellText(row, index), result)
        Return result
    End Function

    ' Quantities can be fractional (2.5). Integer parsing turned 2.5 into 0.
    Private Shared Function CellDouble(ByVal row As DataGridViewRow, ByVal index As Integer) As Double
        Dim value = row.Cells(index).Value
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0R
        If TypeOf value Is Double OrElse TypeOf value Is Decimal OrElse TypeOf value Is Single OrElse
           TypeOf value Is Integer OrElse TypeOf value Is Long OrElse TypeOf value Is Short Then
            Return Convert.ToDouble(value)
        End If
        Dim result As Double = 0R
        Double.TryParse(value.ToString(), result)
        Return result
    End Function

    Private Shared Function CellLong(ByVal row As DataGridViewRow, ByVal index As Integer) As Long
        Dim result As Long = 0
        Long.TryParse(CellText(row, index), result)
        Return result
    End Function

    Private Shared Function CellDecimal(ByVal row As DataGridViewRow, ByVal index As Integer) As Decimal
        Dim result As Decimal = 0D
        Decimal.TryParse(CellText(row, index), result)
        Return result
    End Function

    Private Shared Function CellBool(ByVal row As DataGridViewRow, ByVal index As Integer) As Boolean
        Dim value = row.Cells(index).Value
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return CBool(value)
        Return SafeText(value).ToUpper() = "TRUE" OrElse SafeText(value) = "1"
    End Function

    Private Shared Function SafeText(ByVal value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return value.ToString()
    End Function

    Private Shared Function SafeInt(ByVal value As Object) As Integer
        Dim result As Integer = 0
        Integer.TryParse(SafeText(value), result)
        Return result
    End Function

    Private Shared Function SafeLong(ByVal value As Object) As Long
        Dim result As Long = 0
        Long.TryParse(SafeText(value), result)
        Return result
    End Function

    Private Shared Function SafeDecimal(ByVal value As Object) As Decimal
        Dim result As Decimal = 0D
        Decimal.TryParse(SafeText(value), result)
        Return result
    End Function

    Private Shared Function SafeBool(ByVal value As Object) As Boolean
        If value Is Nothing OrElse value Is DBNull.Value Then Return False
        Return Convert.ToBoolean(value)
    End Function

    Private Shared Function ResolveCustomerPriceLevelText(ByVal customerID As Integer) As String
        If customerID <= 0 Then Return String.Empty

        Try
            Dim priceLevelRaw As String = clsCustomer.getPriceLevel(customerID)
            Dim priceLevel As Integer

            If Integer.TryParse(priceLevelRaw, priceLevel) Then
                Select Case priceLevel
                    Case 0
                        Return "Price"
                    Case 1
                        Return "PriceA"
                    Case 2
                        Return "PriceB"
                    Case 3
                        Return "PriceC"
                End Select
            End If
        Catch
        End Try

        Return String.Empty
    End Function

End Class
