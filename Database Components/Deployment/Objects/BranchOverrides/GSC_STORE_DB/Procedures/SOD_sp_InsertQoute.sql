/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: GSC_STORE_DB
  Object: [dbo].[SOD_sp_InsertQoute]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_sp_InsertQoute]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'P '
    THROW 52001, 'Object type conflict for [dbo].[SOD_sp_InsertQoute].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'

CREATE PROCEDURE [dbo].[SOD_sp_InsertQoute]
(
	@vComment varchar(255),
	@iCustID int,
	@iSalesRepID int,
	@mTax money,
	@mTotal money,
	@vRemarks varchar(255),
	@OPIS varchar(255),
	@Type int
	
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

	IF @vRemarks != '''' BEGIN
		SET @vComment=@vRemarks
	END
		
	INSERT INTO [order] (StoreID,Closed,[Time],[Type],Comment,CustomerID,ShipToID,DepositOverride,Deposit,Tax,Total,LastUpdated,
	ExpirationOrDueDate,Taxable,SalesRepID,ReferenceNumber,ShippingChargeOnOrder,ShippingChargeOverride,ShippingServiceID,
	ShippingTrackingNumber,ShippingNotes,ReasonCodeID,ExchangeID,ChannelType,DefaultDiscountReasonCodeID,DefaultReturnReasonCodeID,DefaultTaxChangeReasonCodeID)
	SELECT @iStoreID,0,GetDate(),@Type,@vComment,@iCustID,0,0,0,@mTax,@mTotal,GetDate(),(CONVERT(VARCHAR(10),GETDATE(),23)),(CASE WHEN @mTax > 0 THEN 1 ELSE 0 END),
	@iSalesRepID,@vRef,0,0,0,'''','''',0,0,0,0,0,0

	SELECT @iOrderID = SCOPE_IDENTITY()
	
	IF @Type = 2
	BEGIN
		INSERT INTO Queueing(OrderID,Status,GroupTo,OPIS)VALUES(@iOrderID,0,@iOrderID,@OPIS)
	END
	

	RETURN @iOrderID


END
';
ELSE
    EXEC sys.sp_executesql N'

ALTER PROCEDURE [dbo].[SOD_sp_InsertQoute]
(
	@vComment varchar(255),
	@iCustID int,
	@iSalesRepID int,
	@mTax money,
	@mTotal money,
	@vRemarks varchar(255),
	@OPIS varchar(255),
	@Type int
	
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

	IF @vRemarks != '''' BEGIN
		SET @vComment=@vRemarks
	END
		
	INSERT INTO [order] (StoreID,Closed,[Time],[Type],Comment,CustomerID,ShipToID,DepositOverride,Deposit,Tax,Total,LastUpdated,
	ExpirationOrDueDate,Taxable,SalesRepID,ReferenceNumber,ShippingChargeOnOrder,ShippingChargeOverride,ShippingServiceID,
	ShippingTrackingNumber,ShippingNotes,ReasonCodeID,ExchangeID,ChannelType,DefaultDiscountReasonCodeID,DefaultReturnReasonCodeID,DefaultTaxChangeReasonCodeID)
	SELECT @iStoreID,0,GetDate(),@Type,@vComment,@iCustID,0,0,0,@mTax,@mTotal,GetDate(),(CONVERT(VARCHAR(10),GETDATE(),23)),(CASE WHEN @mTax > 0 THEN 1 ELSE 0 END),
	@iSalesRepID,@vRef,0,0,0,'''','''',0,0,0,0,0,0

	SELECT @iOrderID = SCOPE_IDENTITY()
	
	IF @Type = 2
	BEGIN
		INSERT INTO Queueing(OrderID,Status,GroupTo,OPIS)VALUES(@iOrderID,0,@iOrderID,@OPIS)
	END
	

	RETURN @iOrderID


END
';
GO
