using System;
using System.Collections;
using System.Collections.Generic;

namespace Dot42.DebuggerLib.Model
{
    public class DalvikMemberList<TId, TMember> : IEnumerable<TMember>
    {
        private readonly Dictionary<TId, TMember> members = new Dictionary<TId, TMember>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikMemberList(IEnumerable<TMember> members, Func<TMember, TId> idSelector)
        {
            foreach (var member in members)
            {
                var id = idSelector(member);
                this.members[id] = member;
            }
        }

        /// <summary>
        /// Try to get a member by its id.
        /// </summary>
        public bool TryGetMember(TId id, out TMember member)
        {
            return members.TryGetValue(id, out member);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TMember> GetEnumerator()
        {
            return members.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
