using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for base classes of struct's that are used as Nullable(T).
    /// </summary>
    internal class NullableEnumBaseClassBuilder : NullableBaseClassBuilder 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public NullableEnumBaseClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            Class.SuperClass = Compiler.GetDot42InternalType("Enum").GetClassReference(targetPackage);
        }

        /// <summary>
        /// Gets all constructors that has to be wrapped in this class.
        /// </summary>
        protected override IEnumerable<XMethodDefinition> GetBaseClassCtors()
        {
            var baseType = Compiler.GetDot42InternalType("Enum").Resolve();
            return baseType.Methods.Where(x => !x.IsStatic && x.IsConstructor);
        }
    }
}
