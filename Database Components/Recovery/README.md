# HO_PO_DB transaction 23266 recovery

Use [HO_PO_DB_23266_Run_In_SSMS.sql](HO_PO_DB_23266_Run_In_SSMS.sql).
This is a self-contained SQL file for personal execution in SSMS. Do not import
it into the Database Updater: it supplies its own target and transaction.
**The assistant has not applied this recovery to live HO_PO_DB.**

## Test a restored copy on localhost

Open [HO_PO_DB_23266_LOCAL_TEST.sql](HO_PO_DB_23266_LOCAL_TEST.sql) in SSMS connected
to **localhost on MSI**, targeting your restored **HO_PO_DB**. This local-only file
has `@LocalTestMode=1` and `@Commit=0` already set.

**No `.bak` file yet?** Run [Create_Local_HO_Backup.sql](Create_Local_HO_Backup.sql)
in SSMS connected to localhost first. It creates a uniquely named copy-only
backup without checksums in the existing local SQL Server backup folder, verifies
the file, and returns a `RecoveryScriptLine`. Replace the local recovery file's
`DECLARE @BackupPath...` line with that returned line. The helper only creates a
backup; it does not apply the fix. Its backup folder can be adjusted if needed.

1. Set `@BackupPath` to the actual local path of your copied `.bak` file.
   SQL Server must be able to read that file; do not use the original server's
   drive path unless the file also exists at that path locally.
   Edit only the `DECLARE @BackupPath...` line. The validation `IF` must not
   compare against your real file path: that would always match and raise 52702.
   Updated scripts validate the placeholder prefix instead, so replacing the
   placeholder cannot accidentally turn a valid path into a rejected path.
2. Execute the entire file with `@Commit=0` for a rollback rehearsal.
3. After checking the four item lines and total 30,200.54, use `@Commit=1` if you
   want to apply the fix to this local test database.

Local test mode skips the live requirements for backup checksums, today's
copy-only backup and a matching local `msdb` history record. It still runs
`RESTORE VERIFYONLY`, requires a readable backup file, and retains every incident
data/schema/transaction guard. Default SQL verification checks backup checksums
when present, but does not require them when absent
([Microsoft documentation](https://learn.microsoft.com/en-us/sql/t-sql/statements/restore-statements-arguments-transact-sql)).
No checksum is removed from the backup file, and no server-wide setting changes.

The exception is restricted to computer **MSI**. It is rejected on the live
server. For production, use **HO_PO_DB_23266_Run_In_SSMS.sql**, which defaults to
`@LocalTestMode=0` and retains its verified checksum/copy-only backup requirement.

## Run it

1. Pause RMS posting during maintenance. Connect SSMS to **172.16.4.50** with an
   authorized SQL login. Open the entire SQL file in a new query window.
2. Inspect the target and settings near the top. The script uses **HO_PO_DB**.
   `@BackupPath` is prefilled with the checksum-verified copy-only full backup
   created on October 14, 2026:

   `F:\AUTO_RMS_BACKUP_DB\HO_PO_DB\HO_PO_DB_IdentityRecovery_20261014_1791961451083.bak`

   This path is on the SQL server. If you run on a later day, create a fresh
   copy-only full backup with checksums and replace `@BackupPath`. The script
   checks backup metadata and runs RESTORE VERIFYONLY before any repair.
3. Keep **`@Commit=0`** for the first execution. Run the **entire file**.
   It converts the history numbering and prepares the exact link repair, shows
   results, then rolls all changes back. Look for **REHEARSAL PASSED**.
4. Check the result: transaction **23266**, Work Order **18870**, **4 item lines**,
   **4 tax rows**, item and tender totals **30,200.54**, sales tax **0.00**,
   receivable **24403** and order-history **19857** pointing to **23266**.
   `HistoryUsesIdentity=0` describes the prepared state inside the rehearsal;
   it is rolled back until you commit.
5. Set **`@Commit=1`** and execute the **entire file again**. Look for **COMMITTED**.
   Any validation/SQL failure rolls back that run. Stop if a guard rejects changed
   amounts, payments, schema, triggers, or records; do not remove those guards.
6. Verify the invoice items and receivable in RMS. For the next normal Work Order
   posting, verify its transaction number, items and payment are linked correctly.
   Do not tender/repost this already-posted sale again.

## What changes

The script preserves audit event IDs/data while replacing the custom history
IDENTITY with a sequence. Existing audit triggers remain intact. This prevents
RMS's legacy `@@IDENTITY` lookup from returning a history event ID.

It relinks only item entries **65233–65236**, tax rows **65198–65201**, tender
**25815**, receivable **24403**, and order history **19857** from wrong number
**17** to actual number **23266**. It verifies other persisted fields, inventory
quantities/commitments, headers, and unrelated transaction-17 records. No sale,
payment, stock movement, tax amount, or customer balance is recreated.

**Optional item logs:** leave `@RelinkItemLogs=0` for the core repair. Set it to
`1` if you also want the four exact historical `SOD_Item_Logs.ReferenceID`
values corrected. This scans a very large unindexed table and can extend locks;
run it during quiet maintenance. It never changes recorded quantities.

**Original receipt:** compressed Journal **23804** is retained. Its stored receipt
can still show the original wrong number. Correct database links do not regenerate
that historical receipt. This script does not rewrite the compressed journal.
Save the SSMS results/messages with the backup record for the recovery trail;
manual SSMS execution does not create a Database Updater deployment record.

## Other files

- `Repair_HO_Transaction_23266.sql`: guarded core repair embedded in the standalone
  file; not a complete standalone execution script.
- `Build_HO_RecoveryScript.ps1`: rebuilds the standalone file from reviewed sources
  without opening SQL. Use `-VerifiedBackupPath` to prefill a verified backup.
- `Recover_Local_GSC.ps1` and `Repair_GSC_Transaction_289655.sql`: incident-specific
  local recovery already committed after a verified backup and rollback rehearsal.
  They do not target HO. The local original compressed receipt was preserved.

The HO recovery files passed **27 disposable LocalDB checks**; history conversion
passed **20 identity compatibility checks**. These are synthetic tests, not live
production execution or an actual new RMS cashier posting.

