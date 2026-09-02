/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_WO]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_WO]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_WO].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_WO](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderID] [bigint] NULL,
	[ItemID] [bigint] NULL,
	[QtyOrder] [int] NULL,
	[Status] [int] NULL,
	[Invoice] [int] NULL,
	[StockIn] [int] NULL,
	[TII] [bigint] NULL,
	[Cancel] [int] NULL,
	[TransactionNumber] [bigint] NULL,
	[DRStatus] [int] NULL,
 CONSTRAINT [PK_SOD_WO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)';
GO
