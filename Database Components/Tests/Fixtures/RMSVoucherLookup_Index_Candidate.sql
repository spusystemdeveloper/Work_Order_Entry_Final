/* TEST CANDIDATE ONLY. Not a live deployment or an updater selection.
   Run inside a caller-owned transaction on a GUID-owned localhost clone.
   Tested lookup: Item/Tender, TenderID equality, IsGlobalVoucher=1, Inactive=0.
   No query hints, trigger edits, isolation changes or business-row updates.
*/
IF CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')) <> HOST_NAME()
    THROW 53700, 'This candidate is restricted to SQL Server on the client computer.', 1;
IF LEN(DB_NAME()) <> 17 OR DB_NAME() NOT LIKE N'DVO[_]PERF[_]%'
    OR RIGHT(DB_NAME(),8) COLLATE Latin1_General_BIN2 LIKE N'%[^0-9a-f]%'
    THROW 53701, 'An owned DVO_PERF_<eight hex characters> disposable clone is required.', 1;
IF @@TRANCOUNT = 0
    THROW 53702, 'The test coordinator must supply the transaction.', 1;
IF (SELECT COUNT(*) FROM sys.columns
    WHERE object_id=OBJECT_ID(N'dbo.Item',N'U') AND user_type_id=system_type_id
      AND ((name=N'TenderID' AND system_type_id=56)
        OR (name IN(N'IsGlobalVoucher',N'Inactive') AND system_type_id=104))) <> 3
    THROW 53703, 'The Item filter-column types differ from the reviewed Davao restore.', 1;
IF EXISTS(SELECT 1 FROM sys.indexes
    WHERE object_id=OBJECT_ID(N'dbo.Item') AND name=N'IX_DVO_Perf_Voucher')
    THROW 53704, 'The candidate name already exists. Do not replace an unreviewed index.', 1;

CREATE NONCLUSTERED INDEX IX_DVO_Perf_Voucher
    ON dbo.Item(TenderID,IsGlobalVoucher,Inactive);
