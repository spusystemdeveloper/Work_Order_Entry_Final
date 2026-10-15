/* HO_PO_DB: new Work Order / imported-order Save timeout, October 15, 2026.
   Execute personally in SSMS on WIN-2847UGJBAV4 / 172.16.4.50.
   Default @Commit=0 rehearses ALTER and rolls it back. Review the results and
   save OriginalTriggerRestoreSql before setting @Commit=1 and running again.
   Only the fingerprinted legacy logging trigger is supported. No log rows,
   indexes, history numbering, RMS data or application timeouts are changed.
   This is a standalone SSMS script, not an updater-importable migration. */
USE [HO_PO_DB];
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Commit bit = 0;
DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';
IF @@TRANCOUNT<>0 THROW 53200, 'Run this complete script outside an existing transaction.', 1;
IF DB_NAME()<>N'HO_PO_DB' THROW 53201, 'This fix is only for the diagnosed HO_PO_DB.', 1;
IF CONVERT(sysname,@@SERVERNAME)<>@ExpectedServer
    THROW 53202, 'Wrong SQL server. Review the intended target before applying this HO fix.', 1;

DECLARE @LegacyHash varbinary(32)=0x90699E994049AD7B720D80E1E3149AE64ABCDEE66D5908FCBDFCD1F4316EB3FE;
DECLARE @FixedHash varbinary(32)=0x5CFB6436B9EAB3B846B8DFAB5AB9AAB52215515CB88C4BB44A70D10234058A8B;
DECLARE @Lookup nvarchar(max)=N'set @lastcommitted = (select top 1 NewCommitted  from SOD_Item_Logs where ItemID = @Itemid and ReferenceID = @Orderid order by date desc )';
DECLARE @GuardedLookup nvarchar(max)=N'IF @Qty <> 0 AND @exist > 0'+NCHAR(13)+NCHAR(10)
    +N'    BEGIN'+NCHAR(13)+NCHAR(10)+N'        '+@Lookup+NCHAR(13)+NCHAR(10)+N'    END';

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @LockResult int;
    EXEC @LockResult=sys.sp_getapplock @Resource=N'HO_OrderEntry_ItemLogs_SaveTimeoutFix',
        @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000;
    IF @LockResult<0 THROW 53203, 'Another timeout fix is running. No changes applied.', 1;
    DECLARE @TriggerID int=OBJECT_ID(N'dbo.trg_OrderEntry_ItemLogs',N'TR');
    IF @TriggerID IS NULL OR NOT EXISTS(SELECT 1 FROM sys.triggers WHERE object_id=@TriggerID
        AND parent_id=OBJECT_ID(N'dbo.OrderEntry') AND is_disabled=0)
        THROW 53204, 'Expected enabled OrderEntry logging trigger is missing or disabled. Review it first.', 1;
    IF NOT EXISTS(SELECT 1 FROM sys.sql_modules WHERE object_id=@TriggerID
        AND uses_ansi_nulls=1 AND uses_quoted_identifier=1)
        THROW 53205, 'Trigger SET options differ from the reviewed definition.', 1;
    DECLARE @Before nvarchar(max)=OBJECT_DEFINITION(@TriggerID);
    IF HASHBYTES('SHA2_256',@Before)=@FixedHash
    BEGIN
        COMMIT TRANSACTION;
        SELECT N'ALREADY APPLIED: trigger matches the reviewed timeout fix.' Result;
        RETURN;
    END;
    IF @Before IS NULL OR HASHBYTES('SHA2_256',@Before)<>@LegacyHash
        THROW 53206, 'Trigger differs from the diagnosed legacy version. No replacement applied; review custom changes.', 1;
    DECLARE @CreateAt int=CHARINDEX(N'CREATE TRIGGER',@Before);
    IF @CreateAt=0 OR CHARINDEX(@Lookup,@Before)=0
        THROW 53207, 'Expected trigger declaration or lookup was not found.', 1;
    DECLARE @Restore nvarchar(max)=STUFF(@Before,@CreateAt,6,N'ALTER');
    DECLARE @After nvarchar(max)=STUFF(REPLACE(@Before,@Lookup,@GuardedLookup),@CreateAt,6,N'ALTER');
    -- SQL Server stores the module definition with CREATE after ALTER.
    IF HASHBYTES('SHA2_256',STUFF(@After,@CreateAt,5,N'CREATE'))<>@FixedHash
        THROW 53208, 'Proposed trigger does not match the reviewed fix.', 1;
    SELECT @@SERVERNAME TargetServer,DB_NAME() TargetDatabase,@Commit CommitRequested,
        @Restore OriginalTriggerRestoreSql;
    EXEC sys.sp_executesql @After;
    IF HASHBYTES('SHA2_256',OBJECT_DEFINITION(@TriggerID))<>@FixedHash
    BEGIN
        DECLARE @VerificationError nvarchar(2048)=N'Trigger verification failed; roll back. Actual: '
            +CONVERT(nvarchar(64),HASHBYTES('SHA2_256',OBJECT_DEFINITION(@TriggerID)),2);
        THROW 53209, @VerificationError, 1;
    END;
    IF @Commit=1
    BEGIN
        COMMIT TRANSACTION;
        SELECT N'COMMITTED: new order-line inserts skip the unused previous-log lookup; logging remains enabled.' Result;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT N'REHEARSAL PASSED: trigger ALTER rolled back. Save the restore definition; set Commit=1 only after review.' Result;
    END;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
