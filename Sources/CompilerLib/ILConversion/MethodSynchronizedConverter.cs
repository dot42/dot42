using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Replace synchronized methods with lock constructs.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class MethodSynchronizedConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 50; }
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            private ReachableContext reachableContext;

            /// <summary>
            /// Convert all synchronized methods.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                this.reachableContext = reachableContext;

                // Collect all names
                var synchronizedMethods =
                    reachableContext.ReachableTypes.SelectMany(x => x.Methods)
                                    .Where(
                                        m =>
                                        m.IsSynchronized && m.IsReachable && m.HasBody && !m.IsConstructor &&
                                        !m.IsStatic)
                                    .ToList();
                if (synchronizedMethods.Count == 0)
                    return;

                foreach (var method in synchronizedMethods)
                {
                    Convert(method.Body);
                }
            }

            /// <summary>
            /// Convert all synchronized methods.
            /// </summary>
            private static void Convert(MethodBody body)
            {
                var typeSystem = body.Method.Module.TypeSystem;
                var monitorType = typeSystem.LookupType("System.Threading", "Monitor");
                var enterMethod = new MethodReference("Enter", typeSystem.Void, monitorType);
                enterMethod.Parameters.Add(new ParameterDefinition(typeSystem.Object));
                var exitMethod = new MethodReference("Exit", typeSystem.Void, monitorType);
                exitMethod.Parameters.Add(new ParameterDefinition(typeSystem.Object));
                var firstInstr = body.Instructions[0];

                // Expand macro's
                body.SimplifyMacros();

                // Prepare new return
                var retSeq = new ILSequence();
                retSeq.Emit(OpCodes.Nop);
                retSeq.Emit(OpCodes.Ret);

                // Monitor.Enter(this)
                var initSeq = new ILSequence();
                initSeq.Emit(OpCodes.Ldarg_0); // ld this
                initSeq.Emit(OpCodes.Call, enterMethod);
                initSeq.InsertTo(0, body);

                // Leave sequence
                var leaveSeq = new ILSequence();
                leaveSeq.Emit(OpCodes.Nop);
                leaveSeq.Emit(OpCodes.Leave, retSeq.First);
                leaveSeq.AppendTo(body);

                // Finally: Monitor.Exit(this)
                var finallySeq = new ILSequence();
                finallySeq.Emit(OpCodes.Ldarg_0); // ld this
                finallySeq.Emit(OpCodes.Call, exitMethod);
                finallySeq.Emit(OpCodes.Endfinally);
                finallySeq.AppendTo(body);

                // Replace Ret instructions
                foreach (var instr in body.Instructions.Where(x => x.OpCode.Code == Mono.Cecil.Cil.Code.Ret))
                {
                    if (instr.Next == leaveSeq.First)
                    {
                        instr.ChangeToNop();
                    }
                    else
                    {
                        instr.OpCode = OpCodes.Br;
                        instr.Operand = leaveSeq.First;
                    }
                }

                // Append ret sequence
                retSeq.AppendTo(body);

                // Update offsets
                body.ComputeOffsets();

                // Add try/finally block
                var handler = new ExceptionHandler(ExceptionHandlerType.Finally);
                handler.TryStart = firstInstr;
                handler.TryEnd = finallySeq.First; // leaveSeq.Last;
                handler.HandlerStart = finallySeq.First;
                handler.HandlerEnd = retSeq.First; // finallySeq.Last;
                body.ExceptionHandlers.Insert(0, handler);
            }
        }
    }    
}