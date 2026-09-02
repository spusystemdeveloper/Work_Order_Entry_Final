/*
  Read-only preflight for Work Order Entry / Order Processing Queueing.
  Run in the intended RMS store database.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 53000, 'Run this preflight in an RMS store database.', 1;

DECLARE @BaseObjects TABLE (ObjectName sysname, ExpectedType char(2));
INSERT @BaseObjects VALUES
    (N'Configuration', N'U'), (N'Customer', N'U'), (N'Item', N'U'),
    (N'Order', N'U'), (N'OrderEntry', N'U'), (N'ReasonCode', N'U'),
    (N'SalesRep', N'U'), (N'Store', N'U'), (N'Transaction', N'U'),
    (N'TransactionEntry', N'U');

IF EXISTS
(
    SELECT 1
    FROM @BaseObjects AS expected
    LEFT JOIN sys.objects AS actual
      ON actual.name = expected.ObjectName
     AND SCHEMA_NAME(actual.schema_id) = N'dbo'
    WHERE actual.object_id IS NULL
       OR actual.type COLLATE DATABASE_DEFAULT <> expected.ExpectedType
)
BEGIN
    SELECT expected.ObjectName, expected.ExpectedType, actual.type AS ActualType
    FROM @BaseObjects AS expected
    LEFT JOIN sys.objects AS actual
      ON actual.name = expected.ObjectName
     AND SCHEMA_NAME(actual.schema_id) = N'dbo'
    WHERE actual.object_id IS NULL
       OR actual.type COLLATE DATABASE_DEFAULT <> expected.ExpectedType;
    THROW 53001, 'Required RMS base objects are missing or have unexpected types.', 1;
END;

IF
(
    SELECT COUNT(*)
    FROM dbo.ReasonCode
    WHERE CONVERT(nvarchar(100), Code) = N'701' AND Type = 6
) <> 1
    THROW 53002, 'Reason code 701, type 6, must exist exactly once.', 1;

SELECT
    CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)) AS ServerName,
    DB_NAME() AS DatabaseName,
    CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128)) AS ProductVersion,
    N'PASS' AS PreflightResult;
GO
