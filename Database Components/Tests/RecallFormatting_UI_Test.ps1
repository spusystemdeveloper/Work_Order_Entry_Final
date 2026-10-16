# Actual compiled checkbox formatting, without login, SQL or saved orders.
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
$config=$app+'.config';$configHash=(Get-FileHash -LiteralPath $config).Hash
$assembly=[Reflection.Assembly]::LoadFrom($app)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$form=[Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.frmItemLookUp',$true))
$errors=[Collections.Generic.List[string]]::new()
$checks=0
function Check($pass,$message){if(!$pass){throw $message};$script:checks++}
try{
    # Prevent quantity/totals/save callbacks; never show or load the application.
    $form.GetType().GetField('isLoadingRecalledOrder',$flags).SetValue($form,$true)
    $grid=$form.GetType().GetProperty('gridSelectItem',$flags).GetValue($form,$null)
    $handler=[Windows.Forms.DataGridViewDataErrorEventHandler]{param($sender,$eventArgs)
        # Test-only observer: any DataError fails the test below. Production has
        # no error-suppression handler and must format the cell correctly.
        $errors.Add(('Column {0}: {1}' -f $eventArgs.ColumnIndex,$eventArgs.Exception.Message))
        $eventArgs.ThrowException=$false
    }
    $grid.add_DataError($handler)
    $row=$grid.Rows.Add()
    $cell=$grid.Rows[$row].Cells[17]
    Check ($cell -is [Windows.Forms.DataGridViewCheckBoxCell]) 'UP-STORE must remain a checkbox'
    foreach($applyRuntimeStyle in @($false,$true)){
        if($applyRuntimeStyle){$null=$form.GetType().GetMethod('FormatGrid',$flags).Invoke($form,$null)}
        foreach($value in @($null,[DBNull]::Value,$false,$true)){
            $cell.Value=$value
            $formatted=$cell.FormattedValue
            Check ($formatted -is [bool]) 'Blank/Boolean UP-STORE must format as Boolean, not text'
            $expected=if($value -is [bool]){$value}else{$false}
            Check ($formatted -eq $expected) 'Blank cells display unchecked; true/false retain their states'
            Check ([object]::Equals($cell.Value,$value)) 'Formatting must not change the stored cell value'
        }
    }
    $grid.AllowUserToAddRows=$true
    Check ($grid.NewRowIndex -ge 0) 'Recall-style temporary new-row placeholder exists'
    $placeholder=$grid.Rows[$grid.NewRowIndex].Cells[17]
    Check ($placeholder.FormattedValue -is [bool]) 'Temporary blank row formats as a checkbox'
    Check ($errors.Count -eq 0) ('Unexpected DataError: '+($errors -join '; '))
    Write-Output "PASS: $checks isolated Recall checkbox formatting checks; zero DataErrors. No SQL or saved-order changes."
}finally{
    $form.Dispose()
    if((Get-FileHash -LiteralPath $config).Hash -ne $configHash){throw 'Store configuration changed'}
}
