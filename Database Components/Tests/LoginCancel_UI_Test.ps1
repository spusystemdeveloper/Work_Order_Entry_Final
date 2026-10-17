param([switch]$ExpectLegacyFailure)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$dir=Join-Path $repo ('TestResults\LoginCancel-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $dir
# Copy only the assembly: no real application config or registry setup.
Copy-Item -LiteralPath (Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe') -Destination $dir
$exe=Join-Path $dir 'LoginCancelProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 /r:System.dll /r:System.Core.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll ("/r:"+(Join-Path $dir 'Work Order Entry_V8.exe')) ("/out:"+$exe) (Join-Path $PSScriptRoot 'LoginCancel_UI_Probe.cs')
if($LASTEXITCODE -ne 0){throw 'Probe compilation failed.'}
foreach($mode in @('Cancel','WindowClose','Success')){
    $p=Start-Process -FilePath $exe -ArgumentList $mode -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $dir ($mode+'.out')) -RedirectStandardError (Join-Path $dir ($mode+'.err'))
    if(-not $p.WaitForExit(15000)){$p.Kill();throw "Probe timed out: $mode"}
    $output=Get-Content (Join-Path $dir ($mode+'.out')) -Raw
    Write-Output $output.Trim()
    if($ExpectLegacyFailure){if($mode -eq 'Cancel' -and $output -notmatch 'Collection was modified'){throw 'Legacy collection error was not reproduced.'};break}
    if($p.ExitCode -ne 0){throw "$mode regression failed; artifacts: $dir"}
}
Write-Output "Artifacts: $dir"
