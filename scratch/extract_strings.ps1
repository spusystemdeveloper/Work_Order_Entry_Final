Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class ResReader {
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeLibrary(IntPtr hModule);

    public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
}
"@

$files = Get-ChildItem -Path 'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations' -Include *.exe,*.dll -Recurse
foreach ($f in $files) {
    $h = [ResReader]::LoadLibraryEx($f.FullName, [IntPtr]::Zero, [ResReader]::LOAD_LIBRARY_AS_DATAFILE)
    if ($h -ne [IntPtr]::Zero) {
        $sb = New-Object System.Text.StringBuilder 1024
        for ($i = 1; $i -lt 65535; $i++) {
            $len = [ResReader]::LoadString($h, $i, $sb, 1024)
            if ($len -gt 0) {
                $str = $sb.ToString()
                if ($str -match 'Overtender') {
                    Write-Host "$($f.FullName) - StringID $i : $str"
                }
            }
        }
        [ResReader]::FreeLibrary($h) | Out-Null
    }
}
