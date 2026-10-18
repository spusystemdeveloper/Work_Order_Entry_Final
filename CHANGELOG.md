# Changelog

All notable changes to the **Work Order Entry** application are documented in this file.

## [Unreleased] - 2026-10-17

### October 18: consistent Escape cancellation in customer and sales lookup

- Confirmed that Esc from either lookup's OK/Cancel button was unhandled, while
  existing search/grid handlers already supported Esc. Assigned the existing
  Cancel button to each form in the Designer, making cancellation consistent
  without changing Enter, F-key shortcuts, tab order, selection or business rules.
- Added an isolated x86 keyboard-routing regression probe and updated the lookup
  presentation check to require the Cancel assignment. No lookup Load handler,
  SQL query, customer selection or sales-rep selection runs in these probes.
  The separate RMS investigation remains paused.
- Both Release/x86 solutions and the Debug app built successfully. All 101 unit
  tests passed on retry outside the sandbox (the first attempt could not connect
  to its x86 test host). Four keyboard, 204 lookup and 28 recall-formatting checks
  passed; the Debug EXE configuration was preserved byte-for-byte. Manual keyboard
  acceptance in the running add-in remains for the user.

### October 18: document paused RMS settlement investigation

- Consolidated the read-only audit, disposable trigger comparison and actual
  localhost RMS normal, timeout and deadlock results in the
  [settlement investigation record](Database%20Components/Tests/RMS_SETTLEMENT_TRIGGER_REVIEW_2026-10-18.md).
- Recorded partial postings after the forced original-trigger failures, cleanup,
  retained evidence and the user's decision to pause. Candidate POS testing and
  deployment remain unapproved/incomplete. This update changes documentation only;
  no application code or database was changed or tested again.

### Fix login-cancel collection error during shutdown

- Traced the October 17 11:44:36 log to frmLogin.Button2_Click calling
  Application.Exit while a closing-form callback can show a custom draft warning.
  Opening/disposing that prompt changes WinForms' OpenForms collection during
  its global exit enumeration. A separate synthetic x86 process reproduced the
  exact InvalidOperationException with the pre-fix compiled login callback.
- Login Cancel now returns Cancel to its modal caller; authenticated completion
  explicitly returns OK. Both startup and logout callers close the main form and
  stop their continuation on cancellation. Removed the redundant FormClosed
  Application.Exit call; the existing VB AfterMainFormCloses shutdown policy
  handles normal termination. Draft saving/warnings and authentication checks
  remain unchanged; no save transaction or database object was modified.
- Both Release/x86 builds and 101 unit tests passed with Debug configuration
  preserved. New no-database UI probes pass Cancel, window close and successful
  completion with a custom closing warning. All 1,159 shared-prompt checks and
  577 ownership/stacking checks also passed; manual startup/logout acceptance
  remains to be run in the actual application.

### Read-only GSC item-log ambiguity review

- Reviewed the existing localhost GSC_STORE_DB directly, without another restore
  or index/data changes: 9,512,243 log rows, zero invalid integer references,
  84 conflicting latest pairs across 73 Items and 246 tied transfer-log rows.
  All tied latest dates were midnight. Reference numbers matched 46 closed Orders
  and 38 missing Orders; none matched an open Order at review time.
- Traced the unfiltered timestamp-only TOP(1) lookup and date-only insert parameter.
  The heap has no sequence to reconstruct the original event ordering. Added a
  capped-memory SELECT-only diagnostic (intermediates in tempdb), findings and
  deployment guidance. The 53305 guard remains intact and the item-log index
  remains uncertified for this dataset; no historical value was chosen or repaired.
- No live connection was made. The earlier interrupted disposable-copy attempt
  was cleaned up before this completed review; no index-comparison result is
  claimed for that abandoned experiment. The voucher index remains a separate fix.
- Both Release/x86 solutions and 101 unit tests passed, with Debug configuration
  preserved. Final local checks found no Item-log index and no active review query.

### Package the tested RMS voucher lookup index in Database Updater

- Added optional Improve_RMS_VoucherLookup.sql under Index / performance for
  DVO, GSC and Portable, initially unchecked. It creates/reuses the tested
  non-unique Item(TenderID,IsGlobalVoucher,Inactive) index without fixed database,
  server or tender IDs. Business rows, triggers, application queries and branch
  rules remain unchanged; the original local-only fixture guards are retained.
- Added caller-transaction/schema guards, serialized index installation, reuse
  of matching differently named coverage and rejection of conflicting named
  indexes. The existing updater target confirmation, backup/test acknowledgement,
  transaction, timeout and history behavior are unchanged.
- Packaged the deployment guide and added disposable migration/updater checks
  for results, preserved data/rowversions/triggers, exact key order, index reuse,
  schema conflicts, initial unchecked listing and atomic deployment/history.
  No existing local restore or live database was migrated during this change.
- Both Release/x86 solutions and all 101 unit tests passed; the current Debug
  EXE configuration was preserved around builds/tests. All 83 updater checks,
  25 voucher migration checks at each compatibility level 100/130, and the
  clean-folder packaging suite passed. Synthetic Item reads fell from 756 to 2;
  this is not new live branch or actual RMS performance certification.

### Local Davao RMS lookup / Work Order save deadlock comparison

- Reproduced Item page-lock deadlocks on a GUID-owned copy of localhost
  DVO_STORE_DB with the unchanged RMS Item/Tender lookup and atomic commitment
  update. Tested a separate Item(TenderID,IsGlobalVoucher,Inactive) index;
  the existing item-log index remained constant in both stages.
- Six actual compiled recalled-order save transactions plus four SQL lookup
  replay workers: median save 9.320 -> 1.351 seconds, slowest 10.015 -> 1.693
  seconds; reader deadlock victim errors 4 -> 0. No actual RMS POS/QSRules client
  or production 10+ user capacity test was performed.
- Verified fractional commitments/physical stock for 31 Items, preserved values
  on 32 saved lines, identical nonempty/empty voucher results across 114 columns,
  and unchanged 84 trigger definitions/states. Added the comparison report and
  guarded test-only candidate SQL; it is not an updater/live deployment script.
- Both Release/x86 builds, all 101 unit tests and both 39-check disposable
  concurrency modes passed. Debug configuration was preserved. Six source-table
  count/checksum comparisons matched the pre-test backup; the owned clone and
  backup were removed. The original local restore received no candidate index
  or test business writes, and no live connection was made during this task.

### Read-only live Davao performance findings and mixed-user plan

- With explicit read-only authorization, collected bounded metadata/DMV snapshots
  from live DVO_STORE_DB and existing system_health deadlock summaries. Two captured
  events show an RMS Item/Tender voucher lookup deadlocking with the atomic Item
  commitment update. The live item-log table still lacks the tested lookup index;
  broad item-list reads also have high cached elapsed/read costs. No sustained
  blocker was observed in the short activity sample; this is not peak-load proof.
- Added a credential-free collector with masked SQL password entry, 10-second
  query/1-second session lock timeouts and SQL-text literal/comment redaction.
  It performs no DML/DDL, full log-table scans, cache clearing, new tracing,
  configuration changes, session killing or live workload replay. Both completed
  collections had zero unavailable sections; the first local builder-keyword
  attempt failed before connecting and was corrected.
- Added DVO_LIVE_READONLY_PERFORMANCE_2026-10-17.md and a separate 10/12/15-client
  actual-RMS test plan. It distinguishes existing two-cashier simulated settlement
  checks from a future capacity test and prioritizes the captured deadlock path.
  The load plan has not been executed; no live index or business rule was changed.
- Both Release/x86 builds and all 101 unit tests passed. Debug store configuration
  and the existing uncommitted Payment Type precheck were preserved. Credentials
  and raw operational evidence are not added to repository documentation.

### Local restored Davao item-log index verification

- At the user's request, installed the unchanged guarded item-log lookup index
  only on localhost MSI / DVO_STORE_DB. Preflight/index creation took 1.606 seconds.
  The local index remains installed; no live target, DVO_POS_DB, trigger logic or
  business rows were changed by this test.
- Seven item/reference probes, three trials per stage, retained identical results.
  Median component times fell from 21.197-23.573 ms to 0.616-0.866 ms; logical reads
  fell from 10,639 to 3. All 84 table-trigger hashes/states, log row count/checksum
  and compatibility level 100 matched. A separate connection verified the commit.
- Both Release/x86 builds, 101 unit tests and 31 disposable item-log checks at
  each compatibility level 100/130 passed. Debug configuration was preserved.
  See Database Components/Tests/DVO_RESTORED_ITEMLOG_INDEX_RESULTS.md for evidence
  and limits: the local backup is from June 9, and no full application save or
  real RMS workflow was performed. Rehearse current Davao data before live rollout.

### Check Work Order payment before price approval

- Added a Work Order-only blank Payment Type precheck at the start of
  ValidateAndSave. It shows the existing Payment Method warning, focuses the
  payment field and stops before item-price lookup or approval/password prompts.
  New and recalled Work Orders use this path. The existing later save checks
  remain in place; quotation requirements and conversion paths are unchanged.
- Price confirmation, minimum prices, saved-price exemptions, password/approval
  conditions, cancellation and save/transaction/stock behavior are unchanged.
- Both Release/x86 builds and all 101 unit tests passed. Added 121 isolated
  validation-order checks across 12 new/recalled Work Order/quotation cases,
  including blank/whitespace payment and CASH/CHARGE continuation. All legacy
  SQL contexts are disabled only inside that test process. Both disposable
  multi-user modes passed 39 checks each (RMS settlement is simulated). No live
  database, real password authentication or live-store save was tested; Debug
  store configuration was preserved byte-for-byte.

### More compact lookup and message windows for 768-high screens

- Reduced Designer client sizes: Customer/Sales Rep 780x470 -> 660x390;
  Recall 984x540 -> 860x460; shared message base 640x300 -> 500x240;
  action confirmation 560x210 -> 440x172; import complete 580x254 -> 460x208;
  saved-number confirmation 520x296 -> 440x238; Recall Selection 580x266 -> 460x220.
- Kept lookup text at its existing size while reducing row minimums and excess
  spacing. Recall customer/comments still wrap; Date/Reference size to displayed
  content with minimum widths. Message headings use 14-point bold text, body
  text 11-point and buttons 10-point. Short prompts shrink vertically and long
  details remain complete/scrollable. Saved numbers retain 42-point bold type.
- Changed only Designer geometry/styles and shared-prompt sizing. Main-form
  button positions, filters, selection, shortcuts, hidden customer columns,
  Boolean NullValue defaults, owner/TopMost fix, dialog results, authentication,
  pricing, credit limits, tax, stock, save/import/printing behavior are preserved.
- Both Release/x86 builds and 101 unit tests passed. Passed 1,159 shared-prompt,
  577 ownership, 53 action-confirmation, 106 Recall Selection, 28 recall-formatting,
  875 small-screen and 185 compact-geometry checks, plus clean-folder import/PDF
  packaging. Reviewed synthetic dialog renders at normal/simulated 125% scaling
  for 1366x768 and 1024x768. Store-loading handlers were detached only on render
  test instances; no live SQL or real store workflows/printers were used.
  Debug store configuration was preserved byte-for-byte. Actual workstation DPI
  and window-switching acceptance remains manual.

### Audit the remaining redesigned prompts for window blocking

- Traced the shared prompts in the other TopMost forms and the separate save/
  remove, import-success, saved-number and Recall Selection callers. No additional
  ownership/stacking blocker was reproduced after the shared-owner fix. Import
  completion runs after file/customer dialogs close, or after website selection
  hides; the other dedicated confirmations use the main form as their owner.
- Added PromptOwnership_UI_Test.ps1: 577 native window checks in 76 scenarios
  cover all six shared button sets and five icon categories under ordinary and
  TopMost modal owners, plus each dedicated confirmation/selection form. Checks
  verify actual window order, input availability, result values and parent recovery.
- Both Release/x86 builds, 101 unit tests, 1,159 existing shared-prompt checks,
  53 action-confirmation, 106 Recall Selection and 28 recall-formatting checks
  passed. No additional application-code or business-rule changes were needed.
  Tests use synthetic owners and presentation dialogs, not store workflows or SQL;
  actual workstation window switching still needs manual acceptance. Debug store
  configuration was preserved byte-for-byte.

### Keep warnings above the active price-selection dialog

- Fixed the shared prompt ownership regression: Price Level Selection uses
  TopMost, and its credit-limit warning could appear behind it. Ownerless prompts
  now use the current modal window on the UI thread (falling back to the active
  visible form), and prompts match their owner's TopMost setting. Explicit caller
  ownership is preserved. No global always-on-top setting was added.
- Credit-limit conditions, warning timing/text, pricing, approvals, item selection
  and dialog results are unchanged. The warning still requires acknowledgement;
  its modal parent remains disabled until then and is restored afterward.
- Both Release/x86 builds, 101 unit tests, 1,159 isolated shared-prompt checks and
  50 existing Price Level checks passed. The regression covers ordinary/TopMost
  nested windows, Show/MsgBox/explicit owners, and an item acceptance already
  marked OK before its warning opens. Native Windows enable/disable checks verify
  both modal levels recover. Tests use synthetic forms; no store/SQL/price action
  was run. Debug store configuration was preserved byte-for-byte.

### Large sales quotation confirmation number

- Reused the Designer-backed frmOrderSaved for saved and updated sales quotations,
  showing the quotation number in the same large bold type as a Work Order.
  The heading identifies Sales Quotation; existing Work Order/split labels remain
  unchanged. Close, Enter and Escape acknowledge the single confirmation.
- Replaced only the two quotation success notifications after commit. New quotation
  printing still opens after acknowledgement with the same type and ID; updating
  a quotation still does not open printing. Refresh/draft cleanup and all save,
  pricing, stock, tax, approval and transaction logic are unchanged.
- Both Release/x86 builds, 101 unit tests, 105 isolated saved-number UI checks
  and 39 multi-user checks per isolation mode passed. Reviewed 100% and simulated
  125% renders. The first sandboxed unit runner could not connect to its test host;
  rerunning outside the sandbox passed. No live saves/database access or actual
  printing was performed; Debug store configuration was preserved byte-for-byte.

### Consistent readable application prompts

- Routed 323 standard MessageBox/MsgBox calls across 31 Work Order Entry files
  through AppPrompt and the shared frmAppPrompt. The Designer shows a large
  heading, 12-point message text, matching information/warning/error/question
  icons and larger buttons. Long details remain complete in a scrollable area.
- Kept the original message/caption arguments, button sets, default choices,
  explicit parent windows and return-value checks. OK, OK/Cancel, Yes/No,
  Yes/No/Cancel, Retry/Cancel and Abort/Retry/Ignore retain their results and
  Enter/Escape/close behavior. Worker calls marshal to the owning UI thread;
  native reporting remains available if the custom dialog cannot initialize.
