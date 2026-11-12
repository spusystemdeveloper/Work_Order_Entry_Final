/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: GSC_STORE_DB
  Object: [dbo].[SOD_fntbl_CheckQty]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_CheckQty]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'IF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_CheckQty].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'


-- =============================================
CREATE FUNCTION [dbo].[SOD_fntbl_CheckQty]
(	
	@vCode VARCHAR(15)
)
RETURNS TABLE 
AS
RETURN 
(
	
	SELECT ID,[Name],StoreCode,
	CASE
	WHEN StoreCode = ''GSC_STORE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_STORE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_MAIN_WHSE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_MAIN_WHSE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_OFFICE_SUPPLY_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_OFFICE_SUPPLY_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	ELSE 0
	END AS Qty,(Address1 + '', '' + Country) as Address
	FROM STORE WHERE ID != (SELECT StoreID FROM configuration)
	AND StoreCode like ''%GSC%''
	
)
';
ELSE
    EXEC sys.sp_executesql N'


-- =============================================
ALTER FUNCTION [dbo].[SOD_fntbl_CheckQty]
(	
	@vCode VARCHAR(15)
)
RETURNS TABLE 
AS
RETURN 
(
	
	SELECT ID,[Name],StoreCode,
	CASE
	WHEN StoreCode = ''GSC_STORE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_STORE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_MAIN_WHSE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_MAIN_WHSE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_OFFICE_SUPPLY_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_OFFICE_SUPPLY_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	ELSE 0
	END AS Qty,(Address1 + '', '' + Country) as Address
	FROM STORE WHERE ID != (SELECT StoreID FROM configuration)
	AND StoreCode like ''%GSC%''
	
)
';
GO
