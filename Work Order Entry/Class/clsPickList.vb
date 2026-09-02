Public Class clsPickList

    Public Property itemCode As String = ""
    Public Property itemDesc As String = ""
    Public Property picker As String = ""
    Public Property wokrOrder As String = ""
    Public Property qtyOrder As Integer = 0
    Public Property qtyPrep As Integer = 0
    Public Property printDate As String = ""
    Public Property printTime As String = ""
    Public Property branch As String = ""
    Public Property OPIS As String = ""
    Public Property OPIS_Name As String = ""
    Public Property type As String = ""
    Public Property Customer As String = ""


    Public Shared Function getPickList(ByVal _orderid As Integer) As Object

        Using dbx = GetDB()
            Dim res = (From a In dbx.Orders
                       Join b In dbx.OrderEntries On a.ID Equals b.OrderID
                       Join c In dbx.Items On b.ItemID Equals c.ID
                       Where a.ID.Equals(_orderid)
                       Select New With {
                           .Description = c.Description.ToString() & c.ExtendedDescription.ToString(),
                           .Itemcode = c.ItemLookupCode,
                           .ItemID = c.ID,
                           .OrderID = a.ID,
                           .QtyOrder = b.QuantityOnOrder,
                           .Remarks = a.Comment}).ToList

            Return res
        End Using


    End Function

    Public Shared Function getOPIS(ByVal _OrderID As Integer) As String
        Try

            Using dbx = GetDB()
                Dim res = (From a In dbx.Queueings Where a.OrderID.Equals(_OrderID)
                               Select a.OPIS).SingleOrDefault
                Return res
            End Using

        Catch ex As Exception
            MessageBox.Show(ex.Message, "MESSAGE : ERROR 0016", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function getOPISname(ByVal uname As String) As String
        Try

            Using dbx = GetDB()
                Dim getname = (From a In dbx.SOD_WO_Users Where a.UserName.Equals(uname)
                               Select a.UserFullName).SingleOrDefault
                Return getname
            End Using

        Catch ex As Exception
            MessageBox.Show(ex.Message, "MESSAGE : ERROR 0016", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function


    Public Shared Function getQtyPrep(ByVal itemID As Integer, ByVal orderID As Integer) As Integer

        Try
            Using dbx = GetDB()
                Dim vqty = (From a In dbx.QueueingItems _
                                 Join b In dbx.Queueings On a.QueueingID Equals b.id Where a.ItemID.Equals(itemID) And b.OrderID.Equals(orderID) _
                                 Select a.QtyPre).ToList()(0)

                Return vqty
            End Using
        Catch ex As Exception
            Return 0
        End Try

    End Function

    Public Shared Function getCustomerName(ByVal OrderID As Integer) As String
        Try

            Using dbx = GetDB()
                Dim res = (From a In dbx.Orders Where a.ID.Equals(OrderID)
                           Join b In dbx.Customers On a.CustomerID Equals b.ID
                           Select b.Company).SingleOrDefault
                Return res
            End Using

        Catch ex As Exception
            '   MessageBox.Show("FROM : clsQueueing Class " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
            Return Nothing
        End Try

    End Function

    Public Shared Function orderExist(ByVal _orderId As String) As Boolean

        Using dbx = GetDB()
            Dim res = (From a In dbx.Orders Where a.ID.Equals(_orderId)
                       Select a).Count

            If res > 0 Then
                Return True
            Else
                Return False

            End If
        End Using

    End Function

    Public Shared Function GetOrderType(ByVal orderId As Integer) As Integer?
        Using dbx = GetDB()
            Return (From order In dbx.Orders
                    Where order.ID = orderId
                    Select CType(order.Type, Integer?)).SingleOrDefault()
        End Using
    End Function

End Class
