using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Mapping;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Cecil;

namespace Dot42.ApkBuilder
{
    /// <summary>
    /// Compile resources to binary variants.
    /// </summary>
    internal class ApkBuilder
    {
        private readonly string path;

        // these formats are already compressed, or don't compress well 
        private static readonly string[] NoCompressExt = new[] {
            ".jpg", ".jpeg", ".png", ".gif",
            ".wav", ".mp2", ".mp3", ".ogg", ".aac",
            ".mpg", ".mpeg", ".mid", ".midi", ".smf", ".jet",
            ".rtttl", ".imy", ".xmf", ".mp4", ".m4a",
            ".m4v", ".3gp", ".3gpp", ".3g2", ".3gpp2",
            ".amr", ".awb", ".wma", ".wmv"
        };

        /// <summary>
        /// Build an APK at the given path.
        /// </summary>
        internal ApkBuilder(string path)
        {
            this.path = path;
            MapFilePath = Path.ChangeExtension(path, ".d42map");
            DexFiles = new List<string>();
            MapFiles = new List<string>();
            Assemblies = new List<string>();
            NativeCodeLibraries = new List<string>();
        }

        /// <summary>
        /// Path of all dex files to include.
        /// </summary>
        public List<string> DexFiles { get; private set; }

        /// <summary>
        /// Path of all dex files to include.
        /// </summary>
        public List<string> Assemblies { get; private set; }

        /// <summary>
        /// Path of all map files to include.
        /// </summary>
        public List<string> MapFiles { get; private set; }

        /// <summary>
        /// Path of output map file.
        /// </summary>
        public string MapFilePath { get; set; }

        /// <summary>
        /// Path of AndroidManifest.xml (compiled) path.
        /// </summary>
        public string ManifestPath { get; set; }

        /// <summary>
        /// Path of compiled and raw resources
        /// </summary>
        public string ResourcesFolder { get; set; }

        /// <summary>
        /// Certificate file used for signing
        /// </summary>
        public string PfxFile { get; set; }

        /// <summary>
        /// Password for certificate file
        /// </summary>
        public string PfxPassword { get; set; }

        /// <summary>
        /// Thumbprint for the signing certificate
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Path for file containing free apps key
        /// </summary>
        public string FreeAppsKeyPath { get; set; }

        /// <summary>
        /// Name of the APK package
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Path of all native code libs to include.
        /// </summary>
        public List<string> NativeCodeLibraries { get; private set; }

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

            // Sign apk?
            const bool sign = true;

            // Collect entries
            var entries = new List<ApkEntry>();
            entries.Add(new ApkEntry("AndroidManifest.xml", ManifestPath));
            entries.AddRange(DexFiles.Select(dexFile => new ApkEntry(Path.GetFileName(dexFile), dexFile)));

            // Collect map files
            var mapFiles = MapFiles.Select(p => new MapFile(p)).ToList();

            // Collect resource files
            var resourceEntries = new List<ApkEntry>();
            if (!string.IsNullOrEmpty(ResourcesFolder))
            {
                CollectResources(resourceEntries, ResourcesFolder, ResourcesFolder);
                entries.AddRange(resourceEntries);
            }

            // Collect embedded resources as assets
            foreach (var assembly in Assemblies)
            {
                var assets = new List<ApkEntry>();
                CollectAssets(assets, assembly);
                entries.AddRange(assets);
            }

            // Collect native code libs
            CollectNativeCodeLibs(entries);

            // Load free apps key (if any)
            string freeAppKey = null;
            if (!string.IsNullOrEmpty(FreeAppsKeyPath))
            {
                freeAppKey = File.ReadAllText(FreeAppsKeyPath);
            }

            // Build ZIP
            var manifest = new MetaInfManifestBuilder();
            var signature = new MetaInfCertSfBuilder(manifest);
            var rsa = new MetaInfCertRsaBuilder(signature, PfxFile, PfxPassword, CertificateThumbprint, freeAppKey, PackageName);

            // Build signatures
            if (sign)
            {
                foreach (var entry in entries)
                {
                    manifest.AddSha1Digest(entry.Name, entry.Data);
                    signature.AddSha1Digest(entry.Name, entry.Data);
                }
            }

