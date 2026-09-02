/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_ViewItems]
  Source type: V 
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_ViewItems]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52001, 'Object type conflict for [dbo].[SOD_ViewItems].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE VIEW [dbo].[SOD_ViewItems]
as
SELECT 
    ItemLookupCode,
    CONCAT(DESCRIPTION,EXTENDEDDESCRIPTION) AS ''DESCRIPTION'',
    PRICE,
    COST,
    CASE WHEN QuantityCommitted < 0 THEN Quantity
      -- WHEN QuantityCommitted > Quantity THEN 0
       WHEN Quantity < 0 THEN 0
       ELSE Quantity - QuantityCommitted
    END AS ''AVAILABLE'',

    ParentQuantity ,
    TaxID ,
    SubDescription2 ,
    PRICE as ''PRICE 1'',
    ID,
    Description as ''DESCRIPTION1'',
    ExtendedDescription as ExtendedDescription ,
    ItemType ,
    Inactive ,
--    CONCAT(DESCRIPTION,EXTENDEDDESCRIPTION) AS ''ITEMDES'',
    ''-'' AS ''ITEMDES'',
    SubDescription2 AS ''SKULEVEL'',
    CONCAT(ItemLookupCode,'' | '', DESCRIPTION,EXTENDEDDESCRIPTION) AS ''FULLDESC''
FROM ITEM';
ELSE
    EXEC sys.sp_executesql N'ALTER VIEW [dbo].[SOD_ViewItems]
as
SELECT 
    ItemLookupCode,
    CONCAT(DESCRIPTION,EXTENDEDDESCRIPTION) AS ''DESCRIPTION'',
    PRICE,
    COST,
    CASE WHEN QuantityCommitted < 0 THEN Quantity
      -- WHEN QuantityCommitted > Quantity THEN 0
       WHEN Quantity < 0 THEN 0
       ELSE Quantity - QuantityCommitted
    END AS ''AVAILABLE'',

    ParentQuantity ,
    TaxID ,
    SubDescription2 ,
    PRICE as ''PRICE 1'',
    ID,
    Description as ''DESCRIPTION1'',
    ExtendedDescription as ExtendedDescription ,
    ItemType ,
    Inactive ,
--    CONCAT(DESCRIPTION,EXTENDEDDESCRIPTION) AS ''ITEMDES'',
    ''-'' AS ''ITEMDES'',
    SubDescription2 AS ''SKULEVEL'',
    CONCAT(ItemLookupCode,'' | '', DESCRIPTION,EXTENDEDDESCRIPTION) AS ''FULLDESC''
FROM ITEM';
GO
