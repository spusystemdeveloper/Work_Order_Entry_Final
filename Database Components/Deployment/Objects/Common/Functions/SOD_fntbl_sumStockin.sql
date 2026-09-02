/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fntbl_sumStockin]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_sumStockin]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'IF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_sumStockin].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION [dbo].[SOD_fntbl_sumStockin]
(	
	@lookup VARCHAR(50)
)
RETURNS TABLE 
AS
RETURN 
(
				select sum(distinct s.qtyorder) as ''Total'' from SOD_WO s
				inner join item i on s.itemid = i.id
				inner join purchaseorderentry pp on s.itemid = pp.itemid
				inner join purchaseorder p on pp.purchaseorderid = p.id
				inner join [order] o on s.orderid = o.id
				where p.POtitle = ''From STOCK-IN'' and s.StockIn = 1
				and i.itemlookupcode = @lookup and o.closed = 0
				group by s.orderid
			
			
)


';
ELSE
    EXEC sys.sp_executesql N'ALTER FUNCTION [dbo].[SOD_fntbl_sumStockin]
(	
	@lookup VARCHAR(50)
)
RETURNS TABLE 
AS
RETURN 
(
				select sum(distinct s.qtyorder) as ''Total'' from SOD_WO s
				inner join item i on s.itemid = i.id
				inner join purchaseorderentry pp on s.itemid = pp.itemid
				inner join purchaseorder p on pp.purchaseorderid = p.id
				inner join [order] o on s.orderid = o.id
				where p.POtitle = ''From STOCK-IN'' and s.StockIn = 1
				and i.itemlookupcode = @lookup and o.closed = 0
				group by s.orderid
			
			
)


';
GO
