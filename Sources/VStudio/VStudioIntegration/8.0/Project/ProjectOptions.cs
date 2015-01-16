/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Specialized;

namespace Microsoft.VisualStudio.Package
{
	
	public class ProjectOptions : System.CodeDom.Compiler.CompilerParameters
	{
		
		public ModuleKindFlags ModuleKind = ModuleKindFlags.ConsoleApplication;
		
		public bool EmitManifest = true;
		//public bool IsModule; // difference between .dll and .netmodule.
		
		public StringCollection DefinedPreProcessorSymbols;
		
		public string XMLDocFileName;
		
		public string RecursiveWildcard;
		
		public StringCollection ReferencedModules;
		
		public string Win32Icon;
		
		public bool PDBOnly;
		
		public bool Optimize;
		
		public bool IncrementalCompile;
		
		public int[] SuppressedWarnings;
		
		public bool CheckedArithmetic;
		
		public bool AllowUnsafeCode;
		
		public bool DisplayCommandLineHelp;
		
		public bool SuppressLogo;
		
		public long BaseAddress;
		
		public string BugReportFileName;
		
		/// <devdoc>must be an int if not null</devdoc>
		public object CodePage;
		
		public bool EncodeOutputInUTF8;
		
		public bool FullyQualifiyPaths;
		
		public int FileAlignment;
		
		public bool NoStandardLibrary;
		
		public StringCollection AdditionalSearchPaths;
		
		public bool HeuristicReferenceResolution;
		
		public string RootNamespace;
		
		public bool CompileAndExecute;
		
		/// <devdoc>must be an int if not null.</devdoc>
		public object UserLocaleId;
		
		public PlatformType TargetPlatform;
		
		public string TargetPlatformLocation;
		
		public virtual string GetOptionHelp()
		{
			return null;
		}
	}
}
