using System;
using Dot42.Ide;
using Dot42.VStudio.Flavors;

namespace Dot42.VStudio.Services
{
    /// <summary>
    /// Output pane implementation.
    /// </summary>
    internal class IdeOutputPane : OutputPane, IIdeOutputPane
    {
        public IdeOutputPane()
        {
        }

        public IdeOutputPane(Guid guid, string title) :base(guid, title)
        {
        }
        /// <summary>
        /// Make sure this pane is loaded.
        /// </summary>
        public void EnsureLoaded()
        {
            Load();
        }
    }
}
