/* TEST RESTORE EXAMPLE. Creates a custom demonstration table and adds a column.
   No RMS business tables or data are edited. Run before example 03. */
IF OBJECT_ID(N'dbo.SOD_UpdaterExample') IS NOT NULL AND
   OBJECT_ID(N'dbo.SOD_UpdaterExample', N'U') IS NULL
    THROW 52312, 'SOD_UpdaterExample exists with an incompatible object type.', 1;
IF OBJECT_ID(N'dbo.SOD_UpdaterExample', N'U') IS NULL
    CREATE TABLE dbo.SOD_UpdaterExample (
        ID int NOT NULL CONSTRAINT PK_SOD_UpdaterExample PRIMARY KEY,
        ExampleNote nvarchar(50) NULL
    );
IF COL_LENGTH(N'dbo.SOD_UpdaterExample', N'UpdatedAtUtc') IS NULL
    ALTER TABLE dbo.SOD_UpdaterExample ADD UpdatedAtUtc datetime2(3) NULL;
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id=OBJECT_ID(N'dbo.SOD_UpdaterExample') AND name=N'UpdatedAtUtc'
      AND TYPE_NAME(system_type_id)=N'datetime2' AND scale=3 AND is_nullable=1)
    THROW 52313, 'Example UpdatedAtUtc column has an incompatible definition.', 1;
