param()
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$scriptPath=Join-Path $root 'Database Components/Migrations/Prepare_CONSIGNMENT_DB_For_V71.sql'
$original=[IO.File]::ReadAllText($scriptPath)
$out=Join-Path $root ('TestResults/ConsignmentV71-'+[guid]::NewGuid().ToString('N').Substring(0,8))
[void](New-Item -ItemType Directory -Path $out -Force)
$db='SOD_ConsignV71_'+[guid]::NewGuid().ToString('N')
$cx=New-Object Data.SqlClient.SqlConnection 'Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False'
$checks=0;$created=$false
function Run([string]$sql){$cmd=$cx.CreateCommand();$cmd.CommandTimeout=60;$cmd.CommandText=$sql;[void]$cmd.ExecuteNonQuery();$cmd.Dispose()}
function Scalar([string]$sql){$cmd=$cx.CreateCommand();$cmd.CommandTimeout=60;$cmd.CommandText=$sql;$v=$cmd.ExecuteScalar();$cmd.Dispose();return $v}
function Check([bool]$condition,[string]$label){if(-not $condition){throw $label};$script:checks++;"PASS: $label"}
function Batches([string]$sql){foreach($batch in [regex]::Split($sql,'(?im)^\s*GO\s*$')){if($batch.Trim()){Run $batch}}}
function Rejected([string]$sql,[int]$number){try{Batches $sql;return $false}catch [Data.SqlClient.SqlException]{if($_.Exception.Number -eq $number){return $true};throw}}
function Snapshot(){return [string](Scalar "SELECT (SELECT * FROM dbo.Configuration FOR XML RAW,TYPE),(SELECT * FROM dbo.Item FOR XML RAW,TYPE),(SELECT * FROM dbo.[Order] ORDER BY ID FOR XML RAW,TYPE),(SELECT * FROM dbo.OrderEntry ORDER BY ID FOR XML RAW,TYPE),(SELECT * FROM dbo.Queueing ORDER BY ID FOR XML RAW,TYPE),(SELECT * FROM dbo.QueueingItems ORDER BY ID FOR XML RAW,TYPE) FOR XML RAW")}
try{
 $cx.Open();Run "CREATE DATABASE [$db]";$created=$true;$cx.ChangeDatabase($db);Run "ALTER DATABASE [$db] SET COMPATIBILITY_LEVEL=130"
 Run @"
CREATE TABLE dbo.Configuration(StoreID int,StoreCity nvarchar(50));INSERT dbo.Configuration VALUES(811,N'KEEP CITY');
CREATE TABLE dbo.SI_ORSeries(RegisterNumber int,NextORNumber nvarchar(200));INSERT dbo.SI_ORSeries VALUES(1,N'CI-KEEP');
CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode varchar(50),Quantity float,QuantityCommitted float);
INSERT dbo.Item VALUES(1,'SAMPLE',10,2);
CREATE TABLE dbo.[Order](ID int IDENTITY PRIMARY KEY,StoreID int,Closed bit,[Time] datetime,[Type] int,Comment varchar(255),CustomerID int,ShipToID int,DepositOverride bit,Deposit money,Tax money,Total money,LastUpdated datetime,ExpirationOrDueDate datetime,Taxable bit,SalesRepID int,ReferenceNumber varchar(30),ShippingChargeOnOrder money,ShippingChargeOverride bit,ShippingServiceID int,ShippingTrackingNumber varchar(255),ShippingNotes nvarchar(max),ReasonCodeID int,ExchangeID int,ChannelType int,DefaultDiscountReasonCodeID int,DefaultReturnReasonCodeID int,DefaultTaxChangeReasonCodeID int);
INSERT dbo.[Order](StoreID,Closed,[Type],Comment,CustomerID,Tax,Total) VALUES(811,0,2,'EXISTING',1,12,112);
CREATE TABLE dbo.OrderEntry(ID int IDENTITY PRIMARY KEY,Cost money,StoreID int,OrderID bigint,ItemID int,FullPrice money,PriceSource varchar(30),Price money,QuantityOnOrder float,SalesRepID int,Taxable bit,DetailID int,Description varchar(100),QuantityRTD float,LastUpdated datetime,Comment varchar(100),DiscountReasonCodeID int,ReturnReasonCodeID int,TaxChangeReasonCodeID int);
CREATE TABLE dbo.Queueing(id bigint IDENTITY PRIMARY KEY,OrderID bigint,[Status] int,GroupTo int,OPIS varchar(max));
CREATE TABLE dbo.QueueingItems(id bigint IDENTITY PRIMARY KEY,QueueingID bigint,ItemID int,QtyPre int,Picker nvarchar(50),[Status] varchar(50),PickLoc varchar(max));
CREATE TABLE dbo.SOD_Item_Logs(ItemLookupCode varchar(50),[Type] varchar(max),[Date] datetime);
INSERT dbo.SOD_Item_Logs VALUES('SAMPLE','Item Auto Logs',GETDATE());
"@
 Run ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures/ConsignmentV71/Header.legacy.sql')))
 Run ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures/ConsignmentV71/Entry.legacy.sql')))
 Run 'CREATE ROLE FixtureRole';Run 'GRANT EXECUTE ON dbo.SOD_sp_InsertQoute TO FixtureRole'
 $server=[string](Scalar "SELECT CONVERT(sysname,SERVERPROPERTY('ServerName'))")
 # Change only explicit target declarations/USE in the owned fixture copy.
 # Predicates, definition checks, transaction and actual backup verification stay intact.
 $fixture=$original.Replace('USE [CONSIGNMENT_DB];',"USE [$db];").Replace("DECLARE @ExpectedDatabase sysname=N'CONSIGNMENT_DB';","DECLARE @ExpectedDatabase sysname=N'$db';")
 Check (Rejected $fixture 52420) 'Wrong server rejected before changes'
 $fixture=$fixture.Replace("DECLARE @ExpectedServer sysname=N'MSI\SQLEXPRESS2017';","DECLARE @ExpectedServer sysname=N'$($server.Replace("'","''"))';")
 $before=Snapshot
 Batches $fixture
 Check ((Snapshot) -eq $before -and [int](Scalar "SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.SOD_sp_InsertQoute')") -eq 6) 'Default review makes no changes'
 $apply=$fixture.Replace('DECLARE @Apply bit=0;','DECLARE @Apply bit=1;')
 Check (Rejected $apply 52425) 'Apply without verified checksum full backup rejected'
 $backup=Join-Path $out 'FixtureBefore.bak'
 $cmd=$cx.CreateCommand();$cmd.CommandText="BACKUP DATABASE [$db] TO DISK=@file WITH COPY_ONLY,CHECKSUM,INIT";[void]$cmd.Parameters.AddWithValue('@file',$backup);[void]$cmd.ExecuteNonQuery();$cmd.Dispose()
 $apply=$apply.Replace("DECLARE @BackupFile nvarchar(4000)=N'';","DECLARE @BackupFile nvarchar(4000)=N'$($backup.Replace("'","''"))';")
 Run "CREATE VIEW dbo.SOD_viewLastUpdatedItem AS SELECT CAST('custom' AS varchar(50)) AS ItemLookupCode,CAST(1 AS float) AS Available"
 Check (Rejected $apply 52424) 'Customized existing view rejected'
 Run 'DROP VIEW dbo.SOD_viewLastUpdatedItem'
 Batches $apply
 Check ((Snapshot) -eq $before) 'Migration preserves all existing business rows'
 Check ([int](Scalar "SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.SOD_sp_InsertQoute')") -eq 8) 'Header has eight parameters'
 Check ([int](Scalar "SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.SOD_sp_InsertQouteEntry')") -eq 12) 'Entry has twelve parameters'
 Check ([int](Scalar "SELECT COUNT(*) FROM sys.views WHERE name IN('SOD_viewTableLastUpdate','SOD_viewLastUpdatedItem','SOD_ViewForInvoice')") -eq 3) 'Three missing views created'
 Check ([int](Scalar "SELECT COUNT(*) FROM sys.database_permissions WHERE major_id=OBJECT_ID('dbo.SOD_sp_InsertQoute') AND grantee_principal_id=DATABASE_PRINCIPAL_ID('FixtureRole') AND permission_name='EXECUTE'") -eq 1) 'ALTER preserves existing permissions'
 [IO.File]::WriteAllText((Join-Path $out 'Header.after.sql'),[string](Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.SOD_sp_InsertQoute'))"))
 $definitions=[string](Scalar "SELECT name,OBJECT_DEFINITION(object_id) AS body FROM sys.objects WHERE name IN('SOD_sp_InsertQoute','SOD_sp_InsertQouteEntry','SOD_viewTableLastUpdate','SOD_viewLastUpdatedItem','SOD_ViewForInvoice') ORDER BY name FOR XML RAW")
 Batches $apply
 Check ($definitions -eq [string](Scalar "SELECT name,OBJECT_DEFINITION(object_id) AS body FROM sys.objects WHERE name IN('SOD_sp_InsertQoute','SOD_sp_InsertQouteEntry','SOD_viewTableLastUpdate','SOD_viewLastUpdatedItem','SOD_ViewForInvoice') ORDER BY name FOR XML RAW")) 'Repeat application preserves definitions'
 $legacy=[int](Scalar "DECLARE @id int;EXEC @id=dbo.SOD_sp_InsertQoute '1',1,1,12,112,'LEGACY';SELECT @id;")
 Check ([int](Scalar "SELECT [Type] FROM dbo.[Order] WHERE ID=$legacy") -eq 3) 'Six-argument caller stays quotation type 3'
 Check ([string](Scalar "SELECT ReferenceNumber FROM dbo.[Order] WHERE ID=$legacy") -eq 'CI-KEEP') 'Reference-series behavior preserved'
 Check ([int](Scalar "SELECT COUNT(*) FROM dbo.Queueing WHERE OrderID=$legacy") -eq 0) 'Legacy header does not add queue rows'
 Run "EXEC dbo.SOD_sp_InsertQouteEntry 2,$legacy,1,10,8,1.25,1,0,'LEGACY ITEM','KEEP COMMENT'"
 Check ([double](Scalar "SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID=$legacy") -eq 1.25) 'Fractional line quantity preserved'
 Check ([string](Scalar "SELECT Comment FROM dbo.OrderEntry WHERE OrderID=$legacy") -eq 'KEEP COMMENT') 'Legacy line comment preserved'
 Check ([int](Scalar 'SELECT COUNT(*) FROM dbo.QueueingItems') -eq 0) 'Ten-argument caller does not add queue items'
 $wo=[int](Scalar "DECLARE @id int;EXEC @id=dbo.SOD_sp_InsertQoute '1',1,1,12,112,'WORK ORDER','TEST USER',2;SELECT @id;")
 Check ([int](Scalar "SELECT [Type] FROM dbo.[Order] WHERE ID=$wo") -eq 2) 'V7.1 work order uses caller type 2'
 Check ([string](Scalar "SELECT OPIS FROM dbo.Queueing WHERE OrderID=$wo") -eq 'TEST USER') 'V7.1 operator and queue header retained'
 Run "EXEC dbo.SOD_sp_InsertQouteEntry 2,$wo,1,10,8,1.25,1,1,'V71 ITEM','UP-STORE',1,2"
 Check ([int](Scalar "SELECT QtyPre FROM dbo.QueueingItems qi JOIN dbo.Queueing q ON q.id=qi.QueueingID WHERE q.OrderID=$wo") -eq 1) 'Prepared quantity retained'
 Check ([string](Scalar "SELECT PickLoc FROM dbo.QueueingItems qi JOIN dbo.Queueing q ON q.id=qi.QueueingID WHERE q.OrderID=$wo") -eq 'UP-STORE') 'Pick location retained'
 Check ([double](Scalar "SELECT Quantity FROM dbo.Item WHERE ID=1") -eq 10 -and [double](Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=1") -eq 2) 'Procedures do not overwrite physical/committed stock'
 Check ([string](Scalar "SELECT Status FROM dbo.SOD_ViewForInvoice WHERE GroupTo=$wo") -eq 'NotPrepared') 'Pending queue group remains not prepared'
 Run "UPDATE dbo.QueueingItems SET Status='Prepared' WHERE QueueingID=(SELECT id FROM dbo.Queueing WHERE OrderID=$wo)"
 Check ([string](Scalar "SELECT Status FROM dbo.SOD_ViewForInvoice WHERE GroupTo=$wo") -eq 'Prepared') 'Prepared queue group recognized'
 Run "UPDATE dbo.Queueing SET Status=1 WHERE OrderID=$wo"
 Check ([int](Scalar "SELECT COUNT(*) FROM dbo.SOD_ViewForInvoice WHERE GroupTo=$wo") -eq 0) 'Completed queue excluded'
 Check ([double](Scalar "SELECT Available FROM dbo.SOD_viewLastUpdatedItem WHERE ItemLookupCode='SAMPLE'") -eq 8) 'Refresh view calculates physical minus committed'
 Check ([string](Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.SOD_viewTableLastUpdate'))") -notmatch 'GSC_STORE_DB|DVO_STORE_DB') 'Refresh metadata uses current database'
 $quote=[int](Scalar "DECLARE @id int;EXEC @id=dbo.SOD_sp_InsertQoute '1',1,1,0,50,'QUOTE','TEST USER',3;SELECT @id;")
 Check ([int](Scalar "SELECT [Type] FROM dbo.[Order] WHERE ID=$quote") -eq 3) 'V7.1 quotation uses caller type 3'
 $beforeFailure=Snapshot
 Run 'BEGIN TRANSACTION'
 try{Run "DECLARE @id int;EXEC @id=dbo.SOD_sp_InsertQoute '1',1,1,0,25,'ROLLBACK','TEST USER',2;EXEC dbo.SOD_sp_InsertQouteEntry 1,@id,1,5,5,1,1,1,'ROLLBACK','STORE',0,2;THROW 52990,'Fixture forced failure',1;"}catch{if([int](Scalar 'SELECT @@TRANCOUNT') -gt 0){Run 'ROLLBACK TRANSACTION'}}
 Check ((Snapshot) -eq $beforeFailure) 'Application transaction rollback removes header/line/queue writes atomically'
 Check (([IO.File]::ReadAllText($scriptPath)) -eq $original) 'Production script target/backup guards unchanged by fixture'
 "PASS: $checks checks; owned LocalDB only. No target database migration or V7.1 UI/RMS POS execution."
 "Artifacts: $out"
}finally{
 if($created){$cx.ChangeDatabase('master');Run "ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$db]"}
 $cx.Dispose()
}