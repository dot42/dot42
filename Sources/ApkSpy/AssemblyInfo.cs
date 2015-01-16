using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dot42 Apk Spy")]
[assembly: AssemblyProduct("dot42 Apk Spy")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:BouncyCastle-v1.7.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Cecil.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:SharpZipLib.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:dot42.ApkLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CecilExtensions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CompilerLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.DexLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.JvmClassLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.LoaderLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.MappingLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Utility.dll /internal:true")]

[assembly: Obfuscation(Feature = "protect-serialization", ApplyToMembers = true)]
