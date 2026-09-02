# Recommended Fixes for Work Order Entry Performance Issues

> **Date:** July 9, 2026  
> **Based on:** Performance Investigation Report — Database & UI Grid Analysis  
> **Status:** Recommendations Only — No Code Has Been Revised

---

## Priority Legend

| Priority | Meaning | User-Visible Symptom |
|----------|---------|----------------------|
| 🔴 P0 | Fix immediately — causes freezes/hangs | App freezes, stops responding |
| 🟠 P1 | Fix soon — causes noticeable lag | Sluggish scrolling, slow saves |
| 🟡 P2 | Fix when possible — causes minor stutter | Flickering, minor delays |

---

## PART A: Grid / UI Fixes

---

### Fix #1 — Cache Font Objects in CellFormatting (🔴 P0)

**Problem:** `gridItem_CellFormatting` (frmItemLookUp.vb, Lines 663–693) creates two `New Font(...)` objects every time any cell is repainted. With hundreds of visible cells, this produces thousands of font allocations per scroll.

**Recommended Fix:**
- Declare the fonts as **class-level `Private Shared ReadOnly`** fields so they are created once and reused forever.
- Move the row-level font assignment out of `CellFormatting` and into the `DataBindingComplete` event (fires once after data loads, not per cell).

```vb
' At class level (created once):
Private Shared ReadOnly _baseFont As New Font("Arial", 12.0F, FontStyle.Bold)
Private Shared ReadOnly _loadedFont As New Font("Arial", 12.0F, FontStyle.Bold)

' Move styling to DataBindingComplete instead of CellFormatting:
Private Sub gridItem_DataBindingComplete(...) Handles gridItem.DataBindingComplete
    For Each row As DataGridViewRow In gridItem.Rows
        If isLoaded Then
            row.DefaultCellStyle.Font = _loadedFont
        Else
            row.DefaultCellStyle.Font = _baseFont
        End If
    Next
End Sub
```

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 663–693

---

### Fix #2 — Async / Debounced Image Loading in SelectionChanged (🔴 P0)

**Problem:** `gridItem_SelectionChanged` (frmItemLookUp.vb, Lines 4343–4363) loads image files from disk synchronously on every row change. This freezes the UI, especially if images are on a network path.

**Recommended Fix:**
- Guard the call: exit early if `chkShowImage.Checked = False`.
- Use a debounce timer (e.g., 200ms) so rapid key-presses don't queue up file reads.
- Load the image on a background thread using `Task.Run()` or `BackgroundWorker`, then marshal the result back to the UI thread.
- Cache recently loaded images in a `Dictionary(Of String, Image)` to avoid re-reading the same file.

```vb
Private Sub gridItem_SelectionChanged(...) Handles gridItem.SelectionChanged
    If Not chkShowImage.Checked Then Return   ' Skip if images disabled
    
    imageLoadTimer.Stop()
    imageLoadTimer.Interval = 200    ' Debounce: wait 200ms before loading
    imageLoadTimer.Start()
End Sub

Private Sub imageLoadTimer_Tick(...) Handles imageLoadTimer.Tick
    imageLoadTimer.Stop()
    Dim itemCode = gridItem.Item(0, gridItem.CurrentRow.Index).Value.ToString()
    
    Task.Run(Sub()
        Dim img = Image.FromFile(rmsPath("Pictures") & "HD\" & itemCode & ".JPG")
        Me.Invoke(Sub() itemImage = img)   ' Marshal back to UI thread
    End Sub)
End Sub
```

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 4326–4363

---

### Fix #3 — Skip DB Queries During Bulk Row Operations (🔴 P0)

**Problem:** `gridSelectItem_CellValueChanged` (frmItemLookUp.vb, Line 2542) calls `ValidatePriceRow()` which runs LINQ queries on the database. This fires on every cell change — including during `RecallQuote()`, `ImportQuote()`, etc. which add rows one-by-one.

**Recommended Fix:**
- Expand the existing `isRestoringDraft` guard flag to cover all bulk-loading scenarios (recall, import, etc.).
- Set the flag to `True` before adding rows, and `False` after all rows are added.
- In `CellValueChanged`, exit early when the flag is `True`.

