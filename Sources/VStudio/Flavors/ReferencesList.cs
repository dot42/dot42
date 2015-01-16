using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.VStudio.ProjectBase;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dot42.VStudio.Flavors
{
    internal class ReferencesList : IVsHierarchyEvents
    {
        private List<ItemId> referenceItems;
        private readonly HashSet<uint> jarReferenceItems = new HashSet<uint>();
        private readonly Dot42Project project;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ReferencesList(Dot42Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Does this list contain the given item id?
        /// </summary>
        public bool Contains(ItemId id)
        {
            return (referenceItems != null) && referenceItems.Contains(id);
        }

        /// <summary>
        /// Is the given item id a jar reference in this list?
        /// </summary>
        public bool IsJarReference(ItemId id)
        {
            GetReferenceItems(); // Force load
            return jarReferenceItems.Contains(id.Value);
        }

        /// <summary>
        /// Gets the first reference or nil if there are no references.
        /// </summary>
        public ItemId GetFirstChild()
        {
            var list = GetReferenceItems();
            if (list.Count == 0) return ItemId.Nil;
            return list[0];
        }

        /// <summary>
        /// Gets the next sibling of the given id.
        /// </summary>
        public ItemId GetNextSibling(ItemId id)
        {
            var list = GetReferenceItems();
            var index = list.IndexOf(id);
            if (index < 0) return ItemId.Nil;
            index++;
            if (index >= list.Count) return ItemId.Nil;
            return list[index];
        }

        /// <summary>
        /// Gets the id's of all reference items.
        /// </summary>
        private List<ItemId> GetReferenceItems()
        {
            if (referenceItems == null)
            {
                var list = new List<ItemId>();
                jarReferenceItems.Clear();
                var id = project.GetOriginalFirstChild(ItemId.Root);
                while (!id.IsNil)
                {
                    if (project.IsJarReference(id))
                    {
                        // Add jar reference
                        list.Add(id);
                        jarReferenceItems.Add(id.Value);
                    }
                    else if (project.IsReferencesContainer(id))
                    {
                        // Add reference items
                        var childId = project.GetOriginalFirstChild(id);
                        while (!childId.IsNil)
                        {
                            list.Add(childId);
                            childId = project.GetOriginalNextSibling(childId);
                        }
                    }
                    id = project.GetOriginalNextSibling(id);
                }

                // Now sort the list
                referenceItems = list.OrderBy(x => project.GetOriginalName(x) ?? "").ToList();
            }
            return referenceItems;
        }

        int IVsHierarchyEvents.OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
        {
            referenceItems = null;
            return VSConstants.S_OK;
        }

        int IVsHierarchyEvents.OnItemsAppended(uint itemidParent)
        {
            referenceItems = null;
            return VSConstants.S_OK;
        }

        int IVsHierarchyEvents.OnItemDeleted(uint itemid)
        {
            if (referenceItems != null)
            {
                var removed = referenceItems.RemoveAll(x => x.Value == itemid);
                if (removed > 0)
                {
                    project.InvalidateReferences();
                }
            }
            return VSConstants.S_OK;
        }

        int IVsHierarchyEvents.OnPropertyChanged(uint itemid, int propid, uint flags)
        {
            if ((propid == (int) __VSHPROPID.VSHPROPID_FirstChild) || (propid == (int)__VSHPROPID.VSHPROPID_NextSibling))
            {
                referenceItems = null;
            }
            return VSConstants.S_OK;
        }

        int IVsHierarchyEvents.OnInvalidateItems(uint itemidParent)
        {
            referenceItems = null;
            return VSConstants.S_OK;
        }

        int IVsHierarchyEvents.OnInvalidateIcon(IntPtr hicon)
        {
            return VSConstants.S_OK;
        }
    }
}
