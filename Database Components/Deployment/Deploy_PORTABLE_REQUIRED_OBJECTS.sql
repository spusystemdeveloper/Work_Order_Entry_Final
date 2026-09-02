:on error exit
/*
  Installs only the seven portable Work Order Entry prerequisites.
  Use for a new branch only after its RMS schema and business rules are
  reviewed. This is not a complete application deployment driver.
*/
:setvar TargetDatabase "CHANGE_ME"
USE [$(TargetDatabase)];
GO
:r .\Validation\00_Preflight.sql
:r .\Objects\PortableBaseline\Tables\SOD_WO_LOGS.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetAvailableQty.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetConvertQty.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetCreditLimit.sql
:r .\Objects\PortableBaseline\Functions\SOD_fn_GetTotalOpenWorkOrder.sql
:r .\Objects\PortableBaseline\Functions\SOD_fntbl_CheckOpenWO.sql
:r .\Objects\PortableBaseline\Views\SOD_ViewItemsWO.sql
PRINT N'Portable required-object baseline completed.';
GO
