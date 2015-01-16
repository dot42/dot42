using System;
using System.Collections.Generic;
using System.Text;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public class Prototype : ICloneable, IEquatable<Prototype>
    {
        public Prototype()
        {
            Parameters = new List<Parameter>();
        }

        public Prototype(TypeReference returntype, params Parameter[] parameters)
            : this()
        {
            ReturnType = returntype;
            Parameters = new List<Parameter>(parameters);
        }

        public TypeReference ReturnType { get; set; }
        public List<Parameter> Parameters { get; set; }

        /// <summary>
        /// Parameter in which the GenericInstance for generic types is passed to the method.
        /// </summary>
        public Parameter GenericInstanceTypeParameter { get; set; }

        /// <summary>
        /// Parameter in which the GenericInstance for generic methods is passed to the method.
        /// </summary>
        public Parameter GenericInstanceMethodParameter { get; set; }

        public bool ContainsAnnotation()
        {
            foreach (Parameter parameter in Parameters)
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
            return Parameters.IndexOf(parameter);
        }

        /// <summary>
        /// Convert to signature string.
        /// </summary>
        public string ToSignature()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            foreach (var p in Parameters)
            {
                builder.Append(p.Type.Descriptor);
            }
            builder.Append(")");
            builder.Append(ReturnType.Descriptor);
            return builder.ToString();            
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(Parameters[i]);
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

        internal Prototype Clone()
        {
            return (Prototype) (this as ICloneable).Clone();
        }

        #endregion

        #region " IEquatable "

        public bool Equals(Prototype other)
        {
            bool result = ReturnType.Equals(other.ReturnType) && Parameters.Count.Equals(other.Parameters.Count);
            if (result)
            {
                for (int i = 0; i < Parameters.Count; i++)
                    result = result && Parameters[i].Equals(other.Parameters[i]);
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

        public override int GetHashCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine(TypeDescriptor.Encode(ReturnType));

            foreach (Parameter parameter in Parameters)
                builder.AppendLine(TypeDescriptor.Encode(parameter.Type));

            return builder.ToString().GetHashCode();
        }

        #endregion
    }
}