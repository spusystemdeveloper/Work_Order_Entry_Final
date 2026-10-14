/* Recommended before deploying edit protection to a branch with large queues.
   Adds non-unique lookup indexes only when no usable index has the same
   leading key. It neither changes RMS columns nor enforces new data rules.
   Test on a disposable branch restore; schedule index creation for deployment.
*/
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.OrderEntry', N'U') IS NULL OR
   OBJECT_ID(N'dbo.Queueing', N'U') IS NULL OR OBJECT_ID(N'dbo.QueueingItems', N'U') IS NULL
    THROW 52110, 'Required order and queue tables are missing.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes ix
        JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE ix.object_id = OBJECT_ID(N'dbo.OrderEntry') AND ic.key_ordinal = 1 AND c.name = N'OrderID'
          AND ix.is_disabled = 0 AND ix.is_hypothetical = 0 AND ix.has_filter = 0)
        CREATE INDEX IX_SOD_WO_Edit_OrderEntry ON dbo.OrderEntry(OrderID);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes ix
        JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE ix.object_id = OBJECT_ID(N'dbo.Queueing') AND ic.key_ordinal = 1 AND c.name = N'OrderID'
          AND ix.is_disabled = 0 AND ix.is_hypothetical = 0 AND ix.has_filter = 0)
        CREATE INDEX IX_SOD_WO_Edit_Queueing ON dbo.Queueing(OrderID);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes ix
        JOIN sys.index_columns ic ON ix.object_id = ic.object_id AND ix.index_id = ic.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE ix.object_id = OBJECT_ID(N'dbo.QueueingItems') AND ic.key_ordinal = 1 AND c.name = N'QueueingID'
          AND ix.is_disabled = 0 AND ix.is_hypothetical = 0 AND ix.has_filter = 0)
        CREATE INDEX IX_SOD_WO_Edit_QueueingItems ON dbo.QueueingItems(QueueingID);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
