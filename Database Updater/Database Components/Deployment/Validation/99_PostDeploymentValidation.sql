/*
  Read-only post-deployment validation.
  Missing optional import and draft features are reported by Manifest.csv.
*/
SET NOCOUNT ON;
DECLARE @Expected TABLE
(
    ObjectName sysname NOT NULL,
    AllowedTypes nvarchar(20) NOT NULL,
    Requirement nvarchar(50) NOT NULL
);
INSERT @Expected (ObjectName, AllowedTypes, Requirement)
VALUES
    (N'Queueing', N'U', N'CoreRecall'),
    (N'QueueingItems', N'U', N'QueueMode'),
    (N'PickerList', N'U', N'QueueMode'),
    (N'SOD_Item_Logs', N'U', N'QueueMode'),
    (N'SOD_Reg', N'U', N'CoreRegisterDependency'),
    (N'SOD_WO', N'U', N'Core'),
    (N'SOD_WO_Conf', N'U', N'Core'),
    (N'SOD_WO_CustType', N'U', N'CustomerType'),
    (N'SOD_WO_CustTypeUser', N'U', N'CustomerType'),
    (N'SOD_WO_LOGS', N'U', N'PriceAudit'),
    (N'SOD_WO_User', N'U', N'CoreLogin'),
    (N'SOD_ViewForInvoice', N'V', N'QueueMode'),
    (N'SOD_ViewItems', N'V', N'MappedLegacy'),
    (N'SOD_ViewItemsWO', N'V', N'CoreSearch'),
    (N'SOD_viewLastUpdatedItem', N'V', N'CoreRefresh'),
    (N'SOD_viewLastUpdatedOrder', N'V', N'QueueMode'),
    (N'SOD_viewTableLastUpdate', N'V', N'CoreRefresh'),
    (N'SOD_fn_GetDiscQty', N'FN,IF,TF', N'CoreFunctionDependency'),
    (N'SOD_fn_GetAvailableQty', N'FN,IF,TF', N'Core'),
    (N'SOD_fn_GetConvertQty', N'FN,IF,TF', N'Core'),
    (N'SOD_fn_GetCreditLimit', N'FN,IF,TF', N'Core'),
    (N'SOD_fn_GetQty', N'FN,IF,TF', N'Core'),
    (N'SOD_fn_GetReg', N'FN,IF,TF', N'Core'),
    (N'SOD_fntbl_sumStockin', N'FN,IF,TF', N'CoreFunctionDependency'),
    (N'SOD_fn_GetTotalOpenWorkOrder', N'FN,IF,TF', N'Core'),
    (N'SOD_fntbl_CheckExQty', N'FN,IF,TF', N'CrossStoreLookup'),
    (N'SOD_fntbl_CheckOpenWO', N'FN,IF,TF', N'Core'),
    (N'SOD_fntbl_CheckQty', N'FN,IF,TF', N'CrossStoreLookup'),
    (N'SOD_fntbl_LastupdateOrder', N'FN,IF,TF', N'QueueMode'),
    (N'SOD_fntbl_NestedSearchItem', N'FN,IF,TF', N'CoreSearch'),
    (N'SOD_fntbl_PriceLevel', N'FN,IF,TF', N'CorePricing'),
    (N'SOD_fntbl_SearchItem', N'FN,IF,TF', N'CoreSearch'),
    (N'SOD_fntbl_WoDetails', N'FN,IF,TF', N'CoreReporting'),
    (N'SOD_fntbl_WoEntry', N'FN,IF,TF', N'CoreReporting'),
    (N'SOD_sp_InsertQoute', N'P,PC', N'CoreSave'),
    (N'SOD_sp_InsertQouteEntry', N'P,PC', N'CoreSave'),
    (N'trg_InventoryTransferLog_ItemLogs', N'TR', N'QueueAudit'),
    (N'trg_ItemLogs', N'TR', N'QueueAudit'),
    (N'trg_OrderEntry_ItemLogs', N'TR', N'QueueAudit'),
    (N'trg_Transaction_closeQueue', N'TR', N'QueueMode'),
    (N'trg_TransactionEntry_ItemLogs', N'TR', N'QueueAudit');

SELECT
    expected.ObjectName,
    expected.Requirement,
    expected.AllowedTypes,
    actual.type AS ActualType,
    actual.type_desc AS ActualTypeDescription,
    CASE
        WHEN actual.object_id IS NULL THEN N'MISSING'
        WHEN N',' + expected.AllowedTypes + N',' NOT LIKE
             N'%,' + RTRIM(actual.type COLLATE DATABASE_DEFAULT) + N',%' THEN N'WRONG TYPE'
        ELSE N'PASS'
    END AS ValidationResult
FROM @Expected AS expected
LEFT JOIN sys.objects AS actual
  ON actual.name = expected.ObjectName
 AND SCHEMA_NAME(actual.schema_id) = N'dbo'
ORDER BY ValidationResult, expected.ObjectName;

IF EXISTS
(
    SELECT 1
    FROM @Expected AS expected
    LEFT JOIN sys.objects AS actual
      ON actual.name = expected.ObjectName
     AND SCHEMA_NAME(actual.schema_id) = N'dbo'
    WHERE actual.object_id IS NULL
       OR N',' + expected.AllowedTypes + N',' NOT LIKE
          N'%,' + RTRIM(actual.type COLLATE DATABASE_DEFAULT) + N',%'
)
    THROW 53100, 'Application database-object validation failed.', 1;
GO
