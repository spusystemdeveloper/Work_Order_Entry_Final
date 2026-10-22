# V4 basic selection and save checks

Scope: `Work Order Entry_4/Work Order Entry`, the active V4 project outside
`Old_`. The user selected **Basic selection/save checks first** on October 21.
Archived variants and the V8 application are not replaced or merged into V4.

## Duplicate company names: follow-up fix

The user also authorized the duplicate-company fix. Customer selection now uses
the current row for OK, the clicked row for double-click, and the highlighted
row on Enter before the grid moves. It no longer selects through the shared
cached `iRowCust` value, which can point to another same-name account after
navigation or a search rebind. Invalid row/header/empty-result selections leave
the existing customer untouched. Account numbers remain visible in the lookup.

The selected row's unique customer ID continues into pricing flags, primary
Sales Rep and saves. The redundant company-name query in LoadLevel was removed:
its `mask` result had no active consumers. The actual price choices and approval
thresholds still use the original iPrice/iPricePass rules and old GSC price
function. No new ID lookup query, filters, customer merge or SQL object changes.

`Database Components/Tests/V4DuplicateCustomer_LocalDB_UI_Test.ps1` creates two
synthetic same-name customers with different accounts, price levels, tax/employee
flags and Sales Reps in an owned LocalDB fixture. The unmodified V4 baseline
reproduces wrong-account selection with a stale row index. The candidate passes
28 checks across OK/double-click/Enter, account filtering/rebinding, both
identities, empty results and header clicks. Only the lookup's grid is hosted
offscreen; production form Load, save and print flows are not invoked. Existing
application configurations retain their hashes. Full old-GSC acceptance remains
required; this fixture does not change the newer localhost GSC restore.
The final duplicate-selection build also passed the existing 54 V4 guards,
both production Release/x86 builds and all 106 main unit tests. The EXE-only
candidate below has been refreshed with both the basic guards and this fix.

The selected V8-style input protections are implemented with a small
`Class/V4SaveValidation.vb` helper and guards in the existing main form:

- Select/keyboard price lookup requires a positive customer ID and nonblank
  customer name, just as the existing double-click path intended.
- Save and F12 share the same guard. New orders/quotes require positive customer
  and Sales Rep IDs, rather than relying solely on displayed names. Existing
  Work Orders require the same selections. Existing quotation edits retain
  V4's optional header fields; quotation payment is still optional.
- Work Orders require payment, including recalled edits. Header checks run
  before explicitly committing a pending grid edit and before quantity lookup.
  V4's existing price/password checks during editing remain in place.
- Empty orders, placeholder rows, invalid item IDs, unconfirmed/nonnumeric
  prices, invalid/nonpositive quantities, numeric conversion failures and values
  outside SQL Money range are blocked before the first insert/update call.
  Every row is checked, including later rows. Hidden numeric defaults keep
  V4's Nothing-to-zero conversion. Prepared counts keep the existing integer
  conversion/rounded-quantity comparison; no new fractional-quantity UI rule.
- A caught error in ValidateAllQty now increments its failure count so saving
  cannot continue after that lookup failed.

The old procedure/function names, argument order, mappings, pricing, tax,
approval thresholds, stock calculations, grouping, save confirmation, print and
refresh code are retained. No SQL migration is needed for these input gates.
This is not an upgrade of V4's transaction implementation: its separate writes,
stale-edit and cancellation limitations remain. Failed database writes can
still consume IDs. RMS investigation remains paused; no live DB was accessed.

The user explicitly selected **Keep old live GSC functions** after the read-only
localhost metadata check. The local GSC_STORE_DB on machine MSI currently has
a one-parameter SOD_fntbl_NestedSearchItem(@keyword); this V4 mapping expects
three parameters (bEmployee, iLevel, sTxt). That restore is not a matching target
for this V4 search path. No compatibility shim or SQL replacement was introduced.
SOD_sp_InsertQoute's eight and SOD_sp_InsertQouteEntry's twelve parameter
names/types/order matched the local metadata. This does not certify every
object's definition or behavior on the old live GSC server.

## Validation and remaining acceptance

Both unchanged baseline and modified V4 built Release/x86 using .NET 4.7.2 and
its existing ReportViewer 2010 references. Imported resource files had Windows
download-zone marks; only inspected active-project .resx files were unblocked
for the build. Resource content hashes did not change. The first attempted
baseline output path was corrected to an absolute path inside TestResults;
the attempt outside that workspace did not build successfully.

`Database Components/Tests/V4SaveGuards_UI_Test.ps1` exercises the compiled V4
helper and inspects the actual compiled callers to verify gates precede
allocation/update and that Save/F12 share the guarded path. It does not start
the app, open SQL, save an order or test a physical print. Pass
`-ApplicationPath <absolute-V4-EXE-path>` to test an isolated build.

All 54 compiled V4 checks passed. For the initial basic-guard build, hash comparison against the unmodified V4
baseline found only the existing main form/project file changed, plus the new
helper. SQL mappings, other classes, resources, reports and existing configs
retained their content hashes. Both main Release/x86 solutions, Debug, 106 main
unit, 38 withholding, 28 checkbox and 875 small-screen regressions passed. The
existing V8 multi-user suite passed 39 normal and 39 versioned-read checks in
its disposable LocalDB fixtures; these are V8 regressions, not V4 transaction
or real RMS POS tests. Main Debug/Release configs were preserved byte-for-byte.

