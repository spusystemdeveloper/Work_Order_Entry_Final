/* Read-only review of the SELECT TOP(1) NewCommitted ... ORDER BY [Date] DESC
   ambiguity. Run only on an explicitly authorized target, preferably an isolated
   restore. Source tables are SELECT-only; intermediate tables are in tempdb.
   Full history scans/sorts can be expensive. This script does not fix history,
   choose a tie-breaker, execute correction procedures or install an index.
*/
SET NOCOUNT ON;
SET LOCK_TIMEOUT 1000;
IF OBJECT_ID(N'dbo.SOD_Item_Logs',N'U') IS NULL
    THROW 53730, 'Item log table is missing.', 1;

SELECT DB_NAME() DatabaseName,COUNT_BIG(*) LogRows,
 SUM(CONVERT(bigint,CASE WHEN TRY_CAST(ReferenceID AS int) IS NULL THEN 1 ELSE 0 END)) InvalidIntegerReferences
FROM dbo.SOD_Item_Logs;

;WITH Ranked AS(
 SELECT ItemID,TRY_CAST(ReferenceID AS int) NumericReferenceID,NewCommitted,[Date],
  DENSE_RANK() OVER(PARTITION BY ItemID,TRY_CAST(ReferenceID AS int) ORDER BY [Date] DESC) LatestRank
 FROM dbo.SOD_Item_Logs
)
SELECT ItemID,NumericReferenceID,MAX([Date]) LatestDate,COUNT_BIG(*) TiedRows,
 MIN(NewCommitted) MinCommitted,MAX(NewCommitted) MaxCommitted
INTO #AmbiguousItemLogs
FROM Ranked WHERE LatestRank=1
GROUP BY ItemID,NumericReferenceID HAVING MIN(NewCommitted)<>MAX(NewCommitted)
OPTION(MAXDOP 1,MAX_GRANT_PERCENT=1);

SELECT COUNT_BIG(*) AmbiguousPairs,COUNT(DISTINCT ItemID) AffectedItems,
 SUM(TiedRows) LatestTiedRows,MIN(LatestDate) EarliestLatestDate,MAX(LatestDate) NewestLatestDate,
 SUM(CONVERT(bigint,CASE WHEN CONVERT(time,LatestDate)='00:00:00' THEN 1 ELSE 0 END)) MidnightPairs
FROM #AmbiguousItemLogs;

-- A matching order number does not prove a log belongs to that order: transaction,
-- transfer and order references share this untyped numeric ReferenceID field.
SELECT CASE WHEN o.ID IS NULL THEN 'No matching Order'
 WHEN o.Closed=1 THEN 'Closed Order number'
 WHEN o.Type=2 THEN 'Open WO number'
 ELSE 'Other open Order number' END ReferenceNumberState,COUNT_BIG(*) AmbiguousPairs
FROM #AmbiguousItemLogs a LEFT JOIN dbo.[Order] o ON o.ID=a.NumericReferenceID
GROUP BY CASE WHEN o.ID IS NULL THEN 'No matching Order'
 WHEN o.Closed=1 THEN 'Closed Order number'
 WHEN o.Type=2 THEN 'Open WO number' ELSE 'Other open Order number' END;

SELECT CONVERT(nvarchar(100),l.[Type]) LogType,COUNT_BIG(*) LatestTiedRows
FROM dbo.SOD_Item_Logs l JOIN #AmbiguousItemLogs a
 ON a.ItemID=l.ItemID AND a.NumericReferenceID=TRY_CAST(l.ReferenceID AS int) AND a.LatestDate=l.[Date]
GROUP BY CONVERT(nvarchar(100),l.[Type]) OPTION(MAXDOP 1,MAX_GRANT_PERCENT=1);

SELECT TOP(20) a.*,o.Type MatchingOrderType,o.Closed MatchingOrderClosed
FROM #AmbiguousItemLogs a LEFT JOIN dbo.[Order] o ON o.ID=a.NumericReferenceID
ORDER BY CASE WHEN o.Closed=0 AND o.Type=2 THEN 0 ELSE 1 END,a.ItemID,a.NumericReferenceID;

-- Leave only intermediate state scoped to this connection.
DROP TABLE #AmbiguousItemLogs;
