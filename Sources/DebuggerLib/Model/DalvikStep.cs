namespace Dot42.DebuggerLib.Model
{
    public class DalvikStep
    {
        public readonly DalvikThread Thread;
        public readonly int RequestId;
        public readonly StepRequest StepRequest;

        public DalvikStep(DalvikThread thread, int requestId, StepRequest stepRequest)
        {
            StepRequest = stepRequest;
            Thread = thread;
            RequestId = requestId;
        }
    }
}
