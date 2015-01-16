using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.Utility;
using Mono.Cecil;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types that have a DexImport attribute.
    /// </summary>
    internal sealed class DexImportClassBuilder : StandardClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DexImportClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            // Do not create a class.
            // It already exists in the framework.
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Add references to all implemented interfaces.
        /// </summary>
        protected override void ImplementInterfaces(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Create and add GenericInstance field.
        /// </summary>
        protected override void CreateGenericInstanceField(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected override bool ShouldImplementField(Mono.Cecil.FieldDefinition field)
        {
            if (!base.ShouldImplementField(field))
                return false;
            if (field.IsStatic)
                return true;
            throw new FrameworkException(
                string.Format("Type {0} should have no non-framework fields ({1})", Type.FullName, field.Name));
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplementMethod(MethodDefinition method)
        {
            if (!base.ShouldImplementMethod(method))
                return false;
            if ((method.Name == ".cctor") || (method.Name == ".ctor"))
                return false;
            if (method.IsPrivate && method.HasOverrides)
                return false;
            return true;
        }
    }
}
