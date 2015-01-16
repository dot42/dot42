using System;
using DevComponents.DotNetBar;
using Dot42.AvdLib;
using Dot42.Shared.UI;

namespace Dot42.Gui.Controls.Emulator
{
    public sealed class StartAvdControl : ProgressControl, IProgressControl
    {
        private readonly Avd avd;

        /// <summary>
        /// Default ctor
        /// </summary>
        public StartAvdControl(Avd avd)
            : base(string.Format("Starting {0}", avd.Name))
        {
            this.avd = avd;
            CloseButtonVisible = true;
        }

        /// <summary>
        /// Run the process.
        /// </summary>
        protected override void DoWork()
        {
            avd.Start(LogOutput);
        }

        /// <summary>
        /// Call in the GUI thread when an error has occurred in <see cref="ProgressControl.DoWork"/>.
        /// </summary>
        protected override void ShowError(Exception error)
        {
#if DEBUG
            CopyLogToClipboard();
#endif
            var msg = string.Format("Failed to start {0} because: {1}.", avd.Name, error.Message);
            MessageBoxEx.Show(msg);
        }
    }
}
