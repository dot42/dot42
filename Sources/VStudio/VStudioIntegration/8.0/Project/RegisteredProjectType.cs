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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace Microsoft.VisualStudio.Package
{
	/// <summary>
	/// Gets registry settings from for a project.
	/// </summary>
	internal class RegisteredProjectType
	{
		private string defaultProjectExtension;

		private string projectTemplatesDir;

		private string wizardTemplatesDir;

		private Guid packageGuid;

		internal const string DefaultProjectExtension = "DefaultProjectExtension";
		internal const string WizardsTemplatesDir = "WizardsTemplatesDir";
		internal const string ProjectTemplatesDir = "ProjectTemplatesDir";
		internal const string Package = "Package";



		internal string DefaultProjectExtensionValue
		{
			get
			{
				return this.defaultProjectExtension;
			}
			set
			{
				this.defaultProjectExtension = value;
			}
		}

		internal string ProjectTemplatesDirValue
		{
			get
			{
				return this.projectTemplatesDir;
			}
			set
			{
				this.projectTemplatesDir = value;
			}
		}

		internal string WizardTemplatesDirValue
		{
			get
			{
				return this.wizardTemplatesDir;
			}
			set
			{
				this.wizardTemplatesDir = value;
			}
		}

		internal Guid PackageGuidValue
		{
			get
			{
				return this.packageGuid;
			}
			set
			{
				this.packageGuid = value;
			}
		}

		/// <summary>
		/// If the project support VsTemplates, returns the path to
		/// the vstemplate file corresponding to the requested template
		/// 
		/// You can pass in a string such as: "Windows\Console Application"
		/// </summary>
		internal string GetVsTemplateFile(string templateFile)
		{
			// First see if this use the vstemplate model
			if (!String.IsNullOrEmpty(DefaultProjectExtensionValue))
			{
				EnvDTE80.DTE2 dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
				if (dte != null)
				{
					EnvDTE80.Solution2 solution = dte.Solution as EnvDTE80.Solution2;
					if (solution != null)
					{
						return solution.GetProjectTemplate(templateFile, DefaultProjectExtensionValue);
					}
				}

			}
			return null;
		}

		internal static RegisteredProjectType CreateRegisteredProjectType(EnvDTE.DTE dte, Guid projectTypeGuid)
		{

			RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(dte.RegistryRoot);
			if (rootKey == null)
			{
				return null;
			}

			RegistryKey projectsKey = rootKey.OpenSubKey("Projects");
			if (projectsKey == null)
			{
				return null;
			}

			RegistryKey projectKey = projectsKey.OpenSubKey(projectTypeGuid.ToString("B"));

			if (projectKey == null)
			{
				return null;
			}

			RegisteredProjectType registederedProjectType = new RegisteredProjectType();
			registederedProjectType.DefaultProjectExtensionValue = projectKey.GetValue(DefaultProjectExtension) as string;
			registederedProjectType.ProjectTemplatesDirValue = projectKey.GetValue(ProjectTemplatesDir) as string;
			registederedProjectType.WizardTemplatesDirValue = projectKey.GetValue(WizardsTemplatesDir) as string;
			registederedProjectType.PackageGuidValue = new Guid(projectKey.GetValue(Package) as string);

			return registederedProjectType;
		}
	}
}
