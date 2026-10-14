/* SQL Server 2016 RTM. Run through the updater or inside an explicit transaction.
   Replaces the custom history IDENTITY with a sequence-backed EventID, preserving
   all existing events and their IDs. RMS uses @@IDENTITY: history must not change it.
   No RMS business table, transaction link, amount, or inventory value is edited.
   Test a restore and back up first. Unsupported custom history schemas stop. */
SET XACT_ABORT ON;
IF @@TRANCOUNT=0 OR XACT_STATE()<>1
    THROW 52400, 'Run the history identity fix inside the updater transaction.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_History') IS NULL RETURN;
IF OBJECT_ID(N'dbo.SOD_WO_History',N'U') IS NULL
    THROW 52401, 'SOD_WO_History is not a table. Review it before migration.', 1;

DECLARE @history int=OBJECT_ID(N'dbo.SOD_WO_History'), @rows bigint, @max bigint;
SELECT @rows=COUNT_BIG(*),@max=ISNULL(MAX(EventID),0)
FROM dbo.SOD_WO_History WITH(TABLOCKX,HOLDLOCK);
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id=@history)<>8 OR EXISTS (
    SELECT name,type_name,max_length,is_nullable FROM (VALUES
    (N'EventID',N'bigint',8,0),(N'OrderID',N'int',4,0),(N'EventTime',N'datetime2',7,0),
    (N'EventName',N'nvarchar',120,0),(N'Actor',N'nvarchar',256,0),
    (N'Terminal',N'nvarchar',512,0),(N'ItemID',N'int',4,1),(N'Details',N'nvarchar',4000,0)
    ) expected(name,type_name,max_length,is_nullable)
    EXCEPT SELECT name,TYPE_NAME(user_type_id),max_length,is_nullable FROM sys.columns WHERE object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=@history AND (is_computed=1 OR (name=N'EventTime' AND scale<>3)))
    THROW 52402, 'History schema is incompatible; existing events were preserved.', 1;

IF COLUMNPROPERTY(@history,N'EventID','IsIdentity')=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.sequences WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_History_EventSequence')
                  AND system_type_id=127 AND CONVERT(bigint,increment)=1 AND is_cycling=0
                  AND CONVERT(bigint,current_value)>=@max)
       OR NOT EXISTS(SELECT 1 FROM sys.sql_expression_dependencies
                     WHERE referencing_id=(SELECT default_object_id FROM sys.columns WHERE object_id=@history AND name=N'EventID')
                       AND referenced_id=OBJECT_ID(N'dbo.SOD_WO_History_EventSequence'))
        THROW 52403, 'Non-identity history needs its approved EventSequence default. Review the schema.', 1;
    RETURN;
END;

-- Changing IDENTITY requires rebuilding this custom table. Reject dependencies or
-- customizations that cannot be safely retained by this focused migration.
IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=@history OR referenced_object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.sql_expression_dependencies WHERE referenced_id=@history AND is_schema_bound_reference=1)
 OR EXISTS(SELECT 1 FROM sys.triggers WHERE parent_id=@history)
 OR EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=@history)
 OR EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=1 AND major_id=@history)
 OR EXISTS(SELECT 1 FROM sys.objects WHERE object_id=@history AND principal_id IS NOT NULL)
 OR EXISTS(SELECT 1 FROM sys.tables WHERE object_id=@history AND (temporal_type<>0 OR is_memory_optimized=1 OR is_tracked_by_cdc=1))
 OR EXISTS(SELECT 1 FROM sys.columns WHERE object_id=@history AND collation_name IS NOT NULL
            AND collation_name<>CONVERT(sysname,DATABASEPROPERTYEX(DB_NAME(),'Collation')))
 OR EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND index_id>0
            AND (name NOT IN(N'PK_SOD_WO_History',N'IX_SOD_WO_History_Order') OR data_space_id<>FILEGROUP_ID(N'PRIMARY') OR has_filter=1 OR is_disabled=1))
 OR NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND name=N'PK_SOD_WO_History' AND is_primary_key=1 AND type=1)
 OR (SELECT COUNT(*) FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
     WHERE ix.object_id=@history AND ix.name=N'PK_SOD_WO_History' AND ic.key_ordinal=1 AND c.name=N'EventID')<>1
 OR EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     WHERE ix.object_id=@history AND ix.name=N'PK_SOD_WO_History' AND (ic.key_ordinal>1 OR ic.is_descending_key=1))
 OR EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=@history AND name=N'IX_SOD_WO_History_Order' AND (type<>2 OR is_unique=1))
 OR EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id
     JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
     WHERE ix.object_id=@history AND ix.name=N'IX_SOD_WO_History_Order'
       AND (ic.is_included_column=1 OR ic.key_ordinal>2 OR (ic.key_ordinal=1 AND (c.name<>N'OrderID' OR ic.is_descending_key<>0))
            OR (ic.key_ordinal=2 AND (c.name<>N'EventID' OR ic.is_descending_key<>1))))
 OR EXISTS(SELECT 1 FROM sys.indexes ix WHERE ix.object_id=@history AND ix.name=N'IX_SOD_WO_History_Order'
     AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id=ix.object_id AND ic.index_id=ix.index_id AND ic.key_ordinal>0)<>2)
 OR EXISTS(SELECT 1 FROM sys.default_constraints WHERE parent_object_id=@history AND name<>N'DF_SOD_WO_History_Time')
    THROW 52404, 'History has custom dependencies, ownership, collation, or indexes. Review before rebuilding.', 1;
