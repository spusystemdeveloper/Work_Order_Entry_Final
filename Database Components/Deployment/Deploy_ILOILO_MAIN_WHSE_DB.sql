:on error exit
/*
  Deployment driver for ILOILO_MAIN_WHSE_DB.
  Verified against live ILOILO_MAIN_WHSE_DB schema (StoreID 1600).
  Preconditions:
    - SQL Server 2016+
    - Active copy-only backup with checksum verified
    - ReasonCode 701, Type 6 present (ID 32)
*/
:setvar TargetDatabase "ILOILO_MAIN_WHSE_DB"
USE [$(TargetDatabase)];
GO
:r .\Validation\00_Preflight.sql
:r .\Objects\Common\Tables\Queueing.sql
:r .\Objects\Common\Tables\QueueingItems.sql
:r .\Objects\Common\Tables\PickerList.sql
:r .\Objects\Common\Tables\SOD_Item_Logs.sql
:r .\Objects\Common\Tables\SOD_Reg.sql
:r .\Objects\Common\Tables\SOD_WO.sql
:r .\Objects\Common\Tables\SOD_WO_CustType.sql
:r .\Objects\Common\Tables\SOD_WO_CustTypeUser.sql
:r .\Objects\Common\Tables\SOD_WO_User.sql
:r .\Objects\Common\Functions\SOD_fn_GetDiscQty.sql
:r .\Objects\Common\Functions\SOD_fn_GetReg.sql
:r .\Objects\Common\Functions\SOD_fntbl_sumStockin.sql
:r .\Objects\Common\Functions\SOD_fn_GetStockInQty.sql
:r .\Objects\Common\Functions\SOD_fntbl_LastupdateOrder.sql
:r .\Objects\Common\Functions\SOD_fntbl_PriceLevel.sql
:r .\Objects\Common\Functions\SOD_fntbl_SearchItem.sql
:r .\Objects\Common\Functions\SOD_fntbl_WoEntry.sql
:r .\Objects\Common\Views\SOD_ViewForInvoice.sql
:r .\Objects\Common\Views\SOD_viewLastUpdatedOrder.sql
:r .\Objects\Common\Views\SOD_viewTableLastUpdate.sql
:r .\Objects\Common\Triggers\trg_InventoryTransferLog_ItemLogs.sql
:r .\Objects\Common\Triggers\trg_ItemLogs.sql
:r .\Objects\Common\Triggers\trg_OrderEntry_ItemLogs.sql
:r .\Objects\Common\Triggers\trg_Transaction_closeQueue.sql
:r .\Objects\Common\Triggers\trg_TransactionEntry_ItemLogs.sql
:r .\Objects\PortableBaseline\Tables\SOD_WO_LOGS.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetAvailableQty.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetConvertQty.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetCreditLimit.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetTotalOpenWorkOrder.sql
:r .\Objects\PortableBaseline\Functions\SOD_fntbl_CheckOpenWO.sql
:r .\Objects\BranchOverrides\GSC_STORE_DB\Functions\SOD_fntbl_WoDetails.sql
:r .\Objects\PortableBaseline\Views\SOD_ViewItemsWO.sql
:r .\Objects\BranchOverrides\GSC_STORE_DB\Views\SOD_viewLastUpdatedItem.sql
:r .\Objects\BranchOverrides\GSC_STORE_DB\Procedures\SOD_sp_InsertQoute.sql
:r .\Objects\BranchOverrides\GSC_STORE_DB\Procedures\SOD_sp_InsertQouteEntry.sql
:r .\Validation\99_PostDeploymentValidation.sql
PRINT N'ILOILO_MAIN_WHSE_DB deployment package completed successfully.';
GO
