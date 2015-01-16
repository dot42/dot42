using System.Drawing;
using Dot42.Shared.UI;

namespace Dot42.DeviceLib.UI
{
    /// <summary>
    /// Form showing log messages.
    /// </summary>
    public partial class LogCatForm : AppForm
    {
        private readonly IDevice device;

        /// <summary>
        /// Designer ctor
        /// </summary>
        public LogCatForm()
            : this(null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public LogCatForm(IDevice device)
        {
            this.device = device;
            InitializeComponent();
            UserPreferences.Preferences.AttachLogCatWindow(this, new Size(800, 600));
        }

        /// <summary>
        /// Start showing messages
        /// </summary>
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            if (device != null)
            {
                logCatControl.Run(device);
            }
        }
    }
}
