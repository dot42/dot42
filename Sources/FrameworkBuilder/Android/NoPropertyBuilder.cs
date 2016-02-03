using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Model;

namespace Dot42.FrameworkBuilder.Android
{
    public class NoPropertyBuilder : PropertyBuilder
    {
        public NoPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder) : base(typeDef, declaringTypeBuilder)
        {
        }

        protected override bool IsGetter(NetMethodDefinition method)
        {
            return false;
        }

        protected override bool IsSetter(NetMethodDefinition method)
        {
            return false;
        }
    }
}
