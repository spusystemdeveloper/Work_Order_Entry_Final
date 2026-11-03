# Source Code Review Findings

**Review date:** July 23, 2026  
**Project:** Work Order Entry  
**Technology:** VB.NET WinForms, .NET Framework 4.7.2, LINQ to SQL

The live database comparison is documented separately in `DVO_STORE_DB_AUDIT.md`.

**Documentation status checked:** October 7, 2026. The findings and source
line numbers below describe the original July review. Historical build and
database results are not new verification results. Later changes are described
in the remediation notes.

## Remediation status

The recommended data-integrity milestone was implemented on July 23, 2026. Findings 3, 4, 6, 8, and the source-level portion of finding 9 are corrected in the active form workflows. Finding 1 has a current-behavior note below: the Price Level form initially highlights its top row. Finding 2 is corrected for the set/clear transition and branch-safe reason lookup: the application resolves reason code `701`, type `6`, rather than assuming ID `39`. RMS privilege and received-entry enforcement remain. New-order and existing-order saves now use a local transaction covering the order, lines, inventory commitments, same-database queue records, price logs, and tax reasons. Credential remediation in finding 5 remains a deployment and data-migration task.

The post-change solution builds successfully. Runtime and rollback behavior still require integration testing against a disposable RMS-compatible database before production use.

## Review scope

The October 14, 2026 change separates recall item-tax reads, total calculation,
and credit-warning wait time. Recalled item tax flags are read in batches, while
normal customer/Set Tax flows retain their previous reads. Calculation and credit
warning rules remain unchanged. The older tax/totals phase included modal-warning
waits; its long samples do not by themselves prove slow tax computation.

The October 13, 2026 recall change batches last purchase prices and warehouse
details using data local to one recall. It adds component timings and preserves
the snapshot/save-lock checks. Approval-price, item-tax, and customer-summary
reads remain candidates for measurement. No production capacity claim follows
from the isolated multi-session tests or the local SELECT-only comparison.

The October 7, 2026 focused refactor shares price approval logic, batches existing
save reads after the locked edit check, and distinguishes committed saves from
cancellations and failures. Optional phase timing is available for save/recall
and refresh. Tax rules were not changed; mixed-tax total calculation still needs
a separate business review. See `ORDER_TOOLS_GUIDE.md` for timing instructions.

This was an initial static review of the current working tree, with particular attention to the modified database-selection, login, price-level, and tax-exemption flows. Existing source changes were not modified or reverted.

## Verification result

The solution builds successfully in the Debug configuration using MSBuild 18.8.2:

```text
Work Order Entry -> Work Order Entry\bin\Debug\Work Order Entry_V4.exe
```

At the initial review, no automated test project was found. The repository now
contains `Work Order Entry.Tests` with 101 database-free test methods. Run them
with `Run-UnitTests.ps1` from the repository root. This documentation update
did not rerun the original review's builds or database checks. The subsequent
order-tools implementation passed the current 101-test suite; see
`ORDER_TOOLS_GUIDE.md` for separate isolated SQL validation. Unit tests do not
verify live RMS database or Windows registry behavior.

## Findings

### 1. High: Customer price level is no longer selected by default

**Location:** `Work Order Entry/Forms/frmPriceLevel.vb`, lines 62-90

`LoadLevels` loads the available rows from Retail through the customer's configured price level. However, `SelectCustomerPriceLevel` now always selects row zero:

```vb
gridLevel.CurrentCell = gridLevel.Rows(0).Cells(0)
gridLevel.Rows(0).Selected = True
```

Row zero is `PRICE (RETAIL)`. Consequently, Price A, Price B, and Price C customers can receive the retail price when an item is added unless the operator manually chooses another row. The comparable selection logic in `frmLinqLevel.vb` still finds the row corresponding to `txtPriceLevel`.

The RMS Customization Guide confirms that customer price-level values are `0 = Default`, `1 = Price Level A`, `2 = Price Level B`, and `3 = Price Level C`. It also documents that setting a customer applies the new customer's default price level and recalculates prices and totals.

**Original recommendation:** Restore label-based selection using the customer's configured price level, preferably through one shared helper used by both price-level forms.

