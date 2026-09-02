/*
  Creates a read-only reporting view for the RMS Item Value List export.
  It returns a value for every display column, including a single space
  when Department, Category, or Supplier is missing. This prevents the
  legacy RMS CSV exporter from collapsing fields to the left.

  Run this script once in the target branch RMS database.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects
     WHERE object_id = OBJECT_ID(N'[dbo].[SOD_ViewItemValueListExport]'));

IF @ExistingType IS NOT NULL AND @ExistingType <> 'V '
    THROW 52051, 'Object type conflict for [dbo].[SOD_ViewItemValueListExport].', 1;

IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'
        CREATE VIEW [dbo].[SOD_ViewItemValueListExport]
        AS
        SELECT CAST(1 AS int) AS Placeholder;';
GO

ALTER VIEW [dbo].[SOD_ViewItemValueListExport]
AS
SELECT
    ISNULL(d.Name, ' ') AS DepartmentName,
    ISNULL(c.Name, ' ') AS CategoryName,
    ISNULL(s.SupplierName, ' ') AS SupplierName,
    i.ItemLookupCode,
    i.Description,
    i.BinLocation,
    i.Quantity,
    i.QuantityCommitted,
    i.Cost,
    i.Price,
    i.PriceA,
    i.PriceB,
    i.PriceC,
    i.MSRP,
    i.PriceLowerBound,
    i.PriceUpperBound,
    i.SalePrice,
    i.SaleStartDate,
    i.SaleEndDate,
    i.ReorderPoint,
    i.RestockLevel,
    i.LastSold,
    i.Cost * i.Quantity AS ExtendedCost,
    i.Price * i.Quantity AS ExtendedPrice,
    i.Inactive
FROM dbo.Item AS i WITH (NOLOCK)
LEFT JOIN dbo.Department AS d WITH (NOLOCK) ON i.DepartmentID = d.ID
LEFT JOIN dbo.Category AS c WITH (NOLOCK) ON i.CategoryID = c.ID
LEFT JOIN dbo.Supplier AS s WITH (NOLOCK) ON i.SupplierID = s.ID;
GO

IF COL_LENGTH(N'dbo.SOD_ViewItemValueListExport', N'ItemLookupCode') IS NULL
    THROW 52052, 'View validation failed: ItemLookupCode is missing.', 1;

IF COL_LENGTH(N'dbo.SOD_ViewItemValueListExport', N'DepartmentName') IS NULL
    THROW 52053, 'View validation failed: DepartmentName is missing.', 1;
GO