- Existing save-number, import-success, action-confirmation and selection forms
  remain in use. Separate Database Updater/Queue prompts are outside this change.
  No pricing, stock, tax, approval, save, printing, SQL or authentication logic changed.
- Both Release/x86 builds and all 101 unit tests passed. Passed 1,075 isolated
  shared-prompt checks, 28 recall-formatting, 106 recall-selection, 53 action-
  confirmation and 39 multi-user checks in each isolation mode, plus clean-folder
  import/PDF packaging checks. Reviewed 100% and simulated 125% prompt renders.
  These use synthetic UI/disposable fixtures; RMS settlement is simulated. No
  live database was accessed, and Debug store configuration was preserved.

### Recall Selection readability

- Updated only frmRecallSelection.Designer.vb: added a Choose an option heading,
  14-point bold list text, larger 12-point OK/Cancel buttons, consistent spacing,
  resize anchors and a minimum window size. Layout is visible in the Designer.
- Preserved the Recall, SaveToQuote and SaveToWorkOrder strings/order, first
  selection, tab/keyboard routing, double-click and OK/Cancel handlers. The later
  shared-prompt update changes only presentation of its two validation warnings.
- Both Release/x86 builds, all 101 unit tests and 106 isolated choice-dialog
  checks passed. Reviewed 100% and simulated 125% renders. No actual recall,
  conversion, save or SQL was run by the UI checks; Debug configuration preserved.

### Save changes and remove-item confirmation readability

- Replaced only the existing Save changes and Remove item Yes/No prompts with
  frmActionConfirmation. The Designer shows 16-point bold question text and
  large 12-point Yes/No buttons, with consistent spacing and bottom/right anchors.
- Preserved question wording, default Yes, explicit result values, Enter on
  focused buttons, arrow navigation and Y/N mnemonics. Escape/user close leave
  the question unanswered, retaining the native Yes/No prompt's lack of Cancel.
  Save/remove action branches, validation, transactions and stock logic are unchanged.
- Both Release/x86 builds, 101 unit tests, 53 isolated dialog checks and 39
  multi-user checks per isolation mode passed. Reviewed 100% and simulated 125%
  renders. The multi-user fixtures are disposable simulations, including simulated
  RMS settlement; no store saves/removals or isolation changes were performed.
  The existing Debug store configuration and earlier import-dialog edits were preserved.

### Import completion message readability

- Replaced the three existing success MessageBoxes in quotation/order, purchase-
  order and website import with a shared frmImportComplete modal dialog. Its
  Designer contains a 16-point bold success heading, 12-point availability
  reminder and large blue OK button. Enter/Escape acknowledge it; the reminder
  still tells users to check available quantities before saving a Work Order.
- Only the three success prompts and a presentation helper changed in the main
  form. Import conditions, item validation, quantities, prices/taxes, totals,
  layout cleanup, error paths and saving behavior are unchanged. No dependencies,
  database migrations or business-rule changes were introduced.
- Both Release/x86 builds, 101 unit tests, clean-folder import/reporting packaging
  checks and 24 isolated dialog checks passed. Reviewed offscreen renders at
  100% and simulated 125% with bounds and fonts scaled; Enter/Escape both close
  the modal dialog. Debug store configuration was preserved byte-for-byte.
  No real import or store database operation was performed during validation.

### Requested one-order HO renumber and reseed script

- User reported the live commit; subsequent SELECT-only verification confirmed
  WO 18879 and mapped lines/queue/approvals/history/order logs, no remaining old
  references in the reviewed tables, 13 unchanged/enabled triggers, and current
  identity/max 18879 with increment 1 (next allocation 18880 absent new activity).
  Recorded evidence and historical-text/external-reference limits in the recovery
  guide. The assistant did not execute the live renumber/reseed or create a test order.
- Prepared Recovery/Renumber_HO_WO_18882_to_18879_And_Reseed.sql for personal SSMS
  execution. Default rehearsal rolls back without reseeding; explicit commit
  maps reviewed references from 18882 to 18879 and reseeds to 18879 in the same
  transaction, making the next allocation 18880 if no intervening allocation occurs.
- Removed the backup-path setting, backup-history/checksum requirement and
  RESTORE VERIFYONLY from this one-order recovery file at the user's request.
  Older recovery files are unchanged; no live SQL was executed for this revision.
- Kept exact target verification, audited header/
  line/identity checks, orphan/unknown-reference guards, trigger fingerprints,
  full mapped-value verification and error rollback. Stops on later activity or
  new posted links; preserves stock, amounts and unrelated reference domains.
- Added usage instructions and 42 passing synthetic LocalDB checks at compatibility
  100, including rollback after reseed and actual next ID/legacy @@IDENTITY.
  No live recovery/reseed or restored-store write was performed by the assistant.
  Offline SQL Server 2016 syntax validation, both Release/x86 builds and all 101
  unit tests pass. The Debug store EXE configuration was preserved byte-for-byte.

### Item-log migration compatibility and restored HO testing

- Prepared Recovery/Deploy_HO_PO_DB_ItemLogLookup_LIVE.sql for personal SSMS
  execution. It embeds the reviewed migration unchanged, selects HO_PO_DB,
  commits only after migration safeguards pass,
  and rolls back/rethrows SQL errors. Includes backup, maintenance, query-timeout
  and cancellation instructions. File preparation does not deploy to live.
  Offline SQL Server 2016 parsing reports zero errors in one batch; the embedded
  migration matches the tested source and transaction/batch structure checks pass.
- Removed the standalone file's explicit server-name, database-name and ambient
  transaction checks at the user's request. Kept USE HO_PO_DB, migration guards,
  transaction/rollback behavior and updater files unchanged. The operator must
  verify the connection and begin without an existing transaction. No SQL ran.
- The localhost HO_PO_DB restore retains compatibility level 100 on SQL Server
  2016. Replaced only the optional index migration's three TRY_CONVERT integer
  checks with TRY_CAST so its existing reference/timestamp safeguards run there.
  No compatibility setting, trigger, numeric comparison or business rule changed.
- Added an optional CompatibilityLevel=100/130 fixture parameter (default 130).
  All 31 item-log checks pass at each level, including malformed/overflow
  references, latest-value ties, rollback and unchanged legacy logging values.
  Both Release/x86 solutions build; 101 unit tests and 71 updater checks pass.
  The Debug store configuration was preserved byte-for-byte. Live SQL was not
  accessed during this local test; restored-store results are recorded separately.
- Committed IX_SOD_WO_ItemLogLookup on the explicitly authorized localhost MSI /
  restored HO_PO_DB only, retaining compatibility 100. The 71.74-million-row
  preflight/build took 654.371 seconds. Two unchanged lookup results improved
  from 6,706.538/7,294.833 ms to 3.472/5.192 ms, with logical reads 806,300 to 5
  each. All table trigger hashes matched. These are component timings, not app
  save or live-store tests. See Database Components/Tests/HO_RESTORED_ITEMLOG_INDEX_RESULTS.md
  for evidence, limits and remaining manual save/import verification.

### Top action-button order

- Placed Set Tax directly above Logout, making Logout the last button in the
  top group. Updated Designer docking order/positions only; handlers, shortcuts,
  tab indices and Set Tax visibility settings are unchanged. Extended the
  existing lookup/button check to cover both shown and hidden Set Tax layouts.
- Both Release/x86 builds, 101 unit tests, 204 lookup/button checks, 875 screen
  checks and 28 Recall formatting checks pass. No live actions or SQL ran;
  the Debug store configuration was preserved.

### Price Level Selection readability

- Enlarged the item heading/description, price-level grid and OK/Cancel buttons
  in frmPriceLevel.Designer.vb. Added 11-point row text, bold right-aligned prices,
  taller rows, clearer headers, alternate shading and blue selected rows.
- Preserved the two column indices, price values/formats, read-only permissions,
  tab order, DialogResults and keyboard routing. frmPriceLevel.vb is unchanged,
  including top-row default selection, price loading, approval/password paths
  and applying prices. No business-rule or other Price Level form changes.
- Both Release/x86 solutions build; all 101 unit tests, 50 isolated Price Level
  checks and 28 Recall formatting checks pass. Checked raw fractional prices,
  existing numeric/currency formats, default selection, empty rows and geometry
  at normal/simulated 125% scaling. Inspected a synthetic preview. No SQL, live
  price application or password approval ran; Debug configuration was preserved.

### Hide internal customer-search columns

- Hid ID and Customer Type in both the Designer and the post-search visibility
  settings. Customer search now shows only Account Number and Company; the
  hidden columns, ordinal mappings and values remain available to selection,
  pricing and tax logic. Updated the existing lookup visibility regression check.
- Both Release/x86 solutions build; all 101 unit tests, 202 lookup checks and
  28 Recall formatting checks pass. No live customer selection or SQL ran;
  the Debug store configuration was preserved byte-for-byte.

### Customer, Sales Rep and action-button design

- Matched Customer and Sales Rep lookup layouts with larger search fields,
  11-point grid text, 42-pixel minimum rows, clearer headers, wrapped names,
  alternating shading and blue selected rows. Widened the name columns and
  reduced the Sales Rep ID width. Both dialogs fit 1024x768 at simulated 125%.
- Serialized column presentation in the Designer. Preserved all nine customer
  properties/ordinals, existing visible/hidden fields, leading-zero accounts,
  pricing/employee/tax data and selection handlers. Hidden Boolean fields remain
  checkbox columns with Boolean False display defaults for blank cells. The
  CustomText4 heading now reads Customer Type; its mapping/value is unchanged.
- Standardized right-side button text/icon layout and border/font styling.
  Emphasized Save in blue, related workflow actions in pale blue, and Remove/
  Cancel in pale red. Preserved icon resources, dock order, bounds, enabled/
  visibility rules, tab indices and shortcut hints; removed caption space padding.
- Both Release/x86 solutions build; all 101 unit tests, 202 lookup-design checks,
  875 small-screen checks and 28 Recall formatting checks pass. Inspected
  synthetic-data renders. Search queries, pricing/tax selection logic and main
  action handlers were not changed. No live search, customer selection or order
  action ran; store configuration was preserved. Manual checks are documented
  in UNIT_TEST_GUIDE.md.

### Layout for smaller store screens

- Compacted Settings to a 984x660 client area without removing settings or
  reducing fonts. Enabled scrolling when larger display scaling requires it.
  Reduced default Recall, Check WO, history/shortage and restored preview widths;
  kept existing column minimums and horizontal grid scrolling.
- Added one presentation-only SmallScreenLayout file to constrain affected
  dialogs to their monitor's working area on opening and subsequent resizing.
  Preview maximization, toolbar/report configuration and all action handlers
  remain unchanged. Import controls now anchor to their existing window edges.
- Main-window panel heights adapt to leave room for the left inputs; totals
  widths fit the available panel without changing values or fonts. Enabled
  horizontal item-lookup scrolling and aligned the image overlay with its grid.
  No authentication, SQL, calculation, save, recall, stock or lifecycle rules
  were changed. Static layout adjustments remain visible in the Designer.
- Both Release/x86 solutions build; all 101 unit tests pass. Isolated checks
  pass: 875 small-screen, 776 Settings, 180 Check WO, 83 Import, 91 Print and
  28 Recall checkbox checks. Clean-folder PDF/ReportViewer packaging checks pass.
  Inspected 1024-width Settings/main-window renders. The screen fixture uses
  1024x768 and 1366x768 with simulated 125% scaling; real workstation DPI/taskbar
  verification remains manual. No live order actions or store writes ran and
  the Debug EXE configuration was preserved byte-for-byte.

### Larger print-choice dialog for readability

- Enlarged the print-input dialog with stacked, full-width 14-point template
  choices, a 24-point order-number field and a larger 14-point Print button.
  Increased spacing and contrast; renamed the existing Order Number choice to
  Work Order / Quotation to describe the same report route more clearly.
- Layout is serialized in frmPrintType.Designer.vb. Default selection, radio
  grouping, tab order, Enter action, input validation and print code are unchanged;
  no report templates, preview code, printer/copy settings or business rules changed.
- Both Release/x86 solutions build, all 101 unit tests and 91 isolated Print
  checks pass, including larger click targets and label-fit checks. Clean-folder
  PDF/ReportViewer packaging checks also pass. Inspected the rendered layout;
  no SQL lookup, printer dialog or print job was executed. Debug configuration
  was preserved; actual eyesight/workstation-scale feedback remains manual.

### Print options and preview presentation

- Aligned template choices, order-number entry and the Print button; added an
  explicit order-number label and a distinct blue Print action. Preserved the
  selected template default, radio grouping, tab order and Enter-to-Print action.
- Added descriptive headings and toolbar guidance to Work Order/Sales Quotation
  and Pick List previews, with consistent outer spacing and a larger restored
  window size. The existing ReportViewer still fills the remaining space; its
  toolbar and maximized opening behavior are preserved. All layout is serialized
  in the three Designer files.
- Print/template validation, data loading, calculations, page settings, printer
  selection, copy-count handling, report templates and printing handlers are
  unchanged. All three code-behind files were verified byte-for-byte unchanged;
  no RDLC or dependency changes were made. Long text inside printed reports was
  deliberately left as defined by the existing templates.
- Both Release/x86 solutions build and all 101 unit tests plus 85 isolated Print
  UI checks pass. Clean-folder packaging checks render both report PDFs and
  verify packaged ReportViewer loading. Inspected empty-viewer/outer-layout
  previews; no live order lookup, printer dialog or print job was executed.
  Debug store configuration was preserved. Workstation printer/copy verification
  remains manual; see UNIT_TEST_GUIDE.md.

### Check WO presentation

- Improved the selected-item heading and enlarged the Check Open Work Orders
  grid, with bold order numbers, wrapped customer text, dedicated date/status
  widths, alternating row shading and clear blue selected rows. Quantity values
  are right-aligned without adding a rounding/number format. Close remains at
  the bottom right; the existing resizable window now has a usable minimum size.
- Moved column headers, widths and existing hidden-item-column settings into
  frmCheckWO.Designer.vb, alongside the layout/styles. Preserved all ten property
  mappings/ordinals, read-only permissions, multiple selection, column reordering,
  tab indices and the existing SQL call using the selected item code. No search,
  order-edit action, queue stage or new filtering rule was added.
- Both Release/x86 solutions build; all 101 unit tests, 180 isolated Check WO
  checks and 28 Recall checkbox formatting checks pass. Checked synthetic-data
  rendering, generated LINQ result binding, nulls, fractional quantities, sorting,
  reordering, resize/simulated scaling and the existing Close handler. Debug
  store configuration was restored byte-for-byte; no store query or write was
  executed. Actual store lookup and workstation DPI testing remain manual.

