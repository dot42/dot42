using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.CompilerLib.XModel
{
    /// <summary>
    /// Reference to a method
    /// </summary>
    public abstract class XMethodReference : XMemberReference, IXGenericParameterProvider
    {
        private XMethodDefinition resolvedMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XMethodReference(XTypeReference declaringType)
            : base(declaringType.Module, declaringType)
        {
        }

        /// <summary>
        /// Is this an instance method ref?
        /// </summary>
        public abstract bool HasThis { get; }

        /// <summary>
        /// Gets all generic parameters
        /// </summary>
        public abstract ReadOnlyCollection<XGenericParameter> GenericParameters { get; }

        /// <summary>
        /// Is this provider the same as the given other provider?
        /// </summary>
        bool IXGenericParameterProvider.IsSame(IXGenericParameterProvider other)
        {
            var otherMethod = other as XMethodReference;
            return (otherMethod != null) && (FullName == otherMethod.FullName); // // Do not use IsSame because of endless recursion
        }

        /// <summary>
        /// Is this a generic instance?
        /// </summary>
        public virtual bool IsGenericInstance
        {
            get { return false; }
        }

        /// <summary>
        /// Name of the member (including all)
        /// </summary>
        public override string FullName
        {
            get
            {
                var generics = "";
                var genericInstance = this as IXGenericInstance;
                if (genericInstance != null)
                {
                    generics = "<" + string.Join(", ", genericInstance.GenericArguments.Select(x => x.Name)) + ">";
                }
                else if (GenericParameters.Any())
                {
                    generics = "<" + string.Join(", ", GenericParameters.Select(x => x.Name)) + ">";
                }
                return ReturnType.FullName + " " + DeclaringType.FullName + "." + Name + generics + "(" + string.Join(", ", Parameters.Select(x => x.ParameterType.FullName)) + ")";
            }
        }

        /// <summary>
        /// Return type of the method
        /// </summary>
        public abstract XTypeReference ReturnType { get; }

        /// <summary>
        /// Parameters of the method
        /// </summary>
        public abstract ReadOnlyCollection<XParameter> Parameters { get; }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// </summary>
        public virtual bool TryResolve(out XMethodDefinition method)
        {
            if (resolvedMethod != null)
            {
                method = resolvedMethod;
                return true;
            }
            method = null;
            XTypeDefinition declaringType;
            if (!DeclaringType.GetElementType().TryResolve(out declaringType))
                return false;
            if (!declaringType.TryGet(this, out method))
                return false;
            // Cache for later
            resolvedMethod = method;
            declaringType.AddFlushAction(() => resolvedMethod = null);
            return true;
        }

        /// <summary>
        /// Resolve this reference to it's definition.
        /// Throw an exception is the resolution failed.
        /// </summary>
        public XMethodDefinition Resolve()
        {
            XMethodDefinition methodDef;
            if (TryResolve(out methodDef))
                return methodDef;
            throw new XResolutionException(this);
        }

        /// <summary>
        /// Gets the non-generic method.
        /// </summary>
        public virtual XMethodReference GetElementMethod()
        {
            return this;
        }

        /// <summary>
        /// Is this reference equal to the given other reference?
        /// </summary>
        public bool IsSame(XMethodReference other)
        {
            return DeclaringType.IsSame(other.DeclaringType) && IsSameExceptDeclaringType(other);
        }

        /// <summary>
        /// Is this reference equal to the given other reference?
        /// </summary>
        public virtual bool IsSameExceptDeclaringType(XMethodReference other)
        {
            if ((Name == other.Name) &&
                (HasThis == other.HasThis) &&
                (Parameters.Count == other.Parameters.Count) &&
                ReturnType.IsSame(other.ReturnType) &&
                (IsGenericInstance == other.IsGenericInstance))
            {
                // Possibly the same
                if (Parameters.Where((t, i) => !t.IsSame(other.Parameters[i])).Any())
                {
                    return false;
                }

                if (IsGenericInstance)
                {
                    if (!((XGenericInstanceMethod)this).IsSame((XGenericInstanceMethod)other))
                        return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Simple implementation
        /// </summary>
        public sealed class Simple : XMethodReference
        {
            private readonly string name;
            private readonly bool hasThis;
            private readonly XTypeReference returnType;
            private readonly ReadOnlyCollection<XParameter> parameters;
            private readonly ReadOnlyCollection<XGenericParameter> genericParameters;

            /// <summary>
            /// Default ctor
            /// </summary>
            public Simple(string name, bool hasThis, XTypeReference returnType, XTypeReference declaringType,
                          IEnumerable<XParameter> parameters, IEnumerable<string> genericParameterNames)
                : base(declaringType)
            {
                this.name = name;
                this.hasThis = hasThis;
                this.returnType = returnType;
                this.parameters = (parameters ?? Enumerable.Empty<XParameter>()).ToList().AsReadOnly();
                genericParameters = (genericParameterNames ?? Enumerable.Empty<string>()).Select((x, i) => new XGenericParameter.SimpleXGenericParameter(this, i)).Cast<XGenericParameter>().ToList().AsReadOnly();
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            public Simple(string name, bool hasThis, XTypeReference returnType, XTypeReference declaringType,
                          IEnumerable<XTypeReference> parameters, IEnumerable<string> genericParameterNames)
                : this(name, hasThis, returnType, declaringType, parameters.Select((x, i) => XParameter.Create("p" + i, x)),
                    genericParameterNames)
            {
            }

            /// <summary>
            /// Default ctor
            /// </summary>
            public Simple(string name, bool hasThis, XTypeReference returnType, XTypeReference declaringType, params XParameter[] parameters) :
                this(name, hasThis, returnType, declaringType, parameters, null)
            {
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return name; }
            }

            /// <summary>
            /// Is this an instance method ref?
            /// </summary>
            public override bool HasThis
            {
                get { return hasThis; }
            }

            /// <summary>
            /// Gets all generic parameters
            /// </summary>
            public override ReadOnlyCollection<XGenericParameter> GenericParameters
            {
                get { return genericParameters; }
            }

            /// <summary>
            /// Return type of the method
            /// </summary>
            public override XTypeReference ReturnType
            {
                get { return returnType; }
            }

            /// <summary>
            /// Parameters of the method
            /// </summary>
            public override ReadOnlyCollection<XParameter> Parameters
            {
                get { return parameters; }
            }
        }
    }
}
