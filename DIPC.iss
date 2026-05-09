#define MyAppName "DIPC"
#define MyAppPublisher "dziondzio"
#define MyAppURL "https://dziondzio.xyz/"
#define MyAppExeName "DIPC.exe"

#ifndef MyAppVersion
#define MyAppVersion "0.0.0"
#endif

#ifndef SourceExe
#define SourceExe "dist\\DIPC.exe"
#endif

[Setup]
AppId={{5D36D6C5-BA35-4D4A-9C71-6A6AC4D57F2F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=dist
OutputBaseFilename=DIPC_Installer_{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "Utwórz ikonę na pulpicie"; GroupDescription: "Skróty:"; Flags: unchecked

[Files]
Source: "{#SourceExe}"; DestDir: "{app}"; DestName: "{#MyAppExeName}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Uruchom {#MyAppName}"; Flags: nowait postinstall skipifsilent
