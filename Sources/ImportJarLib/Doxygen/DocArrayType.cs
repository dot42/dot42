using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dot42.ImportJarLib.Doxygen
{
    public class DocArrayType : IDocResolvedTypeRef
    {
        private readonly IDocResolvedTypeRef componentType;

        public DocArrayType(IDocResolvedTypeRef componentType)
        {
            this.componentType = componentType;
        }

        /// <summary>
        /// Is this typeref equal to other?
        /// </summary>
        public bool Equals(IDocResolvedTypeRef other)
        {
            var otherArray = other as DocArrayType;
            return (otherArray != null) && componentType.Equals(otherArray.componentType);
        }
    }
}
