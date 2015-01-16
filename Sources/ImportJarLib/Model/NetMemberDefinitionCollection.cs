using System.Diagnostics;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("{Count}")]
    internal sealed class NetMemberDefinitionCollection<T> : CustomCollection<T>
        where T : INetMemberDefinition
    {
        private readonly NetTypeDefinition declaringType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetMemberDefinitionCollection(NetTypeDefinition declaringType)
        {
            this.declaringType = declaringType;
        }

        /// <summary>
        /// Item is about to be added
        /// </summary>
        protected override void OnAdding(T item)
        {
            item.DeclaringType = declaringType;
            declaringType.OnModified();
        }

        /// <summary>
        /// Item is about to be removed.
        /// </summary>
        protected override void OnRemoving(T item)
        {
            item.DeclaringType = null;
            declaringType.OnModified();
        }
    }
}
