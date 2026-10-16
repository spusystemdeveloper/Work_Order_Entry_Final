# Shared instructions for AI and human contributors

Read this file before changing this repository. It is the shared project guidance
for Codex, Claude, Gemini and other collaborators. Also read [GEMINI.md](GEMINI.md)
for retained RMS incident knowledge and [UNIT_TEST_GUIDE.md](UNIT_TEST_GUIDE.md)
for validation details. Tool-specific instruction files should reference these
rules instead of maintaining competing copies.

## Scope and working process

- Preserve existing Work Order business rules unless the human explicitly asks
  to change them. A request to refactor, optimize, fix a defect or add tests does
  not authorize changing pricing, stock, tax, approval or order lifecycle rules.
- For an audit-only request, report findings before editing. For implementation,
  trace the actual callers, SQL routines, triggers and branch settings first;
  explain any proposed behavior change before implementing it.
- Keep changes focused and simple. Do not upgrade frameworks, replace the data
  layer, delete features or introduce new dependencies as incidental cleanup.
  A deprecated label alone does not prove a feature is unused: customer-type
  pricing and website import settings still have active consumers.
- Check `git status` first. Preserve collaborators' uncommitted work. Do not
  reset unrelated files, commit, push, merge or publish without authorization.
- Update `CHANGELOG.md` and the relevant usage/test documentation. In the final
  report, state what changed, which checks ran, their results and any limits.
  Never report a simulated test as a successful live-store test.

## Supported environment

- Work Order Entry, its tests and Database Updater use .NET Framework 4.7.2;
  Order Processing Queueing uses .NET Framework 4.0 Client Profile. Keep x86
  compatibility. A .NET 10 migration is outside routine maintenance scope.
- Store SQL targets SQL Server 2016. Keep scripts compatible with that version.
  Convert `ntext` fields such as `Item.ExtendedDescription` to `nvarchar(max)`
  before string operations. Bracket `[LineNo]`, `[Date]` and `[timestamp]`.
- Inspect LINQ-to-SQL mappings before changing persistence. Keep DBML and generated
  mappings consistent; do not bypass rowversion conflict checks to fix an error.

## Business rules to preserve

- Keep header, lines, totals, inventory commitments, queue changes and associated
  history in the existing save transaction. Preserve order locks and stale-edit
  checks. A cancelled or RMS-settled order must stay closed; a stale save must
  not reopen it, and repeated cancellation must not release stock twice.
- Update commitments atomically by the quantity difference; do not read a value
  and overwrite it with a locally calculated total. Preserve non-inventory item
  exemptions and fractional quantities. Do not confuse physical stock, available
  stock, committed stock and warehouse-prepared counts.
- Preserve shortage acceptance, confirmation, prepared quantity validation and
  existing undo behavior. A shortage flag does not authorize silently reducing
  an order or releasing another order's reservation.
- Preserve customer price levels, price bounds, approval/password requirements,
  discounts, VAT calculations, rounding and RMS customer tax exemptions.
  `EnableSetTaxButton` controls the manual Set Tax action only and defaults to
  enabled when missing; it does not disable tax calculations or exemptions.
- Pending, Processing, Prepared and **For Invoicing** orders remain active queue
  stages. Keep For Invoicing visible and counted. Normal completion through RMS
  settlement sets `Queueing.Status = 1`; explicit cancellation uses its existing
  separate path (`Status = 2`). Do not close a queue merely because it is prepared.
- Treat failures after a committed save separately from a failed transaction.
  A print or refresh failure must not cause a second insert of an already saved
  order. Do not increase timeouts as a substitute for investigating slow SQL.

## SQL, identity and store safety

- RMS uses legacy `SELECT @@IDENTITY`. Audit triggers, including nested queue
  triggers, must not insert into IDENTITY audit tables. Use sequence-backed
  history identifiers and test legacy identity lookup, not only `SCOPE_IDENTITY()`.
- `Enable_WorkOrder_History.sql` handles fresh setup and approved legacy history
  conversion atomically, preserving events, IDs and permissions. Keep the older
  identity fix script for recovery compatibility; do not require a second updater
  selection for normal history deployment.
- The October 14 history incident was introduced by our custom history IDENTITY
  interacting with RMS. Read the incident record linked from GEMINI.md before
  touching that area. Do not blame RMS alone or reseed its identities as a fix.
- Identity gaps can occur after rollback. Do not fill gaps, renumber orders or
  reseed automatically. Exceptional recovery is a separate explicitly requested,
  guarded operation, with all references checked.
- Do not execute repairs, migrations or destructive tests against live databases
  without explicit authorization to execute that operation on that target.
  Preparing a script is not authorization to run it. Use owned disposable
  fixtures for automated writes; do not test live saves by rolling them back.
- Preserve backup, target, dry-run and commit guards in recovery scripts. A live
  rollback rehearsal still needs its required verified backup. Do not relax
  production guards to accommodate a local fixture or an incomplete backup.
- Branch objects and tender IDs are not interchangeable. Inspect the selected
  SQL and intended store profile. Do not blindly apply incident-specific example
  IDs or one branch's rules to another branch. Follow the updater's transaction
  contract; imported scripts must not start or commit their own transaction.
- Never add credentials to code, documentation, logs or test output. Avoid
  dumping existing connection strings. Preserve each installation's configuration
  and registry settings; test setup must not point the real app at a fixture.

## Validation and deployment

- Build both `Work Order Entry.sln` and
  `ORDER PROCESSING QUEUEING/OrderProcessingQueueing/OrderProcessingQueueing.sln`
  in Release/x86, then run `./Run-UnitTests.ps1` on Windows with the required
  Visual Studio build tools. Consult the testing guide for prerequisites.
- Preserve the existing Debug EXE configuration byte-for-byte around builds/tests;
  a test-project build can overwrite the local store's configuration. Do not
  include store configuration changes accidentally in a deployment or commit.
- For grid styling or Designer checkbox changes, preserve Boolean checkbox
  NullValue defaults and run `Database Components/Tests/RecallFormatting_UI_Test.ps1`
  after a Debug build. A text NullValue can break blank checkbox formatting during
  recall. Fix the style/type mismatch; do not suppress production DataErrors.
- For save, cancellation, settlement, locking or commitment changes, run
  `Database Components/Tests/MultiUser_Save_LocalDB_Test.ps1` both normally and
  with `-VersionedReads`. Versioned reads belong only to that disposable fixture;
  do not change store isolation settings to make a test pass. Its settlement
  participant is a simulation, not execution of RMS POS/QSRules.
- For history/trigger identity changes, run `HistoryIdentity_LocalDB_Test.ps1`;
  for updater changes, run `DatabaseUpdater_LocalDB_Test.ps1`. These scripts are
  in `Database Components/Tests`. Run other relevant checks from the test guide.
- For packaging, dependency, import or reporting changes, run
  `Database Components/Tests/Packaging_CleanFolder_Test.ps1`. ReportViewer 15
  requires its matching five DLLs beside the EXE; keep `Microsoft.ReportViewer.*`
  excluded from Costura embedding. Deploy the complete output folder while
  retaining the target unit's store-specific EXE config. The updater is a separate
  executable; copying application files does not migrate the database.
- If a check cannot run, report the exact missing prerequisite and remaining
  validation. Do not remove safeguards, weaken assertions or invent pass results.
  Guidance files support review; they do not enforce branch protection or CI.
