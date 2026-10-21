# Tests the recovery UPDATE/audit body, not the production target/backup guards.
# Those guards remain intact; the complete production script is never retargeted.
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$source=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Restore_Cebu_23_ZeroCommitments_2026-10-21.sql'))
$seed=$source.Substring($source.IndexOf('DECLARE @Expected TABLE'))
$seed=$seed.Substring(0,$seed.IndexOf('DECLARE @Triggers TABLE'))
$core=($source -split '-- CEBU_REPAIR_CORE_START[^\r\n]*',2)[1]
$core=($core -split '-- CEBU_REPAIR_CORE_END',2)[0]
$db='CebuRepairFixture_'+[Guid]::NewGuid().ToString('N')
$cx=New-Object System.Data.SqlClient.SqlConnection 'Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;Pooling=False;Connect Timeout=20'
function Sql([string]$sql) { $cmd=$cx.CreateCommand(); try{$cmd.CommandText=$sql;$cmd.CommandTimeout=30;[void]$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()} }
function Scalar([string]$sql) { $cmd=$cx.CreateCommand(); try{$cmd.CommandText=$sql;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()} }
function Check([bool]$ok,[string]$name) { if(-not $ok){throw "FAILED: $name"};"PASS: $name" }
$setup=@'
CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemType int NOT NULL,Quantity float NOT NULL,QuantityCommitted float NOT NULL,Price money NOT NULL);
CREATE TABLE dbo.SOD_Item_Logs(ItemID int,ItemLookupCode varchar(50),[Type] varchar(max),ReferenceID varchar(50),QuantityORIS int,LastQuantity int,NewQuantity int,LastCommitted int,NewCommitted int,[Date] datetime);
'@
$before=@'
DECLARE @Before TABLE(ItemID int PRIMARY KEY,PhysicalStock float,OldCommitted float,NewCommitted float);
DECLARE @Changed TABLE(ItemID int PRIMARY KEY);
INSERT @Before SELECT i.ID,i.Quantity,i.QuantityCommitted,x.QuantityToAdd FROM dbo.Item i JOIN @Expected x ON x.ItemID=i.ID;
'@
$run="SET XACT_ABORT ON; $seed $before BEGIN TRY BEGIN TRANSACTION; $core COMMIT; END TRY BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH;"
try {
 $cx.Open();Sql "CREATE DATABASE [$db]";$cx.ChangeDatabase($db);Sql $setup
 function ResetFixture { Sql "DELETE dbo.SOD_Item_Logs; DELETE dbo.Item; $seed INSERT dbo.Item SELECT ItemID,0,1000,0,12.34 FROM @Expected; INSERT dbo.Item VALUES(999999,0,500,7,99.99);" }
 ResetFixture;Sql $run
 Check ((Scalar "SELECT COUNT(*) FROM dbo.Item WHERE ID<>999999 AND QuantityCommitted>0") -eq 23) '23 exact items updated'
 Check ((Scalar "$seed SELECT COUNT(*) FROM dbo.Item i JOIN @Expected x ON x.ItemID=i.ID WHERE i.QuantityCommitted=x.QuantityToAdd") -eq 23) 'all exact expected differences applied'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.Item WHERE (ID<>999999 AND (Quantity<>1000 OR Price<>12.34)) OR (ID=999999 AND (Quantity<>500 OR QuantityCommitted<>7 OR Price<>99.99))') -eq 0) 'physical stock, prices and unrelated item unchanged'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE LastCommitted=0 AND LastQuantity=NewQuantity') -eq 23) '23 before/after audit rows'
 $failed=$false;try{Sql $run}catch{$failed=$true}
 Check $failed 'repeat execution rejected'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_Item_Logs') -eq 23) 'repeat failure did not add audit rows'
 ResetFixture;Sql 'UPDATE dbo.Item SET QuantityCommitted=5 WHERE ID=2213'
 $failed=$false;try{Sql $run}catch{$failed=$true}
 Check $failed 'concurrently nonzero item rejected'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.Item WHERE ID<>999999 AND ID<>2213 AND QuantityCommitted<>0') -eq 0) 'other updates rolled back on count failure'
 ResetFixture
 Sql "CREATE TRIGGER dbo.FixtureAuditFailure ON dbo.SOD_Item_Logs AFTER INSERT AS THROW 53999,'Owned fixture forced audit failure',1;"
 $failed=$false;try{Sql $run}catch{$failed=$true}
 Check $failed 'audit failure rejected'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.Item WHERE ID<>999999 AND QuantityCommitted<>0') -eq 0) 'audit failure rolls back commitments'
 Sql 'DROP TRIGGER dbo.FixtureAuditFailure'
 ResetFixture
 Sql 'CREATE TRIGGER dbo.FixtureStockChange ON dbo.Item AFTER UPDATE AS UPDATE i SET Quantity=999 FROM dbo.Item i JOIN inserted n ON n.ID=i.ID;'
 $failed=$false;try{Sql $run}catch{$failed=$true}
 Check $failed 'unexpected physical stock change rejected'
 Check ((Scalar 'SELECT COUNT(*) FROM dbo.Item WHERE ID<>999999 AND (QuantityCommitted<>0 OR Quantity<>1000)') -eq 0) 'unexpected stock change fully rolled back'
 'PASS: 12 disposable-fixture checks; no live database used'
} finally {
 if($cx.State -eq 'Open'){$cx.ChangeDatabase('master');Sql "IF DB_ID(N'$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END"};$cx.Dispose()
}