```vb
' Before bulk loading:
isRestoringDraft = True
gridSelectItem.SuspendLayout()

' ... add all rows ...

gridSelectItem.ResumeLayout()
isRestoringDraft = False
```

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 2542–2569 (CellValueChanged guard)
- `Forms/frmItemLookUp.vb` — Lines 3685–3741 (RecallQuote)
- `Forms/frmItemLookUp.vb` — Lines ~3828 (ImportQuote)
- `Forms/frmItemLookUp.vb` — Lines ~3905 (ImportPO)
- `Forms/frmItemLookUp.vb` — Lines ~4003 (ImportFromWebsite)

---

### Fix #4 — Eliminate O(n²) in ValidateAllQty / Remove loadData() from Loop (🔴 P0)

**Problem:** `ValidateAllQty()` (frmItemLookUp.vb, Lines 2814–2920) uses nested loops ($O(n^2)$) and calls the database per row. Worse, it calls `loadData()` (which reloads the entire item grid from the database) inside the inner loop.

**Recommended Fix:**
- **Pre-fetch** all item types and quantities in a single batch query before the loop starts. Store them in a `Dictionary(Of String, ...)`.
- **Replace the inner loop** with a `Dictionary` lookup to sum committed quantities.
- **Move `loadData()` outside** the loop — only call it once after the entire validation is complete, if needed.

```vb
' Pre-fetch all needed data in ONE query:
Dim itemData = (From a In db.Items
                Where a.ItemLookupCode In itemCodes
                Select a.ItemLookupCode, a.ItemType, a.Quantity
               ).ToDictionary(Function(x) x.ItemLookupCode)

' Pre-sum committed qty per item from grid:
Dim committedQty As New Dictionary(Of String, Double)
For Each row As DataGridViewRow In gridSelectItem.Rows
    Dim code = row.Cells(0).Value?.ToString()
    If code IsNot Nothing Then
        If Not committedQty.ContainsKey(code) Then committedQty(code) = 0
        committedQty(code) += Val(row.Cells(2).Value)
    End If
Next

' Now validate without any DB calls or nested loops:
Dim needsReload As Boolean = False
For Each code In committedQty.Keys
    Dim avail = itemData(code).Quantity - committedQty(code)
    If avail < 0 Then needsReload = True
Next

If needsReload Then loadData()   ' Only once, after the loop
```

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 2814–2920 (`ValidateAllQty`)
- `Forms/frmItemLookUp.vb` — Lines 3028–3159 (`ValidateAllQtyForQuote` — same pattern)

---

### Fix #5 — Use SuspendLayout / ResumeLayout for Bulk Row Adds (🟠 P1)

**Problem:** `RecallQuote()`, `ImportQuote()`, `ImportPO()`, and `ImportFromWebsite()` add rows one at a time via `gridSelectItem.Rows.Add()`. Each add triggers layout recalculation, column resizing (since `AutoSizeColumnsMode = Fill`), and event handlers.

**Recommended Fix:**
```vb
gridSelectItem.SuspendLayout()
' ... loop adding rows ...
gridSelectItem.ResumeLayout()
```

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 3685–3741, ~3828, ~3905, ~4003

---

### Fix #6 — Remove Redundant gridItem.Refresh() Calls (🟠 P1)

**Problem:** Over 15 locations call `gridItem.Refresh()` immediately after assigning `gridItem.DataSource`. The DataSource setter already triggers a repaint, so this forces a wasteful double redraw.

**Recommended Fix:**
- Remove the `.Refresh()` call from all locations listed below.

**Locations (Lines in frmItemLookUp.vb):**
180, 211, 218, 243, 426, 706, 717, 1124, 1880, 3291, 3348, 3360, 4826, 5022

---

### Fix #7 — Enable DoubleBuffered on DataGridViews (🟡 P2)

**Problem:** None of the DataGridView controls have `DoubleBuffered = True`. This causes visible flickering during repaints, especially on `gridItem` which uses large row heights (70px).

**Recommended Fix:**
- Create a helper method to set the protected `DoubleBuffered` property via reflection:

```vb
Private Sub EnableDoubleBuffering(dgv As DataGridView)
    Dim dgvType = dgv.GetType()
    Dim pi = dgvType.GetProperty("DoubleBuffered",
        Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
    pi.SetValue(dgv, True, Nothing)
End Sub
```

Call this at form load for `gridItem` and `gridSelectItem`.

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Form_Load

---

### Fix #8 — Debounce frmForInvoice and frmRecall Search (🟡 P2)

**Problem:**
- `frmForInvoice.txtSearch_TextChanged` (Line 10) runs a database query on every keystroke.
- `frmRecall.txtSearch_TextChanged` (Line 126) re-sets the grid DataSource redundantly after filtering.

