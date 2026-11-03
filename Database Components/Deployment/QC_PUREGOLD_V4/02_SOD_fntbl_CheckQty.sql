-- QC PUREGOLD_DB, selected V4 Release EXE. Import through Database Updater.
-- The caller owns the transaction. Backup/restore testing and explicit approval
-- are required before live deployment. This file does not execute itself.
IF DB_NAME()<>N'PUREGOLD_DB' THROW 52600,'Expected PUREGOLD_DB. Use an owned same-name restore for testing.',1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601,'Run in the Database Updater active transaction.',1;
IF (SELECT COUNT(*) FROM dbo.Configuration)<>1 OR NOT EXISTS(SELECT 1 FROM dbo.Configuration WHERE StoreID=100) THROW 52603,'QC Puregold store configuration changed.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.Store WHERE ID=3001 AND StoreCode='QC_STORE_DB') OR NOT EXISTS(SELECT 1 FROM dbo.Store WHERE ID=3012 AND StoreCode='QC_RDC_DB') THROW 52604,'QC warehouse directory changed. Review targets.',1;
DECLARE @Definition nvarchar(max)=N'ALTER FUNCTION [dbo].[SOD_fntbl_CheckQty](@vCode varchar(15))
RETURNS TABLE
AS RETURN
(
 SELECT ID,[Name],StoreCode,
 CASE StoreCode
  WHEN ''QC_STORE_DB'' THEN (SELECT Quantity-QuantityCommitted FROM QC_STORE_DB.dbo.Item WHERE ItemLookupCode=@vCode)
  WHEN ''QC_RDC_DB'' THEN (SELECT Quantity-QuantityCommitted FROM QC_RDC_DB.dbo.Item WHERE ItemLookupCode=@vCode)
 END AS Qty,(Address1+'', ''+Country) AS Address
 FROM dbo.Store
 WHERE ID<>(SELECT StoreID FROM dbo.Configuration)
   AND StoreCode IN (''QC_STORE_DB'',''QC_RDC_DB'')
)';
DECLARE @Current varbinary(32)=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(OBJECT_DEFINITION(OBJECT_ID(N'dbo.SOD_fntbl_CheckQty')),CHAR(13),N'')));
IF @Current=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(REPLACE(@Definition,N'ALTER FUNCTION',N'CREATE FUNCTION'),CHAR(13),N''))) RETURN;
IF @Current IS NULL OR @Current<>0xA23E19515F5278DB69FF0B1E88126E21EFC81CB5141BF47C80EB525A255BF4C9
 THROW 52602,'Existing definition changed or is unavailable. Review instead of overwriting.',1;
EXEC sys.sp_executesql @Definition;
