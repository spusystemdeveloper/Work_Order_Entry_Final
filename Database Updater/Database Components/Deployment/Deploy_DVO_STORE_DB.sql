:on error exit
/* Run with SQLCMD after editing the target database below. */
:setvar TargetDatabase "CHANGE_ME"
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
:r .\Objects\Common\Functions\SOD_fntbl_NestedSearchItem.sql
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
:r .\Objects\BranchOverrides\DVO_STORE_DB\Tables\SOD_WO_Conf.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Tables\SOD_WO_DraftHeader.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Tables\SOD_WO_DraftDetail.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Tables\SOD_WO_LOGS.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Functions\SOD_fn_GetAvailableQty.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Functions\SOD_fn_GetConvertQty.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Functions\SOD_fn_GetCreditLimit.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Functions\SOD_fn_GetQty.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Functions\SOD_fn_GetTotalOpenWorkOrder.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Functions\SOD_fntbl_CheckExQty.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Functions\SOD_fntbl_CheckOpenWO.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Functions\SOD_fntbl_CheckQty.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Functions\SOD_fntbl_WoDetails.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Views\SOD_ViewItems.sql
:r .\Objects\SourceOnly\DVO_STORE_DB\Views\SOD_ViewItemsWO.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Views\SOD_viewLastUpdatedItem.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Procedures\SOD_sp_InsertQoute.sql
:r .\Objects\BranchOverrides\DVO_STORE_DB\Procedures\SOD_sp_InsertQouteEntry.sql
:r .\Security\DVO_STORE_DB_Permissions.sql
:r .\Validation\99_PostDeploymentValidation.sql
PRINT N'DVO_STORE_DB reviewed object package completed.';
GO