### Import dialog presentation

- Enlarged and aligned the custom F6 website-order chooser, quotation CSV
  uploader and missing-item guidance window. Added consistent grid typography,
  alternating row shading, wrapped long text and blue selection/import actions.
  Website order numbers are bold; customer names and dates have dedicated space.
- Serialized layout and styles in the three Designer files. Website columns
  keep the same OrderID/CustomerName/DateOrder property order; their underlying
  values and date format are preserved. CSV detail columns still initialize
  when verification starts, with compact row/column numbers and wider messages.
- Kept the Windows file picker, file filters, search/Show All logic, import
  selection, keyboard mnemonics/tab indices, duplicate checks, validation,
  pricing, logging and Import/Finish behavior unchanged. Website/missing-item
  handler source is unchanged byte-for-byte; CSV source changes are confined
  to GridDesign presentation settings.
- Both Release/x86 solutions build; all 101 unit tests, 83 isolated Import
  checks, 28 Recall formatting checks and clean-folder packaging checks pass.
  The prior executable also passes the 74 shared semantic checks. Inspected
  synthetic-data previews and preserved the local Debug EXE configuration.
  No live import, SQL duplicate lookup, MySQL refresh or import-button execution
  was performed. See UNIT_TEST_GUIDE.md for workstation validation.

### Settings layout and keyboard focus

- Reorganized the existing Settings groups into aligned two-column sections,
  with consistent field spacing and readable fonts. Layout, labels and button
  styles are serialized in frmSettings.Designer.vb for Visual Studio preview.
- Distinguished the blue "Save branch / order options" button from individual
  password Update buttons and separate Apply buttons for defaults, website
  import and connection settings. Existing Alt+O mnemonics and tab indices remain.
- Added presentation-only field/checkbox focus shading and button focus borders
  in frmSettings.Presentation.vb. Existing settings loading, validation, password
  requirements, persistence, restart behavior and business rules are unchanged;
  frmSettings.vb was verified unchanged byte-for-byte for this revision.
- Both Release/x86 solutions build; all 101 unit tests, 776 isolated Settings
  presentation checks and 28 Recall checkbox formatting checks pass. Inspected
  an empty-field rendered preview. Store configuration was preserved; no store
  settings were loaded/saved, and actual Designer/high-DPI hardware testing
  remains manual. See UNIT_TEST_GUIDE.md for the Settings checklist.

### Restore blank UP-STORE checkbox formatting during recall

- Fixed a regression introduced by copying runtime styles into the Designer:
  the selected-item UP-STORE checkbox style inherited a text NullValue instead
  of its original Boolean False. Blank/DBNull cells could therefore raise
  "Formatted value of the cell has a wrong type" during recall while the list
  dialog remained visible. Restored the original Boolean display default;
  actual null/true/false values, recall logic and business rules are unchanged.
- Added RecallFormatting_UI_Test.ps1, which fails on the old compiled build
  and passes 28 checks after the fix, including null/DBNull/false/true formatting
  before/after runtime styling and the temporary new-row placeholder. It observes
  DataErrors only inside the test; no production error suppression was added.
- Both Release/x86 solutions build and all 101 unit tests pass. The exact
  formatter reproduction now reports zero DataErrors; all 32 synthetic Recall
  presentation checks and previous-build filter/selection comparisons still pass.
  No store writes or live recall execution were performed; configuration was
  preserved byte-for-byte. Retest both quotation and Work Order recall in the app.

### Recall window readability

- Made the Recall dialog wider and resizable, retaining the action buttons at
  the same distances from the bottom/right edges. Larger fonts, bold order
  numbers, a dedicated date/time width, larger wrapped Customer/Comment columns,
  subtle alternate rows and a blue/white selection highlight improve readability.
  Wrapped rows grow only for displayed entries, with a 44-pixel minimum; native
  cell tooltips remain available. Defaults are visible in the Designer.
- Kept all five bound fields, headers and their order. Dates/order IDs remain
  the original strings; customer account text and comments are unchanged.
  frmRecall.vb, clsRecall queries, filters, debounce, selection handlers, key
  handling and actual recall logic were not edited.
- Both Release/x86 solutions build and all 101 unit tests pass. A 32-check
  synthetic Recall probe and comparison with the previous build preserve all
  header-based filters (including apostrophes), row IDs, counts, sorting,
  selection, rebinding, control shortcuts/tab indexes and button edge offsets.
  Resizing and long-text renders were checked. This did not execute live recall
  or quotation/order-taker SQL paths. The Debug EXE configuration was preserved.

### Show the current UI defaults in the form Designer

- Serialized the existing grid fonts, colors, spacing, row heights, column
  widths/alignment and left-panel defaults in frmItemLookUp.Designer.vb. The
  lookup grid now declares the same 19 bound columns in their existing order,
  including the hidden Boolean Inactive column and the expanding SOH column.
  The Designer can show headers/layout without loading a database or logging in.
- Retained automatic binding and runtime styling/focus behavior. Compared the
  previous build against both the new runtime and Designer-only native control
  style/layout snapshots at the default scale. Binding, nulls, decimal/negative
  stock, column types, sorting, empty results, rebinding, button bounds/captions,
  tab indexes and KeyPreview also matched in a synthetic fixture. Authentication,
  SQL, keyboard handlers and business rules were not edited.
- Both Release/x86 solutions build and all 101 unit tests pass. Existing isolated
  probes pass 405 grid checks, 136 focus/layout checks, 408 Show Image layout
  checks and 21 LocalDB search checks. Configuration was preserved exactly.
  Visual Studio itself and live-store workflows were not automated in these checks.

### Show full SOH quantities

- Replaced the fixed 64-pixel lookup SOH width with a 110-pixel minimum and
  sizing to displayed cells. Large and fractional values can expand the column;
  the item description uses the remaining space. No all-row measurement is used.
  Existing SOH number formatting, stored quantities, button positions, shortcuts
  and business rules remain unchanged.
- Both Release/x86 solutions build and all 101 unit tests pass. A synthetic grid
  probe passed 405 value/style/layout checks, including large, negative and long
  fractional SOH values, text fitting after scrolling, unchanged cell values and
  button geometry/captions. Rendered grids were inspected; no live-store writes
  were performed. The local Debug EXE configuration was preserved exactly.

### Keep Show Image below the barcode row

- Disabled automatic scrolling on the fixed entry panel and anchored Show Image
  to the top/left. A scrollbar can no longer shift the checkbox upward into the
  barcode field. Its designed position and the existing button geometry, tab
  order, shortcuts, image behavior and business rules remain unchanged.
- Both Release/x86 solutions build and all 101 unit tests pass. An isolated
  native-control probe passed 408 layout checks across three simulated scales,
  including full control bounds, no scrollbars and unchanged button positions
  and tab indexes. This is layout validation, not a live-store workflow test.
  The local Debug EXE configuration was preserved byte-for-byte.

### SQL item-search investigation and repeatable timing

- Reproduced the BOOK + PAPER SQL delay on local GSC (382 matches). Compared
  query rewrites and an indexed search-text prototype on disposable copies.
  Rejected faster variants that slowed broad searches or changed the original
  unordered TOP 500 cutoff. Removed the experimental application query builder;
  the original search SQL and all business rules remain in use.
- Added a read-only timing script recording SQL durations, row counts, result
  hashes and IO/time statistics without exporting item rows or credentials.
  See Database Components/Tests/ITEM_SEARCH_PERFORMANCE_FINDINGS.md for measured
  limits and the proposed restored-copy maintenance investigation. No search
  index, store view, timeout or database setting was changed; this is not a
  deployed SQL speed fix.

### Bold lookup rows and responsive item search

- Restored bold text for every lookup row using cached fonts, removing the
  per-cell committed-quantity check and repeated row-style mutation on paint.
- Kept the exact existing contains-search SQL, TOP 500 limit, keyword history,
  fallback behavior and synchronous caller contract. The filtered SQL read now
  runs on an owned background connection while a modal Searching items indicator
  keeps UI messages pumping and prevents actions against unfinished results.
- Read-only local GSC diagnostics found the query scans Item and reads its
  legacy ntext descriptions (216,723 LOB logical reads for the sampled search).
  This UI change does not remove that database scan or promise faster SQL.
  No schema, search matching, stock freshness or business rule changed.
- Both Release/x86 solutions build and all 101 unit tests pass. The actual
  compiled search helper passes 21 disposable LocalDB checks for UI message
  progress, modal locking/restoration, early-close prevention and identical
  returned values. Synthetic grid checks confirm bold rows without changing
  values, warning colors or button geometry. Store configuration was preserved.

### Left entry panel spacing and focus indication

- Standardized left-panel input fonts and label alignment, with consistent gaps
  and fields centered beside the existing lookup buttons. Button positions,
  tab order, shortcut handlers, field values and read-only/enabled rules are retained.
- Replaced the salmon search/barcode outline with blue only while the enabled
  field actually has focus; inactive/disabled fields use the panel background.
  Outline bounds follow field sizing. Apply presentation before login without
  changing authentication, database loading or business rules.
- Both Release/x86 solutions build and all 101 unit tests pass. An isolated
  probe passes 136 layout/state/focus checks using simulated focus without
  taking keyboard focus or accessing SQL; native input layout was rendered.
  Existing store configuration was preserved byte-for-byte.

### Apply grid presentation before login

- Moved the existing FormatGrid call immediately before the login dialog so
  the empty grids already show the updated headers, spacing and column layout.
  Store configuration still runs first; authentication, registry/register reads,
  store queries, refresh and draft recovery retain their existing sequence.
  No data loads, login checks or business rules moved into grid formatting.
- Both Release/x86 solutions build and all 101 unit tests pass. The isolated
  grid probe passes 373 checks, including empty/unbound grid presentation without
  login or SQL access; the local store configuration was preserved byte-for-byte.

### Grid readability and spacing

- Standardized the lookup and Selected Items grids on cached Tahoma fonts,
  padded headers/cells, subtle alternate rows and clearer grid lines. Lookup
  rows use a compact fixed height with wrapped descriptions; selected lines have
  more vertical space. No automatic all-cell sizing or per-paint font allocation.
- Right-aligned numeric headers/cells, centered UP-STORE checkboxes and gave
  Selected Items descriptions the remaining width with bounded numeric columns.
  Spaced the Last Purchase Price heading for readability. Button positions,
  shortcuts, stored values, numeric formats, editability and warning colors remain.
- Both Release/x86 solutions build and all 101 unit tests pass. Populated-grid
  renders were inspected; 372 synthetic value/style/layout checks cover three
  client widths, inherited numeric alignment, warning colors, editability and
  unchanged button geometry/captions. Store configuration was preserved.

### Align totals with Selected Items

- Inset the totals panel border by the same horizontal padding as the Selected
  Items grid, using a docked wrapper so both edges stay aligned during resize.
  Kept totals right-aligned and preserved their sizes, values and calculation rules.
- Both Release/x86 solutions build and all 101 unit tests pass. An isolated
  layout probe passes 16 alignment/overlap/value checks across four client widths;
  the lower panel was rendered for inspection. Existing store config was preserved.

### Optional Order actions and status notification visibility

- Added independent Show Order actions and Show order notifications checkboxes
  under Settings > Work Order and Queue Options. Both default to visible when
  configuration keys are absent and persist in the existing installation config.
- Apply the choices when Settings closes and during notification updates, so
  polling cannot redisplay hidden ready/shortage status links. The For Invoice
  button/count, notification monitoring, pending reduction reminders and critical
  recall/save/conflict/unsaved-change dialogs retain their existing behavior.
- No database migration, order lifecycle, pricing, tax or commitment rule change.
- Both solutions build Release/x86 and all 101 unit tests pass. An isolated
  temporary-config WinForms probe passes 36 checks across fresh AppDomains for
  defaults, independent toggles, persisted off/on state, repeated status updates,
  failure-state visibility and unchanged other settings. Checkbox layout was
  rendered and inspected; the installed Debug configuration was preserved.

### Optional item-log lookup performance index

- Added an optional SQL Server 2016 migration for a covering ItemID/Date index
  on SOD_Item_Logs, preserving the trigger and its numeric comparison against
  varchar ReferenceID. No business rows, stock rules or RMS identities change.
- Added schema/index/reference guards and a locked preflight for conflicting
  latest timestamp ties. Reuses suitable existing indexes and requires the
  updater's or SSMS caller's transaction; no automatic store installation.
- Added the optional updater catalog entry, packaged deployment/rollback guide
  and disposable LocalDB tests for identical log output, caller rollback,
  unsafe-state rejection and synthetic logical-read reduction.
- Validation: 31 item-log checks, 71 updater checks, 39 existing multi-user
  checks in each isolation mode and all 101 unit tests pass. Both solutions
  build Release/x86; Debug/Release updater packages contain the exact SQL/guide.
  Synthetic lookup reads fall from 5,883 to 4. Store configuration was preserved
  byte-for-byte; no migration was applied to a store database.

### Shared contributor and AI guidance

- Added AGENTS.md as shared guidance for preserving business rules, SQL Server
  2016 and RMS identity compatibility, branch configuration, safe database work,
  required validation and complete application packaging.
- Added CLAUDE.md and linked the existing Gemini guidance and README to the shared
  rules so GitHub collaborators can use consistent instructions across AI tools.
- Documentation only; no application code, SQL deployment or business rule changed.

### Save versus closing and failed-save recovery tests

- Extended the disposable multi-user suite from 17 to 39 checks per isolation
  mode, retaining its original shared-item and same-order save scenarios.
- Added actual `clsRecall.CancelOrder` versus stale-save overlap. The cancelling
  transaction wins, stock is released only for that order, another reservation
  survives and the stale save cannot reopen the order or add partial history.
- Added a synthetic POS settlement participant with the unchanged packaged
  queue-closing trigger, legacy identity lookup and linked item/tax/tender rows.
  A stale real save waits on its Order row lock and is rejected after commit.
  This is a fixture posting contract, not execution of RMS POS/QSRules or a claim
  that every branch's complete posting stack has been tested.
- Injected a late pre-commit failure only after header, two lines, commitments,
  prepared queue rows and history are verified inside the real save transaction.
  Full rollback snapshots must match, and a second cashier must subsequently
  save its original recalled version without partial changes or double reservation.
- Changes are confined to tests and documentation; no application code, business
  rule, deployment SQL or store database/isolation setting is changed.
- All 39 checks pass in each of the default-locking and fixture-only versioned
  modes. Rollback XML files match byte-for-byte. Both solutions build Release/x86
  and all 101 existing unit tests pass; local store configuration was preserved.

### Set Tax availability setting

