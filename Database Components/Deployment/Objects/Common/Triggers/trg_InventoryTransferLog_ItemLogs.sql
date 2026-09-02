/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[trg_InventoryTransferLog_ItemLogs]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_InventoryTransferLog_ItemLogs]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TR'
    THROW 52001, 'Object type conflict for [dbo].[trg_InventoryTransferLog_ItemLogs].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE TRIGGER [dbo].[trg_InventoryTransferLog_ItemLogs]
   ON [dbo].[InventoryTransferLog]
   AFTER INSERT
AS BEGIN
    --SET NOCOUNT ON;    

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
				when @type = 0 then ''Purchase Order Logs''
				when @type = 2 then ''Transfer In Logs''
				when @type = 3 then ''Transfer Out Logs''
				else ''-''
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
';
ELSE
    EXEC sys.sp_executesql N'
ALTER TRIGGER [dbo].[trg_InventoryTransferLog_ItemLogs]
   ON [dbo].[InventoryTransferLog]
   AFTER INSERT
AS BEGIN
    --SET NOCOUNT ON;    

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
				when @type = 0 then ''Purchase Order Logs''
				when @type = 2 then ''Transfer In Logs''
				when @type = 3 then ''Transfer Out Logs''
				else ''-''
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
';
GO
