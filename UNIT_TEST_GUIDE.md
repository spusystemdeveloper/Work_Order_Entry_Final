# Work Order Entry unit tests

## Current ReportViewer 2010 deployment

The October 21 main build uses installed ReportViewer 10.0.0.0, not private
version-15 DLLs. See
`Database Components/Tests/REPORTVIEWER_2010_MAIN_DEPLOYMENT_2026-10-21.md`.
The latest EXE includes withholding suggestions; both Release/x86 builds, Debug,
106 unit, 30 localhost withholding and 28 checkbox checks passed. The expanded
EXE-only packaging probe rendered both actual preview forms for six cases;
all 32 pages matched the accepted version-10 candidate exactly. Keep existing
Debug/Release store configs byte-for-byte around builds. Every target PC needs
the ReportViewer 2010 prerequisite installed once before EXE-only updates.

## Suggested customer withholding amounts

Work Order shows a read-only Suggested W/Tax panel beside its totals when the
selected customer has recognized enabled flags. It uses the existing displayed
VAT-exclusive Sub Total directly, including the order's existing exemption/tax
treatment. It does not divide every total by 1.12, deduct withholding from the
order, combine rates, save withholding amounts, modify customer properties or
fill RMS tender fields. These are separate estimates for cashier reference,
not a determination that a particular tax category legally applies.

Confirmed RMS customer mapping (CustomCaption.Style identifies each caption):

| Rate | Customer property | Required value | Caption style / caption |
| --- | --- | --- | --- |
| 1% | CustomNumber3 | 1 | 8 / W/Tax 1% |
| 2% | CustomNumber4 | 2 | 9 / W/Tax 2% |
| 5% | CustomNumber5 | 5 | 10 / W/Tax 5% |
| 15% | CustomText2 | text 15 (surrounding spaces ignored) | 2 / W/Tax 15% |

Missing, duplicate or unrelated captions do not enable estimates. Caption
comparison ignores letter case, whitespace and a trailing colon. Other flag
values are disabled. The customer SELECT is parameterized and runs only on
customer changes; subtotal edits reuse flags. Clearing/resetting or selecting
another customer reloads current flags. Optional lookup failures hide estimates
and are logged without interrupting existing customer/save validation. No SQL
runs during Designer construction. The panel is hidden for quotations.

Run `Database Components/Tests/WithholdingSuggestions_LocalDB_UI_Test.ps1`
after a Debug build. It creates/removes an owned localhost LocalDB fixture,
points only its process's in-memory contexts at that fixture, and does not
change registry or installed configuration. It never shows/loads the real
application form; a separate test host renders only its totals panel. Thirty
checks cover actual customer SELECTs, independent rates, exact flags/captions,
customer switching, cached flags, actual UpdateAmt results, tax-exempt totals,
signed/fractional/large values, tooltips, invalid/empty input, quotation hiding,
unchanged totals, tab stops and no overlap at the two supported screen widths.
Five unit tests cover rate detection, math, rounding and caption recognition.

October 21 validation: both Release/x86 solutions and Debug built; 106 unit,
30 localhost UI/query and 28 RecallFormatting checks passed. Debug EXE config
was preserved byte-for-byte. Final render evidence is in ignored
`TestResults/Withholding-8c09dbef/`. This is synthetic localhost validation, not
live-store saves, physical printing or execution of RMS POS. No live database
was accessed for this feature and the RMS investigation remains paused.
The compiled customer/caption SELECT also passed a read-only check against the
actual restored localhost DVO_STORE_DB schema (customer ID 11 returned 1%, 2%
and 5%); no app form or configuration was loaded and no data was written.

Manual acceptance: select a tagged customer, enter/edit/remove items, switch
customers, recall/import/restore a draft, toggle Work Order/Quotation and
refresh. Verify the applicable amounts against the displayed subtotal and check
that existing totals, save/password checks, printing and shortcuts behave as
before. Hover over an ellipsized large amount to read the complete value.

## Cebu one-off commitment recovery

`Database Components/Tests/Cebu_ZeroCommitments_Core_LocalDB_Test.ps1` tests
the exact UPDATE/audit body of the October 21 recovery in an owned disposable
LocalDB database. It does not execute the complete production recovery or relax
its backup/server guards. Twelve checks cover exact target quantities, stock,
prices, unrelated items, audits, repeat execution and failure rollback.
See `Database Components/Recovery/CEBU_ZERO_COMMITMENTS_2026-10-21.md` for live
authorization, scope, backup limits, regression results and execution status.
Never use an application save or a live rollback as a rehearsal for this repair.

## ReportViewer 2010 isolated candidate

Historical October 20 experiment; superseded for deployment by the October 21
main-project adoption recorded in
`Database Components/Tests/REPORTVIEWER_2010_MAIN_DEPLOYMENT_2026-10-21.md`.
Current builds require installed ReportViewer 2010 and deliver an EXE-only update.

The October 20 compatibility experiment is documented in
`Database Components/Tests/REPORTVIEWER_2010_COMPATIBILITY_RESULTS_2026-10-20.md`.
The isolated Release/x86 build passed 101 candidate unit tests, EXE-only
clean-folder and existing localhost UNC rendering checks using installed
ReportViewer 10.0.0.0. Both actual preview forms rendered six cases, including
real Work Order/Sales Quotation titles, empty, long-text, large-value and
multipage fixtures. All 32 TIFF pages matched count/dimensions; 27 were exact,
five had two differing Work Order title-edge pixels each. The strict pixel
assertion returned false and was not weakened. Other pixels and preview page
settings matched; operator acceptance remains required.
All 137 code/resource files matched; only isolated dependency references differ.
No real project reference/config, shared deployment or live database was changed.
Affected-unit and physical printer/copy acceptance remain outstanding. This is
not proof of the cause of the original report error. At that time the normal
application's ReportViewer 15 package remained unchanged. The October 21 adoption
changes the packaging contract to installed version-10 GAC loading.

