; -- MSBuildSetup.iss --
; Installer for VisualStudio integration part of TallApplications Dot42 1.x
;

; MSBuild safe import
Root: {#VSROOTKEY}; Subkey: "{#VSROOT}\MSBuild\SafeImports"; ValueType: string; ValueName: "dot42.Common.targets"; ValueData: "{app}\dot42.Common.targets"; Flags: deletevalue uninsdeletevalue; Tasks: {#VSTASK};
Root: {#VSROOTKEY}; Subkey: "{#VSROOT}\MSBuild\SafeImports"; ValueType: string; ValueName: "dot42.CSharp.targets"; ValueData: "{app}\dot42.CSharp.targets"; Flags: deletevalue uninsdeletevalue; Tasks: {#VSTASK};
