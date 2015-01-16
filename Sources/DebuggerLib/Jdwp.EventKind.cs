namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        public enum EventKind
        {
            SingleStep = 1,
            BreakPoint = 2,
            FramePop = 3,
            Exception = 4,
            UserDefined = 5,
            ThreadStart = 6,
            ThreadEnd = 7,
            ClassPrepare = 8,
            ClassUnload = 9,
            ClassLoad = 10,
            FieldAccess = 20,
            FieldModification = 21,
            ExceptionCatch = 30,
            MethodEntry = 40,
            MethodExit = 41,
            VmInit = 90,
            VmDeath = 99,
            VmDisconnected = 100 // Never sent acress JDWP
        }
    }
}
