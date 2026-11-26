[CmdletBinding()]
param([string]$ApplicationPath)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if (-not $ApplicationPath) {
    $ApplicationPath = Join-Path $repo 'Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe'
}
$ApplicationPath = [IO.Path]::GetFullPath($ApplicationPath)
if (-not (Test-Path -LiteralPath $ApplicationPath)) { throw 'Build the V4 candidate first or specify -ApplicationPath.' }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$probe = Join-Path ([IO.Path]::GetDirectoryName($ApplicationPath)) 'V4SaveGuardProbe.exe'
& $compiler /nologo /target:exe /platform:x86 ('/out:' + $probe) ('/reference:' + $ApplicationPath) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll (Join-Path $PSScriptRoot 'V4SaveGuardProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'V4 guard probe did not compile.' }
& $probe
if ($LASTEXITCODE -ne 0) { throw 'V4 guard checks failed.' }
