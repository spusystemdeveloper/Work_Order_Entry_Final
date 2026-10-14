/* Incident-specific recovery, not a general migration or automatic package item.
   Requires a verified backup and an explicit caller transaction.
   Only relinks confirmed records. Never repost the sale or recalculate inventory. */
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
IF DB_NAME()<>N'GSC_STORE_DB' OR CONVERT(sysname,SERVERPROPERTY('MachineName'))<>N'MSI'
    THROW 52500, 'This recovery is only for the diagnosed local GSC database on MSI.', 1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1
    THROW 52501, 'Recovery requires an atomic caller transaction and verified backup.', 1;
IF ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity'),-1)<>0
    THROW 52502, 'Apply the history identity compatibility fix first.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK)
    WHERE TransactionNumber=289655 AND StoreID=600 AND BatchNumber=241354
    AND RecallID=283845 AND RecallType=4 AND [Time]='20261014 14:12:14'
    AND CustomerID=1355 AND Total=5394.25 AND SalesTax=577.96)
    THROW 52503, 'The diagnosed transaction header no longer matches.', 1;
IF EXISTS(SELECT 1 FROM sys.triggers t JOIN sys.trigger_events e ON e.object_id=t.object_id
    WHERE e.type_desc=N'UPDATE' AND t.is_disabled=0 AND t.parent_id IN
    (OBJECT_ID(N'dbo.TransactionEntry'),OBJECT_ID(N'dbo.TenderEntry'),OBJECT_ID(N'dbo.AccountReceivable'),OBJECT_ID(N'dbo.OrderHistory'),OBJECT_ID(N'dbo.TaxEntry'),OBJECT_ID(N'dbo.SOD_Item_Logs')))
    THROW 52504, 'An enabled UPDATE trigger requires additional side-effect review.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=600 AND
    TransactionNumber IN(10128,289655) AND ((ID=957676 AND ItemID=59622 AND Quantity=1 AND Price=5319 AND SalesTax=569.8929)
    OR(ID=957677 AND ItemID=1250 AND Quantity=1 AND Price=75.25 AND SalesTax=8.0625)))<>2
 OR EXISTS(SELECT 1 FROM dbo.TransactionEntry WHERE StoreID=600 AND TransactionNumber=289655 AND ID NOT IN(957676,957677))
    THROW 52505, 'Item lines differ from the diagnosed sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.TenderEntry WITH(UPDLOCK,HOLDLOCK) WHERE ID=390053 AND StoreID=600
    AND TransactionNumber IN(10128,289655) AND BatchNumber=241354 AND OrderHistoryID=268282 AND TenderID=2 AND PaymentID=0 AND Amount=5394.25)
 OR EXISTS(SELECT 1 FROM dbo.TenderEntry WHERE StoreID=600 AND TransactionNumber=289655 AND ID<>390053)
    THROW 52506, 'Tender differs from the diagnosed charge sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WITH(UPDLOCK,HOLDLOCK) WHERE ID=133523 AND StoreID=600 AND CustomerID=1355
    AND TransactionNumber IN(10128,289655) AND [Date]='20261014 14:12:14' AND Type=0 AND OriginalAmount=5394.25 AND Balance=5394.25)
    THROW 52507, 'Receivable differs or has subsequent payment activity.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WITH(UPDLOCK,HOLDLOCK) WHERE ID=268282 AND StoreID=600 AND OrderID=283845
    AND TransactionNumber IN(10128,289655) AND BatchNumber=241354 AND [Date]='20261014 14:12:14' AND DeltaDeposit=0)
    THROW 52508, 'Order history differs from the diagnosed close.', 1;
IF (SELECT COUNT(*) FROM dbo.TaxEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=600 AND TransactionNumber IN(10128,289655)
    AND ((ID=957521 AND TransactionEntryID=957676 AND Tax=569.8929 AND TaxableAmount=4749.1071)
      OR(ID=957522 AND TransactionEntryID=957677 AND Tax=8.0625 AND TaxableAmount=67.1875)))<>2
 OR EXISTS(SELECT 1 FROM dbo.TaxEntry WHERE StoreID=600 AND TransactionNumber=289655 AND ID NOT IN(957521,957522))
    THROW 52509, 'Tax links differ from the diagnosed sale.', 1;
IF (SELECT COUNT(*) FROM dbo.SOD_Item_Logs WITH(UPDLOCK,HOLDLOCK)
    WHERE ReferenceID IN('10128','289655') AND [Type]='Sales Transaction Logs' AND QuantityORIS=1
    AND ((ItemID=59622 AND [Date]='20261014 14:12:16.650') OR(ItemID=1250 AND [Date]='20261014 14:12:16.667')))<>2
    THROW 52510, 'The two exact item-log references no longer match.', 1;

