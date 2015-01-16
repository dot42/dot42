using System.Collections.Generic;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Base class for various annotation attributes
    /// </summary>
    public abstract class AnnotationsAttribute : Attribute
    {
        private readonly Annotation[] annotations;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected AnnotationsAttribute(Annotation[] annotations)
        {
            this.annotations = annotations;
        }

        /// <summary>
        /// Gets the annotations
        /// </summary>
        public IEnumerable<Annotation> Annotations
        {
            get { return annotations; }
        }

        /// <summary>
        /// Gets the number of annotations
        /// </summary>
        public int Count
        {
            get { return annotations.Length; }
        }
    }
}
