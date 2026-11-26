/*
  Work Order Entry / Order Processing Queueing database object
  Portable baseline for RMS store databases
  Object: [dbo].[SOD_fn_GetCreditLimit]
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
    FROM dbo.AccountReceivable
    WHERE Balance > 0
      AND CustomerID = @iCustomerID;

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
    FROM dbo.AccountReceivable
    WHERE Balance > 0
      AND CustomerID = @iCustomerID;

    RETURN @dCreditLimit;
END;
';
GO
