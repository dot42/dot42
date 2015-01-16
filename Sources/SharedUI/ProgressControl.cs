using System;
using System.Windows.Forms;
using TallComponents.Common.Extensions;
using TallComponents.Common.Util;

namespace Dot42.Shared.UI
{
    public partial class ProgressControl : Panel
    {
        /// <summary>
        /// Fired when the activity has completed
        /// </summary>
        public event EventHandler Done;

        /// <summary>
        /// Fired when the close button has been pressed.
        /// </summary>
        public event EventHandler Close;

        /// <summary>
        /// Fired when the cancel button has been pressed.
        /// </summary>
        public event EventHandler Cancel;

        private bool started;
        
        /// <summary>
        /// Designer ctor
        /// </summary>
        [Obsolete]
        public ProgressControl() : this("Starting...")
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProgressControl(string title)
        {
            InitializeComponent();
            lbTitle.Text = title;
        }

        /// <summary>
        /// Is the close button visible?
        /// </summary>
        public bool CloseButtonVisible
        {
            get { return cmdClose.Visible; }
            set { cmdClose.Visible = value; }
        }

        /// <summary>
        /// Set focus to the close button.
        /// </summary>
        public void FocusCloseButton()
        {
            cmdClose.Focus();
        }

        /// <summary>
        /// Is the cancel button visible?
        /// </summary>
        public bool CancelButtonVisible
        {
            get { return cmdCancel.Visible; }
            set { cmdCancel.Visible = value; }
        }

        /// <summary>
        /// Is the progress bar visible?
        /// </summary>
        public bool ProgressBarVisible
        {
            get { return progress.Visible; }
            set { progress.Visible = value; }
        }

        /// <summary>
        /// Start when being showed.
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) Start();
        }

        /// <summary>
        /// Run the process.
        /// </summary>
        private void OnDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DoWork();
        }

        /// <summary>
        /// Perform the action.
        /// </summary>
        protected virtual void DoWork()
        {
            // Override me
            throw new NotImplementedException();
        }

        /// <summary>
        /// Show logging
        /// </summary>
        protected void LogOutput(string line)
        {
            if ((line == null) || IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(LogOutput), line);
            }
            else
            {
                tbLog.AppendText(line.Trim() + Environment.NewLine);
            }
        }

        /// <summary>
        /// We're done
        /// </summary>
        private void OnWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ShowError(e.Error);
            }
            OnDone();
        }

        /// <summary>
        /// Worker has finished.
        /// </summary>
        protected virtual void OnDone()
        {
            try
            {
                Done.Fire(this);
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Fire the Close event.
        /// </summary>
        protected virtual void OnClose()
        {
            try
            {
                Close.Fire(this);
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Fire the cancel event.
        /// </summary>
        protected virtual void OnCancel()
        {
            try
            {
                if (worker.IsBusy)
                {
                    worker.CancelAsync();
                }
            }
            catch
            {
                // Ignore
            }
            try
            {
                Cancel.Fire(this);
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Call in the GUI thread when an error has occurred in <see cref="DoWork"/>.
        /// </summary>
        protected virtual void ShowError(Exception error)
        {
            // Override me
        }

        /// <summary>
        /// Start the activity
        /// </summary>
        public void Start()
        {
            if (!started)
            {
                progress.Enabled = true;
                started = true;
                worker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Text in the title.
        /// </summary>
        public string Title
        {
            get { return lbTitle.Text; }
            set { lbTitle.Text = value; }
        }

        /// <summary>
        /// Copy the entire log to the clipboard.
        /// </summary>
        public void CopyLogToClipboard()
        {
            var text = tbLog.Text;
            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }

        /// <summary>
        /// Close button has been clicked.
        /// </summary>
        private void OnCloseClick(object sender, EventArgs e)
        {
            OnClose();
        }

        /// <summary>
        /// Cancel button has been clicked.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            OnCancel();
        }
    }
}
