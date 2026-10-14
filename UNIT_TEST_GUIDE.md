# Work Order Entry unit tests

These tests check small business rules without opening the application,
connecting to DVO/GSC, or changing database records.

## What is tested

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
- Optional timing produces one record per scope, uses consistent decimal formatting,
  creates no files when disabled, and cannot interrupt processing if logging fails.
  Excluded dialog waits cannot make processing time negative, and invalid wait
  durations cannot corrupt a timing record.

- Shortage acceptance uses the recorded physical count, releases the difference
  without rounding fractional reservations, preserves the picker, and rejects
  changed counts, duplicates, zero/missing counts, closed orders, quotations,
  non-inventory items, and lines already prepared. Agreement requires an
  unchecked-by-default consent box and a nonblank customer note.

The current project contains 103 test methods. All 103 passed on October 14, 2026.

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
in an isolated fixture. Its 94 checks include preserving stock on hand and other
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
See `ORDER_TOOLS_GUIDE.md` for that test and branch deployment checks. Full RMS workflows require
integration tests against a disposable RMS database restore—never a live
production database.

## Check the save refactor manually

For the October 14 timing/tax lookup change, restart the rebuilt app and recall
an ordinary taxable order, an exempt order, and a quotation. Verify the row flags,
prices, quantities, VAT/subtotal/total, and existing credit-limit warning behavior.
When logging is enabled, `RecallItemTaxReadAndApply` and `RecallTotalsCalculation`
separate the two processing steps. `RecallCreditNoticeWait` records credit-warning
wait time; it is excluded from the new overall/tax/totals processing measurements.
Older logs include that wait and are not directly comparable when a warning appeared.

For the October 13 recall change, restart the rebuilt app and recall a normal
multi-line Work Order. Verify its quantities, last purchase prices, prepared
counts, shortage highlighting, picker, totals, and tax setting. Recall a quotation
as well. The underlying business behavior should match before the change.

With two test sessions, recall the same disposable order in both; save in one
and confirm the other session's stale save is blocked. Make a warehouse shortage
change and use Reload order in the cashier session to verify the latest count
and picker appear. Avoid making production changes solely for a load test.
When timing is enabled, inspect `RecallReadAndPopulate` and the component timings.
The prior sample for order 282844 was 2.245 seconds, but older records can include
credit-notice wait. Account for that measurement change before claiming a speed gain.

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
See `ORDER_TOOLS_GUIDE.md` for optional timing logs; no database migration is
required by this refactor.
