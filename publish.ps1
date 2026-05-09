param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutDir = "dist"
)

$ErrorActionPreference = "Stop"

function Resolve-IsccPath {
    $cmd = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($cmd -and (Test-Path $cmd.Source)) {
        return $cmd.Source
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup\ISCC.exe"
    ) | Where-Object { $_ -and (Test-Path $_) }

    if ($candidates.Count -gt 0) {
        return $candidates[0]
    }

    $uninstallKeys = @(
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1"
    )

    foreach ($k in $uninstallKeys) {
        try {
            if (Test-Path $k) {
                $p = Get-ItemProperty -Path $k -ErrorAction Stop
                $loc = $p.InstallLocation
                if ($loc) {
                    $try = Join-Path $loc "ISCC.exe"
                    if (Test-Path $try) {
                        return $try
                    }
                }
            }
        } catch {}
    }

    return $null
}

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
$iscc = Resolve-IsccPath
$iss = Join-Path $root "DIPC.iss"

if (-not (Test-Path $iss)) {
    Write-Host "  Pominieto (brak pliku: $iss)."
} elseif ([string]::IsNullOrWhiteSpace($iscc)) {
    Write-Host "  Pominieto (brak Inno Setup: ISCC.exe)."
    Write-Host "  Zainstaluj Inno Setup 6 i uruchom ponownie skrypt."
    Write-Host "  Przyklad (winget): winget install --id JRSoftware.InnoSetup -e --source winget"
    Write-Host "  Jesli instalowales i dalej nie widzi: winget source update"
} else {
    if (-not (Test-Path $iscc)) {
        $fallback = Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
        if (Test-Path $fallback) {
            $iscc = $fallback
        }
    }

    Write-Host "  ISCC: $iscc"
    $args = @(
        "/DMyAppVersion=$version",
        "/DSourceExe=$portablePath",
        "/O$outFull",
        $iss
    )
    $p = Start-Process -FilePath $iscc -ArgumentList $args -Wait -NoNewWindow -PassThru
    if ($p.ExitCode -ne 0) {
        throw "ISCC.exe zakonczyl sie kodem $($p.ExitCode)"
    }
    $installerPath = Join-Path $outFull "DIPC_Installer_$version.exe"
    if (Test-Path $installerPath) {
        $installerSha = (Get-FileHash -Algorithm SHA256 -Path $installerPath).Hash
        Set-Content -Encoding ascii -Path ($installerPath + ".sha256") -Value $installerSha
        Write-Host "  Installer: $installerPath"
        Write-Host "  SHA256:    $installerSha"
    } else {
        Write-Host "  Zbudowano installer (sprawdz folder: $outFull)"
    }
}
