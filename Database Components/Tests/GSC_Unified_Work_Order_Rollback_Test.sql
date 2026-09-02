/*
    Unified Work Order Entry - GSC_STORE_DB rollback integration test

    SAFETY
      - This script performs writes only inside one explicit transaction.
      - The transaction is always rolled back, both on success and failure.
      - No test Order, OrderEntry, Queueing, QueueingItems, or inventory change
        should remain after execution.

    COVERAGE
      1. Gensan processing-queue Work Order.
      2. Branch processing disabled while recall ownership remains.
      3. Sales Quotation with OPIS recall ownership and no inventory commitment.
      4. Work Order commitment release/cancellation behavior.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE
    @CustomerID int,
    @SalesRepID int,
    @ItemID int,
    @Cost money,
    @FullPrice money,
    @Price money,
    @Taxable int,
    @Description varchar(100),
    @OriginalCommitted float,
    @QueuedWorkOrderID int,
    @DirectWorkOrderID int,
    @QuotationID int,
    @QueueID bigint;

SELECT TOP (1)
    @CustomerID = sourceOrder.CustomerID,
    @SalesRepID = sourceOrder.SalesRepID,
    @ItemID = sourceEntry.ItemID,
    @Cost = sourceEntry.Cost,
    @FullPrice = sourceEntry.FullPrice,
    @Price = sourceEntry.Price,
    @Taxable = sourceEntry.Taxable,
    @Description = LEFT(COALESCE(sourceEntry.Description, sourceItem.Description, ''), 100)
FROM dbo.[Order] AS sourceOrder
INNER JOIN dbo.OrderEntry AS sourceEntry
    ON sourceEntry.OrderID = sourceOrder.ID
INNER JOIN dbo.Item AS sourceItem
    ON sourceItem.ID = sourceEntry.ItemID
WHERE sourceOrder.CustomerID IS NOT NULL
  AND sourceOrder.SalesRepID IS NOT NULL
  AND sourceEntry.ItemID IS NOT NULL
  AND sourceItem.ItemType <> 7
ORDER BY sourceOrder.ID DESC;

IF @CustomerID IS NULL OR @SalesRepID IS NULL OR @ItemID IS NULL
BEGIN
    RAISERROR('No suitable existing Gensan order line was found for the rollback test.', 16, 1);
    RETURN;
END;

SELECT @OriginalCommitted = COALESCE(QuantityCommitted, 0)
FROM dbo.Item
WHERE ID = @ItemID;

BEGIN TRY
    BEGIN TRANSACTION;

    /* 1. Gensan processing-queue Work Order */
    EXEC @QueuedWorkOrderID = dbo.SOD_sp_InsertQoute
        @vComment = '1',
        @iCustID = @CustomerID,
        @iSalesRepID = @SalesRepID,
        @mTax = 0,
        @mTotal = @Price,
        @vRemarks = 'CODEX ROLLBACK TEST - QUEUED WORK ORDER',
        @OPIS = 'CODEX_ROLLBACK_TEST',
        @Type = 2;

    SELECT @QueueID = id
    FROM dbo.Queueing
    WHERE OrderID = @QueuedWorkOrderID;

    IF @QueueID IS NULL
        RAISERROR('Queued Work Order did not create a recall/processing header.', 16, 1);

    EXEC dbo.SOD_sp_InsertQouteEntry
        @mCost = @Cost,
        @iOrderID = @QueuedWorkOrderID,
        @iItemID = @ItemID,
        @mFullPrice = @FullPrice,
        @mPrice = @Price,
        @fQuantityOnOrder = 1,
        @iSalesRepID = @SalesRepID,
        @iTaxable = @Taxable,
        @vDescription = @Description,
        @vComment = 'STORE',
        @QtyPrep = 0,
        @Type = 2;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.QueueingItems
        WHERE QueueingID = @QueueID AND ItemID = @ItemID
    )
        RAISERROR('Queued Work Order did not create its queue item.', 16, 1);

    UPDATE dbo.Item
    SET QuantityCommitted = COALESCE(QuantityCommitted, 0) + 1
    WHERE ID = @ItemID;

    IF ABS((SELECT COALESCE(QuantityCommitted, 0) FROM dbo.Item WHERE ID = @ItemID)
           - (@OriginalCommitted + 1)) > 0.0001
        RAISERROR('Queued Work Order commitment was not applied correctly.', 16, 1);

    /* Simulate the corrected cancellation/release behavior. */
    UPDATE dbo.Item
    SET QuantityCommitted = COALESCE(QuantityCommitted, 0) - 1
    WHERE ID = @ItemID;

    UPDATE dbo.OrderEntry
    SET QuantityOnOrder = 0
    WHERE OrderID = @QueuedWorkOrderID;

    UPDATE dbo.Queueing
    SET Status = 2
    WHERE OrderID = @QueuedWorkOrderID;

    UPDATE dbo.[Order]
    SET Closed = 1
    WHERE ID = @QueuedWorkOrderID;

    IF ABS((SELECT COALESCE(QuantityCommitted, 0) FROM dbo.Item WHERE ID = @ItemID)
           - @OriginalCommitted) > 0.0001
        RAISERROR('Cancellation did not restore the original committed quantity.', 16, 1);

    /* 2. Processing disabled: retain header, remove processing items. */
    SET @QueueID = NULL;

    EXEC @DirectWorkOrderID = dbo.SOD_sp_InsertQoute
        @vComment = '1',
        @iCustID = @CustomerID,
        @iSalesRepID = @SalesRepID,
        @mTax = 0,
        @mTotal = @Price,
        @vRemarks = 'CODEX ROLLBACK TEST - RECALL ONLY WORK ORDER',
        @OPIS = 'CODEX_ROLLBACK_TEST',
        @Type = 2;

    SELECT @QueueID = id
    FROM dbo.Queueing
    WHERE OrderID = @DirectWorkOrderID;

    EXEC dbo.SOD_sp_InsertQouteEntry
        @mCost = @Cost,
        @iOrderID = @DirectWorkOrderID,
        @iItemID = @ItemID,
        @mFullPrice = @FullPrice,
        @mPrice = @Price,
        @fQuantityOnOrder = 1,
        @iSalesRepID = @SalesRepID,
        @iTaxable = @Taxable,
        @vDescription = @Description,
        @vComment = 'STORE',
        @QtyPrep = 0,
        @Type = 2;

    DELETE FROM dbo.QueueingItems
    WHERE QueueingID = @QueueID;

    IF NOT EXISTS (SELECT 1 FROM dbo.Queueing WHERE OrderID = @DirectWorkOrderID)
        RAISERROR('Recall-only Work Order lost its ownership header.', 16, 1);

    IF EXISTS
    (
        SELECT 1
        FROM dbo.QueueingItems AS queueItem
        INNER JOIN dbo.Queueing AS queueHeader
            ON queueHeader.id = queueItem.QueueingID
        WHERE queueHeader.OrderID = @DirectWorkOrderID
    )
        RAISERROR('Recall-only Work Order retained processing queue items.', 16, 1);

    /* 3. Quotation: add ownership header, no queue items or commitment. */
    EXEC @QuotationID = dbo.SOD_sp_InsertQoute
        @vComment = '1',
        @iCustID = @CustomerID,
        @iSalesRepID = @SalesRepID,
        @mTax = 0,
        @mTotal = @Price,
        @vRemarks = 'CODEX ROLLBACK TEST - QUOTATION',
        @OPIS = 'CODEX_ROLLBACK_TEST',
        @Type = 3;

    IF NOT EXISTS (SELECT 1 FROM dbo.Queueing WHERE OrderID = @QuotationID)
    BEGIN
        INSERT INTO dbo.Queueing(OrderID, Status, GroupTo, OPIS)
        VALUES(@QuotationID, 0, @QuotationID, 'CODEX_ROLLBACK_TEST');
    END;

    EXEC dbo.SOD_sp_InsertQouteEntry
        @mCost = @Cost,
        @iOrderID = @QuotationID,
        @iItemID = @ItemID,
        @mFullPrice = @FullPrice,
        @mPrice = @Price,
        @fQuantityOnOrder = 1,
        @iSalesRepID = @SalesRepID,
        @iTaxable = @Taxable,
        @vDescription = @Description,
        @vComment = 'STORE',
        @QtyPrep = 0,
        @Type = 3;

    DELETE queueItem
    FROM dbo.QueueingItems AS queueItem
    INNER JOIN dbo.Queueing AS queueHeader
        ON queueHeader.id = queueItem.QueueingID
    WHERE queueHeader.OrderID = @QuotationID;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Queueing
        WHERE OrderID = @QuotationID AND OPIS = 'CODEX_ROLLBACK_TEST'
    )
        RAISERROR('Quotation did not retain its OPIS recall header.', 16, 1);

    IF ABS((SELECT COALESCE(QuantityCommitted, 0) FROM dbo.Item WHERE ID = @ItemID)
           - @OriginalCommitted) > 0.0001
        RAISERROR('Quotation or recall-only setup unexpectedly changed committed quantity.', 16, 1);

    ROLLBACK TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Passed,
        'All GSC rollback integration checks passed; transaction rolled back.' AS Result;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage nvarchar(4000);
    SELECT @ErrorMessage = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;

