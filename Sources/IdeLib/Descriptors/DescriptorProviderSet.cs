using System.Linq;
using Dot42.ApkLib.Resources;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Set of descriptor providers for a given framework's attrs.xml.
    /// </summary>
    public sealed class DescriptorProviderSet
    {
        private readonly AttrsXmlParser parser;
        private readonly LayoutXmlParser layoutParser;
        private MenuDescriptors menuDescriptors;
        private LayoutDescriptors layoutDescriptors;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DescriptorProviderSet(AttrsXmlParser parser, LayoutXmlParser layoutParser, Table resources)
        {
            this.parser = parser;
            this.layoutParser = layoutParser;

            if (resources != null)
            {
                var pkg = resources.Packages.FirstOrDefault();
                var typeSpec = (pkg != null) ? pkg.TypeSpecs.FirstOrDefault(x => x.Name == "attr") : null;
                parser.AttrTypeSpec = typeSpec;
            }
        }

        /// <summary>
        /// Gets the descriptors for layout resources.
        /// </summary>
        public LayoutDescriptors LayoutDescriptors
        {
            get { return layoutDescriptors ?? (layoutDescriptors = new LayoutDescriptors(parser, layoutParser)); }
        }

        /// <summary>
        /// Gets the descriptors for menu resources.
        /// </summary>
        public MenuDescriptors MenuDescriptors
        {
            get { return menuDescriptors ?? (menuDescriptors = new MenuDescriptors(parser)); }
        }
    }
}
