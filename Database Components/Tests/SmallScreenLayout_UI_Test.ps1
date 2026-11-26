# Compiled layout only. Never show/load application forms or execute order actions.
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
$a=[Reflection.Assembly]::LoadFrom($app)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$fit=$a.GetType('WorkOrderEntry.SmallScreenLayout',$true).GetMethod('Fit',[Reflection.BindingFlags]'Public,NonPublic,Static')
$visible=[Windows.Forms.Control].GetMethod('GetState',$flags)
$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$n){$f.GetType().GetProperty($n,$flags).GetValue($f,$null)}
function Inside($parent,$child){
    Check ($child.Left -ge 0 -and $child.Top -ge 0 -and $child.Right -le $parent.ClientSize.Width -and $child.Bottom -le $parent.ClientSize.Height) ('Clipped: '+$child.Name)
}
function New-Form($name){
    $type=$a.GetType('WorkOrderEntry.'+$name,$true)
    if($name -eq 'frmOrderHistory'){return [Activator]::CreateInstance($type,@(0,''))}
    if($name -eq 'frmShortageAlerts'){
        $list=[Activator]::CreateInstance([Collections.Generic.List``1].MakeGenericType($a.GetType('WorkOrderEntry.WarehouseShortage',$true)))
        return [Activator]::CreateInstance($type,@($list,[int[]]@()))
    }
    return [Activator]::CreateInstance($type)
}
try{
    foreach($width in @(1024,1366,1600)){
        foreach($scale in @(1.0,1.25)){
            # Assume a 40-pixel taskbar; real monitor/taskbar/DPI validation remains manual.
            $area=[Drawing.Rectangle]::new(0,0,$width,728)
            foreach($name in @('frmSettings','frmRecall','frmCheckWO','frmPrintWo','frmPrintPicklist','frmUploader','frmWebsiteImport','frmOrderHistory','frmShortageAlerts')){
                $f=New-Form $name
                try{
                    $f.WindowState='Normal'
                    if($scale -ne 1){$f.Scale([Drawing.SizeF]::new($scale,$scale))}
                    $font=$f.Font.Size;$f.Location=[Drawing.Point]::new(4000,4000)
                    $fit.Invoke($null,@($f,$area)) | Out-Null
                    $f.PerformLayout()
                    Check ($area.Contains($f.Bounds)) ("$name exceeds ${width}x768 at $scale")
                    Check ($f.Font.Size -eq $font) 'Fitting must not shrink fonts'
                    if($name -eq 'frmSettings'){
                        Check ($f.AutoScroll) 'All settings must remain reachable when scaled'
                        foreach($group in $f.Controls){Check ($group.Right -le $f.DisplayRectangle.Right -and $group.Bottom -le $f.DisplayRectangle.Bottom) 'Settings content is not scrollable to its edge'}
                        if($scale -eq 1){Check (!$f.HorizontalScroll.Visible -and !$f.VerticalScroll.Visible) 'Normal-size settings should fit without scrolling'}
                    }else{
                        foreach($child in $f.Controls){Inside $f $child}
                    }
                    if($name -eq 'frmUploader'){
                        foreach($button in @('cmdReport','cmdImport','cmdClose')){Inside (C $f 'Panel2') (C $f $button)}
                        Inside (C $f 'Panel3') (C $f 'panelInfo')
                        Inside (C $f 'panelInfo') (C $f 'lblFile')
                    }
                    # Simulate restoring a previously oversized window; same fitter
                    # is attached to Resize after Shown, without invoking SQL Load here.
                    $f.Size=[Drawing.Size]::new(1600,1000)
                    $fit.Invoke($null,@($f,$area)) | Out-Null
                    Check ($area.Contains($f.Bounds)) 'Restored dialog exceeds the working area'
                }finally{$f.Dispose()}
            }
            $f=New-Form 'frmItemLookUp'
            try{
                $f.WindowState='Normal'
                if($scale -ne 1){$f.Scale([Drawing.SizeF]::new($scale,$scale))}
                $f.ClientSize=[Drawing.Size]::new($width,$area.Height-24)
                # Exercise the branch options with both bottom actions displayed.
                (C $f 'cmdImport').Visible=$true;(C $f 'cmdForInvoice').Visible=$true
                $f.PerformLayout()
                foreach($name in @('Panel5','panelSelectItem','gridSelectItem','panelButtons','panelTotalsContainer','GroupBox2','GroupBox3','GroupBox4')){
                    $c=C $f $name;Inside $c.Parent $c
                }
                $sub=C $f 'GroupBox2';$vat=C $f 'GroupBox3';$total=C $f 'GroupBox4'
                Check ($sub.Right -lt $vat.Left -and $vat.Right -lt $total.Left) 'Totals overlap'
                Check ((C $f 'txtSub').ReadOnly -and (C $f 'txtVat').ReadOnly -and (C $f 'txtTotal').ReadOnly) 'Totals behavior changed'
                $buttons=@((C $f 'panelButtons').Controls | Where-Object {$_ -is [Windows.Forms.Button] -and $visible.Invoke($_,@(2))})
                $invoice=C $f 'cmdForInvoice'
                Check ($invoice.Font.Size -eq (C $f 'cmdImport').Font.Size) 'For Invoice uses the same readable font size as neighboring actions'
                $textSize=[Windows.Forms.TextRenderer]::MeasureText($invoice.Text,$invoice.Font)
                $iconWidth=if($invoice.Image){$invoice.Image.Width}else{0}
                Check ($invoice.ClientSize.Width -ge $textSize.Width+$iconWidth+$invoice.Padding.Horizontal+8) 'For Invoice text and icon fit without clipping'
                $buttonLeft=(C $f 'panelButtons').Left+$invoice.Left
                $lookupRight=(C $f 'panelGrid').Left+(C $f 'panelItem').Left+(C $f 'gridItem').Right
                $selectedRight=(C $f 'panelGrid').Left+(C $f 'panelSelectItem').Left+(C $f 'gridSelectItem').Right
                Check ($buttonLeft-$lookupRight -ge 0 -and $buttonLeft-$lookupRight -le [Math]::Ceiling(2*$scale)) 'Action buttons hug the lookup grid with only a thin separator'
                Check ($buttonLeft-$selectedRight -ge 0 -and $buttonLeft-$selectedRight -le [Math]::Ceiling(2*$scale)) 'Action buttons hug the selected grid with only a thin separator'
                foreach($button in $buttons){Inside $button.Parent $button}
                for($i=0;$i -lt $buttons.Count;$i++){for($j=$i+1;$j -lt $buttons.Count;$j++){
                    Check (!$buttons[$i].Bounds.IntersectsWith($buttons[$j].Bounds)) 'Action buttons overlap'
                }}
                Check ((C $f 'gridSelectItem').Height -ge 75) 'Selected items need a usable scrollable viewport'
                Check ((C $f 'gridItem').ScrollBars -eq [Windows.Forms.ScrollBars]::Both) 'Wide lookup columns must remain reachable by horizontal scrolling'
            }finally{$f.Dispose()}
        }
    }
    # The user's 1600x900 display: check the main action column at its taller
    # client area independently of the existing dialog-fitting fixture above.
    $f=New-Form 'frmItemLookUp'
    try{
        $f.WindowState='Normal';$f.ClientSize=[Drawing.Size]::new(1600,836)
        (C $f 'cmdForInvoice').Visible=$true;(C $f 'cmdImport').Visible=$true
        $f.PerformLayout()
        $invoice=C $f 'cmdForInvoice'
        Inside $invoice.Parent $invoice
        Check ($invoice.Width -ge 129) 'For Invoice has wider space on the 1600x900 main form'
        $textSize=[Windows.Forms.TextRenderer]::MeasureText($invoice.Text,$invoice.Font)
        Check ($invoice.ClientSize.Width -ge $textSize.Width+$invoice.Image.Width+$invoice.Padding.Horizontal+8) 'For Invoice text and icon fit at 1600x900'
        foreach($name in @('panelButtons','panelTotalsContainer','panelGrid','panelSelectItem','gridSelectItem')){$c=C $f $name;Inside $c.Parent $c}
    }finally{$f.Dispose()}
    $f=New-Form 'frmSettings'
    try{
        # Invoke only the new presentation Shown handler, never Show/Load. Check
        # that the attached Resize handler also caps a later oversized resize.
        $f.GetType().GetMethod('FitSettingsWindow',$flags).Invoke($f,@($f,[EventArgs]::Empty)) | Out-Null
        $area=[Windows.Forms.Screen]::FromControl($f).WorkingArea
        $f.Size=[Drawing.Size]::new($area.Width+200,$area.Height+200)
        Check ([Windows.Forms.Screen]::FromControl($f).WorkingArea.Contains($f.Bounds)) 'Attached resize fitting must keep the actual window in its working area'
    }finally{$f.Dispose()}
    Write-Output "PASS: $checks isolated small-screen checks at 1024, 1366 and 1600 widths, normal and simulated 125% scaling; main actions also checked at 1600x900. No SQL or order actions."
}finally{
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