**Current behavior (October 6, 2026):** `frmPriceLevel.LoadLevels` loads rows
through the customer's permitted level, then calls `SelectTopPriceLevel` to
highlight row zero. `PriceLevelFormSelectionTests` explicitly verifies top-row
selection. Automatic customer-level selection is not implemented in this form.

### 2. High: Manual tax exemption can remain attached to a taxable order

**Locations:**

- `Work Order Entry/Forms/frmItemLookUp.vb`, lines 3642 and 6480-6506
- `Work Order Entry/Forms/frmCustomer.vb`, lines 41-50
- `Work Order Entry/Class/clsRecall.vb`, lines 269-320

`SetTaxChangeReasonCode` writes `DefaultTaxChangeReasonCodeID = 39` and assigns the same reason to every order entry when manual exemption is enabled. The method immediately exits when `bTaxExcempt` is false and never clears an existing manual reason.

When an order is recalled, reason code `39` forces `bTaxExcempt` back to true. If the operator changes that order to a taxable customer, the order and entry taxability can be updated, but the old reason code remains. On the next recall, the order is treated as exempt again.

Reason code `39` is also a hard-coded database identifier even though the application now permits selecting different store databases.

The RMS guide states that a taxability change should prompt for a tax-change reason code when applicable, enforce the cashier's tax-modification privilege, reject changes to already received order entries, and recalculate VAT prices and totals. The current `btnTax_Click` flow performs only the confirmation and total recalculation; it bypasses the other documented controls.

**Recommended fix:** Persist both transitions. When manual exemption is removed, clear the order and entry reason codes within the same transaction as the tax update. Resolve the reason by a stable business code or configuration rather than assuming primary key `39` in every database.

**Remediation:** Implemented. Manual exemption now resolves code `701`, type `6`, in the selected store database. The live branch check resolves Davao to ID `39` and Gensan to ID `33`. Saving fails safely when that business reason is missing or duplicated. Cashier privilege and received-entry enforcement remain open.

### 3. High: Database failures can escape the login error handler

**Locations:**

- `Work Order Entry/Forms/frmLogin.vb`, lines 97-111
- `Work Order Entry/Class/clsUser.vb`, lines 156-182

`Button1_Click` calls `clsUser.verifyUsername` before entering the database connection `Try` block. `verifyUsername` creates a data context and executes a query without handling connection exceptions. Therefore, an unavailable server, invalid selected database, or authentication failure can escape the click handler.

Additionally, constructing `ItemLookUpDataContext` does not necessarily open the SQL connection, so the later `Try` block may report success until the first query executes.

**Recommended fix:** Put connection validation and all login queries inside one exception boundary. Explicitly open/test the connection before querying, and do not continue authentication after a failed connection test.

### 4. High: Imported item codes are concatenated into SQL

**Locations:**

- `Work Order Entry/Class/clsImport.vb`, lines 151-153, 193-195, and 267-270
- `Work Order Entry/Forms/frmItemLookUp.vb`, lines 3283-3289

Item lookup codes originating in imported Excel/CSV data are wrapped in quotes and joined directly into an SQL `IN` clause. Apostrophes are not escaped, and the SQL is passed to `load_data` as executable text.

This creates both a malformed-query risk for legitimate item codes containing an apostrophe and an SQL-injection path through a crafted import file.

**Recommended fix:** Use SQL parameters. For a variable-size collection, use a table-valued parameter, a temporary table populated with parameters, or a LINQ `Contains` query over validated item codes.

### 5. High: Database and application credentials are stored as plaintext

**Locations:**

- `Work Order Entry/App.config`, lines 8-31
- `Work Order Entry/My Project/Settings.settings`, multiple connection-string settings
- `Work Order Entry/My Project/Settings.Designer.vb`, generated connection-string defaults
- `Work Order Entry/Class/clsUser.vb`, lines 68-75 and 175-181
- `Work Order Entry/Forms/frmLogin.vb`, line 120

Several committed connection strings include SQL usernames and passwords. Work Order Entry user passwords are also stored in `SOD_WO_Users.UserPass`, retrieved from the database, and compared directly with the entered plaintext password.

