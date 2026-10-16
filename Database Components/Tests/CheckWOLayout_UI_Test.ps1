# Actual compiled grid with owned synthetic data. Never show/load the form or
# execute its item lookup: the Load handler connects to the store database.
[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing,System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$a=[Reflection.Assembly]::LoadFrom($app)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$form=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.frmCheckWO',$true))
$form.BindingContext=[Windows.Forms.BindingContext]::new()
$grid=$form.GetType().GetProperty('gridQtyWO',$flags).GetValue($form,$null)
$errors=[Collections.Generic.List[string]]::new();$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function Geometry($parent){
    foreach($c in $parent.Controls){
        Check ($c.Left -ge 0 -and $c.Top -ge 0 -and $c.Right -le $parent.ClientSize.Width -and $c.Bottom -le $parent.ClientSize.Height) ('Out of bounds: '+$c.Name)
        if($c -is [Windows.Forms.GroupBox]){Geometry $c}
    }
}
try{
    $grid.add_DataError([Windows.Forms.DataGridViewDataErrorEventHandler]{param($s,$e)
        $errors.Add($e.Exception.Message);$e.ThrowException=$false
    })
    Check ($grid.ReadOnly -and !$grid.AllowUserToAddRows -and !$grid.AllowUserToDeleteRows) 'Read-only permissions changed'
    Check ($grid.AllowUserToOrderColumns -and $grid.MultiSelect) 'Column reordering/multiple selection changed'
    Check ($grid.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::FullRowSelect) 'Selection mode changed'
    Check ($null -eq $form.AcceptButton -and $null -eq $form.CancelButton) 'Do not add Enter/Escape actions'
    $names=@('ID','Status','Company','AccountNumber','ItemLookupCode','Description','QuantityOnOrder','Name','OrderDate','DaysOpenLabel')
    $headers=@('ORDER ID','STATUS','COMPANY','ACCOUNT NUMBER','ITEMLOOKUP','DESCRIPTION','QTY ORDERED','ORDER TAKER','DateCreated','No.Days')
    $table=[Data.DataTable]::new()
    foreach($name in $names){
        $type=if($name -eq 'ID'){[int]}elseif($name -eq 'QuantityOnOrder'){[double]}else{[string]}
        $null=$table.Columns.Add($name,$type)
    }
    $long="O'NEIL SYNTHETIC CUSTOMER "+('LONG COMPANY NAME '*10)
    $null=$table.Rows.Add(202,'Open',$long,'SYNTHETIC-ACCOUNT','SAMPLE-ITEM','Synthetic description',2681.75,'SYNTHETIC ORDER TAKER','10/16/2026','1 day')
    $null=$table.Rows.Add(101,'Open',[DBNull]::Value,[DBNull]::Value,'SAMPLE-ITEM','Synthetic description',0.125,[DBNull]::Value,[DBNull]::Value,'12 days')
    $null=$table.Rows.Add(202,'Open','SECOND LINE SAME ORDER','TEST','SAMPLE-ITEM','Synthetic description',0.5,'OTHER TAKER','10/15/2026','2 days')
    foreach($data in @($table,$table.Clone(),$table)){
        $grid.DataSource=$data
        Check ($grid.Columns.Count -eq 10) 'Rebinding changed or duplicated columns'
        Check ($grid.Rows.Count -eq $data.Rows.Count) 'Rows were filtered/grouped by presentation'
        for($i=0;$i -lt 10;$i++){
            Check ($grid.Columns[$i].DataPropertyName -eq $names[$i] -and $grid.Columns[$i].HeaderText -eq $headers[$i]) 'Column mapping/order/header changed'
            Check ($grid.Columns[$i].Visible -eq ($i -notin @(4,5))) 'Hidden item details changed'
        }
        foreach($row in $grid.Rows){foreach($cell in $row.Cells){$null=$cell.FormattedValue;Check ($errors.Count -eq 0) 'Null/typed cell formatting failed'}}
    }
    Check ([double]$grid.Rows[0].Cells[6].Value -eq 2681.75 -and [double]$grid.Rows[1].Cells[6].Value -eq 0.125) 'Fractional quantity changed'
    Check ($grid.Columns[6].DefaultCellStyle.Format -eq '') 'Do not round or impose a quantity format'
    Check ($grid.Rows[0].Cells[2].Value -eq $long) 'Long customer data was truncated'
    Check ($grid.Rows[0].Cells[8].Value -eq '10/16/2026' -and $grid.Columns[8].DefaultCellStyle.Format -eq '') 'Existing date strings changed'
    $grid.CurrentCell=$grid.Rows[1].Cells[0]
    Check ([int]$grid.CurrentRow.Cells[0].Value -eq 101) 'Selected order ID changed'
    $grid.Sort($grid.Columns[0],[ComponentModel.ListSortDirection]::Ascending)
    Check ([int]$grid.Rows[0].Cells[0].Value -eq 101) 'Existing header sorting must remain available'
    $grid.Columns[3].DisplayIndex=1
    Check ($grid.Columns[0].DataPropertyName -eq 'ID' -and $grid.Columns[6].DataPropertyName -eq 'QuantityOnOrder') 'Reordering must preserve underlying column indices'
    # Bind actual generated LINQ result types too, without querying their SQL function.
    $type=$a.GetType('WorkOrderEntry.SOD_fntbl_CheckOpenWOResult',$true)
    $list=[Activator]::CreateInstance([Collections.Generic.List``1].MakeGenericType($type))
    $item=[Activator]::CreateInstance($type)
    $item.ID=303;$item.Status='Open';$item.QuantityOnOrder=0.375;$item.Description='SYNTHETIC';$item.DaysOpenLabel='3 days'
    $list.Add($item);$grid.DataSource=$list
    Check ($grid.Rows.Count -eq 1 -and $grid.Columns.Count -eq 10) 'Generated LINQ result binding changed'
    Check ([double]$grid.Rows[0].Cells[6].Value -eq 0.375) 'Generated Double quantity changed'
    foreach($cell in $grid.Rows[0].Cells){$null=$cell.FormattedValue;Check ($errors.Count -eq 0) 'Generated result formatting failed'}
    foreach($size in @([Drawing.Size]::new(1332,540),[Drawing.Size]::new(1000,400),[Drawing.Size]::new(1500,650))){$form.ClientSize=$size;Geometry $form}
    $form.Scale([Drawing.SizeF]::new(1.25,1.25));Geometry $form
    Check ($grid.DefaultCellStyle.SelectionBackColor -eq [Drawing.Color]::FromArgb(37,99,160)) 'Selected rows must remain clear'
    Check ($errors.Count -eq 0) ('Unexpected DataErrors: '+($errors -join '; '))
    $form.GetType().GetMethod('cmdClose_Click',$flags).Invoke($form,@($form,[EventArgs]::Empty)) | Out-Null
    Check ($form.IsDisposed) 'Existing Close action must dispose the dialog'
    Write-Output "PASS: $checks isolated Check WO binding, quantity, selection, sorting, layout and Close checks. No SQL or store changes."
}finally{
    $form.Dispose()
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
