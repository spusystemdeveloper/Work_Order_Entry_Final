/* PERSONAL SSMS RECOVERY ONLY. SQL Server 2016. NOT an updater migration.
   Requested October 19, 2026: WO 18912 -> 18905 and related structured references.
   NO RESEED: identity stays 18912; next allocation remains 18913 if no other
   allocation intervenes. This file does not reuse 18906-18911.
   Close ALL HO RMS, add-in, queue and integration clients before either run.
   Backup-path/history/checksum validation removed at the user's explicit request.
   Default Commit=0 performs changes inside a transaction and rolls them back.
   Review all grids; Commit=1 commits only this exact reviewed order.
   Preserves prices, quantities, stock, tax, queue stages and existing history.
   Trigger suspension is transaction-scoped maintenance only, with exact hashes
   checked first; rollback restores trigger states. Unknown references stop it.
   Scans legacy item logs: use a maintenance window and sufficient query timeout.
   Read RENUMBER_HO_18912_TO_18905.md. NEVER run via Database Updater.
   Prepared only; NOT executed on live by the assistant. */
USE [HO_PO_DB];
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
DECLARE @Commit bit = 0;
DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';
IF @@TRANCOUNT<>0 THROW 53600,'Close the existing transaction; run this entire file in a fresh SSMS query.',1;
IF DB_NAME()<>N'HO_PO_DB' OR @@SERVERNAME<>@ExpectedServer
    THROW 53601,'Wrong server/database. No changes applied.',1;
