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
    /// Read-only APK file, only opens the file when needed
    /// 
    /// TODO: think of a better name
    /// </summary>
    public class ApkFileOpenOnAccessOnly : IApkFile
    {
        private string path;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ApkFileOpenOnAccessOnly(string path)
        {
            this.path = path;
            // test opening
            using (new ZipFile(path))
            {
            }
        }

        /// <summary>
        /// Gets the names of all file stored in this APK.
        /// </summary>
        public IEnumerable<string> FileNames
        {
            get
            {
                using (var file = new ZipFile(path))
                    return file.Cast<ZipEntry>().Where(x => x.IsFile).Select(x => x.Name).ToList();
            }
        }

        /// <summary>
        /// Load the data for the given filename.
        /// </summary>
        public byte[] Load(string fileName)
        {
            using (var file = new ZipFile(path))
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
        }

        /// <summary>
        /// Load AndroidManifest.xml
        /// </summary>
        public AndroidManifest Manifest
        {
            get
            {
                var data = Load("AndroidManifest.xml");
                return new AndroidManifest(new XmlTree(new MemoryStream(data)).AsXml().Root);
            }
        }

        /// <summary>
        /// Load a resources table from the given apk.
        /// </summary>
        public Table Resources
        {
            get
            {
                var data = Load("resources.arsc");
                return (data != null) ? new Table(new MemoryStream(data)) : new Table(Manifest.PackageName);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }
    }
}
