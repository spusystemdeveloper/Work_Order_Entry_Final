# Order history, notifications, edit protection, and shortage acceptance

Implemented October 6, 2026 in Work Order Entry. These tools support both
branch profiles; ready notifications require branch queueing and the
For Invoicing button to be enabled.
Warehouse shortage notifications require queue processing but do not depend
on the For Invoicing button or the optional history migration.

## Using the tools

Database installation and maintenance can use the separate
[Work Order Database Updater](Database%20Updater/README.md). It lets the database
maintainer preview selected history/queue migrations, function updates, and
performance indexes, then records the applied file hashes. It does not change
the app's business process automatically or distribute new application EXEs.

**Order history:** Choose **Order actions → History...** in the bottom status
bar. A recalled order's number is filled in automatically. Enter another order
number to inspect its recorded history, including closed or cancelled orders.
The window loads in the background and shows newest events first.

History records order creation, edits, type conversion, line changes, price
approvals, grouping, picker/preparation changes, shortages, queue cancellation,
queue completion, and RMS transaction posting. It starts when the branch
enables the database migration; it does not reconstruct earlier history.
Details show previous and new values for the recorded fields. This is an
operational history, not a complete copy of every RMS column or a tamper-proof ledger.

Add-in saves, cancellation, grouping, and invoicing handoff record the logged-in
username and register. Warehouse and other external writes use their SQL login
and host as a fallback; picking details identify the selected picker. RMS posting
details include the RMS cashier ID and transaction number. A selected picker
does not identify which supervisor clicked an action. Times use the database
server's local clock. Price approvals record the approver name, never the password.

**Ready for invoicing:** Every 15 seconds, the status bar and For Invoice button
show the number of eligible prepared groups across the store. The first
successful check establishes a baseline; later additions show **newly ready -
open**. Click the status link or For Invoice to open the invoicing list. A newly
ready group is selected automatically, but printing/invoicing still requires
the cashier's normal action. Opening the list acknowledges the notification.
The count refreshes when the dialog closes.

Eligibility matches the existing invoicing list: prepared groups joined to an
open RMS order, with that order created within the past 60 days. A group that
leaves and later returns to Prepared can notify again. Polling failures show
**Ready status unavailable - retrying**, keep the last successful baseline, and
log once until recovery. The notifier never changes orders or queue statuses.
Set the application `appSettings` key `EnableReadyOrderNotifications` to `false`
to disable polling; its default is `true`.

**Edit protection:** Recall reads the order's state before and after loading.
Saving or cancelling validates that state again inside the write transaction,
before changing the header, lines, commitments, or queue. The check covers the
header, entries, ownership/group records, and picker/prepared state. Changes by
another cashier, a warehouse terminal, or RMS therefore block a stale save.
Closed or removed orders and missing recall baselines are also rejected.

The conflict message keeps unsaved edits on screen. Choose **Order actions →
Reload order** to fetch the latest version; the app asks before discarding those
edits. Review and reapply the required changes. There is no force-overwrite action.
Database locks remain held through the save/cancel transaction. This protection
does not lock an order for the entire time a cashier has it on screen.

## Warehouse shortage notifications

Work Order Entry checks for active warehouse shortages every 15 seconds in the
background. The bottom status bar shows **Shortage WO #... - open** for one
order or **Shortages: N - open** for several. Newly flagged orders and changed
ordered/found quantities use a red **New / changed** alert; existing shortages
remain available in orange. The first successful check establishes a baseline.
Unchanged shortages do not repeatedly create new alerts, and a cleared shortage
can notify again if the warehouse flags it later.

Click the status link or **Order actions → Warehouse shortages...**. The list
shows Work Order number, customer, item, ordered quantity, physical found count,
and picker. New/changed orders appear first. Select a line and choose **Open
selected order** or double-click it to recall that Work Order directly. Closing
the list acknowledges nothing. Successful recall acknowledges only that order
and only the shortage quantities actually loaded; other new alerts remain.

Use **Search** to find an order number, customer, item code, description, or
picker. Choose a picker, **New / changed only**, and/or **Zero found only** to
narrow the list; these filters combine. A missing count is shown as Not recorded
and is excluded from Zero found only. **Clear filters** restores the full list.
The result count shows visible versus total shortage lines. Filtering preserves
a selected order while it remains visible; an empty result disables Open.
Search treats punctuation and wildcard characters literally and uses only the
loaded list, so typing does not contact the database. To fetch a later warehouse
change, close and reopen the list; normal background notifications continue.

