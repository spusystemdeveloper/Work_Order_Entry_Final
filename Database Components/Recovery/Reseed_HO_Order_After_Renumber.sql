/* Run personally in SSMS ONLY AFTER the renumber file reports COMMITTED.
   Keep ALL HO RMS/add-in/queue/integration clients closed until finished.
   Target: live HO_PO_DB on WIN-2847UGJBAV4 (172.16.4.50).
   Sets current Order identity to 18870; next allocation is 18871.
   Changes no existing Order IDs, transaction links, amounts or quantities.
   Standalone file: NOT a Database Updater migration. Run the WHOLE file.
   No additional backup-path setting is needed here: the prerequisite renumber
   already required and verified the maintenance backup. */
USE [HO_PO_DB];
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';
IF @@TRANCOUNT<>0
    THROW 52820,'Close any existing transaction and run this entire file in a fresh SSMS query.',1;
IF DB_NAME()<>N'HO_PO_DB' OR @@SERVERNAME<>@ExpectedServer
    THROW 52821,'Wrong server/database. Identity was not reseeded.',1;

DECLARE @max int,@current decimal(38,0),@lock int;
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC @lock=sys.sp_getapplock @Resource=N'HO_WorkOrder_Renumber_18870_18879',
        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000;
    IF @lock<0 THROW 52822,'Another renumber/reseed is running.',1;
    -- Hold the guard and reseed together, including against ordinary RMS writers.
    SELECT @max=MAX(ID) FROM dbo.[Order] WITH(TABLOCKX,HOLDLOCK);
    SET @current=IDENT_CURRENT(N'dbo.Order');
    IF ISNULL(@max,0)<>18870 OR @current IS NULL OR @current NOT IN(18879,18870)
        THROW 52823,'Maximum Order ID must be 18870 and identity must still be 18879 or already 18870. Check committed renumber results/new activity; do not rewind blindly.',1;
    IF COLUMNPROPERTY(OBJECT_ID(N'dbo.[Order]'),N'ID','IsIdentity')<>1
       OR CONVERT(bigint,IDENT_INCR(N'dbo.Order'))<>1
        THROW 52824,'Unexpected Order identity definition. Stop for review.',1;
    IF NOT EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID=18869 AND StoreID=606 AND Type=2
          AND Closed=1 AND [Time]=CONVERT(datetime,'2026-10-14T13:50:33.653',126)
          AND CustomerID=71 AND Total=30200.54 AND Tax=0)
       OR NOT EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID=18870 AND StoreID=606 AND Type=2
          AND Closed=0 AND [Time]=CONVERT(datetime,'2026-10-15T09:40:07.430',126)
          AND CustomerID=71 AND Total=11042.76 AND Tax=0)
        THROW 52825,'Renamed headers do not match the expected pair. Identity was not reseeded.',1;
    IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK)
          WHERE StoreID=606 AND TransactionNumber=23266 AND RecallID=18869 AND RecallType=4 AND Total=30200.54)
       OR NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WITH(UPDLOCK,HOLDLOCK)
          WHERE ID=19857 AND StoreID=606 AND OrderID=18869 AND TransactionNumber=23266)
       OR NOT EXISTS(SELECT 1 FROM dbo.Journal WITH(UPDLOCK,HOLDLOCK)
          WHERE ID=23804 AND StoreID=606 AND ReferenceID=18869 AND TransactionType=2 AND OrderHistoryID=19857)
        THROW 52826,'Renumbered transaction/history/receipt links do not match. Identity was not reseeded.',1;
    IF NOT EXISTS(SELECT 1 FROM dbo.SOD_WO_History WITH(UPDLOCK,HOLDLOCK)
          WHERE OrderID=18869 AND EventName=N'Order ID changed'
            AND Details=N'Manual renumber: old Order ID 18870 -> 18869. Transaction 23266 unchanged. Original receipt/free text retained.')
       OR NOT EXISTS(SELECT 1 FROM dbo.SOD_WO_History WITH(UPDLOCK,HOLDLOCK)
          WHERE OrderID=18870 AND EventName=N'Order ID changed'
            AND Details=N'Manual renumber: old Order ID 18879 -> 18870. No quantity, amount or inventory change.')
        THROW 52827,'Committed renumber mapping events were not found. Run this only after the renumber COMMITTED result.',1;
    IF @current<>18870
        DBCC CHECKIDENT (N'dbo.[Order]',RESEED,18870) WITH NO_INFOMSGS;
    COMMIT;
    SELECT CASE WHEN @current=18870 THEN N'ALREADY SET: no identity change needed.'
           ELSE N'COMMITTED: Order identity reseeded.' END Result,
           @current PreviousIdentity,18870 CurrentIdentity,18871 NextAllocatedOrderID;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
