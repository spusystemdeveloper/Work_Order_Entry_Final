USE [QUEUEING_DB]
GO

/****** Object:  Trigger [dbo].[trg_TransactionEntry_ItemLogs]    Script Date: 12/1/2020 11:21:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE TRIGGER [dbo].[trg_TransactionEntry_ItemLogs]
   ON [dbo].[TransactionEntry]
   AFTER INSERT
AS BEGIN
    SET NOCOUNT ON;    

	declare @type int
	set @type = (select t.RecallType from inserted i inner join [transaction] t on i.TransactionNumber = t.TransactionNumber )

	if @type = 1
	begin
		insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
		(

		select i.ID,i.ItemLookupCode,'CM Transaction Logs',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 3
	begin
		insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
		(

		select i.ID,i.ItemLookupCode,'Void Transaction Logs',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 4
	begin
	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(

	select i.ID,i.ItemLookupCode,'Sales Transaction Logs',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
	from inserted n
	inner join item i on n.ItemID = i.id
	)
   end
END
GO

ALTER TABLE [dbo].[TransactionEntry] ENABLE TRIGGER [trg_TransactionEntry_ItemLogs]
GO


