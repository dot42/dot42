using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Dot42.JvmClassLib.Attributes;

namespace Dot42.JvmClassLib
{
    [DebuggerDisplay("{Name} {descriptor}")]
    public sealed class MethodDefinition : MemberDefinition, IModifiableAttributeProvider, IAccessModifiers
    {
        private readonly MethodAccessFlags accessFlags;
        private readonly string descriptor;
        private readonly MethodDescriptor methodDescriptor;
        private MethodSignature signature;
        private List<MethodDefinition> overrides;

        public MethodDefinition(ClassFile cf, MethodAccessFlags accessFlags, string name, string descriptor, string signature)
            : base(cf, name)
        {
            this.accessFlags = accessFlags;
            this.descriptor = descriptor;
            methodDescriptor = Descriptors.ParseMethodDescriptor(descriptor);
            this.signature = (signature != null) ? Signatures.ParseMethodSignature(signature) : null;
        }

        /// <summary>
        /// Clone this definition for use in a given other class.
        /// </summary>
        public MethodDefinition CloneTo(ClassFile otherClass)
        {
            var clone = new MethodDefinition(otherClass, accessFlags, Name, descriptor, null);
            clone.signature = signature;
            return clone;
        }

        /// <summary>
        /// Gets the access flags
        /// </summary>
        public MethodAccessFlags AccessFlags { get { return accessFlags; } }

        public bool IsPublic { get { return AccessFlags.HasFlag(MethodAccessFlags.Public); } }
        public bool IsPackagePrivate { get { return ((AccessFlags & MethodAccessFlags.AccessMask) == 0); } }
        public bool IsPrivate { get { return AccessFlags.HasFlag(MethodAccessFlags.Private); } }
        public bool IsProtected { get { return AccessFlags.HasFlag(MethodAccessFlags.Protected); } }
        public bool IsStatic { get { return AccessFlags.HasFlag(MethodAccessFlags.Static); } }
        public bool IsFinal { get { return AccessFlags.HasFlag(MethodAccessFlags.Final); } }
        public bool IsSynchronized { get { return AccessFlags.HasFlag(MethodAccessFlags.Synchronized); } }
        public bool IsBridge { get { return AccessFlags.HasFlag(MethodAccessFlags.Bridge); } }
        public bool IsVarArgs { get { return AccessFlags.HasFlag(MethodAccessFlags.VarArgs); } }
        public bool IsNative { get { return AccessFlags.HasFlag(MethodAccessFlags.Native); } }
        public bool IsAbstract { get { return AccessFlags.HasFlag(MethodAccessFlags.Abstract); } }
        public bool IsStrict { get { return AccessFlags.HasFlag(MethodAccessFlags.Strict); } }
        public bool IsSynthetic { get { return AccessFlags.HasFlag(MethodAccessFlags.Synthetic); } }

        /// <summary>
        /// Gets my parameter types
        /// </summary>
        public ReadOnlyCollection<TypeReference> Parameters { get { return methodDescriptor.Parameters; } }

        /// <summary>
        /// Gets my return type
        /// </summary>
        public TypeReference ReturnType { get { return methodDescriptor.ReturnType; } }

        /// <summary>
        /// Java descriptor of this method
        /// </summary>
        public string Descriptor { get { return descriptor; } }

        /// <summary>
        /// Gets full method signature (descriptor including generics)
        /// </summary>
        public MethodSignature Signature { get { return signature ?? DefaultSignature; } }

        /// <summary>
        /// Name of this field
        /// </summary>
        public string FullName { get { return Name + " " + Descriptor; } }

        /// <summary>
        /// Adds the given attribute to this provider.
        /// </summary>
        void IModifiableAttributeProvider.Add(Attribute attribute)
        {
            Add(attribute);
        }

        /// <summary>
        /// Signals that all attributes have been loaded.
        /// </summary>
        void IModifiableAttributeProvider.AttributesLoaded()
        {
            var signatureAttributes = Attributes.OfType<SignatureAttribute>();
            signature = signatureAttributes.Select(x => Signatures.ParseMethodSignature(x.Value)).SingleOrDefault();
        }

        /// <summary>
        /// Create a default signature for this method
        /// </summary>
        private MethodSignature DefaultSignature
        {
            get { return new MethodSignature(descriptor, null, ReturnType, Parameters, null); }
        }

        /// <summary>
        /// Does this method override a virtual/abstract method in the base class?
        /// </summary>
        public bool IsOverride()
        {
            return Overrides().Any(); 
        }

        /// <summary>
        /// Gets all methods from superclasses/interfaces this method overrides.
        /// </summary>
        public IEnumerable<MethodDefinition> Overrides()
        {
            if (overrides == null)
            {
                var paramsDescriptor = Descriptors.StripMethodReturnType(descriptor);
                ClassFile baseClass;
                var tmpOverrides = new List<MethodDefinition>();
                if (DeclaringClass.TryGetSuperClass(out baseClass))
                {
                    while (baseClass != null)
                    {
                        if (baseClass.IsPublic)
                        {
                            tmpOverrides.AddRange(baseClass.Methods.Where(x => (x.Name == Name) && (Descriptors.StripMethodReturnType(x.descriptor) == paramsDescriptor)));
                        }
                        if (baseClass.ClassName == "java/lang/Object")
                            break;
                        if (!baseClass.TryGetSuperClass(out baseClass))
                            break;
                    }
                }
                overrides = tmpOverrides;
            }
            return overrides;
        }

        /// <summary>
        /// Is this an instance or static constructor
        /// </summary>
        public bool IsConstructor
        {
            get { return (Name == "<init>") || (Name == "<clinit>"); }
        }

        /// <summary>
        /// Does this method have a "this" parameter?
        /// </summary>
        public bool HasThis
        {
            get { return !IsStatic; }
        }

        /// <summary>
        /// Does this method have any code?
        /// </summary>
        public bool HasCode
        {
            get { return Attributes.OfType<CodeAttribute>().Any(); }
        }

        /// <summary>
        /// Gets the code
        /// </summary>
        public CodeAttribute Body
        {
            get { return Attributes.OfType<CodeAttribute>().FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the number of local variable slots (or indexes) are needed to store the parameters of this method (including "this").
        /// </summary>
        /// <remarks>
        /// The result does include a slot for the "this" parameter (if any).
        /// </remarks>
        public int GetParametersLocalVariableSlots()
        {
            return (HasThis ? 1 : 0) + methodDescriptor.GetLocalVariableSlots();
        }
    }
}
