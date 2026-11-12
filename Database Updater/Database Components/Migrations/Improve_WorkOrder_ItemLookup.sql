/* Optional RMS lookup index for Work Order recall and item-tax reads.
   SQL Server 2016 compatible. Run in the intended store database after testing
   on a disposable restore. Schedule a quiet period: index creation takes locks.
   Non-unique; changes no item, order, price, quantity, or business rule.
   Does not replace or drop existing indexes.
*/
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.Item', N'U') IS NULL OR
   COL_LENGTH(N'dbo.Item', N'ItemLookupCode') IS NULL OR
   COL_LENGTH(N'dbo.Item', N'ID') IS NULL OR
   COL_LENGTH(N'dbo.Item', N'Taxable') IS NULL
    THROW 52120, 'Required RMS Item columns are missing.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes ix
        JOIN sys.index_columns leading ON leading.object_id=ix.object_id AND leading.index_id=ix.index_id
        JOIN sys.columns code ON code.object_id=leading.object_id AND code.column_id=leading.column_id
        WHERE ix.object_id=OBJECT_ID(N'dbo.Item') AND ix.type IN (1,2)
          AND ix.is_disabled=0 AND ix.is_hypothetical=0 AND ix.has_filter=0
          AND leading.key_ordinal=1 AND code.name=N'ItemLookupCode'
          AND (ix.type=1 OR EXISTS (
              SELECT 1 FROM sys.index_columns covered
              JOIN sys.columns col ON col.object_id=covered.object_id AND col.column_id=covered.column_id
              WHERE covered.object_id=ix.object_id AND covered.index_id=ix.index_id AND col.name=N'Taxable'))
          AND (ix.type=1 OR EXISTS (
              SELECT 1 FROM sys.index_columns covered
              JOIN sys.columns col ON col.object_id=covered.object_id AND col.column_id=covered.column_id
              WHERE covered.object_id=ix.object_id AND covered.index_id=ix.index_id AND col.name=N'ID')
              OR EXISTS (
              SELECT 1 FROM sys.indexes clusterix
              JOIN sys.index_columns keycol ON keycol.object_id=clusterix.object_id AND keycol.index_id=clusterix.index_id
              JOIN sys.columns col ON col.object_id=keycol.object_id AND col.column_id=keycol.column_id
              WHERE clusterix.object_id=ix.object_id AND clusterix.type=1 AND keycol.key_ordinal>0 AND col.name=N'ID')))
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Item') AND name=N'IX_SOD_WO_ItemLookup')
            THROW 52121, 'IX_SOD_WO_ItemLookup already exists with another definition. Review it before deployment.', 1;
        CREATE NONCLUSTERED INDEX IX_SOD_WO_ItemLookup
            ON dbo.Item(ItemLookupCode) INCLUDE (ID, Taxable);
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
