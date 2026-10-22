

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
         FOR XML PATH ('')), 1, 2, '')) as 'Orders',
		 opis as 'OPIS'
 from Queueing x
GO


