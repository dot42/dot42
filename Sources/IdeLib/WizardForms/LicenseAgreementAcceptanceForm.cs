using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dot42.Utility;

namespace Dot42.Ide.WizardForms
{
    public partial class LicenseAgreementAcceptanceForm : Form
    {
        private readonly LicenseAgreement license;

        /// <summary>
        /// Designer ctor
        /// </summary>
        [Obsolete]
        public LicenseAgreementAcceptanceForm()
            : this(null, null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal LicenseAgreementAcceptanceForm(LicenseAgreement license, IEnumerable<string> libraryNames)
        {
            this.license = license;
            InitializeComponent();
            if ((license != null) && (libraryNames != null))
            {
                this.Text = license.Name;
                tbAgreement.Text = license.Text.Replace("\n", Environment.NewLine);
                foreach (var name in libraryNames)
                {
                    lbLibraries.Items.Add(name);
                }
            }
        }
    }
}
