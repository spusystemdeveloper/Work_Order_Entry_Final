# Work Order save and RMS settlement investigation

Recorded: October 18, 2026. Times below are UTC+8.

**Status: PAUSED at the user's request; not resolved or approved for deployment.**
The user chose to leave the RMS investigation for now and return to add-in work.
This document consolidates the source audit, disposable SQL tests and actual
localhost RMS results. It supersedes earlier phase notes describing the original
trigger deadlock test as pending.

## Scope and current state

- No live company database was changed by these tests.
- Initial experiments used owned disposable LocalDB fixtures. Later actual POS
  tests used the existing **localhost / MSI / DVO_STORE_DB** restore, explicitly
  authorized by the user while disconnected from the company network. Those
  actual POS tests were not performed on another disposable database.
- Actual POS testing used SQL Server 2016, version 13.0.1742.0, and Store
  Operations POS 2.00.2022. The user operated Recall and Cash settlement;
  database inspection and tracing supplied independent evidence.
- The original `dbo.tr_TransactionEntryComputedQuantity` remains unchanged.
  The candidate was tested only in disposable SQL fixtures, never installed in
  the existing DVO/GSC restores or a live database and never packaged for deployment.
- Final saved verification found no owned fault triggers, gate tables or tracing
  sessions remaining. Marked test orders and incomplete postings were retained
  as evidence; no repair, reopening, deletion or identity reseed was performed.
- Assistant testing did not switch normal application configuration or registry
  settings. No application source or business-rule change was implemented.

## Why this trigger was investigated

The read-only audit found an existing enabled AFTER INSERT trigger on
`dbo.TransactionEntry`, named `tr_TransactionEntryComputedQuantity`, in the
localhost DVO and GSC databases. It existed before this investigation. Its
original author, installation date and status as a Microsoft-supplied object
have **not** been established.

Its calculation reads physical stock and transaction-line quantities, then
updates ComputedQuantity using the existing ID, ItemID and rowversion predicate.
Its surrounding transaction/error handler performs an unqualified rollback on
failure and retries errors 1205, 1204 and 1222 after a 500 ms wait, with a retry
limit of 50. The concern was retrying inside a trigger after rollback of the
transaction in which the caller was posting the sale.

The experimental candidate retained the exact declarations, calculation and
rowversion-guarded UPDATE body. It removed the internal transaction and
rollback/retry/error-replacement wrapper, allowing the original SQL error to
reach the caller. It did not change the quantity formula or stock, pricing,
tax, approval or order-lifecycle rules. Whether actual RMS can safely handle
that candidate remains untested.

## Results at a glance

| Test | Result | Limit |
|---|---|---|
| Initial save/settlement audit | Save transaction and locking paths traced; sampled records checked | Read-only sample, not a complete integrity audit |
| Disposable original-trigger reproduction | 10 cases; 55 main checks and 14 artifact checks passed | Synthetic caller, not POS |
| Disposable original/candidate comparison | 32 cases, 16 matched pairs; 191 assertions passed | Candidate validated only with synthetic caller rollback |
| Actual RMS normal Cash settlement | WO 23292 settled correctly; 15 verification checks passed | One local normal settlement |
| Actual RMS forced lock timeout, original trigger | Real SQL 1222 followed by retry and 3609; partial posting persisted | Controlled local fault, not a production incident |
| Actual RMS forced deadlock, original trigger | Real SQL 1205 and deadlock graph, followed by 3609; partial posting persisted | Controlled local fault, not a production incident |
| Actual RMS candidate timeout/deadlock | **NOT RUN** | Investigation paused |

## Actual RMS normal settlement

At 15:11:58 the user settled WO **23292** using Cash **PHP 224.00**, for two
units of item **70854 / ZZRMS18E5150B**. Control WO **23293**, quantity three,
was deliberately left open.

- Physical stock changed **10 -> 8**, commitments **5 -> 3**.
- Target order closed with zero outstanding quantity and QuantityRTD 2;
  its queue became Status 1. The control order and reservation stayed active.
