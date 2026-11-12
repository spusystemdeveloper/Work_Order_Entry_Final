/*
  Work Order Entry / Order Processing Queueing database object
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_viewTableLastUpdate]
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
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_viewTableLastUpdate]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52001, 'Object type conflict for [dbo].[SOD_viewTableLastUpdate].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'


CREATE view [dbo].[SOD_viewTableLastUpdate]
as
SELECT OBJECT_NAME(OBJECT_ID) AS TableName,
 max(last_user_update) as ''LastUpdate''
FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID( ''GSC_STORE_DB'')
AND (OBJECT_ID=OBJECT_ID(''OrderEntry'') 
OR OBJECT_ID=OBJECT_ID(''Order'') 
OR OBJECT_ID=OBJECT_ID(''Queueing'') 
OR OBJECT_ID=OBJECT_ID(''Queueingitems'')
OR OBJECT_ID=OBJECT_ID(''Item''))
group by OBJECT_NAME(OBJECT_ID)
';
ELSE
    EXEC sys.sp_executesql N'


ALTER view [dbo].[SOD_viewTableLastUpdate]
as
SELECT OBJECT_NAME(OBJECT_ID) AS TableName,
 max(last_user_update) as ''LastUpdate''
FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID( ''GSC_STORE_DB'')
AND (OBJECT_ID=OBJECT_ID(''OrderEntry'') 
OR OBJECT_ID=OBJECT_ID(''Order'') 
OR OBJECT_ID=OBJECT_ID(''Queueing'') 
OR OBJECT_ID=OBJECT_ID(''Queueingitems'')
OR OBJECT_ID=OBJECT_ID(''Item''))
group by OBJECT_NAME(OBJECT_ID)
';
GO
