using System.Collections.Generic;
using System.Linq;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Bytecode;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Exception handler data
    /// </summary>
    public sealed class ExceptionHandler : IResolveable
    {
        private readonly CodeAttribute code;
        private readonly int startPC;
        private readonly int endPC;
        private readonly int handlerPC;
        private readonly TypeReference catchType;

        private Instruction start;
        private Instruction end;
        private Instruction handler;

        internal ExceptionHandler(CodeAttribute code, int startPc, int endPc, int handlerPc, TypeReference catchType)
        {
            this.code = code;
            startPC = startPc;
            endPC = endPc;
            handlerPC = handlerPc;
            this.catchType = catchType;
        }

        public int StartPc
        {
            get { return startPC; }
        }

        public int EndPc
        {
            get { return endPC; }
        }

        public int HandlerPc
        {
            get { return handlerPC; }
        }

        public Instruction Start
        {
            get
            {
                if (start == null)
                    code.ResolveInstructions();
                return start;
            }
        }

        public Instruction End
        {
            get
            {
                if (end == null)
                    code.ResolveInstructions();
                return end;
            }
        }

        public Instruction Handler
        {
            get
            {
                if (handler == null)
                    code.ResolveInstructions();
                return handler;
            }
        }

        /// <summary>
        /// Type of exception to catch.
        /// Null to catch all.
        /// </summary>
        public TypeReference CatchType
        {
            get { return catchType; }
        }

        /// <summary>
        /// Does this handler catch all exceptions?
        /// </summary>
        public bool IsCatchAll
        {
            get { return (catchType == null); }
        }

        /// <summary>
        /// Resolve this offset into an instruction.
        /// </summary>
        object IResolveable.Resolve(List<Instruction> instructions, Instruction owner)
        {
            start = instructions.FirstOrDefault(x => x.Offset == startPC);
            end = instructions.Last(x => x.Offset < endPC);
            handler = instructions.FirstOrDefault(x => x.Offset == handlerPC);
            return this;
        }
    }
}
