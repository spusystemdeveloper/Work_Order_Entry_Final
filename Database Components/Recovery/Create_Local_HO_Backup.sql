/* Run personally in SSMS connected to localhost on MSI.
   Creates and verifies a local .bak file only; no recovery/repair is executed.
   Uses a unique filename, COPY_ONLY and NO_CHECKSUM. No existing file is replaced.
   Copy the returned RecoveryScriptLine into HO_PO_DB_23266_LOCAL_TEST.sql. */
USE [HO_PO_DB];
SET NOCOUNT ON;
IF ISNULL(UPPER(CONVERT(sysname,SERVERPROPERTY('MachineName'))),N'')<>N'MSI'
    THROW 52710, 'This backup helper is only for the local MSI test computer.', 1;
IF @@TRANCOUNT<>0 THROW 52711, 'Run the backup helper outside an existing transaction.', 1;
DECLARE @BackupFolder nvarchar(4000)=N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\Backup';
IF RIGHT(@BackupFolder,1)<>N'\' SET @BackupFolder=@BackupFolder+N'\';
DECLARE @BackupPath nvarchar(4000)=@BackupFolder+N'HO_PO_DB_LocalTest_'+REPLACE(CONVERT(nvarchar(36),NEWID()),N'-',N'')+N'.bak';
BEGIN TRY
    BACKUP DATABASE [HO_PO_DB] TO DISK=@BackupPath WITH COPY_ONLY,NO_CHECKSUM,STATS=10;
    RESTORE VERIFYONLY FROM DISK=@BackupPath;
    SELECT @BackupPath BackupPath,
        N'DECLARE @BackupPath nvarchar(4000)=N'''+REPLACE(@BackupPath,N'''',N'''''')+N''';' RecoveryScriptLine,
        N'Local backup created and verified. No repair executed.' Result;
END TRY
BEGIN CATCH
    THROW;
END CATCH;
