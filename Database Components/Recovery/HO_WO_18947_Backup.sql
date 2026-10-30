/* Required recovery backup for the authorized WO 18947 -> 18946 mapping.
   Creates a unique file; does not change business data or the differential base. */
USE [HO_PO_DB];
SET NOCOUNT ON;
IF DB_NAME()<>N'HO_PO_DB' OR @@SERVERNAME<>N'WIN-2847UGJBAV4'
    THROW 53901,'Wrong recovery backup target.',1;
DECLARE @BackupPath nvarchar(4000)=N'F:\AUTO_RMS_BACKUP_DB\HO_PO_DB\HO_PO_DB_WO18947_'
    +CONVERT(nvarchar(8),GETDATE(),112)+N'_'+REPLACE(CONVERT(nvarchar(36),NEWID()),N'-',N'')+N'.bak';
SELECT @BackupPath AS RecoveryBackupPath;
BACKUP DATABASE [HO_PO_DB] TO DISK=@BackupPath
WITH COPY_ONLY,CHECKSUM,COMPRESSION,STATS=10;
RESTORE VERIFYONLY FROM DISK=@BackupPath WITH CHECKSUM;
SELECT b.backup_set_id,b.backup_finish_date,b.is_copy_only,b.has_backup_checksums,
       b.backup_size,b.compressed_backup_size,m.physical_device_name
FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id=b.media_set_id
WHERE b.database_name=N'HO_PO_DB' AND m.physical_device_name=@BackupPath;
