using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dot42 Compiler")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:BouncyCastle-v1.7.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Cecil.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Mono.Options.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:SharpZipLib.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:Dot42.ApkLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.BarLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CecilExtensions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CompilerLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DexLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ImportJarLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.JvmClassLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.LoaderLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.MappingLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ResourcesLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.WcfToolsLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Licensing.dll /internal:true")]

[assembly: Obfuscation(Feature = "protect-serialization", ApplyToMembers = true)]
