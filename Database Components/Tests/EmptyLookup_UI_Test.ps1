param([switch]$ExpectLegacyFailure)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$dir = Join-Path $repo ('TestResults\EmptyLookup-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$null = New-Item -ItemType Directory -Path $dir
$app = Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash = (Get-FileHash -LiteralPath ($app + '.config')).Hash
Copy-Item -LiteralPath $app -Destination $dir
$exe = Join-Path $dir 'EmptyLookupProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 /r:System.dll /r:System.Data.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll ("/r:" + (Join-Path $dir 'Work Order Entry_V8.exe')) ("/out:" + $exe) (Join-Path $PSScriptRoot 'EmptyLookup_UI_Probe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Probe compilation failed' }
$mode = if ($ExpectLegacyFailure) { 'Legacy' } else { 'Fixed' }
$process = Start-Process -FilePath $exe -ArgumentList $mode -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $dir 'result.txt') -RedirectStandardError (Join-Path $dir 'error.txt')
if (-not $process.WaitForExit(15000)) { $process.Kill(); throw 'Empty lookup probe timed out' }
Get-Content -LiteralPath (Join-Path $dir 'result.txt')
if ((Get-FileHash -LiteralPath ($app + '.config')).Hash -ne $configHash) { throw 'Store configuration changed' }
if ($process.ExitCode -ne 0) { throw "Empty lookup probe failed: $dir" }
"Artifacts: $dir"
