/* CONSIGNMENT_DB preflight for Work Order 1029 -> 30.
   Run the entire file in SSMS with CONSIGNMENT_DB selected on the branch server.
   Reads branch data and metadata. The item-log count can scan a large table. */
SET NOCOUNT ON;

IF DB_NAME() <> N'CONSIGNMENT_DB'
    THROW 54001, 'Select CONSIGNMENT_DB in SSMS before running this preflight.', 1;

DECLARE @OldID int = 1029, @NewID int = 30;

DECLARE @References table
(
    TableName sysname NOT NULL,
    ColumnName sysname NOT NULL,
    DomainFilter nvarchar(200) NOT NULL DEFAULT N'',
    SourceRows bigint NULL,
    DestinationRows bigint NULL,
    ReusedRangeRows bigint NULL,
    FirstReusedID bigint NULL,
    PRIMARY KEY (TableName, ColumnName)
);

INSERT @References (TableName, ColumnName) VALUES
    (N'Order', N'ID'), (N'OrderEntry', N'OrderID'),
    (N'Queueing', N'OrderID'), (N'Queueing', N'GroupTo'),
    (N'Queueing_Quote', N'OrderID'), (N'Queueing_Quote', N'GroupTo'),
    (N'SOD_WO_LOGS', N'OrderId'), (N'SOD_WO_History', N'OrderID'),
    (N'OrderHistory', N'OrderID'), (N'Transaction', N'RecallID'),
    (N'SOD_WO', N'OrderID'), (N'SOD_WO_TIO', N'OrderID'),
    (N'SI_ItemExpireTemp', N'OrderID'),
    (N'SI_OrderPrice', N'OrderID'), (N'SI_PickListTemp', N'OrderID'),
    (N'QuoteTenderEntry', N'OrderID'), (N'RMSTMP_Sales', N'RecallID'),
    (N'TransactionHold', N'RecallID'), (N'TransactionHoldEntry', N'RecallID'),
    (N'SOD_ImportedQuote', N'QuoteID'), (N'SOD_ImportedQuote', N'ReferenceID');

INSERT @References (TableName, ColumnName, DomainFilter) VALUES
    (N'Journal', N'ReferenceID', N' AND TransactionType = 2'),
    (N'SOD_Item_Logs', N'ReferenceID',
     N' AND [Type] IN (''Order Entry Logs'', ''Order Update Logs'')');

DECLARE @TableName sysname, @ColumnName sysname, @Filter nvarchar(200),
        @QualifiedName nvarchar(260), @Value nvarchar(300), @Sql nvarchar(max),
        @SourceRows bigint, @DestinationRows bigint, @ReusedRangeRows bigint, @FirstReusedID bigint;

DECLARE reference_counts CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName, ColumnName, DomainFilter FROM @References;
OPEN reference_counts;
FETCH NEXT FROM reference_counts INTO @TableName, @ColumnName, @Filter;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @QualifiedName = N'dbo.' + QUOTENAME(@TableName);
    IF OBJECT_ID(@QualifiedName, N'U') IS NOT NULL
       AND COL_LENGTH(@QualifiedName, @ColumnName) IS NOT NULL
    BEGIN
        SET @Value = QUOTENAME(@ColumnName);
        IF @ColumnName = N'ReferenceID' AND @TableName <> N'Journal'
            SET @Value = N'TRY_CAST(' + @Value + N' AS bigint)';

        SET @Sql = N'SELECT @SourceRows = COALESCE(SUM(CONVERT(bigint, CASE WHEN ' + @Value + N' = @OldID THEN 1 ELSE 0 END)), 0),'
                 + N' @DestinationRows = COALESCE(SUM(CONVERT(bigint, CASE WHEN ' + @Value + N' = @NewID THEN 1 ELSE 0 END)), 0)'
                 + CASE WHEN @TableName = N'SOD_ImportedQuote' AND @ColumnName = N'ReferenceID' THEN N''
                        ELSE N', @ReusedRangeRows = COALESCE(SUM(CONVERT(bigint, CASE WHEN ' + @Value
                             + N' BETWEEN @NewID + 1 AND @OldID - 1 THEN 1 ELSE 0 END)), 0)' END
                 + N' FROM ' + @QualifiedName
                 + N' WHERE ' + @Value + N' BETWEEN @NewID AND @OldID' + @Filter;
        SET @ReusedRangeRows = NULL;
        SET @FirstReusedID = NULL;
        EXEC sys.sp_executesql @Sql,
            N'@OldID int, @NewID int, @SourceRows bigint OUTPUT, @DestinationRows bigint OUTPUT, @ReusedRangeRows bigint OUTPUT',
            @OldID, @NewID, @SourceRows OUTPUT, @DestinationRows OUTPUT, @ReusedRangeRows OUTPUT;
        IF @ReusedRangeRows > 0
        BEGIN
            SET @Sql = N'SELECT @FirstReusedID = MIN(' + @Value + N') FROM ' + @QualifiedName
                     + N' WHERE ' + @Value + N' BETWEEN @NewID + 1 AND @OldID - 1' + @Filter;
            EXEC sys.sp_executesql @Sql,
                N'@OldID int, @NewID int, @FirstReusedID bigint OUTPUT', @OldID, @NewID, @FirstReusedID OUTPUT;
        END;

        UPDATE @References
        SET SourceRows = @SourceRows, DestinationRows = @DestinationRows,
            ReusedRangeRows = @ReusedRangeRows, FirstReusedID = @FirstReusedID
        WHERE TableName = @TableName AND ColumnName = @ColumnName;
    END;
    FETCH NEXT FROM reference_counts INTO @TableName, @ColumnName, @Filter;
