using System;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Event objects
    /// </summary>
    internal abstract class BaseEvent : IDebugEvent2
    {
        private readonly enum_EVENTATTRIBUTES attributes;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected BaseEvent(enum_EVENTATTRIBUTES attributes)
        {
            this.attributes = attributes;
        }

        /// <summary>
        /// Gets my attributes
        /// </summary>
        internal enum_EVENTATTRIBUTES Attributes { get { return attributes; } }

        /// <summary>
        /// GUID that identifies which event interface to obtain.
        /// </summary>
        internal abstract Guid IID { get; }

        /// <summary>
        /// Gets attributes
        /// </summary>
        public int GetAttributes(out uint pdwAttrib)
        {
            pdwAttrib = (uint) attributes;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// Event objects
    /// </summary>
    internal abstract class BaseEvent<T> : BaseEvent
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        protected BaseEvent(enum_EVENTATTRIBUTES attributes) : base(attributes)
        {
        }

        /// <summary>
        /// GUID that identifies which event interface to obtain.
        /// </summary>
        internal override Guid IID { get { return typeof(T).GUID; } }
    }

    /// <summary>
    /// Base class for asynchronous events
    /// </summary>
    internal abstract class ASynchronousEvent<T> : BaseEvent<T>
    {
        protected ASynchronousEvent()
            : base(enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS)
        {
        }
    }

    /// <summary>
    /// Base class for synchronous events
    /// </summary>
    internal abstract class SynchronousEvent<T> : BaseEvent<T>
    {
        protected SynchronousEvent()
            : base(enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS)
        {
        }
    }

    /// <summary>
    /// Base class for stopping events
    /// </summary>
    internal abstract class StoppingEvent<T> : BaseEvent<T>
    {
        protected StoppingEvent()
            : base(enum_EVENTATTRIBUTES.EVENT_STOPPING)
        {
        }
    }

    /// <summary>
    /// Base class for asynchronous stopping events
    /// </summary>
    internal abstract class ASynchronousStoppingEvent<T> : BaseEvent<T>
    {
        protected ASynchronousStoppingEvent()
            : base(enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP)
        {
        }
    }

    /// <summary>
    /// Base class for synchronous stopping events
    /// </summary>
    internal abstract class SynchronousStoppingEvent<T> : BaseEvent<T>
    {
        protected SynchronousStoppingEvent()
            : base(enum_EVENTATTRIBUTES.EVENT_STOPPING | enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS)
        {
        }
    }

    /// <summary>
    /// Sent by the DE when a break in the program has been completed.
    /// </summary>
    internal sealed class ASyncBreakCompleteEvent : StoppingEvent<IDebugBreakEvent2>, IDebugBreakEvent2
    {
    }

    /// <summary>
    /// This interface tells the session debug manager (SDM) that a pending breakpoint has been successfully bound to a loaded program.
    /// </summary>
    internal sealed class BreakpointBoundEvent : ASynchronousEvent<IDebugBreakpointBoundEvent2>, IDebugBreakpointBoundEvent2
    {
        private readonly DebugPendingBreakpoint pendingBreakpoint;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BreakpointBoundEvent(DebugPendingBreakpoint pendingBreakpoint)
        {
            this.pendingBreakpoint = pendingBreakpoint;
        }

        public int GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = pendingBreakpoint;
            return VSConstants.S_OK;
        }

        public int EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            return pendingBreakpoint.EnumBoundBreakpoints(out ppEnum);
        }
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when a program stops at a breakpoint.
    /// </summary>
    internal sealed class BreakpointEvent : StoppingEvent<IDebugBreakpointEvent2>, IDebugBreakpointEvent2
    {
        private readonly IDebugBoundBreakpoint2 breakpoint;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BreakpointEvent(IDebugBoundBreakpoint2 breakpoint)
        {
            this.breakpoint = breakpoint;
        }

        public int EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            DLog.Debug(DContext.VSDebuggerEvent, "BreakpointEvent.EnumBreakpoints");

            ppEnum = new BoundBreakpointsEnum(new[] { breakpoint });
            return VSConstants.S_OK;
        }
    }

    internal sealed class BreakpointUnboundEvent : ASynchronousEvent<IDebugBreakpointUnboundEvent2>, IDebugBreakpointUnboundEvent2
    {
        private readonly IDebugBoundBreakpoint boundBreakpoint;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BreakpointUnboundEvent(IDebugBoundBreakpoint boundBreakpoint)
        {
            this.boundBreakpoint = boundBreakpoint;
        }

        public int GetBreakpoint(out IDebugBoundBreakpoint2 ppBP)
        {
            ppBP = boundBreakpoint;
            return VSConstants.S_OK;
        }

        public int GetReason(enum_BP_UNBOUND_REASON[] pdwUnboundReason)
        {
            pdwUnboundReason[0] = enum_BP_UNBOUND_REASON.BPUR_UNKNOWN;
            pdwUnboundReason[0] = enum_BP_UNBOUND_REASON.BPUR_CODE_UNLOADED;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created.
    /// </summary>
    internal sealed class EngineCreateEvent : ASynchronousEvent<IDebugEngineCreateEvent2>, IDebugEngineCreateEvent2
    {
        private readonly DebugEngine engine;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EngineCreateEvent(DebugEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Gets the created engine
        /// </summary>
        public int GetEngine(out IDebugEngine2 pEngine)
        {
            pEngine = engine;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when the program is about to execute its first instruction of user code.
    /// </summary>
    internal sealed class EntryPointEvent : ASynchronousEvent<IDebugEntryPointEvent2>, IDebugEntryPointEvent2
    {
    }

    /// <summary>
    /// The debug engine (DE) sends this interface to the session debug manager (SDM) when an exception is thrown in the program currently being executed.
    /// </summary>
    internal sealed class ExceptionEvent : StoppingEvent<IDebugExceptionEvent2>, IDebugExceptionEvent2
    {
        private readonly EXCEPTION_INFO info;
        private readonly string description;
        private readonly bool canPassToDebuggee;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ExceptionEvent(EXCEPTION_INFO info, string description, bool canPassToDebuggee)
        {
            this.info = info;
            this.description = description;
            this.canPassToDebuggee = canPassToDebuggee;
        }

        public int GetException(EXCEPTION_INFO[] pExceptionInfo)
        {
            pExceptionInfo[0] = info;
            return VSConstants.S_OK;
        }

        public int GetExceptionDescription(out string pbstrDescription)
        {
            pbstrDescription = description;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether or not the debug engine (DE) supports the option of passing this exception to the program being debugged when execution resumes.
        /// </summary>
        public int CanPassToDebuggee()
        {
            return canPassToDebuggee ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        /// <summary>
        /// Specifies whether the exception should be passed on to the program being debugged when execution resumes, or if the exception should be discarded.
        /// </summary>
        public int PassToDebuggee(int fPass)
        {
            IsPassToDebuggee = (fPass != 0);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Result of <see cref="PassToDebuggee"/>
        /// </summary>
        public bool IsPassToDebuggee { get; private set; }
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is loaded, but before any code is executed.
    /// </summary>
    internal sealed class LoadCompleteEvent : StoppingEvent<IDebugLoadCompleteEvent2>, IDebugLoadCompleteEvent2
    {
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a module is loaded or unloaded.
    /// </summary>
    internal sealed class ModuleLoadEvent : ASynchronousEvent<IDebugModuleLoadEvent2>, IDebugModuleLoadEvent2
    {
        private readonly DebugModule module;
        private readonly string message;
        private readonly bool loaded;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ModuleLoadEvent(DebugModule module, string message, bool loaded)
        {
            this.module = module;
            this.message = message;
            this.loaded = loaded;
        }

        public int GetModule(out IDebugModule2 pModule, ref string pbstrDebugMessage, ref int pbLoad)
        {
            pModule = module;
            pbstrDebugMessage = message;
            pbLoad = loaded ? 1 : 0;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// Sent by the DE or port when a process has been created.
    /// </summary>
    internal sealed class ProcessCreateEvent : ASynchronousEvent<IDebugProcessCreateEvent2>, IDebugProcessCreateEvent2
    {
    }

    /// <summary>
    /// Sent by the DE or port when a process has been destroyed.
    /// </summary>
    internal sealed class ProcessDestroyEvent : ASynchronousEvent<IDebugProcessDestroyEvent2>, IDebugProcessDestroyEvent2
    {
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
    /// </summary>
    internal sealed class ProgramCreateEvent : ASynchronousEvent<IDebugProgramCreateEvent2>, IDebugProgramCreateEvent2
    {
    }

    /// <summary>
    /// Sent by the DE or port when a program has been destroyed.
    /// </summary>
    internal sealed class ProgramDestroyEvent : ASynchronousEvent<IDebugProgramDestroyEvent2>, IDebugProgramDestroyEvent2
    {
        private readonly int exitCode;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProgramDestroyEvent(int exitCode)
        {
            this.exitCode = exitCode;
        }

        /// <summary>
        /// Gets the exit code
        /// </summary>
        public int GetExitCode(out uint pdwExit)
        {
            pdwExit = (uint)exitCode;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when the program being debugged completes a step into, 
    /// a step over, or a step out of a line of source code or statement or instruction.
    /// </summary>
    internal sealed class StepCompleteEvent : StoppingEvent<IDebugStepCompleteEvent2>, IDebugStepCompleteEvent2
    {
    }

    /// <summary>
    /// Specifies the state of symbols for a module.
    /// </summary>
    internal sealed class SymbolSearchEvent : ASynchronousEvent<IDebugSymbolSearchEvent2>, IDebugSymbolSearchEvent2
    {
        private readonly DebugModule module;
        private readonly string message;
        private readonly enum_MODULE_INFO_FLAGS flags;

        public SymbolSearchEvent(DebugModule module, string message, enum_MODULE_INFO_FLAGS flags)
        {
            this.module = module;
            this.message = message;
            this.flags = flags;
        }

        public int GetSymbolSearchInfo(out IDebugModule3 pModule, ref string pbstrDebugMessage, enum_MODULE_INFO_FLAGS[] pdwModuleInfoFlags)
        {
            pModule = module;
            pbstrDebugMessage = message;
            pdwModuleInfoFlags[0] = flags;
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread is created in a program being debugged.
    /// </summary>
    internal sealed class ThreadCreateEvent : ASynchronousEvent<IDebugThreadCreateEvent2>, IDebugThreadCreateEvent2
    {
    }

    /// <summary>
    /// This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread has run to completion.
    /// </summary>
    internal sealed class ThreadDestroyEvent : ASynchronousEvent<IDebugThreadDestroyEvent2>, IDebugThreadDestroyEvent2
    {
        public int GetExitCode(out uint pdwExit)
        {
            pdwExit = 0;
            return VSConstants.S_OK;
        }
    }
}