Opening checks the current order before replacing the screen and uses the
normal recalled-state protection. If you have a working order or pending
reduction, confirm before discarding it. Notifications never automatically
reload an open order, save it, or adjust stock. The normal shortage notice and
customer-agreement action remain available after recall.

The list covers open type-2 Work Orders across the selected store, created in
the past 60 days, with an active queue header and a positive ordered quantity.
It includes zero found and missing warehouse counts so the cashier can follow
up; accepting a reduction still requires a positive recorded count. Quotations,
closed/cancelled queues, removed lines, and older orders are excluded.

Failed checks show **Shortage check unavailable - retrying**, keep the last
successful baseline, and log once until recovery. Disable polling with the
application `appSettings` key `EnableShortageNotifications=false`; it defaults
to true and is also disabled when branch queue processing is off. No new
database migration is required for these alerts.

## Accepting a warehouse shortage

Recall the queued Work Order, select its amber shortage line, and choose
**Order actions → Reduce selected shortage to found quantity...**. The action
uses the warehouse's saved **QtyPre** count from recall, rather than the RMS
stock-on-hand number or a count typed into the cashier's Prepared column.

A Work Order shortage alert can refer to only some of its items. The reduction
uses the current row in **Selected Items**. If a different item is selected,
the warning names its item code and directs the user to the shortage-highlighted
row. If the warehouse flagged the item after recall, reload the order first.

The confirmation shows the ordered/found quantities, revised line total,
reservation release, and warehouse picker. Check the customer-agreement box
and enter a note. **Apply to order** changes the screen only: the line turns blue,
the total recalculates, and a status message says to save. The quantity and
Prepared cells are locked while the decision is pending. **Undo selected
shortage reduction** restores the original quantity before saving. Reload also
discards pending decisions after its normal confirmation.
Closing the application asks whether to discard pending reductions. Choose No
to return and save. If discarded, original quantities are restored before the
existing draft save; draft recovery does not store customer-agreement decisions.

Press the normal **Save** button to commit the reduced quantity, order amounts,
reservation release, prepared status, and **Shortage accepted** history event
in one transaction. The event records the cashier, physical count, picker, and
customer note. The picker is preserved. Other orders' reservations and RMS
`Item.Quantity` (stock on hand) are unchanged; this action is not an inventory
adjustment. If another terminal changes the order or warehouse count, saving
is blocked and the pending edits stay on screen for review/reload.

For example, reducing 10 ordered pieces to 7 physically found releases 3 from
`Item.QuantityCommitted`. At a unit price of 112, the line becomes 784. A stock
discrepancy still needs the branch's normal separate stock-count process.

This action requires queue processing and order history to be enabled. It
accepts only positive found quantities below the original quantity on an open
type-2 inventory order. Zero found stays pending; use the existing Remove
workflow after agreement with the customer. Duplicate order or queue lines for
the same item are rejected because the legacy queue identifies items by SKU,
not order-entry ID. Quotations and non-inventory items are excluded.

## Branch deployment

1. Restore a branch backup into a disposable test database.
2. Run `Database Components/Migrations/Prepare_WorkOrder_Edit_Checks.sql`.
   It adds non-unique lookup indexes only when equivalent leading-key indexes
   are absent. This reduces scanning and broad locking on large queues.
3. To enable history, run
   `Database Components/Migrations/Enable_WorkOrder_History.sql` in that restore.
   It creates `SOD_WO_History` and additive audit triggers on Order, OrderEntry,
   Queueing, QueueingItems, Transaction, and the optional SOD_WO_LOGS table.
   If SOD_WO_LOGS is installed later, rerun the history migration to add its trigger.
4. Allow the application account to select `dbo.SOD_WO_History` and insert
   customer shortage-decision events into it. Review trigger
   permissions, write overhead, storage growth, and retention with the branch DBA.
   Existing RMS columns and the existing transaction-close trigger are preserved.
5. Deploy the updated application to the test terminals and verify the scenarios
   below, including existing branch triggers and POS settlement. Then schedule
   the reviewed scripts and application deployment for the branch.

The scripts use SQL Server 2016 syntax, including pre-SP1 trigger creation.
The history migration rejects an incompatible existing history table or legacy
LOB types in the order text fields read by AFTER triggers. It runs transactionally
and can be rerun. The application never creates audit tables automatically;
without the history migration, the history window reports that the feature is
not enabled and shortage acceptance is blocked, while notifications and edit
protection still operate. No additional migration is needed for shortage acceptance.

These migrations are separate additions to the original dated 49-object
deployment package and are not automatically included in its branch drivers.
If history triggers must be rolled back, disable only the `SOD_trg_WO_History_*`
triggers and preserve `SOD_WO_History` for review.

