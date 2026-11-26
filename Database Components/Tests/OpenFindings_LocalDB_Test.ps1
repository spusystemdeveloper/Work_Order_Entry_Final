# Findings 31-41 (November 18). Owned disposable LocalDB database only; the
# store EXE config is hashed before and after and must not change.
param([string]$ApplicationPath = "$PSScriptRoot\..\..\Work Order Entry\bin\Release\Work Order Entry_V8.exe")
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$output = Join-Path $repo ('TestResults\OpenFindings-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))
$null = New-Item -ItemType Directory -Force -Path $output
$probe = Join-Path $output 'OpenFindingsProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$probe" /r:System.Core.dll /r:System.Data.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'OpenFindingsProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Open findings probe compile failed' }
$config = $ApplicationPath + '.config'
$hash = (Get-FileHash -LiteralPath $config).Hash
try {
    & $probe ([IO.Path]::GetFullPath($ApplicationPath)) $repo 2>&1 | Tee-Object -FilePath (Join-Path $output 'result.log')
    if ($LASTEXITCODE -ne 0) { throw 'Open findings fixture failed' }
} finally {
    if ((Get-FileHash -LiteralPath $config).Hash -ne $hash) { throw 'Store config changed' }
}
