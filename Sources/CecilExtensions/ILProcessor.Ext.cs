using Mono.Cecil.Cil;

namespace Dot42.CecilExtensions
{
    partial class Extensions
    {
        /// <summary>
        /// Insert all instructions before target
        /// </summary>
        internal static void InsertBefore(this ILProcessor worker, Instruction target, params Instruction[] instructions)
        {
            for (int i = instructions.Length - 1; i >= 0; i--)
            {
                var ins = instructions[i];
                worker.InsertBefore(target, ins);
                target = ins;
            }
        }

        /// <summary>
        /// Insert all instructions after target
        /// </summary>
        internal static void InsertAfter(this ILProcessor worker, Instruction target, params Instruction[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                var ins = instructions[i];
                worker.InsertAfter(target, ins);
                target = ins;
            }
        }

        /// <summary>
        /// Append all instructions at the end of the code
        /// </summary>
        internal static void Append(this ILProcessor worker, params Instruction[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                var ins = instructions[i];
                worker.Append(ins);
            }
        }
    }
}
