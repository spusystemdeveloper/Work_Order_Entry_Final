/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[trg_Transaction_closeQueue]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[trg_Transaction_closeQueue]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TR'
    THROW 52001, 'Object type conflict for [dbo].[trg_Transaction_closeQueue].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE TRIGGER [dbo].[trg_Transaction_closeQueue]
   ON [dbo].[Transaction]
   AFTER INSERT
AS BEGIN
    SET NOCOUNT ON;    

	
	declare @exist integer
	declare @recallid integer
	
	set @recallid = (select RecallID from inserted)
	set @exist = (select count(*) from Queueing q where q.OrderID = @recallid )

	if @exist > 0
	begin
	 update Queueing set Status = 1 where OrderID = @recallid
	end

END

';
ELSE
    EXEC sys.sp_executesql N'
ALTER TRIGGER [dbo].[trg_Transaction_closeQueue]
   ON [dbo].[Transaction]
   AFTER INSERT
AS BEGIN
    SET NOCOUNT ON;    

	
	declare @exist integer
	declare @recallid integer
	
	set @recallid = (select RecallID from inserted)
	set @exist = (select count(*) from Queueing q where q.OrderID = @recallid )

	if @exist > 0
	begin
	 update Queueing set Status = 1 where OrderID = @recallid
	end

END

';
GO
