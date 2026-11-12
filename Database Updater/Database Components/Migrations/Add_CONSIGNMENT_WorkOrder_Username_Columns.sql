/* Import using Database Updater > Add SQL script, after backup/restore testing.
   Adds missing approval-username fields only; existing settings are preserved.
   The updater owns the transaction. No passwords/usernames are populated. */
IF DB_NAME() <> N'CONSIGNMENT_DB'
    THROW 52330, 'This migration targets CONSIGNMENT_DB only.', 1;
IF @@TRANCOUNT = 0 OR XACT_STATE() <> 1
    THROW 52331, 'Run through Database Updater in its active transaction.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_Conf', N'U') IS NULL
    THROW 52332, 'Existing dbo.SOD_WO_Conf is required.', 1;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf')
      AND name IN (N'Pass2Username',N'Pass3Username')
      AND (system_type_id<>231 OR max_length<>100 OR is_computed=1 OR is_nullable=0))
    THROW 52333, 'An existing approval-username field has an incompatible definition. Review before updating.', 1;

IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass2Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass2Username nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass3Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass3Username nvarchar(50) NULL;
