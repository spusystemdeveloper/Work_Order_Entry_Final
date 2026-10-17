# Work Order Database Updater: how to use it

This tool applies selected SQL changes to an existing RMS database: migrations,
functions, views, triggers, tables, and indexes. A migration is a script that
changes the database structure or its stored logic. An index helps SQL find rows
more efficiently. A SQL function stores logic that an application/query can call.

For a quick start, read `START_HERE.txt`. Inside the updater, click
**Help / instructions...** to read this guide offline.

For the optional item-log lookup index, read
`Database Components/Migrations/WORKORDER_ITEM_LOG_LOOKUP_GUIDE.md` first.
It preserves trigger SQL and business rows but holds a log-table lock during
preflight/index creation. Use a maintenance window and a restored-branch rehearsal;
large tables may need SSMS because the updater command timeout is 120 seconds.

## 1. Install the tool on a workstation

1. Extract the whole ZIP into one folder; do not run the EXE inside the ZIP.
2. Keep these files and folders together:

   ```text
   Work Order Database Updater.exe
   START_HERE.txt
   README.md
   USER_GUIDE.md
   Templates/
   Database Components/
     Deployment/
     Migrations/
   ```

3. Open **Work Order Database Updater.exe**. The machine needs .NET Framework
   4.7.2. SSMS and Visual Studio are not needed to run the tool.
4. If the package cannot be found, click **Choose package folder...** and select
   **Database Components**, the folder containing **Deployment** and **Migrations**.
   Select that folder, not its Deployment subfolder.

The tool runs using your normal Windows account. The database login must still
have `VIEW DEFINITION` and the SQL permissions required by your selected scripts,
including creating/writing the tool's deployment history. Ask the database
maintainer to grant the needed SQL permissions; running Windows as administrator
does not automatically provide them.

## 2. Connect to the correct database

| Field | What to enter |
| --- | --- |
| Server | `localhost` for a default SQL instance on this PC, or the actual server/instance name supplied by IT, such as `STORE-SERVER\RMS` |
| Database | The exact target, such as `GSC_STORE_DB`; use the name of your disposable restore when testing |
| Authentication | **Windows login** uses your current Windows account; **SQL login** uses the SQL username/password you enter |
| Branch profile | **GSC** or **DVO** matching the store's approved object definitions; **Portable** provides a limited baseline that still needs branch review |

1. Enter the connection information. Do not assume the default database is yours.
2. Choose the matching branch profile. Changing it does not change the database
   field. For a renamed test restore, select the profile of its original branch.
3. Click **Check target**. This operation reads metadata/history and applies no SQL
   migration. A successful check allows a selected plan to be reviewed.
4. If the check fails, resolve the reported connection, target, prerequisite, or
   permission issue before continuing.

Checking the target verifies SQL Server 2016 or later, a writable non-system
database, definition visibility, and the basic RMS Item/Order/OrderEntry tables.
It does not validate every dependency, branch rule, or script permission. The
tool does not install RMS or create a complete store database from scratch.

## 3. Apply a script already in the package

1. Obtain a current database backup and test the intended changes on a disposable
   restore. Choose a quiet deployment period, particularly for indexes.
2. Select a row to read its SQL in the lower pane. Selecting a row only previews
   it; tick its **Apply** checkbox to include it in the update.
3. Tick only the changes you intend to deploy. Use **Clear selection** to untick
   everything. The checked scripts run in the order shown in the list.
4. Click **Review and apply...**. Read the full selection and target again.
5. Check the box confirming branch review, testing, and a current backup. Type
   the database name exactly as displayed, including its capitalization.
6. Click **Apply selected changes**. Wait for the outcome before closing the tool.
7. After **COMMITTED**, click **Check target** again to refresh deployment records,
   then test the affected Work Order and Queueing behavior.

Example: for the existing recall item-code optimization, tick only
**Improve_WorkOrder_ItemLookup.sql**. It adds a non-unique item-code index if suitable
coverage is missing. Local GSC already received it on October 14, 2026; repeating
this particular script is supported. Other scripts may have different effects.

For the tested RMS voucher lookup improvement, no import is needed: the package
offers **Improve_RMS_VoucherLookup.sql** under **Index / performance** for DVO,
GSC and Portable. Select only that migration to deploy just the Item lookup index.
Read `Database Components/Migrations/RMS_VOUCHER_LOOKUP_GUIDE.md`, test a current
branch restore first and use a maintenance window. It preserves business rows,
triggers and branch rules, and rejects incompatible filter-column/index schemas.
The older Davao SQL replay is not proof of actual RMS capacity at every branch.

## 4. Import a SQL script received from a developer

Import means adding a file to the review list. It does not execute the SQL.

1. Save the reviewed file locally with a descriptive `.sql` filename, for example
   `2026-10-14_Update_SOD_FunctionName.sql`. It must be a SQL file, not `.sql.txt`.
2. Connect and select the intended profile. Choose your profile before importing;
   changing profiles or reloading the package clears custom imports/selections.
3. Click **Add SQL script...** and select one `.sql` file. It appears at the bottom
   as **Custom SQL**, initially unchecked.
