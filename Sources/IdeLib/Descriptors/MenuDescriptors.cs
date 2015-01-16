using System.Collections.Generic;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Descriptor provider for menu resources.
    /// </summary>
    public class MenuDescriptors : DescriptorProvider
    {
        private readonly ElementDescriptor menuDescriptor;
        private readonly ElementDescriptor itemDescriptor;
        private readonly ElementDescriptor groupDescriptor;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal MenuDescriptors(AttrsXmlParser parser)
        {
            menuDescriptor = CreateDescriptor(parser, "Menu", "menu");
            itemDescriptor = CreateDescriptor(parser, "MenuItem", "item");
            groupDescriptor = CreateDescriptor(parser, "MenuGroup", "group");

            menuDescriptor.Add(itemDescriptor);
            menuDescriptor.Add(groupDescriptor);
            itemDescriptor.Add(menuDescriptor);
            groupDescriptor.Add(itemDescriptor);
        }

        /// <summary>
        /// Create an element descriptor.
        /// </summary>
        private static ElementDescriptor CreateDescriptor(AttrsXmlParser parser, string descriptorName, string shortName)
        {
            var source = parser.FindElementDescriptor(descriptorName);
            var result = new ElementDescriptor(shortName);
            if (source != null)
                result.AddRange(source.Attributes);
            return result;
        }

        /// <summary>
        /// Gets the descriptors of all possible root elements.
        /// </summary>
        public override IEnumerable<ElementDescriptor> RootDescriptors
        {
            get { yield return menuDescriptor; }
        }
    }
}
