namespace Dot42.DebuggerLib.Model
{
    public enum StepMode
    {
        /// <summary>
        /// Step to the next line. This is the default.
        /// </summary>
        Line,
        /// <summary>
        /// Step to next instuction. This is used for disassembly 
        /// stepping.
        /// </summary>
        SingleInstruction,
    }

    public class StepRequest
    {
        public readonly DalvikThread Thread;
        public readonly Jdwp.StepDepth StepDepth;
        public readonly StepMode StepMode;

        public StepRequest(DalvikThread thread, Jdwp.StepDepth stepDepth, StepMode stepMode = StepMode.Line)
        {
            Thread = thread;
            StepDepth = stepDepth;
            StepMode = stepMode;
        }
    }
}