**Recommended Fix:**
- Use a 300ms debounce timer: start/restart the timer on each keystroke, and only execute the search/filter when the timer ticks.
- In `frmRecall`, remove the redundant `gridOrder.DataSource = recallData` line since `BindingSource.Filter` already updates the view.

**Files to Change:**
- `Forms/frmForInvoice.vb` — Lines 10–12
- `Forms/frmRecall.vb` — Lines 126–143

---

## PART B: Database / Connection Fixes

---

### Fix #9 — Fix Connection Leak in load_data() (🔴 P0)

**Problem:** `ModConnectDB.load_data()` (Lines 175–197) has a `Return` statement before `sql_con.Close()`, making the close unreachable. Every call leaks a connection.

**Recommended Fix:**
- Wrap the connection in a `Using` block so it is guaranteed to close even if an exception occurs.

```vb
Public Function load_data(ByVal query As String) As DataTable
    Dim dt As New DataTable()
    Using con As New SqlConnection(DB_Conn("constr"))
        Using da As New SqlDataAdapter(query, con)
            con.Open()
            da.Fill(dt)
        End Using   ' Connection auto-closes here
    End Using
    Return dt
End Function
```

**Also fix `query()` (Lines 159–172)** — same pattern: add `Finally` block or use `Using`.

**Files to Change:**
- `Module/ModConnectDB.vb` — Lines 159–197

---

### Fix #10 — Fix Connection Leak in mysql_testConnection() (🔴 P0)

**Problem:** `MySQL_DAL.mysql_testConnection()` (Lines 21–29) opens a MySQL connection and returns `True` without ever closing it.

**Recommended Fix:**
```vb
Public Function mysql_testConnection() As Boolean
    Try
        Using con As New MySqlConnection(_connectionString)
            con.Open()
            Return True
        End Using   ' Auto-closes
    Catch
        Return False
    End Try
End Function
```

**Files to Change:**
- `Module/MySQL_DAL.vb` — Lines 21–29

---

### Fix #11 — Replace Global DataContext with Short-Lived Instances (🔴 P0)

**Problem:** `ModConnectDB.vb` (Lines 10–12) declares global `db`, `dbnew`, and `dbInitial` DataContexts that are never disposed. They accumulate tracked entities in memory forever, causing increasing memory usage and slower queries over time.

**Recommended Fix:**
- Replace global DataContexts with a factory method that creates short-lived, scoped instances.
- Use `Using` blocks everywhere a DataContext is needed.

```vb
' Replace:
Public db As New ItemLookUpDataContext(DB_Conn("constr"))

' With factory method:
Public Function GetDB() As ItemLookUpDataContext
    Return New ItemLookUpDataContext(DB_Conn("constr"))
End Function

' Usage:
Using db = GetDB()
    Dim result = (From a In db.Items Where ...).ToList()
End Using
```

> ⚠️ **Note:** This is a large change that affects many files using `db` directly. It should be done carefully, file by file.

**Files to Change:**
- `Module/ModConnectDB.vb` — Lines 10–12
- `Class/clsImport.vb` — Lines 95, 130, 169, 241 (uses global `db`)
- All other files referencing the global `db` variable

---

### Fix #12 — Eliminate N+1 Queries in clsRecall (🟠 P1)

**Problem:** Several methods in `clsRecall.vb` open multiple database connections inside loops:
- `DeleteOrderItem` (Lines 393–458): 5 queries per item in a loop.
- `CancelOrder` (Lines 460–505): Opens a new connection per item.
- `UpdateOrderEntry` (Lines 297–390): Creates 2–3 nested DataContexts.

**Recommended Fix:**
- **Batch queries:** Fetch all needed data in a single query before the loop, store in a `Dictionary`, then loop without DB calls.
- **Single DataContext:** Use one `Using` block for the entire operation instead of opening/closing per item.
- **Batch SubmitChanges:** Accumulate all changes, then call `SubmitChanges()` once at the end.

```vb
' Example for DeleteOrderItem:
Using dbx = GetDB()
    ' Fetch all needed data in ONE query
    Dim allItems = (From oe In dbx.OrderEntries
                    Where aItems.Contains(oe.ItemID)
                    Select oe).ToList()
    
    ' Process in-memory
    For Each item In allItems
        dbx.OrderEntries.DeleteOnSubmit(item)
    Next
    
    dbx.SubmitChanges()   ' Single round-trip
End Using
```