- Added **Enable Set Tax button** under Settings > Work Order and Queue Options.
  The installation's `EnableSetTaxButton` setting defaults to true, persists with
  the existing branch settings, and updates the main button after Settings closes.
  Disabled clicks also exit before reading customer tax state or changing rows.
  Existing VAT calculations, customer exemptions, tax reasons and save rules are
  unchanged. No database migration is required.
- Reviewed the highlighted legacy Settings sections: customer-type price bounds
  and approver assignments remain active, and WEBSITE imports still consume the
  import defaults. The legacy website-import button and website list Refresh also
  call the MySQL helper. Kept the existing sections/data to preserve active
  pricing/import behavior despite the website section's deprecated label.
- Both solutions build Release/x86 and all 101 existing unit tests pass. An
  isolated configuration probe verifies default/on/off persistence, disabled
  action guarding, re-enable behavior and preservation of other branch settings.
  The new checkbox layout was rendered and inspected. The local store's Debug
  configuration was restored byte-for-byte; no store data was changed.

### Existing-order multi-user regression tests

- Added a disposable LocalDB suite calling the unchanged compiled existing-order
  save transaction in two independent x86/STA cashier processes. Controlled locks
  and saved SQL request evidence establish actual overlap, rather than treating
  sequential stale-snapshot checks as simultaneous saves.
- Shared-item edits verify both fractional reservation differences and each
  original order line survive with correct amounts, tax and cashier history.
  Same-order edits verify application-lock waiting, one committed winner and one
  stale rejection without partial stock, amount, queue or audit changes.
- Added 17 assertions per run in default-locking and optional fixture-only
  versioned-read modes. The latter forces both cashiers past their original item
  reads before either stock update completes, strengthening lost-update coverage.
  Documented invocation, fixture ownership, synthetic
  selectivity, endpoint isolation and remaining UI/branch/load-test limits.
  No production application code, SQL migration or business rule was changed.
- Both concurrency modes pass all 17 assertions, both solutions build Release/x86
  and all 101 existing unit tests pass. Store-specific Debug configuration was
  preserved byte-for-byte; no live database or store isolation setting changed.

### Requested manual HO Order ID renumbering

- Saved a separate guarded `Reseed_HO_Order_After_Renumber.sql` beside the
  renumber file for personal execution after commit. It verifies the renamed
  headers, transaction/history/receipt links and committed mapping events before
  setting current identity 18870, with next allocation 18871. Later activity is
  rejected; an unchanged repeat is a no-op. Extended isolated coverage to 36
  checks, including a real automatic allocation in the disposable fixture.
- Prepared a standalone SSMS script for the explicitly requested mapping
  18870 -> 18869 and 18879 -> 18870 after a read-only live dependency audit.
  It relinks the audited order entries, queue/group references, posted transaction
  23266, RMS history, order receipt, approvals, custom history and order-only
  item logs while preserving monetary/quantity fields and all child identities.
- Server, current-backup, original-header, identity, dependency and exact-trigger
  guards reject drift. Default rehearsal rolls back; committed runs add explicit
  mapping history. Stored receipt contents/free text remain historical. No live
  renumber, rollback rehearsal or identity reseed was executed by the assistant.
- All 26 disposable LocalDB checks pass, including overlapping destinations,
  injected failures after header copy, unintended-value detection, reference
  drift, archived bytes, unrelated log domains and unchanged identity allocation.
  Added personal execution instructions and linked the original incident.
- Both solutions build Release/x86 and all 101 application unit tests pass;
  the local store configuration was restored byte-for-byte after validation.

### HO new Work Order and import Save timeout

- Read-only live diagnostics located recent approximately 30-second line saves
  in the existing `trg_OrderEntry_ItemLogs` trigger (341,183 logical reads).
  Its unconditional previous-log lookup targets an unindexed heap with roughly
  71.7 million rows. New WO history triggers were fast and history was already
  sequence-backed. No blocker was observed in the sampled active sessions.
- Prepared a standalone, server/fingerprint-guarded SSMS script that skips the
  unused previous-log lookup for new lines and zero/no-existing-order cases.
  It retains the existing log inserts, edit lookup, quantities and business
  rules. Default rehearsal rolls back; live application is left to the user.
  No production trigger, row, index, timeout or order was changed by the assistant.
- All 16 isolated LocalDB checks pass. Log values match the legacy trigger for
  fractional insert/edit/zero cases; synthetic new-line log reads fall from
  5,884 to 1. Documented deployment, reversal and the remaining existing-edit
  lookup cost; this is not a claimed live throughput measurement.
- Both solutions build Release/x86 and all 101 unit tests pass. Store-specific
  local configuration was restored byte-for-byte. The exported trigger fixture
  retains exact bytes in Git so fingerprint tests survive checkout.
- Read-only cross-database metadata confirms shared trigger logic with different
  log volumes: live HO approximately 71.67 million rows versus live GSC 9.64
  million, local DVO 0.927 million and local QC 0.142 million; all four log tables
  lack indexes. Documented the comparison and distinguished local copies from
  live branch data. No live writes or expensive comparison scans were executed.
- Later read-only checks found the fixed log-trigger fingerprint installed and
  Order 18879 persisted. Recorded the user's 18868 → 18870 → 18879 ID gaps:
  identity seed/increment remain 1; missing IDs have no current/history rows in
  the inspected tables. Failed Save rollbacks are a supported explanation, not
  proof of every gap's origin. No identity reseed/reuse or live write occurred.

### HO_PO_DB incident reference

- Documented the October 14 transaction-23266 / Work-Order-18870 history identity
  incident, exact affected record IDs, root cause, recurrence prevention and
  personal SSMS recovery procedure. Linked the incident record from the recovery
  guide and project knowledge.
- Recorded the October 15 live rehearsal's backup guard 52703 and the user's
  subsequent backup-in-progress report. Explained why `@Commit=0` still requires
  the live backup and retained the current backup query and closure evidence list.
  That initial attempt stopped before the repair transaction began.
- Updated the record after the user's SSMS screenshot showed `COMMITTED` in
  live mode for transaction 23266, four item lines, four tax rows, matching totals
  30,200.54, repaired receivable/order-history links and `HistoryUsesIdentity=0`.
  Database recovery is committed; RMS acceptance checks remain pending. Actual
  backup filename/duration and a separate successful rehearsal were not shown.
  Documentation only; the assistant did not execute live SQL or the repair.

### Explicit ReportViewer SQL Types dependency

- Added an explicit `Microsoft.SqlServer.Types` reference at `14.0.314.76`,
  the same version already resolved by NuGet. ReportViewer's unavailable lower
  bound `14.0.0` previously produced NU1603 on restore. This selects the existing
  resolved package deliberately without suppressing warnings or changing RMS
  rules, report layouts or the application's database operations.
- NuGet restore completes without NU1603. Both solutions build Release/x86;
  all 101 unit tests and clean-folder Import/CSV/XLSX/report checks pass. Rebuilt
  and verified the application ZIP; existing local configuration was preserved.

### Remove optional application timing

- Removed Save/Recall/Refresh/catalogue timing scopes, the CSV logging helper,
  credit-warning wait measurement and `EnableOrderPerformanceLogging` setting
  at the user's request. Removed the five tests belonging to the retired helper.
- Preserved database context/transaction scopes, batched reads and fallback
  behavior, snapshot conflict checks, tax/totals calculation, stock commitments,
  shortage handling and the existing credit-limit warning. Updated instructions
  to stop recommending application timing logs. No database migration required.
- Both solutions build Release/x86, all 101 remaining unit tests and 99 isolated
  Work Order workflow checks pass. Clean-folder Import compilation, CSV/XLSX reads
  and both reports pass. Rebuilt the application ZIP; retained local store config
  while removing its obsolete timing setting. No live store records were changed.

### One Work Order history create/update script

- Integrated the guarded legacy IDENTITY-to-sequence conversion into
  `Enable_WorkOrder_History.sql`. One script now creates fresh history or upgrades
  approved legacy history, preserves events/IDs/permissions and creates/updates
  audit triggers atomically. Custom legacy dependencies still stop for review.
- The updater presents one history selection. Retained the old fix file only for
  existing recovery-script compatibility; it is no longer a required extra step.
  Updated deployment instructions. RMS tables, business rules and already
  mislinked transaction records are unchanged. No live migration was executed.
- Added isolated checks for one-selection upgrades, conversion/trigger rollback,
  repeat deployment, catalog contents and later trigger compilation failure.
- Validation: 27 history compatibility checks, 63 updater checks, 99 Work Order
  workflow checks and all 106 unit tests pass. Both solutions build Release/x86;
  existing local Debug config was preserved. Rebuilt the updater ZIP with the
  consolidated installer and current catalog/instructions.

### Import deployment verification

- Traced the reported import MissingMethodException to the September 15 Desktop
  EXE, which differs from the current Release build. The error log contains two
  loaded ExcelDataReader assemblies; the reference build's manual resource
  resolver reloads assemblies without caching. The current build uses Costura.
- Extended the clean-folder probe to JIT-compile the actual Import entry point
  without opening its file picker and reject duplicate Excel dependency loads
  or mismatched reader type identities. Documented using the updated folder and
  correcting shortcuts. No application import or business rules changed.
- Both solutions build in Release/x86; all 106 unit tests and the extended
  clean-folder checks pass. Verified the application ZIP contains the tested
  Release EXE and five ReportViewer DLLs. Existing local config was restored.

### Printing deployment and loading overlay fixes

- Excluded ReportViewer assemblies from Costura embedding so matching physical
  DLLs ship with the EXE. The new ReportViewer package failed report processing
  when loaded in memory; temporary extraction fixed its version-path lookup but
  did not resolve the expression host's separate assembly lookup. Folder deployment
  passed WO and pick-list PDF rendering with no installed ReportViewer 15 needed.
- Retained existing report layouts and print parameter/amount logic. Added clean
  package checks for CSV/XLSX reads, both actual report definitions and physical
  dependency loading. Documented folder deployment and preserving store config.
- Corrected loading-overlay scope ownership: ignored/disposed owners and failed
  creation now return no-op scopes, avoiding unmatched/double decrements. Added
  three regressions; all 106 unit tests pass. Save/import business rules unchanged.
- Rebuilt the updater distribution from current Release output, replacing an
  old archive that contained the unsafe identity-based history installer and
  omitted the compatibility migration. Preserve the prior ZIP under TestResults.
  Packages contain the sequence installer and `Fix_WorkOrder_History_Identity.sql`;
  selected migrations still require personal review/application. No live SQL ran.
- Final validation: both solutions build in x86 Debug/Release; all 106 units,
  99 isolated workflow checks and 60 updater checks pass. Clean-folder CSV/XLSX,
  WO/pick-list rendering, physical ReportViewer location and package-content
  checks pass. Local Debug configuration restored byte-for-byte. Actual printer
  output remains a workstation acceptance check; no physical print job was sent.

### Critical RMS audit identity compatibility and recovery

- Simplified the backup-path placeholder guard after a user-edited script had
  replaced both the declaration and the comparison with the same real path,
  making error 52702 unconditional. The placeholder now occurs only in the
  declaration; validation checks its prefix and blank values. Regenerated the
  local script with the supplied backup path, preserving rollback-only default
  and live backup requirements. All 27 isolated recovery checks pass, including
  placeholder, whitespace and replace-all regression checks. No store repair ran.

- Added a local-only SQL backup helper for users without a `.bak` file. Creates
  a unique copy-only backup without checksums, verifies it, and returns the exact
  backup-path declaration for local recovery. No repair runs in the helper, and
  it rejects other machines. All 24 disposable recovery checks pass, including
  five new helper checks. The assistant did not create a store backup or execute
  a store repair for this addition; the user runs the helper personally.

- Added a separate local-restore recovery SQL file at the user's request. Only
  computer MSI can use local-test mode, which accepts copied backups without
  requiring checksums or a local backup-history record. File verification and
  all repair guards remain. Live mode retains the checksum/copy-only backup
  requirement. All 19 disposable recovery checks pass, including five new local
  exception checks. No additional store repair was executed for this change.

- Recorded the incident's cause and future identity/trigger review requirements
  in GEMINI.md as persistent project knowledge at the user's request.

- Diagnosed missing transaction children: RMS uses `SELECT @@IDENTITY`, and
  inserts through invoice or nested queue audit triggers into the old history
  IDENTITY table could return an audit event ID instead of the actual invoice.
- Added an atomic history conversion to a bigint sequence, retaining every event
  ID/field, index and object/column permission. Unsupported custom dependencies
  stop migration. Existing audit triggers and RMS posting rules are preserved.
  Fresh history uses the sequence; the installer rejects unsafe legacy history.
  The updater orders the compatibility fix before history installation.
- Verified a copy-only/checksum local GSC backup, rehearsed the repair with a full
  rollback, then committed the local transaction 289655 recovery. Relinked its two
  items, two tax rows, tender, receivable, order history and exact item-log references.
  Amounts, inventory/commitment values, original events and triggers were checked.
- Prepared a self-contained SSMS recovery for live HO_PO_DB transaction 23266
  (order 18870): exact four item/tax links, tender, receivable and order history,
  guarded preservation of unrelated old transaction 17, verified-backup requirement,
  rollback rehearsal by default and optional item-log relinking. **No live repair
  executed**; user will run the file personally. A verified live copy-only full
  backup was created before the user requested script-only recovery. Original
  compressed receipt remains historical evidence and can retain the wrong number.
- Both solutions build in x86 Debug/Release. All 103 unit tests, 60 updater checks,
  99 workflow checks, 20 identity checks and 14 standalone HO recovery checks pass.
  Initial unit-test startup was blocked by sandbox socket access; the authorized
  rerun passed. The workstation Debug configuration was restored byte-for-byte.

### HO_PO_DB password-slot column migration

- Added a focused importable migration for HO_PO_DB's existing SOD_WO_Conf table.
  Adds missing Password2, Password3, Pass2Username, and Pass3Username columns as
  nullable nvarchar(50), preserving existing settings and password values.
- Guards the target database, active transaction, table presence, and existing
  field types. No default passwords, data updates, or branch-function replacement.
- Included live-server/GSC-profile import instructions and the migration in the
  updater package. This change prepares the script; no live HO_PO_DB deployment
  has been performed by the assistant.
- Both solutions build in x86 Debug/Release; all 103 unit tests and 60 isolated
  updater checks passed. Sixteen new HO checks cover target/transaction guards,
  nullable additions, existing-value preservation, partial schemas, repeat runs,
  incompatible types, rollback, and missing/conflicting configuration objects.
  Tests use an HO_PO_DB database only inside a GUID-owned disposable LocalDB
  instance; the workstation's local Debug configuration was restored and verified.

### Work Order Database Updater

