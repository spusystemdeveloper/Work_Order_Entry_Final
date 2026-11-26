# Davao localhost deadlock and lookup-index comparison — October 17, 2026

## Result and scope

Reproduced the RMS voucher lookup / Work Order commitment-update deadlock on an
owned disposable copy of **localhost MSI / DVO_STORE_DB**. With only the candidate
Item lookup index added, the repeated mixed workload reported no deadlocks.
The actual compiled recalled-order save transaction also completed faster under
the replayed lookup contention. These are local measurements, not live-store or
actual RMS POS/QSRules certification, and they do not guarantee zero future
deadlocks on every path.

No live connection was made during this test. The original local DVO_STORE_DB
received no application/test DML or candidate index. Its previously installed
`IX_SOD_WO_ItemLogLookup` remains installed. That item-log index was present in
both comparison stages; it is **different from the Item voucher index** tested here.
No updater selection, application source, trigger, business rule, server setting,
isolation option, command timeout or query hint was changed for this comparison.

## Isolated setup and unchanged statements

- Created a COPY_ONLY, checksum backup of localhost DVO_STORE_DB and restored it
  to GUID-owned `DVO_PERF_20da7108`, using separate SQL data/log files.
- Source backup lineage is June 9, 2026, restored locally June 10. The source at
  this run contained 70,816 Items, 22,279 Orders, 87,802 OrderEntries and 927,157
  SOD_Item_Logs rows. This is older data than the October live system.
- Local SQL Server 2016 retains compatibility 100 and non-versioned reads; the
  computer has 12 logical CPUs. Item is clustered on DBTimeStamp (rowversion),
  with its existing nonclustered ID index. Those definitions were retained.
- Selected Tender ID 1 from this restore, rather than copying an HO branch ID.
  The restore initially has no active global-voucher Items: empty lookup results
  are therefore expected. Separate nonempty equivalence checks are below.

Unchanged captured RMS query shape, executed and fully drained by SQL clients:

```sql
SELECT * from Item,Tender
Where Item.TenderID = Tender.ID
  and Item.IsGlobalVoucher= 1 and Item.Inactive=0 and Tender.ID='1'
```

Unchanged Work Order commitment statement, as used by
`clsItemLookUp.ApplyQuantityCommittedDifference`:

```sql
UPDATE dbo.Item WITH (ROWLOCK)
SET QuantityCommitted = QuantityCommitted + @p0
WHERE ID = @p1 AND ItemType <> 7 AND QuantityCommitted + @p0 >= 0
```

The sole candidate was:

```sql
CREATE NONCLUSTERED INDEX IX_DVO_Perf_Voucher
    ON dbo.Item(TenderID,IsGlobalVoucher,Inactive);
```

The saved, guarded SQL is
`Fixtures/RMSVoucherLookup_Index_Candidate.sql`. It is intentionally **test-only**:
it rejects ordinary store names, requires SQL on the client computer and a
caller-owned transaction, checks column types, and refuses to replace an existing
index. It was executed successfully on the owned clone. Wrong-database,
missing-transaction and existing-index rejection tests passed (53701/53702/53704).
It is not registered in Database Updater and must not be treated as a live
deployment script.

## Query plan and uncontended lookup

Seven timed probes per stage; the first is excluded from the warm median. No
server-wide cache clearing or forced plan/MAXDOP hint was used.

| Measurement | Baseline | Candidate index |
|---|---:|---:|
| Warm client median, six probes | 27.310 ms | 2.024 ms |
| Item logical reads per probe | 7,568 | 2 |
| Tender logical reads per probe | 2 | 2 |
| Item access | Parallel clustered scan | Nonclustered index seek |
| Returned rows on unchanged restore | 0 | 0 |

The saved baseline plan contains a parallel Item scan; the indexed lookup plan is
serial and seeks the candidate. Index creation naturally compiles a new plan;
this comparison is not a separate statistics-only experiment. No claim is made
that the index addresses every other slow query found in the live audit.

## Actual compiled save transaction with concurrent lookup replay

Restored the **same backup before each stage**. Six separate x86 STA worker
processes constructed recalled orders without showing/loading the main form,
redirected all three legacy contexts and GetDB to the owned clone in process
memory, and captured fresh `OrderEditGuard` snapshots. No real app configuration
or registry target was rewritten.

