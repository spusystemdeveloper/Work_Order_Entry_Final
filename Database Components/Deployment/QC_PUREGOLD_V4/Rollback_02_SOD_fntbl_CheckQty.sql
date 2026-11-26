-- QC PUREGOLD_DB, selected V4 Release EXE. Import through Database Updater.
-- The caller owns the transaction. Backup/restore testing and explicit approval
-- are required before live deployment. This file does not execute itself.
IF DB_NAME()<>N'PUREGOLD_DB' THROW 52600,'Expected PUREGOLD_DB. Use an owned same-name restore for testing.',1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601,'Run in the Database Updater active transaction.',1;
DECLARE @Definition nvarchar(max)=N'


-- =============================================
ALTER FUNCTION [dbo].[SOD_fntbl_CheckQty]
(	
	@vCode VARCHAR(15)
)
RETURNS TABLE 
AS
RETURN 
(
	
	SELECT ID,[Name],StoreCode,
	CASE
	WHEN StoreCode = ''GSC_STORE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_STORE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_MAIN_WHSE_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_MAIN_WHSE_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	WHEN StoreCode = ''GSC_OFFICE_SUPPLY_DB'' THEN (SELECT Quantity - QuantityCommitted FROM GSC_OFFICE_SUPPLY_DB.DBO.ITEM WHERE ItemLookupCode = @vCode )
	ELSE 0
	END AS Qty,(Address1 + '', '' + Country) as Address
	FROM STORE WHERE ID != (SELECT StoreID FROM configuration)
	AND StoreCode like ''%GSC%''
	
)
';
DECLARE @Current varbinary(32)=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(OBJECT_DEFINITION(OBJECT_ID(N'dbo.SOD_fntbl_CheckQty')),CHAR(13),N'')));
IF @Current=HASHBYTES('SHA2_256',CONVERT(varbinary(max),REPLACE(REPLACE(@Definition,N'ALTER FUNCTION',N'CREATE FUNCTION'),CHAR(13),N''))) RETURN;
IF @Current IS NULL OR @Current<>0x4A51655218EC63E0198897A2F9E77D6C5DA9934B2CF1CF779B833CD2360BBF78
 THROW 52602,'Existing definition changed or is unavailable. Review instead of overwriting.',1;
EXEC sys.sp_executesql @Definition;
