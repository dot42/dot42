using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.CecilExtensions;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing enum types (the nested enum info type).
    /// </summary>
    internal class EnumInfoClassBuilder : ClassBuilder
    {
        private const string ClassName = "Info";
        private readonly EnumClassBuilder enumClassBuilder;
        private XSyntheticMethodDefinition defaultCtor;
        private XSyntheticMethodDefinition create;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EnumInfoClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef, EnumClassBuilder enumClassBuilder)
            : base(context, compiler, typeDef)
        {
            this.enumClassBuilder = enumClassBuilder;
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 0; } }

        /// <summary>
        /// Gets the default constructor of the enum info type.
        /// </summary>
        internal XMethodDefinition DefaultCtor { get { return defaultCtor; } }

        /// <summary>
        /// Strong typed XType.
        /// </summary>
        internal new XSyntheticTypeDefinition XType
        {
            get { return (XSyntheticTypeDefinition) base.XType; }
        }

        /// <summary>
        /// Create the XType for this builder.
        /// </summary>
        protected override XTypeDefinition CreateXType(XTypeDefinition parentXType)
        {
            var baseType = Compiler.GetDot42InternalType("EnumInfo");
            return XSyntheticTypeDefinition.Create(Compiler.Module, parentXType, XSyntheticTypeFlags.Private, null, ClassName, 
                                                   baseType, parentXType.ScopeId + ":Info");
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            XType.SetDexClass(Class, targetPackage);
        }

        /// <summary>
        /// Create the name of the class.
        /// </summary>
        protected override string CreateClassName(XModel.XTypeDefinition xType)
        {
            return ClassName;
        }

        /// <summary>
        /// Implement java.lang.Cloneable
        /// </summary>
        protected override void ImplementCloneable(DexTargetPackage targetPackage)
        {
            // Do nothing
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            Class.SuperClass = Compiler.GetDot42InternalType("EnumInfo").GetClassReference(targetPackage);
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build value ctor
            var module = Compiler.Module;
            var isWide = Type.GetEnumUnderlyingType().Resolve().IsWide();
            var enumType = Compiler.GetDot42InternalType("Enum");
            var valueType = isWide ? module.TypeSystem.Long : module.TypeSystem.Int;

            // Build default ctor
            defaultCtor = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Constructor, "<init>", null, module.TypeSystem.Void);
            Class.Methods.Add(defaultCtor.GetDexMethod(Class, targetPackage));

            // Build Create method
            create = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Protected, "Create", null, enumType,
                XParameter.Create("value", valueType));
            Class.Methods.Add(create.GetDexMethod(Class, targetPackage));
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        public override void GenerateCode(DexTargetPackage targetPackage)
        {
            base.GenerateCode(targetPackage);

            // Create method bodies
            defaultCtor.Body = CreateCtorBody();
            create.Body = CreateCreateBody(create, enumClassBuilder.GetInstanceConstructorRef());

            // Compile synthetic methods
            XType.Methods.OfType<XSyntheticMethodDefinition>().ForEach(x => x.Compile(Compiler, targetPackage));
        }

        /// <summary>
        /// Create the body of the ctor.
        /// </summary>
        private AstBlock CreateCtorBody()
        {
            // here we could also preserve the original underlying type.
            bool isWide = Type.GetEnumUnderlyingType().Resolve().IsWide();
            var underlying = isWide ? Compiler.Module.TypeSystem.Long : Compiler.Module.TypeSystem.Int;

            return AstBlock.CreateOptimizedForTarget(
                // Call base ctor
                new AstExpression(AstNode.NoSource, AstCode.CallBaseCtor, 0,
                    new AstExpression(AstNode.NoSource, AstCode.Ldthis, null),
                    new AstExpression(AstNode.NoSource, AstCode.TypeOf, underlying)),
                // Return
                new AstExpression(AstNode.NoSource, AstCode.Ret, null));
        }

        /// <summary>
        /// Create the body of the Create(int|long) method.
        /// </summary>
        private AstBlock CreateCreateBody(XSyntheticMethodDefinition method, XMethodReference ctor)
        {
            return AstBlock.CreateOptimizedForTarget(
                new AstExpression(AstNode.NoSource, AstCode.Ret, null,
                    new AstExpression(AstNode.NoSource, AstCode.Newobj, ctor,
                        new AstExpression(AstNode.NoSource, AstCode.Ldstr, "?"),
                        new AstExpression(AstNode.NoSource, AstCode.Ldc_I4, -1),
                        new AstExpression(AstNode.NoSource, AstCode.Ldloc, method.AstParameters[0]))));
        }
    }
}
