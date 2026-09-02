# Work Order Entry and Order Processing Queueing Implementation Plan

**Projects:** Work Order Entry and ORDER PROCESSING QUEUEING  
**Database family:** Microsoft Dynamics RMS store databases  
**Plan date:** 2026-07-23  
**Status:** In progress. Unified branch processing settings and the Work Order Entry data-integrity milestone are implemented; branch integration testing remains pending.

**Read-only reference implementation:**  
`C:\Users\ADMIN\Documents\SPU - JANREY FILES\RMS ADDINS\WORK ORDER\WORK ORDER V4\Work Order Entry\Work Order Entry_4`

The reference project represents the add-in used by other stores. Its Work Order flow creates queue/pick-location data, commits inventory for type-2 orders, and actively prompts for customer order grouping. It is used to preserve the established queue-enabled workflow while allowing those processing functions to be enabled or disabled per deployed branch. The reference files must not be modified.

## Implementation progress (2026-07-23)

The following source fixes are implemented and compile successfully:

- One executable now supports queue-enabled, mixed, and direct-processing branch profiles through non-secret `App.config` settings.
- **Branch Queue** is configured by an authorized user in `frmSettings`, not selected on each order.
- Every Work Order and Sales Quotation retains a `Queueing` header containing its OrderID and OPIS owner so it can be recalled for the current account/order taker or through the All option.
- Branches with processing queue disabled still reserve inventory for type-2 Work Orders, but do not retain `QueueingItems` and do not use grouping or queue invoicing.
- Queue grouping runs only for queued Work Orders and only when enabled for that branch.
- The queue invoicing action is hidden when branch queue processing is disabled.
- The idempotent `Ensure_Queueing_Compatibility_Tables.sql` migration supplies the compatibility tables required by the deployed legacy insert procedures at every branch.
- Customer Price A/B/C selection now defaults to the customer's configured level.
- A successful customer change marks every selected line with `"-"` and requires the operator to confirm a new price by double-clicking each highlighted row. F2 remains the item-search command. The prompt shows the number of unconfirmed items, selection advances to the next unconfirmed row, and saving remains blocked until every line is confirmed; cancel leaves the order unchanged.
- Accounts-receivable credit is calculated from the selected customer ID rather than treating the ID as a currency value.
- Manual tax exemption resolves RMS reason code `701`, type `6`, in the selected store database instead of assuming a shared numeric ID. Davao resolves it to ID `39`; Gensan resolves it to ID `33`. The resolved reason is cleared when the order returns to taxable status.
- Fractional quantities are preserved in the corrected order-update and inventory-commitment method signatures and calculations.
- New order/quotation saving now uses one connection and local SQL transaction for the header, lines, inventory commitment, price logs, tax reason, and same-database queue records.
- Existing-order updates now use one connection and local SQL transaction for header changes, retained/inserted/deleted lines, inventory commitment differences, price logs, tax reasons, and same-database queue records. Success is displayed only after commit.
- Inventory commitment changes now execute as atomic SQL differences (`QuantityCommitted = QuantityCommitted + difference`). Concurrent users working with the same item no longer overwrite each other's committed total, and an adjustment that would make the total negative is rejected.
- Updates and cancellations acquire a transaction-owned SQL application lock for the order ID. Two users cannot update/cancel the same order concurrently; the second operation receives a recall-and-retry message instead of creating duplicate queue or commitment changes.
- Work Order cancellation now releases all committed quantities, zeros the lines, closes the order, and closes its queue records inside one transaction.
- Quotations retain their recall-ownership queue header, remove processing queue items, and do not reserve inventory.
- Imported item-code lookup uses SQL parameters instead of concatenating imported values into an `IN` clause.
- Login explicitly tests the selected database before running authentication queries, and database/query failures are handled by one login error boundary.
- Calculation and inventory-update failures are no longer silently treated as success in the corrected paths.
- Draft recovery is optional. If `SOD_WO_DraftHeader` or `SOD_WO_DraftDetail` is missing, draft save/recovery is skipped without blocking Work Order processing. The idempotent manual migration enables the feature when deployed by an authorized database administrator.
- A read-only DVO/GSC database audit now produces an application-scoped deployment package under `Database Components\Deployment`. Its 49-object manifest separates 26 identical definitions, 9 branch-specific definition pairs, 9 DVO-only definitions, and 5 objects missing from both audited databases. Tables, views, functions, procedures, and triggers are individual guarded SQL files. The inventory includes the transitive `SOD_Reg`, `SOD_fn_GetStockInQty`, and `SOD_fntbl_sumStockin` dependencies required by other exported functions.
- The current DVO database passes the package's required-object validation. Seven reviewed portable, local-database definitions now cover the corresponding GSC gaps: `SOD_WO_LOGS`, `SOD_ViewItemsWO`, `SOD_fn_GetAvailableQty`, `SOD_fn_GetConvertQty`, `SOD_fn_GetCreditLimit`, `SOD_fn_GetTotalOpenWorkOrder`, and `SOD_fntbl_CheckOpenWO`. The GSC driver includes them; the live GSC database remains unchanged until DBA testing and deployment.
- A database-free MSTest project now provides 18 readable Given/When/Then unit tests for price-level mapping, initial top-row selection, multiple-item price confirmation, branch-database matching, senior-citizen tagging, taxable VAT separation, tax exemption, and non-taxable pricing. `Run-UnitTests.ps1` builds and runs the suite without connecting to an RMS database.
- Branch selection is now deployment-controlled. With `BranchSelectionEnabled=false`, startup skips `frmStoreSetup`, uses the local RMS registry connection, and rejects a database that does not match `ExpectedDatabaseName`. The default project configuration is locked to `DVO_STORE_DB`; each other branch must deploy its matching expected database name.