- Added an offline step-by-step user guide, quick-start text, and an in-tool
  Help / instructions button. Covers connection/profile setup, script import
  and ordering, file replacement, function creation/update, schema/index changes,
  package integration, deployment statuses, logs, and recovery limitations.
- Included SQL Server 2016 compatible demonstration templates for a create/update
  function, additive column migration, and non-unique index; examples are for a
  disposable restore and are never auto-selected. Release builds include the
  instructions/templates alongside the SQL package.

- Added a separate .NET Framework 4.7.2 WinForms updater to the main solution.
  Runs as the current Windows user, with Windows or SQL authentication; it does
  not change the RMS registry or store login passwords.
- Supports explicit selection and SQL preview of five optional migrations and
  ordered GSC/DVO/portable object definitions, plus future reviewed SQL files.
  SQLCMD driver/security commands are excluded; no changes are auto-selected.
- Checks the target database/version/RMS prerequisites and definition visibility,
  requires an explicit target confirmation, rejects changed reviewed files, and
  uses an application lock plus one transaction for the complete selected plan.
- Stores committed deployment history with SHA-256 hashes. Saves selected SQL,
  execution results, previous visible module definitions/settings, and index
  inventory locally. The schema snapshot is not a full backup or automatic undo.
- Added disposable LocalDB checks for catalogs, SQL batch handling, target/permission
  rejection, function create/update, real index reuse, history, multi-script rollback,
  edited files, concurrent updater rejection, and WinForms initialization.
- Both existing solutions and the updater build in x86 Debug/Release. All 103
  existing unit tests and 60 isolated updater checks passed, including the six
  guide-example checks for creation, updates, repeat deployment, and deliberate
  reapplication of a prior compatible function definition; the local Debug
  configuration was restored byte-for-byte. Automated SQL checks used disposable
  databases. The user subsequently confirmed a local GSC item-index deployment
  committed and its record appeared; that run log was verified. Desktop launch
  approval timed out for assistant-driven screen checks; form initialization and
  package loading were exercised in the isolated harness.
- Application workflows and rules are unchanged by adding the tool. Applying a
  selected function/migration still requires review of that script's branch rules.


### Item-code lookup index for recall prices

- Repeated SELECT-only measurements and actual plans for local orders 283845 and
  282844 found a full scan of 69,357 Item rows (5,163 logical page reads) for the
  price lookup. Later warm samples were substantially below the earlier 427 ms
  app sample; no typical full-recall speed improvement is claimed from these reads.
- Prepared optional `Improve_WorkOrder_ItemLookup.sql`: a non-unique ItemLookupCode
  index covering ID/Taxable. It reuses a suitable existing index, is idempotent,
  and stops on a conflicting index name. It changes no prices, quantities, or rules.
- Added a SELECT-only analysis tool that records repeated timings, actual plans,
  I/O, and price signatures for before/after comparisons. No app query change is needed.
- Added five isolated checks on a wide synthetic fixture with 69,211 Item rows.
  Item logical reads fell from 4,645 to 7, returned prices matched, and idempotence,
  duplicate-code behavior, equivalent-index reuse, and collision protection passed.
- Both applications build in x86 Debug/Release; all 103 unit tests and 99 isolated
  SQL/UI checks passed. The local Debug configuration is preserved.
- Applied the tested index to local GSC_STORE_DB after confirming no active user
  transactions, with a three-second lock-wait limit. Creation took 410 ms.
  Afterward Item scans became seeks: reads fell from 5,163 to 6/12 for the two
  orders. All before/after price signatures matched. Warm lookup medians fell
  from 37.889 to 1.434 ms (283845) and 40.622 to 3.972 ms (282844).
  These are database-component results; full app recall still needs measurement.

### Recall tax lookup and processing-time measurement

- Split the recall tax/totals phase into item-tax read/application and total
  calculation timings. Added a separate credit-notice wait record. The existing
  credit-limit warning, its condition, and its place in the workflow are preserved.
- Overall recall, tax/totals, and total-calculation timings exclude the measured
  credit-notice wait. Older logs included that wait, so a shorter new record alone
  does not prove faster processing. Other helper error-dialog waits can still count.
- Recalled taxable orders load item tax flags together, up to 200 codes per query,
  using the original lookup text and database collation. Blank codes, missing items,
  Boolean row flags, and exemption behavior are preserved. Data belongs to one
  recall; customer selection and Set Tax outside recall keep their existing reads.
- The total/VAT formulas, line prices and quantities, credit rules, edit guards,
  save transactions, shortage handling, and inventory rules are unchanged.
- Added two timing-log unit checks and ten isolated SQL/UI checks for tax flags,
  fresh reads, parameter bounds, legacy row values/types, and unchanged totals.
  A SELECT-only comparison for order 283845 returned matching flags and reduced
  two tax requests to one; its mixed timings do not establish a speed gain.
- Both applications build in x86 Debug and Release; all 103 unit tests and 94
  isolated SQL/UI checks passed. Preserved the local Debug database/timing settings.

### Clearer selected-item shortage warning

- When Accept shortage is used on a line without a recalled shortage, the warning
  now displays its item code and tells the user to click the shortage-highlighted
  row in Selected Items. Reload guidance remains for shortages flagged after recall.
- This is a message-only change. Selection, eligibility, customer consent, undo,
  saving, stock reservations, and warehouse rules are unchanged.
- Both applications build in x86 Debug and Release; all 101 unit tests passed.
  Updated the shortage guide and preserved the local Debug configuration.

## [Unreleased] - 2026-10-13

### Fresh batch reads for multi-user recall

- Recall loads last purchase prices together (up to 200 item codes per query),
  retaining CH/CI invoice preference, zero-price fallback, positive quantities,
  customer identity, and date/transaction-number ordering. Item codes are SQL
  parameters. A batch failure falls back to the existing per-item price helper.
  Relevant sales are ranked together, preserving selection across duplicate
  catalogue codes without repeated per-item history scans.
- Warehouse quantity, status, and picker are read once using a fresh context for
  the recalled order. Results exist only within that recall; there is no shared
  or persistent cache. The starting/final snapshots and locked save checks remain.
- Added individual recall timings for snapshots, tax reason, header/entries,
  customer setup/summary, last prices, warehouse details, row population/pricing,
  final validation, and tax/totals. These are included in the overall recall time.
- A SELECT-only comparison on local order 282844 returned matching prices and
  warehouse details in three runs, reducing each component from four queries to
  one. These component measurements do not establish new full recall speed or
  production user capacity. See `TestResults/Recall-ReadOnly-Benchmark.csv`.
- Extended the isolated SQL fixture with price-rule comparisons, fresh reads,
  large-order parameter limits, and two-cashier/warehouse conflict checks.
  Business rules, save transactions, physical stock, and queue stages are unchanged.
- Both solutions build in x86 Debug and Release; all 101 unit tests and 84
  isolated SQL/UI checks passed. Preserved the local Debug database and timing settings.

## [Unreleased] - 2026-10-07

### Remove redundant catalogue loading

- Removed the full-catalogue `getItemtoGrid()` call from `loadData()`. The displayed
  grid already uses the separate TOP-500 query; the extra table's only reader is
  a legacy availability helper with no active callers. Retained the public legacy
  helper and table to keep this change focused.
- Refresh still reads current displayed items from SQL Server. Item search,
  stock validation, save/recall rules, and database records are unchanged.
- The previous sample loaded 69,338 unused rows. Actual refresh and save speed
  after this change remains to be measured in the local application.
- Both applications build in x86 Debug and Release; all 101 unit tests and 65
  isolated SQL/UI checks passed. Preserved the local Debug database and timing settings.

### Set Tax save conflict correction

- Reproduced the "1 of 2 updates failed" error when a new Work Order's tax reason
  was changed. The final direct `VoucherID = 0` update advanced the line's RMS
  rowversion after tax processing had already loaded it, causing LINQ's own
  pending tax update to fail its concurrency check.
- Moved the existing Work Order-only voucher update before loading tracked tax
  entities. Tax reasons, voucher values, transaction rollback, and concurrency
  protection retain their rules; no database migration or tax-rate change is needed.
- Extended the isolated fixture to run production tax preparation with complete
  mapped order/line columns and RMS rowversions. Six checks reproduce the old
  conflict and verify corrected tax saving, voucher behavior, rollback, reversal
  to taxable, and quotation behavior. All 101 unit tests and 65 isolated checks
  passed; both applications build in x86 Debug and Release.

### Save and recall refactor

- Shared the existing minimum-price approval rule between row highlighting and
  final save validation. Recalled prices already approved below the minimum,
  newly added lines, price levels, and password requirements retain their rules.
- Existing-order saves load order entries and involved items once per transaction
  instead of looking them up for each updated/deleted line. The reads remain after
  the locked snapshot check; atomic reservation updates and queue writes are preserved.
- Saves now return explicit Saved, Cancelled, ValidationFailed, Conflict, or Failed
  outcomes. Final validation returns success only after a committed save. Follow-up
  problems after commit show an "Order Saved" warning rather than implying the
  database save failed. Shortage UI cleanup runs after the committed database work.
- Added optional CSV timing for database insert/update, recall population, screen
  refresh, and catalogue reload. Logging defaults off, excludes save/print/grouping
  confirmation waits, contains no customer/credential data, and cannot fail a save.
  This adds measurement capability; live store speed has not been measured.
- Added nine unit tests for price-rule boundaries and safe timing logs. All 101
  unit tests and 59 isolated SQL/UI checks passed; both solutions built in x86
  Debug and Release. Updated testing and diagnostic instructions.
- Existing tax calculations, stock validation, splitting, grouping, printing,
  queue stages, and catalogue refresh policy are unchanged. The mixed-tax VAT
  concern remains a separate business-rule review.

## [Unreleased] - 2026-10-06

### Shortage search and focused refactoring

- Added local search by Work Order number, customer, item code, description,
  and picker, plus picker selection, New / changed only, Zero found only,
  result counts, and Clear filters. Filters combine and do not query SQL Server.
  Missing counts are not treated as zero; quotes/wildcards in searches are literal.
  Empty results disable opening, and clearing filters preserves a visible selection.
- Moved shortage-list presentation/filtering into `ShortageAlertList`, keeping
  the form focused on controls and selection. The notification poll, alert
  acknowledgement, recall confirmation, and shortage acceptance rules are preserved.
- Extracted the existing inventory reservation arithmetic into
  `OrderCommitmentRules`, used by new-order saves, existing-order changes, and
  deleted lines. Type-2-only reservations, non-inventory exemptions, fractional
  quantities, transaction boundaries, tax/price calculation, and queue stages
  retain their existing behavior. This is a focused refactor, not a full main-form rewrite.
- Added 13 unit tests and 8 SQL/UI filter checks; all 92 unit tests and 59
  isolated checks passed. Both solutions built in x86 Debug and Release.
  The filter window was rendered and visually checked; live data was unchanged.
- A database-free local benchmark of 50 searches over 5,000 loaded lines
  measured median 1.50 ms and p95 2.38 ms. These are local list-filter timings,
  not measurements of branch database load or full order-save performance.

### Cashier warehouse shortage alerts

- Added a 15-second background shortage check with a status-bar link and
  **Order actions → Warehouse shortages...**. Lists current open Work Orders,
  customer, item, ordered/found quantities, and picker; new or changed shortages
  are highlighted without repeated alerts for unchanged counts.
- Added direct recall from the list, prioritizing new alerts and confirming
  before discarding current screen work. Closing the list leaves alerts pending;
  successful recall acknowledges only the order/counts loaded, preserving later
  warehouse changes and other orders' alerts.
- Includes zero/missing found counts for follow-up; excludes closed, cancelled,
  quotation, removed-line, and older-than-60-day orders. Poll failures preserve
  the baseline and retry. Queue-disabled branches do not poll. The optional
  `EnableShortageNotifications` setting defaults to true independently of ready
  notifications. No new database migration or inventory change is made.
- Added 9 unit tests and 9 isolated SQL/UI checks. All 79 unit tests and 51
  isolated checks passed; the alert window was rendered and visually checked.
  Both solutions built in x86 Debug and Release. Live databases were unchanged.

### Shortage recall and queue refresh corrections

- Fixed the shortage recall reader's numeric conversion: RMS `Item.ItemType`
  is `smallint`, while the reader expects `Integer`. The SQL projection now
  converts it explicitly, preventing "Specified cast is not valid" during recall.
- Queue card reads and update polling use fresh database contexts. Shortage,
  single-line quantity, and status writes also read fresh records and touch the
  order's update time in the same transaction, avoiding stale tracked state.
- Automatic refresh no longer skips the next detected change after a local
  priority refresh, which could hide another terminal's concurrent update.
- Refresh recomputes card color, badge, and editability from its current rows,
  including Prepared-to-Shortage and Shortage-to-Pending transitions. For
  Invoicing cards remain visible. Failed shortage saves now leave the quantity
  dialog open and report the failure instead of appearing successful.
- Updated the isolated fixture to use RMS's actual smallint item type and added
  queue read/write, automatic-update detection, clear-shortage, and card-stage
  regression checks. All 42 isolated checks and 70 unit tests passed; both
  solutions built in x86 Debug and Release. No live branch data was changed.

### Customer acceptance of warehouse shortages

- Added **Order actions → Reduce selected shortage to found quantity...** for
  recalled queued Work Orders. Uses the saved warehouse count, shows the revised
  line amount and reservation release, and requires explicit customer consent
  plus a note. Pending reductions are marked on screen and can be undone before Save.
- Closing with pending reductions asks to return/save or discard them. Discard
  restores original quantities before draft saving, since drafts do not store
  the customer's agreement note.
- Saving validates the recalled and current warehouse state, then reduces the
  quantity/amounts/reservation, marks the item Prepared, preserves its picker,
  and records a **Shortage accepted** event in the same transaction. RMS stock
  on hand and other orders are unchanged. The existing history migration and
  application permission to insert history events are required.
- Rejects zero/missing counts, ambiguous duplicate item/queue lines, quotations,
  non-inventory items, closed orders, and counts at or above the ordered amount.
- Added 16 unit tests and expanded the isolated SQL/UI fixture to 31 checks,
  including an injected failure after all writes, full rollback, changed physical
  count rejection, unchanged stock on hand, picker preservation, and decision history.
- Verified all 70 unit tests and both solutions in x86 Debug and Release. The
  confirmation window was rendered and checked. No live database was modified.

### Order tools

- Added Order actions with an asynchronous history viewer and explicit reload
  confirmation. History remains available by order number after closure.
- Added the optional SQL Server 2016 history migration with transactional,
  set-based triggers for headers, entries, ownership/grouping, picking,
  price approvals, and RMS transaction posting. Logged approver names exclude passwords.
