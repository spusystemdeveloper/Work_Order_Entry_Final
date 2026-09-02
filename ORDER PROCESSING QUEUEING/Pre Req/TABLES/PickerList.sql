USE [QUEUEING_DB]
GO

/****** Object:  Table [dbo].[PickerList]    Script Date: 12/1/2020 11:16:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PickerList](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Initial] [varchar](5) NULL,
	[Name] [varchar](100) NULL,
	[RegisterNo] [int] NULL,
	[Password] [varchar](50) NULL,
 CONSTRAINT [PK_PickerList] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


