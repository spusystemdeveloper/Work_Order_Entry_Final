/*
    Work Order Entry / Order Processing Queueing
    Safe portable SQL-function migration generator

    HOW TO USE
      1. Run this read-only generator in a reviewed source RMS database.
      2. Set @TargetDatabase to the database name used on the target server.
      3. Save result set 1, column DeploymentScript, as a .sql file.
      4. Review result sets 2-4, especially cross-database dependencies.
      5. Run the generated file on the target server with @DryRun = 1.
      6. Review its migration plan and rollback output.
      7. Change @DryRun to 0 and run it again during an approved deployment.

    SAFETY CHARACTERISTICS
      - The generator does not modify the source database.
      - The generated deployment defaults to dry-run.
      - Same definitions are skipped.
      - Missing functions are created.
      - Different existing functions are altered, preserving object IDs and
        explicit permissions.
      - Conflicting object/function types stop deployment.
      - Local and external dependencies are checked before DDL.
      - DDL is transactional and rolls back on error.
      - Existing definitions are returned as rollback commands.

    LIMITS
      - Store-specific definitions are copied exactly. Hard-coded database,
        store ID, warehouse, or branch references must be reviewed.
      - Tables, views, procedures, data, credentials, and permissions for newly
        created functions are not copied automatically.
      - If a function signature or function kind must change, resolve that as a
        separately reviewed migration instead of automatically dropping it.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetDatabase sysname = N'CHANGE_ME';
DECLARE @IncludeStoreSpecificFunctions bit = 1;
DECLARE @MigrationId nvarchar(100) = N'WOE-OPQ-Functions-20260723-02';

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50000, 'Run this generator in the source RMS store database, not a system database.', 1;

IF @TargetDatabase = N'CHANGE_ME' OR NULLIF(LTRIM(RTRIM(@TargetDatabase)), N'') IS NULL
    THROW 50001, 'Set @TargetDatabase before running the generator.', 1;

CREATE TABLE #FunctionManifest
(
    CreateOrder int NOT NULL PRIMARY KEY,
    SchemaName sysname NOT NULL,
    FunctionName sysname NOT NULL,
    IsStoreSpecific bit NOT NULL,
    Purpose nvarchar(300) NOT NULL
);

/* SOD_fn_GetDiscQty is an indirect dependency of SOD_fntbl_SearchItem. */
INSERT #FunctionManifest (CreateOrder, SchemaName, FunctionName, IsStoreSpecific, Purpose)
VALUES
    (10,  N'dbo', N'SOD_fn_GetDiscQty',             0, N'Quantity-discount dependency used by item search.'),
    (20,  N'dbo', N'SOD_fn_GetAvailableQty',        0, N'Available quantity for converted SKU levels.'),
    (30,  N'dbo', N'SOD_fn_GetConvertQty',          0, N'SKU parent-quantity conversion.'),
    (40,  N'dbo', N'SOD_fn_GetReg',                 0, N'Register lookup from workstation IP mapping.'),
    (50,  N'dbo', N'SOD_fn_GetCreditLimit',         1, N'Store-family accounts-receivable lookup.'),
    (60,  N'dbo', N'SOD_fn_GetTotalOpenWorkOrder',  1, N'Store-family open-work-order total.'),
    (70,  N'dbo', N'SOD_fn_GetQty',                 1, N'Explicit warehouse database and store-ID lookup.'),
    (100, N'dbo', N'SOD_fntbl_NestedSearchItem',    0, N'Nested keyword search.'),
    (110, N'dbo', N'SOD_fntbl_PriceLevel',          0, N'RMS item price-level lookup.'),
    (120, N'dbo', N'SOD_fntbl_SearchItem',          0, N'Work Order Entry item search.'),
    (130, N'dbo', N'SOD_fntbl_LastupdateOrder',     0, N'Recent open orders for queue refresh.'),
    (140, N'dbo', N'SOD_fntbl_CheckOpenWO',         0, N'Open work orders for an item.'),
    (150, N'dbo', N'SOD_fntbl_WoEntry',             0, N'Work-order and completed-sale print lines.'),
    (160, N'dbo', N'SOD_fntbl_WoDetails',           1, N'Work-order print header and source branch label.'),
    (170, N'dbo', N'SOD_fntbl_CheckQty',            1, N'Cross-database store quantity lookup.'),
    (180, N'dbo', N'SOD_fntbl_CheckExQty',          1, N'Cross-database extended quantity lookup.');

