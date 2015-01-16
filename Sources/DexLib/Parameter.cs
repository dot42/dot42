using System;
using System.Collections.Generic;

namespace Dot42.DexLib
{
    public class Parameter : IAnnotationProvider, ICloneable, IEquatable<Parameter>
    {
        public Parameter()
        {
            Annotations = new List<Annotation>();
        }

        public Parameter(TypeReference type, string name) : this()
        {
            Type = type;
            Name = name;
        }

        public TypeReference Type { get; set; }
        public string Name { get; set; }

        #region IAnnotationProvider Members

        public List<Annotation> Annotations { get; set; }

        #endregion

        public override string ToString()
        {
            return Type.ToString();
        }

        #region " ICloneable "

        object ICloneable.Clone()
        {
            var result = new Parameter();
            result.Type = Type;
            result.Name = Name;

            return result;
        }

        internal Parameter Clone()
        {
            return (Parameter) (this as ICloneable).Clone();
        }

        #endregion

        #region " IEquatable "

        public bool Equals(Parameter other)
        {
            // do not check annotations at this time.
            return Type.Equals(other.Type);
        }

        #endregion
    }
}