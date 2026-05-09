# DIPC (Windows)

[![Release](https://img.shields.io/github/v/release/Dziondzio/dipc?display_name=tag&sort=semver)](https://github.com/Dziondzio/dipc/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Dziondzio/dipc/total)](https://github.com/Dziondzio/dipc/releases)
[![Build](https://github.com/Dziondzio/dipc/actions/workflows/release.yml/badge.svg)](https://github.com/Dziondzio/dipc/actions/workflows/release.yml)
[![License](https://img.shields.io/github/license/Dziondzio/dipc)](LICENSE)

![Preview](https://github.com/Dziondzio/dipc/Images/v/100.png)

Windowsowe narzędzie (WinForms, .NET 8) do szybkiego sprawdzania informacji o komputerze, temperatur, zdarzeń, dysków (SMART/sensory) oraz do uruchamiania komend serwisowych.

## Pobieranie

Pobierz najnowszą wersję z GitHub Releases:
- [Latest Release](https://github.com/Dziondzio/dipc/releases/latest)

W assets są zwykle dwa pliki:
- `DIPC_Portable_<wersja>.exe` (portable, single-file)
- `DIPC_Installer_<wersja>.exe` (instalator)

## Funkcje

- Informacje o PC: CPU, GPU, RAM, płyta, BIOS, system, sieć, ekrany
- Dyski: informacje + „Dysk SMART” (zależnie od tego, co udostępnia system/czujniki)
- Zdarzenia: filtrowanie + kopiowanie + czyszczenie logów (wymaga admina)
- Temperatury live (co 5s)
- „Syf”: opróżnij kosz, usuń pliki tymczasowe
- „Komendy”: lista komend serwisowych + uruchamianie (także jako admin)
- Linki do narzędzi (np. CrystalDiskInfo, CPU‑Z)

## Aktualizacje (GitHub Releases)

Aplikacja sprawdza aktualizacje z GitHub Releases (`releases/latest`) i pobiera pliki z assets.

- Domyślnie repo do aktualizacji: `Dziondzio/dipc` (jeśli robisz fork, możesz zmienić)
- Możesz to zmienić bez przebudowy ustawiając zmienną środowiskową:
  - `DIPC_GITHUB_REPO=owner/nazwa-repo`

W release powinny być assets:
- `DIPC_Portable_<wersja>.exe` (+ opcjonalnie `DIPC_Portable_<wersja>.exe.sha256`)
- `DIPC_Installer_<wersja>.exe` (+ opcjonalnie `DIPC_Installer_<wersja>.exe.sha256`)

Jeśli `*.sha256` jest dostępny, aplikacja weryfikuje plik po pobraniu.

## Tryb portable

Ustawienia zapisują się obok EXE jeśli:
- program działa z pendrive (dysk removable), albo
- obok pliku EXE znajduje się `portable.flag`, albo
- ustawisz `DIPC_PORTABLE=1`

## Wymagania

- Windows 10/11
- Dla niektórych akcji wymagany jest administrator (np. czyszczenie logów, część komend)

## Budowanie lokalnie

Wymagania:
- .NET SDK 8.x
- (opcjonalnie installer) Inno Setup 6

### Debug / uruchomienie w IDE

```powershell
dotnet build .\DipcClient\DipcClient.csproj
dotnet run --project .\DipcClient\DipcClient.csproj
```

### Portable EXE (single-file)

```powershell
dotnet publish .\DipcClient\DipcClient.csproj -c Release -r win-x64 `
  -p:SelfContained=true -p:PublishSingleFile=true -p:PublishTrimmed=false `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

Gotowy plik będzie w:
`DipcClient\bin\Release\net8.0-windows\win-x64\publish\DipcClient.exe`

### Skrypt build (portable + sha256 + opcjonalnie installer)

```powershell
.\publish.ps1
```

Wynik trafi do folderu `dist\`.

### Installer EXE (Inno Setup)

Zainstaluj Inno Setup 6, potem:

```powershell
$iscc = "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe"
& $iscc "/DMyAppVersion=1.0.0-YYYYMMDDHHmm" "/DSourceExe=.\dist\DIPC_Portable_1.0.0-YYYYMMDDHHmm.exe" .\DIPC.iss
```

## Release automatyczny (GitHub Actions)

Workflow `.github/workflows/release.yml` na Windows runner:
- buduje portable EXE + sha256
- buduje installer (Inno Setup przez Chocolatey) + sha256
- publikuje GitHub Release z tagiem `v<wersja>`

Domyślnie uruchamia się na push do `main` (możesz to zmienić na `workflow_dispatch` jeśli chcesz robić release ręcznie).

## Uwagi bezpieczeństwa

To narzędzie uruchamia komendy systemowe i potrafi czyścić logi zdarzeń. Używaj świadomie i na własną odpowiedzialność.

## Dla forków

Jeśli zmienisz owner/repo, podmień linki w badge’ach na górze tego README na swoje (albo usuń badge’y).


## Bugi
- [ X ] Pamięc karty graficznj jest nieprawidłowy

