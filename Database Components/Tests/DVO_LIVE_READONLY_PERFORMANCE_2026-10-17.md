# Davao live performance: read-only findings, October 17, 2026

## Scope and evidence

The user authorized a read-only live Davao performance check and preparation of
a 10+ user Work Order/RMS test plan. No live migration, load test, application
save/tender, statistics update, cache clearing, isolation/configuration change,
new tracing session or session termination was performed.

Verified live target: **172.16.23.15 / DVO_STORE_DB**, server
WIN-L6UL7AVNSQ6, reported SQL version 13.0.1601.5, Enterprise Edition (64-bit),
compatibility 100, FULL recovery. Credentials were entered through a masked
prompt into process memory; no password or connection string was saved.

The principal capture ran approximately **10:43:47-10:44:17 UTC+8**, followed by
focused deadlock/metadata collection around **10:45 UTC+8**. Raw timestamps named
SampleUtc/EventUtc/CapturedUtc are UTC; ordinary SQL cached-execution/startup
timestamps are server-local values. Both complete collections reported zero
unavailable sections. All SQL commands used a 10-second command timeout and a
1-second session lock timeout; session deadlock priority was LOW.

The collector only reads DMVs, catalog metadata and the existing system_health
ring buffer. It does not run application queries as a benchmark on live and does
not scan the full business/log tables. Query-text string literals, numeric values
and comments are removed before writing local evidence. Server-wide session/wait
data includes other databases to help identify cross-database blockers; it must
not be mistaken for DVO_STORE_DB-only workload.

## 1. Confirmed deadlocks between RMS and commitment updates

The existing system_health buffer contained deadlocks at **10:40:07 and 10:40:12
UTC+8** (02:40:07/12 UTC). These are two captured events, not a complete count of
all deadlocks over the business day.

Both graphs show the same .NET session updating Item.QuantityCommitted while an
RMS **Store Operations** session reads Item and Tender for global-voucher/tender
lookup. The statement shapes, with values removed, are:

```sql
UPDATE dbo.Item WITH (ROWLOCK)
SET QuantityCommitted = QuantityCommitted + @quantityDifference
WHERE ID = @itemID AND ItemType <> @nonInventoryType
  AND QuantityCommitted + @quantityDifference >= @minimumCommitted;

SELECT *
FROM Item, Tender
WHERE Item.TenderID = Tender.ID
  AND Item.IsGlobalVoucher = @voucherFlag
  AND Item.Inactive = @inactiveFlag
  AND Tender.ID = @tenderID;
```

The writer's exact parameterized statement matches the current
clsItemLookUp.ApplyQuantityCommittedDifference path. A generic .NET application
name alone does not identify its executable version. The RMS reader is a tender/
voucher lookup; the graph does not prove that the sale had reached final settlement.

- First event: writer session 67 versus RMS session 109.
- Second event: writer session 67 versus RMS session 87.
- Resources include page locks on **DVO_STORE_DB.dbo.Item** and parallel exchange
  events. Shared reader locks and writer intent-exclusive locks form the cycle.
- Multiple execution-context IDs belong to the same RMS session. The five/six
  graph processes are not five/six separate users. This follows Microsoft's
  [deadlock execution-context documentation](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-deadlocks-guide).
- In both captured graphs the victim process belongs to the RMS reader.

This is direct evidence of a concurrency conflict. It does not establish that
every reported freeze has the same cause. In particular, eliminating the separate
item-log scan does **not** by itself eliminate this Item/Tender deadlock path.
ROWLOCK does not remove the need for intent locks higher in the lock hierarchy;
see Microsoft's [intent-lock explanation](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide#intent-locks).
Do not replace the atomic commitment update, remove its safeguards, add dirty
reads or change isolation simply to hide this conflict.

The Item index inventory includes a clustered DBTimeStamp index and nonclustered
indexes on ID, ItemLookupCode, Description, HQID, DepartmentID, CategoryID,
SupplierID and BinLocation. No index leading on the voucher/tender filtering
columns was present. **A supporting index for the RMS lookup is a test candidate,
not an approved definition or production deployment.** Check its actual plan and
selectivity on a current restore and measure its write-maintenance cost first.
Do not casually replace the RMS clustered index or rowversion mappings.

## 2. The live item-log index is absent

Catalog metadata estimates **1,499,284 SOD_Item_Logs rows**. The table is a heap
without the previously tested lookup index. No full-history live conversion/tie
check or CREATE INDEX was performed during this audit.

One cached last-commitment lookup statement inside trg_OrderEntry_ItemLogs had:

