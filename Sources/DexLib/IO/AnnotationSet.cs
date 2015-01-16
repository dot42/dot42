using System;
using System.Collections.Generic;
using System.Text;

namespace Dot42.DexLib.IO
{
    internal class AnnotationSet : List<Annotation>, IEquatable<AnnotationSet>
    {
        public AnnotationSet(IAnnotationProvider provider)
        {
            AddRange(provider.Annotations);
        }

        #region IEquatable<AnnotationSet> Members

        public bool Equals(AnnotationSet other)
        {
            bool result = Count == other.Count;

            if (result)
            {
                for (int i = 0; i < Count; i++)
                    result &= this[i].Equals(other[i]);
            }

            return result;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is AnnotationSet)
                return Equals(obj as AnnotationSet);

            return false;
        }

        public override int GetHashCode()
        {
            var builder = new StringBuilder();
            foreach (Annotation annotation in this)
                builder.AppendLine(annotation.GetHashCode().ToString());
            return builder.ToString().GetHashCode();
        }
    }
}