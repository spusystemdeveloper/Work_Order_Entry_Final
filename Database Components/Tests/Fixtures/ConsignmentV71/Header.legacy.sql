




CREATE PROCEDURE [dbo].[SOD_sp_InsertQoute]
(
	@vComment varchar(255),
	@iCustID int,
	@iSalesRepID int,
	@mTax money,
	@mTotal money,
	@vRemarks varchar(255)
	
)
	
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @iStoreID int,
			@iOrderID BigInt,
			@vRef varchar(30)

	SELECT @iStoreID=StoreID FROM configuration

	SELECT @vRef= NextORNumber FROM  SI_ORSeries WHERE RegisterNumber=@vComment
	
	IF @vRef IS NULL BEGIN
		SET @vRef=0
	END

	IF @vRemarks != '' BEGIN
		SET @vComment=@vRemarks
	END
		
	INSERT INTO [order] (StoreID,Closed,[Time],[Type],Comment,CustomerID,ShipToID,DepositOverride,Deposit,Tax,Total,LastUpdated,
	ExpirationOrDueDate,Taxable,SalesRepID,ReferenceNumber,ShippingChargeOnOrder,ShippingChargeOverride,ShippingServiceID,
	ShippingTrackingNumber,ShippingNotes,ReasonCodeID,ExchangeID,ChannelType,DefaultDiscountReasonCodeID,DefaultReturnReasonCodeID,DefaultTaxChangeReasonCodeID)
	SELECT @iStoreID,0,GetDate(),3,@vComment,@iCustID,0,0,0,@mTax,@mTotal,GetDate(),(CONVERT(VARCHAR(10),GETDATE(),23)),(CASE WHEN @mTax > 0 THEN 1 ELSE 0 END),
	@iSalesRepID,@vRef,0,0,0,'','',0,0,0,0,0,0

	SELECT @iOrderID = SCOPE_IDENTITY()

	RETURN @iOrderID


END