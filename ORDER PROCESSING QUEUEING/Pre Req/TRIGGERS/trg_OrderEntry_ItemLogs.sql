USE [QUEUEING_DB]
GO

/****** Object:  Trigger [dbo].[trg_OrderEntry_ItemLogs]    Script Date: 12/1/2020 11:19:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


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
			select i.ID,i.ItemLookupCode,'Order Update Logs',n.orderid,x.QuantityOnOrder , i.Quantity ,i.Quantity,
			 @lastcommitted,
			i.QuantityCommitted as 'NewCommitted' , GEtdate()

			from deleted n
			inner join inserted x on n.id = x.id
			inner join item i on n.ItemID = i.id
			)

			end
			else if @exist <= 0
			begin

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
			(
			select i.ID,i.ItemLookupCode,'Order Entry Logs',n.orderid,n.QuantityOnOrder , i.Quantity ,i.Quantity,i.QuantityCommitted,i.QuantityCommitted + n.QuantityOnOrder as 'NewCommitted' , GEtdate()
			from inserted n
			inner join item i on n.ItemID = i.id
			)
			end
	end
	
END
GO

ALTER TABLE [dbo].[OrderEntry] ENABLE TRIGGER [trg_OrderEntry_ItemLogs]
GO