IF @IncludeStoreSpecificFunctions = 0
    DELETE FROM #FunctionManifest WHERE IsStoreSpecific = 1;

CREATE TABLE #Modules
(
    CreateOrder int NOT NULL PRIMARY KEY,
    SchemaName sysname NOT NULL,
    FunctionName sysname NOT NULL,
    SourceType char(2) NULL,
    IsStoreSpecific bit NOT NULL,
    Definition nvarchar(max) NULL,
    AlterDefinition nvarchar(max) NULL
);

INSERT #Modules
(
    CreateOrder,
    SchemaName,
    FunctionName,
    SourceType,
    IsStoreSpecific,
    Definition
)
SELECT
    f.CreateOrder,
    f.SchemaName,
    f.FunctionName,
    o.type,
    f.IsStoreSpecific,
    m.definition
FROM #FunctionManifest AS f
LEFT JOIN sys.objects AS o
    ON o.name = f.FunctionName
   AND SCHEMA_NAME(o.schema_id) = f.SchemaName
   AND o.type IN (N'FN', N'IF', N'TF')
LEFT JOIN sys.sql_modules AS m ON m.object_id = o.object_id;

DECLARE @MissingFunctions nvarchar(max);

SELECT @MissingFunctions = STUFF
(
    (
        SELECT N', ' + QUOTENAME(SchemaName) + N'.' + QUOTENAME(FunctionName)
        FROM #Modules
        WHERE Definition IS NULL
        ORDER BY CreateOrder
        FOR XML PATH(N''), TYPE
    ).value(N'.', N'nvarchar(max)'),
    1,
    2,
    N''
);

IF @MissingFunctions IS NOT NULL
BEGIN
    DECLARE @MissingMessage nvarchar(2048) =
        N'The source database is missing required functions: ' + @MissingFunctions;
    THROW 50002, @MissingMessage, 1;
END;

/* Build ALTER definitions without changing the stored source definition. */
DECLARE
    @ModuleOrder int,
    @ModuleDefinition nvarchar(max),
    @CreatePosition int;

DECLARE alter_definition_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT CreateOrder, Definition FROM #Modules ORDER BY CreateOrder;

OPEN alter_definition_cursor;
FETCH NEXT FROM alter_definition_cursor INTO @ModuleOrder, @ModuleDefinition;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @CreatePosition = CHARINDEX(N'CREATE FUNCTION', UPPER(@ModuleDefinition));

    IF @CreatePosition = 0
        THROW 50003, 'A source module does not contain a CREATE statement.', 1;

    UPDATE #Modules
    SET AlterDefinition = STUFF(@ModuleDefinition, @CreatePosition, 6, N'ALTER')
    WHERE CreateOrder = @ModuleOrder;

    FETCH NEXT FROM alter_definition_cursor INTO @ModuleOrder, @ModuleDefinition;
END;

CLOSE alter_definition_cursor;
DEALLOCATE alter_definition_cursor;

CREATE TABLE #ExternalDependencies
(
    ReferencingFunction sysname NOT NULL,
    ReferencedDatabase sysname NOT NULL,
    ReferencedSchema sysname NULL,
    ReferencedEntity sysname NULL
);

INSERT #ExternalDependencies
(
    ReferencingFunction,
    ReferencedDatabase,
    ReferencedSchema,
    ReferencedEntity
)
SELECT DISTINCT
    OBJECT_NAME(d.referencing_id),
    d.referenced_database_name,
    d.referenced_schema_name,
    d.referenced_entity_name
FROM sys.sql_expression_dependencies AS d
INNER JOIN #Modules AS m
    ON m.FunctionName = OBJECT_NAME(d.referencing_id)
   AND m.SchemaName = OBJECT_SCHEMA_NAME(d.referencing_id)