- Added recalled-state checks for updates and cancellation, including header,
  entries, groups, and warehouse state. Validation acquires database locks
  before writes; changed, closed, missing, or unversioned recalled orders are blocked.
- Added idempotent, non-unique lookup indexes for order/queue checks where
  equivalent usable indexes are absent.
- Added non-interrupting ready-for-invoicing counts with 15-second background
  polling, transition deduplication, failure recovery, and direct opening of
  the existing invoicing dialog. Printing/invoicing still requires explicit action.
- Added 12 database-free tests and an isolated LocalDB migration/runtime smoke
  test. Deployment and branch integration instructions are in `ORDER_TOOLS_GUIDE.md`.
- Verified both solutions in x86 Debug and Release, all 54 unit tests, and 22
  isolated SQL/UI smoke checks. The fixture used LocalDB 17 with compatibility
  level 130; SQL Server 2016 branch-restore pilot testing remains required.

### Documentation review

- Initially updated README and unit-test guidance to describe the then-current 42 test methods,
  queue order-age and logger coverage, repository-root invocation, and runner requirements.
- Corrected the queueing guide's Store-Wide invoicing default, queue column
  names, branch settings location, ownership headers, four card stages, and
  60-day display filters. Separated implemented operational features from proposals.
- Clarified current top-row price selection, quotation ownership headers,
  recall dependencies, and enabled grouping in the implementation plan and
  historical source review.
- Added current implementation notes to the historical performance
  recommendations and corrected the sample collection-membership expression.
- Marked DVO/GSC audit results as historical snapshots. The initial review
  changed documentation only. The subsequent order-tools builds and tests are
  recorded above; performance measurements and live database audits were not rerun.

## [V8.1.4] - 2026-09-21

### 🛡️ Enterprise Global Graceful Error Handling & Crash Prevention
- **Centralized Diagnostic Logger (`ErrorLogger.vb`)**:
  - Implemented thread-safe `ErrorLogger` in both `Work Order Entry` and `OrderProcessingQueueing`.
  - Automatically writes unhandled errors and diagnostic context to `[AppDir]\Logs\Error_YYYY-MM-DD.log`.
  - Captures timestamp, active user/order taker (`usrUsername`), register (`usrRegister`), terminal settings (`PickLoc`, `ReleaseType`), machine name, OS version, full stack traces, and recursive inner exception chains.
  - Fail-safe architecture: logging never throws secondary exceptions if disk operations are constrained.
- **Global Exception Interception in Work Order Entry (`ApplicationEvents.vb`)**:
  - Wired `MyApplication.UnhandledException` event in the VB.NET Application Framework.
  - Intercepts unexpected UI, domain, and background exceptions to prevent abrupt desktop crashes.
  - Displays a clean, professional recovery dialog with actionable error details and log path.
  - Allows the user to keep the application open (`e.ExitApplication = False`) so in-progress orders and screen grid items are preserved.
- **Global Exception Interception in Order Processing Queueing (`ApplicationEvents.vb`)**:
  - Added `ApplicationEvents.vb` handling `MyApplication.UnhandledException`.
  - Intercepts transient database connection timeouts, network glitches, or query hiccups during background timer cycles and suppresses termination (`e.ExitApplication = False`).
  - Ensures warehouse kiosk screens and customer-facing TV Display boards stay active and retry automatically rather than vanishing on transient network drops.
- **Automated Verification**:
  - Added `ErrorLoggerTests.vb` in `Work Order Entry.Tests` covering basic logging, nested inner exceptions, and null-exception safety.
  - All 42 unit tests pass. Both solutions build cleanly in Debug and Release.

## [V8.1.3] - 2026-09-19

### 🧪 Experimental Operational Features (Testing & Warehouse Modernization)
- **Shift Throughput & "Completed Today" Counter (`frmMain.vb`, `frmMain.Designer.vb`, `clsQueueing.vb`)**:
  - Added dedicated throughput counter `📦 Completed Today : N` on the top status panel of `OrderProcessingQueueing`.
  - Automatically queries `clsQueueing.GetCompletedTodayCount()` counting distinct work orders invoiced and completed today.
  - Updates in real-time along with queue count cycles.
- **Bulk Picker Reassignment (`frmPickerSummary.vb`, `clsQueueing.vb`)**:
  - Added `🔄 Reassign...` button on `frmPickerSummary`.
  - Supervisors can reassign all active pending/in-process workload from one picker to another in a single click (e.g., when a picker goes on break or shift change).
  - Automatically batch updates `QueueingItems.Picker`, touches `[Order].LastUpdated`, triggers warehouse card reload, and refreshes workload tallies.
- **Picker Productivity Leaderboard (`frmPickerSummary.vb`, `clsQueueing.vb`)**:
  - Added toggle button `🏆 Leaderboard` / `📋 Workload` on the picker dialog.
  - Computes and displays daily picker ranking: Medal (`🥇`, `🥈`, `🥉`), Picker Name, Items Prepared Today, Orders Completed Today, and Productivity Share %.
- **Shortage / Out of Stock Flagging & Bidirectional Communication (`frmQtyPrep.vb`, `UCGridOrder.vb`, `frmItemLookUp.vb`, `clsRecall.vb`, `clsQueueing.vb`)**:
  - **Warehouse Side**:
    - Added dedicated `⚠️ Shortage` button in `frmQtyPrep` and quantity validation warning when entered quantity is less than ordered.
    - Prompts picker with confirmation of item, ordered quantity, and found stock.
    - Saves `QueueingItems.Status = 'Shortage'`, updates `QtyPre`, and touches `[Order].LastUpdated = DateTime.Now`.
    - `UCGridOrder` highlights shortage item rows in soft amber (`#FEF3C7`) and changes card header badge to `| ⚠️ SHORTAGE (N)` with warning amber header (`#D97706`).
  - **Front Counter Side (`Work Order Entry`)**:
    - `frmRecall` tags orders with shortages with `[⚠️ SHORTAGE] ` in the list comments.
    - `RecallQuote` (`frmItemLookUp.vb`) queries item shortage status and assigned picker name.
    - When recalled, highlights shortage rows in amber and displays an immediate dialog notice: `⚠️ WAREHOUSE SHORTAGE ALERT` listing item code, description, quantity ordered vs found, and the assigned picker name for instant coordination.
- **Visual Wave / Batch Pick View (`frmBatchPick.vb`, `frmBatchPick.Designer.vb`, `frmBatchPick.resx`)**:
  - Added `🌊 Batch Pick` button in `frmMain` top bar opening a consolidated SKU-level wave picking screen.
  - Aggregates all open, in-process items across active queue cards into consolidated pick tasks by SKU and Bin Location.
  - Real-time SKU/description filter and Location filter.
  - Built-in printable Batch Pick Sheet report preview with total quantities and consolidated order list for warehouse runners.
- **Customer-Facing "Now Ready for Pickup" TV Screen (`frmPickupDisplay.vb`, `frmPickupDisplay.Designer.vb`, `frmPickupDisplay.resx`)**:
  - Added `📺 TV Display` button in `frmMain` top bar opening a fullscreen-capable 2-column customer waiting board.
  - Left column: `🔨 BEING PREPARED` (in-process / pending orders).
  - Right column: `✅ READY FOR PICKUP / CASHIER` (prepared & for invoicing orders).
  - Live clock, 5-second auto-refresh from queue cards, and `F11` borderless fullscreen toggle.

### 🐛 Bug Fixes
- **Fixed `ERROR 0003` Object Reference Exception in `frmQtyPrep` (`frmQtyPrep.Designer.vb`)**:
  - Instantiated `Me.btnShortage = New System.Windows.Forms.Button()` in `InitializeComponent()` and restored `Me.Controls.Add(Me.lblGroupID)`, resolving `Object reference not set to an instance of an object` when clicking `Qty Prep` on queue cards.
- **Fixed Empty Data in TV Display Board and Batch Pick View (`frmPickupDisplay.vb`, `frmBatchPick.vb`, `frmMain.vb`)**:
  - Added `GetMainForm()` automatic resolution fallback across `Application.OpenForms` and passed `Me` in `btnBatchPick_Click` / `btnTvDisplay_Click`, resolving empty list (`0 items`) when opened from `frmMain`.

## [V8.1.2] - 2026-09-19

### 🚀 New Features (Tier 1 Operational Enhancements)
- **Picker Assignment Summary Dashboard (`frmPickerSummary.vb` & `frmMain.vb`)**:
  - Added dedicated warehouse picker workload monitoring dialog opened via `👥 Pickers` button on `frmMain` top bar.
  - In-memory aggregation across all active cards: extracts items and pickers (`AJAY`, `JUN`, `MARK`, `CUST`, `[Unassigned]`) directly from memory without slow SQL round-trips.
  - Tally columns: Picker Name, In Process items, Prepared items, Total Items, and Active Orders.
  - Soft amber highlighting (`#FFFBEB` / `#B45309`) for `[Unassigned]` items to immediately signal unallocated workload to supervisors.
  - Click-to-filter interaction: double-clicking a picker row or clicking "Filter Cards" isolates the warehouse queue cards on screen for that picker; "Show All" restores full queue view.
  - Excluded card action footer rows ("Pick All" / "Set all as Prepared") from picker tallies so only real warehouse pickers appear.
  - Widened modal dialog to 740px, enabled compatible text rendering, centered numeric columns, and constrained summary label to eliminate title and button clipping.
  - 5-second automatic refresh timer (`timerRefresh`) keeps picker tallies up to date in real time.
- **Store-Wide "For Invoicing" Checkout (`frmForInvoice.vb` & `clsItemLookUp.vb`)**:
  - Added a `"Store-Wide"` checkbox (checked by default) allowing any register/cashier to view and invoice warehouse-prepared orders across the store, rather than restricting the list solely to the individual user who originally encoded it.
  - Projected `.Encoder = a.OPIS` in `clsItemLookUp.getOrderForInvoicing` so cashiers can see which order taker encoded the work order.
  - When unchecked, preserves user-specific filtering by `frmItemLookUp.usrUsername`.
- **Real-Time Search Bar on Warehouse Queue Screen (`frmMain.vb`)**:
  - Added `txtQueueSearch` with a clear button (`btnClearSearch`) in the top bar of `OrderProcessingQueueing`.
  - Implemented 300ms debounce timer to ensure responsive typing without UI freezing.
  - Searches Customer Name, Work Order #, Group ID, and line-item SKU/ItemCode and Description across active cards in `flowPnl_Orders`.
  - Seamlessly combines with the active Quick-Filter tab (`All`, `For Process`, `Processing`, `Prepared`, `For Invoice`, `Overdue`).
- **Silent Visual Overdue Alert (>20 Minutes) (`frmMain.vb`)**:
  - Designed specifically for speakerless warehouse monitor environments.
  - Added `PanelOverdue` (`🔥 Overdue (>20m) : N`) on the top bar. When overdue active orders exist, the panel highlights in light red (`#FEE2E2`) with crimson text (`#B91C1C`).
  - Single-clicking `PanelOverdue` toggles a quick filter to instantly isolate overdue orders on screen.

### 🐛 Bug Fixes & Queue Lifecycle Synchronization
- **Filtered Closed & Stale Ghost Orders in `frmForInvoice` (`clsItemLookUp.vb`)**:
  - Resolved issue where ancient 2022 ghost work orders (e.g. Work Order 66235) with 0 items and Total: 0.00 were permanently appearing at the top of the "For Invoice" dialog as "Prepared" and would not disappear or invoice.
  - Root cause: `SOD_ViewForInvoice` evaluated `Status = 'Prepared'` whenever `count(total items) = count(prepared items)`. When an order had 0 items in `QueueingItems`, `0 = 0` was TRUE, causing ancient empty/closed orders to be stuck perpetually in the list.
  - Added join to `[Order]` in `clsItemLookUp.getOrderForInvoicing` with filters `o.Closed = False` and `o.Time >= cutoffDate` (60 days), and sorted `Order By o.Time Descending` so only real, active prepared orders appear, with the newest orders at the very top.
- **Fixed `frmForInvoice` NullReferenceException During Form Initialization**:
  - Resolved `System.NullReferenceException: Object reference not set to an instance of an object at WorkOrderEntry.frmForInvoice.InitializeComponent()` on line 98 when clicking `[F7] For Invoice`.
  - Added missing `Me.chkStoreWide = New System.Windows.Forms.CheckBox()` instantiation in `InitializeComponent()`.
- **Restored "For Invoicing" Orders in Order Processing Queueing**:
  - Resolved issue where orders set to "For Invoicing" (e.g. Work Order 282830) disappeared from the warehouse queue screen and failed to display the CornflowerBlue status card and update the bottom "For Invoice" panel counter (`lblForInvoice`).
  - Root cause: In `clsQueueing.vb`, `LoadOpenWO` and `LoadOpenWOUpdate` previously filtered out `AndAlso Not c.Status.Equals("For Invoicing")`. When an order transitioned from "Prepared" to "For Invoicing" in Work Order Entry ([F7] For Invoicing), `LoadOpenWOUpdate` dropped it from open orders, causing `frmMain.removeOldControls` to remove the order card from `flowPnl_Orders`, which in turn prevented `countQueue()` from counting it in `lblForInvoice`.
  - Removed `AndAlso Not c.Status.Equals("For Invoicing")` from both `clsQueueing.LoadOpenWO` and `clsQueueing.LoadOpenWOUpdate`. Active orders remain visible in the queue through all 4 stages (`OldLace` Pending -> `DarkSalmon` In Process -> `PaleGreen` Prepared -> `CornflowerBlue` For Invoicing) until payment is tendered at RMS POS, where trigger `trg_Transaction_closeQueue` sets `Queueing.Status = 1`.
  - Updated `UCGridOrder.vb` (`checkStatus`) to ensure cards with items marked "For Invoicing" (including mixed Prepared/For Invoicing batches) highlight in `Color.CornflowerBlue` with badge `" | For Invoicing"` and dark blue header (`Color.FromArgb(30, 58, 138)`).
  - Updated `clsItemLookUp.vb` (`UpdateQueueStatus`) in Work Order Entry to touch `[Order].LastUpdated = DateTime.Now` upon setting "For Invoicing", ensuring Queueing stations immediately detect the status change via `SOD_fntbl_LastupdateOrder` on their 5-second polling timer.
- **Queue Quick-Filter Tabs on Main Screen**:
  - Single-clicking any top status card (`Total In Queue`, `For Process`, `Processing`, `Prepared`, `For Invoice`) instantly filters visible order cards on screen to that stage without page reloads or leaving the main screen.
  - Clicking the currently active filter tab toggles back to `All` view.
  - Active tab displays clear visual feedback (`Color.FromArgb(215, 230, 250)` light blue highlight with border; inactive tabs remain Ivory).
  - Preserved backward compatibility: Double-clicking any top status card continues to open the legacy `frmInQueue` breakdown list dialog.
  - Top counter totals remain 100% accurate regardless of filter state by counting across all controls irrespective of visibility.
