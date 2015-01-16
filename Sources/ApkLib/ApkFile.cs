using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.ApkLib.Manifest;
using Dot42.ApkLib.Resources;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.ApkLib
{
    /// <summary>
    /// Read-only APK file.
    /// </summary>
    public class ApkFile : IDisposable
    {
        private readonly ZipFile file;
        private AndroidManifest manifest;
        private Table resources;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ApkFile(string path)
        {
            file = new ZipFile(path);
        }

        /// <summary>
        /// Gets the names of all file stored in this APK.
        /// </summary>
        public IEnumerable<string> FileNames
        {
            get { return file.Cast<ZipEntry>().Where(x => x.IsFile).Select(x => x.Name); }
        }

        /// <summary>
        /// Load the data for the given filename.
        /// </summary>
        public byte[] Load(string fileName)
        {
            var entry = file.GetEntry(fileName);
            if (entry == null)
                return null;
            using (var stream = file.GetInputStream(entry))
            {
                var data = new byte[entry.Size];
                stream.Read(data, 0, data.Length);
                return data;
            }
        }

        /// <summary>
        /// Load AndroidManifest.xml
        /// </summary>
        public AndroidManifest Manifest
        {
            get
            {
                if (manifest == null)
                {
                    var data = Load("AndroidManifest.xml");
                    manifest = new AndroidManifest(new XmlTree(new MemoryStream(data)).AsXml().Root);
                }
                return manifest;
            }
        }

        /// <summary>
        /// Load a resources table from the given apk.
        /// </summary>
        public Table Resources
        {
            get
            {
                if (resources == null)
                {
                    var data = Load("resources.arsc");
                    resources = (data != null) ? new Table(new MemoryStream(data)) : new Table(Manifest.PackageName);
                }
                return resources;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ((IDisposable)file).Dispose();
        }
    }
}
