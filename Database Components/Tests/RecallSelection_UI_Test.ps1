# Isolated choice dialog only. Never opens Recall, converts/saves or uses SQL.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$hash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$a=[Reflection.Assembly]::LoadFrom($app);$type=$a.GetType('WorkOrderEntry.frmRecallSelection',$true)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$dir=Join-Path $repo ('TestResults/RecallSelection-'+[Guid]::NewGuid().ToString('N').Substring(0,8));[void](New-Item -ItemType Directory -Path $dir)
$modes=[ordered]@{Recall=@('Recall a quote','Recall a work order');SaveToQuote=@('Save Changes','Convert to a work order');SaveToWorkOrder=@('Convert to a work order')}
$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($form,$name){return $type.GetProperty($name,$flags).GetValue($form,$null)}
function Place($form){$form.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$form.Location=[Drawing.Point]::new(-30000,-30000)}
foreach($mode in $modes.Keys){
    foreach($scale in @(1,1.25)){
        $f=[Activator]::CreateInstance($type,[object[]]@($mode));$bitmap=$null;$graphics=$null
        try{
            Place $f;$f.Show();$f.PerformLayout();[Windows.Forms.Application]::DoEvents()
            $list=C $f 'listBoxSelection';$ok=C $f 'btnOK';$cancel=C $f 'Button1'
            Check ($list.Items.Count -eq $modes[$mode].Count) 'Mode choice count changed'
            foreach($i in 0..($list.Items.Count-1)){Check ($list.Items[$i] -ceq $modes[$mode][$i]) 'Mode choice text/order changed'}
            Check ($list.SelectedIndex -eq 0) 'First choice must remain the default'
            Check ($f.AcceptButton -eq $ok -and $f.CancelButton -eq $cancel -and $f.KeyPreview) 'Existing keyboard routing changed'
            Check ($list.TabIndex -eq 0 -and $ok.TabIndex -eq 1 -and $cancel.TabIndex -eq 2) 'Existing tab order changed'
            Check ($list.SelectionMode -eq [Windows.Forms.SelectionMode]::One -and $list.DrawMode -eq [Windows.Forms.DrawMode]::Normal) 'List behavior changed'
            if($scale -ne 1){$f.AutoScaleMode=[Windows.Forms.AutoScaleMode]::None;$f.Scale([Drawing.SizeF]::new($scale,$scale));foreach($control in $f.Controls){$control.Font=[Drawing.Font]::new($control.Font.FontFamily,([single]($control.Font.SizeInPoints*$scale)),$control.Font.Style)}}
            Check ($f.Width -lt 1024 -and $f.Height -lt 704) 'Dialog does not fit the smaller screen'
            foreach($control in $f.Controls){Check ($control.Left -ge 0 -and $control.Top -ge 0 -and $control.Right -le $f.ClientSize.Width -and $control.Bottom -le $f.ClientSize.Height) ('Clipped control '+$control.Name)}
            Check ($list.Bottom -lt $ok.Top -and $list.Height -ge $list.ItemHeight*$list.Items.Count+4) 'Options/buttons overlap or option rows are clipped'
            $bitmap=[Drawing.Bitmap]::new($f.Width,$f.Height);$graphics=[Drawing.Graphics]::FromImage($bitmap)
            foreach($item in $list.Items){Check ($graphics.MeasureString($item,$list.Font).Width -lt $list.ClientSize.Width-8) 'Option text is clipped'}
            $f.DrawToBitmap($bitmap,[Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height));$bitmap.Save((Join-Path $dir ($mode+'-'+$scale+'.png')),[Drawing.Imaging.ImageFormat]::Png)
        }finally{if($graphics){$graphics.Dispose()};if($bitmap){$bitmap.Dispose()};$f.Dispose()}
    }
    foreach($case in @('OK','DoubleClick','Cancel','Escape','EnterRouting')){
        $f=[Activator]::CreateInstance($type,[object[]]@($mode));Place $f
        $timer=[Windows.Forms.Timer]::new();$timer.Interval=100
        $watchdog=[Windows.Forms.Timer]::new();$watchdog.Interval=3000;$script:failure=$null
        $watchdog.add_Tick({$watchdog.Stop();$script:failure='Choice dialog did not finish';$f.DialogResult=[Windows.Forms.DialogResult]::Cancel;$f.Close()})
        $timer.add_Tick({
            $timer.Stop()
            try{
                $list=C $f 'listBoxSelection';$list.SelectedIndex=$list.Items.Count-1
                switch($case){
                    'OK' {(C $f 'btnOK').PerformClick()}
                    'DoubleClick' {$args=[Windows.Forms.MouseEventArgs]::new([Windows.Forms.MouseButtons]::Left,2,5,5,0);[void]$type.GetMethod('listBoxSelection_MouseDoubleClick',$flags).Invoke($f,@($list,$args))}
                    'Cancel' {(C $f 'Button1').PerformClick()}
                    'Escape' {[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($f,@([Windows.Forms.Keys]::Escape))}
                    'EnterRouting' {$e=[Windows.Forms.KeyEventArgs]::new([Windows.Forms.Keys]::Enter);[void]$type.GetMethod('frmRecallSelection_KeyDown',$flags).Invoke($f,@($f,$e));Check ($e.Handled -and $f.ActiveControl -eq (C $f 'btnOK')) 'Existing Enter handler must focus OK and mark handled';(C $f 'btnOK').PerformClick()}
                }
            }catch{$script:failure=$_;$f.DialogResult=[Windows.Forms.DialogResult]::Cancel;$f.Close()}
        })
        try{
            $watchdog.Start();$timer.Start();$result=$f.ShowDialog();if($script:failure){throw $script:failure}
            if($case -in @('Cancel','Escape')){Check ($result -eq [Windows.Forms.DialogResult]::Cancel -and $f.SelectedOption -ceq '') 'Cancel/Escape must return no selection'}
            else{Check ($result -eq [Windows.Forms.DialogResult]::OK -and $f.SelectedOption -ceq $modes[$mode][-1]) 'Existing OK/double-click/Enter selection changed'}
        }finally{$watchdog.Stop();$watchdog.Dispose();$timer.Stop();$timer.Dispose();$f.Dispose()}
    }
}
$f=[Activator]::CreateInstance($type)
try{$f.Selection();Check ($f._mode -ceq 'Recall' -and (C $f 'listBoxSelection').SelectedIndex -eq 0) 'Default constructor behavior changed'}finally{$f.Dispose()}
Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ceq $hash) 'Store configuration changed'
Write-Output "PASS: $checks isolated Recall Selection checks; renders: $dir. No actual recall/conversion/save or SQL."
