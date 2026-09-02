/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_WO_DraftHeader]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_WO_DraftHeader]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_WO_DraftHeader].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_WO_DraftHeader](
	[DraftID] [int] IDENTITY(1,1) NOT NULL,
	[RegisterID] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UserName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntryType] [int] NULL,
	[CustomerID] [int] NULL,
	[CustomerName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[BusinessCustomerType] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PaymentType] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SalesRepID] [int] NULL,
	[SalesRepName] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Remarks] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReleaseType] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[PriceLevel] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[TaxExempt] [bit] NOT NULL,
	[ZeroRated] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DraftID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

SET ANSI_PADDING ON

CREATE NONCLUSTERED INDEX [IX_SOD_WO_DraftHeader_Register_User] ON [dbo].[SOD_WO_DraftHeader]
(
	[RegisterID] ASC,
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

ALTER TABLE [dbo].[SOD_WO_DraftHeader] ADD  DEFAULT ((0)) FOR [TaxExempt]

ALTER TABLE [dbo].[SOD_WO_DraftHeader] ADD  DEFAULT ((0)) FOR [ZeroRated]

ALTER TABLE [dbo].[SOD_WO_DraftHeader] ADD  DEFAULT (getdate()) FOR [CreatedAt]

ALTER TABLE [dbo].[SOD_WO_DraftHeader] ADD  DEFAULT (getdate()) FOR [UpdatedAt]';
GO
