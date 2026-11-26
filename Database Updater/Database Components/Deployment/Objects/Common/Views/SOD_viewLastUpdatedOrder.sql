/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_viewLastUpdatedOrder]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_viewLastUpdatedOrder]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52001, 'Object type conflict for [dbo].[SOD_viewLastUpdatedOrder].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'

CREATE view [dbo].[SOD_viewLastUpdatedOrder]
as
select top 10 ReferenceID  from SOD_Item_Logs 
where type = ''Order Update Logs''
group by ReferenceID 
order by max(date) desc 
';
ELSE
    EXEC sys.sp_executesql N'

ALTER view [dbo].[SOD_viewLastUpdatedOrder]
as
select top 10 ReferenceID  from SOD_Item_Logs 
where type = ''Order Update Logs''
group by ReferenceID 
order by max(date) desc 
';
GO
