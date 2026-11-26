-- Synthetic fixture for the one-line Davao PO recovery. No store connection.
CREATE TABLE dbo.Item
(
    ID int PRIMARY KEY, ItemLookupCode nvarchar(25) NOT NULL,
    ItemType smallint NOT NULL, Inactive bit NOT NULL, Quantity float NOT NULL,
    QuantityCommitted float NOT NULL, Cost money NOT NULL, Price money NOT NULL,
    TaxID int NOT NULL DEFAULT 1, BarcodeFormat int NOT NULL DEFAULT 6,
    LastUpdated datetime NOT NULL, DBTimeStamp rowversion
);
CREATE TABLE dbo.PurchaseOrder
(
    ID int PRIMARY KEY, PONumber nvarchar(20) NOT NULL, POTitle nvarchar(50) NOT NULL,
    POType int NOT NULL, Status smallint NOT NULL, StoreID int NOT NULL,
    BatchNumber int NOT NULL, DBTimeStamp rowversion
);
CREATE TABLE dbo.PurchaseOrderEntry
(
    ID int PRIMARY KEY, PurchaseOrderID int NOT NULL, ItemID int NOT NULL, StoreID int NOT NULL,
    QuantityOrdered float NOT NULL, QuantityReceivedToDate float NOT NULL,
    QuantityReceived float NOT NULL, LastQuantityReceived float NOT NULL,
    LastReceivedDate datetime NOT NULL, Price money NOT NULL, DBTimeStamp rowversion
);
CREATE INDEX IX_PurchaseOrderID ON dbo.PurchaseOrderEntry(PurchaseOrderID);
CREATE TABLE dbo.InventoryTransferLog
(
    ID int IDENTITY PRIMARY KEY, ItemID int NOT NULL, DetailID int NOT NULL DEFAULT 0,
    Quantity float NOT NULL, DateTransferred datetime NOT NULL, StoreID int NOT NULL,
    DBTimeStamp rowversion, ReferenceID int NOT NULL, ReasonCodeID int NOT NULL DEFAULT 0,
    CashierID int NOT NULL DEFAULT 0, Type int NOT NULL, ReferenceEntryID int NOT NULL,
    Cost money NOT NULL, BatchNumber int NOT NULL, ComputedQuantity float NULL
);
CREATE INDEX IX_ItemID ON dbo.InventoryTransferLog(ItemID);
CREATE TABLE dbo.SOD_Item_Logs
(
    ItemID int, ItemLookupCode varchar(25), [Type] varchar(50), ReferenceID varchar(30),
    QuantityORIS int, LastQuantity int, NewQuantity int, LastCommitted int,
    NewCommitted int, [Date] datetime
);
CREATE TABLE dbo.RMSTMP_InventoryMovement
(
    ID int IDENTITY PRIMARY KEY, ItemLookupCode nvarchar(25) NOT NULL, Quantity float NOT NULL,
    StoreID int NOT NULL, DateCreated datetime NOT NULL DEFAULT GETDATE(),
    LastUpdated datetime NOT NULL DEFAULT GETDATE(), Synched bit NOT NULL CONSTRAINT DF_FixtureMovementSynched DEFAULT 0,
    DateSynched nvarchar(30) NOT NULL DEFAULT N''
);
CREATE TABLE dbo.RMSTMP_TransferInventory
(
    ID int IDENTITY PRIMARY KEY, ReferenceNumber nvarchar(20), ItemLookupCode nvarchar(25),
    Quantity float, FromStore int, ToStore int, DateCreated datetime, Synched bit
);
CREATE TABLE dbo.Configuration(StoreID int);
CREATE TABLE dbo.ItemValueLog
(
    ItemID int, StoreID int, OldAmount money, NewAmount money, AmountType char(1), LastUpdated datetime
);
CREATE TABLE dbo.RecordDeletedLog(TableName nvarchar(50), IDFieldName nvarchar(20), RecordID int, WhenDeleted datetime);

INSERT dbo.Configuration VALUES (4001);
INSERT dbo.Item(ID,ItemLookupCode,ItemType,Inactive,Quantity,QuantityCommitted,Cost,Price,LastUpdated) VALUES
 (16912,N'11688BOX',0,0,0,0,5.2836,12,'20261013'),
 (16914,N'11688PCS',0,0,32844,240,0.4760,1,'20261103'),
 (16915,N'11689BOX',0,0,52,52,9.4980,20,'20261103');
INSERT dbo.PurchaseOrder(ID,PONumber,POTitle,POType,Status,StoreID,BatchNumber) VALUES
 (27782,N'0002600',N'REPACKING_RRF#0147',3,2,4001,2511),
 (27784,N'0002601',N'IIR#0002600_RRF#0147',2,2,4001,2511);
INSERT dbo.PurchaseOrderEntry(ID,PurchaseOrderID,ItemID,StoreID,QuantityOrdered,QuantityReceivedToDate,
    QuantityReceived,LastQuantityReceived,LastReceivedDate,Price) VALUES
 (97643,27782,16914,4001,240,240,0,0,'18991230',0.4760),
 (97646,27784,16912,4001,20,20,0,20,'2026-11-03T09:31:47',5.2836),
 (97647,27784,16915,4001,50,50,0,50,'2026-11-03T10:31:46',9.4980);
SET IDENTITY_INSERT dbo.InventoryTransferLog ON;
INSERT dbo.InventoryTransferLog(ID,ItemID,Quantity,DateTransferred,StoreID,ReferenceID,
    CashierID,Type,ReferenceEntryID,Cost,BatchNumber,ComputedQuantity) VALUES
 (106711,16914,-240,'2026-11-03T09:27:38.627',4001,27782,34,1,97643,0.4760,2511,32844),
 (106737,16915,50,'2026-11-03T10:31:45.720',4001,27784,34,1,97647,9.4980,2511,50);
SET IDENTITY_INSERT dbo.InventoryTransferLog OFF;
INSERT dbo.SOD_Item_Logs VALUES
 (16912,'11688BOX','Item Auto Logs','-',0,0,0,0,0,'2026-11-03T09:31:46.783');
SET IDENTITY_INSERT dbo.RMSTMP_TransferInventory ON;
INSERT dbo.RMSTMP_TransferInventory(ID,ReferenceNumber,ItemLookupCode,Quantity,FromStore,ToStore,DateCreated,Synched)
 VALUES(7166,N'0002601',N'11688BOX',20,0,4001,'2026-11-03T10:31:45.610',0);
SET IDENTITY_INSERT dbo.RMSTMP_TransferInventory OFF;
INSERT dbo.RMSTMP_InventoryMovement(ItemLookupCode,Quantity,StoreID,DateCreated)
 VALUES(N'11689BOX',50,4001,'2026-11-03T10:31:45.720');
