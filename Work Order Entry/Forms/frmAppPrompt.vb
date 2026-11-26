Imports System.Drawing
Imports System.Windows.Forms

Public Class frmAppPrompt
    Private allowedResults As DialogResult() = {DialogResult.OK}

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(message As String, caption As String, buttons As MessageBoxButtons,
                   icon As MessageBoxIcon, defaultButton As MessageBoxDefaultButton)
        Me.New()
        Text = If(caption, "")
        txtMessage.Text = If(message, "")
        Dim captions As String()
        Select Case buttons
            Case MessageBoxButtons.OK
                captions = {"&OK"} : allowedResults = {DialogResult.OK}
            Case MessageBoxButtons.OKCancel
                captions = {"&OK", "&Cancel"} : allowedResults = {DialogResult.OK, DialogResult.Cancel}
            Case MessageBoxButtons.YesNo
                captions = {"&Yes", "&No"} : allowedResults = {DialogResult.Yes, DialogResult.No}
            Case MessageBoxButtons.YesNoCancel
                captions = {"&Yes", "&No", "&Cancel"} : allowedResults = {DialogResult.Yes, DialogResult.No, DialogResult.Cancel}
            Case MessageBoxButtons.RetryCancel
                captions = {"&Retry", "&Cancel"} : allowedResults = {DialogResult.Retry, DialogResult.Cancel}
            Case MessageBoxButtons.AbortRetryIgnore
                captions = {"&Abort", "&Retry", "&Ignore"} : allowedResults = {DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore}
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(buttons))
        End Select
        Dim choices = {btnFirst, btnSecond, btnThird}
        Dim cancelIndex = Array.IndexOf(allowedResults, DialogResult.Cancel)
        If buttons = MessageBoxButtons.OK Then cancelIndex = 0
        ControlBox = cancelIndex >= 0
        For index As Integer = 0 To choices.Length - 1
            choices(index).Visible = index < captions.Length
            If index < captions.Length Then
                choices(index).Text = captions(index)
                choices(index).DialogResult = allowedResults(index)
                choices(index).Left = ClientSize.Width - 16 - (captions.Length - index) * btnFirst.Width - (captions.Length - index - 1) * 10
            End If
        Next
        If cancelIndex >= 0 Then CancelButton = choices(cancelIndex)
        Dim selectedIndex = CInt(defaultButton) \ 256
        If selectedIndex < 0 OrElse selectedIndex >= captions.Length Then
            Throw New ArgumentOutOfRangeException(NameOf(defaultButton))
        End If
        AcceptButton = choices(selectedIndex)
        ActiveControl = choices(selectedIndex)

        Select Case icon
            Case MessageBoxIcon.Error
                lblHeading.Text = "Error" : lblHeading.ForeColor = Color.Firebrick
                picIcon.Image = SystemIcons.Error.ToBitmap()
            Case MessageBoxIcon.Warning
                lblHeading.Text = "Warning" : lblHeading.ForeColor = Color.FromArgb(133, 77, 14)
                picIcon.Image = SystemIcons.Warning.ToBitmap()
            Case MessageBoxIcon.Question
                lblHeading.Text = "Please confirm" : picIcon.Image = SystemIcons.Question.ToBitmap()
            Case MessageBoxIcon.Information
                lblHeading.Text = "Information" : picIcon.Image = SystemIcons.Information.ToBitmap()
            Case MessageBoxIcon.None
                lblHeading.Text = If(buttons = MessageBoxButtons.OK, "Message", "Please confirm")
                picIcon.Visible = False
                lblHeading.Left = 16
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(icon))
        End Select
        Dim measured = TextRenderer.MeasureText(txtMessage.Text, txtMessage.Font,
            New Size(txtMessage.Width, Integer.MaxValue), TextFormatFlags.WordBreak Or TextFormatFlags.TextBoxControl)
        ClientSize = New Size(ClientSize.Width, 130 + Math.Max(48, Math.Min(260, measured.Height + 12)))
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Dim area = Screen.FromControl(Me).WorkingArea
        Size = New Size(Math.Min(Width, area.Width - 24), Math.Min(Height, area.Height - 24))
    End Sub

    Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean
        If keyData = Keys.Escape AndAlso CancelButton Is Nothing Then Return True
        Return MyBase.ProcessDialogKey(keyData)
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If e.CloseReason = CloseReason.UserClosing AndAlso Array.IndexOf(allowedResults, DialogResult) < 0 Then
            If CancelButton Is Nothing Then
                e.Cancel = True
            Else
                DialogResult = CancelButton.DialogResult
            End If
        End If
        MyBase.OnFormClosing(e)
    End Sub
End Class
