using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Dot42.Ide.Project;
using Microsoft.Common.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Dot42.VStudio.Flavors
{
    /// <summary>
    /// Property page for Android settings
    /// </summary>
    [ComVisible(true), Guid(GuidList.Strings.guidDot42AndroidPropertyPage)]
    [Obfuscation(Exclude = false)]
    public class AndroidPropertyPage : CommonPropertyPage, IVsSetTargetFrameworkWorkerCallback
    {
        private readonly AndroidPropertyNameControl control;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AndroidPropertyPage()
        {
            control = new AndroidPropertyNameControl(SetDirty) { Enabled = false };
        }

        /// <summary>
        /// Mark dirty
        /// </summary>
        private void SetDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// UI control
        /// </summary>
        public override Control Control
        {
            get { return control; }
        }

        /// <summary>
        /// Save settings into project.
        /// </summary>
        public override void Apply()
        {
            var oldTargetVersion = GetProjectProperty(Dot42Constants.PropTargetFrameworkVersion);
            var targetFrameworkVersionChanged = (oldTargetVersion != control.TargetFrameworkVersion);
            if (control.SaveTo(new ProjectProperties(this)))
            {
                LoadSettings();
                if (targetFrameworkVersionChanged)
                {
                    OnChangeTargetFramework(oldTargetVersion, control.TargetFrameworkVersion);
                }
                IsDirty = false;
            }
        }

        /// <summary>
        /// Load settings into page.
        /// </summary>
        public override void LoadSettings()
        {
            var props = new ProjectProperties(this);
            control.LoadFrom(props);
            IsDirty = false;
            control.Enabled = true;
        }

        /// <summary>
        /// Gets tab name
        /// </summary>
        public override string Name
        {
            get { return "Android"; }
        }

        /// <summary>
        /// Trigger a target framework moniker change.
        /// </summary>
        private void OnChangeTargetFramework(string oldTargetVersion, string newTargetVersion)
        {
            IServiceProvider serviceProvider;
            ErrorHandler.ThrowOnFailure(ProjectMgr.GetSite(out serviceProvider));
            var retargetingService = GetService(serviceProvider, typeof(SVsTrackProjectRetargeting)) as IVsTrackProjectRetargeting;
            if (retargetingService != null)
            {
                var oldTarget = CreateFrameworkName(oldTargetVersion);
                var newTarget = CreateFrameworkName(newTargetVersion);
                retargetingService.OnSetTargetFramework(ProjectMgr, oldTarget.FullName, newTarget.FullName, this, true);
            }
        }

        /// <summary>
        /// Create a Dot42 framework name.
        /// </summary>
        private static FrameworkName CreateFrameworkName(string version)
        {
            if (version.StartsWith("v"))
            {
                version = version.Substring(1);
            }
            return new FrameworkName(Dot42Constants.TargetFrameworkIdentifier, Version.Parse(version));
        }

        /// <summary>
        /// Gets a service object.
        /// </summary>
        private static object GetService(IServiceProvider serviceProvider, Type type)
        {
            object service = null;
            IntPtr serviceIntPtr;
            int hr = 0;

            var SIDGuid = type.GUID;
            var IIDGuid = SIDGuid;
            hr = serviceProvider.QueryService(ref SIDGuid, ref IIDGuid, out serviceIntPtr);

            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            else if (!serviceIntPtr.Equals(IntPtr.Zero))
            {
                service = Marshal.GetObjectForIUnknown(serviceIntPtr);
                Marshal.Release(serviceIntPtr);
            }

            return service;
        }

        /// <summary>
        /// Update the project to the new target framework.
        /// </summary>
        int IVsSetTargetFrameworkWorkerCallback.UpdateTargetFramework(IVsHierarchy pHier, string currentTargetFramework, string newTargetFramework)
        {
            // Do nothing
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the names of all included references.
        /// </summary>
        public IEnumerable<string> ReferencedLibraryNames
        {
            get
            {
                var project = GetProject();
                if (project != null)
                {
                    foreach (Reference reference in project.References)
                    {
                        yield return reference.Identity;
                    }
                }
            }
        }

        /// <summary>
        /// Add a reference to the given DLL name.
        /// </summary>
        public void AddReferencedLibrary(string name)
        {
            var project = GetProject();
            if (project == null)
                throw new InvalidOperationException("No project found");
            project.References.Add(name);
        }

        /// <summary>
        /// Remove a reference to the given DLL name.
        /// </summary>
        public void RemoveReferencedLibrary(string name)
        {
            var project = GetProject();
            if (project == null)
                throw new InvalidOperationException("No project found");
            var reference = project.References.Find(name);
            if (reference != null)
            {
                reference.Remove();
            }
        }

        private class ProjectProperties : IAndroidProjectProperties
        {
            private readonly AndroidPropertyPage page;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ProjectProperties(AndroidPropertyPage page)
            {
                this.page = page;
            }

            public string PackageName
            {
                get { return page.GetProjectProperty(Dot42Constants.PropPackageName); }
                set { page.SetProjectProperty(Dot42Constants.PropPackageName, value); }
            }
            public string ApkFilename
            {
                get { return page.GetProjectProperty(Dot42Constants.PropApkFilename); }
                set { page.SetProjectProperty(Dot42Constants.PropApkFilename, value); }
            }

            public string RootNamespace
            {
                get { return page.GetProjectProperty(Dot42Constants.RootNamespace); }
                set { page.SetProjectProperty(Dot42Constants.RootNamespace, value); }
            }

            public string AssemblyName
            {
                get { return page.GetProjectProperty(Dot42Constants.AssemblyName); }
                set { page.SetProjectProperty(Dot42Constants.AssemblyName, value); }
            }

            public bool ApkOutputs
            {
                get { return page.GetProjectProperty(Dot42Constants.PropApkOutputs) == "true"; }
            }
            public string ApkCertificatePath
            {
                get { return page.GetProjectProperty(Dot42Constants.PropApkCertificatePath); }
                set { page.SetProjectProperty(Dot42Constants.PropApkCertificatePath, page.MakeProjectRelative(value)); }
            }
            public string ApkCertificateThumbprint
            {
                get { return page.GetProjectProperty(Dot42Constants.PropApkCertificateThumbprint); }
                set { page.SetProjectProperty(Dot42Constants.PropApkCertificateThumbprint, value); }
            }
            public string TargetFrameworkVersion
            {
                get { return page.GetProjectProperty(Dot42Constants.PropTargetFrameworkVersion); }
                set { page.SetProjectProperty(Dot42Constants.PropTargetFrameworkVersion, value); }
            }
            public string TargetSdkAndroidVersion
            {
                get { return page.GetProjectProperty(Dot42Constants.PropTargetSdkAndroidVersion); }
                set { page.SetProjectProperty(Dot42Constants.PropTargetSdkAndroidVersion, value); }
            }
            public bool GenerateWcfProxy
            {
                get { return page.GetProjectProperty(Dot42Constants.PropGenerateWcfProxy) == "true"; }
                set { page.SetProjectProperty(Dot42Constants.PropGenerateWcfProxy, value ? "true" : "false"); }
            }

            public IEnumerable<string> ReferencedLibraryNames { get { return page.ReferencedLibraryNames; } }

            public bool GenerateSetNextInstructionCode
            {
                get { return page.GetProjectProperty(Dot42Constants.PropGenerateSetNextInstructionCode) == "true"; }
                set { page.SetProjectProperty(Dot42Constants.PropGenerateSetNextInstructionCode, value ? "true" : "false"); }
            }

            public void AddReferencedLibrary(string name)
            {
                page.AddReferencedLibrary(name);
            }

            public void RemoveReferencedLibrary(string name)
            {
                page.RemoveReferencedLibrary(name);
            }
        }
    }
}
