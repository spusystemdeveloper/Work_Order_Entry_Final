USE [QUEUEING_DB]
GO

/****** Object:  View [dbo].[SOD_viewLastUpdatedOrder]    Script Date: 12/1/2020 11:17:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create view [dbo].[SOD_viewLastUpdatedOrder]
as
select top 5 ReferenceID  from SOD_Item_Logs 
where type = 'Order Update Logs'
group by ReferenceID 
order by max(date) desc 
GO


