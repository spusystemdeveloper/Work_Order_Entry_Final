# Changelog

All notable changes to the **Work Order Entry** application are documented in this file.

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
