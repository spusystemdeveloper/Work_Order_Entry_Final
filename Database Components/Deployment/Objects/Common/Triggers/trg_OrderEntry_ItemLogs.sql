/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[trg_OrderEntry_ItemLogs]
  Source type: TR
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_OrderEntry_ItemLogs]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TR'
    THROW 52001, 'Object type conflict for [dbo].[trg_OrderEntry_ItemLogs].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE TRIGGER [dbo].[trg_OrderEntry_ItemLogs]
   ON [dbo].[OrderEntry]
   AFTER INSERT, UPDATE
AS BEGIN
    SET NOCOUNT ON;    

	declare @Qty integer
	declare @exist integer
	declare @Orderid integer
	declare @Itemid integer
	declare @lastcommitted integer

	set @Orderid = (select orderid from deleted)
	set @Qty = (select QuantityOnOrder from inserted)
	set @Itemid = (select itemid from deleted)
	set @exist = (select count(id) from [Order] where id = @Orderid)
	set @lastcommitted = (select top 1 NewCommitted  from SOD_Item_Logs where ItemID = @Itemid and ReferenceID = @Orderid order by date desc )
	
	if @qty != 0
	begin
			if @exist > 0
			begin

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)(
			select i.ID,i.ItemLookupCode,''Order Update Logs'',n.orderid,x.QuantityOnOrder , i.Quantity ,i.Quantity,
			 @lastcommitted,
			i.QuantityCommitted as ''NewCommitted'' , GEtdate()

			from deleted n
			inner join inserted x on n.id = x.id
			inner join item i on n.ItemID = i.id
			)

			end
			else if @exist <= 0
			begin

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
			(
			select i.ID,i.ItemLookupCode,''Order Entry Logs'',n.orderid,n.QuantityOnOrder , i.Quantity ,i.Quantity,i.QuantityCommitted,i.QuantityCommitted + n.QuantityOnOrder as ''NewCommitted'' , GEtdate()
			from inserted n
			inner join item i on n.ItemID = i.id
			)
			end
	end
	
END
';
ELSE
    EXEC sys.sp_executesql N'
ALTER TRIGGER [dbo].[trg_OrderEntry_ItemLogs]
   ON [dbo].[OrderEntry]
   AFTER INSERT, UPDATE
AS BEGIN
    SET NOCOUNT ON;    

	declare @Qty integer
	declare @exist integer
	declare @Orderid integer
	declare @Itemid integer
	declare @lastcommitted integer

	set @Orderid = (select orderid from deleted)
	set @Qty = (select QuantityOnOrder from inserted)
	set @Itemid = (select itemid from deleted)
	set @exist = (select count(id) from [Order] where id = @Orderid)
	set @lastcommitted = (select top 1 NewCommitted  from SOD_Item_Logs where ItemID = @Itemid and ReferenceID = @Orderid order by date desc )
	
	if @qty != 0
	begin
			if @exist > 0
			begin

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)(
			select i.ID,i.ItemLookupCode,''Order Update Logs'',n.orderid,x.QuantityOnOrder , i.Quantity ,i.Quantity,
			 @lastcommitted,
			i.QuantityCommitted as ''NewCommitted'' , GEtdate()

			from deleted n
			inner join inserted x on n.id = x.id
			inner join item i on n.ItemID = i.id
			)

			end
			else if @exist <= 0
			begin

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
			(
			select i.ID,i.ItemLookupCode,''Order Entry Logs'',n.orderid,n.QuantityOnOrder , i.Quantity ,i.Quantity,i.QuantityCommitted,i.QuantityCommitted + n.QuantityOnOrder as ''NewCommitted'' , GEtdate()
			from inserted n
			inner join item i on n.ItemID = i.id
			)
			end
	end
	
END
';
GO
