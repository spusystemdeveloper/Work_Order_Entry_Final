/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fntbl_CheckOpenWO]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_CheckOpenWO]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'IF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_CheckOpenWO].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'




CREATE FUNCTION [dbo].[SOD_fntbl_CheckOpenWO]
(	
@vCode VARCHAR(15)
)
RETURNS TABLE 
AS
RETURN 
(
 SELECT 
 [Order].ID, 
 CASE WHEN [Order].Closed = 0 THEN ''Open'' ELSE ''Closed'' END AS Status,
 Customer.Company,
 customer.AccountNumber,
 item.ItemLookupCode,
 concat(item.Description,item.ExtendedDescription) AS Description,
 OrderEntry.QuantityOnOrder,
 Cashier.Name,
 FORMAT([Order].Time, ''MM/dd/yyyy'') AS OrderDate,
 CASE 
    WHEN DATEDIFF(DAY, [Order].Time, GETDATE()) = 1 
        THEN ''1 day''
    ELSE 
        CONCAT(DATEDIFF(DAY, [Order].Time, GETDATE()), '' days'')
END AS DaysOpenLabel

 FROM  OrderEntry WITH(NOLOCK) 
         INNER       JOIN [Order] WITH(NOLOCK) ON OrderEntry.OrderID = [Order].ID 
         LEFT JOIN   Customer WITH(NOLOCK) ON [Order].CustomerID = Customer.ID 
         LEFT JOIN   Item WITH(NOLOCK) ON OrderEntry.ItemID = Item.ID 
	 LEFT JOIN   Queueing ON [Order].ID = Queueing.OrderID
	 LEFT JOIN   [Cashier] ON Queueing.OPIS = Cashier.Number
         LEFT JOIN   ReasonCode AS ReasonCodeDiscount WITH(NOLOCK) ON OrderEntry.DiscountReasonCodeID = ReasonCodeDiscount.ID 
         LEFT JOIN   ReasonCode AS ReasonCodeTaxChange WITH(NOLOCK) ON OrderEntry.TaxChangeReasonCodeID = ReasonCodeTaxChange.ID
		 WHERE ITEM.ITEMLOOKUPCODE = @vCode AND [ORDER].Closed = 0 AND [Order].Type = 2
)
';
ELSE
    EXEC sys.sp_executesql N'




ALTER FUNCTION [dbo].[SOD_fntbl_CheckOpenWO]
(	
@vCode VARCHAR(15)
)
RETURNS TABLE 
AS
RETURN 
(
 SELECT 
 [Order].ID, 
 CASE WHEN [Order].Closed = 0 THEN ''Open'' ELSE ''Closed'' END AS Status,
 Customer.Company,
 customer.AccountNumber,
 item.ItemLookupCode,
 concat(item.Description,item.ExtendedDescription) AS Description,
 OrderEntry.QuantityOnOrder,
 Cashier.Name,
 FORMAT([Order].Time, ''MM/dd/yyyy'') AS OrderDate,
 CASE 
    WHEN DATEDIFF(DAY, [Order].Time, GETDATE()) = 1 
        THEN ''1 day''
    ELSE 
        CONCAT(DATEDIFF(DAY, [Order].Time, GETDATE()), '' days'')
END AS DaysOpenLabel

 FROM  OrderEntry WITH(NOLOCK) 
         INNER       JOIN [Order] WITH(NOLOCK) ON OrderEntry.OrderID = [Order].ID 
         LEFT JOIN   Customer WITH(NOLOCK) ON [Order].CustomerID = Customer.ID 
         LEFT JOIN   Item WITH(NOLOCK) ON OrderEntry.ItemID = Item.ID 
	 LEFT JOIN   Queueing ON [Order].ID = Queueing.OrderID
	 LEFT JOIN   [Cashier] ON Queueing.OPIS = Cashier.Number
         LEFT JOIN   ReasonCode AS ReasonCodeDiscount WITH(NOLOCK) ON OrderEntry.DiscountReasonCodeID = ReasonCodeDiscount.ID 
         LEFT JOIN   ReasonCode AS ReasonCodeTaxChange WITH(NOLOCK) ON OrderEntry.TaxChangeReasonCodeID = ReasonCodeTaxChange.ID
		 WHERE ITEM.ITEMLOOKUPCODE = @vCode AND [ORDER].Closed = 0 AND [Order].Type = 2
)
';
GO
