param([Parameter(Mandatory=$true)][string]$BaselineAssembly)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$suffix = [Guid]::NewGuid().ToString('N').Substring(0,8)
$owned = 'WO_Price_' + $suffix
$server = '(localdb)\' + $owned
$directory = Join-Path $repo ('TestResults\PriceLevelQueries-' + $suffix)
$null = New-Item -ItemType Directory -Path $directory
$localdb = (Get-Command SqlLocalDB.exe).Source
$created = $false; $master = $null; $db = $null
function Sql($connection,$text) {
    $cmd = $connection.CreateCommand()
    try { $cmd.CommandText=$text; $cmd.CommandTimeout=30; $null=$cmd.ExecuteNonQuery() } finally { $cmd.Dispose() }
}
function Scalar($connection,$text) {
    $cmd = $connection.CreateCommand()
    try { $cmd.CommandText=$text; return $cmd.ExecuteScalar() } finally { $cmd.Dispose() }
}
$app = Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash = (Get-FileHash -LiteralPath ($app+'.config')).Hash
try {
    & $localdb create $owned | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'LocalDB creation failed' }; $created=$true
    & $localdb start $owned | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'LocalDB start failed' }
    $master=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=master;Integrated Security=True;Pooling=False")
    $master.Open(); Sql $master "CREATE DATABASE [$owned]; ALTER DATABASE [$owned] SET COMPATIBILITY_LEVEL=130;"
    $db=[Data.SqlClient.SqlConnection]::new("Server=$server;Database=$owned;Integrated Security=True;Pooling=False")
    $db.Open()
    Sql $db @'
CREATE TABLE dbo.Item(ItemLookupCode varchar(30),Price money,PriceA money,PriceB money,PriceC money,Cost money);
CREATE TABLE dbo.SOD_WO_Conf(id int IDENTITY PRIMARY KEY,Password nvarchar(50),LimitEntry int,PriceLevel nvarchar(50),StoreID int,AllowPriceChange bit,Password2 nvarchar(50),Pass2Username nvarchar(50),CustType nvarchar(max),Password3 nvarchar(50),Pass3Username nvarchar(50));
INSERT dbo.SOD_WO_Conf(PriceLevel) VALUES(N'NONE');
'@
    Sql $db "CREATE FUNCTION dbo.SOD_fntbl_PriceLevel(@vItemCode varchar(30)) RETURNS TABLE AS RETURN (SELECT Price,PriceA,PriceB,PriceC,Cost FROM dbo.Item WHERE ItemLookupCode=@vItemCode);"
    $summary=@{}
    foreach ($variant in @('Before','After')) {
        $actor="PriceLevel_${suffix}_$variant"; $session="PL_${suffix}_$variant"
        Sql $master "CREATE EVENT SESSION [$session] ON SERVER ADD EVENT sqlserver.rpc_completed(ACTION(sqlserver.client_app_name) WHERE ([sqlserver].[client_app_name]=N'$actor')), ADD EVENT sqlserver.sql_batch_completed(ACTION(sqlserver.client_app_name) WHERE ([sqlserver].[client_app_name]=N'$actor')) ADD TARGET package0.ring_buffer(SET max_events_limit=10000,max_memory=4096); ALTER EVENT SESSION [$session] ON SERVER STATE=START;"
        $resultPath=Join-Path $directory ($variant+'.json')
        $requestPath=Join-Path $directory ($variant+'-request.json')
        @{Server=$server;Database=$owned;Actor=$actor;Assembly=$(if($variant -eq 'Before'){[IO.Path]::GetFullPath($BaselineAssembly)}else{$app});ResultFile=$resultPath} | ConvertTo-Json | Set-Content -LiteralPath $requestPath
        $worker=Join-Path $PSScriptRoot 'Fixtures\PriceLevel_Query_Worker.ps1'
        $process=Start-Process -FilePath "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -ArgumentList @('-NoProfile','-STA','-ExecutionPolicy','Bypass','-File',('"'+$worker+'"'),'-RequestFile',('"'+$requestPath+'"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $directory ($variant+'.out')) -RedirectStandardError (Join-Path $directory ($variant+'.err'))
        if (!$process.WaitForExit(45000)) { $process.Kill(); throw 'Price fixture worker timed out' }
        if ($process.ExitCode -ne 0) { throw "Price fixture worker failed: $variant; see $directory" }
        $xml=[xml](Scalar $master "SELECT CONVERT(nvarchar(max),t.target_data) FROM sys.dm_xe_session_targets t JOIN sys.dm_xe_sessions s ON s.address=t.event_session_address WHERE s.name=N'$session'")
        $xml.Save((Join-Path $directory ($variant+'-commands.xml')))
        if ([int]$xml.RingBufferTarget.droppedCount -ne 0 -or $xml.RingBufferTarget.truncated -eq '1') { throw 'Incomplete command trace' }
        $summary[$variant]=@($xml.RingBufferTarget.event).Count
        Sql $master "ALTER EVENT SESSION [$session] ON SERVER STATE=STOP; DROP EVENT SESSION [$session] ON SERVER;"
    }
    $before=@(Get-Content (Join-Path $directory 'Before.json') -Raw | ConvertFrom-Json)
    $after=@(Get-Content (Join-Path $directory 'After.json') -Raw | ConvertFrom-Json)
    if ($before.Count -ne 125 -or $after.Count -ne 125) { throw 'Incomplete comparison' }
    for ($i=0;$i -lt $before.Count;$i++) {
        foreach ($property in @('Key','Rows','Lowest','Selected','SelectedColumn','PricePass','Pass','Allow','Approved')) {
            if (($before[$i].$property | ConvertTo-Json -Depth 7 -Compress) -cne ($after[$i].$property | ConvertTo-Json -Depth 7 -Compress)) { throw "Result mismatch: case $i / $property" }
        }
    }
    if ($summary.Before -ne 825 -or $summary.After -ne 250) { throw "Unexpected SQL command counts: $($summary|ConvertTo-Json -Compress)" }
    $summary['MatchedCases']=125
    $summary['BeforeMedianMs']=(@($before[0..74].Milliseconds | Sort-Object))[37]
    $summary['AfterMedianMs']=(@($after[0..74].Milliseconds | Sort-Object))[37]
    $summary['Engine']=Scalar $master "SELECT CONVERT(nvarchar(128),SERVERPROPERTY('ProductVersion'))"
    $summary | ConvertTo-Json | Set-Content (Join-Path $directory 'Summary.json')
    "PASS: 125 compiled before/after cases match; SQL commands $($summary.Before) -> $($summary.After). Dialog median ms $($summary.BeforeMedianMs) -> $($summary.AfterMedianMs). Artifacts: $directory"
} finally {
    if ($db) { $db.Dispose() }
    if ($master) {
        try { Sql $master "IF DB_ID(N'$owned') IS NOT NULL BEGIN ALTER DATABASE [$owned] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$owned]; END;" }
        finally { $master.Dispose() }
    }
    if ($created) {
        & $localdb stop $owned -k | Out-Null
        & $localdb delete $owned | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Owned LocalDB cleanup failed' }
        'Owned fixture database dropped and LocalDB instance deleted.'
    }
    if ((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash) { throw 'Store configuration changed' }
}
