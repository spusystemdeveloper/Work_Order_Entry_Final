# Reproduce RMS's cached SELECT @@IDENTITY, then verify the history fix.
# Uses only a GUID-owned LocalDB database and synthetic data.
$ErrorActionPreference='Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$null=[Reflection.Assembly]::LoadFrom((Join-Path $repo 'Database Updater\bin\Debug\Work Order Database Updater.exe'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$instance="HistoryIdentity_$suffix"; $database="HistoryIdentity_$suffix"
$directory=Join-Path $repo "TestResults\HistoryIdentity-$suffix"
$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe).Source
$created=$false; $master=$null; $connection=$null; $checks=0
function Sql([string]$sql,$tx=$null) {
    $cmd=$connection.CreateCommand()
    try {$cmd.CommandText=$sql;$cmd.CommandTimeout=30;$cmd.Transaction=$tx;$null=$cmd.ExecuteNonQuery()} finally {$cmd.Dispose()}
}
function Scalar([string]$sql,$tx=$null) {
    $cmd=$connection.CreateCommand()
    try {$cmd.CommandText=$sql;$cmd.Transaction=$tx;return $cmd.ExecuteScalar()} finally {$cmd.Dispose()}
}
function Check([bool]$ok,[string]$name) {if(-not $ok){throw "FAILED: $name"};$script:checks++;Write-Output "Passed: $name"}
function Reject([scriptblock]$action,[string]$pattern,[string]$name) {
    $failed=$false;try{& $action | Out-Null}catch{if($_.Exception.ToString()-notmatch $pattern){throw};$failed=$true};Check $failed $name
}
$fixSql=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Fix_WorkOrder_History_Identity.sql'))
$enableSql=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Enable_WorkOrder_History.sql'))
try {
    & $localdb create $instance -s | Out-Null
    if($LASTEXITCODE -ne 0){throw 'LocalDB creation failed'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand();try{$cmd.CommandText="CREATE DATABASE [$database];ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL=130;";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $cs="Server=(localdb)\$instance;Database=$database;Integrated Security=True;Pooling=False"
    $connection=[Data.SqlClient.SqlConnection]::new($cs);$connection.Open()
    Sql @'
CREATE TABLE dbo.Item(ID int PRIMARY KEY);
CREATE TABLE dbo.[Order](ID int IDENTITY(10,1) PRIMARY KEY,Type int NOT NULL DEFAULT 2,Closed bit NOT NULL DEFAULT 0,CustomerID int NOT NULL DEFAULT 1,
 SalesRepID int NOT NULL DEFAULT 1,Total money NOT NULL DEFAULT 112,Tax money NOT NULL DEFAULT 12,DefaultTaxChangeReasonCodeID int NOT NULL DEFAULT 0,Comment nvarchar(255) NULL);
CREATE TABLE dbo.OrderEntry(ID int IDENTITY PRIMARY KEY,OrderID int,ItemID int,QuantityOnOrder float,Price money,FullPrice money,Taxable int,TaxChangeReasonCodeID int,Description nvarchar(30),Comment nvarchar(50));
CREATE TABLE dbo.Queueing(id bigint IDENTITY PRIMARY KEY,OrderID bigint,Status int,GroupTo int,OPIS varchar(max));
CREATE TABLE dbo.QueueingItems(id bigint IDENTITY PRIMARY KEY,QueueingID bigint,ItemID int,QtyPre int,Picker nvarchar(50),Status varchar(50),PickLoc varchar(max));
CREATE TABLE dbo.[Transaction](TransactionNumber int IDENTITY(1000,1) PRIMARY KEY,RecallID int,CashierID int);
CREATE TABLE dbo.TransactionEntry(ID int IDENTITY PRIMARY KEY,TransactionNumber int,ItemID int,Quantity float,Price money);
CREATE TABLE dbo.TenderEntry(ID int IDENTITY PRIMARY KEY,TransactionNumber int,Amount money);
CREATE TABLE dbo.SOD_WO_LOGS(id int IDENTITY PRIMARY KEY,OrderId int,ItemID int,FullPrice money,UpdatedPrice money,ApprovedUser nvarchar(50),Status nvarchar(50));
CREATE TABLE dbo.SOD_WO_History(EventID bigint IDENTITY,OrderID int NOT NULL,
 EventTime datetime2(3) NOT NULL CONSTRAINT DF_SOD_WO_History_Time DEFAULT SYSDATETIME(),EventName nvarchar(60) NOT NULL,
 Actor nvarchar(128) NOT NULL,Terminal nvarchar(256) NOT NULL,ItemID int NULL,Details nvarchar(2000) NOT NULL,
 CONSTRAINT PK_SOD_WO_History PRIMARY KEY CLUSTERED(EventID));
CREATE INDEX IX_SOD_WO_History_Order ON dbo.SOD_WO_History(OrderID,EventID DESC);
INSERT dbo.[Order](Comment) VALUES(N'fixture order');
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES(10,0,10,'TEST');
INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details) VALUES(10,N'Existing event',N'Fixture',N'Test',N'Preserve Ω and every event field.');
CREATE USER HistoryReader WITHOUT LOGIN;
GRANT SELECT ON dbo.SOD_WO_History TO HistoryReader;
DENY UPDATE ON dbo.SOD_WO_History TO HistoryReader;
'@
    Sql @'
CREATE TRIGGER dbo.SOD_trg_WO_History_Invoice ON dbo.[Transaction] AFTER INSERT AS
BEGIN SET NOCOUNT ON;
 INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details)
 SELECT RecallID,N'RMS transaction posted',N'Fixture',N'Test',CONVERT(nvarchar(30),TransactionNumber) FROM inserted;
END;
'@
    $wrong=Scalar 'INSERT dbo.[Transaction](RecallID,CashierID) VALUES(10,1);SELECT CONVERT(int,@@IDENTITY);'
    Check ($wrong -eq 2 -and (Scalar 'SELECT MAX(TransactionNumber) FROM dbo.[Transaction]') -eq 1000) 'Legacy history reproduces RMS receiving event ID instead of transaction number'
    Reject {Sql $enableSql} 'Apply Fix_WorkOrder_History_Identity' 'New history installer refuses to retain the unsafe legacy identity'
    Reject {Sql $fixSql} 'updater transaction' 'History conversion requires an atomic caller transaction'
    $before=Scalar 'SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH(''Event''),ROOT(''History'')'
    Sql 'CREATE INDEX CustomHistoryIndex ON dbo.SOD_WO_History(ItemID);'
    $tx=$connection.BeginTransaction()
    try {Reject {Sql $fixSql $tx} 'custom dependencies' 'Custom history indexes stop conversion before data changes';if($tx.Connection){$tx.Rollback()}}finally{$tx.Dispose()}
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 1 -and (Scalar 'SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH(''Event''),ROOT(''History'')') -eq $before) 'Rejected conversion preserves the original identity and events'
    Sql 'DROP INDEX CustomHistoryIndex ON dbo.SOD_WO_History;'
    $tx=$connection.BeginTransaction()
    try {Sql $fixSql $tx;$tx.Rollback()}finally{$tx.Dispose()}
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 1 -and (Scalar "SELECT COUNT(*) FROM sys.sequences WHERE name='SOD_WO_History_EventSequence'") -eq 0 -and (Scalar 'SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH(''Event''),ROOT(''History'')') -eq $before) 'Rolling back a completed conversion restores the original table and events'
    $fix=[WorkOrderDatabaseUpdater.UpdateScript]::Load((Join-Path $repo 'Database Components\Migrations\Fix_WorkOrder_History_Identity.sql'),'Migrations/Fix_WorkOrder_History_Identity.sql','Compatibility fix','Test',$false)
    $null=[WorkOrderDatabaseUpdater.UpdateRunner]::Apply($cs,$database,[WorkOrderDatabaseUpdater.UpdateScript[]]@($fix),(Join-Path $directory 'Runs'))
    $after=Scalar 'SELECT * FROM dbo.SOD_WO_History ORDER BY EventID FOR XML PATH(''Event''),ROOT(''History'')'
    Check ($before -eq $after) 'Legacy conversion preserves every history ID and event field'
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 0) 'History no longer generates a SQL identity'
    Check ((Scalar "SELECT COUNT(*) FROM sys.database_permissions WHERE major_id=OBJECT_ID(N'dbo.SOD_WO_History') AND grantee_principal_id=USER_ID(N'HistoryReader') AND ((permission_name='SELECT' AND state='G') OR(permission_name='UPDATE' AND state='D'))") -eq 2) 'Existing grant and deny permissions survive conversion'
    Sql $enableSql
    Sql @'
