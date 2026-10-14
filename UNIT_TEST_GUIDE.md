# Work Order Entry unit tests

These tests check small business rules without opening the application,
connecting to DVO/GSC, or changing database records.

## What is tested

Five timing-helper tests were removed with the retired application logger,
leaving 101 unit tests. Three overlay regressions check that ignored owners,
disposed owners and handle-creation failures retain correct nested scope behavior.
The tests use hidden synthetic forms without a real app or database.

For Release deployment dependencies, run:

```powershell
& '.\Database Components\Tests\Packaging_CleanFolder_Test.ps1'
```

Build Release first. This copies only the application EXE and its five packaged
ReportViewer DLLs into an owned `TestResults` folder, compiles an x86 probe and
checks compilation of the actual Import entry point, CSV import, XLSX import,
single Excel dependency loading, WO report rendering, pick-list report rendering,
physical ReportViewer loading and the absence of extra dependency DLLs. PDFs use
synthetic data. No store SQL, app login, screen automation or print job is involved.
The check reproduced the old embedding failure before the DLL packaging fix.

Two additional disposable SQL suites cover the October 14 identity incident:

```powershell
& '.\Database Components\Tests\HistoryIdentity_LocalDB_Test.ps1'
& '.\Database Components\Tests\HO_RecoveryScript_LocalDB_Test.ps1'
```

The first passed 27 checks: actual legacy `@@IDENTITY` reproduction, preserving audit
events/permissions, conversion rollback, custom-schema rejection, nested queue
triggers, and fresh/repeated sequence installation. It also tests one-script
legacy upgrades through the updater, rollback of both conversion and trigger
updates, and a later trigger compilation failure. The second passed 27
checks running the entire standalone HO SSMS script on synthetic data, including
default rehearsal rollback, explicit commit, unchanged unrelated old invoice
records, tax/receivable/payment links, repeat runs, optional log references and
rejection after subsequent receivable activity. Five additional checks exercise
copied non-checksummed backups without local history, local-mode rehearsal,
wrong-machine rejection, missing-file rejection and unchanged live requirements.
Five further checks verify the local backup helper, its copy-only/no-checksum
settings, accepted output path, rejection outside the local test machine, and
stopping after backup creation fails.
Three path-editing checks cover unedited placeholders, whitespace-only values,
and successful rehearsal after replacing every occurrence of the placeholder.
Both own and remove their LocalDB
instances. Neither suite writes to live HO_PO_DB.

The separate database updater has its own disposable integration checks:

```powershell
& 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoProfile -ExecutionPolicy Bypass -File '.\Database Components\Tests\DatabaseUpdater_LocalDB_Test.ps1'
```

Build the Debug updater first (included in `Work Order Entry.sln`). These checks
create and remove their own LocalDB instance; they never use store databases or
application credentials. See [the updater guide](Database%20Updater/README.md)
for deployment, rollback, and snapshot limitations.

All 63 updater checks passed on October 14, 2026. The included user-guide SQL
examples are executed through the actual updater runner, including repeat
deployment and function body/verification changes, on its disposable fixture.
Sixteen additional checks exercise HO_PO_DB's focused password-slot column
migration, including preserved values, target/type rejection, repeat deployment,
and rollback on later failure. This HO_PO_DB exists only in the owned LocalDB
instance; the tests do not connect to the live HO server.

- Price labels map to the correct RMS price-level number.
- The Price Level form initially highlights its top row.
- Multiple customer-change prices are counted and processed one row at a time.
- Matching branch database names are accepted and mismatches are rejected.
- Work order type `2` selects the Work Order print template.
- Quotation type `3` selects the Sales Quotation print template.
- Unknown order types are rejected instead of using the wrong template.
- Empty print-template names fall back to Work Order; explicit names are preserved.
- Quotation/Sale, Transfer/Purchase, and WEBSITE filenames select the correct import workflow.
- Unrecognized import filenames are rejected.
- A numeric senior-citizen ID adds the expected description suffix.
- VAT-inclusive pricing separates VAT correctly.
- Tax-exempt pricing removes VAT.
- Non-taxable items do not add VAT.
- Queue order-age rules select priority at 10 and 20 minutes, format elapsed
  time, and select badge text and header colors, including disabled alerts.
- Error logging handles ordinary errors, nested exceptions, and null exceptions.
- Edit-conflict validation accepts unchanged recalled state and rejects changed
  state, missing baselines, different orders/databases, and empty fingerprints.
- Ready notification tracking reports new prepared groups once, deduplicates
  rows, handles groups returning to Prepared, and preserves state after failure.
- Warehouse shortage tracking handles new/reflagged orders, changed counts,
  duplicate/reordered lines, missing/zero counts, failures, and empty-list selection.
- Local shortage filters combine search/new/zero/picker criteria, treat search
  punctuation literally, preserve the loaded quantities, and restore alert priority.
- The extracted reservation rule keeps new/update/remove/conversion arithmetic,
  fractional quantities, quotation behavior, and non-inventory exemptions unchanged.
- Shared price approval retains the saved below-minimum price on recall, requires
  approval for changed/new below-minimum prices, and distinguishes a saved zero
  price from a missing line.
