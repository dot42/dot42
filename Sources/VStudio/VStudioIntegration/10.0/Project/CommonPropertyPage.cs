using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dot42.VStudio.Flavors;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using ProjectNode = Microsoft.VisualStudio.Project.ProjectNode;

namespace Microsoft.Common.Project
{
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : IPropertyPage
    {
        private IPropertyPageSite _site;
        private bool _dirty;
        private IVsHierarchy _project;
        private VSProject _vsProject;

        public abstract Control Control
        {
            get;
        }

        public abstract void Apply();
        public abstract void LoadSettings();

        public abstract string Name
        {
            get;
        }

        public IVsHierarchy ProjectMgr
        {
            get
            {
                return _project;
            }
            private set
            {
                _project = value;
                _vsProject = null;
            }
        }


        public bool IsDirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                if (_dirty != value)
                {
                    _dirty = value;
                    if (_site != null)
                    {
                        _site.OnStatusChange((uint)(_dirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
                }
            }
        }

        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            NativeMethods.SetParent(Control.Handle, hWndParent);
        }

        int IPropertyPage.Apply()
        {
            try
            {
                Apply();
                return VSConstants.S_OK;
            }
            catch (Exception e)
            {
                return Marshal.GetHRForException(e);
            }
        }

        void IPropertyPage.Deactivate()
        {
            Control.Dispose();
        }

        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            if (pPageInfo == null)
            {
                throw new ArgumentNullException("arrInfo");
            }

            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = Name;
            info.SIZE.cx = Control.Width;
            info.SIZE.cy = Control.Height;
            pPageInfo[0] = info;
        }

        void IPropertyPage.Help(string pszHelpDir)
        {
        }

        int IPropertyPage.IsPageDirty()
        {
            return (IsDirty ? (int)VSConstants.S_OK : (int)VSConstants.S_FALSE);
        }

        void IPropertyPage.Move(RECT[] pRect)
        {
            if (pRect == null)
            {
                throw new ArgumentNullException("arrRect");
            }

            RECT r = pRect[0];

            Control.Location = new Point(r.left, r.top);
            Control.Size = new Size(r.right - r.left, r.bottom - r.top);
        }

        void IPropertyPage.SetObjects(uint count, object[] punk)
        {
            if (punk == null)
            {
                return;
            }

            if (count > 0)
            {
                if (punk[0] is ProjectConfig)
                {
                    ArrayList configs = new ArrayList();

                    for (int i = 0; i < count; i++)
                    {
                        ProjectConfig config = (ProjectConfig)punk[i];

                        if (ProjectMgr == null)
                        {
                            ProjectMgr = config.ProjectMgr;
                            break;
                        }

                        configs.Add(config);
                    }
                }
                else if (punk[0] is NodeProperties)
                {
                    if (ProjectMgr == null)
                    {
                        ProjectMgr = (punk[0] as NodeProperties).Node.ProjectMgr;
                    }
                }
                else if (punk[0] is IVsBrowseObject)
                {
                    IVsHierarchy projectHier;
                    ErrorHandler.ThrowOnFailure(((IVsBrowseObject) punk[0]).GetProjectItem(out projectHier, out count));
                    ProjectMgr = projectHier;
                    //ProjectMgr = (projectHier).ProjectMgr;
                }
            }
            else
            {
                ProjectMgr = null;
            }

            if (ProjectMgr != null)
            {
                LoadSettings();
            }
        }

        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            _site = pPageSite;
        }

        void IPropertyPage.Show(uint nCmdShow)
        {
            Control.Visible = true; // TODO: pass SW_SHOW* flags through      
            Control.Show();
        }

        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            if (pMsg == null)
            {
                throw new ArgumentNullException("arrMsg");
            }

            MSG msg = pMsg[0];

            if ((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) && (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST))
            {
                return VSConstants.S_FALSE;
            }

            return (NativeMethods.IsDialogMessageA(Control.Handle, ref msg)) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        /// <summary>
        /// Gets a configuration independent project property.
        /// </summary>
        protected string GetProjectProperty(string name)
        {
            var storage = ProjectMgr as IVsBuildPropertyStorage;
            if (storage == null)
                return null;
            string result;
            if (ErrorHandler.Succeeded(storage.GetPropertyValue(name, null, (uint) _PersistStorageType.PST_PROJECT_FILE, out result)))
                return result;
            return string.Empty;
        }

        /// <summary>
        /// Sets a configuration independent project property.
        /// </summary>
        protected void SetProjectProperty(string name, string value)
        {
            var storage = ProjectMgr as IVsBuildPropertyStorage;
            if (storage == null)
                throw new InvalidOperationException("IVsBuildPropertyStorage not found");
            ErrorHandler.ThrowOnFailure(storage.SetPropertyValue(name, null, (uint) _PersistStorageType.PST_PROJECT_FILE, value));
        }

        /// <summary>
        /// Make the given path relative to the project file.
        /// </summary>
        protected internal string MakeProjectRelative(string filename)
        {
            string projectPath;
            ErrorHandler.ThrowOnFailure(ProjectMgr.GetCanonicalName((uint)VSConstants.VSITEMID.Root, out projectPath));
            // Try to use common prefix to avoid upper/lower differences.
            var i = 0;
            var max = Math.Min(filename.Length, projectPath.Length);
            while (i < max)
            {
                if (char.ToUpper(filename[i]) != char.ToUpper(projectPath[i])) 
                    break;
                i++;
            }
            if (i > 0)
                projectPath = filename.Substring(0, i) + projectPath.Substring(i);
            var result = PackageUtilities.MakeRelative(projectPath, filename);
            return result;
        }

        /// <summary>
        /// Get <see cref="ProjectMgr"/> as VSProject.
        /// Can be null.
        /// </summary>
        protected VSProject GetProject()
        {
            if ((_vsProject == null) && (ProjectMgr != null))
            {
                // get EnvDTE.Project from hierarchy
                object pVar;
                ErrorHandler.ThrowOnFailure(ProjectMgr.GetProperty(VSConstants.VSITEMID_ROOT,
                                                                   (int) __VSHPROPID.VSHPROPID_ExtObject, out pVar));
                var dteProject = pVar as EnvDTE.Project;
                if (dteProject == null)
                    return null;

                // Get VSProject from EnvDTE.Project
                _vsProject = dteProject.Object as VSProject;
            }
            return _vsProject;
        }
    }
}