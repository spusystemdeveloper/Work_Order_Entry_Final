# HO_PO_DB transaction 23266: history identity incident

Incident date: October 14, 2026. Recovery follow-up: October 15, 2026
(Asia/Shanghai). This record summarizes the diagnosed incident, packaged recovery
guards and the user's live recovery results. A user-supplied SSMS screenshot
now records a committed database repair; final verification in RMS is pending.

## Current status

**Database repair committed — RMS acceptance verification pending.**

On October 15, the user reported running the live recovery with `@Commit=0`
and receiving error 52703: "A copy-only full HO_PO_DB backup with checksums from
today is required." The user then reported that the new backup was running and
had taken approximately six minutes so far. That was elapsed time at the report,
not a measured final backup duration or proof of a stalled backup.

Later on October 15, the user supplied an SSMS screenshot showing
`CommitRequested=1`, `LocalTestMode=0`, `RelinkItemLogs=0`, target database
`HO_PO_DB`, and a final result beginning `COMMITTED: HO transaction 23266 ...`.
The standalone script returns that result only after `COMMIT TRANSACTION`.
This confirms the reported database repair committed; it is not just a rehearsal.
The assistant did not execute the live repair or independently reconnect to it.

The screenshot shows these verification results:

| Result | Visible value |
| --- | --- |
| Transaction / recalled Work Order | `23266` / `18870` |
| Header total / item total / tender total | `30,200.54` / `30,200.54` / `30,200.54` |
| Sales tax | `0.00` |
| Item lines / tax rows | `4` / `4` |
| `HistoryUsesIdentity` | `0` |
| Item entry links | `65233`–`65236` all reference `23266` |
| Receivable | `24403` references `23266`; original amount and balance both `30,200.54` |
| RMS order history | `19857`, order `18870`, references `23266` |

With the reviewed script unchanged, reaching its committed result also means
the live backup guard and verification succeeded. The screenshot does not show
the final backup filename, backup duration or separate backup/verification
messages; retain those independently. A successful separate `@Commit=0`
rehearsal result has not been supplied. Viewing the repaired invoice/receivable
in RMS and verifying the next normal Work Order posting remain unconfirmed.

## Affected transaction and symptoms

| Field | Diagnosed value |
| --- | --- |
| SQL server / database | `172.16.4.50` / `HO_PO_DB` |
| Actual transaction number | `23266` |
| Recalled Work Order | `18870` |
| Transaction timestamp in the recovery guard | `2026-10-14 13:53:01` |
| Store / batch | `606` / `80` |
| Wrong transaction reference on affected child records | `17` |
| Expected item lines / tax rows | `4` / `4` |
| Expected item and tender totals | `30,200.54` each |
| Expected sales tax | `0.00` |

The transaction header existed as 23266, but the affected item and related
records referenced 17, making the item entries appear missing from the actual
sale. Transaction 17 also has legitimate older records; it is not safe to update
every row carrying that number.

## Cause

Our earlier custom history addition used an `IDENTITY` column for
`dbo.SOD_WO_History.EventID`. RMS uses `SELECT @@IDENTITY` after inserts.
An audit trigger's history insert, including a nested queue-history insert,
could therefore replace the identity value returned to RMS with a history event
ID. RMS could then use that wrong value to link transaction children.

This was a compatibility defect in our history addition interacting with RMS's
legacy identity retrieval. It was not a change to RMS's identity columns or the
real transaction number, and it should not be described as a confirmed standalone
RMS bug. The packaged fixture reproduced the identity behavior and checked both
direct and nested trigger paths.

The prevention is sequence-backed history numbering. Do not reseed or redesign
RMS identity columns to address this incident.

## Exact recovery scope

The guarded recovery relinks only the diagnosed records from 17 to 23266:

| Table | Record IDs |
| --- | --- |
| `TransactionEntry` | `65233`, `65234`, `65235`, `65236` |
| `TaxEntry` | `65198`, `65199`, `65200`, `65201` |
| `TenderEntry` | `25815` |
| `AccountReceivable` | `24403` |
| RMS `OrderHistory` | `19857` |

It validates the exact headers, amounts, row identities, payment state and
supported schema before proceeding. It preserves transaction headers, inventory,
customer balances, non-link fields and unrelated transaction-17 records. It does
not create another sale, payment or stock movement. Subsequent receivable/payment
activity can cause the guard to reject execution; investigate rather than
removing that guard.

Custom `SOD_WO_History` audit events are preserved while their numbering is
converted to a sequence. These audit events are distinct from RMS `OrderHistory`
record 19857, whose transaction reference is repaired.

Optional item-log reference correction is off by default
(`@RelinkItemLogs=0`); it is a separate decision because it scans a large table.
Compressed Journal 23804 is retained and its historical receipt may still show
17 after the database links are corrected. The recovery does not regenerate it.

## October 15 backup guard

The live script checks all of these before starting its repair transaction:

- A full backup of `HO_PO_DB`, marked copy-only and created with backup checksums.
- A matching backup-history record in the connected server's `msdb`, completed
  on or after the current date according to SQL Server's `GETDATE()`.
- A physical backup path matching `@BackupPath`.
- Successful `RESTORE VERIFYONLY ... WITH CHECKSUM` before repair.

The script in the repository was prefilled with an October 14 backup path:

```text
F:\AUTO_RMS_BACKUP_DB\HO_PO_DB\HO_PO_DB_IdentityRecovery_20261014_1791961451083.bak
```

That prefilled path is not evidence of a qualifying October 15 backup. A stale
path/date, missing copy-only/checksum flags, or missing matching server backup
history can produce 52703. The user's SSMS connection and current backup metadata
were not independently inspected during this follow-up, so the exact failed
condition beyond the reported guard message remains unconfirmed.

