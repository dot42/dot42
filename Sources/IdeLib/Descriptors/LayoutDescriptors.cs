using System.Collections.Generic;
using System.Linq;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Descriptor provider for layout resources.
    /// </summary>
    public class LayoutDescriptors : DescriptorProvider
    {
        private readonly List<ElementDescriptor> viewGroupDescriptors;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal LayoutDescriptors(AttrsXmlParser parser, LayoutXmlParser layoutParser)
        {
            viewGroupDescriptors = layoutParser.ViewGroupElements.Where(x => !x.IsAbstract).Select(x => CreateElementDescriptor(parser, layoutParser, x)).ToList();
            var viewDescriptors = layoutParser.ViewElements.Where(x => !x.IsAbstract).Select(x => CreateElementDescriptor(parser, layoutParser, x)).ToList();

            // Add all to viewGroup child lists
            foreach (var viewGroupDescr in viewGroupDescriptors)
            {
                viewGroupDescr.AddRange(viewGroupDescriptors);
                viewGroupDescr.AddRange(viewDescriptors);
            }
        }

        /// <summary>
        /// Gets the descriptors of all possible root elements.
        /// </summary>
        public override IEnumerable<ElementDescriptor> RootDescriptors
        {
            get { return viewGroupDescriptors; }
        }

        /// <summary>
        /// Create an element descriptor for the given element.
        /// </summary>
        private static ElementDescriptor CreateElementDescriptor(AttrsXmlParser parser, LayoutXmlParser layoutParser, LayoutElement element)
        {
            var result = new ElementDescriptor(element.Name);
            while (element != null)
            {
                // Get attributes for this element
                var attrs = parser.FindElementDescriptor(element.Name);
                if (attrs != null)
                {
                    foreach (var a in attrs.Attributes)
                    {
                        result.Add(a);
                    }
                }

                // Get super class
                if (string.IsNullOrEmpty(element.SuperClassName))
                    break;
                element = layoutParser.FindElement(element.SuperClassName);
            }

            // Add ViewGroup_Layout
            var extraAttrs = parser.FindElementDescriptor("ViewGroup_Layout");
            if (extraAttrs != null)
                result.AddRange(extraAttrs.Attributes);
            // Add ViewGroup_MarginLayout
            extraAttrs = parser.FindElementDescriptor("ViewGroup_MarginLayout");
            if (extraAttrs != null)
                result.AddRange(extraAttrs.Attributes);

            return result;
        }
    }
}
