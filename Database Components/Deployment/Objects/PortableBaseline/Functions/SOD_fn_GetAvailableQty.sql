/*
  Work Order Entry / Order Processing Queueing database object
  Portable baseline for RMS store databases
  Object: [dbo].[SOD_fn_GetAvailableQty]
  Source type: FN
  Uses the selected RMS database only; review business rules before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetAvailableQty]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetAvailableQty].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE FUNCTION [dbo].[SOD_fn_GetAvailableQty]
(
	@vLookUp varchar(30),
	@iQty int
)
RETURNS float
AS
BEGIN
	
	DECLARE @ID int,
		@ItemLookUpCode varchar(30),
		@ParentItem int,
		@ParentQuantity float,
		@Quantity float,
		@Committed float,
		@SKULevel int,
		@xSKULevel int,
		@Available float,
		@xLoop BIT
	
	SET @Available = 0
	SET @xSKULevel = 0

	DECLARE cur CURSOR FOR

	SELECT ID,ItemLookUpcode,ParentItem,ParentQuantity,Quantity,QuantityCommitted,CONVERT(INT,SubDescription2) AS SKULEVEL
	FROM dbo.Item WHERE ItemLookUpCode LIKE ''%'' + Left(@vLookUp,5) + ''%'' 
	ORDER BY CONVERT(INT,SubDescription2) DESC
	
	OPEN CUR
	FETCH next FROM cur INTO @ID,@ItemLookUpCode,@ParentItem,@ParentQuantity,@Quantity,@Committed,@SKULevel
	WHILE @@FETCH_STATUS = 0
	BEGIN 
		
		IF @ItemLookUpCode = @vLookUp BEGIN
			SET @xSKULevel = @SKUlevel
			SET @Quantity = @iQty
		END ELSE BEGIN
			SET @Quantity = 0
		END

		IF @SKULevel <= @xSKULevel BEGIN
			SET @Available = (@Available * @ParentQuantity) + @Quantity 
		END
		
	FETCH next FROM cur INTO @ID,@ItemLookUpCode,@ParentItem,@ParentQuantity,@Quantity,@Committed,@SKULevel
	END
	CLOSE cur
	DEALLOCATE cur

	RETURN @Available

END';
ELSE
    EXEC sys.sp_executesql N'
ALTER FUNCTION [dbo].[SOD_fn_GetAvailableQty]
(
	@vLookUp varchar(30),
	@iQty int
)
RETURNS float
AS
BEGIN
	
	DECLARE @ID int,
		@ItemLookUpCode varchar(30),
		@ParentItem int,
		@ParentQuantity float,
		@Quantity float,
		@Committed float,
		@SKULevel int,
		@xSKULevel int,
		@Available float,
		@xLoop BIT
	
	SET @Available = 0
	SET @xSKULevel = 0

	DECLARE cur CURSOR FOR

	SELECT ID,ItemLookUpcode,ParentItem,ParentQuantity,Quantity,QuantityCommitted,CONVERT(INT,SubDescription2) AS SKULEVEL
	FROM dbo.Item WHERE ItemLookUpCode LIKE ''%'' + Left(@vLookUp,5) + ''%'' 
	ORDER BY CONVERT(INT,SubDescription2) DESC
	
	OPEN CUR
	FETCH next FROM cur INTO @ID,@ItemLookUpCode,@ParentItem,@ParentQuantity,@Quantity,@Committed,@SKULevel
	WHILE @@FETCH_STATUS = 0
	BEGIN 
		
		IF @ItemLookUpCode = @vLookUp BEGIN
			SET @xSKULevel = @SKUlevel
			SET @Quantity = @iQty
		END ELSE BEGIN
			SET @Quantity = 0
		END

		IF @SKULevel <= @xSKULevel BEGIN
			SET @Available = (@Available * @ParentQuantity) + @Quantity 
		END
		
	FETCH next FROM cur INTO @ID,@ItemLookUpCode,@ParentItem,@ParentQuantity,@Quantity,@Committed,@SKULevel
	END
	CLOSE cur
	DEALLOCATE cur

	RETURN @Available

END';
GO
