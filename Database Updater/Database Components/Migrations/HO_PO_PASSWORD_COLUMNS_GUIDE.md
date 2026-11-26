# Password-slot column update for HO_PO_DB

Use `Add_HO_PO_WorkOrder_Password_Columns.sql` through the updater. This is a
focused schema migration for an existing `dbo.SOD_WO_Conf` table in `HO_PO_DB`.
It adds any missing `Password2`, `Password3`, `Pass2Username`, and `Pass3Username`
fields as nullable `nvarchar(50)`. Existing fields and configuration values are
preserved. New fields start as NULL; the script does not assign new passwords
or usernames. Existing incompatible field types cause the migration to stop.

## Apply using the GSC profile

1. Back up the live database and test the script on a disposable restore named
   `HO_PO_DB` on a separate test SQL instance. The script deliberately checks this
   name. Keep its target guard; do not use the live instance for the initial test.
2. Open the updater and enter the actual live SQL server/instance and database
   **HO_PO_DB** when ready for the real deployment.
3. Select **GSC** and click **Check target**. GSC chooses package definitions;
   the database field chooses the destination. The target must already contain
   the updater's required RMS base tables and `dbo.SOD_WO_Conf`.
4. Click **Clear selection**. Do not select a complete branch package merely to
   add these fields: the GSC table-creation file leaves existing tables unchanged.
5. Click **Add SQL script...** and import
   `Database Components/Migrations/Add_HO_PO_WorkOrder_Password_Columns.sql` from
   the updater package, or the supplied standalone file.
6. Tick only this imported migration. Review its HO_PO_DB target guard and the
   four conditional column additions.
7. Click **Review and apply...**, confirm testing/backup, type **HO_PO_DB**, then
   click **Apply selected changes** during a quiet maintenance period.
8. After **COMMITTED**, click **Check target** again. The imported row should show
   **Recorded: same file**. The record is in HO_PO_DB's deployment-history table.
9. Reopen/test Work Order Settings and the relevant password approval flows using
   your approved existing credentials. Enter new password-slot values using the
   application's existing Settings process if needed; the migration creates only
   the fields. Do not send passwords in logs/screenshots.

The whole selected plan executes in the updater's transaction. Repeating this
migration preserves existing columns and values and adds no duplicate columns.
It contains no BEGIN/COMMIT/ROLLBACK commands, so it is accepted by **Add SQL script**.
Running it outside an active transaction is rejected to avoid partial additions.

## GSC functions on HO_PO_DB

Selecting GSC does not prove that all GSC definitions match HO_PO_DB. Functions,
views, and triggers may contain GSC database/warehouse/StoreID references. Review
those separately before applying them to HO_PO_DB. This column migration does not
install or replace those objects and does not establish full branch compatibility.

If **Check target** fails because HO_PO_DB lacks the RMS base tables, stop and
have the database maintainer confirm that this is the intended Work Order database.
The column migration also stops when the configuration table is missing or when
an existing target column has an incompatible type; it does not recreate the table.
