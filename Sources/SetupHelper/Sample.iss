;
; Author: Vincenzo Giordano
; email: isxkb@vincenzo.net
;
; This script shows how to call psvince.dll and detect if a module
; is loaded in memory or not so you can detect if a program is running


[Setup]
AppName=PSVince
AppVerName=PSVince 1.0
DisableProgramGroupPage=true
DisableStartupPrompt=true
OutputDir=.
OutputBaseFilename=testpsvince
Uninstallable=false
DisableDirPage=true
DefaultDirName={pf}\PSVince

[Files]
Source: psvince.dll; Flags: dontcopy

[Code]
function IsModuleLoaded(modulename: String ):  Boolean;
external 'IsModuleLoaded@files:psvince.dll stdcall';

function InitializeSetup(): Boolean;
begin

  Result := Not IsModuleLoaded( 'notepad.exe' );

end;
