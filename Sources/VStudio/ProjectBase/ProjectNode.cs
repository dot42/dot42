using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.ProjectBase
{
    /// <summary>
    /// Abstract flavored project node.
    /// </summary>
    internal abstract partial class ProjectNode : FlavoredProjectBase, IServiceProvider, IOriginalProjectStructure
    {
        private readonly Dot42Package package;
        private string location;
        private string name;
        private string fileName;
        private Project project;
        private readonly ImageHandler imageHandler;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ProjectNode(Dot42Package package)
        {
            this.package = package;
            imageHandler = new ImageHandler();
        }

        protected virtual Package Package { get { return package; } }
        public virtual string Name { get { return name; } }
        internal virtual ImageHandler ImageHandler { get { return imageHandler; } }

        /// <summary>
        /// Gets the folder containing this project.
        /// </summary>
        protected string ProjectFolder
        {
            get { return fileName == null ? null : Path.GetDirectoryName(fileName); }
        }

        /// <summary>
        /// Provide access to the underlying MSBuild project.
        /// </summary>
        protected Project BuildProject
        {
            get
            {
                if (project == null)
                {
                    project = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(fileName).FirstOrDefault();
                    if (project == null)
                    {
                        // When projects are created the filename points to a temp folder.
                        // Now try to match on name part only
                        var fileNameOnly = Path.GetFileName(fileName);
                        project = ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(x => Path.GetFileName(x.FullPath) == fileNameOnly);
                    }
                }
                return project;
            }
        }

        /// <summary>
        /// Attach inner project.
        /// </summary>
        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            if (serviceProvider == null)
                serviceProvider = package;
            base.SetInnerProject(innerIUnknown);
        }

        /// <summary>
        /// Get a service from VisualStudio.
        /// </summary>
        public object GetService(Type serviceType)
        {
            var service = (serviceProvider != null) ? serviceProvider.GetService(serviceType) : null;
            return service ?? package.GetService(serviceType);
        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel)
        {
            this.location = location;
            this.fileName = fileName;
            this.name = name;
            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);
        }

        protected override void OnAggregationComplete()
        {
            base.OnAggregationComplete();
            eventSinkCookie = base.AdviseHierarchyEvents(hierarchyListeners);
            project = null;
        }

        protected override void Close()
        {
            if (eventSinkCookie != VSConstants.VSCOOKIE_NIL)
            {
                base.UnadviseHierarchyEvents(eventSinkCookie);
                eventSinkCookie = VSConstants.VSCOOKIE_NIL;
            }
            base.Close();
        }

        /// <summary>
        /// Gets a value for an MSBuild property.
        /// </summary>
        public string GetMSBuildProperty(string propName, string configPlaformName)
        {
            string text = null;
            var vsBuildPropertyStorage = _innerVsHierarchy as IVsBuildPropertyStorage;
            if (vsBuildPropertyStorage != null)
            {
                vsBuildPropertyStorage.GetPropertyValue(propName, configPlaformName, 1u, out text);
            }
            return text ?? "";
        }

        /// <summary>
        /// Sets the value of an MSBuild property.
        /// </summary>
        public int SetMSBuildProperty(string propName, string propValue, string configPlaformName)
        {
            var result = VSConstants.E_FAIL;
            var vsBuildPropertyStorage = _innerVsHierarchy as IVsBuildPropertyStorage;
            if (vsBuildPropertyStorage != null)
            {
                result = vsBuildPropertyStorage.SetPropertyValue(propName, configPlaformName, 1u, propValue);
            }
            return result;
        }

        /// <summary>
        /// Gets the first child of the given item (in the original project structure).
        /// </summary>
        uint IOriginalProjectStructure.GetFirstNodeChild(uint itemId)
        {
            object result;
            ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstChild, out result));
            return (uint)(int)result;
        }

        /// <summary>
        /// Gets the sibling of the given item (in the original project structure).
        /// </summary>
        uint IOriginalProjectStructure.GetNodeSibling(uint itemId)
        {
            object result;
            ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_NextSibling, out result));
            return (uint)(int)result;
        }

        /// <summary>
        /// Gets a metadata property for an item with given id.
        /// </summary>
        internal string GetMetadata(uint itemId, string property)
        {
            object browseObject;
            ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_BrowseObject, out browseObject));
            if (browseObject == null)
                return null;
            var getPropertyMethod = browseObject.GetType().GetMethod("GetProperty");
            if (getPropertyMethod == null)
                return null;
            return (string)getPropertyMethod.Invoke(browseObject, new object[] { property, null });
        }

        /// <summary>
        /// Sets a metadata property for an item given id.
        /// </summary>
        internal string SetMetadata(uint itemId, string property, string value)
        {
            object browseObject;
            ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_BrowseObject, out browseObject));
            if (browseObject == null)
                return null;
            var setPropertyMethod = browseObject.GetType().GetMethod("SetProperty");
            if (setPropertyMethod == null)
                return null;
            return (string)setPropertyMethod.Invoke(browseObject, new object[] { property, value });
        }

        /// <summary>
        /// Gets the item type guid of the given item id.
        /// </summary>
        internal Guid GetItemTypeGuid(uint itemId)
        {
            Guid type;
            try
            {
                var hier = (IVsHierarchy)this;
                ErrorHandler.ThrowOnFailure(hier.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out type));
            }
            catch (COMException e)
            {
                // FSharp project returns Guid.Empty as the type guid for reference nodes, which causes the WAP to throw an exception
                var pinfo = e.GetType().GetProperty("HResult", BindingFlags.Instance | BindingFlags.NonPublic);
                if ((pinfo == null) || (int)pinfo.GetValue(e, new object[] { }) == VSConstants.DISP_E_MEMBERNOTFOUND)
                    type = Guid.Empty;
                else
                    throw;
            }
            return type;
        }

        /// <summary>
        /// Gets the build item that belong to the given item id.
        /// </summary>
        internal ProjectItem GetProjectItem(ItemId itemId)
        {
            string name;
            if (ErrorHandler.Succeeded(GetCanonicalName(itemId.Value, out name)))
                return GetProjectItem(name);
            return null;
        }

        /// <summary>
        /// Gets the first child of the given item id using the base implementation.
        /// </summary>
        internal ItemId GetOriginalFirstChild(ItemId id)
        {
            object property;
            base.GetProperty(id.Value, (int)__VSHPROPID.VSHPROPID_FirstChild, out property);
            return ItemId.Get(property);
        }

        /// <summary>
        /// Gets the next sibling of the given item id using the base implementation.
        /// </summary>
        internal ItemId GetOriginalNextSibling(ItemId id)
        {
            object property;
            base.GetProperty(id.Value, (int)__VSHPROPID.VSHPROPID_NextSibling, out property);
            return ItemId.Get(property);
        }

        /// <summary>
        /// Gets the name of the given item id using the base implementation.
        /// </summary>
        internal string GetOriginalName(ItemId id)
        {
            object property;
            base.GetProperty(id.Value, (int)__VSHPROPID.VSHPROPID_Name, out property);
            return property as string;
        }

        /// <summary>
        /// Invalidate the items of the given item id.
        /// </summary>
        internal void InvalidateItems(ItemId id)
        {
            hierarchyListeners.OnInvalidateItems(id.Value);
        }

        /// <summary>
        /// Gets the project item with the given full path.
        /// </summary>
        private ProjectItem GetProjectItem(string fullPath)
        {
            var proj = BuildProject;
            if (proj == null)
                return null;
            return proj.Items.FirstOrDefault(x => NativeMethods.IsSamePath(GetFullPath(x), fullPath));
        }

        /// <summary>
        /// Gets the project item with the given full path.
        /// </summary>
        private string GetFullPath(ProjectItem item)
        {
            if (Path.IsPathRooted(item.EvaluatedInclude))
                return item.EvaluatedInclude;
            return Path.Combine(Path.GetDirectoryName(fileName), item.EvaluatedInclude);
        }
    }
}
