using System.Linq;

namespace Dot42.ImportJarLib.Doxygen
{
    public class DocPrimitiveType : IDocResolvedTypeRef
    {
        private static readonly DocPrimitiveType[] PrimitiveTypes = new[] {
            new DocPrimitiveType("java.lang.boolean", "System.Boolean"), 
            new DocPrimitiveType("java.lang.byte", "System.SByte"), 
            new DocPrimitiveType("java.lang.char", "System.Char"), 
            new DocPrimitiveType("java.lang.short", "System.Int16"), 
            new DocPrimitiveType("java.lang.int", "System.Int32"), 
            new DocPrimitiveType("java.lang.long", "Sytem.Int64"), 
            new DocPrimitiveType("java.lang.float", "System.Single"), 
            new DocPrimitiveType("java.lang.double", "System.Double"), 
            new DocPrimitiveType("java.lang.void", "System.Void"), 
        };

        private readonly string name;
        private readonly string netName;

        private DocPrimitiveType(string name, string netName)
        {
            this.name = name;
            this.netName = netName;
        }

        /// <summary>
        /// Find by name
        /// </summary>
        public static DocPrimitiveType Find(string name)
        {
            return PrimitiveTypes.FirstOrDefault(x => (x.name == name) || (x.netName == name));
        }

        /// <summary>
        /// Is this typeref equal to other?
        /// </summary>
        public bool Equals(IDocResolvedTypeRef other)
        {
            var otherPrimitive = other as DocPrimitiveType;
            return (otherPrimitive != null) && name.Equals(otherPrimitive.name);
        }
    }
}