**Recommended fix:**

1. Rotate credentials that have been committed to source control.
2. Remove secrets from tracked configuration and inject them through a protected deployment configuration or Windows-integrated authentication.
3. Store user passwords using a modern salted password hash such as PBKDF2, bcrypt, or Argon2.
4. Authenticate by verifying the supplied password against the stored hash; do not return password values to the UI.

### 6. Medium: Order saving is not atomic

**Locations:**

- `Work Order Entry/Forms/frmItemLookUp.vb`, lines 1249-1316 and 1453-1559
- `Work Order Entry/Class/clsRecall.vb`, lines 269-388

Order headers, individual entries, queue records, committed quantities, price logs, and tax-reason codes are written through separate data contexts and separate `SubmitChanges` calls. Several lower-level methods catch an exception, show a message, and return without notifying the caller that the operation failed.

A failure halfway through the workflow can therefore leave a partially saved order while the outer operation continues and may display a success message.

**Recommended fix:** Move the complete save/update workflow into a service method using one connection and transaction. Let failures propagate to the transaction boundary, roll back all changes, and display success only after the transaction commits.

### 7. Medium: Empty exception handlers hide calculation and setup failures

**Examples:**

- `Work Order Entry/Forms/frmItemLookUp.vb`, lines 1584-1625
- `Work Order Entry/Forms/frmStoreSetup.vb`, lines 14-19
- `Work Order Entry/Class/clsWorkOrderDraft.vb`, lines 374-393

Some exception handlers are empty. For example, `UpdateAmt` silently ignores conversion or calculation failures, potentially leaving totals from an earlier state on screen. Store-list retrieval also suppresses its error and silently falls back to the current database.

**Recommended fix:** Handle expected conversion cases explicitly. For unexpected exceptions, log enough context and prevent the workflow from continuing with stale or incomplete values.

## RMS Customization Guide validation

The extracted guide in `.codex-temp/rms-customization-guide` is the legacy Microsoft Retail Management System Customization Guide (content dated 2004, extracted help files dated 2007). The following sections were checked against the application:

- `Extending_the_Databases.htm`
- `About_the_RMS_Database.htm`
- `The_Store_Operations_Database.htm`
- `Customer/Customer_PriceLevel_Property.htm`
- `SetCustomer_(FireEvent).htm` and `ClearCustomer_(FireEvent).htm`
- `Transaction/Transaction_SetPriceLevel_Function.htm`
- `Transaction/Transaction_SetTaxable_Function.htm`
- `TransactionEntry/TransactionEntry_SetTaxable_Function.htm`
- `TransactionEntry/TransactionEntry_TaxChangeOccurred_Property.htm`
- `TransactionEntry/TransactionEntry_TaxChangeReasonCodeGet_Property.htm`
- `TransactionEntry/TransactionEntry_TaxChangeReasonCodeSet_Property.htm`
- `ToggleTaxableTransaction_(FireEvent).htm` and `ToggleTaxableItem_(FireEvent).htm`
- `PerformAddItem_(FireEvent).htm`
- `TransactionEntry/TransactionEntry_QuantityOnOrder_Property.htm`
- `TransactionEntry/TransactionEntry_SetQuantityOnOrder_Function.htm`
- `Transaction/Transaction_OrderClosed_Property.htm`

The comparison produced these additional findings.

### 8. High: Changing a customer does not perform the RMS-required resets and recalculation

**Locations:**

- `Work Order Entry/Forms/frmItemLookUp.vb`, lines 2160-2189
- `Work Order Entry/Forms/frmCustomer.vb`, lines 37-101

The RMS `SetCustomer` behavior applies the new customer's price level, discounts, taxes, tax-exempt status, and sales representative, then recalculates prices and totals. Clearing a customer similarly resets price levels, discounts, and taxes to defaults and recalculates prices.

In the current application, changing the customer replaces every selected item's price with the string `"-"`. It does not call `ApplyCustomerTaxStatusToRows` or `UpdateAmt`, and it does not restore prices from the new customer's price level. The displayed totals can consequently remain stale while the grid contains invalid prices.

