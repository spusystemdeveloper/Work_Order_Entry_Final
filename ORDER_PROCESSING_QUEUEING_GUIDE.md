# Order Processing Queueing Guide

This guide explains how **Order Processing Queueing** works, how it integrates with **Work Order Entry** and **Microsoft Dynamics RMS**, and how to configure and troubleshoot it.

**Documentation checked against source:** October 6, 2026. Operational features
described below are present in the current source; this update did not verify a
deployed installation or a live database.

---

## 1. Overview & Architecture

In high-volume retail and wholesale store branches (such as Gensan), the sales and fulfillment workflows are divided into two physical locations:

1. **Sales Counter / Cashier (`Work Order Entry.exe`):**  
   Order takers create Work Orders, select customer accounts, set pricing/discounts, and commit inventory.
2. **Warehouse / Picking Floor (`OrderProcessingQueueing.exe`):**  
   Warehouse staff view queued orders on a wall monitor or warehouse PC, assign physical stock pickers, record prepared quantities, and notify the cashier when items are ready for release.
3. **Tender Register (RMS POS):**  
   Cashier pulls the prepared order via **For Invoicing [F7]**, prints the final Work Order, collects payment at POS, and closes the transaction.

---

## 2. Order Lifecycle Across Three Applications

```
[1. Work Order Entry]            [2. Order Processing Queueing]           [3. RMS POS]
Sales Counter / Cashier              Warehouse Floor                      Register / Tender
         │                                  │                                     │
Creates Work Order (Type 2)                 │                                     │
Chooses Pick-up or Delivery                 │                                     │
Sets Item Pick Locations                    │                                     │
Saves Order  ───────────────────► Appears on Warehouse Screen                     │
(Status: "In Queue")              Supervisor assigns Picker                       │
                                  Picker enters Prepared Qty                      │
                                  Order marked "Prepared"  ────────► Appears in [F7] For Invoicing
                                  (Status: "Prepared")               Cashier prints final Work Order
                                                                     Tenders transaction at POS
                                                                     DB Trigger sets Status: "Closed"
```

### Detailed Lifecycle Stages:

| Stage | Application | Who | Status | What Happens |
|---|---|---|---|---|
| **1. Order Creation** | `Work Order Entry` | Sales Rep / Cashier | Pending / For Process | Saves Work Order (Type 2). Commits stock. Creates a `Queueing` ownership header and, when branch queueing is enabled, `QueueingItems`. Grouping uses `GroupTo`. |
| **2. Warehouse Picking** | `OrderProcessingQueueing` | Warehouse Picker | Pending → In Process → Prepared | Order appears as a visual tile. Supervisor assigns a picker from `PickerList`. Staff gathers items and records prepared quantity in `QtyPre`. A group becomes prepared when all required items are prepared. |
| **3. For Invoicing** | `Work Order Entry` | Cashier | `"Prepared"` → `"For Invoicing"` | Cashier presses **[F7] For Invoicing**. Selects the prepared order batch. Prints the final Work Order. Status updates to `"For Invoicing"`. |
| **4. POS Settlement** | RMS Store Operations POS | Cashier | `"Closed"` | Cashier tenders payment. SQL trigger `trg_Transaction_closeQueue` automatically detects the completed sale and closes the queue record. |

The queue header uses numeric `Queueing.Status`: `0` for open, `1` for
completed, and `2` for cancelled. Item preparation uses text values in
`QueueingItems.Status`; the table above describes the operator-facing stages.
"In Queue" in the diagram is a workflow label, not a queue-header status value.

Active warehouse cards progress through **OldLace** (For Process),
**DarkSalmon** (In Process), **PaleGreen** (Prepared), and **CornflowerBlue**
(For Invoicing). For Invoicing cards stay visible and count toward **For Invoice**
until RMS settlement closes their queue records. The current warehouse loaders
also limit displayed orders to the past 60 days and apply terminal filters.

---

## 3. Key Configuration Settings

### A. In Work Order Entry (`App.config` & Settings Form)

Configured under **Settings → Branch Functions**:

* **`QueueingEnabled`:**
  * `True`: Enables warehouse `QueueingItems` processing (Gensan branch style). Grouping and `[F7] For Invoicing` also require their individual settings below.
  * `False`: Direct processing mode (Davao branch style). Type-2 Work Orders still commit stock. Processing queue items are removed, ownership headers remain for recall, and `[F7] For Invoicing` is hidden.
* **`AllowOrderGrouping`:**
  * When branch queueing is enabled, prompts the cashier to group multiple orders for the same customer under a shared `Queueing.GroupTo`, allowing warehouse staff to pick those orders together.
* **`ShowForInvoiceButton`:**
  * Controls the visibility of the **For Invoicing [F7]** button on the main screen; queueing must also be enabled.

Sales Quotations (type `3`) retain an ownership header for recall but have no
processing queue items and do not reserve inventory.

### B. In Order Processing Queueing (`OrderProcessingQueueing.exe`)

Configured in `frmSetup` on startup of the warehouse application:

* **Pick Location (`PickLoc`):**
  * `STORE`: Shows items assigned to ground floor / store stock.
  * `UP-STORE`: Shows items assigned to upper mezzanine / warehouse stock.
* **Release Type (`ReleaseType`):**
  * `Delivery`: Shows orders where customer requested shipping/delivery (`Order.Comment` contains "Delivery").
  * `Pick-up`: Shows orders where customer will pick up at the counter (`Order.Comment` contains "Pick-up").

