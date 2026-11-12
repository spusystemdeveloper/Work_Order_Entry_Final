/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetQty]
  Source type: FN
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetQty]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetQty].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE FUNCTION [dbo].[SOD_fn_GetQty]
(
	@iStoreID int,
	@vCode varchar(15),
	@iType int
)
RETURNS float
AS
BEGIN
	
	DECLARE @fRes float, @qtyComm INT

	
	IF 	@iType=0 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Quantity FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode 
		END
		ELSE IF @iStoreID=3002 BEGIN
			
			SELECT @qtyComm=ISNULL(SUM(b.QuantityOnOrder),0) from [order] a
			INNER JOIN [orderentry] b ON a.id = b.orderid
			INNER JOIN Item c on b.ItemID = c.ID
			WHERE a.comment like  ''%Pick-up Loc: SOD-CEBU WAREHOUSE 1%''
			and Closed = 0 and c.ItemLookupCode=@vCode

			SELECT @fRes = Quantity-@qtyComm FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END
	END
	ELSE IF  @iType=1 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Cost FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes = Cost FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=2 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Price FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes = Price FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=3 BEGIN

		IF @iStoreID=1600 BEGIN
			
			SELECT @fRes=Quantity1 FROM [ILOILO_MAIN_WHSE_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode)
			
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes=Quantity1 FROM [SBD_CEBU_WHSE_1_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_WHSE_1_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=4 BEGIN

		IF @iStoreID=1600 BEGIN
			
			SELECT @fRes=Price1 FROM [ILOILO_MAIN_WHSE_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_MAIN_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
			
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes=Price1 FROM [SBD_CEBU_WHSE_1_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_WHSE_1_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END
	
	RETURN @fRes

END
';
ELSE
    EXEC sys.sp_executesql N'
ALTER FUNCTION [dbo].[SOD_fn_GetQty]
(
	@iStoreID int,
	@vCode varchar(15),
	@iType int
)
RETURNS float
AS
BEGIN
	
	DECLARE @fRes float, @qtyComm INT

	
	IF 	@iType=0 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Quantity FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode 
		END
		ELSE IF @iStoreID=3002 BEGIN
			
			SELECT @qtyComm=ISNULL(SUM(b.QuantityOnOrder),0) from [order] a
			INNER JOIN [orderentry] b ON a.id = b.orderid
			INNER JOIN Item c on b.ItemID = c.ID
			WHERE a.comment like  ''%Pick-up Loc: SOD-CEBU WAREHOUSE 1%''
			and Closed = 0 and c.ItemLookupCode=@vCode

			SELECT @fRes = Quantity-@qtyComm FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END
	END
	ELSE IF  @iType=1 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Cost FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes = Cost FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=2 BEGIN

		IF @iStoreID=1600 BEGIN
			SELECT @fRes = Price FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes = Price FROM [SBD_CEBU_WHSE_1_DB].dbo.item
			WHERE ItemLookUpCode=@vCode
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=3 BEGIN

		IF @iStoreID=1600 BEGIN
			
			SELECT @fRes=Quantity1 FROM [ILOILO_MAIN_WHSE_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [ILOILO_MAIN_WHSE_DB].dbo.item
			WHERE ItemLookUpCode=@vCode)
			
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes=Quantity1 FROM [SBD_CEBU_WHSE_1_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_WHSE_1_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END

	ELSE IF  @iType=4 BEGIN

		IF @iStoreID=1600 BEGIN
			
			SELECT @fRes=Price1 FROM [ILOILO_MAIN_WHSE_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_MAIN_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
			
		END
		ELSE IF @iStoreID=3002 BEGIN
			SELECT @fRes=Price1 FROM [SBD_CEBU_WHSE_1_DB].dbo.QuantityDiscount
			WHERE ID=(SELECT QuantityDiscountID FROM [SBD_CEBU_WHSE_1_DB].dbo.Item 
			WHERE ItemLookUpCode=@vCode)
		END
		ELSE BEGIN
			SET @fRes=0
		END

	END
	
	RETURN @fRes

END
';
GO
