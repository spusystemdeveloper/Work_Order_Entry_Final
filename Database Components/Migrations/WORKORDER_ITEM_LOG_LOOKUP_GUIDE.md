# Optional Work Order item-log lookup index

`Improve_WorkOrder_ItemLogLookup.sql` adds a non-unique covering index to
`dbo.SOD_Item_Logs`. It changes no application code, trigger definitions, log
rows, stock, prices, approvals, tax, queue rules or RMS identity values.
It targets the slow previous-commitment read inside `trg_OrderEntry_ItemLogs`.

The integer-reference safety checks use `TRY_CAST(ReferenceID AS int)` so SQL
Server 2016 databases retaining compatibility level 100 can run them. The first
restored HO test rejected the former `TRY_CONVERT` syntax at level 100 before
creating an index. No compatibility-level change is needed. Invalid/overflow
references and conflicting latest commitment values still stop deployment;
the trigger's original numeric comparison remains unchanged.

The reviewed columns are non-null `ItemID int`, `ReferenceID varchar(50)`,
`NewCommitted int` and `[Date] datetime`. Other branch schemas stop for review.
The index keys are `(ItemID, [Date] DESC)` with `ReferenceID, NewCommitted`
included. The legacy trigger compares text ReferenceID with an integer order ID;
putting ReferenceID ahead of Date would not provide the same ordered seek for
that unchanged comparison. This index seeks the item, visits newest dates first
and evaluates the original reference comparison. Leading-zero references retain
their numeric comparison behavior. Older orders with many newer logs for the
same item can still require additional reads; this is not a constant-time promise.

## Deployment with the updater

1. Back up the intended branch database and test on a disposable restore first.
2. Build the current Database Updater. Its package includes this SQL file and
   guide. Connect to the intended server/database and verify the displayed target.
3. Select only `Migrations/Improve_WorkOrder_ItemLogLookup.sql` under
   **Index / performance**, unless other changes have been reviewed separately.
   It is optional for GSC, DVO and Portable profiles, not an automatic branch-driver
   change. The updater provides the transaction and records the script/hash.
4. Schedule a maintenance window with enough data/log and tempdb sort space. The migration holds
   an exclusive item-log table lock through commit, checks existing references
   and latest timestamp ties, then builds the index. This blocks log writers and
   can block RMS operations. Large tables can take minutes. The updater currently
   has a 120-second command timeout; for a larger table, use an SSMS maintenance
   deployment with an appropriate query timeout after rehearsing on a restore.
   A timeout/failure must roll back the selection.
   Do not run this as a routine cashier operation or concurrently with saves.
5. After a committed result, compare the same lookup values/read counts, then
   verify new/imported orders, editing, cancellation and queue processing in the
   restored branch. Confirm logging, commitments and RMS links before live rollout.

No updated Work Order Entry EXE is required for the database index. Adding the
repository files alone does not install an index. The October 16 authorized
localhost HO restore test committed the index locally; no live installation
was performed. See [the restored HO results](../Tests/HO_RESTORED_ITEMLOG_INDEX_RESULTS.md).

## Manual SSMS execution

For the reviewed live HO target, use the complete standalone file
[Deploy_HO_PO_DB_ItemLogLookup_LIVE.sql](../Recovery/Deploy_HO_PO_DB_ItemLogLookup_LIVE.sql).
It embeds this migration with BEGIN TRANSACTION, COMMIT and TRY/CATCH rollback
already included. At the user's request, the explicit server-name, database-name
and existing-transaction checks were removed from this standalone file. Manually
verify the connected server and use a connection without an open transaction;
USE [HO_PO_DB] still selects the database. Migration data/schema guards remain.
It commits when all safeguards pass; require the final COMMITTED result, not
just CREATED/REUSED. Execute the entire file personally in SSMS during a backed-up
maintenance window, with sufficient data/log/tempdb space and a query timeout
of 1800 seconds. The local rehearsal took almost 11 minutes; live can differ.
Do not import this standalone wrapper into the updater. Preparing the file is
not evidence of live deployment; it has not been run on live by the assistant.

