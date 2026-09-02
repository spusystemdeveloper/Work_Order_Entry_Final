/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: GSC_STORE_DB
  Object: [dbo].[SOD_fntbl_WoDetails]
  Source type: TF
  Review branch-specific database, StoreID, warehouse, and business-rule references before deployment.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_fntbl_WoDetails]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'TF'
    THROW 52001, 'Object type conflict for [dbo].[SOD_fntbl_WoDetails].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
CREATE function [dbo].[SOD_fntbl_WoDetails]
(
			@Orderid int, @Register int, @Cashier varchar(200)
)
RETURNS @temptable TABLE 
(
		Branch varchar(max),Orderid int,AccountNo varchar(255),Company varchar(255),Register int,
		Cashier varchar(255),OrderDate Varchar(255),Ordertime Varchar(255),Reference varchar(255),Comment varchar(255),
		Subtotal decimal(20,2),SalesTax decimal(20,2),Total decimal(20,2)
)
AS
BEGIN 
			insert into @temptable
			(Branch,Orderid,AccountNo,Company,Register,
			Cashier,OrderDate,Ordertime,Reference,Comment,Subtotal,SalesTax,Total)

			Select (select storeCity from [Configuration]) ,o.id,c.AccountNumber,c.Company , 
			@Register , @Cashier,CONVERT(VARCHAR(50), o.Time , 101) as ''Date'',
			REPLACE(REPLACE(RIGHT(Convert(DateTime, o.Time, 0),7), ''P'', '' P''), ''A'', '' A'') as ''Time'',
			o.ReferenceNumber,o.Comment,o.Total - o.Tax as ''Subtotal'',o.Tax , o.Total 

			from [order] o
			inner join Customer c on o.CustomerID = c.id
			where o.id = @orderid

		RETURN
END
';
ELSE
    EXEC sys.sp_executesql N'
ALTER function [dbo].[SOD_fntbl_WoDetails]
(
			@Orderid int, @Register int, @Cashier varchar(200)
)
RETURNS @temptable TABLE 
(
		Branch varchar(max),Orderid int,AccountNo varchar(255),Company varchar(255),Register int,
		Cashier varchar(255),OrderDate Varchar(255),Ordertime Varchar(255),Reference varchar(255),Comment varchar(255),
		Subtotal decimal(20,2),SalesTax decimal(20,2),Total decimal(20,2)
)
AS
BEGIN 
			insert into @temptable
			(Branch,Orderid,AccountNo,Company,Register,
			Cashier,OrderDate,Ordertime,Reference,Comment,Subtotal,SalesTax,Total)

			Select (select storeCity from [Configuration]) ,o.id,c.AccountNumber,c.Company , 
			@Register , @Cashier,CONVERT(VARCHAR(50), o.Time , 101) as ''Date'',
			REPLACE(REPLACE(RIGHT(Convert(DateTime, o.Time, 0),7), ''P'', '' P''), ''A'', '' A'') as ''Time'',
			o.ReferenceNumber,o.Comment,o.Total - o.Tax as ''Subtotal'',o.Tax , o.Total 

			from [order] o
			inner join Customer c on o.CustomerID = c.id
			where o.id = @orderid

		RETURN
END
';
GO
