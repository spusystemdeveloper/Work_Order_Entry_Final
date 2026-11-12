SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.SOD_WO_DraftHeader', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SOD_WO_DraftHeader
        (
            DraftID int IDENTITY(1,1) NOT NULL,
            RegisterID nvarchar(50) NOT NULL,
            UserName nvarchar(100) NOT NULL,
            EntryType int NULL,
            CustomerID int NULL,
            CustomerName nvarchar(255) NULL,
            BusinessCustomerType nvarchar(100) NULL,
            PaymentType nvarchar(100) NULL,
            SalesRepID int NULL,
            SalesRepName nvarchar(255) NULL,
            Remarks nvarchar(max) NULL,
            ReleaseType nvarchar(50) NULL,
            PriceLevel nvarchar(50) NULL,
            TaxExempt bit NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_TaxExempt DEFAULT (0),
            ZeroRated bit NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_ZeroRated DEFAULT (0),
            CreatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_CreatedAt DEFAULT (GETDATE()),
            UpdatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftHeader_UpdatedAt DEFAULT (GETDATE()),
            CONSTRAINT PK_SOD_WO_DraftHeader PRIMARY KEY CLUSTERED (DraftID),
            CONSTRAINT UQ_SOD_WO_DraftHeader_RegisterUser UNIQUE (RegisterID, UserName)
        );
    END;

    IF OBJECT_ID(N'dbo.SOD_WO_DraftDetail', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SOD_WO_DraftDetail
        (
            DraftDetailID int IDENTITY(1,1) NOT NULL,
            DraftID int NOT NULL,
            [LineNo] int NOT NULL,
            ItemCode nvarchar(50) NOT NULL,
            ItemName nvarchar(500) NULL,
            Qty int NULL,
            Price money NULL,
            Disc money NULL,
            TotalPrice money NULL,
            LessVat money NULL,
            VatSales money NULL,
            DiscPrice money NULL,
            Cost money NULL,
            Taxable int NULL,
            ItemID bigint NULL,
            FullPrice money NULL,
            Description nvarchar(500) NULL,
            ExtendedDescription nvarchar(max) NULL,
            DiscAmount money NULL,
            OrderEntryID int NULL,
            StoreWhse bit NOT NULL CONSTRAINT DF_SOD_WO_DraftDetail_StoreWhse DEFAULT (0),
            PreparedQty int NULL,
            LastPurchasedPrice money NULL,
            CreatedAt datetime NOT NULL CONSTRAINT DF_SOD_WO_DraftDetail_CreatedAt DEFAULT (GETDATE()),
            CONSTRAINT PK_SOD_WO_DraftDetail PRIMARY KEY CLUSTERED (DraftDetailID),
            CONSTRAINT FK_SOD_WO_DraftDetail_SOD_WO_DraftHeader
                FOREIGN KEY (DraftID) REFERENCES dbo.SOD_WO_DraftHeader(DraftID) ON DELETE CASCADE
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.SOD_WO_DraftDetail')
          AND name = N'IX_SOD_WO_DraftDetail_DraftID_LineNo'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SOD_WO_DraftDetail_DraftID_LineNo
            ON dbo.SOD_WO_DraftDetail(DraftID, [LineNo]);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
