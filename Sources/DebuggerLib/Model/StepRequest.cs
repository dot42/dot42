namespace Dot42.DebuggerLib.Model
{
    public class StepRequest
    {
        public readonly DalvikThread Thread;
        public readonly Jdwp.StepDepth StepDepth;

        public StepRequest(DalvikThread thread, Jdwp.StepDepth stepDepth)
        {
            Thread = thread;
            StepDepth = stepDepth;
        }
    }
}
