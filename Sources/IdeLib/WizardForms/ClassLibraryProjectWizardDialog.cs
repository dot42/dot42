using System;
using System.Linq;
using System.Windows.Forms;
using Dot42.FrameworkDefinitions;
using Dot42.Shared.UI;

namespace Dot42.Ide.WizardForms
{
    public partial class ClassLibraryProjectWizardDialog : Form
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ClassLibraryProjectWizardDialog()
        {
            InitializeComponent();
            
            var frameworkNames = Frameworks.Instance.OrderByDescending(x => x.Name).Select(x => x.Name).ToArray();
            cbFramework.Items.AddRange(frameworkNames);
            if (cbFramework.Items.Count > 0)
                cbFramework.SelectedIndex = 0;

            // Restore preferences
            var prefs = UserPreferences.Preferences;
            var lastVersion = prefs.FrameworkVersion;
            if (!string.IsNullOrEmpty(lastVersion))
            {
                var index = Array.IndexOf(frameworkNames, lastVersion);
                if (index >= 0)
                    cbFramework.SelectedIndex = index;
            }

            UpdateState();
        }

        /// <summary>
        /// Version of target framework
        /// </summary>
        public string TargetFrameworkVersion
        {
            get { return (string) cbFramework.SelectedItem; }
        }

        /// <summary>
        /// Update control state
        /// </summary>
        private void UpdateState()
        {
            cmdOK.Enabled = !string.IsNullOrEmpty(TargetFrameworkVersion);
        }

        /// <summary>
        /// Target framework has changed.
        /// </summary>
        private void OnTargetFrameworkVersionChanged(object sender, System.EventArgs e)
        {
            UpdateState();
        }

        /// <summary>
        /// OK has been clicked.
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            // Save preferences
            var prefs = UserPreferences.Preferences;
            prefs.FrameworkVersion = TargetFrameworkVersion;
            UserPreferences.SaveNow();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
