# Execute the complete SSMS recovery in a GUID-owned LocalDB fixture only.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8);$instance="HORecovery_$suffix"
$directory=Join-Path $repo "TestResults\HORecovery-$suffix";$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe).Source;$created=$false;$master=$null;$connection=$null;$checks=0
function Sql([string]$sql){$c=$connection.CreateCommand();try{$c.CommandText=$sql;$c.CommandTimeout=90;$null=$c.ExecuteNonQuery()}finally{$c.Dispose()}}
function Scalar([string]$sql){$c=$connection.CreateCommand();try{$c.CommandText=$sql;return $c.ExecuteScalar()}finally{$c.Dispose()}}
function Check([bool]$ok,[string]$name){if(-not $ok){throw "FAILED: $name"};$script:checks++;"Passed: $name"}
try {
    & $localdb create $instance -s | Out-Null;if($LASTEXITCODE -ne 0){throw 'LocalDB instance creation failed'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $c=$master.CreateCommand();try{$c.CommandText='CREATE DATABASE HO_PO_DB;ALTER DATABASE HO_PO_DB SET COMPATIBILITY_LEVEL=130;';$null=$c.ExecuteNonQuery()}finally{$c.Dispose()}
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=HO_PO_DB;Integrated Security=True;Pooling=False");$connection.Open()
    Sql @'
CREATE TABLE dbo.[Transaction](TransactionNumber int PRIMARY KEY,StoreID int,BatchNumber int,RecallID int,RecallType int,[Time] datetime,CustomerID int,Total money,SalesTax money,DBTimeStamp rowversion);
INSERT dbo.[Transaction](TransactionNumber,StoreID,BatchNumber,RecallID,RecallType,[Time],CustomerID,Total,SalesTax) VALUES(17,606,7,17,4,'20210310 08:32:20',71,514088.54,0),(23266,606,80,18870,4,'20261014 13:53:01',71,30200.54,0);
CREATE TABLE dbo.TransactionEntry(ID int PRIMARY KEY,StoreID int,TransactionNumber int,TransactionTime datetime,ItemID int,Quantity float,Price money,SalesTax money,DBTimeStamp rowversion);
INSERT dbo.TransactionEntry(ID,StoreID,TransactionNumber,TransactionTime,ItemID,Quantity,Price,SalesTax) VALUES(65233,606,17,'20261014 13:53:01',13879,1,7690.49,0),(65234,606,17,'20261014 13:53:01',13881,1,5139.24,0),(65235,606,17,'20261014 13:53:01',46608,3,4077.19,0),(65236,606,17,'20261014 13:53:01',46609,1,5139.24,0),(1,606,17,'20210310 08:32:20',1,1,514088.54,0);
CREATE TABLE dbo.TenderEntry(ID int PRIMARY KEY,StoreID int,TransactionNumber int,BatchNumber int,OrderHistoryID int,TenderID int,PaymentID int,Amount money,DBTimeStamp rowversion);
INSERT dbo.TenderEntry(ID,StoreID,TransactionNumber,BatchNumber,OrderHistoryID,TenderID,PaymentID,Amount) VALUES(25815,606,17,80,19857,2,0,30200.54),(1,606,17,7,1,2,0,514088.54);
CREATE TABLE dbo.AccountReceivable(ID int PRIMARY KEY,StoreID int,CustomerID int,TransactionNumber int,[Date] datetime,Type int,OriginalAmount money,Balance money,DBTimeStamp rowversion);
INSERT dbo.AccountReceivable(ID,StoreID,CustomerID,TransactionNumber,[Date],Type,OriginalAmount,Balance) VALUES(24403,606,71,17,'20261014 13:53:01',0,30200.54,30200.54),(1,606,71,17,'20210310 08:32:20',0,514088.54,500000);
CREATE TABLE dbo.OrderHistory(ID int PRIMARY KEY,StoreID int,OrderID int,TransactionNumber int,BatchNumber int,[Date] datetime,DeltaDeposit money,DBTimeStamp rowversion);
INSERT dbo.OrderHistory(ID,StoreID,OrderID,TransactionNumber,BatchNumber,[Date],DeltaDeposit) VALUES(19857,606,18870,17,80,'20261014 13:53:01',0),(1,606,17,17,7,'20210310 08:32:20',0);
CREATE TABLE dbo.TaxEntry(ID int PRIMARY KEY,StoreID int,TransactionNumber int,TransactionEntryID int,Tax money,TaxableAmount money,DBTimeStamp rowversion);
INSERT dbo.TaxEntry(ID,StoreID,TransactionNumber,TransactionEntryID,Tax,TaxableAmount) VALUES(65198,606,17,65233,0,7690.49),(65199,606,17,65234,0,5139.24),(65200,606,17,65235,0,12231.57),(65201,606,17,65236,0,5139.24),(1,606,17,1,0,514088.54);
CREATE TABLE dbo.Item(ID int PRIMARY KEY,Quantity float,QuantityCommitted float);
INSERT dbo.Item VALUES(13879,2,0),(13881,0,0),(46608,0,0),(46609,0,0);
CREATE TABLE dbo.SOD_Item_Logs(ItemID int,ItemLookupCode varchar(50),[Type] varchar(max),ReferenceID varchar(50),QuantityORIS int,LastQuantity int,NewQuantity int,LastCommitted int,NewCommitted int,[Date] datetime);
INSERT dbo.SOD_Item_Logs VALUES(13879,'25732UNT','Sales Transaction Logs','17',1,3,2,0,0,'20261014 13:53:13.433'),(13881,'25734UNT','Sales Transaction Logs','17',1,1,0,0,0,'20261014 13:53:13.460'),(46608,'06299UNT','Sales Transaction Logs','17',3,3,0,0,0,'20261014 13:53:13.483'),(46609,'06300UNT','Sales Transaction Logs','17',1,1,0,0,0,'20261014 13:53:13.487');
CREATE TABLE dbo.SOD_WO_History(EventID bigint IDENTITY NOT NULL CONSTRAINT PK_SOD_WO_History PRIMARY KEY,
 OrderID int NOT NULL,EventTime datetime2(3) NOT NULL CONSTRAINT DF_SOD_WO_History_Time DEFAULT SYSDATETIME(),EventName nvarchar(60) NOT NULL,Actor nvarchar(128) NOT NULL,Terminal nvarchar(256) NOT NULL,ItemID int NULL,Details nvarchar(2000) NOT NULL);
CREATE INDEX IX_SOD_WO_History_Order ON dbo.SOD_WO_History(OrderID,EventID DESC);
INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details) VALUES(18870,N'Original',N'Fixture',N'Test',N'Preserve history');
'@
    $backup=Join-Path $directory 'HO_fixture.bak';$safeBackup=$backup.Replace("'","''")
    Sql "BACKUP DATABASE HO_PO_DB TO DISK=N'$safeBackup' WITH COPY_ONLY,CHECKSUM;"
    $script=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\HO_PO_DB_23266_Run_In_SSMS.sql'))
    $script=[regex]::Replace($script,"DECLARE @BackupPath nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) "DECLARE @BackupPath nvarchar(4000)=N'$safeBackup';"})
    Sql $script
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Default SSMS execution rehearses and rolls back item relinks'
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 1) 'Rehearsal restores the legacy audit schema'
    Check ((Scalar 'SELECT TransactionNumber FROM dbo.AccountReceivable WHERE ID=24403') -eq 17) 'Rehearsal restores receivable and order links'
    $backupHelper=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Create_Local_HO_Backup.sql'))
    $backupHelper=[regex]::Replace($backupHelper,"DECLARE @BackupFolder nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) "DECLARE @BackupFolder nvarchar(4000)=N'$($directory.Replace("'","''"))';"})
    $helperPath=[string](Scalar $backupHelper);$safeHelperPath=$helperPath.Replace("'","''")
    Check ((Test-Path -LiteralPath $helperPath) -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Backup helper creates a readable unique file without repairing records'
    Check ((Scalar "SELECT COUNT(*) FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id=b.media_set_id WHERE m.physical_device_name=N'$safeHelperPath' AND b.is_copy_only=1 AND b.has_backup_checksums=0") -eq 1) 'Backup helper uses copy-only without backup checksums'
    $helperLocal=$script.Replace('DECLARE @LocalTestMode bit=0;','DECLARE @LocalTestMode bit=1;')
    $helperLocal=[regex]::Replace($helperLocal,"DECLARE @BackupPath nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) "DECLARE @BackupPath nvarchar(4000)=N'$safeHelperPath';"})
    Sql $helperLocal
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Backup helper output is accepted by the local rehearsal'
    $rejected=$false;try{Sql ($backupHelper.Replace("<>N'MSI'","<>N'FixtureOtherMachine'"))}catch{if($_.Exception.ToString() -notmatch 'only for the local MSI test computer'){throw};$rejected=$true}
    Check $rejected 'Backup helper rejects another machine before writing a backup'
    $missingFolderHelper=$backupHelper.Replace($directory.Replace("'","''"),(Join-Path $directory 'missing-folder').Replace("'","''"))
    $rejected=$false;try{Scalar $missingFolderHelper | Out-Null}catch{if($_.Exception.ToString() -notmatch 'Cannot open backup device'){throw};$rejected=$true}
    Check $rejected 'Backup creation failure stops before reporting a usable path'
    $noChecksum=Join-Path $directory 'HO_without_checksums.bak';$safeNoChecksum=$noChecksum.Replace("'","''")
    Sql "BACKUP DATABASE HO_PO_DB TO DISK=N'$safeNoChecksum' WITH NO_CHECKSUM;"
    $copied=Join-Path $directory 'HO_copied_backup.bak';Copy-Item -LiteralPath $noChecksum -Destination $copied
    $safeCopied=$copied.Replace("'","''")
    $copiedScript=[regex]::Replace($script,"DECLARE @BackupPath nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) "DECLARE @BackupPath nvarchar(4000)=N'$safeCopied';"})
    $rejected=$false;try{Sql $copiedScript}catch{if($_.Exception.ToString() -notmatch 'copy-only full HO_PO_DB backup with checksums'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Live mode still rejects a copied non-checksummed backup before changes'
    $localScript=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\HO_PO_DB_23266_LOCAL_TEST.sql'))
    $localScript=[regex]::Replace($localScript,"DECLARE @BackupPath nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) "DECLARE @BackupPath nvarchar(4000)=N'$safeCopied';"})
    Check ((Scalar "SELECT COUNT(*) FROM msdb.dbo.backupmediafamily WHERE physical_device_name=N'$safeCopied'") -eq 0) 'Copied fixture file has no local backup-history record'
    Sql $localScript
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0 -and (Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 1) 'Local-only mode accepts copied non-checksummed backup and rolls back the rehearsal'
    $placeholderScript=$localScript.Replace($safeCopied,'SET_VERIFIED_FULL_BACKUP_PATH_HERE')
    $rejected=$false;try{Sql $placeholderScript}catch{if($_.Exception.ToString() -notmatch 'Set BackupPath'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Unedited backup placeholder is rejected before changes'
    Sql ($placeholderScript.Replace('SET_VERIFIED_FULL_BACKUP_PATH_HERE',$safeCopied))
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Replacing all placeholder occurrences no longer makes the path guard reject a valid file'
    $rejected=$false;try{Sql ($localScript.Replace($safeCopied,'   '))}catch{if($_.Exception.ToString() -notmatch 'Set BackupPath'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Whitespace-only backup path is rejected before changes'
    $wrongMachine=$localScript.Replace("<>N'MSI'","<>N'FixtureOtherMachine'")
    $rejected=$false;try{Sql $wrongMachine}catch{if($_.Exception.ToString() -notmatch 'Local test mode is permitted only on MSI'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Local exception is rejected on a different machine before repair'
    $missingFile=$localScript.Replace($safeCopied,(Join-Path $directory 'missing-backup.bak').Replace("'","''"))
    $rejected=$false;try{Sql $missingFile}catch{if($_.Exception.ToString() -notmatch 'Cannot open backup device'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 0) 'Local mode still requires an accessible backup file'
    $committed=$script.Replace('DECLARE @Commit bit=0;','DECLARE @Commit bit=1;')
    Sql $committed
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 4) 'Explicit commit relinks all four diagnosed items'
    Check ((Scalar 'SELECT CONVERT(money,ROUND(SUM(Quantity*Price),2)) FROM dbo.TransactionEntry WHERE TransactionNumber=23266') -eq 30200.54) 'Floating quantities retain the exact currency total'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TaxEntry WHERE TransactionNumber=23266') -eq 4) 'All four zero-tax rows get the correct transaction link'
    Check ((Scalar 'SELECT TransactionNumber FROM dbo.AccountReceivable WHERE ID=24403') -eq 23266 -and (Scalar 'SELECT TransactionNumber FROM dbo.OrderHistory WHERE ID=19857') -eq 23266) 'Receivable and source Work Order close reference the real invoice'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TenderEntry WHERE TransactionNumber=23266 AND Amount=30200.54') -eq 1) 'Payment is relinked once without creating another tender'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry WHERE ID=1 AND TransactionNumber=17 AND Price=514088.54') -eq 1 -and (Scalar 'SELECT Balance FROM dbo.AccountReceivable WHERE ID=1') -eq 500000) 'Unrelated 2021 transaction data remains unchanged'
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 0 -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History') -eq 1) 'Committed audit conversion preserves the original event'
    Sql $committed
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry') -eq 5 -and (Scalar 'SELECT COUNT(*) FROM dbo.TenderEntry') -eq 2) 'A repeated committed run creates no duplicate sale or payment'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='17'") -eq 4) 'Optional item-log relinking stays off by default'
    Sql ($committed.Replace('DECLARE @RelinkItemLogs bit=0;','DECLARE @RelinkItemLogs bit=1;'))
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='23266'") -eq 4 -and (Scalar 'SELECT SUM(QuantityORIS) FROM dbo.SOD_Item_Logs') -eq 6) 'Optional log relinking preserves quantities'
    Sql 'UPDATE dbo.AccountReceivable SET Balance=1 WHERE ID=24403;'
    $rejected=$false;try{Sql $committed}catch{if($_.Exception.ToString() -notmatch 'subsequent payment activity'){throw};$rejected=$true}
    Check ($rejected -and (Scalar 'SELECT @@TRANCOUNT') -eq 0) 'Subsequent receivable activity blocks recovery and closes its transaction'
    "PASS: $checks standalone HO recovery checks; artifacts: $directory"
} finally {
    if($connection){$connection.Dispose()}
    if($master){try{$c=$master.CreateCommand();$c.CommandText='IF DB_ID(''HO_PO_DB'') IS NOT NULL BEGIN ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE HO_PO_DB;END;';$null=$c.ExecuteNonQuery();$c.Dispose()}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k | Out-Null;& $localdb delete $instance | Out-Null}
}
