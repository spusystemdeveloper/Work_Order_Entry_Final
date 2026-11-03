-- Optional: V4 quotation-import tracking only. No existing customer/order rows change.
-- Import through Database Updater, whose caller owns the transaction.
IF DB_NAME()<>N'PUREGOLD_DB' THROW 52600,'Expected PUREGOLD_DB.',1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1 THROW 52601,'Run in Database Updater active transaction.',1;
IF OBJECT_ID(N'dbo.SOD_ImportedQuote') IS NOT NULL
BEGIN
 IF OBJECT_ID(N'dbo.SOD_ImportedQuote',N'U') IS NULL
  THROW 52605,'SOD_ImportedQuote exists with an unexpected object type.',1;
 IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote'))<>5
  OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND (is_computed=1 OR (name<>N'ID' AND is_identity=1)))
  OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND name=N'ID' AND system_type_id=127 AND is_identity=1 AND is_nullable=0)
  OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND name=N'ImportDate' AND system_type_id=61 AND is_nullable=1)
  OR (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND name IN(N'QuoteID',N'ReferenceID',N'SalesRepID') AND system_type_id=56 AND is_nullable=1)<>3
  OR NOT EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id WHERE i.object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND i.is_primary_key=1 AND ic.key_ordinal=1 AND c.name=N'ID')
  OR (SELECT COUNT(*) FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id WHERE i.object_id=OBJECT_ID(N'dbo.SOD_ImportedQuote') AND i.is_primary_key=1 AND ic.key_ordinal>0)<>1
  THROW 52606,'Existing import tracking differs. Review before changing.',1;
 RETURN;
END;
CREATE TABLE dbo.SOD_ImportedQuote
(
 ID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SOD_ImportedQuote PRIMARY KEY,
 ImportDate datetime NULL,
 QuoteID int NULL,
 ReferenceID int NULL,
 SalesRepID int NULL
);
