/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[trg_ItemLogs]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_ItemLogs]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TR'
    THROW 52001, 'Object type conflict for [dbo].[trg_ItemLogs].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE TRIGGER [dbo].[trg_ItemLogs]
   ON [dbo].[Item]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;    

	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(
	select i.ID,i.ItemLookupCode,''Item Auto Logs'',''-'',d.Quantity - n.Quantity,d.Quantity,n.Quantity ,d.QuantityCommitted,n.QuantityCommitted , GEtdate()
	from item i
	inner join inserted n on i.id = n.id
	inner join deleted d on i.id = d.id
	)
   
END
';
ELSE
    EXEC sys.sp_executesql N'
ALTER TRIGGER [dbo].[trg_ItemLogs]
   ON [dbo].[Item]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;    

	insert into SOD_Item_Logs(ItemID,ItemLookupCode,[Type],ReferenceID ,QuantityORIS ,LastQuantity,NewQuantity,LastCommitted,NewCommitted,Date)
	(
	select i.ID,i.ItemLookupCode,''Item Auto Logs'',''-'',d.Quantity - n.Quantity,d.Quantity,n.Quantity ,d.QuantityCommitted,n.QuantityCommitted , GEtdate()
	from item i
	inner join inserted n on i.id = n.id
	inner join deleted d on i.id = d.id
	)
   
END
';
GO
