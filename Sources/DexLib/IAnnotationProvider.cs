using System.Collections.Generic;

namespace Dot42.DexLib
{
    public interface IAnnotationProvider
    {
        IList<Annotation> Annotations { get; set; }
    }
}