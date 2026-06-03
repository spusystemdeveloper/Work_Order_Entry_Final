USE [SBD_CEBU_MAIN_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SOD_WO_DraftDetail]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SOD_WO_DraftDetail](
        [DraftDetailID] [int] IDENTITY(1,1) NOT NULL,
        [DraftID] [int] NOT NULL,
        [LineNo] [int] NOT NULL,
        [ItemCode] [nvarchar](50) NOT NULL,
        [ItemName] [nvarchar](500) NULL,
        [Qty] [int] NULL,
        [Price] [money] NULL,
        [Disc] [money] NULL,
        [TotalPrice] [money] NULL,
        [LessVat] [money] NULL,
        [VatSales] [money] NULL,
        [DiscPrice] [money] NULL,
        [Cost] [money] NULL,
        [Taxable] [int] NULL,
        [ItemID] [bigint] NULL,
        [FullPrice] [money] NULL,
        [Description] [nvarchar](500) NULL,
        [ExtendedDescription] [nvarchar](max) NULL,
        [DiscAmount] [money] NULL,
        [OrderEntryID] [int] NULL,
        [StoreWhse] [bit] NOT NULL CONSTRAINT [DF_SOD_WO_DraftDetail_StoreWhse] DEFAULT ((0)),
        [PreparedQty] [int] NULL,
        [LastPurchasedPrice] [money] NULL,
        [CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_SOD_WO_DraftDetail_CreatedAt] DEFAULT (getdate()),
        CONSTRAINT [PK_SOD_WO_DraftDetail] PRIMARY KEY CLUSTERED ([DraftDetailID] ASC),
        CONSTRAINT [FK_SOD_WO_DraftDetail_SOD_WO_DraftHeader] FOREIGN KEY ([DraftID])
            REFERENCES [dbo].[SOD_WO_DraftHeader] ([DraftID]) ON DELETE CASCADE
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_SOD_WO_DraftDetail_DraftID_LineNo'
      AND object_id = OBJECT_ID('[dbo].[SOD_WO_DraftDetail]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SOD_WO_DraftDetail_DraftID_LineNo]
    ON [dbo].[SOD_WO_DraftDetail] ([DraftID] ASC, [LineNo] ASC)
END
GO
