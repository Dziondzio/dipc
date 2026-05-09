# DIPC (Windows)

Prywatne narzędzie Windows (WinForms, .NET 8) do szybkiego sprawdzania informacji o komputerze, temperatur, zdarzeń, dysków oraz do uruchamiania komend serwisowych.

## Aktualizacje (GitHub Releases)

Aplikacja sprawdza aktualizacje z GitHub Releases (`releases/latest`) i pobiera pliki z assets.

- Domyślnie repo do aktualizacji: `konra/dipc`
- Możesz to zmienić bez przebudowy ustawiając zmienną środowiskową:
  - `DIPC_GITHUB_REPO=owner/nazwa-repo`

W release powinny być assets:
- `DIPC_Portable_<wersja>.exe` (+ opcjonalnie `DIPC_Portable_<wersja>.exe.sha256`)
- `DIPC_Installer_<wersja>.exe` (+ opcjonalnie `DIPC_Installer_<wersja>.exe.sha256`)

## Tryb portable

Ustawienia zapisują się obok EXE jeśli:
- program działa z pendrive (dysk removable), albo
- obok pliku EXE znajduje się `portable.flag`, albo
- ustawisz `DIPC_PORTABLE=1`

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
