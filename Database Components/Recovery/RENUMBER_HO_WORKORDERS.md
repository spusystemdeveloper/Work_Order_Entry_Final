# Requested HO Work Order renumbering

Prepared October 15, 2026 for **manual execution by the user**. The assistant has
only queried live data and metadata; it has not run the renumber script on live,
even in rollback mode, and has not reseeded anything.

Use [Renumber_HO_WorkOrders_18870_18879.sql](Renumber_HO_WorkOrders_18870_18879.sql).
This is a standalone SSMS maintenance file, **not a Database Updater migration**.
It changes existing order identifiers, not normal Work Order processing rules.
It is separate from the history identity compatibility fix and save-timeout fix.

| Original Order ID | Requested Order ID | Audited state |
| --- | --- | --- |
| 18870 | 18869 | Closed; transaction 23266; total 30,200.54 |
| 18879 | 18870 | Open; total 11,042.76 |

The read-only audit found OrderEntry, Queueing.OrderID/GroupTo, RMS OrderHistory,
Transaction.RecallID, Journal.ReferenceID, price approvals, custom WO history and
order-type item logs. Journal 23804 is a compressed **order** receipt
(TransactionType 2), linked to OrderHistory 19857. The actual transaction number
**23266**, history ID **19857**, queue IDs, entry IDs, approval IDs and receipt ID
**23804** are preserved. Sale transaction numbers are not renumbered.

The stored receipt bytes/text, existing free-text history descriptions, business
ReferenceNumber fields, printed documents and external copies remain historical.
For example, the first order's ReferenceNumber already reads `ITS-0018869` and is
retained. The earlier transaction-23266 repair retained the original receipt
contents, which may still display its old printed transaction number. This
renumbering does not rewrite that receipt or correct its embedded contents.
Keep the mapping with the incident record so historical copies remain traceable.
Two explicit `Order ID changed` history events are appended on committed runs.

## Run it

1. Schedule maintenance and close **all** RMS POS/Manager, Work Order Entry,
   queue and integration clients for HO. Reopen clients after the work is
   finished so no recalled order or cached old ID is reused.
2. Take a fresh **copy-only full HO_PO_DB backup with checksums**. Wait for it
   to finish. The script requires today's backup in this SQL server's `msdb`
   history and runs `RESTORE VERIFYONLY ... WITH CHECKSUM`. A backup pathname
   refers to the SQL server's file system. An October 14 backup will not meet
   an October 15 guard. Do not bypass this guard on live.
3. Connect SSMS to **172.16.4.50**, SQL server **WIN-2847UGJBAV4**, database
   **HO_PO_DB**. Open the whole script in a fresh query window. Set only
   `@BackupPath` to the completed backup's actual full path. Leave `@Commit=0`.
4. Execute the **entire file**. Save all results. It displays the mapping, row
   counts, proposed headers, transaction link, history link and receipt link.
   Final `REHEARSAL ONLY` means the displayed changes were rolled back.
   Check that transaction 23266 points to proposed Order 18869, and proposed
   Order 18870 is the original open order with total 11,042.76.
5. After reviewing those results, set `@Commit=1` and execute the whole file
   again. Only a final **COMMITTED** result confirms the renumber completed.
   Any error rolls the maintenance changes back. Keep the error/results if a
   guard stops execution; do not remove the guard or adjust expected IDs blindly.
6. Verify Order 18869 in RMS transaction/history reports and confirm transaction
   23266 still has the same lines, taxes, tender and receivable. Recall Order
   18870, check its item, price, tax, owner and queue group, then test normal
   recall/edit/save after reopening the applications. Existing receipt contents
   still reflect the original archived document.

## Identity reseeding remains a separate operator step

