/* LOCAL RESTORE TEST on MSI only: HO_PO_DB recovery for transaction 23266 / work order 18870.
   Connect SSMS to localhost on MSI and use a disposable restored HO_PO_DB.
   First run with @Commit=0: all schema/data changes are rolled back.
   After checking the results, set @Commit=1 and execute the entire file again.
   Set @BackupPath to your local copied .bak file; backup checksums are optional.
   No credentials are included. This is NOT an updater-importable migration.
   Original journal receipt is retained and may still print transaction 17. */
USE [HO_PO_DB];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;
SET LOCK_TIMEOUT 5000;
DECLARE @Commit bit=0;
DECLARE @LocalTestMode bit=1; -- 1 is permitted only on the local test computer MSI.
DECLARE @RelinkItemLogs bit=0; -- Optional: scans the large unindexed item-log table.
DECLARE @BackupPath nvarchar(4000)=N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\Backup\HO_PO_DB_LocalTest_48CB1C50654D466BBFDBAE31EC568F41.bak';
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
    DECLARE @HistoryFix nvarchar(max)=N'/* SQL Server 2016 RTM. Run through the updater or inside an explicit transaction.
   Replaces the custom history IDENTITY with a sequence-backed EventID, preserving
   all existing events and their IDs. RMS uses @@IDENTITY: history must not change it.
   No RMS business table, transaction link, amount, or inventory value is edited.
   Test a restore and back up first. Unsupported custom history schemas stop. */
SET XACT_ABORT ON;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1
    THROW 52400, ''Run the history identity fix inside the updater transaction.'', 1;
IF OBJECT_ID(N''dbo.SOD_WO_History'') IS NULL RETURN;
IF OBJECT_ID(N''dbo.SOD_WO_History'',N''U'') IS NULL
    THROW 52401, ''SOD_WO_History is not a table. Review it before migration.'', 1;

DECLARE @history int=OBJECT_ID(N''dbo.SOD_WO_History''), @rows bigint, @max bigint;
SELECT @rows=COUNT_BIG(*),@max=ISNULL(MAX(EventID),0)
FROM dbo.SOD_WO_History WITH(TABLOCKX,HOLDLOCK);
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=@history)<>8 OR EXISTS (
    SELECT name,type_name,max_length,is_nullable FROM (VALUES
    (N''EventID'',N''bigint'',8,0),(N''OrderID'',N''int'',4,0),(N''EventTime'',N''datetime2'',7,0),
    (N''EventName'',N''nvarchar'',120,0),(N''Actor'',N''nvarchar'',256,0),
    (N''Terminal'',N''nvarchar'',512,0),(N''ItemID'',N''int'',4,1),(N''Details'',N''nvarchar'',4000,0)
    ) expected(name,type_name,max_length,is_nullable)
    EXCEPT SELECT name,TYPE_NAME(user_type_id),max_length,is_nullable FROM sys.columns WHERE object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=@history AND (is_computed=1 OR (name=N''EventTime'' AND scale<>3)))
    THROW 52402, ''History schema is incompatible; existing events were preserved.'', 1;

IF COLUMNPROPERTY(@history,N''EventID'',''IsIdentity'')=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.sequences WHERE object_id=OBJECT_ID(N''dbo.SOD_WO_History_EventSequence'')
                  AND system_type_id=127 AND CONVERT(bigint,increment)=1 AND is_cycling=0
                  AND CONVERT(bigint,current_value)>=@max)
       OR NOT EXISTS(SELECT 1 FROM sys.sql_expression_dependencies
                     WHERE referencing_id=(SELECT default_object_id FROM sys.columns WHERE object_id=@history AND name=N''EventID'')
                       AND referenced_id=OBJECT_ID(N''dbo.SOD_WO_History_EventSequence''))
        THROW 52403, ''Non-identity history needs its approved EventSequence default. Review the schema.'', 1;
    RETURN;
END;

