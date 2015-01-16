using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.ApkLib;
using Dot42.ResourcesLib;

namespace Dot42.Compiler.BarBuilder
{
    internal static class IconBuilder
    {
        private const int MaxWidthHeight = 114;

        internal static byte[] GetIcon(string resourceId, ApkFile apk)
        {
            // Look for the files
            var iconFileNames = apk.FileNames.Where(x => MatchesWith(x, resourceId)).ToList();

            // Load all variations
            var icons = iconFileNames.Select(x => LoadBitmap(apk, x)).Where(x => x != null).OrderByDescending(x => Math.Max(x.Width, x.Height)).ToList();

            var icon = icons.FirstOrDefault(x => (x.Height <= MaxWidthHeight) && (x.Width <= MaxWidthHeight));
            if (icon == null)
                return null; // Try scaling

            // Return icon
            var stream = new MemoryStream();
            icon.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        private static bool MatchesWith(string fileName, string id)
        {
            if (!fileName.StartsWith("res/drawable"))
                return false;
            var name = Path.GetFileNameWithoutExtension(fileName);
            return (name == id);
        }

        /// <summary>
        /// Try to load a bitmap from file.
        /// Returns null when that was not possible.
        /// </summary>
        private static Image LoadBitmap(ApkFile apk, string fileName)
        {
            var data = apk.Load(fileName);
            if (data == null)
                return null;
            return Image.FromStream(new MemoryStream(data));
        }
    }
}
