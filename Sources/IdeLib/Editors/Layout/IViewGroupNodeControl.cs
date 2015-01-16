using System.Collections.Generic;

namespace Dot42.Ide.Editors.Layout
{
    public interface IViewGroupNodeControl : IViewNodeControl
    {
        /// <summary>
        /// Add a control representing a child view.
        /// </summary>
        void Add(IViewNodeControl childControl);

        /// <summary>
        /// Remove a control representing a child view.
        /// </summary>
        void Remove(IViewNodeControl childControl);

        /// <summary>
        /// Gets all controls representing child views.
        /// </summary>
        IEnumerable<IViewNodeControl> Children { get; }
    }
}
