using System.Drawing;
using System.Windows.Media;

namespace Dot42.Graphics
{
    public static class Icons32
    {
        public static readonly Bitmap Antenna = RawIcons32.antenna;
        public static readonly Bitmap ArrowRight = RawIcons32.arrow_right;
        public static readonly Bitmap Blog = RawIcons32.blog;
        public static readonly Bitmap Check = RawIcons32.check;
        public static readonly Bitmap CodesOfLaw = RawIcons32.codes_of_law;
        public static readonly Bitmap Download = RawIcons32.download;
        public static Bitmap EmulatorNew = RawIcons32.emulator_new/*, BackgroundColors.EmulatorColor*/;
        public static Bitmap EmulatorPlay = RawIcons32.emulator_play/*, BackgroundColors.EmulatorColor*/;
        public static Bitmap EmulatorDelete = RawIcons32.emulator_delete/*, BackgroundColors.EmulatorColor*/;
        public static readonly Bitmap FolderView = RawIcons32.folder_view;
        public static readonly Bitmap PlayStore = RawIcons32.Google_Play_Store;
        public static readonly Bitmap HelpEarth = RawIcons32.help_earth;
        public static readonly Bitmap History = RawIcons32.history;
        public static readonly Bitmap Information = RawIcons32.information;
        public static readonly Bitmap Key = RawIcons32.key;
        public static readonly Bitmap MagicWand = RawIcons32.magic_wand2;
        public static Bitmap MediaPlay = RawIcons32.media_play;
        public static Bitmap MediaPlayGreen = RawIcons32.media_play_green;
        public static Bitmap MediaPause = RawIcons32.media_pause;
        public static Bitmap PackageAdd = RawIcons32.package_add;
        public static readonly Bitmap PlugUsb = RawIcons32.plug_usb;
        public static readonly Bitmap Refresh = RawIcons32.refresh;
        public static readonly Bitmap ShoppingCardEmpty = RawIcons32.shopping_cart_empty;
    }

    public static class WpfIcons32
    {
        public static readonly ImageSource ArrowRight = BackgroundColors.ConvertToBitmapSource(Icons32.ArrowRight);        
    }
}
