# Tests only a new owned LocalDB instance; no restored-store access.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$instance='SOD_Print_'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$directory=Join-Path ([IO.Path]::GetTempPath()) $instance
$null=New-Item -ItemType Directory -Path $directory
$migration=Get-Content (Join-Path $PSScriptRoot '../Migrations/Add_WorkOrder_Print_Address.sql') -Raw
$cx=$null;$created=$false;$checks=0
function Sql($text,$tx=$null){$c=$cx.CreateCommand();try{$c.CommandText=$text;$c.Transaction=$tx;return $c.ExecuteScalar()}finally{$c.Dispose()}}
function Check($ok,$label){if(-not $ok){throw $label};$script:checks++;Write-Output "PASS: $label"}
function Apply{$t=$cx.BeginTransaction();try{$null=Sql $migration $t;$t.Commit()}catch{$t.Rollback();throw}finally{$t.Dispose()}}
try {
 & SqlLocalDB.exe create $instance -s | Out-Null
 if($LASTEXITCODE -ne 0){throw 'LocalDB creation failed'};$created=$true
 $cx=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$cx.Open()
 try{Apply;throw 'Wrong target accepted'}catch{Check ($_.Exception.ToString() -like '*System databases are not supported*') 'Wrong target rejected'}
 $df=(Join-Path $directory 'fixture.mdf').Replace("'","''");$lf=(Join-Path $directory 'fixture.ldf').Replace("'","''")
 $null=Sql "CREATE DATABASE Portable_Print_Store ON(NAME=N'Data',FILENAME=N'$df') LOG ON(NAME=N'Log',FILENAME=N'$lf');"
 $cx.ChangeDatabase('Portable_Print_Store')
 try{Apply;throw 'Missing function accepted'}catch{Check ($_.Exception.ToString() -like '*multi-statement SOD_fntbl_WoDetails is required*') 'Missing function rejected'}
 $null=Sql "CREATE TABLE dbo.Configuration(StoreCity varchar(50)); INSERT dbo.Configuration VALUES('LOCAL BRANCH'); CREATE TABLE dbo.Customer(ID int,AccountNumber varchar(50),Company varchar(50),Address nvarchar(50),Address2 nvarchar(50),City nvarchar(50),State nvarchar(50),Zip nvarchar(50)); INSERT dbo.Customer VALUES(1,'TEST','Synthetic',N'One ',NULL,N'City ',N'State ',N'8000'); CREATE TABLE dbo.[Order](ID int,CustomerID int,Time datetime,ReferenceNumber varchar(50),Comment varchar(100),Total money,Tax money); INSERT dbo.[Order] VALUES(1,1,'2026-10-28T11:12:00','REF','CASH',638,68.36),(2,1,'2026-10-28T11:12:00','REF2','EXEMPT',2750,0),(3,1,'2026-10-28T11:12:00','REF3','FRACTION',123.45,13.23);"
 # The reviewed legacy definition is embedded in the migration, before alteration.
 $legacy=[regex]::Match($migration,"(?s)DECLARE @Expected nvarchar\(max\)=N'(.*?)';").Groups[1].Value.Replace("''","'")
 $null=Sql $legacy
 $before=Sql "SELECT * FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture') FOR XML RAW;"
 try{$null=Sql $migration;throw 'Missing transaction accepted'}catch{Check ($_.Exception.ToString() -like '*active transaction*') 'Transaction required'}
 $tx=$cx.BeginTransaction();try{$null=Sql $migration $tx;$tx.Rollback()}finally{$tx.Dispose()}
 Check ((Sql "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_fntbl_WoDetails') AND name='Address'") -eq 0) 'Rollback restores original function'
 Apply
 Check ((Sql "SELECT Address FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture')") -eq 'One City State 8000') 'Address concatenation handles NULL components'
 $after=Sql "SELECT Branch,Orderid,AccountNo,Company,Register,Cashier,OrderDate,Ordertime,Reference,Comment,Subtotal,SalesTax,Total FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture') FOR XML RAW;"
 Check ($before -ceq $after) 'All existing output values remain identical'
 Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_fntbl_WoDetails(2,1,'Fixture') WHERE Total=2750 AND SalesTax=0 AND Subtotal=2750") -eq 1) 'VAT-exempt totals retained'
 Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_fntbl_WoDetails(3,1,'Fixture') WHERE Total=123.45 AND SalesTax=13.23 AND Subtotal=110.22") -eq 1) 'Fractional currency calculations retained'
 Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_fntbl_WoDetails(999,1,'Fixture')") -eq 0) 'Missing-order behavior retained'
 Apply
 Check ((Sql "SELECT Address FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture')") -eq 'One City State 8000') 'Repeat deployment succeeds'
 $custom=$legacy.Replace('CREATE function','ALTER function').Replace('o.Total-o.Tax','o.Total')
 $null=Sql $custom
 try{Apply;throw 'Custom function accepted'}catch{Check ($_.Exception.ToString() -like '*differs from the reviewed*') 'Customized calculation rejected'}
 Check ((Sql "SELECT Subtotal FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture')") -eq 638) 'Rejected update preserves custom function'
 $literalCustom=$legacy.Replace('CREATE function','ALTER function').Replace("' P'","'P'")
 $null=Sql $literalCustom
 try{Apply;throw 'Custom literal accepted'}catch{Check ($_.Exception.ToString() -like '*differs from the reviewed*') 'Custom literal whitespace rejected without changing formatting'}
 $branchCustom=$legacy.Replace('CREATE function','ALTER function').Replace('(select storeCity from [Configuration])',"(select 'OTHER STORE' from [Configuration])")
 $null=Sql $branchCustom
 try{Apply;throw 'Custom branch accepted'}catch{Check ($_.Exception.ToString() -like '*differs from the reviewed*') 'Custom branch selection rejected'}
 Check ((Sql "SELECT Branch FROM dbo.SOD_fntbl_WoDetails(1,1,'Fixture')") -eq 'OTHER STORE') 'Custom branch output preserved'
 Write-Output "PASS: $checks checks; disposable LocalDB only."
} finally {
 if($cx){try{if($cx.State -eq 'Open'){$cx.ChangeDatabase('master');$null=Sql "IF DB_ID('Portable_Print_Store') IS NOT NULL BEGIN ALTER DATABASE Portable_Print_Store SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE Portable_Print_Store; END"}}finally{$cx.Dispose()}}
 if($created){& SqlLocalDB.exe stop $instance -k|Out-Null;& SqlLocalDB.exe delete $instance|Out-Null}
}
