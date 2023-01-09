; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Virtual Motion Tracker"
#define MyAppVersion "0.14b"
#define MyAppPublisher "gpsnmeajp"
#define MyAppURL "https://github.com/gpsnmeajp/VirtualMotionTracker"
#define MyAppExeName "vmt_manager.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{D745AC13-2CFD-4BF8-8C02-5177986E80C0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\vmt_driver
DisableDirPage=yes
DisableProgramGroupPage=yes
LicenseFile=D:\myprojects\git\VirtualMotionTracker\build\LICENSE
InfoBeforeFile=D:\myprojects\git\VirtualMotionTracker\build\3rd_licence.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=lowest
OutputDir=D:\myprojects\git\VirtualMotionTracker\setup
OutputBaseFilename=VirtualMotionTrackerSetup_{#MyAppVersion}
SetupIconFile=D:\myprojects\git\VirtualMotionTracker\docs\vmt.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon=D:\myprojects\git\VirtualMotionTracker\docs\vmt.ico
WizardImageFile=D:\myprojects\git\VirtualMotionTracker\docs\vmt_vr.bmp
WizardSmallImageFile=D:\myprojects\git\VirtualMotionTracker\docs\VMTlogo.bmp
DisableWelcomePage=no
AppMutex=VMT_Mutex
SetupMutex=VMTSetupMutex


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\myprojects\git\VirtualMotionTracker\build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\vmt_manager\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\vmt_manager\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\vmt_manager\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