DROP TABLE IF EXISTS #HORenumberScope;
DROP TABLE IF EXISTS #HORenumberMapColumns;
DROP TABLE IF EXISTS #HORenumberExpectedTriggers;
DROP TABLE IF EXISTS #HORenumberSnapshots;
DROP TABLE IF EXISTS #HORenumberUnsupported;
DROP TABLE IF EXISTS #HORenumberHeaders;
CREATE TABLE #HORenumberScope(TableName sysname PRIMARY KEY, KeyColumns nvarchar(300) NOT NULL, Predicate nvarchar(500) NOT NULL);
INSERT #HORenumberScope VALUES
 (N'Order',N'[ID]',N'[ID] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'OrderEntry',N'[ID]',N'[OrderID] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'OrderHistory',N'[ID]',N'[OrderID] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'Queueing',N'[id]',N'[OrderID] IN(18905,18906,18907,18908,18909,18910,18911,18912) OR [GroupTo] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'Transaction',N'[StoreID],[TransactionNumber]',N'[RecallID] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'Journal',N'[ID]',N'[ReferenceID] IN(18905,18906,18907,18908,18909,18910,18911,18912) AND [TransactionType]=2'),
 (N'SOD_WO_LOGS',N'[id]',N'[OrderId] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'SOD_WO_History',N'[EventID]',N'[OrderID] IN(18905,18906,18907,18908,18909,18910,18911,18912)'),
 (N'SOD_Item_Logs',N'[ItemID],[Date],[Type],[ReferenceID],[QuantityORIS],[LastQuantity],[NewQuantity],[LastCommitted],[NewCommitted],[ItemLookupCode]',N'[ReferenceID] IN(''18905'',''18906'',''18907'',''18908'',''18909'',''18910'',''18911'',''18912'') AND [Type] IN(''Order Entry Logs'',''Order Update Logs'')');
CREATE TABLE #HORenumberMapColumns(TableName sysname,ColumnName sysname,PRIMARY KEY(TableName,ColumnName));
INSERT #HORenumberMapColumns VALUES(N'Order',N'ID'),(N'OrderEntry',N'OrderID'),(N'OrderHistory',N'OrderID'),
 (N'Queueing',N'OrderID'),(N'Queueing',N'GroupTo'),(N'Transaction',N'RecallID'),(N'Journal',N'ReferenceID'),
 (N'SOD_WO_LOGS',N'OrderId'),(N'SOD_WO_History',N'OrderID'),(N'SOD_Item_Logs',N'ReferenceID');
CREATE TABLE #HORenumberExpectedTriggers(TableName sysname,TriggerName sysname,DefinitionHash varchar(64),PRIMARY KEY(TableName,TriggerName));
-- Exact reviewed live definitions, including the separately installed save-timeout fix.
INSERT #HORenumberExpectedTriggers VALUES
 (N'Journal',N'tr_JournalDeleted','9E4B64B79CEC1215BA1904D421E51850DB98B36C2BCB84466A9275BBE8DD5D14'),
 (N'Order',N'SOD_trg_WO_History_Order','48528B22E4F9D9F4F246CD9648FE68E85832C82992C1D8463014EA5EAF09CC85'),
 (N'OrderEntry',N'SOD_trg_WO_History_Entry','FB3B60CA9C95F67403E22095D030DBAD725C3C6789E6B33D05D3E1616FEB1179'),
 (N'OrderEntry',N'tr_OrderEntry_D','882F0203FCA49F2E1103B076FE91691F3D8BAFB60B7F0AACC628F73C46D9487D'),
 (N'OrderEntry',N'tr_OrderEntry_I','EC691E3F9C61D96FE38C49F2E8D1855A4C5B80F66E943A4ABF2CE4EFF107D36A'),
 (N'OrderEntry',N'tr_OrderEntry_U','2A340A7B30C265B00DB9A79D63659AC8081764A37746B0305EF1EE012E411518'),
 (N'OrderEntry',N'trg_OrderEntry_ItemLogs','5CFB6436B9EAB3B846B8DFAB5AB9AAB52215515CB88C4BB44A70D10234058A8B'),
 (N'OrderHistory',N'tr_OrderHistory_I','2E41E28EA36DE6901EAC9B67B4AED8984AF77E6C78C6E4FE44735B5886A20F35'),
 (N'Queueing',N'SOD_trg_WO_History_Queue','0CAAA7EFD9028A18F38A75BA633CEB90A11061BF2BCFAE0034681774634DF502'),
 (N'SOD_WO_LOGS',N'SOD_trg_WO_History_Approval','662EBF7DD4C91519C96148DE69DDCF4E40EF30A3BCB00AAD04BB4A536049E8EC'),
 (N'Transaction',N'SOD_trg_WO_History_Invoice','F175A90FEBED4147F21CCEA18F154A936E540C0DB339C341144E66B1C3B34A6B'),
 (N'Transaction',N'tr_Transaction_I','D26E9BF45356450EE7CB7A3F0D205B4834B646CD8FC02C304F82558D296BA344'),
 (N'Transaction',N'trg_Transaction_closeQueue','C05735AB1E94A98C719476EC11926763BDD97EBBB21C527D13543EE5CA2AA318');
CREATE TABLE #HORenumberSnapshots(TableName sysname PRIMARY KEY,ExpectedXml nvarchar(max),RowsBefore bigint);
DECLARE @table sysname,@predicate nvarchar(500),@keys nvarchar(300),@sql nvarchar(max),
 @columns nvarchar(max),@expectedColumns nvarchar(max),@sets nvarchar(max),@before nvarchar(max),
 @after nvarchar(max),@rows bigint,@lock int,@dummy int,@identityBefore decimal(38,0);
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC @lock=sys.sp_getapplock @Resource=N'HO_WorkOrder_Identity_Maintenance',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000;
    IF @lock<0 THROW 53603,'Another renumber is running.',1;
    -- Table locks also cover clients which do not use the add-in application lock.
    DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM #HORenumberScope ORDER BY TableName;
    OPEN tables_cursor; FETCH NEXT FROM tables_cursor INTO @table;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @sql=N'SELECT TOP(1) @dummy=1 FROM dbo.'+QUOTENAME(@table)+N' WITH(TABLOCKX,HOLDLOCK);';
        EXEC sys.sp_executesql @sql,N'@dummy int OUTPUT',@dummy OUTPUT;
        FETCH NEXT FROM tables_cursor INTO @table;
    END;
    CLOSE tables_cursor; DEALLOCATE tables_cursor;
    IF EXISTS(SELECT 1 FROM sys.tables t JOIN #HORenumberScope s ON t.object_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName))
        WHERE t.is_replicated=1 OR t.is_merge_published=1 OR t.is_tracked_by_cdc=1 OR t.temporal_type<>0 OR t.is_memory_optimized=1)
      OR EXISTS(SELECT 1 FROM sys.foreign_keys f JOIN #HORenumberScope s ON f.parent_object_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName)) OR f.referenced_object_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName)))
      OR EXISTS(SELECT 1 FROM sys.sql_expression_dependencies d JOIN #HORenumberScope s ON d.referenced_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName)) WHERE d.is_schema_bound_reference=1)
      OR EXISTS(SELECT 1 FROM sys.triggers WHERE parent_class=0 AND is_disabled=0)
      OR EXISTS(SELECT 1 FROM sys.server_triggers WHERE is_disabled=0)
        THROW 53604,'Unreviewed FK, schema-bound, replicated, tracked or DDL-trigger dependency. Stop for review.',1;
    IF EXISTS(SELECT s.TableName,t.name,CONVERT(varchar(64),HASHBYTES('SHA2_256',m.definition),2)
        FROM #HORenumberScope s JOIN sys.triggers t ON t.parent_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName))
        LEFT JOIN sys.sql_modules m ON m.object_id=t.object_id
        EXCEPT SELECT TableName,TriggerName,DefinitionHash FROM #HORenumberExpectedTriggers)
      OR EXISTS(SELECT TableName,TriggerName,DefinitionHash FROM #HORenumberExpectedTriggers EXCEPT
        SELECT s.TableName,t.name,CONVERT(varchar(64),HASHBYTES('SHA2_256',m.definition),2)
        FROM #HORenumberScope s JOIN sys.triggers t ON t.parent_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName))
        LEFT JOIN sys.sql_modules m ON m.object_id=t.object_id)
      OR EXISTS(SELECT 1 FROM #HORenumberScope s JOIN sys.triggers t ON t.parent_id=OBJECT_ID(N'dbo.'+QUOTENAME(s.TableName)) WHERE t.is_disabled=1)
        THROW 53605,'Trigger definitions/state changed since the read-only audit. Stop for review.',1;
    SET @identityBefore=IDENT_CURRENT(N'dbo.Order');
    IF @identityBefore<>18912 OR (SELECT MAX(ID) FROM dbo.[Order])<>18912
      OR EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID IN(18905,18906,18907,18908,18909,18910,18911))
      OR (SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18912 AND StoreID=606 AND Type=2 AND Closed=0
          AND [Time]=CONVERT(datetime,'2026-10-19T11:42:06.390',126)
          AND LastUpdated=CONVERT(datetime,'2026-10-19T11:42:06.390',126)
          AND Total=54412.5000 AND Tax=0 AND CustomerID=71)<>1
      OR (SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=18912)<>3
      OR NOT EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE ID=61753 AND OrderID=18912 AND ItemID=67317
                    AND QuantityOnOrder=1000 AND Price=20.2000 AND FullPrice=33.5000)
      OR NOT EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE ID=61754 AND OrderID=18912 AND ItemID=67321
                    AND QuantityOnOrder=750 AND Price=18.7000 AND FullPrice=31.0000)
      OR NOT EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE ID=61755 AND OrderID=18912 AND ItemID=67325
                    AND QuantityOnOrder=950 AND Price=21.2500 AND FullPrice=35.5000)
      OR (SELECT COUNT(*) FROM dbo.Queueing WHERE OrderID=18912)<>1
      OR NOT EXISTS(SELECT 1 FROM dbo.Queueing WHERE id=45921 AND OrderID=18912 AND GroupTo=18912 AND Status=0)
      OR (SELECT COUNT(*) FROM dbo.SOD_WO_LOGS WHERE OrderId=18912)<>3
      OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=18912)<>11
        THROW 53606,'Source order/lines/queue or identity changed, destination is occupied, or later activity occurred. Stop for review.',1;
    IF COLUMNPROPERTY(OBJECT_ID(N'dbo.[Order]'),N'ID','IsIdentity')<>1
      OR CONVERT(bigint,IDENT_INCR(N'dbo.Order'))<>1
      OR COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')<>0
      OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.[Order]') AND is_computed=1)
        THROW 53607,'Unexpected header/history identity or computed column.',1;
    IF EXISTS(SELECT 1 FROM dbo.[Transaction] WHERE RecallID IN(18905,18906,18907,18908,18909,18910,18911,18912))
      OR EXISTS(SELECT 1 FROM dbo.OrderHistory WHERE OrderID IN(18905,18906,18907,18908,18909,18910,18911,18912))
      OR EXISTS(SELECT 1 FROM dbo.Journal WHERE ReferenceID IN(18905,18906,18907,18908,18909,18910,18911,18912) AND TransactionType=2)
        THROW 53608,'Posted transaction/history/receipt references now exist. This open-order recovery needs re-audit.',1;
    -- These less common references were empty at audit. Reject rather than guess their meaning.
    CREATE TABLE #HORenumberUnsupported(TableName sysname,ColumnName sysname);
    INSERT #HORenumberUnsupported VALUES(N'SOD_WO',N'OrderID'),(N'SI_ItemExpireTemp',N'OrderID'),(N'SI_OrderPrice',N'OrderID'),
      (N'SI_PickListTemp',N'OrderID'),(N'QuoteTenderEntry',N'OrderID'),(N'RMSTMP_Sales',N'RecallID'),
      (N'TransactionHold',N'RecallID'),(N'TransactionHoldEntry',N'RecallID'),(N'SOD_ImportedQuote',N'QuoteID'),
      (N'SOD_ImportedQuote',N'ReferenceID'),(N'InventoryTransferLog',N'ReferenceID'),(N'ItemExpireHistory',N'ReferenceID');
    -- Reject newly introduced names that look like order references, except known unrelated columns.
    IF EXISTS(SELECT 1 FROM sys.tables t JOIN sys.schemas sc ON sc.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id
        WHERE (c.name IN(N'OrderID',N'RecallID',N'QuoteID',N'GroupTo',N'ReferenceID')) AND NOT EXISTS
        (SELECT 1 FROM #HORenumberMapColumns m WHERE sc.name=N'dbo' AND m.TableName=t.name AND m.ColumnName=c.name)
        AND NOT EXISTS(SELECT 1 FROM #HORenumberUnsupported u WHERE sc.name=N'dbo' AND u.TableName=t.name AND u.ColumnName=c.name))
        THROW 53609,'An additional order-reference column needs manual review.',1;
    DECLARE unsupported_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName,ColumnName FROM #HORenumberUnsupported ORDER BY TableName,ColumnName;
    OPEN unsupported_cursor; FETCH NEXT FROM unsupported_cursor INTO @table,@columns;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @sql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@table)+N' WITH(TABLOCKX,HOLDLOCK) WHERE '+QUOTENAME(@columns)+N' IN(18905,18906,18907,18908,18909,18910,18911,18912)) THROW 53610,''An unsupported order reference exists. Stop for review.'',1;';
        EXEC sys.sp_executesql @sql;
        FETCH NEXT FROM unsupported_cursor INTO @table,@columns;
    END;
    CLOSE unsupported_cursor; DEALLOCATE unsupported_cursor;
    -- Do not silently miss alternate numeric spellings used by the legacy int comparison.
    IF EXISTS(SELECT 1 FROM dbo.SOD_Item_Logs
        WHERE [Type] IN('Order Entry Logs','Order Update Logs')
          AND TRY_CAST(ReferenceID AS int) BETWEEN 18905 AND 18912
          AND CONVERT(varbinary(50),ReferenceID) <>
              CONVERT(varbinary(50),CONVERT(varchar(50),TRY_CAST(ReferenceID AS int))))
        THROW 53616,'Noncanonical numeric order-log references need review before renumber/reuse. No history was rewritten.',1;
    -- Capture all affected values, substituting only the requested reference columns.
    -- timestamp/rowversion necessarily regenerates and is excluded from value comparisons.
    DECLARE snapshot_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName,KeyColumns,Predicate FROM #HORenumberScope ORDER BY TableName;
    OPEN snapshot_cursor; FETCH NEXT FROM snapshot_cursor INTO @table,@keys,@predicate;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SELECT @expectedColumns=STUFF((SELECT N','+CASE WHEN m.ColumnName IS NOT NULL THEN
            N'CASE '+QUOTENAME(c.name)+N' WHEN '+CASE WHEN @table=N'SOD_Item_Logs' THEN N'''18912'' THEN ''18905''' ELSE N'18912 THEN 18905' END+N' ELSE '+QUOTENAME(c.name)+N' END'
            WHEN c.system_type_id=35 OR c.system_type_id=99 THEN N'CONVERT(nvarchar(max),'+QUOTENAME(c.name)+N')'
            WHEN c.system_type_id=34 THEN N'CONVERT(varbinary(max),'+QUOTENAME(c.name)+N')'
            ELSE QUOTENAME(c.name) END+N' AS '+QUOTENAME(c.name)
            FROM sys.columns c LEFT JOIN #HORenumberMapColumns m ON m.TableName=@table AND m.ColumnName=c.name
            WHERE c.object_id=OBJECT_ID(N'dbo.'+QUOTENAME(@table)) AND c.system_type_id<>189 ORDER BY c.column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
        -- A previously orphaned destination reference must not be silently adopted.
        SELECT @sets=STUFF((SELECT N' OR '+QUOTENAME(ColumnName)+CASE WHEN @table=N'SOD_Item_Logs' THEN N' IN(''18905'',''18906'',''18907'',''18908'',''18909'',''18910'',''18911'')' ELSE N' IN(18905,18906,18907,18908,18909,18910,18911)' END FROM #HORenumberMapColumns WHERE TableName=@table FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,4,N'');
        SET @sql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@table)+N' WHERE ('+@predicate+N') AND ('+@sets+N')) THROW 53611,''Destination has orphaned references; stop for review.'',1;
            SELECT @xml=CONVERT(nvarchar(max),(SELECT '+@expectedColumns+N' FROM dbo.'+QUOTENAME(@table)+N' WHERE '+@predicate+N' ORDER BY '+@keys+N' FOR XML PATH(''Row''),BINARY BASE64,ELEMENTS XSINIL,ROOT(''Rows'')));
            SELECT @rows=COUNT_BIG(*) FROM dbo.'+QUOTENAME(@table)+N' WHERE '+@predicate+N';';
        EXEC sys.sp_executesql @sql,N'@xml nvarchar(max) OUTPUT,@rows bigint OUTPUT',@before OUTPUT,@rows OUTPUT;
        INSERT #HORenumberSnapshots VALUES(@table,@before,@rows);
        FETCH NEXT FROM snapshot_cursor INTO @table,@keys,@predicate;
    END;
    CLOSE snapshot_cursor; DEALLOCATE snapshot_cursor;
    SELECT @@SERVERNAME TargetServer,DB_NAME() TargetDatabase,@Commit CommitRequested,@identityBefore IdentityBefore;
    SELECT 18912 AS OldOrderID,18905 AS NewOrderID,18913 AS NextAllocatedOrderIDAfterCommit;
    SELECT TableName,RowsBefore FROM #HORenumberSnapshots ORDER BY TableName;
    -- Suppress side effects for this maintenance only, under transaction-owned table locks.
    -- Rollback restores trigger states if ANY subsequent operation fails.
    DECLARE disable_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM #HORenumberScope ORDER BY TableName;
    OPEN disable_cursor; FETCH NEXT FROM disable_cursor INTO @table;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @sql=N'DISABLE TRIGGER ALL ON dbo.'+QUOTENAME(@table)+N';'; EXEC sys.sp_executesql @sql;
        FETCH NEXT FROM disable_cursor INTO @table;
    END;
    CLOSE disable_cursor; DEALLOCATE disable_cursor;
    SELECT o.* INTO #HORenumberHeaders FROM dbo.[Order] o JOIN (VALUES(18912)) m(ID) ON o.ID=m.ID;
    DELETE dbo.[Order] WHERE ID=18912;
    SELECT @columns=STUFF((SELECT N','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.[Order]') AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
    SET @expectedColumns=REPLACE(@columns,N'[ID]',N'CASE [ID] WHEN 18912 THEN 18905 END');
    SET IDENTITY_INSERT dbo.[Order] ON;
    SET @sql=N'INSERT dbo.[Order]('+@columns+N') SELECT '+@expectedColumns+N' FROM #HORenumberHeaders;';
    EXEC sys.sp_executesql @sql;
    SET IDENTITY_INSERT dbo.[Order] OFF;
    DECLARE update_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName,Predicate FROM #HORenumberScope WHERE TableName<>N'Order' ORDER BY TableName;
    OPEN update_cursor; FETCH NEXT FROM update_cursor INTO @table,@predicate;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SELECT @sets=STUFF((SELECT N','+QUOTENAME(ColumnName)+N'=CASE '+QUOTENAME(ColumnName)+N' WHEN '+CASE WHEN @table=N'SOD_Item_Logs' THEN N'''18912'' THEN ''18905''' ELSE N'18912 THEN 18905' END+N' ELSE '+QUOTENAME(ColumnName)+N' END' FROM #HORenumberMapColumns WHERE TableName=@table FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
        SET @sql=N'UPDATE dbo.'+QUOTENAME(@table)+N' SET '+@sets+N' WHERE '+@predicate+N';';
        EXEC sys.sp_executesql @sql;
        FETCH NEXT FROM update_cursor INTO @table,@predicate;
    END;
    CLOSE update_cursor; DEALLOCATE update_cursor;
    DECLARE verify_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT s.TableName,s.KeyColumns,s.Predicate,b.ExpectedXml FROM #HORenumberScope s JOIN #HORenumberSnapshots b ON s.TableName=b.TableName ORDER BY s.TableName;
    OPEN verify_cursor; FETCH NEXT FROM verify_cursor INTO @table,@keys,@predicate,@before;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SELECT @columns=STUFF((SELECT N','+CASE WHEN system_type_id IN(35,99) THEN N'CONVERT(nvarchar(max),'+QUOTENAME(name)+N')' WHEN system_type_id=34 THEN N'CONVERT(varbinary(max),'+QUOTENAME(name)+N')' ELSE QUOTENAME(name) END+N' AS '+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.'+QUOTENAME(@table)) AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
        SET @sql=N'SELECT @xml=CONVERT(nvarchar(max),(SELECT '+@columns+N' FROM dbo.'+QUOTENAME(@table)+N' WHERE '+@predicate+N' ORDER BY '+@keys+N' FOR XML PATH(''Row''),BINARY BASE64,ELEMENTS XSINIL,ROOT(''Rows'')));';
        EXEC sys.sp_executesql @sql,N'@xml nvarchar(max) OUTPUT',@after OUTPUT;
        IF CONVERT(varbinary(max),@before)<>CONVERT(varbinary(max),@after)
            THROW 53612,'Post-change values differ beyond the intended ID mapping. Everything will roll back.',1;
        FETCH NEXT FROM verify_cursor INTO @table,@keys,@predicate,@before;
    END;
    CLOSE verify_cursor; DEALLOCATE verify_cursor;
    IF IDENT_CURRENT(N'dbo.Order')<>@identityBefore THROW 53613,'Identity unexpectedly changed. Recovery rolls back; no reseed is authorized.',1;
    DECLARE enable_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM #HORenumberScope ORDER BY TableName;
    OPEN enable_cursor; FETCH NEXT FROM enable_cursor INTO @table;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @sql=N'ENABLE TRIGGER ALL ON dbo.'+QUOTENAME(@table)+N';'; EXEC sys.sp_executesql @sql;
        FETCH NEXT FROM enable_cursor INTO @table;
    END;
    CLOSE enable_cursor; DEALLOCATE enable_cursor;
    SELECT ID,StoreID,Closed,[Time],Total,ReferenceNumber FROM dbo.[Order] WHERE ID IN(18905,18906,18907,18908,18909,18910,18911,18912) ORDER BY ID;
    IF @Commit=1
    BEGIN
        INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details)
        VALUES(18905,N'Order ID changed',ORIGINAL_LOGIN(),LEFT(HOST_NAME(),256),
            N'Manual renumber: old Order ID 18912 -> 18905. Order identity left at 18912; no reseed. No quantity, amount or inventory change. Original text retained.');
        IF (SELECT MAX(ID) FROM dbo.[Order])<>18905 OR IDENT_CURRENT(N'dbo.Order')<>@identityBefore
            THROW 53614,'Final identity/max guard failed. Recovery rolls back.',1;
        COMMIT;
        SELECT N'COMMITTED: WO 18912 -> 18905; identity unchanged at 18912; next normal allocation 18913 if no intervening allocation.' AS Result;
    END
    ELSE
    BEGIN
        ROLLBACK;
        SELECT N'REHEARSAL ONLY: ID/link/trigger changes rolled back; identity unchanged at 18912. Review results before Commit=1.' Result;
    END;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    SET IDENTITY_INSERT dbo.[Order] OFF;
    THROW;
END CATCH;
