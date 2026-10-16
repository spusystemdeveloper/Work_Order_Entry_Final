# Disposable synthetic LocalDB only. NEVER executes recovery against a store.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$instance='HO_Renumber18882_'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$localdb=(Get-Command SqlLocalDB.exe).Source
$connection=$null;$master=$null;$created=$false;$checks=0
function Sql([string]$text){$cmd=$connection.CreateCommand();try{$cmd.CommandTimeout=40;$cmd.CommandText=$text;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
function Scalar([string]$text){$cmd=$connection.CreateCommand();try{$cmd.CommandTimeout=40;$cmd.CommandText=$text;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}}
function Check([bool]$ok,[string]$name){if(-not $ok){throw "FAILED: $name"};$script:checks++;Write-Output "Passed: $name"}
function Reject([scriptblock]$action,[string]$pattern,[string]$name){$rejected=$false;try{& $action}catch{if($_.Exception.ToString() -notmatch $pattern){throw};$rejected=$true};Check $rejected $name}
function Seed([string]$table,[hashtable]$values){
    $cmd=$connection.CreateCommand();$cmd.CommandText="SELECT name,TYPE_NAME(system_type_id) DataType,is_identity FROM sys.columns WHERE object_id=OBJECT_ID('dbo.[$table]') AND system_type_id<>189 ORDER BY column_id;"
    $r=$cmd.ExecuteReader();$columns=@();while($r.Read()){$columns+=,@($r.GetString(0),$r.GetString(1),$r.GetBoolean(2))};$r.Close();$cmd.Dispose()
    $names=@();$literals=@();$identity=$false
    foreach($col in $columns){$name=$col[0];$names+="[$name]";$identity=$identity -or $col[2]
        if($values.ContainsKey($name)){$literals+=$values[$name]}
        elseif($col[1] -match 'date|time'){$literals+="'20261014'"}
        elseif($col[1] -match 'char|text'){$literals+="N'Fixture'"}
        elseif($col[1] -match 'binary|image'){$literals+='0x010203'}
        else{$literals+='0'}
    }
    $insert="INSERT dbo.[$table]("+($names-join ',')+') VALUES('+($literals-join ',')+');'
    if($identity){$insert="SET IDENTITY_INSERT dbo.[$table] ON;"+$insert+"SET IDENTITY_INSERT dbo.[$table] OFF;"};Sql $insert
}
function TriggerState {return Scalar "SELECT name,is_disabled,CONVERT(varchar(64),HASHBYTES('SHA2_256',OBJECT_DEFINITION(object_id)),2) DefinitionHash FROM sys.triggers ORDER BY name FOR XML PATH('T');"}
try{
    & $localdb create $instance -s | Out-Null;if($LASTEXITCODE){throw 'Cannot create owned disposable instance'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand();$cmd.CommandText='CREATE DATABASE HO_PO_DB; ALTER DATABASE HO_PO_DB SET COMPATIBILITY_LEVEL=100;';[void]$cmd.ExecuteNonQuery();$cmd.Dispose()
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=HO_PO_DB;Integrated Security=True;Pooling=False");$connection.Open()
    Sql ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures/HO_Renumber_Schema.sql')))
    Sql 'CREATE TABLE dbo.InventoryTransferLog(ReferenceID int);CREATE TABLE dbo.ItemExpireHistory(ReferenceID int);CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode varchar(50),Quantity float,QuantityCommitted float);INSERT dbo.Item VALUES(46612,''FIXTURE1'',100.5,9.25),(54299,''FIXTURE2'',50.25,3.5);'
    Seed 'Order' @{ID='18878';StoreID='606';Type='2';Closed='1'}
    Seed 'Order' @{ID='18882';StoreID='606';Type='2';Closed='0';Time="'2026-10-16T16:21:09.340'";LastUpdated="'2026-10-16T16:21:09.340'";Total='46976.3367';Tax='0';CustomerID='71';ReferenceNumber="N'0'"}
    Seed 'OrderEntry' @{ID='61690';OrderID='18882';ItemID='46612';QuantityOnOrder='3';Price='7962.4833';FullPrice='13000'}
    Seed 'OrderEntry' @{ID='61691';OrderID='18882';ItemID='54299';QuantityOnOrder='3';Price='7696.2956';FullPrice='11800'}
    Seed 'Queueing' @{id='45888';OrderID='18882';GroupTo='18882';Status='0'}
    Seed 'Queueing' @{id='45880';OrderID='18878';GroupTo='18882';Status='0'}
    foreach($id in 1..2){Seed 'SOD_WO_LOGS' @{id="$id";OrderId='18882';ItemID='46612'};Seed 'SOD_WO_History' @{EventID="$id";OrderID='18882';ItemID='46612'}}
    foreach($type in @('Order Entry Logs','Order Update Logs','Sales Transaction Logs','Purchase Order Logs')){Seed 'SOD_Item_Logs' @{ItemID='46612';ReferenceID="'18882'";Type="'$type'";QuantityORIS='3.5';LastCommitted='7';NewCommitted='8'}}
    Seed 'Journal' @{ID='100';ReferenceID='18882';TransactionType='0';ReceiptCompressed='0x010203'}
    foreach($trigger in (Get-Content (Join-Path $PSScriptRoot 'Fixtures/HO_Renumber_Triggers.json') -Raw|ConvertFrom-Json)){Sql $trigger.Definition}
    $triggers=TriggerState
    $source=[IO.File]::ReadAllText((Join-Path $repo 'Database Components/Recovery/Renumber_HO_WO_18882_to_18879_And_Reseed.sql'))
    Reject {Sql $source} 'Wrong server/database' 'Distributed script rejects fixture server without weakening the saved guard'
    $fixture=$source.Replace("DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';","DECLARE @ExpectedServer sysname = CONVERT(sysname,@@SERVERNAME);")
    Check ($source -notmatch '@BackupPath|msdb\.dbo\.backupset|RESTORE VERIFYONLY|WITH CHECKSUM') 'Requested standalone script has no backup-path/history/checksum validation'
    # Only the server target is substituted in this owned synthetic test copy.
    function OriginalState {
        Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18882;') -eq 1 -and (Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18879;') -eq 0) 'Original order IDs remain after rollback/rejection'
        Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18882) 'Identity remains 18882 after rollback/rejection'
        Check ((TriggerState) -ceq $triggers) 'Exact trigger definitions/enabled states preserved'
    }
    Sql $fixture
    OriginalState
    $commit=$fixture.Replace('DECLARE @Commit bit = 0;','DECLARE @Commit bit = 1;')
    $failure=$commit.Replace('SELECT o.* INTO #HORenumberHeaders',"THROW 53699,'Injected after trigger suspension',1; SELECT o.* INTO #HORenumberHeaders")
    Reject {Sql $failure} 'Injected after trigger suspension' 'Failure with suspended triggers rolls back'
    OriginalState
    $failure=$commit.Replace('DECLARE update_cursor CURSOR',"THROW 53699,'Injected after header copy',1; DECLARE update_cursor CURSOR")
    Reject {Sql $failure} 'Injected after header copy' 'Failure after header replacement rolls back'
    OriginalState
    $failure=$commit.Replace('DECLARE verify_cursor CURSOR',"UPDATE dbo.OrderEntry SET Price=Price+1 WHERE ID=61690; DECLARE verify_cursor CURSOR")
    Reject {Sql $failure} 'Post-change values differ' 'Full value comparison rejects an unintended price change'
    Check ((Scalar 'SELECT Price FROM dbo.OrderEntry WHERE ID=61690;') -eq 7962.4833) 'Rejected price change rolled back'
    $failure=$commit.Replace('        COMMIT;',"        THROW 53699,'Injected after reseed',1; COMMIT;")
    Reject {Sql $failure} 'Injected after reseed' 'Failure after DBCC reseed rolls back the maintenance'
    OriginalState
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry;'
    Seed 'OrderEntry' @{ID='999';OrderID='18880'}
    Sql 'ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Reject {Sql $commit} 'Destination has orphaned references' 'Next-ID orphan references are rejected before reuse'
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry;DELETE dbo.OrderEntry WHERE ID=999;ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Sql 'INSERT dbo.SOD_ImportedQuote VALUES(NULL,18882);'
    Reject {Sql $commit} 'unsupported order reference' 'Ambiguous imported reference stops for review'
    Sql 'DELETE dbo.SOD_ImportedQuote;CREATE TABLE dbo.NewOrderReference(OrderID int);'
    Reject {Sql $commit} 'additional order-reference column' 'New schema reference requires audit even when empty'
    Sql 'DROP TABLE dbo.NewOrderReference;'
    Sql 'CREATE TRIGGER dbo.Unreviewed ON dbo.SOD_WO_LOGS AFTER UPDATE AS RETURN;'
    Reject {Sql $commit} 'Trigger definitions/state changed' 'Unreviewed trigger stops recovery'
    Sql 'DROP TRIGGER dbo.Unreviewed;'
    Sql 'DISABLE TRIGGER ALL ON dbo.[Transaction];'
    Seed 'Transaction' @{TransactionNumber='999';RecallID='18882'}
    Sql 'ENABLE TRIGGER ALL ON dbo.[Transaction];'
    Reject {Sql $commit} 'Posted transaction/history/receipt' 'A newly settled/linked transaction stops recovery'
    Sql 'DISABLE TRIGGER ALL ON dbo.[Transaction];DELETE dbo.[Transaction] WHERE TransactionNumber=999;ENABLE TRIGGER ALL ON dbo.[Transaction];'
    Sql 'UPDATE dbo.[Order] SET Total=46976.34 WHERE ID=18882;'
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Edited header requires a fresh audit'
    Sql 'UPDATE dbo.[Order] SET Total=46976.3367 WHERE ID=18882;'
    Seed 'Order' @{ID='18879'}
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Occupied destination order is rejected'
    Sql 'DISABLE TRIGGER ALL ON dbo.[Order];DELETE dbo.[Order] WHERE ID=18879;ENABLE TRIGGER ALL ON dbo.[Order];DELETE dbo.SOD_WO_History WHERE OrderID=18879;'
    Seed 'SOD_Item_Logs' @{ItemID='46612';ReferenceID="'018880'";Type="'Order Entry Logs'"}
    Reject {Sql $commit} 'Noncanonical numeric order-log references' 'Leading-zero references cannot be silently adopted by the next ID'
    Sql "DELETE dbo.SOD_Item_Logs WHERE ReferenceID='018880';"
    Seed 'Order' @{ID='18883'}
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Later order allocation prevents rewinding'
    Sql 'DISABLE TRIGGER ALL ON dbo.[Order];DELETE dbo.[Order] WHERE ID=18883;ENABLE TRIGGER ALL ON dbo.[Order];DELETE dbo.SOD_WO_History WHERE OrderID=18883;DBCC CHECKIDENT(''dbo.[Order]'',RESEED,18882) WITH NO_INFOMSGS;'
    $beforeHistory=Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;'
    Sql $commit
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18882;') -eq 0 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=18879;') -eq 46976.3367) 'Committed mapping preserves exact header total'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=18879 AND ID IN(61690,61691) AND QuantityOnOrder=3;') -eq 2) 'Line IDs and quantities preserved under new order ID'
    Check ((Scalar 'SELECT GroupTo FROM dbo.Queueing WHERE id=45880;') -eq 18879 -and (Scalar 'SELECT OrderID FROM dbo.Queueing WHERE id=45880;') -eq 18878 -and (Scalar 'SELECT id FROM dbo.Queueing WHERE OrderID=18879;') -eq 45888) 'Own and related grouping references map without changing other order/queue IDs'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_LOGS WHERE OrderId=18879;') -eq 2) 'Price approval references mapped'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;') -eq ($beforeHistory+1) -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=18879 AND EventName='Order ID changed';") -eq 1) 'Original history preserved with one explicit mapping event'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='18879' AND [Type] IN('Order Entry Logs','Order Update Logs');") -eq 2 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='18882' AND [Type] IN('Sales Transaction Logs','Purchase Order Logs');") -eq 2) 'Only order-domain item-log references mapped'
    Check ((Scalar 'SELECT ReferenceID FROM dbo.Journal WHERE ID=100;') -eq 18882) 'Unrelated sales journal preserved'
    Check ((Scalar 'SELECT SUM(Quantity) FROM dbo.Item;') -eq 150.75 -and (Scalar 'SELECT SUM(QuantityCommitted) FROM dbo.Item;') -eq 12.75) 'Physical and fractional committed stock unchanged'
    Check ((TriggerState) -ceq $triggers) 'All exact trigger states restored on commit'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18879) 'Renumber and requested reseed committed together'
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Repeated execution cannot rewind identity'
    $columns=Scalar "SELECT STUFF((SELECT ','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.[Order]') AND is_identity=0 AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,'');"
    Sql "INSERT dbo.[Order]($columns) SELECT $columns FROM dbo.[Order] WHERE ID=18879;"
    Check ((Scalar 'SELECT CONVERT(int,@@IDENTITY);') -eq 18880) 'RMS legacy identity lookup returns the next real Order ID'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18880;') -eq 1) 'Actual next automatic order allocation is 18880'
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Script rejects another run after new order activity'
    Write-Output "PASS: $checks synthetic renumber/reseed checks at compatibility 100. No store writes."
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{if($master.State -eq [Data.ConnectionState]::Open){$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('HO_PO_DB') IS NOT NULL BEGIN ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE HO_PO_DB;END;";[void]$cmd.ExecuteNonQuery();$cmd.Dispose()}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
