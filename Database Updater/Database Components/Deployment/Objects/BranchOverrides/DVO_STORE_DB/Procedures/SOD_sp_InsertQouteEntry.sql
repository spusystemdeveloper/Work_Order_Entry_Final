/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_sp_InsertQouteEntry]
  Source type: P 
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_sp_InsertQouteEntry]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'P '
    THROW 52001, 'Object type conflict for [dbo].[SOD_sp_InsertQouteEntry].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE PROCEDURE [dbo].[SOD_sp_InsertQouteEntry]
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
  @QtyPrep int,
  @Type int
)
  
AS
BEGIN
  
  SET NOCOUNT ON;

  DECLARE @iStoreID int, @iQueueID int
  DECLARE @ExtendedDescription nvarchar(50)

  SELECT @iStoreID=StoreID FROM configuration
  SELECT @ExtendedDescription=ExtendedDescription FROM Item WHERE ID = @iItemID
  --For Order Table
  INSERT INTO orderentry (Cost,StoreID,OrderID,ItemID,FullPrice,PriceSource,Price,QuantityOnOrder,SalesRepID,
              Taxable,DetailID,Description,QuantityRTD,LastUpdated,Comment,DiscountReasonCodeID,
              ReturnReasonCodeID,TaxChangeReasonCodeID)
  SELECT @mCost,@iStoreid,@iOrderID,@iItemID,@mFullPrice,'''',@mPrice,@fQuantityOnOrder,@iSalesRepID,@iTaxable,
      0,@vDescription,0,GetDate(),@ExtendedDescription,0,0,0

  --IF @Type = 2
  BEGIN
      --For Queueing Table
    SELECT @iQueueID=ID FROM Queueing WHERE OrderID = @iOrderID
    INSERT INTO QueueingItems (QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
    SELECT @iQueueID, @iItemID,@QtyPrep,'''',''-'',@vComment 

  END


END';
ELSE
    EXEC sys.sp_executesql N'
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
  @QtyPrep int,
  @Type int
)
  
AS
BEGIN
  
  SET NOCOUNT ON;

  DECLARE @iStoreID int, @iQueueID int
  DECLARE @ExtendedDescription nvarchar(50)

  SELECT @iStoreID=StoreID FROM configuration
  SELECT @ExtendedDescription=ExtendedDescription FROM Item WHERE ID = @iItemID
  --For Order Table
  INSERT INTO orderentry (Cost,StoreID,OrderID,ItemID,FullPrice,PriceSource,Price,QuantityOnOrder,SalesRepID,
              Taxable,DetailID,Description,QuantityRTD,LastUpdated,Comment,DiscountReasonCodeID,
              ReturnReasonCodeID,TaxChangeReasonCodeID)
  SELECT @mCost,@iStoreid,@iOrderID,@iItemID,@mFullPrice,'''',@mPrice,@fQuantityOnOrder,@iSalesRepID,@iTaxable,
      0,@vDescription,0,GetDate(),@ExtendedDescription,0,0,0

  --IF @Type = 2
  BEGIN
      --For Queueing Table
    SELECT @iQueueID=ID FROM Queueing WHERE OrderID = @iOrderID
    INSERT INTO QueueingItems (QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
    SELECT @iQueueID, @iItemID,@QtyPrep,'''',''-'',@vComment 

  END


END';
GO
