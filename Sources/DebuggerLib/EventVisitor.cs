using Dot42.DebuggerLib.Events.Dalvik;
using Dot42.DebuggerLib.Events.Jdwp;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Visitor pattern for JDWP events
    /// </summary>
    public class EventVisitor<TResult, TData>
    {
        // Base
        protected virtual TResult Visit(JdwpEvent e, TData data) { return default(TResult); }

        // JDWP events
        public virtual TResult Visit(Breakpoint e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(ClassPrepare e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(ClassUnload e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(Exception e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(FieldAccess e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(FieldModification e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(MethodEntry e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(MethodExit e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(SingleStep e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(ThreadStart e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(Events.Jdwp.ThreadDeath e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(VmDeath e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(VmStart e, TData data) { return Visit((JdwpEvent)e, data); }

        // Dalvik events
        public virtual TResult Visit(AppNameChanged e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(ThreadCreation e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(Events.Dalvik.ThreadDeath e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(ThreadStatus e, TData data) { return Visit((JdwpEvent)e, data); }
        public virtual TResult Visit(WaitForDebugger e, TData data) { return Visit((JdwpEvent)e, data); }
    }
}
