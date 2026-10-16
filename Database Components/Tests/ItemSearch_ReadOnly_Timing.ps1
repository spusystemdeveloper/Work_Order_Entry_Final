# Read-only SQL timing. Does not start the app, log in, save orders, or change SQL objects.
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Server,
    [Parameter(Mandatory=$true)][string]$Database,
    [ValidateRange(1,5)][int]$Repeats=3
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$directory=Join-Path $repo ('TestResults\ItemSearchTiming-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $directory
$builder=[Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source']=$Server
$builder['Initial Catalog']=$Database
$builder['Integrated Security']=$true
$builder['Connect Timeout']=5
$builder['Application Name']='WorkOrderEntry_ReadOnly_SearchTiming'
$connection=[Data.SqlClient.SqlConnection]::new($builder.ConnectionString)
$samples=[Collections.Generic.List[object]]::new()
$sqlMessages=[Collections.Generic.List[string]]::new()
$handler=[Data.SqlClient.SqlInfoMessageEventHandler]{param($sender,$eventArgs) $sqlMessages.Add($eventArgs.Message)}
$connection.add_InfoMessage($handler)
$searches=@(
    [pscustomobject]@{Name='BOOK';Terms=@('BOOK')},
    [pscustomobject]@{Name='BOOK+PAPER';Terms=@('BOOK','PAPER')},
    [pscustomobject]@{Name='PAPER';Terms=@('PAPER')},
    [pscustomobject]@{Name='BOOK+NOTE';Terms=@('BOOK','NOTE')}
)
$firstHashes=@{}
try{
    $connection.Open()
    $cmd=$connection.CreateCommand()
    try{
        $cmd.CommandText=@'
SELECT DB_NAME() AS DatabaseName, compatibility_level, collation_name,
    (SELECT COUNT_BIG(*) FROM dbo.Item) AS ItemRows,
    (SELECT definition FROM sys.sql_modules WHERE object_id=OBJECT_ID(N'dbo.SOD_ViewItemsWO')) AS ViewDefinition
FROM sys.databases WHERE database_id=DB_ID();
'@
        $metadata=[Data.DataTable]::new()
        $adapter=[Data.SqlClient.SqlDataAdapter]::new($cmd)
        try{$null=$adapter.Fill($metadata)}finally{$adapter.Dispose()}
        if($metadata.Rows.Count -ne 1){throw 'Target metadata was unavailable'}
        $row=$metadata.Rows[0]
        if($row.ViewDefinition -is [DBNull]){throw 'View definition is unavailable; verify permissions and target'}
        $sha=[Security.Cryptography.SHA256]::Create()
        try{$viewHash=[BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes([string]$row.ViewDefinition))).Replace('-','')}finally{$sha.Dispose()}
        [pscustomobject]@{Server=$Server;Database=[string]$row.DatabaseName;Compatibility=[int]$row.compatibility_level;Collation=[string]$row.collation_name;ItemRows=[long]$row.ItemRows;ViewSHA256=$viewHash} |
            ConvertTo-Json | Set-Content -LiteralPath (Join-Path $directory 'Target.json') -Encoding UTF8
        $metadata.Dispose()
        $cmd.CommandText='SET STATISTICS IO ON; SET STATISTICS TIME ON;'
        $null=$cmd.ExecuteNonQuery()
    }finally{$cmd.Dispose()}
    foreach($round in 1..$Repeats){foreach($search in $searches){
        # Keep the application's contains/wildcard semantics and unordered TOP 500.
        # Quote literals only; do not replace LIKE with prefix/full-text matching.
        $terms=@($search.Terms | ForEach-Object {$_.Replace("'","''")})
        $filter=[string]::Join("%' AND [FULLDESC] Like '%",[string[]]$terms)
        $sql="SELECT TOP 500 * FROM SOD_VIEWITEMSWO  WHERE Inactive = 0 AND FULLDESC Like '%"+$filter+"%'"
        $dataset=[Data.DataSet]::new()
        # Fill all results, including a tiny final sentinel, so SqlClient consumes
        # the IO/time messages sent after the main result's last row. Filling only
        # one DataTable can discard those trailing messages when its reader closes.
        $adapter=[Data.SqlClient.SqlDataAdapter]::new(($sql+'; SELECT 1 AS DiagnosticEnd;'),$connection)
        $adapter.SelectCommand.CommandTimeout=30
        $messageStart=$sqlMessages.Count
        $watch=[Diagnostics.Stopwatch]::StartNew()
        try{$null=$adapter.Fill($dataset)}finally{$watch.Stop();$adapter.Dispose()}
        $table=$dataset.Tables[0]
        $table.TableName='SearchResults'
        try{
            # Hash in memory: export neither item descriptions/prices nor connection secrets.
            $stream=[IO.MemoryStream]::new()
            $sha=[Security.Cryptography.SHA256]::Create()
            try{
                $table.WriteXml($stream,[Data.XmlWriteMode]::WriteSchema)
                $hash=[BitConverter]::ToString($sha.ComputeHash($stream.ToArray())).Replace('-','')
            }finally{$sha.Dispose();$stream.Dispose()}
            if(!$firstHashes.ContainsKey($search.Name)){$firstHashes[$search.Name]=$hash}
            $samples.Add([pscustomobject]@{Search=$search.Name;Round=$round;Rows=$table.Rows.Count;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,1);OrderedValuesSHA256=$hash;SameAsFirst=$hash -eq $firstHashes[$search.Name]})
            Add-Content -LiteralPath (Join-Path $directory 'SQL-Statistics.txt') -Value ("Search {0}, round {1}" -f $search.Name,$round)
            for($i=$messageStart;$i -lt $sqlMessages.Count;$i++){Add-Content -LiteralPath (Join-Path $directory 'SQL-Statistics.txt') -Value $sqlMessages[$i]}
        }finally{$dataset.Dispose()}
    }}
}finally{
    $connection.remove_InfoMessage($handler)
    $connection.Dispose()
}
$samples | Export-Csv -NoTypeInformation -LiteralPath (Join-Path $directory 'Timings.csv') -Encoding UTF8
$samples | Select-Object Search,Round,Rows,Milliseconds,SameAsFirst | Format-Table -AutoSize
if(@($samples | Where-Object {!$_.SameAsFirst}).Count -gt 0){
    Write-Warning 'Results changed between reads. Check concurrent item/stock changes and the legacy unordered TOP 500 before comparing speed.'
}
Write-Output "Read-only SQL timings (not full UI timings): $directory"
