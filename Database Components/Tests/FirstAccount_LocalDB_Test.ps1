$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$out=Join-Path $repo ('TestResults/FirstAccount-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $out
$app=Join-Path $repo 'Work Order Entry/bin/Debug/Work Order Entry_V8.exe'
$hash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$probe=Join-Path $out 'FirstAccountProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 /r:System.Core.dll /r:System.Data.dll /r:System.Data.Linq.dll /r:System.Windows.Forms.dll "/out:$probe" (Join-Path $PSScriptRoot 'FirstAccountProbe.cs')
if($LASTEXITCODE -ne 0){throw 'First-account probe compile failed'}
try {
 & $probe $app | Tee-Object -FilePath (Join-Path $out 'result.txt')
 if($LASTEXITCODE -ne 0){throw 'First-account checks failed'}
}finally{if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $hash){throw 'Application config changed'}}
Write-Output "Artifacts: $out"
