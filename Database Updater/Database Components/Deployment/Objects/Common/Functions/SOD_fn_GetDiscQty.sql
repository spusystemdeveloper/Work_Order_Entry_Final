/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetDiscQty]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetDiscQty]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetDiscQty].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'

CREATE FUNCTION [dbo].[SOD_fn_GetDiscQty]
(
	@iDiscID int,
	@iType int
)
RETURNS float
AS
BEGIN
	
	DECLARE @fDiscVal float
	
	IF @iType = 0 BEGIN

		SELECT @fDiscVal = Quantity1 FROM QuantityDiscount WHERE ID=@iDiscID

	END

	ELSE BEGIN

		SELECT @fDiscVal = Price1 FROM QuantityDiscount WHERE ID=@iDiscID

	END

	RETURN @fDiscVal

END



';
ELSE
    EXEC sys.sp_executesql N'

ALTER FUNCTION [dbo].[SOD_fn_GetDiscQty]
(
	@iDiscID int,
	@iType int
)
RETURNS float
AS
BEGIN
	
	DECLARE @fDiscVal float
	
	IF @iType = 0 BEGIN

		SELECT @fDiscVal = Quantity1 FROM QuantityDiscount WHERE ID=@iDiscID

	END

	ELSE BEGIN

		SELECT @fDiscVal = Price1 FROM QuantityDiscount WHERE ID=@iDiscID

	END

	RETURN @fDiscVal

END



';
GO