-- Changing IDENTITY requires rebuilding this custom table. Reject dependencies or
-- customizations that cannot be safely retained by this focused migration.
IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=@history OR referenced_object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.sql_expression_dependencies WHERE referenced_id=@history AND is_schema_bound_reference=1)
 OR EXISTS(SELECT 1 FROM sys.triggers WHERE parent_id=@history)
 OR EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=1 AND major_id=@history)
 OR EXISTS(SELECT 1 FROM sys.objects WHERE object_id=@history AND principal_id IS NOT NULL)
 OR EXISTS(SELECT 1 FROM sys.tables WHERE object_id=@history AND (temporal_type<>0 OR is_memory_optimized=1 OR is_tracked_by_cdc=1))
 OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=@history AND collation_name IS NOT NULL
            AND collation_name<>CONVERT(sysname,DATABASEPROPERTYEX(DB_NAME(),''Collation'')))
 OR EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND index_id>0
            AND (name NOT IN(N''PK_SOD_WO_History'',N''IX_SOD_WO_History_Order'') OR data_space_id<>FILEGROUP_ID(N''PRIMARY'') OR has_filter=1 OR is_disabled=1))
 OR NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND name=N''PK_SOD_WO_History'' AND is_primary_key=1 AND type=1)
 OR (SELECT COUNT(*) FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
     WHERE ix.object_id=@history AND ix.name=N''PK_SOD_WO_History'' AND ic.key_ordinal=1 AND c.name=N''EventID'')<>1
 OR EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     WHERE ix.object_id=@history AND ix.name=N''PK_SOD_WO_History'' AND (ic.key_ordinal>1 OR ic.is_descending_key=1))
 OR EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND name=N''IX_SOD_WO_History_Order'' AND (type<>2 OR is_unique=1))
 OR EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
     WHERE ix.object_id=@history AND ix.name=N''IX_SOD_WO_History_Order''
       AND (ic.is_included_column=1 OR ic.key_ordinal>2 OR (ic.key_ordinal=1 AND (c.name<>N''OrderID'' OR ic.is_descending_key<>0))
            OR (ic.key_ordinal=2 AND (c.name<>N''EventID'' OR ic.is_descending_key<>1))))
 OR EXISTS(SELECT 1 FROM sys.indexes ix WHERE ix.object_id=@history AND ix.name=N''IX_SOD_WO_History_Order''
     AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id AND ic.key_ordinal>0)<>2)
 OR EXISTS(SELECT 1 FROM sys.default_constraints WHERE parent_object_id=@history AND name<>N''DF_SOD_WO_History_Time'')
    THROW 52404, ''History has custom dependencies, ownership, collation, or indexes. Review before rebuilding.'', 1;
IF OBJECT_ID(N''dbo.SOD_WO_History_RmsSafe'') IS NOT NULL
 OR OBJECT_ID(N''dbo.SOD_WO_History_EventSequence'') IS NOT NULL
 OR @max>=9223372036854775807
    THROW 52405, ''History migration object conflict or event number limit. Review before migration.'', 1;

DECLARE @permissions TABLE(principal_name sysname,grantor_name sysname,permission_name sysname,state char(1),column_name sysname NULL);
INSERT @permissions
SELECT USER_NAME(p.grantee_principal_id),USER_NAME(p.grantor_principal_id),p.permission_name,p.state,c.name
FROM sys.database_permissions p LEFT JOIN sys.columns c ON c.object_id=p.major_id AND c.column_id=p.minor_id
WHERE p.class=1 AND p.major_id=@history;
DECLARE @timeDefault nvarchar(max)=(SELECT definition FROM sys.default_constraints WHERE parent_object_id=@history AND name=N''DF_SOD_WO_History_Time'');
IF @timeDefault IS NULL THROW 52406, ''The expected history time default is missing.'', 1;
DECLARE @ddl nvarchar(max)=N''CREATE SEQUENCE dbo.SOD_WO_History_EventSequence AS bigint START WITH ''+CONVERT(nvarchar(30),@max+1)+N'' INCREMENT BY 1 MINVALUE 1 NO CYCLE CACHE 100;'';
EXEC sys.sp_executesql @ddl;
SET @ddl=N''CREATE TABLE dbo.SOD_WO_History_RmsSafe (
 EventID bigint NOT NULL CONSTRAINT DF_SOD_WO_History_EventID DEFAULT(NEXT VALUE FOR dbo.SOD_WO_History_EventSequence),
 OrderID int NOT NULL,EventTime datetime2(3) NOT NULL CONSTRAINT DF_SOD_WO_History_Time_RmsSafe DEFAULT ''+@timeDefault+N'',
 EventName nvarchar(60) NOT NULL,Actor nvarchar(128) NOT NULL,Terminal nvarchar(256) NOT NULL,
 ItemID int NULL,Details nvarchar(2000) NOT NULL,
 CONSTRAINT PK_SOD_WO_History_RmsSafe PRIMARY KEY CLUSTERED(EventID));'';
