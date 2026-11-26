# Davao Work Order and RMS mixed-user test plan

Prepared October 17, 2026. **Plan only: the 10+ user load test has not been run.**
The current authorization covers live read-only diagnosis and this plan, not
production load testing, migrations, test sales or configuration changes.

## Objective and known regression to reproduce

Demonstrate that the candidate add-in and reviewed database indexes remain
responsive and correct with 10 or more concurrent clients while Work Orders are
saved/edited and RMS users look up tenders/vouchers and settle orders.

The live audit captured two deadlocks between the RMS Item/Tender voucher lookup
and the atomic Work Order Item.QuantityCommitted update. Reproducing that exact
interaction is mandatory. An item-log microbenchmark alone is insufficient.
See [the live findings](DVO_LIVE_READONLY_PERFORMANCE_2026-10-17.md).

## Isolated environment and reproducible baseline

1. Obtain a current authorized Davao backup and restore to a dedicated test SQL
   instance/network. Use clearly owned names such as DVO_PERF_<run>_BASE and
   DVO_PERF_<run>_INDEX. Do not remove the already installed index from the user's
   existing localhost DVO_STORE_DB as an incidental test reset.
2. Record source backup time, SQL version/edition, compatibility, hardware,
   indexes, statistics dates, trigger hashes/states and relevant branch settings.
   Match production characteristics sufficiently for a capacity comparison; a
   small workstation result is not proof of production capacity.
3. Confirm which databases actual Davao RMS registers use. DVO_STORE_DB and
   DVO_POS_DB are not interchangeable; reproduce any required integration only
   inside the lab. Use the DVO profile and actual approved Davao tender mappings.
   Do not copy HO tender IDs or branch rules into the fixture.
4. Use isolated Windows/RMS test registers with the supported x86 add-in and
   matching RMS/QSRules version. Preserve real units' EXE configs and registry
   settings. Configure only lab installations; verify every endpoint before
   traffic and block access to production endpoints/external payment systems.
5. Keep compatibility/isolation, pricing, tax, approvals, stock and lifecycle rules
   unchanged. No NOLOCK/dirty-read substitution, increased timeouts, or automatic
   retries of ambiguous saves/payments. Use synthetic customers/orders/test tenders
   in the restored lab, with ample starting stock for normal throughput tests.
6. Record application commit/build hash, complete deployment-folder hash, RMS
   version, database script hashes and test data seed. Disable only lab external
   integrations as appropriate; do not alter their live settings.

Keep candidates separate so each result can be attributed:

| Stage | Add-in | Database |
| --- | --- | --- |
| A | Current deployed Davao build | Unmodified fresh restore |
| B | Candidate build | Identical fresh restore, same indexes as A |
| C | Same candidate as B | Only the guarded item-log lookup index added |
| D, if separately reviewed | Same candidate | C plus a reviewed RMS voucher-query index experiment |

The voucher index has not yet been designed or approved. Do not use Stage D as
permission to edit RMS SQL or change server-wide MAXDOP/isolation. If production
already contains the item-log index by test time, document that baseline instead
of pretending A is an unindexed comparison. Reset each stage from the same owned
baseline, never by renumbering/reseeding production orders or fixing identity gaps.

## Actors and concurrency ramp

Each actor has a separate process/register and connection. Use real human timing
or a reproducible bounded harness; do not count pooled SQL connections as users.
The counts below are simultaneous client actors, including RMS actors.

| Level | Work Order actors | RMS actors | Total |
| --- | ---: | ---: | ---: |
| Smoke | 1 | 1 | 2 |
| Initial ramp | 3 | 2 | 5 |
| Normal target | 6 | 4 | 10 |
| Main mixed run | 8 | 4 | 12 |
| Higher target, including ten Work Order users | 10 | 5 | 15 |

Start with five minutes of warm-up. Measure at least 15 minutes per target level;
repeat the 10/12/15-client runs three times with identical data/setup and vary
launch order. Use a 1-5 second reproducible think time between ordinary actions,
plus synchronized bursts below. Finish the chosen candidate with a two-hour
12-client soak if short runs pass. Record actual completed operations/minute;
if actors are idle, that is not a meaningful concurrency result.

## Mixed workload and synchronized cases

Use a documented mix: approximately 50% new saves, 25% recalls/edits and 25%
supported imports among Work Order operations. Include 1-6 line orders and
7/12-line split orders. RMS actors alternate normal sales, opening tender/voucher
selection and settling ready Work Orders using actual RMS, not direct SQL inserts.
Keep print-preview/client fetch work in a subset of ordinary user cycles.

