# Cebu: restore 23 missing work-order commitments

Target: `WIN-7RFPRJCS0F8 / SBD_CEBU_MAIN_DB`. No credentials are stored here.

The user explicitly authorized restoring only the reviewed 23 zero-commitment
inventory items from nine open work orders: 26592, 26596, 26597, 26598, 26599,
26600, 26602, 26603 and 26604. The user defined fulfillment by the order's
Closed field and authorized the correction after all nine were confirmed
Closed=0, Type=2. No matching Transaction.RecallID or OrderHistory.OrderID
references were found. This does not independently establish physical release
outside database records, and the original cause of the mismatch is unknown.

`SOD_View_CommittedvsOpenWo` excludes QuantityCommitted=0 and therefore hid
these mismatches. Its definition is not changed by this recovery.

## Operation and safeguards

`Restore_Cebu_23_ZeroCommitments_2026-10-21.sql` is a one-off recovery, not an
updater migration. The default Commit=0 performs only SELECT preview work.
Commit=1 requires a verified recorded full backup, exact server/database,
23 exact item/code/entry/quantity mappings, still-open headers, no extra active
nonzero reservation lines and no settlement/history references. It checks
the exact three reviewed Item triggers and absence of item-log triggers.

The transaction takes the existing per-order application locks plus short
serialization table locks for order headers/lines/settlement/history and update
locks on the 23 Item rows. Failure to obtain a lock stops it; there is no retry.
It adds only each reviewed difference to a still-zero QuantityCommitted,
verifies physical stock, and inserts 23 existing-schema Item Committed Correction
audit records with the corresponding WO reference. No trigger is disabled or
changed. No order, queue, price, tax, identity or application code is changed.
Re-execution after success is rejected by the zero-commitment guard.

The existing full backup completed October 20, 2026 at 17:10 passed SQL Server
RESTORE VERIFYONLY on October 21 before repair execution. It has no backup
checksums, is not copy-only, and is not a same-day backup. VERIFYONLY is not a
test restore or proof of application consistency. Local before/after records
supplement it; no restore or new backup was performed on the store database.

## Validation and status

- Repair UPDATE/audit body: 12 checks passed in an owned disposable LocalDB
  database, which was removed. Includes exact quantities, unchanged stock and
  prices, unrelated item preservation, audit count, repeat/nonzero rejection,
  audit failure rollback and physical-stock-change rollback. This does not test
  the complete production backup/target guard path on a fixture; those guards
  were not relaxed or retargeted.
- Existing MultiUser_Save_LocalDB_Test.ps1: 39 checks passed normally and 39
  with VersionedReads. Its settlement participant is a simulation, not RMS POS.
- Both Release/x86 solutions and Debug built successfully. All 101 unit tests
  passed after an initial sandbox test-host connection timeout was resolved by
  running the unchanged validation driver outside that sandbox. Debug EXE
  configuration was preserved byte-for-byte.
- **Live COMMITTED October 21, 2026 at 11:26:10, server time:** exactly 23
  QuantityCommitted values corrected and 23 audit rows inserted. Independent
  verification on a new SQL connection confirmed each matches the current
  open-WO total, all physical quantities equal the saved preview, and all nine
  orders remain open. Whole-database ItemType=0 comparison returned zero exact
  mismatches at that instant. This is a point-in-time observation, not a guarantee
  against later changes from store processes.
- Item 53494BTL changed from 0 to 50 committed; physical stock stayed 272.
  WO 26596 was not cancelled or otherwise changed. The previously identified
  negative-commitment blocker is resolved; UI cancellation was not exercised.
- Ignored execution evidence is in
  `TestResults/CebuCommitmentRepair-20261021/`: BeforePreview.json,
  VerifiedBackupMetadata.json, CommittedResult.json, IndependentPostCheck.json.
  The script's shipped Commit value remains 0; only the explicitly authorized
  execution command set Commit=1 and supplied the verified backup metadata.

RMS trigger investigation remains paused. This repair does not explain why
these reservations disappeared or prevent another process from changing them.