EXEC sys.sp_executesql @ddl;
INSERT dbo.SOD_WO_History_RmsSafe(EventID,OrderID,EventTime,EventName,Actor,Terminal,ItemID,Details)
SELECT EventID,OrderID,EventTime,EventName,Actor,Terminal,ItemID,Details FROM dbo.SOD_WO_History;
IF (SELECT COUNT_BIG(*) FROM dbo.SOD_WO_History_RmsSafe)<>@rows
 OR EXISTS(SELECT * FROM dbo.SOD_WO_History EXCEPT SELECT * FROM dbo.SOD_WO_History_RmsSafe)
 OR EXISTS(SELECT * FROM dbo.SOD_WO_History_RmsSafe EXCEPT SELECT * FROM dbo.SOD_WO_History)
    THROW 52407, ''History copy verification failed. Roll back this plan.'', 1;
CREATE INDEX IX_SOD_WO_History_Order ON dbo.SOD_WO_History_RmsSafe(OrderID,EventID DESC);
DROP TABLE dbo.SOD_WO_History;
EXEC sys.sp_rename N''dbo.SOD_WO_History_RmsSafe'',N''SOD_WO_History'';
EXEC sys.sp_rename N''dbo.PK_SOD_WO_History_RmsSafe'',N''PK_SOD_WO_History'',N''OBJECT'';
EXEC sys.sp_rename N''dbo.DF_SOD_WO_History_Time_RmsSafe'',N''DF_SOD_WO_History_Time'',N''OBJECT'';

-- Reapply object/column permissions with their original grantors.
DECLARE @principal sysname,@grantor sysname,@permission sysname,@state char(1),@column sysname;
DECLARE history_permissions CURSOR LOCAL FAST_FORWARD FOR
 SELECT principal_name,grantor_name,permission_name,state,column_name FROM @permissions ORDER BY CASE WHEN column_name IS NULL THEN 0 ELSE 1 END;
OPEN history_permissions;
FETCH NEXT FROM history_permissions INTO @principal,@grantor,@permission,@state,@column;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @ddl=CASE @state WHEN ''D'' THEN N''DENY '' WHEN ''R'' THEN N''REVOKE '' ELSE N''GRANT '' END+@permission+
      N'' ON OBJECT::dbo.SOD_WO_History''+CASE WHEN @column IS NULL THEN N'''' ELSE N'' (''+QUOTENAME(@column)+N'')'' END+
      CASE WHEN @state=''R'' THEN N'' FROM '' ELSE N'' TO '' END+QUOTENAME(@principal)+
      CASE WHEN @state=''W'' THEN N'' WITH GRANT OPTION'' ELSE N'''' END+N'' AS ''+QUOTENAME(@grantor)+N'';'';
    EXEC sys.sp_executesql @ddl;
    FETCH NEXT FROM history_permissions INTO @principal,@grantor,@permission,@state,@column;
END;
CLOSE history_permissions;
DEALLOCATE history_permissions;
IF COLUMNPROPERTY(OBJECT_ID(N''dbo.SOD_WO_History''),N''EventID'',''IsIdentity'')<>0
 OR (SELECT COUNT_BIG(*) FROM dbo.SOD_WO_History)<>@rows
    THROW 52408, ''History identity fix verification failed. Roll back this plan.'', 1;
';
    EXEC sys.sp_executesql @HistoryFix;
/* Incident-specific live HO recovery. Not automatically included in the updater.
   Requires a verified full backup and an atomic caller transaction.
   The old 2021 transaction 17 is valid; do not bulk-relink its records. */
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
IF DB_NAME()<>N'HO_PO_DB' THROW 52600, 'This recovery is only for the diagnosed HO_PO_DB.', 1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601, 'Recovery requires a caller transaction and verified backup.', 1;
IF ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity'),-1)<>0
    THROW 52602, 'Apply the history identity compatibility fix first.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK) WHERE TransactionNumber=23266
    AND StoreID=606 AND BatchNumber=80 AND RecallID=18870 AND RecallType=4 AND [Time]='20261014 13:53:01'
    AND CustomerID=71 AND Total=30200.54 AND SalesTax=0)
    THROW 52603, 'The diagnosed transaction header no longer matches.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK) WHERE TransactionNumber=17
    AND StoreID=606 AND BatchNumber=7 AND [Time]='20210310 08:32:20' AND Total=514088.54)
    THROW 52604, 'The unrelated old transaction header differs.', 1;