WHERE d.referenced_database_name IS NOT NULL
  AND d.referenced_database_name <> DB_NAME();

CREATE TABLE #LocalDependencies
(
    ReferencingFunction sysname NOT NULL,
    ReferencedSchema sysname NOT NULL,
    ReferencedEntity sysname NOT NULL,
    ReferencedColumn sysname NULL
);

INSERT #LocalDependencies
(
    ReferencingFunction,
    ReferencedSchema,
    ReferencedEntity,
    ReferencedColumn
)
SELECT DISTINCT
    OBJECT_NAME(d.referencing_id),
    COALESCE(d.referenced_schema_name, N'dbo'),
    d.referenced_entity_name,
    CASE
        WHEN d.referenced_id IS NOT NULL AND d.referenced_minor_id > 0
        THEN COL_NAME(d.referenced_id, d.referenced_minor_id)
        ELSE NULL
    END
FROM sys.sql_expression_dependencies AS d
INNER JOIN #Modules AS m
    ON m.FunctionName = OBJECT_NAME(d.referencing_id)
   AND m.SchemaName = OBJECT_SCHEMA_NAME(d.referencing_id)
LEFT JOIN #Modules AS selectedFunction
    ON selectedFunction.FunctionName = d.referenced_entity_name
   AND selectedFunction.SchemaName = COALESCE(d.referenced_schema_name, N'dbo')
WHERE (d.referenced_database_name IS NULL OR d.referenced_database_name = DB_NAME())
  AND d.referenced_entity_name IS NOT NULL
  AND selectedFunction.FunctionName IS NULL;

CREATE TABLE #SourcePermissions
(
    SchemaName sysname NOT NULL,
    FunctionName sysname NOT NULL,
    PrincipalName sysname NOT NULL,
    PermissionName nvarchar(60) NOT NULL,
    PermissionState char(1) NOT NULL
);

INSERT #SourcePermissions
(
    SchemaName,
    FunctionName,
    PrincipalName,
    PermissionName,
    PermissionState
)
SELECT
    OBJECT_SCHEMA_NAME(p.major_id),
    OBJECT_NAME(p.major_id),
    USER_NAME(p.grantee_principal_id),
    p.permission_name,
    p.state
FROM sys.database_permissions AS p
INNER JOIN #Modules AS m
    ON m.FunctionName = OBJECT_NAME(p.major_id)
   AND m.SchemaName = OBJECT_SCHEMA_NAME(p.major_id)
WHERE p.class = 1
  AND p.minor_id = 0;

DECLARE @CRLF nchar(2) = NCHAR(13) + NCHAR(10);
DECLARE @Deployment nvarchar(max) = N'';

SET @Deployment =
    N'/*' + @CRLF +
    N'  Generated safe function deployment' + @CRLF +
    N'  Migration: ' + REPLACE(@MigrationId, N'*/', N'') + @CRLF +
    N'  Source database: ' + REPLACE(DB_NAME(), N'*/', N'') + @CRLF +
    N'  Generated UTC: ' + CONVERT(nvarchar(30), SYSUTCDATETIME(), 126) + N'Z' + @CRLF +
    N'*/' + @CRLF + @CRLF +
    N'IF DB_ID(N''' + REPLACE(@TargetDatabase, N'''', N'''''') + N''') IS NULL' + @CRLF +
    N'    THROW 51000, ''Target database does not exist.'', 1;' + @CRLF +
    N'GO' + @CRLF +
    N'USE ' + QUOTENAME(@TargetDatabase) + N';' + @CRLF +
    N'GO' + @CRLF +
    N'SET NOCOUNT ON;' + @CRLF +
    N'SET XACT_ABORT ON;' + @CRLF +
    N'DECLARE @DryRun bit = 1; -- Change to 0 only after reviewing dry-run output.' + @CRLF + @CRLF +
    N'CREATE TABLE #FunctionMigrationPlan' + @CRLF +
    N'(' + @CRLF +
    N'    CreateOrder int NOT NULL,' + @CRLF +
    N'    SchemaName sysname NOT NULL,' + @CRLF +
    N'    FunctionName sysname NOT NULL,' + @CRLF +
    N'    ActionName varchar(10) NOT NULL,' + @CRLF +
    N'    SourceHash varchar(64) NOT NULL,' + @CRLF +
    N'    TargetHash varchar(64) NULL,' + @CRLF +
    N'    PreviousObjectId int NULL' + @CRLF +
    N');' + @CRLF + @CRLF +
    N'CREATE TABLE #FunctionBackup' + @CRLF +
    N'(' + @CRLF +
    N'    CreateOrder int NOT NULL,' + @CRLF +
    N'    SchemaName sysname NOT NULL,' + @CRLF +
    N'    FunctionName sysname NOT NULL,' + @CRLF +
    N'    PreviousDefinition nvarchar(max) NULL,' + @CRLF +
    N'    RollbackCommand nvarchar(max) NOT NULL' + @CRLF +
    N');' + @CRLF + @CRLF;

