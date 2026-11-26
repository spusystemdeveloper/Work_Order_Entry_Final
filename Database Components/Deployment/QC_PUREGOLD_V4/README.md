# QC Puregold: selected Version 4 compatibility package

Prepared October 31, 2026. **Prepared and fixture-tested; not applied to live QC.**
Target: `PUREGOLD_DB` on QC. SQL Server 2016 compatible. SQL files are imported
through Database Updater, which owns the transaction. They reject execution
without an active transaction and reject a different database name. No scripts
are added to the updater's default catalog or selected automatically.

## Exact EXE checked

`C:\Users\ADMIN\source\repos\WORK_ORDER_ENTRY_4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe`

- SHA-256: `D4572A7B1C3B0671824498E058547AF88FE340B59E5535426A89068D36003CAA`
- 2,652,672 bytes; assembly version 1.0.0.0; .NET Framework 4.7.2; x86.
- `bin\Release\EXE-Only` should be verified against this hash before use.
- Assembly mappings and IL were inspected without starting the app. The binary
  contains direct `SELECT * FROM SOD_ViewItems` search/import queries using
  `FULLDESC`; source grid selection uses ordinal columns 0/3/6/9 for item
  code/cost/tax/item ID. Existing V4 configuration must remain on the target PC.

This selected build differs from the older Documents-folder V4 used in the first
audit. That earlier conclusion about missing decimal printing functions does
**not** apply here. This EXE uses `SOD_fntbl_WoDetails` and `SOD_fntbl_WoEntry`,
which already exist on QC. Both were successfully read through the compiled
EXE's mappings in the synthetic fixture. It does not require the newer V8
username, credit, history or draft additions.

## Changes prepared

| File | Purpose | Selection |
| --- | --- | --- |
| `01_SOD_ViewItems.sql` | Move the extra leading `PG SKU` field to the end; restore the 17 original V4 column positions | Main compatibility fix |
| `02_SOD_fntbl_CheckQty.sql` | Replace unavailable GSC lookup targets with the two QC warehouse entries in Puregold's store directory | Select for the Check Other Stores action; confirm both warehouses are intended |
| `03_Optional_QuotationImport.sql` | Create the missing five-column `SOD_ImportedQuote` tracking table | Select only if quotation import is used |

The view preserves every original output name, including `PG SKU`, `FULLDESC`
and `PRICE 1`. It preserves QC's description concatenation and available-stock
CASE expression; `ntext` is explicitly converted before concatenation. No Item
columns are removed and no prices, tax settings, stock or order rows are changed.
`PG SKU` will become an appended column and can be visible at the right edge of
V4's auto-generated grid, whose original code hides only columns 3 and 5-16.
Test other clients that consume `SELECT *` by position before shared deployment.

The stock function still returns `Quantity - QuantityCommitted` and retains the
existing NULL result when an item is absent. Physical and committed stock are not
updated. It excludes the current store and explicitly selects `QC_STORE_DB`
(directory ID 3001) and `QC_RDC_DB` (ID 3012). The script checks those directory
entries and current StoreID 100; it does not rewrite them. This is an intentional
change to the displayed warehouse targets, based on the directory read from QC.

View/stock scripts preserve existing object IDs and permissions with ALTER, accept
only the reviewed original or their own output, and repeat safely. Fingerprints
ignore CR/LF transport differences, retaining all other definition checks. A
changed or encrypted definition stops for review. The optional table repeats
only for its expected schema and does not populate historical import records.

## Findings that do not justify additional migrations for this EXE

- Missing `Pass2Username`/`Pass3Username`: not in this EXE's nine-column mapping.
- Missing `_Decimal` printing functions / header `Address`: not requested by this
  EXE's standard printing mappings. Existing print formatting is retained.
- The one-argument `SOD_fntbl_NestedSearchItem` conflicts with a legacy helper's
  three-argument mapping, but source tracing found no callers of that helper in
  normal V4 search. Compiled search uses the view. The package does not replace
  a working QC cache function to satisfy an unused legacy mapping.
- `SOD_fntbl_SearchItem.SKULevel`, `SOD_ViewItems.FULLDESCRIPTION`, missing
  GetAvailableQty/GetConvertQty and invalid `CheckExQty` remain legacy mapped
  differences. They were not established as active paths for this selected EXE.
- No extra unmapped columns were found on Order or OrderEntry. Existing Item
  FullDesc/DescReversed and configuration Database2 are preserved.

## Review and deployment procedure

1. Verify the EXE hash and retain the previous EXE and the unit's config.
2. Take a verified database backup and restore it on a disposable test server,
   retaining database name `PUREGOLD_DB`. For script 02, restore or provide owned
   copies of the intended QC warehouse databases too; do not point a write test
   at production. A backup or restore has not been taken by this task.
3. Use Database Updater on the restore. Check the exact target, add only the
   numbered SQL files needed, and review them in 01/02/03 order. Do not add
   Rollback files in the same selection. The updater supplies the transaction;
   the files contain no BEGIN/COMMIT transaction commands or SQLCMD directives.
4. Test V4 search (including a PG SKU), item selection, code/ID/cost/tax, price
   selection, new WO/quotation, recall/edit, approvals, warehouse lookup, queue,
   import if enabled, and printed preview. Verify original quantities/totals.
   Test physical printing and native RMS settlement separately. This package
   does not modernize V4's older separate-write save behavior.
5. Obtain explicit authorization for live application to QC after restore
   acceptance. Deployment and live write/rollback tests are not authorized by
   preparing this package. Use a maintenance period with affected clients closed.

If rollback is needed, import `Rollback_02_...` then `Rollback_01_...` through
the updater, selecting only changes actually applied. They reject unexpected
definitions and restore the reviewed original definitions. This restores the
old GSC references and leading PG SKU layout, including their known limitations.
No automatic DROP of import tracking is provided: preserve any records created
after deployment and review separately. A changed live definition stops rollback.

## Validation and evidence

`Database Components/Tests/QC_Puregold_V4_LocalDB_Test.ps1` creates a uniquely owned
LocalDB instance with synthetic databases at compatibility level 130, then drops
its databases and deletes its instance. It never connects to QC. It requires the
selected EXE and matching source DBML (both read only), Windows x86 PowerShell,
SQL LocalDB 170 and .NET Framework 4.7.2. Baseline files here are read-only exported
module definitions used only to construct fixtures; never import those files.

The 76 checks cover the original ordinal regression, all existing view values,
fractional/negative stock rules, permissions/object identity, transaction and
target guards, repeat deployment, definition rollback, changed-definition/schema
rejection, optional import-table creation and actual compiled V4 configuration
and print-function materialization. These are synthetic checks, not a restored
QC integration run, a full visible V4 session, a physical print, or RMS posting.

Evidence: `TestResults/QC-V4-Release-{identity,columns,functions,calls,search-strings}`,
`TestResults/QC-V4-LiveDefinitions`, and `TestResults/QC-V4-LatestTest.log`.

The final fixture run passed 76 checks (`QC_V4_168dc4b764e2`). Both current
repository Release/x86 solutions built, all 153 main unit tests passed, and the
existing clean-folder packaging suite passed. Five pre-existing EXE configs were
restored byte-for-byte. Those broader checks cover this repository's current
application; they do not substitute for testing the selected V4 EXE on a QC
restore. The selected V4 EXE and its EXE-Only copy have identical reviewed hashes
and were not rebuilt or changed.
