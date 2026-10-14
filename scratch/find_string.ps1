$files = Get-ChildItem -Path 'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations' -Include *.exe,*.dll -Recurse
foreach ($f in $files) {
    try {
        $content = [System.IO.File]::ReadAllBytes($f.FullName)
        $strAscii = [System.Text.Encoding]::ASCII.GetString($content)
        $strUni = [System.Text.Encoding]::Unicode.GetString($content)
        if ($strAscii -match 'Overtender' -or $strUni -match 'Overtender') {
            Write-Host "Found in: $($f.FullName)"
        }
    } catch {}
}
