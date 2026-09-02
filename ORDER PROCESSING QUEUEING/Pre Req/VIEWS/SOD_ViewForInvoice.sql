USE [QUEUEING_DB]
GO

/****** Object:  View [dbo].[SOD_ViewForInvoice]    Script Date: 12/1/2020 11:16:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--Select STUFF((
--    SELECT ',' + cast(orderid as varchar(MAX))    
--    FROM Queueing
--    FOR XML PATH('')), 1, 1,'') 
--FROM Queueing

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
			where b.Status  = 'Prepared'  and a.GroupTo = x.GroupTo)
			then 'Prepared'
			else 'NotPrepared'
			end as 'Status',
(STUFF((SELECT ', ' + CAST(z.orderid AS VARCHAR(MAX)) 
         FROM Queueing  z
         WHERE (z.GroupTo = x.GroupTo ) 
         FOR XML PATH ('')), 1, 2, '')) as 'Orders'
 from Queueing x
GO


