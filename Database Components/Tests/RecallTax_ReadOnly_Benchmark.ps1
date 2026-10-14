# SELECT-only tax flag comparison; no application settings or business records are changed.
param([int]$OrderID = 283845, [string]$Server = 'localhost', [string]$Database = 'GSC_STORE_DB')
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -OrderID $OrderID -Server $Server -Database $Database
    exit $LASTEXITCODE
}
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
Add-Type -AssemblyName System.Data, System.Data.Linq
$null = [Reflection.Assembly]::LoadFrom((Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'))
$builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source'] = $Server
$builder['Initial Catalog'] = $Database
$builder['Integrated Security'] = $true
$builder['Connect Timeout'] = 5
$builder['Application Name'] = 'WorkOrderRecallTaxReadOnlyBenchmark'
$context = [WorkOrderEntry.ItemLookUpDataContext]::new($builder.ConnectionString)
$results = [Collections.Generic.List[object]]::new()
try {
    $context.Connection.Open()
    $command = $context.Connection.CreateCommand()
    try {
        $command.CommandText = 'SELECT i.ItemLookupCode FROM dbo.OrderEntry oe JOIN dbo.Item i ON oe.ItemID=i.ID WHERE oe.OrderID=@order ORDER BY oe.ID;'
        $null = $command.Parameters.AddWithValue('@order',$OrderID)
        $reader = $command.ExecuteReader()
        $codes = [Collections.Generic.List[string]]::new()
        try { while ($reader.Read()) { $codes.Add($reader.GetString(0)) } }
        finally { $reader.Dispose() }
    } finally { $command.Dispose() }
    if ($codes.Count -eq 0) { throw 'No item lines were found for the requested order.' }
    for ($run = 1; $run -le 3; $run++) {
        $legacy = [Collections.Generic.Dictionary[string,bool]]::new([StringComparer]::Ordinal)
        $queryCount = 0
        $watch = [Diagnostics.Stopwatch]::StartNew()
        foreach ($code in $codes) {
            if ($code.Length -eq 0) { continue }
            $command = $context.Connection.CreateCommand()
            try {
                $command.CommandText = 'SELECT TOP (1) Taxable FROM dbo.Item WHERE ItemLookupCode=@code;'
                $null = $command.Parameters.AddWithValue('@code',$code)
                $value = $command.ExecuteScalar(); $queryCount++
                $legacy[$code] = if ($null -eq $value -or $value -is [DBNull]) { $false } else { [bool]$value }
            } finally { $command.Dispose() }
        }
        $watch.Stop(); $legacyElapsed = $watch.Elapsed.TotalMilliseconds
        $log = [IO.StringWriter]::new(); $context.Log = $log
        $watch.Restart()
        $batch = [WorkOrderEntry.RecallReadData]::LoadTaxability($context,$codes)
        $watch.Stop()
        $match = $legacy.Count -eq $batch.Count
        foreach ($code in $legacy.Keys) {
            if (-not $batch.ContainsKey($code) -or $legacy[$code] -ne $batch[$code]) { $match = $false }
        }
        $results.Add([pscustomobject]@{Phase='LegacyItemTaxFlags';OrderID=$OrderID;Run=$run;Lines=$codes.Count;Queries=$queryCount;Milliseconds=[math]::Round($legacyElapsed,3);Matches=$match})
        $results.Add([pscustomobject]@{Phase='BatchItemTaxFlags';OrderID=$OrderID;Run=$run;Lines=$codes.Count;Queries=[regex]::Matches($log.ToString(),'(?im)^SELECT ').Count;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,3);Matches=$match})
        $context.Log = $null
    }
    $output = Join-Path $repo ("TestResults\RecallTax-ReadOnly-{0}-{1}.csv" -f $OrderID,[DateTime]::Now.ToString('yyyy-MM-dd'))
    $results | Export-Csv -LiteralPath $output -NoTypeInformation
    $results | Format-Table -AutoSize
    if (@($results | Where-Object { -not $_.Matches }).Count -gt 0) { throw 'Tax flags differed; another session may have changed an item during the comparison.' }
    Write-Output "SELECT-only tax comparison: $output"
} finally { $context.Dispose() }
