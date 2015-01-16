using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using DColor = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using MColor = System.Windows.Media.Color;
using System.Globalization;

namespace Dot42.Graphics
{
    /// <summary>
    /// Support for well known (randomly selected) background colors.
    /// </summary>
    public static class BackgroundColors
    {
        private static readonly string[] Colors = new[] {
            "29619B",
            "9AA616",
            "925A24",
            "247A30",
            "C44100",
            "1C8887",
            "912F62",
            "E88E00",
            "685088"
        };
        private static readonly object RndLock = new object();
        private static readonly Random Rnd = new Random(Environment.TickCount);

        internal static readonly DColor EmulatorColor = GetRandomColor();

        public static DColor DGreen = DColorFromRgb(Colors[1]);
        public static DColor DMetroColor = DColorFromRgb("404040");
        public static MColor MGreen = MColorFromRgb(Colors[5]);

        /// <summary>
        /// Get a random background color.
        /// </summary>
        public static DColor GetRandomColor()
        {
            string color;
            lock (RndLock)
            {
                color = Colors[Rnd.Next(Colors.Length)];
            }
            return DColorFromRgb(color);
        }

        /// <summary>
        /// Get a random background color.
        /// </summary>
        public static MColor GetRandomMediaColor()
        {
            string color;
            lock (RndLock)
            {
                color = Colors[Rnd.Next(Colors.Length)];
            }
            return MColorFromRgb(color);
        }

        /// <summary>
        /// Add a background color to the given image.
        /// </summary>
        internal static Bitmap AddBackground(Image image)
        {
            return AddBackground(image, GetRandomColor());
        }

        /// <summary>
        /// Add a background color to the given image.
        /// </summary>
        internal static Bitmap AddBackground(Image image, DColor color)
        {
            var bm = new Bitmap(image.Width, image.Height);
            using (var g = System.Drawing.Graphics.FromImage(bm))
            {
                //g.Clear(color);
                g.DrawImageUnscaled(image, 0, 0);
            }
            return bm;
        }

        /// <summary>
        /// Add a 'disabled' background color to the given image.
        /// </summary>
        internal static Bitmap AddDisabledBackground(Image image)
        {
            return AddBackground(image, DColor.Silver);
        }

        internal static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap wfBitmap)
        {
            /*var hBitmap = gdiPlusBitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());*/

            // WPF version of Image
            var bi3 = new BitmapImage();

            bi3.BeginInit();

            // Save to a memory stream...
            var ms = new MemoryStream();
            wfBitmap.Save(ms, ImageFormat.Png);

            // Rewind the stream...
            ms.Seek(0, SeekOrigin.Begin);

            // Tell the WPF image to use this stream...
            bi3.StreamSource = ms;
            bi3.EndInit();

            return bi3;
        }

        private static DColor DColorFromRgb(string color)
        {
            var r = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(color.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(color.Substring(4, 2), NumberStyles.HexNumber);
            return DColor.FromArgb(r, g, b);
        }

        private static MColor MColorFromRgb(string color)
        {
            var r = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(color.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(color.Substring(4, 2), NumberStyles.HexNumber);
            return MColor.FromRgb((byte)r, (byte)g, (byte)b);
        }
    }
}
