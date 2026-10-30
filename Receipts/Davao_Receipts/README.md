# Davao wholesale work-order receipt

## Confirmed zero-stock non-inventory markers on localhost

On October 23 the user additionally authorized identifying and marking confirmed
services, rebates and commission with Quantity=0 and ItemType=7 on localhost
MSI/DVO_STORE_DB only. The clarified request was for item data only; no addin or
receipt calculation changes were authorized by the phrase “update also.”

An explicit reviewed ID/code/description manifest added [WTAX:SERVICES] to 162
service entries (photocopying, lamination, binding, repair, delivery/handling and
other descriptions explicitly identifying service work), and [WTAX:EWT15] to:

| Item code | Description |
| --- | --- |
| SVC00123 | ICP MONTHLY LOAD IN REBATE |
| SVC00124 | SELL OUT REBATES PROGRAM |

The existing SVC00118 SERVICES, SVC00121 commission EWT15 and SVC00132 rebates
EWT15 samples were not overwritten. The resulting zero-stock non-inventory
marked totals are 163 SERVICES and four EWT15. Existing 9,860 positive-stock
GOODS notes and the gift-certificate marker remain intact. All 9,864 prior marker
notes were protected, and every other field on changed rows was compared before
commit (except automatic rowversion). Normal item/history triggers ran unchanged.

These nine ambiguous entries remain unmarked rather than inferring their tax
class from their department or the SVC prefix:

| Item code | Description |
| --- | --- |
| 11156PCS | CANON CONSUMABLE CLAIM 5% |
| SVC00117 | SERVICE WARRANTY |
| SVC00119 | SERVICE MONTHLY RENTAL |
| SVC00122 | PSD MONTHLY REPORT SUBMISSION |
| SVC00125 | IP LSM SUPPORT |
| SVC00126 | PARTNERS CLAIM |
| SVC00127 | PRICE DROP SUPPORT |
| SVC00128 | ASC LABOR CLAIM CSP |
| SVC00129 | SELL OUT SUPPORT PROGRAM |

Do not interpret the 5% in a consumable-claim description as confirmed VAT5
eligibility. Audit CSV/script/output are in
TestResults/DavaoWTaxZeroNonInventory-20261023. No live/source server, business
rules, tax/price/stock values or software calculations were changed.

## Approved positive-stock classification on localhost

On October 23 the user explicitly confirmed assigning [WTAX:GOODS] to all
currently unmarked items with physical Item.Quantity > 0 in localhost
MSI/DVO_STORE_DB, including inactive items. This was an explicit local data
instruction, not an inferred rule that all stocked/non-inventory items are goods.
9,859 notes were appended; the existing positive-stock GOODS sample brought
the marked positive-stock total to 9,860. The one inactive eligible item was
included as approved. No existing marker was replaced; no zero-stock item was
newly marked. Earlier zero-stock service, rebate, commission and gift-certificate
sample classifications remain intact, as does the separate (TAX1%) note.

The first attempt timed out and rolled back. Verification confirmed no partial
bulk update. The successful retry retained all guards, added a temporary snapshot
index, and checked every non-Notes item column with null-safe comparisons before
commit (excluding automatic rowversion). Stock, commitments, prices and tax
settings were unchanged. Notes audit rows and both attempt records are retained
under TestResults/DavaoWTaxPositiveStock-20261023. Existing update/history triggers
were left enabled. No live/source database was accessed and no application
calculation or automatic future-item classification was implemented.

## Localhost item-classification samples (October 23)

At the user's explicit request, only these sample Notes were updated in
localhost MSI/DVO_STORE_DB. All five Notes were blank before the append:

| Item code | Description | Appended marker | Accounting-confirmed class rate |
| --- | --- | --- | --- |
| 55157RMS | Book paper | [WTAX:GOODS] | 1% |
| SVC00118 | Service cutting fee | [WTAX:SERVICES] | 2% |
| SVC00132 | Rebates | [WTAX:EWT15] | 15% |
| SVC00121 | Commission | [WTAX:EWT15] | 15% |
| GC00001 | Gift certificate | [WTAX:GIFT_CERT] | 0% |

These are classifications for a future mixed-item computation, not a change to
the current whole-order reference calculation. Neither the addin nor this receipt
has been changed to consume these markers. Customer eligibility/applicability
still requires its agreed rules; no VAT5 marker was inferred. Unknown/missing
classification must not silently become GOODS. The older 60976BDL (TAX1%) note
was left intact and is not automatically reinterpreted as an agreed marker.

The guarded update appended rather than replacing any existing notes and checked
all non-Notes item columns (except rowversion) before committing. Existing stock,
commitments, prices, tax settings and other values were preserved. Normal item
update/history triggers remained enabled. The before/after output and executed
script are retained under TestResults/DavaoWTaxSamples-20261023. No live/source
server or database schema changed. Reports and item sync still consume Notes;
do not distribute these sample classifications without reviewing those effects.

