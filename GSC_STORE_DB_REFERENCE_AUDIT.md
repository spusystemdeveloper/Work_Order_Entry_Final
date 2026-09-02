# GSC_STORE_DB Queueing Reference Audit

**Audit date:** 2026-07-23  
**Mode:** Read-only metadata and aggregate-data inspection  
**Database:** `GSC_STORE_DB`  
**RMS StoreID:** `600`

No database objects or rows were changed during this audit.

## Tax-reason compatibility check

A read-only comparison on July 27, 2026 confirmed:

| Database | ID | Code | Description | Type |
|---|---:|---:|---|---:|
| `GSC_STORE_DB` | `33` | `701` | `No Tax Declaration` | `6` |
| `GSC_STORE_DB` | `39` | `512` | `Pull-out items from customer` | `7` |
| `DVO_STORE_DB` | `39` | `701` | `No Tax Declaration` | `6` |

Therefore, ID `39` cannot be used as a cross-branch tax-exemption constant.
The unified application now resolves reason code `701`, type `6`, in the
selected branch database. This resolves to ID `33` in Gensan and ID `39` in
Davao. No Gensan order header or order entry currently uses reason ID `39` in
the audited tax-reason fields.

## Rollback integration test

On 2026-07-23, the unified workflow was exercised against `GSC_STORE_DB`
inside one explicit SQL transaction using
`Database Components/Tests/GSC_Unified_Work_Order_Rollback_Test.sql`.

Passed checks:

- Type-2 processing Work Order created its queue header and queue item.
- Inventory commitment increased and returned to its exact original value
  during the cancellation simulation.
- A processing-disabled Work Order retained its OPIS recall header after its
  queue items were removed.
- A type-3 Sales Quotation retained an OPIS recall header, had no processing
  queue items, and did not change committed inventory.
- The transaction rolled back successfully.

Post-test verification returned zero Orders with the test comment and zero
Queueing rows with the test OPIS. No test records remained in Gensan.

## Executive finding

Gensan is an established queue-enabled branch. Its database contains an active,
high-volume queue workflow and the supporting Work Order procedures, view, and
transaction-close trigger.

The database confirms that processing behavior should be an explicit branch
setting, not inferred merely from the existence of queue tables. The
`Queueing` header also carries the OPIS recall owner, so the unified application
preserves a header for both Work Orders and Sales Quotations at every branch.
Davao also has the queue tables, view, and close trigger even though its
required processing screen workflow differs.

## Queue usage snapshot

The counts below are a point-in-time snapshot and will change during normal
operations:

| Object/status | Rows |
|---|---:|
| `Queueing` | 216,815 |
| `QueueingItems` | 870,208 |
| Queue status `0` (open) | 414 |
| Queue status `1` (completed) | 210,633 |
| Queue status `2` (cancelled) | 5,768 |

## Confirmed Gensan schema

### `Queueing`

| Column | SQL type |
|---|---|
| `id` | `bigint IDENTITY`, primary key |
| `OrderID` | `bigint NULL` |
| `Status` | `int NULL` |
| `GroupTo` | `int NULL` |
| `OPIS` | `varchar(max) NULL` |

### `QueueingItems`

| Column | SQL type |
|---|---|
| `id` | `bigint IDENTITY`, primary key |
| `QueueingID` | `bigint NULL` |
| `ItemID` | `int NULL` |
| `QtyPre` | `int NULL` |
| `Picker` | `nvarchar(50) NULL` |
| `Status` | `varchar(50) NULL` |
| `PickLoc` | `varchar(max) NULL` |

This matches the queue table shape found in Davao.

## Confirmed Gensan behavior

### Work Order insertion

`SOD_sp_InsertQoute` creates a `Queueing` header only when `@Type = 2`.

`SOD_sp_InsertQouteEntry` creates a `QueueingItems` row only when
`@Type = 2`.

Therefore:

- Type `2` Work Orders enter the Gensan queue.
- Type `3` Sales Quotations do not enter the Gensan queue.

### For Invoicing

`SOD_ViewForInvoice` returns open queue groups and reports the group as
`Prepared` only when all of its queue items are prepared. This view is the
database basis for Gensan's **For Invoicing** screen.

### Closing the queue

`trg_Transaction_closeQueue`, an `AFTER INSERT` trigger on the RMS
`Transaction` table, reads the transaction's `RecallID`. If it matches a queued
OrderID, the trigger changes the queue status to `1`.

This is how a completed RMS transaction closes the corresponding Gensan queue.

## Important Gensan/Davao difference

The Gensan insert procedures contain active `IF @Type = 2` conditions.

In the inspected Davao definitions, those two conditions are commented out.
Consequently, the Davao procedures attempt queue writes for every inserted
order type. The unified application preserves the header for recall ownership
and removes unwanted queue-item rows inside its local transaction, but the
procedure difference should still be corrected through a reviewed database
migration.

## Settings recommendation

Put the branch processing configuration on `frmSettings`, protected by the
existing Settings password.

Recommended Gensan profile:

```text
Queueing enabled                 Yes
Default new Work Order to queue  Yes
Allow Work Order grouping        Yes
Show For Invoicing button        Yes
```

Recommended Davao profile must be confirmed from the actual Davao business
process. It should not be determined from table existence because Davao also
has queue-related database objects.

The processing choice is branch-wide and belongs in `frmSettings`. The main
Work Order screen no longer displays a per-order Branch Queue checkbox.
