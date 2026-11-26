# Owned disposable LocalDB instance only; never connects to the restored store.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$instance='SOD_Consignment_'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$fixtureDirectory=Join-Path ([IO.Path]::GetTempPath()) $instance
$null=New-Item -ItemType Directory -Path $fixtureDirectory
$scriptText=Get-Content (Join-Path $PSScriptRoot '../Migrations/Add_WorkOrder_Username_Columns.sql') -Raw
$cx=$null;$created=$false;$checks=0
function Sql($text,$tx=$null) {
    $cmd=$cx.CreateCommand()
    try {$cmd.CommandText=$text;$cmd.Transaction=$tx;return $cmd.ExecuteScalar()} finally {$cmd.Dispose()}
}
function Check($ok,$name) {if(-not $ok){throw $name};$script:checks++;Write-Output "PASS: $name"}
function Apply {
    $tx=$cx.BeginTransaction()
    try {$null=Sql $scriptText $tx;$tx.Commit()} catch {$tx.Rollback();throw} finally {$tx.Dispose()}
}
try {
    & SqlLocalDB.exe create $instance -s | Out-Null
    if($LASTEXITCODE -ne 0){throw 'LocalDB creation failed'};$created=$true
    $cx=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False")
    $cx.Open()
    try {Apply;throw 'Wrong target accepted'} catch {Check ($_.Exception.ToString() -like '*System databases are not supported*') 'System database rejected'}
    $dataFile=(Join-Path $fixtureDirectory 'fixture.mdf').Replace("'","''")
    $logFile=(Join-Path $fixtureDirectory 'fixture_log.ldf').Replace("'","''")
    $null=Sql "CREATE DATABASE Portable_Custom_Store ON (NAME=N'FixtureData',FILENAME=N'$dataFile') LOG ON (NAME=N'FixtureLog',FILENAME=N'$logFile');"
    $cx.ChangeDatabase('Portable_Custom_Store')
    try {Apply;throw 'Missing table accepted'} catch {Check ($_.Exception.ToString() -like '*Existing dbo.SOD_WO_Conf table is required*') 'Missing table rejected without creating settings'}
    $null=Sql 'CREATE VIEW dbo.SOD_WO_Conf AS SELECT 1 AS id;'
    try {Apply;throw 'View accepted'} catch {Check ($_.Exception.ToString() -like '*Existing dbo.SOD_WO_Conf table is required*') 'View conflict rejected'}
    $null=Sql 'DROP VIEW dbo.SOD_WO_Conf;'
    $null=Sql "CREATE TABLE dbo.SOD_WO_Conf(id int PRIMARY KEY,Password nvarchar(50),Password2 nvarchar(50),Password3 nvarchar(50),StoreID int); INSERT dbo.SOD_WO_Conf VALUES(1,N'synthetic',N'synthetic2',N'synthetic3',42);"
    try {$null=Sql $scriptText;throw 'Missing transaction accepted'} catch {Check ($_.Exception.ToString() -like '*active transaction*') 'Missing transaction rejected'}
    $tx=$cx.BeginTransaction()
    try {$null=Sql $scriptText $tx;$tx.Rollback()} finally {$tx.Dispose()}
    Check ((Sql "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_WO_Conf') AND name LIKE 'Pass%Username'") -eq 0) 'Rollback removes both additions'
    Apply
    Check ((Sql "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_WO_Conf') AND name IN('Pass2Username','Pass3Username') AND system_type_id=231 AND max_length=100 AND is_nullable=1") -eq 2) 'Both mapped nullable fields added'
    Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Password=N'synthetic' AND Password2=N'synthetic2' AND Password3=N'synthetic3' AND StoreID=42 AND Pass2Username IS NULL AND Pass3Username IS NULL") -eq 1) 'Existing settings preserved and new fields empty'
    $null=Sql "UPDATE dbo.SOD_WO_Conf SET Pass2Username=N'synthetic-user';"
    Apply
    Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Pass2Username=N'synthetic-user'") -eq 1) 'Repeat execution preserves values'
    $null=Sql 'ALTER TABLE dbo.SOD_WO_Conf DROP COLUMN Pass3Username;'
    Apply
    Check ((Sql "SELECT COUNT(*) FROM dbo.SOD_WO_Conf WHERE Pass2Username=N'synthetic-user' AND Pass3Username IS NULL") -eq 1) 'Partial schema repaired without replacing existing value'
    $null=Sql 'ALTER TABLE dbo.SOD_WO_Conf ALTER COLUMN Pass2Username nvarchar(100) NULL; ALTER TABLE dbo.SOD_WO_Conf DROP COLUMN Pass3Username;'
    try {Apply;throw 'Incompatible field accepted'} catch {Check ($_.Exception.ToString() -like '*incompatible definition*') 'Incompatible existing field rejected'}
    Check ((Sql "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.SOD_WO_Conf') AND name='Pass3Username'") -eq 0) 'Rejected migration makes no partial addition'
    Write-Output "PASS: $checks checks; owned disposable LocalDB only."
} finally {
    if($cx){
        try {if($cx.State -eq 'Open'){$cx.ChangeDatabase('master');$null=Sql "IF DB_ID('Portable_Custom_Store') IS NOT NULL BEGIN ALTER DATABASE Portable_Custom_Store SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE Portable_Custom_Store; END"}} finally {$cx.Dispose()}
    }
    if($created){& SqlLocalDB.exe stop $instance -k | Out-Null;& SqlLocalDB.exe delete $instance | Out-Null}
}
