# Layout and existing row/default-selection methods only. Never show/load the
# form or run OK, Enter, F9/password, insertion, pricing application or SQL.
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
$a=[Reflection.Assembly]::LoadFrom($app);$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$f=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.frmPriceLevel',$true))
$errors=[Collections.Generic.List[string]]::new();$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($n){$f.GetType().GetProperty($n,$flags).GetValue($f,$null)}
try{
    $grid=C 'gridLevel';$price=C 'Price';$label=C 'ItemCode'
    $grid.add_DataError([Windows.Forms.DataGridViewDataErrorEventHandler]{param($s,$e)
        $errors.Add($e.Exception.Message);$e.ThrowException=$false
    })
    Check ($grid.Columns.Count -eq 2 -and $label.Index -eq 0 -and $price.Index -eq 1) 'Price-level/amount ordinals changed'
    Check ($grid.ReadOnly -and !$grid.AllowUserToAddRows -and !$grid.AllowUserToDeleteRows -and $grid.AllowUserToOrderColumns -and $grid.MultiSelect) 'Grid permissions changed'
    Check ($grid.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::FullRowSelect) 'Selection mode changed'
    Check ($f.KeyPreview -and $null -eq $f.AcceptButton -and $null -eq $f.CancelButton) 'Existing keyboard routing changed'
    Check ((C 'cmdOK').DialogResult -eq [Windows.Forms.DialogResult]::OK -and (C 'cmdCancel').DialogResult -eq [Windows.Forms.DialogResult]::Cancel) 'Dialog results changed'
    Check ($grid.TabIndex -eq 14 -and (C 'cmdOK').TabIndex -eq 15 -and (C 'cmdCancel').TabIndex -eq 16 -and (C 'txtItemName').TabIndex -eq 17 -and (C 'txtItemCode').TabIndex -eq 18) 'Tab order changed'
    Check ((C 'txtItemCode').ReadOnly -and (C 'txtItemName').ReadOnly -and (C 'txtItemName').Multiline -and (C 'txtItemName').ScrollBars -eq [Windows.Forms.ScrollBars]::Both) 'Item heading behavior changed'
    Check ($price.DefaultCellStyle.Format -eq '##0.00' -and $null -eq $price.DefaultCellStyle.NullValue) 'Original price format/null display changed'
    Check ($price.DefaultCellStyle.Alignment -eq [Windows.Forms.DataGridViewContentAlignment]::MiddleRight) 'Prices should align with the larger price-level rows'
    $levels=@('PRICE (RETAIL)','PRICE A (WHOLESALE)','PRICE B (D1)','PRICE C (D2)')
    $values=@([double]178,[double]171,[double]0.125,[double]1234.5678)
    foreach($i in 0..3){$f.DisplayPrice($levels[$i],$values[$i])}
    Check ($grid.Rows.Count -eq 4 -and !$grid.AllowUserToAddRows) 'Existing DisplayPrice row behavior changed'
    foreach($i in 0..3){
        Check ($grid.Rows[$i].Cells[0].Value -eq $levels[$i] -and [double]$grid.Rows[$i].Cells[1].Value -eq $values[$i]) 'Price label/order/value changed'
    }
    # LoadLevels normally chooses currency format; check both existing display
    # paths without querying the store, and ensure neither changes raw prices.
    foreach($format in @('##0.00','C')){
        $price.DefaultCellStyle.Format=$format
        foreach($i in 0..3){
            Check ($grid.Rows[$i].Cells[1].FormattedValue -eq $values[$i].ToString($format,[Globalization.CultureInfo]::CurrentCulture)) 'Price display changed'
            Check ([double]$grid.Rows[$i].Cells[1].Value -eq $values[$i]) 'Formatting altered a raw price'
        }
    }
    $grid.CurrentCell=$grid.Rows[3].Cells[1]
    $f.GetType().GetMethod('SelectTopPriceLevel',$flags).Invoke($f,$null) | Out-Null
    Check ($grid.CurrentRow.Index -eq 0 -and $grid.Rows[0].Selected -and $grid.CurrentCell.ColumnIndex -eq 0) 'Existing top-row default selection changed'
    $price.DisplayIndex=0
    Check ($label.Index -eq 0 -and $price.Index -eq 1 -and [double]$grid.Rows[2].Cells[1].Value -eq 0.125) 'Visual reordering changed the underlying price column'
    (C 'txtItemName').Text='SYNTHETIC LONG ITEM NAME '+('DESCRIPTION '*30)
    foreach($scaled in @($false,$true)){
        if($scaled){$f.Scale([Drawing.SizeF]::new(1.25,1.25))}
        Check ($f.Width -lt 1024 -and $f.Height -lt 728) 'Dialog should fit the smaller screen'
        foreach($c in $f.Controls){Check ($c.Left -ge 0 -and $c.Top -ge 0 -and $c.Right -le $f.ClientSize.Width -and $c.Bottom -le $f.ClientSize.Height) ('Clipped control: '+$c.Name)}
    }
    $grid.Rows.Clear()
    $f.GetType().GetMethod('SelectTopPriceLevel',$flags).Invoke($f,$null) | Out-Null
    Check ($grid.Rows.Count -eq 0) 'Empty default selection must remain safe'
    Check ($errors.Count -eq 0) ('Unexpected DataErrors: '+($errors -join '; '))
    Write-Output "PASS: $checks isolated Price Level layout, raw-price, formatting and default-selection checks. No SQL or price application."
}finally{
    $f.Dispose()
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
