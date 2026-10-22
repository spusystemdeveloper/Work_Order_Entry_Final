
CREATE function [dbo].[SOD_fntbl_WoEntry]
(
			@Orderid int
)
RETURNS @temptable TABLE 
(
		Itemcode varchar(10),ItemDesc varchar(max),QtyOrder int,Price varchar(max),Extended varchar(max)
)
AS
BEGIN 
			insert into @temptable (Itemcode,ItemDesc,QtyOrder,Price,Extended)

			select i.ItemLookupCode, 
			cast(i.Description as varchar(max)) + '' +cast(i.extendedDescription as varchar(max)) as 'Desc',
			o.QuantityOnOrder,convert(varchar,cast(o.price as money),-1),convert(varchar,cast(o.price * o.QuantityOnOrder  as money),-1) as 'Extended'
			from OrderEntry o 
			inner join item i on o.ItemID = i.id
			where o.orderid = @Orderid

		RETURN
END

GO


