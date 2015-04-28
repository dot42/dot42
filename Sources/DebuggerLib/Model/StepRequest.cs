namespace Dot42.DebuggerLib.Model
{
    public class StepRequest
    {
        public readonly DalvikThread Thread;
        public readonly Jdwp.StepDepth StepDepth;
        public readonly bool SingleInstruction;

        public StepRequest(DalvikThread thread, Jdwp.StepDepth stepDepth, bool singleInstruction = false)
        {
            Thread = thread;
            StepDepth = stepDepth;
            SingleInstruction = singleInstruction;
        }
    }
}
