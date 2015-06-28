using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Dot42.Mapping;
using Mono.Cecil;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using MethodReference = Dot42.DexLib.MethodReference;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for marker class of struct's that are used as Nullable(T).
    /// </summary>
    internal class NullableMarkerClassBuilder : ClassBuilder
    {
        private readonly ClassBuilder _underlyingBuilder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NullableMarkerClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef, ClassBuilder underlyingBuilder)
            : base(context, compiler, typeDef)
        {
            _underlyingBuilder = underlyingBuilder;
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
            Class.IsFinal = true;
            Class.IsAbstract = true;
            Class.IsSynthetic = true;
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            // TODO: implement Generic Type Definition Classes and derive from System.Nullable`1-Marker.
            //Class.SuperClass = Compiler.GetDot42InternalType("System", "Nullable`1").GetClassReference(targetPackage);
            Class.SuperClass = FrameworkReferences.Object;
            _underlyingBuilder.Class.NullableMarkerClass = Class;
        }

        protected override XTypeDefinition CreateXType(XTypeDefinition parentXType)
        {
            var typeDef = (XBuilder.ILTypeDefinition)XBuilder.AsTypeReference(Compiler.Module, Type)
                                                             .Resolve();
            string name = NameConverter.GetNullableClassName(typeDef.Name);

            XSyntheticTypeFlags xflags = default(XSyntheticTypeFlags);
            xflags |= XSyntheticTypeFlags.ValueType;
            xflags |= XSyntheticTypeFlags.Sealed;

            return XSyntheticTypeDefinition.Create(Compiler.Module, parentXType, xflags,
                                                   typeDef.Namespace, name, 
                                                   Compiler.Module.TypeSystem.Object,
                                                   string.Join(":", Type.Scope.Name, Type.MetadataToken.ToScopeId(), "Nullable"));
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build ctors
            foreach (var baseCtor in GetBaseClassCtors())
            {
                // TODO: does this make sense? after all, we derive from object. 
                //       probalby one should just remove this code, and generate a 
                //       defaul constructor.
                
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
            // NOTE: at the moment the underlying type is both defined as a type and in the annotation.
            //       remove one or the other when we have determined which is the better way.
            var dfield = new Dot42.DexLib.FieldDefinition();

            dfield.Owner = Class;
            dfield.Name = "underlying$";
            dfield.IsSynthetic = true;
            dfield.IsFinal = true;
            dfield.IsStatic= true;
            dfield.IsPublic = true;

            dfield.Type = Compiler.Module.TypeSystem.Type.GetClassReference(targetPackage);
            dfield.Value = _underlyingBuilder.Class;
            

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

        protected override void ImplementInterfaces(DexTargetPackage targetPackage)
        {
            var genericMarker = Compiler.GetDot42InternalType(InternalConstants.NullableMarker);
            Class.Interfaces.Add(genericMarker.GetClassReference(targetPackage));
        }


        protected override TypeEntry CreateMappingEntry()
        {
            var ret = base.CreateMappingEntry();
            return new TypeEntry(ret.Name + "?", ret.Scope, ret.DexName, ret.Id, ret.ScopeId);
        }

        protected override void CreateGenericInstanceFields(DexTargetPackage targetPackage)
        {
        }

        protected override IEnumerable<ClassBuilder> CreateNestedClassBuilders(ReachableContext context, DexTargetPackage targetPackage, ClassDefinition parent)
        {
            return new ClassBuilder[0];
        }

        protected override void ImplementCloneable(DexTargetPackage targetPackage)
        {
        }

        protected override void ImplementInnerClasses(DexTargetPackage targetPackage)
        {
        }

        protected override bool ShouldImplementField(FieldDefinition field)
        {
            return false;
        }

        protected override bool ShouldImplementMethod(ILMethodDefinition method)
        {
            return false;
        }

    }
}
