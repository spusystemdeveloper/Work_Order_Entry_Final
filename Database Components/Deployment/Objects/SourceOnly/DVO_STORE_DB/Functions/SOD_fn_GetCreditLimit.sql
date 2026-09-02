/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetCreditLimit]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetCreditLimit]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetCreditLimit].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION [dbo].[SOD_fn_GetCreditLimit]
(
    @iCustomerID INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @dCreditLimit DECIMAL(18,2);

    SELECT @dCreditLimit = ISNULL(SUM(OriginalAmount), 0)
    FROM (
        SELECT OriginalAmount 
        FROM DVO_STORE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID

        UNION ALL

        SELECT OriginalAmount 
        FROM DVO_MAA_WHSE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID

        UNION ALL

        SELECT OriginalAmount 
        FROM DVO_VINZON_WHSE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID
    ) AS AllReceivables;

    RETURN @dCreditLimit;
END;
';
ELSE
    EXEC sys.sp_executesql N'ALTER FUNCTION [dbo].[SOD_fn_GetCreditLimit]
(
    @iCustomerID INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @dCreditLimit DECIMAL(18,2);

    SELECT @dCreditLimit = ISNULL(SUM(OriginalAmount), 0)
    FROM (
        SELECT OriginalAmount 
        FROM DVO_STORE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID

        UNION ALL

        SELECT OriginalAmount 
        FROM DVO_MAA_WHSE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID

        UNION ALL

        SELECT OriginalAmount 
        FROM DVO_VINZON_WHSE_DB.dbo.AccountReceivable 
        WHERE Balance > 0 AND CustomerID = @iCustomerID
    ) AS AllReceivables;

    RETURN @dCreditLimit;
END;
';
GO
