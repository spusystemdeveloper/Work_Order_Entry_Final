# Project Rules & Persistent Knowledge

## Target Database Environment
- **Database Engine**: Microsoft SQL Server 2016 is used across store instances (e.g., `GSC_STORE_DB`, `DVO_STORE_DB`).
- **Microsoft Dynamics RMS Schema Conventions**:
  - Legacy datatypes are present in RMS tables: `Item.ExtendedDescription` is `ntext`. In T-SQL, always wrap with `CONVERT(nvarchar(max), ExtendedDescription)` when concatenating with `+` or passing to string functions.
  - Reserved keywords: Column `[LineNo]` is a reserved T-SQL compiler keyword (`LINENO`). Always delimit with brackets: `[LineNo]`.
  - Reserved column names: Wrap `[Date]` and `[timestamp]` in square brackets in audit tables like `dbo.SOD_WO_LOGS`.
  - All migration scripts and custom views/functions must execute safely on SQL Server 2016 without data type mismatches.

## Development & Workflow Constraints
- **Keep it Simple**: Do not overengineer solutions.
- **Changelog Integrity**: After each change or set of changes, encode the details into `CHANGELOG.md`.
- **Validation**: Ensure both `Work Order Entry.sln` and `OrderProcessingQueueing.sln` compile cleanly and all unit tests in `.\Run-UnitTests.ps1` pass before finishing.

## Order Processing Queueing Business Rules
- **Queue Tile Stages**: Orders in `OrderProcessingQueueing` remain active on screen through all 4 color stages:
  1. `OldLace` (For Process / Pending)
  2. `DarkSalmon` (Processing / In Process)
  3. `PaleGreen` (Prepared)
  4. `CornflowerBlue` (For Invoicing)
- **Do Not Filter "For Invoicing" Orders**: Orders marked "For Invoicing" are still active and must display on screen with the CornflowerBlue card and be counted in the bottom "For Invoice" panel (`lblForInvoice`). NEVER exclude `Status = 'For Invoicing'` in `LoadOpenWO` or `LoadOpenWOUpdate`. Orders only exit the queue when tendered in RMS POS (where trigger `trg_Transaction_closeQueue` sets `Queueing.Status = 1`).

