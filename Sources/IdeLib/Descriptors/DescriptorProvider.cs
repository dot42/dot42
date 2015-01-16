using System.Collections.Generic;

namespace Dot42.Ide.Descriptors
{
    public abstract class DescriptorProvider : IElementDescriptorProvider
    {
        /// <summary>
        /// Gets the descriptors of all possible root elements.
        /// </summary>
        public abstract IEnumerable<ElementDescriptor> RootDescriptors { get; }

        /// <summary>
        /// Gets all provided element descriptors.
        /// </summary>
        IEnumerable<ElementDescriptor> IElementDescriptorProvider.Descriptors
        {
            get { return RootDescriptors; }
        }
    }
}
