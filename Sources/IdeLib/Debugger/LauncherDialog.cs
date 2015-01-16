using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Dot42.Graphics;
using TallComponents.Common.Extensions;

namespace Dot42.Ide.Debugger
{
    /// <summary>
    /// Dialog used to show during launching of APK's.
    /// </summary>
    public sealed partial class LauncherDialog : Form
    {
        private readonly bool debug;

        /// <summary>
        /// Fired when the cancel button is clicked.
        /// </summary>
        public event EventHandler Cancel;

        private bool closing;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LauncherDialog(string apkPath, bool debug)
        {
            this.debug = debug;
            InitializeComponent();
            Text = string.Format(Text, Path.GetFileName(apkPath));
            DoSetState(LauncherStates.Deploying, string.Empty);
        }

        /// <summary>
        /// Update the state in the ui.
        /// This can be called on any thread.
        /// </summary>
        public void SetState(LauncherStates state, string error)
        {
            if (closing || IsDisposed)
                return;
            if (InvokeRequired)
            {
                Invoke(new Action<LauncherStates, string>(DoSetState), state, error);
            }
            else
            {
                DoSetState(state, error);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data. </param>
        protected override void OnClosing(CancelEventArgs e)
        {
            closing = true;
            base.OnClosing(e);
        }

        /// <summary>
        /// Update the state in the ui.
        /// This can be called only on the gui thread.
        /// </summary>
        private void DoSetState(LauncherStates state, string error)
        {
            switch (state)
            {
                case LauncherStates.Deploying:
                    pbDeploy.Image = Icons32.ArrowRight;
                    pbStart.Image = Icons16.MediaPause;
                    pbAttach.Image = debug ? Icons16.MediaPause : Icons16.MediaPauseDisabled;
                    break;
                case LauncherStates.Starting:
                    pbDeploy.Image = Icons32.Check;
                    pbStart.Image = Icons32.ArrowRight;
                    pbAttach.Image = debug ? Icons16.MediaPause : Icons16.MediaPauseDisabled;
                    break;
                case LauncherStates.Started:
                    pbDeploy.Image = Icons32.Check;
                    pbStart.Image = Icons32.Check;
                    pbAttach.Image = debug ? Icons16.MediaPause : Icons16.MediaPauseDisabled;
                    if (!debug) Close();
                    break;
                case LauncherStates.Attaching:
                    pbDeploy.Image = Icons32.Check;
                    pbStart.Image = Icons32.Check;
                    pbAttach.Image = Icons32.ArrowRight;
                    break;
                case LauncherStates.Attached:
                    pbDeploy.Image = Icons32.Check;
                    pbStart.Image = Icons32.Check;
                    pbAttach.Image = Icons32.Check;
                    Close();
                    break;
                case LauncherStates.Error:
                    pbDeploy.Image = Icons32.Check;
                    pbStart.Image = Icons32.Check;
                    pbAttach.Image = Icons32.Check;
                    MessageBox.Show("A launch error occurred: " + error, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    break;
            }
        }

        /// <summary>
        /// Cancel button was clicked.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            Launcher.CancelLaunch();
            closing = true;
            Cancel.Fire(this);
            Close();
        }
    }
}
