using System.ComponentModel;
using System.Runtime.InteropServices;
using Dot42.Ide.Extenders;

namespace Dot42.VStudio.Extenders
{
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class ResourceExtender : ResourceExtenderBase
    {
        /// <summary>
        /// Name of this extended
        /// </summary>
        internal const string Name = "Dot42ResourceExtender";

        /// <summary>
        /// GUIDs of extended objects.
        /// </summary>
        internal static readonly string[] CategoryIds = new[] 
        {
            VSLangProj.PrjBrowseObjectCATID.prjCATIDCSharpFileBrowseObject,
            VSLangProj.PrjBrowseObjectCATID.prjCATIDVBFileBrowseObject,
        };

        private readonly Dot42Package pkg;
        private readonly EnvDTE.IExtenderSite extenderSite;
        private readonly int siteCookie;
        private readonly object extendedObject;
        private readonly PropertyDescriptor fileNameDescr;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResourceExtender(Dot42Package pkg, EnvDTE.IExtenderSite extenderSite, int siteCookie, object extendedObject)
        {
            pkg.Log("Extender created");
            this.pkg = pkg;
            this.extenderSite = extenderSite;
            this.siteCookie = siteCookie;
            this.extendedObject = extendedObject;
            fileNameDescr = TypeDescriptor.GetProperties(extendedObject)["FileName"];
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ResourceExtender()
        {
            try
            {
                if ((extenderSite != null) && (siteCookie != 0))
                {
                    extenderSite.NotifyDelete(siteCookie);
                }
            }
            catch
            {
                // Ignore
            }
        }

        /// <summary>
        /// Gets/sets the filename of the extended object
        /// </summary>
        protected override string FileName
        {
            get { return (fileNameDescr != null) ? (string)fileNameDescr.GetValue(extendedObject) : null; }
            set { if (fileNameDescr != null) fileNameDescr.SetValue(extendedObject, value); }
        }
    }
}
