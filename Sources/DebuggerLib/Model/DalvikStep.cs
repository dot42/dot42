namespace Dot42.DebuggerLib.Model
{
    public class DalvikStep
    {
        public readonly DalvikThread Thread;
        public readonly int RequestId;

        public DalvikStep(DalvikThread thread, int requestId)
        {
            Thread = thread;
            RequestId = requestId;
        }
    }
}