## Shared-folder report failure: diagnostic follow-up

The October 20 failure after copying the complete Release folder remains
unresolved pending its actual inner exception. Read-only inspection found
matching ReportViewer DLL hashes on the supplied share. Synthetic Work Order
and Pick List reports passed from that shared application base, including the
actual deployed preview forms' embedded resources and page settings, on the
development PC. These results do not certify another user's PC or real WO data.

The diagnostic-only update logs synchronous preview failures and asynchronous
ReportError exceptions through ErrorLogger; existing popups show the root cause
when present. It leaves e.Handled, data queries, RDLC, printers, copy counts,
page setup and printing behavior unchanged. No remote-trust switch was added.
All four synthetic diagnostic checks passed (nested errors captured, default
handling intact), plus both Release/x86 builds, Debug, 101 unit tests and the
clean-folder packaging/render checks. Debug config preserved byte-for-byte.
Ignored evidence: `TestResults/SharedReportDiagnosis/` and
`TestResults/SharedReportValidation/`. No shared files or database were changed.

The following diagnostic procedure applies to the historical version-15 build,
not the current ReportViewer 2010 EXE. To obtain the missing evidence, deploy
that historical diagnostic EXE only in a
controlled maintenance/test window, retaining the branch's config and matching
five DLLs. Repeat the failing preview on the affected PC; no new order/save is
needed. Record the WO number, which PCs fail, and the popup's Details text.
The full chain is appended to `Logs/Error_yyyy-MM-dd.log` beside the launched
EXE if that user can write there. If logging is denied, supply the Details text;
do not change shared permissions or runtime trust settings as an unverified fix.
The diagnostic build has not been copied to the share by the assistant, and
the actual client's inner exception has not yet been captured. RMS remains paused.

## Neutral column-header selection colors

All 17 addin grids use their normal header colors even when a column contains
the current cell; selected rows remain highlighted. Designer header styles and
the main-grid runtime formatter both set matching normal/selection colors.
Sorting, keyboard selection, read-only flags and checkbox NullValue defaults
are unchanged. Check Price Level, main lookup/selected items, customer, sales
rep and recall grids after selecting rows/clicking sortable headers.

The October 20 comparison passed 40 checks across 19 Designer/runtime states,
including unchanged row colors and column/interaction settings. Price Level
before/after renders are in ignored `TestResults/HeaderHighlightValidation/`.
No form Load or SQL was invoked. Both Release/x86 solutions, Debug build, all
101 unit tests and 28 RecallFormatting_UI_Test.ps1 checks passed; Debug config
was preserved byte-for-byte. Manual application acceptance remains outstanding.

## Queueing column visibility

UP-STORE and PREPARED display only for work orders when the saved branch
Queueing Mode is enabled. Save the mode in Settings and close the settings
dialog to apply it. Both columns stay hidden for quotations. Verify off/on,
refresh, recall and quote-to-WO conversion; hiding must not clear existing cell
values or change save/queue behavior. No Designer checkbox defaults changed.

The October 20 in-memory probe passed 85 checks over repeated off/on settings,
formatting, Settings UI refresh, quotation and conversion, with true UP-STORE
and prepared quantity 3 retained. It did not invoke form Load or save settings;
the EXE configuration hash was unchanged. Both Release/x86 solutions, Debug
build, all 101 unit tests and 28 RecallFormatting_UI_Test.ps1 checks passed.
Ignored evidence: `TestResults/QueueColumnValidation/`. Manual acceptance of
the actual saved Settings flow remains; no store database was accessed.

## New-order preallocation validation

After a Debug build, run
`Database Components/Tests/PreallocationValidation_LocalDB_Test.ps1`.
It creates/deletes only a uniquely named owned LocalDB fixture at compatibility
130 and does not modify application configuration, registry or store databases.
No form Load event is invoked. Legacy global contexts are checked closed,
disposed and cleared before application methods are tested.

The 80 checks compare 18 paired allocation-boundary cases with and without the
compiled ValidateNewOrderRows helper. Mapped fixture header/detail procedures
and the compiled atomic commitment method execute under a real SQL transaction.
Cases cover malformed later/split rows, Decimal overflow, missing items,
prepared/tax/pick conversions, fractional values, duplicate running differences,
zero differences, Type 7 exemptions, tax exemptions, quotations and a committed
change after prevalidation. Eleven predictable failures consume 13 IDs without
preflight and none with it; the concurrent failure still consumes one and is
rolled back. Successful stored values match. Evidence:
`TestResults/Preallocation-a42d5227/Results.json` and
`TestResults/PreallocationValidation/` (ignored).

This boundary comparison bypasses the surrounding prompts and queue/history
orchestration; it is not a full UI/store/RMS save test. Actual caller placement
and retention of the final atomic update are also checked against source.
Both required Release/x86 builds, Debug build and all 101 unit tests passed
with byte-for-byte Debug configuration preservation. Required multi-user tests
passed 39 checks normally and 39 with `-VersionedReads`; their settlement is a
simulation. No live operations occurred. Manual localhost acceptance remains:
normal/imported work order, quotation, price approval/Cancel and a split order
over six rows. Existing prices, quantities, totals and resulting orders should
match; invalid later rows must not leave partially saved orders. Do not insert
test orders on live just to inspect numbering. SQL errors and concurrent changes
after prevalidation may still consume IDs. RMS investigation remains paused.

For the separately requested HO WO 18912 -> 18905 recovery, run
`Database Components/Tests/HO_Renumber18912_LocalDB_Test.ps1`. Its 43 checks run
only in a uniquely owned synthetic LocalDB instance at compatibility 100, using
the existing synthetic HO schema/triggers. They cover absence of the backup
check as explicitly requested, rollback/injected errors, source/reference/trigger/schema
guards, preservation of values and stock, mapped references, commit and actual
legacy @@IDENTITY allocation of 18913. The script does not reseed identity.
The test substitutes only the fixture server in an in-memory
copy; it does not weaken the distributed file's guards. No live/store execution
is part of this test. See
`Database Components/Recovery/RENUMBER_HO_18912_TO_18905.md` for personal SSMS
instructions and the remaining fresh-restore/operator validation.

