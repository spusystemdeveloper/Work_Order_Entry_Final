/*
================================================================================
 MIGRATION SCRIPT: Upgrade GSC_STORE_DB for Work Order Entry
 Database Target: GSC_STORE_DB (General Santos City Store RMS Database)
 Purpose:
   Deploys all missing tables, draft crash-recovery tables, views, functions,
   and schema adjustments required for the unified Work Order Entry application
   to run successfully against GSC_STORE_DB.

 Characteristics:
   - 100% Idempotent: safe to run multiple times without duplicating or breaking data.
   - Transactional: wraps DDL and schema additions in an explicit transaction.
   - Preserves GSC Branch Overrides: does not touch GSC-specific warehouse functions
     (SOD_fn_GetQty, SOD_fntbl_CheckQty, SOD_fntbl_CheckExQty). Aligns SOD_fntbl_WoDetails with Address.
   - Target Environment: Microsoft SQL Server 2016 (compatible with 2008 through 2022).

 How to run:
   1. Open SQL Server Management Studio (SSMS).
   2. Connect to the GSC SQL Server instance.
   3. Ensure target database context is GSC_STORE_DB (USE [GSC_STORE_DB];).
   4. Execute this script.
   5. Review the validation summary at the bottom.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '------------------------------------------------------------';
PRINT 'Starting Work Order Entry migration for GSC_STORE_DB...';
PRINT 'Target Database: ' + DB_NAME();
PRINT '------------------------------------------------------------';

BEGIN TRY
    BEGIN TRANSACTION;

    -- =========================================================================
    -- 1. TABLE: SOD_WO_LOGS (Supervisor price approval & override audit table)
    -- =========================================================================
    IF OBJECT_ID(N'dbo.SOD_WO_LOGS', N'U') IS NULL
    BEGIN
        PRINT 'Creating table: dbo.SOD_WO_LOGS...';
        CREATE TABLE dbo.SOD_WO_LOGS
        (
            id int IDENTITY(1,1) NOT NULL,
            OrderId int NULL,
            ItemID int NULL,
            FullPrice money NULL,
            Price money NULL,
            Pricea money NULL,
            Priceb money NULL,
            Pricec money NULL,
            LastUpdatedPrice datetime NULL,
            UpdatedPrice money NULL,
            ApprovedUser nvarchar(50) NULL,
            Status nvarchar(50) NULL,
            [Date] datetime NULL,
            [timestamp] timestamp NULL,
            OrderTaker nvarchar(50) NULL,
            CustomerPriceLevel int NULL,
            CONSTRAINT PK_SOD_WO_LOGS PRIMARY KEY CLUSTERED (id ASC)
        );
        PRINT '  -> dbo.SOD_WO_LOGS created.';
    END
    ELSE
    BEGIN
        PRINT 'Table dbo.SOD_WO_LOGS already exists. Skipped.';
    END;

    -- =========================================================================
    -- 2. TABLE: SOD_WO_DraftHeader (Crash recovery & auto-save draft header)
    -- =========================================================================
    IF OBJECT_ID(N'dbo.SOD_WO_DraftHeader', N'U') IS NULL
    BEGIN
        PRINT 'Creating table: dbo.SOD_WO_DraftHeader...';
        CREATE TABLE dbo.SOD_WO_DraftHeader
        (
            DraftID int IDENTITY(1,1) NOT NULL,
            RegisterID nvarchar(50) NOT NULL,
            UserName nvarchar(100) NOT NULL,
            EntryType int NULL,
            CustomerID int NULL,
            CustomerName nvarchar(255) NULL,
            BusinessCustomerType nvarchar(100) NULL,
            PaymentType nvarchar(100) NULL,
            SalesRepID int NULL,
            SalesRepName nvarchar(255) NULL,
            Remarks nvarchar(max) NULL,
            ReleaseType nvarchar(50) NULL,
            PriceLevel nvarchar(50) NULL,
            TaxExempt bit NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_TaxExempt DEFAULT (0),
            ZeroRated bit NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_ZeroRated DEFAULT (0),
            CreatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_CreatedAt DEFAULT (GETDATE()),
            UpdatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_UpdatedAt DEFAULT (GETDATE()),
            CONSTRAINT PK_SOD_WO_DraftHeader PRIMARY KEY CLUSTERED (DraftID),
            CONSTRAINT UQ_SOD_WO_DraftHeader_RegisterUser UNIQUE (RegisterID, UserName)
        );
        PRINT '  -> dbo.SOD_WO_DraftHeader created.';
    END
    ELSE
    BEGIN
        PRINT 'Table dbo.SOD_WO_DraftHeader already exists. Skipped.';
    END;

    -- =========================================================================
    -- 3. TABLE: SOD_WO_DraftDetail (Crash recovery & auto-save draft detail)
    -- =========================================================================
    IF OBJECT_ID(N'dbo.SOD_WO_DraftDetail', N'U') IS NULL
    BEGIN
        PRINT 'Creating table: dbo.SOD_WO_DraftDetail...';
        CREATE TABLE dbo.SOD_WO_DraftDetail
        (
            DraftDetailID int IDENTITY(1,1) NOT NULL,
            DraftID int NOT NULL,
            [LineNo] int NOT NULL,
            ItemCode nvarchar(50) NOT NULL,
            ItemName nvarchar(500) NULL,
            Qty int NULL,
            Price money NULL,
            Disc money NULL,
            TotalPrice money NULL,
            LessVat money NULL,
            VatSales money NULL,
            DiscPrice money NULL,
            Cost money NULL,
            Taxable int NULL,
            ItemID bigint NULL,
            FullPrice money NULL,
            Description nvarchar(500) NULL,
            ExtendedDescription nvarchar(max) NULL,
            DiscAmount money NULL,
            OrderEntryID int NULL,
            StoreWhse bit NOT NULL CONSTRAINT DF_SOD_WO_DraftDetail_StoreWhse DEFAULT (0),
            PreparedQty int NULL,
            LastPurchasedPrice money NULL,
            CreatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftDetail_CreatedAt DEFAULT (GETDATE()),
            CONSTRAINT PK_SOD_WO_DraftDetail PRIMARY KEY CLUSTERED (DraftDetailID),
            CONSTRAINT FK_SOD_WO_DraftDetail_SOD_WO_DraftHeader
                FOREIGN KEY (DraftID) REFERENCES dbo.SOD_WO_DraftHeader(DraftID) ON DELETE CASCADE
        );
        PRINT '  -> dbo.SOD_WO_DraftDetail created.';
    END
    ELSE
    BEGIN
        PRINT 'Table dbo.SOD_WO_DraftDetail already exists. Skipped.';
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.SOD_WO_DraftDetail')
          AND name = N'IX_SOD_WO_DraftDetail_DraftID_LineNo'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SOD_WO_DraftDetail_DraftID_LineNo
            ON dbo.SOD_WO_DraftDetail(DraftID, [LineNo]);
        PRINT '  -> Index IX_SOD_WO_DraftDetail_DraftID_LineNo created.';
    END;

    -- =========================================================================
    -- 4. TABLE: SOD_ImportedQuote (Quotation import / upload audit log)
    -- =========================================================================
    IF OBJECT_ID(N'dbo.SOD_ImportedQuote', N'U') IS NULL
    BEGIN
        PRINT 'Creating table: dbo.SOD_ImportedQuote...';
        CREATE TABLE dbo.SOD_ImportedQuote
        (
            ID bigint IDENTITY(1,1) NOT NULL,
            ImportDate datetime NULL,
            QuoteID int NULL,
            ReferenceID int NULL,
            SalesRepID int NULL,
            CONSTRAINT PK_SOD_ImportedQuote PRIMARY KEY CLUSTERED (ID)
        );
        PRINT '  -> dbo.SOD_ImportedQuote created.';
    END
    ELSE
    BEGIN
        PRINT 'Table dbo.SOD_ImportedQuote already exists. Skipped.';
    END;

    -- =========================================================================
    -- 5. COLUMN ALIGNMENT: SOD_WO_Conf (Add Pass2Username & Pass3Username)
    --    ItemLookUpDataContext LINQ mapping requires these columns.
    --    Without them, querying SOD_WO_Conf throws runtime SqlExceptions.
    -- =========================================================================
    IF OBJECT_ID(N'dbo.SOD_WO_Conf', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Pass2Username') IS NULL
        BEGIN
            ALTER TABLE dbo.SOD_WO_Conf ADD Pass2Username nvarchar(50) NULL;
            PRINT '  -> Added Pass2Username column to dbo.SOD_WO_Conf.';
        END;

        IF COL_LENGTH(N'dbo.SOD_WO_Conf', N'Pass3Username') IS NULL
        BEGIN
            ALTER TABLE dbo.SOD_WO_Conf ADD Pass3Username nvarchar(50) NULL;
            PRINT '  -> Added Pass3Username column to dbo.SOD_WO_Conf.';
        END;
    END;

    -- =========================================================================
    -- 5b. QUEUEING PERFORMANCE INDEXES: dbo.Queueing & dbo.QueueingItems
    --     Prevents full table scans across 1M+ rows and thread starvation (Error 8642)
    -- =========================================================================
    IF OBJECT_ID(N'dbo.Queueing', N'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Queueing') AND name = N'IX_Queueing_GroupTo')
        BEGIN
            CREATE NONCLUSTERED INDEX IX_Queueing_GroupTo ON dbo.Queueing (GroupTo) INCLUDE (OrderID, Status, OPIS);
            PRINT '  -> Index IX_Queueing_GroupTo created.';
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Queueing') AND name = N'IX_Queueing_OrderID')
        BEGIN
            CREATE NONCLUSTERED INDEX IX_Queueing_OrderID ON dbo.Queueing (OrderID);
            PRINT '  -> Index IX_Queueing_OrderID created.';
        END;
    END;

    IF OBJECT_ID(N'dbo.QueueingItems', N'U') IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.QueueingItems') AND name = N'IX_QueueingItems_QueueingID')
        BEGIN
            CREATE NONCLUSTERED INDEX IX_QueueingItems_QueueingID ON dbo.QueueingItems (QueueingID) INCLUDE (ItemID, QtyPre, Picker, Status);
            PRINT '  -> Index IX_QueueingItems_QueueingID created.';
        END;
    END;

    -- =========================================================================
    -- 6. VIEW: SOD_ViewItemsWO (Primary Item Search & Available Qty View)
    --    frmItemLookUp queries ''SELECT TOP 500 * FROM SOD_VIEWITEMSWO''
    -- =========================================================================
    PRINT 'Deploying view: dbo.SOD_ViewItemsWO...';
    IF OBJECT_ID(N'dbo.SOD_ViewItemsWO', N'V') IS NULL
        EXEC sys.sp_executesql N'CREATE VIEW dbo.SOD_ViewItemsWO AS SELECT 1 AS Dummy;';

    EXEC sys.sp_executesql N'
    ALTER VIEW dbo.SOD_ViewItemsWO
    AS
    SELECT 
        ItemLookupCode,
        ISNULL(DESCRIPTION, '''') + ISNULL(CONVERT(nvarchar(max), EXTENDEDDESCRIPTION), '''') AS [DESCRIPTION],
        PRICE,
        COST,
        CASE 
            WHEN QuantityCommitted < 0 THEN Quantity
            WHEN Quantity < 0 THEN 0
            ELSE Quantity - QuantityCommitted
        END AS [AVAILABLE],
        ParentQuantity,
        TaxID,
        SubDescription2,
        PRICE AS [PRICE 1],
        ID,
        Description AS [DESCRIPTION1],
        ExtendedDescription AS ExtendedDescription,
        ItemType,
        Inactive,
        ''-'' AS [ITEMDES],
        SubDescription2 AS [SKULEVEL],
        ISNULL(ItemLookupCode, '''') + '' | '' + ISNULL(DESCRIPTION, '''') + ISNULL(CONVERT(nvarchar(max), EXTENDEDDESCRIPTION), '''') AS [FULLDESC],
        Quantity,
        QuantityCommitted
    FROM dbo.Item;';
    PRINT '  -> dbo.SOD_ViewItemsWO deployed.';

    -- =========================================================================
    -- 7. FUNCTION: SOD_fn_GetAvailableQty (SKU available quantity calculation)
    -- =========================================================================
    PRINT 'Deploying scalar function: dbo.SOD_fn_GetAvailableQty...';
    IF OBJECT_ID(N'dbo.SOD_fn_GetAvailableQty', N'FN') IS NULL
        EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fn_GetAvailableQty (@vLookUp varchar(30), @iQty int) RETURNS float AS BEGIN RETURN 0; END;';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fn_GetAvailableQty
    (
        @vLookUp varchar(30),
        @iQty int
    )
    RETURNS float
    AS
    BEGIN
        DECLARE @ID int,
                @ItemLookUpCode varchar(30),
                @ParentItem int,
                @ParentQuantity float,
                @Quantity float,
                @Committed float,
                @SKULevel int,
                @xSKULevel int,
                @Available float;

        SET @Available = 0;
        SET @xSKULevel = 0;

        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT ID, ItemLookUpCode, ParentItem, ParentQuantity, Quantity, QuantityCommitted, CONVERT(INT, SubDescription2) AS SKULEVEL
        FROM dbo.Item
        WHERE ItemLookUpCode LIKE ''%'' + LEFT(@vLookUp, 5) + ''%''
        ORDER BY CONVERT(INT, SubDescription2) DESC;

        OPEN cur;
        FETCH NEXT FROM cur INTO @ID, @ItemLookUpCode, @ParentItem, @ParentQuantity, @Quantity, @Committed, @SKULevel;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF @ItemLookUpCode = @vLookUp
            BEGIN
                SET @xSKULevel = @SKULevel;
                SET @Quantity = @iQty;
            END
            ELSE
            BEGIN
                SET @Quantity = 0;
            END;

            IF @SKULevel <= @xSKULevel
            BEGIN
                SET @Available = (@Available * @ParentQuantity) + @Quantity;
            END;

            FETCH NEXT FROM cur INTO @ID, @ItemLookUpCode, @ParentItem, @ParentQuantity, @Quantity, @Committed, @SKULevel;
        END;
        CLOSE cur;
        DEALLOCATE cur;

        RETURN @Available;
    END;';
    PRINT '  -> dbo.SOD_fn_GetAvailableQty deployed.';

    -- =========================================================================
    -- 8. FUNCTION: SOD_fn_GetConvertQty (Parent SKU conversion quantity)
    -- =========================================================================
    PRINT 'Deploying scalar function: dbo.SOD_fn_GetConvertQty...';
    IF OBJECT_ID(N'dbo.SOD_fn_GetConvertQty', N'FN') IS NULL
        EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fn_GetConvertQty (@vLookUp varchar(30), @vTotal bit) RETURNS float AS BEGIN RETURN 0; END;';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fn_GetConvertQty
    (
        @vLookUp varchar(30),
        @vTotal bit
    )
    RETURNS float
    AS
    BEGIN
        DECLARE @ID int,
                @ItemLookUpCode varchar(30),
                @ParentItem int,
                @ParentQuantity float,
                @Quantity float,
                @Committed float,
                @SKULevel int,
                @Available float,
                @xLoop bit;

        SET @Available = 0;
        SET @xLoop = 0;

        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT ID, ItemLookUpCode, ParentItem, ParentQuantity, Quantity, QuantityCommitted, CONVERT(INT, SubDescription2) AS SKULEVEL
        FROM dbo.Item
        WHERE ItemLookUpCode LIKE ''%'' + LEFT(@vLookUp, 5) + ''%''
        ORDER BY CONVERT(INT, SubDescription2) DESC;

        OPEN cur;
        FETCH NEXT FROM cur INTO @ID, @ItemLookUpCode, @ParentItem, @ParentQuantity, @Quantity, @Committed, @SKULevel;
        WHILE @@FETCH_STATUS = 0 AND @xLoop <> 1
        BEGIN
            IF @vTotal = 1
            BEGIN
                SET @Available = (@Available * @ParentQuantity) + (@Quantity - @Committed);
                IF @ItemLookUpCode = @vLookUp
                    SET @xLoop = 1;
            END
            ELSE
            BEGIN
                SET @Available = (@Available * @ParentQuantity) + (@Quantity - @Committed);
                IF @SKULevel = 1
                    SET @xLoop = 1;
            END;

            FETCH NEXT FROM cur INTO @ID, @ItemLookUpCode, @ParentItem, @ParentQuantity, @Quantity, @Committed, @SKULevel;
        END;
        CLOSE cur;
        DEALLOCATE cur;

        RETURN @Available;
    END;';
    PRINT '  -> dbo.SOD_fn_GetConvertQty deployed.';

    -- =========================================================================
    -- 9. FUNCTION: SOD_fn_GetCreditLimit (Customer Accounts Receivable Balance)
    -- =========================================================================
    PRINT 'Deploying scalar function: dbo.SOD_fn_GetCreditLimit...';
    IF OBJECT_ID(N'dbo.SOD_fn_GetCreditLimit', N'FN') IS NULL
        EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fn_GetCreditLimit (@iCustomerID int) RETURNS decimal(18,2) AS BEGIN RETURN 0; END;';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fn_GetCreditLimit
    (
        @iCustomerID int
    )
    RETURNS decimal(18,2)
    AS
    BEGIN
        DECLARE @dCreditLimit decimal(18,2);

        SELECT @dCreditLimit = ISNULL(SUM(OriginalAmount), 0)
        FROM dbo.AccountReceivable
        WHERE Balance > 0
          AND CustomerID = @iCustomerID;

        RETURN @dCreditLimit;
    END;';
    PRINT '  -> dbo.SOD_fn_GetCreditLimit deployed.';

    -- =========================================================================
    -- 10. FUNCTION: SOD_fn_GetTotalOpenWorkOrder (Customer Total Open WO Total)
    -- =========================================================================
    PRINT 'Deploying scalar function: dbo.SOD_fn_GetTotalOpenWorkOrder...';
    IF OBJECT_ID(N'dbo.SOD_fn_GetTotalOpenWorkOrder', N'FN') IS NULL
        EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fn_GetTotalOpenWorkOrder (@iCustomerID int) RETURNS decimal(18,2) AS BEGIN RETURN 0; END;';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fn_GetTotalOpenWorkOrder
    (
        @iCustomerID int
    )
    RETURNS decimal(18,2)
    AS
    BEGIN
        DECLARE @dTotal decimal(18,2);

        SELECT @dTotal = ISNULL(SUM(Total), 0)
        FROM dbo.[Order]
        WHERE Closed = 0
          AND CustomerID = @iCustomerID
          AND [Type] = 2;

        RETURN @dTotal;
    END;';
    PRINT '  -> dbo.SOD_fn_GetTotalOpenWorkOrder deployed.';

    -- =========================================================================
    -- 11. FUNCTION: SOD_fntbl_CheckOpenWO (Item Open Work Orders Lookup)
    -- =========================================================================
    PRINT 'Deploying inline table function: dbo.SOD_fntbl_CheckOpenWO...';
    IF OBJECT_ID(N'dbo.SOD_fntbl_CheckOpenWO', N'IF') IS NULL
        EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fntbl_CheckOpenWO (@vCode varchar(15)) RETURNS TABLE AS RETURN (SELECT 1 AS Dummy);';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fntbl_CheckOpenWO
    (
        @vCode varchar(15)
    )
    RETURNS TABLE 
    AS
    RETURN 
    (
        SELECT 
            [Order].ID, 
            CASE WHEN [Order].Closed = 0 THEN ''Open'' ELSE ''Closed'' END AS [Status],
            Customer.Company,
            Customer.AccountNumber,
            Item.ItemLookupCode,
            ISNULL(Item.Description, '''') + ISNULL(CONVERT(nvarchar(max), Item.ExtendedDescription), '''') AS [Description],
            OrderEntry.QuantityOnOrder,
            Cashier.Name,
            CONVERT(varchar(10), [Order].Time, 101) AS OrderDate,
            CASE 
                WHEN DATEDIFF(DAY, [Order].Time, GETDATE()) = 1 THEN ''1 day''
                ELSE CAST(DATEDIFF(DAY, [Order].Time, GETDATE()) AS varchar(10)) + '' days''
            END AS DaysOpenLabel
        FROM dbo.OrderEntry WITH(NOLOCK)
        INNER JOIN dbo.[Order] WITH(NOLOCK) ON OrderEntry.OrderID = [Order].ID 
        LEFT JOIN dbo.Customer WITH(NOLOCK) ON [Order].CustomerID = Customer.ID 
        LEFT JOIN dbo.Item WITH(NOLOCK) ON OrderEntry.ItemID = Item.ID 
        LEFT JOIN dbo.Queueing ON [Order].ID = Queueing.OrderID
        LEFT JOIN dbo.Cashier ON Queueing.OPIS = Cashier.Number
        LEFT JOIN dbo.ReasonCode AS ReasonCodeDiscount WITH(NOLOCK) ON OrderEntry.DiscountReasonCodeID = ReasonCodeDiscount.ID 
        LEFT JOIN dbo.ReasonCode AS ReasonCodeTaxChange WITH(NOLOCK) ON OrderEntry.TaxChangeReasonCodeID = ReasonCodeTaxChange.ID
        WHERE Item.ItemLookupCode = @vCode 
          AND [Order].Closed = 0 
          AND [Order].[Type] = 2
    );';
    PRINT '  -> dbo.SOD_fntbl_CheckOpenWO deployed.';

    -- =========================================================================
    -- 12. FUNCTION: SOD_fntbl_WoDetails (WO Print Header Details with Address)
    --     Aligns GSC_STORE_DB table-valued function with LINQ-to-SQL mapping.
    --     Fixes "ERROR 0019: Invalid column name 'Address'".
    -- =========================================================================
    PRINT 'Deploying table-valued function: dbo.SOD_fntbl_WoDetails...';
    IF OBJECT_ID(N'dbo.SOD_fntbl_WoDetails', N'TF') IS NULL
        EXEC sys.sp_executesql N'
        CREATE FUNCTION dbo.SOD_fntbl_WoDetails
        (
            @Orderid int, @Register int, @Cashier varchar(200)
        )
        RETURNS @temptable TABLE 
        (
            Branch varchar(max), Orderid int, AccountNo varchar(255), Company varchar(255), Address nvarchar(255), Register int,
            Cashier varchar(255), OrderDate varchar(255), Ordertime varchar(255), Reference varchar(255), Comment varchar(255),
            Subtotal decimal(20,2), SalesTax decimal(20,2), Total decimal(20,2)
        )
        AS BEGIN RETURN; END;';

    EXEC sys.sp_executesql N'
    ALTER FUNCTION dbo.SOD_fntbl_WoDetails
    (
        @Orderid int, 
        @Register int, 
        @Cashier varchar(200)
    )
    RETURNS @temptable TABLE 
    (
        Branch varchar(max),
        Orderid int,
        AccountNo varchar(255),
        Company varchar(255),
        Address nvarchar(255),
        Register int,
        Cashier varchar(255),
        OrderDate varchar(255),
        Ordertime varchar(255),
        Reference varchar(255),
        Comment varchar(255),
        Subtotal decimal(20,2),
        SalesTax decimal(20,2),
        Total decimal(20,2)
    )
    AS
    BEGIN 
        INSERT INTO @temptable
        (
            Branch, Orderid, AccountNo, Company, Address, Register,
            Cashier, OrderDate, Ordertime, Reference, Comment, Subtotal, SalesTax, Total
        )
        SELECT 
            (SELECT storeCity FROM dbo.[Configuration]),
            o.id,
            c.AccountNumber,
            c.Company,
            CONCAT(c.Address, c.Address2, c.City, c.State, c.Zip) AS Address,
            @Register, 
            @Cashier,
            CONVERT(varchar(50), o.Time, 101) AS [Date],
            REPLACE(REPLACE(RIGHT(CONVERT(varchar(30), o.Time, 100), 7), ''P'', '' P''), ''A'', '' A'') AS [Time],
            o.ReferenceNumber,
            o.Comment,
            o.Total - o.Tax AS Subtotal,
            o.Tax, 
            o.Total 
        FROM dbo.[order] o
        INNER JOIN dbo.Customer c ON o.CustomerID = c.id
        WHERE o.id = @Orderid;

        RETURN;
    END;';
    PRINT '  -> dbo.SOD_fntbl_WoDetails deployed.';

    -- Commit transaction
    COMMIT TRANSACTION;
    PRINT '------------------------------------------------------------';
    PRINT 'Migration successfully committed to GSC_STORE_DB!';
    PRINT '------------------------------------------------------------';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE();
    DECLARE @ErrSeverity int = ERROR_SEVERITY();
    DECLARE @ErrState int = ERROR_STATE();

    PRINT '------------------------------------------------------------';
    PRINT 'ERROR ENCOUNTERED. Transaction rolled back!';
    PRINT 'Error Message: ' + @ErrMsg;
    PRINT '------------------------------------------------------------';

    RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
END CATCH;
GO

-- =============================================================================
-- POST-MIGRATION VERIFICATION
-- Runs a check on all objects required by Work Order Entry in GSC_STORE_DB
-- =============================================================================
PRINT '';
PRINT '============================================================';
PRINT 'POST-MIGRATION OBJECT STATUS VERIFICATION';
PRINT '============================================================';

DECLARE @RequiredObjects TABLE
(
    ObjectName sysname NOT NULL,
    ObjectTypeDesc nvarchar(50) NOT NULL,
    Category nvarchar(50) NOT NULL
);

INSERT INTO @RequiredObjects (ObjectName, ObjectTypeDesc, Category)
VALUES
    (N'Queueing',                     N'USER_TABLE',              N'Queue Core (Pre-existing)'),
    (N'QueueingItems',                N'USER_TABLE',              N'Queue Core (Pre-existing)'),
    (N'PickerList',                   N'USER_TABLE',              N'Queue Core (Pre-existing)'),
    (N'SOD_WO_LOGS',                  N'USER_TABLE',              N'Audit Log (Migrated)'),
    (N'SOD_WO_DraftHeader',           N'USER_TABLE',              N'Draft Auto-Save (Migrated)'),
    (N'SOD_WO_DraftDetail',           N'USER_TABLE',              N'Draft Auto-Save (Migrated)'),
    (N'SOD_ImportedQuote',            N'USER_TABLE',              N'Quotation Import (Migrated)'),
    (N'SOD_WO_Conf',                  N'USER_TABLE',              N'Configuration (Pre-existing)'),
    (N'SOD_ViewItemsWO',              N'VIEW',                    N'Item Search (Migrated)'),
    (N'SOD_ViewForInvoice',           N'VIEW',                    N'For Invoicing (Pre-existing)'),
    (N'SOD_fn_GetAvailableQty',       N'SQL_SCALAR_FUNCTION',     N'Stock Availability (Migrated)'),
    (N'SOD_fn_GetConvertQty',         N'SQL_SCALAR_FUNCTION',     N'Unit Conversion (Migrated)'),
    (N'SOD_fn_GetCreditLimit',        N'SQL_SCALAR_FUNCTION',     N'Credit Limit (Migrated)'),
    (N'SOD_fn_GetTotalOpenWorkOrder', N'SQL_SCALAR_FUNCTION',     N'Open Orders Total (Migrated)'),
    (N'SOD_fntbl_CheckOpenWO',        N'SQL_INLINE_TABLE_VALUED_FUNCTION', N'Open Orders Lookup (Migrated)'),
    (N'SOD_fn_GetQty',                N'SQL_SCALAR_FUNCTION',     N'GSC Fireworks Stock (Pre-existing)'),
    (N'SOD_fntbl_CheckQty',           N'SQL_INLINE_TABLE_VALUED_FUNCTION', N'GSC Store Lookup (Pre-existing)'),
    (N'SOD_fntbl_CheckExQty',         N'SQL_INLINE_TABLE_VALUED_FUNCTION', N'GSC Store Lookup (Pre-existing)'),
    (N'SOD_fntbl_WoDetails',          N'SQL_TABLE_VALUED_FUNCTION',        N'WO Print Header (Migrated)'),
    (N'SOD_sp_InsertQoute',           N'SQL_STORED_PROCEDURE',    N'Order Header Save (Pre-existing)'),
    (N'SOD_sp_InsertQouteEntry',      N'SQL_STORED_PROCEDURE',    N'Order Entry Save (Pre-existing)'),
    (N'trg_Transaction_closeQueue',   N'SQL_TRIGGER',             N'POS Close Trigger (Pre-existing)');

SELECT 
    r.ObjectName,
    r.Category,
    r.ObjectTypeDesc AS ExpectedType,
    ISNULL(o.type_desc, 'NOT FOUND') AS ActualType,
    CASE 
        WHEN o.object_id IS NOT NULL THEN 'READY' 
        ELSE 'MISSING' 
    END AS Status
FROM @RequiredObjects r
LEFT JOIN sys.objects o ON o.name = r.ObjectName AND SCHEMA_NAME(o.schema_id) = 'dbo'
ORDER BY 
    CASE WHEN o.object_id IS NULL THEN 0 ELSE 1 END,
    r.Category,
    r.ObjectName;

-- Column check for SOD_WO_Conf
SELECT 
    'SOD_WO_Conf.Pass2Username' AS ColumnCheck,
    CASE WHEN COL_LENGTH('dbo.SOD_WO_Conf', 'Pass2Username') IS NOT NULL THEN 'READY' ELSE 'MISSING' END AS Status
UNION ALL
SELECT 
    'SOD_WO_Conf.Pass3Username' AS ColumnCheck,
    CASE WHEN COL_LENGTH('dbo.SOD_WO_Conf', 'Pass3Username') IS NOT NULL THEN 'READY' ELSE 'MISSING' END AS Status;
GO
