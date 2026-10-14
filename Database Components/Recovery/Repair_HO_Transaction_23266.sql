/* Incident-specific live HO recovery. Not automatically included in the updater.
   Requires a verified full backup and an atomic caller transaction.
   The old 2021 transaction 17 is valid; do not bulk-relink its records. */
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
IF DB_NAME()<>N'HO_PO_DB' THROW 52600, 'This recovery is only for the diagnosed HO_PO_DB.', 1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601, 'Recovery requires a caller transaction and verified backup.', 1;
IF ISNULL(COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity'),-1)<>0
    THROW 52602, 'Apply the history identity compatibility fix first.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK) WHERE TransactionNumber=23266
    AND StoreID=606 AND BatchNumber=80 AND RecallID=18870 AND RecallType=4 AND [Time]='20261014 13:53:01'
    AND CustomerID=71 AND Total=30200.54 AND SalesTax=0)
    THROW 52603, 'The diagnosed transaction header no longer matches.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.[Transaction] WITH(UPDLOCK,HOLDLOCK) WHERE TransactionNumber=17
    AND StoreID=606 AND BatchNumber=7 AND [Time]='20210310 08:32:20' AND Total=514088.54)
    THROW 52604, 'The unrelated old transaction header differs.', 1;
IF EXISTS(SELECT 1 FROM sys.triggers t JOIN sys.trigger_events e ON e.object_id=t.object_id
    WHERE e.type_desc=N'UPDATE' AND t.is_disabled=0 AND t.parent_id IN
    (OBJECT_ID(N'dbo.TransactionEntry'),OBJECT_ID(N'dbo.TenderEntry'),OBJECT_ID(N'dbo.AccountReceivable'),OBJECT_ID(N'dbo.OrderHistory'),OBJECT_ID(N'dbo.TaxEntry'),OBJECT_ID(N'dbo.SOD_Item_Logs')))
    THROW 52605, 'An enabled UPDATE trigger requires side-effect review.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=606
    AND TransactionNumber IN(17,23266) AND TransactionTime='20261014 13:53:01' AND SalesTax=0
    AND ((ID=65233 AND ItemID=13879 AND Quantity=1 AND Price=7690.49)
     OR(ID=65234 AND ItemID=13881 AND Quantity=1 AND Price=5139.24)
     OR(ID=65235 AND ItemID=46608 AND Quantity=3 AND Price=4077.19)
     OR(ID=65236 AND ItemID=46609 AND Quantity=1 AND Price=5139.24)))<>4
 OR EXISTS(SELECT 1 FROM dbo.TransactionEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID NOT IN(65233,65234,65235,65236))
    THROW 52606, 'Item lines differ from the diagnosed sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.TenderEntry WITH(UPDLOCK,HOLDLOCK) WHERE ID=25815 AND StoreID=606
    AND TransactionNumber IN(17,23266) AND BatchNumber=80 AND OrderHistoryID=19857 AND TenderID=2 AND PaymentID=0 AND Amount=30200.54)
 OR EXISTS(SELECT 1 FROM dbo.TenderEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID<>25815)
    THROW 52607, 'Tender differs from the diagnosed charge sale.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WITH(UPDLOCK,HOLDLOCK) WHERE ID=24403 AND StoreID=606 AND CustomerID=71
    AND TransactionNumber IN(17,23266) AND [Date]='20261014 13:53:01' AND Type=0 AND OriginalAmount=30200.54 AND Balance=30200.54)
    THROW 52608, 'Receivable differs or has subsequent payment activity.', 1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WITH(UPDLOCK,HOLDLOCK) WHERE ID=19857 AND StoreID=606 AND OrderID=18870
    AND TransactionNumber IN(17,23266) AND BatchNumber=80 AND [Date]='20261014 13:53:01' AND DeltaDeposit=0)
    THROW 52609, 'Order history differs from the diagnosed close.', 1;
IF (SELECT COUNT(*) FROM dbo.TaxEntry WITH(UPDLOCK,HOLDLOCK) WHERE StoreID=606 AND TransactionNumber IN(17,23266) AND Tax=0
    AND ((ID=65198 AND TransactionEntryID=65233 AND TaxableAmount=7690.49)
      OR(ID=65199 AND TransactionEntryID=65234 AND TaxableAmount=5139.24)
      OR(ID=65200 AND TransactionEntryID=65235 AND TaxableAmount=12231.57)
      OR(ID=65201 AND TransactionEntryID=65236 AND TaxableAmount=5139.24)))<>4
 OR EXISTS(SELECT 1 FROM dbo.TaxEntry WHERE StoreID=606 AND TransactionNumber=23266 AND ID NOT IN(65198,65199,65200,65201))
    THROW 52610, 'Tax links differ from the diagnosed sale.', 1;

