# Designer/binding only. Detach production Load before transparent rendering.
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
$directory=Join-Path $repo ('TestResults\InterStoreLayout-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $directory
$checks=0
function Check($ok,$message){if(!$ok){throw $message};$script:checks++}
function C($f,$name){$f.GetType().GetProperty($name,$flags).GetValue($f,$null)}
try {
    foreach($scale in @(1.0,1.25)) {
        $f=[Activator]::CreateInstance($a.GetType('WorkOrderEntry.frmCheckStore',$true))
        try {
            $loadMethod=$f.GetType().GetMethod('frmCheckStore_Load',$flags)
            $loadHandler=[Delegate]::CreateDelegate([EventHandler],$f,$loadMethod)
            $f.GetType().GetEvent('Load').RemoveEventHandler($f,$loadHandler)
            $f.BindingContext=[Windows.Forms.BindingContext]::new()
            $grid=C $f 'gridQty'
            Check ($grid.ReadOnly -and !$grid.AllowUserToAddRows -and !$grid.AllowUserToDeleteRows -and $grid.AllowUserToOrderColumns -and $grid.MultiSelect) 'Grid permissions changed'
            Check ($grid.AllowUserToResizeRows) 'Existing row-resize permission changed'
            Check ($grid.SelectionMode -eq [Windows.Forms.DataGridViewSelectionMode]::FullRowSelect) 'Row selection changed'
            Check ($null -eq $f.AcceptButton -and $null -eq $f.CancelButton -and !$f.KeyPreview) 'Keyboard routing changed'
            Check ($grid.TabIndex -eq 0 -and (C $f 'cmdClose').TabIndex -eq 10 -and (C $f 'cmdClose').DialogResult -eq [Windows.Forms.DialogResult]::None) 'Tab order or Close result changed'
            $table=[Data.DataTable]::new()
            foreach($name in @('ID','Name','StoreCode','Qty','Address')){
                $type=if($name -eq 'ID'){[int]}elseif($name -eq 'Qty'){[double]}else{[string]}
                $null=$table.Columns.Add($name,$type)
            }
            $long='DVO_STORE_PREMIUM_STOCK_DB'
            $null=$table.Rows.Add(4002,'DVO_POS_DB','DVO_POS_DB',[double]1234567.125,'Synthetic hidden address')
            $null=$table.Rows.Add(4005,$long,$long,[double]-3.25,'Synthetic hidden address')
            $null=$table.Rows.Add(4006,'Nullable stock','NULL_TEST',[DBNull]::Value,'Synthetic hidden address')
            foreach($data in @($table,$table.Clone(),$table)) {
                $grid.DataSource=$data
                Check ($grid.Columns.Count -eq 5 -and $grid.RowCount -eq $data.Rows.Count) 'Rebinding changed rows/columns'
                for($i=0;$i -lt 5;$i++) {
                    Check ($grid.Columns[$i].DataPropertyName -eq $table.Columns[$i].ColumnName) 'Column mapping/ordinal changed'
                    Check ($grid.Columns[$i].Visible -eq ($i -ne 4)) 'ID/name/code/qty visibility or hidden Address changed'
                    Check ($grid.Columns[$i].SortMode -eq [Windows.Forms.DataGridViewColumnSortMode]::Automatic) 'Header sorting changed'
                }
            }
            Check ($grid.Rows[0].Cells[3].Value -eq [double]1234567.125 -and $grid.Rows[1].Cells[3].Value -eq [double]-3.25) 'Quantity values changed'
            Check ($grid.Rows[1].Cells[1].Value -eq $long -and $grid.Rows[1].Cells[2].Value -eq $long) 'Store text truncated in data'
            Check ($grid.Rows[0].Cells[3].FormattedValue -eq ([double]1234567.125).ToString([Globalization.CultureInfo]::CurrentCulture) -and $grid.Rows[2].Cells[3].FormattedValue -eq '') 'Decimal or null formatting changed'
            Check ($grid.Columns[3].AutoSizeMode -eq [Windows.Forms.DataGridViewAutoSizeColumnMode]::DisplayedCells -and $grid.Columns[3].MinimumWidth -ge 90) 'Quantity must fit visible values'
            Check ($grid.Font.Size -ge 9.75 -and $grid.RowTemplate.Height -ge 30 -and $grid.ColumnHeadersHeight -ge 32) 'Readability styles missing'
            $grid.CurrentCell=$grid.Rows[1].Cells[1]
            Check ([int]$grid.CurrentRow.Cells[0].Value -eq 4005) 'Selection identity changed'
            $grid.Sort($grid.Columns[0],[ComponentModel.ListSortDirection]::Descending)
            Check ([int]$grid.Rows[0].Cells[0].Value -eq 4006) 'Sorting failed'
            $grid.Columns[2].DisplayIndex=1
            Check ($grid.Columns[0].DataPropertyName -eq 'ID' -and $grid.Columns[3].DataPropertyName -eq 'Qty') 'Reordering changed data ordinals'
            $grid.Columns[2].DisplayIndex=2
            (C $f 'lblItemCode').Text='55159RMS'
            (C $f 'lblItemName').Text='BOOK PAPER IK ONE 8-1/2in x 13in 70gsm 500s (RMS) - SYNTHETIC LONG ITEM DESCRIPTION'
            if($scale -ne 1){$f.Scale([Drawing.SizeF]::new($scale,$scale))}
            $f.PerformLayout()
            foreach($width in @(1024,1366)){Check ($f.Width -le $width -and $f.Height -le 728) 'Dialog exceeds supported screen area'}
            foreach($control in $f.Controls){Check ($control.Left -ge 0 -and $control.Top -ge 0 -and $control.Right -le $f.ClientSize.Width -and $control.Bottom -le $f.ClientSize.Height) ('Clipped: '+$control.Name)}
            Check ($grid.Right -le $grid.Parent.ClientSize.Width -and $grid.Bottom -le $grid.Parent.ClientSize.Height) 'Grid clipped inside group'
            $f.TopMost=$false; $f.ShowInTaskbar=$false; $f.Opacity=0
            $f.Show(); [Windows.Forms.Application]::DoEvents()
            $image=[Drawing.Bitmap]::new($f.Width,$f.Height)
            try{$f.DrawToBitmap($image,[Drawing.Rectangle]::new(0,0,$f.Width,$f.Height));$image.Save((Join-Path $directory ('InterStore-'+$scale+'.png')))}
            finally{$image.Dispose()}
        } finally {$f.Dispose()}
    }
    "PASS: $checks isolated InterStore layout/binding checks. Production Load detached; no SQL. Renders: $directory"
} finally {
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}
}
