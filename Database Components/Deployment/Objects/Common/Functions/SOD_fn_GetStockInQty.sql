/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetStockInQty]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetStockInQty]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetStockInQty].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION [dbo].[SOD_fn_GetStockInQty]
(
			@lookupcode varchar(50)
)

	RETURNS int

	AS
	BEGIN
	
		   DECLARE @StockinQty int

				select  distinct  @StockinQty = (select ISNULL(sum(Total),0) from dbo.SOD_fntbl_sumStockin(@lookupcode))

				
			RETURN @StockinQty

	END
';
ELSE
    EXEC sys.sp_executesql N'ALTER FUNCTION [dbo].[SOD_fn_GetStockInQty]
(
			@lookupcode varchar(50)
)

	RETURNS int

	AS
	BEGIN
	
		   DECLARE @StockinQty int

				select  distinct  @StockinQty = (select ISNULL(sum(Total),0) from dbo.SOD_fntbl_sumStockin(@lookupcode))

				
			RETURN @StockinQty

	END
';
GO
