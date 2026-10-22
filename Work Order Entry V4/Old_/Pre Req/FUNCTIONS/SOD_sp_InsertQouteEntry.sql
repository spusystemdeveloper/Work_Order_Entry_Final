

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

	SELECT @iStoreID=StoreID FROM configuration

	--For Order Table
	INSERT INTO orderentry (Cost,StoreID,OrderID,ItemID,FullPrice,PriceSource,Price,QuantityOnOrder,SalesRepID,
							Taxable,DetailID,Description,QuantityRTD,LastUpdated,Comment,DiscountReasonCodeID,
							ReturnReasonCodeID,TaxChangeReasonCodeID)
	SELECT @mCost,@iStoreid,@iOrderID,@iItemID,@mFullPrice,'',@mPrice,@fQuantityOnOrder,@iSalesRepID,@iTaxable,
			0,@vDescription,0,GetDate(),@vComment,0,0,0

	IF @Type = 2
	BEGIN
			--For Queueing Table
		SELECT @iQueueID=ID FROM Queueing WHERE OrderID = @iOrderID
		INSERT INTO QueueingItems (QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
		SELECT @iQueueID, @iItemID,@QtyPrep,'','-',@vComment 

	END


END


GO


