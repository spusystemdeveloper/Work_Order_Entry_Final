/* Independent SELECT-only verification of the authorized 18947 -> 18946 mapping. */
USE [HO_PO_DB];
SET NOCOUNT ON;
IF DB_NAME()<>N'HO_PO_DB' OR @@SERVERNAME<>N'WIN-2847UGJBAV4'
    THROW 53920,'Wrong verification target.',1;
SELECT @@SERVERNAME AS ServerName,DB_NAME() AS DatabaseName,
       IDENT_CURRENT(N'dbo.Order') AS CurrentIdentity;
SELECT ID,StoreID,CustomerID,Closed,[Type],[Time],LastUpdated,Total,Tax
FROM dbo.[Order] WHERE ID IN(18946,18947);
SELECT ID,OrderID,ItemID,QuantityOnOrder,Price,FullPrice FROM dbo.OrderEntry
WHERE OrderID IN(18946,18947) ORDER BY ID;
SELECT id,OrderID,GroupTo,Status FROM dbo.Queueing
WHERE OrderID IN(18946,18947) OR GroupTo IN(18946,18947);
SELECT OrderId,COUNT_BIG(*) AS ApprovalRows FROM dbo.SOD_WO_LOGS
WHERE OrderId IN(18946,18947) GROUP BY OrderId;
SELECT OrderID,EventName,COUNT_BIG(*) AS EventRows FROM dbo.SOD_WO_History
WHERE OrderID IN(18946,18947) GROUP BY OrderID,EventName ORDER BY OrderID,EventName;
SELECT ItemID,[Type],ReferenceID,COUNT_BIG(*) AS LogRows
FROM dbo.SOD_Item_Logs
WHERE ItemID IN(56485,68066,68069,68072)
AND ReferenceID IN('18946','18947') AND [Type] IN('Order Entry Logs','Order Update Logs')
GROUP BY ItemID,[Type],ReferenceID ORDER BY ItemID,[Type];
SELECT ID,ItemLookupCode,Quantity,QuantityCommitted FROM dbo.Item
WHERE ID IN(56485,68066,68069,68072) ORDER BY ID;
SELECT OBJECT_NAME(t.parent_id) AS TableName,t.name AS TriggerName,t.is_disabled,
CONVERT(varchar(64),HASHBYTES('SHA2_256',m.definition),2) AS DefinitionHash
FROM sys.triggers t JOIN sys.sql_modules m ON m.object_id=t.object_id
WHERE OBJECT_NAME(t.parent_id) IN(N'Order',N'OrderEntry',N'Queueing',N'SOD_WO_LOGS',N'SOD_WO_History',N'SOD_Item_Logs',N'Transaction',N'Journal',N'OrderHistory')
ORDER BY TableName,TriggerName;
IF (SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18946 AND Closed=0 AND [Type]=2 AND Total=27274.67 AND Tax=0)<>1
 OR EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID=18947)
 OR EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE OrderID=18947)
 OR EXISTS(SELECT 1 FROM dbo.Queueing WHERE OrderID=18947 OR GroupTo=18947)
 OR EXISTS(SELECT 1 FROM dbo.SOD_WO_LOGS WHERE OrderId=18947)
 OR EXISTS(SELECT 1 FROM dbo.SOD_WO_History WHERE OrderID=18947)
 OR (SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=18946)<>4
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_LOGS WHERE OrderId=18946)<>4
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=18946)<>19
 OR (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=18946 AND EventName=N'Order ID changed')<>1
 OR IDENT_CURRENT(N'dbo.Order')<>18947
    THROW 53921,'Post-repair reference/count/identity verification failed. Keep clients closed.',1;
IF EXISTS(SELECT 1 FROM (VALUES(56485,500.0,500.0),(68066,1000.0,700.0),(68069,200.0,200.0),(68072,500.0,100.0)) e(ID,Quantity,QuantityCommitted)
 LEFT JOIN dbo.Item i ON i.ID=e.ID WHERE i.ID IS NULL OR i.Quantity<>e.Quantity OR i.QuantityCommitted<>e.QuantityCommitted)
    THROW 53922,'Inventory differs from the pre-repair audit. Keep clients closed.',1;
SELECT N'VERIFIED: requested mapping, four lines, queue, four approvals, nineteen audit events and unchanged inventory/identity.' AS Verification;
