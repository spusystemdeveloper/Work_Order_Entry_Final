/* Independent SELECT-only verification of the authorized 19057 -> 19056 mapping and reseed. */
USE [HO_PO_DB];
SET NOCOUNT ON;
IF DB_NAME()<>N'HO_PO_DB' OR @@SERVERNAME<>N'WIN-2847UGJBAV4'
    THROW 53920,'Wrong verification target.',1;
SELECT @@SERVERNAME AS ServerName,DB_NAME() AS DatabaseName,
       IDENT_CURRENT(N'dbo.Order') AS CurrentIdentity,
       (SELECT MAX(ID) FROM dbo.[Order]) AS MaxOrderID;
SELECT ID,StoreID,CustomerID,Closed,[Type],[Time],LastUpdated,Total,Tax
FROM dbo.[Order] WHERE ID IN(19056,19057);
SELECT ID,OrderID,ItemID,QuantityOnOrder,Price,FullPrice FROM dbo.OrderEntry
WHERE OrderID IN(19056,19057) ORDER BY ID;
SELECT id,OrderID,GroupTo,Status FROM dbo.Queueing
WHERE OrderID IN(19056,19057) OR GroupTo IN(19056,19057);
SELECT id,OrderId,ItemID,FullPrice,Price,UpdatedPrice FROM dbo.SOD_WO_LOGS
WHERE OrderId IN(19056,19057);
SELECT OrderID,EventName,COUNT_BIG(*) AS EventRows FROM dbo.SOD_WO_History
WHERE OrderID IN(19056,19057) GROUP BY OrderID,EventName ORDER BY OrderID,EventName;
SELECT ItemID,[Type],ReferenceID,COUNT_BIG(*) AS LogRows
FROM dbo.SOD_Item_Logs
WHERE ItemID IN(59626)
AND ReferenceID IN('19056','19057') AND [Type] IN('Order Entry Logs','Order Update Logs')
GROUP BY ItemID,[Type],ReferenceID ORDER BY ItemID,[Type];
SELECT ID,ItemLookupCode,Quantity,QuantityCommitted FROM dbo.Item
WHERE ID IN(59626) ORDER BY ID;
SELECT OBJECT_NAME(t.parent_id) AS TableName,t.name AS TriggerName,t.is_disabled,
CONVERT(varchar(64),HASHBYTES('SHA2_256',m.definition),2) AS DefinitionHash
FROM sys.triggers t JOIN sys.sql_modules m ON m.object_id=t.object_id
WHERE OBJECT_NAME(t.parent_id) IN(N'Order',N'OrderEntry',N'Queueing',N'SOD_WO_LOGS',N'SOD_WO_History',N'SOD_Item_Logs',N'Transaction',N'Journal',N'OrderHistory')
ORDER BY TableName,TriggerName;
IF (SELECT COUNT(*) FROM dbo.[Order] WHERE ID=19056 AND Closed=0 AND [Type]=2 AND Total=3612280.2000 AND Tax=0 AND CustomerID=203)<>1
 OR EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID=19057)
 OR EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE OrderID=19057)
 OR EXISTS(SELECT 1 FROM dbo.Queueing WHERE OrderID=19057 OR GroupTo=19057)
 OR EXISTS(SELECT 1 FROM dbo.SOD_WO_LOGS WHERE OrderId=19057)
 OR EXISTS(SELECT 1 FROM dbo.SOD_WO_History WHERE OrderID=19057)
 OR (SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=19056 AND ID=62152)<>1
 OR (SELECT COUNT(*) FROM dbo.Queueing WHERE OrderID=19056 AND GroupTo=19056 AND id=46074)<>1
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_LOGS WHERE OrderId=19056 AND id=388)<>1
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=19056)<>6
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=19056 AND EventName=N'Order ID changed')<>1
 OR IDENT_CURRENT(N'dbo.Order')<>19056
 OR (SELECT MAX(ID) FROM dbo.[Order])<>19056
    THROW 53921,'Post-repair reference/count/identity verification failed. Keep clients closed.',1;
IF EXISTS(SELECT 1 FROM (VALUES(59626,6000.0,6000.0)) e(ID,Quantity,QuantityCommitted)
 LEFT JOIN dbo.Item i ON i.ID=e.ID WHERE i.ID IS NULL OR i.Quantity<>e.Quantity OR i.QuantityCommitted<>e.QuantityCommitted)
    THROW 53922,'Inventory differs from the pre-repair audit. Keep clients closed.',1;
SELECT N'VERIFIED: requested 19057 -> 19056 mapping, line 62152, queue 46074, approval 388, six audit events, unchanged inventory, and Order identity reseeded to 19056.' AS Verification;
