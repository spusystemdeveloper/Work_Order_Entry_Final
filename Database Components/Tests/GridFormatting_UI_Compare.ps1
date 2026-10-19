param([Parameter(Mandatory=$true)][string]$BaselineAssembly)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$directory=Join-Path $repo ('TestResults\GridFormatting-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $directory
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$configHash=(Get-FileHash -LiteralPath ($app+'.config')).Hash
$probe=Join-Path $directory 'GridFormattingProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll ("/out:"+$probe) (Join-Path $PSScriptRoot 'GridFormatting_Probe.cs')
if($LASTEXITCODE -ne 0){throw 'Grid comparison probe compilation failed'}
try {
    foreach($variant in @('Before','After')) {
        $folder=Join-Path $directory $variant; $null=New-Item -ItemType Directory -Path $folder
        $assembly=if($variant -eq 'Before'){[IO.Path]::GetFullPath($BaselineAssembly)}else{$app}
        $process=Start-Process -FilePath $probe -ArgumentList @(('"'+$assembly+'"'),('"'+$folder+'"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $folder 'probe.out') -RedirectStandardError (Join-Path $folder 'probe.err')
        if(!$process.WaitForExit(45000)){$process.Kill();throw "Grid probe timed out: $variant"}
        if($process.ExitCode -ne 0){throw "Grid probe failed: $folder"}
    }
    $before=@(Get-Content (Join-Path $directory 'Before\Results.json') -Raw|ConvertFrom-Json)
    $after=@(Get-Content (Join-Path $directory 'After\Results.json') -Raw|ConvertFrom-Json)
    if($before.Count -ne 16 -or $after.Count -ne 16){throw 'Incomplete grid matrix'}
    $checks=0
    foreach($i in 0..15) {
        foreach($property in @('Key','Data','Rows','Columns','Selected','SelectedColumn','HeaderHeight','TemplateHeight','Font','SelectionMode','ReadOnly','ShowCellToolTips','Pixels')) {
            if(($before[$i].$property|ConvertTo-Json -Depth 8 -Compress) -cne ($after[$i].$property|ConvertTo-Json -Depth 8 -Compress)){throw "Grid mismatch: $($before[$i].Key) / $property"}
            $checks++
        }
    }
    $summary=foreach($i in 0..15){[pscustomobject]@{
        Case=$before[$i].Key;BeforeSharedRows=$before[$i].SharedRows;AfterSharedRows=$after[$i].SharedRows
        BeforeBindMs=$before[$i].BindMedianMs;AfterBindMs=$after[$i].BindMedianMs
        BeforeRepeatMs=$before[$i].RepeatMedianMs;AfterRepeatMs=$after[$i].RepeatMedianMs
    }}
    $summary|ConvertTo-Json|Set-Content (Join-Path $directory 'Summary.json')
    $summary|Where-Object {$_.Case -like '*|1|500'}|Format-Table -AutoSize
    "PASS: $checks paired assertions across 16 grid cases; matching rendered pixels, data and selection. Artifacts: $directory"
}finally{if((Get-FileHash -LiteralPath ($app+'.config')).Hash -ne $configHash){throw 'Store configuration changed'}}
