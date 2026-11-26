# Requested HO recovery: WO 18882 to 18879, next allocation 18880

## Committed live result and read-only verification

The user reported the final COMMITTED result after personally running Commit=1.
Read-only verification on WIN-2847UGJBAV4 / HO_PO_DB at approximately 16:48 on
October 16 confirmed:

- WO 18879 exists and 18882 does not; maximum Order ID and current identity are
  both 18879, increment 1. IDs 18880 and 18881 are unoccupied. Next allocation
  is 18880 provided no intervening allocation occurs; no test order was inserted.
- Entries 61690/61691 now reference 18879 with quantity 3 each, exact original
  prices 7962.4833/7696.2956 and full prices 13000/11800. Header total remains
  46976.3367 and tax 0.
- Queue 45888 has OrderID and GroupTo 18879. Approval rows 17/18 and all ten
  original custom audit events reference 18879; mapping event 157 was appended
  at 16:47:41. Original historical description text remains unchanged.
- Three order-domain item-log rows reference 18879. A database-wide old-ID
  order-log check returned zero references to 18882.
- No old structured references were found in the reviewed order/line/queue/
  approval/history, RMS transaction/history/order-receipt, held/import and other
  reviewed reference tables. No posted transaction/receipt existed for this order.
- All 13 reviewed table triggers are enabled with exactly their expected hashes.

The assistant performed SELECT/metadata checks only, not the renumber or reseed.
External files, printouts and arbitrary custom payloads were not verified. No
backup was verified by the assistant; the saved file's backup checks had been
removed at the user's explicit request before execution. Evidence is under
ignored TestResults/HO-Renumber18882-Verification-d81e6813.

Prepared October 16, 2026 for personal SSMS execution only. The assistant read
live metadata and the source rows, but did not run this recovery against live,
even in rollback mode. Automated execution was confined to a uniquely owned
synthetic LocalDB fixture. No restored-store recovery test has been performed.

Use [Renumber_HO_WO_18882_to_18879_And_Reseed.sql](Renumber_HO_WO_18882_to_18879_And_Reseed.sql).
This is a standalone recovery file, not an updater migration. It combines this
specific renumber and reseed in one explicit commit transaction. Do not use the
older two-order renumber or its separate reseed file for this request.

## Reviewed state

- Target: WIN-2847UGJBAV4 / 172.16.4.50 / HO_PO_DB, compatibility level 100.
- Source: open Type 2 order 18882, StoreID 606, CustomerID 71, created and last
  updated at 2026-10-16 16:21:09.340, Total 46976.3367, Tax 0.
- Lines 61690 and 61691: ItemIDs 46612/54299, quantity 3 each. Exact stored prices
  are 7962.4833/7696.2956, with FullPrice 13000/11800. No rounding is applied.
- Queue header 45888 points to/grouped to 18882. Two price approvals and ten
  custom audit events were observed. No posted RMS transaction, OrderHistory or
  order receipt was observed for the source at inspection.
- Maximum Order ID and last allocated identity were 18882, increment 1.
  Destination 18879 and future allocations 18880/18881 must remain unused.

## Personal execution

1. Close all HO RMS POS/Manager, Work Order Entry, queue and integration clients.
   Keep them closed through verification. Recalled/cached snapshots become stale.
2. No backup pathname or checksum validation is required by this revised file.
   At the user's request, the BackupPath setting, backup-history lookup and
   RESTORE VERIFYONLY check were removed. Backup protection is the operator's
   responsibility; this script does not establish that a recovery copy exists.
3. Test on a fresh restore first. Only in a separate restored-test copy, set
   ExpectedServer to that restore's @@SERVERNAME. Keep the distributed live
   file's remaining guards intact.
4. On live, connect SSMS to the reviewed server/database. Use a fresh connection
   without an open transaction. Leave Commit=0 and run the ENTIRE file.
5. Review all grids. Require REHEARSAL ONLY: the proposed ID/reference changes
   have rolled back and the identity has not been reseeded. A rehearsal is not
   a live deployment. Save results/errors; do not remove guards to force success.
6. Set Commit=1 and run the entire file again only after reviewing the rehearsal.
   Require COMMITTED: WO 18882 -> 18879; Order identity 18879; next normal
   allocation 18880. This file performs both changes; no separate reseed is needed.
7. Verify WO 18879 in recall with the original lines, prices, quantities, tax and
   approvals. Reopen clients and verify normal operation. The next successful
   allocation is 18880 only if no other allocation intervenes; failed saves can
   consume identities again. Do not repeat this recovery to fill later gaps.

Use a maintenance window, adequate data/log/tempdb capacity and an appropriate
SSMS execution timeout (for example 1800 seconds). The script locks affected
tables and validates/scans the large item-log history. It can take time. On SQL
errors its catch rolls back and restores trigger state. If execution is manually
cancelled or the connection is lost, inspect transaction/trigger/identity state
before retrying; do not assume cancellation ran the catch.

## References and limits

The mapping covers Order.ID; OrderEntry.OrderID; Queueing.OrderID and GroupTo
(including other queue members referencing that group); SOD_WO_LOGS.OrderId;
SOD_WO_History.OrderID; and order-domain SOD_Item_Logs.ReferenceID. Existing line,
queue, approval and event IDs are retained. One explicit mapping event is appended.
Quantities, exact amounts, stock and commitments are preserved. Rowversions
necessarily regenerate when header/reference rows are written.

OrderHistory, Transaction.RecallID and order Journal.ReferenceID are included in
the maintenance mapping/snapshot infrastructure, but any new posted reference
causes this audited open-order script to stop for re-review. It does not silently
renumber a sale or change a transaction number. Sales/purchase log references and
unrelated sale receipts are preserved even when their number happens to match.

Less common/import/held references, unknown order/reference columns, FKs,
schema-bound dependencies, changed/disabled triggers, replication/CDC/temporal
tables, occupied destinations and orphan references stop execution. Leading-zero
or otherwise noncanonical numeric order-log references in the affected range
also require review rather than being missed during ID reuse.

Existing free-text audit descriptions, business ReferenceNumber, receipt contents,
printed documents and external databases/files are historical and are not globally
searched/replaced. Arbitrarily named custom payload fields cannot be proven absent
from this metadata audit; confirm external consumers before committing.

## Validation

Run `Database Components/Tests/HO_Renumber18882_LocalDB_Test.ps1` on Windows with
LocalDB installed. The saved production server guard remains intact; only an
in-memory test copy substitutes the owned disposable server. Backup checks are
absent from the saved script as explicitly requested.

All 42 synthetic checks passed at compatibility 100, including rollback after
trigger suspension, header replacement and DBCC reseed; changed prices; wrong
target; absence of backup-path/checksum validation; source drift; posted links; newly introduced columns;
unknown triggers; occupied/orphan future IDs; leading-zero references; related
group membership; audit/approval/log mappings; fractional stock; repeated-run
rejection; RMS legacy @@IDENTITY; and actual next automatic allocation 18880.
This is not a live rehearsal, actual RMS POS execution or a full restored-store
acceptance test.

The complete file also passed offline SQL Server 2016 syntax validation. Both
solutions built Release/x86 and all 101 unit tests passed; the Debug store EXE
configuration was preserved byte-for-byte.
