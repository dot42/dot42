; -- Setup.iss --
; Installer for TallComponents dot42 1.x
;

#define VS10
#define DOTNET45
#define DotNetInitializeSetup
#define haveLocalNextButtonClick
#define ISXDLL ".\Application\Install\isxdl.dll"
#define UPDATE_ICON

; Get target
#ifdef Android
  #define ANDROID   1
  #define BB        0
#else
#ifdef BlackBerry
  #define ANDROID   0
  #define BB        1
#else
  #error Define ANDROID or BB
#endif
#endif

#if ANDROID
  #define SHARPDEVELOP
#endif

#define SrcBaseDir   ".\Application\"
#define AppShortName "dot42"
#define OldAppId     "Tall_Dot42_v1"
#define AppVersion   GetFileVersion("..\Build\Application\Dot42Compiler.exe")
#define AppExeName   "dot42"

#if ANDROID
  ; If AppName or AppShortName is changed, request a new PLK for the VStudio integration!
  #define AppName      "dot42 C# for Android"
  #define AppId        "Dot42_Android_v1"
  #define AppMutex     "C51F2F88-FC6C-4AD3-B036-D0DED2A492E0"
  #define TargetName   "Android"

  #define VSGUID       "{{337B7DB7-2D1E-448D-BEBF-1BFB9A61C08F}"
  #define VSPROJFACTGUID "{{337B7DB7-2D1E-448D-BEBF-17E887A46E37}"
  #define VSIXID       "7137EACA-1DD6-41EE-AB8B-35065C077644"
#elif BB
  ; If AppName or AppShortName is changed, request a new PLK for the VStudio integration!
  #define AppName      "dot42 C# for BlackBerry"
  #define AppId        "Dot42_BlackBerry_v1"
  #define AppMutex     "C51F2F88-FC6C-4AD3-B036-D0DED2A492E0"
  #define TargetName   "BlackBerry"

  #define VSGUID       "{{337B7DB7-E1D2-D844-BEBF-1BFB9A61C08F}"
  #define VSPROJFACTGUID "{{337B7DB7-E1D2-D844-BEBF-17E887A46E37}"
  #define VSIXID       "F5BD75B2-2443-4CF6-8CE4-D26624D1A3C9"
#else
  #error Define ANDROID or BB
#endif

#define SrcDir       ".\Application"
#define CodeDir      ".\code"
#define SampleDir    ".\samples"
#define TemplateDir  ".\Templates"
#define FrameworkDir  ".\Frameworks"
#define ProductDir   "..\Product"
#define SharpDevelopDir ".\SharpDevelop"

#define CompanyName  "dot42" 
#define Homepage     "http://www.dot42.com"

#define haveLocalPrepareToInstall

#include "..\Common\Install\dotnet.iss"