The [paused RMS settlement investigation](Database%20Components/Tests/RMS_SETTLEMENT_TRIGGER_REVIEW_2026-10-18.md)
consolidates the October 18 source audit, disposable trigger tests and actual
localhost RMS results, including the confirmed original-trigger deadlock and
partial posting. The candidate has not been tested through actual RMS. Those
experiments are historical evidence, not part of the routine unit-test runner;
do not rerun fault injection against a store or treat synthetic passes as POS
certification. Investigation is paused at the user's request.

The authorized October 17 localhost DVO_STORE_DB index test is recorded in
`Database Components/Tests/DVO_RESTORED_ITEMLOG_INDEX_RESULTS.md`. The guarded
index was committed only on that local restore; seven lookup pairs retained
their values while median component times and logical reads improved. No live
deployment or full application save was tested. The local restore is from an
older June backup; rehearse current Davao data before live deployment.

For the separately requested WO 18882 -> 18879 and next ID 18880 recovery, run
`Database Components/Tests/HO_Renumber18882_LocalDB_Test.ps1`. Its 42 checks use
only a GUID-owned synthetic LocalDB instance at compatibility 100, including
default rollback, recovery failure after reseed, reference/trigger/source drift
guards, preserved values and actual automatic allocation/legacy @@IDENTITY.
Never run the production recovery script against a store as an automated test.
Manual restore/live instructions are in
`Database Components/Recovery/RENUMBER_HO_18882_TO_18879.md`.

The prepared `Database Components/Recovery/Deploy_HO_PO_DB_ItemLogLookup_LIVE.sql`
was checked offline with the SQL Server 2016 ScriptDom parser (zero syntax
errors, one batch). Its embedded migration matches the separately tested source;
one transaction wrapper and absence of GO separators were verified. This is
static validation, not execution of the live deployment wrapper. Never run that
target-specific file as an automated test against a store.

These tests check small business rules without opening the application,
connecting to DVO/GSC, or changing database records.

## What is tested

### InterStore availability layout

After a Debug build, run:

