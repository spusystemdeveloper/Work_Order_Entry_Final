
create view [dbo].[SOD_viewLastUpdatedItem]
as 
select top 20 s.ItemLookupCode,
(select quantity-QuantityCommitted  from item i where i.ItemLookupCode = s.ItemLookupCode) as 'Available'
from SOD_Item_Logs s
where s.type = 'Item Auto Logs'
group by s.ItemLookupCode 
order by max(s.date) desc 
GO


