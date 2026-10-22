USE [QUEUEING_DB]
GO

/****** Object:  Trigger [dbo].[trg_InventoryTransferLog_ItemLogs]    Script Date: 12/1/2020 11:19:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE TRIGGER [dbo].[trg_InventoryTransferLog_ItemLogs]
   ON [dbo].[InventoryTransferLog]
   AFTER INSERT
AS BEGIN
    SET NOCOUNT ON;    

		declare @status int
		declare @type int
		declare @ID int
		declare @Ponumber int

		set @ID = (Select top 1 i.ReferenceID  from Inserted i)
		set @type = (Select top 1 p.POType from Inserted i inner join PurchaseOrder p on i.ReferenceID = p.id where p.id = @ID )
		set @Ponumber = (Select top 1 p.id from Inserted i inner join PurchaseOrder p on i.ReferenceID = p.id where p.id = @ID )

			insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
			(

			select i.ID,i.ItemLookupCode,
			case
				when @type = 0 then 'Purchase Order Logs'
				when @type = 2 then 'Transfer In Logs'
				when @type = 3 then 'Transfer Out Logs'
				else '-'
				end,@Ponumber ,n.Quantity , 
				case
				when @type = 0 then i.Quantity - n.Quantity    -- PurchaseOrder
				when @type = 2 then i.Quantity - n.Quantity     -- Transfer In
				when @type = 3 then i.Quantity + (n.Quantity * -1)   -- Transfer Out
				else 0
				end ,
				case
				when @type = 0 then i.Quantity   -- PurchaseOrder
				when @type = 2 then i.Quantity   -- Transfer In
				when @type = 3 then (i.Quantity - n.Quantity) + n.Quantity   -- Transfer Out
				else 0
				end
				,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
			from inserted n
			inner join item i on n.ItemID = i.id
			)
   
		

END
GO

ALTER TABLE [dbo].[InventoryTransferLog] ENABLE TRIGGER [trg_InventoryTransferLog_ItemLogs]
GO


