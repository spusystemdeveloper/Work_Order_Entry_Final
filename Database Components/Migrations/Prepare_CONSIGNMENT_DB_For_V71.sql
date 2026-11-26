/*
 V7.1 compatibility for the reviewed MSI\SQLEXPRESS2017 / CONSIGNMENT_DB restore.
 Run in SSMS, not inside Database Updater (this standalone script owns its transaction).
 DEFAULT IS REVIEW ONLY: inspect results, take a full CHECKSUM backup, then set
 @Apply=1 and enter that backup's server-side path in @BackupFile.
 This adds two optional parameters to each reviewed save procedure and creates
 three missing views. Old 6/10-argument callers retain quotation/no-queue behavior.
 It does not update existing orders/items/users/registers/store labels or printing.
 Repeated application is a no-op; unreviewed definitions stop rather than overwrite.
*/
USE [CONSIGNMENT_DB];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @ExpectedServer sysname=N'MSI\SQLEXPRESS2017';
DECLARE @ExpectedDatabase sysname=N'CONSIGNMENT_DB';
DECLARE @Apply bit=0;
DECLARE @BackupFile nvarchar(4000)=N'';

IF CONVERT(sysname,SERVERPROPERTY('ServerName'))<>@ExpectedServer OR DB_NAME()<>@ExpectedDatabase
    THROW 52420, 'Wrong target. Confirm the expected server and database before running.', 1;
IF @@TRANCOUNT<>0
    THROW 52421, 'Run this standalone script outside an existing transaction, not in Database Updater.', 1;

