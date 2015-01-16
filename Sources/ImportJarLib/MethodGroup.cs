using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Dot42.ImportJarLib.Model;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Class uses to rename method groups
    /// </summary>
    public sealed partial class MethodRenamer
    {
        /// <summary>
        /// Group of methods that must have the same name
        /// </summary>
        [DebuggerDisplay("{name}")]
        internal sealed class MethodGroup : IEnumerable<NetMethodDefinition>
        {
            private readonly string name;
            private readonly HashSet<NetMethodDefinition> methods = new HashSet<NetMethodDefinition>();

            /// <summary>
            /// Default ctor
            /// </summary>
            internal MethodGroup(string name)
            {
                this.name = name;
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            internal void Add(NetMethodDefinition method)
            {
                if (method.MethodGroup == this)
                    return;
                if (method.MethodGroup != null)
                    throw new InvalidOperationException("Method already part of other group");
                methods.Add(method);
                method.MethodGroup = this;
            }

            internal void AddRange(IEnumerable<NetMethodDefinition> range)
            {
                foreach (var method in range)
                    Add(method);
            }

            /// <summary>
            /// Move all members of source into my group
            /// </summary>
            internal void MergeFrom(MethodGroup source)
            {
                foreach (var m in source.methods)
                {
                    m.MethodGroup = this;
                    methods.Add(m);
                }
                source.methods.Clear();
            }

            public IEnumerator<NetMethodDefinition> GetEnumerator()
            {
                return methods.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
