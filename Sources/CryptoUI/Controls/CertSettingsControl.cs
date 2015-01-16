using System;
using System.Windows.Forms;
using System.Drawing;

namespace Dot42.CryptoUI.Controls
{
	/// <summary>
	/// Summary description for SettingsControl.
	/// </summary>
    internal partial class CertSettingsControl : UserControl, ICertificateWizardPage
	{
        /// <summary>
        /// Fired when the state of this control has changed.
        /// </summary>
	    public event EventHandler StateChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
		public CertSettingsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Populate countries list
            cbCountry.BeginUpdate();
            cbCountry.Items.Add("<Select a country>");
			foreach (var country in ISOCountry.List) 
			{
				cbCountry.Items.Add(country);
			}
            cbCountry.SelectedIndex = 0;
            cbCountry.EndUpdate();
        }

        /// <summary>
        /// Focus the first control
        /// </summary>
        internal void FocusFirst()
        {
            tbName.Focus();
        }

        /// <summary>
        /// Fire StateChanged event
        /// </summary>
		private void UpdateEnabledState() 
		{
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
		}

        /// <summary>
        /// Some text input has changed.
        /// </summary>
		private void OnTextChanged(object sender, EventArgs e)
		{
			UpdateEnabledState();
		}

		public string UserName { get { return tbName.Text.Trim(); } }
		public string OrgUnit { get { return tbOrgUnit.Text.Trim(); } }
		public string OrgName { get { return tbOrgName.Text.Trim(); } }
		public string Email { get { return tbEmail.Text.Trim(); } }
		public string State { get { return tbState.Text.Trim(); } }
		public string City { get { return tbCity.Text.Trim(); } }
		public ISOCountry Country { get { return this.selectedCountry; } }

        private void CbCountrySelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCountry = cbCountry.SelectedItem as ISOCountry;
            UpdateEnabledState();
        }

	    /// <summary>
	    /// Should the back button be disabled?
	    /// </summary>
	    public bool IsBackButtonDisabled
	    {
            get { return false; }
	    }

	    /// <summary>
	    /// Should the next button be disabled?
	    /// </summary>
	    public bool IsNextButtonDisabled
	    {
            get { return (UserName.Length == 0) || (Email.Length == 0) || (Country == null); }
	    }
	}
}
