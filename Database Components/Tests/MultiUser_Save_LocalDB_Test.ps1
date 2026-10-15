# Two actual SaveExistingOrderChanges calls in separate x86 STA cashier processes.
# Creates/removes only an owned LocalDB instance/database; NEVER uses live SQL.
param([switch]$VersionedReads)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$assembly=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
if(-not (Test-Path -LiteralPath $assembly)){throw 'Build the Debug application first.'}
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$instance='SODWO_Multi_'+$suffix;$database=$instance;$server='(localdb)\'+$instance
$directory=Join-Path $repo ('TestResults\MultiUser-'+$suffix)
$null=New-Item -ItemType Directory -Path $directory
$worker=Join-Path $PSScriptRoot 'Fixtures\MultiUser_Save_Worker.ps1'
$powershell=Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
$localdb=(Get-Command SqlLocalDB.exe).Source
$master=$null;$connection=$null;$gate=$null;$gateTransaction=$null;$created=$false;$checks=0
$processes=[Collections.Generic.List[object]]::new()
$connectionString="Server=$server;Database=$database;Integrated Security=True;Pooling=False;Application Name=MultiUser_Coordinator;"
function Sql([string]$text,$transaction=$null){$cmd=$connection.CreateCommand();try{$cmd.CommandTimeout=20;$cmd.CommandText=$text;$cmd.Transaction=$transaction;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
function Scalar([string]$text){$cmd=$connection.CreateCommand();try{$cmd.CommandTimeout=10;$cmd.CommandText=$text;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}}
function Check([bool]$ok,[string]$name){if(-not $ok){throw "FAILED: $name"};$script:checks++;Write-Output "Passed: $name"}
function Wait-For([scriptblock]$condition,[string]$description,[int]$seconds=15){$end=[DateTime]::UtcNow.AddSeconds($seconds);while(-not (& $condition)){if([DateTime]::UtcNow -gt $end){throw "Timed out: $description"};Start-Sleep -Milliseconds 40}}
function Start-Cashier([string]$case,[string]$who,[int]$order,[int]$item,[int]$entry,[double]$quantity){
    $stem=Join-Path $directory ($case+'-'+$who)
    $request=[pscustomobject]@{Server=$server;Database=$database;Assembly=$assembly;Actor=('MultiUser_'+$who+'_'+$suffix);OrderID=$order;ItemID=$item;EntryID=$entry;Quantity=$quantity;Comment=($case+'-'+$who);ReadyFile=($stem+'.ready');GoFile=($stem+'.go');ResultFile=($stem+'.result.json')}
    $requestFile=$stem+'.request.json';$request|ConvertTo-Json|Set-Content -LiteralPath $requestFile
    $workerArgs=@('-NoProfile','-STA','-ExecutionPolicy','Bypass','-File',('"'+$worker+'"'),'-RequestFile',('"'+$requestFile+'"'))
    $process=Start-Process -FilePath $powershell -ArgumentList $workerArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput ($stem+'.stdout.log') -RedirectStandardError ($stem+'.stderr.log')
    $processes.Add($process)
    return [pscustomobject]@{Request=$request;Process=$process}
}
function Ready($cashier){Wait-For {if(Test-Path -LiteralPath $cashier.Request.ResultFile){$r=Get-Content $cashier.Request.ResultFile -Raw|ConvertFrom-Json;throw "Cashier initialization failed: $($r.Type): $($r.Message)"};Test-Path -LiteralPath $cashier.Request.ReadyFile} 'cashier initialization'}
function Go($cashier){[IO.File]::WriteAllText($cashier.Request.GoFile,'Go')}
function Result($cashier){Wait-For {Test-Path -LiteralPath $cashier.Request.ResultFile} 'cashier result' 20;$r=Get-Content -LiteralPath $cashier.Request.ResultFile -Raw|ConvertFrom-Json;if($r.Status -eq 'Error'){throw "Cashier failed: $($r.Type): $($r.Message)"};return $r}
function Blocker($cashier){
    return Scalar ("SELECT ISNULL(MAX(r.blocking_session_id),0) FROM sys.dm_exec_sessions s JOIN sys.dm_exec_requests r ON r.session_id=s.session_id WHERE s.program_name=N'"+$cashier.Request.Actor+"' AND r.wait_type LIKE N'LCK_M_%';")
}
function Session($cashier){return Scalar ("SELECT ISNULL(MAX(session_id),0) FROM sys.dm_exec_sessions WHERE program_name=N'"+$cashier.Request.Actor+"';")}
function Request-State($cashier){
    $cmd=$connection.CreateCommand()
    try {
        $cmd.CommandTimeout=5
        $cmd.CommandText=@'
SELECT s.session_id,r.blocking_session_id,r.wait_type,r.wait_resource,
 SUBSTRING(t.text,r.statement_start_offset/2+1,
  (CASE WHEN r.statement_end_offset=-1 THEN DATALENGTH(t.text) ELSE r.statement_end_offset END-r.statement_start_offset)/2+1) Statement
FROM sys.dm_exec_sessions s JOIN sys.dm_exec_requests r ON r.session_id=s.session_id
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t WHERE s.program_name=@actor;
'@
        $null=$cmd.Parameters.AddWithValue('@actor',$cashier.Request.Actor)
        $r=$cmd.ExecuteReader()
        try{if(-not $r.Read()){throw 'Cashier request disappeared before overlap verification.'};$row=[ordered]@{};for($i=0;$i -lt $r.FieldCount;$i++){$row[$r.GetName($i)]=$r.GetValue($i)};return [pscustomobject]$row}finally{$r.Dispose()}
    }finally{$cmd.Dispose()}
}
function Hold-Gate([string]$text){
    $script:gate=[Data.SqlClient.SqlConnection]::new($connectionString);$gate.Open();$script:gateTransaction=$gate.BeginTransaction()
    $cmd=$gate.CreateCommand();try{$cmd.Transaction=$gateTransaction;$cmd.CommandText=$text;$null=$cmd.ExecuteScalar()}finally{$cmd.Dispose()}
    $cmd=$gate.CreateCommand();try{$cmd.Transaction=$gateTransaction;$cmd.CommandText='SELECT @@SPID;';return [int]$cmd.ExecuteScalar()}finally{$cmd.Dispose()}
}
function Release-Gate {if($null -ne $script:gateTransaction){$script:gateTransaction.Rollback();$script:gateTransaction.Dispose();$script:gateTransaction=$null};if($null -ne $script:gate){$script:gate.Dispose();$script:gate=$null}}
try {
    & $localdb create $instance -s|Out-Null;if($LASTEXITCODE -ne 0){throw 'Cannot create isolated LocalDB instance.'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False;");$master.Open()
    $cmd=$master.CreateCommand();try{$cmd.CommandText="CREATE DATABASE [$database]; ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL=130;";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    if($VersionedReads){$cmd=$master.CreateCommand();try{$cmd.CommandText="ALTER DATABASE [$database] SET READ_COMMITTED_SNAPSHOT ON;";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
    [pscustomobject]@{Server=$server;Database=$database;VersionedReads=[bool]$VersionedReads}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'Fixture.json')
    $connection=[Data.SqlClient.SqlConnection]::new($connectionString);$connection.Open()
    # Exact mapped columns/types, with synthetic defaults only. No store data.
    [xml]$mapping=Get-Content (Join-Path $repo 'Work Order Entry\Linq\ItemLookUp.dbml')
    $tables=@('dbo.[Order]','dbo.OrderEntry','dbo.Item','dbo.Customer','dbo.Queueing','dbo.QueueingItems','dbo.SOD_WO_LOGS')
    foreach($table in $mapping.Database.Table){
        if($tables -notcontains [string]$table.Name){continue}
        $definitions=@()
        foreach($column in $table.Type.Column){
            $type=[string]$column.DbType;$name=[string]$column.Name
            $definition='['+$name+'] '+$type
            if($type -notmatch '(?i)IDENTITY|timestamp|rowversion' -and $type -match 'NOT NULL'){
                $default=if($type -match '(?i)char|text'){"N''"}elseif($type -match '(?i)date|time'){"CONVERT(datetime,'20000101',112)"}elseif($type -match '(?i)binary|image'){'0x'}else{'0'}
                $definition+=' DEFAULT ('+$default+')'
            }
            $definitions+=$definition
        }
        Sql ('CREATE TABLE '+$table.Name+'('+($definitions -join ',')+');')
    }
    Sql @'
CREATE TABLE dbo.ReasonCode(ID int PRIMARY KEY,Code nvarchar(100),Type int);
CREATE TABLE dbo.[Transaction](TransactionNumber int IDENTITY PRIMARY KEY,RecallID int,CashierID int);
ALTER TABLE dbo.[Order] ADD PRIMARY KEY(ID);
ALTER TABLE dbo.OrderEntry ADD PRIMARY KEY(ID);
ALTER TABLE dbo.Item ADD PRIMARY KEY(ID);
ALTER TABLE dbo.Queueing ADD PRIMARY KEY(id);
ALTER TABLE dbo.QueueingItems ADD PRIMARY KEY(id);
CREATE INDEX IX_TestEntry_Order ON dbo.OrderEntry(OrderID);
CREATE INDEX IX_TestQueue_Order ON dbo.Queueing(OrderID);
CREATE INDEX IX_TestQueueItem_Header ON dbo.QueueingItems(QueueingID);
INSERT dbo.Customer(TaxExempt) VALUES(0);
INSERT dbo.Item(ItemLookupCode,Description,ItemType,Quantity,QuantityCommitted)
 VALUES(N'FIXTURE-SKU',N'Synthetic multi-user item',0,100,5),(N'FIXTURE-SKU2',N'Synthetic second item',0,50,5),(N'SPACER',N'Unrelated spacer item',0,10,1);
INSERT dbo.[Order](Type,Closed,CustomerID,SalesRepID,Total,Tax,Comment)
 VALUES(2,0,1,1,224,24,N'Shared A'),(2,0,1,1,112,12,N'Unrelated spacer'),(2,0,1,1,336,36,N'Shared B'),(2,0,1,1,560,60,N'Same order');
INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Cost,Taxable,Description)
 VALUES(1,1,2,112,112,80,1,N'Synthetic multi-user item'),(2,3,1,112,112,80,1,N'Unrelated spacer item'),(3,1,3,112,112,80,1,N'Synthetic multi-user item'),(4,2,5,112,112,80,1,N'Synthetic multi-user item');
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES(1,0,1,'FIXTURE'),(2,0,2,'FIXTURE'),(3,0,3,'FIXTURE'),(4,0,4,'FIXTURE');
INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Status,Picker,PickLoc)
 VALUES(1,1,0,'Pending','PICKER','STORE'),(2,3,0,'Pending','PICKER','STORE'),(3,1,0,'Pending','PICKER','STORE'),(4,2,0,'Pending','PICKER','STORE');
-- Give the normal OrderID indexes realistic selectivity. Four-row tables can
-- choose broad scans under HOLDLOCK and serialize unrelated saves before stock.
-- A spacer order separates the two target key ranges; no index hints are added
-- to the application and no production constraint/query is changed.
;WITH numbers AS(SELECT TOP(1000) ROW_NUMBER() OVER(ORDER BY a.object_id) n FROM sys.all_objects a CROSS JOIN sys.all_objects b)
 INSERT dbo.[Order](Type,Closed,CustomerID,SalesRepID,Total,Tax,Comment)
 SELECT 2,1,1,1,0,0,N'Synthetic filler' FROM numbers;
INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Cost,Taxable,Description)
 SELECT ID,3,0,112,112,80,1,N'Synthetic filler' FROM dbo.[Order] WHERE ID>4;
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS)
 SELECT ID,1,ID,'FIXTURE' FROM dbo.[Order] WHERE ID>4;
INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Status,Picker,PickLoc)
 SELECT id,3,0,'Prepared','PICKER','STORE' FROM dbo.Queueing WHERE OrderID>4;
UPDATE STATISTICS dbo.OrderEntry WITH FULLSCAN;
UPDATE STATISTICS dbo.Queueing WITH FULLSCAN;
UPDATE STATISTICS dbo.QueueingItems WITH FULLSCAN;
CREATE TABLE dbo.TestSaveGate(ID int PRIMARY KEY);
INSERT dbo.TestSaveGate VALUES(1);
'@
    $history=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Enable_WorkOrder_History.sql'))
    Sql ('BEGIN TRANSACTION;'+$history+' COMMIT;')
    # Case 1: hold a shared stock lock while saves overlap. With default locking,
    # a queued X request can block the second reader. Optional fixture-only RCSI
    # forces that reader to see the old committed item and reach its own UPDATE.
    $gatePID=Hold-Gate 'SELECT QuantityCommitted FROM dbo.Item WITH(ROWLOCK,HOLDLOCK) WHERE ID=1;'
    $a=Start-Cashier 'SharedItem' 'A' 1 1 1 7.5
    $b=Start-Cashier 'SharedItem' 'B' 3 1 3 4.25
    Ready $a;Ready $b;Go $a;Go $b
    Wait-For {((Blocker $a) -gt 0) -and ((Blocker $b) -gt 0)} 'both stock writers blocked concurrently' 8
    $aPID=Session $a;$bPID=Session $b
    Check ($aPID -gt 0 -and $bPID -gt 0 -and $aPID -ne $bPID) 'Shared-item saves use independent SQL sessions in separate cashier processes'
    Check ((Blocker $a) -in @($gatePID,$bPID) -and (Blocker $b) -in @($gatePID,$aPID)) 'Both actual save transactions overlap while stock writes wait on the controlled lock'
    $aState=Request-State $a;$bState=Request-State $b
    @($aState,$bState)|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'SharedItem-Overlap.json')
    $stockUpdate='(?is)UPDATE\s+(?:\[?dbo\]?\.)?\[?Item\]?(?:\s|$)'
    if($VersionedReads){
        Check ($aState.Statement -match $stockUpdate -and $bState.Statement -match $stockUpdate) 'Versioned-read race proves both cashiers reached stock UPDATE before either stock write completes'
    }else{
        $stockRead='(?is)FROM\s+\[dbo\]\.\[Item\]'
        Check (($aState.Statement -match $stockUpdate -or $bState.Statement -match $stockUpdate) -and
               ($aState.Statement -match $stockUpdate -or $aState.Statement -match $stockRead) -and
               ($bState.Statement -match $stockUpdate -or $bState.Statement -match $stockRead)) 'Default-locking saves overlap at the actual stock read/update, including queued reader blocking'
    }
    Release-Gate
    $ar=Result $a;$br=Result $b
    Check ($ar.Status -eq 'Committed' -and $br.Status -eq 'Committed') 'Both different-order saves commit successfully'
    Check ((Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=1;') -eq 11.75) 'Both fractional commitment differences survive: 5 + 5.5 + 1.25 = 11.75'
    Check ((Scalar 'SELECT SUM(QuantityOnOrder) FROM dbo.OrderEntry WHERE ItemID=1;') -eq 11.75) 'Shared item commitment equals both saved order quantities'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE (ID=1 AND OrderID=1 AND ItemID=1 AND QuantityOnOrder=7.5 AND Price=112 AND FullPrice=112 AND Cost=80) OR (ID=3 AND OrderID=3 AND ItemID=1 AND QuantityOnOrder=4.25 AND Price=112 AND FullPrice=112 AND Cost=80);') -eq 2) 'Each shared-item save updates only its own original line and preserves prices/cost'
    Check ((Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=1;') -eq 840 -and (Scalar 'SELECT Tax FROM dbo.[Order] WHERE ID=1;') -eq 90 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=3;') -eq 476 -and (Scalar 'SELECT Tax FROM dbo.[Order] WHERE ID=3;') -eq 51) 'Both orders retain their own amounts and taxes'
    Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=1;') -eq 100 -and (Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE ID<=4;') -eq 4) 'Concurrent edits preserve physical stock and existing line identities'
    Check ((Scalar "SELECT COUNT(DISTINCT Actor) FROM dbo.SOD_WO_History WHERE OrderID IN(1,3) AND EventName='Item edited';") -eq 2) 'Both committed saves record their own cashier history'
    # Case 2: pause A inside the real header UPDATE, after the application lock.
    # B has already recalled the same baseline; prove B waits on A's app lock.
    Sql @'
