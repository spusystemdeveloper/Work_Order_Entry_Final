# EXE-only application update: installed ReportViewer 2010, no private DLLs/config.
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$directory=Join-Path $repo ('TestResults\Packaging-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
$null=New-Item -ItemType Directory -Path $directory
Copy-Item -LiteralPath (Join-Path $repo 'Work Order Entry\bin\Release\Work Order Entry_V8.exe') -Destination $directory
$reportFiles=@(Get-ChildItem -LiteralPath (Join-Path $repo 'Work Order Entry\bin\Release') -File -Filter 'Microsoft.ReportViewer.*.dll')
if($reportFiles.Count -ne 0){throw 'The Release output contains stale/private ReportViewer DLLs. This build uses installed ReportViewer 2010.'}
$packages=$env:NUGET_PACKAGES
if(-not $packages){$packages=Join-Path $env:USERPROFILE '.nuget\packages'}
$winforms=Join-Path $env:WINDIR 'assembly\GAC_MSIL\Microsoft.ReportViewer.WinForms\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.WinForms.dll'
$common=Join-Path $env:WINDIR 'assembly\GAC_MSIL\Microsoft.ReportViewer.Common\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.Common.dll'
foreach($file in @($winforms,$common)){
    if(-not (Test-Path -LiteralPath $file)){throw 'Install the Microsoft ReportViewer 2010 Redistributable before building/testing this EXE-only package.'}
    if([Reflection.AssemblyName]::GetAssemblyName($file).Version -ne [version]'10.0.0.0'){throw 'Incorrect ReportViewer runtime identity.'}
}
$references=@(
    '/r:System.Data.dll','/r:System.Core.dll','/r:System.Xml.Linq.dll',
    '/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.IO.Compression.dll',
    ('/r:'+(Join-Path $directory 'Work Order Entry_V8.exe')),
    ('/r:'+$winforms),
    ('/r:'+$common),
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
    if(@(Get-ChildItem -LiteralPath $directory -File -Filter '*.dll').Count -ne 0){throw 'The EXE-only test folder unexpectedly contains dependency DLLs.'}
    if(@(Get-ChildItem -LiteralPath $directory -File -Filter '*.config').Count -ne 0){throw 'The probe unexpectedly relies on an application config.'}
    "PASS: EXE-only update with installed ReportViewer 2010; no private DLLs or app config; artifacts: $directory"
} finally {Pop-Location}