| Cached metric in the focused capture | Value |
| --- | ---: |
| Executions | 306 |
| Average elapsed | 29.694 ms |
| Maximum elapsed | 942.5 ms |
| Average logical reads | approximately 17,368 per execution |

These are cached statement aggregates, not whole-save times. They identify another
avoidable cost while the Work Order save transaction is running. The earlier local
index test reduced reads from 10,639 to 3 on a smaller June restore; it is not a
measurement of this current live data or proof of multi-user capacity.

## 3. Item-list reads are expensive

The focused cache capture included these separate statement shapes:

| Statement shape | Executions | Average elapsed | Average logical reads |
| --- | ---: | ---: | ---: |
| Broad LINQ Item list with ntext-to-nvarchar conversions and descriptions | 50 | 2,813 ms | 464,069 |
| SELECT * FROM SOD_ViewItems | 14 | 3,086 ms | 463,070 |
| One RMS Item/Tender voucher lookup cache entry | 361 | 28.638 ms | 9,829 |

The voucher lookup also has other cache entries; the table does not combine them
or imply 361 total executions across all variants. SOD_ViewItems reads active
items and descriptions; no nested function dependency was found in that view's
captured direct dependency inventory. Other RMS item-list/ItemClass/SupplierList
queries also appeared among the higher elapsed-time statements.

These observations support reviewing broad item-list/refresh reads as a separate
performance task. They are not evidence that a particular user's keyword search
took exactly that time. Preserve search results, descriptions, stock display and
branch rules when testing changes. No query was rewritten by this audit.

## 4. What the activity samples do and do not show

- Six snapshots five seconds apart showed **no blocked user requests**. No open
  user transaction appeared in the first session snapshot. This short, mostly
  quiet interval cannot rule out a blocker during the reported busy periods.
- DVO_STORE_DB had 63 connections across 16 distinct hosts in the first sample:
  44 .NET provider connections and 19 Store Operations connections, mostly
  sleeping. Connection pooling means these are not 63 concurrent users.
- Some background SQL Agent activity was visible in other databases. No causal
  link to the reported Davao stalls was established.
- During the first capture, server-wide wait deltas included CXPACKET 73,035 ms,
  LATCH_EX 14,926 ms and ASYNC_NETWORK_IO 2,306 ms. Waits accumulate across workers
  and databases, so totals can exceed wall time and are not per-user latency.
  No positive lock-wait delta was observed in this interval.
- Visible scheduler snapshots had a maximum runnable queue of zero. SQL memory
  low flags were false; the sampled process used about 4.8 GiB on a server
  reporting about 63.8 GiB RAM and 28 logical CPUs. This is not peak-load proof.
- DVO data-file activity was only four physical reads (5.75 ms mean); log activity
  was 52 writes (0.25 ms mean). This sample is too small to clear storage as a
  possible peak-time factor.
- MAXDOP was 0, cost threshold 5 and max server memory 2,147,483,647 MB. Query Store
  was OFF; READ_COMMITTED_SNAPSHOT and snapshot isolation were OFF. These settings
  were recorded, not changed or declared the cause. Assess patching, memory and
  parallelism separately with the DBA; do not tune production from this snapshot.

Cache counters are per cached-plan lifetime and can reset on recompilation or
eviction. A missing system_health event is not proof that no deadlock occurred.
Actual plans, client-side timing and another capture during a slowdown remain
needed for a complete peak-load diagnosis.

## Priorities and deliverables

1. Reproduce the **RMS voucher lookup versus Item commitment update** conflict on
   a fresh isolated Davao restore with actual RMS clients.
2. Rehearse the existing item-log index against that current restore and measure
   both lookup cost and end-to-end save/tender behavior.
3. Evaluate a narrowly scoped supporting index for the RMS lookup on the restore,
   with before/after plans, deadlocks, logical reads and write costs. No candidate
   DDL was prepared/applied as part of this read-only request.
4. Review broad item-list reads separately, then run the mixed 10/12/15-client plan.
5. Use a controlled deployment/rollback plan only after review of the actual test
   evidence. Do not select unrelated DVO updater objects as incidental tuning.

See [the mixed-user test plan](DVO_MULTIUSER_RMS_TEST_PLAN.md).

Evidence is in ignored TestResults/ReadOnly-Performance-b968151b and
TestResults/ReadOnly-Performance-29719f20. The reusable collector is
Collect_ReadOnly_Performance.ps1; it stores no credentials. The first collector
attempt failed locally on a connection-builder keyword before connecting; both
subsequent collections completed. Both Release/x86 builds and 101 unit tests
passed, with Debug store configuration preserved. Existing uncommitted work was
not reset. No live business-rule or database changes were made.