## Verification

On October 14, 2026, both solutions built successfully in x86 Debug and Release,
all 103 unit tests passed, and all 99 isolated SQL/UI smoke checks passed. The
history and shortage confirmation windows were rendered and visually checked. The SQL fixture ran on LocalDB
17 with compatibility level 130; live branch databases were not changed.

The database-free suite includes 103 tests, with 12 additions covering edit
validation and ready notification transitions and 16 covering shortage eligibility,
reservation calculations, picker preservation, and explicit consent, plus 9
covering shortage alert transitions and selection, plus 13 covering local filters
and unchanged reservation arithmetic, plus 9 covering shared price approval and
safe timing logs, plus 2 checking dialog-time exclusion. Run `Run-UnitTests.ps1` from
the repository root.

The isolated SQL smoke test builds a small RMS-shaped fixture in a new LocalDB
instance, applies the actual invoicing view and the migrations, tests the runtime
helpers and history/confirmation windows, and removes its own database and instance afterward.
Shortage checks cover changed warehouse counts, injected save failure and rollback,
revised amounts, reservation release, unchanged stock on hand/other orders, and
the customer-agreement history event.
The fixture also uses RMS's smallint item type and checks fresh queue reads,
repeated shortage writes after cashier changes, automatic update detection,
clearing shortages, and card stage/editability transitions.
Cashier-alert checks cover eligibility, physical counts including zero/missing
values, new/cleared/reflagged transitions, preferred selection, and the selected
order returned by the alert window.
Filter checks cover combined search/picker/new/zero filters, empty results,
clear/reset, retained selection, and opening the correct filtered order. The
transaction fixture uses the extracted reservation rule and still verifies full
rollback, unchanged stock on hand, and preserved reservations for other orders.
Six additional checks cover the Set Tax save conflict and corrected production
tax preparation with RMS rowversions, voucher handling, reversal, and rollback.

This focused refactor separates the list model from its controls and centralizes
existing reservation arithmetic. Save transaction boundaries, business statuses,
price/tax calculation, and RMS invoicing behavior are preserved. The broader
legacy main form has not been rewritten. A local, database-free check of 50
searches over 5,000 loaded rows measured median 1.50 ms / p95 2.38 ms; branch
database and end-to-end processing speed still require branch-load measurements.

## Save/recall timing and refactor checks

The October 7 refactor shares the existing price approval rules and batches item
and order-entry reads within the locked existing-order save transaction. Save
results distinguish cancellation, failed validation, conflicts, failures, and
committed saves. A printing/grouping/cleanup problem after commit is reported as
an **Order Saved - Follow-up Warning**. Tax calculations and the business workflow
remain unchanged. No additional migration is needed.

Timing is optional and defaults off. In the deployed
`Work Order Entry_V8.exe.config`, change the `EnableOrderPerformanceLogging`
setting from `false` to `true`, then restart Work Order Entry. Run a few ordinary
save/recall tests with representative order sizes. Set it back to `false` and
restart when the measurements are complete.

The daily CSV is saved under
`%LOCALAPPDATA%\WorkOrderEntry\Logs\OrderTiming_YYYY-MM-DD.csv`.
It contains the time, phase, order number, line count, elapsed milliseconds, and
outcome, without item descriptions, customer information, passwords, or connection
strings. New-order database timing uses order number 0 because the numbers are
generated during saving; catalogue timing uses line count 0.

Compare these phases separately:

- `InsertDatabase` / `UpdateDatabase`: database work after confirming the save,
  including transaction checks, reads, writes, and commit.
- `RecallReadAndPopulate`: recall checks, reads, and screen population, ending
  before the final shortage/quotation notice.
- `RefreshDetails`: screen reset and the requested catalogue refresh.
- `CatalogueReload`: the catalogue load within that refresh; this time is already
  included in `RefreshDetails`, so do not add the two measurements together.

Confirmation, grouping, and printing waits are outside database-save timing.
The October 13 recall change also records `RecallStartSnapshot`, `RecallTaxReason`,
`RecallHeaderRead`, `RecallCustomerSetup`, `RecallEntryRead`, `RecallLastPrices`,
`RecallQueueRead`, `RecallPopulateAndPricing`, `RecallFinalValidation`,
`RecallTaxAndTotals`, and `RecallCustomerSummary`. All are components of
`RecallReadAndPopulate`; do not add them to the overall time. Row population
still includes existing approval-price reads. `Fallback` on last prices means the batch failed and the old
per-item helper was used.