**Files to Change:**
- `Class/clsRecall.vb` — Lines 297–505

---

### Fix #13 — Batch DB Queries in RecallQuote (🟠 P1)

**Problem:** `RecallQuote()` in frmItemLookUp.vb (Lines 3685–3741) runs 3 database queries per row item (GetItemLastPrice, QueueingItems LINQ query, SetApprovedPriceText).

**Recommended Fix:**
- Before the loop, fetch all last prices, queueing quantities, and approved prices in bulk queries.
- Store them in dictionaries keyed by item code.
- Inside the loop, look up values from the dictionaries instead of querying the database.

**Files to Change:**
- `Forms/frmItemLookUp.vb` — Lines 3685–3741
- `Class/clsRecall.vb` — `GetItemLastPrice` (Lines 684–699)

---

### Fix #14 — Replace .ToList()(0) with .FirstOrDefault() (🟡 P2)

**Problem:** Multiple locations use `.ToList()(0)` which fetches the entire result set into memory just to get the first row.

**Recommended Fix:**
- Replace all instances of `.ToList()(0)` with `.FirstOrDefault()`.

**Locations:**
- `Class/clsItemLookUp.vb` — Lines 494, 523, 550, 578
- `Class/clsCustomer.vb` — Line 103
- `Class/clsPickList.vb` — Line 80

---

### Fix #15 — Add Connection String Tuning (🟡 P2)

**Problem:** Connection strings in `App.config` lack performance-related settings.

**Recommended Fix:**
- Add these parameters to the SQL Server connection string:

```
Connection Timeout=15;Max Pool Size=50;Min Pool Size=5;
```

- Add `DefaultCommandTimeout=30` to MySQL connection string.
- Remove the 6 unused/duplicate connection strings from App.config (keep only the active one).

**Files to Change:**
- `App.config` — Lines 8–33

---

### Fix #16 — Fix N+1 in clsCustomer.loadCustomerOpenWo (🟡 P2)

**Problem:** `loadCustomerOpenWo()` (clsCustomer.vb, Line 81) calls `getOrderIDs()` inside a LINQ `Select`, which executes a new database query for every row returned.

**Recommended Fix:**
- Fetch all order IDs in a single join query instead of calling a sub-function per row.

**Files to Change:**
- `Class/clsCustomer.vb` — Lines 71–92

---

### Fix #17 — Remove Redundant EnsureTables() Calls (🟡 P2)

**Problem:** `clsWorkOrderDraft.EnsureTables()` (Lines 38–47) runs 2 database queries to check if tables exist. It is called before every draft operation (5 times total).

**Recommended Fix:**
- Call `EnsureTables()` once at application startup, not before every operation. Cache the result in a static `Boolean` flag.

```vb
Private Shared _tablesVerified As Boolean = False

Public Shared Sub EnsureTables()
    If _tablesVerified Then Return
    Using db = GetDB()
        db.SOD_WO_DraftHeaders.Take(1).ToList()
        db.SOD_WO_DraftDetails.Take(1).ToList()
    End Using
    _tablesVerified = True
End Sub
```

**Files to Change:**
- `Class/clsWorkOrderDraft.vb` — Lines 38–47

---

## Implementation Priority Order

We recommend tackling these fixes in the following order for maximum impact:

| Phase | Fixes | Expected Improvement |
|-------|-------|---------------------|
| **Phase 1 — Stop the Bleeding** | #1 (Font cache), #9 (Connection leak), #10 (MySQL leak) | Eliminates memory/connection exhaustion that worsens over time |
| **Phase 2 — Unfreeze the UI** | #2 (Async images), #3 (Bulk load guard), #4 (ValidateAllQty) | Removes the biggest UI-freezing operations |
| **Phase 3 — Reduce DB Load** | #5 (SuspendLayout), #11 (Global DataContext), #12 (N+1 in Recall) | Cuts database round-trips by 80%+ for common operations |
| **Phase 4 — Polish** | #6 (Refresh), #7 (DoubleBuffer), #8 (Debounce), #13–#17 | Smoother scrolling, minor speed improvements |

---

## Notes

- **No code has been changed** — this document contains recommendations only.
- Fixes in Phase 1 and Phase 2 should resolve the most visible lagging.
- The connection leak (Fix #9) is especially urgent as it causes the app to progressively slow down the longer it runs.
