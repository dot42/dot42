using System;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using MethodDefinition = Dot42.DexLib.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET delegate types.
    /// </summary>
    internal sealed class DelegateClassBuilder : ClassBuilder
    {
        private DelegateType delegateType;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DelegateClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 100; } }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            Class.AccessFlags &= ~AccessFlags.Final;
            Class.IsAbstract = true;
            //Class.IsInterface = true;
            // Record in compiler
            delegateType = new DelegateType(Compiler, XType, Class, targetPackage.DexFile, targetPackage.NameConverter);
            Compiler.Record(delegateType);
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            base.CreateMembers(targetPackage);

            // Build default ctor
            XTypeSystem typeSystem = Compiler.Module.TypeSystem;
            XSyntheticMethodDefinition ctor = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Constructor, "<init>", null, typeSystem.Void);
            ctor.Body = CreateCtorBody();
            Class.Methods.Add(ctor.GetDexMethod(Class, targetPackage));

            // Build Invoke method.
            XMethodDefinition sourceMethod = XType.Methods.Single(x => x.EqualsName("Invoke"));
            Prototype prototype = PrototypeBuilder.BuildPrototype(Compiler, targetPackage, Class, sourceMethod);
            MethodDefinition method = new MethodDefinition(Class, sourceMethod.Name, prototype)
            {
                AccessFlags = AccessFlags.Public | AccessFlags.Abstract,
                MapFileId = Compiler.GetNextMapFileId()
            };
            Class.Methods.Add(method);

            // Find xSource method
            targetPackage.NameConverter.Record(sourceMethod, method);

            // If void() delegate, implement java.lang.Runnable
            if (sourceMethod.ReturnType.IsVoid() && (sourceMethod.Parameters.Count == 0))
            {
                // Implement interface
                Class.Interfaces.Add(FrameworkReferences.Runnable);

                // Build run method
                var run = new MethodDefinition(Class, "run", new Prototype(PrimitiveType.Void)) { AccessFlags = AccessFlags.Public | AccessFlags.Final };
                Class.Methods.Add(run);
                run.Body = new DexLib.Instructions.MethodBody(run, 1) { IncomingArguments = 1, OutgoingArguments = 1 };
                var insList = run.Body.Instructions;
                var rThis = run.Body.Registers[0];
                insList.Add(new DexLib.Instructions.Instruction(OpCodes.Invoke_virtual, method, rThis));
                insList.Add(new DexLib.Instructions.Instruction(OpCodes.Return_void));
            }
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        public override void GenerateCode(DexTargetPackage targetPackage)
        {
            base.GenerateCode(targetPackage);

            // Compile synthetic methods
            XType.Methods.OfType<XSyntheticMethodDefinition>().ForEach(x => x.Compile(Compiler, targetPackage));
        }

        protected override bool ShouldImplementMethod(Mono.Cecil.MethodDefinition method)
        {
            if (method.DeclaringType.FullName != typeof (MulticastDelegate).FullName)
                return false;
            var name = method.Name;
            return (name == "Add") || (name == "Remove");
        }

        /// <summary>
        /// Implement java.lang.Cloneable
        /// </summary>
        protected override void ImplementCloneable(DexTargetPackage targetPackage)
        {
            // Do nothing
        }

        /// <summary>
        /// Create the body of the ctor.
        /// </summary>
        private AstBlock CreateCtorBody()
        {
            return AstBlock.CreateOptimizedForTarget(
                // Call base ctor
                new AstExpression(AstNode.NoSource, AstCode.CallBaseCtor, 0,
                    new AstExpression(AstNode.NoSource, AstCode.Ldthis, null)),
                // Return
                new AstExpression(AstNode.NoSource, AstCode.Ret, null));
        }

        public override void RecordMapping(MapFile mapFile)
        {
            base.RecordMapping(mapFile);

            // create the mapping for all our instance type.
            foreach (var instance in delegateType.Instances)
            {
                // Create mapping
                var dexName = instance.InstanceDefinition.Fullname;
                var mapFileId = instance.InstanceDefinition.MapFileId;
                var scopeId = XType.ScopeId.Substring(XType.ScopeId.IndexOf(':') + 1);
                scopeId += ":delegate:" + instance.CalledMethod.DeclaringType.ScopeId + "|" + instance.CalledMethod.ScopeId;
                
                var entry = new TypeEntry(Type.FullName, Type.Scope.Name, dexName, mapFileId, scopeId);
                mapFile.Add(entry);
            }
        }

        protected override TypeEntry CreateMappingEntry()
        {
            var ret = base.CreateMappingEntry();
            foreach (var methodName in new[] {"Invoke" /*...*/})
            {
                var xMethod = XType.Methods.Single(x => x.EqualsName(methodName));
                var dMethod = Class.Methods.Single(x => x.Name == methodName);
                var method = Type.Methods.Single(x => x.Name == methodName);
                MethodBuilder.RecordMapping(ret, xMethod, method, dMethod, null);
            }
            return ret;
        }
    }
}