```powershell
& '.\Database Components\Tests\InterStoreLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

The InterStore probe binds synthetic tables without calling the availability
query. It explicitly detaches the production Load handler before transparent
rendering, so no database access occurs. It checks the five original column
ordinals/property names, ID visibility/hidden Address, read-only/null/fractional
quantity behavior, sorting, reordering, selection, row-resize permissions,
keyboard/tab/DialogResult settings, normal and simulated 125% bounds at 1024/1366
widths, and repeat/empty binding. PNGs are saved to the reported ignored
`TestResults/InterStoreLayout-*` folder.

October 19 validation: 140 InterStore checks, 28 recall-formatting checks, both
Release/x86 builds, Debug build and 101 unit tests passed. Debug store config
was preserved byte-for-byte; synthetic renders were inspected. No actual
availability query or live-store test was executed.

Manual acceptance: select a known item and click InterStore. Compare returned
stores and quantities with the previous version, including zero/negative/decimal
or blank values where applicable. Check long names/codes, row selection, header
sorting, column reordering and Close. Existing shortcuts must behave as before.
All layout/mapping properties are serialized in frmCheckStore.Designer.vb.
RMS investigation remains paused.

### Item-grid formatting comparison

Preserve a pre-change Debug EXE before building the candidate, then run:

```powershell
& '.\Database Components\Tests\GridFormatting_UI_Compare.ps1' -BaselineAssembly '.\TestResults\GridFormattingBaseline\Work Order Entry_V8.exe'
& '.\Database Components\Tests\ItemGridReadability_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
& '.\Database Components\Tests\SmallScreenLayout_UI_Test.ps1'
```

The comparison compiles an isolated x86 STA probe and runs original/candidate
assemblies in separate processes, without form Load, searches, editing or SQL.
Its 16 synthetic cases combine 0/1/100/500 bound rows, 1024/1366 widths and
normal/simulated 125% scaling. The actual GridColumnWidth method runs for seven
trials; the first trial is excluded. Measure binding plus formatting separately
from ten repeated formatting calls per trial. This is not an end-to-end search
or real monitor/DPI benchmark.

All 208 paired assertions must match raw DataTable hashes, row counts, column
settings, selection, font/header/row sizes, read-only/tooltips/selection mode and
rendered bitmap pixels. Programmatically altered row heights must still become
44 without moving selection. Shared-row counts are recorded before indexing
selection cells or rendering, which can materialize rows. No assertion is weakened
to accommodate a visual difference. Timings are evidence, not a fixed speed threshold.

October 18 results in `TestResults/GridFormatting-05f62b36` (ignored): all assertions
passed. At 500 rows, shared counts were 0 -> 500. Binding/formatting medians were
21.92 -> 8.76 ms (1024) and 41.87 -> 12.21 ms (1366); repeated formatting was
8.34 -> 7.91 ms and 19.89 -> 15.41 ms. Both Release/x86 builds, Debug build,
101 unit tests, 50 readability, 28 recall-formatting and 875 small-screen checks
passed, with Debug store configuration preserved byte-for-byte.

Manual acceptance: repeat item searches, scroll results, select rows, resize
and use existing shortcuts. Appearance, result ordering, selection and row height
must remain unchanged. Actual database search speed was not measured. RMS stays paused.

### Price-level query reduction

Preserve an original Debug EXE in an ignored test-results directory before
building the candidate. The comparison requires two separate processes so the
original and updated assemblies do not share type identities. After a Debug build:

```powershell
& '.\Database Components\Tests\PriceLevel_Queries_LocalDB_Test.ps1' -BaselineAssembly '.\TestResults\PriceLevelBaseline\Work Order Entry_V8.exe'
& '.\Database Components\Tests\PriceLevelLayout_UI_Test.ps1'
```

The SQL runner creates only a GUID-owned `WO_Price_*` LocalDB instance/database,
small synthetic Item/config tables and the exported price function's SELECT.
It does not restore or connect to a store. Workers verify legacy contexts are
closed, dispose them and redirect only their process-local endpoint fields to
the owned fixture before any application query. No registry or app config is
switched. Original settings and price data are read through the compiled loader.
Fixture setup uses a separate connection, excluded from the command trace.

The 125 paired cases cover five price masks, levels 0 through 4, fractional/zero/
negative prices, missing item data, changed prices on successive loads, and the
single-level API with display enabled/suppressed. Compare raw rows, minimum
price, selected row/column and approval flags. Empty/unsupported-level behavior
is preserved rather than inventing a new level. Intact Extended Events traces
count actual application SQL batches/RPCs. This is not a password-entry test;
the approval/password handlers are unchanged and no selected price is applied.

October 18 results: 125 paired cases matched; commands 825 -> 250 overall;
four-level dialog reads 12 -> 2; dialog median 92.89 -> 16.19 ms. The engine was
LocalDB 17.0.4025.3 at compatibility 130, not a SQL Server 2016 engine or a live
store. Timing includes local calls with pooling disabled and is not a network or
multi-user benchmark. Artifacts: `TestResults/PriceLevelQueries-9f76b564` (ignored).
The owned database was dropped and instance deleted. Both Release/x86 builds,
Debug build, 101 unit tests and 50 existing Price Level UI checks passed; Debug
store configuration was preserved byte-for-byte.

Manual acceptance: open Price Level Selection for several customer levels and
confirm identical rows/prices and retail top-row selection. Exercise the existing
F9 password approval, wrong password and Cancel paths, then reopen/reload after
an authorized test price/settings change to confirm fresh data. Test normal
price editing and the alternate price-level dialog too. No store write is
authorized by running this fixture; RMS investigation remains paused.

### Messages inside empty lookup grids

After a Debug build, run:

```powershell
& '.\Database Components\Tests\GridEmptyState_UI_Test.ps1'
```

This isolated paint/binding probe constructs Customer, Sales Rep, Recall and the
main form without showing/loading them. It checks that the actual Paint handler
draws nothing before binding, draws guidance for empty/filtered-empty results,
and stops drawing when rows return. Synthetic PNGs are saved under the reported
`TestResults/GridEmptyState-*` directory; those artifacts are not tracked.

The user explicitly approved leaving unsuccessful keyword searches empty instead
of automatically restoring previous/all items. The above-grid notice is removed.
The probe invokes the production empty-search handler on synthetic empty data
with/without preceding keywords: the empty binding/message must remain, only the
failed keyword is removed, and the grid must have no above-grid notice. Barcode
empty feedback and successful rebinding are also checked. No SQL, live searches
or real item selection executes; import-specific fallback is unchanged.

Final validation passed both Release/x86 builds, Debug build, 101 unit tests,
37 feedback checks, 28 recall-formatting and 875 small-screen checks, with Debug
config preserved byte-for-byte. Synthetic grid renders were inspected.

Manual acceptance: search for an unmatched value. Customer, Sales Rep, Recall
and item grids should display their guidance below the headers without a popup.
Keyword item search must stay empty, with no restored 500-item list and no notice
above the grid. Search again to restore matching rows. Verify F4/F5, Esc, barcode
reset/focus, successful selection and genuine errors still behave as expected.
No live-store test is claimed; RMS investigation remains paused.

### Empty Customer, Sales Rep and Recall results

After a Debug build, run:

```powershell
& '.\Database Components\Tests\EmptyLookup_UI_Test.ps1'
& '.\Database Components\Tests\LookupDesign_UI_Test.ps1'
& '.\Database Components\Tests\LookupKeyboard_UI_Test.ps1'
```

The empty-results runner compiles a separate x86 STA probe and copies only the
application assembly, without its store config. Forms are never shown or loaded.
Legacy globals construct data contexts on first access; the probe verifies their
connections remain closed, then removes/disposes the contexts before callbacks.
It invokes OK, Enter and double-click with initially empty and synthetically
filtered-empty grids, requires no error prompt/selection/dialog close, restores
the synthetic results and checks Esc. No SQL or actual row selection is executed.
Windows may require execution outside the sandbox because legacy SqlClient
initialization uses writable performance counters; do not change database or
registry settings to bypass that restriction.

The pre-fix assembly reproduced three exceptions using `-ExpectLegacyFailure`:
Customer/Recall NullReferenceException and Sales Rep ArgumentOutOfRangeException.
Only use that switch with the original assembly. Fixed validation passed 22
empty-result checks, 204 lookup checks, four Escape checks, both Release/x86
builds, Debug build and all 101 unit tests; Debug config was preserved byte-for-byte.

Manual acceptance: search for an unmatched value in Customer, Sales Rep and
Recall; try Enter, OK and a double-click in the blank grid. No error or selection
should occur. Clear/change the search to restore results, verify normal selection
still works, then reopen and test Esc. Recall filters and left/right shortcuts
must retain their existing behavior. RMS investigation remains paused.

### Item-grid numeric readability

After a Debug build, run:

```powershell
& '.\Database Components\Tests\ItemGridReadability_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
& '.\Database Components\Tests\SmallScreenLayout_UI_Test.ps1'
```

The display probe constructs the compiled main form without showing/loading it
and sets the existing recall-loading guard before adding synthetic rows. It
checks Designer and runtime styles: zero, fractional, negative and large
quantities; unchanged raw price/total values and currency format; displayed-cell
numeric sizing; edit permissions; native long-name tooltips; and retained price
warning styles/tooltips. It does not edit through a control, run validation,
execute SQL or save an order. The before-change probe confirmed 1.25 displaying
as 1 and price 1,234,567.89 requiring 109 pixels versus the fixed 100-pixel column
in the local test culture. Currency symbols/separators remain culture-dependent.

October 18: both Release/x86 builds, Debug build, 101 unit tests, 50 display,
28 recall-formatting and 875 small-screen checks passed. Debug store configuration
was preserved byte-for-byte. For manual acceptance at 1024x768 and 1366x768,
inspect existing fractional/negative quantities and large prices/totals, hover
over a clipped description, and verify the active quantity/price cell remains
clear. Wide numeric values may require horizontal scrolling; column sizing reads
displayed rows only. Existing validation and F-key shortcuts must behave as before.
The RMS investigation remains paused.

### Customer and Sales Rep lookup Escape routing

After a Debug build, run:

```powershell
& '.\Database Components\Tests\LookupKeyboard_UI_Test.ps1'
& '.\Database Components\Tests\LookupDesign_UI_Test.ps1'
```

The keyboard runner compiles a separate x86 STA process with only the application
assembly and no store configuration. Derived forms suppress the production Load
handlers and show transparently without a taskbar button, then send Esc through
WinForms preprocessing from the focused OK/Cancel buttons. All four cases must
route to the existing Cancel action and return
Cancel, while AcceptButton remains unset. The original assembly reproduced all
four unhandled cases with `-ExpectLegacyFailure`; use that switch only with a
pre-fix assembly. No database, pricing, tax or selected-customer action runs.

Manual acceptance: open Customers (F8) and Sales Reps (F7), search, use Down and
arrow keys, then select with Enter. Reopen each and Tab onto OK and Cancel;
Esc must close without applying a selection. Confirm mouse OK/Cancel still work
and focus returns to the invoking main window. Existing tab order is unchanged.
The RMS investigation stays paused; these tests do not exercise RMS.

October 18 validation: both Release/x86 builds, Debug build, 101 unit tests,
four keyboard cases, 204 lookup checks and 28 recall-formatting checks passed.
The initial sandboxed VSTest attempt aborted before execution because its x86
test host did not connect; the unrestricted local retry passed. Debug store
configuration was preserved byte-for-byte around builds and both test attempts.
Full manual add-in focus/selection acceptance remains pending.

### Login cancellation with custom shutdown warnings

After a Debug build, run:

```powershell
& '.\Database Components\Tests\LoginCancel_UI_Test.ps1'
```

The runner compiles a separate x86 STA probe using the actual frmLogin Cancel and
completion callbacks and AppPrompt. A derived login suppresses its database Load
handler, and a synthetic main form supplies a custom closing warning. No real
authentication, draft write, registry/configuration change or application main-form
load occurs. Cancel/window-close must return Cancel; completion must return OK;
each host then closes normally with one custom warning and no collection error.
The pre-fix callback reproduced the exact OpenForms enumeration exception in
TestResults/LoginCancel-4ecaf03c; the fixed three cases passed in
TestResults/LoginCancel-38271667. Those are simulated UI tests, not store tests.

For manual acceptance, restart the rebuilt application and cancel initial login;
it must close without the collection exception or proceeding to item loading.
Repeat after Logout, cancelling the login dialog. Then verify successful initial
login and logout/re-login still authenticate/load normally. Close with unsaved
items and verify existing draft saving/warning behavior; no draft rule was changed.
The existing VB AfterMainFormCloses setting remains responsible for final shutdown.

### GSC timestamp ambiguity review

See `Database Components/Tests/GSC_ITEMLOG_AMBIGUITY_REVIEW_2026-10-17.md` for the
completed authorized read-only census on the existing localhost GSC restore.
`Review_ItemLog_Ambiguity.sql` performs full source SELECT scans and uses a
connection-scoped tempdb table; its ranking/type aggregates use MAXDOP 1 and a
one-percent query memory grant cap. It does not create a source index, update
history, call a correction procedure or change server settings. Run only on an
explicitly authorized target and account for scan/sort cost. Its counts are an
observed read-committed state, not a frozen backup under concurrent activity.

The observed 84 conflicting pairs require the existing 53305 guard to remain.
No active Order number matched, but transfer/order reference collisions and lost
times prevent inferring a correct historical value. This audit is not an index
before/after test or authorization to repair history or modify selection rules.

### Packaged branch-neutral RMS voucher lookup migration

`Improve_RMS_VoucherLookup.sql` is an optional, initially unchecked Database
Updater selection for DVO/GSC/Portable. It adds/reuses the tested non-unique Item
index without branch IDs, trigger/query changes or business-row DML. The original
local-only candidate fixture retains its guards; the separately reviewed migration
uses the updater's existing target review and caller transaction. Deployment
instructions are in `Database Components/Migrations/RMS_VOUCHER_LOOKUP_GUIDE.md`.

```powershell
& '.\Database Components\Tests\VoucherLookup_LocalDB_Test.ps1' -CompatibilityLevel 100
& '.\Database Components\Tests\VoucherLookup_LocalDB_Test.ps1' -CompatibilityLevel 130
# Build the Debug updater first:
& '.\Database Components\Tests\DatabaseUpdater_LocalDB_Test.ps1'
```

The voucher fixtures use GUID-owned synthetic LocalDB instances only. They check
matching/nonmatching/null/inactive voucher results, unchanged Item rows including
rowversions/ntext/fractional quantities, trigger preservation and normal atomic
updates, caller rollback/commit, repeat/reused indexes, schema/name-conflict guards
and reduction of logical reads. The production updater integration tests also
check all three catalogs, packaged SQL hashes/guide, initial unchecked visibility,
deployment history and rollback of index/history when a later script fails.
These tests do not connect to GSC/DVO stores or certify actual RMS tendering or
multiuser capacity. Rehearse each current branch restore before deployment.

October 17 packaging validation: all **83 updater integration checks** and
**25 voucher checks at each compatibility level 100/130** passed. Synthetic
unchanged lookup Item reads were 756 before and 2 after. Both Release/x86
solutions, 101 unit tests and clean-folder packaging checks also passed. Ignored
logs are in TestResults/Voucher-Updater-Integration.log, VoucherMigration-100.log,
VoucherMigration-130.log and Voucher-CleanFolder.log. The built updater package
contains the same migration hash and the deployment guide. No live SQL ran.

### Local Davao RMS lookup / actual save-transaction comparison

See `Database Components/Tests/DVO_LOCAL_DEADLOCK_INDEX_RESULTS.md` for the
October 17 authorized localhost reproduction, query plans, before/after timings,
correctness checks and cleanup. The workload used a GUID-owned COPY_ONLY restore
of DVO_STORE_DB, not automated writes against the existing restore. Six actual
compiled recalled-order save transactions were paired with four RMS SQL lookup
replay workers; RMS POS/QSRules and final tendering were not executed. One short
comparison is not the planned actual-client 10/12/15-user capacity certification.

`Database Components/Tests/Fixtures/RMSVoucherLookup_Index_Candidate.sql` preserves
the exact tested index definition and intentionally permits only local GUID-owned
test clones with a caller-supplied transaction. Do not remove its test guards or
add it to an updater deployment without a separately reviewed deployment change.
The candidate was removed with its disposable database; the original restore's
previously installed item-log index was left intact. No live target was contacted.

### Live read-only performance collection and 10+ client test plan

`Database Components/Tests/Collect_ReadOnly_Performance.ps1` is an explicitly
invoked diagnostic collector, not a workload test or deployment script. Usage:

```powershell
& '.\Database Components\Tests\Collect_ReadOnly_Performance.ps1' `
    -Server '<authorized server>' -Database '<authorized database>' -SqlUser '<login>'
```