| Case | Coordinated actions | Required evidence/outcome |
| --- | --- | --- |
| Distinct items | Work Order actors save different items while RMS operates | Baseline throughput and p50/p95/p99 latency; no unexpected SQL errors |
| Shared stock | Several new/edit saves use the same popular items | Each commitment difference applied once; no lost updates |
| Captured deadlock path | Repeated RMS voucher/tender lookup overlaps Work Order commitment updates; include distinct and shared SKUs | Same RMS SQL shape verified; capture plans/deadlocks; no unexplained stall or victim in a passing run |
| Save versus settlement | One actor holds a recalled edit while actual RMS settles that order | Settled order stays closed; stale save rejected; exact transaction links/quantities/queue status |
| Edit versus edit | Two actors recall the same order, then save different quantities | One valid change; stale editor receives existing conflict handling |
| Save versus cancel | Coordinate recalled save and cancellation | Cancelled order cannot reopen; stock released once |
| Repeated cancel | Repeat the existing cancel path on an already cancelled order | No second release, duplicate queue close or misleading success |
| Import versus RMS | Several supported imports/split saves overlap settlement and voucher lookup | Same imported rows/prices/tax, valid split IDs, no duplicate lines/transactions |
| Validation | Blank Work Order payment, changed prices, approval cancellation; equivalent quotation case | Payment first only for Work Orders, existing price/password and quotation rules unchanged |
| Stock/branch exceptions | Approved shortage flow, prepared/fractional quantities, noninventory items | Existing Davao rules retained; no clamping or silent quantity changes |
| Failure/rollback | Owned fixture failure midway through save, then a valid save | Header/lines/totals/commitments/history roll back together; next actor proceeds |
| Committed save then UI failure | Simulate a post-commit print/refresh failure only in an owned fixture | Saved order retained once; no second insert on retry/reconciliation |
| RMS abort/error | Cancel tendering and exercise a controlled lab error | No duplicate financial posting; determine commit status before repeating a payment |
| Session recovery | Close/reopen test clients between runs | No abandoned transaction/locks, retained correct app settings |

The controlled failure cases require explicit fixture implementation before
execution; they are not instructions to kill sessions or inject errors on live.
Do not bypass passwords, stock checks or RMS settlement to speed up the test.

## Correctness ledger

Maintain a per-operation ledger with actor/register, run ID, UTC timestamps,
order ID, transaction number, requested/accepted line quantities, outcome and
approval/settlement/cancellation state. Reconcile after each burst and measured
run, including:

- Header/lines/totals/tax/rounding/customer exemptions match existing calculations.
- Committed stock equals the starting fixture baseline plus the exact ledger
  of accepted commitment changes under existing rules. Physical stock changes
  only through the corresponding RMS actions. Preserve fractional/noninventory
  behavior and prepared quantities; do not recalculate or rewrite all store stock.
- Legacy RMS identity lookup and every transaction/recall/order-entry link are
  correct; no audit IDENTITY value is substituted. Gaps alone are not corruption.
- Active Pending/Processing/Prepared/For Invoicing orders remain visible/countable.
  RMS completion closes through Status=1; cancellation retains its separate
  Status=2 behavior. No duplicate history events or stock releases.
- Every reported committed save/payment exists once. Every aborted operation
  has no partial business writes. Expected stale-edit rejections are recorded
  separately from unexpected SQL deadlocks/timeouts.

Never auto-resubmit an uncertain tender/save. Reconcile its order/transaction
state first using the same existing recovery/support process.

## Measurement and pass/fail

Measure end-to-end Save click to saved-number confirmation, RMS tender-confirm
to completed transaction, recall to loaded order, search/list availability and
import completion. Also record server query duration/CPU/reads, blocking chains
and lock duration, deadlocks, waits, runnable queues, memory grants, file/log I/O
and client/network time. Keep client fetch/printing time distinct from SQL time.

Use the SELECT-only collector for short intervals around each burst on the lab
server. Existing system_health events can roll out; retain lab deadlock evidence
per run. A dedicated lab-only monitoring session can be designed separately if
necessary; none was created on live. Match timestamps/host/register to SPIDs,
including sleeping head blockers with open transactions.

Proposed engineering targets, not business-rule changes or measured guarantees:

- Zero data/financial reconciliation errors, unexpected SQL deadlocks or command
  timeouts in a passing sustained run; expected business validation remains intact.
- Typical 1-6 line saves and normal RMS settlement p95 <=3 seconds; split/import
  workloads tracked separately with a provisional p95 <=8 seconds.
- Recall/list availability p95 <=2 seconds as an improvement target. Compare
  with baseline; do not remove results or required validations to reach it.
- Flag blocking >5 seconds; stop increasing load and retain evidence if a stall
  exceeds 15 seconds, an operation's commit status is uncertain, or any correctness
  check fails. Do not kill production sessions or increase timeouts as a response.
- Compare medians and p95/p99 across three runs, throughput and tail stalls, not
  only average SQL lookup time. Check index write overhead and startup/cold versus
  warm behavior. Success must hold under the shared-item/tender burst, not just
  distinct-item steady traffic.

Resolve target thresholds against real workstation/network conditions before
using them as a release gate. Do not claim that observed quiet-time server metrics
or the older June restore prove the 10+ user goal.

## Existing automation and execution boundary

MultiUser_Save_LocalDB_Test.ps1 provides 39 checks per isolation mode with separate
cashier processes and a **simulated** settlement participant. It is not a 10-user
capacity harness and does not execute RMS POS/QSRules. ItemLogLookup_LocalDB_Test
verifies migration guards/lookup results, not actual RMS concurrency. These remain
useful prerequisite regression tests, not substitutes for the mixed actual-RMS run.

Deliver a run manifest, scenario matrix, client timing CSV, correctness ledger,
deadlock/plan evidence, before/after performance table and deployment recommendation.
Record which tests actually ran and distinguish simulation from actual RMS.
Only then prepare a separately reviewed maintenance deployment and exact index
rollback plan for the confirmed live Davao target. No live change is authorized
by this plan alone, and unrelated updater scripts must remain unselected.
