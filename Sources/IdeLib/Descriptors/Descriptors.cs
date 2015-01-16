using System;
using System.Collections.Generic;
using System.IO;
using Dot42.ApkLib;
using Dot42.ApkLib.Resources;
using TallComponents.Common.Util;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Containing for platform specific descriptor provider sets.
    /// </summary>
    public static class Descriptors
    {
        private static readonly object providerSetsLock = new object();
        private static readonly Dictionary<string, DescriptorProviderSet> providerSets = new Dictionary<string, DescriptorProviderSet>();

        /// <summary>
        /// Gets a descriptor provider set for a given framework folder.
        /// </summary>
        public static DescriptorProviderSet GetDescriptorProviderSet(string frameworkFolder)
        {
            DescriptorProviderSet result;
            lock (providerSetsLock)
            {
                if (providerSets.TryGetValue(frameworkFolder, out result))
                    return result;
            }

            try
            {
                var apk = LoadBaseApk(frameworkFolder);
                result = new DescriptorProviderSet(CreateAttrXmlParser(apk), CreateLayoutXmlParser(apk),
                                                   CreateResourcesArsc(apk));
                lock (providerSetsLock)
                {
                    providerSets[frameworkFolder] = result;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
                throw;
            }
        }

        /// <summary>
        /// Try to load base.apk in the given framework folder.
        /// </summary>
        private static ApkFile LoadBaseApk(string frameworkFolder)
        {
            var path = Path.Combine(frameworkFolder, "base.apk");
            if (!File.Exists(path))
                return null;

            return new ApkFile(path);
        }

        /// <summary>
        /// Create an attrs.xml parser for the data in the given framework folder.
        /// </summary>
        private static AttrsXmlParser CreateAttrXmlParser(ApkFile apkFile)
        {
            var data = (apkFile != null) ? apkFile.Load("attrs.xml") : null;
            if (data == null)
                return new AttrsXmlParser(null);
            return new AttrsXmlParser(new MemoryStream(data));
        }

        /// <summary>
        /// Create an layout.xml parser for the data in the given framework folder.
        /// </summary>
        private static LayoutXmlParser CreateLayoutXmlParser(ApkFile apkFile)
        {
            var data = (apkFile != null) ? apkFile.Load("layout.xml") : null;
            if (data == null)
                return new LayoutXmlParser(null);
            return new LayoutXmlParser(new MemoryStream(data));
        }

        /// <summary>
        /// Load a resources table from the given apk.
        /// </summary>
        private static Table CreateResourcesArsc(ApkFile apkFile)
        {
            var data = (apkFile != null) ? apkFile.Load("resources.arsc") : null;
            if (data == null)
                return null;
            return new Table(new MemoryStream(data));
        }
    }
}
