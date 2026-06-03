USE [SBD_CEBU_MAIN_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[SOD_WO_DraftHeader]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SOD_WO_DraftHeader](
        [DraftID] [int] IDENTITY(1,1) NOT NULL,
        [RegisterID] [nvarchar](50) NOT NULL,
        [UserName] [nvarchar](100) NOT NULL,
        [EntryType] [int] NULL,
        [CustomerID] [int] NULL,
        [CustomerName] [nvarchar](255) NULL,
        [BusinessCustomerType] [nvarchar](100) NULL,
        [PaymentType] [nvarchar](100) NULL,
        [SalesRepID] [int] NULL,
        [SalesRepName] [nvarchar](255) NULL,
        [Remarks] [nvarchar](max) NULL,
        [ReleaseType] [nvarchar](50) NULL,
        [PriceLevel] [nvarchar](50) NULL,
        [TaxExempt] [bit] NOT NULL CONSTRAINT [DF_SOD_WO_DraftHeader_TaxExempt] DEFAULT ((0)),
        [ZeroRated] [bit] NOT NULL CONSTRAINT [DF_SOD_WO_DraftHeader_ZeroRated] DEFAULT ((0)),
        [CreatedAt] [datetime] NOT NULL CONSTRAINT [DF_SOD_WO_DraftHeader_CreatedAt] DEFAULT (getdate()),
        [UpdatedAt] [datetime] NOT NULL CONSTRAINT [DF_SOD_WO_DraftHeader_UpdatedAt] DEFAULT (getdate()),
        CONSTRAINT [PK_SOD_WO_DraftHeader] PRIMARY KEY CLUSTERED ([DraftID] ASC),
        CONSTRAINT [UQ_SOD_WO_DraftHeader_RegisterUser] UNIQUE NONCLUSTERED ([RegisterID] ASC, [UserName] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
