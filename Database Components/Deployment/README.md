# Work Order Entry database deployment package

Generated from read-only metadata exports of DVO_STORE_DB and
GSC_STORE_DB. Generation does not modify either database.

## Scope

Use `../Migrations/Enable_WorkOrder_History.sql` for history creation or updating.
It includes conversion of approved legacy IDENTITY history to a sequence while
preserving events/IDs/permissions, then creates/updates the history triggers in
one transaction. No separate identity fix step is needed. The old audit IDENTITY
could interfere with RMS's `@@IDENTITY` lookup. Customized legacy tables stop
for review. Already mislinked transactions need separate, guarded recovery:
see [the recovery guide](../Recovery/README.md). Recovery files are not automatic
branch deployments or updater catalog items.

For password-slot column alignment on an existing **HO_PO_DB** configuration
table, use the focused migration and
[HO deployment guide](../Migrations/HO_PO_PASSWORD_COLUMNS_GUIDE.md).
Select only that imported migration in the updater; a GSC/DVO table creation file
does not alter an existing table's missing columns. Branch functions require
separate compatibility review when targeting a different database name.

For an interactive way to select and preview these objects, see
[Work Order Database Updater](../../Database%20Updater/README.md).
It discovers the ordered object files from each branch driver and excludes
the driver/security commands. Selecting the correct branch and reviewing its
rules remain necessary. It also supports optional migrations and future SQL fixes.

This package contains individual SQL files for custom objects mapped or
directly referenced by Work Order Entry and Order Processing Queueing, plus
the transitive dependencies SOD_Reg, SOD_fn_GetDiscQty,
SOD_fn_GetStockInQty, and SOD_fntbl_sumStockin, and the queue/item-log
triggers. Standard RMS tables are prerequisites and are not scripted.
Unrelated SOD_ objects used by other add-ins are intentionally excluded.

## Layout

The optional item lookup index is in
`../Migrations/Improve_WorkOrder_ItemLookup.sql`, with deployment notes in
`../Migrations/WORKORDER_ITEM_LOOKUP_GUIDE.md`. It is outside the audited-object
manifest and automatic branch drivers. Run it only in the intended RMS store
database after testing, during a quiet period because index creation takes locks.

- Objects/Common: definitions identical in both audited branches.
- Objects/BranchOverrides: definitions present in both branches but
  different. Use only the matching branch version.
- Objects/SourceOnly: definitions found in only one branch. A source-only
  file is included only in the driver for the branch from which it was
  exported; it is never copied automatically to another branch.
- Comparison: object manifest, hashes, differences, and dependencies.
- Security: explicit object-level permissions, guarded by principal checks.
- Validation: read-only preflight and post-deployment checks.

## Audited result

- 49 application-scoped objects checked.
- 26 identical definitions stored under Common.
- 9 differing definitions stored as DVO/GSC branch overrides.
- 9 objects found only in DVO; the DVO driver includes them.
- 5 optional or inactive objects missing from both audited databases.
- Current DVO passes required-object validation.
- The current live GSC database is missing seven required objects. The GSC
  driver now supplies reviewed local-database portable baselines for them.

## Branch dependencies requiring review

Some exported functions intentionally reference other store or warehouse
databases. Review Comparison/DVO_DEPENDENCIES.csv and
Comparison/GSC_DEPENDENCIES.csv before deployment. In addition,
SOD_viewTableLastUpdate contains a literal GSC_STORE_DB database-name check
in the identical live definition. Identical does not mean branch-neutral.

## Safe workflow

1. Back up and verify the target RMS database.
2. Review Manifest.csv and
   Comparison/OBJECT_DIFFERENCES.md.
3. Review every BranchOverrides script for queue behavior, database names,
   StoreID, warehouse, tax, and reporting assumptions.
4. Confirm whether customer receivables and open Work Orders are local to the
   selected RMS database. The portable baseline is local-only. If a branch
   must aggregate warehouse databases, replace those two functions with a
   reviewed branch override.
5. Run Validation/00_Preflight.sql in the target database.
6. Test scripts against a disposable database restored from that branch.
7. Edit TargetDatabase in the matching SQLCMD driver and execute from this
   package directory:

       sqlcmd -S SERVER -d master -E -i .\Deploy_DVO_STORE_DB.sql

   Use an approved secure authentication method; do not put passwords in this
   repository or command history.
8. Run application create/recall/update/cancel/quotation/queue tests and
   reconcile inventory commitments before production rollout.

## Important limitation

The new order tools have separate migrations outside this original audited
package: `../Migrations/Prepare_WorkOrder_Edit_Checks.sql` adds lookup indexes,
and `../Migrations/Enable_WorkOrder_History.sql` enables the optional timeline.
They are not included in the dated 49-object manifest or branch drivers. Follow
the repository's `ORDER_TOOLS_GUIDE.md` and test them against a branch restore.

The generated table scripts create missing tables but deliberately do not
alter an existing table. Existing-table schema differences must be handled by
a separately reviewed migration. A branch driver includes source-only objects
only for their original branch. The live GSC database remains unchanged until
an authorized DBA tests and runs its driver.
