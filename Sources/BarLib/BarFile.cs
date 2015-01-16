using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.BarLib
{
    /// <summary>
    /// Read-only BAR file.
    /// </summary>
    public class BarFile : IDisposable
    {
        private readonly ZipFile file;
        private Manifest manifest;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BarFile(string path)
        {
            file = new ZipFile(path);
        }

        /// <summary>
        /// Gets the names of all file stored in this BAR.
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
        /// Gets the manifest from the file.
        /// </summary>
        public Manifest Manifest
        {
            get
            {
                if (manifest == null)
                {
                    var data = Load(@"META-INF/MANIFEST.MF");
                    data = data ?? Load(@"META-INF\MANIFEST.MF");
                    data = data ?? new byte[0];
                    manifest = new Manifest(new MemoryStream(data));
                }
                return manifest;
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
