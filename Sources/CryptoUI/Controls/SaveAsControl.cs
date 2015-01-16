using System;
using System.Windows.Forms;

namespace Dot42.CryptoUI.Controls
{
	/// <summary>
	/// Control used to select a file path for the certificate.
	/// </summary>
    internal partial class SaveAsControl: UserControl, ICertificateWizardPage
	{
        /// <summary>
        /// Fired when the state of this control has changed.
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
		public SaveAsControl()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Focus the first control
        /// </summary>
        internal void FocusFirst()
        {
            tbFilename.Focus();
        }

		/// <summary>
        /// Fire the StateChanged event.
        /// </summary>
		private void UpdateEnabledState() 
		{
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
		}

        /// <summary>
        /// Some input text has changed.
        /// </summary>
		private void OnTextChanged(object sender, EventArgs e)
		{
			UpdateEnabledState();
		}

        /// <summary>
        /// Gets/sets entered filename.
        /// </summary>
	    public string FileName
	    {
	        get { return tbFilename.Text.Trim(); }
            set { tbFilename.Text = value; }
	    }

        /// <summary>
        /// If checked, the certificate should be imported in the user's My certificates store.
        /// </summary>
	    public bool ImportInCertificateStore
	    {
            get { return cbImport.Checked; }
	    }

	    /// <summary>
	    /// Should the back button be disabled?
	    /// </summary>
	    public bool IsBackButtonDisabled
	    {
	        get { return true; }
	    }

	    /// <summary>
	    /// Should the next button be disabled?
	    /// </summary>
	    public bool IsNextButtonDisabled
	    {
            get { return string.IsNullOrEmpty(FileName); }
	    }

        /// <summary>
        /// Browse for a path.
        /// </summary>
        private void OnBrowseFileNameClick(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.DefaultExt = ".pfx";
                dialog.Filter = "Certificates|*.pfx";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    tbFilename.Text = dialog.FileName;
                }
            }
        }
	}
}