IF EXISTS(SELECT 1 FROM sys.triggers t JOIN sys.trigger_events e ON e.object_id=t.object_id
    WHERE e.type_desc=N'UPDATE' AND t.is_disabled=0 AND t.parent_id IN
    (OBJECT_ID(N'dbo.TransactionEntry'),OBJECT_ID(N'dbo.TenderEntry'),OBJECT_ID(N'dbo.AccountReceivable'),OBJECT_ID(N'dbo.OrderHistory'),OBJECT_ID(N'dbo.TaxEntry'),OBJECT_ID(N'dbo.SOD_Item_Logs')))
    THROW 52605, 'An enabled UPDATE trigger requires side-effect review.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=606
    AND TransactionNumber IN(17,23266) AND TransactionTime='20261014 13:53:01' AND SalesTax=0
    AND ((ID=65233 AND ItemID=13879 AND Quantity=1 AND Price=7690.49)
     OR(ID=65234 AND ItemID=13881 AND Quantity=1 AND Price=5139.24)
     OR(ID=65235 AND ItemID=46608 AND Quantity=3 AND Price=4077.19)
     OR(ID=65236 AND ItemID=46609 AND Quantity=1 AND Price=5139.24)))<>4
 OR EXISTS(SELECT 1 FROM dbo.TransactionEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID NOT IN(65233,65234,65235,65236))
    THROW 52606, 'Item lines differ from the diagnosed sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.TenderEntry WITH(UPDLOCK,HOLDLOCK) WHERE ID=25815 AND StoreID=606
    AND TransactionNumber IN(17,23266) AND BatchNumber=80 AND OrderHistoryID=19857 AND TenderID=2 AND PaymentID=0 AND Amount=30200.54)
 OR EXISTS(SELECT 1 FROM dbo.TenderEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID<>25815)
    THROW 52607, 'Tender differs from the diagnosed charge sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WITH(UPDLOCK,HOLDLOCK) WHERE ID=24403 AND StoreID=606 AND CustomerID=71
    AND TransactionNumber IN(17,23266) AND [Date]='20261014 13:53:01' AND Type=0 AND OriginalAmount=30200.54 AND Balance=30200.54)
    THROW 52608, 'Receivable differs or has subsequent payment activity.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WITH(UPDLOCK,HOLDLOCK) WHERE ID=19857 AND StoreID=606 AND OrderID=18870
    AND TransactionNumber IN(17,23266) AND BatchNumber=80 AND [Date]='20261014 13:53:01' AND DeltaDeposit=0)
    THROW 52609, 'Order history differs from the diagnosed close.', 1;
IF (SELECT COUNT(*) FROM dbo.TaxEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=606 AND TransactionNumber IN(17,23266) AND Tax=0
    AND ((ID=65198 AND TransactionEntryID=65233 AND TaxableAmount=7690.49)
      OR(ID=65199 AND TransactionEntryID=65234 AND TaxableAmount=5139.24)
      OR(ID=65200 AND TransactionEntryID=65235 AND TaxableAmount=12231.57)
      OR(ID=65201 AND TransactionEntryID=65236 AND TaxableAmount=5139.24)))<>4
 OR EXISTS(SELECT 1 FROM dbo.TaxEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID NOT IN(65198,65199,65200,65201))
    THROW 52610, 'Tax links differ from the diagnosed sale.', 1;

