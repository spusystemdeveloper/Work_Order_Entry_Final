/*
  Work Order Entry / Order Processing Queueing table
  Portable baseline for RMS store databases
  Object: [dbo].[SOD_WO_LOGS]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_WO_LOGS]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_WO_LOGS].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_WO_LOGS](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[ItemID] [int] NULL,
	[FullPrice] [money] NULL,
	[Price] [money] NULL,
	[Pricea] [money] NULL,
	[Priceb] [money] NULL,
	[Pricec] [money] NULL,
	[LastUpdatedPrice] [datetime] NULL,
	[UpdatedPrice] [money] NULL,
	[ApprovedUser] [nvarchar](50) NULL,
	[Status] [nvarchar](50) NULL,
	[Date] [datetime] NULL,
	[timestamp] [timestamp] NULL,
	[OrderTaker] [nvarchar](50) NULL,
	[CustomerPriceLevel] [int] NULL,
 CONSTRAINT [PK_SOD_WO_LOGS] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)';
GO
