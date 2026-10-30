# Iloilo invoice header cleanup — October 29, 2026

## Authorization and cause

The user explicitly authorized deletion of SCHENKER header ID 1 and retention
of Walk-In header ID 227 for CustID 56 in live ILOILO_MAIN_WHSE_DB. For other
customers, only exact copies were authorized for removal; conflicting details
were to remain. No application source or SQL routine change was requested.

Transaction 9850 / CI-0450 belongs to Customer 56 (Walk-In). Its custom invoice
function returned two rows because SOD_InvoiceHeader contained two different
printing names for that customer. SingleOrDefault callers could not consume
those two rows. This was a customer-header conflict, not a transaction-total error.

## Safeguards and execution

- Verified the live server/database, both expected IDs/names, absence of incoming
  foreign keys and enabled DELETE triggers on the header table.
- Exported the entire 327-row header table with XML schema; read it back and
  verified every row. Saved its SHA256, schema and exact ID deletion plan.
  This is a table-level recovery export, not a full SQL Server database backup.
- Compared every stored header payload field exactly, retaining NULL/empty,
  whitespace and case distinctions. ID was excluded from duplicate identity.
  No other identical copies existed, so the sole deletion candidate was ID 1.
- Exercised the same ID-based delete in an owned GUID LocalDB fixture, including
  unchanged survivor checks, rollback restoration and successful commit.
- Under a live transaction/table lock, rechecked the complete table against the
  backup before deleting. Verified all 326 retained rows unchanged, header 227
  preserved, and transaction 9850's transaction/items/tenders unchanged.
- Committed the deletion. A subsequent read found zero duplicate CustID groups.
  The custom report query returned exactly one Walk-In row for transaction 9850.

## Result and recovery evidence

Only SOD_InvoiceHeader ID 1 was deleted; ID 227 was preserved. No other
customer/header, transaction total, VAT, item, tender, identity or SQL routine
was changed. This customer-level printing-header removal also governs other
prints that read CustID 56; it is not a transaction-specific buyer-history edit.
The user still needs to confirm preview in the actual Invoice Manager.

Evidence directory: `TestResults/IloiloHeaderCleanup-6420007f`:

- `SOD_InvoiceHeader.before.xml` — full table-level backup with schema.
- `ColumnSchema.xml`, `DeletePlan.xml`, `SOD_InvoiceHeader.after.xml`.
- `RemainingConflicts.xml`, `verification.json`, `ExecutedProcedure.ps1`.

Do not rerun the executed procedure as a generic cleanup or automatically restore
all old rows. Any recovery must first check subsequent changes and references,
then receive separate authorization for the exact restoration. No permanent
uniqueness constraint or application duplicate-prevention change was deployed.
