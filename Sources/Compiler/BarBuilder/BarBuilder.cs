using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dot42.ApkLib;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.Compiler.BarBuilder
{
    /// <summary>
    /// Package into a .bar file.
    /// </summary>
    internal class BarBuilder
    {
        private readonly string path;

        /// <summary>
        /// Build a BAR at the given path.
        /// </summary>
        internal BarBuilder(string path)
        {
            this.path = path;
            Author = Environment.UserName;
        }

        /// <summary>
        /// Path of the APK file.
        /// </summary>
        public string ApkPath { get; set; }

        /// <summary>
        /// Author of the package.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Path of the debug token.
        /// </summary>
        public string DebugTokenPath { get; set; }

        /// <summary>
        /// Compile the given XML file to a binary XML file in the given output folder.
        /// </summary>
        public void Build()
        {
#if DEBUG
            //Debugger.Launch();
#endif

            // Prepare folder
            var outputFolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Sign bar?
            const bool sign = true;

            // Load APK
            var apk = new ApkFile(ApkPath);

            // Collect entries
            var entries = new List<BarEntry>();
            var apkName = Path.GetFileNameWithoutExtension(path) + ".apk";
            entries.Add(new BarEntry(string.Format(@"android\{0}", apkName), ApkPath));

            // Collect icon's
            var apkPath2barPath = new Dictionary<string, string>();
            CollectIcons(apk, entries, apkPath2barPath);

            // Build ZIP
            var manifest = new MetaInfManifestBuilder(apk, Author, DebugTokenPath, apkPath2barPath);

            // Build signatures
            if (sign)
            {
                foreach (var entry in entries)
                {
                    manifest.AddArchiveAsset(entry.Name, entry.Data);
                    //signature.AddSha1Digest(entry.Name, entry.Data);
                }
            }

            // Create zip
            ////string md5FingerPrint = null;
            ////string sha1FingerPrint = null;
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var zipStream = new ZipOutputStream(fileStream) {UseZip64 = UseZip64.Off})
                {
                    zipStream.SetLevel(9);

                    zipStream.PutNextEntry(new ZipEntry("META-INF/MANIFEST.MF") { CompressionMethod = CompressionMethod.Deflated });
                    manifest.WriteTo(zipStream);
                    zipStream.CloseEntry();

                    ////if (sign)
                    ////{
                    ////    zipStream.PutNextEntry(new ZipEntry("META-INF/LE-C0FC2.SF")
                    ////    {CompressionMethod = CompressionMethod.Deflated});
                    ////    signature.WriteTo(zipStream);
                    ////    zipStream.CloseEntry();

                    ////    zipStream.PutNextEntry(new ZipEntry("META-INF/LE-C0FC2.RSA")
                    ////    {CompressionMethod = CompressionMethod.Deflated});
                    ////    rsa.WriteTo(zipStream, out md5FingerPrint, out sha1FingerPrint);
                    ////    zipStream.CloseEntry();
                    ////}

                    foreach (var entry in entries)
                    {
                        var entryName = entry.Name.Replace('\\', '/');
                        zipStream.PutNextEntry(new ZipEntry(entryName) { CompressionMethod = GetCompressionMethod(entryName) });
                        entry.WriteTo(zipStream);
                        zipStream.CloseEntry();
                    }
                }
            }

#if NO_FINGERPRINT_YET
            // Save MD5 fingerprint
            var md5FingerPrintPath = Path.ChangeExtension(path, ".md5");
            File.WriteAllText(md5FingerPrintPath, md5FingerPrint ?? string.Empty);

            // Save SHA1 fingerprint
            var sha1FingerPrintPath = Path.ChangeExtension(path, ".sha1");
            File.WriteAllText(sha1FingerPrintPath, sha1FingerPrint ?? string.Empty);
#endif

#if DEBUG && NO_RSA_YET
            // Create RSA
            using (var fileStream = new FileStream(Path.Combine(outputFolder, "CERT.RSA"), FileMode.Create,
                                                FileAccess.Write))
            {
                rsa.WriteTo(fileStream, out md5FingerPrint, out sha1FingerPrint);
            }
#endif
        }

        /// <summary>
        /// Gets the method for compressing the given entry name.
        /// </summary>
        private static CompressionMethod GetCompressionMethod(string entryName)
        {
            return CompressionMethod.Deflated;            
        }

        /// <summary>
        /// Collect all icons
        /// </summary>
        private static void CollectIcons(ApkFile apk, List<BarEntry> entries, Dictionary<string, string> apkPath2barPath)
        {
#if DEBUG
            Debugger.Launch();
#endif

            var iconNames = apk.Manifest.Activities.Select(x => x.Icon).Where(x => !string.IsNullOrEmpty(x)).ToList();
            var appIcon = apk.Manifest.ApplicationIcon;
            if (!string.IsNullOrEmpty(appIcon))
                iconNames.Add(appIcon);

            foreach (var name in iconNames)
            {
                if (apkPath2barPath.ContainsKey(name))
                    continue;
                int nameAsInt;
                if (!int.TryParse(name, out nameAsInt))
                    continue;
                var resId = apk.Resources.GetResourceIdentifier(nameAsInt);
                var iconData = IconBuilder.GetIcon(resId, apk);
                if (iconData != null)
                {
                    var barPath = "android/res/drawable/" + resId + ".png";
                    var entry = new BarEntry(barPath, new MemoryStream(iconData));
                    entries.Add(entry);
                    apkPath2barPath[name] = barPath;
                }
            }
        }

        private class BarEntry
        {
            private readonly string name;
            private readonly byte[] data;

            /// <summary>
            /// Default ctor
            /// </summary>
            public BarEntry(string name, string path)
                : this(name, File.OpenRead(path))
            {
                
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            public BarEntry(string name, Stream stream)
            {
                this.name = name;
                data  = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                stream.Dispose();
            }

            /// <summary>
            /// Gets the entry name
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// Gets the data of this entry as new stream.
            /// </summary>
            public Stream Data
            {
                get { return new MemoryStream(data); }
            }

            /// <summary>
            /// Write myself to the given stream
            /// </summary>
            public void WriteTo(Stream stream)
            {
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
