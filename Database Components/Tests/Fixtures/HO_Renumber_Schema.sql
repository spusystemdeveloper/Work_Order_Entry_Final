/* Synthetic schema from the read-only HO metadata audit. NO store/customer data. */
CREATE TABLE dbo.[Journal](
 [ID] int IDENTITY(1,1) NOT NULL,
 [StoreID] int NOT NULL,
 [Time] datetime NOT NULL,
 [TransactionType] int NOT NULL,
 [ReferenceID] int NOT NULL,
 [CashierID] int NOT NULL,
 [CustomerID] int NOT NULL,
 [OrderHistoryID] int NOT NULL,
 [BatchNumber] int NOT NULL,
 [ReceiptText] ntext NOT NULL,
 [DBTimeStamp] timestamp NULL,
 [Compressed] bit NOT NULL,
 [ReceiptCompressed] image NULL);
CREATE TABLE dbo.[Order](
 [StoreID] int NOT NULL,
 [ID] int IDENTITY(1,1) NOT NULL,
 [Closed] bit NOT NULL,
 [Time] datetime NOT NULL,
 [Type] int NOT NULL,
 [Comment] nvarchar(255) NOT NULL,
 [CustomerID] int NOT NULL,
 [ShipToID] int NOT NULL,
 [DepositOverride] bit NOT NULL,
 [Deposit] money NOT NULL,
 [Tax] money NOT NULL,
 [Total] money NOT NULL,
 [LastUpdated] datetime NOT NULL,
 [ExpirationOrDueDate] datetime NOT NULL,
 [Taxable] bit NOT NULL,
 [SalesRepID] int NOT NULL,
 [ReferenceNumber] nvarchar(50) NOT NULL,
 [ShippingChargeOnOrder] money NOT NULL,
 [ShippingChargeOverride] bit NOT NULL,
 [ShippingServiceID] int NOT NULL,
 [ShippingTrackingNumber] nvarchar(255) NOT NULL,
 [ShippingNotes] ntext NOT NULL,
 [DBTimeStamp] timestamp NULL,
 [ReasonCodeID] int NOT NULL,
 [ExchangeID] int NOT NULL,
 [ChannelType] int NOT NULL,
 [DefaultDiscountReasonCodeID] int NOT NULL,
 [DefaultReturnReasonCodeID] int NOT NULL,
 [DefaultTaxChangeReasonCodeID] int NOT NULL);
CREATE TABLE dbo.[OrderEntry](
 [Cost] money NOT NULL,
 [StoreID] int NOT NULL,
 [ID] int IDENTITY(1,1) NOT NULL,
 [OrderID] int NOT NULL,
 [ItemID] int NOT NULL,
 [FullPrice] money NOT NULL,
 [PriceSource] smallint NOT NULL,
 [Price] money NOT NULL,
 [QuantityOnOrder] float NOT NULL,
 [SalesRepID] int NOT NULL,
 [Taxable] int NOT NULL,
 [DetailID] int NOT NULL,
 [Description] nvarchar(30) NOT NULL,
 [QuantityRTD] float NOT NULL,
 [LastUpdated] datetime NOT NULL,
 [Comment] nvarchar(255) NOT NULL,
 [DBTimeStamp] timestamp NULL,
 [DiscountReasonCodeID] int NOT NULL,
 [ReturnReasonCodeID] int NOT NULL,
 [TaxChangeReasonCodeID] int NOT NULL,
 [TransactionTime] datetime NULL,
 [IsAddMoney] bit NOT NULL,
 [VoucherID] int NOT NULL);
CREATE TABLE dbo.[OrderHistory](
 [ID] int IDENTITY(1,1) NOT NULL,
 [StoreID] int NOT NULL,
 [BatchNumber] int NOT NULL,
 [Date] datetime NOT NULL,
 [OrderID] int NOT NULL,
 [CashierID] int NOT NULL,
 [DeltaDeposit] money NOT NULL,
 [TransactionNumber] int NOT NULL,
 [Comment] nvarchar(30) NOT NULL,
 [DBTimeStamp] timestamp NULL);
CREATE TABLE dbo.[Queueing](
 [id] bigint IDENTITY(1,1) NOT NULL,
 [OrderID] bigint NULL,
 [Status] int NULL,
 [GroupTo] int NULL,
 [OPIS] varchar(max) NULL);
