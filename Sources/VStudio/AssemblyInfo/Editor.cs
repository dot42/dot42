using System.Reflection;

// Inject libraries
[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbUILib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ApkLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.IdeLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:SharpZipLib.dll /internal:true")]

// Select features
[assembly: Obfuscation(Feature = "disable-proxy")]
[assembly: Obfuscation(Feature = "disable-restructure")]
[assembly: Obfuscation(Feature = "rename /charset:ascii-lower")]

// Obfuscate all of sharpziplib
[assembly: Obfuscation(Feature = "protect-serialization /namespace:ICSharpCode.SharpZipLib")]
// Obfuscate all of MPF
[assembly: Obfuscation(Feature = "rename /namespace:Microsoft", Exclude = false)]
// Obfuscate all of ResourceLib
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.ResourcesLib", Exclude = false)]
// Obfuscate extenders
[assembly: Obfuscation(Feature = "rename /namespace:Dot42.VStudio.Extenders", Exclude = false)]
