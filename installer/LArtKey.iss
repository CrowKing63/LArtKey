#define AppName    "LArtKey"
#define AppVersion "1.0.0"
#define AppExeName "LArtKey.exe"
#define BuildDir   "..\dist\publish"

[Setup]
AppId={{9E2DF5FD-F184-4B88-A756-4A7C4B450001}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=..\dist
OutputBaseFilename=LArtKey-Setup-v{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\LArtKey\Assets\icon.ico
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#BuildDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LArtKey\layouts\*"; DestDir: "{userappdata}\LArtKey\layouts"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist
Source: "..\LArtKey\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill.exe"; Parameters: "/f /im {#AppExeName}"; Flags: runhidden; RunOnceId: "KillLArtKey"
