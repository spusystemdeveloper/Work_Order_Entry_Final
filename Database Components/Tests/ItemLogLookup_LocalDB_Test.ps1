# Owned disposable SQL only. Never connects to an existing store or reads its credentials.
param([ValidateSet(100,130)][int]$CompatibilityLevel=130)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$instance="SODWO_LogLookup_$suffix"
$dbName=$instance
$directory=Join-Path $repo "TestResults\ItemLogLookup-$suffix"
$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$migration=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Improve_WorkOrder_ItemLogLookup.sql'))
$legacy=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures\HO_OrderEntry_ItemLogs_Legacy.sql'))
$connection=$null;$master=$null;$created=$false;$checks=0
function Sql([string]$text,$tx=$null){
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandText=$text;$cmd.Transaction=$tx;$cmd.CommandTimeout=60;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
}
function Scalar([string]$text,$tx=$null){
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandText=$text;$cmd.Transaction=$tx;$cmd.CommandTimeout=60;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}
}
function Check([bool]$ok,[string]$name){if(-not $ok){throw "FAILED: $name"};$script:checks++;Write-Host "Passed: $name"}
function Reject([scriptblock]$action,[string]$pattern,[string]$name){
    $failed=$false;try{& $action}catch{if($_.Exception.ToString() -notmatch $pattern){throw};$failed=$true};Check $failed $name
}
function Index-Count($tx=$null){return Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.SOD_Item_Logs') AND name='IX_SOD_WO_ItemLogLookup';" $tx}
function Try-Migration([scriptblock]$assertion){
    $tx=$connection.BeginTransaction()
    try{& $assertion $tx}finally{$tx.Rollback();$tx.Dispose()}
}
function Lookup-Reads {
    $script:ioMessages=[Collections.Generic.List[string]]::new()
    $handler=[Data.SqlClient.SqlInfoMessageEventHandler]{param($sender,$e);$script:ioMessages.Add($e.Message)}
    $connection.add_InfoMessage($handler)
    $watch=[Diagnostics.Stopwatch]::StartNew()
    try{
        Sql "SET STATISTICS IO ON; DECLARE @Value int,@ItemID int=1,@OrderID int=1; SELECT TOP(1) @Value=NewCommitted FROM dbo.SOD_Item_Logs WHERE ItemID=@ItemID AND ReferenceID=@OrderID ORDER BY [Date] DESC; SET STATISTICS IO OFF;"
        $watch.Stop();$reads=0
        foreach($message in $script:ioMessages){foreach($match in [regex]::Matches($message,"Table 'SOD_Item_Logs'[^\r\n]*?logical reads (\d+)")){$reads += [int]$match.Groups[1].Value}}
        return [pscustomobject]@{LogicalReads=$reads;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,3)}
    }finally{$connection.remove_InfoMessage($handler);Sql 'SET STATISTICS IO OFF;'}
}
function Log-Signature {
    $tx=$connection.BeginTransaction()
    try{
        Sql 'INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder) VALUES(1,1,3.5); UPDATE dbo.OrderEntry SET QuantityOnOrder=2.25; UPDATE dbo.OrderEntry SET QuantityOnOrder=0;' $tx
        Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE [Type] IN('Order Entry Logs','Order Update Logs');" $tx) -eq 2) 'Legacy insert/edit/zero behavior remains two log rows'
        Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=1;' $tx) -eq 100 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=1;' $tx) -eq 5) 'Trigger leaves physical stock and commitments unchanged'
        return Scalar "SELECT ItemID,ItemLookupCode,[Type],ReferenceID,QuantityORIS,LastQuantity,NewQuantity,LastCommitted,NewCommitted FROM dbo.SOD_Item_Logs WHERE [Type] IN('Order Entry Logs','Order Update Logs') ORDER BY [Type] FOR XML RAW('Log'),ROOT('Logs');" $tx
    }finally{$tx.Rollback();$tx.Dispose()}
}
try{
    & $localdb create $instance -s | Out-Null
    if($LASTEXITCODE -ne 0){throw 'LocalDB instance creation failed'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand()
    try{$cmd.CommandText="CREATE DATABASE [$dbName]; ALTER DATABASE [$dbName] SET COMPATIBILITY_LEVEL=$CompatibilityLevel;";$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=$dbName;Integrated Security=True;Pooling=False");$connection.Open()
    Sql 'SET LANGUAGE us_english; SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;'
    Reject {Sql $migration} 'explicit transaction is required' 'Standalone execution without an explicit transaction is rejected'
    Try-Migration {param($tx) Reject {Sql $migration $tx} 'SOD_Item_Logs is missing' 'Missing branch log table is rejected'}
    Sql @'
CREATE TABLE dbo.[Order](ID int PRIMARY KEY);
CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode varchar(50),Quantity float,QuantityCommitted int);
CREATE TABLE dbo.OrderEntry(ID int IDENTITY PRIMARY KEY,OrderID int,ItemID int,QuantityOnOrder float);
CREATE TABLE dbo.SOD_Item_Logs(ItemID int NOT NULL,ItemLookupCode varchar(50),[Type] varchar(50),ReferenceID varchar(50) NOT NULL,
 QuantityORIS float,LastQuantity float,NewQuantity float,LastCommitted int,NewCommitted int NOT NULL,[Date] datetime NOT NULL,
 FixtureFiller varchar(400) NOT NULL DEFAULT REPLICATE('x',400));
INSERT dbo.[Order] VALUES(1);
INSERT dbo.Item VALUES(1,'FIXTURE',100,5);
INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date])
 SELECT TOP(100000) 2,'100',7,CONVERT(datetime,'20200101',112) FROM sys.all_objects a CROSS JOIN sys.all_objects b;
INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES
 (1,'1',7,'20200102'),(1,'01',8,'20200103'),(1,'2',99,'20200104');
SELECT * INTO dbo.ExpectedLogs FROM dbo.SOD_Item_Logs;
'@
    Sql $legacy
    $triggerBefore=Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.trg_OrderEntry_ItemLogs'));"
    $valueBefore=Scalar "DECLARE @i int=1,@o int=1; SELECT TOP(1) NewCommitted FROM dbo.SOD_Item_Logs WHERE ItemID=@i AND ReferenceID=@o ORDER BY [Date] DESC;"
    Check ($valueBefore -eq 8) 'Baseline uses numeric reference comparison including leading-zero text'
    $signatureBefore=Log-Signature
    $before=Lookup-Reads
    Try-Migration {param($tx) Sql $migration $tx;Check ((Index-Count $tx) -eq 1) 'Optional index is created inside the caller transaction'}
    Check ((Index-Count) -eq 0) 'Rollback removes the index'
    $tx=$connection.BeginTransaction()
    try{Sql $migration $tx;$tx.Commit()}catch{$tx.Rollback();throw}finally{$tx.Dispose()}
    Check ((Index-Count) -eq 1) 'Commit persists the optional index'
    Check ((Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.trg_OrderEntry_ItemLogs'));") -ceq $triggerBefore) 'Trigger definition is byte-for-byte unchanged'
    $valueAfter=Scalar "DECLARE @i int=1,@o int=1; SELECT TOP(1) NewCommitted FROM dbo.SOD_Item_Logs WHERE ItemID=@i AND ReferenceID=@o ORDER BY [Date] DESC;"
    Check ($valueBefore -eq $valueAfter) 'Indexed lookup returns the same commitment with numeric and unrelated-reference cases'
    Check ((Log-Signature) -ceq $signatureBefore) 'Legacy trigger log values match before and after indexing, including fractional insert/edit/zero cases'
    Check ((Scalar 'SELECT COUNT(*) FROM (SELECT * FROM dbo.SOD_Item_Logs EXCEPT SELECT * FROM dbo.ExpectedLogs) d;') -eq 0 -and
        (Scalar 'SELECT COUNT(*) FROM (SELECT * FROM dbo.ExpectedLogs EXCEPT SELECT * FROM dbo.SOD_Item_Logs) d;') -eq 0 -and
        (Scalar 'SELECT COUNT_BIG(*) FROM dbo.SOD_Item_Logs;') -eq 100003) 'All existing log columns and row counts are preserved'
    $after=Lookup-Reads
    Check ($before.LogicalReads -gt 100 -and $after.LogicalReads -lt ($before.LogicalReads * 0.1)) 'Unchanged lookup removes over 90 percent of synthetic log-table reads'
    @([pscustomobject]@{Phase='Before';Rows=100003;LogicalReads=$before.LogicalReads;Milliseconds=$before.Milliseconds},
      [pscustomobject]@{Phase='After';Rows=100003;LogicalReads=$after.LogicalReads;Milliseconds=$after.Milliseconds}) |
        Export-Csv -LiteralPath (Join-Path $directory 'LookupReads.csv') -NoTypeInformation
    Write-Output "Synthetic lookup reads: $($before.LogicalReads) before; $($after.LogicalReads) after."
    Try-Migration {param($tx) Sql $migration $tx;Check ((Index-Count $tx) -eq 1) 'Repeated migration does not duplicate the index'}
    Try-Migration {param($tx)
        Sql ($migration + "; CREATE TABLE dbo.CallerContinued(ID int);") $tx
        Check ((Scalar "SELECT COUNT(*) FROM sys.tables WHERE object_id=OBJECT_ID('dbo.CallerContinued');" $tx) -eq 1) 'Reused-index path continues the caller batch without bypassing its cleanup'
    }
    Sql 'DROP INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs; CREATE INDEX FixtureEquivalent ON dbo.SOD_Item_Logs(ItemID,[Date]) INCLUDE(ReferenceID,NewCommitted);'
    Try-Migration {param($tx) Sql $migration $tx;Check ((Index-Count $tx) -eq 0) 'Usable existing ascending-date covering index is reused'}
    Sql 'DROP INDEX FixtureEquivalent ON dbo.SOD_Item_Logs;'
    Try-Migration {param($tx)
        Sql 'CREATE INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs(ReferenceID);' $tx
        Reject {Sql $migration $tx} 'another definition' 'Conflicting index name is not replaced'
    }
    Try-Migration {param($tx)
        Sql 'CREATE INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs(ItemID,[Date]) INCLUDE(ReferenceID,NewCommitted); ALTER INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs DISABLE;' $tx
        Reject {Sql $migration $tx} 'another definition' 'Disabled named index is rejected'
    }
    Try-Migration {param($tx)
        Sql 'CREATE INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs(ItemID,[Date]) INCLUDE(ReferenceID,NewCommitted) WHERE ItemID=1;' $tx
        Reject {Sql $migration $tx} 'another definition' 'Filtered named index is rejected'
    }
    Try-Migration {param($tx)
        Sql "INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'not-an-order',8,'20200105');" $tx
        Reject {Sql $migration $tx} 'Non-integer/overflow' 'Non-integer references stop for branch review'
        Check ((Index-Count $tx) -eq 0) 'Rejected reference preflight creates no index'
    }
    Try-Migration {param($tx)
        Sql "INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'2147483648',8,'20200105');" $tx
        Reject {Sql $migration $tx} 'Non-integer/overflow' 'Overflow references stop for branch review'
    }
    Try-Migration {param($tx)
        Sql "INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'001',10,'20200103');" $tx
        Reject {Sql $migration $tx} 'timestamps tie' 'Different newest commitment values are rejected across numerically equal text references'
        Check ((Index-Count $tx) -eq 0) 'Ambiguous history is left without a new index'
    }
    Try-Migration {param($tx)
        Sql "INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'001',8,'20200103');" $tx
        Sql $migration $tx;Check ((Index-Count $tx) -eq 1) 'Equal-valued newest timestamp ties remain supported'
    }
    Try-Migration {param($tx)
        Sql "INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'001',10,'20200102');" $tx
        Sql $migration $tx;Check ((Index-Count $tx) -eq 1) 'Older conflicting timestamp ties do not change the newest result'
    }
    Try-Migration {param($tx)
        Sql 'DELETE dbo.SOD_Item_Logs; ALTER TABLE dbo.SOD_Item_Logs ALTER COLUMN ReferenceID nvarchar(50) NOT NULL;' $tx
        Reject {Sql $migration $tx} 'column types differ' 'Unreviewed branch column types are rejected'
    }
    Try-Migration {param($tx)
        Sql 'DELETE dbo.SOD_Item_Logs;' $tx;Sql $migration $tx;Check ((Index-Count $tx) -eq 1) 'Empty compatible branch table is supported'
    }
    Check ((Index-Count) -eq 0 -and (Scalar 'SELECT COUNT_BIG(*) FROM dbo.SOD_Item_Logs;') -eq 100003) 'Guard and rollback tests leave original fixture data intact'
    Write-Output "PASS: $checks item-log lookup checks at compatibility $CompatibilityLevel; artifacts: $directory"
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{
        if($dbName -notmatch '^SODWO_LogLookup_[0-9a-f]{8}$'){throw 'Unexpected fixture cleanup target'}
        $cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('$dbName') IS NOT NULL BEGIN ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$dbName]; END;";$null=$cmd.ExecuteNonQuery();$cmd.Dispose()
    }finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k | Out-Null;& $localdb delete $instance | Out-Null}
}
