Public Class frmActionConfirmation
    Public Sub New()
        InitializeComponent()
        ActiveControl = btnYes
    End Sub

    Public Sub New(question As String, caption As String)
        Me.New()
        lblQuestion.Text = question
        Text = caption
    End Sub

    Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean
        ' A native Yes/No prompt has no Cancel choice. Escape must not answer it.
        If keyData = Keys.Escape Then Return True
        Return MyBase.ProcessDialogKey(keyData)
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If e.CloseReason = CloseReason.UserClosing AndAlso
           DialogResult <> DialogResult.Yes AndAlso DialogResult <> DialogResult.No Then
            e.Cancel = True
        End If
        MyBase.OnFormClosing(e)
    End Sub
End Class