CREATE TRIGGER dbo.trg_Transaction_closeQueue ON dbo.[Transaction] AFTER INSERT AS
BEGIN SET NOCOUNT ON; UPDATE q SET Status=1 FROM dbo.Queueing q JOIN inserted i ON q.OrderID=i.RecallID; END;
'@
    $rmsId=Scalar 'INSERT dbo.[Transaction](RecallID,CashierID) VALUES(10,1);SELECT CONVERT(int,@@IDENTITY);'
    Check ($rmsId -eq 1001) 'RMS @@IDENTITY survives invoice plus nested queue-history triggers'
    Sql "INSERT dbo.TransactionEntry(TransactionNumber,ItemID,Quantity,Price) VALUES($rmsId,1,1,112);INSERT dbo.TenderEntry(TransactionNumber,Amount) VALUES($rmsId,112);"
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.TransactionEntry e JOIN dbo.[Transaction] t ON t.TransactionNumber=e.TransactionNumber WHERE t.TransactionNumber=1001') -eq 1 -and (Scalar 'SELECT COUNT(*) FROM dbo.TenderEntry WHERE TransactionNumber=1001') -eq 1) 'Legacy identity-based posting links item and tender to the real transaction'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Queue completed'") -eq 1 -and (Scalar 'SELECT Status FROM dbo.Queueing WHERE OrderID=10') -eq 1) 'Queue closure and audit events remain active'
    $orderId=Scalar "INSERT dbo.[Order](Comment) VALUES(N'new legacy order');SELECT CONVERT(int,@@IDENTITY);"
    Check ($orderId -eq 11) 'RMS order creation keeps its own identity with auditing enabled'
    $entryId=Scalar 'INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Taxable,TaxChangeReasonCodeID,Description,Comment) VALUES(11,1,1,112,112,1,0,N''Fixture'',N'''');SELECT CONVERT(int,@@IDENTITY);'
    Check ($entryId -eq 1) 'OrderEntry creation keeps its own identity with auditing enabled'
    $nextBefore=Scalar 'SELECT MAX(EventID) FROM dbo.SOD_WO_History'
    $null=[WorkOrderDatabaseUpdater.UpdateRunner]::Apply($cs,$database,[WorkOrderDatabaseUpdater.UpdateScript[]]@($fix),(Join-Path $directory 'Runs'))
    Check ((Scalar 'SELECT MAX(EventID) FROM dbo.SOD_WO_History') -eq $nextBefore) 'Repeated identity fix neither backfills events nor restarts IDs'
    Sql "INSERT dbo.SOD_WO_History(OrderID,EventName,Actor,Terminal,Details) VALUES(11,N'Manual event',N'Fixture',N'Test',N'Next value');"
    Check ((Scalar 'SELECT MAX(EventID) FROM dbo.SOD_WO_History') -gt $nextBefore) 'New sequence event IDs continue beyond all preserved IDs'
    $eventCount=Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History'
    $tx=$connection.BeginTransaction()
    try {Sql "UPDATE dbo.[Order] SET Comment=N'rollback event' WHERE ID=11;" $tx;$tx.Rollback()}finally{$tx.Dispose()}
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History') -eq $eventCount) 'Audit rows still roll back with their business transaction'
    # A fresh installation uses the same sequence-backed contract.
    Sql 'DROP TABLE dbo.SOD_WO_History;DROP SEQUENCE dbo.SOD_WO_History_EventSequence;'
    Sql $enableSql
    Check ((Scalar "SELECT COLUMNPROPERTY(OBJECT_ID(N'dbo.SOD_WO_History'),N'EventID','IsIdentity')") -eq 0) 'Fresh history installation is identity-safe'
    $freshId=Scalar "INSERT dbo.[Order](Comment) VALUES(N'fresh install');SELECT CONVERT(int,@@IDENTITY);"
    Check ($freshId -eq 12) 'Fresh-install audit defaults preserve the RMS identity result'
    Sql $enableSql
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History') -eq 1) 'Fresh sequence installation remains idempotent'
    Write-Output "PASS: $checks identity compatibility checks; artifacts: $directory"
} finally {
    if($connection){$connection.Dispose()}
    if($master){try{$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID(N'$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$database];END;";$null=$cmd.ExecuteNonQuery();$cmd.Dispose()}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k | Out-Null;& $localdb delete $instance | Out-Null}
}
