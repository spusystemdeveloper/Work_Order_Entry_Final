# Bound-grid rendering and existing empty-selection handlers; no SQL or app Load.
param([string]$ApplicationPath)
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
 & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath -ApplicationPath $ApplicationPath
 exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Windows.Forms,System.Drawing,System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if(-not $ApplicationPath){$ApplicationPath=Join-Path $repo 'Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\EXE-Only\Work Order Entry_V4.exe'}
$asm=[Reflection.Assembly]::LoadFrom($ApplicationPath)
$instance=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$static=[Reflection.BindingFlags]'Public,NonPublic,Static'
$module=$asm.GetType('WorkOrderEntry.ModConnectDB',$true)
$output=Join-Path $repo ('TestResults\V4EmptyResults-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
[void](New-Item -ItemType Directory $output)
$checks=0;$forms=@();$hosts=@();$configs=@{}
foreach($relative in @('Work Order Entry\bin\Debug\Work Order Entry_V8.exe.config','Work Order Entry\bin\Release\Work Order Entry_V8.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Debug\Work Order Entry_V4.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe.config')){
 $path=Join-Path $repo $relative;if(Test-Path -LiteralPath $path){$configs[$path]=(Get-FileHash -LiteralPath $path).Hash}
}
function Check($value,$message){if(-not $value){throw "FAILED: $message"};$script:checks++;"PASS: $message"}
function Control($form,$name){$form.GetType().GetProperty($name,$instance).GetValue($form,$null)}
function Call($form,$name,$arguments){[void]$form.GetType().GetMethod($name,$instance).Invoke($form,$arguments)}
function State($name,$value){$module.GetField($name,$static).SetValue($null,$value)}
function Global($name){$module.GetField($name,$static).GetValue($null)}
function DrawMessage($form,$grid,$method,$save){
 $bitmap=[Drawing.Bitmap]::new($grid.Width,$grid.Height);$graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{
  $graphics.Clear([Drawing.Color]::White)
  $event=[Windows.Forms.PaintEventArgs]::new($graphics,$grid.ClientRectangle)
  Call $form $method @($grid,$event)
  $pixels=0;$headerPixels=0
  for($y=0;$y -lt $bitmap.Height;$y++){for($x=0;$x -lt $bitmap.Width;$x++){if($bitmap.GetPixel($x,$y).ToArgb() -ne [Drawing.Color]::White.ToArgb()){$pixels++;if($y -le $grid.ColumnHeadersHeight){$headerPixels++}}}}
  if($save){$bitmap.Save((Join-Path $output $save))}
  return @{Pixels=$pixels;HeaderPixels=$headerPixels}
 }finally{$graphics.Dispose();$bitmap.Dispose()}
}
try{
 State 'iSalesID' 77;State 'iCusID' 88;State 'iRowSales' 99
 foreach($spec in @(@('frmCustomer','gridCustomer'),@('frmSalesRep','gridSalesRep'),@('frmItemLookUp','gridItem'))){
  $form=[Activator]::CreateInstance($asm.GetType('WorkOrderEntry.'+$spec[0]));$forms+=,$form
  $grid=Control $form $spec[1];$grid.BindingContext=[Windows.Forms.BindingContext]::new()
  $hostForm=[Windows.Forms.Form]::new();$hosts+=,$hostForm
  $hostForm.ShowInTaskbar=$false;$hostForm.StartPosition='Manual';$hostForm.Location=[Drawing.Point]::new(-20000,-20000)
  $hostForm.ClientSize=[Drawing.Size]::new(640,260);$hostForm.Controls.Add($grid);$grid.Dock='Fill';$hostForm.Show()
  $method=$spec[1]+'_EmptyResultsPaint'
  $grid.DataSource=$null;$paint=DrawMessage $form $grid $method $null
  Check ($paint.Pixels -eq 0) ($spec[0]+': unbound/loading state has no false no-results message')
  $table=[Data.DataTable]::new();[void]$table.Columns.Add('ID',[int]);[void]$table.Columns.Add('Name',[string])
  $grid.DataSource=$table;[Windows.Forms.Application]::DoEvents()
  Check ($grid.RowCount -eq 0 -and $grid.CurrentRow -eq $null) ($spec[0]+': empty binding has no selectable placeholder')
  $paint=DrawMessage $form $grid $method ($spec[0]+'-empty.png')
  Check ($paint.Pixels -gt 0 -and $paint.HeaderPixels -eq 0) ($spec[0]+': message paints inside empty grid body')
  Check ($table.Rows.Count -eq 0 -and $grid.RowCount -eq 0) ($spec[0]+': paint leaves search result data unchanged')
  if($spec[0] -eq 'frmSalesRep'){
   foreach($handler in @('cmdOk_Click','gridSalesRep_DoubleClick')){Call $form $handler @($grid,[EventArgs]::Empty);Check ((Global 'iSalesID') -eq 77 -and -not $form.IsDisposed) ('empty Sales Rep '+$handler+' safely keeps current selection')}
   $key=[Windows.Forms.KeyPressEventArgs]::new([char]13);Call $form 'gridSalesRep_KeyPress' @($grid,$key)
   Check ((Global 'iSalesID') -eq 77 -and -not $form.IsDisposed) 'empty Sales Rep Enter safely keeps current selection'
  }elseif($spec[0] -eq 'frmCustomer'){
   Call $form 'cmdOk_Click' @($grid,[EventArgs]::Empty)
   $key=[Windows.Forms.KeyEventArgs]::new([Windows.Forms.Keys]::Enter);Call $form 'gridCustomer_KeyDown' @($grid,$key)
   Call $form 'gridCustomer_CellDoubleClick' @($grid,[Windows.Forms.DataGridViewCellEventArgs]::new(0,-1))
   Check ((Global 'iCusID') -eq 88 -and -not $form.IsDisposed) 'empty Customer OK/Enter/header click retain selection without error'
  }else{
   Call $form 'gridItem_DoubleClick' @($grid,[EventArgs]::Empty)
   Check ((Global 'iCusID') -eq 88 -and (Control $form 'gridSelectItem').RowCount -eq 0) 'empty item double click adds no item or customer change'
  }
  [void]$table.Rows.Add(11,'Fixture result');[Windows.Forms.Application]::DoEvents()
  $paint=DrawMessage $form $grid $method $null
  Check ($paint.Pixels -eq 0 -and $grid.RowCount -eq 1 -and $table.Rows[0]['Name'] -eq 'Fixture result') ($spec[0]+': returning results removes message without changing data')
  $table.Rows.Clear();[Windows.Forms.Application]::DoEvents();$paint=DrawMessage $form $grid $method $null
  Check ($paint.Pixels -gt 0) ($spec[0]+': message returns when rebound results are empty')
 }
 foreach($spec in @(@('frmCustomer','txtCompany_KeyPress'),@('frmCustomer','gridCustomer_KeyPress'),@('frmSalesRep','txtSales_KeyPress'),@('frmSalesRep','gridSalesRep_KeyPress'))){
  $dialog=[Activator]::CreateInstance($asm.GetType('WorkOrderEntry.'+$spec[0]));$forms+=,$dialog
  Call $dialog $spec[1] @($null,[Windows.Forms.KeyPressEventArgs]::new([char]27))
  Check ($dialog.IsDisposed -and (Global 'iSalesID') -eq 77 -and (Global 'iCusID') -eq 88) ($spec[1]+': existing Esc closes without changing selection')
 }
 foreach($name in @('db','dbnew','dbInitial')){$context=Global $name;Check ($context.Connection.State -eq 'Closed') ('application SQL connection stayed closed: '+$name)}
 foreach($path in $configs.Keys){Check ((Get-FileHash -LiteralPath $path).Hash -eq $configs[$path]) ('configuration preserved: '+[IO.Path]::GetFileName($path))}
 "PASS: $checks V4 empty-result UI checks; no SQL, production Load, saves or print; images: $output"
}finally{
 foreach($hostForm in $hosts){$hostForm.Dispose()};foreach($form in $forms){$form.Dispose()}
 foreach($path in $configs.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $configs[$path]){throw 'Application config changed'}}
}