using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Dot42.CryptoUI;
using Dot42.FrameworkDefinitions;
using Dot42.Graphics;
using Dot42.Ide.Project;
using Dot42.Ide.WizardForms;

namespace Dot42.VStudio.Flavors
{
    public partial class AndroidPropertyNameControl : UserControl
    {
        private readonly Action setDirty;
        private readonly List<string> frameworkVersions;

        /// <summary>
        /// Designer ctor
        /// </summary>
        public AndroidPropertyNameControl()
            : this(null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AndroidPropertyNameControl(Action setDirty)
        {
            this.setDirty = setDirty;
            InitializeComponent();

            frameworkVersions = Frameworks.Instance.OrderByDescending(x => x.Name).Select(x => x.Name).ToList();

            cmdBrowseCertificate.Image = Icons16.Folder;
            cmdNewCertificate.Image = Icons16.CertificateNew;
            cbAndroidVersion.Items.AddRange(frameworkVersions.ToArray());
            cbTargetSdkVersion.Items.Add("<Same as Android version>");
            UpdateAllowedTargetSdkVersions();
        }

        /// <summary>
        /// Name of the android package
        /// </summary>
        private string PackageName
        {
            get { return tbPackageName.Text; }
            set { tbPackageName.Text = value; }
        }

        /// <summary>
        /// Filename of apk file.
        /// </summary>
        private string ApkFilename
        {
            get { return tbApkFilename.Text + ".apk"; }
            set
            {
                try
                {
                    // Strip extension
                    tbApkFilename.Text = Path.GetFileNameWithoutExtension(value);
                }
                catch
                {
                    tbApkFilename.Text = value;
                }
            }
        }

        /// <summary>
        /// Path of the signing certificate
        /// </summary>
        private string ApkCertificatePath
        {
            get { return tbCertificate.Text; }
            set { tbCertificate.Text = value; setDirty(); }
        }

        /// <summary>
        /// Thumbprint of the signing certificate
        /// </summary>
        private string ApkCertificateThumbprint { get; set; }

        /// <summary>
        /// Android target version
        /// </summary>
        public string TargetFrameworkVersion
        {
            get { return (string) cbAndroidVersion.SelectedItem; }
            set
            {
                cbAndroidVersion.SelectedItem = value;
                UpdateAllowedTargetSdkVersions();
            }
        }

        /// <summary>
        /// Android target SDK version
        /// </summary>
        private string TargetSdkAndroidVersion
        {
            get { return (cbTargetSdkVersion.SelectedIndex <= 0) ? "" : (string)cbTargetSdkVersion.SelectedItem; }
            set
            {
                if (string.IsNullOrEmpty(value) || (cbTargetSdkVersion.Items.IndexOf(value) < 0))
                    cbTargetSdkVersion.SelectedIndex = 0;
                else
                    cbTargetSdkVersion.SelectedItem = value;
            }
        }

        /// <summary>
        /// Load settings from the given source into this control.
        /// </summary>
        public void LoadFrom(IAndroidProjectProperties source)
        {
            PackageName = source.PackageName;
            ApkFilename = source.ApkFilename;
            TargetFrameworkVersion = source.TargetFrameworkVersion;
            TargetSdkAndroidVersion = source.TargetSdkAndroidVersion;
            ApkCertificatePath = source.ApkCertificatePath;
            ApkCertificateThumbprint = source.ApkCertificateThumbprint;
            cbGenerateWcfProxy.Checked = source.GenerateWcfProxy;

            var libNames = new HashSet<string>(source.ReferencedLibraryNames.Select(x => x.ToLowerInvariant()));
            foreach (var libNode in additionalLibrariesControl.Libraries)
            {
                var key = libNode.DllName.ToLowerInvariant();
                if (libNames.Contains(key))
                {
                    libNode.Checked = false;
                }
            }
        }

        /// <summary>
        /// Save settings from the this control to the given destination.
        /// </summary>
        /// <returns>True if all settings have been saved, false otherwise</returns>
        public bool SaveTo(IAndroidProjectProperties destination)
        {
            // Select lib references to add/remove
            var libNames = new HashSet<string>(destination.ReferencedLibraryNames.Select(x => x.ToLowerInvariant()));
            var nodesToAdd = new List<LibraryNode>();
            var nodesToRemove = new List<LibraryNode>();
            foreach (var libNode in additionalLibrariesControl.Libraries)
            {
                var key = libNode.DllName.ToLowerInvariant();
                if (libNode.Checked)
                {
                    if (!libNames.Contains(key))
                        nodesToAdd.Add(libNode);
                }
                else if (!libNode.Checked)
                {
                    if (libNames.Contains(key))
                        nodesToRemove.Add(libNode);
                }
            }

            // Agree to licenses
            if (nodesToAdd.Any(x => x.License != null))
            {
                // Must accept to agreement(s)
                if (!nodesToAdd.AcceptToAll())
                    return false;
            }

            // Do actual save
            destination.PackageName = PackageName;
            destination.ApkFilename = ApkFilename;
            destination.TargetFrameworkVersion = TargetFrameworkVersion;
            destination.TargetSdkAndroidVersion = TargetSdkAndroidVersion;
            destination.ApkCertificatePath = ApkCertificatePath;
            destination.ApkCertificateThumbprint = ApkCertificateThumbprint;
            destination.GenerateWcfProxy = cbGenerateWcfProxy.Checked;
            foreach (var node in nodesToRemove) destination.RemoveReferencedLibrary(node.DllName);
            foreach (var node in nodesToAdd) destination.AddReferencedLibrary(node.DllName);

            return true;
        }

        /// <summary>
        /// A value has changed.
        /// </summary>
        private void OnValueChanged(object sender, EventArgs e)
        {
            setDirty();
        }

        /// <summary>
        /// Browse for a certificate.
        /// </summary>
        private void OnBrowseCertificateClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".pfx";
                dialog.Filter = "Certificates|*.pfx|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenCertificate(dialog.FileName, null);
                }
            }
        }

        /// <summary>
        /// Open a certificate file.
        /// </summary>
        private void OpenCertificate(string path, string password)
        {
            string certificateThumbprint;
            if (CertificateHelper.OpenCertificate(path, password, out certificateThumbprint))
            {
                ApkCertificateThumbprint = certificateThumbprint;
                ApkCertificatePath = path;
            }
        }

        /// <summary>
        /// Create a new certificate.
        /// </summary>
        private void OnNewCertificateClick(object sender, System.EventArgs e)
        {
            using (var dialog = new CertificateWizard())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                OpenCertificate(dialog.Path, dialog.Password);
            }
        }

        /// <summary>
        /// Selected android version has changed
        /// </summary>
        private void OnAndroidVersionSelectedIndexChanged(object sender, System.EventArgs e)
        {
            UpdateAllowedTargetSdkVersions();
        }

        /// <summary>
        /// Make sure the target SDK version shows only newer versions than the android version.
        /// </summary>
        private void UpdateAllowedTargetSdkVersions()
        {
            var allowed = frameworkVersions.Take(Math.Max(0, cbAndroidVersion.SelectedIndex)).ToList();
            if ((allowed.Count + 1) != cbTargetSdkVersion.Items.Count)
            {
                var selection = cbTargetSdkVersion.SelectedValue;
                while (cbTargetSdkVersion.Items.Count > 1) cbTargetSdkVersion.Items.RemoveAt(1);
                cbTargetSdkVersion.Items.AddRange(allowed.ToArray());
                if (allowed.Contains(selection))
                {
                    cbTargetSdkVersion.SelectedValue = selection;
                }
                else
                {
                    cbTargetSdkVersion.SelectedIndex = 0;
                }
            }
        }
    }
}
