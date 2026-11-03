Imports System.Globalization
Imports System.Windows.Forms

' Presentation adapter: callers retain their original arguments and result checks.
Public NotInheritable Class AppPrompt
    Public Const StandardCaption As String = "Work Order Entry"
    Private Const UnexpectedErrorIntro As String = "Something went wrong. Please try again or contact support."

    Private Sub New()
    End Sub

    ' Unexpected errors start with "FROM"; keep that detail for support below a plain sentence.
    Public Shared Function FriendlyText(text As String) As String
        If text Is Nothing OrElse Not text.StartsWith("FROM ", StringComparison.Ordinal) Then Return text
        Return UnexpectedErrorIntro & vbCrLf & vbCrLf & text
    End Function

    ' Numbered error captions and purpose-specific titles are left unchanged.
    Public Shared Function FriendlyCaption(caption As String) As String
        Select Case caption
            Case "Message!", "Message !", "Message", "Warning", "Validation Warning"
                Return StandardCaption
            Case Else
                Return caption
        End Select
    End Function

    Public Shared Function Show(text As String, Optional caption As String = "",
                                Optional buttons As MessageBoxButtons = MessageBoxButtons.OK,
                                Optional icon As MessageBoxIcon = MessageBoxIcon.None,
                                Optional defaultButton As MessageBoxDefaultButton = MessageBoxDefaultButton.Button1) As DialogResult
        Return Show(Nothing, text, caption, buttons, icon, defaultButton)
    End Function

    Public Shared Function Show(owner As IWin32Window, text As String, Optional caption As String = "",
                                Optional buttons As MessageBoxButtons = MessageBoxButtons.OK,
                                Optional icon As MessageBoxIcon = MessageBoxIcon.None,
                                Optional defaultButton As MessageBoxDefaultButton = MessageBoxDefaultButton.Button1) As DialogResult
        text = FriendlyText(text)
        caption = FriendlyCaption(caption)
        Try
            Dim dispatcher As Control = TryCast(owner, Control)
            If dispatcher Is Nothing Then
                For Each form As Form In Application.OpenForms
                    If Not form.IsDisposed AndAlso form.IsHandleCreated Then
                        dispatcher = form
                        Exit For
                    End If
                Next
            End If
            If dispatcher IsNot Nothing AndAlso dispatcher.InvokeRequired Then
                Return DirectCast(dispatcher.Invoke(New Func(Of DialogResult)(
                    Function() ShowDialogCore(owner, text, caption, buttons, icon, defaultButton))), DialogResult)
            End If
            Return ShowDialogCore(owner, text, caption, buttons, icon, defaultButton)
        Catch
            ' Keep critical/error reporting available if the custom UI cannot initialize.
            Return System.Windows.Forms.MessageBox.Show(owner, text, caption, buttons, icon, defaultButton)
        End Try
    End Function

    Private Shared Function ShowDialogCore(owner As IWin32Window, text As String, caption As String,
                                           buttons As MessageBoxButtons, icon As MessageBoxIcon,
                                           defaultButton As MessageBoxDefaultButton) As DialogResult
        If owner Is Nothing Then
            Dim currentOwner As Form = Form.ActiveForm
            ' A price-selection callback can raise a warning before its modal
            ' window has closed. Keep that window below its own warning.
            For index As Integer = Application.OpenForms.Count - 1 To 0 Step -1
                Dim candidate As Form = Application.OpenForms(index)
                If Not candidate.IsDisposed AndAlso candidate.IsHandleCreated AndAlso
                   Not candidate.InvokeRequired AndAlso candidate.Visible Then
                    If candidate.Modal Then
                        currentOwner = candidate
                        Exit For
                    End If
                    If currentOwner Is Nothing Then currentOwner = candidate
                End If
            Next
            owner = currentOwner
        End If
        Using prompt As New frmAppPrompt(text, caption, buttons, icon, defaultButton)
            Dim ownerControl As Control = TryCast(owner, Control)
            Dim ownerForm As Form = If(ownerControl Is Nothing, Nothing, ownerControl.FindForm())
            If ownerForm IsNot Nothing Then prompt.TopMost = ownerForm.TopMost
            Return prompt.ShowDialog(owner)
        End Using
    End Function

    Public Shared Function MsgBox(Prompt As Object, Optional Buttons As Integer = 0,
                                   Optional Title As Object = Nothing) As MsgBoxResult
        ' Preserve uncommon native help/desktop/modal flags rather than ignoring them.
        If (CInt(Buttons) And Not &H3FF) <> 0 Then
            Return Microsoft.VisualBasic.Interaction.MsgBox(Prompt, CType(Buttons, MsgBoxStyle), Title)
        End If
        Dim caption As String = If(Title Is Nothing, My.Application.Info.Title,
                                   Convert.ToString(Title, CultureInfo.CurrentCulture))
        Return CType(Show(Convert.ToString(Prompt, CultureInfo.CurrentCulture), caption,
                          CType(CInt(Buttons) And &HF, MessageBoxButtons),
                          CType(CInt(Buttons) And &HF0, MessageBoxIcon),
                          CType(CInt(Buttons) And &H300, MessageBoxDefaultButton)), MsgBoxResult)
    End Function
End Class
