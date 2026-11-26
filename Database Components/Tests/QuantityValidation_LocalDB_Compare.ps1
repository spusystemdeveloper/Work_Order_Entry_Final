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
    # October 30 approved change: only these error counters differ, from 0 to 1, so an
    # unfinished check stops the save. A pre-change baseline must still show 0.
    $stopsSave=@('UnconfirmedPriceEarlyReturn','WorkOrderCaughtException','QuotationCaughtException')
    # October 30 approved wording: translate each baseline prompt to its exact new text.
    # Any other wording change still fails. The stock prompt adds item detail after its heading.
    function Expected-Prompt([string]$prompt){
        $split=$prompt.IndexOf('|');$title=$prompt.Substring(0,$split);$text=$prompt.Substring($split+1)
        if(@('Message!','Message !','Message','Warning','Validation Warning') -contains $title){$title='Work Order Entry'}
        if($text.StartsWith('FROM ')){$text="Something went wrong. Please try again or contact support.`r`n`r`n"+$text}
        elseif($text -eq 'An invalid quantity was detected.'){$text='Quantity must be greater than zero.'}
        elseif($text.StartsWith('Please Check the Price for the Item : ')){$text=$text.Replace('Please Check the Price for the Item : ','Please set a price for this item:')}
        elseif($text.StartsWith('Please Check The Latest Available Quantity.')){$text='Not enough stock for:*'}
        else{
            $text=$text -replace "^The string '(.*)' is not a valid quantity\.$",'Quantity "$1" is not a number. Please enter a number.'
            $text=$text -replace "^The number '(.*)' is not a valid amount\.$",'Price "$1" is not valid. Please enter a number.'
        }
        return $title+'|'+$text
    }
    function Prompts-Match($old,$new){
        $oldList=@($old);$newList=@($new)
        if($oldList.Count -ne $newList.Count){return $false}
        for($p=0;$p -lt $oldList.Count;$p++){
            $wanted=Expected-Prompt $oldList[$p]
            if($wanted.EndsWith('*')){if(-not $newList[$p].StartsWith($wanted.TrimEnd('*'),[StringComparison]::Ordinal)){return $false}}
            elseif($wanted -cne $newList[$p]){return $false}
        }
        return $true
    }
    $checks=0
    foreach($i in 0..23) {
        foreach($property in @('Name','Result','Errors','ErrorCount','Prompts','Subtotal','Vat','Total','CatalogRows','Rows')) {
            if($property -eq 'Errors' -and $stopsSave -contains $before[$i].Name){
                if($before[$i].Errors -ne 0 -or $after[$i].Errors -ne 1){throw "Unexpected error counter: $($before[$i].Name)"}
                $checks++;continue
            }
            if($property -eq 'Prompts'){
                if(-not (Prompts-Match $before[$i].Prompts $after[$i].Prompts)){throw "Prompt mismatch: $($before[$i].Name): $($after[$i].Prompts -join ' / ')"}
                $checks++;continue
            }
            if(($before[$i].$property|ConvertTo-Json -Depth 8 -Compress) -cne ($after[$i].$property|ConvertTo-Json -Depth 8 -Compress)){throw "Validation mismatch: $($before[$i].Name) / $property"}
            $checks++
        }
    }
    "PASS: $checks paired assertions across 24 quantity-validation cases; styling, row values and totals are identical, prompts match the approved wording map, and three error counters changed as approved. Artifacts: $out"
    & $probe $app (Join-Path $out 'Formatted.json') --formatted
    if($LASTEXITCODE -ne 0){throw 'Formatted-quantity and actual conversion-save regression checks failed'}
} finally {
    if((Get-FileHash -LiteralPath $config).Hash -ne $configHash){throw 'Store EXE configuration changed'}
}
