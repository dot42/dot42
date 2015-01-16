using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Dot42 Certificate Utility")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:dot42.CryptoUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Common.Forms.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:dot42.Common.Licensing.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:BouncyCastle-v1.7.dll /internal:true")]

[assembly: Obfuscation(Feature = "protect-serialization", ApplyToMembers = true)]
