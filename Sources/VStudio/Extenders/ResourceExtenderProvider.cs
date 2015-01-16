using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Dot42.Ide.Project;
using Dot42.VStudio.Shared;
using EnvDTE;

namespace Dot42.VStudio.Extenders
{
    /// <summary>
    /// Provider for resource extensions
    /// </summary>
    internal sealed class ResourceExtenderProvider : EnvDTE.IExtenderProvider
    {
        private readonly Dot42Package pkg;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="pkg"></param>
        internal ResourceExtenderProvider(Dot42Package pkg)
        {
            this.pkg = pkg;
        }

        /// <summary>
        /// Can the given object be extended by this extender.
        /// </summary>
        public bool CanExtend(string extenderCATID, string extenderName, object extendeeObject)
        {
#if DEBUG
            Trace.WriteLine(string.Format("CanExtend({0}, {1}", extenderCATID, extenderName));
#endif
            if (extenderName == ResourceExtender.Name)
            {
                foreach (string catId in ResourceExtender.CategoryIds)
                {
                    if (string.Equals(extenderCATID, catId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var descriptors = TypeDescriptor.GetProperties(extendeeObject);
                        // Check for match in category
                        var actualCatId = (GetPropertyValue(extendeeObject, descriptors, "ExtenderCATID") ?? string.Empty).ToString();

                        if (string.Equals(catId, actualCatId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var itemType = (GetPropertyValue(extendeeObject, descriptors, "ItemType") ?? string.Empty).ToString();
                            if (Dot42Constants.ResourceItemTypes.Any(x => string.Equals(x, itemType, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the value of a property with the given name.
        /// </summary>
        private static object GetPropertyValue(object extendedObject, PropertyDescriptorCollection descriptors, string propertyName)
        {
            var descr = descriptors[propertyName];
            return (descr == null) ? null : descr.GetValue(extendedObject);
        }

        /// <summary>
        /// Create a new extender for the given object.
        /// </summary>
        object EnvDTE.IExtenderProvider.GetExtender(string ExtenderCATID, string ExtenderName, object ExtendeeObject, IExtenderSite ExtenderSite, int Cookie)
        {
            if (CanExtend(ExtenderCATID, ExtenderName, ExtendeeObject))
            {
#if DEBUG
                Trace.WriteLine("GetExtender - creating extended");
#endif
                return new ResourceExtender(pkg, ExtenderSite, Cookie, ExtendeeObject);
            }
            return null;
        }
    }
}
