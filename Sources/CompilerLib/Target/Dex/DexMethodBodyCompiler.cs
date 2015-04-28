using System;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;

namespace Dot42.CompilerLib.Target
{
    /// <summary>
    /// Compile IL to Dex code.
    /// </summary>
    internal abstract class DexMethodBodyCompiler : MethodBodyCompiler
    {
        /// <summary>
        /// Create a method body for the given method.
        /// </summary>
        internal static MethodBody TranslateToRL(AssemblyCompiler compiler, DexTargetPackage targetPackage, MethodSource source, MethodDefinition dmethod, bool generateSetNextInstructionCode, out CompiledMethod compiledMethod)
        {
            try
            {
#if DEBUG
                //Debugger.Launch();
                if ((source.Method != null) && (source.Method.Name == "test6"))
                {
                    //Debugger.Launch();
                }
#endif

                // Create Ast
                var optimizedAst = CreateOptimizedAst(compiler, source, generateSetNextInstructionCode);

                // Generate RL code
                var rlBody = new MethodBody(source);
                var rlGenerator = new AstCompilerVisitor(compiler, source, targetPackage, dmethod, rlBody);
                optimizedAst.Accept(rlGenerator, null);

                // Should we add return_void?
                if (source.ReturnsVoid)
                {
                    var instructions = rlBody.Instructions;
                    if ((instructions.Count == 0) || (instructions.Last().Code != RCode.Return_void && instructions.Last().Code != RCode.Throw))
                    {
                        instructions.Add(new RL.Instruction(RCode.Return_void) { SequencePoint = source.GetLastSourceLine() });
                    }
                }

                // Record results
                compiledMethod = targetPackage.Record(source, rlBody, rlGenerator.Frame);

                return rlBody;
            }
            catch (Exception ex)
            {
                // Forward exception with more information
                var msg = string.Format("Error while compiling {0} in {1}: {2}", source.FullName, source.DeclaringTypeFullName, ex.Message);
                throw new CompilerException(msg, ex);
            }
        }
    }
}
