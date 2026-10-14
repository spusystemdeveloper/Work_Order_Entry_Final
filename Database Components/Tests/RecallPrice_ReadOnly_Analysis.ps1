# Price timing, I/O, and plan inspection. Uses SELECTs/session diagnostics only.
param([string]$OrderIDsText = '283845,282844', [string]$Server = 'localhost', [string]$Database = 'GSC_STORE_DB',
      [ValidateSet('Baseline','AfterIndex')][string]$RunLabel = 'Baseline')
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -OrderIDsText $OrderIDsText -Server $Server -Database $Database -RunLabel $RunLabel
    exit $LASTEXITCODE
}
$OrderIDs = [int[]]($OrderIDsText.Split(',') | ForEach-Object {[int]$_})
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
Add-Type -AssemblyName System.Data, System.Data.Linq
$null = [Reflection.Assembly]::LoadFrom((Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'))
$builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source'] = $Server; $builder['Initial Catalog'] = $Database
$builder['Integrated Security'] = $true; $builder['Connect Timeout'] = 5
$builder['Application Name'] = 'WorkOrderPriceReadOnlyAnalysis'
$context = [WorkOrderEntry.ItemLookUpDataContext]::new($builder.ConnectionString)
$rows = [Collections.Generic.List[object]]::new()
$planRows = [Collections.Generic.List[object]]::new()
$ioMessages = [Collections.Generic.List[string]]::new()
$handler = [System.Data.SqlClient.SqlInfoMessageEventHandler]{param($sender,$eventArgs) $ioMessages.Add($eventArgs.Message)}
function Get-PriceSignature($prices) {
    $keys = [Collections.Generic.List[string]]::new($prices.Keys)
    $keys.Sort([StringComparer]::Ordinal)
    $buffer = [IO.MemoryStream]::new()
    $writer = [IO.BinaryWriter]::new($buffer,[Text.Encoding]::UTF8,$true)
    $hash = [Security.Cryptography.SHA256]::Create()
    try {
        foreach ($key in $keys) {
            $writer.Write($key)
            $writer.Write($prices[$key].ToString([Globalization.CultureInfo]::InvariantCulture))
        }
        $writer.Flush()
        return [BitConverter]::ToString($hash.ComputeHash($buffer.ToArray())).Replace('-','')
    } finally {$writer.Dispose(); $buffer.Dispose(); $hash.Dispose()}
}
try {
    $context.Connection.Open()
    $context.Connection.add_InfoMessage($handler)
    foreach ($orderID in $OrderIDs) {
        $command = $context.Connection.CreateCommand()
        try {
            $command.CommandText = 'SELECT CustomerID FROM dbo.[Order] WHERE ID=@order;'
            $null = $command.Parameters.AddWithValue('@order',$orderID)
            $customer = $command.ExecuteScalar()
            if ($null -eq $customer) { throw "Order $orderID was not found." }
            $command.CommandText = 'SELECT i.ItemLookupCode FROM dbo.OrderEntry oe JOIN dbo.Item i ON i.ID=oe.ItemID WHERE oe.OrderID=@order ORDER BY oe.ID;'
            $codes = [Collections.Generic.List[string]]::new()
            $reader = $command.ExecuteReader()
            try { while ($reader.Read()) {$codes.Add($reader.GetString(0))} }
            finally {$reader.Dispose()}
        } finally {$command.Dispose()}
        $uniqueCodes = [string[]]@($codes | ForEach-Object {$_.Trim()} | Where-Object {$_.Length -gt 0} | Select-Object -Unique)
        $queryText = $null
        for ($run=1; $run -le 5; $run++) {
            $log = [IO.StringWriter]::new(); $context.Log = $log
            $watch = [Diagnostics.Stopwatch]::StartNew()
            $prices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,[int]$customer,$codes)
            $watch.Stop()
            $rows.Add([pscustomobject]@{OrderID=$orderID;Run=$run;Lines=$codes.Count;ReturnedCodes=$prices.Count;Milliseconds=[math]::Round($watch.Elapsed.TotalMilliseconds,3);PriceSignature=(Get-PriceSignature $prices)})
            if ($null -eq $queryText) {
                $queryText = [regex]::Match($log.ToString(),'(?m)^WITH Requested[^\r\n]*').Value
            }
        }
        $context.Log = $null
        if ([string]::IsNullOrEmpty($queryText)) { continue }
        # One separate diagnostic execution; its plan overhead is outside timed samples.
        $command = $context.Connection.CreateCommand()
        try {
            $command.CommandTimeout = 30
            $command.CommandText = 'SET STATISTICS IO ON; SET STATISTICS XML ON; ' + $queryText + '; SET STATISTICS XML OFF; SET STATISTICS IO OFF;'
            $null = $command.Parameters.AddWithValue('@p0',[int]$customer)
            for ($index=0; $index -lt [Math]::Min(200,$uniqueCodes.Count); $index++) {
                $parameter = $command.Parameters.Add('@p'+($index+1),[System.Data.SqlDbType]::NVarChar,4000)
                $parameter.Value = $uniqueCodes[$index]
            }
            $ioMessages.Clear()
            $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
            $data = [System.Data.DataSet]::new()
            try {$null = $adapter.Fill($data)} finally {$adapter.Dispose()}
            if ($data.Tables.Count -lt 2) {throw 'No actual query plan was returned.'}
            $xmlText = [string]$data.Tables[$data.Tables.Count-1].Rows[0][0]
            $planPath = Join-Path $repo "TestResults\RecallPrice-$RunLabel-$orderID.sqlplan"
            [IO.File]::WriteAllText($planPath,$xmlText)
            $ioMessages | Set-Content -LiteralPath (Join-Path $repo "TestResults\RecallPrice-$RunLabel-$orderID-IO.txt")
            [xml]$plan = $xmlText
            $namespaces = [Xml.XmlNamespaceManager]::new($plan.NameTable)
            $namespaces.AddNamespace('p','http://schemas.microsoft.com/sqlserver/2004/07/showplan')
            foreach ($operation in $plan.SelectNodes('//p:RelOp',$namespaces)) {
                if ($operation.PhysicalOp -notmatch 'Scan|Seek|Lookup|Sort') {continue}
                $object = $operation.SelectSingleNode('./p:IndexScan/p:Object',$namespaces)
                $counters = $operation.SelectNodes('./p:RunTimeInformation/p:RunTimeCountersPerThread',$namespaces)
                $actualRows = 0; $rowsRead = 0; $logicalReads = 0
                foreach ($counter in $counters) {
                    $actualRows += [double]$counter.ActualRows
                    $rowsRead += [double]$counter.ActualRowsRead
                    $logicalReads += [double]$counter.ActualLogicalReads
                }
                $planRows.Add([pscustomobject]@{OrderID=$orderID;Operation=$operation.PhysicalOp;Table=if($object){$object.Table}else{''};Index=if($object){$object.Index}else{''};ActualRows=$actualRows;RowsRead=$rowsRead;LogicalReads=$logicalReads})
            }
            Write-Output "Order $orderID diagnostic I/O:"
            $ioMessages | Write-Output
        } finally {$command.Dispose()}
    }
    $rows | Export-Csv -LiteralPath (Join-Path $repo "TestResults\RecallPrice-$RunLabel.csv") -NoTypeInformation
    $planRows | Export-Csv -LiteralPath (Join-Path $repo "TestResults\RecallPrice-$RunLabel-PlanSummary.csv") -NoTypeInformation
    $rows | Format-Table OrderID,Run,Lines,ReturnedCodes,Milliseconds -AutoSize
    $planRows | Format-Table -AutoSize
} finally {
    $context.Connection.remove_InfoMessage($handler)
    $context.Dispose()
}
