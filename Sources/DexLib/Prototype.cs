using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public sealed class Prototype : FreezableBase, ICloneable, IEquatable<Prototype>
    {
        private TypeReference _returnType;
        private List<Parameter> _parameters;
        private Parameter _genericInstanceTypeParameter;
        private Parameter _genericInstanceMethodParameter;
        private int _hashCode;
        private string _signatureCache;

        /// <summary>
        /// when using this constructor, the prototype will be in unfrozen state
        /// </summary>
        public Prototype()
        {
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// When using this constructor, the Prototype will be in frozen state.
        /// Use Unfreeze if you need to make modifications.
        /// </summary>
        public Prototype(TypeReference returntype, params Parameter[] parameters)
        {
            _returnType = returntype;
            Parameters = parameters;

            Freeze();
        }

        public TypeReference ReturnType { get { return _returnType; } set { ThrowIfFrozen(); _returnType = value; } }

        public IList<Parameter> Parameters 
        {
            get { return IsFrozen?(IList<Parameter>)_parameters.AsReadOnly():_parameters; } 
            set { ThrowIfFrozen(); _parameters = new List<Parameter>(value); } }

        /// <summary>
        /// Parameter in which the GenericInstance for generic types is passed to the method.
        /// </summary>
        public Parameter GenericInstanceTypeParameter
        {
            get { return _genericInstanceTypeParameter; }
            set { ThrowIfFrozen();_genericInstanceTypeParameter = value; }
        }

        /// <summary>
        /// Parameter in which the GenericInstance for generic methods is passed to the method.
        /// </summary>
        public Parameter GenericInstanceMethodParameter
        {
            get { return _genericInstanceMethodParameter; }
            set { ThrowIfFrozen(); _genericInstanceMethodParameter = value; }
        }

        public bool ContainsAnnotation()
        {
            foreach (Parameter parameter in _parameters)
            {
                if (parameter.Annotations.Count > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a 0-based index of the given parameter
        /// </summary>
        public int IndexOf(Parameter parameter)
        {
            return _parameters.IndexOf(parameter);
        }

        /// <summary>
        /// Convert to signature string.
        /// </summary>
        public string ToSignature()
        {
            if (IsFrozen && _signatureCache != null)
                return _signatureCache;

            var builder = new StringBuilder();
            builder.Append("(");
            foreach (var p in _parameters)
            {
                builder.Append(p.Type.Descriptor);
            }
            builder.Append(")");
            builder.Append(ReturnType.Descriptor);

            if (!IsFrozen)
                return _signatureCache = builder.ToString();

            return builder.ToString();            
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            for (int i = 0; i < _parameters.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(_parameters[i]);
            }
            builder.Append(")");
            builder.Append(" : ");
            builder.Append(ReturnType);
            return builder.ToString();
        }

        #region " ICloneable "

        object ICloneable.Clone()
        {
            var result = new Prototype();
            result.ReturnType = ReturnType;

            foreach (Parameter p in Parameters)
            {
                result.Parameters.Add(p.Clone());
            }

            return result;
        }

        /// <summary>
        /// this will return the cloned prototype in a frozen state.
        /// use Unfreeze if you need to modify it.
        /// </summary>
        /// <returns></returns>
        internal Prototype Clone()
        {
            var prot = (Prototype) (this as ICloneable).Clone();
            prot.Freeze();
            return prot;
        }

        #endregion

        #region " IEquatable "

        public bool Equals(Prototype other)
        {
            bool result = _returnType.Equals(other._returnType) && _parameters.Count.Equals(other._parameters.Count);
            if (result)
            {
                for (int i = 0; i < _parameters.Count; i++)
                    result = result && _parameters[i].Equals(other._parameters[i]);
            }
            return result;
        }

        #endregion

        #region " Object "

        public override bool Equals(object obj)
        {
            if (obj is Prototype)
                return Equals(obj as Prototype);

            return false;
        }


        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (IsFrozen && _hashCode != 0)
                return _hashCode;

            var builder = new StringBuilder();
            builder.AppendLine(TypeDescriptor.Encode(ReturnType));

            foreach (Parameter parameter in Parameters)
                builder.AppendLine(TypeDescriptor.Encode(parameter.Type));

            var ret = builder.ToString().GetHashCode();

            if (IsFrozen)
                _hashCode = ret;

            return ret;
        }

        #endregion

        #region Freezable

        public override bool Freeze()
        {
            bool gotFrozen = base.Freeze();
            if (gotFrozen)
            {
                if (_returnType != null)
                    _returnType.Freeze();
                foreach (var p in Parameters)
                    p.Freeze();
                if (_genericInstanceMethodParameter != null)
                    _genericInstanceMethodParameter.Freeze();
                if (_genericInstanceTypeParameter != null)
                    _genericInstanceTypeParameter.Freeze();
            }

            return gotFrozen;
        }

        public override bool Unfreeze()
        {
            bool thawed = base.Unfreeze();
            if (thawed)
            {
                _hashCode = 0; _signatureCache = null;

                if (_returnType != null)
                    _returnType.Freeze();
                foreach (var p in _parameters)
                    p.Freeze();
                if (_genericInstanceMethodParameter != null)
                    _genericInstanceMethodParameter.Freeze();
                if (_genericInstanceTypeParameter != null)
                    _genericInstanceTypeParameter.Freeze();
            }

            return thawed;
        }
        #endregion
    }
}