`SOD-DAVAO_STORE_RECEIPT_WHOLESALE.xml` is an RMS XML receipt template,
independent of the V4/V8 EXE's embedded ReportViewer reports.

The added **Suggested W/Tax (Reference)** section follows the existing totals
and precedes the existing signatures. It uses the same SmallBold font as Sub
Total and prints each enabled rate on its own line, with no deduction from
Sub Total, Sales Tax or Total. No tender is selected or posted by this template.

Customer fields must have the configured captions and exact enabling values:

| Customer field | Caption | Enabled value |
| --- | --- | --- |
| CustomNumber3 | W/Tax 1% | 1 |
| CustomNumber4 | W/Tax 2% | 2 |
| CustomNumber5 | W/Tax 5% | 5 |
| CustomText2 | W/Tax 15% | 15 (text) |

The receipt engine checks these captions literally; keep the captions as shown.
Unlike the addin helper, this XML does not normalize arbitrary whitespace or
trailing colons. Unsupported/unrelated captions or values stay hidden.
Only work orders and sales receipts with a selected customer and at least one
enabled rate show
the section. Quotations and no-rate customers retain their existing layout.
Labels are neutral reference labels (`WTax 1%`, etc.); this section does not
infer CASH/CHARGE tender names from free-form comments or customer credit status.

The base is `Transaction.Total - Transaction.SalesTax`, using the actual VAT
amount rather than assuming 12%. Each amount is base times its enabled rate,
rounded to cents with midpoint values away from zero. A VAT-exempt customer's
zero sales tax leaves the full amount as the base; withholding eligibility
continues to depend on the enabled custom fields, independently of VAT exemption.
Currency values use RMS's regional currency formatting.

Example: total 1,100.00, VAT 117.86, base 982.14. Enabled 1% and 15% show 9.82
and 147.32; total remains 1,100.00. If only 15% is enabled, only that rate appears.

## Validation and deployment

Run `python "Database Components/Tests/DavaoReceiptWithholding_Test.py"` from the
repository. Seven offline contract tests cover the actual XML's conditions and
arithmetic with synthetic inputs, and a canonical hash checks all original
receipt elements remain unchanged after removing the new section and call.
This interpreter is a test harness, not the RMS compiler or a print preview.
Both production Release/x86 builds, main unit tests and the application packaging
regression are separate checks; ReportViewer rendering does not validate RMS XML.

Before deployment, use a separate receipt format on a confirmed localhost test
database in RMS Manager. Open this template in its work-order receipt properties
and Refresh/Preview. Check a customer with all rates, sole 15%, no rates, VAT
exemption, and a long multi-page work order. Verify full amounts, unchanged
totals, page breaks and signature spacing. Keep the existing receipt format
until this preview is accepted. No live receipt format, register, database or
installed template has been changed by this source edit.

Both source filenames (original and `_revised.xml`) contain the corrected sales
receipt/work-order condition. The user's local installed `_revised.xml` still
contains the earlier work-order-only condition: Windows denied the attempted
replacement in Program Files. Copy the corrected source there with appropriate
Windows permissions, then reopen the intended receipt format, reload the XML
and Refresh/Preview. Previously stored journal receipts are already-rendered
records; viewing one again does not regenerate it from the new XML.

This template must be loaded into the intended RMS receipt format separately;
replacing the addin EXE does not install it. Keep the old template for rollback.

## Missing reference diagnostics (investigation still open)

The narrow POS receipt's WTax Tendered rows are existing actual tender amounts;
they do not prove the new suggested reference rendered in this full-page XML.
Read-only localhost checks found customer 313's four enabling values and expected
captions, plus the new section and sales condition in SAMPLE_NEW_WHOLESALE's
stored Sales/Work Order templates. The installed revised XML also matches the
corrected source. The user's full-page output still lacks the reference, so the
offline interpreter is not sufficient evidence of RMS runtime compatibility.

`Diagnostics/SOD-DAVAO_WITHHOLDING_DIAGNOSTIC.xml` adds an unconditional local
preview table with raw receipt fields and direct condition results. In a localhost
receipt Properties dialog, Open this file and Refresh; do not save the diagnostic
as a deployed format. Record Number3/caption, direct numeric/caption results,
type/customer guard and SET-enabled values. Cancel afterward. This separates
field exposure/caption matching from variable/conditional execution issues.
The diagnostic has not yet been verified by the actual RMS renderer. The desktop
helper could not activate the elevated RMS window; user-run preview is required.
No further production-template change was guessed from the missing output.

Syntax and fields were checked against the supplied Microsoft **Customization
Guide September 2007.chm**: Customer Information Template Variables, POS Template
Variables, SET Element, IF Element, Boolean Expression and Numeric Expression.
These document the native custom fields, typed currency variables, conditional
sections and Int/Abs operators used here. Actual RMS preview/physical printing
remains a manual acceptance check; the RMS trigger investigation stays paused.