The price reset is also performed after `frmCustomer.ShowDialog()` without checking whether the operator actually selected a customer. The customer form does not return an explicit success `DialogResult`, so cancelling the dialog can still erase the displayed prices.

**Recommended fix:** Make customer selection return `DialogResult.OK` only after a valid selection. On success, apply the customer's price level, sales representative, discounts, and tax status to every row and recalculate all totals. On cancel, leave the order unchanged.

### 9. High: Fractional order quantities are rounded to integers during updates

**Locations:**

- `Work Order Entry/Class/clsRecall.vb`, lines 297, 312, 369, and 414-427
- `Work Order Entry/Class/clsItemLookUp.vb`, lines 719 and 746
- `Work Order Entry/Linq/ItemLookUp.designer.vb`, lines 419 and 4967-5251

The RMS guide defines ordered quantity as a `Double`, and the mapped RMS `OrderEntry.QuantityOnOrder` and `Item.QuantityCommitted` columns are also `Double`/SQL `float`. New-order insertion accepts a `Double`.

The update and committed-quantity methods instead accept or store these values in `Integer` variables. Because `Option Strict` is off, VB performs implicit numeric conversion, causing fractional quantities to be rounded and corrupting both the order quantity and inventory commitment during update, cancellation, or conversion workflows.

**Recommended fix:** Use `Double` consistently for RMS quantity fields and calculations, or `Decimal` only if conversion is performed deliberately at the RMS database boundary. Add tests covering quantities such as `0.5`, `1.25`, and negative return quantities.

### 10. Medium: Custom draft tables do not follow the guide's preferred database separation

**Locations:**

- `Database Components/SBD_CEBU_MAIN_DB/SOD_WO_DraftHeader.sql`
- `Database Components/SBD_CEBU_MAIN_DB/SOD_WO_DraftDetail.sql`
- `Work Order Entry/Class/clsWorkOrderDraft.vb`

The guide explicitly says not to add columns to existing RMS tables and recommends storing custom information in a separate database with uniquely prefixed table names. The draft implementation does avoid altering RMS tables and uses prefixed table names, which are positive points. However, it creates the custom draft tables directly in the selected RMS store database rather than a separate customization database.

This increases upgrade, backup, permission, and multi-store deployment coupling. It may be operationally acceptable for this installation, but it is not the guide's preferred design.

**Recommended fix:** For future custom data, use a separate customization database and reference RMS records by stable IDs. If the draft tables must remain in each RMS database, document that exception, include deployment/version checks, and ensure RMS upgrades preserve them.

### Guide-level architectural observation

The guide describes RMS POS transaction processing through QSRules classes and the Gateway, which apply validation, privileges, reason codes, totals, and transaction-type behavior before posting. This application instead writes directly to RMS `Order`, `OrderEntry`, and `Item.QuantityCommitted` data through LINQ to SQL and stored procedures.

Direct database access is not automatically invalid, but it means this application must reproduce all RMS invariants itself. The customer-change, tax-control, fractional-quantity, and partial-save findings show several places where that parity is currently incomplete.

## October 30, 2026 V8 focused review

Scope: audit only, no code changed by this review. Line numbers describe the
working tree on October 30, 2026, on branch codex/work-order-ui-testing.
Findings come from reading the code unless stated.

Reviewed: clsUser, clsImport, clsRecall, ModConnectDB, clsItemLookUp from line
560, frmLogin, frmPassword, frmRecall filter, OrderEditGuard, and the
frmItemLookUp save, validation, recall and import paths.

Not reviewed: the rest of frmItemLookUp, the first 560 lines of clsItemLookUp,
clsCustomer, clsWorkOrderDraft, shortage code, stored procedures, reports, V4.
VAT rules were not reviewed; mixed-tax calculation remains the separate
business review already noted in this file.

Nothing was compiled and no application workflow was run. Two isolated checks
ran: a PowerShell double-arithmetic calculation and an in-memory DataView
filter test.

Confirmed sound: the save transaction, order lock, stale-edit snapshot guard
including rejection of closed orders, and repeated cancellation not releasing
stock twice (quantities are zeroed in the transaction).

