# DVO_STORE_DB Read-Only Audit

**Audit date:** July 23, 2026  
**Database:** `DVO_STORE_DB`  
**Audit mode:** Read-only metadata and aggregate consistency checks  
**Credential storage:** `.codex-temp/DVO_STORE_DB.connection.txt` (excluded by `.gitignore`)

The connection password is intentionally not reproduced in this report.

## Environment

| Property | Observed value |
|---|---|
| Connection | Successful |
| Database state | `ONLINE` |
| User access | `MULTI_USER` |
| Read-only | No |
| SQL Server product version | `13.0.1601.5` |
| SQL Server edition | Enterprise Edition, 64-bit |
| Database compatibility level | `100` |
| User tables | 222 |
| Views | 82 |
| Stored procedures | 60 |
| Scalar functions | 53 |
| Table-valued functions | 47 |
| Triggers | 84 |

## Compatibility summary

- All 16 functions and stored procedures mapped by `ItemLookUp.dbml` exist.
- 25 of 28 mapped tables/views exist after normalizing bracketed RMS names such as `[Order]` and `[Transaction]`.
- Core objects used by order entry are present: `Customer`, `Item`, `Order`, `OrderEntry`, `Configuration`, `ReasonCode`, `Queueing`, and `QueueingItems`.
- Custom draft tables and their detail-to-header foreign key are present.
- Tax reason ID `39` exists as code `701`, description `No Tax Declaration`, type `6`.
- RMS quantity columns confirm the source-review finding: both `OrderEntry.QuantityOnOrder` and `Item.QuantityCommitted` are SQL `float`/VB `Double` fields.

### Cross-branch tax-reason check (July 27, 2026)

The same business reason does not have the same numeric ID in every store:

| Database | ID | Code | Description | Type |
|---|---:|---:|---|---:|
| `DVO_STORE_DB` | `39` | `701` | `No Tax Declaration` | `6` |
| `GSC_STORE_DB` | `33` | `701` | `No Tax Declaration` | `6` |
| `GSC_STORE_DB` | `39` | `512` | `Pull-out items from customer` | `7` |

The application now resolves code `701`, type `6`, in the selected store
database. It no longer assumes that ID `39` represents manual tax exemption.

## Findings

### 1. High: Existing reason-code-39 orders have conflicting tax data

Aggregate checks found:

| Check | Count |
|---|---:|
| Orders with reason `39` and positive order tax | 9 |
| Orders with reason `39` but taxable entries | 9 |
| Orders with reason `39` whose entries do not all carry reason `39` | 3 |

Reason `39` represents `No Tax Declaration`, but these orders contain tax or taxable lines. This is consistent with the source-code finding that manual tax exemption is written in one direction and is not reliably cleared or synchronized.

**Recommended action:** Back up the database, identify the affected orders with the business owner, determine the intended tax state from source documents, and correct the order header and all entries together. Do not automatically rewrite these records without business validation.

### 2. High: Eleven orders have duplicate queue rows

The audit found 11 distinct `OrderID` values with more than one row in `Queueing`. The table has a primary key on `id`, but no unique index on `OrderID`.

Several source paths assume a single queue row and use `SingleOrDefault`, including:

- `Work Order Entry/Class/clsPickList.vb`
- `Work Order Entry/Class/clsCustomer.vb`
- `Work Order Entry/Class/clsRecall.vb`

Those paths can throw `Sequence contains more than one element`, select an arbitrary queue, or update the wrong queue items.

**Recommended action:** Inspect the 11 orders and associated `QueueingItems`, decide which queue row is authoritative, and merge/remove duplicates in a controlled migration. After cleanup, add an appropriate uniqueness rule if one queue row per order is the intended business invariant.

### 3. High: Twenty-eight item commitment totals do not match open work orders

Using the application's apparent rule—`Item.QuantityCommitted` should equal the sum of `OrderEntry.QuantityOnOrder` for open type-2 orders—28 item records differ from the calculated total.

The database contains disabled quantity-offset triggers on `OrderEntry`, while the application manually updates `Item.QuantityCommitted` through several separate code paths. Integer conversions and partial saves can therefore leave commitment values out of sync.

**Recommended action:** Validate the commitment formula against the store's RMS workflow, then reconcile the 28 items using a reviewed maintenance script or the existing `SOD_sp_FixCommitted` procedure after its definition and effects have been verified.

