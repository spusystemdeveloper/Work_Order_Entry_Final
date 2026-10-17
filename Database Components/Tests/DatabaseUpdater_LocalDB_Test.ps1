# Disposable SQL/UI tests; no app credentials and no existing store connections.
param([switch]$RenderOnly)
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    $renderArgument = @()
    if ($RenderOnly) { $renderArgument += '-RenderOnly' }
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath @renderArgument
    exit $LASTEXITCODE
}
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$exe = Join-Path $repo 'Database Updater\bin\Debug\Work Order Database Updater.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw 'Build the Debug updater first.' }
Add-Type -AssemblyName System.Data, System.Windows.Forms
$null = [Reflection.Assembly]::LoadFrom($exe)
$localdb = (Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$suffix = [Guid]::NewGuid().ToString('N').Substring(0, 8)
$instance = "SODWO_UpdateTest_$suffix"
$dbName = "SODWO_UpdateTest_$suffix"
$directory = Join-Path $repo "TestResults\DatabaseUpdater-$suffix"
$null = New-Item -ItemType Directory -Path $directory
$components = Join-Path $repo 'Database Components'
$checks = 0
$connection = $null
$master = $null
$created = $false
$hoFixtureCreated = $false
function Check([bool]$condition, [string]$label) {
    if (-not $condition) { throw "FAILED: $label" }
    $script:checks++
    Write-Output "Passed: $label"
}
function Sql([string]$sql) {
    $cmd = $connection.CreateCommand()
    try { $cmd.CommandText = $sql; $null = $cmd.ExecuteNonQuery() } finally { $cmd.Dispose() }
}
function Scalar([string]$sql) {
    $cmd = $connection.CreateCommand()
    try { $cmd.CommandText = $sql; return $cmd.ExecuteScalar() } finally { $cmd.Dispose() }
}
function Reject([scriptblock]$action, [string]$pattern, [string]$label) {
    $failed = $false
    try { & $action | Out-Null }
    catch { if ($_.Exception.ToString() -notmatch $pattern) { throw }; $failed = $true }
    Check $failed $label
}
function Script([string]$name, [string]$sql) {
    $path = Join-Path $directory ($name + '.sql')
    [IO.File]::WriteAllText($path, $sql)
    return [WorkOrderDatabaseUpdater.UpdateScript]::Load($path, "Test/$name.sql", 'Test', 'Disposable fixture', $false)
}
function Apply($scripts) {
    return [WorkOrderDatabaseUpdater.UpdateRunner]::Apply($cs, $dbName, [WorkOrderDatabaseUpdater.UpdateScript[]]$scripts, (Join-Path $directory 'Runs'))
}
function Apply-HoMigration($scripts) {
    return [WorkOrderDatabaseUpdater.UpdateRunner]::Apply($hoCs, 'HO_PO_DB', [WorkOrderDatabaseUpdater.UpdateScript[]]$scripts, (Join-Path $directory 'Runs'))
}
function Initialize-RenderControl($control) {
    $null = $control.Handle
    foreach ($child in $control.Controls) { Initialize-RenderControl $child }
}
try {
    foreach ($profile in @('GSC','DVO','Portable')) {
        $catalog = [WorkOrderDatabaseUpdater.UpdateCatalog]::Load($components, $profile)
        Check ($catalog.Count -gt 5) "$profile catalog loads all migration/object batches"
        Check (-not ($catalog.Id -match 'Security|Validation|Deploy_')) "$profile excludes permissions and SQLCMD driver execution"
        Check (@($catalog | Where-Object {$_.Id -eq 'Migrations/Enable_WorkOrder_History.sql'}).Count -eq 1 -and
            -not ($catalog.Id -match 'Fix_WorkOrder_History_Identity')) "$profile offers one history create/update selection"
        Check (@($catalog | Where-Object {$_.Id -eq 'Migrations/Improve_WorkOrder_ItemLogLookup.sql' -and $_.Category -eq 'Index / performance'}).Count -eq 1) "$profile offers the optional item-log performance index"
        Check (@($catalog | Where-Object {$_.Id -eq 'Migrations/Improve_RMS_VoucherLookup.sql' -and $_.Category -eq 'Index / performance'}).Count -eq 1) "$profile offers the optional RMS voucher lookup index"
        $packaged = [WorkOrderDatabaseUpdater.UpdateCatalog]::Load((Join-Path (Split-Path $exe) 'Database Components'), $profile)
        Check (($packaged | Where-Object Id -eq 'Migrations/Improve_RMS_VoucherLookup.sql').Hash -eq
            ($catalog | Where-Object Id -eq 'Migrations/Improve_RMS_VoucherLookup.sql').Hash) "$profile built package includes the exact reviewed voucher migration"
    }
    Check (Test-Path (Join-Path (Split-Path $exe) 'Database Components\Migrations\RMS_VOUCHER_LOOKUP_GUIDE.md')) 'Built updater includes the voucher deployment guide'
    $maskedGo = "SELECT 'first`nGO`nlast';`n/* outer /* nested */`nGO`n*/`nGO -- separator`nSELECT 2;"
    Check ([WorkOrderDatabaseUpdater.SqlBatches]::Split($maskedGo, $false).Count -eq 2) 'GO in strings and nested comments stays inside its batch'
    Check ([WorkOrderDatabaseUpdater.SqlBatches]::Split("SELECT [GO], `"GO`";`nGO /* multi`nline */`nSELECT 2", $false).Count -eq 2) 'Quoted identifiers and multiline separator comments are supported'
    Reject { [WorkOrderDatabaseUpdater.SqlBatches]::Split("SELECT 1`nGO 2", $false) } 'repeat counts' 'GO repeat counts rejected'
    Reject { [WorkOrderDatabaseUpdater.SqlBatches]::Split(":r file.sql", $false) } 'SQLCMD' 'SQLCMD includes rejected'
    Reject { [WorkOrderDatabaseUpdater.SqlBatches]::Split('USE another; SELECT 1', $false) } 'selected database' 'Direct target switching rejected'
    Reject { [WorkOrderDatabaseUpdater.SqlBatches]::Split('BEGIN TRAN; SELECT 1; COMMIT;', $false) } 'transaction commands' 'Custom transaction control rejected'
    Reject { [WorkOrderDatabaseUpdater.SqlBatches]::Split("SELECT 'broken", $false) } 'unfinished' 'Unclosed SQL literal rejected'
    $secret = 'updater-test-secret'
    $safeCs = [WorkOrderDatabaseUpdater.UpdateRunner]::ConnectionString('localhost', 'Test', 'test', $secret, $false)
    $safeMessage = [WorkOrderDatabaseUpdater.UpdateRunner]::DescribeError([Exception]::new("Test $secret"), $safeCs)
    Check ($safeMessage -notlike "*$secret*" -and $safeMessage -like '*redacted*') 'Configured password redacted from error text'
    $form = [WorkOrderDatabaseUpdater.UpdaterForm]::new()
    try {
        $flags = [Reflection.BindingFlags]'NonPublic,Instance'
        $form.GetType().GetField('packageDirectory', $flags).SetValue($form, $components)
        $form.GetType().GetMethod('LoadPackage', $flags).Invoke($form, @()) | Out-Null
        $grid = $form.GetType().GetField('grid', $flags).GetValue($form)
        $voucherRows = @($grid.Rows | Where-Object {$_.Tag.Id -eq 'Migrations/Improve_RMS_VoucherLookup.sql'})
        Check ($voucherRows.Count -eq 1 -and -not [bool]$voucherRows[0].Cells[0].Value) 'Voucher migration is visible and initially unchecked'
        # Render the content as a detached control so hidden Form visibility does
        # not suppress child painting. No desktop window or user input is used.
        $content = $form.Controls[0]
        $form.Controls.Remove($content)
        $hostControl = [Windows.Forms.UserControl]::new()
        $hostControl.Size = $form.ClientSize
        $hostControl.Controls.Add($content)
        Initialize-RenderControl $hostControl; $hostControl.PerformLayout(); $content.PerformLayout()
        $bitmap = [Drawing.Bitmap]::new($hostControl.Width, $hostControl.Height)
        try {
            $hostControl.DrawToBitmap($bitmap, [Drawing.Rectangle]::new(0,0,$hostControl.Width,$hostControl.Height))
            $bitmap.Save((Join-Path $directory 'UpdaterPreview.png'), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose(); $hostControl.Dispose() }
        Check ($null -ne $content) 'WinForms updater initializes and renders with the package loaded'
    }
    finally { $form.Dispose() }
    if ($RenderOnly) { Write-Output "Render preview: $directory\UpdaterPreview.png"; return }

    & $localdb create $instance | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'LocalDB instance creation failed.' }
    $created = $true
    & $localdb start $instance | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'LocalDB start failed.' }
    $server = "(localdb)\$instance"
    $masterCs = [WorkOrderDatabaseUpdater.UpdateRunner]::ConnectionString($server, 'master', '', '', $true)
    $master = [Data.SqlClient.SqlConnection]::new($masterCs); $master.Open()
    $cmd = $master.CreateCommand()
    try { $cmd.CommandText = "CREATE DATABASE [$dbName]; ALTER DATABASE [$dbName] SET COMPATIBILITY_LEVEL=130;"; $null = $cmd.ExecuteNonQuery() }
    finally { $cmd.Dispose() }
    $cs = [WorkOrderDatabaseUpdater.UpdateRunner]::ConnectionString($server, $dbName, '', '', $true)
    $connection = [Data.SqlClient.SqlConnection]::new($cs); $connection.Open()
    Sql @'
CREATE TABLE dbo.Item(ID int NOT NULL PRIMARY KEY,ItemLookupCode nvarchar(25) NOT NULL,Taxable bit NOT NULL);
CREATE TABLE dbo.[Order](ID int NOT NULL PRIMARY KEY);
CREATE TABLE dbo.OrderEntry(ID int NOT NULL PRIMARY KEY,OrderID int NOT NULL);
INSERT dbo.Item VALUES(1,N'A',1),(2,N'A',0);
CREATE TABLE dbo.Invariant(ID int NOT NULL PRIMARY KEY,Qty int NOT NULL);
INSERT dbo.Invariant VALUES(1,7);
'@
    $info = [WorkOrderDatabaseUpdater.UpdateRunner]::Check($cs, $dbName)
    Check ($info.RecordedHashes.Count -eq 0 -and (Scalar "SELECT OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory')") -is [DBNull]) 'Check target is read-only; it does not create history'
    $initialFailure = Script 'first-deployment-failure' "CREATE TABLE dbo.FirstFailure(ID int); THROW 52302, 'Initial fixture failure', 1;"
    Reject { Apply @($initialFailure) } 'Initial fixture failure' 'First deployment failure is reported'
    Check ((Scalar "SELECT OBJECT_ID(N'dbo.SOD_WO_DeploymentHistory')") -is [DBNull] -and
        (Scalar "SELECT OBJECT_ID(N'dbo.FirstFailure')") -is [DBNull]) 'First failed deployment rolls back both new schema and history table'
    Reject { [WorkOrderDatabaseUpdater.UpdateRunner]::Check($cs, 'wrong-target') } 'mismatch' 'Incorrect expected target rejected'
    Reject { [WorkOrderDatabaseUpdater.UpdateRunner]::Check($masterCs, 'master') } 'system database' 'System database rejected'
    Sql 'CREATE USER UpdaterReadOnly WITHOUT LOGIN; GRANT SELECT TO UpdaterReadOnly;'
    $connection.Close()
    $limited = [Data.SqlClient.SqlConnection]::new($cs); $limited.Open()
    $cmd = $limited.CreateCommand()
    try {
        $cmd.CommandText = 'EXECUTE AS USER=N''UpdaterReadOnly'';'; $null = $cmd.ExecuteNonQuery()
        $guard = [WorkOrderDatabaseUpdater.UpdateRunner].GetMethod('Guard', [Reflection.BindingFlags]'NonPublic,Static')
        Reject { $guard.Invoke($null, [object[]]@($limited, $dbName, $null)) } 'VIEW DEFINITION' 'Restricted user cannot apply without schema snapshot permission'
    }
    finally { $cmd.Dispose(); $limited.Dispose(); $connection.Open() }

    $createFn = Script 'create-function' "CREATE FUNCTION dbo.SOD_UpdateTest() RETURNS int AS BEGIN RETURN 7 END;`nGO /* separator`ncomment */`nSELECT dbo.SOD_UpdateTest();"
    $first = Apply @($createFn)
    Check ((Scalar 'SELECT dbo.SOD_UpdateTest()') -eq 7) 'Function creation with GO executes through production runner'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq 1) 'Successful script hash and history committed'
    Check ((Get-Content (Join-Path $first.RunDirectory 'result.txt') -Raw) -match 'Status: COMMITTED') 'Committed result recorded locally'
    $alterFn = Script 'alter-function' 'ALTER FUNCTION dbo.SOD_UpdateTest() RETURNS int AS BEGIN RETURN 9 END;'
    $second = Apply @($alterFn)
    Check ((Scalar 'SELECT dbo.SOD_UpdateTest()') -eq 9) 'Existing function can be updated'
    Check ((Get-Content (Join-Path $second.RunDirectory 'before-schema.xml') -Raw) -match 'RETURN 7') 'Previous function definition saved before ALTER'
    $catalog = [WorkOrderDatabaseUpdater.UpdateCatalog]::Load($components, 'GSC')
    $index = $catalog | Where-Object Id -eq 'Migrations/Improve_WorkOrder_ItemLookup.sql'
    $queue = $catalog | Where-Object Id -eq 'Migrations/Ensure_Queueing_Compatibility_Tables.sql'
    $edit = $catalog | Where-Object Id -eq 'Migrations/Prepare_WorkOrder_Edit_Checks.sql'
    $null = Apply @($queue, $edit, $index)
    $null = Apply @($queue, $edit, $index)
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Item') AND name='IX_SOD_WO_ItemLookup' AND is_unique=0") -eq 1) 'Real index migration is idempotent and non-unique'
    $logIndex = $catalog | Where-Object Id -eq 'Migrations/Improve_WorkOrder_ItemLogLookup.sql'
    Sql @'
CREATE TABLE dbo.SOD_Item_Logs(ItemID int NOT NULL,ReferenceID varchar(50) NOT NULL,NewCommitted int NOT NULL,[Date] datetime NOT NULL);
INSERT dbo.SOD_Item_Logs VALUES(1,'01',7,'20200101');
'@
    $historyBeforeLogIndex = Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory;'
    $indexFailure = Script 'log-index-later-failure' "THROW 53320, 'Item log fixture failure', 1;"
    Reject { Apply @($logIndex,$indexFailure) } 'Item log fixture failure' 'Later updater failure rolls back the real item-log index selection'
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.SOD_Item_Logs') AND name='IX_SOD_WO_ItemLogLookup';") -eq 0 -and
        (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory;') -eq $historyBeforeLogIndex) 'Failed item-log deployment leaves no index or success record'
    $null = Apply @($logIndex)
    $null = Apply @($logIndex)
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.SOD_Item_Logs') AND name='IX_SOD_WO_ItemLogLookup' AND is_unique=0;") -eq 1) 'Updater installs and reuses the real non-unique item-log index'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory WHERE ScriptId='Migrations/Improve_WorkOrder_ItemLogLookup.sql' AND ScriptHash='$($logIndex.Hash)';") -eq 2) 'Committed item-log deployments record the reviewed script hash'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ItemID=1 AND ReferenceID=''01'' AND NewCommitted=7;') -eq 1) 'Updater item-log selection preserves existing business rows'
    $voucherIndex = $catalog | Where-Object Id -eq 'Migrations/Improve_RMS_VoucherLookup.sql'
    Sql 'ALTER TABLE dbo.Item ADD TenderID int NOT NULL DEFAULT 0, IsGlobalVoucher bit NOT NULL DEFAULT 0, Inactive bit NOT NULL DEFAULT 0;'
    $historyBeforeVoucher = Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory;'
    Reject { Apply @($voucherIndex,$indexFailure) } 'Item log fixture failure' 'Later updater failure rolls back the real voucher index selection'
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Item') AND name='IX_SOD_RMS_VoucherLookup';") -eq 0 -and
        (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory;') -eq $historyBeforeVoucher) 'Failed voucher deployment leaves no index or success record'
    $null = Apply @($voucherIndex)
    $null = Apply @($voucherIndex)
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Item') AND name='IX_SOD_RMS_VoucherLookup' AND is_unique=0;") -eq 1) 'Updater installs and reuses the non-unique voucher index'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory WHERE ScriptId='Migrations/Improve_RMS_VoucherLookup.sql' AND ScriptHash='$($voucherIndex.Hash)';") -eq 2) 'Committed voucher deployments record the reviewed script hash'
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.OrderEntry') AND name='IX_SOD_WO_Edit_OrderEntry'") -eq 1) 'Nested packaged migration transactions retain the outer transaction'
    Check ((Scalar 'SELECT Qty FROM dbo.Invariant WHERE ID=1') -eq 7) 'Selected index/migration scripts preserve fixture business data'
    $info = [WorkOrderDatabaseUpdater.UpdateRunner]::Check($cs, $dbName)
    Check ($info.RecordedHashes[$index.Id] -eq $index.Hash) 'Target check reports the recorded successful hash'

    $examples = @(Get-ChildItem -LiteralPath (Join-Path $repo 'Database Updater\Templates') -Filter '*.sql' | Sort-Object Name | ForEach-Object {
        [WorkOrderDatabaseUpdater.UpdateScript]::Load($_.FullName, ('Custom/' + $_.Name), 'Custom SQL', 'User-guide example', $false)
    })
    $null = Apply $examples
    Check ((Scalar 'SELECT dbo.SOD_fn_UpdaterExample(3)') -eq 6) 'Imported guide template creates and verifies the demonstration function'
    Check ((Scalar "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_UpdaterExample') AND name=N'UpdatedAtUtc' AND TYPE_NAME(system_type_id)=N'datetime2' AND scale=3 AND is_nullable=1") -eq 1) 'Imported guide migration adds the intended nullable column'
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SOD_UpdaterExample') AND name=N'IX_SOD_UpdaterExample_UpdatedAtUtc' AND is_unique=0") -eq 1) 'Imported guide index follows its table/column prerequisite'
    $null = Apply $examples
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory WHERE ScriptId LIKE N'Custom/0%Example%.sql'") -eq 6) 'All three guide examples support explicit repeat deployment'
    $updatedExampleSql = $examples[0].Sql.Replace('RETURN @Value * 2;', 'RETURN @Value * 3;').Replace(') <> 6', ') <> 9')
    $updatedExample = Script 'update-guide-example' $updatedExampleSql
    $null = Apply @($updatedExample)
    Check ((Scalar 'SELECT dbo.SOD_fn_UpdaterExample(3)') -eq 9) 'Documented edit to the example body and verification updates an existing function'
    $null = Apply @($examples[0])
    Check ((Scalar 'SELECT dbo.SOD_fn_UpdaterExample(3)') -eq 6) 'Previous compatible function definition can be deliberately reapplied'

    $count = Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory'
    $change = Script 'change-before-failure' "UPDATE dbo.Invariant SET Qty=3 WHERE ID=1;`nGO`nALTER FUNCTION dbo.SOD_UpdateTest() RETURNS int AS BEGIN RETURN 2 END;"
    $failure = Script 'forced-failure' "THROW 52301, 'Fixture failure', 1;"
    $later = Script 'must-not-run' 'CREATE TABLE dbo.ShouldNotExist(ID int);'
    Reject { Apply @($change, $failure, $later) } 'Fixture failure' 'SQL failure stops a multi-script deployment'
    Check ((Scalar 'SELECT Qty FROM dbo.Invariant WHERE ID=1') -eq 7 -and (Scalar 'SELECT dbo.SOD_UpdateTest()') -eq 9) 'Earlier DML and function changes roll back together'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq $count) 'Failed plan leaves no successful deployment records'
    Check ((Scalar "SELECT OBJECT_ID(N'dbo.ShouldNotExist')") -is [DBNull]) 'Scripts after a failure are not executed'
    $changed = Script 'edited-after-review' 'SELECT 1;'
    [IO.File]::WriteAllText($changed.Path, 'SELECT 2;')
    Reject { Apply @($changed) } 'changed after review' 'Edited file cannot execute using an older reviewed hash'

    $lockTransaction = $connection.BeginTransaction()
    $cmd = $connection.CreateCommand(); $cmd.Transaction = $lockTransaction
    try {
        $cmd.CommandText = "DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'SOD_WO_DatabaseUpdater',@LockMode='Exclusive',@LockOwner='Transaction'; SELECT @r;"
        $null = $cmd.ExecuteScalar()
        Reject { Apply @($alterFn) } 'Another database update' 'Concurrent updater cannot deploy while the application lock is held'
    } finally { $cmd.Dispose(); $lockTransaction.Rollback(); $lockTransaction.Dispose() }
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq $count) 'Rejected concurrent attempt leaves history unchanged'
    $schema = Scalar "SELECT definition FROM sys.sql_modules WHERE object_id=OBJECT_ID('dbo.SOD_UpdateTest')"
    Check ($schema -match 'RETURN 9') 'All rejected updates preserve the committed function'
    $hoPath = Join-Path $components 'Migrations\Add_HO_PO_WorkOrder_Password_Columns.sql'
    $hoScript = [WorkOrderDatabaseUpdater.UpdateScript]::Load($hoPath, 'Custom/Add_HO_PO_WorkOrder_Password_Columns.sql', 'Custom SQL', 'HO target column migration', $false)
    $beforeWrongTarget = Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory'
    Reject { Apply @($hoScript) } 'targets HO_PO_DB only' 'HO migration rejects a different target database'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq $beforeWrongTarget) 'Wrong-target HO attempt leaves deployment records unchanged'
    # HO_PO_DB below exists only in this GUID-owned disposable LocalDB instance.
    $cmd = $master.CreateCommand()
    try { $cmd.CommandText = 'CREATE DATABASE HO_PO_DB; ALTER DATABASE HO_PO_DB SET COMPATIBILITY_LEVEL=130;'; $null = $cmd.ExecuteNonQuery(); $hoFixtureCreated = $true }
    finally { $cmd.Dispose() }
    $hoCs = [WorkOrderDatabaseUpdater.UpdateRunner]::ConnectionString($server, 'HO_PO_DB', '', '', $true)
    $hoConnection = [Data.SqlClient.SqlConnection]::new($hoCs); $hoConnection.Open()
    $previousConnection = $connection
    $connection = $hoConnection
    try {
        Sql @'
CREATE TABLE dbo.Item(ID int NOT NULL PRIMARY KEY,ItemLookupCode nvarchar(25),Taxable bit);
CREATE TABLE dbo.[Order](ID int NOT NULL PRIMARY KEY);
CREATE TABLE dbo.OrderEntry(ID int NOT NULL PRIMARY KEY);
CREATE TABLE dbo.SOD_WO_Conf (
 id int IDENTITY(1,1) NOT NULL PRIMARY KEY,[Password] nvarchar(50) NULL,
 LimitEntry int NULL,PriceLevel nvarchar(50) NULL,StoreID int NULL,AllowPriceChange bit NULL,CustType nvarchar(max) NULL);
INSERT dbo.SOD_WO_Conf([Password],LimitEntry,PriceLevel,StoreID,AllowPriceChange,CustType)
 VALUES(N'fixture-only-one',12,N'fixture-level',37,1,N'fixture-customer'),
       (N'fixture-only-two',14,N'fixture-level',38,0,N'fixture-customer');
'@
        Reject { Sql $hoScript.Sql } 'active transaction' 'HO column script refuses execution without the updater transaction'
        $null = Apply-HoMigration @($hoScript)
        Check ((Scalar "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf') AND name IN(N'Password2',N'Password3',N'Pass2Username',N'Pass3Username') AND system_type_id=231 AND max_length=100 AND is_nullable=1") -eq 4) 'HO migration creates all four missing nullable nvarchar(50) fields'
        Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Password2 IS NULL AND Password3 IS NULL AND Pass2Username IS NULL AND Pass3Username IS NULL') -eq 2) 'New HO password fields stay NULL; no credentials are generated'
        Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE (id=1 AND [Password]=N'fixture-only-one' AND LimitEntry=12 AND StoreID=37 AND AllowPriceChange=1) OR (id=2 AND [Password]=N'fixture-only-two' AND LimitEntry=14 AND StoreID=38 AND AllowPriceChange=0)") -eq 2 -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_Conf') -eq 2) 'HO configuration rows, existing credentials, and settings are preserved'
        Sql "UPDATE dbo.SOD_WO_Conf SET Password2=N'fixture-secondary',Password3=N'fixture-third',Pass2Username=N'fixture-user'; ALTER TABLE dbo.SOD_WO_Conf DROP COLUMN Pass3Username;"
        $null = Apply-HoMigration @($hoScript)
        Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Password2=N'fixture-secondary' AND Password3=N'fixture-third' AND Pass2Username=N'fixture-user' AND Pass3Username IS NULL") -eq 2) 'Partial HO schema upgrade adds only the missing field and preserves populated fields'
        $columnDefinition = Scalar "SELECT column_id,name,system_type_id,max_length,is_nullable,collation_name FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf') ORDER BY column_id FOR XML PATH('Column'),ROOT('Columns')"
        $null = Apply-HoMigration @($hoScript)
        $repeatedDefinition = Scalar "SELECT column_id,name,system_type_id,max_length,is_nullable,collation_name FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SOD_WO_Conf') ORDER BY column_id FOR XML PATH('Column'),ROOT('Columns')"
        Check ($columnDefinition -eq $repeatedDefinition -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq 3) 'Repeated HO deployment preserves schema definitions and records explicit runs'
        Sql 'ALTER TABLE dbo.SOD_WO_Conf ALTER COLUMN Password3 nvarchar(100) NULL; ALTER TABLE dbo.SOD_WO_Conf DROP COLUMN Pass2Username;'
        Reject { Apply-HoMigration @($hoScript) } 'not a regular nvarchar' 'Incompatible HO column definition stops the migration'
        Check ((Scalar "SELECT COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass2Username')") -is [DBNull] -and (Scalar "SELECT COL_LENGTH(N'dbo.SOD_WO_Conf',N'Password3')") -eq 200 -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq 3) 'Incompatible HO schema is left intact and missing columns are not partially added'
        Sql 'ALTER TABLE dbo.SOD_WO_Conf ALTER COLUMN Password3 nvarchar(50) NULL;'
        Reject { Apply-HoMigration @($hoScript,$failure) } 'Fixture failure' 'HO additions roll back when a later selected script fails'
        Check ((Scalar "SELECT COL_LENGTH(N'dbo.SOD_WO_Conf',N'Pass2Username')") -is [DBNull] -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq 3 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Password3=N'fixture-third'") -eq 2) 'Rolled-back HO plan preserves prior fields and deployment history'
        Sql 'DROP TABLE dbo.SOD_WO_Conf;'
        Reject { Apply-HoMigration @($hoScript) } 'existing dbo.SOD_WO_Conf table' 'Missing HO configuration table requires review'
        Check ((Scalar "SELECT OBJECT_ID(N'dbo.SOD_WO_Conf')") -is [DBNull]) 'HO migration does not invent a missing branch configuration table'
        Sql 'CREATE VIEW dbo.SOD_WO_Conf AS SELECT 1 AS id;'
        Reject { Apply-HoMigration @($hoScript) } 'existing dbo.SOD_WO_Conf table' 'HO configuration object type conflict is rejected'
        Check ((Scalar "SELECT OBJECT_ID(N'dbo.SOD_WO_Conf',N'V')") -isnot [DBNull] -and (Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_DeploymentHistory') -eq 3) 'Rejected HO object conflict preserves the existing view and history'
    } finally { $connection = $previousConnection; $hoConnection.Dispose() }
    Write-Output "PASS: $checks updater checks; artifacts: $directory"
} finally {
    if ($null -ne $connection) { $connection.Dispose() }
    if ($null -ne $master) {
        if ($master.State -eq [Data.ConnectionState]::Open) {
            $cmd = $master.CreateCommand()
            try {
                $cmd.CommandText = "IF DB_ID(N'$dbName') IS NOT NULL BEGIN ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$dbName]; END"
                $null = $cmd.ExecuteNonQuery()
                if ($hoFixtureCreated) {
                    $cmd.CommandText = 'ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE HO_PO_DB;'
                    $null = $cmd.ExecuteNonQuery()
                }
            }
            finally { $cmd.Dispose() }
        }
        $master.Dispose()
    }
    if ($created) {
        & $localdb stop $instance -k | Out-Null
        & $localdb delete $instance | Out-Null
    }
}