SELECT * INTO #EntryBefore FROM dbo.TransactionEntry WHERE ID IN(65233,65234,65235,65236);
SELECT * INTO #TenderBefore FROM dbo.TenderEntry WHERE ID=25815;
SELECT * INTO #ARBefore FROM dbo.AccountReceivable WHERE ID=24403;
SELECT * INTO #OrderHistoryBefore FROM dbo.OrderHistory WHERE ID=19857;
SELECT * INTO #TaxBefore FROM dbo.TaxEntry WHERE ID IN(65198,65199,65200,65201);
SELECT * INTO #ItemsBefore FROM dbo.Item WHERE ID IN(13879,13881,46608,46609);
-- Preserve every unrelated old transaction-17 row, including rowversions.
DECLARE @oldEntries nvarchar(max)=(SELECT * FROM dbo.TransactionEntry WHERE TransactionNumber=17 AND ID NOT IN(65233,65234,65235,65236) ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldTenders nvarchar(max)=(SELECT * FROM dbo.TenderEntry WHERE TransactionNumber=17 AND ID<>25815 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldAR nvarchar(max)=(SELECT * FROM dbo.AccountReceivable WHERE TransactionNumber=17 AND ID<>24403 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldOrders nvarchar(max)=(SELECT * FROM dbo.OrderHistory WHERE TransactionNumber=17 AND ID<>19857 ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldTax nvarchar(max)=(SELECT * FROM dbo.TaxEntry WHERE TransactionNumber=17 AND ID NOT IN(65198,65199,65200,65201) ORDER BY ID FOR XML PATH('Row'));
DECLARE @oldHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=17 FOR XML PATH('Row'));
DECLARE @newHeader nvarchar(max)=(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=23266 FOR XML PATH('Row'));

UPDATE dbo.TransactionEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID IN(65233,65234,65235,65236);
UPDATE dbo.TenderEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=25815;
UPDATE dbo.AccountReceivable SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=24403;
UPDATE dbo.OrderHistory SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID=19857;
UPDATE dbo.TaxEntry SET TransactionNumber=23266 WHERE StoreID=606 AND TransactionNumber=17 AND ID IN(65198,65199,65200,65201);

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
        N' WHERE ID IN(SELECT ID FROM '+@before+N')) THROW 52611, ''A non-link field changed; roll back recovery.'', 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM checks INTO @table,@before;
END;
CLOSE checks; DEALLOCATE checks;
IF EXISTS(SELECT ID,Quantity,QuantityCommitted FROM #ItemsBefore EXCEPT SELECT ID,Quantity,QuantityCommitted FROM dbo.Item)
 OR @oldHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=17 FOR XML PATH('Row'))
 OR @newHeader<>(SELECT * FROM dbo.[Transaction] WHERE TransactionNumber=23266 FOR XML PATH('Row'))
 OR @oldEntries<>(SELECT * FROM dbo.TransactionEntry WHERE TransactionNumber=17 AND ID NOT IN(65233,65234,65235,65236) ORDER BY ID FOR XML PATH('Row'))
 OR @oldTenders<>(SELECT * FROM dbo.TenderEntry WHERE TransactionNumber=17 AND ID<>25815 ORDER BY ID FOR XML PATH('Row'))
 OR @oldAR<>(SELECT * FROM dbo.AccountReceivable WHERE TransactionNumber=17 AND ID<>24403 ORDER BY ID FOR XML PATH('Row'))
 OR @oldOrders<>(SELECT * FROM dbo.OrderHistory WHERE TransactionNumber=17 AND ID<>19857 ORDER BY ID FOR XML PATH('Row'))
 OR @oldTax<>(SELECT * FROM dbo.TaxEntry WHERE TransactionNumber=17 AND ID NOT IN(65198,65199,65200,65201) ORDER BY ID FOR XML PATH('Row'))
    THROW 52612, 'Inventory, transaction headers or unrelated old records changed.', 1;
IF (SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=23266 AND StoreID=606)<>4
 OR (SELECT CONVERT(money,ROUND(SUM(Quantity*Price),2)) FROM dbo.TransactionEntry WHERE TransactionNumber=23266 AND StoreID=606)<>30200.54
 OR (SELECT COUNT(*) FROM dbo.TenderEntry WHERE TransactionNumber=23266 AND StoreID=606)<>1
 OR (SELECT SUM(Amount) FROM dbo.TenderEntry WHERE TransactionNumber=23266 AND StoreID=606)<>30200.54
 OR (SELECT COUNT(*) FROM dbo.TaxEntry WHERE TransactionNumber=23266 AND StoreID=606)<>4
 OR NOT EXISTS(SELECT 1 FROM dbo.AccountReceivable WHERE ID=24403 AND TransactionNumber=23266)
 OR NOT EXISTS(SELECT 1 FROM dbo.OrderHistory WHERE ID=19857 AND TransactionNumber=23266)
    THROW 52613, 'Final transaction linkage verification failed.', 1;
DROP TABLE #EntryBefore;DROP TABLE #TenderBefore;DROP TABLE #ARBefore;
DROP TABLE #OrderHistoryBefore;DROP TABLE #TaxBefore;DROP TABLE #ItemsBefore;
/* The four historical item-log references and compressed receipt are evidence.
   They are handled separately; this core transaction repair does not rewrite them. */
