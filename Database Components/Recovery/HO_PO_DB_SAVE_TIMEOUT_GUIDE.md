# HO_PO_DB new-order Save timeout — October 15, 2026

## Findings from read-only live diagnostics

The user reported error 0015, "Execution Timeout Expired", when saving new
Work Orders, including orders populated by Import. Diagnostic reads targeted
`172.16.4.50`, server name `WIN-2847UGJBAV4`, database `HO_PO_DB`.

| Cached execution evidence | Latest elapsed time | Latest logical reads |
| --- | ---: | ---: |
| `SOD_sp_InsertQoute` (header) | 90.587 ms | 71 |
| `SOD_sp_InsertQouteEntry` (line) | 30,003.715 ms | 341,193 |
| `trg_OrderEntry_ItemLogs` (existing line-log trigger) | 30,003.463 ms | 341,183 |
| `SOD_trg_WO_History_Order` (new history trigger) | 0.140 ms | 6 |

The maximum cached duration of the new order-history trigger was 1.434 ms.
History numbering was sequence-backed (`HistoryUsesIdentity=0`). The metadata
estimate for `SOD_Item_Logs` was **71,668,895 rows**, stored as a heap with no
indexes. No active HO request, blocker or open HO transaction was observed in
the initial diagnostic snapshot; this does not rule out earlier blocking.
Cached statistics describe the recent executions observed, not a live trace of
every Save stage. The repeated approximately 30-second line/trigger durations
match the reported application timeout.

A later read-only comparison found the following metadata estimates. These are
snapshots and naturally change as new log rows are written:

| Database / connection | Log rows (estimated) | Enabled log-table indexes |
| --- | ---: | ---: |
| `HO_PO_DB`, live `172.16.4.50` | 71,669,159 | 0 |
| `GSC_STORE_DB`, live `172.16.4.50` | 9,639,836 | 0 |
| `DVO_STORE_DB`, localhost | 927,088 | 0 |
| `QC_STORE_DB`, localhost | 141,746 | 0 |

The HO and local DVO trigger definitions have identical fingerprints. Local and
live GSC and local QC share another fingerprint; the local GSC definition matches
HO when whitespace/case are ignored. The observed logic is therefore shared,
despite different raw fingerprints caused by formatting.

HO has roughly 7.4 times the log rows of live GSC on the same server and 77 times
the rows of the local DVO copy. The same unindexed lookup can reach a timeout on
HO while completing on smaller databases. Cache, query plans, I/O and server load
also affect duration; no expensive comparison scans or Save operations were run
on these live databases to claim a precise time ratio. Local counts describe the
local copies, not current remote DVO/QC production data.

The legacy log trigger unconditionally runs:

```sql
set @lastcommitted = (
    select top 1 NewCommitted
    from SOD_Item_Logs
    where ItemID = @Itemid and ReferenceID = @Orderid
    order by date desc
)
```

For a new OrderEntry insert, `@Itemid` and `@Orderid` come from empty `deleted`
rows. The previous-log result is not used by the insert-log branch, but the
lookup still incurs substantial work against the large unindexed table.
The new history identity repair is a separate issue; its numbering is already
safe in the observed live metadata.

## Focused fix

Use [Fix_HO_PO_DB_NewOrder_Save_Timeout.sql](Fix_HO_PO_DB_NewOrder_Save_Timeout.sql).
It wraps only the existing previous-log lookup in:

```sql
IF @Qty <> 0 AND @exist > 0
BEGIN
    -- Existing lookup, unchanged.
END
```

New lines still produce their existing `Order Entry Logs` record. Updates that
use the previous committed quantity retain the original lookup and log values.
Zero-quantity changes retain the existing no-log behavior. The fix does not
disable logging, remove historical rows, add indexes, change order/inventory/tax
values, change history numbering or increase application command timeouts.

