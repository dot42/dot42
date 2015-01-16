/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.VisualStudio.Package
{

	/// <summary>
	/// All public properties on Nodeproperties or derived classes are assumed to be used by Automation by default.
	/// Set this attribute to false on Properties that should not be visible for Automation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class AutomationBrowsableAttribute : System.Attribute
	{
		public AutomationBrowsableAttribute(bool browsable)
		{
			this.browsable = browsable;
		}

		public bool Browsable
		{
			get
			{
				return this.browsable;
			}
		}

		private bool browsable;
	}

	/// <devdoc>
	/// To create your own localizable node properties, subclass this and add public properties
	/// decorated with your own localized display name, category and description attributes.
	/// </devdoc>
	[CLSCompliant(false), ComVisible(true)]
	public class NodeProperties : Microsoft.VisualStudio.Package.LocalizableProperties,
		ISpecifyPropertyPages,
		IVsGetCfgProvider,
		IVsSpecifyProjectDesignerPages,
		EnvDTE80.IInternalExtenderProvider,
		IVsBrowseObject
	{
		#region fields
		private HierarchyNode node;
		#endregion

		#region properties

		[Browsable(false)]
		[AutomationBrowsable(false)]
		public HierarchyNode Node
		{
			get { return this.node; }
			set { this.node = value; }
		}

		/// <summary>
		/// Used by Property Pages Frame to set it's title bar. The Caption of the Hierarchy Node is returned.
		/// </summary>
		[Browsable(false)]
		[AutomationBrowsable(false)]
		public virtual string Name
		{
			get { return this.node.Caption; }
		}

		#endregion

		#region ctors
		public NodeProperties()
		{

		}

		public NodeProperties(HierarchyNode node)
		{
			this.node = node;
		}
		#endregion

		#region ISpecifyPropertyPages methods
		public virtual void GetPages(CAUUID[] pages)
		{
			this.GetCommonPropertyPages(pages);
		}
		#endregion

		#region IVsSpecifyProjectDesignerPages
		/// <summary>
		/// Implementation of the IVsSpecifyProjectDesignerPages. It will retun the pages that are configuration independent.
		/// </summary>
		/// <param name="pages">The pages to return.</param>
		/// <returns></returns>
		public virtual int GetProjectDesignerPages(CAUUID[] pages)
		{
			this.GetCommonPropertyPages(pages);
			return VSConstants.S_OK;
		}
		#endregion

		#region IVsGetCfgProvider methods
		public virtual int GetCfgProvider(out IVsCfgProvider p)
		{
			p = null;
			return VSConstants.E_NOTIMPL;
		}
		#endregion

		#region IVsBrowseObject methods
		/// <summary>
		/// Maps back to the hierarchy or project item object corresponding to the browse object.
		/// </summary>
		/// <param name="hier">Reference to the hierarchy object.</param>
		/// <param name="itemid">Reference to the project item.</param>
		/// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code. </returns>
		public virtual int GetProjectItem(out IVsHierarchy hier, out uint itemid)
		{
			if (this.node == null)
			{
				throw new InvalidOperationException();
			}
			hier = HierarchyNode.GetOuterHierarchy(this.node.ProjectMgr);
			itemid = this.node.ID;
			return VSConstants.S_OK;
		}
		#endregion

		#region overridden methods
		/// <summary>
		/// Get the Caption of the Hierarchy Node instance. If Caption is null or empty we delegate to base
		/// </summary>
		/// <returns>Caption of Hierarchy node instance</returns>
		public override string GetComponentName()
		{
			Debug.Assert(this.node != null, "The associated hierarchy node has not been initialized");
			string caption = this.node.Caption;
			if (string.IsNullOrEmpty(caption))
			{
				return base.GetComponentName();
			}
			else
			{
				return caption;
			}
		}
		#endregion

		#region helper methods
		protected string GetProperty(string name, string def)
		{
			Debug.Assert(this.node != null, "The associated hierarchy node has not been initialized");
			string a = this.node.ItemNode.GetMetadata(name);
			return (a == null) ? def : a;
		}

		protected void SetProperty(string name, string value)
		{
			Debug.Assert(this.node != null, "The associated hierarchy node has not been initialized");
			this.node.ItemNode.SetMetadata(name, value);
		}

		/// <summary>
		/// Retrieves the common property pages. The NodeProperties is the BrowseObject and that will be called to support 
		/// configuration independent properties.
		/// </summary>
		/// <param name="pages">The pages to return.</param>
		private void GetCommonPropertyPages(CAUUID[] pages)
		{
			// We do not check whether the supportsProjectDesigner is set to false on the ProjectNode.
			// We rely that the caller knows what to call on us.
			Debug.Assert(this.node != null, "The associated hierarchy node has not been initialized");

			if (pages == null)
			{
				throw new ArgumentNullException("pages");
			}

			if (pages.Length == 0)
			{
				throw new ArgumentException(SR.GetString(SR.InvalidParameter), "pages");
			}

			// Only the project should show the property page the rest should show the project properties.
			if (this.node != null && (this.node is ProjectNode))
			{
				// Retrive the list of guids from hierarchy properties.
				// Because a flavor could modify that list we must make sure we are calling the outer most implementation of IVsHierarchy
				string guidsList = String.Empty;
				IVsHierarchy hierarchy = HierarchyNode.GetOuterHierarchy(this.Node.ProjectMgr);
				object variant = null;
				ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList, out variant));
				guidsList = (string)variant;

				Guid[] guids = Utilities.GuidsArrayFromSemicolonDelimitedStringOfGuids(guidsList);
				if (guids == null || guids.Length == 0)
				{
					pages[0] = new CAUUID();
					pages[0].cElems = 0;
				}
				else
				{
					pages[0] = PackageUtilities.CreateCAUUIDFromGuidArray(guids);
				}
			}
			else
			{
				pages[0] = new CAUUID();
				pages[0].cElems = 0;
			}
		}
		#endregion

		#region IInternalExtenderProvider Members

		bool EnvDTE80.IInternalExtenderProvider.CanExtend(string extenderCATID, string extenderName, object extendeeObject)
		{
			IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.Node);
			if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
				return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).CanExtend(extenderCATID, extenderName, extendeeObject);
			return false;
		}

		object EnvDTE80.IInternalExtenderProvider.GetExtender(string extenderCATID, string extenderName, object extendeeObject, EnvDTE.IExtenderSite extenderSite, int cookie)
		{
			IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.Node);
			if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
				return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtender(extenderCATID, extenderName, extendeeObject, extenderSite, cookie);
			return null;
		}

		object EnvDTE80.IInternalExtenderProvider.GetExtenderNames(string extenderCATID, object extendeeObject)
		{
			IVsHierarchy outerHierarchy = HierarchyNode.GetOuterHierarchy(this.Node);
			if (outerHierarchy is EnvDTE80.IInternalExtenderProvider)
				return ((EnvDTE80.IInternalExtenderProvider)outerHierarchy).GetExtenderNames(extenderCATID, extendeeObject);
			return null;
		}

		#endregion

		#region ExtenderSupport
		[Browsable(false)]
		public virtual string ExtenderCATID()
		{
			Guid catid = this.Node.ProjectMgr.GetCATIDForType(this.GetType());
			if (Guid.Empty.CompareTo(catid) == 0)
				return null;
			return catid.ToString("B");
		}
		[Browsable(false)]
		public object ExtenderNames()
		{
			EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.Node.GetService(typeof(EnvDTE.ObjectExtenders));
			return extenderService.GetExtenderNames(this.ExtenderCATID(), this);
		}

		public object Extender(string extenderName)
		{
			EnvDTE.ObjectExtenders extenderService = (EnvDTE.ObjectExtenders)this.Node.GetService(typeof(EnvDTE.ObjectExtenders));
			return extenderService.GetExtender(this.ExtenderCATID(), extenderName, this);
		}

		#endregion
	}

	[CLSCompliant(false), ComVisible(true)]
	public class FileNodeProperties : NodeProperties
	{	
		#region properties
		[SRCategoryAttribute(SR.Advanced)]
		[LocDisplayName(SR.BuildAction)]
		[SRDescriptionAttribute(SR.BuildActionDescription)]
		public virtual Microsoft.VisualStudio.Package.BuildAction BuildAction
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");
				string value = this.Node.ItemNode.ItemName;
				if (value == null || value.Length == 0)
					return Microsoft.VisualStudio.Package.BuildAction.None;
				return (Microsoft.VisualStudio.Package.BuildAction)Enum.Parse(typeof(Microsoft.VisualStudio.Package.BuildAction), value);
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				this.Node.ItemNode.ItemName = value.ToString();
			}
		}

		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.FileName)]
		[SRDescriptionAttribute(SR.FileNameDescription)]
		public string FileName
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.Caption;
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				this.Node.SetEditLabel(value);
			}
		}

		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.FullPath)]
		[SRDescriptionAttribute(SR.FullPathDescription)]
		public string FullPath
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.Url;
			}
		}

		#region non-browsable properties - used for automation only
		[Browsable(false)]
		public string Extension
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return Path.GetExtension(this.Node.Caption);
			}
		}
		#endregion

		#endregion

		#region ctors
		public FileNodeProperties()
		{
		}

		public FileNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region overridden methods
		public override string GetClassName()
		{
			return SR.GetString("FileProperties");
		}
		#endregion

	}

	[CLSCompliant(false), ComVisible(true)]
	public class SingleFileGeneratorNodeProperties : FileNodeProperties
	{
		#region fields
		private EventHandler<HierarchyNodeEventArgs> onCustomToolChanged;
		private EventHandler<HierarchyNodeEventArgs> onCustomToolNameSpaceChanged;
		protected string customTool = "";
		protected string customToolNamespace = "";

		#endregion

		#region custom tool events
		internal event EventHandler<HierarchyNodeEventArgs> OnCustomToolChanged
		{
			add { onCustomToolChanged += value; }
			remove { onCustomToolChanged -= value; }
		}

		internal event EventHandler<HierarchyNodeEventArgs> OnCustomToolNameSpaceChanged
		{
			add { onCustomToolNameSpaceChanged += value; }
			remove { onCustomToolNameSpaceChanged -= value; }
		}

		#endregion

		#region properties
		[SRCategoryAttribute(SR.Advanced)]
		[LocDisplayName(SR.CustomTool)]
		[SRDescriptionAttribute(SR.CustomToolDescription)]
		public virtual string CustomTool
		{
			get
			{
				customTool = this.Node.ItemNode.GetMetadata(ProjectFileConstants.Generator);
				return customTool;
			}
			set
			{
				customTool = value;
				if (!string.IsNullOrEmpty(customTool))
				{
					this.Node.ItemNode.SetMetadata(ProjectFileConstants.Generator, customTool);
					HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(this.Node);
					if (onCustomToolChanged != null)
					{
						onCustomToolChanged(this.Node, args);
					}
				}
			}
		}

		[SRCategoryAttribute(Microsoft.VisualStudio.Package.SR.Advanced)]
		[LocDisplayName(SR.CustomToolNamespace)]
		[SRDescriptionAttribute(SR.CustomToolNamespaceDescription)]
		public virtual string CustomToolNamespace
		{
			get
			{
				customToolNamespace = this.Node.ItemNode.GetMetadata(ProjectFileConstants.CustomToolNamespace);
				return customToolNamespace;
			}
			set
			{
				customToolNamespace = value;
				if (!string.IsNullOrEmpty(customToolNamespace))
				{
					this.Node.ItemNode.SetMetadata(ProjectFileConstants.CustomToolNamespace, customToolNamespace);
					HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(this.Node);
					if (onCustomToolNameSpaceChanged != null)
					{
						onCustomToolNameSpaceChanged(this.Node, args);
					}
				}
			}
		}
		#endregion

		#region ctors
		public SingleFileGeneratorNodeProperties()
		{
		}

		public SingleFileGeneratorNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion	
	}

	[CLSCompliant(false), ComVisible(true)]
	public class ProjectNodeProperties : NodeProperties
	{
		#region properties
		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.ProjectFolder)]
		[SRDescriptionAttribute(SR.ProjectFolderDescription)]
		[AutomationBrowsable(false)]
		public string ProjectFolder
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.ProjectMgr.ProjectFolder;
			}
		}

		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.ProjectFile)]
		[SRDescriptionAttribute(SR.ProjectFileDescription)]
		[AutomationBrowsable(false)]
		public string ProjectFile
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.ProjectMgr.ProjectFile;
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				this.Node.ProjectMgr.ProjectFile = value;
			}
		}

		#region non-browsable properties - used for automation only
		[Browsable(false)]
		public string FileName
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.ProjectMgr.ProjectFile;
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				this.Node.ProjectMgr.ProjectFile = value;
			}
		}


		[Browsable(false)]
		public string FullPath
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");
				string fullPath = this.Node.ProjectMgr.ProjectFolder;
				if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					return fullPath + Path.DirectorySeparatorChar;
				}
				else
				{
					return fullPath;
				}
			}
		}
		#endregion

		#endregion

		#region ctors
		public ProjectNodeProperties()
		{
		}

		public ProjectNodeProperties(ProjectNode node)
			: base(node)
		{
		}
		#endregion

		#region overridden methods
		public override string GetClassName()
		{
			return SR.GetString("ProjectProperties");
		}

		/// <summary>
		/// ICustomTypeDescriptor.GetEditor
		/// To enable the "Property Pages" button on the properties browser
		/// the browse object (project properties) need to be unmanaged
		/// or it needs to provide an editor of type ComponentEditor.
		/// </summary>
		/// <param name="editorBaseType">Type of the editor</param>
		/// <returns>Editor</returns>
		public override object GetEditor(Type editorBaseType)
		{
			// Override the scenario where we are asked for a ComponentEditor
			// as this is how the Properties Browser calls us
			if (editorBaseType == typeof(ComponentEditor))
			{
				IOleServiceProvider sp;
				ErrorHandler.ThrowOnFailure(this.Node.GetSite(out sp));
				return new PropertiesEditorLauncher(new ServiceProvider(sp));
			}

			return base.GetEditor(editorBaseType);
		}

		public override int GetCfgProvider(out IVsCfgProvider p)
		{
			Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

			if (this.Node != null && this.Node.ProjectMgr != null)
			{
				return this.Node.ProjectMgr.GetCfgProvider(out p);
			}

			return base.GetCfgProvider(out p);
		}
		#endregion
	}

	[CLSCompliant(false), ComVisible(true)]
	public class FolderNodeProperties : NodeProperties
	{
		#region properties
		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.FolderName)]
		[SRDescriptionAttribute(SR.FolderNameDescription)]
		[AutomationBrowsable(false)]
		public string FolderName
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.Caption;
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");
				this.Node.SetEditLabel(value);
				this.Node.ReDraw(UIHierarchyElement.Caption);
			}
		}

		#region properties - used for automation only
		[Browsable(false)]
		[AutomationBrowsable(true)]
		public string FileName
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.Caption;
			}
			set
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				this.Node.SetEditLabel(value);
			}
		}

		[Browsable(false)]
		[AutomationBrowsable(true)]
		public string FullPath
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				string fullPath = this.Node.GetMkDocument();
				if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					return fullPath + Path.DirectorySeparatorChar;
				}
				else
				{
					return fullPath;
				}
			}
		}
		#endregion

		#endregion

		#region ctors
		public FolderNodeProperties()
		{
		}
		public FolderNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region overridden methods
		public override string GetClassName()
		{
			return SR.GetString("FolderProperties");
		}
		#endregion
	}

	[CLSCompliant(false), ComVisible(true)]
	public class ReferenceNodeProperties : NodeProperties
	{
		#region properties
		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.RefName)]
		[SRDescriptionAttribute(SR.RefNameDescription)]
		[Browsable(true)]
		[AutomationBrowsable(true)]
		public override string Name
		{
			get
			{
				Debug.Assert(this.Node != null, "The associated hierarchy node has not been initialized");

				return this.Node.Caption;
			}
		}

		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.CopyToLocal)]
		[SRDescriptionAttribute(SR.CopyToLocalDescription)]
		public bool CopyToLocal
		{
			get
			{
				string copyLocal = this.GetProperty(ProjectFileConstants.Private, "False");
				if (copyLocal == null || copyLocal.Length == 0)
					return true;
				return bool.Parse(copyLocal);
			}
			set
			{
				this.SetProperty(ProjectFileConstants.Private, value.ToString());
			}
		}

		[SRCategoryAttribute(SR.Misc)]
		[LocDisplayName(SR.FullPath)]
		[SRDescriptionAttribute(SR.FullPathDescription)]
		public virtual string FullPath
		{
			get
			{
				return this.Node.Url;
			}
		}
		#endregion

		#region ctors
		public ReferenceNodeProperties()
		{
		}
		public ReferenceNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region overridden methods
		public override string GetClassName()
		{
			return SR.GetString("ReferenceProperties");
		}
		#endregion
	}

	[ComVisible(true)]
	public class ProjectReferencesProperties : ReferenceNodeProperties
	{
		#region ctors
		public ProjectReferencesProperties(ProjectReferenceNode node)
			: base(node)
		{
		}
		#endregion

		#region overriden methods
		public override string FullPath
		{
			get
			{
				return ((ProjectReferenceNode)Node).ReferencedProjectOutputPath;
			}
		}
		#endregion
	}
}