> ⚠️ **Important:** A warehouse terminal configured for `Delivery` + `UP-STORE` will **only** display items matching both criteria. If an order was encoded as `Pick-up`, it will only appear on terminals set to `Pick-up`.

---

## 4. Database Objects & Prerequisites

Both applications connect to the same store database and rely on these core objects:

| Object Name | Type | Purpose |
|---|---|---|
| `Queueing` | Table | Header record linking `OrderID`, `GroupTo`, `OPIS` (order taker username), and numeric `Status`. |
| `QueueingItems` | Table | Queue item records with `QueueingID`, `ItemID`, `PickLoc`, `QtyPre`, `Picker`, and text `Status`. Ordered quantity is read from RMS `OrderEntry.QuantityOnOrder`. |
| `PickerList` | Table | Master list of authorized warehouse picking staff. |
| `SOD_Item_Logs` | Table | Audit log recording inventory adjustments and queue changes. |
| `SOD_ViewForInvoice` | View | Returns open queue groups with a computed readiness status. The application filters for `Prepared`, open RMS orders, and orders from the past 60 days. |
| `SOD_viewTableLastUpdate` | View | Used by `OrderProcessingQueueing` to detect when new orders are created. |
| `trg_Transaction_closeQueue` | Trigger | Trigger on RMS `Transaction` table that closes the queue record when payment is tendered. |

---

## 5. Troubleshooting Checklist

### Issue 1: Order does not appear on the Warehouse screen
1. **Check Release Type:** Was the order created as `Pick-up` or `Delivery`? Ensure the warehouse terminal is set to the matching mode.
2. **Check Pick Location:** In `Work Order Entry`, is the `Pick Loc` checkbox checked (`UP-STORE`) or unchecked (`STORE`)?
3. **Check Database:** Confirm that both `Work Order Entry` and `OrderProcessingQueueing` are connected to the exact same SQL database.
4. **Check Age and Stage Filters:** The warehouse loaders restrict orders to the past 60 days. Check the active quick filter and search text. For Invoicing orders remain active until settlement.

### Issue 2: Order does not appear in [F7] For Invoicing
1. **Status Check:** Orders only appear in `[F7]` once the warehouse has marked all items as **"Prepared"**. If an order is still "In Queue", it will not show up.
2. **Store-Wide / OPIS Filter:** **Store-Wide** is checked by default, so any cashier can view eligible prepared orders across the store. Clear it to restrict the list to the logged-in order taker (`frmItemLookUp.usrUsername`). The Encoder column identifies the original order taker.
3. **Closed / Older Orders:** The application excludes closed orders and orders older than 60 days. Readiness comes from the queue-group view; investigate missing or inconsistent queue-item records if a group is unexpectedly marked prepared.

---

## 6. Operational Features Present in Source

These features are implemented in the current source. The changelog records
several as experimental; validate them in the branch pilot before relying on
them operationally.

- **Order age and overdue alerts:** card headers show elapsed time and use
  green below 10 minutes, amber from 10 minutes, and red from 20 minutes.
  Prepared and For Invoicing cards retain their stage styling. The top overdue
  panel filters orders older than 20 minutes when clicked.
- **Queue search and quick filters:** search customer, Work Order number,
  group, SKU, or description alongside the stage filters.
- **Shortage flags:** the prepared-quantity dialog records found stock and
  marks the item `Shortage`. Warehouse cards highlight shortages; recalling
  the order in Work Order Entry shows a shortage notice.
  After customer agreement, the cashier can reduce that line to the recorded
  found quantity through Order actions and Save. This releases only the shortage
  reservation and preserves the picker; stock on hand is unchanged. See
  `ORDER_TOOLS_GUIDE.md` for requirements and undo behavior.
  The card's Refresh button reads fresh database records and resets its stage,
  shortage badge, and editability. Shortage/status saves also notify the normal
  automatic refresh through the order's update timestamp.
- **Picker workload and productivity:** the Pickers dialog shows workload,
  filters cards by picker, supports bulk reassignment, and offers a daily
  leaderboard. The main screen also shows Completed Today.
- **Batch Pick:** consolidates pending/in-process items by SKU and bin
  location, with filters and a printable batch pick sheet.
- **TV Display:** shows Being Prepared and Ready for Pickup / Cashier columns,
  refreshes from queue cards every five seconds, and supports F11 fullscreen.

---

## 7. Remaining Enhancement Ideas

The following ideas remain proposals rather than completed features:

### A. Warehouse Floor & Picking Speed

1. **Scan-to-Verify (Barcode Scanning)**:
   * Allow pickers to scan the physical item barcode before marking it "Prepared" to eliminate wrong-item and wrong-pack-size picking errors.
2. **Optimized Bin / Shelf Route**:
   * Extend the existing batch pick view with an optimized aisle/bin route that minimizes backtracking.

### B. Cashier & Warehouse Coordination

3. **Optional Sound Cue**:
   * The visual ready notification is now implemented in Work Order Entry; see `ORDER_TOOLS_GUIDE.md`. An optional sound cue remains a proposal.
4. **Packing Slip / Box Label Printing**:
   * Quick-print option for sticky box tags (Order #, Customer Name, Box X of Y) for multi-carton wholesale orders.

### C. Accountability & Management Insights

5. **Stage Timestamps & SLA Tracking**:
   * The optional order history migration now records stage events and timestamps. Extend those records with dedicated SLA and bottleneck reports; see `ORDER_TOOLS_GUIDE.md`.
6. **Exportable End-of-Day Picker Report**:
   * Extend the existing daily leaderboard with an exportable end-of-day summary for workload and performance review.