SELECT * INTO #EntryBefore FROM dbo.TransactionEntry WHERE ID IN(957676,957677);
SELECT * INTO #TenderBefore FROM dbo.TenderEntry WHERE ID=390053;
SELECT * INTO #ARBefore FROM dbo.AccountReceivable WHERE ID=133523;
SELECT * INTO #OrderHistoryBefore FROM dbo.OrderHistory WHERE ID=268282;
SELECT * INTO #TaxBefore FROM dbo.TaxEntry WHERE ID IN(957521,957522);
SELECT * INTO #ItemsBefore FROM dbo.Item WHERE ID IN(59622,1250);
DECLARE @oldHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=10128 FOR XML PATH('Row'));
DECLARE @newHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=289655 FOR XML PATH('Row'));

UPDATE dbo.TransactionEntry SET TransactionNumber=289655 WHERE StoreID=600 AND TransactionNumber=10128 AND ID IN(957676,957677);
UPDATE dbo.TenderEntry SET TransactionNumber=289655 WHERE StoreID=600 AND TransactionNumber=10128 AND ID=390053;
UPDATE dbo.AccountReceivable SET TransactionNumber=289655 WHERE StoreID=600 AND TransactionNumber=10128 AND ID=133523;
UPDATE dbo.OrderHistory SET TransactionNumber=289655 WHERE StoreID=600 AND TransactionNumber=10128 AND ID=268282;
UPDATE dbo.TaxEntry SET TransactionNumber=289655 WHERE StoreID=600 AND TransactionNumber=10128 AND ID IN(957521,957522);
UPDATE dbo.SOD_Item_Logs SET ReferenceID='289655' WHERE ReferenceID='10128' AND [Type]='Sales Transaction Logs' AND QuantityORIS=1
    AND ((ItemID=59622 AND [Date]='20261014 14:12:16.650') OR(ItemID=1250 AND [Date]='20261014 14:12:16.667'));

-- Compare every other persisted field, including costs, taxes and tender details.
DECLARE @table sysname,@before sysname,@columns nvarchar(max),@sql nvarchar(max);
DECLARE checks CURSOR LOCAL FAST_FORWARD FOR SELECT * FROM(VALUES
 (N'TransactionEntry',N'#EntryBefore'),(N'TenderEntry',N'#TenderBefore'),
 (N'AccountReceivable',N'#ARBefore'),(N'OrderHistory',N'#OrderHistoryBefore'),(N'TaxEntry',N'#TaxBefore')) v(t,b);
OPEN checks; FETCH NEXT FROM checks INTO @table,@before;
WHILE @@FETCH_STATUS=0
BEGIN
    SELECT @columns=STUFF((SELECT N','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.'+@table)
        AND name<>N'TransactionNumber' AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,N'');
    SET @sql=N'IF EXISTS(SELECT '+@columns+N' FROM '+@before+N' EXCEPT SELECT '+@columns+N' FROM dbo.'+QUOTENAME(@table)+
        N' WHERE ID IN(SELECT ID FROM '+@before+N')) THROW 52511, ''A non-link field changed; roll back recovery.'', 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM checks INTO @table,@before;
END;
CLOSE checks; DEALLOCATE checks;
IF EXISTS(SELECT ID,Quantity,QuantityCommitted FROM #ItemsBefore EXCEPT SELECT ID,Quantity,QuantityCommitted FROM dbo.Item)
 OR @oldHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=10128 FOR XML PATH('Row'))
 OR @newHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=289655 FOR XML PATH('Row'))
    THROW 52512, 'Inventory or transaction headers changed; roll back recovery.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=289655 AND StoreID=600)<>2
 OR (SELECT CONVERT(money,ROUND(SUM(Quantity*Price),2)) FROM dbo.TransactionEntry WHERE TransactionNumber=289655 AND StoreID=600)<>5394.25
 OR (SELECT COUNT(*) FROM dbo.TenderEntry WHERE TransactionNumber=289655 AND StoreID=600)<>1
 OR (SELECT SUM(Amount) FROM dbo.TenderEntry WHERE TransactionNumber=289655 AND StoreID=600)<>5394.25
 OR (SELECT COUNT(*) FROM dbo.TaxEntry WHERE TransactionNumber=289655 AND StoreID=600)<>2
 OR NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WHERE ID=133523 AND TransactionNumber=289655)
 OR NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WHERE ID=268282 AND TransactionNumber=289655)
    THROW 52513, 'Final transaction linkage verification failed.', 1;
DROP TABLE #EntryBefore;DROP TABLE #TenderBefore;DROP TABLE #ARBefore;
DROP TABLE #OrderHistoryBefore;DROP TABLE #TaxBefore;DROP TABLE #ItemsBefore;
