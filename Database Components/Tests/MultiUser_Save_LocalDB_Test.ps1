# Concurrent save/edit/close and rollback checks in separate x86 STA processes.
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
function Start-Cashier([string]$case,[string]$who,[int]$order,[int]$item,[int]$entry,[double]$quantity,[string]$operation='Save',[object[]]$lines=$null){
    $stem=Join-Path $directory ($case+'-'+$who)
    $request=[pscustomobject]@{Server=$server;Database=$database;Assembly=$assembly;Actor=('MultiUser_'+$who+'_'+$suffix);OrderID=$order;ItemID=$item;EntryID=$entry;Quantity=$quantity;Operation=$operation;Lines=$lines;Comment=($case+'-'+$who);ReadyFile=($stem+'.ready');GoFile=($stem+'.go');ResultFile=($stem+'.result.json')}
    $requestFile=$stem+'.request.json';$request|ConvertTo-Json|Set-Content -LiteralPath $requestFile
    $workerArgs=@('-NoProfile','-STA','-ExecutionPolicy','Bypass','-File',('"'+$worker+'"'),'-RequestFile',('"'+$requestFile+'"'))
    $process=Start-Process -FilePath $powershell -ArgumentList $workerArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput ($stem+'.stdout.log') -RedirectStandardError ($stem+'.stderr.log')
    $processes.Add($process)
    return [pscustomobject]@{Request=$request;Process=$process}
}
function Ready($cashier){Wait-For {if(Test-Path -LiteralPath $cashier.Request.ResultFile){$r=Get-Content $cashier.Request.ResultFile -Raw|ConvertFrom-Json;throw "Cashier initialization failed: $($r.Type): $($r.Message)"};Test-Path -LiteralPath $cashier.Request.ReadyFile} 'cashier initialization'}
function Go($cashier){[IO.File]::WriteAllText($cashier.Request.GoFile,'Go')}
function Result($cashier,[switch]$AllowError){Wait-For {Test-Path -LiteralPath $cashier.Request.ResultFile} 'cashier result' 20;$r=Get-Content -LiteralPath $cashier.Request.ResultFile -Raw|ConvertFrom-Json;if($r.Status -eq 'Error' -and -not $AllowError){throw "Cashier failed: $($r.Type): $($r.Message)"};return $r}
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
function New-FixtureItem([double]$physical,[double]$committed){return [int](Scalar ("INSERT dbo.Item(ItemLookupCode,Description,ItemType,Quantity,QuantityCommitted) VALUES(N'NEW-FIXTURE',N'Synthetic race item',0,$physical,$committed);SELECT CONVERT(int,SCOPE_IDENTITY());"))}
function New-FixtureOrder([int[]]$items,[double[]]$quantities){
    $sum=($quantities|Measure-Object -Sum).Sum
    $order=[int](Scalar ("INSERT dbo.[Order](Type,Closed,CustomerID,SalesRepID,Total,Tax,Comment) VALUES(2,0,1,1,"+($sum*112)+","+($sum*12)+",N'Closing/failure fixture'); SELECT CONVERT(int,SCOPE_IDENTITY());"))
    $queue=[int](Scalar "INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES($order,0,$order,'FIXTURE');SELECT CONVERT(int,SCOPE_IDENTITY());")
    $lines=@()
    for($i=0;$i -lt $items.Count;$i++){
        $entry=[int](Scalar ("INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice,Cost,Taxable,Description) VALUES($order,"+$items[$i]+","+$quantities[$i]+",112,112,80,1,N'Synthetic race item');SELECT CONVERT(int,SCOPE_IDENTITY());"))
        $qi=[int](Scalar ("INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Status,Picker,PickLoc) VALUES($queue,"+$items[$i]+",0,'Pending','PICKER','STORE');SELECT CONVERT(int,SCOPE_IDENTITY());"))
        $lines += [pscustomobject]@{ItemID=$items[$i];EntryID=$entry;QueueItemID=$qi;Quantity=$quantities[$i];PreparedQuantity=0}
    }
    return [pscustomobject]@{OrderID=$order;QueueID=$queue;Lines=$lines}
}
function Fixture-State($fixture){
    $order=$fixture.OrderID;$itemList=($fixture.Lines|ForEach-Object {$_.ItemID}) -join ','
    return Scalar @"
SELECT CONVERT(nvarchar(max),(SELECT
 (SELECT * FROM dbo.[Order] WHERE ID=$order FOR XML RAW('Order'),BINARY BASE64,TYPE),
 (SELECT * FROM dbo.OrderEntry WHERE OrderID=$order ORDER BY ID FOR XML RAW('Entry'),BINARY BASE64,TYPE),
 (SELECT * FROM dbo.Item WHERE ID IN($itemList) ORDER BY ID FOR XML RAW('Stock'),BINARY BASE64,TYPE),
 (SELECT * FROM dbo.Queueing WHERE OrderID=$order ORDER BY id FOR XML RAW('Queue'),BINARY BASE64,TYPE),
 (SELECT qi.* FROM dbo.QueueingItems qi JOIN dbo.Queueing q ON q.id=qi.QueueingID WHERE q.OrderID=$order ORDER BY qi.id FOR XML RAW('Picking'),BINARY BASE64,TYPE),
 (SELECT * FROM dbo.SOD_WO_History WHERE OrderID=$order ORDER BY EventID FOR XML RAW('History'),BINARY BASE64,TYPE),
 (SELECT * FROM dbo.SOD_WO_LOGS WHERE OrderID=$order ORDER BY id FOR XML RAW('Approval'),BINARY BASE64,TYPE)
 FOR XML PATH('State'),TYPE));
"@
}
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
ALTER TABLE dbo.[Transaction] ADD StoreID int NOT NULL DEFAULT 1,RecallType int NOT NULL DEFAULT 4,
 Total money NOT NULL DEFAULT 0,SalesTax money NOT NULL DEFAULT 0;