CREATE TRIGGER dbo.TestPauseFirstCashier ON dbo.[Order] AFTER UPDATE AS
BEGIN
 SET NOCOUNT ON;
 IF APP_NAME() LIKE N'MultiUser_A_%' AND EXISTS(SELECT 1 FROM inserted WHERE ID=4)
 BEGIN
  DECLARE @gate int;
  SELECT @gate=ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;
 END;
END;
'@
    $gatePID=Hold-Gate 'SELECT ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;'
    $a=Start-Cashier 'SameOrder' 'A' 4 2 4 7
    $b=Start-Cashier 'SameOrder' 'B' 4 2 4 9
    Ready $a;Ready $b;Go $a
    Wait-For {(Blocker $a) -eq $gatePID} 'first same-order save paused inside header update' 8
    $aPID=Session $a;Go $b
    Wait-For {(Blocker $b) -eq $aPID} 'second same-order save waits for first' 8
    @((Request-State $a),(Request-State $b))|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'SameOrder-Overlap.json')
    Check ((Scalar ("SELECT COUNT(*) FROM sys.dm_tran_locks WHERE request_session_id="+(Session $b)+" AND resource_type='APPLICATION' AND request_status='WAIT';")) -eq 1) 'Second complete save waits on the first cashier application lock'
    Release-Gate
    $ar=Result $a;$br=Result $b
    Check ($ar.Status -eq 'Committed' -and $br.Status -eq 'Conflict') 'First complete save commits; the stale second save raises OrderEditConflictException'
    Check ((Scalar 'SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE ID=4;') -eq 7 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=2;') -eq 7) 'Rejected stale save cannot overwrite quantity or double-adjust commitments'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.[Order] WHERE ID=4 AND Total=784 AND Tax=84 AND Comment=N'SameOrder-A';") -eq 1) 'Rejected save cannot overwrite the winning header, total or tax'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.Queueing WHERE OrderID=4 AND Status=0 AND GroupTo=4 AND OPIS='FIXTURE';") -eq 1 -and (Scalar "SELECT COUNT(*) FROM dbo.QueueingItems WHERE QueueingID=4 AND QtyPre=0 AND Status='Pending' AND Picker='PICKER';") -eq 1) 'Queue status, grouping, owner and picker remain consistent after stale rejection'
    Check ((Scalar ("SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=4 AND Actor=N'"+$b.Request.Actor+"';")) -eq 0 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=4 AND EventName='Item edited';") -eq 1) 'Stale save leaves no partial audit history; the winning item edit is logged once'
    Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=2;') -eq 50 -and (Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE ID<=4;') -eq 4) 'Same-order concurrency preserves physical stock and all existing line identities'
    Write-Output "PASS: $checks multi-user save checks; VersionedReads=$([bool]$VersionedReads); artifacts: $directory"
} finally {
    Release-Gate
    foreach($process in $processes){if(-not $process.HasExited){if(-not $process.WaitForExit(1500)){Stop-Process -Id $process.Id -Force}};$process.Dispose()}
    if($null -ne $connection){$connection.Dispose()}
    if($null -ne $master){try{if($master.State -eq 'Open' -and $database -match '^SODWO_Multi_[a-f0-9]{8}$'){$cmd=$master.CreateCommand();try{$cmd.CommandText="ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
