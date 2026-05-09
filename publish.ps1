param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$BaseUrl = "https://cdn.dziondzio.xyz/dipc/",
    [string]$Notes = "",
    [string]$OutDir = "dist",
    [string]$VpsScpTarget = "",
    [string]$VpsRemoteDir = ""
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Join-Path $root "DipcClient\DipcClient.csproj"
$latestJson = Join-Path $root "DipcServer\wwwroot\dipc\latest.json"

if (-not (Test-Path $csproj)) { throw "Brak: $csproj" }
if (-not (Test-Path $latestJson)) { throw "Brak: $latestJson" }

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
$downloadUrl = ($BaseUrl.TrimEnd('/') + "/" + $portableName)

$manifestObj = [ordered]@{
    version    = $version
    downloadUrl = $downloadUrl
    sha256     = $sha
    notes      = $Notes
}

$manifestJson = ($manifestObj | ConvertTo-Json -Depth 3)
Set-Content -Encoding UTF8 -Path $latestJson -Value $manifestJson

Write-Host ""
Write-Host "Gotowe:"
Write-Host "  Portable: $portablePath"
Write-Host "  SHA256:   $sha"
Write-Host "  Manifest: $latestJson"
Write-Host "  URL:      $downloadUrl"

Write-Host ""
Write-Host "Installer (Inno Setup):"
$isccCandidates = @(
    "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)
$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$iss = Join-Path $root "DIPC.iss"

if (Test-Path $iss -and -not [string]::IsNullOrWhiteSpace($iscc)) {
    & $iscc "/DMyAppVersion=$version" "/DSourceExe=$portablePath" $iss | Out-Host
    Write-Host "  Zbudowano installer (sprawdź folder: $outFull)"
} else {
    Write-Host "  Pominięto (brak DIPC.iss lub ISCC.exe)."
}

if (-not [string]::IsNullOrWhiteSpace($VpsScpTarget) -and -not [string]::IsNullOrWhiteSpace($VpsRemoteDir)) {
    Write-Host ""
    Write-Host "Upload na VPS (scp):"
    Write-Host "  scp `"$portablePath`" `"$latestJson`" $VpsScpTarget:`"$VpsRemoteDir`""
}