CREATE TABLE dbo.TransactionEntry(ID int IDENTITY PRIMARY KEY,
 TransactionNumber int NOT NULL REFERENCES dbo.[Transaction](TransactionNumber),
 ItemID int NOT NULL,Quantity float NOT NULL,Price money NOT NULL,SalesTax money NOT NULL);
CREATE TABLE dbo.TaxEntry(ID int IDENTITY PRIMARY KEY,
 TransactionNumber int NOT NULL REFERENCES dbo.[Transaction](TransactionNumber),
 TransactionEntryID int NOT NULL REFERENCES dbo.TransactionEntry(ID),SalesTax money NOT NULL);
CREATE TABLE dbo.TenderEntry(ID int IDENTITY PRIMARY KEY,
 TransactionNumber int NOT NULL REFERENCES dbo.[Transaction](TransactionNumber),Amount money NOT NULL);
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
    # Exercise the exact packaged legacy queue-close trigger, not a rewritten
    # substitute. GO appears only as batch separators in this trusted export.
    $closeQueue=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Deployment\Objects\Common\Triggers\trg_Transaction_closeQueue.sql'))
    foreach($batch in [regex]::Split($closeQueue,'(?im)^\s*GO\s*$')){if(-not [string]::IsNullOrWhiteSpace($batch)){Sql $batch}}
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
    # Case 3: actual cancellation wins while an already-recalled Save waits.
    $cancelItem=New-FixtureItem 60 8
    $cancel=New-FixtureOrder @($cancelItem) @(5)
    $other=New-FixtureOrder @($cancelItem) @(3)
    $cancelID=$cancel.OrderID;$cancelEntry=$cancel.Lines[0].EntryID
    $a=Start-Cashier 'SaveVsCancel' 'A' $cancelID $cancelItem $cancelEntry 7
    $b=Start-Cashier 'SaveVsCancel' 'B' $cancelID $cancelItem $cancelEntry 5 'Cancel'
    Ready $a;Ready $b
    $gatePID=Hold-Gate "SELECT QuantityCommitted FROM dbo.Item WITH(ROWLOCK,HOLDLOCK) WHERE ID=$cancelItem;"
    Go $b
    Wait-For {(Blocker $b) -eq $gatePID} 'cancellation paused at stock release' 8
    $bPID=Session $b;Go $a
    Wait-For {(Blocker $a) -eq $bPID} 'stale save waits for cancellation' 8
    @((Request-State $a),(Request-State $b))|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'SaveVsCancel-Overlap.json')
    Check ((Scalar ("SELECT COUNT(*) FROM sys.dm_tran_locks WHERE request_session_id="+(Session $a)+" AND resource_type='APPLICATION' AND request_status='WAIT';")) -eq 1) 'Save overlaps actual cancellation and waits on its application lock'
    Release-Gate
    $br=Result $b;$ar=Result $a
    Check ($br.Status -eq 'Committed' -and $ar.Status -eq 'Conflict') 'Cancellation commits and the previously recalled save is rejected'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.[Order] WHERE ID=$cancelID AND Closed=1 AND Total=560 AND Tax=60;") -eq 1 -and (Scalar "SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE ID=$cancelEntry;") -eq 0) 'Stale save cannot reopen cancellation or overwrite its zeroed line'
    Check ((Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=$cancelItem;") -eq 3 -and (Scalar "SELECT Quantity FROM dbo.Item WHERE ID=$cancelItem;") -eq 60 -and (Scalar ("SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID="+$other.OrderID+";")) -eq 3) 'Cancellation releases only its five reserved pieces and preserves physical stock and another order'
    Check ((Scalar "SELECT Status FROM dbo.Queueing WHERE OrderID=$cancelID;") -eq 2 -and (Scalar ("SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$cancelID AND Actor=N'"+$a.Request.Actor+"';")) -eq 0) 'Cancelled queue status survives without partial history from the rejected save'
    $cancelState=Fixture-State $cancel
    $repeat=Start-Cashier 'CancelAgain' 'B' $cancelID $cancelItem $cancelEntry 5 'Cancel'
    $rr=Result $repeat
    Check ($rr.Status -eq 'Conflict' -and (Fixture-State $cancel) -ceq $cancelState) 'Re-recall for another cancellation is rejected without releasing stock or writing history twice'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$cancelID AND EventName='Order cancelled';") -eq 1) 'Cancellation history is recorded exactly once'

    # Case 4: a simulated POS settles without the add-in application lock.
    # The stale save must wait on the real Order row lock and then see Closed.
    $settleItem=New-FixtureItem 40 8
    $settle=New-FixtureOrder @($settleItem) @(5)
    $settleOther=New-FixtureOrder @($settleItem) @(3)
    $settleID=$settle.OrderID;$settleEntry=$settle.Lines[0].EntryID
    $a=Start-Cashier 'SaveVsSettlement' 'A' $settleID $settleItem $settleEntry 7
    $b=Start-Cashier 'SaveVsSettlement' 'B' $settleID $settleItem $settleEntry 5 'Settle'
    Ready $a;Ready $b
    $gatePID=Hold-Gate 'SELECT ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;'
    Go $b
    Wait-For {(Blocker $b) -eq $gatePID} 'simulated POS paused before settlement commit' 8
    $bPID=Session $b;Go $a
    Wait-For {(Blocker $a) -eq $bPID} 'stale save waits on POS Order lock' 8
    $aState=Request-State $a;$bState=Request-State $b
    @($aState,$bState)|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'SaveVsSettlement-Overlap.json')
    Check ($aState.Statement -match '(?is)SELECT Closed FROM dbo\.\[Order\]' -and $aState.wait_type -like 'LCK_M_*') 'Save overlaps settlement and waits on the POS-held Order lock, not a simulated add-in lock'
    Release-Gate
    $br=Result $b;$ar=Result $a;$number=[int]$br.TransactionNumber
    Check ($br.Status -eq 'Committed' -and $ar.Status -eq 'Conflict') 'Simulated POS settlement commits and the stale save is rejected'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.[Order] WHERE ID=$settleID AND Closed=1 AND Total=560 AND Tax=60;") -eq 1 -and (Scalar "SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE ID=$settleEntry;") -eq 5) 'Settled order stays closed with its original quantity and amounts'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.[Transaction] WHERE TransactionNumber=$number AND RecallID=$settleID AND RecallType=4 AND Total=560 AND SalesTax=60;") -eq 1 -and (Scalar "SELECT COUNT(*) FROM dbo.TransactionEntry WHERE TransactionNumber=$number AND ItemID=$settleItem AND Quantity=5 AND Price=112 AND SalesTax=60;") -eq 1) 'Legacy identity lookup links the settlement header and item to the correct order and real transaction'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.TaxEntry x JOIN dbo.TransactionEntry e ON e.ID=x.TransactionEntryID AND e.TransactionNumber=x.TransactionNumber WHERE x.TransactionNumber=$number AND x.SalesTax=60;") -eq 1 -and (Scalar "SELECT SUM(Amount) FROM dbo.TenderEntry WHERE TransactionNumber=$number;") -eq 560) 'Settlement tax and tender references remain attached to the real transaction'
    Check ((Scalar "SELECT Quantity FROM dbo.Item WHERE ID=$settleItem;") -eq 35 -and (Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=$settleItem;") -eq 3 -and (Scalar ("SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID="+$settleOther.OrderID+";")) -eq 3) 'Stale save cannot undo the settled stock movement or another order reservation'
    Check ((Scalar "SELECT Status FROM dbo.Queueing WHERE OrderID=$settleID;") -eq 1 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$settleID AND EventName='RMS transaction posted';") -eq 1 -and (Scalar ("SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$settleID AND Actor=N'"+$a.Request.Actor+"';")) -eq 0) 'Unchanged deployment trigger completes the queue; settlement is logged once with no rejected-save history'

    # Case 5: inject a failure only AFTER every intended kind of write is visible
    # inside the actual save transaction. The trigger exists only in this fixture.
    $firstItem=New-FixtureItem 100 2;$secondItem=New-FixtureItem 100 3
    $failure=New-FixtureOrder @($firstItem,$secondItem) @(2,3)
    $failureID=$failure.OrderID;$firstLine=$failure.Lines[0];$secondLine=$failure.Lines[1]
    $firstQueue=$firstLine.QueueItemID;$secondQueue=$secondLine.QueueItemID
    $firstEntry=$firstLine.EntryID;$secondEntry=$secondLine.EntryID
    $newLines=@([pscustomobject]@{ItemID=$firstItem;EntryID=$firstEntry;Quantity=4;PreparedQuantity=4},[pscustomobject]@{ItemID=$secondItem;EntryID=$secondEntry;Quantity=6;PreparedQuantity=6})
    Sql @"
CREATE TRIGGER dbo.TestFailCompleteSave ON dbo.SOD_WO_History AFTER INSERT AS
BEGIN
 SET NOCOUNT ON;
 IF APP_NAME()=N'MultiUser_A_$suffix' AND EXISTS(SELECT 1 FROM inserted WHERE OrderID=$failureID)
  AND EXISTS(SELECT 1 FROM dbo.[Order] WHERE ID=$failureID AND Total=1120 AND Tax=120)
  AND EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE ID=$firstEntry AND QuantityOnOrder=4)
  AND EXISTS(SELECT 1 FROM dbo.OrderEntry WHERE ID=$secondEntry AND QuantityOnOrder=6)
  AND EXISTS(SELECT 1 FROM dbo.Item WHERE ID=$firstItem AND QuantityCommitted=4)
  AND EXISTS(SELECT 1 FROM dbo.Item WHERE ID=$secondItem AND QuantityCommitted=6)
  AND EXISTS(SELECT 1 FROM dbo.QueueingItems WHERE id=$firstQueue AND QtyPre=4 AND Status='Prepared')
  AND EXISTS(SELECT 1 FROM dbo.QueueingItems WHERE id=$secondQueue AND QtyPre=6 AND Status='Prepared')
  AND (SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$failureID AND EventName='Item edited' AND Actor=N'MultiUser_A_$suffix')=2
 BEGIN
  DECLARE @gate int;
  SELECT @gate=ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;
  THROW 52941,'Injected save failure: header, both lines, stock, picking and history writes verified.',1;
 END;
END;
"@
    $beforeFailure=Fixture-State $failure
    [IO.File]::WriteAllText((Join-Path $directory 'MidSaveFailure-Before.xml'),$beforeFailure)
    $a=Start-Cashier 'MidSaveFailure' 'A' $failureID $firstItem $firstEntry 4 'Save' $newLines
    $b=Start-Cashier 'RecoverySave' 'B' $failureID $firstItem $firstEntry 4 'Save' $newLines
    Ready $a;Ready $b
    $gatePID=Hold-Gate 'SELECT ID FROM dbo.TestSaveGate WITH(UPDLOCK,HOLDLOCK) WHERE ID=1;'
    Go $a
    Wait-For {(Blocker $a) -eq $gatePID} 'save reached verified partial-write failure point' 8
    Request-State $a|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $directory 'MidSaveFailure-Overlap.json')
    Check ((Blocker $a) -eq $gatePID) 'Fault injection is reached only after header, both lines, stock, picking and audit writes exist inside the save'
    Release-Gate
    $ar=Result $a -AllowError
    Check ($ar.Status -eq 'Error' -and $ar.SqlNumber -eq 52941 -and $ar.Message -like '*header, both lines, stock, picking and history writes verified*') 'Actual save reports the deliberate late database failure'
    $afterFailure=Fixture-State $failure
    [IO.File]::WriteAllText((Join-Path $directory 'MidSaveFailure-After.xml'),$afterFailure)
    Check ($afterFailure -ceq $beforeFailure) 'Failed save restores exact original header, rowversions, lines, stock, queues, approvals and history'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$failureID AND Actor=N'MultiUser_A_$suffix';") -eq 0) 'No audit events from the failed transaction remain committed'
    Sql 'DROP TRIGGER dbo.TestFailCompleteSave;'
    Go $b;$br=Result $b
    Check ($br.Status -eq 'Committed') 'A second cashier can save the original recalled version after rollback and lock release'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.[Order] WHERE ID=$failureID AND Closed=0 AND Total=1120 AND Tax=120 AND Comment=N'RecoverySave-B';") -eq 1 -and (Scalar "SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE ID=$firstEntry;") -eq 4 -and (Scalar "SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE ID=$secondEntry;") -eq 6) 'Recovery save commits both line quantities and matching total/tax once'
    Check ((Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=$firstItem;") -eq 4 -and (Scalar "SELECT QuantityCommitted FROM dbo.Item WHERE ID=$secondItem;") -eq 6 -and (Scalar "SELECT SUM(Quantity) FROM dbo.Item WHERE ID IN($firstItem,$secondItem);") -eq 200) 'Recovery applies each commitment difference once and leaves physical stock unchanged'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.QueueingItems WHERE (id=$firstQueue AND QtyPre=4 AND Status='Prepared') OR (id=$secondQueue AND QtyPre=6 AND Status='Prepared');") -eq 2 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=$failureID AND EventName='Item edited' AND Actor=N'MultiUser_B_$suffix';") -eq 2) 'Recovery persists both prepared lines and only the successful cashier item history'
    Write-Output "PASS: $checks multi-user save checks; VersionedReads=$([bool]$VersionedReads); artifacts: $directory"
} finally {
    Release-Gate
    foreach($process in $processes){if(-not $process.HasExited){if(-not $process.WaitForExit(1500)){Stop-Process -Id $process.Id -Force}};$process.Dispose()}
    if($null -ne $connection){$connection.Dispose()}
    if($null -ne $master){try{if($master.State -eq 'Open' -and $database -match '^SODWO_Multi_[a-f0-9]{8}$'){$cmd=$master.CreateCommand();try{$cmd.CommandText="ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database];";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
