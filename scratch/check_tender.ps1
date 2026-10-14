$connStr = "Server=localhost;Database=GSC_STORE_DB;User ID=sa;Password=admin123;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)

try {
    $conn.Open()
    Write-Output "=== SEARCHING ALL STORED PROCEDURES & TRIGGERS ==="

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    o.name AS ObjectName,
    o.type_desc AS ObjectType,
    m.definition AS ObjectDefinition
FROM sys.sql_modules m
JOIN sys.objects o ON m.object_id = o.object_id
WHERE m.definition LIKE '%tender%'
   OR m.definition LIKE '%over%'
ORDER BY o.name
"@
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        $name = $reader["ObjectName"]
        $type = $reader["ObjectType"]
        $def = $reader["ObjectDefinition"].ToString()
        if ($def -like "*overtender*" -or $def -like "*over tender*" -or $def -like "*over-tender*") {
            Write-Output ">>> MATCH ON OVERTENDER: $name ($type)"
        }
    }
    $reader.Close()
    Write-Output "Done searching."

} catch {
    Write-Output "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq [System.Data.ConnectionState]::Open) {
        $conn.Close()
    }
}