Verification command:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' '.\Work Order Entry.sln' /t:Build /p:Configuration=Debug /m /v:minimal
```

Verification result: `Work Order Entry_V4.exe` built successfully with MSBuild 18.8.2.

Remaining before production rollout:

- Confirm each branch's four processing settings and deploy its matching configuration file.
- Run `Ensure_Queueing_Compatibility_Tables.sql` and verify both legacy Work Order insert procedures in every branch database.
- Confirm that RMS reason code `701`, type `6`, exists exactly once in every branch database before deployment.
- Test the seven portable GSC prerequisites on a disposable GSC restore. Confirm whether Gensan credit exposure and open Work Orders are local-only; if they span warehouse databases, replace the two local aggregate functions with approved GSC overrides.
- Coordinate RMS database changes for any fractional fields still defined as SQL `int`, particularly custom functions and queue prepared quantity.
- Remove tracked database secrets and replace plaintext application-user password comparison with a staged password-hash migration.
- Add integration tests against a disposable RMS-compatible database before deployment.

## 1. Objective

Create one maintainable application workflow that supports stores with different processing requirements without maintaining separate source-code versions.

The target design must support:

- Stores that save work orders without using Order Processing Queueing.
- Stores that use picking, prepared quantities, grouping, and the For Invoicing queue.
- Stores that keep queue tables in the RMS database.
- Stores that keep queue tables in a separate database on the same store SQL Server.
- The same RMS schema at every store, while each store installation connects only to its own server.
- Atomic order saving, accurate inventory commitments, correct tax and price handling, and auditable approvals.

## 2. Confirmed design direction

### 2.1 One codebase, configurable per store

Do not create separate Davao, Cebu, Manila, queue, and non-queue application branches. Both applications must load a store-processing profile after the store is selected.

Recommended settings:

| Setting | Purpose |
|---|---|
| `StoreCode` | Stable identifier for the store. |
| `UseQueueing` | Enables creation and processing of queue records. |
| `QueueStorageMode` | `Disabled`, `SameRmsDatabase`, or `SeparateDatabaseSameServer`. |
| `QueueDatabaseName` | Queue database when it is separate from RMS. |
| `CommitInventory` | Controls whether a work order reserves RMS inventory. |
| `RequirePicker` | Requires picker assignment before preparation. |
| `AllowGrouping` | Enables grouping of compatible customer work orders. |
| `DefaultPickLocation` | Usually `STORE` or `UP-STORE`. |

`UseQueueing` and `CommitInventory` must remain separate decisions. A store may reserve inventory without using the queueing application.

### 2.2 Local store processing only

Each installation connects to its own store server. This plan does not require:

- Direct writes from one store server to another store server.
- Cross-store order fulfillment.
- Outbox/inbox messaging between stores.
- MSDTC transactions between store servers.

If cross-store fulfillment becomes a real requirement later, it should be planned as a separate integration project.

### 2.3 Intended workflows

#### Direct-processing store

```text
Login and select store
  -> Create or recall work order
  -> Apply customer, pricing, tax and credit rules
  -> Save order and entries
  -> Optionally commit inventory
  -> Print work order
  -> Complete invoicing through the store's normal RMS process
