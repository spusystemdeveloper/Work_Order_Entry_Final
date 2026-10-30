# Actual compiled V4 recall update methods against owned synthetic rows only.
param([string]$ApplicationPath)
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
 $arguments=@('-NoProfile','-STA','-ExecutionPolicy','Bypass','-File',$PSCommandPath)
 if($ApplicationPath){$arguments+=@('-ApplicationPath',$ApplicationPath)}
 & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" @arguments
 exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq,System.Windows.Forms,System.Drawing
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if(-not $ApplicationPath){$ApplicationPath=Join-Path $repo 'Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\EXE-Only\Work Order Entry_V4.exe'}
$asm=[Reflection.Assembly]::LoadFrom([IO.Path]::GetFullPath($ApplicationPath))
$static=[Reflection.BindingFlags]'Public,NonPublic,Static'
$instance=[Reflection.BindingFlags]'Public,NonPublic,Instance'
$module=$asm.GetType('WorkOrderEntry.ModConnectDB',$true)
$recall=$asm.GetType('WorkOrderEntry.clsRecall',$true)
$server='(localdb)\MSSQLLocalDB';$database='SOD_V4RecallTax_'+[Guid]::NewGuid().ToString('N')
$cx=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20")
$created=$false;$main=$null;$checks=0;$configs=@{}
foreach($relative in @('Work Order Entry\bin\Debug\Work Order Entry_V8.exe.config','Work Order Entry\bin\Release\Work Order Entry_V8.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Debug\Work Order Entry_V4.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe.config')){
 $path=Join-Path $repo $relative;if(Test-Path -LiteralPath $path){$configs[$path]=(Get-FileHash -LiteralPath $path).Hash}
}
function Sql($sql){$cmd=$cx.CreateCommand();try{$cmd.CommandText=$sql;$cmd.CommandTimeout=20;[void]$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
function Scalar($sql){$cmd=$cx.CreateCommand();try{$cmd.CommandText=$sql;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}}
function Check($value,$message){if(-not $value){throw "FAILED: $message"};$script:checks++;"PASS: $message"}
function State($name,$value){$module.GetField($name,$static).SetValue($null,$value)}
function Global($name){$module.GetField($name,$static).GetValue($null)}
function Control($name){$main.GetType().GetProperty($name,$instance).GetValue($main,$null)}
function ResetContext{
 foreach($name in @('db','dbnew','dbInitial')){
  $old=Global $name;if($old){$old.Dispose()}
  State $name ([Activator]::CreateInstance($asm.GetType('WorkOrderEntry.ItemLookUpDataContext'),@($script:endpoint)))
 }
}
function SaveRecall($orderID,$entryID,$tax,$total,$taxable){
 $recall.GetMethod('UpdateOrder',$static).Invoke($null,@([int]$orderID,501,11,[double]$tax,[double]$total,'CASH; Pick-up; Fixture')) | Out-Null
 $recall.GetMethod('UpdateOrderEntry',$static).Invoke($null,@([int]$orderID,101,11,[double]50,[double]112,[double]112,2,[int]$taxable,'Fixture item','STORE',0,[int]$entryID)) | Out-Null
}
try{
 $cx.Open();Sql "CREATE DATABASE [$database]";$created=$true;$cx.ChangeDatabase($database)
 # Use only selected mappings, excluding connection metadata. This also gives
 # real rowversion columns to exercise the unchanged optimistic update checks.
 $mapping=[xml](Get-Content (Join-Path $repo 'Work Order Entry V4\Work Order Entry_4\Work Order Entry\Linq\ItemLookUp.dbml') -Raw)
 foreach($table in $mapping.Database.Table | Where-Object {$_.Member -in @('Customers','Orders','OrderEntries','Items','Queueings','QueueingItems')}){
  $columns=@()
  foreach($column in $table.Type.Column){
   $type=[string]$column.DbType;$identity=$type -match '\bIDENTITY\b'
   if($identity){$type=$type -replace '\s+IDENTITY','' -replace '\s+NOT NULL','';$type+=' IDENTITY(1,1) NOT NULL'}
   $definition='['+$column.Name+'] '+$type
   if($column.IsPrimaryKey -eq 'true'){$definition+=' PRIMARY KEY'}
   if(-not $identity -and $type -notmatch 'rowversion|timestamp' -and $type -match 'NOT NULL'){
    $default=if($type -match '^(N?VarChar|N?Char|NText|Text)'){"N''"}elseif($type -match '^(SmallDateTime|DateTime|Date)'){"'2026-01-01'"}elseif($type -match 'Binary|Image'){'0x'}else{'0'}
    $definition+=' DEFAULT ('+$default+')'
   }
   $columns+=$definition
  }
  Sql ('CREATE TABLE '+$table.Name+' ('+($columns -join ',')+');')
 }
 Sql @'
SET IDENTITY_INSERT dbo.Customer ON;
INSERT dbo.Customer(ID,Company,TaxExempt) VALUES(501,N'Fixture customer',0);
SET IDENTITY_INSERT dbo.Customer OFF;
SET IDENTITY_INSERT dbo.Item ON;
INSERT dbo.Item(ID,ItemLookupCode,Description,ItemType,Quantity,QuantityCommitted) VALUES(101,N'FIXTURE',N'Fixture item',0,100,2);
SET IDENTITY_INSERT dbo.Item OFF;
SET IDENTITY_INSERT dbo.[Order] ON;
INSERT dbo.[Order](ID,StoreID,[Type],CustomerID,SalesRepID,Tax,Total,Taxable) VALUES(100,1,2,501,11,24,224,0),(200,1,3,501,11,24,224,0);
SET IDENTITY_INSERT dbo.[Order] OFF;
SET IDENTITY_INSERT dbo.OrderEntry ON;
INSERT dbo.OrderEntry(ID,StoreID,OrderID,ItemID,Cost,FullPrice,Price,QuantityOnOrder,SalesRepID,Taxable,Description) VALUES(1001,1,100,101,50,112,112,2,11,0,N'Fixture item'),(2001,1,200,101,50,112,112,2,11,0,N'Fixture item');
SET IDENTITY_INSERT dbo.OrderEntry OFF;
SET IDENTITY_INSERT dbo.Queueing ON;
INSERT dbo.Queueing(id,OrderID,Status,GroupTo,OPIS) VALUES(500,100,0,100,N'Fixture');
SET IDENTITY_INSERT dbo.Queueing OFF;
SET IDENTITY_INSERT dbo.QueueingItems ON;
INSERT dbo.QueueingItems(id,QueueingID,ItemID,QtyPre,Picker,Status,PickLoc) VALUES(600,500,101,0,N'',N'Pending',N'STORE');
SET IDENTITY_INSERT dbo.QueueingItems OFF;
'@
 foreach($name in @('db','dbnew','dbInitial')){$old=Global $name;if($old){if($old.Connection.State -ne 'Closed'){throw 'Unexpected open application connection'};$old.Dispose()};State $name $null}
 State 'dbs' ($server+';Integrated Security=True;Pooling=False;Application Name=V4RecallTaxOwnedFixture')
 State 'dbn' $database;State 'dbu' '';State 'dbp' '';State 'iSalesID' 11;State 'iCusID' 501
 $script:endpoint=[string]$module.GetMethod('DB_Conn',$static).Invoke($null,@('constr'))
 $guard=[Data.SqlClient.SqlConnectionStringBuilder]::new($endpoint)
 if($guard.DataSource -ne $server -or $guard.InitialCatalog -ne $database -or -not $guard.IntegratedSecurity){throw 'Owned localhost fixture target required'}
 ResetContext
 $forms=$asm.GetType('WorkOrderEntry.My.MyProject').GetProperty('Forms',$static).GetValue($null,$null)
 $main=$forms.GetType().GetProperty('frmItemLookUp',$instance).GetValue($forms,$null)
 $grid=Control 'gridSelectItem';$grid.AllowUserToAddRows=$false;$row=$grid.Rows.Add()
 $grid.Rows[$row].Cells['Price'].Value=[double]112;$grid.Rows[$row].Cells['QTY'].Value=2;$grid.Rows[$row].Cells[10].Value=1
 $initialErrors=Global 'ErrorCount'
 State 'bTaxExcempt' $false;$main.UpdateAmt()
 Check ((Control 'txtVat').Text -eq '24.00' -and (Control 'txtTotal').Text -eq '224.00') 'existing taxable calculation is unchanged'
 SaveRecall 100 1001 24 224 1
 Check ([bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=100')) 'recalled order repairs stale header flag with positive VAT'
 Check ((Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=1001') -eq 1) 'existing recalled line stores supplied taxable flag'
 Check ((Scalar 'SELECT Tax FROM dbo.[Order] WHERE ID=100') -eq 24 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=100') -eq 224) 'VAT amount and total preserved'
 Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=101') -eq 100 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=101') -eq 2) 'physical stock and unchanged-quantity commitments preserved'
 Check ((Scalar 'SELECT Status FROM dbo.Queueing WHERE id=500') -eq 0 -and (Scalar 'SELECT Status FROM dbo.QueueingItems WHERE id=600') -eq 'Pending') 'queue lifecycle stays active and pending'
 Sql 'UPDATE dbo.Customer SET TaxExempt=1 WHERE ID=501';ResetContext
 State 'bTaxExcempt' $true;$main.UpdateAmt()
 Check ((Control 'txtVat').Text -eq '0.00' -and (Control 'txtTotal').Text -eq '224.00') 'customer exemption retains zero VAT and original total'
 SaveRecall 100 1001 0 224 0
 Check (-not [bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=100') -and (Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=1001') -eq 0) 'exempt customer clears header and existing-line flags'
 Check ((Scalar 'SELECT Tax FROM dbo.[Order] WHERE ID=100') -eq 0 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=100') -eq 224) 'exempt save does not deduct tax from inclusive total'
 Sql 'UPDATE dbo.Customer SET TaxExempt=0 WHERE ID=501';ResetContext
 State 'bTaxExcempt' $false;$main.UpdateAmt();SaveRecall 100 1001 24 224 1
 Check ([bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=100') -and (Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=1001') -eq 1) 'taxable customer restores both flags on another recall save'
 SaveRecall 100 1001 24 224 1
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order]') -eq 2 -and (Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry') -eq 2) 'repeated save updates existing records without extra inserts'
 Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=101') -eq 100 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=101') -eq 2) 'repeat save does not change stock or duplicate commitments'
 ResetContext;SaveRecall 200 2001 24 224 1
 Check ([bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=200') -and (Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=2001') -eq 1) 'existing quotation update uses same supplied tax values without stock writes'
 ResetContext;SaveRecall 200 2001 0 224 0
 Check (-not [bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=200') -and (Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=2001') -eq 0) 'existing quotation exemption remains respected'
 ResetContext;SaveRecall 200 2001 0 224 1
 Check (-not [bool](Scalar 'SELECT Taxable FROM dbo.[Order] WHERE ID=200') -and (Scalar 'SELECT Taxable FROM dbo.OrderEntry WHERE ID=2001') -eq 1) 'zero VAT retains old header amount rule and independently supplied line flag'
 Check ((Global 'ErrorCount') -eq $initialErrors) 'actual recall calls completed without caught application errors'
 # Stage the same mapped header property as V4's form, then use its unchanged
 # GSC-compatible header/line update methods. Only owned fixture rows are used.
 Sql "UPDATE dbo.[Order] SET ShippingNotes=N'Deliver to reception [OTHER:KEEP]' WHERE ID=100;"
 ResetContext
 (Control 'chkWorkOrder').Checked=$true;(Control 'chkQuote').Checked=$false
 $main.GetType().GetField('walkInCustomer',$instance).SetValue($main,$true)
 $main.GetType().GetField('walkInRatesCustomerID',$instance).SetValue($main,501)
 $rates=[Collections.Generic.List[int]]::new();$rates.Add(1);$rates.Add(2)
 $main.GetType().GetField('walkInRates',$instance).SetValue($main,$rates)
 $header=(Global 'db').Orders | Where-Object {$_.ID -eq 100}
 $header.ShippingNotes=$main.GetType().GetMethod('ShippingNotesWithWithholding',$instance).Invoke($main,@($header.ShippingNotes))
 SaveRecall 100 1001 24 224 1
 Check ((Scalar 'SELECT CONVERT(nvarchar(max),ShippingNotes) FROM dbo.[Order] WHERE ID=100') -eq ('Deliver to reception [OTHER:KEEP]'+[Environment]::NewLine+'[WO-WTAX:v1;RATES=1,2]')) 'V4 existing header save retains selection and unrelated shipping notes'
 ResetContext
 $main.GetType().GetMethod('LoadWalkInWithholding',$instance).Invoke($main,@(100)) | Out-Null
 $restored=$main.GetType().GetField('walkInRates',$instance).GetValue($main)
 Check (($restored -join ',') -eq '1,2') 'V4 recall loader restores saved rates'
 $main.GetType().GetField('walkInRates',$instance).SetValue($main,[Collections.Generic.List[int]]::new())
 $main.GetType().GetField('walkInRatesCustomerID',$instance).SetValue($main,501)
 $header=(Global 'db').Orders | Where-Object {$_.ID -eq 100}
 $header.ShippingNotes=$main.GetType().GetMethod('ShippingNotesWithWithholding',$instance).Invoke($main,@($header.ShippingNotes))
 SaveRecall 100 1001 24 224 1
 Check ((Scalar 'SELECT CONVERT(nvarchar(max),ShippingNotes) FROM dbo.[Order] WHERE ID=100') -eq ('Deliver to reception [OTHER:KEEP]'+[Environment]::NewLine+'[WO-WTAX:v1;RATES=]')) 'V4 existing header save retains explicit all-off selection'
 Check ((Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=100') -eq 224 -and (Scalar 'SELECT Tax FROM dbo.[Order] WHERE ID=100') -eq 24 -and (Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=101') -eq 100 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=101') -eq 2) 'WALK-IN selections do not change V4 amounts, physical stock or commitments'
 foreach($path in $configs.Keys){Check ((Get-FileHash -LiteralPath $path).Hash -eq $configs[$path]) 'application configuration unchanged'}
 "PASS: $checks V4 recall-taxable checks on owned localhost fixture; actual mapped update methods, no production Load, live SQL or print."
}finally{
 if($main){$main.Dispose()}
 foreach($name in @('db','dbnew','dbInitial')){$context=Global $name;if($context){$context.Dispose()};State $name $null}
 if($created -and $cx.State -eq 'Open'){$cx.ChangeDatabase('master');Sql "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]"};$cx.Dispose()
 foreach($path in $configs.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $configs[$path]){throw 'Application config changed'}}
}
