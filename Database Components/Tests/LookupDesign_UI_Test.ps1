# Presentation and binding only. Never show/load the forms, search SQL or invoke
# customer/sales selection: those paths apply pricing/tax/account information.
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
$a=[Reflection.Assembly]::LoadFrom($app);$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$errors=[Collections.Generic.List[string]]::new();$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$n){$f.GetType().GetProperty($n,$flags).GetValue($f,$null)}
try{
    foreach($name in @('frmCustomer','frmSalesRep')){
        $f=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.'+$name))
        try{
            $f.BindingContext=[Windows.Forms.BindingContext]::new()
            $customer=$name -eq 'frmCustomer';$grid=C $f $(if($customer){'gridCustomer'}else{'gridSalesRep'})
            $grid.add_DataError([Windows.Forms.DataGridViewDataErrorEventHandler]{param($s,$e)
                $errors.Add($e.Exception.Message);$e.ThrowException=$false
            })
            Check ($grid.ReadOnly -and !$grid.AllowUserToAddRows -and !$grid.AllowUserToDeleteRows -and $grid.MultiSelect -and $grid.AllowUserToOrderColumns) 'Lookup permissions changed'
            Check ($grid.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::FullRowSelect) 'Row selection changed'
            Check ($null -eq $f.AcceptButton -and $null -eq $f.CancelButton) 'Do not introduce new Enter/Escape actions'
            Check ((C $f 'cmdOk').TabIndex -eq $(if($customer){3}else{8}) -and (C $f 'cmdCancel').TabIndex -eq $(if($customer){2}else{7})) 'Button tab indices changed'
            $names=if($customer){@('AccountNumber','Company','PriceLevel','Employee','Title','TaxExempt','ID','CustomText2','CustomText4')}else{@('ID','Name')}
            $table=[Data.DataTable]::new()
            foreach($n in $names){
                $t=if($n -eq 'ID'){[int]}elseif($n -eq 'PriceLevel'){[int16]}elseif($n -in @('Employee','TaxExempt')){[bool]}else{[string]}
                $null=$table.Columns.Add($n,$t)
            }
            $long="O'NEIL SYNTHETIC "+('LONG DISPLAY NAME '*14)
            if($customer){
                $null=$table.Rows.Add('000123',$long,[int16]2,$true,'TEST TITLE',$false,202,'TEST HIDDEN','TEST CUSTOMER TYPE')
                $null=$table.Rows.Add('000007',[DBNull]::Value,[int16]0,[DBNull]::Value,'',[DBNull]::Value,101,[DBNull]::Value,[DBNull]::Value)
            }else{$null=$table.Rows.Add(202,$long);$null=$table.Rows.Add(101,[DBNull]::Value)}
            foreach($data in @($table,$table.Clone(),$table)){
                $grid.DataSource=$data
                $f.GetType().GetMethod('gridCustomerWidth',$flags).Invoke($f,$null) | Out-Null
                Check ($grid.Columns.Count -eq $names.Count -and $grid.Rows.Count -eq $data.Rows.Count) 'Rebinding duplicated columns or changed records'
                for($i=0;$i -lt $names.Count;$i++){
                    Check ($grid.Columns[$i].DataPropertyName -eq $names[$i]) 'Column ordinal mapping changed'
                    Check ($grid.Columns[$i].Visible -eq (!$customer -or $i -in @(0,1))) 'Customer search must display only Account Number and Company'
                }
                foreach($row in $grid.Rows){foreach($cell in $row.Cells){$null=$cell.FormattedValue;Check ($errors.Count -eq 0) 'Formatting raised DataError'}}
            }
            Check ($grid.Rows[0].Cells[1].Value -eq $long) 'Long name data was truncated'
            Check ($grid.Columns[1].AutoSizeMode -eq [Windows.Forms.DataGridViewAutoSizeColumnMode]::Fill) 'Name column should use available space'
            $id=if($customer){6}else{0}
            $grid.CurrentCell=$grid.Rows[1].Cells[0]
            Check ([int]$grid.CurrentRow.Cells[$id].Value -eq 101) 'Selected ID changed'
            if($customer){
                Check ($grid.Rows[0].Cells[0].Value -ceq '000123') 'Account leading zeroes changed'
                Check ([int16]$grid.Rows[0].Cells[2].Value -eq 2 -and $grid.Rows[0].Cells[3].Value -eq $true -and $grid.Rows[0].Cells[5].Value -eq $false) 'Pricing/employee/tax values changed'
                foreach($i in @(3,5)){
                    Check ($grid.Columns[$i] -is [Windows.Forms.DataGridViewCheckBoxColumn] -and $grid.Rows[1].Cells[$i].FormattedValue -is [bool]) 'Blank Boolean fields must retain checkbox formatting'
                }
            }else{Check ($grid.Columns[0].Width -eq 75) 'Sales Rep ID should leave more room for the name'}
            $grid.Sort($grid.Columns[0],[ComponentModel.ListSortDirection]::Ascending)
            Check ([int]$grid.Rows[0].Cells[$id].Value -eq 101) 'Header sorting changed'
            $grid.Columns[1].DisplayIndex=0
            Check ($grid.Columns[$id].DataPropertyName -eq 'ID') 'Visual reordering changed the underlying selected-ID column'
            $f.Scale([Drawing.SizeF]::new(1.25,1.25))
            Check ($f.Width -lt 1024 -and $f.Height -lt 728) 'Lookup should fit 1024x768 at simulated 125%'
            foreach($c in $f.Controls){Check ($c.Left -ge 0 -and $c.Top -ge 0 -and $c.Right -le $f.ClientSize.Width -and $c.Bottom -le $f.ClientSize.Height) ('Clipped: '+$c.Name)}
        }finally{$f.Dispose()}
    }
    $f=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.frmItemLookUp'))
    try{
        $top=@('cmdSelect','cmdRefresh','cmdInterStore','cmdPriceLevel','cmdSettings','cmdOpenWO','Button2','btnTax')
        $bottom=@('cmdForInvoice','cmdImport','Button1','cmdOk','cmdRecall','cmdRemove','cmdCancel','cmdClose')
        foreach($n in @($top+$bottom)){
            $b=C $f $n
            Check ($b.Dock -eq $(if($n -in $top){[Windows.Forms.DockStyle]::Top}else{[Windows.Forms.DockStyle]::Bottom})) 'Button group/dock changed'
            Check ($b.Height -eq $(if($n -eq 'cmdImport'){34}else{33}) -and $b.Width -eq 111 -and $null -ne $b.Image) ('Button bounds or icon changed: '+$n)
            Check ($b.Text -eq $b.Text.Trim()) 'Avoid space-padding captions; use image/text layout'
        }
        Check ((C $f 'cmdOk').Text -eq 'Save [F12]' -and (C $f 'cmdRecall').Text -eq 'Recall [F11]' -and (C $f 'cmdImport').Text -eq 'Import [F6]') 'Shortcut hints changed'
        Check ((C $f 'cmdOk').BackColor -ne (C $f 'cmdCancel').BackColor) 'Save and Cancel should be visually distinct'
        Check ((C $f 'btnTax').Bottom -eq (C $f 'Button2').Top) 'Set Tax must be directly above Logout'
        (C $f 'btnTax').Visible=$false;(C $f 'panelButtons').PerformLayout()
        Check ((C $f 'Button2').Top -eq (C $f 'cmdOpenWO').Bottom) 'Logout must stay last in the top group when Set Tax is hidden'
    }finally{$f.Dispose()}
    Check ($errors.Count -eq 0) 'Unexpected lookup formatting errors'
    Write-Output "PASS: $checks isolated Customer/Sales Rep mapping, selection, formatting and button-design checks. No SQL or business actions."
}finally{
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
