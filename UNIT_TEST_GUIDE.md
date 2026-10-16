# Work Order Entry unit tests

These tests check small business rules without opening the application,
connecting to DVO/GSC, or changing database records.

## What is tested

### Smaller store screens

After building Debug, run:

```powershell
& '.\Database Components\Tests\SmallScreenLayout_UI_Test.ps1'
& '.\Database Components\Tests\SettingsLayout_UI_Test.ps1'
& '.\Database Components\Tests\CheckWOLayout_UI_Test.ps1'
& '.\Database Components\Tests\ImportLayout_UI_Test.ps1'
& '.\Database Components\Tests\PrintLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

The 875 small-screen checks instantiate compiled forms without showing them,
logging in or executing SQL/order actions. They cover simulated 1024x768 and
1366x768 layouts at 100% and 125%, dialog fitting/restoring, Settings scrollable
content, import anchors, totals boundaries, main input availability, selected
item viewports and action-button overlap with Import and For Invoice displayed.
One check invokes only the new Settings presentation handler to verify its
Resize hookup. Main layout events execute naturally when the fixture resizes.
The existing Debug store configuration is hash-checked and preserved.

The fixture assumes a 40-pixel taskbar and a 704-pixel maximized client height.
Simulated scaling does not reproduce every DPI/font/driver/taskbar combination.
The application itself uses the actual monitor WorkingArea to fit dialogs.
Settings scrollbars keep all existing fields reachable when scaling makes the
contents larger than the available viewport; no fonts are automatically shrunk.

Manual check on each intended unit:

1. Use its actual 1024x768 or 1366x768 resolution and Windows scaling. Log in
   normally; verify customer/sales fields, search, Barcode and Show Image remain
   reachable. Confirm Save, Recall, Cancel, Close and the other existing actions.
2. Check subtotal, VAT and total visibility. Add several owned test-order lines
   and use both grids' scrolling to reach quantities, price, SOH, commitments,
   prepared counts and the other existing columns. Values/formats must match.
3. Open Settings, Recall, Check WO, history and shortage alerts. Confirm windows
   fit the usable screen, long grids scroll, and existing filters/actions work.
   At higher scaling, scroll Settings to its right/bottom sections and back.
4. Check CSV/website import dialogs with owned test files, and restore/maximize
   both print previews. Confirm buttons remain reachable and report contents,
   printer choices and copy counts remain unchanged.
5. Repeat with that unit's taskbar position/size and actual DPI settings. Report
   any clipping with the resolution, scaling percentage and affected window.

The unchanged business and integration tests remain applicable; the layout
fixture does not replace live workstation or authorized test-order validation.

### Print options and preview presentation

After building Debug and Release, run:

```powershell
& '.\Database Components\Tests\PrintLayout_UI_Test.ps1'
& '.\Database Components\Tests\Packaging_CleanFolder_Test.ps1'
```

The 91 isolated Print UI checks instantiate the three compiled forms without
showing them. Only the safe print-options Load handler runs to verify field
reset and existing keyboard validation; neither preview Load handler nor the
Print button is invoked. Checks cover radio grouping/defaults, existing Enter
action/tab order, toolbar availability, embedded report/data-source names,
timer defaults, docking, resizing and simulated 125% geometry. No SQL lookup,
report refresh, printer dialog or spool job runs in this UI fixture.

The larger print-choice revision adds checks for 14-point choice/button text,
24-point order digits, full-width clickable choices, separated rows and labels
that fit. In the Designer and on the workstation, confirm that Work Order /
Quotation and Pick List are easy to read, then enter a known order number and
use the same existing Print/Enter action. The renamed first choice still uses
the original order-type/template selection; it does not force a Work Order
template for quotations.

Packaging checks separately render synthetic Work Order and Pick List PDFs
using the packaged ReportViewer DLLs. They do not prove physical printer,
driver, actual order data or copy-count behavior. Print code-behind and RDLCs
remain unchanged; this revision changes only outer forms and Designer layout.

Manual checklist on the test workstation:

1. Open frmPrintType, frmPrintWo and frmPrintPicklist in the Designer. Confirm
   the aligned options, order-number label, Print button and preview headings.
2. Open Print from Work Order Entry. Confirm the existing default template,
   Tab/Shift+Tab navigation, arrow-key radio selection and Enter action. Test
   invalid/missing order numbers and both valid report choices as before.
3. Preview a known Work Order, Sales Quotation and Pick List. Compare customer,
   item text, quantities, totals, VAT, dates, page count and printed layout with
   the previous version. The new headings/help are outside the report and must
   not appear in PDF exports or printed pages.
4. Check the existing toolbar's page navigation, zoom, find, page setup and
   export. Resize/restore the preview; the toolbar and page area should fill
   the space below the heading. Existing previews still open maximized.
5. Open the existing Print dialog, verify printer selection and copy-count
   options, then Cancel. If physical output is needed, print an authorized test
   report and compare it with the previous version. Verify closing/reopening
   previews behaves as before. No automatic print action was added.

Check actual workstation DPI as well; the automated scale test checks geometry
only. Printer selection, copy counts and page settings remain controlled by
the existing ReportViewer dialog and unchanged printing code.

### Check WO presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\CheckWOLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

The 180 isolated Check WO checks instantiate the compiled dialog without
showing it or invoking Load. Synthetic DataTables and actual generated
SOD_fntbl_CheckOpenWOResult instances exercise ten-column mapping/ordinals,
headers, hidden item columns, nulls, fractional Double quantities (including
0.125 and 0.375), existing date strings, repeated/empty binding, selected IDs,
sorting, column reordering and the existing Close handler. Geometry is checked
at default, smaller and larger window sizes, plus simulated 125% scaling.
No SQL function, login, store lookup or save is executed. Simulated scaling
does not substitute for actual workstation DPI validation.

Manual check:

1. Open frmCheckWO in Visual Studio Designer. Confirm the item heading, grid
   styles, column headers/widths and Close button match the new layout.
2. In the rebuilt application, select an item and click Check WO. The existing
   caller still passes that item's code/name to SOD_fntbl_CheckOpenWO. Check WO
   lists orders for that item; it does not add a search/filter or order action.
3. Compare known open-order IDs, statuses, companies, accounts, quantities,
   order takers, dates and age labels with the previous version. Verify an item
   with no open orders too. Item lookup/description columns stay hidden; the
   selected item description remains in the heading.
4. Check long company/order-taker text and fractional quantities. Select rows,
   use arrow keys, click sortable headers and reorder columns as before. Confirm
   rows remain read-only and quantity/date values are unchanged.
5. Resize the dialog and check display scaling. At narrow widths, horizontal
   scrolling retains the readable column minimums. Confirm Close and the window
   close control still dispose the dialog and it can be opened again.

### Import presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\ImportLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
& '.\Database Components\Tests\Packaging_CleanFolder_Test.ps1'
```

