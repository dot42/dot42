using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Mono.Cecil;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using MethodReference = Dot42.DexLib.MethodReference;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for base classes of struct's that are used as Nullable(T).
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
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 0; } }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XModel.XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            Class.IsFinal = false;
            Class.IsAbstract = true;
        }

        /// <summary>
        /// Create the name of the class.
        /// </summary>
        protected override string CreateClassName(XTypeDefinition xType)
        {
            return NameConverter.GetNullableBaseClassName(xType);
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
        }

        /// <summary>
        /// Gets all constructors that has to be wrapped in this class.
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
    }
}
