/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetReg]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetReg]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetReg].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'





CREATE FUNCTION [dbo].[SOD_fn_GetReg]
(
	@vIP varchar(30)
)
RETURNS money
AS
BEGIN
	
	DECLARE @vReg varchar(5)	
	
	SELECT @vReg=r.Number FROM Register r,SOD_Reg s
	WHERE s.ipadd=@vIP AND (r.ID=s.regid)

	RETURN @vReg

END





';
ELSE
    EXEC sys.sp_executesql N'





ALTER FUNCTION [dbo].[SOD_fn_GetReg]
(
	@vIP varchar(30)
)
RETURNS money
AS
BEGIN
	
	DECLARE @vReg varchar(5)	
	
	SELECT @vReg=r.Number FROM Register r,SOD_Reg s
	WHERE s.ipadd=@vIP AND (r.ID=s.regid)

	RETURN @vReg

END





';
GO
