Imports System.Drawing
Imports System.Windows.Forms

Partial Class frmSettings
    'Presentation only. Keep settings loading, validation and persistence in frmSettings.vb.
    Private Sub SettingsFieldEntered(sender As Object, e As EventArgs) Handles txtLimit.Enter, cmbLevel.Enter, txtPassword.Enter, txtConfirmP.Enter, txtPassword2.Enter, txtConfirmP2.Enter, txtPassword3.Enter, txtConfirmP3.Enter, txtCustType.Enter, cmbPriceBound.Enter, cmbPricePass.Enter, txtExpectedDatabase.Enter, txtWebCust.Enter, txtWebRep.Enter, txtWebShip.Enter, txtMySQLsvr.Enter, txtMySQLdb.Enter, txtMySQLuser.Enter, txtMySQLpass.Enter
        DirectCast(sender, Control).BackColor = Color.FromArgb(232, 242, 255)
    End Sub

    Private Sub SettingsFieldLeft(sender As Object, e As EventArgs) Handles txtLimit.Leave, cmbLevel.Leave, txtPassword.Leave, txtConfirmP.Leave, txtPassword2.Leave, txtConfirmP2.Leave, txtPassword3.Leave, txtConfirmP3.Leave, txtCustType.Leave, cmbPriceBound.Leave, cmbPricePass.Leave, txtExpectedDatabase.Leave, txtWebCust.Leave, txtWebRep.Leave, txtWebShip.Leave, txtMySQLsvr.Leave, txtMySQLdb.Leave, txtMySQLuser.Leave, txtMySQLpass.Leave
        DirectCast(sender, Control).BackColor = SystemColors.Window
    End Sub

    Private Sub SettingsButtonEntered(sender As Object, e As EventArgs) Handles cmdOK.Enter, cmdPass.Enter, cmdPass2.Enter, cmdPass3.Enter, cmdSaveBranchSettings.Enter, Button1.Enter, Button2.Enter, Button3.Enter, cmdNew.Enter, cmdAdd.Enter, cmdDel.Enter
        Dim button = DirectCast(sender, Button)
        button.FlatAppearance.BorderColor = Color.FromArgb(15, 57, 110)
        button.FlatAppearance.BorderSize = 2
    End Sub

    Private Sub SettingsButtonLeft(sender As Object, e As EventArgs) Handles cmdOK.Leave, cmdPass.Leave, cmdPass2.Leave, cmdPass3.Leave, cmdSaveBranchSettings.Leave, Button1.Leave, Button2.Leave, Button3.Leave, cmdNew.Leave, cmdAdd.Leave, cmdDel.Leave
        Dim button = DirectCast(sender, Button)
        button.FlatAppearance.BorderColor = Color.SlateGray
        button.FlatAppearance.BorderSize = 1
    End Sub

    Private Sub SettingsOptionEntered(sender As Object, e As EventArgs) Handles chkAllowBranchSelection.Enter, chkLockCustomerSalesRepOnRecall.Enter, chkLockSalesRepToCustomer.Enter, chkEnableBranchQueue.Enter, chkAllowOrderGrouping.Enter, chkShowForInvoice.Enter, chkAllowDuplicateItemEntry.Enter, chkShowImportButton.Enter, chkEnableSetTaxButton.Enter, chkShowOrderActions.Enter, chkShowOrderNotifications.Enter
        DirectCast(sender, Control).BackColor = Color.FromArgb(232, 242, 255)
    End Sub

    Private Sub SettingsOptionLeft(sender As Object, e As EventArgs) Handles chkAllowBranchSelection.Leave, chkLockCustomerSalesRepOnRecall.Leave, chkLockSalesRepToCustomer.Leave, chkEnableBranchQueue.Leave, chkAllowOrderGrouping.Leave, chkShowForInvoice.Leave, chkAllowDuplicateItemEntry.Leave, chkShowImportButton.Leave, chkEnableSetTaxButton.Leave, chkShowOrderActions.Leave, chkShowOrderNotifications.Leave
        DirectCast(sender, Control).BackColor = Color.Transparent
    End Sub
End Class
