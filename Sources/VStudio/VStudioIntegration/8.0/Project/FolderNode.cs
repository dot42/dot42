/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace Microsoft.VisualStudio.Package
{
	[CLSCompliant(false)]
	[ComVisible(true)]
	public class FolderNode : HierarchyNode
	{
		#region ctors
		/// <summary>
		/// Constructor for the FolderNode
		/// </summary>
		/// <param name="root">Root of the hierarchy</param>
		/// <param name="strDirectoryPath">Root of the hierarchy</param>
		/// <param name="e">Associated project element</param>
		public FolderNode(ProjectNode root, string strDirectoryPath, ProjectElement e)
			: base(root, e)
		{
			this.VirtualNodeName = strDirectoryPath.TrimEnd('\\'); //complete path from root project i.e.: "NewFolder1\\NewFolder2\\NewFolder3
		}
		#endregion

		#region overridden properties
        public override int SortPriority
		{
			get { return DefaultSortOrderNode.FolderNode; }
		}

		/// <summary>
		/// This relates to the SCC glyph
		/// </summary>
		public override VsStateIcon StateIconIndex
		{
			get
			{
				// The SCC manager does not support being asked for the state icon of a folder (result of the operation is undefined)
				return VsStateIcon.STATEICON_NOSTATEICON;
			}
		}
		#endregion

		#region overridden methods
		protected override NodeProperties CreatePropertiesObject()
		{
			return new FolderNodeProperties(this);
		}

		protected internal override void DeleteFromStorage(string path)
		{
			this.DeleteFolder(path);
		}

		/// <summary>
		/// Get the automation object for the FolderNode
		/// </summary>
		/// <returns>An instance of the Automation.OAFolderNode type if succeeded</returns>
		public override object GetAutomationObject()
		{
			if (this.ProjectMgr == null || this.ProjectMgr.IsClosed)
			{
				return null;
			}

			return new Automation.OAFolderItem(this.ProjectMgr.GetAutomationObject() as Automation.OAProject, this);
		}

		public override object GetIconHandle(bool open)
		{
            return this.ProjectMgr.ImageHandler.GetIconHandle(open ? (int)ProjectNode.ImageName.OpenFolder : (int)ProjectNode.ImageName.Folder);
		}

		/// <summary>
		/// Rename Folder
		/// </summary>
		/// <param name="label">new Name of Folder</param>
		/// <returns>VSConstants.S_OK, if succeeded</returns>
		public override int SetEditLabel(string label)
		{
			if (String.Compare(Path.GetFileName(this.Url), label, StringComparison.Ordinal) == 0)
			{
				// Label matches current Name
				return VSConstants.S_OK;
			}

			// Verify that No Directory/file already exists with the new name (both among current children and non-project items
			for (HierarchyNode n = Parent.FirstChild; n != null; n = n.NextSibling)
			{
				if (n != this && String.Compare(n.Caption, label, true) == 0)
				{
					//A file or folder with the name '{0}' already exists on disk at this location. Please choose another name.
					//If this file or folder does not appear in the Solution Explorer, then it is not currently part of your project. To view files which exist on disk, but are not in the project, select Show All Files from the Project menu.
					throw new InvalidOperationException(SR.GetString(SR.FileOrFolderAlreadyExists, new string[] { label }));
				}
			}

			try
			{
				RenameFolder(label);
				IVsUIShell shell = this.ProjectMgr.GetService(typeof(SVsUIShell)) as IVsUIShell;
				Debug.Assert(shell != null, "Could not get the ui shell from the project");
				ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(0));
				// Notify the listeners that the name of this folder is changed. This will
				// also force a refresh of the SolutionExplorer's node.
				this.OnPropertyChanged(this, (int)__VSHPROPID.VSHPROPID_Caption, 0);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.RenameFolder), e.Message));
			}
			return VSConstants.S_OK;
		}

		public override int MenuCommandId
		{
			get { return VsMenus.IDM_VS_CTXT_FOLDERNODE; }
		}

		public override Guid ItemTypeGuid
		{
			get
			{
				return VSConstants.GUID_ItemType_PhysicalFolder;
			}
		}

		public override string Url
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(this.ProjectMgr.Url), this.VirtualNodeName) + "\\";
			}
		}

		public override string Caption
		{
			get
			{
				// it might have a backslash at the end... 
				// and it might consist of Grandparent\parent\this\
				string caption = this.VirtualNodeName;
				string[] parts;
				parts = caption.Split(Path.DirectorySeparatorChar);
				caption = parts[parts.GetUpperBound(0)];
				return caption;
			}
		}

		public override string GetMkDocument()
		{
			Debug.Assert(this.Url != null, "No url sepcified for this node");

			return this.Url;
		}

		/// <summary>
		/// Enumerate the files associated with this node.
		/// A folder node is not a file and as such no file to enumerate.
		/// </summary>
		/// <param name="files">The list of files to be placed under source control.</param>
		/// <param name="flags">The flags that are associated to the files.</param>
		protected internal override void GetSccFiles(System.Collections.Generic.IList<string> files, System.Collections.Generic.IList<tagVsSccFilesFlags> flags)
		{
			return;
		}

		/// <summary>
		/// Recursevily walks the folder nodes and redraws the state icons
		/// </summary>
		protected internal override void UpdateSccStateIcons()
		{
			for (HierarchyNode child = this.FirstChild; child != null; child = child.NextSibling)
			{
				child.UpdateSccStateIcons();
			}
		}

		protected override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
		{
			if (guidCmdGroup == VsMenus.guidStandardCommandSet97)
			{
				switch ((VsCommands)cmd)
				{
					case VsCommands.Copy:
					case VsCommands.Paste:
					case VsCommands.Cut:
					case VsCommands.Rename:
						result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
						return VSConstants.S_OK;

					case VsCommands.NewFolder:
					case VsCommands.AddNewItem:
					case VsCommands.AddExistingItem:
						result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
						return VSConstants.S_OK;
				}
			}
			else if (guidCmdGroup == VsMenus.guidStandardCommandSet2K)
			{
				if ((VsCommands2K)cmd == VsCommands2K.EXCLUDEFROMPROJECT)
				{
					result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
					return VSConstants.S_OK;
				}
			}
			else
			{
				return (int)OleConstants.OLECMDERR_E_UNKNOWNGROUP;
			}
			return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
		}

		protected override bool CanDeleteItem(__VSDELETEITEMOPERATION deleteOperation)
		{
			if (deleteOperation == __VSDELETEITEMOPERATION.DELITEMOP_DeleteFromStorage)
			{
				return this.ProjectMgr.CanProjectDeleteItems;
			}
			return false;
		}

		#endregion

		#region virtual methods
		/// <summary>
		/// Override if your node is not a file system folder so that
		/// it does nothing or it deletes it from your storage location.
		/// </summary>
		/// <param name="url">unique identifier of the folder to delete</param>
		public virtual void DeleteFolder(string url)
		{
			if (Directory.Exists(url))
				Directory.Delete(url, true);
		}

		/// <summary>
		/// creates the physical directory for a folder node
		/// Override if your node does not use file system folder
		/// </summary>
		public virtual void CreateDirectory()
		{
			try
			{
				if (Directory.Exists(this.Url) == false)
				{
					Directory.CreateDirectory(this.Url);
				}
			}
			//TODO - this should not digest all exceptions.
			catch (System.Exception e)
			{
				CCITracing.Trace(e);
				throw e;
			}
		}
		/// <summary>
		/// Creates a folder nodes physical directory
		/// Override if your node does not use file system folder
		/// </summary>
		/// <param name="strNewName"></param>
		/// <returns></returns>
		public virtual void CreateDirectory(string strNewName)
		{
			if (String.IsNullOrEmpty(strNewName))
			{
				throw new ArgumentException(SR.GetString(SR.ParameterCannotBeNullOrEmpty), "strNewName");
			}

			try
			{
				// on a new dir && enter, we get called with the same name (so do nothing if name is the same
				char[] dummy = new char[1];
				dummy[0] = Path.DirectorySeparatorChar;
				string oldDir = this.Url;
				oldDir = oldDir.TrimEnd(dummy);
				string strNewDir = Path.Combine(Path.GetDirectoryName(oldDir), strNewName);

				if (String.Compare(strNewDir, oldDir, StringComparison.OrdinalIgnoreCase) != 0)
				{
					if (Directory.Exists(strNewDir))
					{
						throw new InvalidOperationException(SR.GetString(SR.DirectoryExistError));
					}
					Directory.CreateDirectory(strNewDir);
				}
			}
			//TODO - this should not digest all exceptions.
			catch (System.Exception e)
			{
				CCITracing.Trace(e);
				throw e;
			}
		}

		/// <summary>
		/// Rename the physical directory for a folder node
		/// Override if your node does not use file system folder
		/// </summary>
		/// <returns></returns>
		public virtual void RenameDirectory(string newPath)
		{
			if (Directory.Exists(this.Url))
			{
				if (Directory.Exists(newPath))
				{
					string errorMessage = SR.GetString(SR.FileOrFolderAlreadyExists, new string[] { newPath });
					if (!Utilities.IsInAutomationFunction(this.ProjectMgr.Site))
					{
						string title = null;
						OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
						OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
						OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
						VsShellUtilities.ShowMessageBox(this.ProjectMgr.Site, title, errorMessage, icon, buttons, defaultButton);
						return;
					}
					else
					{
						throw new InvalidOperationException(errorMessage);
					}
				}

				Directory.Move(this.Url, newPath);
			}
		}
		#endregion

		#region helper methods
		private void RenameFolder(string newName)
		{
			// Do the rename (note that we only do the physical rename if the leaf name changed)
			string newPath = Path.Combine(this.Parent.VirtualNodeName, newName);
			if (String.Compare(Path.GetFileName(VirtualNodeName), newName, StringComparison.Ordinal) != 0)
				this.RenameDirectory(Path.Combine(this.ProjectMgr.ProjectFolder, newPath));
			this.VirtualNodeName = newPath;

			this.ItemNode.Rename(VirtualNodeName);

			// Let all children know of the new path
			for (HierarchyNode child = this.FirstChild; child != null; child = child.NextSibling)
			{
				if (!(child is FolderNode))
				{
					child.SetEditLabel(child.Caption);
				}
				else
				{
					FolderNode childFolderNode = (FolderNode)child;
					childFolderNode.RenameFolder(childFolderNode.Caption);
				}
			}

			// Some of the previous operation may have changed the selection so set it back to us
			IVsUIHierarchyWindow uiWindow = UIHierarchyUtilities.GetUIHierarchyWindow(this.ProjectMgr.Site, SolutionExplorer);
			ErrorHandler.ThrowOnFailure(uiWindow.ExpandItem(this.ProjectMgr, this.ID, EXPANDFLAGS.EXPF_SelectItem));

		}
		#endregion
	}
}