### 11. High: Fractional commitments can block cancelling or editing

**Location:** Work Order Entry/Class/clsItemLookUp.vb:752;
Forms/frmItemLookUp.vb:1335

QuantityCommitted is float and the guard requires the result >= 0, so rounding
can reject a valid release with "would make QuantityCommitted negative".
Commit 0.7 and 0.1, release 0.7 then 0.1, ends at -2.7755575615628914E-17. A
calculation over 400 fractional pairs gave 133 below zero. This was arithmetic
only, not an executed cancel.

Any fix changes the commitment guard, so needs human approval.

### 12. High: Recall truncates order remarks

**Location:** Forms/frmItemLookUp.vb:3530-3532, 3561, 1438

Work Order comments are split on "; " and only part 2 is kept as remarks, so
"call first; gate 2" becomes "call first", and saving makes that permanent. A
Type 2 comment with fewer than three parts raises an index error during recall.

**Remediation:** Fixed October 30 through WorkOrderComment.Parse. Remarks keep
any "; ". Imported "CHARGE; reference" comments, 62 of 418 open work orders on
localhost DVO_STORE_DB, load the payment and reference. Other short text loads as
remarks.

### 13. High: Repeated items break queue rows

**Location:** Forms/frmItemLookUp.vb:1632, 1686; Class/clsRecall.vb:290

Applies with queueing on, when duplicates are allowed or accepted at the
prompt. Edits use FirstOrDefault per item, so only the first queue row updates.
Deleting one duplicate line deletes every queue row for that item, so the
remaining line may lose its picking row. Recall also shows the first PickLoc
for both lines.

**Remediation:** Fixed October 30 in Work Order Entry through QueueLineMatcher,
which pairs lines and queue rows per item in ID order, with no schema change.
Open limits: the queueing app still updates every row of an item, shortage
acceptance still rejects repeated items, and existing damaged rows are not
repaired.

### 14. High: Imports can drop lines and still report completion

**Location:** Class/clsImport.vb:342, 378; Forms/frmItemLookUp.vb:3903, 4024

checkIfItemCodeExist reads ImportData("Contents"), but website imports leave
ImportData Nothing or stale, and SingleOrDefault throws on an Excel file
repeating a missing code. The Catch shows the error and returns 0, so the
caller shows import complete and the missing item is not on the order.

**Remediation:** Fixed October 30 through ImportFileRules.FindMissingItems for
purchase and website imports. The Excel quotation path was missed and was fixed
under finding 29. A failed check no longer reports completion.

### 15. High: Stock validation rounds availability to whole numbers

**Location:** Class/clsItemLookUp.vb:659-672; Forms/frmItemLookUp.vb:2971-2976

getItemQty returns Integer with Option Strict off. 0.6 rounds to 1, so 0.8
passes; 2.5 rounds to 2, possibly raising a false shortage. New work orders
have no atomic stock check, as requireAvailableStock applies only to quotation
conversion. Prepared quantities are also integers (QueueingItems.QtyPre,
Convert.ToInt32 at lines 1219 and 1581).

Any fix touches stock validation and needs human approval; the existing paired
validation comparison preserved current behavior.

### 16. High: Credentials in tracked files

**Location:** Module/ModConnectDB.vb:8; App.config;
My Project/Settings.settings; Module/MySQL_DAL.vb

ModConnectDB.vb:8 contains an sa password in a comment, confirmed. A pattern
search matched Password= values in App.config (6) and Settings.settings (12);
those values were deliberately not opened. All four files are tracked and the
repository has a GitHub remote. User and approval passwords remain plaintext.
This extends finding 5.

Rotating credentials is an operational action for the store owner.

### 17. Medium: Validation errors do not stop the save

**Location:** Forms/frmItemLookUp.vb:3018-3021, 2950, 914

An exception in ValidateQuantities returns Nothing with iErrCnt unchanged, so
SaveOrder continues without that check. A "-" price returns at line 2950
without counting an error; later conversion fails safely inside the
transaction, but later rows are not stock-checked first.

