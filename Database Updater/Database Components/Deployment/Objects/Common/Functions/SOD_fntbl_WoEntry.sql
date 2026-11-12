/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_fntbl_WoEntry]
  Source type: TF
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_WoEntry]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_WoEntry].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE function [dbo].[SOD_fntbl_WoEntry]
(
			@Orderid int
)
RETURNS @temptable TABLE 
(
		Itemcode varchar(10),ItemDesc varchar(max),QtyOrder int,Price varchar(max),Extended varchar(max)
)
AS
BEGIN 

DECLARE @type int
set @type = (SELECT closed from [order] where id = @Orderid )

IF @type = 0 BEGIN
			insert into @temptable (Itemcode,ItemDesc,QtyOrder,Price,Extended)
			select i.ItemLookupCode, 
			cast(i.Description as varchar(max)) + '''' +cast(i.extendedDescription as varchar(max)) as ''Desc'',
			o.QuantityOnOrder,convert(varchar,cast(o.price as money),-1),convert(varchar,cast(o.price * o.QuantityOnOrder  as money),-1) as ''Extended''
			from OrderEntry o 
			inner join item i on o.ItemID = i.id
			where o.orderid = @Orderid
END
ELSE BEGIN
			insert into @temptable (Itemcode,ItemDesc,QtyOrder,Price,Extended)
			select i.ItemLookupCode, 
			cast(i.Description as varchar(max)) + '''' +cast(i.extendedDescription as varchar(max)) as ''Desc'',
			o.Quantity ,convert(varchar,cast(o.price as money),-1),convert(varchar,cast(o.price * o.Quantity   as money),-1) as ''Extended''
			from TransactionEntry o 
			inner join [Transaction] t on o.TransactionNumber = t.TransactionNumber 
			inner join item i on o.ItemID = i.id
			where t.RecallID  = @Orderid
END
		RETURN
END

';
ELSE
    EXEC sys.sp_executesql N'
ALTER function [dbo].[SOD_fntbl_WoEntry]
(
			@Orderid int
)
RETURNS @temptable TABLE 
(
		Itemcode varchar(10),ItemDesc varchar(max),QtyOrder int,Price varchar(max),Extended varchar(max)
)
AS
BEGIN 

DECLARE @type int
set @type = (SELECT closed from [order] where id = @Orderid )

IF @type = 0 BEGIN
			insert into @temptable (Itemcode,ItemDesc,QtyOrder,Price,Extended)
			select i.ItemLookupCode, 
			cast(i.Description as varchar(max)) + '''' +cast(i.extendedDescription as varchar(max)) as ''Desc'',
			o.QuantityOnOrder,convert(varchar,cast(o.price as money),-1),convert(varchar,cast(o.price * o.QuantityOnOrder  as money),-1) as ''Extended''
			from OrderEntry o 
			inner join item i on o.ItemID = i.id
			where o.orderid = @Orderid
END
ELSE BEGIN
			insert into @temptable (Itemcode,ItemDesc,QtyOrder,Price,Extended)
			select i.ItemLookupCode, 
			cast(i.Description as varchar(max)) + '''' +cast(i.extendedDescription as varchar(max)) as ''Desc'',
			o.Quantity ,convert(varchar,cast(o.price as money),-1),convert(varchar,cast(o.price * o.Quantity   as money),-1) as ''Extended''
			from TransactionEntry o 
			inner join [Transaction] t on o.TransactionNumber = t.TransactionNumber 
			inner join item i on o.ItemID = i.id
			where t.RecallID  = @Orderid
END
		RETURN
END

';
GO
