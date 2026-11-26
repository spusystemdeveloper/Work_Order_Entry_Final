# Tests the recovery with its target and row stamps rebound to a disposable LocalDB fixture.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$database = 'SOD_DvoPoRepair_' + [Guid]::NewGuid().ToString('N').Substring(0, 12)
$directory = Join-Path $repo ('TestResults\' + $database)
$null = New-Item -ItemType Directory -Path $directory
$connection = [System.Data.SqlClient.SqlConnection]::new(
    'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Pooling=False;Connect Timeout=10')
$created = $false
$checks = 0

function Sql([string]$query, [switch]$Scalar) {
    $command = $connection.CreateCommand()
    try {
        $command.CommandTimeout = 25
        $command.CommandText = $query
        if ($Scalar) { return $command.ExecuteScalar() }
        $null = $command.ExecuteNonQuery()
    } finally { $command.Dispose() }
}
function Check([bool]$condition, [string]$description) {
    if (!$condition) { throw ('FAILED: ' + $description) }
    $script:checks++
    Write-Output ('PASS: ' + $description)
}
function Reject([string]$query, [int]$number, [string]$description) {
    try { Sql $query; throw ('Expected SQL error ' + $number) }
    catch {
        $errorObject = $_.Exception.GetBaseException()
        if ($errorObject -isnot [System.Data.SqlClient.SqlException] -or
            $number -notin @($errorObject.Errors | ForEach-Object Number)) { throw }
    }
    Check ((Sql 'SELECT @@TRANCOUNT;' -Scalar) -eq 0) $description
}
function Snapshot([switch]$Unaffected) {
    $itemWhere = if ($Unaffected) { 'WHERE ID <> 16912' } else { '' }
    $movementWhere = if ($Unaffected) { 'WHERE ItemID <> 16912' } else { '' }
    $reads = @(
        "dbo.Item $itemWhere ORDER BY ID", 'dbo.PurchaseOrder ORDER BY ID', 'dbo.PurchaseOrderEntry ORDER BY ID',
        "dbo.InventoryTransferLog $movementWhere ORDER BY ID", 'dbo.RMSTMP_TransferInventory ORDER BY ID',
        'dbo.ItemValueLog ORDER BY ItemID,LastUpdated'
    )
    if (!$Unaffected) { $reads += 'dbo.SOD_Item_Logs ORDER BY ItemID,[Date],[Type]', 'dbo.RMSTMP_InventoryMovement ORDER BY ID' }
    return (($reads | ForEach-Object { Sql "SELECT CONVERT(nvarchar(max), (SELECT * FROM $_ FOR XML RAW,BINARY BASE64,TYPE));" -Scalar }) -join [Environment]::NewLine)
}

try {
    $connection.Open()
    if ((Sql "SELECT CONVERT(int,SERVERPROPERTY('IsLocalDB'));" -Scalar) -ne 1) { throw 'The test requires SQL Server LocalDB.' }
    Sql "CREATE DATABASE [$database];"
    $created = $true
    Sql "ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL = 100;"
    $connection.ChangeDatabase($database)
    Sql ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures\DVO_PO_0002601.sql')))
    $triggers = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Fixtures\DVO_PO_0002601_Triggers.json') -Raw | ConvertFrom-Json
    foreach ($trigger in $triggers) { Sql $trigger.Definition }

    $production = [IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Repair_DVO_PO_0002601_11688BOX.sql'))
    $reviewSetting = 'DECLARE @Commit bit = 0;'
    $commitSetting = 'DECLARE @Commit bit = 1;'
    $originalTarget = "DB_NAME() <> N'DVO_STORE_DB' OR @@SERVERNAME <> N'WIN-L6UL7AVNSQ6'"
    if (!$production.Contains($originalTarget) -or !$production.Contains($reviewSetting)) { throw 'Production target or review default changed.' }
    Reject ($production.Replace($reviewSetting,$commitSetting)) 53260 'Production SQL refuses LocalDB even with commit enabled'
    Sql 'SET XACT_ABORT ON; BEGIN TRANSACTION;'
    try {
        try { Sql $production; throw 'Expected the open-session guard to reject the run.' }
        catch {
            if ($_.Exception.GetBaseException().Message -notmatch 'dedicated session') { throw }
        }
        Check ((Sql 'SELECT CASE WHEN @@TRANCOUNT=1 AND XACT_STATE()=1 THEN 1 ELSE 0 END;' -Scalar) -eq 1) 'Open-session guard preserves the caller transaction'
    } finally { Sql 'IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;' }
    $local = $production.Replace($originalTarget,
        "DB_NAME() <> N'$database' OR ISNULL(CONVERT(int,SERVERPROPERTY('IsLocalDB')),0) <> 1")
    $stampQueries = @{
        ExpectedItemStamp = 'dbo.Item WHERE ID=16912'
        ExpectedPOStamp = 'dbo.PurchaseOrder WHERE ID=27784'
        ExpectedLineStamp = 'dbo.PurchaseOrderEntry WHERE ID=97646'
        ExpectedSiblingStamp = 'dbo.PurchaseOrderEntry WHERE ID=97647'
    }
    foreach ($name in $stampQueries.Keys) {
        $stamp = Sql ("SELECT CONVERT(varchar(18),CONVERT(binary(8),DBTimeStamp),1) FROM " + $stampQueries[$name]) -Scalar
        if ($stamp -notmatch '^0x[0-9A-Fa-f]{16}$') { throw 'Invalid fixture row version.' }
        $pattern = ('DECLARE \@{0} binary\(8\) = 0x[0-9A-Fa-f]{{16}};' -f $name)
        if ([regex]::Matches($local,$pattern).Count -ne 1) { throw ('Missing reviewed stamp ' + $name) }
        $local = [regex]::Replace($local,$pattern,('DECLARE @{0} binary(8) = {1};' -f $name,$stamp))
    }
    $baseline = Snapshot
    $identity = Sql "SELECT IDENT_CURRENT(N'dbo.InventoryTransferLog');" -Scalar
    Check ((Sql $local -Scalar) -eq 'REVIEW ONLY') 'Read-only review passes with the seven live trigger fingerprints'
    Check (((Snapshot) -ceq $baseline) -and ((Sql "SELECT IDENT_CURRENT(N'dbo.InventoryTransferLog');" -Scalar) -eq $identity)) 'Review changes no rows or movement identity'
    Reject ($local.Replace($reviewSetting,$commitSetting)) 53262 'Commit is blocked without a verified backup'

    $marker = '-- BEGIN SHARED REPAIR BODY (used as-is by the disposable LocalDB test)'
    if (!$local.Contains($marker)) { throw 'Repair body boundary is missing.' }
    $scenarios = @(
        @{Number=53264; Label='Disabled audit trigger is rejected'; SQL='DISABLE TRIGGER dbo.trg_ItemLogs ON dbo.Item;'},
        @{Number=53266; Label='Receipt row version drift is rejected'; SQL='UPDATE dbo.PurchaseOrderEntry SET Price=Price WHERE ID=97646;'},
        @{Number=53267; Label='An item balance change is rejected'; SQL='UPDATE dbo.Item SET Quantity=1 WHERE ID=16912;'},
        @{Number=53268; Label='A PO line movement on the wrong item is rejected'; SQL="INSERT dbo.InventoryTransferLog(ItemID,Quantity,DateTransferred,StoreID,ReferenceID,Type,ReferenceEntryID,Cost,BatchNumber) VALUES(16915,20,'20261103',4001,27784,1,97646,5.2836,2511);"},
        @{Number=53269; Label='Changed repacking deduction is rejected'; SQL='UPDATE dbo.InventoryTransferLog SET Quantity=-239 WHERE ID=106711;'},
        @{Number=53270; Label='An existing integration movement is rejected'; SQL="INSERT dbo.RMSTMP_InventoryMovement(ItemLookupCode,Quantity,StoreID,DateCreated) VALUES(N'11688BOX',20,4001,'2026-11-03T09:31:47');"},
        @{Number=53271; Label='A synchronized transfer document is rejected'; SQL='UPDATE dbo.RMSTMP_TransferInventory SET Synched=1 WHERE ID=7166;'}
    )
    foreach ($scenario in $scenarios) {
        $injection = @($marker, 'BEGIN TRANSACTION;', $scenario.SQL) -join [Environment]::NewLine
        Reject ($local.Replace($marker,$injection)) $scenario.Number $scenario.Label
        Check ((Snapshot) -ceq $baseline) 'No row changes survive the rejected attempt'
    }
    $backup = (Join-Path $directory 'Fixture.bak').Replace("'","''")
    Sql "BACKUP DATABASE [$database] TO DISK=N'$backup' WITH COPY_ONLY,CHECKSUM;"
    $apply = $local.Replace($reviewSetting,$commitSetting).Replace(
        "DECLARE @BackupPath nvarchar(4000) = N'';", "DECLARE @BackupPath nvarchar(4000) = N'$backup';")
    Sql 'ALTER TABLE dbo.InventoryTransferLog ADD CONSTRAINT CK_TestRejectMovement CHECK(ItemID <> 16912);'
    try {
        Reject $apply 547 'Forced native movement insert failure aborts the repair'
        Check ((Snapshot) -ceq $baseline) 'Failure rolls back the stock increment and audit writes'
    } finally { Sql 'ALTER TABLE dbo.InventoryTransferLog DROP CONSTRAINT CK_TestRejectMovement;' }

    Sql 'ALTER TABLE dbo.RMSTMP_InventoryMovement DROP CONSTRAINT DF_FixtureMovementSynched;
         ALTER TABLE dbo.RMSTMP_InventoryMovement ADD CONSTRAINT DF_FixtureMovementSynched DEFAULT 1 FOR Synched;'
    try {
        Reject $apply 53273 'Incorrect integration output fails verification before commit'
        Check ((Snapshot) -ceq $baseline) 'Verification failure rolls back stock, native movement, audits and integration row'
    } finally {
        Sql 'ALTER TABLE dbo.RMSTMP_InventoryMovement DROP CONSTRAINT DF_FixtureMovementSynched;
             ALTER TABLE dbo.RMSTMP_InventoryMovement ADD CONSTRAINT DF_FixtureMovementSynched DEFAULT 0 FOR Synched;'
    }

    $unaffected = Snapshot -Unaffected
    Check ((Sql $apply -Scalar) -eq 'COMMITTED') 'Commit verifies the backup and posts the receipt'
    Check ((Sql 'SELECT Quantity FROM dbo.Item WHERE ID=16912;' -Scalar) -eq 20) '11688BOX stock is exactly 20'
    Check ((Sql 'SELECT COUNT(*) FROM dbo.InventoryTransferLog WHERE ItemID=16912 AND ReferenceID=27784 AND ReferenceEntryID=97646 AND Quantity=20 AND ComputedQuantity=20 AND BatchNumber=2511 AND Cost=5.2836 AND CashierID=0;' -Scalar) -eq 1) 'Exactly one native movement uses the original PO, line, cost and batch'
    Check ((Sql 'SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ItemID=16912 AND LastQuantity=0 AND NewQuantity=20;' -Scalar) -eq 2) 'Davao triggers write two audit views of the same 20-box movement'
    Check ((Sql "SELECT COUNT(*) FROM dbo.RMSTMP_InventoryMovement WHERE ItemLookupCode=N'11688BOX' AND Quantity=20 AND StoreID=4001 AND Synched=0;" -Scalar) -eq 1) 'One normal integration movement is queued'
    Check ((Snapshot -Unaffected) -ceq $unaffected) 'POs, receipt lines, source deduction, sibling stock and pending transfer document are preserved'
    $committed = Snapshot
    Reject $apply 53268 'Second commit is rejected as already posted'
    Check ((Snapshot) -ceq $committed) 'Retry adds no stock or log rows'
    Write-Output ("PASS: $checks localhost checks. Evidence: $directory")
} finally {
    try {
        if ($created -and $connection.State -eq [System.Data.ConnectionState]::Open) {
            Sql 'IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;'
            $connection.ChangeDatabase('master')
            Sql "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];"
        }
    } finally { $connection.Dispose() }
}
