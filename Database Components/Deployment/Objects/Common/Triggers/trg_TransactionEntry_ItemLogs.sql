/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[trg_TransactionEntry_ItemLogs]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_TransactionEntry_ItemLogs]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TR'
    THROW 52001, 'Object type conflict for [dbo].[trg_TransactionEntry_ItemLogs].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'

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

		select i.ID,i.ItemLookupCode,''CM Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 3
	begin
		insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
		(

		select i.ID,i.ItemLookupCode,''Void Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 4 or @type = 0
	begin
	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(

	select i.ID,i.ItemLookupCode,''Sales Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
	from inserted n
	inner join item i on n.ItemID = i.id
	)
   end
END
';
ELSE
    EXEC sys.sp_executesql N'

ALTER TRIGGER [dbo].[trg_TransactionEntry_ItemLogs]
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

		select i.ID,i.ItemLookupCode,''CM Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 3
	begin
		insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
		(

		select i.ID,i.ItemLookupCode,''Void Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
		from inserted n
		inner join item i on n.ItemID = i.id
		)
	end
	else if  @type = 4 or @type = 0
	begin
	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(

	select i.ID,i.ItemLookupCode,''Sales Transaction Logs'',n.TransactionNumber ,n.Quantity , i.Quantity ,i.Quantity - n.Quantity ,i.QuantityCommitted,i.QuantityCommitted, GEtdate()
	from inserted n
	inner join item i on n.ItemID = i.id
	)
   end
END
';
GO