Before deploying, use a test restore with the matching older GSC functions and
its existing configuration: no customer, no Sales Rep, blank payment, valid new WO,
quotation with blank payment, recall/edit, cancelled price approval and import.
Confirm prices, stock checks, printing and refresh still follow old GSC behavior.
Do not deploy this as proof that V4 now has V8's transaction protections.

The separate candidate EXE is
`Work Order Entry_4/Work Order Entry/bin/Release/EXE-Only/Work Order Entry_V4.exe`.
Only the EXE is supplied there; keep the old GSC installation's configuration.

Build the V4 project with Release/x86. Keep the target installation's existing
V4 EXE config and dependency/runtime setup. No configuration or credentials
are supplied by this update. Do not copy development configuration into a store.

## Suggested withholding display (October 22)

The current font revision matches the printed Sub Total: V4's withholding heading
and amounts use 8-point bold; V8 uses its existing 10-point bold. CASH still
prints `WTax N%`; CHARGE still prints `WTax N%_Charge`, based on saved payment.
The rate flags, calculations and totals are unchanged. Read-only checks of
localhost's latest CASH WOs 23311/23312 returned plain labels in both EXEs;
only 1% and 15% were enabled. No missing-label defect was reproduced in those
saved-data lookups. Both versions passed 62 local report checks and 160 PDF
comparisons, with builds/unit/V4 regressions and EXE-only packaging passing.
Use the updated EXE and saved CASH order number for physical print acceptance.

The latest payment-label update prints CASH references as `WTax N%` and CHARGE
references as `WTax N%_Charge`, using the first payment segment of the saved
order's Comment. Later remarks mentioning another payment type do not affect
labels. Missing/other payment types keep their existing neutral percent labels.
Only enabled customer rates appear; computations, totals and quotation rules
are unchanged. CHARGE uses one rate per line and the reference can grow to fit
the longer labels at the approved 14-point bold font. No SQL update is needed.
Both versions passed 62 local reference/render checks each and all 140 PDF
comparisons, plus builds, regressions, packaging and configuration preservation.
Physical printing remains a local manual acceptance check; no live DB was used.

The subsequent readability build matches the approved print sample: the
reference heading is 11 point and its amounts are 14 point/bold, with a
0.85-inch reference area. Only this section's fonts/height change. Each version
passed 46 local print-reference checks and all 74 cross-version PDF comparisons
passed, including full large amounts and no overlap with signatures. Totals,
quotation layouts and printable page counts are unchanged. EXE-only packaging,
main/V4 regression tests and store-config preservation also passed. Perform the
physical print check locally; no live store or printer was used in validation.

The latest report build also adds Withholding Tax (Reference) to the printed
Work Order, beside the existing totals. It uses the saved order's customer and
the exact printed VAT-exclusive subtotal, showing each enabled rate separately.
Rates are read from current customer settings when printing, not saved as an
order tax snapshot. No amount is deducted and no RMS tender is filled. Quotes,
customers without applicable rates and unavailable optional lookups omit it.
No SQL script or function update is required. Existing direct-print behavior,
ReportViewer 2010, paper settings, totals and GSC functions remain unchanged.

Both V4 and V8 passed 44 local print-reference checks each, plus 72 PDF
comparisons, main/V4 regressions and EXE-only packaging. No physical print or
live-store workflow was executed. V4's existing very-large-total line wrapping
is retained; all digits were verified against the original report. Perform a
local printed Work Order test with one rate, several rates and no rates, plus
quotation printing, before store deployment. Keep the installation's config.

The subsequent empty-results update adds messages inside bound, empty Customer,
Sales Rep and item grids. It also guards Sales Rep OK/double-click/Enter when
there is no row; the original compiled V4 reproduced an out-of-range error.
All 33 isolated empty-result checks passed, including the existing Esc handlers.
Queries, filters, nonempty selection rules and key assignments are unchanged.
Item search still shows its existing No Items Found warning and restores prior
results/filters; this update does not change that search behavior. Test searches
with no matching customer, Sales Rep and item, then search again successfully.

The later October 22 build also removes the pre-existing empty MessageBox.Show
in clsItemLookUp.LoadLevel. Check that choosing an item/price level no longer
opens a blank OK prompt and that price choices, approval and Cancel still work.
The price query and approval logic are unchanged.

V4 now shows the same prominent read-only Suggested W/Tax strip as V8.
It checks the branch's CustomCaption labels before interpreting customer
CustomNumber3=1, CustomNumber4=2, CustomNumber5=5 or CustomText2="15" as rates.
Each applicable amount is the existing displayed subtotal times that rate,
rounded to two decimals, and enabled rates pack from the left. Rates are
cached per selected customer ID, including customers with the same company name.
The strip hides for quotations, blank customers and customers without flags.
It does not populate an RMS tender or change an order total.

The old GSC function signatures, tax calculation, approval and save/printing
code remain unchanged. No database migration is required. At smaller heights,
the selected-items panel gives space back to keep customer controls usable.
The default selected-items height returns when there is sufficient room.

V4 Release/x86 built with the existing LocalReportExtensions ByRef warning.
57 disposable LocalDB withholding/UI checks, 54 compiled save-guard and 28
duplicate-customer checks passed; both main Release/x86 solutions and 106 unit
tests passed. Existing reports, mappings and store configurations retain their
hashes. No live database was accessed. These fixtures do not certify a real
GSC save, RMS tender or physical print. Manually confirm the strip on the GSC
test installation with its existing old functions/config before deployment.
