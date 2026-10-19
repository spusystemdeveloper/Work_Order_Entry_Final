# Requested HO recovery: WO 18912 to 18905

Prepared October 19, 2026 for personal SSMS execution only. The assistant has
not executed this recovery on live, including in rollback mode. This is separate
from the proposed addin prevalidation improvement; no save code changes are
included. RMS settlement investigation remains paused.

Use [Renumber_HO_WO_18912_to_18905.sql](Renumber_HO_WO_18912_to_18905.sql).
Do not use an earlier renumber script or select this file in Database Updater.

## Reviewed live state

SELECT-only inspection of WIN-2847UGJBAV4 / 172.16.4.50 / HO_PO_DB found:

- WO 18912 is open, Type 2, StoreID 606, CustomerID 71, saved/last updated
  at 2026-10-19 11:42:06.390. Total 54412.5000, tax 0. WO 18905 is absent.
- Entry 61753: ItemID 67317, quantity 1000, price 20.2000, full price 33.5000.
- Entry 61754: ItemID 67321, quantity 750, price 18.7000, full price 31.0000.
- Entry 61755: ItemID 67325, quantity 950, price 21.2500, full price 35.5000.
- Queue 45921 points to and groups to 18912, Status 0. Three price approvals
  and eleven WO audit events reference this order. Four order-domain item-log
  rows for its items reference 18912 (three entry events and one update event).
- No RMS Transaction.RecallID, OrderHistory.OrderID or Type 2 Journal.ReferenceID
  record was found for 18905 or 18912. Reviewed trigger hashes match the script.
- Maximum order ID and current identity were 18912. Missing IDs 18905-18911
  were confirmed across all order types. This does not prove their cause.

This is a snapshot, not permission to ignore subsequent activity. The script
rejects later allocations, changed source values/counts, occupied destinations,
posted transactions, unknown reference columns and unreviewed dependencies.
Less common reference tables are checked at execution and cause rejection if
they contain any affected IDs; their contents are not guessed or silently moved.

## Scope

The script maps 18912 to 18905 in the header, order lines, queue OrderID/GroupTo,
price approvals, WO audit OrderID and order-domain SOD_Item_Logs.ReferenceID.
Related groups retain their own order and queue IDs. It rejects posted RMS,
order-history and receipt records rather than rewriting completed sales.
Unrelated sales/purchase item-log numbers are not work order references.

Existing business values are compared before/after with only the requested ID
mapping substituted. Stock is not updated. The script temporarily suspends the
exact reviewed table triggers under transaction-owned table locks, restores
them before commit and appends one explicit order-ID mapping audit event.
Any failure rolls back the maintenance. Rowversion values necessarily change;
original free-text history, comments and receipt payloads are retained.
External documents, printouts and cached client state cannot be updated by SQL.

**There is no identity reseed.** After this mapping the identity remains 18912,
and the next normal allocation is 18913 if no intervening allocation occurs.
This request does not authorize filling 18906-18911 or resetting the next ID.

## Personal SSMS execution

1. Rehearse on a fresh isolated restore first. Only for that restore, change
   ExpectedServer in a separate copy to the restored instance name. Preserve
   the source, dependency, rollback and commit guards. The supplied
   production file targets only WIN-2847UGJBAV4 / HO_PO_DB.
2. Close all HO RMS POS/Manager, Work Order Entry, queue and integration clients.
   Keep them closed through verification; recalled forms become stale.
3. Backup-path, backup-history and checksum verification were removed at the
   user's explicit request. No BackupPath setting is required. The script does
   not establish that a recovery copy exists; backup protection is the operator's
   responsibility.
4. Open a fresh SSMS query on the target and run the whole file with Commit=0.
   This is a real transactional rehearsal, not just SELECTs: it acquires table
   locks, performs the mapping and rolls it back. The legacy item-log checks
   scan large history, so allow adequate maintenance time and query timeout.
5. Review the target, mapping, affected row counts and final
   `REHEARSAL ONLY` result. Original WO 18912 and identity 18912 must remain.
   On any error or unexpected count, stop; do not remove a guard to force it.
6. With clients still closed and the reviewed state unchanged, set Commit=1
   and rerun the whole file. Require the final `COMMITTED` result. A later
   save, edit or settlement causes a guard failure and requires another audit.
7. Verify with SELECTs before reopening clients:

```sql
SELECT ID, Closed, [Type], Total, Tax FROM dbo.[Order]
WHERE ID IN (18905, 18912);
SELECT ID, OrderID, ItemID, QuantityOnOrder, Price, FullPrice
FROM dbo.OrderEntry WHERE OrderID IN (18905, 18912);
SELECT id, OrderID, GroupTo, Status FROM dbo.Queueing
WHERE OrderID IN (18905, 18912) OR GroupTo IN (18905, 18912);
SELECT OrderId, COUNT(*) AS ApprovalCount FROM dbo.SOD_WO_LOGS
WHERE OrderId IN (18905, 18912) GROUP BY OrderId;
SELECT OrderID, COUNT(*) AS EventCount FROM dbo.SOD_WO_History
WHERE OrderID IN (18905, 18912) GROUP BY OrderID;
SELECT ItemID, [Type], ReferenceID, QuantityORIS, [Date]
FROM dbo.SOD_Item_Logs
WHERE ItemID IN (67317, 67321, 67325)
  AND [Type] IN ('Order Entry Logs', 'Order Update Logs')
  AND ReferenceID IN ('18905', '18912');
SELECT IDENT_CURRENT('dbo.[Order]') AS CurrentOrderIdentity;
```

Expected: only WO 18905; the same three lines and exact amounts; queue 45921
mapped to/grouped to 18905 with Status 0; three approvals; twelve audit events
(the original eleven plus one mapping event); four reviewed order-domain item
logs under 18905; identity still 18912. No test order should be inserted on live
just to check the next number. Recall WO 18905 in a newly opened client and
check its displayed details before normal work resumes.

## Validation

`Database Components/Tests/HO_Renumber18912_LocalDB_Test.ps1` creates a uniquely
owned synthetic LocalDB instance/database. Only its
in-memory copy substitutes the fixture server. The distributed
wrong-server guard, transaction, trigger and value
guards remain intact. The test covers rollback, injected failures, unchanged
stock/amounts, related grouping, unsupported/orphaned references, occupied IDs,
settlement, schema/trigger drift, commit and actual next legacy @@IDENTITY.
Fixtures are dropped and their LocalDB instance deleted in finally.

These are synthetic tests, not a restored-store or live recovery certification.
All 43 checks passed at compatibility 100. Both Release/x86 solutions, the Debug
application build and all 101 unit tests passed; the Debug EXE configuration was
preserved byte-for-byte. Logs are in ignored `TestResults/HO18912-Fixture.log`
and `TestResults/HO18912Validation/`.
No backup or restore rehearsal of the live database was performed by the
assistant. Operator execution and post-commit verification remain outstanding.