4. For several files, add them one at a time in dependency order: prerequisite
   tables/columns first, then functions using them, then callers/views/indexes
   that need those definitions. Files are not automatically sorted by filename.
5. Select each imported row and read its SQL. Tick only the files you need, and
   include any necessary packaged prerequisites above them.
6. If needed, click **Check target**, then follow the review/apply steps above.
7. Keep the original scripts for future deployments. Custom imports exist only
   for the current session; reopening the app does not restore their list.

To fix the import order or replace an already imported file, click **Reload
package**, then add the custom scripts again in the correct order and recheck
the target. A duplicate filename cannot be added twice in the same custom list.
If you edit a file after importing it, reload and import it again before applying;
the updater rejects files changed since their reviewed snapshot.

## 5. Prepare an importable script

Use a text editor to create the `.sql` file, or ask the developer to supply it.
The updater previews and executes SQL; it does not write a function for you.

| Supported | Do not put in an imported script |
| --- | --- |
| SQL Server 2016 compatible SQL, comments, plain `GO` separators | `GO 2` or other repeat counts |
| Local `dbo` objects and reviewed changes | `USE ...`, database creation/restore, or server maintenance |
| Separate function definition batches | SQLCMD drivers/directives such as `:r`, `:setvar`, or variable placeholders |
| Guarded, repeatable changes where appropriate | `BEGIN TRANSACTION`, `COMMIT`, or `ROLLBACK`; the updater owns the transaction |
| Verification using `IF ... THROW` | Passwords, connection strings, or changes to the updater's deployment-history table |

Do not import the SQLCMD **Deploy_GSC_STORE_DB.sql** or **Deploy_DVO_STORE_DB.sql**
drivers. The tool already uses them to list individual object files. Its built-in
migration entries support their existing balanced transaction wrappers; importing
those same wrapped files through **Add SQL script** is rejected. Select their
built-in rows, or have a developer supply an importable version.

Use local database references unless a reviewed branch rule specifically requires
something else. The tool is not a sandbox: dynamic SQL, triggers, and cross-database
or external effects still require review. Keep the selected plan within the
updater's supported transaction behavior.

The tool does not show SELECT result grids. To make a script fail when verification
is wrong, add an `IF ... THROW` check. A developer can use SSMS to inspect query
results separately. Never remove a failing verification merely to make a deployment
appear successful.

## 6. Set up a new function, then update it

The **Templates** folder contains runnable examples for a disposable RMS restore.
They use separate demonstration objects; they do not introduce Work Order features.

### Create a demonstration function

1. Copy `Templates/01_CreateOrUpdate_ExampleFunction.sql` to a working folder.
2. Open the copy in a text editor. It creates `dbo.SOD_fn_UpdaterExample` if missing,
   then applies its definition using `ALTER FUNCTION`. It accepts one integer and
   returns twice its value. Its verification checks that input `3` returns `6`.
3. In the updater, target your disposable restore and import this copy.
4. Tick only this example, review, and apply. On a fresh restore it creates the
   function; running it again applies the same definition without creating a duplicate.

The template separates the function definition into its own batch with `GO` and
uses a small dynamic CREATE stub. This works without relying on `CREATE OR ALTER`
being available on every SQL Server 2016 installation.

### Update that function

1. Save a copy with a new release filename, for example
   `2026-10-15_Update_ExampleFunction.sql`.
2. Change `RETURN @Value * 2;` to `RETURN @Value * 3;` in that copy.
3. Change the verification expectation from `<> 6` to `<> 9`; input `3` must now
   return `9`. Keep the function name, parameter, and return type unchanged.
4. Reload the package if the old custom row is still present, import the new file,
   select it, and apply it to the same disposable restore.
5. Verify the affected function/app behavior before distributing a real change.

For a real function, use the approved name/body, dependencies, and test expectations.
Keep its interface compatible with callers. Altering parameter or return types can
require application changes; changing a scalar function into a table function is
not a routine body update. The example stops if its name belongs to another object
type rather than replacing that object.

Creating a function in SQL does not add a button or feature to Work Order Entry.
The application must already call it, or a developer must add and test the calling
code and distribute the matching application EXE. Replacing a function already
used by the app can change behavior immediately; review its business rules.

## 7. Add a column or an index

**History create/update:** select only `Enable_WorkOrder_History.sql`. It handles
fresh installations and approved legacy IDENTITY history tables, preserving
event records, IDs and permissions while using a sequence so RMS receives its
own transaction/order number. It also creates/updates the history triggers in
the same transaction. Back up first; customized legacy tables stop for review.
No separate history fix selection is required. Already incorrect transaction links
need a separate incident-specific recovery. The
[HO recovery guide](../Database%20Components/Recovery/README.md) provides a
self-contained SSMS file with rollback rehearsal enabled by default.

