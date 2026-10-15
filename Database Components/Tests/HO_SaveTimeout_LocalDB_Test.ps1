# Synthetic LocalDB only. The fix is never run against a store by this test.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix=[Guid]::NewGuid().ToString('N').Substring(0,8)
$instance="HO_SaveTimeout_$suffix"
$directory=Join-Path $repo "TestResults\HO-SaveTimeout-$suffix"
$null=New-Item -ItemType Directory -Path $directory
$localdb=(Get-Command SqlLocalDB.exe).Source
$created=$false;$master=$null;$connection=$null;$checks=0
function Sql([string]$text,$tx=$null){
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandTimeout=30;$cmd.Transaction=$tx;$cmd.CommandText=$text;$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
}
function Scalar([string]$text,$tx=$null){
    $cmd=$connection.CreateCommand()
    try{$cmd.CommandTimeout=30;$cmd.Transaction=$tx;$cmd.CommandText=$text;return $cmd.ExecuteScalar()}finally{$cmd.Dispose()}
}
function Check([bool]$ok,[string]$name){if(-not $ok){throw "FAILED: $name"};$script:checks++;Write-Host "Passed: $name"}
function Reject([scriptblock]$action,[string]$pattern,[string]$name){
    $failed=$false;try{& $action}catch{if($_.Exception.ToString() -notmatch $pattern){throw};$failed=$true};Check $failed $name
}
function Hash-Trigger {return Scalar "SELECT CONVERT(varchar(64),HASHBYTES('SHA2_256',OBJECT_DEFINITION(OBJECT_ID('dbo.trg_OrderEntry_ItemLogs'))),2);"}
function Insert-Reads {
    $script:ioMessages=[Collections.Generic.List[string]]::new()
    $handler=[Data.SqlClient.SqlInfoMessageEventHandler]{param($sender,$eventArgs);$script:ioMessages.Add($eventArgs.Message)}
    $connection.add_InfoMessage($handler)
    $tx=$connection.BeginTransaction()
    try{
        Sql "SET STATISTICS IO ON; INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder) VALUES(1,1,3.5); SET STATISTICS IO OFF;" $tx
        $reads=0
        foreach($message in $script:ioMessages){foreach($match in [regex]::Matches($message,"Table 'SOD_Item_Logs'[^\r\n]*?logical reads (\d+)")){$reads += [int]$match.Groups[1].Value}}
        return $reads
    }finally{$tx.Rollback();$tx.Dispose();$connection.remove_InfoMessage($handler);Sql 'SET STATISTICS IO OFF;'}
}
function Log-Signature {
    $tx=$connection.BeginTransaction()
    try{
        Sql "INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder) VALUES(1,1,3.5); UPDATE dbo.OrderEntry SET QuantityOnOrder=2.25; UPDATE dbo.OrderEntry SET QuantityOnOrder=0;" $tx
        $count=Scalar "SELECT COUNT(*) FROM dbo.SOD_Item_Logs WHERE [Type] IN('Order Entry Logs','Order Update Logs');" $tx
        Check ($count -eq 2) 'Insert and nonzero edit log once each; zero-quantity edit adds no log'
        Check ((Scalar 'SELECT Quantity FROM dbo.Item WHERE ID=1;' $tx) -eq 100 -and
            (Scalar 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=1;' $tx) -eq 5) 'Logging preserves physical stock and commitments'
        return Scalar "SELECT ItemID,ItemLookupCode,[Type],ReferenceID,QuantityORIS,LastQuantity,NewQuantity,LastCommitted,NewCommitted FROM dbo.SOD_Item_Logs WHERE ItemID=1 ORDER BY [Type] FOR XML RAW('Log'),ROOT('Logs');" $tx
    }finally{$tx.Rollback();$tx.Dispose()}
}
$fix=[IO.File]::ReadAllText((Join-Path $repo 'Database Components\Recovery\Fix_HO_PO_DB_NewOrder_Save_Timeout.sql'))
$legacy=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Fixtures\HO_OrderEntry_ItemLogs_Legacy.sql'))
try{
    & $localdb create $instance -s | Out-Null
    if($LASTEXITCODE -ne 0){throw 'LocalDB instance creation failed'};$created=$true
    $master=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=master;Integrated Security=True;Pooling=False");$master.Open()
    $cmd=$master.CreateCommand();try{$cmd.CommandText='CREATE DATABASE HO_PO_DB; ALTER DATABASE HO_PO_DB SET COMPATIBILITY_LEVEL=130;';$null=$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}
    $connection=[Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Database=HO_PO_DB;Integrated Security=True;Pooling=False");$connection.Open()
    Sql @'
SET LANGUAGE us_english; SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;
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
INSERT dbo.SOD_Item_Logs(ItemID,ReferenceID,NewCommitted,[Date]) VALUES(1,'1',7,'20200102');
'@
    Sql $legacy
    $oldHash=Hash-Trigger
    Check ($oldHash -eq '90699E994049AD7B720D80E1E3149AE64ABCDEE66D5908FCBDFCD1F4316EB3FE') 'Fixture uses the exact reviewed live trigger definition'
    Reject {Sql $fix} 'Wrong SQL server' 'Unmodified live fix rejects the disposable server'
    Check ((Hash-Trigger) -eq $oldHash) 'Wrong-target rejection preserves the trigger'
    # Only this in-memory fixture copy changes the expected server declaration.
    $fixtureFix=$fix.Replace("DECLARE @ExpectedServer sysname = N'WIN-2847UGJBAV4';","DECLARE @ExpectedServer sysname = CONVERT(sysname,@@SERVERNAME);")
    if($fixtureFix -eq $fix){throw 'Expected server declaration missing'}
    $beforeReads=Insert-Reads
    $beforeSignature=Log-Signature
    Sql $fixtureFix
    Check ((Hash-Trigger) -eq $oldHash) 'Default rehearsal rolls trigger changes back'
    $commitFix=$fixtureFix.Replace('DECLARE @Commit bit = 0;','DECLARE @Commit bit = 1;')
    if($commitFix -eq $fixtureFix){throw 'Commit declaration missing'}
    Sql $commitFix
    $fixedHash=Hash-Trigger
    Check ($fixedHash -ne $oldHash) 'Explicit commit installs the reviewed conditional lookup'
    $afterSignature=Log-Signature
    Check ($beforeSignature -eq $afterSignature) 'Original and patched triggers produce identical log values for fractional insert/edit/zero cases'
    $afterReads=Insert-Reads
    Check ($beforeReads -gt 10 -and $afterReads -lt ($beforeReads * 0.1)) 'New-line insert removes over 90 percent of synthetic log-table logical reads'
    [pscustomobject]@{BeforeLogicalReads=$beforeReads;AfterLogicalReads=$afterReads;FixtureRows=100001}|Export-Csv -LiteralPath (Join-Path $directory 'InsertLogReads.csv') -NoTypeInformation
    Write-Output "Synthetic new-line log reads: $beforeReads before; $afterReads after."
    Sql $commitFix
    Check ((Hash-Trigger) -eq $fixedHash) 'Repeated execution preserves the fixed definition'
    Check ((Scalar 'SELECT COUNT(*) FROM dbo.OrderEntry;') -eq 0 -and
        (Scalar 'SELECT COUNT(*) FROM dbo.SOD_Item_Logs;') -eq 100001) 'Rolled-back fixture operations leave no extra orders or log rows'
    Sql 'DISABLE TRIGGER dbo.trg_OrderEntry_ItemLogs ON dbo.OrderEntry;'
    Reject {Sql $commitFix} 'missing or disabled' 'Disabled trigger is rejected rather than silently re-enabled'
    Sql 'DROP TRIGGER dbo.trg_OrderEntry_ItemLogs;'
    Sql ($legacy+'-- Fixture customization')
    $customHash=Hash-Trigger
    Reject {Sql $commitFix} 'differs from the diagnosed' 'Unknown custom trigger definition is rejected'
    Check ((Hash-Trigger) -eq $customHash) 'Rejected customization is preserved'
    Write-Output "PASS: $checks HO Save timeout checks; artifacts: $directory"
}finally{
    if($connection){$connection.Dispose()}
    if($master){try{$cmd=$master.CreateCommand();$cmd.CommandText="IF DB_ID('HO_PO_DB') IS NOT NULL BEGIN ALTER DATABASE HO_PO_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE HO_PO_DB; END;";$null=$cmd.ExecuteNonQuery();$cmd.Dispose()}finally{$master.Dispose()}}
    if($created){& $localdb stop $instance -k | Out-Null;& $localdb delete $instance | Out-Null}
}