### 18. Medium: Blank approval password is conditional

**Location:** Forms/frmPassword.vb:15; Class/clsItemLookUp.vb:588

If Password, Password2 or Password3 in SOD_WO_Conf is an empty string, a blank
entry passes. It guards price, Settings and cancel actions. The data was not
inspected; verify per store.

### 19. Medium: Price-approval state and approver logging

**Location:** Forms/frmPassword.vb:25, 29; Forms/frmItemLookUp.vb:1681,
5261-5266

Line 29 sets ispriceApproved = 1 for every password type, including Settings
and CancelOrder, and overwrites the assignment at line 25. Deleted lines pass
an empty password, and VB treats Nothing as equal to an empty string, so a
blank Password2 or Password3 can name the wrong approver.

### 20. Medium: SQL failures close the application

**Location:** Module/ModConnectDB.vb:188-190

load_data calls Application.Exit on any error, so a failed barcode search
(frmItemLookUp.vb:4621) or item-list load (line 2095) ends the session
mid-order.

### 21. Medium: Recall search rejects some characters

**Location:** Forms/frmRecall.vb:150-165

Only apostrophes are escaped. In an isolated DataView test, `[A` and `a%b`
raised EvaluateException "Error in Like operator". The timer path has no Try,
so the error reaches the application-level handler. Not tested in the running
application.

### 22. Low: Legacy reservation check uses a different column

**Location:** Class/clsRecall.vb:617, 622

Line 617 tests SOD_WOs.ID = orderid while line 622 uses OrderID. Likely a
defect; not confirmed against data.

### 23. Low: Possible doubled website shipping

**Location:** Class/clsImport.vb:271-279

Each import adds a shipping row to the shared ImportDataCSV table, so
importing an order twice without refreshing may add it twice. Not executed.

### 24. Low: Possible duplicate insert after a post-commit failure

**Location:** Forms/frmItemLookUp.vb:1272-1303

A failure after commit but before RefreshDetails returns Saved without
clearing the screen, so saving again may insert a new order. AGENTS.md forbids
this outcome. The caller's handling was not traced; printing has its own error
handling, so the window appears narrow.

### 25. Low: Unescaped item code in SetItemAvailable

**Location:** Forms/frmItemLookUp.vb:4107-4115

The code is concatenated into DataTable.Select, the error is suppressed, and
AcceptChanges is called on dt3 although dt1 is changed.

### 26. Low: Unused legacy order methods

**Location:** Class/clsRecall.vb

UpdateOrder, UpdateOrderEntry, DeleteOrderItem, UpdateQTYCommitted and
CancelQuotation have no callers in this project. They use the older
non-transactional logic and must not be reused.

### Recommended order

1. Fix findings 12, 13, 14 and 15 first.
2. Then fix finding 11.
3. Findings 11, 13 and 15 touch stock or queue rules and need human approval
   before implementation.
4. Finding 16's credential rotation is a store-owner action.

### Remediation status

Findings 12, 13 and 14 are fixed as noted above. On October 30 the user also
approved and received fixes for the following findings. Only finding 16 remains
open.

- **11:** A 0.000000001 tolerance in OrderCommitmentRules treats float remainders
  as zero in the atomic commitment update, and a result within it is stored as
  exactly zero. Real shortfalls are still rejected.
- **15:** getItemQty returns the exact Double and the out-of-stock checks use
  `<= 0`. QueueingItems.QtyPre stays an integer column; that needs a schema change.
- **17:** A failed quantity check and a "-" price now count as errors, so the save
  stops.
- **18:** ConfPass rejects blank and all-spaces passwords before SQL.
- **19:** Only price approvals set ispriceApproved. The approving password is kept
  separately from the dialog text, and blank passwords name no approver.
- **20:** load_data shows the error and returns an empty table. It no longer exits.
- **21:** Recall search reuses ShortageAlertList.EscapeSearch.
- **22:** CommittedWO matches legacy rows by OrderID and item.
- **23:** Website import adds shipping once per order.
- **24:** ClearFormAfterCommittedSave clears a saved order from the screen even when
  its refresh fails.
