param([string]$ApplicationPath = "$PSScriptRoot\..\..\Work Order Entry\bin\Release\Work Order Entry_V8.exe")
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$output = Join-Path $repo 'TestResults\CreditWarning-20261026'
$null = New-Item -ItemType Directory -Force -Path $output
$probe = Join-Path $output 'CreditWarningProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$probe" /r:System.Core.dll /r:System.Data.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'CreditWarningProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Credit warning probe compile failed' }
$config = $ApplicationPath + '.config'
$hash = (Get-FileHash -LiteralPath $config).Hash
try {
    & $probe ([IO.Path]::GetFullPath($ApplicationPath))
    if ($LASTEXITCODE -ne 0) { throw 'Credit warning fixture failed' }
} finally {
    if ((Get-FileHash -LiteralPath $config).Hash -ne $hash) { throw 'Store config changed' }
}
