/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_ViewForInvoice]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_ViewForInvoice]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52001, 'Object type conflict for [dbo].[SOD_ViewForInvoice].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'


CREATE view [dbo].[SOD_ViewForInvoice]
as

select distinct x.groupto,
case when (select count(a.GroupTo)
			from Queueing a
			inner join Queueingitems b on a.id = b.QueueingID 
			inner join item c on b.ItemID = c.id
			inner join [order] d on a.OrderID = d.id
			where a.GroupTo = x.GroupTo) 
			= 
			(select count(a.GroupTo)
			from Queueing a
			inner join Queueingitems b on a.id = b.QueueingID 
			inner join item c on b.ItemID = c.id
			inner join [order] d on a.OrderID = d.id
			where b.Status  = ''Prepared''  and a.GroupTo = x.GroupTo)
			then ''Prepared''
			else ''NotPrepared''
			end as ''Status'',
(STUFF((SELECT '', '' + CAST(z.orderid AS VARCHAR(MAX)) 
         FROM Queueing  z
         WHERE (z.GroupTo = x.GroupTo ) 
         FOR XML PATH ('''')), 1, 2, '''')) as ''Orders'',
		 opis as ''OPIS''
 from Queueing x
 where x.Status = 0
';
ELSE
    EXEC sys.sp_executesql N'


ALTER view [dbo].[SOD_ViewForInvoice]
as

select distinct x.groupto,
case when (select count(a.GroupTo)
			from Queueing a
			inner join Queueingitems b on a.id = b.QueueingID 
			inner join item c on b.ItemID = c.id
			inner join [order] d on a.OrderID = d.id
			where a.GroupTo = x.GroupTo) 
			= 
			(select count(a.GroupTo)
			from Queueing a
			inner join Queueingitems b on a.id = b.QueueingID 
			inner join item c on b.ItemID = c.id
			inner join [order] d on a.OrderID = d.id
			where b.Status  = ''Prepared''  and a.GroupTo = x.GroupTo)
			then ''Prepared''
			else ''NotPrepared''
			end as ''Status'',
(STUFF((SELECT '', '' + CAST(z.orderid AS VARCHAR(MAX)) 
         FROM Queueing  z
         WHERE (z.GroupTo = x.GroupTo ) 
         FOR XML PATH ('''')), 1, 2, '''')) as ''Orders'',
		 opis as ''OPIS''
 from Queueing x
 where x.Status = 0
';
GO