**The script contains no identity reseed.** Its current-identity guard expects
18879, and both rehearsal and commit verify that this value has not changed.
Save the successful commit results and verify the final maximum Order ID before
running [Reseed_HO_Order_After_Renumber.sql](Reseed_HO_Order_After_Renumber.sql),
saved in this same folder, while all clients remain closed. Execute the entire
reseed file only after the renumber reports **COMMITTED**. It checks the target
server, maximum ID, expected headers, posted links and both committed mapping
events before setting the current identity to 18870. It needs no additional
backup-path setting. A repeat before new activity returns `ALREADY SET`; later
allocations or orders cause a rejection rather than rewinding them.
For an existing nonempty table with increment 1, reseeding its current identity
to **18870** makes the next automatically allocated value **18871**, not 18870.
Never reseed below a higher existing Order ID. See Microsoft's
[DBCC CHECKIDENT documentation](https://learn.microsoft.com/en-us/sql/t-sql/database-console-commands/dbcc-checkident-transact-sql).

Renumbering does not make IDENTITY a gapless document sequence. Failed or rolled
back inserts can consume numbers again. Reusing old IDs can also confuse copies
outside this database; this file is a narrowly scoped, requested maintenance,
not an automatic gap-filling feature.

## Guards, side effects and limits

- The script rejects changed target headers, a new higher Order ID/allocation,
  occupied destination 18869, orphaned destination references, a repeat commit,
  changed posted transaction/history/receipt links, unknown or disabled triggers,
  FKs, schema-bound dependencies, replication, CDC, temporal tables and enabled
  database/server DDL triggers.
- Less common references were empty at audit. If one now exists in `SOD_WO`,
  `SI_ItemExpireTemp`, `SI_OrderPrice`, `SI_PickListTemp`, `QuoteTenderEntry`,
  `RMSTMP_Sales`, held transactions or imported-quote links, the script stops for
  review. Newly added OrderID/RecallID/QuoteID/GroupTo columns also stop it.
  Arbitrarily named custom fields and external databases/files are outside this
  audit; confirm no external consumers rely on the original IDs before commit.
- IDENTITY cannot be updated in place. The script copies both headers, removes
  the originals and explicitly inserts the mapped IDs in one transaction.
  This avoids the overlap where old 18870 is also the second destination.
  Reviewed triggers are temporarily disabled under transaction-owned exclusive
  table locks, then restored. Rollback restores their state on errors. This
  prevents duplicate stock logs and misleading create/delete history events.
- All affected column values are compared against the intended mapping before
  commit. Quantities, prices, tax, deposits, stock and commitments are not edited.
  SQL rowversion/timestamp values necessarily regenerate; old client snapshots
  are invalid. Order logs are mapped only for `Order Entry Logs` and
  `Order Update Logs`; sales and purchase references stay unchanged.
- The 71.7-million-row unindexed item-log table must be scanned, including for
  validation. This maintenance can take time and holds locks; run it in SSMS
  during the maintenance window. Do not execute only highlighted fragments or
  kill the session merely because a scan is taking time.
- Test a fresh restored copy before live execution. For that restore test,
  change `@ExpectedServer` only in a separate test copy to the restored server's
  actual `@@SERVERNAME`, and create a fresh copy-only/checksummed full backup of
  the restored HO_PO_DB there. Keep the production file's live target guard.
  The automated fixture test
  only changes server/backup checks in an in-memory synthetic test copy; the
  distributed live script keeps those checks. No live rehearsal or operator
  commit has been reported for this renumbering.
- Before commit, recovery is transaction rollback. After commit, do not rerun
  this mapping as an undo; subsequent activity requires a separately reviewed
  reverse mapping or full database recovery plan.

## Isolated verification

Run `Database Components/Tests/HO_Renumber_LocalDB_Test.ps1`. It creates a unique
LocalDB instance with synthetic rows and the reviewed trigger definitions, then
drops only its owned database/instance. It verifies default rollback, deliberate
failure after trigger suspension/header copy, unintended-price rejection,
overlapping IDs, destination orphans, custom triggers, reference drift, posted
links, group membership, archived receipt bytes, log domains, audit events,
inventory preservation, repeat-run rejection and unchanged identity allocation.
This is not a live throughput or end-to-end RMS acceptance test.

The suite now also checks the saved reseed file: wrong target, execution before
renumber commit, changed links, preserved data, an unchanged repeat, next actual
allocation 18871 and rejection after new activity. Total: **36 isolated checks**.
