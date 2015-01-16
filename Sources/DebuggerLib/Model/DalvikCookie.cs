using System;

namespace Dot42.DebuggerLib.Model
{
    public class DalvikCookie<T> : IEquatable<DalvikCookie<T>>
    {
        private readonly int value;
        public readonly T Payload;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikCookie(int value, T payload)
        {
            this.value = value;
            Payload = payload;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DalvikCookie<T> other)
        {
            return (other != null) && (other.value == value);
        }

        /// <summary>
        /// Is this equal to obj?
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DalvikCookie<T>);
        }

        /// <summary>
        /// Hashing
        /// </summary>
        public override int GetHashCode()
        {
            return value;
        }
    }
}