For other reviewed targets or a rollback rehearsal, use the manual pattern below.
Select the intended database explicitly. Keep all statements on the same SSMS
connection. Unlike the older item-code migration, this migration requires an
existing transaction and never commits or rolls back its caller's work.

```sql
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 5000;
BEGIN TRANSACTION;
-- Run the complete contents of Improve_WorkOrder_ItemLogLookup.sql here.
-- Inspect the CREATED / REUSED result and verify the expected database.
ROLLBACK TRANSACTION; -- rehearsal; COMMIT only for an approved deployment
```

Use TRY/CATCH in an unattended wrapper to roll back on error. In interactive
SSMS, if any error occurs, execute `IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;`
immediately; do not leave the transaction/table lock open. For an actual install,
run the full sequence again and commit instead of rolling back. Index creation
is offline and may hold a table lock for much longer than the lock-wait limit.

## Guards and limitations

- Enabled, unfiltered covering indexes beginning with ItemID/Date are reused;
  either ascending or descending Date can support ordered access for one item.
  Conflicting/disabled/filtered indexes with the proposed name are not replaced.
- Before creating an index, non-integer or overflow ReferenceIDs stop for branch
  review. The script does not rewrite references or silently change the legacy
  comparison to text equality. Do not delete existing history to force it through.
- The existing `TOP (1) ... ORDER BY [Date] DESC` has no tie-break rule. If the
  latest rows for the same item/numeric reference have different NewCommitted
  values, preflight stops. Equal-valued ties and ambiguous older dates are allowed
  because they do not change the current newest result. The table lock keeps
  these checks consistent with index creation. Future conflicting timestamp ties
  remain an ambiguity in the original trigger; this migration invents no new
  business rule to resolve them.
- Existing usable indexes are reused without creating/revalidating data. Reuse
  is not a declaration that every branch trigger or existing log row is correct.
- The HO-specific guarded timeout repair remains separate. This migration does
  not replace that script, remove its guards, or install its trigger patch in
  another branch. It also does not repair previously mislinked transactions.

## Rollback

Record whether the result was CREATED or REUSED. If this deployment created the
index and its definition is still the one above, a DBA can remove only that index:

```sql
DROP INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs;
```

Do not drop a reused pre-existing index or one another collaborator has changed.
Dropping this index does not delete log rows; it restores the unindexed access
cost. The index adds disk space and maintenance work to future log writes, so
assess the complete restored-branch save workload, not only lookup speed.

## Validation

Run `Database Components/Tests/ItemLogLookup_LocalDB_Test.ps1` with Windows
PowerShell and LocalDB installed. It creates and cleans up a uniquely owned
database at SQL compatibility level 130. It compares the unchanged legacy
trigger's insert/edit/zero-quantity log values, fractional quantities, stock,
numeric reference matching and all existing log columns. It also checks caller
rollback, repeat/reuse behavior, unsafe data/schema/index rejection and the
unchanged lookup's logical reads against a wide synthetic log heap.

The local baseline audit on October 15, 2026 found approximately 9.5 million
item-log rows with no index in GSC_STORE_DB. One existing-order lookup took
1,650 ms and then 918 ms. Those were SELECT-only component measurements, not
whole-save timings. Synthetic before/after evidence is stored under ignored
`TestResults/ItemLogLookup-*`; it does not establish live-store performance.

For the October 16 compatibility adjustment, run the same disposable checks at
both levels (the switch affects only the owned fixture):

```powershell
& '.\Database Components\Tests\ItemLogLookup_LocalDB_Test.ps1' -CompatibilityLevel 100
& '.\Database Components\Tests\ItemLogLookup_LocalDB_Test.ps1' -CompatibilityLevel 130
```

All 31 checks passed at each level. The default remains 130. These fixtures do
not execute RMS POS or prove complete restored-store or live-store save timing.

On October 15, all 31 item-log checks passed. The unchanged lookup on a 100,003-row
synthetic heap read 5,883 pages before indexing and 4 afterward. The final run took
66.975 ms versus 6.555 ms; these are fixture component timings, not guaranteed
store timings. All 71 updater checks, 39 existing multi-user checks per isolation
mode and 101 unit tests passed; both solutions built Release/x86. The updater
integration verifies index/history rollback if a later selected script fails.
