# Builds a single self-contained SSMS script from the reviewed recovery sources.
# Only reads/writes workspace files; never opens a SQL connection.
param([string]$VerifiedBackupPath='SET_VERIFIED_FULL_BACKUP_PATH_HERE',[string]$LocalBackupPath='SET_VERIFIED_FULL_BACKUP_PATH_HERE')
$ErrorActionPreference='Stop'
$fix=[IO.File]::ReadAllText((Join-Path $PSScriptRoot '..\Migrations\Fix_WorkOrder_History_Identity.sql'))
$repair=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Repair_HO_Transaction_23266.sql'))
$header=@'
/* LIVE HO_PO_DB recovery for transaction 23266 / work order 18870.
   Connect SSMS to 172.16.4.50. Pause posting during this short maintenance.
   First run with @Commit=0: all schema/data changes are rolled back.
   After checking the results, set @Commit=1 and execute the entire file again.
   Set @BackupPath to today's verified copy-only full HO backup.
   No credentials are included. This is NOT an updater-importable migration.
   Original journal receipt is retained and may still print transaction 17. */
USE [HO_PO_DB];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET LOCK_TIMEOUT 5000;
DECLARE @Commit bit=0;
DECLARE @LocalTestMode bit=0; -- 1 is permitted only on the local test computer MSI.
DECLARE @RelinkItemLogs bit=0; -- Optional: scans the large unindexed item-log table.
DECLARE @BackupPath nvarchar(4000)=N'SET_VERIFIED_FULL_BACKUP_PATH_HERE';
IF @@TRANCOUNT<>0 THROW 52700, 'Close any existing transaction before running the complete recovery file.', 1;
IF DB_NAME()<>N'HO_PO_DB' THROW 52701, 'Wrong database.', 1;
IF LEFT(LTRIM(@BackupPath),4)=N'SET_' OR NULLIF(LTRIM(RTRIM(@BackupPath)),N'') IS NULL
    THROW 52702, 'Set BackupPath to a current HO_PO_DB full backup before running.', 1;
IF @LocalTestMode=1 AND ISNULL(UPPER(CONVERT(sysname,SERVERPROPERTY('MachineName'))),N'')<>N'MSI'
    THROW 52708, 'Local test mode is permitted only on MSI; use the checksum-protected live script on other servers.', 1;
IF @LocalTestMode=0
BEGIN
IF NOT EXISTS(SELECT 1 FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id=b.media_set_id
    WHERE b.database_name=N'HO_PO_DB' AND b.type='D' AND b.is_copy_only=1 AND b.has_backup_checksums=1
      AND b.backup_finish_date>=CONVERT(date,GETDATE()) AND m.physical_device_name=@BackupPath)
    THROW 52703, 'A copy-only full HO_PO_DB backup with checksums from today is required.', 1;
RESTORE VERIFYONLY FROM DISK=@BackupPath WITH CHECKSUM;
END
ELSE
BEGIN
    -- Copied local backups need not have a local msdb record or backup checksums.
    -- Default verification still checks any checksums that are present.
    RESTORE VERIFYONLY FROM DISK=@BackupPath;
    PRINT N'LOCAL TEST ONLY (MSI): copied backup accepted without requiring checksum/history metadata.';
END;
SELECT @@SERVERNAME TargetServer,DB_NAME() TargetDatabase,@Commit CommitRequested,@LocalTestMode LocalTestMode,@RelinkItemLogs RelinkItemLogs;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lock int;
    EXEC @lock=sys.sp_getapplock @Resource=N'HO_PO_DB_23266_IdentityRecovery',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000;
    IF @lock<0 THROW 52704, 'Another recovery is running. No changes applied.', 1;
    DECLARE @historyBefore nvarchar(max)=(SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH('Event'),ROOT('History'));
    DECLARE @triggersBefore nvarchar(max)=(SELECT name,is_disabled,OBJECT_DEFINITION(object_id) Definition FROM sys.triggers
        WHERE name LIKE N'SOD_trg_WO_History_%' ORDER BY name FOR XML PATH('Trigger'));
    DECLARE @HistoryFix nvarchar(max)=N'
'@
$middle=@'
';
    EXEC sys.sp_executesql @HistoryFix;

