/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: GSC_STORE_DB
  Object: [dbo].[SOD_viewLastUpdatedItem]
  Source type: V 
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_viewLastUpdatedItem]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52001, 'Object type conflict for [dbo].[SOD_viewLastUpdatedItem].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE view SOD_viewLastUpdatedItem
as
select top 20 s.ItemLookupCode,
(select quantity-QuantityCommitted  from item i where i.ItemLookupCode = s.ItemLookupCode) as ''Available''
from SOD_Item_Logs s
where s.type = ''Item Auto Logs''
group by s.ItemLookupCode 
order by max(s.date) desc ';
ELSE
    EXEC sys.sp_executesql N'ALTER view SOD_viewLastUpdatedItem
as
select top 20 s.ItemLookupCode,
(select quantity-QuantityCommitted  from item i where i.ItemLookupCode = s.ItemLookupCode) as ''Available''
from SOD_Item_Logs s
where s.type = ''Item Auto Logs''
group by s.ItemLookupCode 
order by max(s.date) desc ';
GO
