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
MinVersion=10.0.19041
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
Source: "{#PublishDir}\MicrosoftEdgeWebview2Setup.exe"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall

[Code]
const
  RuntimeKey = 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

function RuntimePresent(): Boolean;
var
  V: String;
begin
  Result := (RegQueryStringValue(HKLM32, RuntimeKey, 'pv', V) and (V <> '') and (V <> '0.0.0.0'));
  if not Result then
    Result := (RegQueryStringValue(HKCU, RuntimeKey, 'pv', V) and (V <> '') and (V <> '0.0.0.0'));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ExitCode: Integer;
begin
  Result := '';
  if RuntimePresent() then Exit;
  if not WizardSilent() then
    if MsgBox('Microsoft Edge WebView2 Runtime is required and is not installed. Install it now? Internet access may be needed.', mbConfirmation, MB_YESNO) <> IDYES then
    begin
      Result := 'WebView2 is required. Install it before continuing.';
      Exit;
    end;
  try
    ExtractTemporaryFile('MicrosoftEdgeWebview2Setup.exe');
    if not Exec(ExpandConstant('{tmp}\MicrosoftEdgeWebview2Setup.exe'), '/silent /install', '', SW_SHOW, ewWaitUntilTerminated, ExitCode) then
      Result := 'Unable to start the Microsoft WebView2 installer.'
    else if not RuntimePresent() then
      Result := 'WebView2 installation was not confirmed. Check Internet access or install the Microsoft Evergreen Standalone Runtime, then retry. Exit code: ' + IntToStr(ExitCode);
  except
    Result := 'WebView2 installation failed: ' + GetExceptionMessage;
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\GrowBox-Panel"
