# Shared prompt presentation/adapter only. No main form, registry or SQL actions.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$hash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app);$type=$assembly.GetType('WorkOrderEntry.frmAppPrompt',$true)
$adapter=$assembly.GetType('WorkOrderEntry.AppPrompt',$true)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$directory=Join-Path $repo ('TestResults/AppPrompt-'+[Guid]::NewGuid().ToString('N').Substring(0,8));[void](New-Item -ItemType Directory -Path $directory)
$checks=0
$sets=[ordered]@{OK=@(1);OKCancel=@(1,2);AbortRetryIgnore=@(3,4,5);YesNoCancel=@(6,7,2);YesNo=@(6,7);RetryCancel=@(4,2)}
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$name){return $type.GetProperty($name,$flags).GetValue($f,$null)}
function Place($f){$f.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$f.Location=[Drawing.Point]::new(-30000,-30000)}
function New-Prompt($buttons,$icon,$default=0,$text='Synthetic message & details.',$caption='Synthetic caption'){
    $f=[Activator]::CreateInstance($type,[object[]]@([string]$text,[string]$caption,[Windows.Forms.MessageBoxButtons]$buttons,[Windows.Forms.MessageBoxIcon]$icon,[Windows.Forms.MessageBoxDefaultButton]$default))
    Place $f
    return $f
}
foreach($set in $sets.Keys){
    foreach($icon in @(0,16,32,48,64)){
        foreach($defaultIndex in 0..($sets[$set].Count-1)){
            $f=New-Prompt ([Windows.Forms.MessageBoxButtons]::$set) $icon ($defaultIndex*256)
            try{
                $f.Show();$f.PerformLayout();[Windows.Forms.Application]::DoEvents()
                $buttons=@((C $f 'btnFirst'),(C $f 'btnSecond'),(C $f 'btnThird'))
                Check ($f.Text -ceq 'Synthetic caption' -and (C $f 'txtMessage').Text -ceq 'Synthetic message & details.') 'Message or caption changed'
                Check ((C $f 'txtMessage').ReadOnly -and !(C $f 'txtMessage').TabStop -and (C $f 'txtMessage').Multiline) 'Message should be read-only and complete'
                foreach($i in 0..2){
                    Check ($buttons[$i].Visible -eq ($i -lt $sets[$set].Count)) 'Button visibility differs from native set'
                    if($i -lt $sets[$set].Count){Check ([int]$buttons[$i].DialogResult -eq $sets[$set][$i]) 'Button return value changed'}
                }
                Check ($f.AcceptButton -eq $buttons[$defaultIndex] -and $f.ActiveControl -eq $buttons[$defaultIndex]) 'Default/focused button changed'
                $cancelIndex=[Array]::IndexOf([object[]]$sets[$set],2);if($set -eq 'OK'){$cancelIndex=0}
                if($cancelIndex -ge 0){Check ($f.CancelButton -eq $buttons[$cancelIndex]) 'Cancel/close result changed'}else{Check ($null -eq $f.CancelButton -and !$f.ControlBox) 'No-Cancel prompt gained a close choice'}
                Check (((C $f 'picIcon').Image -ne $null) -eq ($icon -ne 0)) 'Icon semantics changed'
                foreach($control in $f.Controls){if($control.Visible){Check ($control.Left -ge 0 -and $control.Top -ge 0 -and $control.Right -le $f.ClientSize.Width -and $control.Bottom -le $f.ClientSize.Height) ('Clipped '+$control.Name)}}
            }finally{$f.Dispose()}
        }
    }
}
foreach($set in $sets.Keys){
    foreach($action in @('EachButton','DefaultEnter','Escape','UserClose')){
        $indexes=if($action -eq 'EachButton'){0..($sets[$set].Count-1)}else{@($sets[$set].Count-1)}
        foreach($index in $indexes){
            $f=New-Prompt ([Windows.Forms.MessageBoxButtons]::$set) 32 ($index*256)
            $timer=[Windows.Forms.Timer]::new();$timer.Interval=60;$watchdog=[Windows.Forms.Timer]::new();$watchdog.Interval=3000;$script:failure=$null
            $watchdog.add_Tick({$watchdog.Stop();$script:failure='Prompt did not finish';$f.DialogResult=[Windows.Forms.DialogResult]$sets[$set][0];$f.Close()})
            $timer.add_Tick({
                $timer.Stop()
                try{
                    $buttons=@((C $f 'btnFirst'),(C $f 'btnSecond'),(C $f 'btnThird'))
                    if($action -eq 'EachButton'){$buttons[$index].PerformClick()}
                    elseif($action -eq 'DefaultEnter'){[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($f,@([Windows.Forms.Keys]::Enter))}
                    else{
                        if($action -eq 'Escape'){[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($f,@([Windows.Forms.Keys]::Escape))}else{$f.Close()}
                        if($null -eq $f.CancelButton){Check ($f.Visible -and $f.DialogResult -eq [Windows.Forms.DialogResult]::None) 'No-Cancel prompt must remain unanswered';$buttons[0].PerformClick()}
                    }
                }catch{$script:failure=$_;$f.DialogResult=[Windows.Forms.DialogResult]$sets[$set][0];$f.Close()}
            })
            try{
                $watchdog.Start();$timer.Start();$result=$f.ShowDialog();if($script:failure){throw $script:failure}
                $expected=$sets[$set][$index]
                if($action -in @('Escape','UserClose')){$expected=if($set -eq 'OK'){1}elseif($sets[$set] -contains 2){2}else{$sets[$set][0]}}
                Check ([int]$result -eq $expected) ('Incorrect result: '+$set+'/'+$action)
            }finally{$watchdog.Stop();$watchdog.Dispose();$timer.Stop();$timer.Dispose();$f.Dispose()}
        }
    }
}
foreach($scale in @(1,1.25)){
    $text=('Split order / long error details & literal text.'+"`r`n")*300
    $f=New-Prompt ([Windows.Forms.MessageBoxButtons]::YesNoCancel) 48 256 $text
    $bitmap=$null
    try{
        $f.Show();$f.PerformLayout();[Windows.Forms.Application]::DoEvents()
        if($scale -ne 1){$f.AutoScaleMode=[Windows.Forms.AutoScaleMode]::None;$f.Scale([Drawing.SizeF]::new($scale,$scale));foreach($control in $f.Controls){$control.Font=[Drawing.Font]::new($control.Font.FontFamily,([single]($control.Font.SizeInPoints*$scale)),$control.Font.Style)}}
        Check ((C $f 'txtMessage').Text -ceq $text -and (C $f 'txtMessage').ScrollBars -eq [Windows.Forms.ScrollBars]::Vertical) 'Long messages/previews must remain complete and scrollable'
        Check ($f.Width -lt 1024 -and $f.Height -lt 704) 'Prompt exceeds smaller-screen bounds'
        foreach($control in $f.Controls){if($control.Visible){Check ($control.Right -le $f.ClientSize.Width -and $control.Bottom -le $f.ClientSize.Height -and $control.Left -ge 0 -and $control.Top -ge 0) ('Scaled clipping '+$control.Name)}}
        $bitmap=[Drawing.Bitmap]::new($f.Width,$f.Height);$f.DrawToBitmap($bitmap,[Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height));$bitmap.Save((Join-Path $directory ('Long-'+$scale+'.png')),[Drawing.Imaging.ImageFormat]::Png)
    }finally{if($bitmap){$bitmap.Dispose()};$f.Dispose()}
}
# Exercise the public adapters rather than merely inspecting the form.
foreach($api in @('Show','MsgBox')){
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=60;$script:seen=$false
    $timer.add_Tick({foreach($f in @([Windows.Forms.Application]::OpenForms)){if($f.GetType() -eq $type){$script:seen=$true;$timer.Stop();Check ((C $f 'txtMessage').Text -ceq 'Adapter message') 'Adapter altered text';(C $f 'btnSecond').PerformClick();break}}})
    try{
        $timer.Start()
        if($api -eq 'Show'){
            $method=$adapter.GetMethod('Show',[type[]]@([string],[string],[Windows.Forms.MessageBoxButtons],[Windows.Forms.MessageBoxIcon],[Windows.Forms.MessageBoxDefaultButton]))
            $result=$method.Invoke($null,@('Adapter message','Adapter caption',[Windows.Forms.MessageBoxButtons]::YesNo,[Windows.Forms.MessageBoxIcon]::Question,[Windows.Forms.MessageBoxDefaultButton]::Button2))
        }else{
            $result=$adapter.GetMethod('MsgBox').Invoke($null,@([object]'Adapter message',[int](4+32+256),[object]'Adapter caption'))
        }
        Check ($script:seen -and [int]$result -eq 7) ('Adapter result changed: '+$api)
    }finally{$timer.Stop();$timer.Dispose()}
}
# A real worker thread exercises Invoke marshalling without a PowerShell runspace.
Add-Type -TypeDefinition @'
using System;
using System.Reflection;
using System.Threading;
public sealed class PromptWorker {
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr handle);
    public object Result;
    public Exception Error;
    public Thread Thread;
    public void Start(MethodInfo method, object[] arguments) {
        Thread = new Thread(() => {
            try { Result = method.Invoke(null, arguments); }
            catch (Exception error) { Error = error; }
        });
        Thread.IsBackground = true;
        Thread.Start();
    }
}
'@
foreach($background in @($false,$true)){
    $hostForm=[Windows.Forms.Form]::new();Place $hostForm;$hostForm.Show()
    $uiThread=[Threading.Thread]::CurrentThread.ManagedThreadId
    $method=$adapter.GetMethod('Show',[type[]]@([Windows.Forms.IWin32Window],[string],[string],[Windows.Forms.MessageBoxButtons],[Windows.Forms.MessageBoxIcon],[Windows.Forms.MessageBoxDefaultButton]))
    $arguments=[object[]]@($hostForm,'Owned message','Owned caption',[Windows.Forms.MessageBoxButtons]::YesNo,[Windows.Forms.MessageBoxIcon]::Question,[Windows.Forms.MessageBoxDefaultButton]::Button1)
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=60;$script:seen=$false;$script:failure=$null
    $timer.add_Tick({
        foreach($f in @([Windows.Forms.Application]::OpenForms)){
            if($f.GetType() -eq $type){
                $timer.Stop();$script:seen=$true
                try{
                    Check ($f.Owner -eq $hostForm) 'Explicit parent ownership changed'
                    Check ([Threading.Thread]::CurrentThread.ManagedThreadId -eq $uiThread -and !$f.InvokeRequired) 'Worker prompt did not marshal to the owner UI thread'
                }catch{$script:failure=$_}
                (C $f 'btnSecond').PerformClick();break
            }
        }
    })
    try{
        $timer.Start()
        if($background){
            $worker=[PromptWorker]::new();$worker.Start($method,$arguments)
            $deadline=[DateTime]::UtcNow.AddSeconds(5)
            while($worker.Thread.IsAlive -and [DateTime]::UtcNow -lt $deadline){[Windows.Forms.Application]::DoEvents();Start-Sleep -Milliseconds 10}
            Check (!$worker.Thread.IsAlive) 'Worker prompt did not return'
            if($worker.Error){throw $worker.Error};$result=$worker.Result
        }else{$result=$method.Invoke($null,$arguments)}
        if($script:failure){throw $script:failure}
        Check ($script:seen -and [int]$result -eq 7) 'Owned prompt result changed'
    }finally{$timer.Stop();$timer.Dispose();$hostForm.Dispose()}
}
# Reproduce an item-price modal that raises a warning from a parent-form callback.
# Synthetic forms only: no real price-selection Load/insert/credit/SQL routines.
foreach($topMost in @($false,$true)){foreach($api in @('Show','OwnedShow','MsgBox')){foreach($pendingClose in @($false,$true)){
    $hostForm=[Windows.Forms.Form]::new();Place $hostForm;$hostForm.Show()
    $priceForm=[Windows.Forms.Form]::new();Place $priceForm;$priceForm.TopMost=$topMost
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=60
    $script:seen=$false;$script:failure=$null;$script:warningResult=$null
    $timer.add_Tick({
        foreach($f in @([Windows.Forms.Application]::OpenForms)){
            if($f.GetType() -eq $type){
                $timer.Stop();$script:seen=$true
                try{
                    Check ($f.Owner -eq $priceForm) 'Warning must belong to the open modal, not the main form'
                    Check ($f.TopMost -eq $priceForm.TopMost) 'Warning must match its owner stacking level'
                    Check (![PromptWorker]::IsWindowEnabled($priceForm.Handle) -and ![PromptWorker]::IsWindowEnabled($hostForm.Handle)) 'Parent windows must remain disabled until the warning is answered'
                    Check ((C $f 'txtMessage').Text -ceq 'Exceeded available credit limit.') 'Credit warning text changed'
                }catch{$script:failure=$_}
                (C $f 'btnFirst').PerformClick();break
            }
        }
    })
    $priceForm.add_Shown({
        try{
            # insertItem sets OK before UpdateAmt raises the credit warning.
            if($pendingClose){$priceForm.DialogResult=[Windows.Forms.DialogResult]::OK}
            $timer.Start()
            if($api -eq 'MsgBox'){
                $script:warningResult=$adapter.GetMethod('MsgBox').Invoke($null,@([object]'Exceeded available credit limit.',[int]48,[object]'Warning'))
            }elseif($api -eq 'OwnedShow'){
                $method=$adapter.GetMethod('Show',[type[]]@([Windows.Forms.IWin32Window],[string],[string],[Windows.Forms.MessageBoxButtons],[Windows.Forms.MessageBoxIcon],[Windows.Forms.MessageBoxDefaultButton]))
                $script:warningResult=$method.Invoke($null,@($priceForm,'Exceeded available credit limit.','Warning',[Windows.Forms.MessageBoxButtons]::OK,[Windows.Forms.MessageBoxIcon]::Warning,[Windows.Forms.MessageBoxDefaultButton]::Button1))
            }else{
                $method=$adapter.GetMethod('Show',[type[]]@([string],[string],[Windows.Forms.MessageBoxButtons],[Windows.Forms.MessageBoxIcon],[Windows.Forms.MessageBoxDefaultButton]))
                $script:warningResult=$method.Invoke($null,@('Exceeded available credit limit.','Warning',[Windows.Forms.MessageBoxButtons]::OK,[Windows.Forms.MessageBoxIcon]::Warning,[Windows.Forms.MessageBoxDefaultButton]::Button1))
            }
            Check ([PromptWorker]::IsWindowEnabled($priceForm.Handle)) 'Price selection must be usable after acknowledging the warning'
        }catch{$script:failure=$_}finally{$priceForm.DialogResult=[Windows.Forms.DialogResult]::Cancel;$priceForm.Close()}
    })
    try{
        [void]$priceForm.ShowDialog($hostForm)
        if($script:failure){throw $script:failure}
        Check ($script:seen -and [int]$script:warningResult -eq 1) 'Warning must return OK'
        Check ([PromptWorker]::IsWindowEnabled($hostForm.Handle) -and !$priceForm.Visible) 'Main form must be usable after closing price selection'
    }finally{$timer.Stop();$timer.Dispose();$priceForm.Dispose();$hostForm.Dispose()}
}}}
Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ceq $hash) 'Store configuration changed'
Write-Output "PASS: $checks shared prompt checks; renders: $directory. No business actions or database."
