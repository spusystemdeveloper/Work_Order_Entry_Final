/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_WO_DraftDetail]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_WO_DraftDetail]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_WO_DraftDetail].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_WO_DraftDetail](
	[DraftDetailID] [int] IDENTITY(1,1) NOT NULL,
	[DraftID] [int] NOT NULL,
	[LineNo] [int] NOT NULL,
	[ItemCode] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ItemName] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
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
	[Description] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ExtendedDescription] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DiscAmount] [money] NULL,
	[OrderEntryID] [int] NULL,
	[StoreWhse] [bit] NOT NULL,
	[PreparedQty] [int] NULL,
	[LastPurchasedPrice] [money] NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DraftDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE NONCLUSTERED INDEX [IX_SOD_WO_DraftDetail_DraftID] ON [dbo].[SOD_WO_DraftDetail]
(
	[DraftID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

ALTER TABLE [dbo].[SOD_WO_DraftDetail] ADD  DEFAULT ((0)) FOR [StoreWhse]

ALTER TABLE [dbo].[SOD_WO_DraftDetail] ADD  DEFAULT (getdate()) FOR [CreatedAt]

ALTER TABLE [dbo].[SOD_WO_DraftDetail]  WITH CHECK ADD  CONSTRAINT [FK_SOD_WO_DraftDetail_Header] FOREIGN KEY([DraftID])
REFERENCES [dbo].[SOD_WO_DraftHeader] ([DraftID])
ON DELETE CASCADE

ALTER TABLE [dbo].[SOD_WO_DraftDetail] CHECK CONSTRAINT [FK_SOD_WO_DraftDetail_Header]';
GO
