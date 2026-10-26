param([switch]$CompileOnly, [switch]$Smoke,
    [string]$BuildTools = '')
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$output = Join-Path $PSScriptRoot 'bin\Console'
New-Item -ItemType Directory -Force $output | Out-Null
$packages = $env:NUGET_PACKAGES
if (!$packages) { $packages = Join-Path $env:USERPROFILE '.nuget\packages' }
$html = Join-Path $packages 'htmlagilitypack\1.11.70\lib\Net45\HtmlAgilityPack.dll'
if (!(Test-Path $html)) { throw 'Restore ExViewer.sln NuGet packages first (see repository CI).' }
$production = Join-Path $repo 'ExClient\Internal'
$sources = @('ThumbnailSource.cs','ThumbnailParser.cs','ThumbnailLoadState.cs','ThumbnailDownloadCache.cs') | ForEach-Object { Join-Path $production $_ }
$sources += (Join-Path $PSScriptRoot 'Regression.cs'), (Join-Path $PSScriptRoot 'ConsoleMain.cs'), (Join-Path $PSScriptRoot 'ReviewCases.cs')
if ($BuildTools) {
    $compiler = Join-Path $BuildTools 'MSBuild\Current\Bin\Roslyn\csc.exe'
} else {
    $msbuild = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($msbuild) { $compiler = Join-Path (Split-Path $msbuild.Source) 'Roslyn\csc.exe' }
    else { $compiler = 'C:\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe' }
}
if (!(Test-Path $compiler)) { throw 'VS MSBuild/Roslyn not found. Use -BuildTools or setup-msbuild.' }
& $compiler /nologo /langversion:latest /warnaserror+ /target:exe /reference:System.Runtime.Serialization.dll "/out:$output\ThumbnailRegression.exe" "/reference:$html" $sources
if ($LASTEXITCODE -ne 0) { throw "Regression compile failed: $LASTEXITCODE" }
Copy-Item $html $output -Force
if (!$CompileOnly) {
    $arguments = @('--review-cases', (Join-Path $PSScriptRoot 'parser-review-cases.json'))
    if ($Smoke) { $arguments += '--smoke' }
    & "$output\ThumbnailRegression.exe" @arguments
    if ($LASTEXITCODE -ne 0) { throw "Regression failed: $LASTEXITCODE" }
}
