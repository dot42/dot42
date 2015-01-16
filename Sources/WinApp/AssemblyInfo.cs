using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dot42 Device Manager")]
[assembly: AssemblyProduct("dot42 Device Manager")]
[assembly: AssemblyCompany("TallComponents BV")]
[assembly: AssemblyCopyright("TallComponents BV")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ApkLib.dll /internal:true")]
//[assembly: Obfuscation(Feature = "inject /a:Dot42.AvdLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.BarLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.BarDeployLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CryptoUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DeviceLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DeviceUILib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Forms.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Licensing.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:BouncyCastle-v1.7.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Mono.Options.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:SharpZipLib.dll /internal:true")]


[assembly: Obfuscation(Feature = "protect-serialization", ApplyToMembers = true)]

