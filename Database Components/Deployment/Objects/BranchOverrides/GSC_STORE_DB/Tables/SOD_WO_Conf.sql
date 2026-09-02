/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: GSC_STORE_DB
  Object: [dbo].[SOD_WO_Conf]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_WO_Conf]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_WO_Conf].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_WO_Conf](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Password] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[LimitEntry] [int] NULL,
	[PriceLevel] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[StoreID] [int] NULL,
	[AllowPriceChange] [bit] NULL,
	[Password2] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CustType] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Password3] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_SOD_WO_Conf] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)';
GO
