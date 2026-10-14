# Run with x86 Windows PowerShell 5.1 after building the Debug application.
# Creates its own LocalDB instance and fixture database; never uses app credentials.
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$assemblyPath = Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$queueAssemblyPath = Join-Path $repo 'ORDER PROCESSING QUEUEING\OrderProcessingQueueing\OrderProcessingQueueing\bin\Debug\OrderProcessingQueueing.exe'
if (-not (Test-Path -LiteralPath $assemblyPath)) { throw 'Build the Debug application first.' }
if (-not (Test-Path -LiteralPath $queueAssemblyPath)) { throw 'Build the Debug queueing application first.' }
$localdb = (Get-Command SqlLocalDB.exe -ErrorAction Stop).Source
$suffix = [Guid]::NewGuid().ToString('N').Substring(0, 8)
$instance = "SODWO_Test_$suffix"
$database = "SOD_WO_Tools_Test_$suffix"
$testDirectory = Join-Path $repo "TestResults\OrderTools-$suffix"
$null = New-Item -ItemType Directory -Path $testDirectory
$filePrefix = (Join-Path $testDirectory 'OrderTools').Replace("'", "''")
$connection = $null
$master = $null
$created = $false
$checks = 0

function Execute-Sql([string]$sql, $transaction = $null) {
    $command = $connection.CreateCommand()
    try {
        $command.CommandTimeout = 30
        $command.CommandText = $sql
        $command.Transaction = $transaction
        $null = $command.ExecuteNonQuery()
    } finally { $command.Dispose() }
}
function Scalar-Sql([string]$sql, $transaction = $null) {
    $command = if ($null -ne $transaction) { $transaction.Connection.CreateCommand() }
               else { $connection.CreateCommand() }
    try {
        $command.CommandText = $sql
        $command.Transaction = $transaction
        return $command.ExecuteScalar()
    } finally { $command.Dispose() }
}
function Check([bool]$condition, [string]$description) {
    if (-not $condition) { throw "FAILED: $description" }
    $script:checks++
    Write-Output "Passed: $description"
}
function Capture-Order([int]$id) {
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($script:testConnectionString)
    try { return [WorkOrderEntry.OrderEditGuard]::Capture($context, $id, $false) }
    finally { $context.Dispose() }
}
function Check-Conflict($before, [string]$description) {
    $caught = $false
    try {
        $after = Capture-Order $before.OrderID
        [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($before, $after)
    } catch {
        $errorObject = $_.Exception
        while ($null -ne $errorObject.InnerException) { $errorObject = $errorObject.InnerException }
        if ($errorObject.GetType().FullName -ne 'WorkOrderEntry.OrderEditConflictException') { throw }
        $caught = $true
    }
    Check $caught $description
}

try {
    & $localdb create $instance -s
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the isolated LocalDB instance.' }
    $created = $true
    Add-Type -AssemblyName System.Data, System.Data.Linq
    $null = [Reflection.Assembly]::LoadFrom($assemblyPath)
    $null = [Reflection.Assembly]::LoadFrom($queueAssemblyPath)
    $master = [System.Data.SqlClient.SqlConnection]::new("Server=(localdb)\$instance;Integrated Security=true;Database=master;Connect Timeout=15;Pooling=false")
    $master.Open()
    $command = $master.CreateCommand()
    $command.CommandText = "CREATE DATABASE [$database] ON PRIMARY (NAME=N'OrderTools', FILENAME=N'${filePrefix}.mdf') LOG ON (NAME=N'OrderTools_log', FILENAME=N'${filePrefix}_log.ldf'); ALTER DATABASE [$database] SET COMPATIBILITY_LEVEL = 130;"
    $null = $command.ExecuteNonQuery()
    $command.Dispose()
    $script:testConnectionString = "Server=(localdb)\$instance;Integrated Security=true;Database=$database;Connect Timeout=15;Pooling=false"
    $connection = [System.Data.SqlClient.SqlConnection]::new($testConnectionString)
    $connection.Open()
    Execute-Sql @'
CREATE TABLE dbo.[Order](ID int IDENTITY PRIMARY KEY, Type int NOT NULL DEFAULT 2,
    Closed bit NOT NULL DEFAULT 0, CustomerID int NOT NULL DEFAULT 0, SalesRepID int NOT NULL DEFAULT 0,
    Total money NOT NULL DEFAULT 112, Tax money NOT NULL DEFAULT 12,
    DefaultTaxChangeReasonCodeID int NOT NULL DEFAULT 0, Comment nvarchar(255) NOT NULL DEFAULT N'',
    Time datetime NOT NULL DEFAULT GETDATE(), LastUpdated datetime NOT NULL DEFAULT GETDATE(), DBTimeStamp rowversion);
CREATE TABLE dbo.Item(ID int PRIMARY KEY, ExtendedDescription ntext NULL,
    ItemLookupCode nvarchar(50) NOT NULL DEFAULT N'TEST-SKU', Description nvarchar(30) NOT NULL DEFAULT N'Fixture item',
    ItemType smallint NOT NULL DEFAULT 0, Quantity float NOT NULL DEFAULT 100,
    QuantityCommitted float NOT NULL DEFAULT 0);
CREATE TABLE dbo.Customer(ID int PRIMARY KEY, Company nvarchar(50) NOT NULL,
    TaxExempt bit NOT NULL DEFAULT 0);
INSERT dbo.Customer(ID,Company) VALUES(0,N'TEST CUSTOMER');
CREATE TABLE dbo.OrderEntry(ID int IDENTITY PRIMARY KEY, OrderID int NOT NULL, ItemID int NOT NULL,
    QuantityOnOrder float NOT NULL, Price money NOT NULL, FullPrice money NOT NULL,
    Taxable int NOT NULL DEFAULT 1, TaxChangeReasonCodeID int NOT NULL DEFAULT 0,
    Description nvarchar(30) NOT NULL DEFAULT N'Fixture item', Comment nvarchar(255) NOT NULL DEFAULT N'', DBTimeStamp rowversion);
CREATE TABLE dbo.Queueing(id bigint IDENTITY PRIMARY KEY, OrderID bigint NULL,
    Status int NULL, GroupTo int NULL, OPIS varchar(max) NULL);
CREATE TABLE dbo.QueueingItems(id bigint IDENTITY PRIMARY KEY, QueueingID bigint NULL,
    ItemID int NULL, QtyPre int NULL, Picker nvarchar(50) NULL, Status varchar(50) NULL, PickLoc varchar(max) NULL);
CREATE TABLE dbo.[Transaction](TransactionNumber int PRIMARY KEY, RecallID int, CashierID int);
CREATE TABLE dbo.SOD_WO_LOGS(id int IDENTITY PRIMARY KEY, OrderId int, ItemID int,
    FullPrice money, UpdatedPrice money, ApprovedUser nvarchar(50), Status nvarchar(50));
INSERT dbo.Item(ID) VALUES(1);
UPDATE dbo.Item SET ExtendedDescription=N'';
INSERT dbo.[Order](Comment) VALUES(N'Test 1'),(N'Test 2');
INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice) VALUES(1,1,1,112,112),(2,1,1,112,112);
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES(1,0,1,'TEST'),(2,0,2,'TEST');
INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
    VALUES(1,1,0,'','Pending','STORE'),(2,1,0,'','Pending','STORE');
'@
    # Complete only the fixture's mapped Order/OrderEntry columns so these checks
    # execute the production LINQ tax workflow with real RMS rowversion behavior.
    [xml]$mapping = [IO.File]::ReadAllText((Join-Path $repo 'Work Order Entry\Linq\ItemLookUp.dbml'))
    foreach ($table in $mapping.Database.Table) {
        if ($table.Name -notin @('dbo.[Order]','dbo.OrderEntry')) { continue }
        $tableName = $table.Name.Split('.')[1].Trim('[',']')
        foreach ($column in $table.Type.Column) {
            $columnName = [string]$column.Name
            if ((Scalar-Sql "SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID('dbo.[$tableName]') AND name='$columnName';") -ne 0) { continue }
            $dbType = [string]$column.DbType
            $default = if ($dbType -match '(?i)char|text') { "N''" }
                       elseif ($dbType -match '(?i)datetime') { "CONVERT(datetime,'20000101',112)" }
                       else { '0' }
            $defaultClause = if ($dbType -match 'NOT NULL') { "DEFAULT ($default) WITH VALUES" } else { '' }
            Execute-Sql "ALTER TABLE dbo.[$tableName] ADD [$columnName] $dbType $defaultClause;"
        }
    }
    Execute-Sql "CREATE TABLE dbo.ReasonCode(ID int PRIMARY KEY, Code nvarchar(100), Type int); INSERT dbo.ReasonCode VALUES(33,N'701',6);"
    # Batch recall reads: verify the production SQL against legacy price selection
    # and refresh behavior using only this fixture's connection.
    Execute-Sql @'
ALTER TABLE dbo.Customer ADD AccountNumber nvarchar(50) NULL;
ALTER TABLE dbo.Item ADD Taxable bit NOT NULL DEFAULT 1 WITH VALUES;
ALTER TABLE dbo.[Transaction] ADD CustomerID int NULL, [Time] datetime NULL, ReferenceNumber nvarchar(50) NULL;
CREATE TABLE dbo.TransactionEntry(TransactionNumber int NOT NULL, ItemID int NOT NULL, Quantity float NOT NULL, Price money NOT NULL);
'@
    Execute-Sql @'
INSERT dbo.Customer(ID,Company,AccountNumber) VALUES(10,N'PRICE CUSTOMER',N'ACCT-10'),(11,N'  PADDED CUSTOMER  ',N'ACCT-11');
INSERT dbo.Item(ID,ItemLookupCode) VALUES(100,N'PREFERRED'),(101,N'ZERO-INVOICE'),(102,N'ANY-PRICE'),(103,N'NO-HISTORY'),(104,N'QUOTE''SKU'),(105,N'TIE-BREAK'),(106,N'OTHER-CUSTOMER'),(107,N'ZERO-LATEST');
INSERT dbo.[Transaction](TransactionNumber,CustomerID,[Time],ReferenceNumber) VALUES
(1000,10,'20260101',N'CH1000'),(1001,10,'20260102',N'OTHER'),(1002,10,'20260103',N'CI1002'),
(1003,10,'20260104',N'OTHER'),(1004,10,'20260104',N'OTHER'),(1005,11,'20260105',N'CH1005');
INSERT dbo.TransactionEntry(TransactionNumber,ItemID,Quantity,Price) VALUES
(1000,100,1,100),(1004,100,1,200),
(1000,101,1,0),(1004,101,1,210),
(1001,102,1,75),(1004,102,-1,900),(1003,102,0,800),
(1002,104,1,123.4567),(1003,105,1,40),(1004,105,1,50),
(1005,106,1,555),(1002,107,1,0);
'@
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $readLog = [IO.StringWriter]::new()
        $context.Log = $readLog
        $priceCodes = [string[]]@('PREFERRED','ZERO-INVOICE','ANY-PRICE','NO-HISTORY',"QUOTE'SKU",'TIE-BREAK','OTHER-CUSTOMER','ZERO-LATEST','MISSING-SKU',' PREFERRED ','PREFERRED')
        $prices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,10,$priceCodes)
        Check ([regex]::Matches($readLog.ToString(), '(?im)^(SELECT |WITH )').Count -eq 1) 'Recall prices use one parameterized read for all requested items'
        Check ($prices['PREFERRED'] -eq 100) 'Recall retains CH/CI invoice priority over a newer ordinary sale'
        Check ($prices['ZERO-INVOICE'] -eq 210 -and $prices['ZERO-LATEST'] -eq 0) 'Zero invoice price retains the legacy latest-sale fallback'
        Check ($prices['ANY-PRICE'] -eq 75) 'Recall ignores returns and zero-quantity transaction lines'
        Check ($prices['TIE-BREAK'] -eq 50) 'Equal sale dates still use the descending transaction number'
        Check ($prices["QUOTE'SKU"] -eq [decimal]123.4567) 'Quoted item codes are parameters and money precision is preserved'
        Check ($prices['OTHER-CUSTOMER'] -eq 0 -and $prices['NO-HISTORY'] -eq 0 -and
            [WorkOrderEntry.RecallReadData]::LastPriceOrZero($prices,'MISSING-SKU') -eq 0) 'Missing history and another customer never supply a last price'
        # Compare every requested code to the exact original two-query selection rule.
        $legacyMatches = $true
        foreach ($priceCode in $priceCodes) {
            $legacyCommand = $connection.CreateCommand()
            try {
                $legacyCommand.CommandText = @'
SELECT TOP (1) te.Price FROM dbo.[Transaction] t
JOIN dbo.TransactionEntry te ON t.TransactionNumber=te.TransactionNumber
JOIN dbo.Item i ON te.ItemID=i.ID
WHERE t.CustomerID=@customer AND i.ItemLookupCode=@code AND te.Quantity>0
AND (t.ReferenceNumber LIKE N'CH%' OR t.ReferenceNumber LIKE N'CI%')
ORDER BY t.Time DESC,t.TransactionNumber DESC;
'@
                $null = $legacyCommand.Parameters.AddWithValue('@customer',10)
                $null = $legacyCommand.Parameters.AddWithValue('@code',$priceCode.Trim())
                $legacyPrice = $legacyCommand.ExecuteScalar()
                if ($null -eq $legacyPrice -or $legacyPrice -eq 0) {
                    $legacyCommand.CommandText = $legacyCommand.CommandText.Replace("AND (t.ReferenceNumber LIKE N'CH%' OR t.ReferenceNumber LIKE N'CI%')",'')
                    $legacyPrice = $legacyCommand.ExecuteScalar()
                }
                if ($null -eq $legacyPrice) { $legacyPrice = 0 }
                if ([WorkOrderEntry.RecallReadData]::LastPriceOrZero($prices,$priceCode) -ne $legacyPrice) { $legacyMatches = $false }
            } finally { $legacyCommand.Dispose() }
        }
        Check $legacyMatches 'Batch last prices match the original two-query rule for every fixture code'
        Check ([WorkOrderEntry.RecallReadData]::ResolveCustomer($context,10,'WRONG') -eq 10 -and
            [WorkOrderEntry.RecallReadData]::ResolveCustomer($context,0,' ACCT-10 ') -eq 10 -and
            [WorkOrderEntry.RecallReadData]::ResolveCustomer($context,0,'PADDED CUSTOMER') -eq 11) 'Recall retains customer ID, account-number, and trimmed-name resolution'
        $readLog.GetStringBuilder().Clear() | Out-Null
        $emptyPrices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,0,$priceCodes)
        Check ($emptyPrices.Count -eq 0 -and $readLog.ToString().Length -eq 0) 'An unresolved customer performs no transaction-history query'
        Execute-Sql 'UPDATE dbo.TransactionEntry SET Price=101 WHERE TransactionNumber=1000 AND ItemID=100;'
        $freshPrices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,10,$priceCodes)
        Check ($prices['PREFERRED'] -eq 100 -and $freshPrices['PREFERRED'] -eq 101) 'A later recall sees a committed price change without changing the earlier recall data'
        Execute-Sql "INSERT dbo.Item(ID,ItemLookupCode) VALUES(108,N'PREFERRED'); INSERT dbo.TransactionEntry(TransactionNumber,ItemID,Quantity,Price) VALUES(1002,108,1,110);"
        $duplicateCodePrices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,10,[string[]]@('PREFERRED'))
        Check ($duplicateCodePrices['PREFERRED'] -eq 110) 'Duplicate catalogue codes retain the legacy latest invoice across matching item IDs'
        # More than one chunk verifies parameter bounds and that requested items are not dropped.
        Execute-Sql @'
