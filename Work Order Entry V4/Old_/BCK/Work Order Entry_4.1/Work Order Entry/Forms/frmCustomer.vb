Public Class frmCustomer

    Private Sub frmCustomer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Me.gridCustomer.DataSource = clsCustomer.SearchCustomer(txtCompany.Text)
            gridCustomerWidth()
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

        

    End Sub

    Private Sub gridCustomerWidth()

        Try

            gridCustomer.Columns(0).Width = 100
            gridCustomer.Columns(1).Width = 369
            gridCustomer.Columns(2).Visible = True
            gridCustomer.Columns(3).Visible = False
            gridCustomer.Columns(4).Visible = False
            gridCustomer.Columns(5).Visible = False
            gridCustomer.Columns(6).Visible = False
            gridCustomer.Columns(7).Visible = False
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0002", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub GetCustomerInfo(ByVal iRow As Integer)

        Try

            Dim i = gridCustomer.CurrentRow.Index
            sAcctNum = gridCustomer.Item(0, iRow).Value
            iPrice = gridCustomer.Item(2, iRow).Value
            bEmployee = gridCustomer.Item(3, iRow).Value
            sTitle = gridCustomer.Item(4, iRow).Value
            bTaxExcempt = gridCustomer.Item(5, iRow).Value
            iCusID = gridCustomer.Item(6, iRow).Value
            frmItemLookUp.txtCustomer.Text = gridCustomer.Item(1, iRow).Value
            frmItemLookUp.txtType.Text = gridCustomer.Item(7, iRow).Value

            Dim iSalesRep = clsCustomer.getPrimarySalesRepID(Convert.ToInt32(gridCustomer.Item(6, iRow).Value))

            iSalesID = iSalesRep

            frmItemLookUp.txtSales.Text = clsSalesRep.getPrimarySalesRepName(iSalesRep)

            'frmItemLookUp.txtSearch.Text = ""
            'frmItemLookUp.gridSelectItem.Rows.Clear()
            'frmItemLookUp.txtSub.Text = "0.00"
            'frmItemLookUp.txtVat.Text = "0.00"
            'frmItemLookUp.txtTotal.Text = "0.00"
            dTotalTSales = 0
            dTotalVSales = 0
            dTotalSales = 0

            Me.Close()
            frmItemLookUp.txtSearch.Focus()
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0003", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub
    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click

        Me.Dispose()

    End Sub

    Private Sub txtCompany_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCompany.KeyDown

        Try
            If e.KeyCode = Keys.Down Then
                gridCustomer.Focus()
            End If

        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0004", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

      
    End Sub

    Private Sub txtCompany_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCompany.KeyPress

        Try
            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then
                Me.gridCustomer.DataSource = clsCustomer.SearchCustomer(txtCompany.Text)
                gridCustomerWidth()
                gridCustomer.Focus()
            ElseIf e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then
                Me.Dispose()
            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0005", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

      

    End Sub

    Private Sub cmdOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOk.Click
        Try

            GetCustomerInfo(iRowCust)
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0006", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub gridCustomer_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles gridCustomer.CellDoubleClick

        Try
            GetCustomerInfo(iRowCust)
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0007", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Private Sub CallIndex()

        Try
            If gridCustomer.RowCount = 0 Then
                Exit Sub
            End If

            iRowCust = clsItemLookUp.RowIndex(gridCustomer)
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0008", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

      

    End Sub

    Private Sub gridCustomer_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles gridCustomer.Click

        Try
            CallIndex()
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0009", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try


    End Sub

    Private Sub gridCustomer_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles gridCustomer.KeyUp

        Try
            CallIndex()
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0010", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try



    End Sub

    Private Sub gridCustomer_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles gridCustomer.KeyPress

        Try
            If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Enter) Then

                iRowCust = clsItemLookUp.RowIndexEnter(gridCustomer, iRowCust)
                GetCustomerInfo(iRowCust)

            ElseIf e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Escape) Then
                Me.Dispose()

            End If
        Catch ex As Exception
            MessageBox.Show("FROM : frmCustomer Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0011", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

      

    End Sub

End Class
