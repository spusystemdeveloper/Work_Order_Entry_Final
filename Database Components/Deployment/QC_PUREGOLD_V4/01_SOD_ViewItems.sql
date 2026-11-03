-- QC PUREGOLD_DB, selected V4 Release EXE. Import through Database Updater.
-- The caller owns the transaction. Backup/restore testing and explicit approval
-- are required before live deployment. This file does not execute itself.
IF DB_NAME()<>N'PUREGOLD_DB' THROW 52600,'Expected PUREGOLD_DB. Use an owned same-name restore for testing.',1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601,'Run in the Database Updater active transaction.',1;
DECLARE @Definition nvarchar(max)=N'

ALTER VIEW [dbo].[SOD_ViewItems] AS
SELECT 
    ItemLookupCode,
    CONCAT(Description, CONVERT(nvarchar(max), ExtendedDescription)) AS DESCRIPTION, 
    Price, 
    Cost, 
    CASE 
        WHEN QuantityCommitted < 0 THEN Quantity 
        WHEN Quantity < 0 THEN 0 
        ELSE Quantity - QuantityCommitted 
    END AS AVAILABLE, 
    ParentQuantity, 
    TaxID, 
    SubDescription2, 
    Price AS [PRICE 1], 
    ID, 
    Description AS DESCRIPTION1, 
    ExtendedDescription, 
    ItemType, 
    Inactive, 
    ''-'' AS ITEMDES, 
    SubDescription2 AS SKULEVEL,
    CONCAT(ItemLookupCode, '' | '', Description, CONVERT(nvarchar(max), ExtendedDescription), '' | '', SubDescription1) AS FULLDESC,
    SubDescription1 AS [PG SKU]
FROM dbo.Item;
';
DECLARE @Current varbinary(32)=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(OBJECT_DEFINITION(OBJECT_ID(N'dbo.SOD_ViewItems')),CHAR(13),N'')));
IF @Current=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(REPLACE(@Definition,N'ALTER VIEW',N'CREATE VIEW'),CHAR(13),N''))) RETURN;
IF @Current IS NULL OR @Current<>0xC61A0DAF41B528E5A062C239DB8830BA2B5DB82AD1782A1C25D524D0C4DFB71B
 THROW 52602,'Existing definition changed or is unavailable. Review instead of overwriting.',1;
EXEC sys.sp_executesql @Definition;