The 83 isolated Import checks instantiate the compiled forms without showing
them or running their Load/import handlers. Synthetic website CSV data exercises
the existing Show All search/grouping path (including apostrophes, long names,
dates and blank values), repeated binding, selected order IDs, formatting and
column order. Show All=True deliberately bypasses SQL duplicate-order checks.
The test also exercises CSV detail presentation, retained action states and
mnemonics, missing-item descriptions, and normal/simulated 125% geometry. It
does not execute the Import/Finish buttons, network log writes, MySQL Refresh,
actual file import, pricing or store saves. Shared CSV data is restored afterward.

For the presentation revision, the previous executable passed the same 74
semantic/layout assertions that apply to its older appearance. Website and
missing-item handler source remained byte-for-byte unchanged; CSV code differs
only in GridDesign presentation settings. These checks do not prove live
duplicate filtering, external connectivity or actual workstation DPI behavior.

Manual checklist on the intended test installation:

1. Open frmWebsiteImport, frmUploader and frmImportItems in the Designer.
   Confirm aligned controls, larger grids and the import-button styling.
   CSV detail columns still initialize when validation starts, as before.
2. Press F6. Confirm the standard file picker still offers Supported Import
   Files, Excel Workbook and CSV File filters; cancellation leaves the order
   unchanged. Confirm the branch Import visibility/enable setting still applies.
3. For a known website test file, verify Enter in Search, Show All, Refresh,
   arrow-key selection and Import Order retain their existing behavior. Check
   that long customer names wrap, dates remain complete and the selected ID
   imports the intended order. Verify already-imported orders with Show All
   unchecked against the test database.
4. For a known quotation CSV on an authorized test configuration, check the
   filename, Import (Alt+I), detail messages, Total/Success/Error counts and
   existing Finish (Alt+F), Report visibility and Close behavior. Verify valid,
   invalid and duplicate rows still follow the established import process.