From the October 14 build, `RecallItemTaxReadAndApply` measures the batched item
tax lookup and row application; `RecallTotalsCalculation` measures the existing
total calculation and control updates. Both are included in `RecallTaxAndTotals`.
`RecallCreditNoticeWait` separately records time spent in the existing credit-limit
warning during recall. That wait is excluded from `RecallTotalsCalculation`,
`RecallTaxAndTotals`, and `RecallReadAndPopulate`. The warning condition and display
are unchanged. Earlier builds counted this wait, so comparing old/new processing
times without accounting for notices can imply an improvement that did not occur.
Other helper error dialogs can still contribute wait time.

Last prices, warehouse details, and recalled item tax flags are loaded in batches and used only for
that recall. Each new recall uses fresh contexts. Starting/final snapshot checks
and save-time transaction locks are unchanged. No migration is required.

For a local read-only component comparison, build Debug and run
`Database Components/Tests/Recall_ReadOnly_Benchmark.ps1` with explicit `OrderID`,
`Server`, and `Database` arguments. It uses Windows authentication and SELECTs
only. It compares results and records three samples in
`TestResults/Recall-ReadOnly-Benchmark.csv`; it does not time the UI or simulate
many workstations. Database caching, query plans, and measurement order can
affect the samples.

`Database Components/Tests/RecallTax_ReadOnly_Benchmark.ps1` compares old and
batch item-tax SELECTs for an explicit order/server/database with Windows
authentication. It changes no branch records and writes order/date-specific CSV
results under `TestResults`. It excludes UI updates and notice waits.

For deeper price diagnostics, `Database Components/Tests/RecallPrice_ReadOnly_Analysis.ps1`
runs five SELECT-only samples per specified order, then captures a separate actual
plan and I/O. Use `OrderIDsText`, `Server`, `Database`, and `RunLabel` (`Baseline`
or `AfterIndex`). It writes timing/signature CSVs and plans under `TestResults`.
Plan capture is outside timed samples; for orders over 200 distinct codes, the
diagnostic plan represents the first chunk. Price signatures allow comparison
without logging customer names or returned price values.

`Database Components/Migrations/Improve_WorkOrder_ItemLookup.sql` is an optional
database index for stores lacking a covered ItemLookupCode index. It is separate
from the automatic branch drivers and requires no new application executable.
See `Database Components/Migrations/WORKORDER_ITEM_LOOKUP_GUIDE.md` for deployment
and validation. Index creation takes locks, so use a quiet period.

An error dialog inside an existing helper may still contribute to a phase's
duration. These are phase measurements, not a complete stopwatch of every F12
step or proof of faster store processing. Logging failures are ignored so normal
order processing can continue. See `UNIT_TEST_GUIDE.md` for the manual checklist.
It does not read application connection settings or connect to a branch database.
Run it after Debug builds of both solutions with Windows PowerShell 5.1:

```powershell
powershell.exe -NoProfile -File '.\Database Components\Tests\OrderTools_LocalDB_SmokeTest.ps1'
```

It selects the x86 PowerShell runtime automatically to match the application.
LocalDB must already be installed. The fixture uses database compatibility
level 130; it is not a substitute for testing the actual SQL Server 2016 branch restore.

Before branch deployment, verify:

- Two cashiers recall the same order: the second save is blocked after the first commits.
- A warehouse terminal changes a picker or prepared quantity while an order is open: saving stale data is blocked.
- An RMS settlement or cancellation closes an order: stale saves and repeated cancellation are blocked.
- A new item or group change is detected, even if the header timestamp is unchanged.
- A failed save rolls back the business changes and their history together.
- A newly prepared group notifies once; opening it retains the normal explicit invoicing action.
- Notifications respect disabled queueing, the 60-day cutoff, and temporary database failures.
- History identifies cashier, approver, picker, and RMS cashier ID according to the source of each event.
- Accept 7 of 10 pieces: confirm consent, undo before saving, then accept and save;
  verify quantity, amount, release of 3 reserved pieces, picker, and history.
- Change the warehouse count after recall: the shortage save is blocked. Check
  zero-found and duplicate-item lines, branch permissions, and existing RMS triggers.
- With Work Order Entry already running, flag a shortage in the warehouse:
  the status link appears within a normal polling interval. Open it and verify
  the customer/count/picker and correct recalled order. Repeat unchanged polls,
  change the found count, clear/reflag, and confirm the expected alert transitions.
- Try opening an alert while editing another order or holding a pending
  reduction: declining the discard confirmation preserves the screen and alert.
- Search and combine picker/new/zero filters; clear them, try a no-match search,
  then open a filtered order. Recheck normal Work Order and quotation saves,
  type conversion, deleted lines, fractional quantities, and invoice handoff.
