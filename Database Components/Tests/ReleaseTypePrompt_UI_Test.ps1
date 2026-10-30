$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$out=Join-Path $repo ('TestResults\ReleaseTypePrompt-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $out
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$probe=Join-Path $out 'ReleaseTypePromptProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /platform:x86 /target:exe /r:System.Core.dll /r:System.Configuration.dll /r:System.Windows.Forms.dll "/out:$probe" (Join-Path $PSScriptRoot 'ReleaseTypePromptProbe.cs')
if($LASTEXITCODE -ne 0){throw 'Release type probe compilation failed'}
try {
    foreach($queued in @($false,$true)) {
        # This is the test host's own config. Never edit the application's config.
        [IO.File]::WriteAllText($probe+'.config',('<configuration><appSettings><add key="QueueingEnabled" value="'+$queued.ToString()+'" /></appSettings></configuration>'))
        & $probe $app $queued.ToString()
        if($LASTEXITCODE -ne 0){throw "Release type prompt checks failed: QueueingEnabled=$queued"}
    }
    "PASS: release type prompts respect both queue modes; artifacts: $out"
} finally {
    if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Application configuration changed'}
}