SQL authentication prompts for a SecureString; omit SqlUser for Windows login.
Never supply a password in source, command arguments, reports or app configuration.
The collector performs bounded SELECTs of metadata, waits, cached statements,
sessions, requests, I/O and existing system_health deadlock data. Default sampling
is six snapshots five seconds apart. Timeouts and deadlock priority apply only to
its own connection; ApplicationIntent does not replace proper account permissions.
Server-level VIEW SERVER STATE/definition visibility are needed for full coverage.
Unavailable sections are recorded, not reported as zero activity. Server-wide
wait/session/deadlock evidence must be distinguished from database-specific data.

SQL text literals/comments are redacted before saving under ignored TestResults/
ReadOnly-Performance-*. Keep raw host/session operational evidence private. The
collector creates no schema, traces or server settings and does not issue saves,
tenders, scans of full business history, DBCC cache commands or KILL.

The authorized October 17 live Davao findings are documented in
`Database Components/Tests/DVO_LIVE_READONLY_PERFORMANCE_2026-10-17.md`.
`Database Components/Tests/DVO_MULTIUSER_RMS_TEST_PLAN.md` is a prepared plan for
isolated 10/12/15-client runs, including actual RMS voucher lookup and settlement.
Those load tests have NOT run. Existing MultiUser fixtures simulate settlement
and do not prove actual RMS or 10+ user capacity. Do not run that workload on live
or deploy a candidate index based on this read-only authorization alone.

