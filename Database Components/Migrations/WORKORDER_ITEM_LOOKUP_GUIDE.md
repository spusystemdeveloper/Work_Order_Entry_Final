# Optional Work Order item lookup index

`Improve_WorkOrder_ItemLookup.sql` adds a non-unique index for item-code lookups,
covering the item ID and tax flag. It helps recall-price and item-tax reads on
stores whose Item table has no suitable existing index. It changes no items,
prices, quantities, Work Order rules, or application settings. No new executable
is required, and the branch drivers do not run this optional migration automatically.

## Deployment

1. Test against a disposable copy of the intended RMS store database first.
2. Choose a quiet period. Index creation takes locks and can briefly block RMS
   activity. Record whether this deployment creates a new index or reuses an old one.
3. Capture SELECT-only baseline diagnostics with
   `Database Components/Tests/RecallPrice_ReadOnly_Analysis.ps1`. Use explicit
   `Server`, `Database`, and `OrderIDsText` arguments plus `RunLabel` set to
   `Baseline`. Refresh it immediately before deployment when the store has been active.
4. Connect to the intended store database and run `Improve_WorkOrder_ItemLookup.sql`.
   Run the same SELECT-only diagnostics afterward with `RunLabel` set to `AfterIndex`.
5. Compare price signatures, Item logical reads, and repeated times. Then recall
   the same orders in the app and confirm prices, quantities, totals, and shortages.

The script skips creation if a usable unfiltered index already has ItemLookupCode
as its leading key and covers ID/Taxable. It does not replace or drop indexes.
If the proposed name already exists with another definition, it stops for review.
The new index permits existing duplicate item codes and adds no uniqueness rule.

## Rollback

If this deployment created `IX_SOD_WO_ItemLookup`, a DBA can remove that specific
new index to restore the prior lookup behavior. Do not remove an existing index
that the migration merely reused, or an index with a conflicting definition.
No business records need to be restored because the migration performs no DML.

## Evidence and limits

Local GSC plans on October 14 scanned 69,357 Item rows and read 5,163 Item pages
per price query. In a separate synthetic wide-table fixture, the index reduced
Item logical reads from 4,645 to 7 and returned the same prices. All 103 unit
tests and 99 isolated SQL/UI checks passed; both solutions built in x86 Debug
and Release. Fixture SQL used LocalDB 17 at compatibility level 130.

Synthetic read reduction is not a measured live-store speed or capacity guarantee.
First-call initialization, query compilation, active workload, and UI processing
still affect full recall time. Customer-specific prices, CH/CI priority, zero-price
fallback, positive-quantity filtering, and transaction ordering remain in the app query.

The index was applied to the local `GSC_STORE_DB` on October 14 after verifying
no active user transactions; a three-second lock-wait limit protected deployment.
Creation took 410 ms. Plans then used item-code seeks with 6/12 Item logical reads
instead of 5,163, and all price signatures matched. Warm lookup medians improved
from 37.889 to 1.434 ms for WO 283845 and 40.622 to 3.972 ms for WO 282844.
This local database change does not require replacing the application executable.
