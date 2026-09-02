USE [QUEUEING_DB]
GO

/****** Object:  Table [dbo].[SOD_Item_Logs]    Script Date: 12/1/2020 11:16:10 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SOD_Item_Logs](
	[ItemID] [int] NOT NULL,
	[ItemLookupCode] [varchar](50) NOT NULL,
	[Type] [varchar](max) NOT NULL,
	[ReferenceID] [varchar](50) NOT NULL,
	[QuantityORIS] [int] NOT NULL,
	[LastQuantity] [int] NOT NULL,
	[NewQuantity] [int] NOT NULL,
	[LastCommitted] [int] NOT NULL,
	[NewCommitted] [int] NOT NULL,
	[Date] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


