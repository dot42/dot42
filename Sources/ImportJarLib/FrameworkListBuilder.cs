using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.ImportJarLib
{
    public static class FrameworkListBuilder
    {
        public static void Build(string frameworkFolder, string forwardAssembliesFolder, string publicKeyToken, Version assemblyVersion, string platformVersion)
        {
            var folder = Path.Combine(frameworkFolder, "RedistList");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var root = new XElement("FileList");
            var doc = new XDocument(root);

            root.Add(new XAttribute("Redist", "Microsoft-Windows-CLRCoreComp.Dot42." + assemblyVersion));
            root.Add(new XAttribute("Name", "Dot42 for Android " + platformVersion));
            root.Add(new XAttribute("RuntimeVersion", "4.0"));
            root.Add(new XAttribute("ToolsVersion", "4.0"));

            var assemblyNames = Directory.GetFiles(forwardAssembliesFolder, "*.cs").Select(Path.GetFileNameWithoutExtension).ToList();
            assemblyNames.Add("Dot42");
            foreach (var iterator in assemblyNames)
            {
                var name = (iterator == "corlib") ? "mscorlib" : iterator;
                var element = new XElement("File",
                    new XAttribute("AssemblyName", name),
                    new XAttribute("Version", assemblyVersion),
                    new XAttribute("Culture", "neutral"),
                    new XAttribute("ProcessorArchitecture", "MSIL"),
                    new XAttribute("InGac", "false"));

                if (publicKeyToken != null)
                    element.Add(new XAttribute("PublicKeyToken", publicKeyToken));

                root.Add(element);
            }

            var path = Path.Combine(folder, "FrameworkList.xml");
            if (File.Exists(path))
                File.Delete(path);
            doc.Save(path);
        }
    }
}
