/* ONE-OFF CEBU REPAIR; not an updater migration or general reconciliation.
   User authorized these exact 23 items on nine Closed=0, Type=2 orders.
   Commit=0 is a SELECT-only preview, NOT a live rollback rehearsal.
   Before Commit=1: RESTORE VERIFYONLY the recorded backup and supply its
   path/finish time below. Preserve the backup/target/scope/commit guards.
   Commit adds the verified difference atomically from zero, records 23 audit
   rows, and verifies physical stock. No order, line, price or trigger edits.
   All clients may briefly wait on the transaction's serialization locks.
   Rerunning after success fails the zero-commitment guard; never auto-retry.
*/
USE [SBD_CEBU_MAIN_DB];
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
DECLARE @Commit bit = 0;
DECLARE @VerifiedBackupPath nvarchar(4000) = N'';
DECLARE @VerifiedBackupFinished datetime = NULL;
IF @@TRANCOUNT <> 0 THROW 53700, 'Use a fresh query window without an existing transaction.', 1;
IF DB_NAME() <> N'SBD_CEBU_MAIN_DB' OR @@SERVERNAME <> N'WIN-7RFPRJCS0F8'
    THROW 53701, 'Wrong Cebu server/database.', 1;

DECLARE @Expected TABLE
 (ItemID int PRIMARY KEY, ItemCode nvarchar(50), OrderID int,
  EntryID int UNIQUE, QuantityToAdd float);
INSERT @Expected VALUES
 (1560,N'10878PCS',26599,81004,10),
 (1568,N'10882PCS',26600,81007,20),
 (1572,N'10884PCS',26599,81006,20),
 (1574,N'10885PCS',26599,81005,10),
 (2213,N'53494BTL',26596,80984,50),
 (2359,N'53031PCS',26597,80989,30),
 (2362,N'53032PCS',26597,80990,50),
 (2365,N'53033PCS',26597,80992,50),
 (2368,N'53034PCS',26597,80991,50),
 (2656,N'55796PCS',26603,81123,20),
 (3604,N'56164PCS',26597,80994,100),
 (3727,N'56571PCS',26600,81008,200),
 (3729,N'56572PCS',26600,81009,200),
 (3828,N'56635PCS',26598,80999,288),
 (3987,N'57251PCS',26592,80960,50),
 (4040,N'57296PCS',26602,81020,24),
 (4376,N'57079PCS',26600,81011,12),
 (4388,N'57082PCS',26600,81010,144),
 (4830,N'56865PCS',26592,80959,80),
 (5128,N'57884PCS',26598,80996,200),
 (5192,N'57893BOX',26602,81015,30),
 (6328,N'57517PCS',26604,81028,20),
 (6330,N'57518PCS',26604,81029,20);
DECLARE @Triggers TABLE (Name sysname PRIMARY KEY, Hash varchar(64));
INSERT @Triggers VALUES
 (N'tr_ItemValueLog','85A9AE1CACE36BB0922121CF9A3CFC9A8FF26D59FED31ADDD55E56539E655415'),
 (N'trg_Item_VAT','478BC7F51BF16481140AF0E6D213B479D308CF7E08C16949DF86E2B2A8A1AE0E'),
 (N'tr_ItemDeleted','651E6B225785096C1B56D1C46E2A34D65ABC95710A95B8027F0EE4BA40E337EC');

IF @Commit=0
BEGIN
    SELECT N'PREVIEW ONLY: no database writes' AS Result;
    SELECT x.*,i.Quantity AS PhysicalStock,i.QuantityCommitted AS CurrentCommitted,
           o.Closed,o.[Type],e.QuantityOnOrder AS CurrentOrderQuantity
    FROM @Expected x LEFT JOIN dbo.Item i ON i.ID=x.ItemID
    LEFT JOIN dbo.[Order] o ON o.ID=x.OrderID
    LEFT JOIN dbo.OrderEntry e ON e.ID=x.EntryID ORDER BY x.ItemID;
    RETURN;
END;

IF @VerifiedBackupPath=N'' OR @VerifiedBackupFinished IS NULL OR NOT EXISTS
 (SELECT 1 FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m
  ON m.media_set_id=b.media_set_id
  WHERE b.database_name=DB_NAME() AND b.[type]='D' AND b.is_damaged=0
    AND b.backup_finish_date=@VerifiedBackupFinished
    AND m.physical_device_name=@VerifiedBackupPath
    AND b.backup_finish_date>='20261020')
    THROW 53702, 'Supply the recorded full backup path and finish time after successful VERIFYONLY.', 1;

DECLARE @Before TABLE (ItemID int PRIMARY KEY,PhysicalStock float,
                       OldCommitted float,NewCommitted float);
