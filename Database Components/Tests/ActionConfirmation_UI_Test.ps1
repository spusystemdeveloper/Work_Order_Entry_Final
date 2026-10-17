# Isolated confirmation forms only. Never invokes main-form Save/Remove or SQL.
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$hash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$type=$assembly.GetType('WorkOrderEntry.frmActionConfirmation',$true)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$directory=Join-Path $repo ('TestResults/ActionConfirmation-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
[void](New-Item -ItemType Directory -Path $directory)
$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function Control($form,$name){return $type.GetProperty($name,$flags).GetValue($form,$null)}
foreach($question in @('Save changes?','Do you want to remove this item?')){
    foreach($scale in @(1,1.25)){
        $caption=if($question -eq 'Save changes?'){'Confirm changes'}else{'Remove item'}
        $form=[Activator]::CreateInstance($type,[object[]]@($question,$caption))
        $bitmap=$null;$graphics=$null
        try{
            $yes=Control $form 'btnYes';$no=Control $form 'btnNo';$label=Control $form 'lblQuestion'
            Check ($label.Text -ceq $question -and $form.Text -ceq $caption) 'Prompt copy/caption changed'
            Check ($yes.DialogResult -eq [Windows.Forms.DialogResult]::Yes -and $no.DialogResult -eq [Windows.Forms.DialogResult]::No) 'Yes/No result values changed'
            Check ($form.AcceptButton -eq $yes -and $form.ActiveControl -eq $yes) 'Yes must remain the default/focused choice'
            Check ($null -eq $form.CancelButton -and !$form.ControlBox) 'A Yes/No prompt must not gain a Cancel/close choice'
            Check ($label.Font.Bold -and $label.Font.SizeInPoints -eq 14) 'Compact question should retain readable bold text'
            $form.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$form.Location=[Drawing.Point]::new(-30000,-30000)
            $form.Show();$form.PerformLayout();[Windows.Forms.Application]::DoEvents()
            if($scale -ne 1){
                $form.AutoScaleMode=[Windows.Forms.AutoScaleMode]::None
                $form.Scale([Drawing.SizeF]::new($scale,$scale))
                foreach($control in $form.Controls){$control.Font=[Drawing.Font]::new($control.Font.FontFamily,([single]($control.Font.SizeInPoints*$scale)),$control.Font.Style)}
            }
            Check ($form.Width -lt 1024 -and $form.Height -lt 704) 'Prompt should fit the smaller workstation'
            foreach($control in $form.Controls){Check ($control.Left -ge 0 -and $control.Top -ge 0 -and $control.Right -le $form.ClientSize.Width -and $control.Bottom -le $form.ClientSize.Height) ('Clipped control '+$control.Name+' scale='+$scale+' bounds='+$control.Bounds+' client='+$form.ClientSize)}
            $form.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$form.Location=[Drawing.Point]::new(-30000,-30000)
            $form.Show();$form.PerformLayout();[Windows.Forms.Application]::DoEvents()
            $bitmap=[Drawing.Bitmap]::new($form.Width,$form.Height);$graphics=[Drawing.Graphics]::FromImage($bitmap)
            Check ($graphics.MeasureString($label.Text,$label.Font,$label.Width).Height -le $label.Height) 'Question text should not be clipped'
            $form.DrawToBitmap($bitmap,[Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height))
            $bitmap.Save((Join-Path $directory ($caption.Replace(' ','')+'-'+$scale+'.png')),[Drawing.Imaging.ImageFormat]::Png)
        }finally{if($graphics){$graphics.Dispose()};if($bitmap){$bitmap.Dispose()};$form.Dispose()}
    }
}
foreach($case in @('Yes','No','DefaultEnter','NoEnter','RightEnter','MnemonicY','MnemonicN','Escape','UserClose')){
    $form=[Activator]::CreateInstance($type)
    $yes=Control $form 'btnYes';$no=Control $form 'btnNo'
    $form.StartPosition=[Windows.Forms.FormStartPosition]::Manual;$form.Location=[Drawing.Point]::new(-30000,-30000)
    $timer=[Windows.Forms.Timer]::new();$timer.Interval=100
    $watchdog=[Windows.Forms.Timer]::new();$watchdog.Interval=3000
    $script:failure=$null
    $watchdog.add_Tick({$watchdog.Stop();$script:failure='Confirmation did not finish: '+$case;$form.DialogResult=[Windows.Forms.DialogResult]::No;$form.Close()})
    $timer.add_Tick({
        $timer.Stop()
        try{
            switch($case){
                'Yes' {$yes.PerformClick()}
                'No' {$no.PerformClick()}
                'DefaultEnter' {[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($form,@([Windows.Forms.Keys]::Enter))}
                'NoEnter' {$no.Select();[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($form,@([Windows.Forms.Keys]::Enter))}
                'RightEnter' {[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($form,@([Windows.Forms.Keys]::Right));Check ($form.ActiveControl -eq $no) 'Right arrow should move Yes to No';[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($form,@([Windows.Forms.Keys]::Enter))}
                'MnemonicY' {[void]$type.GetMethod('ProcessDialogChar',$flags).Invoke($form,@([char]'y'))}
                'MnemonicN' {[void]$type.GetMethod('ProcessDialogChar',$flags).Invoke($form,@([char]'n'))}
                'Escape' {[void]$type.GetMethod('ProcessDialogKey',$flags).Invoke($form,@([Windows.Forms.Keys]::Escape));Check ($form.Visible -and $form.DialogResult -eq [Windows.Forms.DialogResult]::None) 'Escape must leave the question unanswered';$no.PerformClick()}
                'UserClose' {$form.Close();Check ($form.Visible -and $form.DialogResult -eq [Windows.Forms.DialogResult]::None) 'Closing must leave the question unanswered';$no.PerformClick()}
            }
        }catch{$script:failure=$_;$form.DialogResult=[Windows.Forms.DialogResult]::No;$form.Close()}
    })
    try{
        $watchdog.Start();$timer.Start();$result=$form.ShowDialog()
        if($script:failure){throw $script:failure}
        $expected=if($case -in @('Yes','DefaultEnter','MnemonicY')){[Windows.Forms.DialogResult]::Yes}else{[Windows.Forms.DialogResult]::No}
        Check ($result -eq $expected) ('Incorrect answer for '+$case)
    }finally{$watchdog.Stop();$watchdog.Dispose();$timer.Stop();$timer.Dispose();$form.Dispose()}
}
Check ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ceq $hash) 'Store EXE configuration changed'
Write-Output "PASS: $checks isolated action-confirmation checks; renders: $directory. No Save/Remove or SQL executed."
