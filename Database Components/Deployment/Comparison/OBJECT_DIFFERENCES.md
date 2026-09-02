# Application database object comparison

Generated from read-only DVO_STORE_DB and GSC_STORE_DB metadata on 2026-07-27 17:25:56 +08:00.

| Object | Category | Requirement | Davao | Gensan | Result |
|---|---|---|---|---|---|
| `Queueing` | Tables | CoreRecall | U  | U  | Identical |
| `QueueingItems` | Tables | QueueMode | U  | U  | Identical |
| `PickerList` | Tables | QueueMode | U  | U  | Identical |
| `SOD_Item_Logs` | Tables | QueueMode | U  | U  | Identical |
| `SOD_ImportedQuote` | Tables | QuotationImport | Missing | Missing | NotExported |
| `SOD_Reg` | Tables | CoreRegisterDependency | U  | U  | Identical |
| `SOD_WO` | Tables | Core | U  | U  | Identical |
| `SOD_WO_Conf` | Tables | Core | U  | U  | Different |
| `SOD_WO_CustType` | Tables | CustomerType | U  | U  | Identical |
| `SOD_WO_CustTypeUser` | Tables | CustomerType | U  | U  | Identical |
| `SOD_WO_DraftHeader` | Tables | OptionalDraft | U  | Missing | MissingInGSC |
| `SOD_WO_DraftDetail` | Tables | OptionalDraft | U  | Missing | MissingInGSC |
| `SOD_WO_LOGS` | Tables | PriceAudit | U  | Missing | MissingInGSC |
| `SOD_WO_User` | Tables | CoreLogin | U  | U  | Identical |
| `Queueing_Quote` | Tables | MappedInactive | Missing | Missing | NotExported |
| `WorkOrder_Logs` | Tables | MappedInactive | Missing | Missing | NotExported |
| `SOD_ViewForInvoice` | Views | QueueMode | V  | V  | Identical |
| `SOD_ViewItems` | Views | MappedLegacy | V  | V  | Different |
| `SOD_ViewItemsWO` | Views | CoreSearch | V  | Missing | MissingInGSC |
| `SOD_viewLastUpdatedItem` | Views | CoreRefresh | V  | V  | Different |
| `SOD_viewLastUpdatedOrder` | Views | QueueMode | V  | V  | Identical |
| `SOD_viewTableLastUpdate` | Views | CoreRefresh | V  | V  | Identical |
| `SOD_WEBSITE_VIEW_ORDER_ITEMS` | Views | WebsiteImport | Missing | Missing | NotExported |
| `SOD_WEBSITE_VIEW_ORDERS` | Views | WebsiteImport | Missing | Missing | NotExported |
| `SOD_fn_GetDiscQty` | Functions | CoreFunctionDependency | FN | FN | Identical |
| `SOD_fn_GetAvailableQty` | Functions | Core | FN | Missing | MissingInGSC |
| `SOD_fn_GetConvertQty` | Functions | Core | FN | Missing | MissingInGSC |
| `SOD_fn_GetCreditLimit` | Functions | Core | FN | Missing | MissingInGSC |
| `SOD_fn_GetQty` | Functions | Core | FN | FN | Different |
| `SOD_fn_GetReg` | Functions | Core | FN | FN | Identical |
| `SOD_fntbl_sumStockin` | Functions | CoreFunctionDependency | IF | IF | Identical |
| `SOD_fn_GetStockInQty` | Functions | GSCBranchDependency | FN | FN | Identical |
| `SOD_fn_GetTotalOpenWorkOrder` | Functions | Core | FN | Missing | MissingInGSC |
| `SOD_fntbl_CheckExQty` | Functions | CrossStoreLookup | IF | IF | Different |
| `SOD_fntbl_CheckOpenWO` | Functions | Core | IF | Missing | MissingInGSC |
| `SOD_fntbl_CheckQty` | Functions | CrossStoreLookup | IF | IF | Different |
| `SOD_fntbl_LastupdateOrder` | Functions | QueueMode | IF | IF | Identical |
| `SOD_fntbl_NestedSearchItem` | Functions | CoreSearch | IF | IF | Identical |
| `SOD_fntbl_PriceLevel` | Functions | CorePricing | IF | IF | Identical |
| `SOD_fntbl_SearchItem` | Functions | CoreSearch | IF | IF | Identical |
| `SOD_fntbl_WoDetails` | Functions | CoreReporting | TF | TF | Different |
| `SOD_fntbl_WoEntry` | Functions | CoreReporting | TF | TF | Identical |
| `SOD_sp_InsertQoute` | Procedures | CoreSave | P  | P  | Different |
| `SOD_sp_InsertQouteEntry` | Procedures | CoreSave | P  | P  | Different |
| `trg_InventoryTransferLog_ItemLogs` | Triggers | QueueAudit | TR | TR | Identical |
| `trg_ItemLogs` | Triggers | QueueAudit | TR | TR | Identical |
| `trg_OrderEntry_ItemLogs` | Triggers | QueueAudit | TR | TR | Identical |
| `trg_Transaction_closeQueue` | Triggers | QueueMode | TR | TR | Identical |
| `trg_TransactionEntry_ItemLogs` | Triggers | QueueAudit | TR | TR | Identical |

Definitions under BranchOverrides differ and must not be interchanged without review. A SourceOnly definition is included only in its original branch driver and is never copied automatically to the other branch.
