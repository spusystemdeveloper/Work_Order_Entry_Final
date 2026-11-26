param([string]$ApplicationPath='C:\Users\ADMIN\source\repos\WORK_ORDER_ENTRY_4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe')
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
 & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -ApplicationPath $ApplicationPath
 exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data,System.Data.Linq
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$package=Join-Path $repo 'Database Components\Deployment\QC_PUREGOLD_V4'
$baseline=Join-Path $package 'Baseline'
$expected=(Get-Content (Join-Path $package 'EXE-identity.json') -Raw|ConvertFrom-Json).SHA256
if((Get-FileHash $ApplicationPath).Hash -ne $expected){throw 'Selected EXE does not match reviewed V4 hash.'}
$localdb='C:\Program Files\Microsoft SQL Server\170\Tools\Binn\SqlLocalDB.exe'
$instance='QC_V4_'+[Guid]::NewGuid().ToString('N').Substring(0,12)
$result=Join-Path $repo ('TestResults\'+$instance)
[void](New-Item -ItemType Directory -Path $result)
$checks=0;$created=$false;$cx=$null;$ctx=$null
function Check($condition,$message){if(!$condition){throw "FAILED: $message"};$script:checks++;"PASS: $message"}
function Sql($text){$cmd=$cx.CreateCommand();$cmd.CommandTimeout=30;$cmd.CommandText=$text;try{[void]$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
function Scalar($text){$cmd=$cx.CreateCommand();$cmd.CommandText=$text;try{$cmd.ExecuteScalar()}finally{$cmd.Dispose()}}
function Table($text){$cmd=$cx.CreateCommand();$cmd.CommandText=$text;$t=[Data.DataTable]::new();try{$t.Load($cmd.ExecuteReader());return ,$t}finally{$cmd.Dispose()}}
function Apply($file){$sql=[IO.File]::ReadAllText((Join-Path $package $file));Sql 'BEGIN TRAN';try{Sql $sql;Sql 'COMMIT'}catch{Sql 'IF @@TRANCOUNT>0 ROLLBACK';throw}}
function Reject($file,$message){$failed=$false;try{Apply $file}catch{$failed=$true};Check $failed $message}
function BaseTable($node){
 $defs=@()
 foreach($col in $node.Type.Column){
  $type=[string]$col.DbType
  if($type -match 'IDENTITY'){$type=$type -replace '\s+IDENTITY','' -replace '\s+NOT NULL','';$type+=' IDENTITY(1,1) NOT NULL'}
  $def='['+$col.Name+'] '+$type
  if($col.IsPrimaryKey -eq 'true'){$def+=' PRIMARY KEY'}
  if($type -match 'NOT NULL' -and $type -notmatch 'IDENTITY|timestamp|rowversion'){
   $default=if($type -match '^(N?VarChar|N?Char|NText|Text)'){"N''"}elseif($type -match '^(SmallDateTime|DateTime|Date)'){"'2026-01-01'"}elseif($type -match 'Binary|Image'){'0x'}else{'0'}
   $def+=' DEFAULT ('+$default+')'
  }
  $defs+=$def
 }
 Sql ('CREATE TABLE '+$node.Name+' ('+($defs -join ',')+');')
}
try{
 & $localdb create $instance | Out-Null;if($LASTEXITCODE){throw 'LocalDB create failed'};$created=$true
 & $localdb start $instance|Out-Null;if($LASTEXITCODE){throw 'LocalDB start failed'}
 $endpoint="Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20"
 $cx=[Data.SqlClient.SqlConnection]::new($endpoint);$cx.Open()
 foreach($db in @('PUREGOLD_DB','QC_STORE_DB','QC_RDC_DB','GSC_STORE_DB','GSC_MAIN_WHSE_DB','GSC_OFFICE_SUPPLY_DB')){
  $mdf=(Join-Path $result ($db+'.mdf')).Replace("'","''");$ldf=(Join-Path $result ($db+'_log.ldf')).Replace("'","''")
  Sql "CREATE DATABASE [$db] ON (NAME=N'$db',FILENAME=N'$mdf') LOG ON (NAME=N'$($db)_log',FILENAME=N'$ldf'); ALTER DATABASE [$db] SET COMPATIBILITY_LEVEL=130;"
 }
 $cx.ChangeDatabase('PUREGOLD_DB')
 [xml]$map=Get-Content 'C:\Users\ADMIN\source\repos\WORK_ORDER_ENTRY_4\Work Order Entry_4\Work Order Entry\Linq\ItemLookUp.dbml'
 foreach($node in $map.Database.Table|Where-Object {$_.Member -in @('Items','Customers','Orders','OrderEntries','TransactionEntries','Configurations','Stores','SOD_WO_Confs')}){BaseTable $node}
 Sql 'CREATE TABLE dbo.[Transaction](TransactionNumber int,RecallID int);'
 foreach($db in @('QC_STORE_DB','QC_RDC_DB','GSC_STORE_DB','GSC_MAIN_WHSE_DB','GSC_OFFICE_SUPPLY_DB')){Sql "CREATE TABLE [$db].dbo.Item(ItemLookupCode nvarchar(25),Quantity float,QuantityCommitted float);"}
 Sql @'
SET IDENTITY_INSERT dbo.Item ON;
INSERT dbo.Item(ID,ItemLookupCode,Description,ExtendedDescription,SubDescription1,SubDescription2,Price,Cost,Quantity,QuantityCommitted,ParentQuantity,TaxID,ItemType,Inactive)
VALUES(101,N'CODE-A',N'Fixture ',N'description',N'PG-123',N'2',112,50,10.5,2.25,1,7,0,0),
(102,N'CODE-B',N'Negative ',N'stock',N'PG-234',N'3',224,100,-2,1,1,8,0,0),
(103,N'CODE-C',N'Negative ',N'commit',N'PG-345',N'4',336,150,9,-2,1,9,0,0);
SET IDENTITY_INSERT dbo.Item OFF;
INSERT dbo.Configuration(StoreID,StoreCity) VALUES(100,N'QUEZON CITY');
INSERT dbo.Store(ID,[Name],StoreCode,City,Address1,Country) VALUES(100,N'Puregold',N'PUREGOLD_DB',N'',N'fixture',N'PH'),(3001,N'QC store',N'QC_STORE_DB',N'',N'fixture',N'PH'),(3012,N'QC RDC',N'QC_RDC_DB',N'',N'fixture',N'PH');
INSERT QC_STORE_DB.dbo.Item VALUES(N'CODE-A',20.5,3.25);
INSERT QC_RDC_DB.dbo.Item VALUES(N'CODE-A',9.5,2.25);
'@
 foreach($obj in @('SOD_ViewItems','SOD_fntbl_CheckQty','SOD_fntbl_WoDetails','SOD_fntbl_WoEntry')){Sql ([IO.File]::ReadAllText((Join-Path $baseline ($obj+'.sql.txt'))))}
 Check ((Scalar "SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_ViewItems') ORDER BY column_id") -eq 'PG SKU') 'Baseline reproduces the extra leading PG SKU column'
 $before=Table 'SELECT * FROM dbo.SOD_ViewItems ORDER BY ID'
 $original=[IO.File]::ReadAllText((Join-Path $baseline 'SOD_ViewItems.sql.txt'))
 Sql (($original -replace '(?i)CREATE VIEW','ALTER VIEW')+"`r`n-- fixture drift")
 Reject '01_SOD_ViewItems.sql' 'An edited view definition is rejected'
 Sql 'DROP VIEW dbo.SOD_ViewItems';Sql $original
 Sql "CREATE USER FixtureReader WITHOUT LOGIN;GRANT SELECT ON dbo.SOD_ViewItems TO FixtureReader;"
 $viewID=Scalar "SELECT OBJECT_ID('dbo.SOD_ViewItems')"
 $file1=[IO.File]::ReadAllText((Join-Path $package '01_SOD_ViewItems.sql'))
 $failed=$false;try{Sql $file1}catch{$failed=$true};Check $failed 'Execution without an owning transaction is rejected'
 Sql 'BEGIN TRAN';Sql $file1;Sql 'ROLLBACK'
 Check ((Scalar "SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_ViewItems') ORDER BY column_id") -eq 'PG SKU') 'Caller rollback restores the original layout'
 Apply '01_SOD_ViewItems.sql'
 $after=Table 'SELECT * FROM dbo.SOD_ViewItems ORDER BY ID'
 $expectedColumns=@('ItemLookupCode','DESCRIPTION','Price','Cost','AVAILABLE','ParentQuantity','TaxID','SubDescription2','PRICE 1','ID','DESCRIPTION1','ExtendedDescription','ItemType','Inactive','ITEMDES','SKULEVEL','FULLDESC','PG SKU')
 Check (($after.Columns|ForEach-Object ColumnName) -join '|' -eq ($expectedColumns -join '|')) 'All 18 view columns have the V4-compatible order'
 Check ($after.Rows[0][0] -eq 'CODE-A' -and $after.Rows[0][3] -eq 50 -and $after.Rows[0][6] -eq 7 -and $after.Rows[0][9] -eq 101) 'V4 item-code/cost/tax/item-ID ordinal reads return the intended values'
 foreach($col in $before.Columns){for($i=0;$i -lt $before.Rows.Count;$i++){Check ([string]$before.Rows[$i][$col.ColumnName] -eq [string]$after.Rows[$i][$col.ColumnName]) ('Existing value preserved: '+$col.ColumnName+' row '+$i)}}
 Check ($after.Rows[0].AVAILABLE -eq 8.25 -and $after.Rows[1].AVAILABLE -eq 0 -and $after.Rows[2].AVAILABLE -eq 9) 'QC availability arithmetic and negative-value rules are preserved'
 Check ((Scalar "SELECT OBJECT_ID('dbo.SOD_ViewItems')") -eq $viewID) 'View object identity is retained'
 Check ((Scalar "SELECT COUNT(*) FROM sys.database_permissions WHERE major_id=OBJECT_ID('dbo.SOD_ViewItems') AND grantee_principal_id=USER_ID('FixtureReader')") -eq 1) 'View SELECT permission survives ALTER'
 [IO.File]::WriteAllText((Join-Path $result 'patched-view-definition.sql'),[string](Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.SOD_ViewItems'))"))
 Apply '01_SOD_ViewItems.sql';Check ($true) 'View script repeats successfully'
 Apply '02_SOD_fntbl_CheckQty.sql'
 $stock=Table "SELECT * FROM dbo.SOD_fntbl_CheckQty('CODE-A') ORDER BY StoreCode"
 Check ($stock.Rows.Count -eq 2) 'Other-store lookup returns only the two configured QC warehouses'
 Check ($stock.Rows[0].Qty -eq 7.25 -and $stock.Rows[1].Qty -eq 17.25) 'QC warehouse available stock preserves fractional quantity minus commitments'
 Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_fntbl_CheckQty('MISSING') WHERE Qty IS NULL") -eq 2) 'Absent warehouse items retain NULL availability'
 Apply '02_SOD_fntbl_CheckQty.sql';Check ($true) 'Warehouse script repeats successfully'
 Apply '03_Optional_QuotationImport.sql';Apply '03_Optional_QuotationImport.sql'
 Check ((Scalar "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_ImportedQuote')") -eq 5) 'Optional import table created with five mapped columns and repeats'
 $assembly=[Reflection.Assembly]::LoadFrom($ApplicationPath)
 $ctx=[Activator]::CreateInstance($assembly.GetType('WorkOrderEntry.ItemLookUpDataContext'),@("Server=(localdb)\$instance;Database=PUREGOLD_DB;Integrated Security=True;Pooling=False"))
 Sql @'
SET IDENTITY_INSERT dbo.Customer ON;INSERT dbo.Customer(ID,AccountNumber,Company) VALUES(501,N'FIXTURE',N'Fixture Customer');SET IDENTITY_INSERT dbo.Customer OFF;
SET IDENTITY_INSERT dbo.[Order] ON;INSERT dbo.[Order](ID,CustomerID,Closed,[Time],Tax,Total,ReferenceNumber,Comment) VALUES(100,501,0,'2026-10-31T10:00:00',24,224,N'F100',N'Fixture');SET IDENTITY_INSERT dbo.[Order] OFF;
INSERT dbo.OrderEntry(StoreID,OrderID,ItemID,QuantityOnOrder,Price) VALUES(100,100,101,2,112);
INSERT dbo.SOD_WO_Conf(StoreID) VALUES(100);
'@
 $headers=@($ctx.SOD_fntbl_WoDetails(100,1,'Fixture cashier'))
 Check ($headers.Count -eq 1 -and $headers[0].Company -eq 'Fixture Customer' -and $headers[0].Total -eq 224) 'Selected compiled V4 reads the existing standard print-header function'
 $lines=@($ctx.SOD_fntbl_WoEntry(100))
 Check ($lines.Count -eq 1 -and $lines[0].Itemcode -eq 'CODE-A' -and $lines[0].QtyOrder -eq 2) 'Selected compiled V4 reads the existing standard print-line function'
 $conf=@($ctx.SOD_WO_Confs)
 Check ($conf.Count -eq 1) 'Selected compiled V4 reads configuration without username-column additions'
 $ctx.Dispose();$ctx=$null
 Sql 'ALTER TABLE dbo.SOD_ImportedQuote ADD Unexpected int NULL'
 Reject '03_Optional_QuotationImport.sql' 'Optional import script rejects a customized existing table'
 [IO.File]::WriteAllText((Join-Path $result 'patched-stock-definition.sql'),[string](Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.SOD_fntbl_CheckQty'))"))
 Apply 'Rollback_02_SOD_fntbl_CheckQty.sql';Apply 'Rollback_01_SOD_ViewItems.sql'
 Check ((Scalar "SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_ViewItems') ORDER BY column_id") -eq 'PG SKU') 'Definition rollback restores the original view order'
 Check ((Scalar "SELECT COUNT(*) FROM sys.sql_expression_dependencies WHERE referencing_id=OBJECT_ID('dbo.SOD_fntbl_CheckQty') AND referenced_database_name LIKE 'GSC%'") -eq 3) 'Definition rollback restores original stock targets'
 $cx.ChangeDatabase('master');Reject '01_SOD_ViewItems.sql' 'Wrong database is rejected'
 [pscustomobject]@{Checks=$checks;Result='PASS';Target='Owned disposable LocalDB only';ExeSHA256=$expected}|ConvertTo-Json|Set-Content (Join-Path $result 'result.json')
 "PASS: $checks checks; evidence $result"
}finally{
 if($ctx){$ctx.Dispose()}
 if($cx -and $cx.State -eq 'Open'){
  $cx.ChangeDatabase('master')
  foreach($db in @('PUREGOLD_DB','QC_STORE_DB','QC_RDC_DB','GSC_STORE_DB','GSC_MAIN_WHSE_DB','GSC_OFFICE_SUPPLY_DB')){Sql "IF DB_ID(N'$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END"}
 }
 if($cx){$cx.Dispose()}
 if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
