Public Class frmSetup

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        loadMainFrm("STORE")
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        loadMainFrm("UP-STORE")

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Me.Close()
    End Sub


    Private Sub loadMainFrm(ByVal pickLock As String)

        If rbtnPickup.Checked = False And rbntDelivery.Checked = False Then
            MessageBox.Show("Choose Release Type", "Message!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        End If

        If rbntDelivery.Checked = True Then
            '  frmMain.txtReleaseType.Text = "Open Work Order : For Delivery"
            My.Settings.ReleaseType = "Delivery"

        ElseIf rbtnPickup.Checked = True Then

            '  frmMain.txtReleaseType.Text = "Open Work Order : For Pick-up"
            My.Settings.ReleaseType = "Pick-up"

        End If

        If pickLock = "STORE" Then
            My.Settings.PickLoc = "STORE"
        ElseIf pickLock = "UP-STORE" Then
            My.Settings.PickLoc = "UP-STORE"
        End If

        My.Settings.Save()


        My.Application.OpenForms.Cast(Of Form)() _
              .Except({Me}) _
              .ToList() _
              .ForEach(Sub(form) form.Close())

        Dim frm = New frmMain
        frm.Show()

        Me.Close()

    End Sub

    Private Sub rbntDelivery_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbntDelivery.CheckedChanged
        If rbntDelivery.Checked = True Then
            Button1.Enabled = False
        Else
            Button1.Enabled = True

        End If
    End Sub

    Private Sub frmSetup_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If My.Settings.ReleaseType = "Pick-up" Then
            rbtnPickup.Checked = True
        End If

        If My.Settings.ReleaseType = "Delivery" Then
            rbntDelivery.Checked = True
        End If


    End Sub

End Class