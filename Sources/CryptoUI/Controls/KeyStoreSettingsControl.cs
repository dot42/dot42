using System;
using System.Windows.Forms;

namespace Dot42.CryptoUI.Controls
{
	/// <summary>
	/// Summary description for SettingsControl.
	/// </summary>
    internal partial class KeyStoreSettingsControl : UserControl, ICertificateWizardPage
	{
        /// <summary>
        /// Fired when the state of this control has changed.
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
		public KeyStoreSettingsControl()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Focus the first control
        /// </summary>
        internal void FocusFirst()
        {
            tbPassword.Focus();
        }

		/// <summary>
        /// Fire the StateChanged event.
        /// </summary>
		private void UpdateEnabledState() 
		{
			var pw1 = Password;
			var pw2 = Password2;
			lbNotEqual.Visible = (pw1 != pw2) && (pw2.Length > 0);
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
		}

        /// <summary>
        /// Some input text has changed.
        /// </summary>
		private void OnTextChanged(object sender, System.EventArgs e)
		{
			UpdateEnabledState();
		}

		public string Password { get { return tbPassword.Text.Trim(); } }
		private string Password2 { get { return tbPassword2.Text.Trim(); } }

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
	        get
	        {
                var pw1 = Password;
                var pw2 = Password2;

                return (pw1.Length > 0) && (pw1 != pw2);
	        }
	    }
	}
}
