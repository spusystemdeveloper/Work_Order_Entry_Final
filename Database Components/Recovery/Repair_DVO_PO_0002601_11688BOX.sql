/* One-line Davao stock recovery. Run the entire file in a dedicated SSMS session.
   Default is read-only review. See DVO_PO_0002601_11688BOX.md before any commit. */
SET NOCOUNT ON;
IF @@TRANCOUNT <> 0 OR (@@OPTIONS & 2) <> 0
BEGIN
    -- RAISERROR leaves a caller's existing transaction alone, even with XACT_ABORT ON.
    RAISERROR('Use a dedicated session with no open or implicit transaction.', 16, 1);
    RETURN;
END;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;

DECLARE @Commit bit = 0;
DECLARE @BackupPath nvarchar(4000) = N'';
DECLARE @ExpectedItemStamp binary(8) = 0x0000000028367257;
DECLARE @ExpectedPOStamp binary(8) = 0x0000000028367D47;
DECLARE @ExpectedLineStamp binary(8) = 0x0000000028367D48;
DECLARE @ExpectedSiblingStamp binary(8) = 0x0000000028367D49;

IF DB_NAME() <> N'DVO_STORE_DB' OR @@SERVERNAME <> N'WIN-L6UL7AVNSQ6'
    THROW 53260, 'Wrong target. This file is specific to live DVO_STORE_DB on WIN-L6UL7AVNSQ6.', 1;
IF @Commit IS NULL
    THROW 53261, 'Commit must be either 0 for review or 1 for apply.', 1;
IF @Commit = 1
BEGIN
    IF NULLIF(@BackupPath, N'') IS NULL OR NOT EXISTS
    (
        SELECT 1
        FROM msdb.dbo.backupset b
        JOIN msdb.dbo.backupmediafamily f ON f.media_set_id = b.media_set_id
        WHERE b.database_name = DB_NAME() AND b.server_name = @@SERVERNAME
          AND b.type = 'D' AND b.is_copy_only = 1 AND b.has_backup_checksums = 1
          AND b.is_damaged = 0 AND b.position = 1
          AND b.backup_finish_date >= CONVERT(date, GETDATE())
          AND f.physical_device_name = @BackupPath
    )
        THROW 53262, 'Supply a fresh copy-only full DVO_STORE_DB backup with checksums at a unique server path.', 1;
    RESTORE VERIFYONLY FROM DISK = @BackupPath WITH FILE = 1, CHECKSUM;
END;

-- BEGIN SHARED REPAIR BODY (used as-is by the disposable LocalDB test)
DECLARE @ItemID int = 16912, @POID int = 27784, @EntryID int = 97646;
DECLARE @StoreID int = 4001, @BatchNumber int = 2511, @Boxes int = 20;
DECLARE @ItemCode nvarchar(25) = N'11688BOX', @UnitCost money = 5.2836;
DECLARE @ReceiptAuditTime datetime = '2026-11-03T09:31:46.783';
DECLARE @ReceiptTime datetime = '2026-11-03T09:31:47';
DECLARE @PostedAt datetime, @MovementID int, @LockResult int;
DECLARE @ExpectedTriggers table
(
    TableName sysname, TriggerName sysname, DefinitionHash varchar(64)
);
INSERT @ExpectedTriggers VALUES
 (N'InventoryTransferLog', N'tr_ComputeQuantity', '65BCD6453B02FF1A592762BF5F09CEDB041CE6721BA2C37CD23E4B9AD3A5DFB2'),
 (N'InventoryTransferLog', N'tr_RMSTMP_InventoryMovement', '2846B73D620BF953E13F23E8DE257997AB71FAA8BDD67711AD95D63241DB48CB'),
 (N'InventoryTransferLog', N'trg_InventoryTransferLog_ItemLogs', '9E822C1C6804A9BF8C5DAE8760D46F972A55024682FFA057F3EF12C2D9DFD930'),
 (N'Item', N'tr_ItemDeleted', '651E6B225785096C1B56D1C46E2A34D65ABC95710A95B8027F0EE4BA40E337EC'),
 (N'Item', N'tr_ItemValueLog', '85A9AE1CACE36BB0922121CF9A3CFC9A8FF26D59FED31ADDD55E56539E655415'),
 (N'Item', N'trg_Item_VAT', '478BC7F51BF16481140AF0E6D213B479D308CF7E08C16949DF86E2B2A8A1AE0E'),
 (N'Item', N'trg_ItemLogs', '229596DE6CA65DA2BADC49EACA0744BB13729D1F8D98F209CB2950C298C8F329');