- **Parallel Query Thread Starvation (Error 8642) & Cascading NullReference (Error 0002) Fix**:
  - Resolved `MESSAGE : ERROR 0003 FROM : clsQueueing Class REASON : The query processor could not start the necessary thread resources for parallel query execution` followed by `MESSAGE : ERROR 0002 FROM : UCGridOrder UserControl REASON : Object reference not set to an instance of an object`.
  - Root causes:
    1. Table `dbo.Queueing` (222,000+ rows) and `dbo.QueueingItems` (890,000+ rows) lacked nonclustered indexes on `GroupTo`, `OrderID`, and `QueueingID`. Queries in `clsQueueing.LoadOrders` and `CountOrderEntry` joined 4 large tables (`Queueing`, `QueueingItems`, `Item`, `[Order]`), forcing SQL Server into massive full-table scans across 53M+ row scans that triggered costly parallel query plans which exhausted SQL Server worker threads (Error 8642).
    2. When `LoadOrders` caught Error 8642, it returned `Nothing`, causing `UCGridOrder.LoadOrders` to throw an unhandled `NullReferenceException` (`For Each d In data`), halting queue loading and showing `Total In Queue : ---`.
    3. `CountOrderEntry` executed a redundant 4-table join projecting `ExtendedDescription` (`ntext`) and invoking `db.Refresh()`.
  - Fixes:
    1. Created high-performance nonclustered indexes: `IX_Queueing_GroupTo` (on `GroupTo` INCLUDE `OrderID`, `Status`, `OPIS`), `IX_Queueing_OrderID` (on `OrderID`), and `IX_QueueingItems_QueueingID` (on `QueueingID` INCLUDE `ItemID`, `QtyPre`, `Picker`, `Status`). These turn full table scans into instantaneous single-threaded index seeks, eliminating parallel plans entirely.
    2. Added these indexes to `Ensure_Queueing_Compatibility_Tables.sql` and `Upgrade_GSC_STORE_DB_For_WorkOrderEntry.sql`.
    3. Updated `clsQueueing.LoadOrders` to return an empty list (`New List(Of Object)()`) instead of `Nothing` on exceptions, and removed unnecessary `db.Refresh()`.
    4. Added `If data Is Nothing Then Exit Sub` guard in `UCGridOrder.LoadOrders` to prevent any possibility of `NullReferenceException`.
    5. Streamlined `clsQueueing.CountOrderEntry` to a lightweight `Count()` without joining `Item` or converting `ExtendedDescription`.
- **Row-Snap Scrolling on Warehouse Floor Screen (`flowPnl_Orders`)**:
  - Resolved issue where scrolling the queue screen cut cards in half midway, slicing off card headers (`Group ID`, `Customer`, and table columns).
  - Implemented `MouseWheelMessageFilter` and `flowPnl_Orders_Scroll` snap alignment so mouse-wheel scrolling and scrollbar releases automatically step by exactly one full row (413 px), ensuring cards and headers always align cleanly to the top of the screen.
  - Enabled `DoubleBuffered` on `flowPnl_Orders` to eliminate visual flickering during scrolling.

## [V8.1.1] - 2026-09-18

### ⚡ Performance & Stability Fixes (Order Processing Queueing)
- **Completed & Stale Order Filtering (`LoadOpenWO` / `LoadOpenWOUpdate`)**:
  - Filtered out already-finished items (`Not c.Status.Equals("For Invoicing") AndAlso Not c.Status.Equals("Closed")`) and orders older than 60 days (`b.Time >= cutoffDate`).
  - Reduces loaded warehouse tiles from 409 groups (1,718 item lines, including 317 already-invoiced/finished orders and abandoned orders from 2022–2025) down to ~33–74 actively open orders, cutting memory and control overhead by over 80%.
- **N+1 SQL Query Elimination via Batch Quantity Lookup**:
  - Introduced `clsQueueing.GetBatchQtyOrd(orderIDs)` to fetch ordered quantities for all items in an order group with a single LINQ-to-SQL query.
  - In `UCGridOrder.vb` (`LoadOrders`), replaced per-item `GetQtyOrd`, `getCustomerName`, and `getOrderIDs` calls inside the row loop with a single group lookup and dictionary mapping, eliminating over 3,000 synchronous round-trips to the database on UI load.
- **WinForms Layout Suspension & $O(N)$ Queue Counting**:
  - In `frmMain.vb` (`LoadOrders`), wrapped control creation in `flowPnl_Orders.SuspendLayout()` and `flowPnl_Orders.ResumeLayout(True)` and cached `clsQueueing.getOrderIDs(d)` once per group to prevent layout thrashing, UI freeze, and blank gray tile artifacts.
  - In `frmMain.vb` (`countQueue`), replaced nested $O(N^2)$ loop over `groupIDList` and `flowPnl_Orders.Controls` with four repetitive `Controls.Find()` calls with a single thread-safe $O(N)$ pass over the controls snapshot.
- **Duplicate "Set all as Prepared" Footer Rows Fix**:
  - Fixed bug where `RefreshOrderAge()` called `checkStatus()` on every 10-second timer tick, repeatedly adding duplicate `"Set all as Prepared"` / `"Prep All"` / `"Pick All"` footer rows to active orders.
  - Added `removeFooter()` to remove existing footer rows before adding, ensuring at most one footer row can ever exist.
  - Updated `checkStatus()` to exclude footer rows from item status counting and to automatically remove the footer row when all items are Prepared or For Invoicing.
  - Updated `RefreshOrderAge()` to only refresh the elapsed time badge and header color without modifying grid rows.
- **Hidden GroupID Label ("Grey Rectangle") Display Fix**:
  - Resolved issue where an unintended dark grey rectangle appeared in the header row of warehouse order cards between *Work Order* and *OPIS*.
  - Root cause: In `UCGridOrder.Designer.vb`, a legacy hidden label `lblGroupID` was placed in `flowLayout_Header` with hardcoded `BackColor = ControlDarkDark` and `ForeColor = ControlDarkDark`. When headers were given dynamic priority colors (Red, Amber, Green), the hardcoded grey background became visible as a grey block.
  - Set `lblGroupID.Visible = False` in `UCGridOrder.Designer.vb` and on load, removing the grey artifact from the layout while preserving `lblGroupID.Text` access for code logic.
- **Picker Selection "ERROR 0004" Object Reference Fix**:
  - Resolved `MESSAGE : ERROR 0004 FROM : UCGridOrder UserControl REASON : Object reference not set to an instance of an object` when clicking a line-item's Picker button.
  - Root cause: In `UCGridOrder.vb` (`gridOrder_CellContentClick`), `frmPicker._updateType` was not reset to `"Single"` when an item row picker was clicked, causing `frmPicker` to retain `"All"` state if `"Pick All"` was previously opened. Furthermore, `frmPicker.vb` (`LoadPickers`) attempted to assign `gridPickers.CurrentCell` during `Form_Load` before layout completion, throwing an unhandled `NullReferenceException` which bubbled up to `gridOrder_PickerButtonClick`.
  - Added explicit reset `frmPicker._updateType = "Single"` in `gridOrder_CellContentClick`, corrected DataRow numeric typing (`queueid` and `itemid` as `Int32`), guarded `LoadPickers` and `frmPicker_Load` with `Try ... Catch`, and verified row bounds in `gridOrder_PickerButtonClick`.
- **Queueing "Prep All", "Pick All", and "Set all as Prepared" Batch Database Optimization**:
  - Resolved noticeable UI freezes (1.5–3+ seconds) when clicking **"Prep All"**, **"Pick All"**, or **"Set all as Prepared"** on warehouse queue cards.
  - Root cause: Previously, clicking these buttons executed synchronous SELECT + `SubmitChanges` loops over every item row (e.g. 15 items = 30 to 60 sequential SQL Server round-trips over the network). In addition, single-item `UpdatePicker` called `SubmitChanges` twice per item.
  - Introduced `clsQueueing.UpdateBatchQtyPrep()`, `clsQueueing.UpdateBatchPicker()`, and `clsQueueing.UpdateBatchStatus()` to load matching `QueueingItems` in **1 single query** and submit all updates in **1 single `SubmitChanges()`**, slashing database round-trips from $2N$/$4N$ down to **2**.
  - Optimized single-item `UpdatePicker()` to set both `Picker` and `Status` simultaneously in a single `SubmitChanges()`.
  - Updated `gridOrder_CellContentClick` to update visible in-memory grid cells immediately and wrapped database calls in `Cursor.Current = Cursors.WaitCursor`.
  - Streamlined `frmMain.prioritizeUserControl()` from an $O(N)$ loop over controls to direct `Controls.Find()` lookup.
- **Store Configuration Automatic Sync & Order Touch Optimization**:
  - Resolved issue where updating pickers in **STORE** configuration was not syncing automatically to other stations (requiring manual reload) and experienced lag compared to **UP-STORE**.
  - Root cause: In `clsQueueing.vb`, updating `QueueingItems` never updated `[Order].LastUpdated` in RMS. Station timers check `SOD_fntbl_LastupdateOrder(rType)` which sorts by `[Order].LastUpdated DESC` (TOP 10). In UP-STORE (<10 total orders), orders always appeared in the top 10; but in STORE (40–70 orders), orders whose pickers changed were excluded from the top 10, preventing automatic 5-second updates. Furthermore, `getGroupID()` performed slow XML aggregations over `SOD_ViewForInvoices` on every tick, and `reloadUserControl()` ran nested $O(N)$ control searches.
  - Implemented `TouchOrderLastUpdated()` across all picker, prep, and status update methods in `clsQueueing.vb` to update `[Order].LastUpdated = DateTime.Now` during submits, guaranteeing that updated orders appear at the very top of `SOD_fntbl_LastupdateOrder` for all stations.
  - Optimized `getGroupID()` to perform a direct index seek on `db.Queueings` for numeric order IDs instead of heavy XML subqueries.
  - Streamlined `frmMain.reloadUserControl()` to use direct `flowPnl_Orders.Controls.Find()` lookup, ensuring instantaneous, lag-free automatic updates across all STORE and UP-STORE screens.
- **SQL Migration Script T-SQL Compatibility Fixes (`Upgrade_GSC_STORE_DB_For_WorkOrderEntry.sql`)**:
  - Resolved `Msg 156, Level 15, State 1: Incorrect syntax near the keyword 'LineNo'` on lines 115 and 157 by escaping `[LineNo]` in square brackets in table and index definitions.
  - Escaped reserved keywords `[Date]` and `[timestamp]` in `dbo.SOD_WO_LOGS`.
  - Resolved `The data types nvarchar and ntext are incompatible in the add operator` when deploying `dbo.SOD_ViewItemsWO`.
    - Root cause: In RMS `dbo.Item`, `ExtendedDescription` is defined as `ntext`. In T-SQL, string concatenation (`+`) between `nvarchar` and legacy `ntext` throws an incompatible types error.
    - Wrapped with `CONVERT(nvarchar(max), EXTENDEDDESCRIPTION)` to guarantee safe string concatenation.
  - Resolved `Operand data type varchar is invalid for subtract operator` when deploying `dbo.SOD_ViewItemsWO`.
    - Root cause: In dynamic SQL string literal `N'...'`, `'-' AS [ITEMDES]` was over-escaped as `''''-''''`. At runtime, `sp_executesql` evaluated this as `''-''` (an empty varchar string minus another empty varchar string), triggering arithmetic subtraction on varchar types.
    - Corrected string literal quote escaping from `''''-''''` to `''-''` (`'-'`) and audited all dynamic SQL routines (`SOD_ViewItemsWO`, `SOD_fn_GetAvailableQty`, `SOD_fn_GetConvertQty`, `SOD_fntbl_CheckOpenWO`).
  - Updated `dbo.SOD_fntbl_CheckOpenWO` to use `CONVERT(nvarchar(max), ...)` and standard `CONVERT(varchar(10), ..., 101)` date formatting, ensuring complete compatibility across all SQL Server versions (2008 through 2022).
- **Work Order Print Header Address Alignment (`SOD_fntbl_WoDetails` / ERROR 0019)**:
  - Resolved `MESSAGE : ERROR 0019 FROM : clsItemLookUp Class REASON : Invalid column name 'Address'` when printing Work Orders or viewing For Invoicing templates in `GSC_STORE_DB`.
  - Root cause: `ItemLookUp.dbml` LINQ-to-SQL mapping queries `[Address]` from `dbo.SOD_fntbl_WoDetails`, but the legacy GSC version of the function did not include the `Address` column.
  - Updated `dbo.SOD_fntbl_WoDetails` in `Upgrade_GSC_STORE_DB_For_WorkOrderEntry.sql` and `BranchOverrides/GSC_STORE_DB/Functions/SOD_fntbl_WoDetails.sql` to include `Address nvarchar(255)` with `CONCAT(c.Address, c.Address2, c.City, c.State, c.Zip)`.
  - Added null-guard in `clsItemLookUp.viewWoDetials` to protect against missing order records.

---

## [V8.1.0] - 2026-09-16

### 🚀 Added
- **Allow Duplicate Items Option (Settings-Only)**:
  - Added `AllowDuplicateItemEntry` configuration in `App.config` and `StoreProcessingSettings.vb`.
  - Added `"Allow duplicate items"` setting in `frmSettings` under **Order Entry Features**.
  - When enabled, selecting/scanning items in `frmItemLookUp` creates separate line items rather than incrementing `QTY + 1`.
  - Bypasses duplicate warning dialogs on order save (`InsertEntry`) and order update (`UpdateOrder`).
- **Below-Cost & Low-Margin Visual Warning**:
  - Implemented real-time color highlighting in `gridSelectItem` via `gridSelectItem_CellFormatting`:
    - **Below Cost (`Price < Cost`)**: Price cell highlighted in **Soft Red** (`#FFD7D7`) with dark red text, plus a generic tooltip: *"Price Alert: Selling price is below minimum cost threshold."* (confidential cost amounts are strictly hidden from users).
    - **Low Margin (< 5% above Cost)**: Price cell highlighted in **Soft Yellow / Amber** (`#FFF8C8`) with dark goldenrod text, plus a low-margin warning tooltip: *"Price Alert: Low profit margin on this item."*.
    - **Normal Margin**: Standard white background and empty tooltip.
- **High-DPI Support & Modern Windows Theming**:
  - Added .NET 4.7.2 native `PerMonitorV2` High-DPI section and `EnableWindowsFormsHighDpiAutoResizing` in `App.config` to prevent blurry text and clipped controls on 125%/150% scaled displays.
  - Enabled Windows `Common-Controls` 6.0 manifest dependency in `app.manifest` for modern ClearType font rendering and modern controls.
