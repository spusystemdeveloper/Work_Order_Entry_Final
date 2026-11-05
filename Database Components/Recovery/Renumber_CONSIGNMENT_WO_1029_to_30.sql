/* CONSIGNMENT_DB / WIN-63HGUF2272P. Work Order 1029 -> 30 only.
   Run the entire file in SSMS with CONSIGNMENT_DB selected. This owns its transaction.
   Default @Commit = 0 runs a real rollback rehearsal. Close branch RMS, Work Order,
   queue and integration clients first. Use a new full copy-only backup with checksums
   on a unique path on the SQL Server. Review the rehearsal before setting @Commit = 1.
   Commit also archives seven orphan quote links and reseeds Order to 30 for next ID 31. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;

DECLARE @Commit bit = 0;
DECLARE @MaintenanceConfirmed bit = 0;
DECLARE @BackupPath nvarchar(4000) = N'';
DECLARE @ExpectedServer sysname = N'WIN-63HGUF2272P';

IF DB_NAME() <> N'CONSIGNMENT_DB' OR CONVERT(sysname, SERVERPROPERTY('ServerName')) <> @ExpectedServer
    THROW 54010, 'Wrong server/database for this renumber.', 1;
IF @@TRANCOUNT <> 0
    THROW 54011, 'Run the entire file in a fresh SSMS session outside any transaction.', 1;
IF @MaintenanceConfirmed <> 1
    THROW 54012, 'Confirm all branch clients and integrations are closed first.', 1;

DECLARE @BackupPosition int;
SELECT TOP (1) @BackupPosition = b.position
FROM msdb.dbo.backupset AS b
JOIN msdb.dbo.backupmediafamily AS m ON m.media_set_id = b.media_set_id
JOIN sys.database_recovery_status AS d ON d.database_id = DB_ID() AND d.database_guid = b.database_guid
WHERE b.database_name = N'CONSIGNMENT_DB' AND b.server_name = @ExpectedServer
  AND b.[type] = 'D' AND b.is_copy_only = 1 AND b.has_backup_checksums = 1 AND b.is_damaged = 0
  AND b.backup_start_date >= CONVERT(date, GETDATE())
  AND b.backup_start_date >= CONVERT(datetime, '2026-11-04T09:54:39.920', 126)
  AND m.physical_device_name = @BackupPath
ORDER BY b.backup_finish_date DESC, b.backup_set_id DESC;
IF @BackupPosition IS NULL
    THROW 54013, 'Supply a current-day full copy-only checksum backup of this branch database.', 1;
RESTORE VERIFYONLY FROM DISK = @BackupPath WITH FILE = @BackupPosition, CHECKSUM;

DECLARE @Scope table (TableName sysname PRIMARY KEY, KeyColumn sysname, Predicate nvarchar(120));
INSERT @Scope VALUES
    (N'Order', N'ID', N'ID IN (30, 1029)'),
    (N'OrderEntry', N'ID', N'OrderID IN (30, 1029)'),
    (N'Queueing', N'id', N'OrderID IN (30, 1029) OR GroupTo IN (30, 1029)'),
    (N'SOD_WO_History', N'EventID', N'OrderID IN (30, 1029)');

DECLARE @Mapped table (TableName sysname, ColumnName sysname, PRIMARY KEY (TableName, ColumnName));
INSERT @Mapped VALUES (N'Order', N'ID'), (N'OrderEntry', N'OrderID'),
    (N'Queueing', N'OrderID'), (N'Queueing', N'GroupTo'), (N'SOD_WO_History', N'OrderID');

DECLARE @Checked table
(
    TableName sysname, ColumnName sysname, DomainFilter nvarchar(160) NOT NULL DEFAULT N'',
    PRIMARY KEY (TableName, ColumnName)
);
INSERT @Checked (TableName, ColumnName) VALUES
    (N'OrderHistory', N'OrderID'), (N'Transaction', N'RecallID'),
    (N'Queueing_Quote', N'OrderID'), (N'Queueing_Quote', N'GroupTo'),
    (N'QuoteTenderEntry', N'OrderID'), (N'RMSTMP_Sales', N'RecallID'),
    (N'SI_ItemExpireTemp', N'OrderID'), (N'SI_OrderPrice', N'OrderID'), (N'SI_PickListTemp', N'OrderID'),
    (N'SOD_WO', N'OrderID'), (N'SOD_WO_TIO', N'OrderID'),
    (N'SOD_WO_LOGS', N'OrderId'), (N'TransactionHold', N'RecallID'), (N'TransactionHoldEntry', N'RecallID'),
    (N'ItemExpireHistory', N'ReferenceID');
INSERT @Checked (TableName, ColumnName, DomainFilter) VALUES
    (N'Journal', N'ReferenceID', N' AND TransactionType = 2'),
    (N'SOD_Item_Logs', N'ReferenceID', N' AND [Type] IN (''Order Entry Logs'', ''Order Update Logs'')');

-- Only these tables are written. Match the exact reviewed definitions and enabled states.
DECLARE @Triggers table (TableName sysname, TriggerName sysname, DefinitionHash varchar(64));
INSERT @Triggers VALUES
    (N'Order', N'SOD_trg_WO_History_Order', '48528B22E4F9D9F4F246CD9648FE68E85832C82992C1D8463014EA5EAF09CC85'),
    (N'Order', N'trg_Order_WOUpdate', '4D540ED2F124250DEDC6CF365C222C7804FDC910A472ACAD05DC1DCBCAF5DFB3'),
    (N'OrderEntry', N'SOD_trg_WO_History_Entry', 'FB3B60CA9C95F67403E22095D030DBAD725C3C6789E6B33D05D3E1616FEB1179'),
    (N'OrderEntry', N'tr_OrderEntry_D', '882F0203FCA49F2E1103B076FE91691F3D8BAFB60B7F0AACC628F73C46D9487D'),
    (N'OrderEntry', N'tr_OrderEntry_I', 'EC691E3F9C61D96FE38C49F2E8D1855A4C5B80F66E943A4ABF2CE4EFF107D36A'),
    (N'OrderEntry', N'tr_OrderEntry_U', '2A340A7B30C265B00DB9A79D63659AC8081764A37746B0305EF1EE012E411518'),
    (N'OrderEntry', N'trg_OrderEntry_QtyOfset_Delete', 'C866F8FDE9374C88424309D22138455A729DC8BC4A3581C04EF43E0726F6B764'),
    (N'OrderEntry', N'trg_OrderEntry_WOEdit', '9F9A28281EFF7B34BB14AFEBEAFE0354D0B009B335F9E456FA336CE516F4FFC2'),
    (N'OrderEntry', N'trg_OrderEntry_WORec', '24EFD0558C4206F32F10EC2255A9975669DFD7377464727955E076FCC5D9050C'),
    (N'Queueing', N'SOD_trg_WO_History_Queue', '0CAAA7EFD9028A18F38A75BA633CEB90A11061BF2BCFAE0034681774634DF502');

DECLARE @ActualTriggers table (TableName sysname, TriggerName sysname, DefinitionHash varchar(64), IsDisabled bit);
DECLARE @Snapshots table (TableName sysname PRIMARY KEY, Expected nvarchar(max), RowsBefore bigint);
DECLARE @Table sysname, @Column sysname, @Filter nvarchar(160), @Key sysname, @Predicate nvarchar(120),
        @Qualified nvarchar(260), @Sql nvarchar(max), @Columns nvarchar(max), @ExpectedColumns nvarchar(max),
        @Before nvarchar(max), @After nvarchar(max), @Rows bigint, @Probe int, @LockResult int;

BEGIN TRY
    BEGIN TRANSACTION;
    EXEC @LockResult = sys.sp_getapplock @Resource = N'CONSIGNMENT_WorkOrder_Identity_Maintenance',
        @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 5000;
    IF @LockResult < 0 THROW 54014, 'Another Consignment renumber is running.', 1;

    DECLARE lock_tables CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName FROM @Scope UNION SELECT TableName FROM @Checked
        UNION SELECT N'InventoryTransferLog' UNION SELECT N'SOD_ImportedQuote' ORDER BY TableName;
    OPEN lock_tables;
    FETCH NEXT FROM lock_tables INTO @Table;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Qualified = N'dbo.' + QUOTENAME(@Table);
        IF OBJECT_ID(@Qualified, N'U') IS NULL
            THROW 54015, 'A reviewed table is absent or inaccessible. Rerun the preflight.', 1;
        SET @Sql = N'SELECT TOP (1) @Probe = 1 FROM ' + @Qualified + N' WITH (TABLOCKX, HOLDLOCK);';
        EXEC sys.sp_executesql @Sql, N'@Probe int OUTPUT', @Probe OUTPUT;
        FETCH NEXT FROM lock_tables INTO @Table;
    END;
    CLOSE lock_tables;
    DEALLOCATE lock_tables;

    IF EXISTS (SELECT 1 FROM sys.tables AS t JOIN @Scope AS s ON t.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(s.TableName))
               WHERE t.is_replicated = 1 OR t.is_merge_published = 1 OR t.is_tracked_by_cdc = 1
                  OR t.temporal_type <> 0 OR t.is_memory_optimized = 1)
       OR EXISTS (SELECT 1 FROM sys.foreign_keys AS f JOIN @Scope AS s
                  ON OBJECT_ID(N'dbo.' + QUOTENAME(s.TableName)) IN (f.parent_object_id, f.referenced_object_id))
       OR EXISTS (SELECT 1 FROM sys.sql_expression_dependencies AS d JOIN @Scope AS s
                  ON d.referenced_id = OBJECT_ID(N'dbo.' + QUOTENAME(s.TableName)) WHERE d.is_schema_bound_reference = 1)
       OR EXISTS (SELECT 1 FROM sys.triggers WHERE parent_class = 0 AND is_disabled = 0)
       OR EXISTS (SELECT 1 FROM sys.server_triggers WHERE is_disabled = 0)
        THROW 54016, 'An unsupported dependency or DDL trigger requires review.', 1;

    INSERT @ActualTriggers
    SELECT s.TableName, t.name, CONVERT(varchar(64), HASHBYTES('SHA2_256', m.definition), 2), t.is_disabled
    FROM @Scope AS s JOIN sys.triggers AS t ON t.parent_id = OBJECT_ID(N'dbo.' + QUOTENAME(s.TableName))
    LEFT JOIN sys.sql_modules AS m ON m.object_id = t.object_id;
    IF EXISTS (SELECT TableName, TriggerName, DefinitionHash FROM @ActualTriggers WHERE IsDisabled = 0
               EXCEPT SELECT TableName, TriggerName, DefinitionHash FROM @Triggers)
       OR EXISTS (SELECT TableName, TriggerName, DefinitionHash FROM @Triggers
                  EXCEPT SELECT TableName, TriggerName, DefinitionHash FROM @ActualTriggers WHERE IsDisabled = 0)
       OR EXISTS (SELECT 1 FROM @ActualTriggers WHERE IsDisabled = 1)
        THROW 54017, 'Write-table trigger definitions or states differ from the branch preflight.', 1;

    IF EXISTS (SELECT 1 FROM sys.tables AS t JOIN sys.columns AS c ON c.object_id = t.object_id
               WHERE c.name IN (N'OrderID', N'RecallID', N'QuoteID', N'GroupTo', N'ReferenceID', N'WorkOrderID', N'WOID')
                 AND NOT EXISTS (SELECT 1 FROM @Mapped AS m WHERE t.schema_id = SCHEMA_ID(N'dbo')
                                 AND m.TableName = t.name AND m.ColumnName = c.name)
                 AND NOT EXISTS (SELECT 1 FROM @Checked AS a WHERE t.schema_id = SCHEMA_ID(N'dbo')
                                 AND a.TableName = t.name AND a.ColumnName = c.name)
                 AND NOT (t.schema_id = SCHEMA_ID(N'dbo') AND
                          ((t.name = N'SOD_ImportedQuote' AND c.name IN (N'ReferenceID', N'QuoteID'))
                           OR (t.name = N'InventoryTransferLog' AND c.name = N'ReferenceID'))))
        THROW 54018, 'An additional possible order-reference column requires review.', 1;

    IF ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.[Order]'), N'ID', 'IsIdentity'), 0) <> 1
       OR ISNULL(IDENT_INCR(N'dbo.[Order]'), 0) <> 1
       OR ISNULL(IDENT_CURRENT(N'dbo.[Order]'), 0) <> 1029
       OR ISNULL((SELECT MAX(ID) FROM dbo.[Order]), 0) <> 1029
       OR EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[Order]')
                  AND (is_computed = 1 OR generated_always_type <> 0 OR is_hidden = 1))
        THROW 54019, 'The Order identity, maximum ID or header schema changed.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints AS d
                   JOIN sys.sql_expression_dependencies AS e ON e.referencing_id = d.object_id
                   WHERE d.parent_object_id = OBJECT_ID(N'dbo.SOD_WO_History')
                     AND d.parent_column_id = COLUMNPROPERTY(d.parent_object_id, N'EventID', 'ColumnId')
                     AND e.referenced_id = OBJECT_ID(N'dbo.SOD_WO_History_EventSequence', N'SO'))
       OR ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'), N'EventID', 'IsIdentity'), 1) <> 0
        THROW 54020, 'History EventID must use the reviewed sequence default.', 1;

    IF (SELECT COUNT(*) FROM dbo.[Order] WHERE ID = 1029 AND StoreID = 811 AND [Type] = 2 AND Closed = 0
        AND [Time] = CONVERT(datetime, '2026-11-04T09:54:39.920', 126)
        AND LastUpdated = CONVERT(datetime, '2026-11-04T09:54:39.920', 126)) <> 1
       OR (SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID = 1029) <> 5
       OR (SELECT COUNT(*) FROM dbo.Queueing WHERE OrderID = 1029) <> 1
       OR (SELECT COUNT(*) FROM dbo.Queueing WHERE GroupTo = 1029) <> 1
       OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID = 1029) <> 17
        THROW 54021, 'The source order or its linked row counts changed since preflight.', 1;

    IF EXISTS (SELECT 1 FROM dbo.[Order] WHERE ID >= 30 AND ID <> 1029)
        THROW 54029, 'An existing order at or above 30 prevents reseeding for next ID 31.', 1;
    IF EXISTS (SELECT 1 FROM dbo.OrderEntry WHERE OrderID BETWEEN 31 AND 1028)
       OR EXISTS (SELECT 1 FROM dbo.Queueing WHERE OrderID BETWEEN 31 AND 1028 OR GroupTo BETWEEN 31 AND 1028)
       OR EXISTS (SELECT 1 FROM dbo.SOD_WO_History WHERE OrderID BETWEEN 31 AND 1028)
        THROW 54030, 'Work-order links already use IDs in the range that would be reused.', 1;
    IF EXISTS (SELECT 1 FROM dbo.OrderEntry WHERE OrderID = 30)
       OR EXISTS (SELECT 1 FROM dbo.Queueing WHERE OrderID = 30 OR GroupTo = 30)
       OR EXISTS (SELECT 1 FROM dbo.SOD_WO_History WHERE OrderID = 30)
        THROW 54022, 'Destination 30 has an existing order or orphaned work-order links.', 1;

    DECLARE check_references CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, ColumnName, DomainFilter FROM @Checked;
    OPEN check_references;
    FETCH NEXT FROM check_references INTO @Table, @Column, @Filter;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Qualified = N'dbo.' + QUOTENAME(@Table);
        IF COL_LENGTH(@Qualified, @Column) IS NULL
            THROW 54023, 'A reviewed order-reference column is absent or inaccessible.', 1;
        SET @Columns = QUOTENAME(@Column);
        IF @Table = N'SOD_Item_Logs' SET @Columns = N'TRY_CAST(' + @Columns + N' AS bigint)';
        SET @Sql = N'IF EXISTS (SELECT 1 FROM ' + @Qualified + N' WHERE ' + @Columns
            + CASE WHEN @Table = N'ItemExpireHistory' THEN N' IN (30, 1029)' ELSE N' BETWEEN 30 AND 1029' END
            + @Filter + N') THROW 54024, ''Additional reference in ' + @Qualified + N'.' + QUOTENAME(@Column)
            + N'. Stop for review.'', 1;';
        EXEC sys.sp_executesql @Sql;
        FETCH NEXT FROM check_references INTO @Table, @Column, @Filter;
    END;
    CLOSE check_references;
    DEALLOCATE check_references;

    IF EXISTS (SELECT 1 FROM dbo.InventoryTransferLog AS l WHERE l.ReferenceID IN (30, 1029)
               AND NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrder AS p WITH (UPDLOCK, HOLDLOCK)
                   JOIN dbo.PurchaseOrderEntry AS pe WITH (UPDLOCK, HOLDLOCK) ON pe.PurchaseOrderID = p.ID
                   WHERE l.ID = 130 AND l.[Type] = 1 AND l.ReferenceID = 30
                     AND l.ReferenceEntryID = 135 AND l.ItemID = 20888
                     AND p.ID = l.ReferenceID AND p.POType = 2
                     AND pe.ID = l.ReferenceEntryID AND pe.ItemID = l.ItemID))
        THROW 54033, 'An unverified inventory transfer references 30 or 1029. Stop for review.', 1;

    -- Imported quote ReferenceID belongs to the source document. QuoteID is the local link.
    IF (SELECT COUNT(*) FROM dbo.SOD_ImportedQuote WHERE ReferenceID = 30) <> 4
       OR EXISTS (SELECT 1 FROM dbo.SOD_ImportedQuote WHERE ReferenceID = 1029)
        THROW 54025, 'The imported-quote source references changed since preflight.', 1;

    DECLARE @ImportObject int = OBJECT_ID(N'dbo.SOD_ImportedQuote', N'U');
    IF ISNULL(COLUMNPROPERTY(@ImportObject, N'QuoteID', 'AllowsNull'), 0) <> 1
       OR EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = @ImportObject)
       OR EXISTS (SELECT 1 FROM sys.foreign_keys WHERE @ImportObject IN (parent_object_id, referenced_object_id))
       OR EXISTS (SELECT 1 FROM sys.tables WHERE object_id = @ImportObject AND
           (is_replicated = 1 OR is_merge_published = 1 OR is_tracked_by_cdc = 1 OR temporal_type <> 0 OR is_memory_optimized = 1))
       OR OBJECT_ID(N'dbo.SOD_ImportedQuoteLinkArchive_WO1029') IS NOT NULL
        THROW 54034, 'Unexpected import table dependency, non-nullable QuoteID, or archive already exists.', 1;

    DECLARE @OrphanLinks table (ImportLogID bigint PRIMARY KEY, OriginalQuoteID int, SourceReferenceID int, ImportDate datetime);
    INSERT @OrphanLinks VALUES (1,49,8,'20170602'), (2,50,8,'20170602'),
        (3,989,14,'20170830'), (4,991,13,'20170830'), (5,993,15,'20170830'),
        (6,995,11,'20170830'), (7,996,12,'20170830');

    IF (SELECT COUNT_BIG(*) FROM dbo.SOD_ImportedQuote WHERE QuoteID BETWEEN 30 AND 1029) <> 7
       OR EXISTS (SELECT 1 FROM @OrphanLinks AS e WHERE NOT EXISTS
           (SELECT 1 FROM dbo.SOD_ImportedQuote AS q WHERE q.ID = e.ImportLogID AND q.QuoteID = e.OriginalQuoteID
               AND q.ReferenceID = e.SourceReferenceID AND q.ImportDate = e.ImportDate))
        THROW 54035, 'The seven reviewed orphan import links changed. Stop for review.', 1;

    SELECT DISTINCT ItemID INTO #ConsignmentRenumberItems FROM dbo.OrderEntry WHERE OrderID = 1029;
    DECLARE @InventoryBefore nvarchar(max) = (SELECT i.ID, i.Quantity, i.QuantityCommitted FROM dbo.Item AS i WITH (UPDLOCK, HOLDLOCK)
        JOIN #ConsignmentRenumberItems AS ids ON ids.ItemID = i.ID ORDER BY i.ID FOR XML PATH('Item'), ROOT('Inventory'));

    -- Capture all values, substituting only the requested ID fields. SQL regenerates rowversion.
    DECLARE snapshot_rows CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, KeyColumn, Predicate FROM @Scope;
    OPEN snapshot_rows;
    FETCH NEXT FROM snapshot_rows INTO @Table, @Key, @Predicate;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Qualified = N'dbo.' + QUOTENAME(@Table);
        SELECT @ExpectedColumns = STUFF((SELECT N',' + CASE
            WHEN m.ColumnName IS NOT NULL THEN N'CASE ' + QUOTENAME(c.name) + N' WHEN 1029 THEN 30 ELSE ' + QUOTENAME(c.name) + N' END'
            WHEN c.system_type_id IN (35, 99) THEN N'CONVERT(nvarchar(max),' + QUOTENAME(c.name) + N')'
            WHEN c.system_type_id = 34 THEN N'CONVERT(varbinary(max),' + QUOTENAME(c.name) + N')'
            ELSE QUOTENAME(c.name) END + N' AS ' + QUOTENAME(c.name)
            FROM sys.columns AS c LEFT JOIN @Mapped AS m ON m.TableName = @Table AND m.ColumnName = c.name
            WHERE c.object_id = OBJECT_ID(@Qualified) AND c.system_type_id <> 189
            ORDER BY c.column_id FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, N'');
        SET @Sql = N'SELECT @Xml = CONVERT(nvarchar(max), (SELECT ' + @ExpectedColumns + N' FROM ' + @Qualified
            + N' WHERE ' + @Predicate + N' ORDER BY ' + QUOTENAME(@Key)
            + N' FOR XML PATH(''Row''), BINARY BASE64, ELEMENTS XSINIL, ROOT(''Rows'')));'
            + N'SELECT @Rows = COUNT_BIG(*) FROM ' + @Qualified + N' WHERE ' + @Predicate + N';';
        EXEC sys.sp_executesql @Sql, N'@Xml nvarchar(max) OUTPUT, @Rows bigint OUTPUT', @Before OUTPUT, @Rows OUTPUT;
        INSERT @Snapshots VALUES (@Table, @Before, @Rows);
        FETCH NEXT FROM snapshot_rows INTO @Table, @Key, @Predicate;
    END;
    CLOSE snapshot_rows;
    DEALLOCATE snapshot_rows;

    CREATE TABLE dbo.SOD_ImportedQuoteLinkArchive_WO1029
    (
        ImportLogID bigint NOT NULL PRIMARY KEY,
        OriginalQuoteID int NOT NULL,
        SourceReferenceID int NOT NULL,
        OriginalImportDate datetime NOT NULL,
        OriginalSalesRepID int NULL,
        ArchivedAtUtc datetime2(3) NOT NULL
    );
    INSERT dbo.SOD_ImportedQuoteLinkArchive_WO1029
    SELECT q.ID, q.QuoteID, q.ReferenceID, q.ImportDate, q.SalesRepID, SYSUTCDATETIME()
    FROM dbo.SOD_ImportedQuote AS q JOIN @OrphanLinks AS e ON e.ImportLogID = q.ID;

    UPDATE q SET QuoteID = NULL
    FROM dbo.SOD_ImportedQuote AS q JOIN @OrphanLinks AS e ON e.ImportLogID = q.ID;
    IF @@ROWCOUNT <> 7 OR EXISTS
       (SELECT ImportLogID, SourceReferenceID, OriginalImportDate, OriginalSalesRepID
        FROM dbo.SOD_ImportedQuoteLinkArchive_WO1029
        EXCEPT
        SELECT q.ID, q.ReferenceID, q.ImportDate, q.SalesRepID FROM dbo.SOD_ImportedQuote AS q WHERE q.QuoteID IS NULL)
        THROW 54036, 'Import link cleanup or duplicate-detection fields failed verification.', 1;

    -- The write-table trigger states are restored inside the same transaction.
    DISABLE TRIGGER ALL ON dbo.[Order];
    DISABLE TRIGGER ALL ON dbo.OrderEntry;
    DISABLE TRIGGER ALL ON dbo.Queueing;

    SELECT * INTO #ConsignmentRenumberHeader FROM dbo.[Order] WHERE ID = 1029;
    DELETE dbo.[Order] WHERE ID = 1029;
    SELECT @Columns = STUFF((SELECT N',' + QUOTENAME(name) FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.[Order]') AND system_type_id <> 189
        ORDER BY column_id FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, N'');
    SET @ExpectedColumns = REPLACE(@Columns, N'[ID]', N'30');
    SET IDENTITY_INSERT dbo.[Order] ON;
    SET @Sql = N'INSERT dbo.[Order] (' + @Columns + N') SELECT ' + @ExpectedColumns + N' FROM #ConsignmentRenumberHeader;';
    EXEC sys.sp_executesql @Sql;
    SET IDENTITY_INSERT dbo.[Order] OFF;

    UPDATE dbo.OrderEntry SET OrderID = 30 WHERE OrderID = 1029;
    UPDATE dbo.Queueing
    SET OrderID = CASE WHEN OrderID = 1029 THEN 30 ELSE OrderID END,
        GroupTo = CASE WHEN GroupTo = 1029 THEN 30 ELSE GroupTo END
    WHERE OrderID = 1029 OR GroupTo = 1029;
    UPDATE dbo.SOD_WO_History SET OrderID = 30 WHERE OrderID = 1029;

    DECLARE verify_rows CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.TableName, s.KeyColumn, s.Predicate, b.Expected
        FROM @Scope AS s JOIN @Snapshots AS b ON b.TableName = s.TableName;
    OPEN verify_rows;
    FETCH NEXT FROM verify_rows INTO @Table, @Key, @Predicate, @Before;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Qualified = N'dbo.' + QUOTENAME(@Table);
        SELECT @Columns = STUFF((SELECT N',' + CASE
            WHEN system_type_id IN (35, 99) THEN N'CONVERT(nvarchar(max),' + QUOTENAME(name) + N')'
            WHEN system_type_id = 34 THEN N'CONVERT(varbinary(max),' + QUOTENAME(name) + N')'
            ELSE QUOTENAME(name) END + N' AS ' + QUOTENAME(name)
            FROM sys.columns WHERE object_id = OBJECT_ID(@Qualified) AND system_type_id <> 189
            ORDER BY column_id FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, N'');
        SET @Sql = N'SELECT @Xml = CONVERT(nvarchar(max), (SELECT ' + @Columns + N' FROM ' + @Qualified
            + N' WHERE ' + @Predicate + N' ORDER BY ' + QUOTENAME(@Key)
            + N' FOR XML PATH(''Row''), BINARY BASE64, ELEMENTS XSINIL, ROOT(''Rows'')));';
        EXEC sys.sp_executesql @Sql, N'@Xml nvarchar(max) OUTPUT', @After OUTPUT;
        IF @After IS NULL OR CONVERT(varbinary(max), @Before) <> CONVERT(varbinary(max), @After)
            THROW 54026, 'Post-change values differ from the expected ID-only mapping.', 1;
        FETCH NEXT FROM verify_rows INTO @Table, @Key, @Predicate, @Before;
    END;
    CLOSE verify_rows;
    DEALLOCATE verify_rows;

    DECLARE @InventoryAfter nvarchar(max) = (SELECT i.ID, i.Quantity, i.QuantityCommitted FROM dbo.Item AS i
        JOIN #ConsignmentRenumberItems AS ids ON ids.ItemID = i.ID ORDER BY i.ID FOR XML PATH('Item'), ROOT('Inventory'));
    IF CONVERT(varbinary(max), @InventoryBefore) <> CONVERT(varbinary(max), @InventoryAfter)
        THROW 54027, 'Inventory or commitments changed during the renumber.', 1;
    IF ISNULL(IDENT_CURRENT(N'dbo.[Order]'), 0) <> 1029
        THROW 54028, 'The Order identity changed unexpectedly.', 1;

    ENABLE TRIGGER ALL ON dbo.[Order];
    ENABLE TRIGGER ALL ON dbo.OrderEntry;
    ENABLE TRIGGER ALL ON dbo.Queueing;

    SELECT @ExpectedServer AS BranchServer, DB_NAME() AS BranchDatabase, @Commit AS CommitRequested,
           1029 AS OldOrderID, 30 AS NewOrderID, 1029 AS IdentityBefore, 31 AS ProposedNextOrderID;
    SELECT TableName, RowsBefore FROM @Snapshots ORDER BY TableName;
    SELECT ImportLogID, OriginalQuoteID, SourceReferenceID FROM dbo.SOD_ImportedQuoteLinkArchive_WO1029 ORDER BY ImportLogID;
    SELECT ID, StoreID, Closed, [Time], Total, ReferenceNumber FROM dbo.[Order] WHERE ID = 30;

    IF @Commit = 1
    BEGIN
        INSERT dbo.SOD_WO_History (OrderID, EventName, Actor, Terminal, Details)
        VALUES (30, N'Order ID changed', ORIGINAL_LOGIN(), LEFT(HOST_NAME(), 256),
                N'Manual renumber: Order ID 1029 to 30. Order identity reseeded to 30.');
        IF ISNULL((SELECT MAX(ID) FROM dbo.[Order]), 0) <> 30
            THROW 54031, 'The highest order ID must be 30 before reseeding.', 1;
        DBCC CHECKIDENT (N'dbo.[Order]', RESEED, 30) WITH NO_INFOMSGS;
        IF ISNULL(IDENT_CURRENT(N'dbo.[Order]'), 0) <> 30
            THROW 54032, 'Order identity reseed did not produce the expected value.', 1;
        COMMIT;
        SELECT N'COMMITTED: Work Order 1029 changed to 30. Next automatic ID is 31.' AS Result;
    END
    ELSE
    BEGIN
        ROLLBACK;
        SELECT N'REHEARSAL ONLY: order changes and quote-link archive rolled back. Identity remains 1029.' AS Result;
    END;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    SET IDENTITY_INSERT dbo.[Order] OFF;
    THROW;
END CATCH;
