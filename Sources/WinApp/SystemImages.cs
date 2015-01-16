using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.Gui
{
    /// <summary>
    /// Locate all installed system images
    /// </summary>
    internal class SystemImages : IEnumerable<SystemImage>
    {
        private readonly List<SystemImage> images = new List<SystemImage>();

        /// <summary>
        /// Single instance
        /// </summary>
        public static readonly SystemImages Instance = new SystemImages();

        /// <summary>
        /// Default ctor
        /// </summary>
        private SystemImages()
        {
            Reload();
        }

        /// <summary>
        /// Reload all image descriptors
        /// </summary>
        private void Reload()
        {
            images.Clear();
            var root = Locations.SystemImages;
            if (Directory.Exists(root))
            {
                var subFolders = Directory.GetDirectories(root);
                images.AddRange(subFolders.SelectMany(Directory.GetDirectories).Select(SystemImage.TryLoad).Where(x => x != null).OrderByDescending(x => x.ApiLevel));
            }            
        }

        /// <summary>
        /// Install the given system image.
        /// </summary>
        public void Install(SdkSystemImage image, string archivePath)
        {
            var root = Locations.SystemImages;
            var folder = Path.Combine(root, string.Format("android-{0}", image.ApiLevel));
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var archive = new ZipFile(archivePath);
            foreach (var entry in archive.Cast<ZipEntry>().Where(x => x.IsFile))
            {
                var path = Path.GetFullPath(Path.Combine(folder, entry.Name));
                var entryFolder = Path.GetDirectoryName(path);
                if (!Directory.Exists(entryFolder))
                    Directory.CreateDirectory(entryFolder);
                using (var stream = archive.GetInputStream(entry))
                {
                    using (var fstream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fstream);
                    }
                }
            }

            // Reload now
            Reload();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<SystemImage> GetEnumerator()
        {
            return images.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
