using System;
using Dot42.AdbLib;
using Dot42.Shared.UI;
using Dot42.Utility;

namespace Dot42.DeviceLib.UI
{
    public class ConnectingTcpipControl : ProgressControl
    {
        private readonly string host;
        private readonly string port;
        private DeviceMonitor deviceMonitor;
        private bool connecting;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConnectingTcpipControl(string host, string port)
            : base(string.Format(Properties.Resources.ConnectingToX, host))
        {
            this.host = host;
            this.port = port;
            CancelButtonVisible = true;
            InitializeComponent();
            deviceMonitor.DeviceAdded += OnDeviceAdded;
        }

        /// <summary>
        /// Called when a new device was added.
        /// </summary>
        private void OnDeviceAdded(object sender, EventArgs<AndroidDevice> eventArgs)
        {
            if (connecting)
            {
                if (eventArgs.Data.Serial.Contains(host))
                {
                    AutoClose();
                }
            }
        }

        /// <summary>
        /// Automatically close the form.
        /// </summary>
        private void AutoClose()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(AutoClose));
            }
            else
            {
                OnClose();
            }
        }

        /// <summary>
        /// Perform the action.
        /// </summary>
        protected override void DoWork()
        {
            // Listen for devices
            deviceMonitor.Start();
            deviceMonitor.WaitForInitialUpdate();

            // Perform the connect
            connecting = true;
            var adb = new Adb { Logger = LogOutput };
            adb.Connect(host, port, Adb.Timeout.Connect);
        }

        /// <summary>
        /// Call in the GUI thread when an error has occurred in <see cref="ProgressControl.DoWork"/>.
        /// </summary>
        protected override void ShowError(Exception error)
        {
            LogOutput(string.Format("Failed to connect to {0} because: {1}.", host, error.Message));
#if DEBUG
            CopyLogToClipboard();
#endif
        }

        /// <summary>
        /// Worker has finished.
        /// </summary>
        protected override void OnDone()
        {
            ProgressBarVisible = false;
            CancelButtonVisible = false;
            CloseButtonVisible = true;
            FocusCloseButton();
        }

        private void InitializeComponent()
        {
            this.deviceMonitor = new Dot42.AdbLib.DeviceMonitor();
            this.SuspendLayout();
            // 
            // deviceMonitor
            // 
            this.deviceMonitor.Enabled = false;
            // 
            // ConnectingTcpipControl
            // 
            this.CancelButtonVisible = true;
            this.CloseButtonVisible = true;
            this.Name = "ConnectingTcpipControl";
            this.ResumeLayout(false);

        }
    }
}
