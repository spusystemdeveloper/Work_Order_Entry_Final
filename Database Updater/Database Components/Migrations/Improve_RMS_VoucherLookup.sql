/* Optional SQL Server 2016 RMS Item/Tender voucher-lookup index.
   Tested key order: TenderID, IsGlobalVoucher, Inactive. No branch/tender IDs,
   data, triggers, query hints, isolation settings or business rules are changed.
   The updater owns the transaction. In SSMS use an explicit caller transaction.
   Index creation can block POS/Work Order traffic: test a restore and use a
   maintenance window. See RMS_VOUCHER_LOOKUP_GUIDE.md.
*/
IF @@TRANCOUNT = 0
    THROW 53710, 'An explicit transaction is required. Use Database Updater or follow RMS_VOUCHER_LOOKUP_GUIDE.md.', 1;

DECLARE @ItemTable int=OBJECT_ID(N'dbo.Item',N'U');
IF @ItemTable IS NULL
    THROW 53711, 'dbo.Item is missing. No voucher index created.', 1;
IF (SELECT COUNT(*) FROM sys.columns
    WHERE object_id=@ItemTable AND user_type_id=system_type_id AND is_computed=0
      AND ((name=N'TenderID' AND system_type_id=56)
        OR (name IN(N'IsGlobalVoucher',N'Inactive') AND system_type_id=104))) <> 3
    THROW 53712, 'Item voucher column types differ from reviewed int/bit/bit columns. Review this branch; no index created.', 1;

-- Serialize this migration's metadata check and creation until caller completion.
DECLARE @LockResult int;
EXEC @LockResult=sys.sp_getapplock @Resource=N'SOD_RMS_VoucherLookup_Index',
    @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=5000;
IF @LockResult < 0
    THROW 53713, 'Could not acquire the voucher-index deployment lock. Retry in a maintenance window.', 1;

DECLARE @Usable TABLE(IndexID int PRIMARY KEY);
INSERT @Usable(IndexID)
SELECT ix.index_id FROM sys.indexes ix
WHERE ix.object_id=@ItemTable AND ix.type=2 AND ix.is_unique=0
  AND ix.is_disabled=0 AND ix.is_hypothetical=0 AND ix.has_filter=0
  AND NOT EXISTS(
    SELECT wanted.Ordinal,wanted.ColumnName
    FROM (VALUES(1,N'TenderID'),(2,N'IsGlobalVoucher'),(3,N'Inactive')) wanted(Ordinal,ColumnName)
    WHERE NOT EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.columns c
        ON c.object_id=ic.object_id AND c.column_id=ic.column_id
        WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id
          AND ic.key_ordinal=wanted.Ordinal AND c.name=wanted.ColumnName));

IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@ItemTable
    AND name=N'IX_SOD_RMS_VoucherLookup' AND index_id NOT IN(SELECT IndexID FROM @Usable))
    THROW 53714, 'IX_SOD_RMS_VoucherLookup has another definition or is disabled/filtered/unique. Review it; nothing is replaced.', 1;

IF NOT EXISTS(SELECT 1 FROM @Usable)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SOD_RMS_VoucherLookup
        ON dbo.Item(TenderID,IsGlobalVoucher,Inactive);
    SELECT N'CREATED: IX_SOD_RMS_VoucherLookup. Business rows and triggers unchanged; caller must commit or roll back.' Result;
END
ELSE
    SELECT N'REUSED: an enabled, unfiltered non-unique TenderID/IsGlobalVoucher/Inactive index already exists.' Result;