5. For a known workbook with missing item codes, confirm the guidance list
   retains complete descriptions and still clears when closed. No automatic
   replacement or new search action was added.

Confirm Tab navigation and display scaling on the target workstation. Use
owned test files/orders for actual import and save validation.

### Settings presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\SettingsLayout_UI_Test.ps1'
```

This instantiates the compiled Settings form without showing it or executing
its Load/save handlers. The 776 checks cover control boundaries and overlap at
normal and simulated 125% geometry, password masks, retained Alt+O mnemonics,
distinct Save styling, field/button focus cues and existing queue/assigned
database dependencies. It does not connect to SQL or save settings. Simulated
scaling does not replace testing on actual high-DPI displays.

Manual check on an authorized test installation:

1. Open frmSettings in Visual Studio Designer. Confirm all six existing sections
   and the aligned password rows are visible. Layout is in the Designer file;
   focus shading/borders are presentation-only Enter/Leave handlers.
2. Open Settings normally. Confirm existing values and password masks remain.
   Use Tab and Shift+Tab through fields, checkboxes and buttons; verify clear
   focus shading/borders without changing values. Existing tab indices remain.
3. Toggle Branch Queue and Allow Branch Selection. Confirm their existing
   dependent controls enable/disable as before. Review every existing checkbox.
4. On a disposable/test configuration only, check each existing save action
   separately. Apply defaults retains its original validation/save/close action;
   each Update password button retains its matching/nonempty confirmation
   requirements and saves only its own password. Apply import and Apply
   connection still save their own settings and restart the application.
5. The blue Save branch / order options button retains the existing combined
   branch, queue and order-option save action. It does not save the other
   sections. Reopen Settings to verify intended test values persisted.

Do not exercise password updates, connection changes or application restart
against an operational store simply to validate appearance.

After changes to grid/Designer checkbox styles, build Debug and run:

```powershell
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

This instantiates the compiled form without showing it, logging in, connecting
to SQL or saving orders. Its 28 checks exercise actual UP-STORE FormattedValue
for Nothing, DBNull, False and True, before/after runtime styling, plus the
temporary new-row placeholder used while recalling. Formatting must return a
Boolean without changing the cell's actual value or raising DataError. Preserve
the checkbox's original Boolean NullValue=False when copying styles; an empty
text NullValue is invalid for a two-state checkbox. The test-only error observer
fails on any DataError and must not be copied into production as suppression.

For the fix, restart the rebuilt app and recall both a quotation and a Work
Order. Confirm there is no formatting dialog and UP-STORE checkbox states remain
correct, along with quantities, prices and totals. Automated checks reproduce
the formatting defect without executing a live recall or save.

For the Recall presentation update, restart the rebuilt app, press **F11**, and
open the existing Work Order or Sales Quotation list. Check bold order numbers,
complete date/time strings, wrapped customer/comment text and the blue selected
row. Resize the dialog: OK/Cancel should remain at the bottom right and long
rows should grow as needed. At narrow widths, the existing horizontal scroll
allows access to the larger columns; no all-row automatic measurement is used.

Click each column header and search as before. Verify counts and matches,
including a customer containing an apostrophe. Check Down/Left/Right, Enter,
double-click, OK, Cancel and Escape using known disposable test orders, and
confirm the recalled ID is the selected one. Verify the existing Work Order/
Quotation and Show All Entries filters. The Customer field still shows the
existing RMS customer account text; this change does not substitute a new name
query or change date formats or search matching.

Automated validation compared synthetic filters, selection IDs, sorting,
rebinding and controls with the previous build and inspected long-text renders.
The Recall logic source was unchanged. Live order loading, order-taker queries,
actual Enter/double-click/OK recall execution and workstation DPI remain manual
integration checks. Presentation defaults are in frmRecall.Designer.vb.

The main form now serializes its current presentation defaults in
`Work Order Entry/Forms/frmItemLookUp.Designer.vb`. After building, close and reopen
`frmItemLookUp.vb [Design]` in Visual Studio to refresh the preview. Both grids
should show the current header/row styling and column sizes; the left panel
should show the current input fonts, spacing and inactive focus outlines.
No login or database connection is needed to view these defaults. Actual data,
focused outlines and warning/approval colors still depend on runtime state.

The lookup's 19 Designer columns retain the existing view field names/order,
including the hidden Boolean Inactive field. Preserve DataPropertyName, column
order and types when editing presentation; application callers also use ordinal
positions. Keep automatic binding enabled. Future style revisions should keep
the Designer defaults and ApplyGridReadability/GridColumnWidth/
ApplyLeftPanelReadability consistent, since those runtime routines still apply
the existing presentation after initialization or data loading.

