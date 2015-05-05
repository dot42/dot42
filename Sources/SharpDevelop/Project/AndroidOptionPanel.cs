
using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Ide.Project;
using Dot42.VStudio.Flavors;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of AndroidOptionPanel.
	/// </summary>
	public class AndroidOptionPanel : IOptionPanel, ICanBeDirty
	{
		private bool isDirty;
		private readonly AndroidPropertyNameControl control;
		
		public event EventHandler IsDirtyChanged;

		/// <summary>
		/// Default ctor
		/// </summary>
		public AndroidOptionPanel()
		{
			control = new AndroidPropertyNameControl(SetDirty);
		}
		
		public object Owner { get;set; }
		
		public object Control {
			get {
				return control;
			}
		}
		
		public void LoadOptions()
		{
			var project = (MSBuildBasedProject)Owner;
			control.LoadFrom(new ProjectProperties(project));
			IsDirty = false;
		}
		
		public bool SaveOptions()
		{
			var project = (MSBuildBasedProject)Owner;
			control.SaveTo(new ProjectProperties(project));
			// Saved
			IsDirty = false;
			return true;
		}
		
		private void SetDirty() {
			IsDirty = true;
		}
		
		public bool IsDirty {
			get { return isDirty; }
			set {
				if (isDirty != value) {
					isDirty = value;
					IsDirtyChanged.Fire(this);
				}
			}
		}

		private class ProjectProperties : IAndroidProjectProperties
		{
			private readonly MSBuildBasedProject project;

			/// <summary>
			/// Default ctor
			/// </summary>
			public ProjectProperties(MSBuildBasedProject page)
			{
				this.project = page;
			}

			public string PackageName
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropPackageName); }
				set { project.SetProperty(Dot42Constants.PropPackageName, value); }
			}
			public string ApkFilename
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropApkFilename); }
				set { project.SetProperty(Dot42Constants.PropApkFilename, value); }
			}

            public string RootNamespace
            {
                get { return project.GetUnevalatedProperty(Dot42Constants.RootNamespace); }
                set { project.SetProperty(Dot42Constants.RootNamespace, value); }
            }

            public string AssemblyName
            {
                get { return project.GetUnevalatedProperty(Dot42Constants.AssemblyName); }
                set { project.SetProperty(Dot42Constants.AssemblyName, value); }
            }

		    public bool ApkOutputs
            {
                get { return project.GetUnevalatedProperty(Dot42Constants.PropApkOutputs) == "true"; }
            }
            public string ApkCertificatePath
			{
                get { return project.GetUnevalatedProperty(Dot42Constants.PropApkCertificatePath); }
				set { project.SetProperty(Dot42Constants.PropApkCertificatePath, FileUtility.GetRelativePath(project.Directory, value)); }
			}
			public string ApkCertificateThumbprint
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropApkCertificateThumbprint); }
				set { project.SetProperty(Dot42Constants.PropApkCertificateThumbprint, value); }
			}
			public string TargetFrameworkVersion
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropTargetFrameworkVersion); }
				set { project.SetProperty(Dot42Constants.PropTargetFrameworkVersion, value); }
			}
			public string TargetSdkAndroidVersion
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropTargetSdkAndroidVersion); }
				set { project.SetProperty(Dot42Constants.PropTargetSdkAndroidVersion, value); }
			}
			public bool GenerateWcfProxy
			{
				get { return project.GetUnevalatedProperty(Dot42Constants.PropGenerateWcfProxy) == "true"; }
				set { project.SetProperty(Dot42Constants.PropGenerateWcfProxy, value ? "true" : "false"); }
			}
			
			public IEnumerable<string> ReferencedLibraryNames
			{
				get { return project.Items.OfType<ReferenceProjectItem>().Select(x => x.Include); }
			}

            public bool GenerateSetNextInstructionCode
            {
                get { return project.GetUnevalatedProperty(Dot42Constants.PropGenerateSetNextInstructionCode) == "true"; }
                set { project.SetProperty(Dot42Constants.PropGenerateSetNextInstructionCode, value ? "true" : "false"); }
            }

		    public bool EnableCompilerCache
		    {
                get { return project.GetUnevalatedProperty(Dot42Constants.PropEnableCompilerCache) == "true"; }
                set { project.SetProperty(Dot42Constants.PropEnableCompilerCache, value ? "true" : "false"); }
		        
		    }

		    public void AddReferencedLibrary(string name)
			{
				project.PerformUpdateOnProjectFile(() => { project.MSBuildProjectFile.AddItem(ItemType.Reference.ItemName, name); });
			}
			
			public void RemoveReferencedLibrary(string name)
			{
				project.PerformUpdateOnProjectFile(() => { project.Items.RemoveWhere(x => (x.Include == name) && (x.ItemType == ItemType.Reference)); });
			}
		}
	}
}
