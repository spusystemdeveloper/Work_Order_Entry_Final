# Instantiate compiled forms without showing them. Never execute Print, preview
# Load, report refresh, printer dialogs or spool jobs; preview Load queries SQL.
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
$forms=[Collections.Generic.List[Windows.Forms.Form]]::new();$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function New-Form($name){$f=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.'+$name,$true));$forms.Add($f);return $f}
function C($f,$n){$f.GetType().GetProperty($n,$flags).GetValue($f,$null)}
function Bounds($parent){
    foreach($c in $parent.Controls){
        Check ($c.Left -ge 0 -and $c.Top -ge 0 -and $c.Right -le $parent.ClientSize.Width -and $c.Bottom -le $parent.ClientSize.Height) ('Out of bounds: '+$c.Name)
        if($c.Name -eq 'previewHeader'){Bounds $c}
    }
}
try{
    $options=New-Form 'frmPrintType'
    $input=C $options 'txtWO';$order=C $options 'rbtnOrder';$pick=C $options 'rbtnPickList';$print=C $options 'btnPrint'
    Check ($options.AcceptButton -eq $print -and $null -eq $options.CancelButton -and $options.KeyPreview) 'Existing Enter/keyboard behavior changed'
    Check ($order.Checked -and !$pick.Checked) 'Default template changed'
    Check ($order.Parent -eq $pick.Parent -and $order.Parent -eq $options) 'Radio grouping/arrow navigation changed'
    Check ($input.TabIndex -eq 0 -and $order.TabIndex -eq 1 -and $pick.TabIndex -eq 2 -and $print.TabIndex -eq 3) 'Existing tab order changed'
    Check ($input.TextAlign -eq [Windows.Forms.HorizontalAlignment]::Center -and !$input.ReadOnly -and !$input.Multiline) 'Order input behavior changed'
    Check ($order.Font.Size -ge 14 -and $pick.Font.Size -ge 14) 'Template choices must use readable large text'
    Check ($input.Font.Size -ge 24 -and $print.Font.Size -ge 14) 'Order digits and Print action must remain large'
    Check ($order.Height -ge 44 -and $pick.Height -ge 44 -and !$order.AutoSize -and !$pick.AutoSize) 'Choices need large clickable areas'
    Check ($order.Bottom -lt $pick.Top -and $pick.Bottom -lt (C $options 'lblOrderNumber').Top) 'Stacked choices must remain separated'
    foreach($choice in @($order,$pick)){
        $text=[Windows.Forms.TextRenderer]::MeasureText($choice.Text,$choice.Font)
        Check ($text.Width -le ($choice.Width-$choice.Padding.Horizontal-24) -and $text.Height -le $choice.Height) 'Large choice labels must fit without clipping'
    }
    $pick.Checked=$true;Check (!$order.Checked -and $pick.Checked) 'Pick List must remain mutually exclusive'
    $order.Checked=$true;Check ($order.Checked -and !$pick.Checked) 'Order must remain mutually exclusive'
    $input.Text='SYNTHETIC'
    # Only this options Load handler is safe: it clears the field and attaches
    # existing keyboard validation. Never invoke either preview's Load handler.
    $options.GetType().GetMethod('frmPrintType_Load',$flags).Invoke($options,@($options,[EventArgs]::Empty)) | Out-Null
    Check ($input.Text -eq '') 'Existing load/reset behavior changed'
    $onKey=[Windows.Forms.Control].GetMethod('OnKeyPress',$flags)
    foreach($char in @('1','9','x',[string][char]8)){
        $e=[Windows.Forms.KeyPressEventArgs]::new([char]$char)
        $onKey.Invoke($input,@($e)) | Out-Null
        Check ($e.Handled -eq ($char -eq 'x')) 'Existing keyboard validation changed'
    }
    Bounds $options
    $options.Scale([Drawing.SizeF]::new(1.25,1.25));Bounds $options
    foreach($n in @('frmPrintWo','frmPrintPicklist')){
        $f=New-Form $n;$viewer=C $f 'rptviewer';$header=C $f 'previewHeader'
        Check ($viewer.Parent -eq $f -and $viewer.Dock -eq [Windows.Forms.DockStyle]::Fill) 'Existing viewer instance must fill the preview'
        Check ($viewer.TabIndex -eq $(if($n -eq 'frmPrintWo'){0}else{1})) 'Preview tab order changed'
        Check ($f.WindowState -eq [Windows.Forms.FormWindowState]::Maximized) 'Existing maximized opening state changed'
        Check ($f.TopMost -eq ($n -eq 'frmPrintPicklist')) 'Existing preview ownership/topmost policy changed'
        Check ($viewer.ShowToolBar -and $viewer.ShowPrintButton -and $viewer.ShowPageNavigationControls -and $viewer.ShowZoomControl -and $viewer.ShowExportButton -and $viewer.ShowFindControls -and $viewer.ShowRefreshButton) 'Existing ReportViewer controls were hidden'
        $report=if($n -eq 'frmPrintWo'){'WorkOrderEntry.rprtOrder.rdlc'}else{'WorkOrderEntry.rptPickList.rdlc'}
        $source=if($n -eq 'frmPrintWo'){'ItemWoEntry'}else{'ItemOrderEntry'}
        Check ($viewer.LocalReport.ReportEmbeddedResource -eq $report) 'Report template changed'
        Check ($viewer.LocalReport.DataSources.Count -eq 1 -and $viewer.LocalReport.DataSources[0].Name -eq $source) 'Report data-source contract changed'
        Check ($header.Dock -eq [Windows.Forms.DockStyle]::Top -and !$header.TabStop) 'Heading should not intercept keyboard navigation'
        if($n -eq 'frmPrintWo'){
            $timer=C $f 'Print_DialogTimer'
            Check ($timer.Interval -eq 3500 -and !$timer.Enabled) 'Existing print timer changed'
        }
        foreach($size in @([Drawing.Size]::new(1100,650),[Drawing.Size]::new(800,450),[Drawing.Size]::new(1400,800))){
            $f.ClientSize=$size;$f.PerformLayout();Bounds $f
            Check ($viewer.Top -ge $header.Bottom) 'Preview heading overlaps the toolbar'
            Check ($viewer.Bottom -eq ($f.ClientSize.Height-$f.Padding.Bottom)) 'Preview must fill remaining space'
        }
        $f.Scale([Drawing.SizeF]::new(1.25,1.25));Bounds $f
    }
    Write-Output "PASS: $checks isolated Print UI/default/keyboard/report-contract/resize checks. No SQL, printer dialog or print job."
}finally{
    foreach($f in $forms){$f.Dispose()}
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