CREATE TABLE dbo.[SOD_Item_Logs](
 [ItemID] int NOT NULL,
 [ItemLookupCode] varchar(50) NOT NULL,
 [Type] varchar(max) NOT NULL,
 [ReferenceID] varchar(50) NOT NULL,
 [QuantityORIS] int NOT NULL,
 [LastQuantity] int NOT NULL,
 [NewQuantity] int NOT NULL,
 [LastCommitted] int NOT NULL,
 [NewCommitted] int NOT NULL,
 [Date] datetime NOT NULL);
CREATE TABLE dbo.[SOD_WO_History](
 [EventID] bigint NOT NULL,
 [OrderID] int NOT NULL,
 [EventTime] datetime2(3) NOT NULL,
 [EventName] nvarchar(60) NOT NULL,
 [Actor] nvarchar(128) NOT NULL,
 [Terminal] nvarchar(256) NOT NULL,
 [ItemID] int NULL,
 [Details] nvarchar(2000) NOT NULL);
CREATE TABLE dbo.[SOD_WO_LOGS](
 [id] int IDENTITY(1,1) NOT NULL,
 [OrderId] int NULL,
 [ItemID] int NULL,
 [FullPrice] money NULL,
 [Price] money NULL,
 [Pricea] money NULL,
 [Priceb] money NULL,
 [Pricec] money NULL,
 [LastUpdatedPrice] datetime NULL,
 [UpdatedPrice] money NULL,
 [ApprovedUser] nvarchar(50) NULL,
 [Status] nvarchar(50) NULL,
 [Date] datetime NULL,
 [timestamp] timestamp NULL,
 [OrderTaker] nvarchar(50) NULL,
 [CustomerPriceLevel] int NULL);
CREATE TABLE dbo.[Transaction](
 [ShipToID] int NOT NULL,
 [StoreID] int NOT NULL,
 [TransactionNumber] int IDENTITY(1,1) NOT NULL,
 [BatchNumber] int NOT NULL,
 [Time] datetime NOT NULL,
 [CustomerID] int NOT NULL,
 [CashierID] int NOT NULL,
 [Total] money NOT NULL,
 [SalesTax] money NOT NULL,
 [Comment] nvarchar(255) NOT NULL,
 [ReferenceNumber] nvarchar(50) NOT NULL,
 [DBTimeStamp] timestamp NULL,
 [Status] int NOT NULL,
 [ExchangeID] int NOT NULL,
 [ChannelType] int NOT NULL,
 [RecallID] int NOT NULL,
 [RecallType] int NOT NULL);
ALTER TABLE dbo.[Order] ADD CONSTRAINT PK_Order PRIMARY KEY(ID);
CREATE SEQUENCE dbo.SOD_WO_History_EventSequence AS bigint START WITH 1000 INCREMENT BY 1;
ALTER TABLE dbo.SOD_WO_History ADD CONSTRAINT DF_HO_TestEvent DEFAULT(NEXT VALUE FOR dbo.SOD_WO_History_EventSequence) FOR EventID;
ALTER TABLE dbo.SOD_WO_History ADD CONSTRAINT DF_HO_TestTime DEFAULT(SYSDATETIME()) FOR EventTime;
ALTER TABLE dbo.SOD_WO_History ADD CONSTRAINT DF_HO_TestItem DEFAULT(NULL) FOR ItemID;
CREATE TABLE dbo.[SOD_WO](OrderID int NULL);
CREATE TABLE dbo.[SI_ItemExpireTemp](OrderID int NULL);
CREATE TABLE dbo.[SI_OrderPrice](OrderID int NULL);
CREATE TABLE dbo.[SI_PickListTemp](OrderID int NULL);
CREATE TABLE dbo.[QuoteTenderEntry](OrderID int NULL);
CREATE TABLE dbo.[RMSTMP_Sales](RecallID int NULL);
CREATE TABLE dbo.[TransactionHold](RecallID int NULL);
CREATE TABLE dbo.[TransactionHoldEntry](RecallID int NULL);
CREATE TABLE dbo.SOD_ImportedQuote(QuoteID int,ReferenceID int);