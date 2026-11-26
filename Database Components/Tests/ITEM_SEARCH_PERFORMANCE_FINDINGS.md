# Item search investigation — October 15, 2026

## Outcome

No SQL search speed fix is enabled from this investigation. The experimental
query builder was removed. The application retains its existing contains search,
keyword conjunction, inactive filter, unordered TOP 500 and fresh database reads.
The previous bold-grid and responsive-search UI changes remain.

The reported BOOK followed by PAPER delay reproduces in the SQL read itself on
localhost / GSC_STORE_DB. The older reference project's SearchItem builds the same
SQL predicates; there is no evidence that a new matching rule caused the delay.
The measurements do not establish why an older deployed EXE took one second.

## Read-only local observations

- Item contains 69,357 rows; database compatibility is 100, with
  SQL_Latin1_General_CP1_CI_AS collation. These settings were not changed.
- BOOK + PAPER returns 382 rows. Repeated original-query reads commonly took
  roughly 2.5–5.1 seconds, with occasional longer outliers.
- One diagnostic series returned the same ordered values at 3.18 seconds
  and then 1.20 seconds on a warmer repeat. The same 249,578 LOB logical reads
  occurred both times; physical LOB reads dropped from 446 to zero. Cache/load
  effects matter, so a single one-second run is not proof of an optimization.
- The final script, using the application's exact SELECT text, measured 2.99
  and 3.76 seconds for BOOK + PAPER with identical ordered result hashes.
- BOOK + NOTE reaches 500 rows in roughly 0.3–0.6 seconds.
- The sampled view plan scans Item. One sampled lookup reported 216,723 LOB
  logical reads. The view assembles FULLDESC using a conversion from the legacy
  ntext ExtendedDescription column. Leading-wildcard LIKE cannot be treated as
  an ordinary prefix lookup without changing matching semantics.
- Timings vary with caching and machine load. They measure SqlDataAdapter.Fill,
  not typing, the entire UI interaction, a live branch, or multi-user capacity.

Final validation: both Release/x86 solutions built, all 101 unit tests passed,
and the existing compiled search helper passed its 21 isolated LocalDB checks.
The read-only timing script was exercised on local GSC with matching ordered
result hashes across repeats. The store EXE configuration was restored exactly.

## Alternatives rejected

1. Materializing all first-keyword rows in a session temporary table reduced
   BOOK + PAPER to about 1.2–1.5 seconds in most prototype reads. However, it
   slowed the broad 500-result example to about 1.1–1.4 seconds, and presentation
   order differed. One narrow-search run also took about 7.5 seconds. It was
   not accepted as a consistent overall improvement.
2. An indexed calculated search-text prototype, tested on disposable copies of
   local Item data in tempdb, gave about 0.9 seconds when the copied view used
   an internal index hint. The broad query returned a different 500-item set,
   even after comparing by ID. The old query has no ORDER BY, so changing the
   access path can change the cutoff. The index/view change was not deployed.
   Indexed computed columns would also require reviewing RMS writer SET options.
3. Predicate reversal gave only a small apparent gain in one series; recompile,
   serial execution, CASE, CHARINDEX and other expression rewrites did not
   demonstrate a reliable improvement. No such hint or rewrite remains enabled.

All store queries were reads. Disposable copy objects were removed. No store
view, index, compatibility setting, statistics, timeout, stock, order or business
rule was changed. No updater script is needed or offered as a proven search fix.

## Repeatable timing

Use Windows PowerShell with a Windows login allowed to read the specified test
database and its view definition:

```powershell
& '.\Database Components\Tests\ItemSearch_ReadOnly_Timing.ps1' `
    -Server localhost -Database GSC_STORE_DB -Repeats 3
```

This runs BOOK, BOOK + PAPER, PAPER and BOOK + NOTE using the original query
shape. It saves durations, row counts, hashes of all ordered values, target
metadata and SQL IO/time messages under ignored TestResults/ItemSearchTiming-*.
It exports no item rows or credentials, and uses Windows authentication only.
The 30-second command timeout remains bounded. Run in a quiet period: repeated
reads can still consume server resources. Changes between result hashes may
reflect concurrent stock/item changes or the unordered cutoff; do not treat
them automatically as a matching bug.

The next controlled experiment is a disposable restore preserving the store's
physical layout: inspect Item/LOB storage and statistics, then measure any
maintenance before and after with this script. Compare the returned item IDs,
values and 500-row cutoff as well as elapsed time. Do not run unproven maintenance
on a live branch or claim the one-second target is achieved from these findings.
