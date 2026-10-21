# Project Rules & Persistent Knowledge

Read and follow [AGENTS.md](AGENTS.md) first for the shared contributor rules.
This file retains additional RMS and incident knowledge for all collaborators.
Keep project-wide workflow rules in AGENTS.md and maintain the details below
when related behavior or incident evidence changes. Branch-specific examples
below require target verification; they are not automatic deployment instructions.

## Target Database Environment
- **Database Engine**: Microsoft SQL Server 2016 is used across store instances (e.g., `GSC_STORE_DB`, `DVO_STORE_DB`).
- **Microsoft Dynamics RMS Schema Conventions**:
  - Legacy datatypes are present in RMS tables: `Item.ExtendedDescription` is `ntext`. In T-SQL, always wrap with `CONVERT(nvarchar(max), ExtendedDescription)` when concatenating with `+` or passing to string functions.
  - Reserved keywords: Column `[LineNo]` is a reserved T-SQL compiler keyword (`LINENO`). Always delimit with brackets: `[LineNo]`.
  - Reserved column names: Wrap `[Date]` and `[timestamp]` in square brackets in audit tables like `dbo.SOD_WO_LOGS`.
  - All migration scripts and custom views/functions must execute safely on SQL Server 2016 without data type mismatches.

## Development & Workflow Constraints
- **Keep it Simple**: Do not overengineer solutions.
- **Printing package**: On October 21 the user authorized adopting installed Microsoft ReportViewer 2010 (assembly 10.0.0.0) after the isolated candidate printed on the affected PC once that redistributable was installed. The main project now uses Common/WinForms version-10 references with Copy Local=False. Costura still excludes `Microsoft.ReportViewer.*`; it loads from the installed GAC, including the expression-processing AppDomain. Install the prerequisite once per PC, then distribute only the application EXE while preserving each unit's store-specific config. The older ReportViewer 15 build required five private DLLs because embedding/extraction failed; that historical package is not the current deployment contract. Run `Database Components/Tests/Packaging_CleanFolder_Test.ps1` after packaging/dependency changes.
- **Changelog Integrity**: After each change or set of changes, encode the details into `CHANGELOG.md`.
- **Validation**: Ensure both `Work Order Entry.sln` and `OrderProcessingQueueing.sln` compile cleanly and all unit tests in `.\Run-UnitTests.ps1` pass before finishing.
- **RMS identity compatibility**: RMS uses `SELECT @@IDENTITY` after inserting orders, entries and transactions. Audit triggers, including nested queue triggers, must never insert into an IDENTITY table. Use sequence-backed history EventID values. `Enable_WorkOrder_History.sql` must handle both fresh creation and approved legacy IDENTITY conversion in one transaction, preserving events/IDs/permissions; no separate updater selection is needed. Keep `Fix_WorkOrder_History_Identity.sql` only for compatibility with existing recovery scripts. Test legacy identity lookup, not just `SCOPE_IDENTITY()`.
- **Remember the October 14, 2026 incident**: The old WO history IDENTITY was a compatibility bug introduced by our history addition interacting with RMS's legacy `@@IDENTITY` lookup. It did not change RMS's identity column or real transaction number; it caused child records to use the wrong returned number. Do not describe it as a confirmed standalone RMS bug or change/reseed RMS identity columns to fix it. Preserve history with sequence-backed numbering, test direct and nested trigger paths using `HistoryIdentity_LocalDB_Test.ps1`, and handle already mislinked sales with a separate guarded recovery. Do not claim all branch-specific custom triggers are safe based only on the packaged objects reviewed.
- **HO incident reference and closure**: See [the transaction 23266 incident record](Database%20Components/Recovery/INCIDENT_2026-10-14_HO_PO_DB_23266.md). A live rollback rehearsal still requires the verified current-day copy-only/checksum backup. Do not treat synthetic test passes, a backup in progress or rehearsal output as evidence of a committed live repair; record actual commit and RMS verification before closing the incident.

## Order Processing Queueing Business Rules
- **Queue Tile Stages**: Orders in `OrderProcessingQueueing` remain active on screen through all 4 color stages:
  1. `OldLace` (For Process / Pending)
  2. `DarkSalmon` (Processing / In Process)
  3. `PaleGreen` (Prepared)
  4. `CornflowerBlue` (For Invoicing)
- **Do Not Filter "For Invoicing" Orders**: Orders marked "For Invoicing" are still active and must display on screen with the CornflowerBlue card and be counted in the bottom "For Invoice" panel (`lblForInvoice`). NEVER exclude `Status = 'For Invoicing'` in `LoadOpenWO` or `LoadOpenWOUpdate`. Orders only exit the queue when tendered in RMS POS (where trigger `trg_Transaction_closeQueue` sets `Queueing.Status = 1`).

## Microsoft Dynamics RMS Tender Configuration & Knowledge
- **Withholding Tax (WTax) & Adjustment Tender Types**:
  - `dbo.Tender.PreventOverTendering` must be set to `0` (False / unchecked) for withholding tax and adjustment tender types (e.g., `WTax 1% or 2%` [ID 17], `WTax 5%` [ID 18], `WTax 15%` [ID 41], `Municipal tax` [ID 19]).
  - **Root Cause in RMS**: In Store Operations POS (`QSRules.dll!Tender.ValidateAmount`), Receive Payment / Account Payment (`Shift+F4`) transactions have no item purchase lines (`PurchaseAmount = 0.00`). If a tender under category `Other` has `PreventOverTendering = 1`, RMS POS evaluates the payment against purchase amounts and blocks the cashier with:
    > *"Overtendering is not allowed for the following tender types: Other:WTax 1% or 2%. Adjust the payment amounts and try again."*
    even when `Total Tendered` exactly equals `Total Due` and `Change = 0.00`.
  - **Resolution**:
    ```sql
    UPDATE dbo.Tender
    SET PreventOverTendering = 0
    WHERE ID IN (17, 18, 19, 41);
    ```
    Or uncheck **"Prevent over tendering"** in RMS Store Operations Manager under `Database` → `Tender Types` → `Properties`.

