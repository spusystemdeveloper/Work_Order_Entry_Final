param([Parameter(Mandatory=$true)][string]$BaselineAssembly)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$out=Join-Path $repo ('TestResults\QuantityValidation-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $out
$probe=Join-Path $out 'QuantityValidationProbe.exe'
$app=Join-Path $repo 'Work Order Entry\bin\Debug\Work Order Entry_V8.exe'
$config=$app+'.config';$configHash=(Get-FileHash -LiteralPath $config).Hash
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 /r:System.Core.dll /r:System.Data.dll /r:System.Data.Linq.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll "/out:$probe" (Join-Path $PSScriptRoot 'QuantityValidationProbe.cs')
if($LASTEXITCODE -ne 0){throw 'Quantity validation probe compilation failed'}
try {
    foreach($variant in @('Before','After')) {
        $assembly=if($variant -eq 'Before'){[IO.Path]::GetFullPath($BaselineAssembly)}else{$app}
        & $probe $assembly (Join-Path $out ($variant+'.json'))
        if($LASTEXITCODE -ne 0){throw "Quantity validation probe failed: $variant"}
    }
    $before=Get-Content (Join-Path $out 'Before.json') -Raw|ConvertFrom-Json
    $after=Get-Content (Join-Path $out 'After.json') -Raw|ConvertFrom-Json
    if($before.Count -ne 24 -or $after.Count -ne 24){throw 'Incomplete validation case matrix'}
    $checks=0
    foreach($i in 0..23) {
        foreach($property in @('Name','Result','Errors','ErrorCount','Prompts','Subtotal','Vat','Total','CatalogRows','Rows')) {
            if(($before[$i].$property|ConvertTo-Json -Depth 8 -Compress) -cne ($after[$i].$property|ConvertTo-Json -Depth 8 -Compress)){throw "Validation mismatch: $($before[$i].Name) / $property"}
            $checks++
        }
    }
    "PASS: $checks paired assertions across 24 quantity-validation cases; identical prompts, counters, styling, row values and totals. Artifacts: $out"
    & $probe $app (Join-Path $out 'Formatted.json') --formatted
    if($LASTEXITCODE -ne 0){throw 'Formatted-quantity and actual conversion-save regression checks failed'}
} finally {
    if((Get-FileHash -LiteralPath $config).Hash -ne $configHash){throw 'Store EXE configuration changed'}
}
