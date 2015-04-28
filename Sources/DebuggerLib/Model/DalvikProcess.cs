using System;
using System.Linq;
using System.Threading.Tasks;
using Dot42.Mapping;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Wraps the entire virtual machine process.
    /// </summary>
    public class DalvikProcess
    {
        public string ApkPath { get; set; }
        public static int VmTimeout = 15*1000;
        private readonly Debugger debugger;
        private readonly Lazy<DalvikBreakpointManager> breakpointManager;
        private readonly Lazy<DalvikExceptionManager> exceptionManager;
        private readonly Lazy<DalvikReferenceTypeManager> referenceTypeManager;
        private readonly Lazy<DalvikThreadManager> threadManager;
        private readonly Lazy<DalvikDisassemblyProvider> disassemblyProvider;
        private readonly DalvikStepManager stepManager = new DalvikStepManager();
        private readonly MapFile mapFile;
        private StepRequest lastStepRequest;
        private bool isSuspended;

        /// <summary>
        /// The process suspend status has changed.
        /// </summary>
        public event EventHandler IsSuspendedChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikProcess(Debugger debugger, MapFile mapFile, string apkPath)
        {
            ApkPath = apkPath;
            this.debugger = debugger;
            debugger.Process = this;
            this.mapFile = mapFile;
            debugger.ConnectedChanged += OnDebuggerConnectionChanged;
            breakpointManager = new Lazy<DalvikBreakpointManager>(CreateBreakpointManager);
            exceptionManager = new Lazy<DalvikExceptionManager>(CreateExceptionManager);
            referenceTypeManager = new Lazy<DalvikReferenceTypeManager>(CreateReferenceTypeManager);
            threadManager = new Lazy<DalvikThreadManager>(CreateThreadManager);
            disassemblyProvider =
                new Lazy<DalvikDisassemblyProvider>(() => new DalvikDisassemblyProvider(this, ApkPath, mapFile));

        }

        /// <summary>
        /// Is this process in suspended state?
        /// </summary>
        public bool IsSuspended { get { return isSuspended; } }

        /// <summary>
        /// Suspend the entire process.
        /// </summary>
        public Task SuspendAsync()
        {
            return
                debugger.VirtualMachine.SuspendAsync()
                    .ContinueWith(x => OnSuspended(SuspendReason.ProcessSuspend, null));
        }

        /// <summary>
        /// Resume the entire process
        /// </summary>
        public Task ResumeAsync()
        {
            return debugger.VirtualMachine.ResumeAsync().ContinueWith(x => OnResumed());
        }

        /// <summary>
        /// Perform a single step on the given thread.
        /// </summary>
        public Task StepAsync(StepRequest request)
        {
            // Set event
            lastStepRequest = request;
            var thread = request.Thread;
            var setTask = debugger.EventRequest.SetAsync(Jdwp.EventKind.SingleStep, Jdwp.SuspendPolicy.All,
                new EventStepModifier(thread.Id, request.SingleInstruction ? Jdwp.StepSize.Minimum : Jdwp.StepSize.Line,
                    request.StepDepth));
            return setTask.ContinueWith(t =>
            {
                t.ForwardException();
                // Record step
                StepManager.Add(new DalvikStep(thread, t.Result));
                // Resume execution
                return debugger.VirtualMachine.ResumeAsync().ContinueWith(x => OnResumed());
            }).Unwrap();
        }

        /// <summary>
        /// Perform the last step request again.
        /// </summary>
        private Task StepOutLastRequestAsync()
        {
            var request = lastStepRequest;
            if (request == null)
                return null;
            return StepAsync(new StepRequest(request.Thread, Jdwp.StepDepth.Out));
        }

        /// <summary>
        /// Stop the entire process and disconnect the debugger.
        /// </summary>
        public void ExitAndDisconnect(int exitCode)
        {
            if (debugger.Connected)
            {
                debugger.VirtualMachine.Exit(exitCode);
                debugger.Disconnect();
            }
        }

        /// <summary>
        /// Debugger has disconnected.
        /// </summary>
        protected virtual void OnConnectionLost()
        {
            // Override me
        }

        /// <summary>
        /// Notify listeners that we're suspended.
        /// </summary>
        /// <param name="reason">The reason the VM is suspended</param>
        /// <param name="thread">The thread involved in the suspend. This can be null depending on the reason.</param>
        /// <returns>True if the suspend is performed, false if execution is continued.</returns>
        protected internal virtual bool OnSuspended(SuspendReason reason, DalvikThread thread)
        {
            if ((reason == SuspendReason.SingleStep) && (thread != null))
            {
                // Make sure we're are a location where we have a source.
                thread.OnProcessSuspended(reason, true);
                var topFrame = thread.GetCallStack().FirstOrDefault();
                if (topFrame != null)
                {
                    var location = topFrame.GetDocumentLocationAsync().Await(VmTimeout);
                    if (location.Document == null)
                    {
                        // Not my code
                        StepOutLastRequestAsync();
                        return false;
                    }

                    // check if we have a valid source code position, or if this is 
                    // compiler generated code (in which case dalvik will perform single stepping)
                    if (lastStepRequest != null && !lastStepRequest.SingleInstruction)
                    {
                        if (location.Position == null || location.Position.IsSpecial ||
                            location.Location.Index < (ulong) location.Position.MethodOffset)
                        {
                            var stepDepth = lastStepRequest.StepDepth == Jdwp.StepDepth.Out ? Jdwp.StepDepth.Over : lastStepRequest.StepDepth;
                            StepAsync(new StepRequest(thread, stepDepth));
                            return false;
                        }
                    }

                }
            }
            ThreadManager.OnProcessSuspended(reason, thread);
            ReferenceTypeManager.OnProcessSuspended(reason);
            isSuspended = true;
            IsSuspendedChanged.Fire(this);
            return true;
        }

        /// <summary>
        /// Process has resumed
        /// </summary>
        protected virtual void OnResumed()
        {
            isSuspended = false;
            IsSuspendedChanged.Fire(this);
        }

        /// <summary>
        /// Maintains information about registered breakpoints
        /// </summary>
        public DalvikBreakpointManager BreakpointManager { get { return breakpointManager.Value; } }

        /// <summary>
        /// Maintains information about exceptions
        /// </summary>
        public DalvikExceptionManager ExceptionManager { get { return exceptionManager.Value; } }

        /// <summary>
        /// Maintains information about classes.
        /// </summary>
        public DalvikReferenceTypeManager ReferenceTypeManager { get { return referenceTypeManager.Value; } }

        /// <summary>
        /// Maintains information about threads.
        /// </summary>
        public DalvikThreadManager ThreadManager { get { return threadManager.Value; } }

        /// <summary>
        /// Provides information retrieved from the .APK
        /// </summary>
        public DalvikDisassemblyProvider DisassemblyProvider { get { return disassemblyProvider.Value; } }


        /// <summary>
        /// Gets the holder of steps.
        /// </summary>
        internal DalvikStepManager StepManager { get { return stepManager; } }

        /// <summary>
        /// Resolve the given dalvik location to a source location.
        /// </summary>
        public Task<DocumentLocation> ResolveAsync(Location location)
        {
            return Task.Factory.StartNew(() => DoResolve(location), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Resolve the given dalvik location to a source location.
        /// </summary>
        private DocumentLocation DoResolve(Location location)
        {
            var refType = ReferenceTypeManager[location.Class];
            DalvikMethod method = null;
            if (refType != null)
            {
                method = refType.GetMethodsAsync().Select(t =>
                {
                    DalvikMethod m;
                    return t.TryGetMember(location.Method, out m) ? m : null;
                }).Await(VmTimeout);
            }

            var typeName = (refType != null) ? refType.GetNameAsync().Await(VmTimeout) : null;
            var typeEntry = (typeName != null) ? mapFile.GetTypeByNewName(typeName) : null;
            var methodDexName = (method != null) ? method.Name : null;
            var methodDexSignature = (method != null) ? method.Signature : null;
            var methodEntry = ((typeEntry != null) && (method != null))
                ? typeEntry.FindDexMethod(methodDexName, methodDexSignature)
                : null;

            Document document = null;
            DocumentPosition position = null;
            if (methodEntry != null)
            {
                mapFile.TryFindLocation(typeEntry, methodEntry, (int) location.Index, out document, out position);
            }

            return new DocumentLocation(location, document, position, refType, method, typeEntry, methodEntry);
        }

        /// <summary>
        /// Get a DocumentLocation from a DocumentPosition. This will only return locations
        /// for classes that have already been loaded by the VM. Delegates are typically not
        /// loaded until their first invocation.
        /// </summary>
        public Task<DocumentLocation> GetLocationFromPositionAsync(Document doc, DocumentPosition pos)
        {
            return Task.Factory.StartNew(() =>
            {
                // Lookup class & method
                var typeEntry = MapFile.GetTypeById(pos.TypeId);
                if (typeEntry == null)
                    return null;
                var methodEntry = typeEntry.GetMethodById(pos.MethodId);
                if (methodEntry == null)
                    return null;

                var signature = typeEntry.DexSignature;
                var refType = ReferenceTypeManager.FindBySignature(signature);
                if (refType == null)
                    return null;

                var refTypeMethods = refType.GetMethodsAsync().Await(DalvikProcess.VmTimeout);
                var dmethod = refTypeMethods.FirstOrDefault(x => x.IsMatch(methodEntry));
                if (dmethod == null)
                    return null;

                var loc = new Location(refType.Id, dmethod.Id, (ulong)pos.MethodOffset);

                return new DocumentLocation(loc, doc, pos, refType, dmethod, typeEntry, methodEntry);
            });
        }

        /// <summary>
        /// Create our breakpoint manager.
        /// </summary>
        protected virtual DalvikBreakpointManager CreateBreakpointManager()
        {
            return new DalvikBreakpointManager(this);
        }

        /// <summary>
        /// Create our exception manager.
        /// </summary>
        protected virtual DalvikExceptionManager CreateExceptionManager()
        {
            return new DalvikExceptionManager(this);
        }

        /// <summary>
        /// Create our class manager.
        /// </summary>
        protected virtual DalvikReferenceTypeManager CreateReferenceTypeManager()
        {
            return new DalvikReferenceTypeManager(this);
        }

        /// <summary>
        /// Create our thread manager.
        /// </summary>
        protected virtual DalvikThreadManager CreateThreadManager()
        {
            return new DalvikThreadManager(this);
        }

        /// <summary>
        /// Gets access to the low level debugger connection.
        /// </summary>
        protected internal Debugger Debugger { get { return debugger; } }

        /// <summary>
        /// Gets access to the debug map file.
        /// </summary>
        protected internal MapFile MapFile { get { return mapFile; } }

        /// <summary>
        /// Initialize the debugger so we're ready to start debugging.
        /// </summary>
        internal Task PrepareForDebuggingAsync()
        {
            // Prepare all managers
            return
                ExceptionManager.PrepareForDebuggingAsync()
                    .ContinueWith(t => ReferenceTypeManager.PrepareForDebuggingAsync())
                    .Unwrap();
        }

        /// <summary>
        /// Listener for connection changes
        /// </summary>
        private void OnDebuggerConnectionChanged(object sender, EventArgs e)
        {
            if (!debugger.Connected)
            {
                OnConnectionLost();
            }
        }


    }
}
