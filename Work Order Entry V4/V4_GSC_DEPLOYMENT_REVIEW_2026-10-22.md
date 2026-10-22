# V4 GSC deployment review — October 22, 2026

The updated candidate retains the imported V4's old GSC function mappings and
save implementation. No unexpected business-logic change was found in this
review. This is a source/build/local-fixture assessment, not certification of
the current live GSC database or every cashier PC.

## Comparison

Later printed-reference update: frmPrintWo now supplies one optional reference
parameter and rprtOrder.rdlc adds its textbox/parameter. The new read-only helper
uses the saved order's customer/type and existing printed subtotal. Both V4 and
V8 passed 44 local print-reference checks each and 72 PDF comparisons. Thus the
original report template is intentionally extended; the older unchanged-report
statement below describes the earlier pre-reference build. Every pre-existing
report element/expression is still structurally equal to that original. The
printing invocation, paper settings and business calculations remain unchanged;
no live database or physical printer was used. Current customer rates apply at
print time; no historical withholding snapshot is introduced.

Follow-on empty-result update: added paint-only empty-grid messages and guarded
empty Sales Rep selection. The original V4 reproduces ArgumentOutOfRangeException;
33 new isolated UI checks passed, along with the existing build/guard/customer/
withholding/unit regressions. Only frmSalesRep.vb and the project compile list
change from the preceding candidate, plus two new display files. The source
search queries, shortcuts and item no-match warning/filter-restoration path
remain unchanged. This adds frmSalesRep.vb to the four originally changed files
listed below; those earlier comparison details describe the pre-empty-grid build.

Only four original project files differ from the retained pre-update hashes:
the project compile list, frmItemLookUp, frmCustomer and clsItemLookUp.
Added files implement the previously approved validation gates and optional
withholding display. Original SQL mappings/generated mappings, save/recall
classes, Designer, report layouts, assembly resolver, resources and configs
retain their hashes.

The main form's changed source methods are SelectPrice, SaveOrder, InsertEntry,
UpdateOrder, ValidateAllQty and UpdateAmt, plus the new ValidateSaveInputs.
The changes add selection/numeric gates, stop a caught quantity-check failure
from continuing, and refresh the read-only withholding display. Existing
pricing, totals calculation, stock writes and order lifecycle paths remain.

Compiled comparison covered 340 original methods across frmItemLookUp,
frmCustomer and clsItemLookUp; none was removed. Changes correspond to the
approved gates, customer selection handlers, withholding event subscriptions
and LoadLevel cleanup. RecallQuote's compiled closure names were renumbered
by the compiler; its source and resolved call sequence are unchanged apart
from those generated names.

LoadLevel still uses SOD_fntbl_PriceLevel(sCode) and the existing iPricePass
approval threshold. Its unused company-name lookup and blank MessageBox were
removed. Customer selection now uses the selected row's ID, including duplicate
company names. Withholding amounts never feed a save, total or RMS tender.

## GSC requirements and limits

- Search still expects the older SOD_fntbl_NestedSearchItem(bEmployee, iLevel,
  sTxt) signature. The previously inspected localhost GSC restore has a newer
  one-argument version and is not a matching full-workflow test target. The
  live GSC definitions were not connected to or changed in this review.
- No database migration is required for these application changes. Do not
  replace old GSC functions with V8/other-branch definitions as part of this
  deployment. Missing/unrelated withholding captions hide the optional display.
- Retain the GSC installation's configuration and dependency setup. V4 still
  targets .NET Framework 4.7.2/x86 and installed ReportViewer 2010. Its existing
  resource-based dependency resolver is unchanged. Full EXE-only operation on
  a clean GSC client, including import and physical printing, was not exercised.
- V4 retains its separate-write transaction, stale-edit and cancellation
  limitations. These basic guards do not give it all V8 transaction protections;
  failed database writes can still leave identity gaps.

## Evidence and release recommendation

Latest V4 and both main Release/x86 builds passed. The latest existing logs
record 106 main unit, 54 V4 guard, 28 V4 customer-selection and 57 V4 withholding
checks passing. The earlier Debug checkbox regression passed 28 checks.
Those suites were not rerun just for this read-only comparison. The delivered
EXE's SHA256 still matches the tested blank-popup-fix build. Store configuration
hashes were preserved; this review made no application-code or database changes.
The existing V4 LocalReportExtensions ByRef warning remains.

Use one GSC test PC with the matching old functions and its existing config
before a wider rollout. Verify search, duplicate-name selection, new WO and
quotation, recall/edit, price approval/Cancel, save, and print; check withholding
if configured. These manual checks are still required to confirm that store's
actual workflow. Keep the prior EXE available for rollback. RMS investigation
remains paused.