- Sale **24688**, line **80909**, Cash tender **22078**, tax **80904** and
  native OrderHistory **21574** were linked correctly. Total was 224, VAT 24,
  and ComputedQuantity was **8**. No accounts receivable was created for Cash.
- The trace retained 416 events without dropped/truncated events and contained
  no 1204, 1205, 1222 or 3609. It also included informational database-context
  events and temporary-table probe errors; it was not globally error-free.
- All 18 captured trigger definitions/states remained unchanged. The owned
  trace was saved, stopped and dropped.

## Actual RMS forced timeout: original trigger

At approximately 15:23:06 the user attempted Cash 224 once for WO **23294**,
item **70855 / ZZRMS79CE964B**, quantity two. Control WO **23295** stayed open.

The screenshots showed Microsoft Cursor Engine **-2147217864 / NativeError 32**:
“Row cannot be located for updating. Some values may have been changed since it
was last read,” in PostTransactionEntries, followed by **run-time error 5**.
Later inspection found no active Store Operations SQL session; the helper did
not kill POS.

The intact 156-event trace recorded the original computed update, SQL **1222**
at 15:23:06.999, rollback, 500 ms wait, a second update, COMMIT and SQL **3609**
at 15:23:07.528. Persisted state was verified at 15:24:27:

| Record | Persisted result |
|---|---|
| WO 23294 / entry 92660 | Closed; outstanding 0, QuantityRTD 2; total 224, VAT 24 |
| Item stock / commitments | Physical **10 unchanged**; committed **5 -> 3** |
| Queue 25 | Status 1, completed |
| Sale header 24689 | Exists, RecallID 23294, RecallType 4, total 224 |
| Sale lines / tender / tax / native OrderHistory / receivable | **0 / 0 / 0 / 0 / 0** |
| Control WO 23295 / queue 26 | Open, quantity 3; queue Status 0; reservation preserved |

Typing Cash into the dialog did not produce a TenderEntry. This is evidence
about the marked local test, not a finding about real customer money.

## Actual RMS forced deadlock: original trigger

The user next attempted Cash 224 once for WO **23296**, item
**70856 / ZZRMS4604FDE6**, quantity two, leaving control WO **23297** open.
The attempt began around 15:26:46. The user reported a POS error but did not
provide its exact text; the timeout screenshot above must not be attributed
to this deadlock case.

SQL evidence independently confirms a genuine two-session deadlock: RMS session
**57** and gate session **56**, with uncommitted TransactionEntry **80911**
observed during the overlap. The saved trace contains one deadlock graph,
SQL **1205 at 15:26:51.531**, the original handler's rollback/retry path, and
SQL **3609 at 15:26:53.219**. No trace events were dropped or truncated.

| Record | Persisted result |
|---|---|
| WO 23296 / entry 92662 | Closed; outstanding 0, QuantityRTD 2; total 224, VAT 24 |
| Item stock / commitments | Physical **10 unchanged**; committed **5 -> 3** |
| Queue 27 | Status 1, completed |
| Sale header 24690 | Exists, RecallID 23296, RecallType 4, total 224 |
| Sale lines / tender / tax / native OrderHistory | **0 / 0 / 0 / 0** |
| Control WO 23297 / queue 28 | Open, quantity 3; queue Status 0; reservation preserved |

Read-only verification at **19:55:58** confirmed the persisted state. Cleanup
records confirm removal of the owned fault trigger, gate and trace, and confirm
the original computed trigger definition. The incomplete posting was not repaired.

## What the controlled faults establish

An additional fixture-only AFTER UPDATE trigger targeted the exact marked item
and recalled order and read an owned gate row. It left the original computed
trigger untouched. An 800 ms lock timeout exercised 1222; the deadlock case
constructed an actual lock cycle using the blocked RMS session and gate session.
The deadlock graph verifies 1205 rather than an invented error message.
Session timeout/priority defaults were restored, and watchdog/finally cleanup
removed the temporary fault objects and tracing sessions.

These cases demonstrate that the original trigger's retry can obscure the
initial failure, and that **actual local RMS retained an incomplete posting**
under these forced failures. A closed order or completed queue alone therefore
did not prove successful settlement in these tests. They do not establish the
cause of any historical live incident, including KOR WO 237894: that incident
did not have the same retained sale-header pattern.