DECLARE @Plan TABLE(Id int PRIMARY KEY,Name sysname,Kind char(1),LegacyHash varbinary(32),ExpectedHash varbinary(32),DDL nvarchar(max));
INSERT @Plan VALUES(1,N'SOD_sp_InsertQoute','P',0x4c8c89733281dc05331299a020ffd18ee835421dc7fe56d4e2f7b6d7d4bd47e3,0xe11b175f901c597274da0c0ccc2b1b4bc7e96976537c815509e15d3aafa36aad,N'




ALTER PROCEDURE [dbo].[SOD_sp_InsertQoute]
(
	@vComment varchar(255),
	@iCustID int,
	@iSalesRepID int,
	@mTax money,
	@mTotal money,
	@vRemarks varchar(255),
    @OPIS varchar(255) = NULL,
    @Type int = 3
	
)
	
AS
BEGIN

	SET NOCOUNT ON;
    IF @Type IS NULL OR @Type NOT IN (2,3) THROW 52431, ''Order type must be 2 (work order) or 3 (quotation).'', 1;

	DECLARE @iStoreID int,
			@iOrderID BigInt,
			@vRef varchar(30)

	SELECT @iStoreID=StoreID FROM configuration

	SELECT @vRef= NextORNumber FROM  SI_ORSeries WHERE RegisterNumber=@vComment
	
	IF @vRef IS NULL BEGIN
		SET @vRef=0
	END

	IF @vRemarks != '''' BEGIN
		SET @vComment=@vRemarks
	END
		
	INSERT INTO [order] (StoreID,Closed,[Time],[Type],Comment,CustomerID,ShipToID,DepositOverride,Deposit,Tax,Total,LastUpdated,
	ExpirationOrDueDate,Taxable,SalesRepID,ReferenceNumber,ShippingChargeOnOrder,ShippingChargeOverride,ShippingServiceID,
	ShippingTrackingNumber,ShippingNotes,ReasonCodeID,ExchangeID,ChannelType,DefaultDiscountReasonCodeID,DefaultReturnReasonCodeID,DefaultTaxChangeReasonCodeID)
	SELECT @iStoreID,0,GetDate(),@Type,@vComment,@iCustID,0,0,0,@mTax,@mTotal,GetDate(),(CONVERT(VARCHAR(10),GETDATE(),23)),(CASE WHEN @mTax > 0 THEN 1 ELSE 0 END),
	@iSalesRepID,@vRef,0,0,0,'''','''',0,0,0,0,0,0

	SELECT @iOrderID = SCOPE_IDENTITY()
    -- New V7.1 calls supply OPIS; old six-argument calls keep their old behavior.
    IF @OPIS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Queueing WHERE OrderID=@iOrderID)
        INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES(@iOrderID,0,@iOrderID,@OPIS);


	RETURN @iOrderID


END');
INSERT @Plan VALUES(2,N'SOD_sp_InsertQouteEntry','P',0xde54bed74500b82c24314044e90804bf77b7e0f4d15f63a9d7c5888a88a194a6,0xfaa299992bfc2d9467f8bb9f3a3fd93c204c2dd70ae685468e447f32b9d4484b,N'




ALTER PROCEDURE [dbo].[SOD_sp_InsertQouteEntry]
(
	@mCost money,
	@iOrderID BigInt,
	@iItemID int,
	@mFullPrice money,
	@mPrice money,
	@fQuantityOnOrder float,
	@iSalesRepID int,
	@iTaxable int,
	@vDescription varchar(100),
	@vComment varchar(100),
    @QtyPrep int = NULL,
    @Type int = 3
)
	
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @iStoreID int

	SELECT @iStoreID=StoreID FROM configuration

	INSERT INTO orderentry (Cost,StoreID,OrderID,ItemID,FullPrice,PriceSource,Price,QuantityOnOrder,SalesRepID,
							Taxable,DetailID,Description,QuantityRTD,LastUpdated,Comment,DiscountReasonCodeID,
							ReturnReasonCodeID,TaxChangeReasonCodeID)
	SELECT @mCost,@iStoreid,@iOrderID,@iItemID,@mFullPrice,'''',@mPrice,@fQuantityOnOrder,@iSalesRepID,@iTaxable,
			0,@vDescription,0,GetDate(),@vComment,0,0,0


    -- Old ten-argument calls do not create queue processing rows.
    IF @QtyPrep IS NOT NULL
    BEGIN
        DECLARE @QueueID bigint;
        SELECT @QueueID=id FROM dbo.Queueing WHERE OrderID=@iOrderID;
        IF @QueueID IS NULL THROW 52432, ''V7.1 queue header is missing; save must run in the application transaction.'', 1;
        INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
        VALUES(@QueueID,@iItemID,@QtyPrep,N'''',''-'',@vComment);
    END;
END




');
INSERT @Plan VALUES(3,N'SOD_viewTableLastUpdate','V',NULL,0x8eccbeb4610825da2863fd12f80265f663435eb1db20dc89bb55e6402e72c724,N'CREATE VIEW dbo.SOD_viewTableLastUpdate AS
SELECT OBJECT_NAME(object_id) AS TableName, MAX(last_user_update) AS LastUpdate
FROM sys.dm_db_index_usage_stats
WHERE database_id=DB_ID()
AND object_id IN(OBJECT_ID(N''dbo.OrderEntry''),OBJECT_ID(N''dbo.Order''),OBJECT_ID(N''dbo.Queueing''),OBJECT_ID(N''dbo.QueueingItems''),OBJECT_ID(N''dbo.Item''))
GROUP BY OBJECT_NAME(object_id);');
INSERT @Plan VALUES(4,N'SOD_viewLastUpdatedItem','V',NULL,0x97fc1c72754e603a1efbe94ae78d656e39c9dd4a42ad8a764bf9b69d5d023ee3,N'CREATE VIEW dbo.SOD_viewLastUpdatedItem AS
SELECT TOP (20) s.ItemLookupCode,
(SELECT i.Quantity-i.QuantityCommitted FROM dbo.Item i WHERE i.ItemLookupCode=s.ItemLookupCode) AS Available
FROM dbo.SOD_Item_Logs s
WHERE s.[Type]=''Item Auto Logs''
GROUP BY s.ItemLookupCode
ORDER BY MAX(s.[Date]) DESC;');
INSERT @Plan VALUES(5,N'SOD_ViewForInvoice','V',NULL,0x69e9017316b32f2674c44f1b753097b53e0a1e1ca2f7e82eb5e95a9529eb9196,N'CREATE VIEW dbo.SOD_ViewForInvoice AS
SELECT DISTINCT x.GroupTo,
CASE WHEN
 (SELECT COUNT(a.GroupTo) FROM dbo.Queueing a
  INNER JOIN dbo.QueueingItems b ON a.id=b.QueueingID
  INNER JOIN dbo.Item c ON b.ItemID=c.id
  INNER JOIN dbo.[Order] d ON a.OrderID=d.id WHERE a.GroupTo=x.GroupTo)
 =
 (SELECT COUNT(a.GroupTo) FROM dbo.Queueing a
  INNER JOIN dbo.QueueingItems b ON a.id=b.QueueingID
  INNER JOIN dbo.Item c ON b.ItemID=c.id
  INNER JOIN dbo.[Order] d ON a.OrderID=d.id WHERE b.Status=''Prepared'' AND a.GroupTo=x.GroupTo)
THEN ''Prepared'' ELSE ''NotPrepared'' END AS Status,
STUFF((SELECT '', ''+CAST(z.OrderID AS varchar(max)) FROM dbo.Queueing z
       WHERE z.GroupTo=x.GroupTo FOR XML PATH('''')),1,2,'''') AS Orders,
x.OPIS
FROM dbo.Queueing x WHERE x.Status=0;');

-- SQL Server stores the procedure definition using CREATE after ALTER.
-- Expected hashes therefore use that exact canonical keyword; no body text is ignored.
-- Required existing tables/columns. No tables are created or replaced here.
IF OBJECT_ID(N'dbo.Order',N'U') IS NULL OR OBJECT_ID(N'dbo.OrderEntry',N'U') IS NULL
 OR OBJECT_ID(N'dbo.Item',N'U') IS NULL OR OBJECT_ID(N'dbo.Configuration',N'U') IS NULL
 OR OBJECT_ID(N'dbo.SI_ORSeries',N'U') IS NULL OR OBJECT_ID(N'dbo.Queueing',N'U') IS NULL
 OR OBJECT_ID(N'dbo.QueueingItems',N'U') IS NULL OR OBJECT_ID(N'dbo.SOD_Item_Logs',N'U') IS NULL
    THROW 52422, 'Required RMS/queue/log tables are missing. Review schema before applying.', 1;
IF COL_LENGTH(N'dbo.Queueing',N'OPIS') IS NULL OR COL_LENGTH(N'dbo.Queueing',N'GroupTo') IS NULL
 OR COL_LENGTH(N'dbo.QueueingItems',N'QtyPre') IS NULL OR COL_LENGTH(N'dbo.QueueingItems',N'PickLoc') IS NULL
 OR COL_LENGTH(N'dbo.SOD_Item_Logs',N'Date') IS NULL OR COL_LENGTH(N'dbo.Item',N'QuantityCommitted') IS NULL
    THROW 52422, 'Required columns are missing. Existing schema preserved.', 1;

DECLARE @Id int=1,@Name sysname,@Kind char(1),@Legacy varbinary(32),@DDL nvarchar(max),
 @ExpectedHash varbinary(32),@Current varbinary(32),@ObjectID int,@Message nvarchar(2048);
WHILE @Id<=5
BEGIN
 SELECT @Name=Name,@Kind=Kind,@Legacy=LegacyHash,@ExpectedHash=ExpectedHash,@DDL=DDL FROM @Plan WHERE Id=@Id;
 SET @ObjectID=OBJECT_ID(N'dbo.'+@Name);
 SET @Current=HASHBYTES('SHA2_256',OBJECT_DEFINITION(@ObjectID));
 IF @Legacy IS NOT NULL AND @ObjectID IS NULL
     THROW 52423, 'Reviewed legacy procedures are missing; this is not a fresh-install script.', 1;
 IF @ObjectID IS NOT NULL AND
    (NOT EXISTS(SELECT 1 FROM sys.objects WHERE object_id=@ObjectID AND LEFT(type,1)=@Kind)
     OR @Current IS NULL
     OR (@Current<>@ExpectedHash AND (@Legacy IS NULL OR @Current<>@Legacy))
     OR NOT EXISTS(SELECT 1 FROM sys.sql_modules WHERE object_id=@ObjectID AND uses_ansi_nulls=1 AND uses_quoted_identifier=1))
 BEGIN
     SET @Message=N'Unreviewed definition or SET options for dbo.'+@Name+N'. Existing object preserved; review first.';
     THROW 52424,@Message,1;
 END;
 SET @Id+=1;
END;

SELECT Name AS ObjectName,
 CASE WHEN OBJECT_ID(N'dbo.'+Name) IS NULL THEN 'Create missing view'
      WHEN HASHBYTES('SHA2_256',OBJECT_DEFINITION(OBJECT_ID(N'dbo.'+Name)))=ExpectedHash THEN 'Already compatible'
      ELSE 'Extend reviewed procedure' END AS PlannedAction
FROM @Plan ORDER BY Id;
IF @Apply=0
BEGIN
 PRINT 'REVIEW ONLY: no changes made. Back up, review this plan, then set @Apply=1 and @BackupFile.';
 RETURN;
END;

-- Require a recent completed checksum full backup of this exact database GUID.
IF NULLIF(@BackupFile,N'') IS NULL OR NOT EXISTS(
 SELECT 1 FROM msdb.dbo.backupset b
 JOIN msdb.dbo.backupmediafamily f ON b.media_set_id=f.media_set_id
 JOIN sys.database_recovery_status r ON r.database_id=DB_ID() AND r.database_guid=b.database_guid
 WHERE b.database_name=DB_NAME() AND b.[type]='D' AND b.has_backup_checksums=1
 AND b.backup_finish_date>=DATEADD(day,-1,GETDATE()) AND f.physical_device_name=@BackupFile)
 THROW 52425, 'Supply a completed full CHECKSUM backup from the last 24 hours for this exact database.', 1;
RESTORE VERIFYONLY FROM DISK=@BackupFile WITH CHECKSUM;

BEGIN TRY
 BEGIN TRANSACTION;
 DECLARE @Lock int;
 EXEC @Lock=sys.sp_getapplock @Resource=N'SOD_WO_DatabaseUpdater',@LockMode='Exclusive',
     @LockOwner='Transaction',@LockTimeout=5000;
 IF @Lock<0 THROW 52426, 'Another updater is running. No changes applied.', 1;
 SET @Id=1;
 WHILE @Id<=5
 BEGIN
  SELECT @Name=Name,@Legacy=LegacyHash,@ExpectedHash=ExpectedHash,@DDL=DDL FROM @Plan WHERE Id=@Id;
  SET @ObjectID=OBJECT_ID(N'dbo.'+@Name);
  SET @Current=HASHBYTES('SHA2_256',OBJECT_DEFINITION(@ObjectID));
  IF @ObjectID IS NOT NULL AND (@Current IS NULL OR
      (@Current<>@ExpectedHash AND (@Legacy IS NULL OR @Current<>@Legacy)))
      THROW 52424, 'Definition changed since review. Transaction rolled back.', 1;
  IF @ObjectID IS NULL OR @Current<>@ExpectedHash
      EXEC sys.sp_executesql @DDL;
  SET @Id+=1;
 END;
 COMMIT TRANSACTION;
 PRINT 'COMMITTED: V7.1 procedures/views are compatible. No existing business rows were updated.';
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