```

#### Queue-enabled store

```text
Login and select store
  -> Create or recall work order
  -> Apply customer, pricing, tax and credit rules
  -> Save order, entries and inventory commitment
  -> Create queue and queue-item records
  -> Assign picker
  -> Processing
  -> Enter prepared quantity
  -> Prepared
  -> Optionally group compatible work orders
  -> For Invoicing
  -> Complete RMS transaction
  -> Close queue
```

#### Sales quotation

```text
Create quotation
  -> Save and print quotation
  -> Do not commit inventory
  -> Do not create queue records
  -> Convert to work order only after customer acceptance
```

## 3. Phase 0: Store discovery and business confirmation

Before changing code, create and approve a store matrix.

| Store | RMS server | RMS database | Queueing | Queue storage | Commit inventory | Grouping | Pick locations |
|---|---|---|---:|---|---:|---:|---|
| Davao | To confirm | `DVO_STORE_DB` | To confirm | To confirm | To confirm | To confirm | To confirm |
| Other stores | To confirm | To confirm | To confirm | To confirm | To confirm | To confirm | To confirm |

For each store, confirm:

1. Whether Order Processing Queueing is used.
2. Whether open work orders reserve `Item.QuantityCommitted`.
3. Whether queue tables are in RMS or a separate database.
4. Whether the separate queue database is on the same SQL Server instance.
5. Whether grouping is operationally required.
6. Who may approve price overrides, tax changes, picker changes, and cancellation.
7. How a completed RMS transaction identifies the originating work order.

**Exit criterion:** A business owner and technical owner approve the store matrix.

## 4. Phase 1: Store-processing configuration

### 4.0 Implemented interim profile

The current unified build reads these settings from `App.config`:

| Setting | Meaning |
|---|---|
| `QueueingEnabled` | Enables Gensan-style queue-item processing at this branch. It does not control recall ownership headers. |
| `QueueingDefaultForNewWorkOrders` | Legacy compatibility setting; synchronized with the branch queue setting. |
| `AllowPerOrderQueueChoice` | Legacy compatibility setting; the unified screen now sets this to `false`. |
| `AllowOrderGrouping` | Enables the existing customer Work Order grouping prompt for queued orders. |
| `ShowForInvoiceButton` | Controls whether `cmdForInvoice` is visible at the deployed branch. It also requires queueing to be enabled. |
| `ShowImportButton` | Controls whether the main `Import [F6]` button and its F6 action are available at the deployed branch. |

Authorized users can configure **Enable Branch Queue**, **Allow order
grouping**, and **Show For Invoicing button** from **Settings → Branch
Functions**. The **Show Import button** option independently controls
`Import [F6]`. Values are written to the deployed executable configuration and
applied to the main screen immediately. The installation folder must allow the
application to update its configuration file.

Deployment examples:

| Branch mode | QueueingEnabled | Grouping | For Invoice button |
|---|---:|---:|---:|
| Gensan processing queue | `true` | `true` | `true` |
| Davao recall ownership only | `false` | `false` | `false` |

All three modes preserve the current inventory rule: type-2 Work Orders commit
inventory and type-3 Sales Quotations do not. Queue choice does not change that
rule.

The Davao `SOD_sp_InsertQoute` and `SOD_sp_InsertQouteEntry` procedures contain
unconditional queue writes, while Gensan restricts those writes to type-2 Work
Orders. Every branch must have the two compatibility tables because queue
headers now provide recall ownership for both Work Orders and Sales Quotations.
When processing is disabled, the application removes queue-item rows but keeps
the ownership header. Run
`Database Components/Migrations/Ensure_Queueing_Compatibility_Tables.sql`
before deploying this build.

### 4.1 Add a configuration model

Create a `StoreProcessingProfile` class shared conceptually by both applications:

```vb
Public Class StoreProcessingProfile
    Public Property StoreCode As String
    Public Property UseQueueing As Boolean
    Public Property QueueStorageMode As QueueStorageMode
    Public Property QueueDatabaseName As String
    Public Property CommitInventory As Boolean
    Public Property RequirePicker As Boolean
    Public Property AllowGrouping As Boolean
    Public Property DefaultPickLocation As String