## Disposable SQL tests and their limits

Both captured branch definitions were tested on GUID-owned SQL LocalDB 2019
15.0.4382.1 fixtures at compatibility 130, not the SQL Server 2016 engine.
The initial reproduction tested baseline plus timeout/deadlock with XACT_ABORT
OFF/ON for each branch. All eight forced failures reached 3609 after retry.

The comparison covered two branches, two trigger variants and eight modes:
normal posting with XACT_ABORT OFF/ON, fractional quantity 1.25, negative quantity
-1.5, timeout OFF/ON and deadlock OFF/ON. Sixteen matched pairs preserved the
same business values. The candidate preserved 1222/1205 without trigger retry;
the original returned 3609. Thirty-two traces and eight deadlock graphs were
verified. Both owned fixture instances/databases were removed afterward.

**The synthetic caller explicitly owned and rolled back the entire posting.**
All its failed cases restored business state. Actual RMS behaved differently,
so those passes cannot certify the candidate for RMS. Synthetic statement order
also produced ComputedQuantity 6 in its quantity-two baseline, versus 8 in actual
RMS; no formula was changed to force agreement. Identity/rowversion allocator
gaps were excluded from value comparisons and were never reseeded.

## Other findings retained from the initial audit

- Add-in saves include headers, lines, totals, history, commitments and queues
  in the existing transaction. Edit/cancel paths retain order application locks,
  stale/closed-order checks, rowversion checks and atomic commitment differences.
  Fractional quantities and non-inventory exemptions were preserved.
- Thirty recent linked WO sales per localhost branch had closed headers,
  lines and matching tender totals. Among each branch's latest 500 WO IDs,
  none matched the narrow closed/no-linked-sale/nonzero-outstanding test.
  These samples are not a database-wide certificate.
- Source-only leads remain unproven: post-commit print/refresh failures before
  screen reset could permit resaving; scalar inserted/deleted trigger assumptions
  may limit batching; SI_ORSeries TABLOCKX and missing lookup coverage may affect
  concurrency. DVO's ItemValueLog retry wrapper is a separate untested path.
- The GSC ambiguous item-log index remains a separate blocked review. This
  investigation does not authorize choosing a history value or bypassing its guard.
- The October 14 custom history IDENTITY compatibility incident is also separate;
  these results do not replace its documented cause or recovery safeguards.

## Evidence and paused follow-up

Detailed local artifacts are under ignored `TestResults/`; they are not included
in a fresh clone. This committed summary preserves the key results without
credentials, connection strings or database dumps.

| Local evidence directory | Contents |
|---|---|
| `TestResults/LocalFlowAudit-786c05f1` | Initial read-only findings |
| `TestResults/TriggerRetry-cdabac7132` | Disposable original-trigger reproduction |
| `TestResults/TriggerCompare-178d6c4596` | Disposable original/candidate comparison |
| `TestResults/RMS-ActualFlow-Preflight/Run-18e5150b` | Actual normal settlement report and traces |
| `TestResults/RMS-ActualFlow-Preflight/Run-79ce964b` | Actual timeout report, screenshots supplied in chat, captured state and trace |
| `TestResults/RMS-ActualFlow-Preflight/Run-4604fde6` | Actual deadlock graph/events, overlap, before/after/verified CSVs and cleanup JSON |

No ten-user capacity test, candidate POS failure test, SQL 1204 test or
network/commit-loss test was completed. No live deployment is recommended from
these results. Resume only on a new user instruction: first verify the actual
local POS target again, then test candidate normal and failure paths through
actual RMS before considering any deployment. Do not retry or automatically
repair the retained incomplete postings.

General SQL behavior is documented in Microsoft's
[ROLLBACK TRANSACTION reference](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/rollback-transaction-transact-sql)
and [deadlocks guide](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-deadlocks-guide).
These explain transaction rollback and deadlock victims; they do not establish
this trigger's provenance or approve the candidate for RMS.
