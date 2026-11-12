/* Optional Portable schema alignment. The updater owns the transaction.
   Adds missing approval-password, username and customer-type fields; existing settings are preserved.
   No passwords/usernames are populated and no existing field is altered. */
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb')
    THROW 52330, 'System databases are not supported.', 1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1
    THROW 52331, 'Run through Database Updater in its active transaction.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_Conf',N'U') IS NULL
    THROW 52332, 'Existing dbo.SOD_WO_Conf table is required. No configuration table was created.', 1;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf')
      AND name IN (N'Password2',N'Password3',N'Pass2Username',N'Pass3Username')
      AND (system_type_id<>231 OR max_length<>100 OR is_computed=1 OR is_nullable=0))
    THROW 52333, 'An existing approval field has an incompatible definition. Review before updating.', 1;

IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Password2') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Password2 nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Password3') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Password3 nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass2Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass2Username nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass3Username') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD Pass3Username nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.SOD_WO_Conf',N'CustType') IS NULL
    ALTER TABLE dbo.SOD_WO_Conf ADD CustType nvarchar(max) NULL;