### Payment Type before Work Order price approval

After building Debug, run:

```powershell
& '.\Database Components\Tests\PaymentBeforeApproval_UI_Test.ps1'
```

The 121 isolated x86 checks invoke the actual ValidateAndSave method with synthetic
rows, without showing/loading the main form. The fixture temporarily sets all
legacy SQL contexts to Nothing inside its own process and restores them afterward;
it changes no registry/app configuration or store connection. Twelve cases cover
new/recalled Work Orders with empty/whitespace payment and numeric or unconfirmed
prices, CASH/CHARGE continuation to price confirmation, and new/recalled quotations
with blank payment retaining their existing price-confirmation path. Checks verify
the first prompt, early False result, unchanged line values/approval flags, no
password window and no new legacy context. This is validation-order coverage, not
real approval authentication or a save. Existing price-approval unit tests and both
MultiUser_Save_LocalDB_Test.ps1 modes must also pass for save-path changes.

For manual localhost acceptance, use a Work Order with a price that needs approval
and leave Payment Type blank. Save: the payment warning must appear first, leaving
the payment field focused and lines unchanged. Select CASH/CHARGE and save again:
the existing price/password requirement must still apply. Cancel password entry
and verify nothing is saved; then use valid approval to complete the existing
flow. Repeat for a recalled Work Order. A quotation with blank payment must keep
its existing price/approval behavior without a new payment requirement.

### Compact lookup and prompt layouts

The October 17 compact layouts are serialized in the Designers for Customer,
Sales Rep, Recall, shared prompts, action confirmations, import completion,
saved-number confirmations and Recall Selection. Customer/Sales Rep client size
is 660x390 and Recall 860x460. Other new message dialogs use roughly 440-500 pixels
of client width, with short prompts shrinking vertically. Lookup fonts remain
unchanged, headings/body/buttons remain readable, saved numbers stay large, and
long details retain scrolling or wrapping.

Validation includes both Release/x86 builds, 101 unit tests, the shared-prompt,
ownership, action-confirmation, Recall Selection, RecallFormatting and
SmallScreenLayout UI scripts below, plus Packaging_CleanFolder_Test.ps1. The
action-confirmation font assertion now expects the requested 14-point compact
heading; all behavior assertions remain intact. An additional 185 geometry checks
and synthetic renders covered the eight layouts at 1024/1366x768 and normal/
simulated 125% scaling. Customer/Sales Rep Load delegates were removed only on
those isolated render instances before showing sample rows offscreen; production
data-loading handlers are unchanged. Renders are under TestResults/CompactDialogs-*.

For manual localhost acceptance on the actual 1366x768 unit, check F8 Customer,
F7 Sales Rep, F11 Recall/quotation selection, save/remove confirmations, import
completion and saved Work Order/Quotation numbers. Confirm dates/references,
customer names/comments, focus and OK/Cancel/Enter/Escape remain usable. Repeat
the CHARGE credit-warning/Price Level case and window switching. For long messages
or long names/comments confirm scrolling/wrapping keeps details accessible. No
live-store workflow is implied by the isolated checks.

