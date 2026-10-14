# Read-only comparison of the two recall components; this does not measure the UI.
param([int]$OrderID = 282844, [string]$Server = 'localhost', [string]$Database = 'GSC_STORE_DB')
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
$builder['Application Name'] = 'WorkOrderRecallReadOnlyBenchmark'
$context = [WorkOrderEntry.ItemLookUpDataContext]::new($builder.ConnectionString)
$results = [Collections.Generic.List[object]]::new()
try {
    $context.Connection.Open()
    $command = $context.Connection.CreateCommand()
    try {
        $command.CommandText = 'SELECT CustomerID FROM dbo.[Order] WHERE ID=@order;'
        $null = $command.Parameters.AddWithValue('@order',$OrderID)
        $customer = $command.ExecuteScalar()
        if ($null -eq $customer -or $customer -is [DBNull]) { throw 'Requested order was not found.' }
        $command.CommandText = 'SELECT i.ItemLookupCode,oe.ItemID FROM dbo.OrderEntry oe JOIN dbo.Item i ON oe.ItemID=i.ID WHERE oe.OrderID=@order ORDER BY oe.ID;'
        $reader = $command.ExecuteReader()
        $entries = [Collections.Generic.List[object]]::new()
        try { while ($reader.Read()) { $entries.Add([pscustomobject]@{Code=$reader.GetString(0);ItemID=$reader.GetInt32(1)}) } }
        finally { $reader.Dispose() }
    } finally { $command.Dispose() }
    $codes = [string[]]@($entries | ForEach-Object { $_.Code })
    $priceSql = @'
SELECT TOP (1) te.Price FROM dbo.[Transaction] t
JOIN dbo.TransactionEntry te ON t.TransactionNumber=te.TransactionNumber
JOIN dbo.Item i ON te.ItemID=i.ID
WHERE t.CustomerID=@customer AND i.ItemLookupCode=@code AND te.Quantity>0
{invoiceFilter}
ORDER BY t.Time DESC,t.TransactionNumber DESC;
'@
    for ($run = 1; $run -le 3; $run++) {
        $legacyPrices = [Collections.Generic.Dictionary[string,decimal]]::new([StringComparer]::Ordinal)
        $queryCount = 0
        $watch = [Diagnostics.Stopwatch]::StartNew()
        foreach ($entry in $entries) {
            $value = 0
            if ($customer -gt 0) {
                $command = $context.Connection.CreateCommand()
                try {
                    $command.CommandTimeout = 30
                    $command.CommandText = $priceSql.Replace('{invoiceFilter}',"AND (t.ReferenceNumber LIKE N'CH%' OR t.ReferenceNumber LIKE N'CI%')")
                    $null = $command.Parameters.AddWithValue('@customer',[int]$customer)
                    $null = $command.Parameters.AddWithValue('@code',$entry.Code.Trim())
                    $value = $command.ExecuteScalar(); $queryCount++
                    if ($null -eq $value -or $value -eq 0) {
                        $command.CommandText = $priceSql.Replace('{invoiceFilter}','')
                        $value = $command.ExecuteScalar(); $queryCount++
                    }
                } finally { $command.Dispose() }
            }
            if ($null -eq $value) { $value = 0 }
            $legacyPrices[$entry.Code.Trim()] = [decimal]$value
        }
        $watch.Stop()
        $legacyElapsed = $watch.Elapsed.TotalMilliseconds
        $log = [IO.StringWriter]::new(); $context.Log = $log
        $watch.Restart()
        $batchPrices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,[int]$customer,$codes)
        $watch.Stop()
        $priceMatch = $true
        foreach ($entry in $entries) {
            if ($legacyPrices[$entry.Code.Trim()] -ne [WorkOrderEntry.RecallReadData]::LastPriceOrZero($batchPrices,$entry.Code)) { $priceMatch = $false }
        }
        $results.Add([pscustomobject]@{Phase='LegacyLastPrices';Run=$run;Lines=$entries.Count;Queries=$queryCount;Milliseconds=[math]::Round($legacyElapsed,3);Matches=$priceMatch})
        $results.Add([pscustomobject]@{Phase='BatchLastPrices';Run=$run;Lines=$entries.Count;Queries=[regex]::Matches($log.ToString(),'(?im)^(SELECT |WITH )').Count;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,3);Matches=$priceMatch})
        $context.Log = $null
        $legacyQueue = [Collections.Generic.Dictionary[int,object]]::new()
        $watch.Restart()
        foreach ($entry in $entries) {
            $command = $context.Connection.CreateCommand()
            try {
                $command.CommandText = 'SELECT TOP (1) qi.QtyPre,qi.Status,qi.Picker FROM dbo.QueueingItems qi JOIN dbo.Queueing q ON qi.QueueingID=q.id WHERE qi.ItemID=@item AND q.OrderID=@order;'
                $null = $command.Parameters.AddWithValue('@item',$entry.ItemID)
                $null = $command.Parameters.AddWithValue('@order',$OrderID)
                $reader = $command.ExecuteReader()
                try {
                    if ($reader.Read()) {
                        $legacyQueue[$entry.ItemID] = [pscustomobject]@{Prepared=$reader.GetValue(0);Status=$reader.GetValue(1);Picker=$reader.GetValue(2)}
                    }
                } finally { $reader.Dispose() }
            } finally { $command.Dispose() }
        }
        $watch.Stop(); $legacyElapsed = $watch.Elapsed.TotalMilliseconds
        $log = [IO.StringWriter]::new(); $context.Log = $log
        $watch.Restart()
        $batchQueue = [WorkOrderEntry.RecallReadData]::LoadQueue($context,$OrderID)
        $watch.Stop()
        $queueMatch = $true
        foreach ($entry in $entries) {
            if ($legacyQueue.ContainsKey($entry.ItemID) -ne $batchQueue.ContainsKey($entry.ItemID)) { $queueMatch = $false; continue }
            if ($legacyQueue.ContainsKey($entry.ItemID)) {
                $previous = $legacyQueue[$entry.ItemID]; $current = $batchQueue[$entry.ItemID]
                foreach ($field in @('Prepared','Status','Picker')) {
                    $previousValue = if ($previous.$field -is [DBNull]) { $null } else { $previous.$field }
                    if ($previousValue -ne $current.$field) { $queueMatch = $false }
                }
            }
        }
        $results.Add([pscustomobject]@{Phase='LegacyQueue';Run=$run;Lines=$entries.Count;Queries=$entries.Count;Milliseconds=[math]::Round($legacyElapsed,3);Matches=$queueMatch})
        $results.Add([pscustomobject]@{Phase='BatchQueue';Run=$run;Lines=$entries.Count;Queries=[regex]::Matches($log.ToString(),'(?im)^SELECT ').Count;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,3);Matches=$queueMatch})
        $context.Log = $null
    }
    $output = Join-Path $repo 'TestResults\Recall-ReadOnly-Benchmark.csv'
    $results | Export-Csv -LiteralPath $output -NoTypeInformation
    $results | Format-Table -AutoSize
    if (@($results | Where-Object { -not $_.Matches }).Count -gt 0) { throw 'Read results differed; concurrent store changes may require another comparison.' }
    Write-Output "Read-only benchmark: $output"
} finally { $context.Dispose() }
