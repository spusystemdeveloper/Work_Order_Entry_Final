USE [QUEUEING_DB]
GO

/****** Object:  Trigger [dbo].[trg_ItemLogs]    Script Date: 12/1/2020 11:18:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[trg_ItemLogs]
   ON [dbo].[Item]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;    

	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(
	select i.ID,i.ItemLookupCode,'Item Auto Logs','-',d.Quantity - n.Quantity,d.Quantity,n.Quantity ,d.QuantityCommitted,n.QuantityCommitted , GEtdate()
	from item i
	inner join inserted n on i.id = n.id
	inner join deleted d on i.id = d.id
	)
   
END
GO

ALTER TABLE [dbo].[Item] ENABLE TRIGGER [trg_ItemLogs]
GO