;WITH numbers AS (SELECT TOP (201) ROW_NUMBER() OVER (ORDER BY object_id) AS n FROM sys.all_objects)
INSERT dbo.Item(ID,ItemLookupCode) SELECT 10000+n,N'BATCH-'+CONVERT(nvarchar(10),n) FROM numbers;
'@
        $manyCodes = [string[]](1..201 | ForEach-Object { "BATCH-$_" })
        $readLog.GetStringBuilder().Clear() | Out-Null
        $manyPrices = [WorkOrderEntry.RecallReadData]::LoadLastPrices($context,10,$manyCodes)
        Check ($manyPrices.Count -eq 201 -and [regex]::Matches($readLog.ToString(), '(?im)^(SELECT |WITH )').Count -eq 2) 'Large recalls bound SQL parameters while reading every item'
        $readLog.GetStringBuilder().Clear() | Out-Null
        $queueRead = [WorkOrderEntry.RecallReadData]::LoadQueue($context,1)
        Check ([regex]::Matches($readLog.ToString(), '(?im)^SELECT ').Count -eq 1) 'Warehouse details use one read per recalled order'
        $beforeQueueChange = Capture-Order 1
        Execute-Sql "UPDATE dbo.QueueingItems SET QtyPre=3,Status='Shortage',Picker=N'FRESH PICKER' WHERE QueueingID=1;"
        $freshQueue = [WorkOrderEntry.RecallReadData]::LoadQueue($context,1)
        Check ($queueRead[1].Prepared -eq 0 -and $freshQueue[1].Prepared -eq 3 -and
            $freshQueue[1].Status -eq 'Shortage' -and $freshQueue[1].Picker -eq 'FRESH PICKER') 'Repeated queue reads see another session''s latest count, shortage, and picker'
        Check-Conflict $beforeQueueChange 'A warehouse change during batched recall still invalidates the starting snapshot'
        $otherQueue = [WorkOrderEntry.RecallReadData]::LoadQueue($context,2)
        Check ($otherQueue[1].Prepared -eq 0 -and $otherQueue[1].Status -eq 'Pending') 'Batch queue details stay isolated to the recalled order'
        Execute-Sql "UPDATE dbo.QueueingItems SET QtyPre=0,Status='Pending',Picker=N'' WHERE QueueingID=1;"
    } finally { $context.Dispose() }
    $taxForm = [WorkOrderEntry.frmItemLookUp]::new()
    $flags = [Reflection.BindingFlags]'Instance,NonPublic'
    $applyTax = $taxForm.GetType().GetMethod('ApplyTaxChangeReasonCode', $flags)
    $prepareTax = $taxForm.GetType().GetMethod('PrepareNewOrderTaxReasons', $flags)
    $taxFlag = $taxForm.GetType().Assembly.GetType('WorkOrderEntry.ModConnectDB', $true).
        GetField('bTaxExcempt', [Reflection.BindingFlags]'Static,Public')
    $oldTaxExempt = $taxFlag.GetValue($null)
    # Compare production row tax application in legacy and recall/batch modes.
    $applyRowTax = $taxForm.GetType().GetMethod('ApplyCustomerTaxStatusToRows', $flags, $null,
        [type[]]@([WorkOrderEntry.ItemLookUpDataContext],[bool]), $null)
    $loadingField = $taxForm.GetType().GetField('isLoadingRecalledOrder',$flags)
    $loadingField.SetValue($taxForm,$true)
    $taxGrid = $taxForm.GetType().GetProperty('gridSelectItem',([Reflection.BindingFlags]'Instance,Public,NonPublic')).GetValue($taxForm)
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        Execute-Sql "UPDATE dbo.Item SET Taxable=0 WHERE ItemLookupCode IN (N'NO-HISTORY',N'QUOTE''SKU');"
        $taxFlag.SetValue($null,$false)
        $taxReadLog = [IO.StringWriter]::new(); $context.Log = $taxReadLog
        $taxCodes = [string[]]@('PREFERRED','NO-HISTORY',"QUOTE'SKU",'MISSING-SKU',' PREFERRED','PREFERRED ','preferred','','PREFERRED')
        $taxFlags = [WorkOrderEntry.RecallReadData]::LoadTaxability($context,$taxCodes)
        Check ([regex]::Matches($taxReadLog.ToString(), '(?im)^SELECT ').Count -eq 1) 'Recall tax flags use one parameterized read for all selected item codes'
        Check ($taxFlags['PREFERRED'] -and -not $taxFlags['NO-HISTORY'] -and -not $taxFlags["QUOTE'SKU"] -and -not $taxFlags['MISSING-SKU']) 'Tax batching preserves taxable, non-taxable, quoted, and missing-item results'
        Check (-not $taxFlags[' PREFERRED'] -and $taxFlags['PREFERRED '] -and $taxFlags['preferred'] -and -not $taxFlags.ContainsKey('')) 'Tax lookup preserves untrimmed codes, database collation, and blank-code skipping'
        $taxReadLog.GetStringBuilder().Clear() | Out-Null
        $emptyTaxFlags = [WorkOrderEntry.RecallReadData]::LoadTaxability($context,[string[]]@('',$null))
        Check ($emptyTaxFlags.Count -eq 0 -and $taxReadLog.ToString().Length -eq 0) 'An empty tax lookup does not query SQL Server'
        Execute-Sql "UPDATE dbo.Item SET Taxable=0 WHERE ItemLookupCode=N'PREFERRED';"
        $freshTaxFlags = [WorkOrderEntry.RecallReadData]::LoadTaxability($context,[string[]]@('PREFERRED'))
        Check ($taxFlags['PREFERRED'] -and -not $freshTaxFlags['PREFERRED']) 'A later tax lookup sees another session''s committed item-tax change'
        Execute-Sql "UPDATE dbo.Item SET Taxable=1 WHERE ItemLookupCode=N'PREFERRED';"
        $taxReadLog.GetStringBuilder().Clear() | Out-Null
        $manyTaxFlags = [WorkOrderEntry.RecallReadData]::LoadTaxability($context,$manyCodes)
        Check ($manyTaxFlags.Count -eq 201 -and [regex]::Matches($taxReadLog.ToString(), '(?im)^SELECT ').Count -eq 2) 'Large tax lookups bound parameters and retain all 201 item flags'
        $taxGrid.AllowUserToAddRows = $false
        foreach ($code in @('PREFERRED','NO-HISTORY',"QUOTE'SKU",'MISSING-SKU','')) {
            $rowIndex = $taxGrid.Rows.Add()
            $taxGrid.Rows[$rowIndex].Cells[0].Value = $code
            $taxGrid.Rows[$rowIndex].Cells[2].Value = [double]1
            $taxGrid.Rows[$rowIndex].Cells[3].Value = [double]0
            $taxGrid.Rows[$rowIndex].Cells[10].Value = [int]0
        }
        $taxGrid.Rows[0].Cells[2].Value = [double]2
        $taxGrid.Rows[0].Cells[3].Value = [double]112
        $taxReadLog.GetStringBuilder().Clear() | Out-Null
        $null = $applyRowTax.Invoke($taxForm,[object[]]@($context,$false))
        $legacyRowFlags = @($taxGrid.Rows | ForEach-Object { $_.Cells[10].Value })
        $legacyTaxReads = [regex]::Matches($taxReadLog.ToString(), '(?im)^SELECT ').Count
        $taxReadLog.GetStringBuilder().Clear() | Out-Null
        $null = $applyRowTax.Invoke($taxForm,[object[]]@($context,$true))
        $rowFlagsMatch = $true
        for ($rowIndex=0; $rowIndex -lt $taxGrid.Rows.Count; $rowIndex++) {
            $value = $taxGrid.Rows[$rowIndex].Cells[10].Value
            if ($value -ne $legacyRowFlags[$rowIndex] -or $value.GetType() -ne $legacyRowFlags[$rowIndex].GetType()) { $rowFlagsMatch = $false }
        }
        Check ($rowFlagsMatch -and $legacyTaxReads -eq 4 -and [regex]::Matches($taxReadLog.ToString(), '(?im)^SELECT ').Count -eq 1) 'Production recall row flags match legacy values and types while four reads become one'
        $payment = $taxForm.GetType().GetProperty('cboPayment',([Reflection.BindingFlags]'Instance,Public,NonPublic')).GetValue($taxForm)
        $payment.SelectedIndex = -1 # No credit-warning modal in this isolated totals check.
        $totalsWatch = [Diagnostics.Stopwatch]::StartNew()
        $taxForm.UpdateAmt()
        $totalsWatch.Stop()
        $taxableTotalsMilliseconds = $totalsWatch.Elapsed.TotalMilliseconds
        $textTotal = $taxForm.GetType().GetProperty('txtTotal',([Reflection.BindingFlags]'Instance,Public,NonPublic')).GetValue($taxForm)
        $textVat = $taxForm.GetType().GetProperty('txtVat',([Reflection.BindingFlags]'Instance,Public,NonPublic')).GetValue($taxForm)
        $textSub = $taxForm.GetType().GetProperty('txtSub',([Reflection.BindingFlags]'Instance,Public,NonPublic')).GetValue($taxForm)
        Check ([decimal]::Parse($textTotal.Text) -eq 224 -and [decimal]::Parse($textVat.Text) -eq 24 -and [decimal]::Parse($textSub.Text) -eq 200) 'Existing taxable totals remain 224 total, 24 VAT, and 200 subtotal after batch tax application'
        $taxFlag.SetValue($null,$true)
        $taxReadLog.GetStringBuilder().Clear() | Out-Null
        $null = $applyRowTax.Invoke($taxForm,[object[]]@($context,$true))
        Check (@($taxGrid.Rows | Where-Object { $_.Cells[10].Value -ne 0 }).Count -eq 0 -and $taxReadLog.ToString().Length -eq 0) 'Exempt recalled orders clear row tax flags without an item-tax query'
        $totalsWatch.Restart()
        $taxForm.UpdateAmt()
        $totalsWatch.Stop()
        $exemptTotalsMilliseconds = $totalsWatch.Elapsed.TotalMilliseconds
        Check ([decimal]::Parse($textTotal.Text) -eq 224 -and [decimal]::Parse($textVat.Text) -eq 0 -and [decimal]::Parse($textSub.Text) -eq 224) 'Existing exempt totals keep the same line amounts with zero VAT'
        @(
            [pscustomobject]@{Phase='TaxableTotalsFixture';Lines=$taxGrid.Rows.Count;Milliseconds=[math]::Round($taxableTotalsMilliseconds,3)},
            [pscustomobject]@{Phase='ExemptTotalsFixture';Lines=$taxGrid.Rows.Count;Milliseconds=[math]::Round($exemptTotalsMilliseconds,3)}
        ) | Export-Csv -LiteralPath (Join-Path $testDirectory 'TotalsFixtureTiming.csv') -NoTypeInformation
        Write-Output "Unshown fixture totals timing: taxable $([math]::Round($taxableTotalsMilliseconds,3)) ms; exempt $([math]::Round($exemptTotalsMilliseconds,3)) ms (no credit-warning dialog)."
    } finally {
        $context.Dispose()
        $taxGrid.Rows.Clear()
        $loadingField.SetValue($taxForm,$false)
        $taxFlag.SetValue($null,$oldTaxExempt)
    }
    try {
        $taxFlag.SetValue($null, $true)
        $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
        try {
            $context.Connection.Open()
            $transaction = $context.Connection.BeginTransaction()
            $context.Transaction = $transaction
            try {
                $null = $applyTax.Invoke($taxForm, [object[]]@($context, [int]1))
                $null = $context.ExecuteCommand('UPDATE dbo.OrderEntry SET VoucherID=0 WHERE OrderID=1;')
                $conflicted = $false
                try { $context.SubmitChanges() }
                catch {
                    $errorObject = $_.Exception
                    while ($null -ne $errorObject) {
                        if ($errorObject -is [System.Data.Linq.ChangeConflictException]) { $conflicted = $true }
                        $errorObject = $errorObject.InnerException
                    }
                    if (-not $conflicted) { throw }
                }
                Check $conflicted 'Old Set Tax save ordering reproduces the tracked rowversion conflict'
            } finally { $transaction.Rollback(); $transaction.Dispose() }
        } finally { $context.Dispose() }

        $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
        try {
            $context.Connection.Open()
            $transaction = $context.Connection.BeginTransaction()
            $context.Transaction = $transaction
            try {
                $null = $prepareTax.Invoke($taxForm, [object[]]@($context, [int]1, [int]2))
                $context.SubmitChanges()
                Check ((Scalar-Sql 'SELECT DefaultTaxChangeReasonCodeID FROM dbo.[Order] WHERE ID=1;' $transaction) -eq 33 -and
                    (Scalar-Sql 'SELECT TaxChangeReasonCodeID FROM dbo.OrderEntry WHERE OrderID=1;' $transaction) -eq 33) 'Fixed Set Tax save writes the header and line reason without a rowversion conflict'
                Check ((Scalar-Sql 'SELECT VoucherID FROM dbo.OrderEntry WHERE OrderID=1;' $transaction) -eq 0) 'Fixed tax preparation preserves the Work Order voucher rule'
            } finally { $transaction.Rollback(); $transaction.Dispose() }
        } finally { $context.Dispose() }
        Check ((Scalar-Sql 'SELECT DefaultTaxChangeReasonCodeID FROM dbo.[Order] WHERE ID=1;') -eq 0 -and
            (Scalar-Sql 'SELECT TaxChangeReasonCodeID FROM dbo.OrderEntry WHERE OrderID=1;') -eq 0) 'Tax preparation and reason changes roll back together'

        $taxFlag.SetValue($null, $false)
        $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
        try {
            $context.Connection.Open()
            $transaction = $context.Connection.BeginTransaction()
            $context.Transaction = $transaction
            try {
                $null = $context.ExecuteCommand('UPDATE dbo.[Order] SET DefaultTaxChangeReasonCodeID=33 WHERE ID=1; UPDATE dbo.OrderEntry SET TaxChangeReasonCodeID=33 WHERE OrderID=1;')
                $null = $prepareTax.Invoke($taxForm, [object[]]@($context, [int]1, [int]2))
                $context.SubmitChanges()
                Check ((Scalar-Sql 'SELECT DefaultTaxChangeReasonCodeID FROM dbo.[Order] WHERE ID=1;' $transaction) -eq 0 -and
                    (Scalar-Sql 'SELECT TaxChangeReasonCodeID FROM dbo.OrderEntry WHERE OrderID=1;' $transaction) -eq 0) 'Saving after returning to taxable clears manual header and line reasons'
            } finally { $transaction.Rollback(); $transaction.Dispose() }
        } finally { $context.Dispose() }

        $taxFlag.SetValue($null, $true)
        $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
        try {
            $context.Connection.Open()
            $transaction = $context.Connection.BeginTransaction()
            $context.Transaction = $transaction
            try {
                $null = $context.ExecuteCommand('UPDATE dbo.[Order] SET Type=3 WHERE ID=1; UPDATE dbo.OrderEntry SET VoucherID=7 WHERE OrderID=1;')
                $null = $prepareTax.Invoke($taxForm, [object[]]@($context, [int]1, [int]3))
                $context.SubmitChanges()
                Check ((Scalar-Sql 'SELECT VoucherID FROM dbo.OrderEntry WHERE OrderID=1;' $transaction) -eq 7 -and
                    (Scalar-Sql 'SELECT TaxChangeReasonCodeID FROM dbo.OrderEntry WHERE OrderID=1;' $transaction) -eq 33) 'Quotation tax save still skips the Work Order-only voucher update'
            } finally { $transaction.Rollback(); $transaction.Dispose() }
        } finally { $context.Dispose() }
    } finally {
        $taxFlag.SetValue($null, $oldTaxExempt)
        $taxForm.Dispose()
    }
    Execute-Sql "CREATE VIEW dbo.SOD_viewTableLastUpdate AS SELECT CAST(N'Order' AS nvarchar(128)) AS TableName, MAX(LastUpdated) AS LastUpdate FROM dbo.[Order];"
    $viewPath = Join-Path $repo 'Database Components\Deployment\Objects\Common\Views\SOD_ViewForInvoice.sql'
    foreach ($batch in ([IO.File]::ReadAllText($viewPath) -split '(?im)^GO\s*$')) {
        if (-not [string]::IsNullOrWhiteSpace($batch)) { Execute-Sql $batch }
    }
    $migration = [IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Enable_WorkOrder_History.sql'))
    $indexMigration = [IO.File]::ReadAllText((Join-Path $repo 'Database Components\Migrations\Prepare_WorkOrder_Edit_Checks.sql'))
    Execute-Sql $indexMigration
    Execute-Sql $indexMigration
    Check ((Scalar-Sql "SELECT COUNT(*) FROM sys.indexes WHERE name IN ('IX_SOD_WO_Edit_OrderEntry','IX_SOD_WO_Edit_Queueing','IX_SOD_WO_Edit_QueueingItems')") -eq 3) 'Lookup index migration is idempotent'
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $blocked = $false
        try { [WorkOrderEntry.ShortageReductionService]::EnsureHistoryEnabled($context) }
        catch { $blocked = $_.Exception.ToString().Contains('Order history must be enabled') }
        Check $blocked 'Shortage acceptance requires history before recording a customer decision'
    } finally { $context.Dispose() }
    Execute-Sql $migration
    Execute-Sql $migration
    Check ((Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History') -eq 0) 'Migration is idempotent and does not backfill old events'

    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $context.Connection.Open()
        [WorkOrderEntry.OrderHistory]::SetActor($context, 'TEST CASHIER', 'TEST01')
        $null = $context.ExecuteCommand("INSERT dbo.[Order](Comment) VALUES(N'Test 3'),(N'Test 4');")
    } finally { $context.Dispose() }
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Order created' AND Actor='TEST CASHIER' AND Terminal LIKE '%TEST01%'") -eq 2) 'Multi-row order creation records user and register'
    Execute-Sql "INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice) VALUES(3,1,0.5,112,112),(4,1,1.25,112,112);"
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Item added'") -eq 2) 'Multi-row entry inserts are logged without narrowing fractional quantities'
    Execute-Sql "UPDATE dbo.QueueingItems SET Status='Prepared',Picker='TEST PICKER',QtyPre=1;"
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Picking status changed' AND Details LIKE '%TEST PICKER%'") -eq 2) 'Multi-row preparation records picker, quantity, and stage'
    $groups = [WorkOrderEntry.ReadyOrderNotifications]::LoadPreparedGroups($testConnectionString)
    Check ($groups.Count -eq 2) 'Ready notifications use the actual deployment view'
    Execute-Sql "UPDATE dbo.[Order] SET Time=DATEADD(day,-61,GETDATE()) WHERE ID=2;"
    $groups = [WorkOrderEntry.ReadyOrderNotifications]::LoadPreparedGroups($testConnectionString)
    Check ($groups.Count -eq 1 -and $groups[0] -eq 1) 'Older orders are excluded from ready notifications'

    $before = Capture-Order 1
    Execute-Sql 'UPDATE dbo.[Order] SET Total=200 WHERE ID=1;'
    Check-Conflict $before 'Changed order header blocks a stale save'
    $before = Capture-Order 1
    Execute-Sql 'UPDATE dbo.OrderEntry SET QuantityOnOrder=0.5 WHERE OrderID=1;'
    Check-Conflict $before 'Changed entry blocks a stale save without touching the header'
    $before = Capture-Order 1
    Execute-Sql "UPDATE dbo.QueueingItems SET Picker='ANOTHER PICKER' WHERE QueueingID=1;"
    Check-Conflict $before 'Warehouse changes block a stale save without touching the header'
    $before = Capture-Order 1
    Execute-Sql 'UPDATE dbo.Queueing SET GroupTo=2 WHERE OrderID=1;'
    Check-Conflict $before 'Group changes block a stale save'
    Execute-Sql 'UPDATE dbo.Queueing SET GroupTo=1 WHERE OrderID=1;'
    $before = Capture-Order 1
    [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($before, (Capture-Order 1))
    Check $true 'Unchanged snapshots remain saveable'

    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $context.Connection.Open()
        $transaction = $context.Connection.BeginTransaction()
        $context.Transaction = $transaction
        try {
            $locked = [WorkOrderEntry.OrderEditGuard]::Capture($context, 1, $true)
            [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($before, $locked)
            $command = $connection.CreateCommand()
            try {
                $command.CommandTimeout = 1
                $command.CommandText = 'UPDATE dbo.QueueingItems SET QtyPre=0 WHERE QueueingID=1;'
                $blocked = $false
                try { $null = $command.ExecuteNonQuery() }
                catch [System.Data.SqlClient.SqlException] {
                    if ($_.Exception.Number -ne -2) { throw }
                    $blocked = $true
                }
                Check $blocked 'Warehouse writes cannot race the validated save transaction'
                $command.CommandText = 'UPDATE dbo.[Order] SET Total=999 WHERE ID=1;'
                $blocked = $false
                try { $null = $command.ExecuteNonQuery() }
                catch [System.Data.SqlClient.SqlException] {
                    if ($_.Exception.Number -ne -2) { throw }
                    $blocked = $true
                }
                Check $blocked 'A second cashier cannot overwrite an order while the first save holds its validated locks'
            } finally { $command.Dispose() }
        } finally { $transaction.Rollback(); $transaction.Dispose() }
    } finally { $context.Dispose() }

    # Two users recall the same version. The first commits; the second validates
    # under save locks and must be rejected before its attempted update.
    $cashierA = Capture-Order 1
    $cashierB = Capture-Order 1
    $previousTotal = Scalar-Sql 'SELECT Total FROM dbo.[Order] WHERE ID=1;'
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $context.Connection.Open()
        $transaction = $context.Connection.BeginTransaction()
        $context.Transaction = $transaction
        try {
            [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($cashierA,[WorkOrderEntry.OrderEditGuard]::Capture($context,1,$true))
            $null = $context.ExecuteCommand('UPDATE dbo.[Order] SET Total=331 WHERE ID=1;')
            $transaction.Commit()
        } catch { $transaction.Rollback(); throw }
        finally { $transaction.Dispose() }
    } finally { $context.Dispose() }
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $context.Connection.Open()
        $transaction = $context.Connection.BeginTransaction()
        $context.Transaction = $transaction
        try {
            $blocked = $false
            try {
                [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($cashierB,[WorkOrderEntry.OrderEditGuard]::Capture($context,1,$true))
                $null = $context.ExecuteCommand('UPDATE dbo.[Order] SET Total=999 WHERE ID=1;')
            } catch { $blocked = $_.Exception.ToString().Contains('OrderEditConflictException'); if (-not $blocked) { throw } }
        } finally { $transaction.Rollback(); $transaction.Dispose() }
    } finally { $context.Dispose() }
    Check ($blocked -and (Scalar-Sql 'SELECT Total FROM dbo.[Order] WHERE ID=1;') -eq 331) 'After one cashier commits, the second recalled version is rejected before overwriting the order'
    Execute-Sql "UPDATE dbo.[Order] SET Total=$previousTotal WHERE ID=1;"

    $eventCount = Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History;'
    $transaction = $connection.BeginTransaction()
    try {
        Execute-Sql 'UPDATE dbo.[Order] SET Total=300 WHERE ID=1;' $transaction
        Check ((Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History;' $transaction) -gt $eventCount) 'Order changes create history inside the same transaction'
    } finally { $transaction.Rollback(); $transaction.Dispose() }
    Check ((Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History;') -eq $eventCount) 'Rolled-back changes leave no history events'
    Execute-Sql "INSERT dbo.SOD_WO_LOGS(OrderId,ItemID,FullPrice,UpdatedPrice,ApprovedUser,Status) VALUES(1,1,112,100,N'TEST APPROVER',N'Updated');"
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Price approval recorded' AND Details LIKE '%TEST APPROVER%'") -eq 1) 'Price approval records the approver without a password'
    Execute-Sql 'INSERT dbo.[Transaction](TransactionNumber,RecallID,CashierID) VALUES(101,1,5),(102,2,6);'
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='RMS transaction posted'") -eq 2) 'Multi-row RMS settlement records each recalled order and cashier ID'
    $before = Capture-Order 1
    Execute-Sql 'UPDATE dbo.Queueing SET Status=2 WHERE OrderID=1; UPDATE dbo.[Order] SET Closed=1 WHERE ID=1;'
    Check-Conflict $before 'Closed orders cannot be edited or cancelled again'
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE EventName='Order cancelled'") -eq 1) 'Queue cancellation is recorded'
    $before = Capture-Order 3
    Execute-Sql 'DELETE dbo.[Order] WHERE ID=3;'
    Check-Conflict $before 'Removed orders cannot be saved from a stale screen'
    $history = [WorkOrderEntry.OrderHistory]::Load($testConnectionString, 1)
    Check ($history.Rows.Count -gt 0) 'Closed-order history is available to the viewer'

    # Two orders share an item: only the accepted shortage's reservation is released.
    Execute-Sql @'
INSERT dbo.Item(ID,Quantity,QuantityCommitted) VALUES(2,100,13);
INSERT dbo.[Order](Comment,Total,Tax) VALUES(N'Shortage test',1120,120),(N'Other reservation',336,36);
INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice) VALUES(5,2,10,112,112),(6,2,3,112,112);
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS) VALUES(5,0,5,'TEST');
INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Picker,Status,PickLoc) VALUES(3,2,7,'WAREHOUSE PICKER','Shortage','STORE');
'@
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try { $line = [WorkOrderEntry.ShortageReductionService]::Load($context, 5)[0] }
    finally { $context.Dispose() }
    Check ($line.OrderedQuantity -eq 10 -and $line.FoundQuantity -eq 7 -and $line.Picker -eq 'WAREHOUSE PICKER') 'Shortage recall handles RMS smallint ItemType and loads the saved warehouse count'
    $request = [WorkOrderEntry.ShortageReductionRequest]::new($line, 'Customer confirmed seven pieces.')
    $requests = [WorkOrderEntry.ShortageReductionRequest[]]@($request)
    $beforeShortage = Capture-Order 5
    Execute-Sql "UPDATE dbo.QueueingItems SET QtyPre=6 WHERE id=3;"
    $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
    try {
        $context.Connection.Open()
        $transaction = $context.Connection.BeginTransaction()
        $context.Transaction = $transaction
        try {
            $blocked = $false
            try { [WorkOrderEntry.ShortageReductionService]::ValidateBeforeSave($context, 5, $requests) }
            catch { $blocked = $_.Exception.ToString().Contains('OrderEditConflictException') }
            Check $blocked 'Changed physical count blocks shortage acceptance before writing'
        } finally { $transaction.Rollback(); $transaction.Dispose() }
    } finally { $context.Dispose() }
    Execute-Sql "UPDATE dbo.QueueingItems SET QtyPre=7 WHERE id=3;"

    $beforeShortage = Capture-Order 5
    $shortageHistoryCount = Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History;'
    foreach ($shouldCommit in @($false, $true)) {
        $context = [WorkOrderEntry.ItemLookUpDataContext]::new($testConnectionString)
        try {
            $context.Connection.Open()
            [WorkOrderEntry.OrderHistory]::SetActor($context, 'SHORTAGE CASHIER', 'TEST01')
            $transaction = $context.Connection.BeginTransaction()
            $context.Transaction = $transaction
            try {
                [WorkOrderEntry.clsItemLookUp]::AcquireOrderTransactionLock($context, 5)
                $locked = [WorkOrderEntry.OrderEditGuard]::Capture($context, 5, $true)
                [WorkOrderEntry.OrderEditGuard]::EnsureUnchanged($beforeShortage, $locked)
                [WorkOrderEntry.ShortageReductionService]::ValidateBeforeSave($context, 5, $requests)
                $null = $context.ExecuteCommand('UPDATE dbo.[Order] SET Total=784,Tax=84 WHERE ID=5; UPDATE dbo.OrderEntry SET QuantityOnOrder=7 WHERE OrderID=5;')
                $commitmentChange = [WorkOrderEntry.OrderCommitmentRules]::CalculateChange(2, 2, $request.OriginalQuantity, $request.FoundQuantity, 0)
                [WorkOrderEntry.clsItemLookUp]::ApplyQuantityCommittedDifference($context, 2, $commitmentChange)
                $queueItem = $context.QueueingItems | Where-Object { $_.id -eq 3 } | Select-Object -First 1
                [WorkOrderEntry.ShortageReductionService]::ApplyToQueueItem($queueItem, $request)
                $context.SubmitChanges()
                [WorkOrderEntry.ShortageReductionService]::RecordDecision($context, $request)
                if (-not $shouldCommit) { throw 'Injected failure after recording the shortage decision.' }
                $transaction.Commit()
            } catch {
                $transaction.Rollback()
                if ($shouldCommit -or $_.Exception.Message -ne 'Injected failure after recording the shortage decision.') { throw }
            }
            finally { $transaction.Dispose() }
        } finally { $context.Dispose() }
        if (-not $shouldCommit) {
            Check ((Scalar-Sql 'SELECT Total FROM dbo.[Order] WHERE ID=5;') -eq 1120 -and
                (Scalar-Sql 'SELECT Tax FROM dbo.[Order] WHERE ID=5;') -eq 120 -and
                (Scalar-Sql 'SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID=5;') -eq 10 -and
                (Scalar-Sql 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=2;') -eq 13 -and
                (Scalar-Sql 'SELECT Status FROM dbo.QueueingItems WHERE id=3;') -eq 'Shortage' -and
                (Scalar-Sql 'SELECT COUNT(*) FROM dbo.SOD_WO_History;') -eq $shortageHistoryCount) 'Failed shortage save rolls back quantity, total, tax, reservation, queue, and customer history together'
        }
    }
    Check ((Scalar-Sql 'SELECT Total FROM dbo.[Order] WHERE ID=5;') -eq 784 -and
        (Scalar-Sql 'SELECT Tax FROM dbo.[Order] WHERE ID=5;') -eq 84 -and
        (Scalar-Sql 'SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID=5;') -eq 7) 'Committed shortage reduction saves the revised quantity and order amounts'
    Check ((Scalar-Sql 'SELECT QuantityCommitted FROM dbo.Item WHERE ID=2;') -eq 10 -and
        (Scalar-Sql 'SELECT Quantity FROM dbo.Item WHERE ID=2;') -eq 100 -and
        (Scalar-Sql 'SELECT QuantityOnOrder FROM dbo.OrderEntry WHERE OrderID=6;') -eq 3) 'Only three reserved pieces are released; physical RMS stock and the other order are unchanged'
    Check ((Scalar-Sql 'SELECT Status FROM dbo.QueueingItems WHERE id=3;') -eq 'Prepared' -and
        (Scalar-Sql 'SELECT QtyPre FROM dbo.QueueingItems WHERE id=3;') -eq 7 -and
        (Scalar-Sql 'SELECT Picker FROM dbo.QueueingItems WHERE id=3;') -eq 'WAREHOUSE PICKER') 'Accepted shortage becomes Prepared while retaining the warehouse picker'
    Check ((Scalar-Sql "SELECT COUNT(*) FROM dbo.SOD_WO_History WHERE OrderID=5 AND EventName='Shortage accepted' AND Actor='SHORTAGE CASHIER' AND Details LIKE '%10 -> 7%' AND Details LIKE '%Customer confirmed seven pieces.%';") -eq 1) 'Committed acceptance records the cashier, physical count, reduction, and customer note once'

    # Keep a stale tracked entity alive while an external cashier changes the row.
    $queueContext = [OrderProcessingQueueing.DatabaseDataContext]::new($testConnectionString)
    try {
        $cached = $queueContext.QueueingItems | Where-Object { $_.id -eq 3 } | Select-Object -First 1
        Check ([OrderProcessingQueueing.clsQueueing]::FlagItemShortage(3, 2, 6, $testConnectionString)) 'Queue can flag a shortage after cashier acceptance without restarting'
        Execute-Sql "UPDATE dbo.QueueingItems SET Status='Prepared' WHERE id=3;"
        Check ([OrderProcessingQueueing.clsQueueing]::FlagItemShortage(3, 2, 5, $testConnectionString) -and
            (Scalar-Sql 'SELECT Status FROM dbo.QueueingItems WHERE id=3;') -eq 'Shortage') 'Repeated shortage writes use current database state despite cached entities'
        $rows = [OrderProcessingQueueing.clsQueueing]::LoadOrders(5, 'STORE', '', $testConnectionString)
        Check ($rows.Count -eq 1 -and $rows[0].Status -eq 'Shortage' -and $rows[0].QtyPre -eq 5) 'Queue Refresh reads the latest shortage count and status'
        Execute-Sql "UPDATE dbo.QueueingItems SET Status='Prepared',QtyPre=7 WHERE id=3;"
        $rows = [OrderProcessingQueueing.clsQueueing]::LoadOrders(5, 'STORE', '', $testConnectionString)
        Check ($rows[0].Status -eq 'Prepared' -and $rows[0].QtyPre -eq 7) 'Queue Refresh sees the subsequent cashier preparation change'
        $beforeUpdate = [OrderProcessingQueueing.clsQueueing]::checkTableUpdate($testConnectionString)
        Execute-Sql 'UPDATE dbo.[Order] SET LastUpdated=DATEADD(day,1,GETDATE()) WHERE ID=5;'
        Check ($beforeUpdate -ne [OrderProcessingQueueing.clsQueueing]::checkTableUpdate($testConnectionString)) 'Queue automatic refresh detects a later external order update'
        $null = [OrderProcessingQueueing.clsQueueing]::FlagItemShortage(3, 2, 4, $testConnectionString)
        [OrderProcessingQueueing.clsQueueing]::UpdateQtyPrep(3, 2, '7', $testConnectionString)
        Check ((Scalar-Sql 'SELECT Status FROM dbo.QueueingItems WHERE id=3;') -eq 'Processing') 'Normal preparation clears the latest saved shortage without stale cached status'
        $null = [OrderProcessingQueueing.clsQueueing]::FlagItemShortage(3, 2, 4, $testConnectionString)
        [OrderProcessingQueueing.clsQueueing]::UpdateStatus(3, 2, 'Processing', $testConnectionString)
        Check ((Scalar-Sql 'SELECT Status FROM dbo.QueueingItems WHERE id=3;') -eq 'Processing') 'Explicit Clear Short restores the current queue status'
    } finally { $queueContext.Dispose() }

    Add-Type -AssemblyName System.Windows.Forms, System.Drawing
    [System.Windows.Forms.Application]::EnableVisualStyles()
    Execute-Sql "UPDATE dbo.QueueingItems SET Status='Shortage',QtyPre=3 WHERE id=3;"
    $alertLines = [WorkOrderEntry.ShortageNotifications]::Load($testConnectionString)
    Check ($alertLines.Count -eq 1 -and $alertLines[0].OrderID -eq 5 -and
        $alertLines[0].Customer -eq 'TEST CUSTOMER' -and $alertLines[0].OrderedQuantity -eq 7 -and
        $alertLines[0].FoundQuantity -eq 3 -and $alertLines[0].Picker -eq 'WAREHOUSE PICKER') 'Cashier shortage poll loads the order, customer, physical count, and picker'
    $alertMonitor = [WorkOrderEntry.ShortageNotifications]::new()
    Check ($alertMonitor.Observe($alertLines).Count -eq 0) 'Existing shortage establishes a notification baseline'
    Execute-Sql @'
INSERT dbo.[Order](Type,Time,Closed,Comment) VALUES
 (3,GETDATE(),0,N'Excluded quote'),(2,DATEADD(day,-61,GETDATE()),0,N'Excluded old'),
 (2,GETDATE(),1,N'Excluded closed'),(2,GETDATE(),0,N'Excluded queue'),
 (2,GETDATE(),0,N'Zero found'),(2,GETDATE(),0,N'Missing count');
INSERT dbo.OrderEntry(OrderID,ItemID,QuantityOnOrder,Price,FullPrice)
SELECT ID,2,5,112,112 FROM dbo.[Order] WHERE ID BETWEEN 7 AND 12;
INSERT dbo.Queueing(OrderID,Status,GroupTo,OPIS)
SELECT ID,CASE WHEN ID=10 THEN 2 ELSE 0 END,ID,'TEST' FROM dbo.[Order] WHERE ID BETWEEN 7 AND 12;
INSERT dbo.QueueingItems(QueueingID,ItemID,QtyPre,Picker,Status,PickLoc)
SELECT id,2,CASE WHEN OrderID=11 THEN 0 WHEN OrderID=12 THEN NULL ELSE 1 END,'TEST PICKER','Shortage','STORE'
FROM dbo.Queueing WHERE OrderID BETWEEN 7 AND 12;
'@
    $alertLines = [WorkOrderEntry.ShortageNotifications]::Load($testConnectionString)
    Check ($alertLines.Count -eq 3 -and ($alertLines | Where-Object { $_.OrderID -in 7,8,9,10 }).Count -eq 0) 'Shortage poll excludes quotations, old orders, closed orders, and cancelled queues'
    Check (($alertLines | Where-Object { $_.OrderID -eq 11 })[0].FoundQuantity -eq 0 -and
        $null -eq ($alertLines | Where-Object { $_.OrderID -eq 12 })[0].FoundQuantity) 'Zero and missing physical counts remain visible to cashiers'
    $newAlerts = $alertMonitor.Observe($alertLines)
    Check ($newAlerts.Count -eq 2 -and $newAlerts.Contains(11) -and $newAlerts.Contains(12) -and
        $alertMonitor.Observe($alertLines).Count -eq 0) 'New shortage orders notify once without duplicating unchanged alerts'
    Execute-Sql "UPDATE dbo.QueueingItems SET Status='Processing' WHERE id=3 OR QueueingID IN (SELECT id FROM dbo.Queueing WHERE OrderID IN (11,12));"
    $clearedLines = [WorkOrderEntry.ShortageNotifications]::Load($testConnectionString)
    $null = $alertMonitor.Observe($clearedLines)
    Check ($clearedLines.Count -eq 0) 'Cleared shortages disappear from the cashier alert list'
    Execute-Sql "UPDATE dbo.QueueingItems SET Status='Shortage',QtyPre=2 WHERE id=3;"
    $newAlerts = $alertMonitor.Observe([WorkOrderEntry.ShortageNotifications]::Load($testConnectionString))
    Check ($newAlerts.Count -eq 1 -and $newAlerts[0] -eq 5) 'Reflagged shortage alerts again after being cleared'
    $alertWindow = [WorkOrderEntry.frmShortageAlerts]::new($alertLines, [int[]]@(11))
    try {
        $alertWindow.ShowInTaskbar = $false
        $alertWindow.StartPosition = 'Manual'
        $alertWindow.Location = [System.Drawing.Point]::new(-2400, -2400)
        $alertWindow.Show()
        [System.Windows.Forms.Application]::DoEvents()
        $alertGrid = $alertWindow.Controls.Find('ShortagesGrid', $true)[0]
        Check ($alertWindow.SelectedOrderID -eq 11 -and $alertGrid.Rows.Count -eq 3) 'Shortage window selects a newly flagged order ahead of existing shortages'
        $searchControl = $alertWindow.Controls.Find('ShortageSearch', $true)[0]
        $newControl = $alertWindow.Controls.Find('NewShortagesOnly', $true)[0]
        $zeroControl = $alertWindow.Controls.Find('ZeroFoundOnly', $true)[0]
        $pickerControl = $alertWindow.Controls.Find('ShortagePicker', $true)[0]
        $clearControl = $alertWindow.Controls.Find('ClearShortageFilters', $true)[0]
        $openControl = $alertWindow.Controls.Find('OpenShortageOrder', $true)[0]
        $searchControl.Text = '11'
        Check ($alertGrid.Rows.Count -eq 1 -and $alertWindow.SelectedOrderID -eq 11) 'Typing an order number filters the existing shortage list locally'
        $searchControl.Text = "no match [*] O'Neil"
        Check ($alertGrid.Rows.Count -eq 0 -and -not $openControl.Enabled -and $alertWindow.SelectedOrderID -eq 0) 'No-match search disables opening a previously selected hidden order'
        $clearControl.PerformClick()
        Check ($alertGrid.Rows.Count -eq 3 -and $searchControl.Text -eq '' -and
            -not $newControl.Checked -and -not $zeroControl.Checked -and $pickerControl.SelectedIndex -eq 0) 'Clear filters restores the complete list and resets every filter'
        $newControl.Checked = $true
        Check ($alertGrid.Rows.Count -eq 1 -and $alertWindow.SelectedOrderID -eq 11) 'New-only filter displays the newly flagged order'
        $newControl.Checked = $false
        $zeroControl.Checked = $true
        Check ($alertGrid.Rows.Count -eq 1 -and $alertWindow.SelectedOrderID -eq 11) 'Zero-found filter excludes shortages whose physical count is missing'
        $zeroControl.Checked = $false
        $pickerControl.SelectedItem = 'TEST PICKER'
        Check ($alertGrid.Rows.Count -eq 2) 'Picker filter narrows the list to the selected warehouse picker'
        $searchControl.Text = '12'
        Check ($alertGrid.Rows.Count -eq 1 -and $alertWindow.SelectedOrderID -eq 12) 'Combined search and picker filters select the correct order'
        $clearControl.PerformClick()
        Check ($alertGrid.Rows.Count -eq 3 -and $alertWindow.SelectedOrderID -eq 12) 'Clearing filters preserves a selected order that remains visible'
        $bitmap = [System.Drawing.Bitmap]::new($alertWindow.Width, $alertWindow.Height)
        try {
            $alertWindow.DrawToBitmap($bitmap, [System.Drawing.Rectangle]::new(0, 0, $alertWindow.Width, $alertWindow.Height))
            $previewPath = Join-Path $testDirectory 'ShortageAlertsPreview.png'
            $bitmap.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Output "Shortage alerts preview: $previewPath"
        } finally { $bitmap.Dispose() }
        $searchControl.Text = '12'
        $alertWindow.Controls.Find('OpenShortageOrder', $true)[0].PerformClick()
        Check ($alertWindow.DialogResult -eq [System.Windows.Forms.DialogResult]::OK -and
            $alertWindow.SelectedOrderID -eq 12) 'Open selected filtered order returns the correct Work Order number'
    } finally { $alertWindow.Close(); $alertWindow.Dispose() }
    # Do not show the card: its Load event uses the production application's connection.
    $card = [OrderProcessingQueueing.UCGridOrder]::new()
    $cardGrid = $card.Controls.Find('gridOrder', $true)[0]
    try {
        $null = $cardGrid.Rows.Add()
        $cardGrid.Rows[0].Cells[1].Value = 'TEST-SKU'
        $cardGrid.Rows[0].Cells[6].Value = 'Prepared'
        $card.checkStatus()
        Check (-not $cardGrid.Enabled -and $card.BackColor -eq [System.Drawing.Color]::PaleGreen) 'Prepared card uses its normal disabled green stage'
        $cardGrid.Rows[0].Cells[6].Value = 'Shortage'
        $card.checkStatus()
        Check ($cardGrid.Enabled -and $card.BackColor -eq [System.Drawing.Color]::DarkSalmon -and
            $card.lblOrderAge.Text.Contains('SHORTAGE')) 'Refreshing Prepared to Shortage re-enables the card and replaces the green badge'
        $cardGrid.Rows[0].Cells[6].Value = 'Pending'
        $card.checkStatus()
        Check ($card.BackColor -eq [System.Drawing.Color]::OldLace -and -not $card.lblOrderAge.Text.Contains('SHORTAGE')) 'Cleared shortage restores the pending card stage'
        $cardGrid.Rows[0].Cells[6].Value = 'For Invoicing'
        $card.checkStatus()
        Check ($card.BackColor -eq [System.Drawing.Color]::CornflowerBlue -and $cardGrid.Rows.Count -eq 1) 'For Invoicing remains visible after refresh'
    } finally { $card.Dispose() }
    $viewer = [WorkOrderEntry.frmOrderHistory]::new(1, $testConnectionString)
    try {
        $viewer.ShowInTaskbar = $false
        $viewer.StartPosition = 'Manual'
        $viewer.Location = [System.Drawing.Point]::new(-2400, -2400)
        $viewer.Show()
        $grid = $viewer.Controls | Where-Object { $_ -is [System.Windows.Forms.DataGridView] } | Select-Object -First 1
        $deadline = [DateTime]::UtcNow.AddSeconds(10)
        while ($null -eq $grid.DataSource -and [DateTime]::UtcNow -lt $deadline) {
            [System.Windows.Forms.Application]::DoEvents()
            Start-Sleep -Milliseconds 25
        }
        Check ($null -ne $grid.DataSource -and $grid.Rows.Count -eq $history.Rows.Count) 'History window loads the fixture asynchronously'
        $bitmap = [System.Drawing.Bitmap]::new($viewer.Width, $viewer.Height)
        try {
            $viewer.DrawToBitmap($bitmap, [System.Drawing.Rectangle]::new(0, 0, $viewer.Width, $viewer.Height))
            $previewPath = Join-Path $testDirectory 'OrderHistoryPreview.png'
            $bitmap.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)
            Write-Output "History preview: $previewPath"
        } finally { $bitmap.Dispose() }
    } finally { $viewer.Close(); $viewer.Dispose() }
    $confirm = [WorkOrderEntry.frmShortageReductionConfirm]::new('TEST-SKU', $line, 112)
    try {
        $confirm.ShowInTaskbar = $false
        $confirm.StartPosition = 'Manual'
        $confirm.Location = [System.Drawing.Point]::new(-2400, -2400)
        $confirm.Show()
        [System.Windows.Forms.Application]::DoEvents()
        $bitmap = [System.Drawing.Bitmap]::new($confirm.Width, $confirm.Height)
        try {
            $confirm.DrawToBitmap($bitmap, [System.Drawing.Rectangle]::new(0, 0, $confirm.Width, $confirm.Height))
            $previewPath = Join-Path $testDirectory 'ShortageConfirmationPreview.png'
            $bitmap.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)
            Check (-not $confirm.Controls.Find('ApplyReduction', $true)[0].Enabled) 'Shortage confirmation starts with customer agreement unchecked'
            Write-Output "Shortage preview: $previewPath"
        } finally { $bitmap.Dispose() }
    } finally { $confirm.Close(); $confirm.Dispose() }
    Write-Output "All $checks isolated SQL smoke checks passed."
} finally {
    if ($null -ne $connection) { $connection.Dispose() }
    if ($null -ne $master) {
        try {
            # Only the GUID-named fixture database created above can be dropped.
            if ($master.State -eq 'Open' -and $database -match '^SOD_WO_Tools_Test_[a-f0-9]{8}$') {
                $command = $master.CreateCommand()
                $command.CommandText = "IF DB_ID(N'$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END;"
                $null = $command.ExecuteNonQuery()
                $command.Dispose()
            }
        } finally { $master.Dispose() }
    }
    if ($created) {
        & $localdb stop $instance -k
        & $localdb delete $instance
    }
}
