using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.ProjectBase
{
    /// <summary>
    /// Abstract flavored project node.
    /// </summary>
    partial class ProjectNode 
    {
        private readonly HierarchyListeners hierarchyListeners = new HierarchyListeners();
        private uint eventSinkCookie = VSConstants.VSCOOKIE_NIL;

        /// <summary>
        /// Override event hierarchy notifications.
        /// </summary>
        protected sealed override uint AdviseHierarchyEvents(IVsHierarchyEvents listener)
        {
            return hierarchyListeners.AdviseHierarchyEvents(listener);
        }

        /// <summary>
        /// Override event hierarchy notifications.
        /// </summary>
        protected sealed override void UnadviseHierarchyEvents(uint cookie)
        {
            hierarchyListeners.UnadviseHierarchyEvents(cookie);
        }

        /// <summary>
        /// All event listeners are registered in here.
        /// </summary>
        internal sealed class HierarchyListeners : IVsHierarchyEvents 
        {
            private readonly EventSinkCollection eventSinkCollection = new EventSinkCollection();

            /// <summary>
            /// Override event hierarchy notifications.
            /// </summary>
            internal uint AdviseHierarchyEvents(IVsHierarchyEvents eventSink)
            {
                return eventSinkCollection.Add(eventSink) + 1;
            }

            /// <summary>
            /// Override event hierarchy notifications.
            /// </summary>
            internal void UnadviseHierarchyEvents(uint cookie)
            {
                eventSinkCollection.RemoveAt(cookie - 1);
            }

            /// <summary>
            /// Gets all hierarchy event listeners.
            /// </summary>
            internal IEnumerable<IVsHierarchyEvents> HierarchyEventListeners
            {
                get { return eventSinkCollection.Cast<IVsHierarchyEvents>(); }
            }

            /// <summary>
            /// Forward the OnItemAdded event to all listeners.
            /// </summary>
            public int OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
            {
                foreach (var l in HierarchyEventListeners) l.OnItemAdded(itemidParent, itemidSiblingPrev, itemidAdded);
                return VSConstants.S_OK;
            }

            /// <summary>
            /// Forward the OnItemAppended event to all listeners.
            /// </summary>
            public int OnItemsAppended(uint itemidParent)
            {
                foreach (var l in HierarchyEventListeners) l.OnItemsAppended(itemidParent);
                return VSConstants.S_OK;
            }

            /// <summary>
            /// Forward the OnItemDeleted event to all listeners.
            /// </summary>
            public int OnItemDeleted(uint itemid)
            {
                foreach (var l in HierarchyEventListeners) l.OnItemDeleted(itemid);
                return VSConstants.S_OK;
            }

            /// <summary>
            /// Forward the OnPropertyChanged event to all listeners.
            /// </summary>
            public int OnPropertyChanged(uint itemid, int propid, uint flags)
            {
                foreach (var l in HierarchyEventListeners) l.OnPropertyChanged(itemid, propid, flags);
                return VSConstants.S_OK;
            }

            /// <summary>
            /// Forward the OnInvalidItems event to all listeners.
            /// </summary>
            public int OnInvalidateItems(uint itemidParent)
            {
                foreach (var l in HierarchyEventListeners) l.OnInvalidateItems(itemidParent);
                return VSConstants.S_OK;
            }

            /// <summary>
            /// Forward the OnInvalidIcon event to all listeners.
            /// </summary>
            public int OnInvalidateIcon(IntPtr hicon)
            {
                foreach (var l in HierarchyEventListeners) l.OnInvalidateIcon(hicon);
                return VSConstants.S_OK;
            }
        }
    }
}
