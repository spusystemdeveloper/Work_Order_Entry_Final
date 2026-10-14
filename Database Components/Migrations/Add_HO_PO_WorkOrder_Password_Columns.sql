/* Work Order password-slot column alignment for HO_PO_DB, SQL Server 2016.
   Import through Work Order Database Updater after restore testing and backup.
   Adds only missing nullable nvarchar(50) columns to the existing configuration
   table. Preserves every existing row/value and existing column definition.
   Does not set passwords, populate usernames, or deploy GSC functions/triggers.
   The updater owns the transaction; do not add transaction commands here. */
SET XACT_ABORT ON;

IF DB_NAME() <> N'HO_PO_DB'
    THROW 52320, 'This migration targets HO_PO_DB only. Check the selected database.', 1;
IF @@TRANCOUNT = 0 OR XACT_STATE() <> 1
    THROW 52321, 'Run this migration through the updater in its active transaction.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_Conf', N'U') IS NULL
    THROW 52322, 'HO_PO_DB requires an existing dbo.SOD_WO_Conf table. No configuration table was created.', 1;

-- Validate existing fields before making additions. Incompatible definitions
-- require review instead of overwriting a column or truncating existing values.
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id=OBJECT_ID(N'dbo.SOD_WO_Conf')
      AND c.name IN (N'Password2',N'Password3',N'Pass2Username',N'Pass3Username')
      AND (c.system_type_id <> 231 OR c.max_length <> 100 OR c.is_computed=1))
    THROW 52323, 'An existing password/username column is not a regular nvarchar(50). Review its definition; existing fields were preserved.', 1;

IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Password2') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Password2 nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Password3') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Password3 nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Pass2Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass2Username nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Pass3Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass3Username nvarchar(50) NULL;

IF (SELECT COUNT(*) FROM sys.columns
    WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf')
      AND name IN (N'Password2',N'Password3',N'Pass2Username',N'Pass3Username')
      AND system_type_id=231 AND max_length=100 AND is_computed=0) <> 4
    THROW 52324, 'Password-slot column verification failed. The updater will roll back this plan.', 1;
