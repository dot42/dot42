using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Dot42.Mapping;
using Mono.Cecil;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using MethodReference = Dot42.DexLib.MethodReference;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for base classes of struct's that are used as Nullable(T).
    /// Currently only used for Enums.
    /// </summary>
    internal class NullableBaseClassBuilder : ClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public NullableBaseClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Sorting low comes first.
        /// </summary>
        protected override int SortPriority { get { return -60; } }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XModel.XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            Class.IsFinal = false;
            Class.IsAbstract = true;
            Class.IsSynthetic = true;
        }

        protected override XTypeDefinition CreateXType(XTypeDefinition parentXType)
        {
            var typeDef = (XBuilder.ILTypeDefinition)XBuilder.AsTypeReference(Compiler.Module, Type)
                                                             .Resolve();

            string name = NameConverter.GetNullableClassName(typeDef.Name);

            XSyntheticTypeFlags xflags = default(XSyntheticTypeFlags);

            return XSyntheticTypeDefinition.Create(Compiler.Module, parentXType, xflags,
                                                   typeDef.Namespace, name,
                                                  Compiler.Module.TypeSystem.Object,
                                                  string.Join(":", Type.Scope.Name, Type.MetadataToken.ToScopeId(), "Nullable"));
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            var baseType = Type.BaseType;
            if (baseType != null)
            {
                Class.SuperClass = (ClassReference)baseType.GetReference(targetPackage, Compiler.Module);
            }
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build ctors
            foreach (var baseCtor in GetBaseClassCtors())
            {
                // Build ctor
                var prototype = PrototypeBuilder.BuildPrototype(Compiler, targetPackage, null, baseCtor);
                var ctor = new MethodDefinition(Class, "<init>", prototype);
                ctor.AccessFlags = AccessFlags.Public | AccessFlags.Constructor;
                Class.Methods.Add(ctor);
                // Create ctor body
                var ctorBody = CreateCtorBody(prototype);
                targetPackage.Record(new CompiledMethod { DexMethod = ctor, RLBody = ctorBody });                
            }

            // build original type field
            // Create field definition
            var dfield = new Dot42.DexLib.FieldDefinition();

            dfield.Owner = Class;
            dfield.Name = "underlying$";
            dfield.IsSynthetic = true;
            dfield.IsFinal = true;
            dfield.IsStatic= true;
            dfield.IsPublic = true;

            dfield.Type = Compiler.Module.TypeSystem.Type.GetClassReference(targetPackage);

            // not sure if GetClassReference is the best way to go forward here.
            // might depend on the sort order of the class builders.
            var underlyingType = XBuilder.AsTypeReference(Compiler.Module, Type);
            dfield.Value = underlyingType.GetClassReference(targetPackage);

            Class.Fields.Add(dfield);

        }

        /// <summary>
        /// Gets all constructors that have to be wrapped in this class.
        /// </summary>
        protected virtual IEnumerable<XMethodDefinition> GetBaseClassCtors()
        {
            var baseTypeRef = Type.BaseType;
            var baseType = (baseTypeRef != null) ? baseTypeRef.Resolve() : null;
            if (baseType == null)
                return Enumerable.Empty<XMethodDefinition>();
            return baseType.Methods.Where(x => x.IsReachable && !x.IsStatic && x.IsConstructor && !x.IsPrivate)
                        .Select(x => XBuilder.AsMethodDefinition(Compiler.Module, x));
        }

        /// <summary>
        /// Create the body of the ctor.
        /// </summary>
        private MethodBody CreateCtorBody(Prototype prototype)
        {
            var body = new MethodBody(null);
            // Create code
            var ins = body.Instructions;
            var rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            // Add parameters
            var paramRegs = new List<Register> { rthis };
            foreach (var p in prototype.Parameters)
            {
                if (p.Type.IsWide())
                {
                    var pair = body.AllocateWideRegister(RCategory.Argument);
                    paramRegs.Add(pair.Item1);
                    paramRegs.Add(pair.Item2);
                }
                else
                {
                    var reg = body.AllocateRegister(RCategory.Argument, p.Type.IsPrimitive() ? RType.Value : RType.Object);
                    paramRegs.Add(reg);
                }
            }

            // Call base ctor
            var baseCtorRef = new MethodReference(Class.SuperClass, "<init>", prototype);
            ins.Add(new Instruction(RCode.Invoke_direct, paramRegs.ToArray()) { Operand = baseCtorRef });
            ins.Add(new Instruction(RCode.Return_void));
            return body;
        }

        protected override TypeEntry CreateMappingEntry()
        {
            var ret = base.CreateMappingEntry();
            return new TypeEntry(ret.Name + "?", ret.Scope, ret.DexName, ret.Id, ret.ScopeId);
        }

        protected override void ImplementInterfaces(DexTargetPackage targetPackage)
        {
            base.ImplementInterfaces(targetPackage);

            var marker = Compiler.GetDot42InternalType(InternalConstants.NullableMarker);
            Class.Interfaces.Add(marker.GetClassReference(targetPackage));

        }
    }
}