DECLARE @Changed TABLE (ItemID int PRIMARY KEY);
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @OrderID int,@LockResult int,@Resource nvarchar(100),@Dummy int;
    DECLARE OrderLocks CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT OrderID FROM @Expected ORDER BY OrderID;
    OPEN OrderLocks;
    FETCH NEXT FROM OrderLocks INTO @OrderID;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @Resource=N'SOD_WO_ORDER_'+CONVERT(nvarchar(20),@OrderID);
        EXEC @LockResult=sys.sp_getapplock @Resource=@Resource,
             @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000;
        IF @LockResult<0 THROW 53703, 'An affected order is in use. Nothing committed.', 1;
        FETCH NEXT FROM OrderLocks INTO @OrderID;
    END;
    CLOSE OrderLocks;
    DEALLOCATE OrderLocks;
    -- RMS does not necessarily use the addin application locks. Table locks
    -- keep headers/lines/settlement references stable during this short repair.
    SELECT TOP(1) @Dummy=ID FROM dbo.[Order] WITH (TABLOCKX,HOLDLOCK);
    SELECT TOP(1) @Dummy=ID FROM dbo.OrderEntry WITH (TABLOCKX,HOLDLOCK);
    SELECT TOP(1) @Dummy=TransactionNumber FROM dbo.[Transaction] WITH (TABLOCKX,HOLDLOCK);
    SELECT TOP(1) @Dummy=ID FROM dbo.OrderHistory WITH (TABLOCKX,HOLDLOCK);

    INSERT @Before
    SELECT i.ID,i.Quantity,i.QuantityCommitted,x.QuantityToAdd
    FROM dbo.Item i WITH (UPDLOCK,HOLDLOCK) JOIN @Expected x ON x.ItemID=i.ID
    WHERE i.ItemType=0 AND i.ItemLookupCode=x.ItemCode AND i.QuantityCommitted=0;
    IF (SELECT COUNT(*) FROM @Before)<>23
        THROW 53704, 'The 23 zero-commitment items changed. Nothing committed.', 1;
    IF EXISTS (SELECT 1 FROM @Expected x LEFT JOIN dbo.[Order] o ON o.ID=x.OrderID
               LEFT JOIN dbo.OrderEntry e ON e.ID=x.EntryID
               WHERE o.ID IS NULL OR o.Closed<>0 OR o.[Type]<>2
                  OR e.ID IS NULL OR e.OrderID<>x.OrderID OR e.ItemID<>x.ItemID
                  OR e.QuantityOnOrder<>x.QuantityToAdd)
        THROW 53705, 'A reviewed order or line changed. Nothing committed.', 1;
    IF EXISTS
      (SELECT 1 FROM @Expected x CROSS APPLY
        (SELECT COUNT(*) AS Lines,SUM(e.QuantityOnOrder) AS Qty
         FROM dbo.OrderEntry e JOIN dbo.[Order] o ON o.ID=e.OrderID
         WHERE e.ItemID=x.ItemID AND o.Closed=0 AND o.[Type]=2
           AND e.QuantityOnOrder<>0) q
       WHERE q.Lines<>1 OR q.Qty<>x.QuantityToAdd)
        THROW 53706, 'Open-WO reservation scope changed. Nothing committed.', 1;
    IF EXISTS (SELECT 1 FROM dbo.[Transaction] t JOIN @Expected x ON x.OrderID=t.RecallID)
       OR EXISTS (SELECT 1 FROM dbo.OrderHistory h JOIN @Expected x ON x.OrderID=h.OrderID)
        THROW 53707, 'An affected order has settlement/history references. Review before repair.', 1;
    IF EXISTS
      (SELECT 1 FROM sys.triggers t FULL JOIN @Triggers x ON x.Name=t.name
           AND t.parent_id=OBJECT_ID(N'dbo.Item')
       WHERE (t.parent_id=OBJECT_ID(N'dbo.Item') OR x.Name IS NOT NULL)
         AND (t.object_id IS NULL OR x.Name IS NULL OR t.is_disabled<>0
           OR OBJECT_DEFINITION(t.object_id) IS NULL
           OR CONVERT(varchar(64),HASHBYTES('SHA2_256',OBJECT_DEFINITION(t.object_id)),2)<>x.Hash))
       OR EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id=OBJECT_ID(N'dbo.SOD_Item_Logs'))
        THROW 53708, 'Reviewed trigger scope changed. Nothing committed.', 1;
    -- This branch's existing audit fields are int; never silently round stock.
    IF EXISTS (SELECT 1 FROM @Before WHERE PhysicalStock<>FLOOR(PhysicalStock)
       OR PhysicalStock NOT BETWEEN -2147483648.0 AND 2147483647.0)
        THROW 53709, 'Physical stock cannot be represented exactly in the existing audit schema.', 1;

    -- CEBU_REPAIR_CORE_START (also exercised in an owned disposable fixture)
    UPDATE i SET QuantityCommitted=i.QuantityCommitted+x.QuantityToAdd
    OUTPUT inserted.ID INTO @Changed
    FROM dbo.Item i JOIN @Expected x ON x.ItemID=i.ID
    WHERE i.QuantityCommitted=0 AND i.ItemType=0;
    IF (SELECT COUNT(*) FROM @Changed)<>23
        THROW 53710, 'Unexpected update count. Nothing committed.', 1;
    IF EXISTS (SELECT 1 FROM @Before b JOIN dbo.Item i ON i.ID=b.ItemID
               WHERE i.Quantity<>b.PhysicalStock OR i.QuantityCommitted<>b.NewCommitted)
        THROW 53711, 'Physical stock or final commitments differ. Nothing committed.', 1;
    INSERT dbo.SOD_Item_Logs
      (ItemID,ItemLookupCode,[Type],ReferenceID,QuantityORIS,
       LastQuantity,NewQuantity,LastCommitted,NewCommitted,[Date])
    SELECT x.ItemID,x.ItemCode,'Item Committed Correction',CONVERT(varchar(50),x.OrderID),
           0,b.PhysicalStock,b.PhysicalStock,b.OldCommitted,b.NewCommitted,GETDATE()
    FROM @Expected x JOIN @Before b ON b.ItemID=x.ItemID;
    IF @@ROWCOUNT<>23 THROW 53712, 'Unexpected audit count. Nothing committed.', 1;
    -- CEBU_REPAIR_CORE_END
    COMMIT TRANSACTION;
    SELECT N'COMMITTED: 23 commitments corrected; physical stock unchanged.' AS Result;
    SELECT x.ItemID,x.ItemCode,x.OrderID,b.OldCommitted,b.NewCommitted,
           b.PhysicalStock AS PhysicalStockUnchanged
    FROM @Expected x JOIN @Before b ON b.ItemID=x.ItemID ORDER BY x.ItemID;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
