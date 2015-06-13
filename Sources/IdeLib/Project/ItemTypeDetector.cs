using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Xml;
using System.Xml.Linq;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Project
{
    public static class ItemTypeDetector
    {
        /// <summary>
        /// Try to detect the type of item based on the given file.
        /// </summary>
        public static bool TryDetectItemType(string path, string frameworkFolder, out string itemType)
        {
            itemType = null;
            var ext = ConfigurationQualifiers.GetExtension(path) ?? string.Empty;

            switch (ext.ToLower())
            {
                case ".gif":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".9.png":
                    itemType = Dot42Constants.ItemTypeDrawableResource;
                    return true;
                case ".axml":
                    itemType = Dot42Constants.ItemTypeLayoutResource;
                    return true;
                case ".xml":
                    return TryDetectXmlItemType(path, frameworkFolder, out itemType);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to detect the type of item based on the given file.
        /// </summary>
        private static bool TryDetectXmlItemType(string path, string frameworkFolder, out string itemType)
        {
            // try detection based on the base;
            if (TryDetectionItemTypeFromPath(path, out itemType))
                return true;

            // Try loading the file.
            try
            {
                var doc = XDocument.Load(path); // todo: why load the whole document, when we are 
                                                // only interested in the root  node?
                var root = doc.Root;
                if (root == null)
                    return false;

                switch (root.Name.LocalName)
                {
                    case "manifest":
                        itemType = Dot42Constants.ItemTypeManifestResource;
                        break;
                    case "ViewGroup":
                    case "View":
                    case "AbsoluteLayout":
                    case "FrameLayout":
                    case "GridLayout":
                    case "LinearLayout":
                    case "RelativeLayout":
                    case "merge":
                        itemType = Dot42Constants.ItemTypeLayoutResource;
                        break;
                    case "bitmap":
                    case "clip":
                    case "nine-patch":
                    case "inset":
                    case "layer-list":
                    case "animation-list":
                    case "level-list":
                    case "selector":
                    case "scale":
                    case "shape":
                    case "transition":
                        itemType = Dot42Constants.ItemTypeDrawableResource;
                        break;
                    case "menu":
                        itemType = Dot42Constants.ItemTypeMenuResource;
                        break;
                    case "resources":
                        itemType = Dot42Constants.ItemTypeValuesResource;
                        break;
                    case "set":
                        itemType = Dot42Constants.ItemTypeAnimationResource;
                        break;
                    default:
                        TryDetectLayoutXmlItemType(root, frameworkFolder, out itemType);
                        break;
                }
                return (itemType != null);
            }
            catch
            {
                // Ignore
                return false;
            }
        }

        private static bool TryDetectionItemTypeFromPath(string path, out string itemType)
        {
            itemType = null;
            var dir = Path.GetFileName(Path.GetDirectoryName(path));
            if (dir == null) return false;

            dir = dir.Split(new [] {'-'}, 2)[0]; // remove possible configuration specifier.
            dir = dir.ToUpperInvariant();
            switch (dir)
            {
                case "ANIMATION":
                    itemType = Dot42Constants.ItemTypeAnimationResource;
                    return true;
                case "DRAWABLE":
                    itemType = Dot42Constants.ItemTypeDrawableResource;
                    return true;
                case "LAYOUT":
                    itemType = Dot42Constants.ItemTypeLayoutResource;
                    return true;
                case "MENU":
                    itemType = Dot42Constants.ItemTypeMenuResource;
                    return true;
                case "VALUES":
                    itemType = Dot42Constants.ItemTypeValuesResource;
                    return true;
                // Don't detect 'xml' as it is to common a name.
                //case "XML":  
                //    itemType = Dot42Constants.ItemTypeXmlResource;
                //    return true;
                case "RAW":
                    itemType = Dot42Constants.ItemTypeRawResource;
                    return true;
                default:
                    return false;
            }

        }
        

        /// <summary>
        /// Try to detect layout files.
        /// </summary>
        private static bool TryDetectLayoutXmlItemType(XElement root, string frameworkFolder, out string itemType)
        {
            var descriptorProviderSet = Descriptors.Descriptors.GetDescriptorProviderSet(frameworkFolder);
            var descriptors = descriptorProviderSet.LayoutDescriptors;
            if (descriptors.RootDescriptors.Any(x => x.Name == root.Name.LocalName))
            {
                // It's a layout descriptor
                itemType = Dot42Constants.ItemTypeLayoutResource;
                return true;
            }
            itemType = null;
            return false;
        }
    }
}
