; Rectangle for Windows - Inno Setup 安装脚本
; 用法：先 dotnet publish -c Release -r win-x64 -o publish，再用 Inno Setup 编译此脚本

#define AppName "Rectangle"
#define AppVersion "1.0.0"
#define AppPublisher "Rectangle"
#define AppExeName "Rectangle.Windows.WinUI.exe"
#define PublishDir "publish"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=installer_output
OutputBaseFilename=Rectangle-Setup-{#AppVersion}
SetupIconFile=Assets\AppIcon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "开机自动启动"; GroupDescription: "附加任务:"; Flags: unchecked

[Files]
; 主程序 exe（单文件发布后只有这一个 exe + WinAppSDK 的原生 dll）
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\卸载 {#AppName}"; Filename: "{uninstallexe}"

[Registry]
; 开机自启
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
  ValueType: string; ValueName: "{#AppName}"; \
  ValueData: """{app}\{#AppExeName}"""; \
  Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#AppExeName}"; Description: "启动 {#AppName}"; Flags: nowait postinstall skipifsilent