END;
CLOSE reference_counts;
DEALLOCATE reference_counts;

SELECT TableName, ColumnName, ReusedRangeRows, FirstReusedID
FROM @References
WHERE ReusedRangeRows > 0
ORDER BY FirstReusedID, TableName, ColumnName;

SELECT CONVERT(sysname, SERVERPROPERTY('ServerName')) AS ServerName,
       DB_NAME() AS DatabaseName, d.compatibility_level,
       IDENT_CURRENT(N'dbo.[Order]') AS CurrentOrderIdentity,
       IDENT_INCR(N'dbo.[Order]') AS OrderIdentityIncrement,
       (SELECT MAX(ID) FROM dbo.[Order]) AS HighestOrderID
FROM sys.databases AS d
WHERE d.database_id = DB_ID();

SELECT ID, StoreID, [Type], Closed, [Time], LastUpdated
FROM dbo.[Order]
WHERE ID IN (@OldID, @NewID)
ORDER BY ID;

-- NULL counts indicate an absent or inaccessible table or column.
-- Imported-quote ReferenceID is the source document's number, outside the reused local ID range.
SELECT TableName, ColumnName, SourceRows, DestinationRows, ReusedRangeRows, FirstReusedID
FROM @References
ORDER BY TableName, ColumnName;

SELECT OBJECT_SCHEMA_NAME(t.object_id) AS SchemaName, t.name AS TableName,
       c.name AS ColumnName, TYPE_NAME(c.user_type_id) AS ColumnType
FROM sys.tables AS t
JOIN sys.columns AS c ON c.object_id = t.object_id
WHERE c.name IN (N'OrderID', N'RecallID', N'QuoteID', N'GroupTo', N'ReferenceID', N'WorkOrderID', N'WOID')
  AND NOT EXISTS (SELECT 1 FROM @References AS r
                  WHERE t.schema_id = SCHEMA_ID(N'dbo') AND r.TableName = t.name AND r.ColumnName = c.name)
ORDER BY SchemaName, TableName, ColumnName;

SELECT OBJECT_SCHEMA_NAME(tr.parent_id) AS SchemaName, OBJECT_NAME(tr.parent_id) AS TableName,
       tr.name AS TriggerName, tr.is_disabled,
       CONVERT(varchar(64), HASHBYTES('SHA2_256', sm.definition), 2) AS DefinitionSHA256
FROM sys.triggers AS tr
LEFT JOIN sys.sql_modules AS sm ON sm.object_id = tr.object_id
WHERE tr.parent_class = 1
  AND EXISTS (SELECT 1 FROM @References AS r
              WHERE tr.parent_id = OBJECT_ID(N'dbo.' + QUOTENAME(r.TableName), N'U'))
ORDER BY SchemaName, TableName, TriggerName;

SELECT fk.name AS ForeignKeyName,
       OBJECT_SCHEMA_NAME(fk.parent_object_id) AS ReferencingSchema,
       OBJECT_NAME(fk.parent_object_id) AS ReferencingTable,
       OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS ReferencedSchema,
       OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
       fk.is_disabled, fk.update_referential_action_desc, fk.delete_referential_action_desc
FROM sys.foreign_keys AS fk
WHERE EXISTS (SELECT 1 FROM @References AS r
              WHERE OBJECT_ID(N'dbo.' + QUOTENAME(r.TableName), N'U')
                    IN (fk.parent_object_id, fk.referenced_object_id))
ORDER BY ReferencingSchema, ReferencingTable, ForeignKeyName;
