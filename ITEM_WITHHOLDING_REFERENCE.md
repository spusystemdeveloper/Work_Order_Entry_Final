# Item-based withholding references in V4 and V8

Main-screen amount labels now hide rounded zero amounts and pack nonzero enabled
rates to the left. Calculation bases, customer flags, totals and the existing
basis/review-warning panel remain unchanged. Printing also hides zero amounts.

The main-form suggestions and printed Work Order income-withholding reference
use each item's current Notes markers; 5% uses the separate customer rule below. This changes the optional reference calculation only.
Prices, totals, VAT calculation, exemptions, approval/password checks, stock,
commitments, save transactions, quotations and RMS tender entry are unchanged.
The Davao RMS XML receipt is excluded from this update.

| Primary marker (one explicit classification, or default GOODS) | Reference rate |
| --- | --- |
| [WTAX:GOODS] | 1% |
| [WTAX:SERVICES] | 2% |
| [WTAX:EWT15] | 15%, using accounting's confirmed classification |
| [WTAX:GIFT_CERT] | No withholding contribution |

**Current 5% rule (October 24, user requested pending accounting review):**
The customer's existing 5% flag/caption enables a suggestion of the entire
existing VAT-exclusive order subtotal x 5%, when VAT applies. No item VAT5
marker is required. The existing two-decimal rounding is retained. For this
reference, gifts and unclassified amounts within that subtotal are not removed
from the 5% base; marker exclusions concern 1%, 2% and 15% income withholding.
VAT-exempt/zero-VAT saved orders do not suggest 5%. This is a configured estimate,
not automatic validation of a transaction's statutory withholding applicability.
Existing paired `[WTAX:VAT5]` notes may remain; they add no extra 5% amount.

Customer additional-property flags and their existing caption checks still
control which rates are enabled. An item marker does not enable a customer rate.
Enabled income rates (1%, 2%, 15%) without matching items display zero.
Customer-enabled 5% is independent of those item classes. CASH/CHARGE tender suffix rules remain.

Markers are case-insensitive, can appear among ordinary notes, and repeated
identical markers do not double-count. **October 27: blank Notes or Notes with
no WTax marker default to GOODS (1% only when enabled for the customer/order).**
`[WTAX:REVIEW]` is retired and ignored; existing Notes text is not deleted. If it
is the only old token, the item defaults to GOODS. An explicit SERVICES, EWT15
or GIFT_CERT marker still takes priority beside retired REVIEW text.
Unknown markers, conflicting primary classes, VAT5 alone, or GIFT_CERT combined
with VAT5 still require review and are excluded from 1%, 2% and 15% suggestions;
customer-enabled 5% still uses the whole VAT-exclusive subtotal when VAT applies. The UI shows NEEDS REVIEW and the printed reference
states the excluded line count. This does not block or alter existing saves.

## Computation example

Assuming the customer has both rates enabled and the items have approved markers:

| Line class | VAT-inclusive amount at the existing 12% VAT | VAT-exclusive base | Rate | Suggested amount |
| --- | ---: | ---: | ---: | ---: |
| GOODS | 1,120.00 | 1,000.00 | 1% | 10.00 |
| SERVICES | 560.00 | 500.00 | 2% | 10.00 |

Combined reference: 20.00. Existing subtotal remains 1,500.00, VAT 180.00 and total
1,680.00. Nothing is deducted from the order total or entered into RMS tenders.
If both lines are VAT-exempt instead, their existing zero-VAT amounts are the
bases; the reference does not divide those amounts by 1.12.

The existing V4/V8 totals routines apply VAT uniformly at order level. To retain
those exact routines and their rounding, the reference groups each class's line
price-times-quantity amounts and allocates the existing VAT-exclusive subtotal
proportionally: `class gross * existing subtotal / all line gross`. Gift and
unknown/conflicting amounts remain in that denominator; their share is never assigned
to another class. Each rate is then rounded once to cents, midpoint away from
zero. No new line-level VAT rule or tax rate is introduced. Multiply before
division to avoid a repeating intermediate VAT factor shifting a half-cent.

The UI reads Notes in batches of up to 1,000 distinct integer IDs, not a query
per line, and refreshes those markers when suggestions refresh. Customer flags
retain the existing customer cache. Saved-order printing reads the saved
customer/payment comment. Open-order lines use OrderEntry.QuantityOnOrder;
closed-order reprints use TransactionEntry linked by Transaction.RecallID,
matching the existing print function. Printed VAT eligibility follows the actual
saved tax amount rather than trusting a possibly stale Taxable bit. Reprints
use current item Notes; classifications are not frozen in historical orders.

## Validation and rollout

- Both production solutions and the isolated V4 project built Release/x86.
- 112 main unit tests passed, including mixed rates, fractional/signed rounding,
  gifts, conflicting/missing markers, VAT5 and exemptions.
- `ItemWithholding_LocalDB_Test.ps1` passed 73 actual compiled UI/helper/report
  checks for each version using separate owned disposable LocalDB databases.
  It never loads the production main form or physically prints. Covers marker
  changes with unchanged totals, eligibility, closed reprints, review messages,
  all enabled-rate layouts, and PDF generation from the unchanged embedded RDLC.
- Existing V4 guards/customer/empty/recall/UI checks passed (54/28/33/19/57),
  V8 withholding UI 55, print-helper/render 62 each, print UI 91, existing PDF
  comparisons 160 and new marker PDF cases 12. EXE-only packaging passed.