IF OBJECT_ID(N'dbo.SOD_WO_History_RmsSafe') IS NOT NULL
 OR OBJECT_ID(N'dbo.SOD_WO_History_EventSequence') IS NOT NULL
 OR @max>=9223372036854775807
    THROW 52405, 'History migration object conflict or event number limit. Review before migration.', 1;

DECLARE @permissions TABLE(principal_name sysname,grantor_name sysname,permission_name sysname,state char(1),column_name sysname NULL);
INSERT @permissions
SELECT USER_NAME(p.grantee_principal_id),USER_NAME(p.grantor_principal_id),p.permission_name,p.state,c.name
FROM sys.database_permissions p LEFT JOIN sys.columns c ON c.object_id=p.major_id AND c.column_id=p.minor_id
WHERE p.class=1 AND p.major_id=@history;
DECLARE @timeDefault nvarchar(max)=(SELECT definition FROM sys.default_constraints WHERE parent_object_id=@history AND name=N'DF_SOD_WO_History_Time');
IF @timeDefault IS NULL THROW 52406, 'The expected history time default is missing.', 1;
DECLARE @ddl nvarchar(max)=N'CREATE SEQUENCE dbo.SOD_WO_History_EventSequence AS bigint START WITH '+CONVERT(nvarchar(30),@max+1)+N' INCREMENT BY 1 MINVALUE 1 NO CYCLE CACHE 100;';
EXEC sys.sp_executesql @ddl;
SET @ddl=N'CREATE TABLE dbo.SOD_WO_History_RmsSafe (
 EventID bigint NOT NULL CONSTRAINT DF_SOD_WO_History_EventID DEFAULT(NEXT VALUE FOR dbo.SOD_WO_History_EventSequence),
 OrderID int NOT NULL,EventTime datetime2(3) NOT NULL CONSTRAINT DF_SOD_WO_History_Time_RmsSafe DEFAULT '+@timeDefault+N',
 EventName nvarchar(60) NOT NULL,Actor nvarchar(128) NOT NULL,Terminal nvarchar(256) NOT NULL,
 ItemID int NULL,Details nvarchar(2000) NOT NULL,
 CONSTRAINT PK_SOD_WO_History_RmsSafe PRIMARY KEY CLUSTERED(EventID));';
EXEC sys.sp_executesql @ddl;
INSERT dbo.SOD_WO_History_RmsSafe(EventID,OrderID,EventTime,EventName,Actor,Terminal,ItemID,Details)
SELECT EventID,OrderID,EventTime,EventName,Actor,Terminal,ItemID,Details FROM dbo.SOD_WO_History;
IF (SELECT COUNT_BIG(*) FROM dbo.SOD_WO_History_RmsSafe)<>@rows
 OR EXISTS(SELECT * FROM dbo.SOD_WO_History EXCEPT SELECT * FROM dbo.SOD_WO_History_RmsSafe)
 OR EXISTS(SELECT * FROM dbo.SOD_WO_History_RmsSafe EXCEPT SELECT * FROM dbo.SOD_WO_History)
    THROW 52407, 'History copy verification failed. Roll back this plan.', 1;
CREATE INDEX IX_SOD_WO_History_Order ON dbo.SOD_WO_History_RmsSafe(OrderID,EventID DESC);
DROP TABLE dbo.SOD_WO_History;
EXEC sys.sp_rename N'dbo.SOD_WO_History_RmsSafe',N'SOD_WO_History';
EXEC sys.sp_rename N'dbo.PK_SOD_WO_History_RmsSafe',N'PK_SOD_WO_History',N'OBJECT';
EXEC sys.sp_rename N'dbo.DF_SOD_WO_History_Time_RmsSafe',N'DF_SOD_WO_History_Time',N'OBJECT';

-- Reapply object/column permissions with their original grantors.
DECLARE @principal sysname,@grantor sysname,@permission sysname,@state char(1),@column sysname;
DECLARE history_permissions CURSOR LOCAL FAST_FORWARD FOR
 SELECT principal_name,grantor_name,permission_name,state,column_name FROM @permissions ORDER BY CASE WHEN column_name IS NULL THEN 0 ELSE 1 END;
OPEN history_permissions;
FETCH NEXT FROM history_permissions INTO @principal,@grantor,@permission,@state,@column;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @ddl=CASE @state WHEN 'D' THEN N'DENY ' WHEN 'R' THEN N'REVOKE ' ELSE N'GRANT ' END+@permission+
      N' ON OBJECT::dbo.SOD_WO_History'+CASE WHEN @column IS NULL THEN N'' ELSE N' ('+QUOTENAME(@column)+N')' END+
      CASE WHEN @state='R' THEN N' FROM ' ELSE N' TO ' END+QUOTENAME(@principal)+
      CASE WHEN @state='W' THEN N' WITH GRANT OPTION' ELSE N'' END+N' AS '+QUOTENAME(@grantor)+N';';
    EXEC sys.sp_executesql @ddl;
    FETCH NEXT FROM history_permissions INTO @principal,@grantor,@permission,@state,@column;
END;
CLOSE history_permissions;
DEALLOCATE history_permissions;
IF COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')<>0
 OR (SELECT COUNT_BIG(*) FROM dbo.SOD_WO_History)<>@rows
    THROW 52408, 'History identity fix verification failed. Roll back this plan.', 1;