### 4. High: The quotation-import log table is missing

The LINQ model maps `dbo.SOD_ImportedQuote`, and active code in `clsImportQuotation.vb` and `frmUploader.vb` queries and inserts that table. It does not exist in `DVO_STORE_DB`.

The quotation uploader's duplicate-check/logging path will fail when executed.

Two other mapped tables are also absent:

- `dbo.WorkOrder_Logs`
- `dbo.Queueing_Quote`

No active handwritten source reference to those two tables was found; they appear to be stale generated mappings.

**Recommended action:** Add a version-controlled deployment script for `SOD_ImportedQuote` if the uploader is required. Otherwise, remove the inactive feature and stale mappings. Regenerate the DBML only after deciding which objects are supported.

### 5. High: Work-order reporting functions narrow RMS field sizes

The live `SOD_fntbl_WoEntry` function returns:

- `QtyOrder int`, although `OrderEntry.QuantityOnOrder` is `float`.
- `Itemcode varchar(10)`, although `Item.ItemLookupCode` is `nvarchar(50)`.

Fractional order quantities are rounded in report output, and item codes longer than 10 characters can be truncated. The live `SOD_fntbl_PriceLevel` function similarly accepts `varchar(30)` for an `nvarchar(50)` item lookup code.

**Recommended action:** Change function result/parameter definitions to preserve the RMS source types and lengths, then refresh the DBML result mappings and verify the RDLC reports with fractional quantities and long item codes.

### 6. Medium: Quote procedures contain unconditional queue writes

Both `SOD_sp_InsertQoute` and `SOD_sp_InsertQouteEntry` contain a commented `IF @Type = 2` followed by an unconditional `BEGIN`. As deployed, they create `Queueing` and `QueueingItems` rows for every supplied order type, including type-3 sales quotations. In the entry procedure, `@Type` has no effective behavior.

The procedures also perform header/entry and queue writes without an explicit transaction. A failure after the first insert can leave partial records.

**Recommended action:** Confirm whether quotations should enter the picking queue. Reinstate the type condition if they should not. Wrap each multi-table operation—or preferably the complete order save—in one transaction with rollback on failure.

### 7. Medium: Live draft indexes differ from the repository scripts

The repository scripts define:

- A unique `(RegisterID, UserName)` constraint on `SOD_WO_DraftHeader`.
- A `(DraftID, LineNo)` detail index.

The live database instead has:

- A non-unique `(RegisterID, UserName)` index.
- A detail index on `DraftID` only.

No duplicate register/user drafts currently exist, but the live schema does not enforce the one-draft-per-register/user assumption made by the code.

**Recommended action:** Add schema versioning and an idempotent migration that validates existing data before applying the intended unique constraint and composite detail index.

### 8. Medium: The LINQ view mapping is stale

`ItemLookUp.dbml` maps `SOD_ViewItems.FULLDESCRIPTION`, while the live view exposes `FULLDESC`. Current handwritten code primarily uses `SOD_VIEWITEMSWO`, so this appears dormant, but querying the stale mapped property would fail.

**Recommended action:** Remove unused mappings or regenerate the DBML from the supported live schema after the missing-object decisions are resolved.

## Integrity checks that passed

The audit found zero:

- `OrderEntry` rows without a matching `Order`.
- `OrderEntry` rows without a matching `Item`.
- Orders referencing a missing nonzero customer.
- Queue rows without a matching order.
- Queue-item rows without a matching queue or item.
- Nonzero order or entry tax-reason IDs missing from `ReasonCode`.
- Duplicate or blank Work Order Entry usernames.
- Duplicate draft `(RegisterID, UserName)` pairs at the time of inspection.
- Fractional quantities among currently open order entries.

There are 31 Work Order Entry users. Only aggregate counts were inspected; usernames, customer records, order details, and password values were not exported.

## Safe remediation sequence

1. Take and verify a full database backup.
2. Investigate the 9 inconsistent tax orders and 11 duplicate queue orders with business owners.
3. Validate and reconcile the 28 quantity-commitment mismatches.
4. Deploy or remove the missing quotation-import log feature.
5. Correct the work-order function field types and regenerate affected mappings.
6. Make order/queue/tax writes transactional.
7. Apply versioned draft-table index migrations.
8. Re-run this audit and application workflow tests against a non-production copy before production changes.
