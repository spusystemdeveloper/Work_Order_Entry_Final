$ErrorActionPreference = "Stop"

$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
$vstest = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
$testProject = Join-Path $PSScriptRoot "Work Order Entry.Tests\Work Order Entry.Tests.vbproj"
$testAssembly = Join-Path $PSScriptRoot "Work Order Entry.Tests\bin\Debug\Work Order Entry.Tests.dll"

if (-not (Test-Path -LiteralPath $msbuild)) {
    throw "MSBuild was not found at $msbuild"
}
if (-not (Test-Path -LiteralPath $vstest)) {
    throw "VSTest was not found at $vstest"
}

& $msbuild $testProject /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "The unit-test project did not build."
}

& $vstest $testAssembly /Platform:x86 /Logger:Console
if ($LASTEXITCODE -ne 0) {
    throw "One or more unit tests failed."
}
