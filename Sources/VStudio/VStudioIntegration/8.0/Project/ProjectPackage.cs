/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;
using Microsoft.Win32;
using EnvDTE;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.Package
{
	/// <summary>
	/// Defines abstract package.
	/// </summary>
	[ComVisible(true)]
	[CLSCompliant(false)]
	public abstract class ProjectPackage : Microsoft.VisualStudio.Shell.Package
	{
		#region fields
		// This is the place to register all the solution listeners.
		private List<SolutionListener> solutionListeners = new List<SolutionListener>();
		#endregion 

		#region properties
		/// <summary>
		/// Add your listener to this list. They should be added in the overridden Initialize befaore calling the base.
		/// </summary>
		protected internal List<SolutionListener> SolutionListeners
		{
			get
			{
				return this.solutionListeners;
			}
		}
		#endregion

		#region ctor
		protected ProjectPackage()
		{ 
		
		}
		#endregion

		#region methods

		protected override void Initialize()
		{
			base.Initialize();

			// Subscribe to the solution events
			this.solutionListeners.Add(new SolutionListenerForProjectReferenceUpdate(this));
			this.solutionListeners.Add(new SolutionListenerForProjectOpen(this));
			this.solutionListeners.Add(new SolutionListenerForBuildDependencyUpdate(this));
			this.solutionListeners.Add(new SolutionListenerForProjectEvents(this));

			foreach (SolutionListener solutionListener in this.solutionListeners)
			{
				solutionListener.Init();
			}
		}


		protected override void Dispose(bool disposing)
		{
			// Unadvise solution listeners.
			try
			{
				if (disposing)
				{
					foreach (SolutionListener solutionListener in this.solutionListeners)
					{
						solutionListener.Dispose();
					}
				}
			}
			finally
			{

				base.Dispose(disposing);
			}
		}
		#endregion

	}
}
