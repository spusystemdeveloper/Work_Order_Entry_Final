/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[SOD_Item_Logs]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SOD_Item_Logs]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[SOD_Item_Logs].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[SOD_Item_Logs](
	[ItemID] [int] NOT NULL,
	[ItemLookupCode] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Type] [varchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ReferenceID] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[QuantityORIS] [int] NOT NULL,
	[LastQuantity] [int] NOT NULL,
	[NewQuantity] [int] NOT NULL,
	[LastCommitted] [int] NOT NULL,
	[NewCommitted] [int] NOT NULL,
	[Date] [datetime] NOT NULL
)';
GO
