# Source Code Review Findings

**Review date:** July 23, 2026  
**Project:** Work Order Entry  
**Technology:** VB.NET WinForms, .NET Framework 4.7.2, LINQ to SQL

The live database comparison is documented separately in `DVO_STORE_DB_AUDIT.md`.

## Remediation status

The recommended data-integrity milestone was implemented on July 23, 2026. Findings 1, 3, 4, 6, 8, and the source-level portion of finding 9 are corrected in the active form workflows. Finding 2 is corrected for the set/clear transition and branch-safe reason lookup: the application resolves reason code `701`, type `6`, rather than assuming ID `39`. RMS privilege and received-entry enforcement remain. New-order and existing-order saves now use a local transaction covering the order, lines, inventory commitments, same-database queue records, price logs, and tax reasons. Credential remediation in finding 5 remains a deployment and data-migration task.

The post-change solution builds successfully. Runtime and rollback behavior still require integration testing against a disposable RMS-compatible database before production use.

## Review scope

This was an initial static review of the current working tree, with particular attention to the modified database-selection, login, price-level, and tax-exemption flows. Existing source changes were not modified or reverted.

## Verification result

The solution builds successfully in the Debug configuration using MSBuild 18.8.2:

```text
Work Order Entry -> Work Order Entry\bin\Debug\Work Order Entry_V4.exe
```

No automated test project was found. The successful build therefore confirms compilation only; it does not verify behavior against a live RMS database or Windows registry.

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

**Recommended fix:** Restore label-based selection using the customer's configured price level, preferably through one shared helper used by both price-level forms.

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

## Suggested remediation order

1. Correct customer-change pricing/recalculation and default price-level selection.
2. Correct tax-exemption reversal and enforce reason, privilege, and received-entry rules.
3. Preserve fractional quantities throughout order and inventory-commitment workflows.
4. Make login connection failures safe and deterministic.
5. Parameterize imported-item queries.
6. Rotate and remove committed credentials; introduce password hashing.
7. Make order saves transactional.
8. Add automated tests for price selection, customer changes, tax transitions, fractional quantities, login failure, and order rollback behavior.
