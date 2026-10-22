Public Class frmUserCustType
    Dim _add As New List(Of String)
    Dim _del As New List(Of String)
    Public _custypeid As Integer = 0

    Private Sub frmUserCustType_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gridUser.DataSource = clsUser.viewUser()
        LoadCustUser()
    End Sub

    Private Sub LoadCustUser()
        Dim query = (From a In db.SOD_WO_CustTypeUsers
                     Where a.CustTypeID.Equals(_custypeid)
                     Join b In db.SOD_WO_Users On a.USERID Equals b.UserID
                     Order By b.UserID
                     Select a.USERID, b.UserName, b.UserFullName).ToList

        TreeView1.Nodes.Clear()

        Dim name As String

        For Each n In query

            name = n.USERID & " - " & n.UserName & " - " & n.UserFullName
            Dim nodes As TreeNodeCollection = TreeView1.Nodes
            Dim root = New TreeNode(name)
            root.Name = name

            TreeView1.Nodes.Add(root)

        Next

        

    End Sub
    Private Sub gridUser_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles gridUser.DoubleClick
        FilterText()
    End Sub

    Private Sub FilterText()

        Try
            Dim str_opis_i, str_opis, str_opis_n, str As String

            str_opis_i = gridUser.Item(0, gridUser.CurrentRow.Index).Value
            str_opis = gridUser.Item(2, gridUser.CurrentRow.Index).Value
            str_opis_n = gridUser.Item(1, gridUser.CurrentRow.Index).Value

            str = str_opis_i & " - " & str_opis & " - " & str_opis_n

            Dim nodes As TreeNodeCollection = TreeView1.Nodes
            Dim root = New TreeNode(str)
            root.Name = str

            Dim node As TreeNodeCollection = TreeView1.Nodes
            Dim txt As String = String.Empty

            If Not TreeView1.Nodes.ContainsKey(str) Then
                TreeView1.Nodes.Add(root)
            End If
            'If txtSearch.Text <> String.Empty Then

            'End If

            'For Each n As TreeNode In TreeView1.Nodes
            '    txt = txt + n.Text + ","
            '    lastFilterTxt = n.Name
            'Next

            'Try
            '    sfilterTxt = txt.Substring(0, txt.Length - 1)
            'Catch ex As Exception

            'End Try

        Catch ex As Exception
            MessageBox.Show("FROM : frmUserCustType Form " & vbCrLf & vbCrLf & "REASON : " & ex.Message, "MESSAGE : ERROR 0001", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorCount = ErrorCount + 1
        End Try

    End Sub

    Private Sub TreeView1_NodeMouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeNodeMouseClickEventArgs) Handles TreeView1.NodeMouseDoubleClick

        RemoveCustTypeUser()

    End Sub


    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try

            _add.Clear()

            For Each n As TreeNode In TreeView1.Nodes
                _add.Add(Split(n.Name, " - ")(0))
            Next

            For Each x In _add
                clsUser.AddCustTypeUsers(_custypeid, x)
            Next

            MessageBox.Show("Sucess !", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try



    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        RemoveCustTypeUser()
    End Sub

    Private Sub RemoveCustTypeUser()
        If MsgBox("Confirm removed?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Message") = vbYes Then
            Dim _userid = Split(TreeView1.SelectedNode.Text, " - ")(0)

            TreeView1.Nodes.Remove(TreeView1.SelectedNode)
            clsUser.DelCustTypeUser(_custypeid, _userid)
        End If
    End Sub
End Class