`@Commit=0` is a rollback rehearsal: it still performs the schema conversion and
link changes inside a transaction before rolling them back. It is not a read-only
preview. The same backup prerequisite applies. Error 52703 occurs before that
repair transaction begins; that rejected run did not apply the recovery.

The local-only checksum exception is restricted to the MSI test computer. Keep
`@LocalTestMode=0` on the live server. Do not substitute the local test file or
remove the production guard.

## Recovery procedure for reference

The user executed the recovery personally in SSMS. The committed result is now
recorded above; the procedure below preserves the original recovery instructions
and is not a request to rerun the completed repair. No automatic live repair is
authorized by this incident record.

1. Allow the current backup and verification query to finish. If they fail,
   retain the exact error and resolve it before recovery.
2. Copy the returned path into **only** the `DECLARE @BackupPath` line of
   [the live SSMS script](HO_PO_DB_23266_Run_In_SSMS.sql). The file must be
   accessible to SQL Server on the live server. Do not edit the validation `IF`.
3. Pause RMS posting for recovery maintenance. Keep `@Commit=0`,
   `@LocalTestMode=0` and `@RelinkItemLogs=0`; execute the entire live file.
4. Require `REHEARSAL PASSED`. Check transaction 23266 / Work Order 18870,
   four item lines, four tax rows, totals 30,200.54 and tax 0.00. Rehearsal results
   describe the temporary prepared state; the script rolls it back.
5. After personally reviewing those results, set `@Commit=1` and run the entire
   file again. Require `COMMITTED`. Record a failure rather than bypassing a guard.
6. Verify the repaired items and receivable in RMS. Verify that the next ordinary
   Work Order posting links its own transaction, items and tender correctly.
   Do not tender or repost the already-posted transaction 23266 again.

The backup query provided during this follow-up creates a uniquely named file
and verifies it; it does not execute the repair:

```sql
USE [HO_PO_DB];

DECLARE @BackupPath nvarchar(4000) =
    N'F:\AUTO_RMS_BACKUP_DB\HO_PO_DB\HO_PO_DB_Recovery_'
    + CONVERT(nvarchar(8), GETDATE(), 112) + N'_'
    + REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'')
    + N'.bak';

BACKUP DATABASE [HO_PO_DB]
TO DISK = @BackupPath
WITH COPY_ONLY, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY
FROM DISK = @BackupPath
WITH CHECKSUM;

SELECT @BackupPath AS BackupPath;
```

Use this on the intended live connection and outside any open transaction.
The folder must already exist on the SQL server and be writable by its SQL
Server service account. The recovery script independently rechecks the backup.
References: [SQL Server copy-only backups](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/copy-only-backups-sql-server)
and [backup checksums](https://learn.microsoft.com/en-us/sql/relational-databases/backup-restore/enable-or-disable-backup-checksums-during-backup-or-restore-sql-server).

## Prevention, testing and evidence to retain

- [Enable_WorkOrder_History.sql](../Migrations/Enable_WorkOrder_History.sql)
  now creates fresh sequence-backed history or converts approved legacy history
  and creates/updates audit triggers in one transaction. The updater offers this
  single history selection. It prevents recurrence; it does not relink old sales.
- [The live recovery file](HO_PO_DB_23266_Run_In_SSMS.sql) is self-contained and
  already includes its history compatibility conversion plus the exact repair.
  Execute it directly in SSMS, not through the updater.
- [Repair_HO_Transaction_23266.sql](Repair_HO_Transaction_23266.sql) is the guarded
  core used by that file, not the complete standalone entry point.
- Previously recorded automated validation: 27 disposable HO recovery checks,
  27 history identity checks and 99 isolated Work Order workflow checks passed.
  These synthetic checks do not establish that the live incident is resolved.

Retain the final backup path and backup/verification messages, rehearsal output,
commit output, target server/database, execution date/time and operator, the SQL
script version or hash, RMS verification results and any rejected guard messages.
Manual SSMS recovery does not create an updater deployment-history record.
Keep passwords and connection secrets out of the incident record.

Record the remaining RMS verification results before marking the incident closed.
See [the recovery guide](README.md) for the detailed operating instructions.

## Separate follow-up: new-order Save timeout

On October 15, the user also reported timeouts when saving new Work Orders and
imported orders. Read-only live diagnostics located approximately 30-second
execution in the existing `trg_OrderEntry_ItemLogs` trigger's unused new-line
lookup over a roughly 71.7-million-row unindexed log table. The new history
trigger was fast, and history identity remained disabled. This is separate from
the repaired transaction-23266 links. See [the timeout diagnosis and tested
manual SQL fix](HO_PO_DB_SAVE_TIMEOUT_GUIDE.md). The assistant has not applied
that trigger fix to live. Later read-only verification matched its fixed trigger
fingerprint and found saved Order 18879; full import/edit acceptance remains
pending. The reported ID gaps are documented in the timeout guide, with the
Order identity increment verified as 1 and no reseeding performed.
## Subsequent requested Order ID change (October 15)

The user separately requested Order 18870 -> 18869 and Order 18879 -> 18870.
See [the guarded manual renumber procedure](RENUMBER_HO_WORKORDERS.md).
It relinks transaction 23266 to the renamed first order while retaining the
actual transaction number, child identities and archived receipt contents.
The assistant prepared and tested it only on a disposable synthetic database;
no live renumber or reseed has been performed or confirmed. This request is
separate from repairing the original child-transaction links and does not make
SQL identity allocation gapless. The incident's original IDs below describe
the historical event and are intentionally retained.

