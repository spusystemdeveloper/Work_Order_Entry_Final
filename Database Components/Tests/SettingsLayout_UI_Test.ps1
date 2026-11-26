# Isolated compiled Settings presentation checks. Never show/load the actual form:
# its Load handler reads store settings and SQL. Never invoke save/password handlers.
[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$form=[Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.frmSettings',$true))
$checks=0
function Check($pass,$message){if(!$pass){throw $message};$script:checks++}
function Control($name){$form.GetType().GetProperty($name,$flags).GetValue($form,$null)}
function Check-Layout($parent){
    $children=@($parent.Controls)
    foreach($child in $children){
        Check ($child.Left -ge 0 -and $child.Top -ge 0 -and $child.Right -le $parent.ClientSize.Width -and $child.Bottom -le $parent.ClientSize.Height) ('Control exceeds parent: '+$child.Name)
        if($child -is [Windows.Forms.GroupBox]){Check-Layout $child}
    }
    for($i=0;$i -lt $children.Count;$i++){
        for($j=$i+1;$j -lt $children.Count;$j++){
            Check (!$children[$i].Bounds.IntersectsWith($children[$j].Bounds)) ('Overlapping controls: '+$children[$i].Name+' / '+$children[$j].Name)
        }
    }
}
try{
    Check ($null -eq $form.AcceptButton -and $null -eq $form.CancelButton) 'Do not introduce Enter/Esc save actions'
    foreach($name in @('txtPassword','txtPassword2','txtPassword3','txtConfirmP','txtConfirmP2','txtConfirmP3')){
        Check ((Control $name).UseSystemPasswordChar) ('Password mask lost: '+$name)
    }
    foreach($name in @('cmdOK','cmdPass','cmdPass2','cmdPass3','Button2','Button3')){
        Check ((Control $name).Text.Contains('&O')) ('Existing mnemonic lost: '+$name)
    }
    Check ((Control 'cmdSaveBranchSettings').BackColor -ne (Control 'cmdPass').BackColor) 'Save must be visually distinct from password actions'
    foreach($name in @('txtLimit','txtExpectedDatabase','txtPassword','cmbPriceBound')){
        $c=Control $name;$before=$c.Text
        $form.GetType().GetMethod('SettingsFieldEntered',$flags).Invoke($form,@($c,[EventArgs]::Empty)) | Out-Null
        Check ($c.BackColor -eq [Drawing.Color]::FromArgb(232,242,255)) ('Missing field focus cue: '+$name)
        $form.GetType().GetMethod('SettingsFieldLeft',$flags).Invoke($form,@($c,[EventArgs]::Empty)) | Out-Null
        Check ($c.BackColor -eq [Drawing.SystemColors]::Window -and $c.Text -eq $before) 'Focus must restore appearance without modifying input'
    }
    $button=Control 'cmdPass'
    $form.GetType().GetMethod('SettingsButtonEntered',$flags).Invoke($form,@($button,[EventArgs]::Empty)) | Out-Null
    Check ($button.FlatAppearance.BorderSize -eq 2) 'Button focus must have a clear border'
    $form.GetType().GetMethod('SettingsButtonLeft',$flags).Invoke($form,@($button,[EventArgs]::Empty)) | Out-Null
    Check ($button.FlatAppearance.BorderSize -eq 1) 'Button focus border must reset'
    $queue=Control 'chkEnableBranchQueue'
    foreach($enabled in @($false,$true)){
        $queue.Checked=$enabled
        $form.GetType().GetMethod('UpdateBranchFunctionControlState',$flags).Invoke($form,$null) | Out-Null
        Check ((Control 'chkAllowOrderGrouping').Enabled -eq $enabled -and (Control 'chkShowForInvoice').Enabled -eq $enabled) 'Existing queue dependency must remain unchanged'
        (Control 'chkAllowBranchSelection').Checked=$enabled
        $form.GetType().GetMethod('UpdateBranchSelectionControlState',$flags).Invoke($form,$null) | Out-Null
        Check ((Control 'txtExpectedDatabase').Enabled -eq !$enabled) 'Existing assigned database dependency must remain unchanged'
    }
    Check-Layout $form
    # Scaled geometry only; this does not claim testing on actual high-DPI hardware.
    $form.Scale([Drawing.SizeF]::new(1.25,1.25));Check-Layout $form
    Write-Output "PASS: $checks isolated Settings checks (geometry, focus cues, masks, mnemonics and existing dependencies). No SQL/settings saves."
}finally{
    $form.Dispose()
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
