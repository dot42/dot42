using System.Windows.Forms;
using Dot42.Utility;
using System.Collections.Generic;

namespace Dot42.Ide.WizardForms
{
    public class LibraryNode : ListViewItem
    {
        private readonly string dllName;
        private readonly LicenseAgreement license;
        private readonly List<LibraryNode> dependingOnOther;
        private readonly List<LibraryNode> dependingOnMe;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LibraryNode(string dllName, string name, LicenseAgreement license, params LibraryNode[] dependingOn)
        {
            this.dllName = dllName;
            this.license = license;
            this.dependingOnOther = new List<LibraryNode>();
            if (null != dependingOn)
            {
                this.dependingOnOther.AddRange(dependingOn);
            }

            this.dependingOnMe = new List<LibraryNode>();
            this.Text = name;
            this.SubItems.Add(new ListViewSubItem(this, (license != null) ? license.Name : "-"));

            foreach (LibraryNode other in this.dependingOnOther)
            {
                other.dependingOnMe.Add(this);
            }
        }

        /// <summary>
        /// Make sure dependencies are checked.
        /// </summary>
        public virtual void CheckDependencies()
        {
            List<LibraryNode> dependencies = this.Checked ? this.dependingOnOther : this.dependingOnMe;
            foreach (LibraryNode dep in dependencies)
            {
                if ((dep.Index >= 0) && (dep.Checked != this.Checked))
                {
                    dep.Checked = this.Checked;
                    dep.CheckDependencies();
                }
            }
        }

        /// <summary>
        /// DLL name of this library
        /// </summary>
        public string DllName
        {
            get { return dllName; }
        }

        /// <summary>
        /// License agreement covering this library (can be null).
        /// </summary>
        public LicenseAgreement License
        {
            get { return license; }
        }
    }
}
