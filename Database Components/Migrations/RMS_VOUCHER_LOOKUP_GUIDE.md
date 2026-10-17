# Optional RMS voucher lookup index

Select **Improve_RMS_VoucherLookup.sql** in Database Updater to add the tested
non-unique Item index on `(TenderID, IsGlobalVoucher, Inactive)`. It is listed for
**DVO**, **GSC** and **Portable** profiles and is unchecked until you select it.
Distribute the whole rebuilt updater Release folder, including Database Components.

This supports RMS's Item/Tender global-voucher lookup. The local Davao comparison
reproduced its conflict with Work Order commitment updates; the index changed
the Item scan to a seek. Six compiled recalled-order saves plus SQL lookup replay
improved from a 9.320-second median to 1.351 seconds, with lookup deadlock victim
errors falling from four to zero. This was an older disposable Davao restore,
not actual RMS POS/QSRules or a sustained 10+ user test. See
`../Tests/DVO_LOCAL_DEADLOCK_INDEX_RESULTS.md` for the measurement and limits.

The migration has no fixed server, database, StoreID or Tender ID. It checks the
selected database's existing `dbo.Item` filter columns: regular native `int`
TenderID and `bit` IsGlobalVoucher/Inactive. Different definitions stop for review;
the migration never converts columns or changes branch settings to make it fit.
Matching enabled, unfiltered, non-unique indexes with these three leading keys
are reused even under another name. A conflicting index named
`IX_SOD_RMS_VoucherLookup` stops the migration; nothing is dropped or rebuilt.

No Item/Tender rows, prices, stock, commitments, transactions, triggers, queries,
approval rules, isolation settings or identity values are modified. The updater
records its normal deployment history. This is separate from
**Improve_WorkOrder_ItemLogLookup.sql**, which supports reads of SOD_Item_Logs.
One index does not replace the other, and neither guarantees every slow operation
or every possible deadlock is fixed.

## Deploy one branch at a time

1. Have a current backup and rehearse on a current isolated restore of that
   branch. Test actual RMS lookup/tendering and Work Order save/edit/recall there.
   GSC and other live branches have not been performance-tested by this change.
2. Choose a maintenance window with POS and Work Order activity paused. Index
   creation needs schema locks and extra disk/log space and can block users.
   The updater retains its five-second lock wait and two-minute command limit;
   a timeout rolls back. Do not increase limits or retry during peak traffic as
   a substitute for reviewing a larger branch's deployment requirements.
3. Enter the exact target server/database, choose the branch's profile (Portable
   for other branches), and click **Check target**. The profile does not infer
   the server or database. Tick **only Improve_RMS_VoucherLookup.sql** to deploy
   just this fix; other selected objects can have different branch behavior.
4. Preview the SQL, choose **Review and apply**, confirm the existing backup/test
   requirement and exact database name, then apply. No script runs automatically.
5. After **COMMITTED**, check the target record again and test RMS tender lookup
   and Work Order save/edit/recall. Compare waits/deadlocks under representative
   activity; short local success does not certify 10+ cashier capacity.

All selected scripts and success records share the updater's transaction. A later
selected-script failure rolls back a newly created index and its success record.
Repeat deployment is supported and reuses matching coverage. A recorded hash
means the script ran; it does not prove an index has not subsequently drifted.

## SSMS and rollback

Prefer the updater for target review, schema snapshots and execution records.
For a separately authorized SSMS deployment, select the intended database in the
query-window selector, start an explicit transaction, run the migration, inspect
its CREATED/REUSED result, and explicitly commit or roll back. The migration does
not start/commit a transaction, switch databases or embed credentials.

After a committed deployment there is no automatic downgrade. If removal is
necessary, review it separately in a maintenance window and remove only the
newly created `IX_SOD_RMS_VoucherLookup` after confirming its exact definition.
Do not drop another branch's reused index. Removing it restores the possibility
of the original scans/conflicts. No live deployment was executed during development.
