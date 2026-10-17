# Invoke actual validation with synthetic rows; never show/load the main form.
# All legacy contexts are replaced with Nothing in this test process before use.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$type=$assembly.GetType('WorkOrderEntry.frmItemLookUp',$true)
$module=$assembly.GetType('WorkOrderEntry.ModConnectDB',$true)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance';$static=[Reflection.BindingFlags]'Public,Static'
$saved=@{};$checks=0;$scenarios=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$name){$type.GetProperty($name,$flags).GetValue($f,$null)}
try{
    foreach($name in @('db','dbnew','dbInitial','bAllow','ispriceApproved','WoRecallType')){
        $field=$module.GetField($name,$static);$saved[$name]=$field.GetValue($null)
        if($name -in @('db','dbnew','dbInitial')){
            if($saved[$name] -and $saved[$name].Connection.State -ne [Data.ConnectionState]::Closed){throw 'Unexpected pre-existing connection; test aborted.'}
            $field.SetValue($null,$null)
        }
    }
    $module.GetField('bAllow',$static).SetValue($null,$false)
    $module.GetField('ispriceApproved',$static).SetValue($null,0)
    foreach($recall in @(0,1)){foreach($case in @(
        @{Quote=$false;Payment='';Price=[decimal]75;Caption='Message!'},
        @{Quote=$false;Payment='';Price='?';Caption='Message!'},
        @{Quote=$false;Payment='   ';Price=[decimal]75;Caption='Message!'},
        @{Quote=$false;Payment='CASH';Price='?';Caption='Price Confirmation Required'},
        @{Quote=$false;Payment='CHARGE';Price='?';Caption='Price Confirmation Required'},
        @{Quote=$true;Payment='';Price='?';Caption='Price Confirmation Required'}
    )){
        $f=[Activator]::CreateInstance($type);$hostForm=[Windows.Forms.Form]::new();$timer=[Windows.Forms.Timer]::new()
        try{
            $type.GetField('isLoadingRecalledOrder',$flags).SetValue($f,$true)
            $type.GetField('isBulkLoading',$flags).SetValue($f,$true)
            $module.GetField('WoRecallType',$static).SetValue($null,[string]$recall)
            (C $f 'chkWorkOrder').Checked=!$case.Quote;(C $f 'chkQuote').Checked=$case.Quote
            Check ($f.getEntryType() -eq $(if($case.Quote){3}else{2})) 'Wrong entry type'
            $payment=C $f 'cboPayment';$payment.SelectedIndex=-1
            if($case.Payment -ne ''){if(!$payment.Items.Contains($case.Payment)){[void]$payment.Items.Add($case.Payment)};$payment.SelectedItem=$case.Payment}
            $grid=C $f 'gridSelectItem';$grid.AllowUserToAddRows=$false;$grid.Rows.Clear();$row=$grid.Rows.Add()
            foreach($cell in $grid.Rows[$row].Cells){$cell.Value=if($cell -is [Windows.Forms.DataGridViewCheckBoxCell]){$false}else{0}}
            $grid.Item(0,$row).Value='SYNTHETIC-SKU';$grid.Item(1,$row).Value='Synthetic validation item'
            $grid.Item(2,$row).Value=1;$grid.Item(3,$row).Value=$case.Price
            $hostForm.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$hostForm.Location=[Drawing.Point]::new(-30000,-30000);$hostForm.Show()
            $script:seen=0;$script:failure=$null;$timer.Interval=50
            $timer.add_Tick({foreach($prompt in @([Windows.Forms.Application]::OpenForms)){
                if($prompt.GetType().FullName -eq 'WorkOrderEntry.frmAppPrompt'){
                    $script:seen++
                    try{
                        Check ($prompt.Text -ceq $case.Caption) 'Validation prompt order changed'
                        $message=$prompt.GetType().GetProperty('txtMessage',$flags).GetValue($prompt,$null).Text
                        if($case.Caption -eq 'Message!'){Check ($message -ceq 'Please select a Payment Method') 'Payment warning changed'}else{Check ($message.StartsWith('The price for item SYNTHETIC-SKU has not been confirmed.')) 'Price confirmation changed'}
                        Check (@([Windows.Forms.Application]::OpenForms | Where-Object {$_.GetType().FullName -eq 'WorkOrderEntry.frmPassword'}).Count -eq 0) 'Password opened before mandatory validation'
                    }catch{$script:failure=$_}
                    $prompt.GetType().GetProperty('btnFirst',$flags).GetValue($prompt,$null).PerformClick()
                }
            }})
            $timer.Start();$result=$type.GetMethod('ValidateAndSave',$flags).Invoke($f,$null);$timer.Stop()
            if($script:failure){throw $script:failure}
            Check (!$result -and $script:seen -eq 1) 'Invalid entry must stop after one validation prompt'
            Check ($grid.Rows.Count -eq 1 -and $grid.Item(3,0).Value -ceq $case.Price -and [int]$grid.Item(2,0).Value -eq 1) 'Validation changed the order line'
            Check (!$module.GetField('bAllow',$static).GetValue($null) -and [int]$module.GetField('ispriceApproved',$static).GetValue($null) -eq 0) 'Validation changed approval state'
            foreach($name in @('db','dbnew','dbInitial')){Check ($null -eq $module.GetField($name,$static).GetValue($null)) 'A database context was created'}
            $scenarios++
        }finally{$timer.Stop();$timer.Dispose();$hostForm.Dispose();$f.Dispose()}
    }}
    Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ceq $configHash) 'Store configuration changed'
    Write-Output "PASS: $checks payment/approval-order checks across $scenarios new/recalled Work Order and quotation cases. No SQL, password authentication or saves."
}finally{foreach($name in $saved.Keys){$module.GetField($name,$static).SetValue($null,$saved[$name])}}
