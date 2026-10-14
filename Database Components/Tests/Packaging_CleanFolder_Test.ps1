# Test the Release EXE with only its packaged ReportViewer DLLs and no app config.
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$directory=Join-Path $repo ('TestResults\Packaging-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $directory
Copy-Item -LiteralPath (Join-Path $repo 'Work Order Entry\bin\Release\Work Order Entry_V8.exe') -Destination $directory
$reportFiles=@(Get-ChildItem -LiteralPath (Join-Path $repo 'Work Order Entry\bin\Release') -File -Filter 'Microsoft.ReportViewer.*.dll')
if($reportFiles.Count -ne 5){throw 'The Release package must include its five ReportViewer DLLs.'}
$reportFiles | Copy-Item -Destination $directory
$packages=$env:NUGET_PACKAGES
if(-not $packages){$packages=Join-Path $env:USERPROFILE '.nuget\packages'}
$report=Join-Path $packages 'microsoft.reportingservices.reportviewercontrol.winforms\150.1652.0\lib\net40'
$references=@(
    '/r:System.Data.dll','/r:System.Core.dll','/r:System.Xml.Linq.dll',
    '/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.IO.Compression.dll',
    ('/r:'+(Join-Path $directory 'Work Order Entry_V8.exe')),
    ('/r:'+(Join-Path $report 'Microsoft.ReportViewer.WinForms.dll')),
    ('/r:'+(Join-Path $report 'Microsoft.ReportViewer.Common.dll')),
    ('/r:'+(Join-Path $packages 'exceldatareader\3.7.0\lib\net462\ExcelDataReader.dll')),
    ('/r:'+(Join-Path $packages 'exceldatareader.dataset\3.7.0\lib\net462\ExcelDataReader.DataSet.dll'))
)
$probe=Join-Path $directory 'PackagingProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 @references "/out:$probe" (Join-Path $PSScriptRoot 'PackagingProbe.cs')
if($LASTEXITCODE -ne 0){throw 'Packaging probe compilation failed.'}
Push-Location $directory
try {
    & $probe
    if($LASTEXITCODE -ne 0){throw 'Clean-folder dependency/render checks failed.'}
    if(@(Get-ChildItem -LiteralPath $directory -File -Filter '*.dll').Count -ne 5){throw 'The test folder unexpectedly contains extra dependency DLLs.'}
    "PASS: clean application folder with only five packaged ReportViewer DLLs; artifacts: $directory"
} finally {Pop-Location}
