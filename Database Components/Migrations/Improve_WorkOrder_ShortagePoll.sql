/* Optional index for the Work Order "warehouse shortage" background poll.
   The app polls every 15 seconds for QueueingItems.Status = 'Shortage'. Without an
   index on Status, every poll scans the whole queue history and can hit the poll's
   timeout on large databases (HO). This index lets the poll seek straight to the
   few shortage rows.
   SQL Server 2016 compatible. Run in the intended store database after testing
   on a disposable restore. Schedule a quiet period: index creation takes locks.
   Non-unique and unfiltered (filtered indexes would require strict SET options on
   every writer, including RMS triggers). Changes no data or business rule.
   Does not replace or drop existing indexes.
   Rollback: DROP INDEX IX_SOD_WO_QueueingItems_Status ON dbo.QueueingItems;
   (only if this script created it).
*/
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.QueueingItems', N'U') IS NULL OR
   COL_LENGTH(N'dbo.QueueingItems', N'Status') IS NULL OR
   COL_LENGTH(N'dbo.QueueingItems', N'QueueingID') IS NULL OR
   COL_LENGTH(N'dbo.QueueingItems', N'ItemID') IS NULL
    THROW 52130, 'Required QueueingItems columns are missing.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes ix
        JOIN sys.index_columns leading ON leading.object_id=ix.object_id AND leading.index_id=ix.index_id
        JOIN sys.columns col ON col.object_id=leading.object_id AND col.column_id=leading.column_id
        WHERE ix.object_id=OBJECT_ID(N'dbo.QueueingItems') AND ix.type IN (1,2)
          AND ix.is_disabled=0 AND ix.is_hypothetical=0 AND ix.has_filter=0
          AND leading.key_ordinal=1 AND col.name=N'Status')
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.QueueingItems') AND name=N'IX_SOD_WO_QueueingItems_Status')
            THROW 52131, 'IX_SOD_WO_QueueingItems_Status already exists with another definition. Review it before deployment.', 1;
        CREATE NONCLUSTERED INDEX IX_SOD_WO_QueueingItems_Status
            ON dbo.QueueingItems(Status) INCLUDE (QueueingID, ItemID, QtyPre, Picker);
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