'@
$footer=@'

    IF @RelinkItemLogs=1
    BEGIN
        -- Match all four confirmed log records; no quantity or inventory fields change.
        SELECT * INTO #LogBefore FROM dbo.SOD_Item_Logs WITH(UPDLOCK)
        WHERE ReferenceID IN('17','23266') AND [Type]='Sales Transaction Logs'
          AND ((ItemID=13879 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.433')
            OR(ItemID=13881 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.460')
            OR(ItemID=46608 AND QuantityORIS=3 AND [Date]='20261014 13:53:13.483')
            OR(ItemID=46609 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.487')) OPTION(MAXDOP 1);
        IF (SELECT COUNT(*) FROM #LogBefore)<>4 OR (SELECT COUNT(DISTINCT ItemID) FROM #LogBefore)<>4
            THROW 52705, 'The four item-log records differ; roll back.', 1;
        UPDATE l SET ReferenceID='23266' FROM dbo.SOD_Item_Logs l JOIN #LogBefore b
          ON l.ItemID=b.ItemID AND l.[Date]=b.[Date] AND l.QuantityORIS=b.QuantityORIS
          AND l.[Type]=b.[Type] AND l.ReferenceID=b.ReferenceID
        WHERE l.ReferenceID='17' OPTION(MAXDOP 1);
        IF (SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='23266' AND [Type]='Sales Transaction Logs'
          AND ((ItemID=13879 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.433')
            OR(ItemID=13881 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.460')
            OR(ItemID=46608 AND QuantityORIS=3 AND [Date]='20261014 13:53:13.483')
            OR(ItemID=46609 AND QuantityORIS=1 AND [Date]='20261014 13:53:13.487')))<>4
            THROW 52706, 'Item-log relink verification failed.', 1;
        DROP TABLE #LogBefore;
    END;
    IF @historyBefore<>(SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH('Event'),ROOT('History'))
      OR @triggersBefore<>(SELECT name,is_disabled,OBJECT_DEFINITION(object_id) Definition FROM sys.triggers
          WHERE name LIKE N'SOD_trg_WO_History_%' ORDER BY name FOR XML PATH('Trigger'))
        THROW 52707, 'Existing audit events or trigger definitions changed.', 1;
    SELECT t.TransactionNumber,t.RecallID,t.Total,t.SalesTax,
      (SELECT COUNT(*) FROM dbo.TransactionEntry e WHERE e.TransactionNumber=t.TransactionNumber AND e.StoreID=t.StoreID) ItemLines,
      (SELECT SUM(e.Quantity*e.Price) FROM dbo.TransactionEntry e WHERE e.TransactionNumber=t.TransactionNumber AND e.StoreID=t.StoreID) ItemTotal,
      (SELECT SUM(p.Amount) FROM dbo.TenderEntry p WHERE p.TransactionNumber=t.TransactionNumber AND p.StoreID=t.StoreID) TenderTotal,
      (SELECT COUNT(*) FROM dbo.TaxEntry x WHERE x.TransactionNumber=t.TransactionNumber AND x.StoreID=t.StoreID) TaxRows,
      COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity') HistoryUsesIdentity
    FROM dbo.[Transaction] t WHERE t.TransactionNumber=23266;
    SELECT ID,TransactionNumber,ItemID,Quantity,Price FROM dbo.TransactionEntry WHERE ID IN(65233,65234,65235,65236) ORDER BY ID;
    SELECT ID,TransactionNumber,OriginalAmount,Balance FROM dbo.AccountReceivable WHERE ID=24403;
    SELECT ID,OrderID,TransactionNumber FROM dbo.OrderHistory WHERE ID=19857;
    IF @Commit=1
    BEGIN COMMIT TRANSACTION; SELECT N'COMMITTED: HO transaction 23266 links and RMS-safe audit numbering repaired.' Result; END
    ELSE
    BEGIN ROLLBACK TRANSACTION; SELECT N'REHEARSAL PASSED: all repair changes rolled back. Set Commit=1 only after reviewing these results.' Result; END;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
'@
$output=$header+$fix.Replace("'","''")+$middle+$repair+$footer
$output=$output.Replace("DECLARE @BackupPath nvarchar(4000)=N'SET_VERIFIED_FULL_BACKUP_PATH_HERE';","DECLARE @BackupPath nvarchar(4000)=N'"+$VerifiedBackupPath.Replace("'","''")+"';")
$path=Join-Path $PSScriptRoot 'HO_PO_DB_23266_Run_In_SSMS.sql'
[IO.File]::WriteAllText($path,$output,[Text.UTF8Encoding]::new($false))
Write-Output "Created standalone recovery: $path"
$localOutput=$output.Replace('DECLARE @LocalTestMode bit=0;','DECLARE @LocalTestMode bit=1;')
$localDeclaration="DECLARE @BackupPath nvarchar(4000)=N'"+$LocalBackupPath.Replace("'","''")+"';"
$localOutput=[regex]::Replace($localOutput,"DECLARE @BackupPath nvarchar\(4000\)=N'(?:''|[^'])*';",[Text.RegularExpressions.MatchEvaluator]{param($m) $localDeclaration})
$localOutput=$localOutput.Replace('/* LIVE HO_PO_DB recovery','/* LOCAL RESTORE TEST on MSI only: HO_PO_DB recovery')
$localOutput=$localOutput.Replace('Connect SSMS to 172.16.4.50. Pause posting during this short maintenance.','Connect SSMS to localhost on MSI and use a disposable restored HO_PO_DB.')
$localOutput=$localOutput.Replace("Set @BackupPath to today's verified copy-only full HO backup.","Set @BackupPath to your local copied .bak file; backup checksums are optional.")
$localPath=Join-Path $PSScriptRoot 'HO_PO_DB_23266_LOCAL_TEST.sql'
[IO.File]::WriteAllText($localPath,$localOutput,[Text.UTF8Encoding]::new($false))
Write-Output "Created local-only recovery: $localPath"