### Quotation saved-number presentation

Saved and updated quotations now reuse frmOrderSaved with its quotation label.
The large number, read-only field and Close button use the same Designer layout
as Work Orders. Existing Work Order callers retain the default Work Order mode.
The change replaces only two post-commit notifications; it performs no save or
print operation itself.

For localhost acceptance, save a new quotation and confirm the large number
matches the saved quotation. Close with the button or Enter/Escape on separate
saves and confirm the existing quotation print window opens afterward. Update
an existing quotation and confirm its number and updated heading, with no new
print window. Check existing single/split Work Order confirmations remain the
same. Isolated synthetic UI checks cover both quotation states, both single Work
Order states and split Work Orders at 100% and simulated 125%, including number,
labels, font, focus, bounds and Enter/Escape results (105 checks). These are not
actual quotation saves or printer tests.

### Shared application prompt presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\AppPrompt_UI_Test.ps1'
& '.\Database Components\Tests\PromptOwnership_UI_Test.ps1'
```

The 1,159 isolated x86 checks instantiate only frmAppPrompt and synthetic owner
forms. They verify all six standard button sets, five icon categories, valid
default choices, exact message/caption text, return values, Enter, Escape and
window-close handling. Explicit ownership and background-thread marshalling are
checked through AppPrompt itself. Long text stays complete and scrollable, with
bounds and fonts checked at 100% and simulated 125%; renders are written under
TestResults/AppPrompt-*. The base layout is editable in frmAppPrompt.Designer.vb.
These checks do not open the real application forms or use a database.

PromptOwnership_UI_Test.ps1 adds 577 native window checks across 76 scenarios.
It exercises all six shared button sets with all five icon categories under
ordinary/TopMost synthetic modal owners, then the dedicated Save changes/Remove
item, Import complete, Work Order/Quotation saved and all three Recall Selection
modes with ordinary/TopMost owners. GetWindow verifies that each prompt is above
its owner in actual Windows z-order; IsWindowEnabled verifies the prompt accepts
input while its owner is blocked, and that the owner recovers after dismissal.
Explicit results are checked. This fixture does not run saves, imports, password
checks, SQL or full application workflows. No application changes were required
by the audit; manual acceptance on the actual workstation remains necessary.

The nested-owner regression uses a synthetic Price Level-style modal with TopMost
both enabled and disabled. Show, MsgBox and explicit-owner calls must keep the
warning owned by that modal and at the same stacking level. Native Windows
IsWindowEnabled checks verify the price/main windows stay disabled while the
warning is open and recover after acknowledgement and closing price selection.
Cases also set the parent's DialogResult to OK before raising the warning, matching
the existing insertItem -> UpdateAmt -> credit-warning timing. No real price-form
Load/insert, pricing checks or credit-limit logic is invoked in this fixture.

For localhost acceptance of this fix, select CHARGE with a customer/entry that
exceeds available credit, choose a price level, acknowledge the warning with OK,
then confirm price selection and the main form can close normally. Repeat using
keyboard acknowledgement and normal window switching. The same credit warning
must still occur; this fix only changes its window ownership/stacking.

AppPrompt now presents 321 standard prompt calls in 31 Work Order Entry files
(two quotation success calls now use the large saved-number dialog).
Keep caller arguments and result handling unchanged when adding a prompt. Existing
save-number/import-success/action-confirmation/selection forms remain in use.
Uncommon native MsgBox flags retain their native fallback; failure to initialize
the custom UI also falls back to native reporting. The separate Database Updater
and Queue application prompts are outside this change.

For manual acceptance on localhost, try information, invalid-input/password and
stock warnings, split-order details and Yes/No/Cancel confirmations as available.
Check message completeness, keyboard choices and parent-window focus at the
workstation's actual display scaling. Do not intentionally trigger production
database failures. Automated validation also passed both Release/x86 builds,
101 unit tests, the existing recall/action dialog checks, clean-folder import/PDF
packaging and 39 multi-user checks per isolation mode in disposable fixtures.
RMS settlement in those fixtures is simulated; these are not live-store tests.

### Recall Selection presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\RecallSelection_UI_Test.ps1'
```

The 106 isolated x86 checks instantiate only frmRecallSelection. They verify the
exact choices/order/default across Recall, SaveToQuote and SaveToWorkOrder, the
default constructor, existing keyboard/tab routing, OK, double-click, Cancel,
Escape and the existing Enter-focus handler. All event code remains unchanged.
They also check text/control bounds at 100% and simulated 125%, smaller-screen
fit and return values, with offscreen renders under TestResults/RecallSelection-*.
These checks never open the actual Recall form, convert/save orders or use SQL.

For manual localhost acceptance, open F11 and try quote/work-order choices via
OK and double-click, then Cancel/Escape. For a recalled quotation, also verify
the Save Changes / Convert to a work order selection appears with the same
choices/default and existing behavior. Confirm readability at actual display
scaling. The layout is editable in frmRecallSelection.Designer.vb.

### Save changes / remove item confirmation

After building Debug, run:

```powershell
& '.\Database Components\Tests\ActionConfirmation_UI_Test.ps1'
```

The 53 isolated x86 checks instantiate only frmActionConfirmation and never call
the main form's Save/Remove code or connect to a database. They verify exact
question text, default Yes, Yes/No return values, focused/default Enter, arrow
navigation, Y/N mnemonics, ignored Escape/user close and readable text/control
bounds at 100% and simulated 125%. Offscreen renders are written under ignored
TestResults/ActionConfirmation-*. The form layout is visible in the Designer.

