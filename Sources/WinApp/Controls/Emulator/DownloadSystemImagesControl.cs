using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using Dot42.Utility;
using TallApplications.Common.Extensions;

namespace Dot42.Gui.Controls.Emulator
{
    public partial class DownloadSystemImagesControl : SlidePanel, IProgressControl
    {
        public event EventHandler Cancel;
        public event EventHandler Done;

        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Default ctor
        /// </summary>
        public DownloadSystemImagesControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load data now
        /// </summary>
        internal void Initialize()
        {
            UpdateControlState();
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = Task.Factory.StartNew(() => new SdkRepository(), cancelTokenSource.Token);
            task.ContinueWith(ProcessRepository, ui);
        }

        /// <summary>
        /// Load the repository into the UI.
        /// </summary>
        private void ProcessRepository(Task<SdkRepository> downloadTask)
        {
            SuspendLayout();
            progress.Visible = false;
            lbProgress.Visible = false;
            if (downloadTask.Exception != null)
            {
                // Show error
            }
            else
            {
                // Load possible images
                lvImages.BeginUpdate();
                lvImages.Items.Clear();
                foreach (var image in downloadTask.Result.SystemImages.OrderByDescending(x => x.ApiLevel))
                {
                    var item = lvImages.Items.Add(image.Description);
                    item.Tag = image;
                }
                lvImages.EndUpdate();
                if (lvImages.Items.Count > 0)
                {
                    lvImages.Items[0].Selected = true;
                }
                else
                {
                    lvImages.Items.Add("No system images found");
                }
            }
            ResumeLayout(true);
            UpdateControlState();
        }

        /// <summary>
        /// AVD selection changed.
        /// </summary>
        private void OnAvdsSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlState();
        }

        /// <summary>
        /// Update the state of all controls
        /// </summary>
        private void UpdateControlState()
        {
            var selection = SelectedImage;
            cmdDownload.Enabled = (selection != null);
        }

        /// <summary>
        /// Download the selected images
        /// </summary>
        private void OnDownloadClick(object sender, EventArgs e)
        {
            var image = SelectedImage;
            if (image == null)
                return;
            var archive = image.Archives.FirstOrDefault();
            if (archive == null)
                return;

            lbProgress.Text = string.Format("Downloading system image {0}...", image.Description);
            lbProgress.Visible = true;
            progress.ProgressType = eProgressItemType.Standard;            
            cmdDownload.Enabled = false;
            lvImages.Enabled = true;
            progress.Visible = true;

            var path = Path.GetTempFileName();
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = new WebClient().DownloadFileTask(new Uri(archive.Url), path, DownloadProgress);
            task.ContinueWith(x => ProcessDownloadImage(x, image, path), ui);
        }

        /// <summary>
        /// Update UI on progress
        /// </summary>
        private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DownloadProgressChangedEventHandler(DownloadProgress), sender, e);
            }
            else
            {
                progress.Value = e.ProgressPercentage;
            }
        }

        /// <summary>
        /// Download image is complete (or failed)
        /// </summary>
        private void ProcessDownloadImage(Task downloadTask, SdkSystemImage image, string path)
        {
            if (downloadTask.Exception != null)
            {
                SuspendLayout();
                progress.Visible = false;
                lbProgress.Visible = false;
                ResumeLayout(true);
                UpdateControlState();            

                // Show error
                var msg = string.Format("Failed to download system image because {0}", downloadTask.Exception.Message);
                MessageBoxEx.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Install image
                lbProgress.Text = string.Format("Installing {0}...", image.Description);
                var ui = TaskScheduler.FromCurrentSynchronizationContext();
                var task = Task.Factory.StartNew(() => SystemImages.Instance.Install(image, path));
                task.ContinueWith(x => ProcessInstallImage(x, image, path), ui);
            }
        }

        /// <summary>
        /// Installation of image is complete (or failed)
        /// </summary>
        private void ProcessInstallImage(Task downloadTask, SdkSystemImage image, string path)
        {
            if (downloadTask.Exception != null)
            {
                SuspendLayout();
                progress.Visible = false;
                lbProgress.Visible = false;
                ResumeLayout(true);
                UpdateControlState();

                // Show error
                var msg = string.Format("Failed to install system image because {0}", downloadTask.Exception.Message);
                MessageBoxEx.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // We're done
                Done.Fire(this);
            }
        }

        /// <summary>
        /// Cancel this control
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            cancelTokenSource.Cancel();
            Cancel.Fire(this);
        }

        /// <summary>
        /// Gets the selected system image or null if there is no selection.
        /// </summary>
        private SdkSystemImage SelectedImage
        {
            get
            {
                var selection = (lvImages.SelectedItems.Count > 0) ? lvImages.SelectedItems[0] : null;
                return (selection != null) ? (SdkSystemImage)selection.Tag : null;
            }
        }

        /// <summary>
        /// Gets the title to show in the progress window.
        /// </summary>
        public string Title { get { return "Download emulator system images"; } }
    }
}
