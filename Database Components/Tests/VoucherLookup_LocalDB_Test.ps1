# Owned synthetic LocalDB only; never connects to a branch restore or live store.
param([ValidateSet(100,130)][int]$CompatibilityLevel=130)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$instance='SODWO_Voucher_'+$suffix;$database=$instance
$directory=Join-Path $repo ('TestResults\VoucherLookup-'+$suffix)
$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$migration=Get-Content (Join-Path $repo 'Database Components\Migrations\Improve_RMS_VoucherLookup.sql') -Raw
$connection=$null;$master=$null;$created=$false;$checks=0
function Sql([string]$text,$tx=$null){$q=$connection.CreateCommand();try{$q.CommandTimeout=60;$q.Transaction=$tx;$q.CommandText=$text;$null=$q.ExecuteNonQuery()}finally{$q.Dispose()}}
function Scalar([string]$text,$tx=$null){$q=$connection.CreateCommand();try{$q.CommandTimeout=60;$q.Transaction=$tx;$q.CommandText=$text;return $q.ExecuteScalar()}finally{$q.Dispose()}}
function Check([bool]$ok,[string]$label){if(-not $ok){throw "FAILED: $label"};$script:checks++;Write-Output "Passed: $label"}
function Reject([scriptblock]$action,[string]$pattern,[string]$label){$failed=$false;try{& $action}catch{if($_.Exception.ToString() -notmatch $pattern){throw};$failed=$true};Check $failed $label}
function Trial([scriptblock]$action){$tx=$connection.BeginTransaction();try{& $action $tx}finally{$tx.Rollback();$tx.Dispose()}}
function IndexCount($tx=$null){return Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Item') AND name='IX_SOD_RMS_VoucherLookup';" $tx}
function Results([int]$tender){
    $q=$connection.CreateCommand();$adapter=$null
    try{
        $q.CommandText="SELECT * from Item,Tender Where Item.TenderID = Tender.ID and Item.IsGlobalVoucher= 1 and Item.Inactive=0 and Tender.ID='$tender'"
        $adapter=[Data.SqlClient.SqlDataAdapter]::new($q);$set=[Data.DataSet]::new();$null=$adapter.Fill($set)
        $set.Tables[0].DefaultView.Sort='ID'
        $sorted=[Data.DataSet]::new();$sorted.Tables.Add($set.Tables[0].DefaultView.ToTable())
        return $sorted.GetXml()
    }finally{if($adapter){$adapter.Dispose()};$q.Dispose()}
}
function Reads {
    $script:messages=[Collections.Generic.List[string]]::new()
    $handler=[Data.SqlClient.SqlInfoMessageEventHandler]{param($sender,$e) $script:messages.Add($e.Message)}
    $connection.add_InfoMessage($handler)
    try{
        Sql "SET STATISTICS IO ON; SELECT * from Item,Tender Where Item.TenderID=Tender.ID and Item.IsGlobalVoucher=1 and Item.Inactive=0 and Tender.ID='9'; SET STATISTICS IO OFF;"
        $reads=0;foreach($msg in $script:messages){foreach($m in [regex]::Matches($msg,"Table 'Item'[^\r\n]*?logical reads (\d+)")){$reads += [int]$m.Groups[1].Value}}
        return $reads
    }finally{$connection.remove_InfoMessage($handler);Sql 'SET STATISTICS IO OFF;'}
}
try{
    & $localdb create $instance -s | Out-Null;if($LASTEXITCODE -ne 0){throw 'LocalDB creation failed.'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False;");$master.Open()
    $q=$master.CreateCommand();try{$q.CommandText="CREATE DATABASE [$database]; ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL=$CompatibilityLevel;";$null=$q.ExecuteNonQuery()}finally{$q.Dispose()}
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=$database;Integrated Security=True;Pooling=False;");$connection.Open()
    Sql 'SET LANGUAGE us_english; SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;'
    Reject {Sql $migration} 'explicit transaction is required' 'Caller transaction is required'
    Trial {param($tx) Reject {Sql $migration $tx} 'dbo.Item is missing' 'Missing Item table stops safely'}
    Sql @'
CREATE TABLE dbo.Item(ID int NOT NULL PRIMARY KEY NONCLUSTERED,DBTimeStamp rowversion,
 TenderID int NULL,IsGlobalVoucher bit NULL,Inactive bit NULL,ItemType int NOT NULL,
 Quantity float NOT NULL,QuantityCommitted float NOT NULL,Description nvarchar(100),ExtendedDescription ntext,
 Filler varchar(400) NOT NULL DEFAULT REPLICATE('x',400));
CREATE CLUSTERED INDEX IX_Item_TimeStamp ON dbo.Item(DBTimeStamp);
CREATE TABLE dbo.Tender(ID int NOT NULL PRIMARY KEY,Description nvarchar(100));
CREATE TABLE dbo.FixtureAudit(ItemID int,Delta float);
INSERT dbo.Tender VALUES(5,N'Voucher fixture'),(9,N'Empty fixture');
INSERT dbo.Item(ID,TenderID,IsGlobalVoucher,Inactive,ItemType,Quantity,QuantityCommitted,Description,ExtendedDescription)
 SELECT TOP(12000) ROW_NUMBER() OVER(ORDER BY a.object_id,b.object_id),0,0,0,0,100,3.5,N'Ordinary',N'Legacy text'
 FROM sys.all_objects a CROSS JOIN sys.all_objects b;
UPDATE dbo.Item SET TenderID=5,IsGlobalVoucher=1,Description=N'Voucher',ExtendedDescription=N'Voucher legacy text' WHERE ID IN(1,2);
UPDATE dbo.Item SET TenderID=5,IsGlobalVoucher=1,Inactive=1 WHERE ID=3;
UPDATE dbo.Item SET TenderID=NULL,IsGlobalVoucher=NULL,Inactive=NULL WHERE ID=4;
'@
    Sql 'CREATE TRIGGER dbo.Fixture_Item_Audit ON dbo.Item AFTER UPDATE AS INSERT dbo.FixtureAudit SELECT i.ID,i.QuantityCommitted-d.QuantityCommitted FROM inserted i JOIN deleted d ON d.ID=i.ID;'
    $trigger=Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.Fixture_Item_Audit'));"
    $rowImage=Scalar "SELECT (SELECT ID,DBTimeStamp,TenderID,IsGlobalVoucher,Inactive,ItemType,Quantity,QuantityCommitted,Description,CONVERT(nvarchar(max),ExtendedDescription) ExtendedDescription,Filler FROM dbo.Item ORDER BY ID FOR XML RAW, BINARY BASE64);"
    $expected=@{};foreach($tid in 5,9,-1){$expected[$tid]=Results $tid}
    $before=Reads
    Trial {param($tx) Sql $migration $tx;Check ((IndexCount $tx) -eq 1) 'Index exists within the caller transaction'}
    Check ((IndexCount) -eq 0) 'Caller rollback removes the index'
    $tx=$connection.BeginTransaction();try{Sql $migration $tx;$tx.Commit()}catch{$tx.Rollback();throw}finally{$tx.Dispose()}
    Check ((IndexCount) -eq 1) 'Caller commit persists the index'
    Check ((Scalar "SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Item') AND name='IX_SOD_RMS_VoucherLookup' AND type=2 AND is_unique=0 AND has_filter=0 AND is_disabled=0;") -eq 1) 'Index is enabled, unfiltered and non-unique'
    Check ((Scalar "SELECT COUNT(*) FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id JOIN sys.indexes ix ON ix.object_id=ic.object_id AND ix.index_id=ic.index_id WHERE ix.object_id=OBJECT_ID('dbo.Item') AND ix.name='IX_SOD_RMS_VoucherLookup' AND ((ic.key_ordinal=1 AND c.name='TenderID') OR (ic.key_ordinal=2 AND c.name='IsGlobalVoucher') OR (ic.key_ordinal=3 AND c.name='Inactive'));") -eq 3) 'Exact tested leading key order is retained'
    foreach($tid in 5,9,-1){Check ((Results $tid) -ceq $expected[$tid]) "Full lookup results unchanged for synthetic tender $tid"}
    Check ((Scalar "SELECT (SELECT ID,DBTimeStamp,TenderID,IsGlobalVoucher,Inactive,ItemType,Quantity,QuantityCommitted,Description,CONVERT(nvarchar(max),ExtendedDescription) ExtendedDescription,Filler FROM dbo.Item ORDER BY ID FOR XML RAW, BINARY BASE64);") -ceq $rowImage) 'All Item values including rowversions, ntext and fractional quantities unchanged'
    Check ((Scalar "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.Fixture_Item_Audit'));") -ceq $trigger -and (Scalar 'SELECT COUNT(*) FROM dbo.FixtureAudit;') -eq 0) 'Trigger definition unchanged and no DML trigger fired during deployment'
    $after=Reads
    Check ($before -gt 100 -and $after -lt $before*.1) 'Original lookup removes over 90 percent of synthetic Item reads'
    @([pscustomobject]@{Stage='Before';Reads=$before},[pscustomobject]@{Stage='After';Reads=$after})|Export-Csv -NoTypeInformation (Join-Path $directory 'LookupReads.csv')
    Trial {param($tx) Sql ($migration+'; CREATE TABLE dbo.CallerContinued(ID int);') $tx;Check ((IndexCount $tx) -eq 1 -and (Scalar "SELECT OBJECT_ID('dbo.CallerContinued');" $tx) -isnot [DBNull]) 'Repeat deployment reuses coverage and continues the caller batch'}
    Trial {param($tx) Sql 'DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item; CREATE INDEX DifferentBranchName ON dbo.Item(TenderID,IsGlobalVoucher,Inactive) INCLUDE(Description);' $tx;Sql $migration $tx;Check ((IndexCount $tx) -eq 0) 'Equivalent differently named coverage is reused'}
    foreach($ddl in @('CREATE INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item(Inactive,TenderID,IsGlobalVoucher);','CREATE INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item(TenderID,IsGlobalVoucher,Inactive) WHERE IsGlobalVoucher=1;','CREATE INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item(TenderID,IsGlobalVoucher,Inactive); ALTER INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item DISABLE;','CREATE UNIQUE INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item(TenderID,IsGlobalVoucher,Inactive,ID);')){
        Trial {param($tx) Sql ('DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item;'+$ddl) $tx;Reject {Sql $migration $tx} 'another definition' 'Conflicting, filtered or disabled named index stops without replacement'}
    }
    Trial {param($tx) Sql 'DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item; CREATE INDEX WrongKeyOrder ON dbo.Item(IsGlobalVoucher,TenderID,Inactive);' $tx;Sql $migration $tx;Check ((IndexCount $tx) -eq 1) 'Unreviewed key order is not mistaken for matching coverage'}
    Trial {param($tx) Sql 'DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item; ALTER TABLE dbo.Item ALTER COLUMN TenderID bigint NULL;' $tx;Reject {Sql $migration $tx} 'column types differ' 'Different branch column types stop safely'}
    Trial {param($tx) Sql 'DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item; ALTER TABLE dbo.Item DROP COLUMN IsGlobalVoucher;' $tx;Reject {Sql $migration $tx} 'column types differ' 'Missing branch filter column stops safely'}
    Trial {param($tx) Sql 'UPDATE dbo.Item WITH(ROWLOCK) SET QuantityCommitted=QuantityCommitted+0.25 WHERE ID=10 AND ItemType<>7 AND QuantityCommitted+0.25>=0;' $tx;Check ((Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=10;' $tx) -eq 3.75 -and (Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=10;' $tx) -eq 100 -and (Scalar 'SELECT Delta FROM dbo.FixtureAudit WHERE ItemID=10;' $tx) -eq .25) 'Existing atomic fractional update and DML trigger still work'}
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.FixtureAudit;') -eq 0 -and (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=10;') -eq 3.5) 'Test writes roll back without residue'
    Trial {param($tx) Sql 'DROP INDEX IX_SOD_RMS_VoucherLookup ON dbo.Item; DELETE dbo.Item;' $tx;Sql $migration $tx;Check ((IndexCount $tx) -eq 1) 'Empty compatible branch table is supported'}
    Write-Output "PASS: $checks voucher lookup checks at compatibility $CompatibilityLevel; reads $before -> $after; artifacts: $directory"
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{if($database -notmatch '^SODWO_Voucher_[a-f0-9]{8}$'){throw 'Wrong cleanup target.'};$q=$master.CreateCommand();try{$q.CommandText="IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END;";$null=$q.ExecuteNonQuery()}finally{$q.Dispose()}}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k|Out-Null;& $localdb delete $instance|Out-Null}
}