            // Create zip
            string md5FingerPrint = null;
            string sha1FingerPrint = null;
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var zipStream = new ZipOutputStream(fileStream) {UseZip64 = UseZip64.Off})
                {
                    zipStream.SetLevel(9);

                    zipStream.PutNextEntry(new ZipEntry("META-INF/MANIFEST.MF")
                    {CompressionMethod = CompressionMethod.Deflated});
                    manifest.WriteTo(zipStream);
                    zipStream.CloseEntry();

                    if (sign)
                    {
                        zipStream.PutNextEntry(new ZipEntry("META-INF/LE-C0FC2.SF")
                        {CompressionMethod = CompressionMethod.Deflated});
                        signature.WriteTo(zipStream);
                        zipStream.CloseEntry();

                        zipStream.PutNextEntry(new ZipEntry("META-INF/LE-C0FC2.RSA")
                        {CompressionMethod = CompressionMethod.Deflated});
                        rsa.WriteTo(zipStream, out md5FingerPrint, out sha1FingerPrint);
                        zipStream.CloseEntry();
                    }

                    foreach (var entry in entries)
                    {
                        var entryName = entry.Name.Replace('\\', '/');
                        zipStream.PutNextEntry(new ZipEntry(entryName) { CompressionMethod = GetCompressionMethod(entryName) });
                        entry.WriteTo(zipStream);
                        zipStream.CloseEntry();
                    }
                }
            }

            // Save MD5 fingerprint
            var md5FingerPrintPath = Path.ChangeExtension(path, ".md5");
            File.WriteAllText(md5FingerPrintPath, md5FingerPrint ?? string.Empty);

            // Save SHA1 fingerprint
            var sha1FingerPrintPath = Path.ChangeExtension(path, ".sha1");
            File.WriteAllText(sha1FingerPrintPath, sha1FingerPrint ?? string.Empty);

            // Merge map files
            var mapFile = MergeMapFiles(mapFiles);
            mapFile.Save(MapFilePath);

#if DEBUG
            // Create RSA
            using (
                var fileStream = new FileStream(Path.Combine(outputFolder, "CERT.RSA"), FileMode.Create,
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
            if (NoCompressExt.Any(entryName.EndsWith))
                return CompressionMethod.Stored;
            return CompressionMethod.Deflated;            
        }

        /// <summary>
        /// Merge the given set of map files into 1.
        /// </summary>
        private static MapFile MergeMapFiles(List<MapFile> mapFiles)
        {
            if (mapFiles.Count == 0) return new MapFile();
            if (mapFiles.Count == 1) return mapFiles[0];

            var result = new MapFile();
            foreach (var entry in mapFiles.SelectMany(mapFile => mapFile))
            {
                result.Add(entry);
            }
            return result;
        }

        /// <summary>
        /// Collect all resources in the given folder.
        /// </summary>
        private static void CollectResources(List<ApkEntry> resourceEntries, string folder, string baseFolder)
        {
            if (!Directory.Exists(folder))
                return;

            var fullBase = Path.GetFullPath(baseFolder);

            // Collect all files
            foreach (var path in Directory.GetFiles(folder))
            {
                var fullPath = Path.GetFullPath(path);
                var relPath = fullPath.Substring(fullBase.Length);
                if (relPath.StartsWith("\\"))
                    relPath = relPath.Substring(1);

                if (relPath.Equals("AndroidManifest.xml", StringComparison.OrdinalIgnoreCase))
                    continue;

                resourceEntries.Add(new ApkEntry(relPath, fullPath));
            }            

            // Collect all sub folders
            foreach (var path in Directory.GetDirectories(folder))
            {
                CollectResources(resourceEntries, path, baseFolder);
            }
        }

        /// <summary>
        /// Collect all embedded resources in the given assembly.
        /// </summary>
        private static void CollectAssets(List<ApkEntry> resourceEntries, string assemblyPath)
        {
            // Load assembly
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

            // Collect all resources
            foreach (var resource in assembly.MainModule.Resources.OfType<EmbeddedResource>())
            {
                var data = resource.GetResourceData();
                var relPath = Path.Combine("assets", resource.Name);
                resourceEntries.Add(new ApkEntry(relPath, new MemoryStream(data)));
            }
        }

        /// <summary>
        /// Collect all native code libraries.
        /// </summary>
        private void CollectNativeCodeLibs(List<ApkEntry> entries)
        {
            foreach (var path in NativeCodeLibraries)
            {
                var data = File.ReadAllBytes(path);
                var fileName = Path.GetFileName(path);
                var abi = Path.GetFileName(Path.GetDirectoryName(path));
                var relPath = Path.Combine("lib", Path.Combine(abi, fileName));
                entries.Add(new ApkEntry(relPath, new MemoryStream(data)));
            }
        }

        private class ApkEntry
        {
            private readonly string name;
            private readonly byte[] data;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ApkEntry(string name, string path)
                : this(name, File.OpenRead(path))
            {
                
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            public ApkEntry(string name, Stream stream)
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
