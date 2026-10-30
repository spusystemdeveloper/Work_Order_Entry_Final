/* SELECT-only prerequisite audit for the user-authorized WO 18947 -> 18946.
   Run in SSMS on 172.16.4.50 / HO_PO_DB and return all result grids.
   Does NOT renumber, reseed, repair commitments, suspend triggers or create backups.
   This is NOT a Database Updater migration or a repair/rehearsal script. */
USE [HO_PO_DB];
SET NOCOUNT ON;
SET LOCK_TIMEOUT 5000;
IF DB_NAME() <> N'HO_PO_DB' OR @@SERVERNAME <> N'WIN-2847UGJBAV4'
    THROW 53900,'Unexpected HO server/database. Stop for target review.',1;

SELECT @@SERVERNAME AS ServerName,DB_NAME() AS DatabaseName,
       SYSDATETIME() AS ServerTime,IDENT_CURRENT(N'dbo.Order') AS CurrentOrderIdentity;

SELECT ID,StoreID,CustomerID,Closed,[Type],[Time],LastUpdated,Total,Tax,ReferenceNumber
FROM dbo.[Order] WHERE ID BETWEEN 18943 AND 18950 ORDER BY ID;
SELECT MAX(ID) AS HighestOrderID FROM dbo.[Order];

SELECT ID,OrderID,ItemID,QuantityOnOrder,Price,FullPrice
FROM dbo.OrderEntry WHERE OrderID IN(18946,18947) ORDER BY OrderID,ID;

IF OBJECT_ID(N'dbo.Queueing',N'U') IS NOT NULL
    SELECT id,OrderID,GroupTo,Status FROM dbo.Queueing
    WHERE OrderID IN(18946,18947) OR GroupTo IN(18946,18947) ORDER BY id;
IF OBJECT_ID(N'dbo.SOD_WO_LOGS',N'U') IS NOT NULL
    SELECT OrderId,COUNT_BIG(*) AS ApprovalRows FROM dbo.SOD_WO_LOGS
    WHERE OrderId IN(18946,18947) GROUP BY OrderId;
IF OBJECT_ID(N'dbo.SOD_WO_History',N'U') IS NOT NULL
    SELECT OrderID,COUNT_BIG(*) AS HistoryRows FROM dbo.SOD_WO_History
    WHERE OrderID IN(18946,18947) GROUP BY OrderID;

SELECT StoreID,TransactionNumber,RecallID,[Time],Total
FROM dbo.[Transaction] WHERE RecallID IN(18946,18947);
SELECT OrderID,COUNT_BIG(*) AS RmsOrderHistoryRows FROM dbo.OrderHistory
WHERE OrderID IN(18946,18947) GROUP BY OrderID;
SELECT ReferenceID,TransactionType,COUNT_BIG(*) AS JournalRows FROM dbo.Journal
WHERE ReferenceID IN(18946,18947) AND TransactionType=2 GROUP BY ReferenceID,TransactionType;

IF OBJECT_ID(N'dbo.SOD_Item_Logs',N'U') IS NOT NULL
    SELECT [Type],ReferenceID,COUNT_BIG(*) AS OrderItemLogRows
    FROM dbo.SOD_Item_Logs
    WHERE ItemID IN(56485,68066,68069,68072) AND ReferenceID IN('18946','18947')
      AND [Type] IN('Order Entry Logs','Order Update Logs')
    GROUP BY [Type],ReferenceID;

-- The repair additionally audits all ItemID ranges for unexpected/noncanonical log links.
-- Inventory is evidence only. The requested renumber must not update these values.
SELECT i.ID,i.ItemLookupCode,i.ItemType,i.Quantity,i.QuantityCommitted
FROM dbo.Item i
WHERE EXISTS(SELECT 1 FROM dbo.OrderEntry e WHERE e.OrderID IN(18946,18947) AND e.ItemID=i.ID)
ORDER BY i.ID;

-- Include both declared relationships and legacy columns without foreign keys.
SELECT OBJECT_SCHEMA_NAME(c.object_id) AS SchemaName,OBJECT_NAME(c.object_id) AS TableName,
       c.name AS ColumnName,TYPE_NAME(c.user_type_id) AS ColumnType
FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id
WHERE c.name IN(N'OrderID',N'RecallID',N'QuoteID',N'GroupTo',N'ReferenceID')
ORDER BY SchemaName,TableName,ColumnName;
SELECT fk.name AS ForeignKeyName,OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SchemaName,
       OBJECT_NAME(fk.parent_object_id) AS TableName,pc.name AS ColumnName,
       OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,rc.name AS ReferencedColumn
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
WHERE fk.referenced_object_id=OBJECT_ID(N'dbo.Order') OR fk.parent_object_id=OBJECT_ID(N'dbo.Order');

SELECT OBJECT_SCHEMA_NAME(t.parent_id) AS SchemaName,OBJECT_NAME(t.parent_id) AS TableName,
       t.name AS TriggerName,t.is_disabled,
       CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),m.definition)),2) AS DefinitionHash
FROM sys.triggers t LEFT JOIN sys.sql_modules m ON m.object_id=t.object_id
WHERE OBJECT_NAME(t.parent_id) IN(N'Order',N'OrderEntry',N'Queueing',N'SOD_WO_LOGS',N'SOD_WO_History',N'SOD_Item_Logs')
ORDER BY SchemaName,TableName,TriggerName;

-- Metadata only: this does not create/verify a recovery backup.
SELECT TOP (5) b.backup_finish_date,b.is_copy_only,b.has_backup_checksums,
       b.backup_size,m.physical_device_name
FROM msdb.dbo.backupset b
JOIN msdb.dbo.backupmediafamily m ON m.media_set_id=b.media_set_id
WHERE b.database_name=N'HO_PO_DB' AND b.[type]='D'
ORDER BY b.backup_finish_date DESC;