Validation compared Designer-only and runtime native-control snapshots with the
previous build at the default scale and compared synthetic binding, sorting and
rebinding results. It did not automate Visual Studio's Designer host or a live
RMS workflow. Manually reopen the Designer and the rebuilt app to confirm the
appearance on the workstation's actual display scaling.

For the SOH column fix, restart the rebuilt app and search for items with large,
negative or fractional stock on hand. The full SOH value should be visible as
the column expands for displayed rows; scroll to other results and verify the
same behavior. The existing SOH format and actual quantities are unchanged.
The description absorbs the width difference; buttons and keyboard shortcuts
remain in place. Automated coverage used synthetic quantities, not store writes.

For the Show Image layout fix, restart the rebuilt app and confirm the checkbox
is fully visible below the barcode row, with no horizontal scrollbar in the
left entry panel. Resize the main window and check the same spacing. After
login, toggle Show Image with an item selected and confirm its existing image
preview behavior. Button locations and Enter/F2/F4/F5 search shortcuts should
remain unchanged. Automated checks used three simulated layout scales; actual
workstation display scaling and image loading remain manual checks.

For item search, confirm every lookup row is bold, including zero committed
stock. Submit searches with Enter, F2 and the search button: the Searching items
indicator should animate during SQL work, and the main form must stay unavailable
until results are ready. Existing contains/multiple-keyword matches, TOP 500,
F4/F5 history controls and no-match fallback remain unchanged. The SQL query can
still take seconds on large ntext-backed store views; UI responsiveness is not
a reduction in database execution time. No stale inventory-result cache is used.

For SQL search timing, run `Database Components/Tests/ItemSearch_ReadOnly_Timing.ps1`
with explicit `-Server` and `-Database` parameters. It performs reads only using
Windows authentication. See [the investigation findings](Database%20Components/Tests/ITEM_SEARCH_PERFORMANCE_FINDINGS.md)
for the measured slowdown, rejected alternatives and the remaining restored-copy
maintenance experiment. It is a diagnostic, not a speed assertion or full UI test.

Run `Database Components/Tests/ItemSearch_LocalDB_Test.ps1` after building the
Debug x86 app. It needs Windows PowerShell, LocalDB and the existing RMS registry
prerequisite for the legacy connection-module initializer. It replaces closed
contexts/connection fields only in its own test process and validates the owned
fixture target before application SQL. It verifies message-timer progress during
a delayed SQL read, modal owner locking/restoration, prevention of early close,
the existing result-publication contract and exact returned values. The real
application/login is not shown; no store configuration, registry or data changes.

For the left entry panel, check aligned labels/fields and readable customer,
sales, payment and comment text before and after login. Tab/click into item
search: its outline should turn blue, then clear when focus leaves. Enabling
search without focusing it must not leave a highlight. Check the same outline
behavior when using the existing barcode mode. Verify F2/F7/F8 and the existing
tab order still work, all buttons stay in place, and customer/sales read-only
rules and payment choices remain unchanged. Test the actual workstation's
display scaling for clipping. This is presentation only, with no new SQL.

For the grid readability changes, search items and recall an existing order.
On initial launch, verify the empty Selected Items grid already has the updated
headers and widths while login is open. Cancel/failed login must not gain access;
after normal login, item loading and draft recovery should behave as before.
Check padded headers, readable descriptions, right-aligned numeric columns,
centered UP-STORE checkboxes and the Last Purchase Price heading. Resize the
window: descriptions should use spare width, with horizontal scrolling available
when the visible columns cannot fit. Check shortage/approval highlights, editable
quantity/price/prepared cells and existing keyboard shortcuts. Stored values and
number formats are unchanged; this is presentation only. Buttons stay in place.

For Order actions/status visibility, open **Settings > Work Order and Queue
Options** and independently toggle **Show Order actions** and **Show order
notifications**, save, then close Settings. Verify the dropdown and the bottom
ready/shortage notification links follow their respective checkboxes. Wait at
least 15 seconds to confirm polling does not redisplay hidden links. Reopen
Settings/application to check persistence, then re-enable each option. Existing
installations without the new keys default to showing both. Notification polling
and For Invoice button/count remain active according to their existing settings;
pending shortage reduction reminders and recall/save/conflict/unsaved-change
dialogs are unaffected. These choices change visibility, not permissions or
business rules. Keep Order actions visible on units that need its reload,
history and shortage reduction/undo commands, or re-enable it before using them.

