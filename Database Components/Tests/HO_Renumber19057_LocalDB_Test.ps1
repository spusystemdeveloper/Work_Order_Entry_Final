# Disposable synthetic LocalDB only. NEVER executes recovery against a store.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$instance='HO_Renumber19057_'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$localdb=(Get-Command SqlLocalDB.exe).Source
$connection=$null;$master=$null;$created=$false;$checks=0;$backupFolder=$null
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
        elseif($col[1] -match 'date|time'){$literals+="'20261114'"}
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
    Sql 'CREATE TABLE dbo.InventoryTransferLog(ReferenceID int);CREATE TABLE dbo.ItemExpireHistory(ReferenceID int);CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode varchar(50),Quantity float,QuantityCommitted float);INSERT dbo.Item VALUES(59626,''55142CSE'',6000,6000);CREATE INDEX IX_SOD_WO_ItemLogLookup ON dbo.SOD_Item_Logs(ItemID,[Date]) INCLUDE(ReferenceID,NewCommitted);'
    Seed 'Order' @{ID='19055';StoreID='606';Type='2';Closed='1'}
    Seed 'Order' @{ID='19057';StoreID='606';Type='2';Closed='0';Time="'2026-11-14T11:23:27.647'";LastUpdated="'2026-11-14T11:23:27.647'";Total='3612280.2000';Tax='0';CustomerID='203';ReferenceNumber="N'0'"}
    Seed 'OrderEntry' @{ID='62152';OrderID='19057';ItemID='59626';QuantityOnOrder='6000';Price='602.0467';FullPrice='980.0000'}
    Seed 'Queueing' @{id='46074';OrderID='19057';GroupTo='19057';Status='0'}
    Seed 'Queueing' @{id='46073';OrderID='19055';GroupTo='19057';Status='0'}
    Seed 'SOD_WO_LOGS' @{id='388';OrderId='19057';ItemID='59626'}
    foreach($id in 2813..2817){Seed 'SOD_WO_History' @{EventID="$id";OrderID='19057';ItemID='59626'}}
    Seed 'SOD_Item_Logs' @{ItemID='59626';ReferenceID="'19057'";Type="'Order Entry Logs'";QuantityORIS='6000';LastCommitted='0';NewCommitted='6000'}
    Seed 'SOD_Item_Logs' @{ItemID='59626';ReferenceID="'19057'";Type="'Order Update Logs'";QuantityORIS='6000';LastCommitted='6000';NewCommitted='6000'}
    Seed 'SOD_Item_Logs' @{ItemID='59626';ReferenceID="'19057'";Type="'Sales Transaction Logs'"}
    Seed 'Journal' @{ID='100';ReferenceID='19057';TransactionType='0';ReceiptCompressed='0x010203'}
    foreach($trigger in (Get-Content (Join-Path $PSScriptRoot 'Fixtures/HO_Renumber_Triggers.json') -Raw|ConvertFrom-Json)){Sql $trigger.Definition}
    $triggers=TriggerState
    $source=[IO.File]::ReadAllText((Join-Path $repo 'Database Components/Recovery/Renumber_HO_WO_19057_to_19056.sql'))
    Reject {Sql $source} 'Wrong server/database' 'Distributed file rejects fixture server'
    Check ($source -match 'DBCC CHECKIDENT\s*\(N''dbo\.\[Order\]''\s*,\s*RESEED\s*,\s*19056\)') 'Recovery specifies exact reseed to 19056 on commit'
    $fixture=$source.Replace("DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';","DECLARE @ExpectedServer sysname = CONVERT(sysname,@@SERVERNAME);")
    Reject {Sql $fixture} 'clients remain closed' 'Maintenance confirmation is required'
    $fixture=$fixture.Replace('DECLARE @MaintenanceConfirmed bit = 0;','DECLARE @MaintenanceConfirmed bit = 1;')
    Reject {Sql $fixture} 'recovery backup is required' 'Missing verified backup is rejected'
    $backupFolder=Join-Path $repo ('TestResults/HO19057Fixture-'+[Guid]::NewGuid().ToString('N'));$null=New-Item -ItemType Directory -Path $backupFolder
    $backup=Join-Path $backupFolder 'fixture.bak';$quoted=$backup.Replace("'","''")
    Sql "BACKUP DATABASE HO_PO_DB TO DISK=N'$quoted' WITH COPY_ONLY,CHECKSUM;"
    $fixture=$fixture.Replace("DECLARE @BackupPath nvarchar(4000) = N'';","DECLARE @BackupPath nvarchar(4000) = N'$quoted';")
    $commit=$fixture.Replace('DECLARE @Commit bit = 0;','DECLARE @Commit bit = 1;')
    function OriginalState {
        Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=19057;') -eq 1 -and (Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=19056;') -eq 0) 'Original IDs remain after rollback/rejection'
        Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 19057) 'Original identity retained'
        Check ((TriggerState) -ceq $triggers) 'Exact trigger definitions/states preserved'
    }
    Sql $fixture
    OriginalState
    $failure=$commit.Replace('SELECT o.* INTO #HORenumberHeaders',"THROW 53699,'Injected before header replacement',1; SELECT o.* INTO #HORenumberHeaders")
    Reject {Sql $failure} 'Injected before header replacement' 'Failure after trigger suspension rolls back'
    OriginalState
    $failure=$commit.Replace('DECLARE update_cursor CURSOR',"THROW 53699,'Injected after header replacement',1; DECLARE update_cursor CURSOR")
    Reject {Sql $failure} 'Injected after header replacement' 'Failure after header replacement rolls back'
    OriginalState
    $failure=$commit.Replace('DECLARE verify_cursor CURSOR',"UPDATE dbo.OrderEntry SET Price=Price+1 WHERE ID=62152; DECLARE verify_cursor CURSOR")
    Reject {Sql $failure} 'Post-change values differ' 'Full-row snapshots reject unintended price change'
    Check ((Scalar 'SELECT Price FROM dbo.OrderEntry WHERE ID=62152') -eq 602.0467) 'Unintended price rolled back'
    $failure=$commit.Replace('DECLARE @inventoryAfter',"UPDATE dbo.Item SET QuantityCommitted=0 WHERE ID=59626; DECLARE @inventoryAfter")
    Reject {Sql $failure} 'Inventory changed during' 'Inventory snapshot rejects any quantity/commitment change'
    Check ((Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=59626') -eq 6000) 'Unintended commitment change rolled back'
    $failure=$commit.Replace('        COMMIT;',"        THROW 53699,'Injected before commit',1; COMMIT;")
    Reject {Sql $failure} 'Injected before commit' 'Late failure rolls back (including reseed)'
    OriginalState
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry;'
    Seed 'OrderEntry' @{ID='999';OrderID='19056'}
    Sql 'ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Reject {Sql $commit} 'Destination has orphaned references' 'Destination orphan entries prevent reuse'
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry;DELETE dbo.OrderEntry WHERE ID=999;ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Sql 'INSERT dbo.SOD_ImportedQuote VALUES(NULL,19057);'
    Reject {Sql $commit} 'unsupported order reference' 'Ambiguous import references prevent repair'
    Sql 'DELETE dbo.SOD_ImportedQuote;CREATE TABLE dbo.NewOrderReference(OrderID int);'
    Reject {Sql $commit} 'additional order-reference column' 'New legacy reference columns require audit'
    Sql 'DROP TABLE dbo.NewOrderReference;'
    Sql 'CREATE TRIGGER dbo.Unreviewed ON dbo.SOD_WO_LOGS AFTER UPDATE AS RETURN;'
    Reject {Sql $commit} 'Trigger definitions/state changed' 'Unreviewed triggers prevent repair'
    Sql 'DROP TRIGGER dbo.Unreviewed;DISABLE TRIGGER ALL ON dbo.[Transaction];'
    Seed 'Transaction' @{TransactionNumber='999';RecallID='19057'}
    Sql 'ENABLE TRIGGER ALL ON dbo.[Transaction];'
    Reject {Sql $commit} 'Posted transaction/history/receipt' 'Settled/recalled transactions prevent repair'
    Sql 'DISABLE TRIGGER ALL ON dbo.[Transaction];DELETE dbo.[Transaction] WHERE TransactionNumber=999;ENABLE TRIGGER ALL ON dbo.[Transaction];'
    Seed 'SOD_Item_Logs' @{ItemID='59626';ReferenceID="'0019056'";Type="'Order Entry Logs'"}
    Reject {Sql $commit} 'Unexpected/noncanonical' 'Leading-zero destination log references are rejected'
    Sql "DELETE dbo.SOD_Item_Logs WHERE ReferenceID='0019056';"
    Seed 'SOD_Item_Logs' @{ItemID='999';ReferenceID="'19057'";Type="'Order Entry Logs'"}
    Reject {Sql $commit} 'Unexpected/noncanonical' 'Global index audit rejects source logs for unknown items'
    Sql "DELETE dbo.SOD_Item_Logs WHERE ItemID=999;"
    $inventory=Scalar 'SELECT ID,Quantity,QuantityCommitted FROM dbo.Item ORDER BY ID FOR XML PATH(''Item''),ROOT(''Inventory'');'
    Sql $commit
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=19057;') -eq 0 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=19056;') -eq 3612280.2000) 'Commit maps only requested ID and retains total'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=19056;') -eq 1) 'Original entry ID preserved under new reference'
    Check ((Scalar 'SELECT GroupTo FROM dbo.Queueing WHERE id=46073;') -eq 19056 -and (Scalar 'SELECT OrderID FROM dbo.Queueing WHERE id=46073;') -eq 19055 -and (Scalar 'SELECT OrderID FROM dbo.Queueing WHERE id=46074;') -eq 19056) 'Related grouping maps without changing other orders'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_LOGS WHERE OrderId=19056') -eq 1) 'Approval row preserved and remapped'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=19056') -eq 6 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=19056 AND EventName='Order ID changed'") -eq 1) 'Five history rows preserved plus one mapping event'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='19056' AND [Type] IN('Order Entry Logs','Order Update Logs')") -eq 2 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='19057' AND [Type]='Sales Transaction Logs'") -eq 1) 'Two order-domain logs mapped; unrelated sales logs untouched'
    Check ((Scalar 'SELECT ReferenceID FROM dbo.Journal WHERE ID=100') -eq 19057) 'Unrelated sales journal untouched'
    Check ((Scalar 'SELECT ID,Quantity,QuantityCommitted FROM dbo.Item ORDER BY ID FOR XML PATH(''Item''),ROOT(''Inventory'');') -ceq $inventory) 'All stock/commitment values preserved exactly'
    Check ((TriggerState) -ceq $triggers) 'All reviewed triggers restored'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order')") -eq 19056) 'Identity reseeded to 19056 on commit'
    $verifyScript=[IO.File]::ReadAllText((Join-Path $repo 'Database Components/Recovery/Verify_HO_WO_19056.sql')).Replace("@@SERVERNAME<>N'WIN-2847UGJBAV4'","1=0")
    $vcmd=$connection.CreateCommand();$vcmd.CommandTimeout=40;$vcmd.CommandText=$verifyScript
    $vda=[Data.SqlClient.SqlDataAdapter]::new($vcmd);$vds=[Data.DataSet]::new();[void]$vda.Fill($vds);$vcmd.Dispose()
    $verifyMsg=[string]$vds.Tables[$vds.Tables.Count-1].Rows[0][0]
    Check ($verifyMsg -match '^VERIFIED:') 'Verify script executes cleanly and returns VERIFIED'
    Reject {Sql $commit} 'Source order/lines/queue or identity changed' 'Repeated execution rejected'
    $columns=Scalar "SELECT STUFF((SELECT ','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.[Order]') AND is_identity=0 AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,'');"
    Sql "INSERT dbo.[Order]($columns) SELECT $columns FROM dbo.[Order] WHERE ID=19056;"
    Check ((Scalar 'SELECT CONVERT(int,@@IDENTITY)') -eq 19057) 'Actual next legacy identity allocation is 19057'
    Write-Output "PASS: $checks synthetic renumber/backup/reseed checks at compatibility 100. No live writes."
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{if($master.State -eq [Data.ConnectionState]::Open){$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('HO_PO_DB') IS NOT NULL BEGIN ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE HO_PO_DB;END;";[void]$cmd.ExecuteNonQuery();$cmd.Dispose()}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
    if($backupFolder -and (Test-Path $backupFolder)){Remove-Item -Recurse -Force $backupFolder -ErrorAction SilentlyContinue}
}
