/* TEST RESTORE EXAMPLE. Run example 02 first.
   Adds a non-unique index to the demonstration table, not RMS business tables. */
IF OBJECT_ID(N'dbo.SOD_UpdaterExample', N'U') IS NULL OR
   COL_LENGTH(N'dbo.SOD_UpdaterExample', N'UpdatedAtUtc') IS NULL
    THROW 52314, 'Run example 02 to create the demonstration table and column first.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SOD_UpdaterExample')
               AND name=N'IX_SOD_UpdaterExample_UpdatedAtUtc')
    CREATE NONCLUSTERED INDEX IX_SOD_UpdaterExample_UpdatedAtUtc
        ON dbo.SOD_UpdaterExample(UpdatedAtUtc);
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes ix
    JOIN sys.index_columns ic ON ic.object_id=ix.object_id AND ic.index_id=ix.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE ix.object_id=OBJECT_ID(N'dbo.SOD_UpdaterExample')
      AND ix.name=N'IX_SOD_UpdaterExample_UpdatedAtUtc'
      AND ix.type=2 AND ix.is_unique=0 AND ix.is_disabled=0 AND ix.has_filter=0
      AND ic.key_ordinal=1 AND c.name=N'UpdatedAtUtc'
      AND (SELECT COUNT(*) FROM sys.index_columns keys
           WHERE keys.object_id=ix.object_id AND keys.index_id=ix.index_id AND keys.key_ordinal>0)=1)
    THROW 52315, 'Example index exists with an incompatible definition.', 1;