Only the two pictured prompts use this dialog. New-order/split previews, quotation
save questions and other warnings now use the shared AppPrompt presentation;
their text, choices and result handling are unchanged. For manual localhost acceptance,
edit a recalled order and try both Yes and No on Save changes; separately try
both answers on removing an item. Confirm No preserves the entry and Yes follows
the existing save/removal flow, including existing totals and validation behavior.
Check actual workstation display scaling. Default-locking and VersionedReads
MultiUser_Save_LocalDB_Test.ps1 checks also passed (39 each) in disposable fixtures;
their RMS settlement participant is simulated, not real POS execution.

### Import completion message

The three existing success prompts in ImportQuote, ImportPO and ImportFromWebsite
now open frmImportComplete at the same locations, with their existing conditions
unchanged. The form is editable in the WinForms Designer. Its larger heading,
availability reminder and OK button are presentation only; it performs no import,
stock refresh or save itself.

Validation for this change: both Release/x86 builds and all 101 unit tests passed,
as did Packaging_CleanFolder_Test.ps1. Twenty-four isolated x86 dialog checks
verified readable text/bounds at 100% and simulated 125%, fit within 1024x768,
initial OK focus and actual Enter/Escape dismissal. The two offscreen renders
were visually reviewed. These checks did not connect to a store or run imports.

For manual acceptance on localhost, import a supported file and confirm the
larger success heading and complete availability reminder appear. Acknowledge
with OK, Enter and Escape on separate imports. Confirm the same imported lines,
quantities, prices and totals remain afterward, and existing invalid-item/error
prompts still occur normally. Check the actual workstation's display scaling.

### Price Level Selection presentation

After building Debug, run:

```powershell
& '.\Database Components\Tests\PriceLevelLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

The 50 isolated Price Level checks instantiate frmPriceLevel without showing or
loading it. They exercise the existing DisplayPrice and SelectTopPriceLevel
methods with synthetic values, preserving labels/order, raw fractional prices,
numeric/currency formats and first-row selection. They also verify fixed column
indices, read-only behavior, existing tab/DialogResult/keyboard configuration,
empty selection and fit within 1024x768 at normal/simulated 125% geometry.
No SQL, OK/Enter price application, F9/password dialog or insertion runs.

Manual check on the test installation:

1. Open frmPriceLevel in the Designer. Verify larger item text, price-level
   names, bold amounts, row spacing, selected row and OK/Cancel buttons.
2. Use an existing test item/customer. Confirm the same levels and prices load,
   with the same first-row selection, order and currency display as before.
3. Check arrow navigation, Enter, OK, Cancel, Escape and the existing F9 password
   route. Validate required approvals with the existing authorized test process;
   the presentation test does not exercise password/approval behavior.
4. Verify both inserting an item and changing an existing line's price retain
   their original behavior, quantities, totals and approval requirements.
5. Check long item names and the actual workstation resolution/scaling. The
   item description retains its existing read-only multiline/scroll behavior.

All pricing/approval code-behind remains unchanged; only the Designer differs.

### Customer, Sales Rep and right-side button design

After building Debug, run:

```powershell
& '.\Database Components\Tests\LookupDesign_UI_Test.ps1'
& '.\Database Components\Tests\SmallScreenLayout_UI_Test.ps1'
& '.\Database Components\Tests\RecallFormatting_UI_Test.ps1'
```

The 204 lookup-design checks use synthetic typed DataTables without showing or
loading the forms. They check repeated/empty binding, all customer property
ordinals and hidden fields, Boolean/DBNull formatting, leading-zero accounts,
unchanged price/employee/tax values, long names, selected IDs, sorting, visual
column reordering, 125% geometry and main button dock/bounds/icons/shortcut hints.
The existing column-presentation methods are safe to invoke after binding; the
fixture never invokes SQL searches or customer/sales selection handlers.
No business actions, login or store writes run; Debug configuration is checked.

Manual checklist:

1. Open frmCustomer, frmSalesRep and frmItemLookUp in the Designer. Confirm the
   lookup styling/columns and right-side button colors match the runtime layout.
2. On the test installation, press F8 and search by company/account as before.
   Check long company names, leading-zero accounts and the selected row. Only
   Account Number and Company should be visible, including after searching again.
   ID and Customer Type remain hidden bound fields used by selection/pricing.
3. Verify Down, grid arrows, Enter, double-click, OK, Cancel and Escape. Confirm
   the intended customer is selected and existing pricing, tax exemptions,
   approval requirements and Sales Rep assignment remain unchanged.
4. Press F7 and repeat selection/search checks for Sales Reps, including WALK-IN.
   The ID column is narrower and the Name column fills the remaining space.
5. Confirm all right-side buttons retain their existing order, icons, visibility
   and keyboard shortcuts. Save is blue, related workflow buttons pale blue,
   Remove/Cancel pale red and lookup/Close buttons neutral. Check the dynamic
   For Invoice count, including longer counts, on both supported screen sizes.
   Set Tax should sit directly above Logout, which is last in the top group;
   when Set Tax is disabled in Settings, Logout follows Check WO directly.

Actual query matching, application focus and customer-selection side effects
remain manual integration checks. The presentation revision changes neither
query nor selection method bodies; only old fixed-width assignments moved from
the code-behind to the Designer.

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
& '.\Database Components\Tests\ItemLogLookup_LocalDB_Test.ps1' -CompatibilityLevel 100
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

Build Release first with the installed ReportViewer 2010 prerequisite. This
copies only the application EXE into an owned `TestResults` folder and compiles
an x86 probe. It checks actual Import entry-point JIT, CSV/XLSX imports and single
Excel dependency identity. Both actual preview forms render normal Work Order,
quotation, empty, long-text, large-value and multipage PDF/TIFF samples without
invoking their production Load handlers. It asserts unchanged 850x650 preview
paper and existing zero integer margins, version-10 references/GAC loading and
zero private DLLs/config. No store SQL, app login or physical print job is involved.
The former five-DLL requirement belonged to the historical version-15 package;
the changed deployment contract is explicit, not a suppressed dependency failure.

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
