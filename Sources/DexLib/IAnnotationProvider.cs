using System.Collections.Generic;

namespace Dot42.DexLib
{
    public interface IAnnotationProvider
    {
        List<Annotation> Annotations { get; set; }
    }
}