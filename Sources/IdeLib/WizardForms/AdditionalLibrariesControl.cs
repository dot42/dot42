using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.Ide.WizardForms
{
    public partial class AdditionalLibrariesControl : UserControl
    {
        public event EventHandler CheckedLibrariesChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AdditionalLibrariesControl()
        {
            InitializeComponent();

            LibraryNode supportLib;
            tvLibs.Items.Add(supportLib = new LibraryNode("dot42.AndroidSupportLibrary", "Android Support Library", LicenseAgreement.AndroidSdk));
            tvLibs.Items.Add(new LibraryNode("dot42.GooglePlayServices", "Google Play Services", LicenseAgreement.AndroidSdk, supportLib));
        }

        /// <summary>
        /// Gets all checked libraries.
        /// </summary>
        public IEnumerable<LibraryNode> CheckedLibraries
        {
            get { return tvLibs.Items.OfType<LibraryNode>().Where(x => x.Checked); }
        }

        /// <summary>
        /// Gets all library nodes.
        /// </summary>
        public IEnumerable<LibraryNode> Libraries
        {
            get { return tvLibs.Items.OfType<LibraryNode>(); }
        }

        private void tvLibsItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var node = e.Item as LibraryNode;
            if (node != null)
            {
                node.CheckDependencies();

                // Notify about it
                CheckedLibrariesChanged.Fire(this);
            }
        }
    }
}