For the existing password-slot fields specifically in **HO_PO_DB**, use
`Database Components/Migrations/Add_HO_PO_WorkOrder_Password_Columns.sql` through
**Add SQL script...**. Set the actual server and **HO_PO_DB** as the database; you
can leave the profile at **GSC** for this focused import. Select only the imported
migration. It adds missing Password2/Password3/Pass2Username/Pass3Username fields
and preserves existing values. Test a restore and back up before live deployment.
Selecting the GSC profile alone does not add missing columns to an existing table
or confirm that its branch functions match HO_PO_DB.

Two more test-restore examples are included:

| File | Demonstration |
| --- | --- |
| `02_AddExampleColumn.sql` | Creates a custom demonstration table if missing, then adds a nullable UTC timestamp column if needed; stops on an incompatible existing column |
| `03_AddExampleIndex.sql` | Adds a non-unique index on that demonstration column; requires example 02 first and checks an existing index's definition |

Import **02**, then **03**, and tick both to apply them in that order. These files
contain no transaction commands because the updater supplies the transaction.
Use them as patterns, then write a separately reviewed migration for actual tables.
Copying a new table creation script does not upgrade existing columns automatically.

For performance work, measure the affected query/flow before and after the change,
confirm that returned values match, and check for suitable existing indexes first.
An index helps specific queries; it does not guarantee that every save/recall becomes
faster. Avoid adding duplicate indexes to every table.

## 8. Make a function appear in the standard package list

Importing a standalone file needs no updater rebuild. To distribute a function as
a standard package entry, a developer should:

1. Put its reviewed, create/update SQL in the repository's
   `Database Components/Deployment/Objects` folder. Use **Common/Functions** only
   if the definition is appropriate for both branches; otherwise use the matching
   branch override location/pattern.
2. Add its include line to the intended driver after its dependencies and before
   objects that call it. For a common function the line format is:

   ```text
   :r .\Objects\Common\Functions\SOD_fn_NewFunction.sql
   ```

   Use that exact path style at the start of a line. This include goes in the package
   driver, not inside an imported function file. Update the applicable GSC/DVO driver
   and, only if intended, the portable driver.
3. Update the deployment manifest/documentation for the new object and its branch
   requirements. This preserves the project's object audit.
4. Test creation, replacement, dependencies, and repeat deployment on a disposable
   restore, plus the application's behavior if it uses the function.
5. Build **Work Order Entry.sln** in **Release / x86** to copy the SQL package into
   the updater output. Distribute the whole updated folder/ZIP.
6. At the workstation, open the new package or use **Choose package folder...**,
   choose the correct profile, and click **Reload package**. The function appears
   unchecked; it is not applied merely because it was added to the driver.

The existing updater reads object paths from its driver files when loading. Updating
only those SQL files/drivers in a complete package does not require changes to the
updater's C# logic. Optional migrations have an explicit list in `UpdateCatalog.Load`:
copying a new file into **Migrations** alone will not list it automatically. Use
**Add SQL script** for it, or extend that catalog and rebuild the updater.

## 9. Understand the result and deployment records

| Message/status | Meaning and next action |
| --- | --- |
| **COMMITTED** | Selected SQL and history entries committed. Check target again and test the affected flow. |
| **No tool record** | No successful execution of that script ID is recorded by this tool. The object might already exist from another deployment method. |
| **Recorded: same file** | The file hash matches its latest successful tool record. This does not compare the live object definition with the file. Reapply only if appropriate for that script. |
| **Recorded: file changed** | This file differs from its latest successful record. Review the changes and test before applying. |
| File changed after review | Reload/re-import the updated file, check target, and review the new SQL. |
| SQL error | Read the failing script/batch and SQL error in the local log. Supported transactional changes are rolled back, and later scripts stop. Correct the cause before retrying. |
| Another update is running | A different updater has the database deployment lock. Wait for it to finish, then check again. |
| Lock/command timeout | Review the log, choose a quiet period, and check database activity with the maintainer. Do not keep retrying during contention. |
| Connection lost during commit | The outcome can be uncertain. Have the maintainer check deployment history before retrying. |
| Changes committed, final log warning | Changes are already committed. Confirm using history; do not assume the update failed and run it again blindly. |

Run files are stored in:

```text
%LOCALAPPDATA%\WorkOrderDatabaseUpdater\Runs
```

Each run contains selected numbered SQL files, `result.txt`, and, once the database
preflight/snapshot stage succeeds, `before-schema.xml`. The result records selected
hashes and execution progress. Successful runs are also recorded in
`dbo.SOD_WO_DeploymentHistory` with run ID, script ID/hash, UTC time, and SQL login.

The schema snapshot contains visible module definitions/settings and index inventory.
It contains no table data, cannot recover encrypted function definitions, and is
not a full backup or a one-click undo. After a committed change, reverting requires
a reviewed reverse migration or a database restore. For an unchanged function
interface, an approved previous create/update definition can be a reverse change;
reverting table/data changes may require a different recovery plan.

## 10. Final check before a real store update

Confirm the intended server/database/profile, exact selected files and execution
order, backup, successful restore testing, and branch rules. After committing,
test the relevant save/recall/queue flow and compare results with the expected
behavior. Distribute a new Work Order/Queueing EXE separately when the change also
needs application code; this database tool does not replace either EXE.
