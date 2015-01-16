using System.Reflection;

[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.AdbUILib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ApkLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.AvdLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.CryptoUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DebuggerLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.DexLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.FrameworkDefinitions.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Graphics.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.IdeLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.JvmClassLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.MappingLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Common.Licensing.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.ResourcesLib.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.SharedUI.dll /internal:true")]
[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:BouncyCastle.dll /internal:true")]

[assembly: Obfuscation(Feature = "disable-proxy")]
[assembly: Obfuscation(Feature = "rename /charset:ascii-lower")]

[assembly: Obfuscation(Feature = "make-internal /t:Dot42.Ide.Editors.Layout.XmlLayoutDesignerControl", Exclude = false)]
[assembly: Obfuscation(Feature = "cleanup /t:Dot42.Ide.Editors.Layout.XmlLayoutDesignerControl", Exclude = false)]

