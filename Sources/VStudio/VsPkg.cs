#define INCLUDE_EDITOR

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using Dot42.Ide;
using Dot42.Utility;
using Dot42.VStudio.Debugger;
using Dot42.VStudio.Editors;
using Dot42.VStudio.Extenders;
using Dot42.VStudio.Flavors;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Dot42.VStudio
{
    //
    // KEEP THESE CONSTANT:
    // - Namespace
    // - Classname
    // - Guids
    // - Attributes
    //
    // The installer depends on it!
    //

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [
        PackageRegistration(UseManagedResourcesOnly = true),
        ProvideAutoLoad(UIContextGuids.SolutionExists),
        ProvideAutoLoad(UIContextGuids.SolutionHasSingleProject),
        ProvideAutoLoad(UIContextGuids.SolutionHasMultipleProjects),
        ProvideProjectFactory(typeof(Dot42ProjectFlavorFactory), null, null, null, null, null, TemplateGroupIDsVsTemplate = "dot42"),
        DefaultRegistryRoot(@"Software\Microsoft\VisualStudio\10.0Exp"),
        InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400),
        // Register project type
        //ProvideFlavoredProjectFactory(typeof(Dot42ProjectFactory)),
        //ProvideObject(typeof(BuildPropertyPage)),
        ProvideMenuResource(2000, 1),
        // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
        // package needs to have a valid load key (it can be requested at 
        // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
        // package has a load key embedded in its resources.
        //ProvideLoadKey("Standard", "1.0", "dot42 C# for Android", "dot42", 104),
        ProvideObject(typeof(AndroidPropertyPage)),
        Clsid(typeof(DebugEngine)),
        Clsid(typeof(DebugPortSupplier)),
        Clsid(typeof(DebugProgramProvider)),
        Guid(GuidList.Strings.guidDot42Package),
        ProvideDebugEngine(
            GuidList.Strings.guidDot42DebuggerId,
            "dot42 Debug Engine",
            PortSupplier = GuidList.Strings.guidDot42PortSupplierId,
            ProgramProvider = GuidList.Strings.guidDot42ProgramProviderClsid, 
            CLSID = GuidList.Strings.guidDot42DebugEngineClsid,
            AlwaysLoadLocal = true,
            Attach = false, 
            AutoSelectPriority = 4,
            CallStackBP = true,
            Exceptions = true,
            Disassembly = true,
            //RemoteDebugging = true
            SuspendThread = true
            ),
        ProvidePortSupplier(
            CLSID = GuidList.Strings.guidDot42PortSupplierClsid,
            ID = GuidList.Strings.guidDot42PortSupplierId,
            Name = DebugPortSupplier.Name),
        ProvideIncompatibleEngineInfo("{032F4B8C-7045-4B24-ACCF-D08C9DA108FE}"), // Silverlight
        ProvideIncompatibleEngineInfo("{3B476D35-A401-11D2-AAD4-00C04F990171}"), // NativeOnlyEng
        ProvideIncompatibleEngineInfo("{A7D86ADA-3833-4259-B9CC-2E991EAB3F75}"), // ManagedXbox360
        ProvideIncompatibleEngineInfo("{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}"), // Scriptengine
#if INCLUDE_EDITOR
	    ProvideEditorExtension(typeof (XmlEditorFactory), ".xml", 50, DefaultName = "dot42 Xml Editor"),
        ProvideEditorLogicalView(typeof(XmlEditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}"), // LOGVIEWID_Designer
        ProvideEditorLogicalView(typeof(XmlEditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}"), // LOGVIEWID_Code
#endif
        // Exceptions (make sure to provide all levels in the tree
        ProvideDebugException(GuidList.Strings.guidDot42DebuggerId, ExceptionConstants.TopLevelCode, ExceptionConstants.TopLevelName),
        // Tool windows
        ProvideToolWindow(typeof(LogCatToolWindow)),
        // Do not rename type
        Obfuscation(ApplyToMembers = false)
    ]
    public sealed partial class Dot42Package : ProjectPackage, IVsInstalledProduct
    {
        private EnvDTE.DTE dte;
        private EnvDTE.ObjectExtenders extensionMgr;
        private readonly List<int> extenderCookies = new List<int>();
        private IVsActivityLog log;
        private readonly IIde ide = new Services.Ide();
        private static string dteVersion;
        private static Dot42Package instance;

#if ANDROID
        internal const Targets Target = Targets.Android;
#elif BB
        internal const Targets Target = Targets.BlackBerry;
#endif

        /// <summary>
        /// Initialize the <see cref="Locations"/> class.
        /// </summary>
        internal static void InitLocations()
        {
            Locations.Target = Target;
        }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public Dot42Package()
        {
            instance = this;
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            InitLocations();
        }

        /// <summary>
        /// Gets the single instance.
        /// </summary>
        internal static Dot42Package Instance { get { return instance; } }

        /// <summary>
        /// Add a log message to the activity log.
        /// </summary>
        /// <param name="message"></param>
        internal void Log(string message)
        {
            IVsActivityLog log = this.ActivityLog;
            if (log != null)
            {
                log.LogEntry((UInt32) __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, this.ToString(), message);
            }
            else
            {
#if DEBUG
                Trace.WriteLine("Failed to log: " + message);
#endif
            }
        }

        /// <summary>
        /// Gets the activity log
        /// </summary>
        private IVsActivityLog ActivityLog
        {
            get
            {
                if (log == null)
                {
                    log = GetService(typeof (SVsActivityLog)) as IVsActivityLog;
                }
                return log;
            }
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            // Base initialize
            base.Initialize();

            // Register project factories
            try
            {
                this.RegisterProjectFactory(new Dot42ProjectFlavorFactory(this));
                Trace.WriteLine("Registered ObfuscationProjectFactory");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Error {0}: {1}\n{2}", ex.GetType().FullName,
                                                                   ex.Message, ex.StackTrace));
            }

#if INCLUDE_EDITOR
            // Register editor factories
            try
            {
                RegisterEditorFactory(new XmlEditorFactory(this));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Error {0}: {1}\n{2}", ex.GetType().FullName, ex.Message, ex.StackTrace));
            }
#endif

            try
            {
                // Get DTE
                dte = (EnvDTE.DTE) this.GetService(typeof (EnvDTE.DTE));
                if (dte == null)
                {
                    throw new InvalidOperationException("EnvDTE.DTE not found");
                }
                dteVersion = dte.Version;

                // Get extender manager
                this.extensionMgr = (EnvDTE.ObjectExtenders) GetService(typeof (EnvDTE.ObjectExtenders));
                if (extensionMgr == null)
                {
                    throw new InvalidOperationException("ObjectExtenders not found");
                }

                foreach (string catid in ResourceExtender.CategoryIds)
                {
                    extenderCookies.Add(extensionMgr.RegisterExtenderProvider(catid, ResourceExtender.Name, new ResourceExtenderProvider(this), "Dot42 Resource Extender"));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "ObjectExtender registration failed: {0}",
                                              ex.Message));
            }

            try
            {
                // Add our command handlers for menu (commands must exist in the .vsct file) 
                var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (null != mcs)
                {
                    // Create the command for the tool window 
                    var logCatCommandId = new CommandID(GuidList.Guids.guidDot42ProjectCmdSet, (int)PkgCmdIds.cmdidLogCatTool);
                    var menuToolWin = new MenuCommand(ShowLogCatWindow, logCatCommandId);
                    mcs.AddCommand(menuToolWin);
                } 
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Menu initialization failed: {0}",
                                              ex.Message));                
            }
        }

        /// <summary>
        /// Show the logcat tool window
        /// </summary>
        private void ShowLogCatWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance 
            // is actually the only one. 
            // The last flag is set to true so that if the tool window does not exists it will be created. 
            var window = FindToolWindow(typeof(LogCatToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create Device log window");
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show()); 
        }

        internal static string DteVersion
        {
            get
            {
                if (dteVersion == null)
                    throw new InvalidOperationException("Not initialized yet");
                return dteVersion;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (extensionMgr != null) 
            {
                foreach (int cookie in extenderCookies)
                {
                    extensionMgr.UnregisterExtenderProvider(cookie);
                }
            }
            base.Dispose(disposing);
        }

#if VS10
        /// <summary>
        /// Used as application title (so it seems)
        /// </summary>
        public override string ProductUserContext
        {
            get { return "Dot42"; }
        }
#endif

        /// <summary>
        /// Get a service from VisualStudio.
        /// </summary>
        internal new object GetService(Type serviceType)
        {
            if (serviceType == typeof (IIde)) return ide;
            return base.GetService(serviceType);
        }

        /// <summary>
        /// Gets my IDE implementation
        /// </summary>
        public IIde Ide { get { return ide; } }

        /// <summary>
        /// Creates the specified instance by type.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TClass">The type of the class.</typeparam>
        /// <returns></returns>
        public TInterface CreateInstance<TInterface, TClass>()
            where TInterface : class
            where TClass : class
        {
            var clsid = typeof(TClass).GUID;
            var riid = typeof(TInterface).GUID;

            return (TInterface)CreateInstance(ref clsid, ref riid, typeof(TInterface));
        }

        #region IVsInstalledProduct Members

        int IVsInstalledProduct.IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 300;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 400;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.OfficialName(out string pbstrName)
        {
            pbstrName = VSPackage._110;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = VSPackage._112;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductID(out string pbstrPID)
        {
            pbstrPID = this.GetType().Assembly.GetName().Version.ToString(3);
            return VSConstants.S_OK;
        }

        #endregion
    }
}