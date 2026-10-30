/* Portable optional migration, CONSIGNMENT_DB only. The updater owns the
   transaction. Only the reviewed legacy function is changed; customized
   definitions stop for review. Existing amounts/joins/branch text are retained. */
IF DB_NAME() <> N'CONSIGNMENT_DB'
    THROW 52340, 'This printing migration targets CONSIGNMENT_DB only.', 1;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1
    THROW 52341, 'Run through Database Updater in its active transaction.', 1;
IF OBJECT_ID(N'dbo.SOD_fntbl_WoDetails',N'TF') IS NULL
    THROW 52342, 'Existing reviewed multi-statement SOD_fntbl_WoDetails is required.', 1;

DECLARE @Expected nvarchar(max)=N'CREATE function [dbo].[SOD_fntbl_WoDetails]
(@Orderid int, @Register int, @Cashier varchar(200))
RETURNS @temptable TABLE
(Branch varchar(max),Orderid int,AccountNo varchar(255),Company varchar(255),Register int,
Cashier varchar(255),OrderDate Varchar(255),Ordertime Varchar(255),Reference varchar(255),Comment varchar(255),
Subtotal decimal(20,2),SalesTax decimal(20,2),Total decimal(20,2))
AS BEGIN
insert into @temptable
(Branch,Orderid,AccountNo,Company,Register,Cashier,OrderDate,Ordertime,Reference,Comment,Subtotal,SalesTax,Total)
Select (select storeCity from [Configuration]),o.id,c.AccountNumber,c.Company,
@Register,@Cashier,CONVERT(VARCHAR(50),o.Time,101) as ''Date'',
REPLACE(REPLACE(RIGHT(Convert(DateTime,o.Time,0),7),''P'','' P''),''A'','' A'') as ''Time'',
o.ReferenceNumber,o.Comment,o.Total-o.Tax as ''Subtotal'',o.Tax,o.Total
from [order] o inner join Customer c on o.CustomerID=c.id where o.id=@orderid
RETURN END';
-- Compare the reviewed body ignoring formatting; the replacement below retains
-- its calculations, literals, parameters and join/filter behavior.
DECLARE @Actual nvarchar(max)=OBJECT_DEFINITION(OBJECT_ID(N'dbo.SOD_fntbl_WoDetails'));
DECLARE @Normalized nvarchar(max)=LOWER(REPLACE(REPLACE(REPLACE(REPLACE(@Actual,CHAR(13),''),CHAR(10),''),CHAR(9),''),' ',''));
DECLARE @Baseline nvarchar(max)=LOWER(REPLACE(REPLACE(REPLACE(REPLACE(@Expected,CHAR(13),''),CHAR(10),''),CHAR(9),''),' ',''));
-- ALTER and CREATE identify the same reviewed body after initial deployment.
IF LEFT(@Normalized,5)='alter' SET @Normalized=STUFF(@Normalized,1,5,'create');
DECLARE @Updated nvarchar(max)=REPLACE(@Baseline,'companyvarchar(255),register','companyvarchar(255),addressnvarchar(255),register');
SET @Updated=REPLACE(@Updated,'accountno,company,register','accountno,company,address,register');
SET @Updated=REPLACE(@Updated,'c.accountnumber,c.company,','c.accountnumber,c.company,concat(c.address,c.address2,c.city,c.state,c.zip),');
IF @Normalized=@Updated RETURN;
IF @Normalized IS NULL OR @Normalized<>@Baseline
    THROW 52343, 'Printing function differs from the reviewed CONSIGNMENT definition. Existing function preserved; review customization first.', 1;

-- Use a canonical equivalent of the verified body to add the single output.
SET @Expected=REPLACE(@Expected,'Company varchar(255),Register','Company varchar(255),Address nvarchar(255),Register');
SET @Expected=REPLACE(@Expected,'AccountNo,Company,Register','AccountNo,Company,Address,Register');
SET @Expected=REPLACE(@Expected,'c.AccountNumber,c.Company,','c.AccountNumber,c.Company,CONCAT(c.Address,c.Address2,c.City,c.State,c.Zip),');
SET @Expected=STUFF(@Expected,1,6,'ALTER');
EXEC sys.sp_executesql @Expected;
