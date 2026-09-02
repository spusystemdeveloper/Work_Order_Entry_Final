/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fntbl_SearchItem]
  Source type: IF
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_SearchItem]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'IF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_SearchItem].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'










CREATE FUNCTION [dbo].[SOD_fntbl_SearchItem]
(	
	@bEmployee bit,
	@iLevel int,
	@sTxt varchar(MAX)
)
RETURNS TABLE 
AS
RETURN 
(
	
	SELECT ItemLookUpCode,(Description + '' '' + CONVERT(NVARCHAR(MAX),ExtendedDescription)) AS ItemName,
			CASE 
				WHEN @bEmployee=0 THEN
					CASE
						 WHEN @iLevel=0 THEN Price 
						 WHEN @iLevel=1 THEN PriceA
						 WHEN @iLevel=2 THEN PriceB
						 WHEN @iLevel=3 THEN PriceC
					ELSE Price END
				WHEN @bEmployee=1 THEN	
					(Cost + (Cost * .10)) 
			ELSE Price END AS MyPrice,
			Cost,Quantity-QuantityCommitted AS AvailableQty,ParentQuantity,TaxID,SubDescription1,Price,ID,Description,ExtendedDescription,ItemType, Inactive, 
			PARSENAME(REPLACE(REPLACE((Description + '' '' + CONVERT(VARCHAR(MAX),ExtendedDescription)),''.'',''''), ''('', ''.''), 2) AS ItemDes,subdescription2, QuantityDiscountID,
			[dbo].[SOD_fn_GetDiscQty](QuantityDiscountID,0) as DiscQTy,[dbo].[SOD_fn_GetDiscQty](QuantityDiscountID,1) as DiscPrice,SalePrice
	FROM item
	WHERE ItemLookUpCode like ''%'' + @sTxt +''%'' OR Description like  ''%'' + @sTxt +''%'' OR ExtendedDescription like  ''%'' + @sTxt +''%''
)








';
ELSE
    EXEC sys.sp_executesql N'










ALTER FUNCTION [dbo].[SOD_fntbl_SearchItem]
(	
	@bEmployee bit,
	@iLevel int,
	@sTxt varchar(MAX)
)
RETURNS TABLE 
AS
RETURN 
(
	
	SELECT ItemLookUpCode,(Description + '' '' + CONVERT(NVARCHAR(MAX),ExtendedDescription)) AS ItemName,
			CASE 
				WHEN @bEmployee=0 THEN
					CASE
						 WHEN @iLevel=0 THEN Price 
						 WHEN @iLevel=1 THEN PriceA
						 WHEN @iLevel=2 THEN PriceB
						 WHEN @iLevel=3 THEN PriceC
					ELSE Price END
				WHEN @bEmployee=1 THEN	
					(Cost + (Cost * .10)) 
			ELSE Price END AS MyPrice,
			Cost,Quantity-QuantityCommitted AS AvailableQty,ParentQuantity,TaxID,SubDescription1,Price,ID,Description,ExtendedDescription,ItemType, Inactive, 
			PARSENAME(REPLACE(REPLACE((Description + '' '' + CONVERT(VARCHAR(MAX),ExtendedDescription)),''.'',''''), ''('', ''.''), 2) AS ItemDes,subdescription2, QuantityDiscountID,
			[dbo].[SOD_fn_GetDiscQty](QuantityDiscountID,0) as DiscQTy,[dbo].[SOD_fn_GetDiscQty](QuantityDiscountID,1) as DiscPrice,SalePrice
	FROM item
	WHERE ItemLookUpCode like ''%'' + @sTxt +''%'' OR Description like  ''%'' + @sTxt +''%'' OR ExtendedDescription like  ''%'' + @sTxt +''%''
)








';
GO
