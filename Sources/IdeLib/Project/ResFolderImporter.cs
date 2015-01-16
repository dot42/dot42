using System;
using System.IO;
using Dot42.ResourcesLib;
using Dot42.Utility;

namespace Dot42.Ide.Project
{
    public static class ResFolderImporter
    {
        /// <summary>
        /// Import an existing res folder into the project.
        /// </summary>
        public static void ImportResFolder(string resFolderPath, string projectFolder, Action<string, string> addFileToProject)
        {
            resFolderPath = Path.GetFullPath(resFolderPath);
            projectFolder = Path.GetFullPath(projectFolder);

            if (resFolderPath.StartsWith(projectFolder, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(string.Format("Folder ({0}) cannot be inside the project folder.", resFolderPath));
            if (!Directory.Exists(resFolderPath))
                throw new ArgumentException(string.Format("Folder ({0}) not found.", resFolderPath));

            // Copy all files
            foreach (var path in Directory.GetFiles(resFolderPath, "*.*", SearchOption.AllDirectories))
            {
                // Get target location
                string itemType;
                string targetPath;
                if (!TryGetTargetPath(resFolderPath, Path.Combine(projectFolder, "Resources"), path, out targetPath, out itemType))
                    continue;

                // Make sure target folder exists
                var targetFolder = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetFolder) && !Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                // Copy file (when needed)
                if (!File.Exists(targetPath))
                {
                    File.Copy(path, targetPath);
                    File.SetAttributes(targetPath, FileAttributes.Normal);
                }

                // Add to project
                var relTargetPath = targetPath.MakeRelativeTo(projectFolder);
                addFileToProject(relTargetPath, itemType);
            }
        }

        /// <summary>
        /// A file with the given path has been added to the project.
        /// See if we have to rename it.
        /// </summary>
        public static bool TryGetTargetPath(string sourceResFolder, string targetResFolder, string filePath, out string targetPath, out string itemType)
        {
            var relPath = filePath.MakeRelativeTo(sourceResFolder);

            // If file has a configuration in it's parent directory, rename the file itself.
            var folder = Path.GetDirectoryName(relPath);
            if (folder == null)
            {
                targetPath = null;
                itemType = null;
                return false;
            }

            var folderName = Path.GetFileName(folder);
            if ((folderName != null) && (folderName.IndexOf('-') > 0))
            {
                var index = folderName.IndexOf('-');
                var folderPrefix = folderName.Substring(0, index);
                var config = folderName.Substring(index);

                var name = ConfigurationQualifiers.GetFileNameWithoutExtension(relPath);
                var ext = ConfigurationQualifiers.GetExtension(relPath);

                var newFolder = Path.Combine(targetResFolder, folderPrefix);
                targetPath = Path.Combine(newFolder, name + config + ext);
            }
            else
            {
                // No need to modify the name
                targetPath = Path.Combine(targetResFolder, relPath);
            }

            // Now get the item type.
            return TryGetItemType(folderName, out itemType);
        }

        /// <summary>
        /// Try to get the item type for the given folder name.
        /// </summary>
        private static bool TryGetItemType(string folderName, out string itemType)
        {
            var index = folderName.IndexOf('-');
            if (index > 0) folderName = folderName.Substring(0, index);

            switch (folderName.ToLowerInvariant())
            {
                case "anim":
                case "animator":
                    itemType = Dot42Constants.ItemTypeAnimationResource;
                    break;
                case "drawable":
                    itemType = Dot42Constants.ItemTypeDrawableResource;
                    break;
                case "layout":
                    itemType = Dot42Constants.ItemTypeLayoutResource;
                    break;
                case "menu":
                    itemType = Dot42Constants.ItemTypeMenuResource;
                    break;
                case "values":
                    itemType = Dot42Constants.ItemTypeValuesResource;
                    break;
                case "raw":
                    itemType = Dot42Constants.ItemTypeRawResource;
                    break;
                case "xml":
                    itemType = Dot42Constants.ItemTypeXmlResource;
                    break;
                default:
                    itemType = null;
                    return false;
            }
            return true;
        }
    }
}