/* Validate cross-database databases and referenced objects. */
DECLARE
    @ExternalDatabase sysname,
    @ExternalSchema sysname,
    @ExternalEntity sysname;

DECLARE external_dependency_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT DISTINCT ReferencedDatabase, ReferencedSchema, ReferencedEntity
FROM #ExternalDependencies
ORDER BY ReferencedDatabase, ReferencedSchema, ReferencedEntity;

OPEN external_dependency_cursor;
FETCH NEXT FROM external_dependency_cursor
INTO @ExternalDatabase, @ExternalSchema, @ExternalEntity;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Deployment +=
        N'IF DB_ID(N''' + REPLACE(@ExternalDatabase, N'''', N'''''') + N''') IS NULL' + @CRLF +
        N'    THROW 51001, ''Required database is missing: ' +
        REPLACE(@ExternalDatabase, N'''', N'''''') + N''', 1;' + @CRLF;

    IF @ExternalEntity IS NOT NULL
    BEGIN
        SET @Deployment +=
            N'IF OBJECT_ID(N''' +
            REPLACE(QUOTENAME(@ExternalDatabase) + N'.' +
                    QUOTENAME(COALESCE(@ExternalSchema, N'dbo')) + N'.' +
                    QUOTENAME(@ExternalEntity), N'''', N'''''') + N''') IS NULL' + @CRLF +
            N'    THROW 51002, ''Required external object is missing: ' +
            REPLACE(@ExternalDatabase + N'.' + COALESCE(@ExternalSchema, N'dbo') + N'.' + @ExternalEntity,
                    N'''', N'''''') + N''', 1;' + @CRLF;
    END;

    FETCH NEXT FROM external_dependency_cursor
    INTO @ExternalDatabase, @ExternalSchema, @ExternalEntity;
END;

CLOSE external_dependency_cursor;
DEALLOCATE external_dependency_cursor;

/* Validate local tables/views and any resolvable referenced columns. */
DECLARE
    @LocalSchema sysname,
    @LocalEntity sysname,
    @LocalColumn sysname;

DECLARE local_dependency_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT DISTINCT ReferencedSchema, ReferencedEntity, ReferencedColumn
FROM #LocalDependencies
ORDER BY ReferencedSchema, ReferencedEntity, ReferencedColumn;

OPEN local_dependency_cursor;
FETCH NEXT FROM local_dependency_cursor INTO @LocalSchema, @LocalEntity, @LocalColumn;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Deployment +=
        N'IF OBJECT_ID(N''' +
        REPLACE(QUOTENAME(@LocalSchema) + N'.' + QUOTENAME(@LocalEntity), N'''', N'''''') + N''') IS NULL' + @CRLF +
        N'    THROW 51003, ''Required local object is missing: ' +
        REPLACE(@LocalSchema + N'.' + @LocalEntity, N'''', N'''''') + N''', 1;' + @CRLF;

    IF @LocalColumn IS NOT NULL
    BEGIN
        SET @Deployment +=
            N'IF COL_LENGTH(N''' +
            REPLACE(QUOTENAME(@LocalSchema) + N'.' + QUOTENAME(@LocalEntity), N'''', N'''''') +
            N''', N''' + REPLACE(@LocalColumn, N'''', N'''''') + N''') IS NULL' + @CRLF +
            N'    THROW 51004, ''Required column is missing: ' +
            REPLACE(@LocalSchema + N'.' + @LocalEntity + N'.' + @LocalColumn, N'''', N'''''') + N''', 1;' + @CRLF;
    END;

    FETCH NEXT FROM local_dependency_cursor INTO @LocalSchema, @LocalEntity, @LocalColumn;
END;

CLOSE local_dependency_cursor;
DEALLOCATE local_dependency_cursor;

SET @Deployment += @CRLF + N'BEGIN TRY' + @CRLF +
    N'    IF @DryRun = 0 BEGIN TRANSACTION;' + @CRLF + @CRLF;

DECLARE
    @CreateOrder int,
    @SchemaName sysname,
    @FunctionName sysname,
    @SourceType char(2),
    @Definition nvarchar(max),
    @AlterDefinition nvarchar(max),
    @EscapedObjectName nvarchar(600),
    @SourceHash varchar(64);

DECLARE function_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT CreateOrder, SchemaName, FunctionName, SourceType, Definition, AlterDefinition
FROM #Modules
ORDER BY CreateOrder;

OPEN function_cursor;
FETCH NEXT FROM function_cursor
INTO @CreateOrder, @SchemaName, @FunctionName, @SourceType, @Definition, @AlterDefinition;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @EscapedObjectName =
        REPLACE(QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@FunctionName), N'''', N'''''' );
    SET @SourceHash = CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varbinary(max), @Definition)), 2);

    SET @Deployment +=
        N'    -- ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@FunctionName) + @CRLF +
        N'    DECLARE @SourceDefinition_' + CONVERT(nvarchar(10), @CreateOrder) +
        N' nvarchar(max) = N''' + REPLACE(@Definition, N'''', N'''''') + N''';' + @CRLF +
        N'    DECLARE @AlterDefinition_' + CONVERT(nvarchar(10), @CreateOrder) + N' nvarchar(max) =' + @CRLF +
        N'        STUFF(@SourceDefinition_' + CONVERT(nvarchar(10), @CreateOrder) +
        N', CHARINDEX(N''CREATE FUNCTION'', UPPER(@SourceDefinition_' +
        CONVERT(nvarchar(10), @CreateOrder) + N')), 6, N''ALTER'');' + @CRLF +
        N'    DECLARE @ObjectId_' + CONVERT(nvarchar(10), @CreateOrder) +
        N' int = OBJECT_ID(N''' + @EscapedObjectName + N''');' + @CRLF +
        N'    DECLARE @TargetType_' + CONVERT(nvarchar(10), @CreateOrder) + N' char(2) =' + @CRLF +
        N'        (SELECT type FROM sys.objects WHERE object_id = @ObjectId_' +
        CONVERT(nvarchar(10), @CreateOrder) + N');' + @CRLF +
        N'    DECLARE @PreviousDefinition_' + CONVERT(nvarchar(10), @CreateOrder) + N' nvarchar(max) =' + @CRLF +
        N'        (SELECT definition FROM sys.sql_modules WHERE object_id = @ObjectId_' +
        CONVERT(nvarchar(10), @CreateOrder) + N');' + @CRLF +
        N'    DECLARE @TargetHash_' + CONVERT(nvarchar(10), @CreateOrder) + N' varchar(64) =' + @CRLF +
        N'        CONVERT(varchar(64), HASHBYTES(''SHA2_256'', CONVERT(varbinary(max), @PreviousDefinition_' +
        CONVERT(nvarchar(10), @CreateOrder) + N')), 2);' + @CRLF +
        N'    DECLARE @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' varchar(10);' + @CRLF +
        N'    IF @ObjectId_' + CONVERT(nvarchar(10), @CreateOrder) + N' IS NULL' + @CRLF +
        N'        SET @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''CREATE'';' + @CRLF +
        N'    ELSE IF @TargetType_' + CONVERT(nvarchar(10), @CreateOrder) + N' <> ''' + @SourceType + N'''' + @CRLF +
        N'        THROW 51005, ''Function type conflict: ' +
        REPLACE(@SchemaName + N'.' + @FunctionName, N'''', N'''''') + N''', 1;' + @CRLF +
        N'    ELSE IF @TargetHash_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''' + @SourceHash + N'''' + @CRLF +
        N'        SET @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''SKIP'';' + @CRLF +
        N'    ELSE' + @CRLF +
        N'        SET @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''ALTER'';' + @CRLF +
        N'    INSERT #FunctionMigrationPlan' + @CRLF +
        N'        (CreateOrder, SchemaName, FunctionName, ActionName, SourceHash, TargetHash, PreviousObjectId)' + @CRLF +
        N'    VALUES (' + CONVERT(nvarchar(10), @CreateOrder) + N', N''' +
        REPLACE(@SchemaName, N'''', N'''''') + N''', N''' +
        REPLACE(@FunctionName, N'''', N'''''') + N''', @Action_' +
        CONVERT(nvarchar(10), @CreateOrder) + N', ''' + @SourceHash + N''', @TargetHash_' +
        CONVERT(nvarchar(10), @CreateOrder) + N', @ObjectId_' +
        CONVERT(nvarchar(10), @CreateOrder) + N');' + @CRLF +
        N'    INSERT #FunctionBackup' + @CRLF +
        N'        (CreateOrder, SchemaName, FunctionName, PreviousDefinition, RollbackCommand)' + @CRLF +
        N'    VALUES (' + CONVERT(nvarchar(10), @CreateOrder) + N', N''' +
        REPLACE(@SchemaName, N'''', N'''''') + N''', N''' +
        REPLACE(@FunctionName, N'''', N'''''') + N''', @PreviousDefinition_' +
        CONVERT(nvarchar(10), @CreateOrder) + N',' + @CRLF +
        N'        CASE WHEN @ObjectId_' + CONVERT(nvarchar(10), @CreateOrder) + N' IS NULL' + @CRLF +
        N'             THEN N''DROP FUNCTION ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@FunctionName) + N';''' + @CRLF +
        N'             ELSE N''EXEC sys.sp_executesql N'''''' +' + @CRLF +
        N'                  REPLACE(STUFF(@PreviousDefinition_' + CONVERT(nvarchar(10), @CreateOrder) +
        N', CHARINDEX(N''CREATE FUNCTION'', UPPER(@PreviousDefinition_' + CONVERT(nvarchar(10), @CreateOrder) +
        N')), 6, N''ALTER''), N'''''''', N'''''''''''') + N'''''';''' + @CRLF +
        N'        END);' + @CRLF +
        N'    IF @DryRun = 0 AND @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''CREATE''' + @CRLF +
        N'        EXEC sys.sp_executesql @SourceDefinition_' + CONVERT(nvarchar(10), @CreateOrder) + N';' + @CRLF +
        N'    ELSE IF @DryRun = 0 AND @Action_' + CONVERT(nvarchar(10), @CreateOrder) + N' = ''ALTER''' + @CRLF +
        N'        EXEC sys.sp_executesql @AlterDefinition_' + CONVERT(nvarchar(10), @CreateOrder) + N';' + @CRLF + @CRLF;

    FETCH NEXT FROM function_cursor
    INTO @CreateOrder, @SchemaName, @FunctionName, @SourceType, @Definition, @AlterDefinition;
END;

CLOSE function_cursor;
DEALLOCATE function_cursor;

/* Verify action and object identity. ALTER must preserve the original object ID. */
SET @Deployment +=
    N'    IF @DryRun = 0 AND EXISTS' + @CRLF +
    N'    (' + @CRLF +
    N'        SELECT 1' + @CRLF +
    N'        FROM #FunctionMigrationPlan AS p' + @CRLF +
    N'        LEFT JOIN sys.objects AS o' + @CRLF +
    N'          ON o.name = p.FunctionName AND SCHEMA_NAME(o.schema_id) = p.SchemaName' + @CRLF +
    N'        LEFT JOIN sys.sql_modules AS m ON m.object_id = o.object_id' + @CRLF +
    N'        WHERE o.object_id IS NULL' + @CRLF +
    N'           OR CONVERT(varchar(64), HASHBYTES(''SHA2_256'', CONVERT(varbinary(max), m.definition)), 2) <> p.SourceHash' + @CRLF +
    N'           OR (p.ActionName = ''ALTER'' AND o.object_id <> p.PreviousObjectId)' + @CRLF +
    N'    )' + @CRLF +
    N'        THROW 51006, ''Post-deployment function verification failed.'', 1;' + @CRLF + @CRLF +
    N'    IF @DryRun = 0 COMMIT TRANSACTION;' + @CRLF +
    N'END TRY' + @CRLF +
    N'BEGIN CATCH' + @CRLF +
    N'    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;' + @CRLF +
    N'    THROW;' + @CRLF +
    N'END CATCH;' + @CRLF + @CRLF +
    N'SELECT CreateOrder, SchemaName, FunctionName, ActionName, SourceHash, TargetHash, PreviousObjectId' + @CRLF +
    N'FROM #FunctionMigrationPlan ORDER BY CreateOrder;' + @CRLF + @CRLF +
    N'SELECT CreateOrder, SchemaName, FunctionName, PreviousDefinition, RollbackCommand' + @CRLF +
    N'FROM #FunctionBackup WHERE PreviousDefinition IS NOT NULL OR RollbackCommand IS NOT NULL' + @CRLF +
    N'ORDER BY CreateOrder DESC;' + @CRLF + @CRLF +
    N'SELECT' + @CRLF +
    N'    p.SchemaName,' + @CRLF +
    N'    p.FunctionName,' + @CRLF +
    N'    dp.state_desc AS PermissionState,' + @CRLF +
    N'    dp.permission_name AS PermissionName,' + @CRLF +
    N'    USER_NAME(dp.grantee_principal_id) AS PrincipalName' + @CRLF +
    N'FROM #FunctionMigrationPlan AS p' + @CRLF +
    N'INNER JOIN sys.objects AS o' + @CRLF +
    N'  ON o.name = p.FunctionName AND SCHEMA_NAME(o.schema_id) = p.SchemaName' + @CRLF +
    N'LEFT JOIN sys.database_permissions AS dp' + @CRLF +
    N'  ON dp.class = 1 AND dp.major_id = o.object_id AND dp.minor_id = 0' + @CRLF +
    N'ORDER BY p.CreateOrder, PrincipalName, PermissionName;' + @CRLF +
    N'GO' + @CRLF;

/* Result set 1: portable deployment package. */
SELECT @Deployment AS DeploymentScript;

/* Result set 2: mandatory review of cross-database references. */
SELECT
    ReferencingFunction,
    ReferencedDatabase,
    ReferencedSchema,
    ReferencedEntity
FROM #ExternalDependencies
ORDER BY ReferencedDatabase, ReferencingFunction, ReferencedSchema, ReferencedEntity;

/* Result set 3: local prerequisites detected from source dependencies. */
SELECT
    ReferencingFunction,
    ReferencedSchema,
    ReferencedEntity,
    ReferencedColumn
FROM #LocalDependencies
ORDER BY ReferencedSchema, ReferencedEntity, ReferencedColumn, ReferencingFunction;

/* Result set 4: source permissions for review; they are not granted automatically. */
SELECT
    SchemaName,
    FunctionName,
    PrincipalName,
    PermissionName,
    PermissionState
FROM #SourcePermissions
ORDER BY SchemaName, FunctionName, PrincipalName, PermissionName;

/* Result set 5: selected application function manifest. */
SELECT
    m.CreateOrder,
    m.SchemaName,
    m.FunctionName,
    m.SourceType,
    m.IsStoreSpecific,
    f.Purpose
FROM #Modules AS m
INNER JOIN #FunctionManifest AS f ON f.CreateOrder = m.CreateOrder
ORDER BY m.CreateOrder;
