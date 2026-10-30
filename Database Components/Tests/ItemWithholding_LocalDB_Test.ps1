param([Parameter(Mandatory=$true)][string]$ApplicationPath,
      [Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
$null=New-Item -ItemType Directory -Force -Path $OutputDirectory
$runtime=Join-Path $env:WINDIR 'assembly\GAC_MSIL'
$winforms=Join-Path $runtime 'Microsoft.ReportViewer.WinForms\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.WinForms.dll'
$common=Join-Path $runtime 'Microsoft.ReportViewer.Common\10.0.0.0__b03f5f7f11d50a3a\Microsoft.ReportViewer.Common.dll'
$probe=Join-Path $OutputDirectory 'ItemWithholdingProbe.exe'
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:exe /platform:x86 "/out:$probe" /r:System.Data.dll /r:System.Xml.Linq.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "/r:$winforms" "/r:$common" (Join-Path $PSScriptRoot 'ItemWithholdingProbe.cs')
if($LASTEXITCODE -ne 0){throw 'Probe compile failed'}
$configs=@{}
foreach($relative in @('Work Order Entry\bin\Debug\Work Order Entry_V8.exe.config','Work Order Entry\bin\Release\Work Order Entry_V8.exe.config','Work Order Entry V4\Work Order Entry_4\Work Order Entry\bin\Release\Work Order Entry_V4.exe.config')){
 $path=Join-Path $repo $relative;if(Test-Path -LiteralPath $path){$configs[$path]=(Get-FileHash -LiteralPath $path).Hash}
}
try{
 & $probe ([IO.Path]::GetFullPath($ApplicationPath)) $OutputDirectory
 if($LASTEXITCODE -ne 0){throw 'Item marker fixture failed'}
}finally{
 foreach($path in $configs.Keys){if((Get-FileHash -LiteralPath $path).Hash -ne $configs[$path]){throw 'Store config changed'}}
}
