USE [QUEUEING_DB]
GO

/****** Object:  Table [dbo].[QueueingItems]    Script Date: 12/1/2020 11:15:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[QueueingItems](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[QueueingID] [bigint] NULL,
	[ItemID] [int] NULL,
	[QtyPre] [int] NULL,
	[Picker] [nvarchar](50) NULL,
	[Status] [varchar](50) NULL,
	[PickLoc] [varchar](max) NULL,
 CONSTRAINT [PK_QueueingItems] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


