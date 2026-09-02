# Target deployment coverage

This report distinguishes the live audit from the generated deployment
package. `Manifest.csv` correctly reports that the seven objects below are
currently absent from the live GSC database. The GSC driver supplies reviewed
portable definitions; no live database has been changed.

| Required object | DVO deployment | GSC deployment | Portable behavior |
|---|---|---|---|
| `SOD_WO_LOGS` | Existing DVO definition | Portable baseline | Local price-audit table |
| `SOD_ViewItemsWO` | Existing DVO definition | Portable baseline | Reads local `dbo.Item` |
| `SOD_fn_GetAvailableQty` | Existing DVO definition | Portable baseline | Reads local `dbo.Item` |
| `SOD_fn_GetConvertQty` | Existing DVO definition | Portable baseline | Reads local `dbo.Item`; safely terminates if a SKU is not found |
| `SOD_fn_GetCreditLimit` | Existing DVO cross-warehouse definition | Portable baseline | Sums local `dbo.AccountReceivable` only |
| `SOD_fn_GetTotalOpenWorkOrder` | Existing DVO cross-warehouse definition | Portable baseline | Sums local open type-2 `dbo.Order` rows only |
| `SOD_fntbl_CheckOpenWO` | Existing DVO definition | Portable baseline | Reads local RMS and queue tables |

For Cebu or another new branch, these seven files can be used as its required
local baseline. A complete branch deployment still requires auditing the
branch-specific functions, procedures, views, queue behavior, StoreID rules,
and external warehouse database references.
