using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dot42 Assembly Check")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:Dot42.Documentation.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CecilExtensions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:Cecil.dll /internal:true")]

[assembly: Obfuscation(Feature = "protect-serialization", ApplyToMembers = true)]
