#define MyAppName "GrowBox Panel"
#define MyAppPublisher "GrowBox Panel"
#define MyAppURL "https://github.com/ropecope/HA-WinKiosk"
#define MyAppExeName "HAWinKiosk.exe"
#define MyAppVersion "3.30.2"

[Setup]
AppId={{B4730DF5-6F27-41F0-A1F2-44CB55EB7249}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
; Per-user install (no admin / UAC). Matches self-contained publish output from tools\Build-ExeFromGDrive.ps1.
DefaultDirName={localappdata}\Programs\GrowBox Panel
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64compatible
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
OutputDir=.\output
OutputBaseFilename=HAWinKiosk-Setup
SetupIconFile=..\light_logo.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
#ifndef PublishDir
#define PublishDir "..\src\HAWinKiosk\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
#endif
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\GrowBox-Panel"
