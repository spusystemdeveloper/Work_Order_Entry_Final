# Compiled import UI checks using synthetic data. No file imports, store writes,
# MySQL refresh, duplicate-order SQL checks, printing or import-button execution.
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
$assembly=[Reflection.Assembly]::LoadFrom($app)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$forms=[Collections.Generic.List[Windows.Forms.Form]]::new()
$errors=[Collections.Generic.List[string]]::new()
$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function Get-Control($form,$name){$form.GetType().GetProperty($name,$flags).GetValue($form,$null)}
function New-Form($name){
    $f=[Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.'+$name,$true))
    $forms.Add($f);$f.BindingContext=[Windows.Forms.BindingContext]::new();return $f
}
function Observe-Errors($grid){
    $grid.add_DataError([Windows.Forms.DataGridViewDataErrorEventHandler]{param($s,$e)
        $errors.Add($e.Exception.Message);$e.ThrowException=$false
    })
}
function In-Bounds($parent,$child){
    Check ($child.Left -ge 0 -and $child.Top -ge 0 -and $child.Right -le $parent.ClientSize.Width -and $child.Bottom -le $parent.ClientSize.Height) ('Out of bounds: '+$child.Name)
}
try{
    $website=New-Form 'frmWebsiteImport'
    $grid=Get-Control $website 'gridOrders';Observe-Errors $grid
    Check ($grid.ReadOnly -and !$grid.MultiSelect -and !$grid.AllowUserToAddRows -and !$grid.AllowUserToDeleteRows) 'Website selection/edit permissions changed'
    Check ($grid.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::FullRowSelect) 'Website selection mode changed'
    Check ($null -eq $website.AcceptButton -and $null -eq $website.CancelButton) 'Do not add Enter/Escape import actions'
    Check (!(Get-Control $website 'chkShowAll').Checked) 'Show All default changed'
    Check ((Get-Control $website 'txtSearch').TabIndex -eq 4 -and (Get-Control $website 'btnImport').TabIndex -eq 2) 'Website tab order changed'
    $data=[Data.DataTable]::new()
    $null=$data.Columns.Add('OrderID',[int]);$null=$data.Columns.Add('CustomerName',[string]);$null=$data.Columns.Add('OrderPlaced',[datetime])
    $long='SYNTHETIC LONG CUSTOMER NAME '+('DISPLAY TEST '*12)
    $null=$data.Rows.Add(100,$long,[datetime]'2026-10-15T09:15:00')
    $null=$data.Rows.Add(100,$long,[datetime]'2026-10-15T10:15:00')
    $null=$data.Rows.Add(200,"O'NEIL TEST",[datetime]'2026-10-16T11:20:00')
    $null=$data.Rows.Add(300,[DBNull]::Value,[DBNull]::Value)
    $import=$assembly.GetType('WorkOrderEntry.clsImport',$true)
    $field=$import.GetField('ImportDataCSV',[Reflection.BindingFlags]'Public,Static')
    $original=$field.GetValue($null);$field.SetValue($null,$data)
    # Show All bypasses duplicate SQL lookups. Never invoke the Load handler,
    # Refresh, Import or the Show All=False query in this isolated fixture.
    (Get-Control $website 'chkShowAll').Checked=$true
    $search=Get-Control $website 'txtSearch'
    foreach($term in @('',"O'NEIL",'100','MISSING')){
        $search.Text=$term
        $website.GetType().GetMethod('loadImports',$flags).Invoke($website,$null) | Out-Null
        $expected=@($data.Rows | Where-Object {$_.OrderID.ToString().Contains($term) -or $_.CustomerName.ToString().Contains($term)} | ForEach-Object {$_.OrderID} | Select-Object -Unique)
        Check ($grid.Columns.Count -eq 3) 'Rebinding duplicated/reordered website columns'
        Check ($grid.Rows.Count -eq $expected.Count) 'Existing grouped search result count changed'
        Check ($grid.Columns[0].DataPropertyName -eq 'OrderID' -and $grid.Columns[1].DataPropertyName -eq 'CustomerName' -and $grid.Columns[2].DataPropertyName -eq 'DateOrder') 'Website property/ordinal mapping changed'
        for($i=0;$i -lt $grid.Rows.Count;$i++){
            $grid.CurrentCell=$grid.Rows[$i].Cells[0]
            Check ([int]$grid.CurrentRow.Cells[0].Value -eq $expected[$i]) 'Selected import ID changed'
            foreach($cell in $grid.Rows[$i].Cells){$null=$cell.FormattedValue;Check ($errors.Count -eq 0) 'Website formatting error'}
        }
    }
    $search.Text='100';$website.GetType().GetMethod('loadImports',$flags).Invoke($website,$null) | Out-Null
    Check ([datetime]$grid.Rows[0].Cells[2].Value -eq [datetime]'2026-10-15T10:15:00') 'Grouped date value changed'
    Check ($grid.Columns[2].DefaultCellStyle.Format -eq '') 'Do not impose a new date format'
    Check ($grid.Rows[0].Cells[1].Value -eq $long) 'Long customer text was truncated in data'
    Check ($grid.AutoSizeRowsMode -eq [Windows.Forms.DataGridViewAutoSizeRowsMode]::DisplayedCellsExceptHeaders) 'Long rows should wrap without sizing every record'
    foreach($c in $website.Controls){In-Bounds $website $c}
    $uploader=New-Form 'frmUploader';$details=Get-Control $uploader 'gridInfo';Observe-Errors $details
    Check ((Get-Control $uploader 'cmdImport').Text -eq '&Import' -and (Get-Control $uploader 'cmdImport').TabIndex -eq 5) 'CSV Import mnemonic/tab order changed'
    Check (!(Get-Control $uploader 'cmdReport').Enabled -and !(Get-Control $uploader 'cmdReport').Visible) 'CSV Report initial permissions changed'
    Check ((Get-Control $uploader 'cmdClose').DialogResult -eq [Windows.Forms.DialogResult]::Cancel) 'CSV Close result changed'
    $uploader.GetType().GetMethod('GridDesign',$flags).Invoke($uploader,$null) | Out-Null
    Check ($details.Columns.Count -eq 4) 'CSV detail columns changed'
    Check (($details.Columns | ForEach-Object {$_.HeaderText}) -join '|' -eq 'Row #|Col #|Data|Message') 'CSV detail ordinals changed'
    $row=$details.Rows.Add(6,1,'SYNTHETIC ITEM',('Synthetic validation message '*12))
    Check ($details.Rows[$row].Cells[3].Value.Length -gt 200) 'Long CSV message lost'
    Check ($details.Columns[0].Width -eq 74 -and $details.Columns[1].Width -eq 74) 'Row/column numbers must leave space for details'
    Check ($details.Columns[3].MinimumWidth -ge 320 -and $details.Columns[3].FillWeight -gt $details.Columns[2].FillWeight) 'Validation messages need the larger share of detail space'
    foreach($cell in $details.Rows[$row].Cells){$null=$cell.FormattedValue;Check ($errors.Count -eq 0) 'CSV detail formatting error'}
    Check ($details.DefaultCellStyle.SelectionBackColor -eq [Drawing.Color]::FromArgb(37,99,160)) 'Runtime CSV style differs from Designer selection'
    Check ($details.Font.Size -eq 10 -and $details.RowTemplate.Height -eq 40) 'Runtime CSV font/spacing differs from layout'
    foreach($p in @('Panel1','Panel2','Panel3')){In-Bounds $uploader (Get-Control $uploader $p)}
    foreach($n in @('cmdReport','cmdImport','cmdClose')){In-Bounds (Get-Control $uploader 'Panel2') (Get-Control $uploader $n)}
    $missing=New-Form 'frmImportItems';$items=Get-Control $missing 'griditems';Observe-Errors $items
    Check ($items.Columns.Count -eq 1 -and $items.ReadOnly -and !$items.MultiSelect) 'Missing-item list semantics changed'
    $row=$items.Rows.Add('SYNTHETIC-ITEM - '+('Long description '*25))
    Check ($items.Rows[$row].Cells[0].Value.Length -gt 300) 'Missing-item description lost'
    foreach($c in $missing.Controls){In-Bounds $missing $c}
    foreach($f in $forms){$f.Scale([Drawing.SizeF]::new(1.25,1.25));foreach($c in $f.Controls){In-Bounds $f $c}}
    Check ($errors.Count -eq 0) ('Unexpected DataError: '+($errors -join '; '))
    Write-Output "PASS: $checks isolated Import layout/filter/selection/formatting checks; no SQL, MySQL or import/save execution."
}finally{
    if($field){$field.SetValue($null,$original)}
    foreach($f in $forms){$f.Dispose()}
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