The optional item-log performance migration has a separate disposable test:

```powershell
& '.\Database Components\Tests\ItemLogLookup_LocalDB_Test.ps1'
```

It requires Windows PowerShell and LocalDB. It verifies the unchanged legacy
trigger's log output, fractional insert/edit/zero cases, stock preservation,
numeric ReferenceID comparisons, original rows, transactional rollback,
idempotency/reused indexes and rejection of incompatible/ambiguous data or index
definitions. It compares logical reads for the same lookup before/after indexing
in a synthetic fixture. No store database or application configuration is used.
See [the deployment guide](Database%20Components/Migrations/WORKORDER_ITEM_LOG_LOOKUP_GUIDE.md).

For the manual Set Tax availability check, open **Settings > Work Order and Queue
Options**, clear **Enable Set Tax button**, click **Save**, and close Settings.
The main Set Tax button should be disabled. Reopen Settings/application to verify
the checkbox stays cleared; enable it again and confirm the button is available.
Existing deployments without `EnableSetTaxButton` keep it enabled by default.
This switch controls only the manual action: selecting an RMS tax-exempt customer
must still apply the existing customer tax rules. VAT/save calculations and
existing customer-type/website-import settings are unchanged.
An isolated temporary-config probe verified the default, persistence across fresh
AppDomains, disabled click guard, re-enable behavior and preserved branch settings;
the Settings options layout was rendered and inspected without loading store data.

Five existing-order concurrency and rollback scenarios are covered separately by:

```powershell
& '.\Database Components\Tests\MultiUser_Save_LocalDB_Test.ps1'
& '.\Database Components\Tests\MultiUser_Save_LocalDB_Test.ps1' -VersionedReads
```

Build the current Debug Work Order Entry application first. This suite runs 39
checks per mode using separate hidden x86/STA Windows PowerShell cashier processes and
independent SQL sessions on a uniquely owned LocalDB database (compatibility 130).
It invokes the actual compiled `SaveExistingOrderChanges` transaction through
reflection and calls the existing `clsRecall.CancelOrder` method. No application
source, registry setting or business rule is changed.

- Two different orders edit the same item with fractional quantities. A held
  shared stock lock establishes overlap. With default SQL locking, a queued
  stock update can block the second stock reader. SQL request snapshots verify
  both actual save transactions reached stock access before release. Both
  deltas, each original line, amounts, tax and cashier history must survive.
  The optional `-VersionedReads` run enables READ_COMMITTED_SNAPSHOT only in its
  disposable fixture, letting both cashiers read the earlier committed item
  state. That stricter lost-update case requires both stock UPDATE statements to
  be waiting before release. No store isolation setting is changed. Explicit
  order snapshot/app-lock protection is exercised in both modes.
- Two cashiers recall the same order before either saves. A fixture-only trigger
  pauses the first inside its header update. The second must wait on its
  application lock, then reject the stale snapshot after the first commits.
  Quantity, commitments, amounts, queue ownership/status and history are checked
  for partial writes. Physical stock remains unchanged in both scenarios.
- A real cancellation pauses at stock release while the already-recalled save
  waits on its application lock. Cancellation wins, zeroes its line, closes the
  order and sets queue status 2. The stale save is rejected. Another order's
  reservation and physical stock survive; a second recall for cancellation is
  rejected without another stock release or cancellation history event.
- A **synthetic POS participant** closes an order, adjusts fixture stock and
  posts transaction/item/tax/tender records in one transaction. It deliberately
  does not acquire the add-in application lock. The actual packaged
  `trg_Transaction_closeQueue.sql` is installed unchanged in the fixture, and
  legacy `@@IDENTITY` supplies the transaction link. A held gate makes the stale
  save wait on the POS-held Order row. After settlement commits, the save must
  reject Closed, preserve all posting links/amounts/stock, and leave queue status 1.
  This verifies the save guard against a posting contract; **it does not execute
  RMS POS/QSRules**, validate every branch trigger, or prove a full RMS sale.
- A fixture-only history trigger injects SQL error 52941 **before commit**, but
  only after verifying the header, both line quantities, both stock commitments,
  both prepared picking rows and both item audit events exist in the transaction.
  The test compares complete pre/post rollback XML, including actual rowversions,
  queue and approval/history rows. A second cashier's previously captured
  original snapshot must then save successfully, applying each difference once.
  Sequence allocations can have gaps after rollback; no counter is reseeded.