This is targeted at **new-line Save timeouts**, including imported new orders.
The existing-line update lookup remains unchanged and can still be expensive
on this unindexed table. Any later index or query/type optimization needs its
own review; this script does not claim to solve all editing performance issues
or change the legacy trigger's single-row assumptions.

## Personal execution in SSMS

**Prepared and tested locally; not applied to live by the assistant.**

Later read-only verification on October 15 found the live trigger fingerprint
`5CFB6436B9EAB3B846B8DFAB5AB9AAB52215515CB88C4BB44A70D10234058A8B`,
matching the prepared fixed definition. Order 18879 and its item/audit events
were present. This confirms the fixed definition is installed and a subsequent
order persisted; it does not independently verify every import or edit workflow
or identify who executed the ALTER. Do not repeat deployment solely because the
original diagnosis above describes the old definition.

1. Connect SSMS to `172.16.4.50` / `HO_PO_DB`. Open the entire fix file.
   The script checks the server name and exact reviewed trigger fingerprint;
   a different/customized or disabled trigger stops for review.
2. Use a quiet maintenance period. Leave `@Commit=0` for the first run and
   execute the entire file outside any existing transaction.
3. Save the returned `OriginalTriggerRestoreSql` for reversal. Require
   `REHEARSAL PASSED`; that run rolls the trigger change back.
4. After reviewing the result, set `@Commit=1` and execute the whole file again.
   Require `COMMITTED`. Repeated execution recognizes the reviewed fixed version.
5. Verify whether the earlier failed order already exists before retrying it.
   Then test a small new Work Order and an imported new order. Verify header,
   lines, totals, commitments, queue details and item-log records in the store.
6. Record the result and any remaining timeout. Do not disable the trigger or
   delete its history to make the Save complete.

The file supplies its own database selection and rehearsal transaction, so run
it directly in SSMS, not through **Add SQL script** in the updater. If reversal
is needed, personally review and execute the saved original ALTER definition
on the same target. Reversal restores the original trigger, including its slow
lookup. No production Save or SQL repair was executed during diagnosis.

## Reported Order ID gaps

The user reported 18868, then 18870, then 18879. Read-only checks found Order
`ID` still uses seed 1 and increment 1, with last allocated value 18879 at the
snapshot. Orders 18868 and 18870 existed as closed, and 18879 existed as open.
No rows for 18869 or 18871–18878 were found in the current Order table, RMS
OrderHistory or custom audit history.

Failed insert transactions can consume identity values even when all order
and audit rows roll back. This is consistent with the recent timed-out Save
attempts and explains how later successful saves can show gaps without changing
the identity increment. The available rows do not prove the cause of each
individual gap, particularly the older 18869 gap. No reseed or reuse of those
values was performed. See [SQL Server's documented identity rollback behavior](https://learn.microsoft.com/en-us/sql/t-sql/functions/scope-identity-transact-sql).

## Local validation

[HO_SaveTimeout_LocalDB_Test.ps1](../Tests/HO_SaveTimeout_LocalDB_Test.ps1)
creates and removes a GUID-owned LocalDB instance containing a synthetic
`HO_PO_DB` at compatibility level 130. It uses the exact exported legacy trigger
definition, not live store data or credentials.

**16 checks passed.** They cover target/fingerprint/disabled-trigger guards,
default rollback, explicit commit, repeated execution, preserved fractional
insert/edit/zero-quantity log values and unchanged physical stock/commitments.
In a 100,001-row synthetic heap, new-line log-table logical reads fell from
**5,884 to 1**. This is a synthetic read-count result, not a promised live Save
duration. Existing-line edit logs matched the legacy output.

Run from the repository root:

```powershell
& '.\Database Components\Tests\HO_SaveTimeout_LocalDB_Test.ps1'
```

Results are saved in a GUID-owned `TestResults/HO-SaveTimeout-*` folder. The
legacy SQL fixture must retain its original bytes because the deployment
guard compares the exact SQL Server module fingerprint.
