using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dot42.Graphics;
using Dot42.Ide.Project;
using Dot42.VStudio.ProjectBase;
using Dot42.VStudio.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using ProjectNode = Dot42.VStudio.ProjectBase.ProjectNode;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Dot42.VStudio.Flavors
{
    internal sealed partial class Dot42Project : ProjectNode, IVsProjectFlavorCfgProvider
    {
        private const string BuildPropertyPageGuid = "{1E78F8DB-6C07-4D61-A18F-7514010ABD56}";

        private static readonly IVsSolution solution = (IVsSolution)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsSolution)); 
        private static readonly EnvDTE.DTE dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE));
        private readonly ReferencesList referencesList;
        private uint referencesListCookie = VSConstants.VSCOOKIE_NIL;
        private ItemId? referenceContainerItemId;
        private ImageHandler imageHandler;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="package"></param>
        public Dot42Project(Dot42Package package) : base(package)
        {
            referencesList = new ReferencesList(this);
            FileAdded += OnFileAdded;
        }

        /// <summary>
        /// File has been added.
        /// Maybe we need to update it's type.
        /// </summary>
        private void OnFileAdded(object sender, ProjectDocumentsChangeEventArgs e)
        {
            var mkDocument = e.MkDocument;
            uint itemId;
            if (!ErrorHandler.Succeeded(ParseCanonicalName(e.MkDocument, out itemId)))
                return;
            if (!File.Exists(mkDocument))
                return;
            string itemType;
            var frameworkFolder = GetFrameworkFolder();
            if (!ItemTypeDetector.TryDetectItemType(mkDocument, frameworkFolder, out itemType))
                return;
            // Found item type, set it
            var pItem = _innerVsHierarchy.GetProjectItemFromHierarchy(itemId);
            pItem.Properties.Item("ItemType").Value = itemType;
        }

        /// <summary>
        /// Gets the strongly type package.
        /// </summary>
        public new Dot42Package Package
        {
            get { return (Dot42Package) base.Package; }
        }

        /// <summary>
        /// Gets my image handler
        /// </summary>
        internal override ImageHandler ImageHandler
        {
            get
            {
                if (imageHandler == null)
                {
                    var imageList = new ImageList();
                    imageList.Images.Add(RawIcons16.jar, Color.Magenta);
                    imageHandler = new ImageHandler(imageList);
                }
                return imageHandler;
            }
        }

        /// <summary>
        /// Allows the base project to ask the project subtype to create an <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsProjectFlavorCfg"/> object corresponding to each one of its (project subtype's) configuration objects.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        /// <param name="pBaseProjectCfg">[in] Pointer to the <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsCfg"/> object of the base project.</param><param name="ppFlavorCfg">[out] The <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsProjectFlavorCfg"/> object of the project subtype.</param>
        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            ppFlavorCfg = new ProjectFlavorCfg(this);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Is the given item a JarReference?
        /// </summary>
        internal bool IsJarReference(ItemId itemId)
        {
            /*object property;
            base.GetProperty(itemId.Value, (int)__VSHPROPID.VSHPROPID_Name, out property);
            var name = property as string;
            return (name != null) && (name.EndsWith(".jar", StringComparison.OrdinalIgnoreCase));*/
            
            var projectItem = GetProjectItem(itemId.Value);
            return (projectItem != null) && (projectItem.ItemType == "JarReference");
        }

        /// <summary>
        /// Is the given item a References node?
        /// </summary>
        internal bool IsReferencesContainer(ItemId itemId)
        {
            // Try cache first
            if (referenceContainerItemId.HasValue)
            {
                return referenceContainerItemId.Value.Equals(itemId);
            }
            // Do full lookup
            object property;
            base.GetProperty(itemId.Value, (int) __VSHPROPID.VSHPROPID_Name, out property);
            var name = property as string;
            if (name == ReferenceContainerNode.ReferencesNodeVirtualName)
            {
                referenceContainerItemId = itemId;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restructure the JAR references
        /// </summary>
        protected override void OnAggregationComplete()
        {
            base.OnAggregationComplete();
            referencesListCookie = AdviseHierarchyEvents(referencesList);
        }

        /// <summary>
        /// Close this project.
        /// </summary>
        protected override void Close()
        {
            if (referencesListCookie != VSConstants.VSCOOKIE_NIL)
            {
                UnadviseHierarchyEvents(referencesListCookie);
                referencesListCookie = VSConstants.VSCOOKIE_NIL;
            }
            base.Close();
        }

        /// <summary>
        /// Get a property by the given id for the given item.
        /// </summary>
        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            switch (propId)
            {
                case (int)__VSHPROPID.VSHPROPID_FirstChild:
                case (int)__VSHPROPID.VSHPROPID_FirstVisibleChild:
                    {
                        if (!IsReferencesContainer(itemId))
                        {
                            // Base call
                            var rc = base.GetProperty(itemId, propId, out property);
                            var nextPropId = (propId == (int) __VSHPROPID.VSHPROPID_FirstChild)
                                                 ? __VSHPROPID.VSHPROPID_NextSibling
                                                 : __VSHPROPID.VSHPROPID_NextVisibleSibling;

                            // Filter out JarReferences
                            while (ErrorHandler.Succeeded(rc))
                            {
                                var childId = ItemId.Get(property);
                                if (childId.IsNil)
                                    break;
                                if (!referencesList.IsJarReference(childId.Value))
                                {
                                    return VSConstants.S_OK;
                                }

                                // Get next
                                rc = base.GetProperty(childId.Value, (int) nextPropId, out property);
                            }

                            // No first child found
                            property = VSConstants.VSITEMID_NIL;
                            return VSConstants.S_OK;
                        }
                        else
                        {
                            // Return the first reference child
                            property = referencesList.GetFirstChild().Value;
                            return VSConstants.S_OK;
                        }
                    }
                case (int)__VSHPROPID.VSHPROPID_NextSibling:
                case (int)__VSHPROPID.VSHPROPID_NextVisibleSibling:
                    {
                        if (!referencesList.Contains(itemId))
                        {
                            // Request for item that is not in the references node.
                            var rc = base.GetProperty(itemId, propId, out property);

                            // Filter out JarReferences
                            while (ErrorHandler.Succeeded(rc))
                            {
                                var childId = ItemId.Get(property);
                                if (childId.IsNil)
                                    break;
                                if (!referencesList.IsJarReference(childId.Value))
                                {
                                    return VSConstants.S_OK;
                                }

                                // Get next
                                rc = base.GetProperty(childId.Value, propId, out property);
                            }

                            // No first child found
                            property = VSConstants.VSITEMID_NIL;
                            return VSConstants.S_OK;
                        }
                        else
                        {
                            // We're in references; return the next reference
                            property = referencesList.GetNextSibling(itemId).Value;
                            return VSConstants.S_OK;
                        }
                    }
                case (int)__VSHPROPID.VSHPROPID_Parent:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            if (referenceContainerItemId.HasValue)
                            {
                                property = referenceContainerItemId.Value;
                                return VSConstants.S_OK;
                            }
                            property = VSConstants.VSITEMID_NIL;
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (int)__VSHPROPID.VSHPROPID_ParentHierarchy:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            property = null;
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (int)__VSHPROPID.VSHPROPID_ParentHierarchyItemid:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            property = VSConstants.VSITEMID_NIL;
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (int)__VSHPROPID.VSHPROPID_IconHandle:
                case (int)__VSHPROPID.VSHPROPID_OpenFolderIconHandle:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            // Get our own jar icon
                            property = ImageHandler.GetIconHandle(0);
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (int)__VSHPROPID.VSHPROPID_IconIndex:
                case (int)__VSHPROPID.VSHPROPID_OpenFolderIconIndex:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            // Force using the IconHandle property
                            property = null;
                            return VSConstants.E_NOTIMPL;
                        }
                    }
                    break;
                case (int)__VSHPROPID.VSHPROPID_OverlayIconIndex:
                    {
                        if (referencesList.IsJarReference(itemId))
                        {
                            // No overlay
                            property = VSOVERLAYICON.OVERLAYICON_NONE;
                            return VSConstants.S_OK;
                        }
                    }
                    break;
                case (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList:
                    {
                        // Get a semicolon-delimited list of clsids of the configuration-independent property pages.
                        ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
                        var list = ((string) property).Split(';').ToList();
                        list.RemoveAll(x => x != BuildPropertyPageGuid);
                        list.Insert(0, new Guid(GuidList.Strings.guidDot42AndroidPropertyPage).ToString("B"));
                        property = string.Join(";", list);
                        return VSConstants.S_OK;
                    }
                case (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
                    {
                        // Get a semicolon-delimited list of clsids of the configuration-dependent property pages.
                        ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
                        var list = ((string) property).Split(';').ToList();
                        list.RemoveAll(x => x != BuildPropertyPageGuid);

                        property = string.Join(";", list);
                        return VSConstants.S_OK;
                    }
            }
            return base.GetProperty(itemId, propId, out property);
        }

        /// <summary>
        /// Query command status.
        /// </summary>
        protected override int QueryStatusCommand(uint itemid, ref Guid pguidCmdGroup, uint cCmds, Microsoft.VisualStudio.OLE.Interop.OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == GuidList.Guids.guidDot42ProjectCmdSet)
            {
                prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                return VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cCmds)
                {
                    case VsCommands.Delete:
                    case VsCommands.Remove:
                        if (referencesList.IsJarReference(itemid))
                        {
                            prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
                            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                        }
                        break;
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                var cmd = (VsCommands2K) cCmds;
                switch (cmd)
                {
                    case VsCommands2K.DELETE:
                    case VsCommands2K.REMOVE:
                        if (referencesList.IsJarReference(itemid))
                        {
                            prgCmds[0].cmdf = (uint) (OLECMDF.OLECMDF_INVISIBLE);
                            return (int) OleConstants.OLECMDERR_E_NOTSUPPORTED;
                        }
                        break;
                        /*case VsCommands2K.EXCLUDEFROMPROJECT:
                    result = VSConstants.S_OK;
                    return true;*/
                }
            }
            else
            {
                
            }
            return base.QueryStatusCommand(itemid, ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        /// <summary>
        /// Execute commands.
        /// </summary>
        protected override int ExecCommand(uint itemid, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == GuidList.Guids.guidDot42ProjectCmdSet)
            {
                var cmd = (PkgCmdIds) nCmdID;
                switch (cmd)
                {
                    case PkgCmdIds.cmdIdAddJarReference:
                        AddJarReference();
                        return VSConstants.S_OK;
                    case PkgCmdIds.cmdIdImportResFolder:
                        ImportResFolder();
                        return VSConstants.S_OK;
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                var cmd = (VsCommands) nCmdID;
                switch (cmd)
                {
                    case VsCommands.Delete:
                    case VsCommands.Remove:
                        if (referencesList.IsJarReference(itemid))
                        {
                            return (int) OleConstants.OLECMDERR_E_NOTSUPPORTED;
                        }
                        break;
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                var cmd = (VsCommands2K) nCmdID;
                switch (cmd)
                {
                    case VsCommands2K.DELETE:
                    case VsCommands2K.REMOVE:
                        if (referencesList.IsJarReference(itemid))
                        {
                            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
                        }
                        break;
                    /*case VsCommands2K.EXCLUDEFROMPROJECT:
                    result = VSConstants.S_OK;
                    return true;*/
                }
            }
            return base.ExecCommand(itemid, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Provide UI to add a JAR reference.
        /// </summary>
        private void AddJarReference()
        {
            // Get the current project
            var project = (IVsHierarchy)GetCurrentProject();

            using (var dialog = new AddJarReferenceDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var jarPath = dialog.JarPath;
                    var libName = dialog.LibraryName;
                    var importCode = dialog.ImportCode;

                    var item = BuildProject.AddItem("JarReference", jarPath).Single();
                    if (!string.IsNullOrEmpty(libName))
                    {
                        item.SetMetadataValue("LibraryName", libName);
                    }
                    if (importCode)
                    {
                        item.SetMetadataValue("ImportCode", "yes");
                    }

                    // Save project
                    BuildProject.Save();

                    // Unload the project - also saves the modifications
                    ErrorHandler.ThrowOnFailure(solution.CloseSolutionElement((uint)__VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, project, 0));

                    // Reload project
                    dte.ExecuteCommand("Project.ReloadProject", "");
                }
            }
        }


        /// <summary>
        /// Provide UI to import a res folder.
        /// </summary>
        private void ImportResFolder()
        {
            // Get the current project
            var project = (IVsHierarchy)GetCurrentProject();

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select existing res folder to import.";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var resFolder = dialog.SelectedPath;
                        var projectFolder = ProjectFolder;

                        // Do the import
                        ResFolderImporter.ImportResFolder(resFolder, projectFolder, AddFileToProject);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Failed to import res folder because: {0}", ex.Message), VSPackage._110, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // Save project
                        BuildProject.Save();

                        // Unload the project - also saves the modifications
                        ErrorHandler.ThrowOnFailure(solution.CloseSolutionElement((uint) __VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, project, 0));

                        // Reload project
                        dte.ExecuteCommand("Project.ReloadProject", "");
                    }
                }
            }
        }

        /// <summary>
        /// Add a file to the project with given item type.
        /// </summary>
        private void AddFileToProject(string path, string itemType)
        {
            BuildProject.AddItem(itemType, path);
        }

        /// <summary>
        /// Redraw the References node.
        /// </summary>
        internal void InvalidateReferences()
        {
            if (referenceContainerItemId.HasValue)
            {
                InvalidateItems(referenceContainerItemId.Value);
            }            
        }

        /// <summary>
        /// retrieves the IVsProject interface for currently selected project
        /// </summary>
        private static IVsProject GetCurrentProject()
        {
            var ppHier = IntPtr.Zero;
            uint pitemid;
            IVsMultiItemSelect ppMIS;
            IntPtr ppSC;

            var selectionTracker = GetSelectionTracker();
            ErrorHandler.ThrowOnFailure(selectionTracker.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC));
            var result = (IVsProject)Marshal.GetObjectForIUnknown(ppHier);
            Marshal.Release(ppHier);
            if (!IntPtr.Zero.Equals(ppSC))
                Marshal.Release(ppSC);
            return result;
        }

        /// <summary>
        /// Gets the current selection tracker.
        /// </summary>
        private static IVsTrackSelectionEx GetSelectionTracker()
        {
            IVsTrackSelectionEx result;
            ErrorHandler.ThrowOnFailure(((IVsMonitorSelection2)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsMonitorSelection))).GetEmptySelectionContext(out result));
            return result;
        }

        /// <summary>
        /// Change the android target version to the given version.
        /// </summary>
        private void ChangeAndroidTargetVersion(string newTargetVersion)
        {
            // Change target version
            SetProjectProperty(Dot42Constants.PropTargetFrameworkVersion, newTargetVersion);
        }

        /// <summary>
        /// Load the framework folder of the current project.
        /// </summary>
        private string GetFrameworkFolder()
        {
            var buildProject = BuildProject;
            var folder = (buildProject != null) ? buildProject.GetPropertyValue("TargetFrameworkDirectory") : null;
            return folder ?? string.Empty;
        }

        /// <summary>
        /// Sets a configuration independent project property.
        /// </summary>
        private void SetProjectProperty(string name, string value)
        {
            var storage = _innerVsHierarchy as IVsBuildPropertyStorage;
            if (storage == null)
                throw new InvalidOperationException("IVsBuildPropertyStorage not found");
            ErrorHandler.ThrowOnFailure(storage.SetPropertyValue(name, null, (uint)_PersistStorageType.PST_PROJECT_FILE, value));
        }

        /// <summary>
        /// Gets a configuration independent project property.
        /// </summary>
        private string GetProjectProperty(string name)
        {
            var storage = _innerVsHierarchy as IVsBuildPropertyStorage;
            if (storage == null)
                throw new InvalidOperationException("IVsBuildPropertyStorage not found");
            string value;
            ErrorHandler.ThrowOnFailure(storage.GetPropertyValue(name, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out value));
            return value;
        }
    }
}
