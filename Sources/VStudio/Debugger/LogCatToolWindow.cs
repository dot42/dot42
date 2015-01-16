using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Show logcat output in tool window
    /// </summary>
    [Guid(GuidList.Strings.guidDot42LogCatToolWindow)]
    public sealed class LogCatToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public LogCatToolWindow()
        {
            Caption = "Device log";
            BitmapResourceID = 301;
            BitmapIndex = 0;

            var ide = Dot42Package.Instance.Ide;
            Content = new LogCatWindowControl(ide);
        }
    }
}