Orders 23287, 23283, 23282, 23264, 23263 and 23258 each received a test increase of
0.25 per existing line. Six ready workers were released together while four SQL
clients replayed the unchanged RMS lookup for bounded 18-second loops with a
10 ms client pause between lookups. Workers invoked the actual private
`frmItemLookUp.SaveExistingOrderChanges` transaction: application lock, stale-state
check, line/header/commitment/queue/tax work and commit. This exercises the compiled
transaction, **not** the full Save-button validation/password/printing flow.
No retry was added after a deadlock.

| Measurement | Baseline | Candidate index |
|---|---:|---:|
| Saves committed | 6 / 6 | 6 / 6 |
| Median save transaction | 9.320 s | 1.351 s |
| Slowest save transaction | 10.015 s | 1.693 s |
| RMS SQL lookup attempts | 394 | 4,199 |
| Successful lookups | 390 | 4,199 |
| Lookup SQL error 1205 (deadlock victims) | 4 | 0 |
| Other SQL lookup errors | 0 | 0 |
| Successful lookup median | 77.081 ms | 2.369 ms |
| Successful lookup p95 | 150.588 ms | 6.355 ms |

Different completed lookup counts reflect the fixed-duration loops, not a fixed
number of customer transactions. This is one six-save pair with short lookup
replay, not a production throughput/p95 save-capacity study.

Two existing system_health deadlock graphs were retained for the actual-save
baseline. They identify `DvoActualSave_23282` and replay reader sessions competing
on Item pages, with reader shared and writer intent-exclusive locks. Multiple
parallel execution contexts are not additional users. Four victim errors are
not claimed to be four independently retained graphs. The indexed stage reported
zero victim errors and retained zero graphs in its time window.

An earlier deliberately tight SQL stress reproduction used six atomic-update
transaction loops and four unpaced lookup loops, with eight disjoint Items per
writer and 2 ms client pauses between updates. Writer transactions were rolled
back in the owned fixture. It produced 265 reader and one writer deadlock victim
errors before indexing, versus zero afterward. This stress is supplementary;
it does not represent normal cashier throughput. The actual-save table above is
the more relevant transaction measurement.

## Correctness and cleanup

- Both actual-save stages passed expected fractional commitment-delta and
  unchanged physical-stock checks for all **31 affected Items**. Only committed
  saves contribute to the expected delta, including Items shared between orders.
- All **32 indexed-save lines** retained their prices, full prices, costs,
  descriptions and Taxable values, with the requested quantities stored exactly.
- Separately seeded three matching vouchers and one inactive voucher **only in
  the disposable clone**. Compared the original query before/after index creation
  for two matching tenders, a configured empty tender and a missing tender.
  Results were respectively 2, 1, 0 and 0 rows. Sorted, typed full-row SHA-256
  fingerprints matched across all **114 returned columns**, including binary
  values and nulls. No ORDER BY was added or result order promised.
- All **84 table-trigger definitions and enabled states** remained unchanged;
  a source/clone metadata comparison also found zero differences.
- Restored the owned clone once more from the original pre-test backup and
  compared counts and aggregate BINARY_CHECKSUM fingerprints against the source
  for Item, Order, OrderEntry, Queueing, QueueingItems and SOD_Item_Logs: all six
  matched. These are aggregate checks, not collision-proof full-content hashes.
- Verified the clone's exact file paths against its GUID manifest, dropped only
  that owned database, and removed its test backup. The original local restore
  remains available, without the voucher candidate index. No test orders or stock
  changes were left in it.

## Other validation and remaining work

- Work Order Entry.sln and OrderProcessingQueueing.sln: Release/x86 builds passed.
- Run-UnitTests.ps1: **101 / 101** passed.
- MultiUser_Save_LocalDB_Test.ps1: **39 / 39** passed in normal mode and **39 / 39**
  with -VersionedReads. These use separate synthetic LocalDB fixtures; no store
  versioning setting was changed and settlement is simulated.
- Debug EXE configuration retained SHA-256
  `C63C2D61823B0BED6F2DA25943B9CCF3A17B2C0A37C947FC8E378F75F9BF6B2D`.
- Raw local plans, graphs, harnesses, request/results, stock/line checks and
  cleanup evidence are private ignored artifacts under
  `TestResults/DVO-RMS-20da7108/`, with helper scripts in TestResults. They contain
  restored business data and are not added to Git. No credentials were embedded.

The next validation is a fresh isolated Davao restore with actual RMS clients,
including real voucher/tender configuration and settlement, following
`DVO_MULTIUSER_RMS_TEST_PLAN.md`. The 10/12/15 actual-client ramp and soak have not
run. The old local restore has no history feature installed; this test does not
certify different live trigger versions. Live deployment needs a separately
reviewed target/maintenance plan. No live execution is authorized by this report.
