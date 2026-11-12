/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fn_GetTotalOpenWorkOrder]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fn_GetTotalOpenWorkOrder]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fn_GetTotalOpenWorkOrder].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION [dbo].[SOD_fn_GetTotalOpenWorkOrder]
(
    @iCustomerID INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @dTotal DECIMAL(18,2);

    SELECT @dTotal = ISNULL(SUM(Total), 0)
    FROM (
   
		SELECT Total
		FROM DVO_STORE_DB.dbo.[Order] 
		WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2

        UNION ALL

       SELECT Total
        FROM DVO_MAA_WHSE_DB.dbo.[Order] 
       	WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2

        UNION ALL

        
       SELECT Total 
        FROM DVO_VINZON_WHSE_DB.dbo.[Order]  
        WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2
    ) AS OpenWorkOrder;

    RETURN @dTotal;
END;
';
ELSE
    EXEC sys.sp_executesql N'ALTER FUNCTION [dbo].[SOD_fn_GetTotalOpenWorkOrder]
(
    @iCustomerID INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @dTotal DECIMAL(18,2);

    SELECT @dTotal = ISNULL(SUM(Total), 0)
    FROM (
   
		SELECT Total
		FROM DVO_STORE_DB.dbo.[Order] 
		WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2

        UNION ALL

       SELECT Total
        FROM DVO_MAA_WHSE_DB.dbo.[Order] 
       	WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2

        UNION ALL

        
       SELECT Total 
        FROM DVO_VINZON_WHSE_DB.dbo.[Order]  
        WHERE Closed = 0 AND CustomerID = @iCustomerID AND Type = 2
    ) AS OpenWorkOrder;

    RETURN @dTotal;
END;
';
GO
