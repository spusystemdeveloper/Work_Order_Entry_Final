# Order Processing Queueing Guide

This guide explains how **Order Processing Queueing** works, how it integrates with **Work Order Entry** and **Microsoft Dynamics RMS**, and how to configure and troubleshoot it.

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

## 2. The 3-Step Order Lifecycle

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
| **1. Order Creation** | `Work Order Entry` | Sales Rep / Cashier | `"In Queue"` | Saves Work Order (Type 2). Commits stock. Creates records in `Queueing` and `QueueingItems`. If grouping is enabled, assigns a `GroupNumber`. |
| **2. Warehouse Picking** | `OrderProcessingQueueing` | Warehouse Picker | `"In Queue"` → `"Prepared"` | Order appears as a visual tile. Supervisor assigns a picker from `PickerList`. Staff gathers items and logs `QtyPrep`. When all items are complete, status changes to `"Prepared"`. |
| **3. For Invoicing** | `Work Order Entry` | Cashier | `"Prepared"` → `"For Invoicing"` | Cashier presses **[F7] For Invoicing**. Selects the prepared order batch. Prints the final Work Order. Status updates to `"For Invoicing"`. |
| **4. POS Settlement** | RMS Store Operations POS | Cashier | `"Closed"` | Cashier tenders payment. SQL trigger `trg_Transaction_closeQueue` automatically detects the completed sale and closes the queue record. |

---

## 3. Key Configuration Settings

### A. In Work Order Entry (`App.config` & Settings Form)

Configured under **Settings → Work Order and Queue Options**:

* **`QueueingEnabled`:**
  * `True`: Full warehouse queueing enabled (Gensan branch style). Writes to `QueueingItems`, shows `[F7] For Invoicing`, and enables order grouping.
  * `False`: Direct processing mode (Davao branch style). Work Orders commit stock directly without writing queue item lines. `[F7] For Invoicing` is hidden.
* **`AllowOrderGrouping`:**
  * Prompts the cashier to group multiple orders for the same customer under a single batch/group number (`GroupNumber`), allowing warehouse staff to pick all orders for that customer together.
* **`ShowForInvoiceButton`:**
  * Controls the visibility of the **For Invoicing [F7]** button on the main screen.

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
| `Queueing` | Table | Header record linking `OrderID`, `GroupNumber`, and `OPIS` (Order Taker username). |
| `QueueingItems` | Table | Individual line items with `PickLoc`, `Quantity`, `QuantityPrep`, `Picker`, and `Status`. |
| `PickerList` | Table | Master list of authorized warehouse picking staff. |
| `SOD_Item_Logs` | Table | Audit log recording inventory adjustments and queue changes. |
| `SOD_ViewForInvoice` | View | Feeds `frmForInvoice` with orders where `Status = 'Prepared'`. |
| `SOD_viewTableLastUpdate` | View | Used by `OrderProcessingQueueing` to detect when new orders are created. |
| `trg_Transaction_closeQueue` | Trigger | Trigger on RMS `Transaction` table that closes the queue record when payment is tendered. |

---

## 5. Troubleshooting Checklist

### Issue 1: Order does not appear on the Warehouse screen
1. **Check Release Type:** Was the order created as `Pick-up` or `Delivery`? Ensure the warehouse terminal is set to the matching mode.
2. **Check Pick Location:** In `Work Order Entry`, is the `Pick Loc` checkbox checked (`UP-STORE`) or unchecked (`STORE`)?
3. **Check Database:** Confirm that both `Work Order Entry` and `OrderProcessingQueueing` are connected to the exact same SQL database.

### Issue 2: Order does not appear in [F7] For Invoicing
1. **Status Check:** Orders only appear in `[F7]` once the warehouse has marked all items as **"Prepared"**. If an order is still "In Queue", it will not show up.
2. **Order Taker / OPIS Filter:** By default, `frmForInvoice` filters orders created by the currently logged-in user (`frmItemLookUp.usrUsername`).

---

## 6. Future Enhancements & Feature Roadmap (Queueing)

The following high-value suggestions are documented for future iterations:

### A. Warehouse Floor & Picking Speed
1. **Scan-to-Verify (Barcode Scanning)**:
   * Allow pickers to scan the physical item barcode before marking it "Prepared" to eliminate wrong-item and wrong-pack-size picking errors.
2. **Bin / Shelf Location Ordering**:
   * Sort items on the pick list and screen by warehouse aisle/bin location so pickers follow a single optimized path through the warehouse without backtracking.
3. **Out-of-Stock / Short-Pick Flagging**:
   * Allow pickers to flag damaged or missing stock with the actual quantity found, alerting the cashier immediately rather than stalling the entire order batch.

### B. Visibility & Customer Experience
4. **Order Age & Priority Color-Coding**:
   * Add visual age indicators on the queue grid (e.g., Green = `< 10 mins`, Yellow = `10–20 mins`, Red = `> 20 mins`) to prioritize older orders and reduce customer wait times.
5. **Customer-Facing TV Display (Queue Board)**:
   * A lightweight status display facing the customer counter with two columns: **"Preparing"** and **"Ready for Payment / Pickup"**.

### C. Cashier & Warehouse Coordination
6. **Cashier "Ready" Notification / Sound Cue**:
   * A subtle visual alert or chime on the Work Order Entry screen when an order batch hits 100% "Prepared", signaling the cashier to pull the order into `[F7] For Invoicing`.
7. **Packing Slip / Box Label Printing**:
   * Quick-print option for sticky box tags (Order #, Customer Name, Box X of Y) for multi-carton wholesale orders.

### D. Accountability & Management Insights
8. **Stage Timestamps & SLA Tracking**:
   * Log timestamps across transitions (*Created → Pick Assigned → Prepared → Invoiced*) to track prep speed and pinpoint fulfillment bottlenecks.
9. **Daily Picker Productivity Summary**:
   * An end-of-day report summarizing total lines and orders completed per picker for workload balance and performance tracking.

