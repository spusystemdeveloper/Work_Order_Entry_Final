/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fntbl_NestedSearchItem]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_NestedSearchItem]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'IF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_NestedSearchItem].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION [dbo].[SOD_fntbl_NestedSearchItem]
(
	@keyword VARCHAR(200)
)
RETURNS TABLE 
AS
RETURN
(
	SELECT * FROM NestedSearchItem WHERE Keyword = @keyword
)';
ELSE
    EXEC sys.sp_executesql N'ALTER FUNCTION [dbo].[SOD_fntbl_NestedSearchItem]
(
	@keyword VARCHAR(200)
)
RETURNS TABLE 
AS
RETURN
(
	SELECT * FROM NestedSearchItem WHERE Keyword = @keyword
)';
GO
