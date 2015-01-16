using System;
using System.IO;
using System.Windows.Forms;
using Dot42.JvmClassLib;

namespace Dot42.Ide.Project
{
    /// <summary>
    /// Dialog used to add jar references.
    /// </summary>
    public partial class AddJarReferenceDialog : Form
    {
        private bool importCode = true;
        private string libName = string.Empty;
        private bool working = false;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AddJarReferenceDialog()
        {
            InitializeComponent();
            UpdateState();
        }

        /// <summary>
        /// Path of the selected jar file.
        /// </summary>
        public string JarPath { get { return tbJarPath.Text; } }

        /// <summary>
        /// Should code be imported from the jar?
        /// </summary>
        public bool ImportCode { get { return importCode; } }

        /// <summary>
        /// Name of the library to reference.
        /// </summary>
        public string LibraryName { get { return libName; } }

        /// <summary>
        /// Browse for a jar file.
        /// </summary>
        private void OnBrowseJarClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".jar";
                dialog.Filter = "Jar files|*.jar";
                dialog.FileName = JarPath;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    importCode = true;
                    libName = string.Empty;
                    tbJarPath.Text = dialog.FileName;
                    cmdBrowseJar.Enabled = false;
                    jarLoader.RunWorkerAsync(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Some text has changed.
        /// </summary>
        private void OnTextChanged(object sender, EventArgs e)
        {
            UpdateState();            
        }

        /// <summary>
        /// Update the state of the controls.
        /// </summary>
        private void UpdateState()
        {
            var hasJarPath = !string.IsNullOrEmpty(JarPath) && File.Exists(JarPath);
            cmdOk.Enabled = hasJarPath && !working;
        }

        /// <summary>
        /// Try loading the jar file to see if we have a stub jar.
        /// </summary>
        private void jarLoader_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            importCode = true;
            libName = string.Empty;
            var jarPath = (string) e.Argument;
            try
            {
                var hasJarPath = !string.IsNullOrEmpty(jarPath) && File.Exists(jarPath);
                if (hasJarPath)
                {
                    var jf = new JarFile(File.OpenRead(jarPath), jarPath, null);
                    ClassFile result;
                    if (jf.TryLoadClass("com/google/android/maps/MapActivity", out result))
                    {
                        importCode = false;
                        libName = "com.google.android.maps";
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void jarLoader_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            working = false;
            cmdBrowseJar.Enabled = true;
            UpdateState();
        }
    }
}
