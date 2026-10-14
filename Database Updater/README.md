# Work Order Database Updater

Start with [the step-by-step user guide](USER_GUIDE.md), or click
**Help / instructions...** inside the tool. `START_HERE.txt` has the quick start.
The `Templates` folder supplies test-restore examples for creating/updating a
function, adding a column, and adding an index. The guide explains importing
files, dependency order, repeat deployment, errors, and standard package setup.

A separate Windows tool for SQL Server 2016 and later. It installs or updates
the selected Work Order database objects, migrations, and performance indexes.
It does not install application EXEs, change the RMS registry, or modify the
Work Order application's business process by itself. Selected SQL determines
what changes; replacing a branch function can change its business rules.

## Build and distribute

Build `Work Order Entry.sln` in Release / x86. The executable is:

`Database Updater/bin/Release/Work Order Database Updater.exe`

Copy the **whole Release folder**, including its `Database Components` folder,
to the workstation. Requires Windows and .NET Framework 4.7.2. No Visual Studio,
SSMS, sqlcmd, or Windows administrator elevation is required to run it. A database
administrator must grant the chosen SQL login the permissions needed by its
scripts and `VIEW DEFINITION`; Windows administrator rights do not replace SQL
permissions. Windows authentication is the default. SQL-login passwords remain
in memory and are not stored in settings or run logs.

## Use

1. Back up the target database and test the selected scripts on a disposable
   restore. Close database maintenance tools and choose a quiet period.
2. Open the updater. Enter the SQL server and exact database name. Select
   Windows login or enter an approved SQL login.
3. Choose **GSC**, **DVO**, or **Portable**. The profile selects object definitions;
   it does not infer your store from its database name. For renamed restores,
   select the profile matching the original branch. Review all warehouse,
   StoreID, and cross-database references before using an exported function.
4. Click **Check target**. This is read-only. It checks the database, SQL version,
   basic RMS tables, writeability, definition visibility, and past tool records.
   It does not prove every script dependency exists or that every DDL permission
   has been granted. Each script performs its own checks when executed.
5. Tick only the scripts needed. Selecting a row shows its SQL below the list.
   Checked scripts execute in the displayed order; column sorting is disabled.
   No scripts or permissions are applied automatically.
6. Click **Review and apply**. Inspect the complete selection and exact target,
   confirm testing/backup, and type the database name to enable **Apply**.
7. Wait for **COMMITTED** or the failure result. Click **Check target** again to
   refresh the recorded hashes. Then test the affected Work Order/Queueing flow.

For the current recall performance improvement, select only
`Improve_WorkOrder_ItemLookup.sql`. It creates a non-unique item-code index or
reuses suitable coverage; it preserves prices, quantities, and business rules.
Local GSC already received that index on October 14, 2026. Running it again is
idempotent. **No tool record** does not mean an object/index is absent: changes
made using SSMS or earlier tools have no entry in this updater's history.

## Future migrations and function fixes

Select **Enable_WorkOrder_History.sql** for both new installations and updates.
This one script creates/updates the history triggers and converts the approved
legacy history IDENTITY table to a sequence, retaining events, IDs and permissions
in the same transaction. RMS uses `@@IDENTITY`; history must not replace its
returned order/transaction number. Customized legacy history schemas stop for
review. Back up first. The former `Fix_WorkOrder_History_Identity.sql` remains
only for existing recovery scripts and is no longer a separate catalog selection.
This update prevents recurrence; it does not repair already mislinked sales. See
[the incident recovery instructions](../Database%20Components/Recovery/README.md).

Use **Add SQL script** for trusted, reviewed `.sql` files. Functions can use
`CREATE FUNCTION` or `ALTER FUNCTION` as their own batches separated by `GO`.
For create-or-update behavior on SQL Server 2016 RTM, use the existing packaged
pattern: check `OBJECT_ID`, then execute a dynamic `CREATE` or `ALTER` statement.
Do not assume `CREATE OR ALTER` is supported on all SQL Server 2016 installations.

Added scripts must omit `USE`, SQLCMD directives (`:r`, `:setvar`, variables),
database/server maintenance, and transaction commands. The updater provides
one transaction for the whole selection. Packaged migrations with existing
balanced nested transactions are supported. `GO` repeat counts are rejected;
strings, quoted identifiers, and nested comments are handled when splitting.
The updater is **not a sandbox for untrusted SQL**: dynamic SQL, cross-database
references, triggers, and external side effects require manual review. Keep
added scripts local to the target and do not alter deployment history or
transaction/session behavior. SQLCMD branch drivers are used only to discover
their ordered object files; their security scripts are excluded.

Future changes can also be added to the existing branch drivers as individual
object files, then rebuilt to distribute the updated package. Optional migrations
are listed explicitly in `UpdateCatalog.Load`. Existing table creation scripts
do not automatically alter existing columns; supply a reviewed migration for a
schema change. Deployment hashes record executions, not current object drift.
There is no automatic version upgrade, skip, downgrade, or business-rule merge.

## History, rollback, and run files

The selected SQL and its history entries commit in one SQL transaction. A SQL
failure rolls back transactional changes and stops later scripts. An application
lock prevents two updater instances from deploying to the same database at once.
Ordinary POS traffic is not paused. Lock waits are limited to five seconds and
individual commands to two minutes; timeout causes rollback. Run smaller batches
if a larger maintenance operation needs a separate deployment process.

Successful executions add `dbo.SOD_WO_DeploymentHistory` with run ID, script ID,
SHA-256 file hash, UTC time, and SQL login. Re-running a recorded script is an
explicit choice; only idempotent scripts should be repeated unchanged.
If the connection is lost during commit, check this history before retrying.

Run files are under `%LOCALAPPDATA%\WorkOrderDatabaseUpdater\Runs`:

- `result.txt`: target, selected file hashes, execution progress, outcome/errors.
- Numbered SQL files: the exact reviewed SQL text executed.
- `before-schema.xml`: visible module definitions/settings and index inventory
  taken before changes. Encrypted definitions appear without SQL text.

The snapshot helps review a function's previous definition. It is **not a full
database backup or an automatic restore script**: it does not contain table data
or a complete schema. After a successful update, undo requires a reviewed reverse
migration or database restore. A local-log failure after commit is reported as a
warning, not as a failed database update. Treat run folders as internal database
documentation; custom scripts must not contain passwords or other secrets.

## Verification

`Database Components/Tests/DatabaseUpdater_LocalDB_Test.ps1` creates its own
GUID-named LocalDB instance and SQL Server 2016 compatibility database. It checks
function creation/update, migrations/index reuse, transaction rollback, history,
concurrent deployment rejection, target and permission checks, file changes,
catalog loading, SQL batch handling, and form initialization. It never connects
to an existing store database. Build the Debug updater before running it.
All 60 checks passed on October 14, 2026, including the guide's runnable examples
and the documented change from doubling to tripling the example function result.
The HO_PO_DB column migration also passed target/type guards, value preservation,
repeat deployment, and rollback checks in a separate database on the owned fixture
instance. The live HO_PO_DB server has not been updated by these tests.
