# Item Value List export fix

1. Back up the target RMS database.
2. Run `Deploy_SOD_ViewItemValueListExport.sql` in that database.
3. Copy `Custom - Item Value List - Fixed Excel Export View.qrp` to the RMS
   `Reports` folder using an Administrator account.
4. Restart RMS, run **Item Value List - Fixed Excel Export (SQL View)**, and
   export to CSV or Excel.

The original `Items - Item Value List.qrp` is not changed. The SQL view is
read-only; it does not update any RMS item, Department, Category, or Supplier
record.
