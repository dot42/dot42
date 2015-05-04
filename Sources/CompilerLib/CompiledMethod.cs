using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Transformations;
using Dot42.CompilerLib.RL2DexCompiler;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.JvmClassLib.Attributes;
using Dot42.Mapping;
using Dot42.Utility;
using MethodDefinition = Dot42.JvmClassLib.MethodDefinition;

namespace Dot42.CompilerLib
{
    /// <summary>
    /// Holder for compiled method results.
    /// </summary>
    public sealed class CompiledMethod
    {
        private readonly XMethodDefinition method;
        private bool rlIsOptimized;
        private RegisterMapper regMapper;

        /// <summary>
        /// Default ctor
        /// </summary>
        public CompiledMethod()
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public CompiledMethod(XMethodDefinition method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            this.method = method;
        }

        /// <summary>
        /// Is the source an IL method?
        /// </summary>
        public bool HasILSource
        {
            get { return (method is XModel.DotNet.XBuilder.ILMethodDefinition); }
        }

        /// <summary>
        /// Is the source a Java method?
        /// </summary>
        public bool HasJavaSource
        {
            get { return (method is XModel.Java.XBuilder.JavaMethodDefinition); }
        }

        /// <summary>
        /// Source method (IL)
        /// </summary>
        public Mono.Cecil.MethodDefinition ILSource
        {
            get
            {
                var methodDef = method as XModel.DotNet.XBuilder.ILMethodDefinition;
                return (methodDef != null) ? methodDef.OriginalMethod : null;
            }
        }

        /// <summary>
        /// Source method (Java)
        /// </summary>
        public MethodDefinition JavaSource
        {
            get
            {
                var methodDef = method as XModel.Java.XBuilder.JavaMethodDefinition;
                return (methodDef != null) ? methodDef.OriginalMethod : null;
            }
        }

        /// <summary>
        /// Does the source method have a body?
        /// </summary>
        public bool HasBody
        {
            get
            {
                var ilMethod = ILSource;
                if (ilMethod != null) return ilMethod.HasBody;
                var javaMethod = JavaSource;
                if (javaMethod != null) return javaMethod.Attributes.OfType<CodeAttribute>().Any();
                return false;
            }
        }

        /// <summary>
        /// Final Dex method
        /// </summary>
        public Dot42.DexLib.MethodDefinition DexMethod { get; set; }

        /// <summary>
        /// RL intermediate result
        /// </summary>
        public MethodBody RLBody { get; set; }

        /// <summary>
        /// Stack frame
        /// </summary>
        internal InvocationFrame InvocationFrame { get; set; }

        /// <summary>
        /// Optimize the RL structure.
        /// </summary>
        private void OptimizeRL(Dex target)
        {
            if (rlIsOptimized)
                return;
            var rlBody = RLBody;
            if (rlBody == null)
                throw new ArgumentException("No RL body set");

            // Optimize RL code
            RLTransformations.Transform(target, rlBody);
            rlIsOptimized = true;
        }
           
        /// <summary>
        /// Compile RL into the Dex method body.
        /// </summary>
        internal void CompileToTarget(ITargetPackage targetPackage, bool generateDebugInfo, MapFile mapFile)
        {
            CompileToDex((DexTargetPackage) targetPackage, generateDebugInfo, mapFile);
        }
           
        /// <summary>
        /// Compile RL into the Dex method body.
        /// </summary>
        private void CompileToDex(DexTargetPackage targetPackage, bool generateDebugInfo, MapFile mapFile)
        {
            var dmethod = DexMethod;
            if (dmethod == null)
                throw new ArgumentException("No DexMethod set");
            if ((dmethod.IsAbstract) || (dmethod.IsNative))
                return;

            var rlBody = RLBody;

            if (rlBody == null && dmethod.Body != null) // already satisfied from the cache?
                return;

            if (rlBody == null)
                throw new ArgumentException(string.Format("internal compiler error: No RL body set on method '{2}'.'{3}' => '{0}'.'{1}'", dmethod.Owner.Name, dmethod.Name, method == null ? null : method.DeclaringType.FullName, method == null ? null : method.Name));

            // Ensure RL is optimized
            OptimizeRL(targetPackage.DexFile);

            // Compile to Dex
            var dbody = new Dot42.DexLib.Instructions.MethodBody(dmethod, 0);
            var dexCompiler = new DexCompiler(rlBody, dbody, InvocationFrame);
            regMapper = dexCompiler.Compile();

            // Optimize code
            //dbody.UpdateInstructionOffsets();
            DexOptimizer.DexOptimizer.Optimize(dbody);

            // Ensure correct offsets
            dbody.UpdateInstructionOffsets();
            dmethod.Body = dbody;

            if (generateDebugInfo || (mapFile != null))
            {
                // Add debug info
                var debugInfoBuilder = new DebugInfoBuilder(this);
                if (generateDebugInfo)
                    debugInfoBuilder.CreateDebugInfo(dbody, regMapper, targetPackage);
                if (mapFile != null && dmethod.MapFileId != 0)
                    debugInfoBuilder.AddDocumentMapping(mapFile);
            }
        }

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        public void RecordMapping(MethodEntry methodEntry)
        {
            if (methodEntry == null)
                throw new ArgumentNullException("methodEntry");
            if (regMapper == null)
                return;
            var frame = InvocationFrame;
            var paramIndex = 0;
            foreach (var argSpec in frame.Arguments)
            {
                var dreg = regMapper[argSpec.Register];
                if (dreg == null)
                    throw new Dot42Exception(string.Format("Cannot find dex register for {0}", argSpec.Register));
                var paramName = (argSpec.Parameter != null) ? argSpec.Parameter.Name : null;
                paramName = paramName ?? string.Format("p{0}", paramIndex);
                methodEntry.Parameters.Add(new Mapping.Parameter(dreg.Index, paramName));
                paramIndex++;
            }
            var varIndex = 0;
            foreach (var varSpec in frame.Variables)
            {
                if (regMapper.ContainsKey(varSpec.Register))
                {
                    var dreg = regMapper[varSpec.Register];
                    if (dreg == null)
                        throw new Dot42Exception(string.Format("Cannot find dex register for {0}", varSpec.Register));
                    var varName = (varSpec.Variable != null) ? varSpec.Variable.OriginalName : null;
                    varName = varName ?? string.Format("v{0}", varIndex);
                    methodEntry.Variables.Add(new Mapping.Variable(dreg.Index, varName));
                }
                varIndex++;
            }
        }
    }
}