- **GSC Store DB Upgrade Migration Script**:
  - Created standalone idempotent SQL migration script [`Upgrade_GSC_STORE_DB_For_WorkOrderEntry.sql`](file:///c:/Users/ADMIN/source/repos/WORK%20ORDER%20ENTRY_ASOF_15SEPT2026/Database%20Components/Migrations/Upgrade_GSC_STORE_DB_For_WorkOrderEntry.sql) to prepare `GSC_STORE_DB` for Work Order Entry.
  - Deploys missing tables (`SOD_WO_LOGS`, `SOD_WO_DraftHeader`, `SOD_WO_DraftDetail`, `SOD_ImportedQuote`), views (`SOD_ViewItemsWO`), and functions (`SOD_fn_GetAvailableQty`, `SOD_fn_GetConvertQty`, `SOD_fn_GetCreditLimit`, `SOD_fn_GetTotalOpenWorkOrder`, `SOD_fntbl_CheckOpenWO`).
  - Idempotently adds missing `Pass2Username` and `Pass3Username` columns to `dbo.SOD_WO_Conf` to satisfy LINQ-to-SQL entity mappings and prevent runtime query crashes.
  - Strictly preserves existing GSC-specific warehouse functions and procedures without accidental cross-branch overwrite.
  - Includes automated post-migration object verification report.

### 🎨 UI & UX Improvements
- **Settings Screen Overhaul (`frmSettings`)**:
  - Reorganized cluttered bottom section into a dedicated **`GroupBox8` ("Work Order and Queue Options")**.
  - Structured settings into **Queueing Mode** (with indented dependent options) and **Order Entry Features**.
  - Positioned **Save** button in the bottom-right of `GroupBox8` for intuitive workflow.
  - Renamed `GroupBox4` to **"Branch Configuration"** and `GroupBox5` to **"Website - Deprecate"**.
- **Interactive Branch Database Mismatch Confirmation**:
  - Replaced the hard-blocking "Branch Database Mismatch" error dialog with an interactive confirmation prompt (`Yes`/`No`).
  - When the configured database does not match the local RMS Store Operations database, the app asks if you want to switch and continue with the detected database.
  - Choosing **Yes** automatically updates `ExpectedDatabaseName` in configuration and proceeds smoothly, remembering your selection for future launches.
  - Choosing **No** safely cancels startup.

### ⚡ Performance & Stability Fixes
- **Quote Recall & Bulk Import UI Freeze Prevention (Phase 2 - Item 1)**:
  - Added `Public isBulkLoading As Boolean` state guard in `frmItemLookUp`.
  - In `gridSelectItem_CellValueChanged`, added `If isRestoringDraft OrElse isLoadingRecalledOrder OrElse isBulkLoading Then Exit Sub`.
  - Wrapped `ImportQuote()`, `ImportPO()`, `ImportFromWebsite()`, and `frmUploader.VerifyImportFile()` with `isBulkLoading = True ... Finally isBulkLoading = False`.
  - Eliminates 30–60 redundant blocking SQL Server network queries during bulk row insertion while leaving interactive manual edits fully validated.
- **`ValidateAllQty()` & `ValidateAllQtyForQoute()` Optimization (Phase 2 - Item 2)**:
  - Replaced $O(N^2)$ nested grid iteration with a single-pass $O(N)$ dictionary aggregation (`totalQtyByItem`) to compute requested quantities per item.
  - Added local dictionary caches (`itemTypeCache`, `netAvailableCache`) to eliminate redundant `GetItemType`, `getItemQty`, and `clsRecall.CommittedWO` network queries for repeated line items.
  - Deferred catalog refresh (`loadData()` + `GridColumnWidth()`) until after all line validations finish, executing at most once instead of repeating once per low-stock item found.
- **`SuspendLayout()` / `ResumeLayout()` during Bulk Load & Recall (Phase 2 - Item 3)**:
  - Wrapped `RecallQuote()`, `ImportQuote()`, `ImportPO()`, `ImportFromWebsite()` in `frmItemLookUp.vb`, and `VerifyImportFile()` in `frmUploader.vb` with `gridSelectItem.SuspendLayout()` and `gridSelectItem.ResumeLayout()`.
  - Prevents the WinForms rendering engine from calculating layouts and firing intermediate repaint events for every single row added during bulk import or recall operations, eliminating UI lag on large orders (50+ items).
- **Search Debounce Timers & Rebind Fix in Recall & Invoicing (Phase 2 - Item 4)**:
  - **`frmRecall.vb`**: Added 300ms debounce timer (`searchDebounceTimer`) to `txtSearch_TextChanged` and immediate execution on `Enter`. Removed redundant `gridOrder.DataSource = recallData` re-assignment on every keystroke that previously caused complete DataGridView teardown and rebuild. Added single-quote escaping (`''`) in `BindingSource.Filter` to prevent filter syntax crashes when searching items or customers with apostrophes.
  - **`frmForInvoice.vb`**: Replaced direct per-keystroke LINQ-to-SQL network queries in `txtSearch_TextChanged` with a 300ms debounce timer (`searchDebounceTimer`) and immediate execution on `Enter`. Prevents continuous server queries while typing, dramatically reducing network traffic to SQL Server. Added safe timer disposal in `FormClosing` and Cancel.
- **Removal of Redundant `gridItem.Refresh()` Calls**:
  - Removed redundant `gridItem.Refresh()` calls across `frmItemLookUp.vb` (`SearchItem`, `checkSearch`, `ApplyGridItemLayout`, `gridItem_KeyDown`, `RefreshItemList`, `frmItemLookUp_Load`, `FilterText`, `gridItem_KeyUp`, and `FilterText1`).
  - Setting `gridItem.DataSource` already triggers native DataGridView invalidation and repainting; removing forced `.Refresh()` calls eliminates wasteful synchronous double redraws and makes catalog navigation and search filtering snappier.
- **Smooth Image Browsing with Debounced Loading**:
  - In `frmItemLookUp.vb`, added a 200ms debounce timer (`imageLoadTimer`) to `gridItem_SelectionChanged` when `"Show Image"` is enabled.
  - Rapidly arrowing through the catalog no longer triggers blocking disk I/O on every row change. The image loads smoothly only when navigation pauses on an item.
  - Added `File.Exists` check before calling `Image.FromFile`, avoiding costly `FileNotFoundException` handling when items do not have picture files on disk.
  - Removed disruptive hourglass cursor flickering during image display, and added clean timer disposal on form close.
- **GDI Font Handle Leak in Grid Scrolling**:
  - Replaced on-the-fly `New Font(...)` instantiations in `gridItem_CellFormatting` with cached static instances (`GridBaseFont` and `GridLoadedFont`).
  - Eliminates GDI handle pressure and scrolling stutter.
- **SQL Server Connection Leaks in `ModConnectDB.vb`**:
  - Refactored `load_data(ByVal sql As String)` and `query(ByVal sql_query As String)` to use `Using con As New SqlConnection(...) ... End Using` blocks.
  - Guaranteed connection disposal and closure in all scenarios (success or exception), eliminating connection pool starvation.
- **MySQL Test Connection Leak in `MySQL_DAL.vb`**:
  - Wrapped `mysql_testConnection()` in a `Using` block to prevent open connection leaks.
- **DataGridView Double Buffering**:
  - Enabled protected `DoubleBuffered = True` via reflection on `gridItem` and `gridSelectItem` in `frmItemLookUp_Load`.
  - Eliminates visible screen tearing, repainting artifacts, and flickering during scrolling.
- **Fast Row Selection Guard**:
  - Added early exit in `gridItem_SelectionChanged` when `Show Image` is unchecked, ensuring instant cursor navigation when browsing item lists.

### 🐛 Fixed
- **Customer Last Purchase Price (LPP) Zero-Return Bug**:
  - Fixed issue where customer `'TRUST M'` showed LPP as `0.00` on item `'55157RMS'`.
  - Added `GetItemLastPrice(ByVal customerID As Integer, ByVal itemCode As String)` overload in `clsRecall.vb` to query directly by integer `CustomerID`, eliminating string matching issues caused by trailing spaces or name variations.
  - Added fallback query in `clsRecall.vb` to retrieve the latest transaction where `Price > 0`.
  - Updated `frmItemLookUp.vb` lines 858 & 4437 to pass `iCusID`.
- **Quotation Save Quantity Validation Logic**:
  - Fixed an issue in `frmItemLookUp.vb` (`SaveOrder`) where recalling a Sales Quotation and selecting *"Save Changes"* erroneously ran `ValidateAllQty()`, blocking quote saves if quoted items were out of stock in warehouse inventory.
  - Since Quotations do not commit warehouse inventory, all Quotations (new or recalled) now only validate that quantities are greater than zero (`Quantity > 0`).
- **Order Processing Queueing Picker NullReferenceException Bug**:
  - Resolved `System.NullReferenceException: Object reference not set to an instance of an object at OrderProcessingQueueing.frmPicker.btnProcess_Click` triggered when pressing `[Enter]` or clicking `"Process"`:
    - In `frmPicker.vb`, added guards against empty picker grids (`gridPickers.Rows.Count = 0`), unselected/null rows (`gridPickers.CurrentRow Is Nothing`), and null/whitespace picker values.
    - Added safe bounds check for `_data.Rows.Count > 0` before removing summary row when assigning pickers to all items (`_updateType = "All"`).
    - Enhanced `clsQueueing.LoadPickers()` to safely parse the workstation register number, match pickers assigned to the register, and automatically fall back to all pickers if no register match is found or if `RegisterNo` is NULL in the database, preventing empty picker lists.
    - Updated `LoadPickers()` in `frmPicker.vb` to format column headers (`Reg #`, `Initial`, `Name`) and auto-select the first row on load so `CurrentRow` is always valid.
    - Added double-click event handler (`gridPickers_CellDoubleClick`) so users can double-click a row to immediately select and process a picker.
    - Explicitly set `frmPicker._updateType = "Single"` in `UCGridOrder.vb` when picking an individual item.
- **Order Processing Queueing ERROR 0004 NullReferenceException Bug**:
  - Resolved `MESSAGE : ERROR 0004 FROM : UCGridOrder UserControl REASON : Object reference not set to an instance of an object` occurring when clicking the Picker button:
    - Replaced unsafe `gridOrder.CurrentRow` and `gridOrder.CurrentCell` references in `gridOrder_PickerButtonClick` with `e.RowIndex` and `gridOrder.Rows(e.RowIndex)`, preventing null reference crashes when clicking a button before a row or cell is active.
    - Added bounds checking `e.RowIndex < 0 OrElse e.RowIndex >= gridOrder.Rows.Count` and null/numeric guards on all cell values (`QueueingID`, `ItemID`, `ItemCode`, `Description`, `Picker`).
    - Fixed state clash in `gridOrder_PickerButtonClick`: preserves `_updateType = "All"` when triggered from "Pick All" instead of overwriting it to `"Single"`.
    - Hardened `gridOrder_PrepButtonClick` (ERROR 0003) to use `e.RowIndex` and safe null parsing instead of `CurrentRow`.
    - In `gridOrder_CellContentClick` (ERROR 0007), replaced all `CurrentRow` accesses with `e.RowIndex`, added precise footer button detection (`"Pick All"`, `"Prep All"`, `"Set all as Prepared"`) to prevent single-item orders from falsely matching footer actions, and safely handled null picker values when populating `_data`.
- **Order Processing Queueing Pick List Half-Sheet Printing Fix**:
  - Resolved issue where printing to PDF or physical printer created a full US Letter Landscape (11" × 8.5") page with the pick list squeezed into the upper-left corner:
    - Updated `deviceInfo` in `UCGridOrder.vb` and `frmMain.vb` from `11in × 8.5in` to `8.5in × 5.5in`.
    - Explicitly assigned `PaperSize("HalfLetter", 850, 550)` and `Margins(10, 10, 10, 10)` on `printDoc.DefaultPageSettings` to enforce exact half-sheet dimensions.
    - Added stream filtering (`<= 408 bytes`) to eliminate blank trailing pages from printing.
    - Expanded `rpt_PickList.rdlc` layout from `5.43in` to `8.20in` wide, enlarging the table to `7.93in` across 7 columns (widening the Description column from `1.54in` to `3.03in` and Qty Picked line to `1.25in`) and right-aligning the header textboxes to the 8.10in margin for a balanced, full half-sheet presentation matching Work Order Entry.
- **Work Order Entry Missing "Work Order" Header on For Invoicing**:
  - Resolved issue where printing orders from the **For Invoicing [F7]** screen (`frmForInvoice.vb`) resulted in a blank document header above `#: <OrderNumber>` on the printed sheet (`rprtOrder.rdlc`).
  - In `frmForInvoice.vb` (`btnGroup_Click`), explicitly set `frmPrintWo.Type = OrderPrintRules.WorkOrderTemplate` before invoking `frmPrintWo.ShowDialog()`.
  - In `OrderPrintRules.vb`, introduced `WorkOrderTemplate = "Work Order"`, `SalesQuotationTemplate = "Sales Quotation"`, and `ResolveTemplateName()` to safely fallback to `"Work Order"` if a template name is missing, empty, or whitespace.
  - In `frmPrintWo.vb`, passed resolved template name to `paramType` report parameter, and reset `Type = ""` and `wo = 0` in `frmPrintWo_FormClosed` to prevent stale template state leaking across invocations.
  - Added unit test coverage in `OrderPrintRulesTests.vb` to verify `ResolveTemplateName()` default and explicit behavior.

### 🧪 Experimental Features (Queueing)
- **Order Age & Priority Visual Highlighting (`OrderProcessingQueueing`)**:
  - Implemented real-time order age tracking and dynamic priority color-coding on warehouse order tiles (`UCGridOrder`):
    - **Fresh (`< 10 mins`)**: Dark Forest Green header (`#2E7D32`) with `⏱️ <time>`.
    - **In Progress (`10–20 mins`)**: Warm Amber header (`#D97706`) with `⚠️ <time>`.
    - **High Priority / Overdue (`> 20 mins`)**: Crimson Red header (`#C62828`) with `🔥 <time>`.
    - **Prepared**: Pale green card with `" | Prepared"`.
    - **For Invoicing**: Cornflower blue card with `" | For Invoicing"`.
  - Added `QueueOrderAgeRules.vb` with clean toggle `EnableOrderAgeAlerts` (can be switched off anytime or configured in `App.config`).
  - Added periodic age recalculation (`RefreshOrderAges`) on each `chk_Panels` timer tick in `frmMain.vb`.
  - Added 10 unit tests in `QueueOrderAgeRulesTests.vb` covering priority level calculation, time formatting, badge text, and color resolution (Total suite: 39 tests passed).
