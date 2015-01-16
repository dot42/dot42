namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        public enum ThreadStatus
        {
            Zombie = 0,
            Running = 1,
            Sleeping = 2,
            Monitor = 3,
            Wait = 4
        }
    }
}
