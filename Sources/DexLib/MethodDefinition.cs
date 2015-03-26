using System;
using System.Collections.Generic;
using Dot42.DexLib.Instructions;

namespace Dot42.DexLib
{
    public class MethodDefinition : MethodReference, IMemberDefinition
    {
        public MethodDefinition()
        {
            Annotations = new List<Annotation>();
        }

        // for prefetching
        internal MethodDefinition(MethodReference mref) : this()
        {
            Owner = mref.Owner as ClassDefinition;
            Name = mref.Name;
            Prototype = mref.Prototype;
        }

        public MethodDefinition(ClassDefinition owner, string name, Prototype prototype) : this()
        {
            Owner = owner;
            Name = name;
            Prototype = prototype;
        }

        public int MapFileId { get; set; }

        public MethodBody Body { get; set; }

        public bool IsVirtual { get; set; }

        /// <summary>
        /// Is this a "direct" method
        /// </summary>
        public bool IsDirect
        {
            get
            {
                var direct = (IsPrivate || IsStatic || IsConstructor);
                if (!direct)
                    return false;

                if (IsVirtual || IsAbstract)
                {
                    throw new ArgumentException(string.Format("Direct method {0} cannot be virtual or abstract", this));
                }
                return true;
            }
        }

        public bool IsPublic
        {
            get { return (AccessFlags & AccessFlags.Public) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Public); }
        }

        public bool IsPrivate
        {
            get { return (AccessFlags & AccessFlags.Private) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Private); }
        }

        public bool IsProtected
        {
            get { return (AccessFlags & AccessFlags.Protected) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Protected); }
        }

        public bool IsStatic
        {
            get { return (AccessFlags & AccessFlags.Static) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Static); }
        }

        public bool IsFinal
        {
            get { return (AccessFlags & AccessFlags.Final) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Final); }
        }

        public bool IsSynchronized
        {
            get { return (AccessFlags & AccessFlags.Synchronized) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Synchronized); }
        }

        public bool IsBridge
        {
            get { return (AccessFlags & AccessFlags.Bridge) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Bridge); }
        }

        public bool IsVarArgs
        {
            get { return (AccessFlags & AccessFlags.VarArgs) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.VarArgs); }
        }

        public bool IsNative
        {
            get { return (AccessFlags & AccessFlags.Native) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Native); }
        }

        public bool IsAbstract
        {
            get { return (AccessFlags & AccessFlags.Abstract) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Abstract); }
        }

        public bool IsSynthetic
        {
            get { return (AccessFlags & AccessFlags.Synthetic) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Synthetic); }
        }

        public bool IsStrictFp
        {
            get { return (AccessFlags & AccessFlags.StrictFp) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.StrictFp); }
        }

        public bool IsConstructor
        {
            get { return (AccessFlags & AccessFlags.Constructor) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.Constructor); }
        }

        public bool IsDeclaredSynchronized
        {
            get { return (AccessFlags & AccessFlags.DeclaredSynchronized) != 0; }
            set { AccessFlags = AccessFlags.Set(value, AccessFlags.DeclaredSynchronized); }
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public override bool Equals(IMemberReference other)
        {
            return Equals(other as MethodDefinition);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(MethodDefinition other)
        {
            // Should be enough (ownership)
            return base.Equals(other);
        }

        public AccessFlags AccessFlags { get; set; }

        public new ClassDefinition Owner
        {
            get { return base.Owner as ClassDefinition; }
            set { base.Owner = value; }
        }

        public List<Annotation> Annotations { get; set; }
    }
}