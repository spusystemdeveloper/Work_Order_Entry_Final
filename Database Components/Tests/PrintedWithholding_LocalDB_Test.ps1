param([Parameter(Mandatory=$true)][string]$ApplicationPath,
      [Parameter(Mandatory=$true)][string]$BaselineReport,
      [Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$ApplicationPath=[IO.Path]::GetFullPath($ApplicationPath)
$BaselineReport=[IO.Path]::GetFullPath($BaselineReport)
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
$null=New-Item -ItemType Directory -Force $OutputDirectory
$runtime=Join-Path $env:WINDIR 'assembly\GAC_MSIL'
$winforms=Join-Path $runtime 'Microsoft.ReportViewer.WinForms\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.WinForms.dll'
$common=Join-Path $runtime 'Microsoft.ReportViewer.Common\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.Common.dll'
if(-not(Test-Path $winforms) -or -not(Test-Path $common)){throw 'Install Microsoft ReportViewer 2010 Redistributable.'}
$probe=Join-Path $OutputDirectory 'PrintedWithholdingProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$probe" /r:System.Data.dll /r:System.Xml.Linq.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "/r:$winforms" "/r:$common" (Join-Path $PSScriptRoot 'PrintedWithholdingProbe.cs')
if($LASTEXITCODE -ne 0){throw 'Printed withholding probe compilation failed.'}
$configs=@{}
foreach($version in @('Work Order Entry\bin\Debug\Work Order Entry_V8.exe.config','Work Order Entry\bin\Release\Work Order Entry_V8.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe.config')){
 $path=Join-Path $repo $version;if(Test-Path -LiteralPath $path){$configs[$path]=(Get-FileHash -LiteralPath $path).Hash}
}
try{
 & $probe $ApplicationPath $OutputDirectory $BaselineReport
 if($LASTEXITCODE -ne 0){throw 'Printed withholding checks failed.'}
}finally{
 foreach($path in $configs.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $configs[$path]){throw 'Store configuration changed.'}}
}