End Class
```

Validate combinations during startup. Examples:

- `UseQueueing = False` requires `QueueStorageMode = Disabled`.
- `AllowGrouping = True` requires `UseQueueing = True`.
- A separate queue database requires a validated database name.

### 4.2 Store the configuration safely

Preferred: store processing settings in a customization database, following the RMS customization guide. As an interim deployment option, store non-secret settings in protected application configuration per installation.

Do not store plaintext SQL passwords in the processing-profile table or source code. Store connection secrets through an approved protected configuration mechanism.

### 4.3 Apply the profile to the user interface

For direct-processing stores, hide or disable:

- Picker selection.
- Prepared quantity.
- Pick-location controls when not required.
- Order grouping.
- For Invoicing queue actions.
- Queue-specific print functions.

For queue-enabled stores, enable only the features permitted by the profile.

**Exit criterion:** Both applications load and display the correct profile for a test store without changing source code.

## 5. Phase 2: Separate order processing from queue processing

### 5.1 Introduce application services

Move business operations out of form event handlers into services:

```text
OrderService
  SaveQuotation
  SaveWorkOrder
  UpdateOrder
  ConvertQuotationToWorkOrder
  CancelOrder

PricingService
  ApplyCustomerPricing
  ValidateMinimumPrice
  RecordPriceApproval

InventoryCommitmentService
  AddCommitment
  ApplyQuantityDifference
  ReleaseCommitment

QueueService
  CreateQueue
  UpdatePreparation
  GroupOrders
  MarkForInvoicing
  CloseQueue
```

Forms should collect input, display validation, call a service, and show the final result. They should not coordinate separate database writes themselves.

### 5.2 Add queue repository implementations

Use one interface with store-specific implementations:

```vb
Public Interface IQueueRepository
    Sub CreateForOrder(orderId As Integer, lines As IEnumerable(Of OrderLine), transaction As SqlTransaction)
    Sub GroupOrders(orderIds As IEnumerable(Of Integer), groupId As Integer, transaction As SqlTransaction)
    Sub UpdateStatus(groupId As Integer, status As String, transaction As SqlTransaction)
