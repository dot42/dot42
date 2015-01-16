using System.Drawing;
using System.Windows.Media;

namespace Dot42.Graphics
{
    public static class Icons16
    {
        public static readonly Image EmulatorDelete = RawIcons16.emulator_delete;
        public static readonly Image EmulatorEdit = RawIcons16.emulator_edit;
        public static readonly Image MediaPause = RawIcons16.media_pause;
        public static readonly Image MediaPauseDisabled = RawIcons16.media_pause_disabled;
        public static readonly Image Folder = RawIcons16.folder_16;
        public static readonly Image CertificateNew = RawIcons16.certificate_new_16;
        public static readonly Image Unknown = RawIcons16.unknown;
    }

    public static class WpfIcons16
    {
        public static readonly ImageSource ArrowDown = BackgroundColors.ConvertToBitmapSource(RawIcons16.arrow_down_green);
        public static readonly ImageSource ArrowLeft = BackgroundColors.ConvertToBitmapSource(RawIcons16.arrow_left_green);
        public static readonly ImageSource ArrowRight = BackgroundColors.ConvertToBitmapSource(RawIcons16.arrow_right_green);
        public static readonly ImageSource ArrowUp = BackgroundColors.ConvertToBitmapSource(RawIcons16.arrow_up_green);
    }
}
