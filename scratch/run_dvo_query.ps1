$ErrorActionPreference = 'Stop'
$sqlPath = 'C:\Users\ADMIN\source\repos\Queries\Query_DVO_Local.sql'
$outDir  = 'C:\Users\ADMIN\source\repos\Queries\DVO_Output'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$base = Get-Content $sqlPath -Raw
$cs = 'Server=localhost;Database=DVO_STORE_DB;User ID=sa;Password=admin123;'

foreach ($brand in 'AMBER', 'ENLIVO') {
    $sql = $base -replace "N'%AMBER%';", "N'%$brand%';"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $cmd = $conn.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 300
    $dt = New-Object System.Data.DataTable
    (New-Object System.Data.SqlClient.SqlDataAdapter $cmd).Fill($dt) | Out-Null
    $conn.Close()
    $file = Join-Path $outDir "DVO_$brand.csv"
    $dt | Select-Object Customer, ItemLookupCode, Description, Price, PriceA, PriceB, PriceC, 'Sold Price', TotalQtySold, TotalSales, SalesRep |
        Export-Csv -Path $file -NoTypeInformation -Encoding UTF8
    $qty = ($dt | Measure-Object TotalQtySold -Sum).Sum
    $sales = ($dt | Measure-Object TotalSales -Sum).Sum
    $cust = ($dt | Select-Object -ExpandProperty Customer -Unique).Count
    $items = ($dt | Select-Object -ExpandProperty ItemLookupCode -Unique).Count
    $nullRep = ($dt | Where-Object { $_.SalesRep -is [DBNull] }).Count
    '{0,-7} rows={1,5}  customers={2,4}  items={3,4}  qty={4,10:N0}  sales={5,15:N2}  nullRep={6}' -f $brand, $dt.Rows.Count, $cust, $items, $qty, $sales, $nullRep
    "  top 5 by sales:"
    $dt | Sort-Object TotalSales -Descending | Select-Object -First 5 | ForEach-Object {
        '    {0,-40} {1,-10} qty={2,7:N0} sales={3,12:N2}' -f ($_.Customer.ToString().Substring(0, [Math]::Min(40, $_.Customer.ToString().Length))), $_.ItemLookupCode, $_.TotalQtySold, $_.TotalSales
    }
}