- SELECT-only checks against existing localhost DVO_STORE_DB settled WO 23309
  returned 1%_Charge 7.95 for both versions from its current GOODS marker and
  printed VAT-exclusive base 794.64. No existing store record was written by
  this software task. Earlier separately authorized local marker changes remain.
- Debug/Release configurations, report templates, and all Davao receipt-folder
  files were preserved. No SQL objects, triggers, isolation settings, live data
  or RMS settlement logic changed. RMS investigation remains paused.

Deploy only the matching updated EXE and retain the unit's existing EXE config
and installed ReportViewer 2010. No schema/script deployment is required, but
approved item markers must be maintained in the source master data before sync
to a branch. Untagged branch data now defaults to GOODS. Services, 15% and gift-certificate
exceptions still need their explicit markers; do not assume localhost markings
already exist on GSC/Davao live databases. Notes still
appears in existing reports and can be overwritten by item sync.

Manual localhost acceptance remains: save/recall a mixed GOODS/SERVICES order,
check the displayed and printed 1%/2% bases/amounts, then check an exempt customer,
a gift certificate and an unmarked item. Automated tests are not live-store,
actual RMS POS settlement or physical printer acceptance.
# Printed payment breakdown (October 23)

The original item table and Sub Total / Sales Tax / Total section are retained.
Nonzero WTax amounts appear in one column on the left, with CASH, CHARGE, DEBIT
or CREDIT and its net amount at the top. Net payment is the saved total minus
enabled rounded withholding amounts. CHARGE retains _Charge tender labels.
No nonzero withholding means no payment/WTax block. Review warnings remain.
This replaces the subsequently reverted item VAT columns and Amount Due layout.
No saved amounts, tax rules, stock or tenders are changed.

## Compact Work Order printing (October 24)

The half-sheet report shows individual nonzero WTax rows in regular 10pt,
followed by bold Total WTax and Est. Cash/Charge/Debit/Credit Due. Total WTax
sums the same already-rounded enabled amounts; estimated due subtracts that
sum from the original saved total, exactly as before. No Less label is used.
Zero/no-enabled references remain hidden, review warnings remain visible,
and unknown payment types omit the estimated payment line. Original order
subtotal/VAT/total values remain untouched; the printed total label is Order
Total. Signatures share one row. Both the existing preview (8.5 x 6.5) and direct
print (8.5 x 6) paper paths remain unchanged. More/longer items paginate naturally;
six normal and six wrapped test items fit on one sheet in local rendering.
No real printer/RMS POS or live database was used during validation.

Withholding labels, colons and amounts now have separate report columns. All
amounts share a right edge, including Total WTax and estimated payment due.
This is presentation only; the reference string/calculation stays unchanged.

The fixed colon column is kept close to the withholding labels; numeric values
retain their separate shared right edge. Long 15%_Charge/estimated payment
labels are included in the rendered layout checks.

The withholding numeric column is close to its colons while retaining a common
right edge. Sub Total, Sales Tax and Order Total use a fixed shared colon column.


Example with customer 1%, 2% and 5% enabled: goods gross 1,120.00 + services
560.00 = 1,680.00 total; existing VAT-exclusive subtotal 1,500.00. Suggestions:
1% goods = 10.00; 2% services = 10.00; 5% order subtotal = 75.00. Total WTax
95.00; estimated payment 1,585.00. If customer 5% is disabled, the same item
notes produce only 20.00 WTax and estimated payment 1,660.00. Stored subtotal,
VAT and total never change. No SQL migration, item tagging or RMS receipt edit.
# V4/V8 WALK-IN selections (October 26, 2026)

For a selected customer whose AccountNumber normalizes to WALKIN (WALK-IN or
WALK IN), the right-hand Set W/Tax button appears for Work Orders. Select the
applicable 1%, 2%, 5% and/or 15% rates, then OK. Uncheck all to explicitly disable
withholding for this order. Cancel leaves the previous selection unchanged.
The button is hidden for named customer accounts and quotations.

Saved orders retain the rates in the existing Order.ShippingNotes field:
`[WO-WTAX:v1;RATES=1,2]`. `[WO-WTAX:v1;RATES=]` means explicitly no rates, while
no marker preserves the existing customer-setting fallback. Other shipping-note
text remains intact. No new table, column or SQL deployment is required.
In V8, every new split header receives the selection within its save transaction;
recalled-order edits retain existing locking and stale-edit checks. V4 stores
the selection with its legacy header save, without changing its save boundaries.
Reprint reads the saved order, independently of the main form's current selection.

This overrides rate eligibility only for the WALK-IN order. Item Notes still
determine the 1%, 2%, 15% bases; enabled 5% still uses the existing subtotal only
when VAT applies. Shared WALK-IN properties, original order totals, VAT,
commitments and RMS tender entries are not changed. Changing to a named
customer uses that customer's settings and removes only the custom marker on
save. V4 uses the same marker and rate-selection behavior, preserving its old
GSC routines and legacy per-step save boundaries; its header save carries the
metadata without adopting V8's transaction implementation.

The selection is retained by a successful Work Order save; it is not added to
the separate unsaved-draft storage. Local tests cover a simulated settlement
and closed-order reprint. Native RMS POS may have other ShippingNotes users;
verify that local RMS tendering preserves the marker before store deployment.

