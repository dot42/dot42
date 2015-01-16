/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;
using System.Net;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.Package
{
	// TODO (VSSDK): The tracker class will need to implement helpers to CALL IVsTrackProjectDocuments2 for notifications
	internal class TrackDocumentsHelper
	{
		private ProjectNode projectMgr;


		internal TrackDocumentsHelper(ProjectNode project)
		{
			this.projectMgr = project;
		}

		private IVsTrackProjectDocuments2 GetIVsTrackProjectDocuments2()
		{
			Debug.Assert(this.projectMgr != null && !this.projectMgr.IsClosed && this.projectMgr.Site != null);
			
			IVsTrackProjectDocuments2 documentTracker = this.projectMgr.Site.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
			if (documentTracker == null)
			{
				throw new InvalidOperationException();
			}
			
			return  documentTracker;
		}

		/// <summary>
		/// Calls the tracker with OnQueryAddFiles
		/// </summary>
		/// <param name="files">The files to add.</param>
		/// <param name="flags">The VSQUERYADDFILEFLAGS flags associated to teh files added</param>
		/// <returns>true if the file can be added, false if not.</returns>
		internal bool CanAddItems(string[] files, VSQUERYADDFILEFLAGS[] flags)
		{
			// If we are silent then we assume that the file can be added, since we do not want to trigger this event.
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
			{
				return true;
			}
			
			if (files == null || files.Length == 0)
			{
				return false;
			}

			int len = files.Length;
			VSQUERYADDFILERESULTS[] summary = new VSQUERYADDFILERESULTS[1];
			ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryAddFiles(this.projectMgr, len, files, flags, summary, null));
			if (summary[0] == VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK)
			{
				return false;
			}
			
			return true;
		}


		/// <summary>
		/// Gets called when an item is added.
		/// </summary>
		internal void OnItemAdded(string file,  VSADDFILEFLAGS flag)
		{
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
			{
				ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterAddFilesEx(this.projectMgr, 1, new string[1] { file }, new VSADDFILEFLAGS[1] { flag }));
			}
		}

		/// <summary>
		///  Get's called to tell the env that a file can be deleted.
		/// </summary>
		internal bool CanRemoveItems(string[] files, VSQUERYREMOVEFILEFLAGS[] flags)
		{
			// If we are silent then we assume that the file can be removed, since we do not want to trigger this event.
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
			{
				return true;
			}

			if (files == null || files.Length == 0)
			{
				return false;
			}
			int length = files.Length;

			VSQUERYREMOVEFILERESULTS[] summary = new VSQUERYREMOVEFILERESULTS[1];

			ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryRemoveFiles(this.projectMgr, length, files, flags, summary, null));
			if (summary[0] == VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK)
			{
				return false;
			}

			return true;
		}


		/// <summary>
		/// Gets called when an item is removed.
		/// </summary>
		internal void OnItemRemoved(string file, VSREMOVEFILEFLAGS flag)
		{
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
			{
				ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterRemoveFiles(this.projectMgr, 1, new string[1] { file }, new VSREMOVEFILEFLAGS[1] { flag }));
			}
		}
		
		/// <summary>
		/// Get's called to ask the environent if a file is allowed to be renamed
		/// </summary>
		/// returns FALSE if the doc can not be renamed
		internal bool CanRenameItem(string strOldName, string strNewName, VSRENAMEFILEFLAGS flag)
		{
			// If we are silent then we assume that the file can be renamed, since we do not want to trigger this event.
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
			{
				return true;
			}

			int iCanContinue = 0;
			ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnQueryRenameFile(this.projectMgr, strOldName, strNewName, flag, out iCanContinue));
			return (iCanContinue != 0);
		}

		/// <summary>
		/// Get's called to tell the env that a file was renamed
		/// </summary>
		/// 
		internal void OnItemRenamed(string strOldName, string strNewName, VSRENAMEFILEFLAGS flag)
		{
			if ((this.projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
			{
				ErrorHandler.ThrowOnFailure(this.GetIVsTrackProjectDocuments2().OnAfterRenameFile(this.projectMgr, strOldName, strNewName, flag));
			}
		}		
	}
}

