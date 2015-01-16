using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using Dot42.FrameworkDefinitions;
using Dot42.Shared.UI;
using Dot42.Utility;

namespace Dot42.Gui.Controls.Emulator
{
    public partial class CreateAvdControl : AppForm
    {
        /// <summary>
        /// Fired when a new AVD has been created.
        /// </summary>
        public event EventHandler<AvdEventArgs> Created;

        private readonly Dot42.AvdLib.AvdManager manager;

        /// <summary>
        /// Default ctor
        /// </summary>
        public CreateAvdControl(Dot42.AvdLib.AvdManager manager)
        {
            this.manager = manager;
            InitializeComponent();
            // Load frameworks
            cbTarget.Items.AddRange(Frameworks.Instance.ToArray());
            if (cbTarget.Items.Count > 0)
            {
                cbTarget.SelectedIndex = 0;
            }
            else
            {
                cbTarget.Enabled = false;
            }
            // Update controls
            OnNameTextChanged(null, null);
        }

        /// <summary>
        /// Create AVD
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            if (!superValidator1.Validate())
                return;

            var name = tbName.Text.Trim();
            var framework = (FrameworkInfo)cbTarget.SelectedItem;

            // Get system image
            var image = SystemImages.Instance.First();
            // Get skin
            var skin = framework.DefaultSkin;

            var avd = manager.Create(name);
            avd.Target = framework.Descriptor.Target;
            avd.Config.AbiType = image.Abi;
            avd.Config.Images1 = image.RelativeFolder;
            if (skin != null)
            {
                avd.Config.SkinName = skin.Name;
                avd.Config.SkinPath = skin.RelativeFolder;
                foreach (var key in skin.Hardware.Keys)
                {
                    avd.Config[key] = skin.Hardware[key];
                }
            }
            avd.Save();

            if (Created != null)
            {
                Created(this, new AvdEventArgs(avd));
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Cancel.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Text in name field has changed.
        /// </summary>
        private void OnNameTextChanged(object sender, EventArgs e)
        {
            var name = tbName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                cmdOK.Enabled = false;
                superValidator1.ClearFailedValidations();
            }
            else
            {
                cmdOK.Enabled = superValidator1.Validate();
            }
        }

        /// <summary>
        /// Is the given name a valid AVD name?
        /// </summary>
        private void OnValidateNameIsValid(object sender, DevComponents.DotNetBar.Validator.ValidateValueEventArgs e)
        {
            var name = tbName.Text.Trim();
            e.IsValid = (name.Length > 0) && !ProcessRunner.ContainEscapeCharacter(name);
        }

        /// <summary>
        /// Is the given name "free"
        /// </summary>
        private void OnValidateNameExists(object sender, DevComponents.DotNetBar.Validator.ValidateValueEventArgs e)
        {
            var name = tbName.Text.Trim();
            e.IsValid = !manager.Contains(name);
        }
    }
}