- **25:** SetItemAvailable had no callers and was removed.
- **26:** The listed methods and their now-unused helpers were removed:
  getWOCommittedQty, setQuantityCommitted, SetQuantityCommittedToQoute,
  getItemQtyCommitted, setQtyCommittedQuotetoWo, InsertPriceLogs and the duplicate
  frmItemLookUp.ConfPass.

Finding 16 stays open at the user's request.

## October 30, 2026 V8 second review

This audit covered clsCustomer, clsWorkOrderDraft, the first 560 lines of
clsItemLookUp, the order tools, shortage, withholding and price-level forms,
customer and sales rep pickers, frmUser, frmUploader, frmCustomerOrders, pick
lists and the insert procedure. Not reviewed: most stored procedures, RDLC
reports, the queueing app, frmLinqLevel, frmStoreSetup, print forms and the
notification and history classes. "Proven" means run against a throwaway restored
copy of localhost DVO_STORE_DB; other findings come from reading the code.

### 27. High: Closing with a recalled order creates a duplicate on recovery

**Location:** Forms/frmItemLookUp.vb (FormClosing, RecoverDraftIfAvailable);
Class/clsWorkOrderDraft.vb

Proven. A draft has no order number, so order 23325 recovered and saved as new
order 23339 while the original stayed open, reserving stock twice.

**Remediation:** Fixed October 30. Recalled orders no longer save a draft on
close, and an existing draft holding order lines is removed with a message
instead of being recovered. Unsaved edits to a recalled order are now lost on
close, because no draft is kept.

### 28. High: Customer and sales rep pickers can select the wrong row

**Location:** Forms/frmCustomer.vb; Forms/frmSalesRep.vb

Proven. OK used a remembered row number: highlighted customer 3265, selected 2.

**Remediation:** Fixed October 30. OK and double-click use the actual row, and
the remembered row resets on load and search.

### 29. High: Excel quotation import dropped missing items

**Location:** Class/clsImport.vb; Forms/frmItemLookUp.vb (ImportQuote)

Proven. A two-line file loaded one line and reported complete.

**Remediation:** Fixed October 30. It now uses the shared missing-item check.

### 30. High: Excel quotation import used the file's item ID

**Location:** Class/clsImport.vb (importOrderEntry)

Proven. Item ID 24 was loaded as 70856 from the file.

**Remediation:** Fixed October 30. The ID comes from the matched Item row.
importOrder still matches customer and sales rep by the file's numeric IDs.

### Open findings

- **31, Medium:** Draft Qty is an int column, so 2.5 recovers as 0. Proven. A fix
  needs a schema change at each store.
- **32, Medium:** frmUploader's gate omits bItem, so a missing item is added with
  no error counted, and the later save fails.
- **33, Medium:** frmUploader uses CInt, rounding 2.5 to 2.
- **34, Medium:** InsertQuotationLog has no callers, so the duplicate upload check
  cannot trigger.
- **35, Medium:** The upload log path is fixed to a shared address, and retrying
  after a failure can add rows twice.
- **36, Medium:** clsPickList stores quantities as Integer, and getQtyPrep returns
  the first row of a repeated item.
- **37, Medium:** Logout discards the current order with no warning or draft.
- **38, Low:** Clicking a price-level column header can insert the item.
- **39, Low:** Recall of an order with no customer keeps the previous header.
- **40, Low:** Editing a user can create a duplicate username, which blocks that
  login, and frmUser can display passwords.
- **41, Low:** Ungrouping accepts any order number, some price checks use the
  cached global context, and website lines are saved with Taxable 0 (business
  review needed).

## Suggested remediation order

1. Correct customer-change pricing/recalculation and default price-level selection.
2. Correct tax-exemption reversal and enforce reason, privilege, and received-entry rules.
3. Preserve fractional quantities throughout order and inventory-commitment workflows.
4. Make login connection failures safe and deterministic.
5. Parameterize imported-item queries.
6. Rotate and remove committed credentials; introduce password hashing.
7. Make order saves transactional.
8. Add automated tests for price selection, customer changes, tax transitions, fractional quantities, login failure, and order rollback behavior.
