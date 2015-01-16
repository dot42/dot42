using System.Reflection;

// Inject libraries
[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ApkLib.dll /internal:true")]
//[assembly: Obfuscation(Feature = "inject /a:Dot42.AvdLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.BarLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.BarDeployLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CryptoUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DeviceLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DeviceUILib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.IdeLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.JvmClassLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DebuggerLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DexLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.MappingLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ResourcesLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]

// Inject dependencies
[assembly: Obfuscation(Feature = "inject /a:BouncyCastle-v1.7.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:SharpZipLib.dll /internal:true")]

// Select features
[assembly: Obfuscation(Feature = "disable-proxy")]
[assembly: Obfuscation(Feature = "disable-restructure")]
[assembly: Obfuscation(Feature = "disable-encrypt-resources")]
[assembly: Obfuscation(Feature = "disable-anti-decompiler")]
[assembly: Obfuscation(Feature = "rename /charset:ascii-lower")]

// Obfuscate all of sharpziplib
[assembly: Obfuscation(Feature = "protect-serialization /namespace:ICSharpCode.SharpZipLib")]
// Obfuscate all of MPF
[assembly: Obfuscation(Feature = "rename /namespace:Microsoft", Exclude = false)]
// Rename overrides
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.ResourcesLib", Exclude = false)]
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.Ide.Extenders", Exclude = false)]
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.VStudio.Debugger", Exclude = false)]
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.VStudio.Extenders", Exclude = false)]
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.VStudio.Flavors", Exclude = false)]

[assembly: Obfuscation(Feature = "change-namespace /from:Dot42.Ide.Serialization.Nodes.Menu /to:Dot42.Menus", Exclude = false)]
