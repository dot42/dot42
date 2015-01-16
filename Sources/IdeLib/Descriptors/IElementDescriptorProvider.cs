using System.Collections.Generic;

namespace Dot42.Ide.Descriptors
{
    public interface IElementDescriptorProvider
    {
        /// <summary>
        /// Gets all provided element descriptors.
        /// </summary>
        IEnumerable<ElementDescriptor> Descriptors { get; }
    }
}
