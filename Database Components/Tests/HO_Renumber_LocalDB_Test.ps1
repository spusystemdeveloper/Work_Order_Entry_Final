# Disposable synthetic LocalDB only. NEVER executes recovery against a store.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$instance='HO_Renumber_'+[Guid]::NewGuid().ToString('N').Substring(0,8)
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
    & $localdb create $instance | Out-Null;if($LASTEXITCODE -ne 0){throw 'Cannot create disposable LocalDB'};$created=$true
    & $localdb start $instance | Out-Null;if($LASTEXITCODE -ne 0){throw 'Cannot start disposable LocalDB'}
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand();$cmd.CommandText='CREATE DATABASE HO_PO_DB; ALTER DATABASE HO_PO_DB SET COMPATIBILITY_LEVEL=130;';$null=$cmd.ExecuteNonQuery();$cmd.Dispose()
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=HO_PO_DB;Integrated Security=True;Pooling=False");$connection.Open()
    Sql ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures\HO_Renumber_Schema.sql')))
    Sql 'SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; CREATE TABLE dbo.Item(ID int PRIMARY KEY,ItemLookupCode varchar(50),Quantity float,QuantityCommitted int); INSERT dbo.Item VALUES(1,''FIXTURE'',100,7);'
    Seed 'Order' @{ID='18868';StoreID='606';Type='2';Closed='1'}
    Seed 'Order' @{ID='18870';StoreID='606';Type='2';Closed='1';Time="'2026-10-14T13:50:33.653'";Total='30200.54';Tax='0';CustomerID='71';ReferenceNumber="N'ITS-0018869'"}
    Seed 'Order' @{ID='18879';StoreID='606';Type='2';Closed='0';Time="'2026-10-15T09:40:07.430'";Total='11042.76';Tax='0';CustomerID='71'}
    foreach($i in 1..5){$order=if($i -le 4){18870}else{18879};Seed 'OrderEntry' @{ID="$i";OrderID="$order";ItemID='1';QuantityOnOrder="$i";Price='123.45';FullPrice='125'};Seed 'SOD_WO_LOGS' @{id="$i";OrderId="$order";ItemID='1'};Seed 'SOD_WO_History' @{EventID="$i";OrderID="$order";ItemID='1'}}
    Seed 'Queueing' @{id='45867';OrderID='18870';GroupTo='18870';Status='1'}
    Seed 'Queueing' @{id='45876';OrderID='18879';GroupTo='18879';Status='0'}
    # A different order grouped to the renamed open order must retain its own identity.
    Seed 'Queueing' @{id='45800';OrderID='18868';GroupTo='18879';Status='0'}
    Seed 'Transaction' @{TransactionNumber='23266';StoreID='606';RecallID='18870';RecallType='4';Total='30200.54'}
    Seed 'OrderHistory' @{ID='19857';StoreID='606';OrderID='18870';TransactionNumber='23266'}
    Seed 'Journal' @{ID='23804';StoreID='606';ReferenceID='18870';TransactionType='2';OrderHistoryID='19857';Compressed='1';ReceiptCompressed='0x010203040506'}
    # Numeric IDs belonging to other domains must never be renumbered.
    Seed 'Journal' @{ID='23805';StoreID='606';ReferenceID='18870';TransactionType='0'}
    foreach($ref in @('18870','18879')){foreach($type in @('Order Entry Logs','Order Update Logs','Sales Transaction Logs')){Seed 'SOD_Item_Logs' @{ItemID='1';ReferenceID="'$ref'";Type="'$type'";QuantityORIS='3';LastCommitted='7';NewCommitted='8'}}}
    Seed 'SOD_Item_Logs' @{ItemID='1';ReferenceID="'18869'";Type="'Purchase Order Logs'"}
    foreach($trigger in (Get-Content (Join-Path $PSScriptRoot 'Fixtures\HO_Renumber_Triggers.json') -Raw|ConvertFrom-Json)){Sql $trigger.Definition}
    $originalTriggers=TriggerState
    $reseedSource=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Reseed_HO_Order_After_Renumber.sql'))
    Reject {Sql $reseedSource} 'Wrong server/database' 'Unmodified reseed script rejects a disposable server'
    $reseed=$reseedSource.Replace("DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';","DECLARE @ExpectedServer sysname = CONVERT(sysname,@@SERVERNAME);")
    Reject {Sql $reseed} 'Maximum Order ID must be 18870' 'Reseed rejects the original orders before renumber commit'
    $source=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Renumber_HO_WorkOrders_18870_18879.sql'))
    Reject {Sql $source} 'Wrong server/database' 'Unmodified script rejects a disposable SQL server'
    # In-memory TEST COPY only: replace server guard and backup verification for synthetic data.
    $fixture=$source.Replace("DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';","DECLARE @ExpectedServer sysname = CONVERT(sysname,@@SERVERNAME);")
    $backupStart=$fixture.IndexOf('IF NOT EXISTS(SELECT 1 FROM msdb.dbo.backupset')
    $backupEnd=$fixture.IndexOf('RESTORE VERIFYONLY FROM DISK=@BackupPath WITH CHECKSUM;')+'RESTORE VERIFYONLY FROM DISK=@BackupPath WITH CHECKSUM;'.Length
    if($backupStart -lt 0 -or $backupEnd -le $backupStart){throw 'Backup test splice markers changed'}
    $fixture=$fixture.Remove($backupStart,$backupEnd-$backupStart)
    Sql $fixture
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID IN(18870,18879);') -eq 2 -and (Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18869;') -eq 0) 'Default rehearsal restores original headers'
    Check ((Scalar 'SELECT RecallID FROM dbo.[Transaction] WHERE TransactionNumber=23266;') -eq 18870) 'Rehearsal restores posted transaction reference'
    Check ((TriggerState) -eq $originalTriggers) 'Rehearsal restores all trigger definitions and enabled states'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18879) 'Rehearsal does not reseed or advance Order identity'
    $injected=$fixture.Replace('SELECT o.* INTO #HORenumberHeaders',"THROW 52899,'Injected maintenance failure',1; SELECT o.* INTO #HORenumberHeaders")
    Reject {Sql $injected} 'Injected maintenance failure' 'Failure after trigger suspension rolls everything back'
    Check ((TriggerState) -eq $originalTriggers) 'Failure restores trigger states'
    $injected=$fixture.Replace('DECLARE update_cursor CURSOR',"THROW 52899,'Injected after header copy',1; DECLARE update_cursor CURSOR")
    Reject {Sql $injected} 'Injected after header copy' 'Failure after both headers are copied rolls back'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID IN(18870,18879);') -eq 2 -and (Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18869;') -eq 0 -and (TriggerState) -eq $originalTriggers) 'Header copy failure restores original IDs and trigger states'
    $injected=$fixture.Replace('DECLARE verify_cursor CURSOR',"UPDATE dbo.OrderEntry SET Price=Price+1 WHERE ID=1; DECLARE verify_cursor CURSOR")
    Reject {Sql $injected} 'Post-change values differ' 'Full value comparison rejects an unintended price change'
    Check ((Scalar 'SELECT Price FROM dbo.OrderEntry WHERE ID=1;') -eq 123.45) 'Rejected price change rolls back'
    Sql 'CREATE TRIGGER dbo.HO_Unreviewed ON dbo.SOD_WO_LOGS AFTER UPDATE AS RETURN;'
    Reject {Sql $fixture} 'Trigger definitions/state changed' 'Additional trigger is rejected'
    Sql 'DROP TRIGGER dbo.HO_Unreviewed;'
    Sql 'INSERT dbo.SI_OrderPrice VALUES(18879);'
    Reject {Sql $fixture} 'unsupported order reference' 'Previously empty reference table now containing this order is rejected'
    Sql 'DELETE dbo.SI_OrderPrice;'
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry;'
    Seed 'OrderEntry' @{ID='99';OrderID='18869'}
    Sql 'ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Reject {Sql $fixture} 'Destination has orphaned references' 'Existing orphan at destination is rejected'
    Sql 'DISABLE TRIGGER ALL ON dbo.OrderEntry; DELETE dbo.OrderEntry WHERE ID=99; ENABLE TRIGGER ALL ON dbo.OrderEntry;'
    Sql 'UPDATE dbo.[Order] SET Total=11042.77 WHERE ID=18879;'
    Reject {Sql $fixture} 'Orders/identity no longer match' 'Changed order amount is rejected'
    Sql 'UPDATE dbo.[Order] SET Total=11042.76 WHERE ID=18879;'
    $historyBefore=Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;'
    $commit=$fixture.Replace('DECLARE @Commit bit = 0;','DECLARE @Commit bit = 1;')
    Sql $commit
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18879;') -eq 0 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=18869;') -eq 30200.54 -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=18870;') -eq 11042.76) 'Overlapping old/new IDs map to the correct headers without double remapping'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=18869;') -eq 4 -and (Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry WHERE OrderID=18870;') -eq 1) 'All existing line IDs and quantities survive under the correct order'
    Check ((Scalar 'SELECT RecallID FROM dbo.[Transaction] WHERE TransactionNumber=23266;') -eq 18869 -and (Scalar 'SELECT TransactionNumber FROM dbo.OrderHistory WHERE ID=19857 AND OrderID=18869;') -eq 23266) 'Transaction 23266 and history identity stay unchanged; order references change'
    Check ((Scalar 'SELECT GroupTo FROM dbo.Queueing WHERE id=45800 AND OrderID=18868;') -eq 18870 -and (Scalar 'SELECT Status FROM dbo.Queueing WHERE id=45867 AND OrderID=18869;') -eq 1) 'Grouping references change while queue IDs, ownership and statuses survive'
    Check ((Scalar "SELECT CONVERT(varchar(max),CONVERT(varbinary(max),ReceiptCompressed),2) FROM dbo.Journal WHERE ID=23804 AND ReferenceID=18869;") -eq '010203040506' -and (Scalar 'SELECT ReferenceID FROM dbo.Journal WHERE ID=23805;') -eq 18870) 'Order journal link changes; compressed bytes and unrelated sale journal stay intact'
    Check ((Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='18869' AND [Type] LIKE 'Order%';") -eq 2 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE ReferenceID='18870' AND [Type] LIKE 'Order%';") -eq 2 -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE [Type]='Sales Transaction Logs' AND ReferenceID IN('18870','18879');") -eq 2) 'Only order log types are mapped; sales/purchase IDs are preserved'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;') -eq ($historyBefore+2) -and (Scalar "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Order ID changed';") -eq 2) 'Original history is preserved and two explicit mapping audit events are appended'
    Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=1;') -eq 100 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=1;') -eq 7) 'Inventory and commitments are unchanged'
    Check ((TriggerState) -eq $originalTriggers) 'Commit restores exact original enabled trigger definitions'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18879) 'Commit leaves identity reseeding to the operator'
    Reject {Sql $commit} 'Orders/identity no longer match' 'A repeated commit is rejected without remapping the second order'
    Sql 'UPDATE dbo.[Transaction] SET RecallID=18870 WHERE TransactionNumber=23266;'
    Reject {Sql $reseed} 'transaction/history/receipt links do not match' 'Reseed rejects a changed posted transaction link'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18879) 'Rejected reseed preserves identity'
    Sql 'UPDATE dbo.[Transaction] SET RecallID=18869 WHERE TransactionNumber=23266;'
    $historyCount=Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;'
    Sql $reseed
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18870) 'Saved reseed script sets the current identity to 18870'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.SOD_WO_History;') -eq $historyCount -and (TriggerState) -eq $originalTriggers -and (Scalar 'SELECT Total FROM dbo.[Order] WHERE ID=18869;') -eq 30200.54) 'Reseed preserves rows, totals, audit history and triggers'
    Sql $reseed
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18870) 'Repeated reseed is an unchanged no-op before any new allocation'
    # Exercise a real automatic allocation in the disposable fixture only.
    $columns=Scalar "SELECT STUFF((SELECT ','+QUOTENAME(name) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.[Order]') AND is_identity=0 AND system_type_id<>189 ORDER BY column_id FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,1,'');"
    Sql "INSERT dbo.[Order]($columns) SELECT $columns FROM dbo.[Order] WHERE ID=18870;"
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.[Order] WHERE ID=18871;') -eq 1) 'Next real automatic Order allocation is 18871'
    Reject {Sql $reseed} 'Maximum Order ID must be 18870' 'Reseed rejects later order activity'
    Check ((Scalar "SELECT IDENT_CURRENT('dbo.Order');") -eq 18871) 'Rejected repeat does not rewind the new allocation'
    Write-Output "PASS: $checks isolated HO Order renumber checks. No store database was modified."
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{if($master.State -eq [Data.ConnectionState]::Open){$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('HO_PO_DB') IS NOT NULL BEGIN ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE HO_PO_DB; END;";$null=$cmd.ExecuteNonQuery();$cmd.Dispose()}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