The coordinator uses controlled locks and bounded condition waits, rather than
assuming overlap from a fixed delay. A spacer order and 1,000 synthetic filler
orders make normal lookup indexes selective enough to avoid incidental broad
scans serializing the shared-item scenario before it reaches stock arithmetic.
`SharedItem-Overlap.json`, `SameOrder-Overlap.json` and per-cashier results/logs
are retained under the owned `TestResults\MultiUser-<id>` directory. Only the
suite's own database, LocalDB instance and child processes are cleaned up.
The new cases also retain `SaveVsCancel-Overlap.json`,
`SaveVsSettlement-Overlap.json`, `MidSaveFailure-Overlap.json` and the matching
`MidSaveFailure-Before.xml` / `MidSaveFailure-After.xml` rollback evidence.

The workers validate the GUID-named LocalDB endpoint and use Windows authentication
only. The legacy application module initializer may read existing RMS registry
defaults when its assembly initializes; workers dispose its unopened contexts,
replace connection fields only in their own process, and validate the fixture
target before application SQL runs. Consequently this harness expects the same
RMS registry prerequisite as the existing form-based smoke tests. It never opens
a store SQL connection, persists store credentials or changes registry values.

These are default queued Work Order **edit/cancellation transaction** tests and
a synthetic POS posting contract. They do not
automate login, F12 confirmation, printing, full Recall UI, new-order/import saves,
the RMS POS executable, reverse-winner closing races, disconnection after an
uncertain commit, arbitrary deadlocks, branch-specific trigger stacks or load tests.
`Run-UnitTests.ps1` still runs the unit suite only; run this LocalDB suite explicitly.

The separate HO new-line Save timeout fixture is
`Database Components/Tests/HO_SaveTimeout_LocalDB_Test.ps1`. Its 16 checks
verify the fingerprinted legacy trigger fix, rehearsal/commit/repeat guards,
unchanged insert/edit log values and a synthetic logical-read reduction from
5,884 to 1. See the [HO timeout guide](Database%20Components/Recovery/HO_PO_DB_SAVE_TIMEOUT_GUIDE.md).
It uses a disposable LocalDB instance and never executes the fix on a live store.

Five timing-helper tests were removed with the retired application logger,
leaving 101 unit tests. Three overlay regressions check that ignored owners,
disposed owners and handle-creation failures retain correct nested scope behavior.
The tests use hidden synthetic forms without a real app or database.

For Release deployment dependencies, run:

```powershell
& '.\Database Components\Tests\Packaging_CleanFolder_Test.ps1'
```

Build Release first. This copies only the application EXE and its five packaged
ReportViewer DLLs into an owned `TestResults` folder, compiles an x86 probe and
checks compilation of the actual Import entry point, CSV import, XLSX import,
single Excel dependency loading, WO report rendering, pick-list report rendering,
physical ReportViewer loading and the absence of extra dependency DLLs. PDFs use
synthetic data. No store SQL, app login, screen automation or print job is involved.
The check reproduced the old embedding failure before the DLL packaging fix.

Two additional disposable SQL suites cover the October 14 identity incident:

```powershell
& '.\Database Components\Tests\HistoryIdentity_LocalDB_Test.ps1'
& '.\Database Components\Tests\HO_RecoveryScript_LocalDB_Test.ps1'
```

The first passed 27 checks: actual legacy `@@IDENTITY` reproduction, preserving audit
events/permissions, conversion rollback, custom-schema rejection, nested queue
triggers, and fresh/repeated sequence installation. It also tests one-script
legacy upgrades through the updater, rollback of both conversion and trigger
updates, and a later trigger compilation failure. The second passed 27
checks running the entire standalone HO SSMS script on synthetic data, including
default rehearsal rollback, explicit commit, unchanged unrelated old invoice
records, tax/receivable/payment links, repeat runs, optional log references and
rejection after subsequent receivable activity. Five additional checks exercise
copied non-checksummed backups without local history, local-mode rehearsal,
wrong-machine rejection, missing-file rejection and unchanged live requirements.
Five further checks verify the local backup helper, its copy-only/no-checksum
settings, accepted output path, rejection outside the local test machine, and
stopping after backup creation fails.
Three path-editing checks cover unedited placeholders, whitespace-only values,
and successful rehearsal after replacing every occurrence of the placeholder.
Both own and remove their LocalDB
instances. Neither suite writes to live HO_PO_DB.

