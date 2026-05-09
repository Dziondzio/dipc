param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutDir = "dist"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Join-Path $root "DipcClient\DipcClient.csproj"

if (-not (Test-Path $csproj)) { throw "Brak: $csproj" }

[xml]$xml = Get-Content $csproj
$versionPrefix = $xml.Project.PropertyGroup.VersionPrefix | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($versionPrefix)) { $versionPrefix = "1.0.0" }

$suffix = (Get-Date).ToUniversalTime().ToString("yyyyMMddHHmm")
$version = "$versionPrefix-$suffix"

$outFull = Join-Path $root $OutDir
New-Item -ItemType Directory -Force -Path $outFull | Out-Null

Write-Host "Wersja: $version"
Write-Host "Publikowanie portable..."

dotnet publish $csproj `
    -c $Configuration `
    -r $Runtime `
    -p:SelfContained=true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:VersionSuffix=$suffix | Out-Host

$publishExe = Join-Path $root "DipcClient\bin\$Configuration\net8.0-windows\$Runtime\publish\DipcClient.exe"
if (-not (Test-Path $publishExe)) { throw "Brak pliku publish: $publishExe" }

$portableName = "DIPC_Portable_$version.exe"
$portablePath = Join-Path $outFull $portableName
Copy-Item -Force $publishExe $portablePath

$sha = (Get-FileHash -Algorithm SHA256 -Path $portablePath).Hash
Set-Content -Encoding ascii -Path ($portablePath + ".sha256") -Value $sha

Write-Host ""
Write-Host "Gotowe:"
Write-Host "  Portable: $portablePath"
Write-Host "  SHA256:   $sha"
Write-Host "  SHA file: $($portablePath + '.sha256')"

Write-Host ""
Write-Host "Installer (Inno Setup):"
$isccCandidates = @(
    "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)
$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$iss = Join-Path $root "DIPC.iss"

if (-not (Test-Path $iss)) {
    Write-Host "  Pominięto (brak pliku: $iss)."
} elseif ([string]::IsNullOrWhiteSpace($iscc)) {
    Write-Host "  Pominięto (brak Inno Setup: ISCC.exe)."
    Write-Host "  Zainstaluj Inno Setup 6 i uruchom ponownie skrypt."
    Write-Host "  Przykład (winget): winget install InnoSetup.InnoSetup"
} else {
    & $iscc "/DMyAppVersion=$version" "/DSourceExe=$portablePath" $iss | Out-Host
    $installerPath = Join-Path $outFull "DIPC_Installer_$version.exe"
    if (Test-Path $installerPath) {
        $installerSha = (Get-FileHash -Algorithm SHA256 -Path $installerPath).Hash
        Set-Content -Encoding ascii -Path ($installerPath + ".sha256") -Value $installerSha
        Write-Host "  Installer: $installerPath"
        Write-Host "  SHA256:    $installerSha"
    } else {
        Write-Host "  Zbudowano installer (sprawdź folder: $outFull)"
    }
}
