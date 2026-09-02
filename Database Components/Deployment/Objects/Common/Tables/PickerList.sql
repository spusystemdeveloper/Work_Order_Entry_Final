/*
  Work Order Entry / Order Processing Queueing table
  Read-only source export: DVO_STORE_DB
  Object: [dbo].[PickerList]
  Creates the table only when it is missing. Existing schemas are validated separately and are not altered automatically.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PickerList]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'U '
    THROW 52000, 'Object type conflict for [dbo].[PickerList].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[PickerList](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Initial] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Name] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[RegisterNo] [int] NULL,
	[Password] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Active] [int] NULL,
 CONSTRAINT [PK_PickerList] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)';
GO
