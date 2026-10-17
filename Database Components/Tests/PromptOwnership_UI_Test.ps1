# Presentation audit only: isolated prompts/synthetic owners, no store forms or SQL.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class PromptWindowAudit {
    [DllImport("user32.dll")] public static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")] static extern IntPtr GetWindow(IntPtr window, uint direction);
    public static bool IsAbove(IntPtr prompt, IntPtr owner) {
        IntPtr next = prompt;
        for(int i=0;i<10000;i++) {
            next=GetWindow(next,2);
            if(next==owner) return true;
            if(next==IntPtr.Zero) return false;
        }
        return false;
    }
}
'@
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$hash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$adapter=$assembly.GetType('WorkOrderEntry.AppPrompt',$true)
$promptType=$assembly.GetType('WorkOrderEntry.frmAppPrompt',$true)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$show=$adapter.GetMethod('Show',[type[]]@([string],[string],[Windows.Forms.MessageBoxButtons],[Windows.Forms.MessageBoxIcon],[Windows.Forms.MessageBoxDefaultButton]))
$checks=0;$scenarios=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$name){$f.GetType().GetProperty($name,$flags).GetValue($f,$null)}
function Place($f){$f.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$f.Location=[Drawing.Point]::new(-30000,-30000)}
function Check-Owner($f,$owner){
    Check ($f.Owner -eq $owner) 'Prompt has the wrong owner'
    Check ([PromptWindowAudit]::IsAbove($f.Handle,$owner.Handle)) 'Prompt is below its owner in native window order'
    Check (![PromptWindowAudit]::IsWindowEnabled($owner.Handle)) 'Owner must remain disabled while its prompt is open'
    Check ([PromptWindowAudit]::IsWindowEnabled($f.Handle)) 'Prompt itself must accept input'
}
$sets=[ordered]@{OK=@(1);OKCancel=@(1,2);YesNo=@(6,7);YesNoCancel=@(6,7,2);RetryCancel=@(4,2);AbortRetryIgnore=@(3,4,5)}
foreach($topMost in @($false,$true)){foreach($set in $sets.Keys){foreach($icon in @(0,16,32,48,64)){
    $hostForm=[Windows.Forms.Form]::new();Place $hostForm;$hostForm.Show()
    $modal=[Windows.Forms.Form]::new();Place $modal;$modal.TopMost=$topMost
    $script:failure=$null;$script:seen=$false;$script:result=$null
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=50
    $timer.add_Tick({foreach($f in @([Windows.Forms.Application]::OpenForms)){if($f.GetType() -eq $promptType){
        $timer.Stop();$script:seen=$true
        try{Check-Owner $f $modal;Check ($f.TopMost -eq $topMost) 'Stacking setting differs from modal owner'}catch{$script:failure=$_}
        $buttons=@((C $f 'btnFirst'),(C $f 'btnSecond'),(C $f 'btnThird'))
        $buttons[$sets[$set].Count-1].PerformClick();break
    }}})
    $modal.add_Shown({
        try{
            $timer.Start()
            $script:result=$show.Invoke($null,@('Synthetic prompt audit','Prompt audit',[Windows.Forms.MessageBoxButtons]::$set,[Windows.Forms.MessageBoxIcon]$icon,[Windows.Forms.MessageBoxDefaultButton]::Button1))
            Check ([PromptWindowAudit]::IsWindowEnabled($modal.Handle)) 'Modal was not restored after the prompt'
        }catch{$script:failure=$_}finally{$modal.DialogResult=[Windows.Forms.DialogResult]::Cancel;$modal.Close()}
    })
    try{
        [void]$modal.ShowDialog($hostForm)
        if($script:failure){throw $script:failure}
        Check ($script:seen -and [int]$script:result -eq $sets[$set][-1]) 'Prompt result changed'
        Check ([PromptWindowAudit]::IsWindowEnabled($hostForm.Handle)) 'Main owner was not restored'
        $scenarios++
    }finally{$timer.Stop();$timer.Dispose();$modal.Dispose();$hostForm.Dispose()}
}}}
$cases=@(
    @{Type='frmActionConfirmation';Args=[object[]]@('Save changes?','Confirm changes');Button='btnNo';Result=7},
    @{Type='frmActionConfirmation';Args=[object[]]@('Do you want to remove this item?','Remove item');Button='btnNo';Result=7},
    @{Type='frmImportComplete';Args=[object[]]@();Button='btnOK';Result=1},
    @{Type='frmOrderSaved';Args=[object[]]@([int[]]@(23286),$false,6,$false);Button='btnClose';Result=1},
    @{Type='frmOrderSaved';Args=[object[]]@([int[]]@(18886),$false,6,$true);Button='btnClose';Result=1},
    @{Type='frmRecallSelection';Args=[object[]]@('Recall');Button='btnOK';Result=1},
    @{Type='frmRecallSelection';Args=[object[]]@('SaveToQuote');Button='btnOK';Result=1},
    @{Type='frmRecallSelection';Args=[object[]]@('SaveToWorkOrder');Button='btnOK';Result=1}
)
foreach($topMost in @($false,$true)){foreach($case in $cases){
    $hostForm=[Windows.Forms.Form]::new();Place $hostForm;$hostForm.TopMost=$topMost;$hostForm.Show()
    $f=[Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.'+$case.Type,$true),$case.Args);Place $f
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=50;$script:failure=$null
    $timer.add_Tick({$timer.Stop();try{Check-Owner $f $hostForm}catch{$script:failure=$_};(C $f $case.Button).PerformClick()})
    try{
        $timer.Start();$result=$f.ShowDialog($hostForm)
        if($script:failure){throw $script:failure}
        Check ([int]$result -eq $case.Result) ('Result changed: '+$case.Type)
        Check ([PromptWindowAudit]::IsWindowEnabled($hostForm.Handle)) 'Owner did not recover after closing confirmation'
        $scenarios++
    }finally{$timer.Stop();$timer.Dispose();$f.Dispose();$hostForm.Dispose()}
}}
Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ceq $hash) 'Store configuration changed'
Write-Output "PASS: $checks ownership/stacking checks across $scenarios scenarios. Synthetic owners; no business actions or SQL."
