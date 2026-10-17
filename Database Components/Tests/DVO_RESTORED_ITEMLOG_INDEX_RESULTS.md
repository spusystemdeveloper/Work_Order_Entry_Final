# Local Davao item-log lookup index test — October 17, 2026

## Target and authorization

The user authorized testing the index on localhost DVO_STORE_DB and comparing
before/after performance, with no live database or business-rule changes.

- Verified connection: localhost with Windows authentication, machine/server MSI,
  database DVO_STORE_DB, SQL Server 2016 (13.0.1742.0).
- Restore history: restored June 10, 2026 at 10:26:21 from DVO_STORE_DB on
  WIN-L6UL7AVNSQ6, backup completed June 9 at 18:24:57. This is an older local
  restore, not verification of the current live Davao database.
- Compatibility level 100 was retained. DVO_POS_DB was not modified.
- The item-log table had 927,139 rows and no lookup index before the test.

## Local installation and verification

Executed the unchanged `Migrations/Improve_WorkOrder_ItemLogLookup.sql` inside
an explicit transaction. The harness refuses a different machine/server/database,
requires the recorded Davao restore provenance and an absent lookup index, and
rolls back on a failed pre-commit comparison. The migration's schema, integer-
reference and ambiguous-latest-timestamp guards remained intact and passed.

The exact tested migration SHA256 was
`B6BE279C42A9215F58DDFB967D7A8481CE486DCA19454137A03F29C734FBFDE6`.

The migration preflight plus index creation took **1.606 seconds** on this restore.
This excludes the separate performance/data checks and is not a full updater-UI
deployment timing. No updater code or command timeout was changed.

Before commit, all seven lookup results matched across all three before/after
trials, all 84 table-trigger definition hashes and enabled/disabled states matched,
and the log row count and key/commitment-field aggregate checksum matched. The
checksum is a consistency check, not a cryptographic hash of every business table.
No database/server DDL triggers were present in the subsequent metadata check.

After commit, a separate SQL connection verified the installed, enabled,
non-unique, unfiltered IX_SOD_WO_ItemLogLookup index:

- Keys: ItemID ASC, [Date] DESC.
- Included: ReferenceID, NewCommitted.
- Log row count remained 927,139; compatibility remained 100.

The index is **left installed on localhost DVO_STORE_DB** for manual application
testing. Trigger SQL, comparison semantics and application code were not changed.
No application save, import, cancellation, RMS settlement or business-data DML
was executed on the restored store. No live database was connected to or changed.
The installation used the migration directly, not the full updater UI/history flow.

## Before/after lookup performance

Measured the existing trigger-shaped lookup with int ItemID/order-reference
variables and the unchanged numeric ReferenceID comparison and date ordering:

```sql
SELECT TOP (1) NewCommitted
FROM dbo.SOD_Item_Logs
WHERE ItemID = @ItemID AND ReferenceID = @OrderID
ORDER BY [Date] DESC;
```

Samples comprise two recent log-reference groups, two oldest groups, two most
frequent groups and one verified missing-item lookup. Reference 0 is existing
history data, not a newly created order. Three trials per pair per stage were
recorded (42 lookups total); the table shows median client elapsed milliseconds.
Logical reads were identical across the three trials for each listed stage.

| ItemID | Numeric reference | Before median ms | After median ms | Logical reads before -> after | Returned NewCommitted, unchanged |
| --- | --- | ---: | ---: | ---: | ---: |
| 456 | 23269 | 21.538 | 0.717 | 10,639 -> 3 | 0 |
| 59679 | 23286 | 22.830 | 0.866 | 10,639 -> 3 | 2698 |
| 59679 | 0 | 23.158 | 0.834 | 10,639 -> 3 | 2698 |
| 59683 | 0 | 23.573 | 0.667 | 10,639 -> 3 | 260 |
| 65061 | 2 | 22.645 | 0.616 | 10,639 -> 3 | 0 |
| 65064 | 1 | 21.776 | 0.619 | 10,639 -> 3 | 0 |
| Verified missing item | Verified missing reference | 21.197 | 0.625 | 10,639 -> 3 | No row |

These are warm-cache component measurements, not whole-save timings or a promise
for all items/orders. Caches were not cleared; no hints, trigger edits or new
tie-break rules were used. A restored database with different data or competing
workload can have different results.

## Validation and next acceptance

- Both solutions built in Release/x86; all 101 unit tests passed.
- All 31 existing item-log fixture checks passed at each compatibility level
  100 and 130 in owned disposable LocalDB instances.
- Debug EXE configuration was preserved byte-for-byte; no registry/store settings
  were edited and the real application was not pointed at a fixture.
- Ignored evidence: `TestResults/DVO-Restore-Index-08a696a1` contains target/restore
  metadata, sample groups, all raw timings/IO, median summary, trigger fingerprints,
  log checks and index metadata. The local-only harness is
  `TestResults/DVO-Restore-Index-Test.ps1`.

Next manually test new/imported Work Orders, save, recall, edit and print against
this localhost DVO_STORE_DB using the installation's normal configuration process.
Verify commitments and logging. Before deploying to live Davao, rehearse against
a current Davao backup, verify the exact live database/profile and use a backed-up
maintenance window. This older local result does not approve other branch scripts
or automatically verify the live database or its current data.
