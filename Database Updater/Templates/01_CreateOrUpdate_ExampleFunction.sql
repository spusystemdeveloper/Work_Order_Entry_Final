/* TEST RESTORE EXAMPLE. Creates/updates a demonstration function only.
   Copy and adapt its name, interface, body, and verification for real work.
   No USE or transaction commands: the updater supplies the target/transaction. */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @ExistingType char(2) =
    (SELECT type FROM sys.objects WHERE object_id=OBJECT_ID(N'dbo.SOD_fn_UpdaterExample'));
IF @ExistingType IS NOT NULL AND @ExistingType <> 'FN'
    THROW 52310, 'SOD_fn_UpdaterExample exists with an incompatible object type.', 1;
IF @ExistingType IS NULL
    EXEC sys.sp_executesql N'CREATE FUNCTION dbo.SOD_fn_UpdaterExample(@Value int)
    RETURNS int AS BEGIN RETURN @Value; END;';
GO
ALTER FUNCTION dbo.SOD_fn_UpdaterExample(@Value int)
RETURNS int
AS
BEGIN
    RETURN @Value * 2;
END;
GO
IF ISNULL(dbo.SOD_fn_UpdaterExample(3), -1) <> 6
    THROW 52311, 'Example function verification failed.', 1;
