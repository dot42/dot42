using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Dot42.VStudio.Flavors
{
    [Guid(GuidList.Strings.guidDot42ProjectFlavorPackage)]
    internal sealed class Dot42ProjectFlavorFactory : FlavoredProjectFactoryBase
    {
        private readonly Dot42Package package;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Dot42ProjectFlavorFactory(Dot42Package package)
        {
            this.package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            return new Dot42Project(package);
        }
    }
}
