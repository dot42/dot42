using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using MethodReference = Mono.Cecil.MethodReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// This will redirect the base class of Android.App.Application derived classes,
    /// sothat our initialization code is run.
    /// </summary>
    internal class AndroidAppApplicationDerivedBuilder : ClassBuilder
    {
        private static AndroidAppApplicationDerivedBuilder Dot42InternalApplicationBuilder;
        /// <summary>
        /// Default ctor
        /// </summary>
        public AndroidAppApplicationDerivedBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
            if (IsDot42InternalApplication())
                Dot42InternalApplicationBuilder = this;
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 0; } }

        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            if (!IsDot42InternalApplication())
            {
                var dot42Internal = Compiler.GetDot42InternalType("Application");
                Class.SuperClass = dot42Internal.GetClassReference(targetPackage);
            }
            else
                base.ImplementSuperClass(targetPackage);
        }

        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent,
            TypeDefinition parentType,
            XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);

            if (IsDot42InternalApplication())
            {
                // FixUp Visiblility.
                Class.IsPublic = true;
                Class.IsProtected = false;
                Class.IsPrivate = false;
            }
        }

        public override void GenerateCode(DexTargetPackage targetPackage, bool stopAtFirstError)
        {
            if(!IsDot42InternalApplication())
            {
                // replace call to base class constructor before generating code
                foreach (var ctor in Type.Methods.Where(m => m.IsConstructor && m.HasBody && m.IsReachable))
                {
                    foreach (var ins in ctor.Body.Instructions)
                    {
                        var methodRef = ins.Operand as MethodReference;
                        if (methodRef == null)
                            continue;
                        var method = methodRef.Resolve();
                        if (!method.IsConstructor || method.DeclaringType.FullName != "Android.App.Application")
                            continue;

                        // redirect
                        methodRef.DeclaringType = Dot42InternalApplicationBuilder.Type;
                    }
                }
            }
            base.GenerateCode(targetPackage, stopAtFirstError);
        }

        private bool IsDot42InternalApplication()
        {
            return Type.FullName == "Dot42.Internal.Application";
        }
    }
}
