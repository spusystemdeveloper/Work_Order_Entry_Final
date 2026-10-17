# GSC item-log ambiguity: read-only localhost review

## Decision

Keep **Improve_WorkOrder_ItemLogLookup.sql unchecked for this GSC restore**.
The 53305 guard is justified. The latest timestamps do not uniquely identify
which historical NewCommitted value to return. A matching result in a few probes
would not establish a stable ordering or make a guard bypass safe.

The separately packaged **Improve_RMS_VoucherLookup.sql** indexes Item, not
SOD_Item_Logs, and does not depend on resolving these histories. Rehearse that
migration separately using its deployment guide; it was not executed in this review.

## Target and method

Reviewed **localhost / GSC_STORE_DB**, server MSI, SQL Server 2016, compatibility
100, on October 17, 2026. The original restore lineage reports a September 9,
2026 backup of GSC_FOR_TESTING_DB restored locally that day. These findings do not
describe the current live GSC database.

The completed census ran directly against the existing local restore after the
user requested no further full database copies. It issued source SELECTs only;
one intermediate ambiguity table lived in tempdb and was dropped before completion.
Diagnostic ranking/type aggregation used MAXDOP 1 and MAX_GRANT_PERCENT=1 to limit
their query memory grants. No server configuration, business table, trigger,
procedure, identity, index or deployment record was changed. No live connection
was made. The earlier abandoned copy had already been removed, including its
backup/data/log files, before this run.

The reusable diagnostic is `Review_ItemLog_Ambiguity.sql`. This performs full
history scans/sorts and is for an explicitly authorized review, not routine POS
use. It does not repair data or select a correct tied value. No snapshot isolation
or source-table lock was introduced: counts describe the observed local state,
not an immutable backup if concurrent application activity occurs.

## Completed census

| Finding | Count |
|---|---:|
| Log rows | 9,512,243 |
| Invalid/overflow integer ReferenceIDs | 0 |
| Item/numeric-reference pairs with conflicting latest values | 84 |
| Distinct affected Items | 73 |
| Rows sharing those conflicting latest timestamps | 246 |
| Pairs with midnight latest timestamps | 84 |
| Pairs whose reference matches a closed Order number | 46 |
| Pairs whose reference matches no Order number | 38 |
| Pairs whose reference matches an open Order number | 0 |

The conflicting latest dates range from **2023-06-27 through 2026-06-03**.
All 246 tied rows have transfer-related types: **165 POS Transfer Out Update Logs**
and **81 POS Delete Transfer In Logs**. They are not Order Update Logs.

ReferenceID is an untyped varchar field shared by several log producers. A
transfer reference can have the same numeric value as a closed order ID; that
does not turn the transfer into a work-order event. Conversely, the actual
order-update trigger does not filter Type, so those rows are still candidates
whenever its ItemID/numeric ReferenceID predicate matches. Zero currently open
matches does not prove all future or external callers can never reach them.

## Why the existing data does not establish the latest event

SOD_Item_Logs is a heap with **no identity, rowversion or sequence column**.
The relevant trigger, `trg_OrderEntry_ItemLogs`, uses:

```sql
SELECT TOP(1) NewCommitted
FROM SOD_Item_Logs
WHERE ItemID = @ItemID AND ReferenceID = @OrderID
ORDER BY [Date] DESC
```

It copies the selected value into LastCommitted on a subsequent Order Update
Logs row. Changing the selected tied row can therefore change newly written
history even though the index itself contains no business-row UPDATE.

`SOD_sp_InsertItemLogs` accepts **@Date date**, while the table stores datetime.
That procedure cannot retain a supplied time of day, providing a possible route
to midnight timestamps. The current InventoryTransferLog trigger instead calls
GETDATE(). This review did not establish which external POS producer emitted each
old row, so the procedure is evidence of time-loss capability, not proof of every
row's origin. No in-database dependency on this insert procedure was listed.

Representative latest groups:

| ItemID | Numeric reference | Latest date | Tied rows | Distinct NewCommitted |
|---:|---:|---|---:|---|
| 7271 | 253101 | 2025-01-09 00:00:00 | 3 | 0, 34 |
| 17624 | 178017 | 2024-02-01 00:00:00 | 2 | 3, 8 |
| 42438 | 134534 | 2023-06-27 00:00:00 | 6 | 20, 270 |

In these samples LastCommitted equals NewCommitted within each row. They do not
form a uniquely recoverable before/after commitment chain. Row location, display
order, greatest commitment, present-day stock or repeated equal results do not
recover the missing event time. The original lookup for 7271/253101 returned 34
in three read-only probes; **34 was not certified as the true final event**.

Other reviewed consumers include the quantity-history view (Item Auto Logs),
recent-order/item views and transfer/item/transaction log triggers. The existing
SOD_sp_FixCommitted procedure would write commitments and correction logs; it was
read for context and **not executed**. It is not a timestamp-ambiguity repair.

## How to proceed without silently changing the rules

1. Leave the current item-log migration and its data guard unchanged. Do not
   create the same index manually to make a later run report REUSED, remove the
   guard, delete the 246 records, rewrite their dates or add fake correction rows.
2. If the original POS/transfer audit trail or another authoritative source
   retains actual event times, reconcile the 84 groups against that evidence.
   Only a separately requested, guarded recovery should change historical rows;
   retain provenance and verify every affected reference. This review is not
   authorization to perform that recovery.
3. If that evidence is unavailable, the stored data alone cannot identify the
   intended final event. A deterministic selection rule would need explicit
   business approval and separate implementation/validation. Adding a new sequence
   now would help only future ordering and would not reconstruct old chronology.
   Filtering out transfer rows, using MAX(NewCommitted), or ignoring closed-order
   collisions would change the selection contract and was not implemented.
4. After a justified resolution, rerun the unchanged preflight on the intended
   current branch restore, compare original/indexed values and logging, then
   rehearse the backed-up maintenance deployment. Until then, the full item-log
   index is **not certified safe for this GSC dataset under unchanged rules**.

No index was created on the existing restore, even temporarily. Therefore this
review does not claim a before/after index performance or result comparison.
The earlier disposable-copy restore was stopped/cleaned up before that experiment
completed. The current task completed the read-only review requested afterward.

## Evidence and validation

The census completed in **84.966 seconds**. Private ignored CSVs and Status.json
are under `TestResults/GSC-ReadOnly-d9da732e/`; the file captures five result sets.
Only bounded summaries/samples were brought into memory. No credentials were
written to scripts, logs or this report.

Application/database deployment logic was not edited for this audit. Existing
validation was rerun: both solutions built in Release/x86 and all 101 unit tests
passed, with Debug EXE configuration preserved around the build/test run.
Final metadata checks found zero Item-log indexes and zero active review requests.
A successful build is not evidence of a history repair or branch index deployment.