BEGIN TRY
    BEGIN TRANSACTION;
    IF @Commit = 1
    BEGIN
        EXEC @LockResult = sys.sp_getapplock
            @Resource = N'Recovery:DVO:PO27784:Entry97646',
            @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 5000;
        IF @LockResult < 0
            THROW 53263, 'Could not obtain the recovery lock.', 1;
    END;

    IF (SELECT COUNT(*) FROM sys.triggers WHERE parent_id IN
        (OBJECT_ID(N'dbo.Item'), OBJECT_ID(N'dbo.InventoryTransferLog'),
         OBJECT_ID(N'dbo.SOD_Item_Logs'), OBJECT_ID(N'dbo.RMSTMP_InventoryMovement'))) <> 7
        OR EXISTS
        (
            SELECT TableName, TriggerName, DefinitionHash FROM @ExpectedTriggers
            EXCEPT
            SELECT OBJECT_NAME(parent_id), name,
                   CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varbinary(max), OBJECT_DEFINITION(object_id))), 2)
            FROM sys.triggers
            WHERE is_disabled = 0 AND parent_id IN (OBJECT_ID(N'dbo.Item'), OBJECT_ID(N'dbo.InventoryTransferLog'))
        )
        THROW 53264, 'Inventory or audit triggers differ from the reviewed Davao definitions.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.PurchaseOrder WITH (UPDLOCK, HOLDLOCK)
        WHERE ID = @POID AND PONumber = N'0002601' AND POTitle = N'IIR#0002600_RRF#0147'
          AND POType = 2 AND Status = 2 AND StoreID = @StoreID AND BatchNumber = @BatchNumber
          AND DBTimeStamp = @ExpectedPOStamp
    )
        THROW 53265, 'PO 0002601 no longer matches the reviewed closed transfer-in.', 1;
    IF (SELECT COUNT(*) FROM dbo.PurchaseOrderEntry WITH (UPDLOCK, HOLDLOCK) WHERE PurchaseOrderID = @POID) <> 2
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.PurchaseOrderEntry WITH (UPDLOCK, HOLDLOCK)
            WHERE ID = @EntryID AND PurchaseOrderID = @POID AND ItemID = @ItemID AND StoreID = @StoreID
              AND QuantityOrdered = @Boxes AND QuantityReceivedToDate = @Boxes AND LastQuantityReceived = @Boxes
              AND QuantityReceived = 0 AND LastReceivedDate = @ReceiptTime AND Price = @UnitCost
              AND DBTimeStamp = @ExpectedLineStamp
        )
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.PurchaseOrderEntry WITH (UPDLOCK, HOLDLOCK)
            WHERE ID = 97647 AND PurchaseOrderID = @POID AND ItemID = 16915 AND StoreID = @StoreID
              AND QuantityOrdered = 50 AND QuantityReceivedToDate = 50 AND LastQuantityReceived = 50
              AND DBTimeStamp = @ExpectedSiblingStamp
        )
        THROW 53266, 'One or both receipt lines changed since the review.', 1;

    -- Lock this item and its movement range. Also reject a line reference on a different item.
    IF NOT EXISTS (SELECT 1 FROM dbo.Item WITH (UPDLOCK, HOLDLOCK) WHERE ID = @ItemID)
        THROW 53267, 'Item 16912 is missing.', 1;
    IF EXISTS
    (
        SELECT 1 FROM dbo.InventoryTransferLog WITH (UPDLOCK, HOLDLOCK)
        WHERE ItemID = @ItemID
          AND (ReferenceID = @POID OR DateTransferred >= @ReceiptAuditTime)
    )
        OR EXISTS (SELECT 1 FROM dbo.InventoryTransferLog WHERE ReferenceEntryID = @EntryID AND Type = 1)
        THROW 53268, 'A receipt or later native movement already exists. Reconcile it before any stock repair.', 1;
    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.Item WITH (UPDLOCK, HOLDLOCK)
        WHERE ID = @ItemID AND ItemLookupCode = @ItemCode AND ItemType = 0 AND Inactive = 0
          AND Quantity = 0 AND QuantityCommitted = 0 AND Cost = @UnitCost AND DBTimeStamp = @ExpectedItemStamp
    )
        THROW 53267, '11688BOX has changed since the review. Recheck stock and the item row.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.PurchaseOrder p
        JOIN dbo.PurchaseOrderEntry e ON e.PurchaseOrderID = p.ID
        JOIN dbo.InventoryTransferLog m ON m.ReferenceID = p.ID AND m.ReferenceEntryID = e.ID
        WHERE p.ID = 27782 AND p.PONumber = N'0002600' AND p.POType = 3 AND p.Status = 2 AND p.StoreID = @StoreID
          AND e.ID = 97643 AND e.ItemID = 16914 AND e.QuantityReceivedToDate = 240
          AND m.ID = 106711 AND m.ItemID = 16914 AND m.Quantity = -240 AND m.StoreID = @StoreID
    )
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.InventoryTransferLog
            WHERE ID = 106737 AND ReferenceID = @POID AND ReferenceEntryID = 97647
              AND ItemID = 16915 AND StoreID = @StoreID AND Quantity = 50 AND Type = 1 AND BatchNumber = @BatchNumber
        )
        THROW 53269, 'The original 240-piece deduction or the verified 50-box sibling movement is missing or changed.', 1;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.SOD_Item_Logs WHERE ItemID = @ItemID AND [Date] = @ReceiptAuditTime
          AND [Type] = 'Item Auto Logs' AND LastQuantity = 0 AND NewQuantity = 0
    )
        OR EXISTS
        (
            SELECT 1 FROM dbo.SOD_Item_Logs WHERE ItemID = @ItemID
              AND (([Date] >= @ReceiptAuditTime AND LastQuantity <> NewQuantity)
                   OR ([Type] = 'Transfer In Logs' AND ReferenceID IN ('27784', '0002601')))
        )
        OR EXISTS
        (
            SELECT 1 FROM dbo.RMSTMP_InventoryMovement
            WHERE ItemLookupCode = @ItemCode AND StoreID = @StoreID AND DateCreated >= @ReceiptAuditTime
        )
        THROW 53270, 'The item audit or integration movement history no longer supports a missing receipt.', 1;

    IF (SELECT COUNT(*) FROM dbo.RMSTMP_TransferInventory
        WHERE ReferenceNumber = N'0002601' AND ItemLookupCode = @ItemCode) <> 1
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.RMSTMP_TransferInventory WITH (UPDLOCK, HOLDLOCK)
            WHERE ID = 7166 AND ReferenceNumber = N'0002601' AND ItemLookupCode = @ItemCode
              AND FromStore = 0 AND ToStore = @StoreID AND Quantity = @Boxes AND Synched = 0
              AND DateCreated = '2026-11-03T10:31:45.610'
        )
        THROW 53271, 'The pending transfer document changed. Reconcile integration state before proceeding.', 1;

    IF @Commit = 0
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'REVIEW ONLY' AS Result, '0002601' AS PONumber, @EntryID AS EntryID,
               @ItemCode AS ItemLookupCode, 0 AS StockBefore, @Boxes AS StockAfter,
               @Boxes AS ProposedNativeMovement, @UnitCost AS MovementUnitCost;
        RETURN;
    END;

    SET @PostedAt = GETDATE();
    UPDATE dbo.Item
    SET Quantity = Quantity + @Boxes, LastUpdated = @PostedAt
    WHERE ID = @ItemID AND DBTimeStamp = @ExpectedItemStamp;
    IF @@ROWCOUNT <> 1
        THROW 53272, 'Expected to update exactly one item.', 1;

    -- Type 1 is the native PO/transfer movement. The PO itself has type 2 (transfer-in).
    -- Use the actual repair time, the original PO batch and system cashier 0.
    INSERT dbo.InventoryTransferLog
      (ItemID, DetailID, Quantity, DateTransferred, StoreID, ReferenceID, ReasonCodeID,
       CashierID, Type, ReferenceEntryID, Cost, BatchNumber, ComputedQuantity)
    VALUES
      (@ItemID, 0, @Boxes, @PostedAt, @StoreID, @POID, 0,
       0, 1, @EntryID, @UnitCost, @BatchNumber, @Boxes);
    SET @MovementID = CONVERT(int, SCOPE_IDENTITY());

    IF XACT_STATE() <> 1 OR @@TRANCOUNT <> 1
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.Item WHERE ID = @ItemID AND Quantity = @Boxes
              AND QuantityCommitted = 0 AND Cost = @UnitCost AND LastUpdated = @PostedAt
        )
        OR NOT EXISTS
        (
            SELECT 1 FROM dbo.InventoryTransferLog WHERE ID = @MovementID AND ItemID = @ItemID
              AND ReferenceID = @POID AND ReferenceEntryID = @EntryID AND Quantity = @Boxes
              AND ComputedQuantity = @Boxes AND DateTransferred = @PostedAt AND StoreID = @StoreID
              AND Type = 1 AND BatchNumber = @BatchNumber AND Cost = @UnitCost
        )
        OR (SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ItemID = @ItemID AND [Date] >= @PostedAt
            AND [Type] = 'Item Auto Logs' AND ReferenceID = '-' AND QuantityORIS = -@Boxes
            AND LastQuantity = 0 AND NewQuantity = @Boxes AND LastCommitted = 0 AND NewCommitted = 0) <> 1
        OR (SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ItemID = @ItemID AND [Date] >= @PostedAt
            AND [Type] = 'Transfer In Logs' AND ReferenceID = '27784' AND QuantityORIS = @Boxes
            AND LastQuantity = 0 AND NewQuantity = @Boxes AND LastCommitted = 0 AND NewCommitted = 0) <> 1
        OR (SELECT COUNT(*) FROM dbo.RMSTMP_InventoryMovement WHERE ItemLookupCode = @ItemCode
            AND StoreID = @StoreID AND Quantity = @Boxes AND DateCreated = @PostedAt AND Synched = 0) <> 1
        THROW 53273, 'Posting verification failed. The item, movement and audit writes must all succeed together.', 1;

    COMMIT TRANSACTION;
    SELECT 'COMMITTED' AS Result, '0002601' AS PONumber, @EntryID AS EntryID, @ItemCode AS ItemLookupCode,
           @MovementID AS InventoryTransferLogID, @PostedAt AS PostedAt, @Boxes AS StockAfter;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
-- END SHARED REPAIR BODY