SELECT * INTO #EntryBefore FROM dbo.TransactionEntry WHERE ID IN(65233,65234,65235,65236);
SELECT * INTO #TenderBefore FROM dbo.TenderEntry WHERE ID=25815;
SELECT * INTO #ARBefore FROM dbo.AccountReceivable WHERE ID=24403;
SELECT * INTO #OrderHistoryBefore FROM dbo.OrderHistory WHERE ID=19857;
SELECT * INTO #TaxBefore FROM dbo.TaxEntry WHERE ID IN(65198,65199,65200,65201);
SELECT * INTO #ItemsBefore FROM dbo.Item WHERE ID IN(13879,13881,46608,46609);
-- Preserve every unrelated old transaction-17 row, including rowversions.
DECLARE @oldEntries nvarchar(max)=(SELECT * FROM dbo.TransactionEntry WHERE TransactionNumber=17 AND ID NOT IN(65233,65234,65235,65236) ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldTenders nvarchar(max)=(SELECT * FROM dbo.TenderEntry WHERE TransactionNumber=17 AND ID<>25815 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldAR nvarchar(max)=(SELECT * FROM dbo.AccountReceivable WHERE TransactionNumber=17 AND ID<>24403 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldOrders nvarchar(max)=(SELECT * FROM dbo.OrderHistory WHERE TransactionNumber=17 AND ID<>19857 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldTax nvarchar(max)=(SELECT * FROM dbo.TaxEntry WHERE TransactionNumber=17 AND ID NOT IN(65198,65199,65200,65201) ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=17 FOR XML PATH('Row'));
DECLARE @newHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=23266 FOR XML PATH('Row'));

UPDATE dbo.TransactionEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID IN(65233,65234,65235,65236);
UPDATE dbo.TenderEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=25815;
UPDATE dbo.AccountReceivable SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=24403;
UPDATE dbo.OrderHistory SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=19857;
UPDATE dbo.TaxEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID IN(65198,65199,65200,65201);

DECLARE @table sysname,@before sysname,@columns nvarchar(max),@sql nvarchar(max);
DECLARE checks CURSOR LOCAL FAST_FORWARD FOR SELECT * FROM(VALUES
 (N'TransactionEntry',N'#EntryBefore'),(N'TenderEntry',N'#TenderBefore'),
 (N'AccountReceivable',N'#ARBefore'),(N'OrderHistory',N'#OrderHistoryBefore'),(N'TaxEntry',N'#TaxBefore')) v(t,b);
OPEN checks; FETCH NEXT FROM checks INTO @table,@before;
WHILE @@FETCH_STATUS=0
BEGIN
    SELECT @columns=STUFF((SELECT N','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.'+@table)
        AND name<>N'TransactionNumber' AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
    SET @sql=N'IF EXISTS(SELECT '+@columns+N' FROM '+@before+N' EXCEPT SELECT '+@columns+N' FROM dbo.'+QUOTENAME(@table)+
        N' WHERE ID IN(SELECT ID FROM '+@before+N')) THROW 52611, ''A non-link field changed; roll back recovery.'', 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM checks INTO @table,@before;
END;
CLOSE checks; DEALLOCATE checks;
IF EXISTS(SELECT ID,Quantity,QuantityCommitted FROM #ItemsBefore EXCEPT SELECT ID,Quantity,QuantityCommitted FROM dbo.Item)
 OR @oldHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=17 FOR XML PATH('Row'))
 OR @newHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=23266 FOR XML PATH('Row'))
 OR @oldEntries<>(SELECT * FROM dbo.TransactionEntry WHERE TransactionNumber=17 AND ID NOT IN(65233,65234,65235,65236) ORDER BY ID FOR XML PATH('Row'))
 OR @oldTenders<>(SELECT * FROM dbo.TenderEntry WHERE TransactionNumber=17 AND ID<>25815 ORDER BY ID FOR XML PATH('Row'))
 OR @oldAR<>(SELECT * FROM dbo.AccountReceivable WHERE TransactionNumber=17 AND ID<>24403 ORDER BY ID FOR XML PATH('Row'))
 OR @oldOrders<>(SELECT * FROM dbo.OrderHistory WHERE TransactionNumber=17 AND ID<>19857 ORDER BY ID FOR XML PATH('Row'))
 OR @oldTax<>(SELECT * FROM dbo.TaxEntry WHERE TransactionNumber=17 AND ID NOT IN(65198,65199,65200,65201) ORDER BY ID FOR XML PATH('Row'))
    THROW 52612, 'Inventory, transaction headers or unrelated old records changed.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266 AND StoreID=606)<>4
 OR (SELECT CONVERT(money,ROUND(SUM(Quantity*Price),2)) FROM dbo.TransactionEntry WHERE TransactionNumber=23266 AND StoreID=606)<>30200.54
 OR (SELECT COUNT(*) FROM dbo.TenderEntry WHERE TransactionNumber=23266 AND StoreID=606)<>1
 OR (SELECT SUM(Amount) FROM dbo.TenderEntry WHERE TransactionNumber=23266 AND StoreID=606)<>30200.54
 OR (SELECT COUNT(*) FROM dbo.TaxEntry WHERE TransactionNumber=23266 AND StoreID=606)<>4
 OR NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WHERE ID=24403 AND TransactionNumber=23266)
 OR NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WHERE ID=19857 AND TransactionNumber=23266)
    THROW 52613, 'Final transaction linkage verification failed.', 1;
DROP TABLE #EntryBefore;DROP TABLE #TenderBefore;DROP TABLE #ARBefore;
DROP TABLE #OrderHistoryBefore;DROP TABLE #TaxBefore;DROP TABLE #ItemsBefore;
/* The four historical item-log references and compressed receipt are evidence.
   They are handled separately; this core transaction repair does not rewrite them. */

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