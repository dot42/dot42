/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Xml;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.Package
{
	#region structures
	[StructLayoutAttribute(LayoutKind.Sequential)]
	internal struct _DROPFILES
	{
		public Int32 pFiles;
		public Int32 X;
		public Int32 Y;
		public Int32 fNC;
		public Int32 fWide;
	}
	#endregion

	#region enums
	[PropertyPageTypeConverterAttribute(typeof(OutputTypeConverter))]
	public enum OutputType { Library, WinExe, Exe }

	[PropertyPageTypeConverterAttribute(typeof(DebugModeConverter))]
	public enum DebugMode { Project, Program, URL }

	[PropertyPageTypeConverterAttribute(typeof(BuildActionConverter))]
	public enum BuildAction { None, Compile, Content, EmbeddedResource }


	[PropertyPageTypeConverterAttribute(typeof(PlatformTypeConverter))]
	public enum PlatformType { notSpecified, v1, v11, v2, cli1 }

	[Flags]
	public enum PropPageStatus
	{

		Dirty = 0x1,

		Validate = 0x2,

		Clean = 0x4
	}

	[Flags]
	public enum ModuleKindFlags
	{

		ConsoleApplication,

		WindowsApplication,

		DynamicallyLinkedLibrary,

		ManifestResourceFile,

		UnmanagedDynamicallyLinkedLibrary
	}

	[Flags]
	public enum QueryStatusResult
	{
		NOTSUPPORTED = 0,
		SUPPORTED = 1,
		ENABLED = 2,
		LATCHED = 4,
		NINCHED = 8,
		INVISIBLE = 16
	}

	public enum HierarchyAddType
	{

		AddNewItem,

		AddExistingItem
	}

	public enum CommandOrigin
	{
		UiHierarchy,
		OleCommandTarget
	}

	public enum MSBuildResult
	{ 		
		Suspended,
		Resumed,
		Failed,
		Sucessful,
	}

	public enum WindowFrameShowAction
	{ 
		DontShow,
		Show,
		ShowNoActivate,
		Hide,
	}

	internal enum DropDataType
	{//Drop types
		None,
		Shell,
		VsStg,
		VsRef
	}

	/// <summary>
	/// Used by the hierarchy node to decide which element to redraw.
	/// </summary>
	[Flags]
	public enum UIHierarchyElement
	{ 
		None = 0,
		/// <summary>
		/// This will be translated to VSHPROPID_IconIndex
		/// </summary>
		Icon = 1,
		/// <summary>
		/// This will be translated to VSHPROPID_StateIconIndex
		/// </summary>
		SccState = 2,
		/// <summary>
		/// This will be translated to VSHPROPID_Caption
		/// </summary>
		Caption = 4
	}
	#endregion

	public class AfterProjectFileOpenedEventArgs : EventArgs
	{
		#region fields
		private bool added;
		#endregion

		#region properties
		/// <summary>
		/// True if the project is added to the solution after the solution is opened. false if the project is added to the solution while the solution is being opened.
		/// </summary>
		internal bool Added
		{
			get { return this.added; }
		}
		#endregion

		#region ctor
		internal AfterProjectFileOpenedEventArgs(bool added)
		{
			this.added = added;
		}
		#endregion
	}

	public class BeforeProjectFileClosedEventArgs : EventArgs
	{
		#region fields
		private bool removed;
		#endregion

		#region properties
		/// <summary>
		/// true if the project was removed from the solution before the solution was closed. false if the project was removed from the solution while the solution was being closed.
		/// </summary>
		internal bool Removed
		{
			get { return this.removed; }
		}
		#endregion

		#region ctor
		internal BeforeProjectFileClosedEventArgs(bool removed)
		{
			this.removed = removed;
		}
		#endregion
	}

	/// <summary>
	/// This class is used for the events raised by a HierarchyNode object.
	/// </summary>
	internal class HierarchyNodeEventArgs : EventArgs
	{
		private HierarchyNode child;
		
		internal HierarchyNodeEventArgs(HierarchyNode child)
		{
			this.child = child;
		}

		public HierarchyNode Child
		{
			get { return this.child; }
		}
	}

	/// <summary>
	/// Event args class for triggering file change event arguments.
	/// </summary>
	internal class FileChangedOnDiskEventArgs : EventArgs
	{
		#region Private fields
		/// <summary>
		/// File name that was changed on disk.
		/// </summary>
		private string fileName;

		/// <summary>
		/// The item ide of the file that has changed.
		/// </summary>
		private uint itemID;
		#endregion

		/// <summary>
		/// Constructs a new event args.
		/// </summary>
		/// <param name="fileName">File name that was changed on disk.</param>
		/// <param name="id">The item id of the file that was changed on disk.</param>
		internal FileChangedOnDiskEventArgs(string fileName, uint id)
		{
			this.fileName = fileName;
			this.itemID = id; 
		}

		/// <summary>
		/// Gets the file name that was changed on disk.
		/// </summary>
		/// <value>The file that was changed on disk.</value>
		internal string FileName
		{
			get
			{
				return this.fileName;
			}
		}

		/// <summary>
		/// Gets item id of the file that has changed
		/// </summary>
		/// <value>The file that was changed on disk.</value>
		internal uint ItemID
		{
			get
			{
				return this.itemID;
			}
		}
	}
} 
