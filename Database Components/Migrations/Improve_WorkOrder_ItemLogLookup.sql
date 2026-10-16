/* Optional SQL Server 2016 item-log lookup index. No triggers or business rows
   are changed. Test on a branch restore and schedule a maintenance window.
   The updater supplies the transaction. In SSMS explicitly BEGIN TRANSACTION
   before this script, inspect its result, then COMMIT or ROLLBACK.
   ReferenceID is varchar(50), compared with an int by the legacy trigger.
   Preserve that comparison: seek ItemID, read newest first, filter ReferenceID.
*/
IF @@TRANCOUNT = 0
    THROW 53300, 'An explicit transaction is required. Use the updater or follow WORKORDER_ITEM_LOG_LOOKUP_GUIDE.md.', 1;

DECLARE @LogTable int = OBJECT_ID(N'dbo.SOD_Item_Logs', N'U');
IF @LogTable IS NULL
    THROW 53301, 'dbo.SOD_Item_Logs is missing. Review this branch before installing the optional index.', 1;
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=@LogTable AND is_nullable=0
    AND user_type_id=system_type_id AND
    ((name IN (N'ItemID',N'NewCommitted') AND system_type_id=56)
     OR (name=N'ReferenceID' AND system_type_id=167 AND max_length=50)
     OR (name=N'Date' AND system_type_id=61))) <> 4
    THROW 53302, 'Item-log column types differ from reviewed int/varchar(50)/datetime columns. No index created.', 1;

-- Serialize the check and creation, including when called in an updater batch.
-- A table lock also keeps the tie/reference checks consistent until commit.
DECLARE @LockProbe int;
SELECT TOP (1) @LockProbe=ItemID FROM dbo.SOD_Item_Logs WITH (TABLOCKX,HOLDLOCK);

DECLARE @Usable TABLE (IndexID int PRIMARY KEY);
INSERT @Usable(IndexID)
SELECT ix.index_id FROM sys.indexes ix
WHERE ix.object_id=@LogTable AND ix.type=2 AND ix.is_disabled=0
  AND ix.is_hypothetical=0 AND ix.has_filter=0
  AND EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.columns c
      ON c.object_id=ic.object_id AND c.column_id=ic.column_id
      WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id
        AND ic.key_ordinal=1 AND c.name=N'ItemID')
  AND EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.columns c
      ON c.object_id=ic.object_id AND c.column_id=ic.column_id
      WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id
        AND ic.key_ordinal=2 AND c.name=N'Date')
  AND NOT EXISTS(SELECT wanted.name FROM (VALUES(N'ReferenceID'),(N'NewCommitted')) wanted(name)
      WHERE NOT EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.columns c
          ON c.object_id=ic.object_id AND c.column_id=ic.column_id
          WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id AND c.name=wanted.name));

IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@LogTable
    AND name=N'IX_SOD_WO_ItemLogLookup' AND index_id NOT IN(SELECT IndexID FROM @Usable))
    THROW 53303, 'IX_SOD_WO_ItemLogLookup has another definition or is disabled/filtered. Review it; nothing is replaced.', 1;
IF NOT EXISTS(SELECT 1 FROM @Usable)
BEGIN

-- An index can change which rows are visited. Do not conceal legacy conversion
-- errors or choose a new result for TOP(1) where newest timestamps are tied.
IF EXISTS(SELECT 1 FROM dbo.SOD_Item_Logs WHERE TRY_CONVERT(int,ReferenceID) IS NULL)
    THROW 53304, 'Non-integer/overflow item-log references need branch review. Existing rows were not changed.', 1;
DECLARE @Ambiguous bit=0;
;WITH Ranked AS (
    SELECT ItemID,TRY_CONVERT(int,ReferenceID) AS NumericReferenceID,NewCommitted,
        DENSE_RANK() OVER(PARTITION BY ItemID,TRY_CONVERT(int,ReferenceID)
                          ORDER BY [Date] DESC) AS LatestRank
    FROM dbo.SOD_Item_Logs
)
SELECT TOP (1) @Ambiguous=1 FROM Ranked WHERE LatestRank=1
GROUP BY ItemID,NumericReferenceID HAVING MIN(NewCommitted)<>MAX(NewCommitted);
IF @Ambiguous=1
    THROW 53305, 'Latest item-log timestamps tie with different commitment values. Review the ambiguous history before indexing; no tie-break rule was invented.', 1;

CREATE NONCLUSTERED INDEX IX_SOD_WO_ItemLogLookup
    ON dbo.SOD_Item_Logs(ItemID,[Date] DESC) INCLUDE(ReferenceID,NewCommitted);
SELECT N'CREATED: IX_SOD_WO_ItemLogLookup. Trigger definitions, comparisons and log data are unchanged; commit or roll back the enclosing transaction.' Result;
END
ELSE
    SELECT N'REUSED: an enabled, unfiltered covering ItemID/Date index already exists.' Result;
