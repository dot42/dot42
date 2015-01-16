using System;
using System.ComponentModel;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using Dot42.AvdLib;
using Dot42.Shared.UI;
using TallApplications.Common.Extensions;

namespace Dot42.Gui.Controls.Emulator
{
    public partial class EditAvdControl : AppForm
    {
        /// <summary>
        /// Fired when changed have been saved.
        /// </summary>
        public event EventHandler ModificationsSaved;

        private readonly Avd avd;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EditAvdControl(Avd avd)
        {
            this.avd = avd;
            InitializeComponent();
        }

        /// <summary>
        /// Save changes
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            ModificationsSaved.Fire(this);
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Do not save changes.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