[Setup]
AppName={#AppName}
AppId={#AppId}
AppCopyright={#CompanyName}
AppMutex={#AppMutex}
AppVerName={#AppName} ({#AppVersion})
AppVersion={#AppVersion}
AppPublisher={#CompanyName}
AppPublisherURL={#Homepage}
DefaultDirName={pf}\{#AppShortName}\{#TargetName}
DefaultGroupName={#AppName}
DisableDirPage=yes
DisableProgramGroupPage=yes
SetupIconFile=..\Sources\Graphics\Icons\App.ico
UninstallDisplayIcon={app}\{#AppExeName}.exe
Compression=lzma
SolidCompression=yes
OutputDir=Setup
OutputBaseFileName={#AppExeName}{#TargetName}Setup
VersionInfoDescription={#AppName} installer
VersionInfoVersion={#AppVersion}
ChangesAssociations=yes
ChangesEnvironment=yes
AllowUNCPath=no
WizardImageFile=..\Sources\Graphics\Install\banner-164x314.bmp
WizardSmallImageFile=..\Sources\Graphics\Install\banner-55x58.bmp
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x86 x64
SourceDir=..\Build

[Tasks]
Name: vstudio10; Description: "Install VisualStudio 2010 integration"; Check: VStudio10Installed;
Name: vstudio11; Description: "Install VisualStudio 2012 integration"; Check: VStudio11Installed;
Name: vstudio12; Description: "Install VisualStudio 2013 integration"; Check: VStudio12Installed;
#ifdef SHARPDEVELOP
Name: sharpDevelop; Description: "Install SharpDevelop 4.3";
#endif

[Files]
; Binaries
Source: "{#SrcDir}\dot42ApkSpy.exe"; DestDir: "{app}";
Source: "{#SrcDir}\{#TargetName}\dot42DevCenter.exe"; DestDir: "{app}";
Source: "{#SrcDir}\dot42Check.exe"; DestDir: "{app}";
Source: "{#SrcDir}\dot42Compiler.exe"; DestDir: "{app}";
Source: "{#SrcDir}\dot42.MSBuild.Tasks.{#TargetName}.dll"; DestDir: "{app}";
; Visual Studio 2010 extension
Source: "{#SrcDir}\{#TargetName}\Extension\*"; DestDir: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\10.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0"; Flags: recursesubdirs; Tasks: vstudio10;
; Visual Studio 2012 extension
Source: "{#SrcDir}\{#TargetName}\Extension\*"; DestDir: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\11.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0"; Flags: recursesubdirs; Tasks: vstudio11;
; Visual Studio 2013 extension
Source: "{#SrcDir}\{#TargetName}\Extension\*"; DestDir: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\12.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0"; Flags: recursesubdirs; Tasks: vstudio12;
; Android platform tools
Source: "{#SrcDir}\Platform-tools\*"; DestDir: "{app}\Platform-tools";
; Android tools
Source: "{#SrcDir}\Tools\*"; DestDir: "{app}\Tools";
; 'dx' from Android SDK tools
Source: "{#SrcDir}\Tools\dx\*"; DestDir: "{app}"; Excludes: README.txt
; MSBuild Targets files
Source: "{#SrcDir}\Scripts\{#TargetName}\*.targets"; DestDir: "{app}";
; Frameworks
Source: "{#FrameworkDir}\{#TargetName}\*"; Excludes: "*.cs,*.pdb"; DestDir: "{app}\Frameworks"; Flags: recursesubdirs;
; Frameworks redist lists
Source: "{#FrameworkDir}\{#TargetName}\*"; Excludes: "*.cs,*.pdb,skins\*,*.ini,*.properties,*.apk"; DestDir: "{pf32}\Reference Assemblies\Microsoft\Framework\{#AppName}"; Flags: recursesubdirs;
; Used by install
Source: "{#SrcBaseDir}\dot42.Setup.dll"; Flags: dontcopy;
; Used by uninstall
Source: "{#SrcBaseDir}\dot42.Setup.dll"; DestDir: "{app}";
; Samples 
Source: "{#SrcDir}\Samples.zip"; DestDir: "{app}"; 
; Url's
Source: "{#ProductDir}\*.url"; DestDir: "{app}";
#ifdef SHARPDEVELOP
; SharpDevelop
Source: "{#SharpDevelopDir}\*"; DestDir: "{app}\SharpDevelop"; Flags: recursesubdirs; Tasks: sharpDevelop;
#endif

[Registry]
; Installation folder into current version registry key
Root: HKLM; Subkey: "SOFTWARE\dot42\{#TargetName}\v1"; ValueType: string; ValueName: "Folder"; ValueData: "{app}"; Flags: uninsdeletekey;
Root: HKLM32; Subkey: "SOFTWARE\dot42\{#TargetName}\v1"; ValueType: string; ValueName: "Folder"; ValueData: "{app}"; Flags: uninsdeletekey; Check: IsWin64;
; Set current version app folder in generic registry key
Root: HKLM; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "Current"; ValueData: "{app}"; Flags: uninsdeletekey;
Root: HKLM32; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "Current"; ValueData: "{app}"; Flags: uninsdeletekey; Check: IsWin64;
; Set current version extensions folder in generic registry key
Root: HKLM; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "ExtensionsPath"; ValueData: "{app}"; Flags: uninsdeletekey;
Root: HKLM32; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "ExtensionsPath"; ValueData: "{app}"; Flags: uninsdeletekey; Check: IsWin64;
; Set current version reference assemblies folder in generic registry key
Root: HKLM; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "ReferenceAssemblies"; ValueData: "{app}\Frameworks"; Flags: uninsdeletekey;
Root: HKLM32; Subkey: "SOFTWARE\dot42\{#TargetName}"; ValueType: string; ValueName: "ReferenceAssemblies"; ValueData: "{app}\Frameworks"; Flags: uninsdeletekey; Check: IsWin64;

; Old path
; Set current version extensions folder in generic registry key
Root: HKLM; Subkey: "SOFTWARE\TallApplications\dot42"; ValueType: string; ValueName: "ExtensionsPath"; ValueData: "{app}"; Flags: uninsdeletekey;
Root: HKLM32; Subkey: "SOFTWARE\TallApplications\dot42"; ValueType: string; ValueName: "ExtensionsPath"; ValueData: "{app}"; Flags: uninsdeletekey; Check: IsWin64;

#define VSROOTKEY "HKCU"
#define VS10ROOT   "SOFTWARE\Microsoft\VisualStudio\10.0"
#define VSROOT    "SOFTWARE\Microsoft\VisualStudio\10.0\Configuration"
#define VSTASK    "vstudio10"
#define VSASM     "Dot42.VStudio10.dll"
#define VSTEMPLATES "VStudio10"
#include "MSBuildSetup.iss"

#define VS11ROOT   "SOFTWARE\Microsoft\VisualStudio\11.0"
#define VSROOT    "SOFTWARE\Microsoft\VisualStudio\11.0\Configuration"
#define VSTASK    "vstudio11"
#define VSASM     "Dot42.VStudio10.dll"
#define VSTEMPLATES "VStudio10"
#include "MSBuildSetup.iss"

#define VS12ROOT   "SOFTWARE\Microsoft\VisualStudio\12.0"
#define VSROOT    "SOFTWARE\Microsoft\VisualStudio\12.0\Configuration"
#define VSTASK    "vstudio12"
#define VSASM     "Dot42.VStudio10.dll"
#define VSTEMPLATES "VStudio10"
#include "MSBuildSetup.iss"

#include "..\Build\Registry\Frameworks.iss"

[Icons]
Name: "{group}\{#AppName} Device Center"; Filename: "{app}\dot42DevCenter.exe"; WorkingDir: "{app}"
Name: "{group}\{#AppName} Assembly Check"; Filename: "{app}\dot42Check.exe"; WorkingDir: "{app}"
Name: "{group}\{#AppName} Samples"; Filename: "{userdocs}\dot42\Samples"; Check: UserDocsExists;
Name: "{group}\Getting started"; Filename: "{app}\GettingStarted.url"; 
#ifdef SHARPDEVELOP
Name: "{group}\Sharp Develop ({#AppName})"; Filename: "{app}\SharpDevelop\bin\SharpDevelop.exe"; WorkingDir: "{app}\SharpDevelop"; Tasks: sharpDevelop;
#endif

[Run]
; Optimize assemblies
Filename: "{dotnet4032}\ngen.exe"; Parameters: "install ""{app}\ApkSpy.exe"""; StatusMsg: {cm:Optimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "install ""{app}\dot42DevCenter.exe"""; StatusMsg: {cm:Optimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "install ""{app}\dot42Check.exe"""; StatusMsg: {cm:Optimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "install ""{app}\dot42Compiler.exe"""; StatusMsg: {cm:Optimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "install ""{app}\dot42.MSBuild.Tasks.dll"""; StatusMsg: {cm:Optimize}; Flags: runhidden;
; Setup devenv 10.0
Filename: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\10.0,InstallDir}\devenv.exe"; Parameters: "/setup"; StatusMsg: "{cm:ConfigureDevEnv10}"; Tasks: vstudio10;
; Setup devenv 11.0
Filename: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\11.0,InstallDir}\devenv.exe"; Parameters: "/setup"; StatusMsg: "{cm:ConfigureDevEnv11}"; Tasks: vstudio11;
; Setup devenv 12.0
Filename: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\12.0,InstallDir}\devenv.exe"; Parameters: "/setup"; StatusMsg: "{cm:ConfigureDevEnv12}"; Tasks: vstudio12;
; Update samples
Filename: "{app}\dot42DevCenter.exe"; Parameters: "-samplefolder ""{userdocs}\dot42\{#TargetName}\Samples"""; Description: "{cm:UpdatingSamples}"; Flags: runasoriginaluser; Check: UserDocsExists;
; Open sample folder
Filename: "{userdocs}\dot42\{#TargetName}\Samples\"; Description: "{cm:OpenSamples}"; Flags: postinstall nowait skipifsilent shellexec; Check: UserDocsExists;

[UninstallRun]
; Stop adb
Filename: "{app}\Platform-Tools\adb.exe"; Parameters: "kill-server"; StatusMsg: "{cm:KillAdb}"; Flags: skipifdoesntexist runhidden; 
; Un-ngen
Filename: "{dotnet4032}\ngen.exe"; Parameters: "uninstall ""{app}\ApkSpy.exe"""; StatusMsg: {cm:UnOptimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "uninstall ""{app}\dot42DevCenter.exe"""; StatusMsg: {cm:UnOptimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "uninstall ""{app}\dot42Check.exe"""; StatusMsg: {cm:UnOptimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "uninstall ""{app}\dot42Compiler.exe"""; StatusMsg: {cm:UnOptimize}; Flags: runhidden;
Filename: "{dotnet4032}\ngen.exe"; Parameters: "uninstall ""{app}\dot42.MSBuild.Tasks.dll"""; StatusMsg: {cm:UnOptimize}; Flags: runhidden;

[InstallDelete]
; Remove the extensions in an early version of dot42
Type: filesandordirs; Name: "{localappdata}\Microsoft\VisualStudio\10.0\Extensions\TallApplications\dot42\1.0";
Type: filesandordirs; Name: "{localappdata}\Microsoft\VisualStudio\11.0\Extensions\TallApplications\dot42\1.0";
Type: filesandordirs; Name: "{localappdata}\Microsoft\VisualStudio\10.0\Extensions\dot42\{#TargetName}\1.0";
Type: filesandordirs; Name: "{localappdata}\Microsoft\VisualStudio\11.0\Extensions\dot42\{#TargetName}\1.0";

[UninstallDelete]
Type: filesandordirs; Name: "{app}";
Type: files; Name: "{userdocs}\dot42\{#TargetName}\Samples\Samples.pfx";
Type: filesandordirs; Name: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\10.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0";
Type: filesandordirs; Name: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\11.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0";
Type: filesandordirs; Name: "{reg:HKLM32\SOFTWARE\Microsoft\VisualStudio\12.0,InstallDir}\Extensions\{#CompanyName}\{#AppName}\1.0";

[CustomMessages]
StartApp=Start {#AppName} Device Center
OpenSamples=Open {#AppName} Samples folder
ConfigureDevEnv10=Configuring Visual Studio 2010 ...
ConfigureDevEnv11=Configuring Visual Studio 2012 ...
ConfigureDevEnv12=Configuring Visual Studio 2013 ...
Optimize=Optimizing performance
UnOptimize=Cleanup performance optimizations
KillAdb=Stopping Android Debug Bridge ...
UpdatingSamples=Updating sample projects

[Code]
const
#if ANDROID
  v1OldUninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\Tall_Dot42_v1_is1';
  v1UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\Dot42_Android_v1_is1';
 #elif BB
  v1UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\Dot42_BlackBerry_v1_is1';
 #else
   #error Define target
 #endif

// function IsModuleLoaded to call at install time
// added also setuponly flag
function IsModuleLoaded(modulename: AnsiString ):  Boolean;
external 'IsModuleLoaded@files:dot42.Setup.dll stdcall setuponly';

// function IsModuleLoadedU to call at uninstall time
// added also uninstallonly flag
function IsModuleLoadedU(modulename: AnsiString ):  Boolean;
external 'IsModuleLoaded@{app}\dot42.Setup.dll stdcall uninstallonly' ;

function IsWin86(): Boolean;
begin
  Result := not IsWin64;
end;

function UserDocsExists: Boolean;
begin
  Result := (GetShellFolder(False, sfDocs) <> '');
end;

function VStudio10Installed: Boolean;
var root: string;
begin
  root := ExpandConstant('{#VS10ROOT}');
  Result := RegKeyExists(HKLM32, root) and RegValueExists(HKLM32, root, 'InstallDir');
end;

function VStudio11Installed: Boolean;
var root: string;
begin
  root := ExpandConstant('{#VS11ROOT}');
  Result := RegKeyExists(HKLM32, root) and RegValueExists(HKLM32, root, 'InstallDir');
end;

function VStudio12Installed: Boolean;
var root: string;
begin
  root := ExpandConstant('{#VS12ROOT}');
  Result := RegKeyExists(HKLM32, root) and RegValueExists(HKLM32, root, 'InstallDir');
end;

function AnyValidVStudioInstalled: Boolean;
begin
  Result := VStudio10Installed or VStudio11Installed or VStudio12Installed;
end;

//function VPDExpress10Installed: Boolean;
//var root: string;
//begin
//  root := ExpandConstant('{#VPD10ROOT}');
//  Result := RegKeyExists(HKLM32, root) and RegValueExists(HKLM32, root, 'InstallDir');
//end;

function KillAdb(): Boolean;
var path: string;
    ResultCode: Integer;
begin
  // If adb.exe exists, invoke it with kill-server argument
  path := ExpandConstant('{app}\Platform-tools\adb.exe');
  if (FileExists(path)) then
  begin
    Exec(path, 'kill-server', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

function GetUninstallString(key: String): String;
var
  sUnInstallString: String;
begin
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, key, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKLM32, key, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function DoUnInstallPreviousVersion(key: String): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  sUnInstallString := GetUninstallString(key);
  if sUnInstallString <> '' then begin
    
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

function UnInstallPreviousVersion: Integer;
var
  rc1: Integer;
  rc2: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

#if ANDROID
  rc1 := DoUnInstallPreviousVersion(v1OldUninstallKey);
#else
  rc1 := 1;
#endif

  rc2 := DoUnInstallPreviousVersion(v1UninstallKey);

  if ((rc1 = 2) or (rc2 = 2)) then 
    Result := 2
  else if ((rc1 = 1) and (rc2 = 1)) then
    Result := 1
  else
    Result := 3;
end;

function InitializeSetup(): Boolean;
begin
  // check if devenv is running
  if IsModuleLoaded( 'devenv.exe' ) then
  begin
    MsgBox( 'VisualStudio is running, please close it and run setup again.',
             mbError, MB_OK );
    Result := false;
  end
  else                                
  begin
    if ((not WizardSilent) and (not AnyValidVStudioInstalled)) then
    begin
      if (MsgBox( 'You do not have Visual Studio 2010, 2012 or 2013 Professional installed. Hit Cancel if you want to install one of these first. Then restart the installer. Otherwise, continue and SharpDevelop will be installed.',
             mbInformation, MB_OKCANCEL ) = IDCANCEL) then
      begin
        Result := false;
      end 
      else 
      begin
        Result := DotNetInitializeSetup();
      end;
    end
    else
    begin
      Result := DotNetInitializeSetup();
    end;
  end;
end;

function LocalNextButtonClick(CurPageId: Integer): Boolean;
begin
  if (CurPageId = wpSelectTasks) then
  begin
    if (IsTaskSelected('vstudio10') or IsTaskSelected('vstudio11') or IsTaskSelected('vstudio12') or IsTaskSelected('sharpDevelop')) then
    begin
      // Normal install integrating into Visual Studio
      Result := true;
    end
    else
    begin
      MsgBox( 'Check at least one integration option.', mbError, MB_OK );
      Result := false;
    end;
  end
  else
  begin
    Result := true;
  end;
end;

function LocalPrepareToInstall(var NeedsRestart: Boolean): String;
begin
  KillAdb();
  UnInstallPreviousVersion;
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  // check if notepad is running
  if IsModuleLoadedU( 'devenv.exe' ) then
  begin
    MsgBox( 'VisualStudio is running, please close it and run uninstall again.',
             mbError, MB_OK );
    Result := false;
  end
  else Result := true;

  // Unload the DLL, otherwise the dll is not deleted
  UnloadDLL(ExpandConstant('{app}\dot42.Setup.dll'));
end;
