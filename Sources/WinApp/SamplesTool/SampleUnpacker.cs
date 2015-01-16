using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.Gui.SamplesTool
{
    /// <summary>
    /// Unzip all samples
    /// </summary>
    internal static class SampleUnpacker
    {
        /// <summary>
        /// Unzip all files.
        /// </summary>
        /// <returns>True on succes, false otherwise</returns>
        internal static bool Unzip(string sampleFolder, string zipFilePath, Action<string> log, Func<bool> updateSampleProjects)
        {
            var messages = new List<string>();
            var entries = new List<UnzipEntry>();
            var fileHashes = new FileHashesSet();
            var backupFolders = new BackupFolders();
            var result = true;
            using (var zipFile = new ZipFile(zipFilePath))
            {
                // Collect entries
                foreach (ZipEntry zipEntry in zipFile)
                {
                    entries.Add(new UnzipEntry(zipEntry, sampleFolder));
                }

                // Unzip all
                foreach (var entry in entries)
                {
                    log(string.Format("Unpacking {0}", entry.Name));
                    entry.UnzipToTempPath(zipFile);
                }

                // Backup if needed
                foreach (var entry in entries)
                {
                    entry.BackupIfNeeded(fileHashes, backupFolders);
                }

                // Move all to final target
                foreach (var entry in entries)
                {
                    entry.MoveToTarget(messages);
                }

                // Update all sample projects
                if (!updateSampleProjects())
                    result = false;

                // Update hashes
                foreach (var entry in entries)
                {
                    entry.UpdateHash(fileHashes);
                }

                // Save hashes
                fileHashes.SaveAll();
            }
            return result;
        }

        /// <summary>
        /// Set of file hashes.
        /// </summary>
        private sealed class FileHashesSet
        {
            private readonly Dictionary<string, FileHashes> fileHashes = new Dictionary<string, FileHashes>();

            /// <summary>
            /// Get or create.
            /// </summary>
            public FileHashes Get(string folder)
            {
                FileHashes result;
                folder = Path.GetFullPath(folder);
                if (!fileHashes.TryGetValue(folder, out result))
                {
                    result = new FileHashes(folder);
                    fileHashes[folder] = result;
                }
                return result;
            }

            /// <summary>
            /// Save all files.
            /// </summary>
            public void SaveAll()
            {
                foreach (var x in fileHashes.Values)
                {
                    x.Save();
                }
            }
        }

        /// <summary>
        /// Set of backup folders.
        /// </summary>
        private sealed class BackupFolders
        {
            private readonly Dictionary<string, string> folders = new Dictionary<string, string>();

            /// <summary>
            /// Get or create the backup folder for the given folder.
            /// </summary>
            public string Get(string folder)
            {
                string result;
                folder = Path.GetFullPath(folder);
                if (!folders.TryGetValue(folder, out result))
                {
                    result = CreateBackupFolder(folder);
                    folders[folder] = result;
                }
                return result;
            }

            /// <summary>
            /// Create a backup file path for the given path.
            /// </summary>
            private static string CreateBackupFolder(string folder)
            {
                var index = 1;
                while (true)
                {
                    var backupFolder = Path.Combine(folder, string.Format("Backup{0}", index));
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                        return backupFolder;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Containing for data in .files file.
        /// </summary>
        private sealed class FileHashes
        {
            private readonly string path;
            private readonly Dictionary<string, string> hashes = new Dictionary<string, string>();

            /// <summary>
            /// Default ctor
            /// </summary>
            public FileHashes(string folder)
            {
                path = Path.Combine(folder, ".files");
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        var index = line.IndexOf(':');
                        if (index < 0)
                            continue;
                        var key = line.Substring(0, index).Trim();
                        var hash = line.Substring(index + 1).Trim();
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(hash))
                            hashes[key] = hash;
                    }
                }
            }

            /// <summary>
            /// Add an entry
            /// </summary>
            public void Add(string fileName, string hash)
            {
                var key = Path.GetFileName(fileName);
                hashes[key] = hash;
            }

            /// <summary>
            /// Try to get a hash for a given filename.
            /// </summary>
            public bool TryGetHash(string fileName, out string hash)
            {
                var key = Path.GetFileName(fileName);
                return hashes.TryGetValue(key, out hash);
            }

            /// <summary>
            /// Save to disk.
            /// </summary>
            public void Save()
            {
                var folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllLines(path, hashes.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));
            }
        }

        /// <summary>
        /// Single zip entry
        /// </summary>
        private sealed class UnzipEntry
        {
            private readonly ZipEntry entry;
            private readonly string targetPath;
            private string tempPath;

            /// <summary>
            /// Default ctor
            /// </summary>
            public UnzipEntry(ZipEntry entry, string targetFolder)
            {
                this.entry = entry;
                targetPath = Path.Combine(targetFolder, entry.Name);
                var folder = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            public string Name { get { return entry.Name; } }

            /// <summary>
            /// Unzip the entry to the temporary path.
            /// </summary>
            public void UnzipToTempPath(ZipFile zipFile)
            {
                tempPath = CreateTempPath(Path.GetDirectoryName(targetPath));
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    using (var source = zipFile.GetInputStream(entry))
                    {
                        var buffer = new byte[64 * 1024];
                        int len;
                        while ((len = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            stream.Write(buffer, 0, len);
                        }
                    }
                }
            }

            /// <summary>
            /// Backup the target path if needed.
            /// </summary>
            public void BackupIfNeeded(FileHashesSet fileHashes, BackupFolders backupFolders)
            {
                if (!File.Exists(targetPath))
                    return;
                var hashes = fileHashes.Get(Path.GetDirectoryName(targetPath));
                string originalHash;
                if (hashes.TryGetHash(targetPath, out originalHash))
                {
                    // Compare with existing
                    var currentHash = Hash(targetPath);
                    if (currentHash == originalHash)
                        return; // No need to backup
                }
                var backupFolder = backupFolders.Get(Path.GetDirectoryName(targetPath));
                Directory.CreateDirectory(backupFolder);
                File.Copy(targetPath, Path.Combine(backupFolder, Path.GetFileName(targetPath)));
            }

            /// <summary>
            /// Move the temp file to the target file.
            /// </summary>
            public void MoveToTarget(List<string> messages)
            {
                if (!DeleteTarget())
                {
                    // Failed to delete, try to move
                    if (!MoveTarget())
                    {
                        // Give up
                        messages.Add(string.Format("Failed to replace {0}", targetPath));
                        return;
                    }
                }

                // Move temp to target
                try
                {
                    File.Move(tempPath, targetPath);
                }
                catch (Exception ex)
                {
                    messages.Add(string.Format("Failed to move unzipped file to {0} because {1}.", targetPath, ex.Message));
                }
            }

            /// <summary>
            /// Update the hash of the target path.
            /// </summary>
            public void UpdateHash(FileHashesSet fileHashes)
            {
                var hashes = fileHashes.Get(Path.GetDirectoryName(targetPath));
                hashes.Add(targetPath, Hash(targetPath));
            }

            /// <summary>
            /// Try to delege target file.
            /// </summary>
            private bool DeleteTarget()
            {
                var attempts = 10;
                while (attempts > 0)
                {
                    if (!File.Exists(targetPath))
                        return true;
                    try
                    {
                        File.Delete(targetPath);
                        if (!File.Exists(targetPath))
                            return true;
                    }
                    catch
                    {
                        // Ignore
                    }
                    attempts--;
                }
                return false;
            }

            /// <summary>
            /// Try to move target file.
            /// </summary>
            private bool MoveTarget()
            {
                var attempts = 10;
                while (attempts > 0)
                {
                    if (!File.Exists(targetPath))
                        return true;
                    try
                    {
                        var tempPath = CreateTempPath(Path.GetDirectoryName(targetPath));
                        File.Move(targetPath, tempPath);
                        if (!File.Exists(targetPath))
                            return true;
                    }
                    catch
                    {
                        // Ignore
                    }
                    attempts--;
                }
                return false;
            }

            /// <summary>
            /// Create a file with a temporary name in the given folder.
            /// </summary>
            private static string CreateTempPath(string folder)
            {
                var rnd = new Random();
                while (true)
                {
                    var name = string.Format("__$${0}", rnd.Next());
                    var path = Path.Combine(folder, name);
                    if (!File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            /// <summary>
            /// Create a hash of the given path.
            /// </summary>
            private static string Hash(string path)
            {
                var sha1 = SHA1.Create();
                var bytes = File.ReadAllBytes(path);
                var hash = sha1.ComputeHash(bytes);
                return string.Join("", hash.Select(x => x.ToString("X2")));
            }
        }
    }
}
