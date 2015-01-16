using System.Threading.Tasks;
using Dot42.Utility;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Handle all exceptions.
    /// </summary>
    public class DalvikExceptionManager : DalvikProcessChild
    {
        private readonly ExceptionBehaviorMap exceptionBehaviorMap = new ExceptionBehaviorMap();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikExceptionManager(DalvikProcess process)
            : base(process)
        {
        }

        /// <summary>
        /// Process the given exception event.
        /// </summary>
        protected internal virtual void OnExceptionEvent(Events.Jdwp.Exception @event, DalvikThread thread)
        {
            // Log
            DLog.Debug(DContext.VSDebuggerEvent, "OnExceptionEvent location: {0}", @event.Location);

            // Save exception in thread
            thread.CurrentException = @event.ExceptionObject;
        }

        /// <summary>
        /// Initialize the debugger so we're ready to start debugging.
        /// </summary>
        internal Task PrepareForDebuggingAsync()
        {
            // Catch all non-caught exceptions
            var sizeInfo = Debugger.GetIdSizeInfo();
            return Debugger.EventRequest.SetAsync(Jdwp.EventKind.Exception, Jdwp.SuspendPolicy.All, 
                new ExceptionOnlyModifier(new ClassId(sizeInfo), exceptionBehaviorMap.DefaultStopOnThrow, exceptionBehaviorMap.DefaultStopUncaught));
        }

        /// <summary>
        /// Gets the map that controls the behavior for specifc exceptions.
        /// </summary>
        public ExceptionBehaviorMap ExceptionBehaviorMap
        {
            get { return exceptionBehaviorMap; }
        }
    }
}
