# Restored HO item-log lookup index test — October 16, 2026

## Target and scope

The user authorized testing only on localhost's restored HO_PO_DB. All SQL
connections in this test used localhost with Windows authentication. No live
server connection, migration or application save was performed during this test.

- Verified target: MSI / HO_PO_DB, SQL Server 2016 (13.0.1742.0).
- Restore recorded at 14:48:17 on October 16, from the HO_PO_DB backup completed
  at 14:32:05 on WIN-2847UGJBAV4.
- Compatibility level 100 and SIMPLE recovery were retained.
- Item-log metadata estimated 71,744,077 rows before and after installation.

## Compatibility adjustment

The first migration attempt stopped at compilation because TRY_CONVERT is
unavailable at this database's compatibility level. No index was installed by
that attempt. The three reference-validation conversions were changed to
TRY_CAST(ReferenceID AS int). The same invalid/overflow-reference and conflicting
latest-timestamp guards remain in place. No compatibility setting or trigger
comparison was changed. All 31 existing checks then passed at both fixture
compatibility levels 100 and 130.

## Committed local result

The updated migration passed its full-history conversion and timestamp-tie
checks and created IX_SOD_WO_ItemLogLookup within an explicit transaction. Before
committing, the harness compared two existing-order lookups and all table trigger
definition hashes. Both lookup values and all hashes matched. A separate
connection subsequently verified the committed index and compatibility level.

The enabled non-unique index keys are ItemID ASC, Date DESC, with ReferenceID
and NewCommitted included. No trigger definitions, order records, inventory
quantities, tax rules, queue rules or application configuration were changed by
the migration. No save, import, cancellation or RMS settlement was executed on
the restored store by this test.

| Item / existing order | Before | After | Log-table logical reads | Returned NewCommitted |
| --- | ---: | ---: | ---: | ---: |
| 06328UNT (ItemID 46612), order 18543 | 6,706.538 ms | 3.472 ms | 806,300 → 5 | 3 → 3 |
| 06332UNT (ItemID 54299), order 18875 | 7,294.833 ms | 5.192 ms | 806,300 → 5 | 3 → 3 |

These are single-run component measurements using the existing integer order-ID
comparison, not whole-save times or a guarantee for all orders. SQL caches were
not cleared. Safety checks plus index creation took 654.371 seconds (about
10 minutes 54 seconds), exceeding the updater's 120-second command timeout.
Any proposed live installation needs separate authorization, rehearsal and a
maintenance-window deployment following the migration guide. No live deployment
was performed.

## Other validation and evidence

- 31 item-log fixture checks passed at each of compatibility levels 100 and 130.
- Both solutions built in Release/x86; all 101 unit tests passed.
- All 71 disposable updater checks passed, including selected-script rollback.
- The Debug application's EXE configuration was preserved byte-for-byte.
- Restored-store evidence: ignored TestResults/HO-Restore-Index-08e89cfb contains
  lookup timings/IO, index columns and before/after trigger fingerprints.
- The first rejected attempt is under TestResults/HO-Restore-Index-4384eb31.
- Disposable fixture results are under TestResults/ItemLogLookup-4ada59e1
  (100), TestResults/ItemLogLookup-59947629 (130), and
  TestResults/DatabaseUpdater-5f056471.

The local index is left installed for manual Work Order testing. Next verify a
new order and an order populated from the supplied Transfer In file through the
app on localhost, then recall/edit them and check totals, commitments and logs.
The test did not change registry or EXE settings to select that database.