- Shortage acceptance uses the recorded physical count, releases the difference
  without rounding fractional reservations, preserves the picker, and rejects
  changed counts, duplicates, zero/missing counts, closed orders, quotations,
  non-inventory items, and lines already prepared. Agreement requires an
  unchecked-by-default consent box and a nonblank customer note.

The current project contains 101 test methods. All 101 passed on October 14, 2026.

Many test names describe the scenario in **Given / When / Then** form:

- **Given** describes the starting information.
- **When** describes the action.
- **Then** describes the expected result.

Example:

> Given two taxable items priced at 112.00 each, when their price is
> calculated, then the result is 200.00 VATable sales plus 24.00 VAT.

## Run the tests

From the repository root (the directory containing `Run-UnitTests.ps1`):

```powershell
.\Run-UnitTests.ps1
```

The runner requires MSBuild and VSTest at the Visual Studio 18 Community
paths configured in `Run-UnitTests.ps1` and builds the tests for x86.

A successful run displays each test as `Passed` and ends with a summary such
as:

```text
Passed: 103
Failed: 0
```

If a future change breaks a rule, the relevant test displays `Failed`, its
readable test name, and the expected and actual values.

## What these tests do not do

The unit tests do not exercise SQL functions, stored procedures, saving,
recall, inventory commitment, actual database locks, or queue processing.
`Database Components/Tests/OrderTools_LocalDB_SmokeTest.ps1` separately tests
history triggers, edit checks/locks, ready eligibility, and shortage save/rollback
in an isolated fixture. Its 99 checks include preserving stock on hand and other
orders' reservations while releasing only the accepted shortage difference.
It also checks the RMS smallint item type, fresh queue refresh/shortage writes,
clear-shortage behavior, and card stage changes. Build both solutions in Debug first.
Six tax-save checks reproduce the old rowversion conflict, execute corrected
production tax preparation, verify voucher behavior/rollback, clear manual
reasons when returning to taxable, and preserve quotation behavior.
The October 13 recall checks compare batch prices to the legacy selection rule,
test quoted codes, customer isolation, zero-price fallback, returns, missing
history, duplicate catalogue codes, transaction tie-breaking, and bounded query counts for 201 items. They
verify fresh price/warehouse changes, snapshot conflicts during recall, competing
save locks, and rejection of a second cashier's old version after the first commits.
These are overlapping database-session and conflict tests, not a production
capacity/load test with many workstations.
Ten October 14 checks compare production row tax application in legacy and
batch modes, preserving Boolean types, quoted/untrimmed codes, missing and blank
items, fresh tax changes, parameter bounds, exemption behavior, and existing
taxable/exempt totals. Fixture totals timings use an unshown form without a
credit-warning dialog; they do not measure a full live recall.
Five lookup-index checks add synthetic wide items and sale history, compare the
same production price query before/after the index, verify fewer logical reads
without timing thresholds, and check idempotence, equivalent-index reuse, and
safe handling of a conflicting name. All writes remain inside the disposable fixture.
See `ORDER_TOOLS_GUIDE.md` for that test and branch deployment checks. Full RMS workflows require
integration tests against a disposable RMS database restore—never a live
production database.

## Check the save refactor manually

For the October 14 batched tax lookup change, restart the rebuilt app and recall
an ordinary taxable order, an exempt order, and a quotation. Verify the row flags,
prices, quantities, VAT/subtotal/total, and existing credit-limit warning behavior.

For the October 13 recall change, restart the rebuilt app and recall a normal
multi-line Work Order. Verify its quantities, last purchase prices, prepared
counts, shortage highlighting, picker, totals, and tax setting. Recall a quotation
as well. The underlying business behavior should match before the change.

With two test sessions, recall the same disposable order in both; save in one
and confirm the other session's stale save is blocked. Make a warehouse shortage
change and use Reload order in the cashier session to verify the latest count
and picker appear. Avoid making production changes solely for a load test.

Use the updated Work Order Entry executable with disposable test orders:

1. Save a normal new order, then recall it, change a quantity, and save again.
   Confirm the order total, reservation, and queue quantity match the changes.
2. Choose **No** at the save confirmation. Confirm the order stays on screen and
   no database changes or success message appear.
3. Recall an order with a previously approved price below the minimum. Saving that
   same price should keep the existing approval behavior. Changing it to another
   below-minimum price or adding a new below-minimum line should require approval.
4. Accept a warehouse shortage and save. Confirm the reduced quantity, total,
   reservation difference, prepared state, and original picker are preserved.
5. Change the recalled order from another session, then try saving the older
   screen. The existing conflict warning must still block the save.
6. Check quotation saving/conversion and, where enabled, order splitting, grouping,
   and printing. These workflows should behave as before.
7. Select a regular taxable customer, add an item, use **Set Tax** to make the
   order exempt, and save. It should save without "1 of 2 updates failed".
   Recall it and confirm the exempt tax setting remains. Also test returning
   the order to taxable and saving a tax-exempt quotation.

The isolated SQL fixture does not execute every main-form save prompt or the full
RMS stored-procedure workflow. These manual checks cover that remaining scope.
See `ORDER_TOOLS_GUIDE.md` for the refactor checklist; no database migration is
required by this refactor.