End Interface
```

Implement:

- `NoQueueRepository` for direct-processing stores.
- `SameDatabaseQueueRepository` when queue tables are in RMS.
- `SeparateDatabaseQueueRepository` when the queue database is on the same SQL Server.

The application must not determine queue behavior merely by checking whether a `Queueing` table exists.

### 5.3 Make recall independent of queueing

Work-order ownership and recall must not depend on `Queueing.OPIS`, because direct-processing stores may not have queue rows.

Add custom order metadata containing at least:

```text
OrderID
StoreCode
CreatedBy
CreatedAt
RegisterID
ProcessingMode
```

Recall should use the order and metadata records. Queue tables should contain fulfillment state only.

**Exit criterion:** A work order can be saved, recalled, updated, printed, and cancelled with queueing disabled.

## 6. Phase 3: Transactional order lifecycle

### 6.1 Save atomically

The following changes must succeed or fail together:

- Order header.
- Order entries.
- Inventory commitment.
- Optional queue header and items.
- Tax change reason.
- Price-approval log.
- Order metadata.

For one RMS database, use one connection and `SqlTransaction`.

For RMS and queue databases on the same SQL Server instance, use one connection and fully qualified database names so the same local SQL transaction covers both databases.

Do not use MSDTC unless a future requirement explicitly needs one operation to update separate SQL Server instances.

### 6.2 Propagate errors

Lower-level methods must stop swallowing exceptions. They should return a clear failure result or rethrow to the transaction boundary. A success message must be displayed only after commit.

### 6.3 Make retries safe

After existing duplicate data is cleaned, enforce one queue per order:

```sql
CREATE UNIQUE INDEX UX_Queueing_OrderID
ON dbo.Queueing(OrderID);
```

Queue creation must first locate an existing record and must tolerate a safe retry without creating duplicates.

**Exit criterion:** A forced failure at every save stage rolls back all order, queue, tax, log, and commitment changes.

## 7. Phase 4: Correct existing business-rule defects

### 7.1 Quotation isolation

- Restore the type check currently commented out in the quotation insert procedures, or remove queue writes from those procedures entirely.
- Type-3 quotations must never create queue records or commit inventory.
- Conversion to type-2 work order creates the queue only when `UseQueueing = True` and commits inventory only when `CommitInventory = True`.

### 7.2 Customer and pricing changes

When a customer changes:

1. Load the customer's price level, discount, tax status, salesperson, and credit information.
2. Clear each existing line price to `"-"` and require the operator to select/confirm a permitted price for every line.
3. Recalculate subtotal, VAT, total, and credit warning as prices are confirmed; do not allow saving while any price is unconfirmed.
4. Clear approvals that are no longer valid and require new approval where necessary.

Correct `frmPriceLevel` so it selects the customer's configured price rather than defaulting to Retail.

### 7.3 Tax transitions

- Applying manual exemption resolves reason code `701`, type `6`, in the selected branch and stores that database-specific reason ID on the order and entries.
- Removing manual exemption clears the old reason code and restores line taxability.
- Header tax, line taxability, reason codes, and totals must be updated in the same transaction.
- Saving a manual exemption must fail safely when code `701`, type `6`, is missing or duplicated.

### 7.4 Fractional quantities

Use `Decimal` or the RMS-compatible non-integer type consistently for:

- Ordered quantity.
- Prepared quantity.
- Committed quantity adjustments.
- Method parameters and local variables.
- LINQ mappings and SQL function/procedure parameters.
- Reports.

Do not narrow RMS quantities to `Integer` in either project.

### 7.5 Inventory commitments

Centralize calculations:

```text
New work order:       committed += ordered quantity
Edit work order:      committed += new quantity - old quantity
Remove line:          committed -= outstanding old quantity
Convert quotation:    committed += ordered quantity once
Cancel work order:    committed -= outstanding quantity
```

Preserve original cancelled order quantities for audit instead of zeroing the order lines. Prevent commitments from becoming negative without raising an actionable error.

### 7.6 Imports and credentials

- Parameterize all SQL used by imports.
- Deploy or remove the missing quotation-import log feature.
- Remove plaintext connection and application passwords from source-controlled files.
- Replace plaintext Work Order Entry and picker password comparison with an approved password-hashing and migration process.

**Exit criterion:** Automated tests cover the corrected rules and no known high-severity finding remains unaddressed without an approved exception.

## 8. Phase 5: Restore order grouping safely

The grouping implementation exists, but its normal calls are commented out. Do not merely uncomment them.

### 8.1 Enable by configuration

```vb
If profile.UseQueueing AndAlso profile.AllowGrouping Then
    OfferCompatibleOrderGrouping(customerId, orderId)
