/*
  Optional Work Order history, SQL Server 2016 (including RTM).
  Run in a disposable branch restore first. This creates one custom table
  and additive, set-based audit triggers; it does not add RMS table columns
  or replace the existing queue-close trigger. No historical data is backfilled.
  Deploy using sqlcmd -b or stop on the first error in SSMS.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.[Order]', N'U') IS NULL OR
   OBJECT_ID(N'dbo.OrderEntry', N'U') IS NULL OR
   OBJECT_ID(N'dbo.Queueing', N'U') IS NULL OR
   OBJECT_ID(N'dbo.QueueingItems', N'U') IS NULL OR
   OBJECT_ID(N'dbo.[Transaction]', N'U') IS NULL
    THROW 52100, 'Required RMS and Queueing tables are missing.', 1;

IF OBJECT_ID(N'dbo.SOD_WO_History') IS NOT NULL AND
   OBJECT_ID(N'dbo.SOD_WO_History', N'U') IS NULL
    THROW 52101, 'SOD_WO_History exists with an incompatible object type.', 1;

-- RMS maps these fields as nvarchar. Legacy LOBs cannot be read from the
-- inserted/deleted tables of AFTER triggers, even when wrapped in CONVERT.
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id IN (OBJECT_ID(N'dbo.[Order]'), OBJECT_ID(N'dbo.OrderEntry'))
             AND name IN (N'Comment', N'Description') AND system_type_id IN (34, 35, 99))
    THROW 52103, 'Order/entry text fields use unsupported legacy LOB types. Review the branch schema before enabling history.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.SOD_WO_History', N'U') IS NULL
        CREATE TABLE dbo.SOD_WO_History (
            EventID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SOD_WO_History PRIMARY KEY,
            OrderID int NOT NULL,
            EventTime datetime2(3) NOT NULL CONSTRAINT DF_SOD_WO_History_Time DEFAULT SYSDATETIME(),
            EventName nvarchar(60) NOT NULL,
            Actor nvarchar(128) NOT NULL,
            Terminal nvarchar(256) NOT NULL,
            ItemID int NULL,
            Details nvarchar(2000) NOT NULL
        );

    -- Reject incompatible existing schemas rather than altering business data.
    IF EXISTS (
        SELECT expected.name, expected.type_name, expected.max_length, expected.is_nullable
        FROM (VALUES
            (N'EventID', N'bigint', CONVERT(smallint, 8), CONVERT(bit, 0)),
            (N'OrderID', N'int', 4, 0),
            (N'EventTime', N'datetime2', 7, 0),
            (N'EventName', N'nvarchar', 120, 0),
            (N'Actor', N'nvarchar', 256, 0),
            (N'Terminal', N'nvarchar', 512, 0),
            (N'ItemID', N'int', 4, 1),
            (N'Details', N'nvarchar', 4000, 0)
        ) expected(name, type_name, max_length, is_nullable)
        EXCEPT
        SELECT c.name, TYPE_NAME(c.user_type_id), c.max_length, c.is_nullable
        FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'dbo.SOD_WO_History')
    ) OR COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'), N'EventID', 'IsIdentity') <> 1
        THROW 52102, 'SOD_WO_History schema is incompatible. Review the existing table before deployment.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.SOD_WO_History')
                   AND name = N'IX_SOD_WO_History_Order')
        CREATE INDEX IX_SOD_WO_History_Order ON dbo.SOD_WO_History(OrderID, EventID DESC);

    DECLARE @actor nvarchar(max) = N'COALESCE(NULLIF(CONVERT(nvarchar(128), SESSION_CONTEXT(N''SOD_WO_Actor'')), N''''), ORIGINAL_LOGIN())';
    DECLARE @terminal nvarchar(max) = N'LEFT(CONCAT(HOST_NAME(), N'' / '', CONVERT(nvarchar(128), SESSION_CONTEXT(N''SOD_WO_Register''))), 256)';
    DECLARE @ddl nvarchar(max);

    SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Order ON dbo.[Order] AFTER INSERT, UPDATE, DELETE AS
    BEGIN
        SET NOCOUNT ON;
        INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, Details)
        SELECT COALESCE(i.ID, d.ID),
            CASE WHEN d.ID IS NULL THEN N''Order created'' WHEN i.ID IS NULL THEN N''Order removed''
                 WHEN i.Closed = 1 AND d.Closed = 0 THEN N''Order closed''
                 WHEN i.Type <> d.Type THEN N''Order type changed'' ELSE N''Order edited'' END,
            ' + @actor + N', ' + @terminal + N',
            LEFT(CONCAT(N''Type: '', d.Type, N'' -> '', i.Type,
                N''; customer: '', d.CustomerID, N'' -> '', i.CustomerID,
                N''; sales rep: '', d.SalesRepID, N'' -> '', i.SalesRepID,
                N''; total: '', d.Total, N'' -> '', i.Total,
                N''; tax: '', d.Tax, N'' -> '', i.Tax,
                N''; tax reason: '', d.DefaultTaxChangeReasonCodeID, N'' -> '', i.DefaultTaxChangeReasonCodeID,
                N''; remarks: '', LEFT(CONVERT(nvarchar(max), d.Comment), 300),
                N'' -> '', LEFT(CONVERT(nvarchar(max), i.Comment), 300)), 2000)
        FROM inserted i FULL JOIN deleted d ON i.ID = d.ID
        WHERE i.ID IS NULL OR d.ID IS NULL OR EXISTS (
            SELECT i.Type, i.Closed, i.CustomerID, i.SalesRepID, i.Total, i.Tax,
                   i.DefaultTaxChangeReasonCodeID, CONVERT(nvarchar(max), i.Comment)
            EXCEPT SELECT d.Type, d.Closed, d.CustomerID, d.SalesRepID, d.Total, d.Tax,
                   d.DefaultTaxChangeReasonCodeID, CONVERT(nvarchar(max), d.Comment));
    END';
    IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Order', N'TR') IS NOT NULL
        SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
    EXEC sys.sp_executesql @ddl;

    SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Entry ON dbo.OrderEntry AFTER INSERT, UPDATE, DELETE AS
    BEGIN
        SET NOCOUNT ON;
        INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, ItemID, Details)
        SELECT COALESCE(i.OrderID, d.OrderID),
            CASE WHEN d.ID IS NULL THEN N''Item added'' WHEN i.ID IS NULL THEN N''Item removed'' ELSE N''Item edited'' END,
            ' + @actor + N', ' + @terminal + N', COALESCE(i.ItemID, d.ItemID),
            LEFT(CONCAT(N''Entry: '', COALESCE(i.ID, d.ID), N''; quantity: '', d.QuantityOnOrder,
                N'' -> '', i.QuantityOnOrder, N''; price: '', d.Price, N'' -> '', i.Price,
                N''; full price: '', d.FullPrice, N'' -> '', i.FullPrice,
                N''; taxable: '', d.Taxable, N'' -> '', i.Taxable,
                N''; tax reason: '', d.TaxChangeReasonCodeID, N'' -> '', i.TaxChangeReasonCodeID,
                N''; description: '', LEFT(CONVERT(nvarchar(max), d.Description), 200),
                N'' -> '', LEFT(CONVERT(nvarchar(max), i.Description), 200),
                N''; pick location: '', LEFT(CONVERT(nvarchar(max), d.Comment), 200),
                N'' -> '', LEFT(CONVERT(nvarchar(max), i.Comment), 200)), 2000)
        FROM inserted i FULL JOIN deleted d ON i.ID = d.ID
        WHERE i.ID IS NULL OR d.ID IS NULL OR EXISTS (
            SELECT i.OrderID, i.ItemID, i.QuantityOnOrder, i.Price, i.FullPrice, i.Taxable,
                   i.TaxChangeReasonCodeID, CONVERT(nvarchar(max), i.Description), CONVERT(nvarchar(max), i.Comment)
            EXCEPT SELECT d.OrderID, d.ItemID, d.QuantityOnOrder, d.Price, d.FullPrice, d.Taxable,
                   d.TaxChangeReasonCodeID, CONVERT(nvarchar(max), d.Description), CONVERT(nvarchar(max), d.Comment));
    END';
    IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Entry', N'TR') IS NOT NULL
        SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
    EXEC sys.sp_executesql @ddl;

    SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Queue ON dbo.Queueing AFTER INSERT, UPDATE, DELETE AS
    BEGIN
        SET NOCOUNT ON;
        INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, Details)
        SELECT COALESCE(i.OrderID, d.OrderID),
            CASE WHEN d.id IS NULL THEN N''Ownership recorded'' WHEN i.id IS NULL THEN N''Queue removed''
                 WHEN i.Status = 2 AND ISNULL(d.Status, -1) <> 2 THEN N''Order cancelled''
                 WHEN i.Status = 1 AND ISNULL(d.Status, -1) <> 1 THEN N''Queue completed''
                 WHEN ISNULL(i.GroupTo, -1) <> ISNULL(d.GroupTo, -1) THEN N''Order grouped / ungrouped''
                 ELSE N''Queue edited'' END,
            ' + @actor + N', ' + @terminal + N',
            LEFT(CONCAT(N''Queue: '', COALESCE(i.id, d.id), N''; status: '', d.Status, N'' -> '', i.Status,
                N''; group: '', d.GroupTo, N'' -> '', i.GroupTo, N''; owner: '', d.OPIS, N'' -> '', i.OPIS), 2000)
        FROM inserted i FULL JOIN deleted d ON i.id = d.id
        WHERE COALESCE(i.OrderID, d.OrderID) IS NOT NULL AND
            (i.id IS NULL OR d.id IS NULL OR EXISTS (
                SELECT i.OrderID, i.Status, i.GroupTo, CONVERT(nvarchar(max), i.OPIS)
                EXCEPT SELECT d.OrderID, d.Status, d.GroupTo, CONVERT(nvarchar(max), d.OPIS)));
    END';
    IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Queue', N'TR') IS NOT NULL
        SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
    EXEC sys.sp_executesql @ddl;

    SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Picking ON dbo.QueueingItems AFTER INSERT, UPDATE, DELETE AS
    BEGIN
        SET NOCOUNT ON;
        INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, ItemID, Details)
        SELECT q.OrderID,
            CASE WHEN d.id IS NULL THEN N''Picking item added'' WHEN i.id IS NULL THEN N''Picking item removed''
                 WHEN ISNULL(i.Status, N'''') <> ISNULL(d.Status, N'''') THEN N''Picking status changed''
                 WHEN ISNULL(i.Picker, N'''') <> ISNULL(d.Picker, N'''') THEN N''Picker assigned / changed''
                 ELSE N''Prepared quantity / location changed'' END,
            ' + @actor + N', ' + @terminal + N', COALESCE(i.ItemID, d.ItemID),
            LEFT(CONCAT(N''Queue item: '', COALESCE(i.id, d.id), N''; status: '', d.Status, N'' -> '', i.Status,
                N''; picker: '', d.Picker, N'' -> '', i.Picker, N''; prepared quantity: '', d.QtyPre, N'' -> '', i.QtyPre,
                N''; location: '', CONVERT(nvarchar(max), d.PickLoc), N'' -> '', CONVERT(nvarchar(max), i.PickLoc)), 2000)
        FROM inserted i FULL JOIN deleted d ON i.id = d.id
        JOIN dbo.Queueing q ON q.id = COALESCE(i.QueueingID, d.QueueingID)
        WHERE q.OrderID IS NOT NULL AND (i.id IS NULL OR d.id IS NULL OR EXISTS (
            SELECT i.QueueingID, i.ItemID, i.Status, i.Picker, i.QtyPre, CONVERT(nvarchar(max), i.PickLoc)
            EXCEPT SELECT d.QueueingID, d.ItemID, d.Status, d.Picker, d.QtyPre, CONVERT(nvarchar(max), d.PickLoc)));
    END';
    IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Picking', N'TR') IS NOT NULL
        SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
    EXEC sys.sp_executesql @ddl;

    SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Invoice ON dbo.[Transaction] AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, Details)
        SELECT i.RecallID, N''RMS transaction posted'', ' + @actor + N', ' + @terminal + N',
            CONCAT(N''Transaction: '', i.TransactionNumber, N''; RMS cashier ID: '', i.CashierID)
        FROM inserted i WHERE i.RecallID > 0 AND
            (EXISTS (SELECT 1 FROM dbo.Queueing q WHERE q.OrderID = i.RecallID)
             OR EXISTS (SELECT 1 FROM dbo.SOD_WO_History h WHERE h.OrderID = i.RecallID));
    END';
    IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Invoice', N'TR') IS NOT NULL
        SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
    EXEC sys.sp_executesql @ddl;

    -- Optional price-approval table. Rerun after installing that table if absent.
    IF OBJECT_ID(N'dbo.SOD_WO_LOGS', N'U') IS NOT NULL
    BEGIN
        SET @ddl = N'CREATE TRIGGER dbo.SOD_trg_WO_History_Approval ON dbo.SOD_WO_LOGS AFTER INSERT AS
        BEGIN
            SET NOCOUNT ON;
            INSERT dbo.SOD_WO_History(OrderID, EventName, Actor, Terminal, ItemID, Details)
            SELECT i.OrderId,
                CASE WHEN i.Status = N''Deleted'' THEN N''Item removal logged'' ELSE N''Price approval recorded'' END,
                ' + @actor + N', ' + @terminal + N', i.ItemID,
                LEFT(CONCAT(N''Approver: '', i.ApprovedUser, N''; full price: '', i.FullPrice,
                    N''; selected price: '', i.UpdatedPrice, N''; action: '', i.Status), 2000)
            FROM inserted i WHERE i.OrderId IS NOT NULL;
        END';
        IF OBJECT_ID(N'dbo.SOD_trg_WO_History_Approval', N'TR') IS NOT NULL
            SET @ddl = STUFF(@ddl, 1, 6, N'ALTER');
        EXEC sys.sp_executesql @ddl;
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