The separate database updater has its own disposable integration checks:

```powershell
& 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoProfile -ExecutionPolicy Bypass -File '.\Database Components\Tests\DatabaseUpdater_LocalDB_Test.ps1'
```

Build the Debug updater first (included in `Work Order Entry.sln`). These checks
create and remove their own LocalDB instance; they never use store databases or
application credentials. See [the updater guide](Database%20Updater/README.md)
for deployment, rollback, and snapshot limitations.

All 63 updater checks passed on October 14, 2026. The included user-guide SQL
examples are executed through the actual updater runner, including repeat
deployment and function body/verification changes, on its disposable fixture.
Sixteen additional checks exercise HO_PO_DB's focused password-slot column
migration, including preserved values, target/type rejection, repeat deployment,
and rollback on later failure. This HO_PO_DB exists only in the owned LocalDB
instance; the tests do not connect to the live HO server.

- Price labels map to the correct RMS price-level number.
- The Price Level form initially highlights its top row.
- Multiple customer-change prices are counted and processed one row at a time.
- Matching branch database names are accepted and mismatches are rejected.
- Work order type `2` selects the Work Order print template.
- Quotation type `3` selects the Sales Quotation print template.
- Unknown order types are rejected instead of using the wrong template.
- Empty print-template names fall back to Work Order; explicit names are preserved.
- Quotation/Sale, Transfer/Purchase, and WEBSITE filenames select the correct import workflow.
- Unrecognized import filenames are rejected.
- A numeric senior-citizen ID adds the expected description suffix.
- VAT-inclusive pricing separates VAT correctly.
- Tax-exempt pricing removes VAT.
- Non-taxable items do not add VAT.
- Queue order-age rules select priority at 10 and 20 minutes, format elapsed
  time, and select badge text and header colors, including disabled alerts.
- Error logging handles ordinary errors, nested exceptions, and null exceptions.
- Edit-conflict validation accepts unchanged recalled state and rejects changed
  state, missing baselines, different orders/databases, and empty fingerprints.
- Ready notification tracking reports new prepared groups once, deduplicates
  rows, handles groups returning to Prepared, and preserves state after failure.
- Warehouse shortage tracking handles new/reflagged orders, changed counts,
  duplicate/reordered lines, missing/zero counts, failures, and empty-list selection.
- Local shortage filters combine search/new/zero/picker criteria, treat search
  punctuation literally, preserve the loaded quantities, and restore alert priority.
- The extracted reservation rule keeps new/update/remove/conversion arithmetic,
  fractional quantities, quotation behavior, and non-inventory exemptions unchanged.
- Shared price approval retains the saved below-minimum price on recall, requires
  approval for changed/new below-minimum prices, and distinguishes a saved zero
  price from a missing line.
- Shortage acceptance uses the recorded physical count, releases the difference
  without rounding fractional reservations, preserves the picker, and rejects
  changed counts, duplicates, zero/missing counts, closed orders, quotations,
  non-inventory items, and lines already prepared. Agreement requires an
  unchecked-by-default consent box and a nonblank customer note.

The current project contains 101 test methods. All 101 passed on October 14, 2026.

Many test names describe the scenario in **Given / When / Then** form:

- **Given** describes the starting information.
- **When** describes the action.
- **Then** describes the expected result.

Example:

> Given two taxable items priced at 112.00 each, when their price is
> calculated, then the result is 200.00 VATable sales plus 24.00 VAT.

## Run the tests

From the repository root (the directory containing `Run-UnitTests.ps1`):

```powershell
.\Run-UnitTests.ps1
```

The runner requires MSBuild and VSTest at the Visual Studio 18 Community
paths configured in `Run-UnitTests.ps1` and builds the tests for x86.

A successful run displays each test as `Passed` and ends with a summary such
as:

```text
Passed: 103
Failed: 0
```

If a future change breaks a rule, the relevant test displays `Failed`, its
readable test name, and the expected and actual values.

## What these tests do not do

