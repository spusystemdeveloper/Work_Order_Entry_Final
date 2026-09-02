

ALTER view [dbo].[SOD_viewTableLastUpdate]
as
SELECT OBJECT_NAME(OBJECT_ID) AS TableName,
 max(last_user_update) as 'LastUpdate'
FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID( 'GSC_STORE_DB')
AND (OBJECT_ID=OBJECT_ID('OrderEntry') 
OR OBJECT_ID=OBJECT_ID('Order') 
OR OBJECT_ID=OBJECT_ID('Queueing') 
OR OBJECT_ID=OBJECT_ID('Queueingitems')
OR OBJECT_ID=OBJECT_ID('Item'))
group by OBJECT_NAME(OBJECT_ID)
GO


