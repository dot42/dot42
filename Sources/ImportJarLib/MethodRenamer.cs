using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Class uses to rename method groups
    /// </summary>
    public sealed partial class MethodRenamer
    {
        private readonly List<MethodGroup> groups = new List<MethodGroup>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal MethodRenamer(TargetFramework target)
        {
            // Build groups
            foreach (var method in target.MethodMap.AllMethods.Where(x => !(x.IsStatic || x.IsConstructor)))
            {
                if (method.MethodGroup != null)
                    continue;
                
                var possibleGroups = method.Overrides.Select(x => x.MethodGroup).Where(x => x != null).Distinct().ToList();
                while (possibleGroups.Count > 1)
                {
                    var sourceIndex = possibleGroups.Count - 1;
                    possibleGroups[0].MergeFrom(possibleGroups[sourceIndex]);
                    possibleGroups.RemoveAt(sourceIndex);
                }
                var group = possibleGroups.SingleOrDefault();
                if (group == null)
                {
                    // Create new group
                    group = new MethodGroup(method.Name);
                    groups.Add(group);
                }

                // Add method to group
                group.Add(method);

                // Add all overrides to group
                group.AddRange(method.Overrides);
            }
        }

        /// <summary>
        /// Rename the all methods in the method group of the given method to the given new name.
        /// </summary>
        public void Rename(NetMethodDefinition method, string newName)
        {
            if (method.Name == newName)
                return;
            var group = method.MethodGroup;
            if (group == null)
            {
                if (method.IsStatic)
                {
                    method.SetName(newName);
                    return;
                }
                if (method.IsConstructor)
                {
                    throw new InvalidOperationException("Constructor cannot be renamed");
                }
                throw new InvalidOperationException("Method has no group");
            }
            foreach (var m in group)
            {
                m.SetName(newName);
            }
        }

        /// <summary>
        /// Rename only the given method to the given new name.
        /// USE WITH CARE!!!!
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void RenameMethodOnly(NetMethodDefinition method, string newName)
        {
            method.SetName(newName);
        }
    }
}