The unit tests do not exercise SQL functions, stored procedures, saving,
recall, inventory commitment, actual database locks, or queue processing.
`Database Components/Tests/OrderTools_LocalDB_SmokeTest.ps1` separately tests
history triggers, edit checks/locks, ready eligibility, and shortage save/rollback
in an isolated fixture. Its 99 checks include preserving stock on hand and other
orders' reservations while releasing only the accepted shortage difference.
It also checks the RMS smallint item type, fresh queue refresh/shortage writes,
clear-shortage behavior, and card stage changes. Build both solutions in Debug first.
Six tax-save checks reproduce the old rowversion conflict, execute corrected
production tax preparation, verify voucher behavior/rollback, clear manual
reasons when returning to taxable, and preserve quotation behavior.
The October 13 recall checks compare batch prices to the legacy selection rule,
test quoted codes, customer isolation, zero-price fallback, returns, missing
history, duplicate catalogue codes, transaction tie-breaking, and bounded query counts for 201 items. They
verify fresh price/warehouse changes, snapshot conflicts during recall, competing
save locks, and rejection of a second cashier's old version after the first commits.
These are overlapping database-session and conflict tests, not a production
capacity/load test with many workstations.
Ten October 14 checks compare production row tax application in legacy and
batch modes, preserving Boolean types, quoted/untrimmed codes, missing and blank
items, fresh tax changes, parameter bounds, exemption behavior, and existing
taxable/exempt totals. Fixture totals timings use an unshown form without a
credit-warning dialog; they do not measure a full live recall.
Five lookup-index checks add synthetic wide items and sale history, compare the
same production price query before/after the index, verify fewer logical reads
without timing thresholds, and check idempotence, equivalent-index reuse, and
safe handling of a conflicting name. All writes remain inside the disposable fixture.
See `ORDER_TOOLS_GUIDE.md` for that test and branch deployment checks. Full RMS workflows require
integration tests against a disposable RMS database restore—never a live
production database.

## Check the save refactor manually

For the October 14 batched tax lookup change, restart the rebuilt app and recall
an ordinary taxable order, an exempt order, and a quotation. Verify the row flags,
prices, quantities, VAT/subtotal/total, and existing credit-limit warning behavior.

For the October 13 recall change, restart the rebuilt app and recall a normal
multi-line Work Order. Verify its quantities, last purchase prices, prepared
counts, shortage highlighting, picker, totals, and tax setting. Recall a quotation
as well. The underlying business behavior should match before the change.

With two test sessions, recall the same disposable order in both; save in one
and confirm the other session's stale save is blocked. Make a warehouse shortage
change and use Reload order in the cashier session to verify the latest count
and picker appear. Avoid making production changes solely for a load test.

Use the updated Work Order Entry executable with disposable test orders:

1. Save a normal new order, then recall it, change a quantity, and save again.
   Confirm the order total, reservation, and queue quantity match the changes.
2. Choose **No** at the save confirmation. Confirm the order stays on screen and
   no database changes or success message appear.
3. Recall an order with a previously approved price below the minimum. Saving that
   same price should keep the existing approval behavior. Changing it to another
   below-minimum price or adding a new below-minimum line should require approval.
4. Accept a warehouse shortage and save. Confirm the reduced quantity, total,
   reservation difference, prepared state, and original picker are preserved.
5. Change the recalled order from another session, then try saving the older
   screen. The existing conflict warning must still block the save.
6. Check quotation saving/conversion and, where enabled, order splitting, grouping,
   and printing. These workflows should behave as before.
7. Select a regular taxable customer, add an item, use **Set Tax** to make the
   order exempt, and save. It should save without "1 of 2 updates failed".
   Recall it and confirm the exempt tax setting remains. Also test returning
   the order to taxable and saving a tax-exempt quotation.

The isolated SQL fixture does not execute every main-form save prompt or the full
RMS stored-procedure workflow. These manual checks cover that remaining scope.
See `ORDER_TOOLS_GUIDE.md` for the refactor checklist; no database migration is
required by this refactor.
## HO manual Order renumber recovery tests

Run `Database Components/Tests/HO_Renumber_LocalDB_Test.ps1` for the requested
18870 -> 18869 / 18879 -> 18870 standalone recovery. All 26 checks run on a unique
synthetic LocalDB instance and cover rollback, post-copy failures, unexpected
value changes, dependency guards, overlapping IDs, posted/history/journal links,
group membership, immutable receipt bytes, log domains, audit mapping events
and unchanged inventory/identity allocation. The test changes only the in-memory
fixture copy's server/backup checks; the distributed live script retains them.
LocalDB requires local registry/socket access. No store database is modified.
The same suite now includes ten checks for the separate saved reseed script,
bringing the total to 36: committed links, unchanged repeats, preserved data,
actual next allocation 18871 and rejection after later activity.