End If
```

### 8.2 Define compatible orders

Only group orders with compatible:

- Customer.
- Store and queue database.
- Work-order type.
- Open/not-invoiced state.
- Release method and operational pick rules.

Exclude quotations, cancelled orders, closed orders, and orders already being invoiced.

### 8.3 Update groups atomically

- Select the group before committing the final grouping update.
- Update every selected `Queueing.GroupTo` in one transaction.
- Ungroup by restoring `GroupTo = OrderID`.
- Record who grouped or ungrouped the orders and when.

### 8.4 Group readiness rule

A group may become `For Invoicing` only when every required queue item in every order in the group is prepared.

**Exit criterion:** Group, partial preparation, full preparation, invoicing handoff, and ungroup scenarios work without duplicate or lost queue records.

## 9. Phase 6: Align ORDER PROCESSING QUEUEING

### 9.1 Startup validation

The Queueing application must:

- Load the same store profile as Work Order Entry.
- Refuse to operate when `UseQueueing = False`.
- Connect to the configured queue storage location.
- Check required database objects and schema version before showing orders.

### 9.2 Standardize statuses

Replace scattered string literals with one shared set of permitted states:

```text
For Process
Processing
Prepared
For Invoicing
Completed
Cancelled
```

Document allowed transitions and reject invalid transitions.

### 9.3 Picker and preparation controls

- Assigning a picker moves an eligible item to `Processing`.
- Prepared quantity cannot be negative or exceed outstanding ordered quantity.
- A fully prepared item moves to `Prepared`.
- Changing an assigned picker requires authorization and an audit entry.
- Prepared quantities must support the same numeric precision as RMS order quantities.

### 9.4 Invoice completion

Verify the `Transaction` trigger against live RMS transaction behavior and multi-row inserts. Rewrite it as a set-based trigger if retained. It must close only the queue associated with the correct recalled order.

**Exit criterion:** Queue status accurately follows picker actions through RMS invoice completion.

## 10. Phase 7: Database remediation and migration

Perform all data changes first on a restored non-production copy.

1. Back up each target database.
2. Identify and resolve duplicate `Queueing` rows and their queue items.
3. Reconcile `Item.QuantityCommitted` with approved open-work-order rules.
4. Review conflicting tax-reason orders with the business owner.
5. Correct narrowed database functions and stale LINQ mappings.
6. Deploy required custom tables, indexes, procedures, and configuration.
7. Add schema-version tracking.
8. Re-run integrity checks.

Do not automatically correct live tax or order records without business validation.

**Exit criterion:** No queue duplicates, unexplained commitment differences, missing required objects, or incompatible schema versions remain for the pilot store.

## 11. Phase 8: Verification

### 11.1 Build and automated tests

- Build both solutions in a clean environment.
- Unit-test price, tax, quantity, commitment, and status-transition rules.
- Integration-test transaction rollback and both queue storage modes.
- Verify that quotation saves never touch queue or commitment data.

### 11.2 Workflow test matrix

Test at minimum:

| Scenario | Direct store | Queue store |
|---|---:|---:|
| New work order | Required | Required |
| Sales quotation | Required | Required |
| Convert quotation | Required | Required |
| Change customer | Required | Required |
| Tax exempt and reverse exemption | Required | Required |
| Price override | Required | Required |
| Fractional quantity | Required | Required |
| Recall and edit | Required | Required |
| Cancel order | Required | Required |
| Draft recovery | Required | Required |
| Picker/preparation | Not shown | Required |
| Group/ungroup | Not shown | Required when enabled |
| For Invoicing and RMS completion | Normal RMS route | Queue route |
| Forced database failure | Full rollback | Full rollback |

### 11.3 Reconciliation checks

After each test run, verify:

- One order header and the expected entry count.
- At most one queue header per queued order.
- One recall-ownership queue header for each Work Order and Sales Quotation.
- Queue-item data only where branch processing requires it.
- Correct committed-quantity difference.
- Consistent tax flags and reason codes.
- Complete approval and cancellation audit history.

## 12. Phase 9: Deployment

1. Select one direct-processing pilot store and one queue-enabled pilot store.
2. Confirm and deploy each store profile.
3. Back up databases and record pre-deployment reconciliation totals.
4. Apply versioned database migrations.
5. Deploy both applications where applicable.
6. Run smoke tests using controlled test orders.
7. Monitor save errors, queue duplicates, commitment differences, and stuck statuses.
8. Obtain operational sign-off before expanding to other stores.
9. Deploy store by store, using the approved matrix rather than copying one store's configuration.

Rollback must restore the previous application and compatible database objects without deleting newly created business orders. Database rollback scripts must be tested before production deployment.

## 13. Recommended implementation order

1. Approve the store matrix and business rules.
2. Add store-processing profiles and startup validation.
3. Make recall independent of queue records.
4. Introduce the transactional order service.
5. Make inventory commitment and queue creation conditional.
6. Isolate quotations from fulfillment.
7. Correct fractional quantities, tax transitions, and customer repricing.
8. Clean duplicate queues and reconcile committed quantities.
9. Align the Queueing application and status transitions.
10. Restore grouping behind `AllowGrouping`.
11. Complete security and import remediation.
12. Pilot, reconcile, and deploy incrementally.

## 14. Definition of done

The work is complete when:

- The same build supports direct and queue-enabled stores through configuration.
- Each installation connects only to its own approved store server.
- Saving is atomic and does not report success after a partial failure.
- Quotations never reserve stock or enter queueing.
- Work-order inventory commitments remain accurate after create, edit, conversion, removal, and cancellation.
- Fractional quantities survive all workflows without rounding.
- Customer pricing and tax changes recalculate consistently.
- Direct-mode orders can be recalled without queue rows.
- Queue-enabled orders move through controlled, auditable statuses.
- Grouping is enabled only for compatible stores and orders.
- Database constraints prevent duplicate queue headers.
- Both pilot-store workflows pass the test matrix and reconciliation checks.

## 15. Related review documents

- `SOURCE_CODE_REVIEW_FINDINGS.md`
- `DVO_STORE_DB_AUDIT.md`
- `.codex-temp/rms-customization-guide`
- `Database Components/Migrations/Export_Application_Function_Migration.sql`
